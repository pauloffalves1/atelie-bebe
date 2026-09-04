# Ateliê Bebê

E-commerce e gestão de encomendas para um ateliê de enxovais e itens de bebê feitos sob medida. O projeto é um monorepo com uma API backend em .NET e uma SPA frontend em Angular, cobrindo tanto a loja pública quanto o painel administrativo.

## Arquitetura

O repositório é dividido em dois projetos independentes, sem build ou tooling compartilhado na raiz:

```
client/   Angular 22 (SPA — loja pública + painel admin)
server/   .NET 10 (API — Clean Architecture)
```

### Backend (`server/`)

API HTTP construída com **ASP.NET Core Minimal APIs** (sem controllers), organizada em **Clean Architecture** com quatro projetos, dependências fluindo sempre para dentro:

```
AtelieBebe.Api             → Endpoints, composição da aplicação (Program.cs)
AtelieBebe.Application     → Casos de uso, DTOs, regras de orquestração
AtelieBebe.Domain          → Entidades, value objects, eventos de domínio (sem dependências externas)
AtelieBebe.Infrastructure  → EF Core, repositórios, JWT, hashing, outbox
```

- **Domain** possui entidades ricas (`Product`, `Order`, `Customer`, `Admin`, `ContactMessage`) que encapsulam suas próprias invariantes e levantam eventos de domínio quando algo relevante acontece (pedido criado, status alterado, estoque baixo, cliente cadastrado).
- **Application** expõe um serviço por feature (`IProductService`, `IOrderService`, `ICustomerAuthService`, `IAdminAuthService`, `IContactService`), dependendo apenas de abstrações (`IUnitOfWork`, `IPasswordHasher`, `IJwtTokenGenerator`, `INotificationSender`) — nunca de `Infrastructure` diretamente.
- **Infrastructure** implementa persistência via **Entity Framework Core + SQLite**, repositórios, geração de JWT, hashing de senha (BCrypt) e o mecanismo de outbox.
- **Api** mapeia cada feature em um grupo de endpoints (`Endpoints/*Endpoints.cs`), com rotas públicas em `/api/{feature}` e rotas administrativas em `/api/admin/{feature}` protegidas pela policy `AdminOnly`.

**Eventos de domínio → Outbox → Notificações**: quando uma entidade levanta um evento de domínio, um interceptor do EF Core (`DomainEventsToOutboxInterceptor`) grava esse evento como uma linha na tabela de outbox, na mesma transação da mudança de estado — garantindo atomicidade. Um `BackgroundService` (`OutboxProcessor`) faz polling a cada 5s, desserializa cada evento e despacha para `INotificationSender` (hoje apenas `LoggingNotificationSender`, que loga — não há envio real de e-mail/SMS ainda). Entrega é *at-least-once*, com até 5 tentativas por mensagem.

**Autenticação** é feita via **JWT Bearer**, com dois fluxos independentes e papéis (roles) separados: **Admin** (`AdminOnly` policy, acesso ao painel) e **Cliente** (`CustomerOnly` policy, acesso à área "Minha Conta"). Um administrador padrão e um catálogo de produtos de exemplo são semeados automaticamente na primeira execução (`DbInitializer`).

### Frontend (`client/`)

**Angular 22**, componentes *standalone* (sem `NgModule`), rotas com lazy loading (`loadComponent`), estilização com Bootstrap 5. `src/app/` é dividido em:

- **`core/`** — serviços singleton (`services/`, um por feature do backend), modelos/DTOs (`models/`), guards de rota (`guards/`) e um interceptor HTTP (`interceptors/auth.interceptor.ts`) que anexa o token Bearer correto (admin ou cliente) conforme a URL da requisição.
- **`features/`** — componentes de rota, divididos em `public/` (loja: home, catálogo, produto, carrinho, checkout, cadastro/login, minha conta, contato, galeria, encomenda personalizada) e `admin/` (dashboard, gestão de produtos/pedidos/mensagens), protegidos por `adminGuard`/`customerGuard`.
- **`shared/components/`** — reservado para componentes reutilizáveis entre features.

Existem dois serviços de autenticação paralelos no cliente (`auth.service.ts` para clientes, `admin-auth.service.ts` para administradores), espelhando os dois fluxos JWT do backend. Toda a interface e as rotas públicas estão em português (`/loja`, `/carrinho`, `/minha-conta`, etc.), com `LOCALE_ID` fixado em `pt-BR`.

## Tecnologias

**Backend**
- .NET 10 / ASP.NET Core Minimal APIs
- Entity Framework Core + SQLite
- Autenticação JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt para hash de senhas
- Padrão Outbox com `BackgroundService` para eventos de domínio
- OpenAPI (Swagger) em ambiente de desenvolvimento

**Frontend**
- Angular 22 (standalone components, lazy loading)
- RxJS
- Bootstrap 5 + Bootstrap Icons
- Vitest (testes unitários)
- Prettier (formatação, incluindo parser Angular para templates HTML)

## Regras de negócio

### Produtos e estoque
- Todo produto tem nome, slug (único, gerado automaticamente a partir do nome, com sufixo aleatório em caso de colisão), categoria e preço obrigatórios; estoque nunca pode ser negativo.
- Produtos inativos (`Active = false`) não aparecem na listagem pública, apenas no painel admin.
- Reservar estoque (`Reserve`) ao fechar um pedido de loja falha se a quantidade solicitada exceder o estoque disponível.
- Sempre que o estoque de um produto cai para **3 unidades ou menos**, um evento de estoque baixo é disparado (hoje apenas logado via outbox).

### Pedidos
- Um pedido pode ser do tipo **Loja** (itens do catálogo) ou **Personalizado** (encomenda sob medida, com um único item representando o valor estimado e detalhes em JSON livre).
- Pedidos de loja exigem pelo menos um item; pedidos personalizados sempre têm exatamente um "item" (a encomenda em si).
- Itens só podem ser adicionados/alterados enquanto o pedido está no status inicial (`Recebido`) — depois disso, o pedido é imutável em termos de conteúdo.
- O status do pedido segue uma máquina de estados estrita, sem pular etapas nem retroceder:
  ```
  Recebido → EmProducao → Pronto → Enviado → Entregue
  Recebido, EmProducao, Pronto → Cancelado
  ```
  `Entregue` e `Cancelado` são estados finais — nenhuma transição é permitida a partir deles.
- Toda mudança de status válida dispara um evento de notificação ao cliente (`OrderStatusChangedDomainEvent`); a criação de um pedido dispara `OrderCreatedDomainEvent`.
- O total do pedido é sempre calculado dinamicamente como a soma dos subtotais dos itens (nunca armazenado/cacheado).

### Contas e autenticação
- Clientes se cadastram com e-mail único (validação de duplicidade antes do registro) e senha com **mínimo de 6 caracteres**; a senha é sempre armazenada com hash (BCrypt), nunca em texto plano.
- Login (tanto de cliente quanto de administrador) retorna uma mensagem genérica ("E-mail ou senha inválidos") tanto para e-mail inexistente quanto para senha incorreta, evitando enumeração de contas.
- Administradores não se autocadastram pela API pública — o único admin inicial é semeado na inicialização do banco (credenciais configuráveis via `AdminSeed:Email`/`AdminSeed:Password`).
- Tokens JWT carregam o papel do usuário (`Admin` ou `Customer`) e expiram após um tempo configurável (`Jwt:ExpiryMinutes`).

### Contato
- Mensagens de contato do site público são persistidas e disparam um evento de confirmação por e-mail ao remetente (via outbox); ficam disponíveis para consulta apenas no painel admin.

### Valores monetários
- Todo valor monetário é representado por um value object `Money` (moeda + quantia), sempre arredondado para 2 casas decimais, e nunca pode ser negativo.
