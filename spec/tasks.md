# Implementation Plan — Ateliê Layette Baby

Este plano reflete o que já está **implementado e verificado** no sistema (marcado `[x]`), organizado pelos requisitos de `requirements.md`. Serve como registro de rastreabilidade requisito → código, e como template para novas tarefas: ao planejar uma funcionalidade nova, adicione-a como `[ ]` na seção correspondente (ou crie uma nova seção) e referencie o(s) requisito(s) que ela atende.

- [x] 1. Domínio: entidades, value objects e eventos
  - [x] 1.1 Implementar `Entity`/`IAggregateRoot` e o mecanismo de eventos de domínio (`Domain/Common`)
  - [x] 1.2 Implementar `Money` e `Email` como value objects imutáveis com invariantes (Requisito 2, 5, 12)
  - [x] 1.3 Implementar `Product` com invariantes de estoque e evento de estoque baixo (Requisito 7)
  - [x] 1.4 Implementar `Order`/`OrderItem` com máquina de estados e imutabilidade pós-`Recebido` (Requisito 2, 8)
  - [x] 1.5 Implementar `Customer`, `Admin`, `ContactMessage` (Requisito 5, 6, 9)
  - [x] 1.6 Cobrir as regras acima com testes de unidade (`AtelieBebe.Domain.Tests`, 58 testes)

- [x] 2. Infraestrutura: persistência, outbox e segurança
  - [x] 2.1 Configurar `AppDbContext` + `IEntityTypeConfiguration` por entidade, migrations EF Core
  - [x] 2.2 Implementar repositórios e `UnitOfWork`
  - [x] 2.3 Implementar `DomainEventsToOutboxInterceptor` (grava evento na mesma transação) (Requisito 11)
  - [x] 2.4 Implementar `OutboxProcessor` (polling 5s, lote 20, retry até 5 tentativas) (Requisito 11)
  - [x] 2.5 Implementar `BCryptPasswordHasher` e `JwtTokenGenerator` (Requisito 5, 6, 12)
  - [x] 2.6 Mover o segredo JWT para `dotnet user-secrets`, fora do controle de versão (Requisito 12)
  - [x] 2.7 Implementar `DbInitializer` idempotente (seed de admin + catálogo, sem duplicar ao reexecutar)

- [x] 3. Aplicação: casos de uso
  - [x] 3.1 `ProductService` — listar/filtrar/CRUD/estoque/ativação (Requisito 1, 7)
  - [x] 3.2 `OrderService` — criar pedido de loja/personalizado, listar, mudar status (Requisito 2, 4, 8)
  - [x] 3.3 `CustomerAuthService` / `AdminAuthService` — registro e login (Requisito 5, 6)
  - [x] 3.4 `ContactService` — submissão e listagem de mensagens (Requisito 9)
  - [x] 3.5 `DashboardService` — agregação de métricas (Requisito 10)

- [x] 4. API: endpoints e composição
  - [x] 4.1 Mapear grupos de endpoints por feature (`Endpoints/*.cs`), públicos em `/api/*`, admin em `/api/admin/*`
  - [x] 4.2 Configurar autenticação JWT Bearer e policies `AdminOnly`/`CustomerOnly`
  - [x] 4.3 Configurar CORS a partir de `Cors:AllowedOrigins`
  - [x] 4.4 Implementar `AppExceptionHandler` central (mapa exceção → `ProblemDetails`) (Requisito 12)
  - [x] 4.5 Rodar migrations + seed automaticamente no startup

- [x] 5. Frontend: loja pública
  - [x] 5.1 `Home`, `Shop`, `ProductDetail` consumindo `ProductService` (Requisito 1)
  - [x] 5.2 `CartService` (signals + `localStorage`) com limite por estoque (Requisito 2)
  - [x] 5.3 `CartPage`, `Checkout` com validação por campo e criação de pedido (Requisito 2)
  - [x] 5.4 `OrderConfirmation`, `MyAccount` (Requisito 4)
  - [x] 5.5 `LoginPage`, `RegisterPage`, `AuthService` (Requisito 5)
  - [x] 5.6 Unificar contato + encomenda personalizada em `Contact`, com montagem de mensagem e link `wa.me` (Requisito 3)
  - [x] 5.7 Redesenhar layout visual de `Contact` (card "Outros canais", selos de confiança, alternador destacado)
  - [x] 5.8 Redirecionar `/encomenda-personalizada` → `/contato`; atualizar nav/footer/CTAs (Requisito 3)

- [x] 6. Frontend: painel administrativo
  - [x] 6.1 `AdminLogin`, `AdminAuthService`, `adminGuard` (Requisito 6)
  - [x] 6.2 `AdminProductList`, `AdminProductForm` com validação por campo (Requisito 7)
  - [x] 6.3 `AdminOrderList`, `AdminOrderDetail` com transição de status (Requisito 8)
  - [x] 6.4 `AdminContactMessages` (Requisito 9)
  - [x] 6.5 `AdminDashboard` consumindo `IDashboardService` (Requisito 10)

- [x] 7. Qualidade e acessibilidade transversal
  - [x] 7.1 Associar `label for`/`input id` em todos os formulários (Requisito 12 — usabilidade)
  - [x] 7.2 Substituir banner de erro genérico por validação por campo (`is-invalid`/`invalid-feedback`) em todos os formulários
  - [x] 7.3 Adicionar título de página (`Router.title`) e meta description dinâmica por rota
  - [x] 7.4 Adicionar `loading="lazy"` às imagens fora da dobra (grades de produto, carrinho, galeria)
  - [x] 7.5 Reduzir o CSS do Bootstrap aos partials realmente usados (~314 KB → ~270 KB)
  - [x] 7.6 Testes de frontend: `CartService`, `Contact` (mensagem WhatsApp), guards, `ProductService` (25 testes)

- [x] 8. Dados de catálogo
  - [x] 8.1 ~~Semear categorias e produtos iniciais (Bodies, Mantas, Saída de Maternidade, Kits Enxoval, Acessórios)~~ — descontinuado no item 8.3
  - [x] 8.2 ~~Adicionar produtos bordados nas categorias Roupinhas, Acessórios e Toalhas~~ — descontinuado no item 8.3
  - [x] 8.3 Especializar o catálogo exclusivamente em fraldas de ombro e boca (Requisito 1): remover do banco todo produto fora das categorias "Kit Ombro e Boca", "Fralda de Ombro" e "Fralda de Boca" (`DbInitializer` passa a limpar categorias descontinuadas a cada start) e semear os 9 produtos das 3 categorias novas. Reescrever home, sobre, rodapé, `index.html` e os tipos de peça do formulário de encomenda personalizada para refletir a especialização.

- [x] 9. Documentação e operação
  - [x] 9.1 `README.md` — arquitetura, tecnologias, regras de negócio, requisitos numerados (RF/RNF)
  - [x] 9.2 `CLAUDE.md` — guia de arquitetura e comandos para sessões futuras de agente
  - [x] 9.3 `spec/requirements.md`, `spec/design.md`, `spec/tasks.md` — este conjunto de documentos SDD
  - [x] 9.4 Repositório Git inicializado e publicado em `github.com/pauloffalves1/atelie-bebe`

- [x] 10. Paginação de listagens (Requisito 13 / RF26, design em `spec/design.md`)
  - [x] 10.1 Backend: criar `PagedResult<T>` e o helper de normalização de `page`/`pageSize` em `AtelieBebe.Application/Common`
  - [x] 10.2 Backend: paginar `IProductRepository.ListAsync` (`.Skip().Take()` + `.CountAsync()`) e propagar em `ProductService.ListAsync`
  - [x] 10.3 Backend: paginar `IOrderRepository.ListAsync` (admin) e propagar em `OrderService.ListAsync`
  - [x] 10.4 Backend: paginar `IContactMessageRepository.ListAsync` e propagar em `ContactService.ListAsync`
  - [x] 10.5 Backend: adicionar `page`/`pageSize` aos endpoints `GET /api/products`, `GET /api/admin/products`, `GET /api/admin/orders`, `GET /api/admin/contact-messages`, cada um com seu próprio default (12 público, 20 admin)
  - [x] 10.6 Backend: criar o projeto `AtelieBebe.Application.Tests` (xUnit) e cobrir `TotalPages`, normalização de `page`/`pageSize` fora do intervalo, e página além do fim (20 testes)
  - [x] 10.7 Frontend: criar `core/models/pagination.model.ts` (`PagedResult<T>`) e o componente `shared/components/pagination`
  - [x] 10.8 Frontend: atualizar `ProductService`, `OrderService` (admin) e `ContactService` para aceitar `page`/`pageSize` e retornar `PagedResult<T>`
  - [x] 10.9 Frontend: integrar paginação em `Shop` (com `?pagina=` na URL, reset ao trocar categoria)
  - [x] 10.10 Frontend: integrar paginação em `AdminProductList`, `AdminOrderList` (reset ao trocar status) e `AdminContactMessages`
  - [x] 10.11 Rodar `dotnet test` e `ng test` (suítes completas) e verificar manualmente no navegador/API as 4 listagens paginadas (páginas intermediárias, primeira, última, filtro trocando, página fora do intervalo)
  - [x] 10.12 Atualizar `README.md` (RF26) e `spec/requirements.md`/`spec/design.md` (remover as notas de "proposto") após a verificação

- [x] 11. Especialização do catálogo em fraldas de ombro e boca (Requisito 1, item 8)
  - [x] 11.1 Reescrever o seed do backend com as 3 categorias (Kit Ombro e Boca, Fralda de Ombro, Fralda de Boca) e remover as categorias antigas do banco a cada start
  - [x] 11.2 Reescrever textos do frontend (home, sobre, rodapé, `index.html`, tipos de peça da encomenda personalizada)
  - [x] 11.3 Atualizar documentação (README, CLAUDE.md, spec/*)

- [x] 12. Preenchimento automático de endereço via ViaCEP no checkout (Requisito 2, itens 11-13)
  - [x] 12.1 Criar `CepService`/`ViaCepAddress` (frontend) para consultar `viacep.com.br`
  - [x] 12.2 Escopar `authInterceptor` para só anexar o token Bearer a requisições da própria API — correção de segurança necessária antes de chamar qualquer domínio externo
  - [x] 12.3 `Checkout`: observar o campo CEP (debounced, 8 dígitos), preencher rua/bairro/cidade/estado, tratar CEP não encontrado
  - [x] 12.4 Reordenar o formulário de endereço para o CEP vir primeiro
  - [x] 12.5 Verificar no navegador com CEP real (preenchimento correto) e inválido (mensagem de erro, sem apagar os demais campos)

## Próximas tarefas (não iniciadas)

Use esta seção para novas funcionalidades planejadas. Nenhuma tarefa abaixo foi iniciada ainda.

- [x] 13. Produtos exclusivos por cliente + bordado personalizado (Requisitos 14 e 15, design em `spec/design.md`)
  - Backend — visibilidade (Requisito 14)
    - [x] 13.1 `Product` (Domain): `_allowedCustomerAccess`, `AllowedCustomerIds`, `IsExclusive`, `SetAllowedCustomers`, `HasAccess`; testes de domínio
    - [x] 13.2 EF Core: mapear `ProductCustomerAccess (ProductId, CustomerId)` em `ProductConfiguration`/`ProductCustomerAccessEntryConfiguration`; nova migration
    - [x] 13.3 `ICustomerRepository.ListAsync` (novo) + implementação
    - [x] 13.4 `IProductRepository`: `customerId` opcional em `ListAsync`, `GetBySlugAsync`, `ListCategoriesAsync`; filtro EXISTS na implementação (`Include("_allowedCustomerAccess")` para IsExclusive/AllowedCustomerIds refletirem o estado persistido)
    - [x] 13.5 `ProductService`/`IProductService`: propagar `customerId`; `ProductDto.IsExclusive`; `AdminProductDto` com `AllowedCustomerIds` (`GetForAdminAsync`, `SetAllowedCustomersAsync`)
    - [x] 13.6 Novo endpoint `GET /api/admin/customers` (`AdminOnly`) via `ICustomerAdminService`/`CustomerEndpoints`
    - [x] 13.7 Novo endpoint `PUT /api/admin/products/{id}/customers` (`AdminOnly`) → `Product.SetAllowedCustomers`
    - [x] 13.8 `ProductEndpoints`: ler `customerId` opcional de `http.User` (sem `RequireAuthorization`) em `/`, `/{slug}`, `/categories` via `ClaimsPrincipalExtensions.GetUserIdOrNull()`
  - Backend — bordado (Requisito 15)
    - [x] 13.9 `OrderService.CreateStoreOrderAsync`: repassar `itemRequest.OptionsJson` também para itens com `ProductId` (hoje só linhas avulsas recebem)
  - Frontend — visibilidade e admin (Requisito 14)
    - [x] 13.10 `Product` (model): `isExclusive`; `AdminProduct` com `allowedCustomerIds`
    - [x] 13.11 Novo `CustomerAdminService`/model `CustomerSummary`; `AdminProductForm`: seção "Acesso exclusivo" (checklist de clientes) salvando via `PUT .../customers`
    - [x] 13.12 Badge "Exclusivo pra você" em `Shop`/`Home` para produtos exclusivos visíveis
  - Frontend — bordado (Requisito 15)
    - [x] 13.13 `CartItem`/`CartService`: `embroideryText?`; chave de mesclagem `(productId, embroideryText)`; atualizar `cart.service.spec.ts`
    - [x] 13.14 `ProductDetail`: campo "Texto para bordar" (obrigatório se `isExclusive`); esconder botão de adicionar rápido para exclusivos nas grades (`Shop`/`Home`)
    - [x] 13.15 `Checkout`: montar `optionsJson` a partir de `item.embroideryText`; `CartPage`: exibir o texto do bordado por item
    - [x] 13.16 `AdminOrderDetail`: desserializar `OptionsJson` e exibir "Bordado: {texto}" quando presente
  - Verificação e documentação
    - [x] 13.17 `dotnet test`/`ng test` completos (63+20 backend, 28 frontend); verificado no navegador e via API: produto exclusivo aparece só pro cliente liberado (badge "Exclusivo pra você", campo de bordado), 404 + ausente das listagens/categorias/destaques pra visitante anônimo e pra outro cliente sem acesso; bordado propagado carrinho → checkout → pedido → exibido no admin ("Bordado: ANA")
    - [x] 13.18 Atualizar `README.md` (RF27, RF28) e remover as notas "proposto" de `spec/requirements.md`/`spec/design.md`

- [ ] 14. Notificações por WhatsApp (Requisito 16, design em `spec/design.md`) — código completo, bloqueado em 14.14 até a conta Meta existir
  - Domínio — telefone obrigatório
    - [x] 14.1 `Customer.Register`: validar `phone` obrigatório (`DomainException`); `Order.Create`: validar `customerPhone` obrigatório; testes de domínio para os dois casos de rejeição
    - [x] 14.2 `ContactMessage`: nova propriedade `Phone` (obrigatória, mesma validação); `ContactMessage.Create` ganha parâmetro `phone`; migration `AddContactMessagePhone`
    - [x] 14.3 Domain events: `OrderCreatedDomainEvent`, `OrderStatusChangedDomainEvent` (+ `CustomerName`), `CustomerRegisteredDomainEvent`, `ContactMessageReceivedDomainEvent` ganham campo de telefone; `Order.Submit()`/`ChangeStatus()`, `Customer.Register`, `ContactMessage.Create` repassam
  - Backend — canal WhatsApp
    - [x] 14.4 `INotificationSender`: adicionar parâmetro de telefone em `SendOrderCreatedAsync`/`SendOrderStatusChangedAsync`/`SendContactAcknowledgementAsync` (e-mail removido — não é mais o canal); renomeado `SendWelcomeEmailAsync` → `SendWelcomeMessageAsync` (+ telefone). `SendLowStockAlertAsync` existia sem mudança de assinatura, depois removido por completo na tarefa 15
    - [x] 14.5 `WhatsAppOptions` (`AccessToken`, `PhoneNumberId`, `ApiVersion`, `AdminPhoneNumber`) bound via `IOptions<>`; seção `WhatsApp` em branco no `appsettings.json`, real via `dotnet user-secrets`
    - [x] 14.6 `WhatsAppPhoneFormatter`: normaliza telefone para E.164 (heurística de dígitos + prefixo `55`)
    - [x] 14.7 `WhatsAppNotificationSender : INotificationSender` (HttpClient tipado) — `POST /{phoneNumberId}/messages` na Graph API com templates (`pedido_recebido`, `pedido_status_atualizado`, `boas_vindas_cliente`, `confirmacao_contato` — `alerta_estoque_baixo` existia aqui, removido na tarefa 15 junto com o controle de estoque); lança exceção clara se `AccessToken`/`PhoneNumberId` vazios
    - [x] 14.8 `AddInfrastructure`: registrar `WhatsAppOptions` + `AddHttpClient<INotificationSender, WhatsAppNotificationSender>()` no lugar de `LoggingNotificationSender` (pacote `Microsoft.Extensions.Http` adicionado ao `.csproj`)
    - [x] 14.9 `OutboxProcessor.DispatchAsync`: repassar telefone (e nome) de cada evento ao `INotificationSender`
    - [x] 14.10 `RegisterCustomerRequest`/`CreateStoreOrderRequest`/`CreateCustomOrderRequest`/`SubmitContactRequest`: `Phone` continua `string?` no DTO, rejeição por ausência acontece no Domain; `ContactService.SubmitAsync`/`SubmitContactRequest`/`ContactMessageDto` ganham `Phone`
  - Frontend — telefone obrigatório
    - [x] 14.11 `register-page`: campo "Telefone (opcional)" → "Telefone / WhatsApp" com `Validators.required` + mensagem de erro
    - [x] 14.12 `checkout`: campo "Telefone / WhatsApp" ganha `Validators.required` + `invalid-feedback`
  - Verificação e documentação
    - [x] 14.13 `dotnet test`/`ng test` completos (69+20 backend, 28 frontend); verificado via API: cadastro/pedido/contato sem telefone rejeitados com 400 e mensagem clara; pedido válido com telefone é aceito (200) e a falha de envio (sem credencial configurada) fica isolada no outbox (`Attempts`/`Error`, até 5 tentativas), sem afetar a criação do pedido
    - [ ] 14.14 **Bloqueado até o administrador criar a conta Meta WhatsApp Business Cloud API, obter `AccessToken`/`PhoneNumberId` e ter os 4 templates aprovados** (`alerta_estoque_baixo` foi removido junto com o controle de estoque, tarefa 15) — só então é possível verificar o envio real de ponta a ponta; até lá, o `WhatsAppNotificationSender` está implementado e verificado até a chamada HTTP (falha limpa e isolada quando não configurado), mas nenhuma mensagem real foi enviada ainda
    - [ ] 14.15 Atualizar `README.md` (RF29) e remover a nota "proposto" de `spec/requirements.md`/`spec/design.md` — **fazer só depois de 14.14**, quando o envio real for confirmado

- [x] 15. Remover controle de estoque — negócio é feito sob encomenda (Requisitos 2, 7, 10, 11; RF15/RF21/RF22 removidos do README)
  - Backend
    - [x] 15.1 `Product` (Domain): remover `Stock`, `LowStockThreshold`, `SetStock`, `Reserve`, `RaiseLowStockEventIfNeeded`; `Create`/`UpdateDetails` sem parâmetro de estoque
    - [x] 15.2 Remover `ProductLowStockDomainEvent`; `OutboxProcessor.DispatchAsync` sem o case desse evento
    - [x] 15.3 `INotificationSender`/`LoggingNotificationSender`/`WhatsAppNotificationSender`/`WhatsAppOptions`: remover `SendLowStockAlertAsync` e `AdminPhoneNumber` (sem destinatário fixo a manter)
    - [x] 15.4 `ProductDtos`/`IProductService`/`ProductService`/`ProductEndpoints`: remover `Stock` de `ProductDto`/`AdminProductDto`/`CreateProductRequest`, remover `UpdateStockRequest`/`UpdateStockAsync`/`PATCH .../stock`
    - [x] 15.5 `OrderService.CreateStoreOrderAsync`: remover `product.Reserve(...)` — item de catálogo é sempre aceito, sem checagem de disponibilidade
    - [x] 15.6 `DbInitializer`: seed sem `Stock`; `DashboardDto`/`DashboardService`: remover `LowStockProducts`
    - [x] 15.7 Migration `RemoveProductStock` (`DropColumn Stock` em `Products`)
    - [x] 15.8 `ProductTests.cs`: remover testes de `Reserve`/`SetStock`/evento de estoque baixo; `dotnet test` completo (61+20)
  - Frontend
    - [x] 15.9 `Product`/`AdminProduct`/`CreateProductRequest` (model): remover `stock`; `ProductService`: remover `updateStock()`
    - [x] 15.10 `CartService.add()`: remover o `Math.min(quantity, product.stock)` — quantidade nunca é limitada; `cart.service.spec.ts` atualizado
    - [x] 15.11 `Shop`/`ProductDetail`: remover badges "Últimas unidades"/"Esgotado", `[disabled]`/`[max]` baseados em estoque; `ProductDetail` sempre mostra os controles de compra
    - [x] 15.12 `CartPage`: botão de incrementar quantidade sem limite de estoque
    - [x] 15.13 `AdminProductForm`/`AdminProductList`/`AdminDashboard`: remover campo/coluna/card de estoque
  - Documentação
    - [x] 15.14 `dotnet test`/`ng test`/build de produção do Angular confirmados; `README.md` (RF15/RF21/RF22 marcados removidos, seção "Catálogo e estoque" → "Catálogo"), `spec/requirements.md` (Requisitos 2, 7, 10, 11, 16 atualizados) e `spec/design.md` (modelo de dados, diagramas, tabela de templates WhatsApp) atualizados

- [x] 16. Estender bordado personalizado para todos os produtos, não só exclusivos (Requisito 15)
  - [x] 16.1 `ProductDetail`: campo "Texto para bordar" + teclado de alfabeto sempre visíveis (removido o `@if (p.isExclusive)`); `addToCart()` exige bordado para qualquer produto, não só exclusivo
  - [x] 16.2 `Shop`/`Home`: removido o botão de "adicionar rápido" para produtos não exclusivos — todo card agora usa o link "Personalizar" para `ProductDetail`; `addToCart()`/injeção de `CartService` removidos de `Shop`/`Home` (ficaram sem uso)

- [x] 17. CPF no cadastro de cliente (Requisito 17 / RF30, design em `spec/design.md`)
  - Backend
    - [x] 17.1 Novo value object `Cpf` (Domain/ValueObjects): normaliza dígitos, valida comprimento/sequência repetida/dígitos verificadores (módulo 11); `CpfTests.cs`
    - [x] 17.2 `Customer`: nova propriedade `Cpf?` (nullable — não retroativo); `Register(name, email, cpf, passwordHash, phone)` exige `Cpf` não nulo; `CustomerTests.cs` atualizado
    - [x] 17.3 `CustomerConfiguration`: coluna `Cpf` (nullable, conversor null-safe) + índice único `IX_Customers_Cpf`; migration `AddCustomerCpf`
    - [x] 17.4 `ICustomerRepository`/`CustomerRepository`: novo `CpfExistsAsync`
    - [x] 17.5 `RegisterCustomerRequest` ganha `Cpf` (obrigatório); `CustomerAuthService.RegisterAsync` valida formato (`Cpf.Create`, 400) e unicidade (`CpfExistsAsync`, 409) antes de criar a conta
  - Frontend
    - [x] 17.6 `register-page`: novo campo "CPF" (`Validators.required` + `Validators.pattern`) entre e-mail e telefone; `RegisterCustomerRequest`/`auth.service` repassam o campo
  - Verificação e documentação
    - [x] 17.7 `dotnet test`/`ng test` completos (71+20 backend, 26 frontend); verificado via API: CPF válido aceito (200), CPF com dígito verificador errado rejeitado (400, mensagem clara), CPF duplicado rejeitado (409); migration aplicada localmente sem quebrar clientes existentes (coluna nullable)
    - [x] 17.8 `README.md` (RF30), `spec/requirements.md` (Requisito 17) e `spec/design.md` (Requisito 17) atualizados
  - Máscara de telefone/WhatsApp (transversal, não numerado como requisito próprio)
    - [x] 17.9 Novo `PhoneMaskDirective` (`shared/directives/phone-mask.directive.ts`); aplicado em `register-page`, `checkout` e `contact`

- [x] 18. Listagem de clientes no admin (Requisito 18 / RF31, design em `spec/design.md`)
  - [x] 18.1 `CustomerSummaryDto`/`CustomerSummary` ganham `Phone`/`Cpf`/`CreatedAt` (campos aditivos — `ICustomerAdminService`/endpoint continuam sem paginação, reaproveitados do seletor de clientes exclusivos do Requisito 14)
  - [x] 18.2 Nova tela `admin-customer-list` (`/admin/clientes`) + link no menu lateral do admin
  - [x] 18.3 `dotnet test`/`ng test` completos (71+20 backend, 26 frontend); verificado visualmente: tela lista clientes com `—` para telefone/CPF ausente; seletor de clientes exclusivos (Requisito 14) continua funcionando sem regressão
  - [x] 18.4 `README.md` (RF31) e `spec/requirements.md`/`spec/design.md` (Requisito 18) atualizados

- [x] 19. CPF obrigatório no checkout (Requisito 19 / RF32, design em `spec/design.md`)
  - Backend
    - [x] 19.1 `Order`: nova propriedade `CustomerCpf` (`Cpf?`, nullable — não retroativo, mesmo padrão do Requisito 17); `Order.Create(...)` exige `Cpf` não nulo; `OrderTests.cs` atualizado (`Create_WithoutCpf_Throws`)
    - [x] 19.2 `OrderConfiguration`: coluna `CustomerCpf` (nullable, conversor null-safe); migration `AddOrderCpf`
    - [x] 19.3 `CreateStoreOrderRequest`/`CreateCustomOrderRequest` ganham `CustomerCpf` (obrigatório); `OrderService` chama `Cpf.Create(request.CustomerCpf)` antes de `Order.Create`; `OrderDto.CustomerCpf` (`string?`)
  - Frontend
    - [x] 19.4 `checkout.ts`/`.html`: novo campo "CPF" (`Validators.required` + `Validators.pattern`, mesmo padrão do cadastro) enviado em `CreateStoreOrderRequest`
  - Verificação e documentação
    - [x] 19.5 `dotnet test`/`ng test` completos (72+20 backend, 27 frontend); migration aplicada localmente sem quebrar pedidos existentes (coluna nullable)
    - [x] 19.6 `README.md` (RF32), `spec/requirements.md` (Requisito 19) e `spec/design.md` (Requisito 19) atualizados

- [x] 20. Login ou cadastro obrigatório para finalizar a compra (Requisito 20 / RF33, design em `spec/design.md`)
  - [x] 20.1 `app.routes.ts`: rota `checkout` ganha `canActivate: [customerGuard]`; `customer.guard.ts` passa a preservar `returnUrl` (`state.url`) na `UrlTree` de redirecionamento; `customer.guard.spec.ts` atualizado
  - [x] 20.2 `login-page.ts`/`register-page.ts`: signal `returnUrl` lido da query string, usado em `navigateByUrl` no sucesso (em vez do destino fixo `/minha-conta`); `login-page.html`/`register-page.html`: link cruzado entre as duas telas propaga `returnUrl`; alerta contextual quando `returnUrl === '/checkout'`
  - [x] 20.3 `ng test` completo (27 frontend); verificado no navegador: `/checkout` sem sessão redireciona para `/entrar?returnUrl=/checkout`, link "Cadastre-se" preserva o parâmetro, cadastro concluído retorna a `/checkout` com o carrinho intacto
  - [x] 20.4 `README.md` (RF33) e `spec/requirements.md`/`spec/design.md` (Requisito 20) atualizados

- [x] 21. Pré-preenchimento de dados no checkout (Requisito 21 / RF34, design em `spec/design.md`)
  - Backend
    - [x] 21.1 `CustomerProfileDto`; `ICustomerAuthService`/`CustomerAuthService.GetProfileAsync` (busca por `GetByIdAsync`, 404 se não encontrado); `GET /api/auth/me` (`CustomerOnly`) em `AuthEndpoints`
  - Frontend
    - [x] 21.2 `AuthService.getProfile()`; `checkout.ts` chama `getProfile()` para preencher nome/e-mail/telefone/CPF, e `OrderService.listMine()` para achar o pedido mais recente com `shippingAddressJson` e preencher CEP/rua/número/complemento/bairro/cidade/estado
  - Verificação e documentação
    - [x] 21.3 `dotnet test`/`ng test` completos (72+20 backend, 27 frontend); verificado no navegador: pedido de teste criado com endereço, checkout seguinte já veio com todos os campos (dados + endereço) preenchidos e editáveis
    - [x] 21.4 `README.md` (RF34) e `spec/requirements.md`/`spec/design.md` (Requisito 21) atualizados
  - [x] 16.3 `spec/requirements.md` (Requisito 15: user story e critérios reescritos, nota de histórico da mudança de escopo) e `spec/design.md` (seção "Frontend — bordado em todos os produtos") atualizados

- [x] 22. Cálculo de frete no checkout (Requisito 22 / RF35, design em `spec/design.md`)
  - Backend
    - [x] 22.1 `Order`: nova propriedade `ShippingCost` (`Money`, default zero); `ItemsTotal` extraído do antigo cálculo de `Total`; `Total = ItemsTotal + ShippingCost`; `Order.Create(...)` ganha `shippingCost` opcional; `OrderTests.cs` atualizado (`Total_WithoutShippingCost_DefaultsToZero`, `Total_IncludesShippingCostOnTopOfItemsTotal`)
    - [x] 22.2 `OrderConfiguration`: coluna `ShippingCostAmount` (`decimal(18,2)`, `DEFAULT 0`, não retroativo); migration `AddOrderShippingCost`; `builder.Ignore(o => o.ItemsTotal)`
    - [x] 22.3 `CreateStoreOrderRequest` ganha `ShippingCost` (obrigatório, calculado e enviado pelo frontend); `OrderDto` ganha `ItemsTotal`/`ShippingCost`
  - Frontend
    - [x] 22.4 Novo `ShippingService` (`core/services/shipping.service.ts`) — estimativa por faixa de UF + acréscimo por item, sem chamada HTTP (não usa API oficial dos Correios, que exigiria contrato/credenciais e peso/dimensão por produto, nenhum dos dois existente hoje)
    - [x] 22.5 `checkout.ts`/`.html`: signal `destinationState` + `shippingCost` computado a partir do estado e da quantidade de itens; resumo do pedido mostra subtotal/frete/total separados; `shippingCost` enviado em `createStoreOrder(...)`
    - [x] 22.6 `order-confirmation.html`/`admin-order-detail.html`: mostram subtotal/frete quando `shippingCost > 0`
  - Verificação e documentação
    - [x] 22.7 `dotnet test`/`ng test` completos (74+20 backend, 27 frontend); verificado no navegador: checkout com UF=SP mostrou frete R$12,90, pedido confirmado persistiu e exibiu Subtotal/Frete/Total corretamente na confirmação; migration aplicada localmente sem quebrar pedidos existentes (coluna com default 0)
    - [x] 22.8 `README.md` (RF35) e `spec/requirements.md`/`spec/design.md` (Requisito 22) atualizados

- [x] 23. CPF mascarado nas telas administrativas (Requisito 23 / RF36, design em `spec/design.md`)
  - [x] 23.1 Novo `CpfMaskPipe` (`shared/pipes/cpf-mask.pipe.ts`); `cpf-mask.pipe.spec.ts` (CPF cru, formatado, nulo/vazio, inválido)
  - [x] 23.2 Aplicado via `| cpfMask` em `admin-customer-list.html` e `admin-order-detail.html` — únicos pontos do frontend que exibem CPF fora de formulário; API continua retornando o CPF completo sem máscara
  - [x] 23.3 `ng test` completo (31 frontend); verificado no navegador: `/admin/clientes` e `/admin/encomendas/:id` mostram `***.XXX.XXX-**`, cliente sem CPF continua mostrando `—`
  - [x] 23.4 `README.md` (RF36) e `spec/requirements.md`/`spec/design.md` (Requisito 23) atualizados
