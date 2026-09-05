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
    - [x] 14.4 `INotificationSender`: adicionar parâmetro de telefone em `SendOrderCreatedAsync`/`SendOrderStatusChangedAsync`/`SendContactAcknowledgementAsync` (e-mail removido — não é mais o canal); renomeado `SendWelcomeEmailAsync` → `SendWelcomeMessageAsync` (+ telefone). `SendLowStockAlertAsync` sem mudança de assinatura
    - [x] 14.5 `WhatsAppOptions` (`AccessToken`, `PhoneNumberId`, `ApiVersion`, `AdminPhoneNumber`) bound via `IOptions<>`; seção `WhatsApp` em branco no `appsettings.json`, real via `dotnet user-secrets`
    - [x] 14.6 `WhatsAppPhoneFormatter`: normaliza telefone para E.164 (heurística de dígitos + prefixo `55`)
    - [x] 14.7 `WhatsAppNotificationSender : INotificationSender` (HttpClient tipado) — `POST /{phoneNumberId}/messages` na Graph API com os 5 templates (`pedido_recebido`, `pedido_status_atualizado`, `boas_vindas_cliente`, `confirmacao_contato`, `alerta_estoque_baixo`); lança exceção clara se `AccessToken`/`PhoneNumberId` vazios
    - [x] 14.8 `AddInfrastructure`: registrar `WhatsAppOptions` + `AddHttpClient<INotificationSender, WhatsAppNotificationSender>()` no lugar de `LoggingNotificationSender` (pacote `Microsoft.Extensions.Http` adicionado ao `.csproj`)
    - [x] 14.9 `OutboxProcessor.DispatchAsync`: repassar telefone (e nome) de cada evento ao `INotificationSender`
    - [x] 14.10 `RegisterCustomerRequest`/`CreateStoreOrderRequest`/`CreateCustomOrderRequest`/`SubmitContactRequest`: `Phone` continua `string?` no DTO, rejeição por ausência acontece no Domain; `ContactService.SubmitAsync`/`SubmitContactRequest`/`ContactMessageDto` ganham `Phone`
  - Frontend — telefone obrigatório
    - [x] 14.11 `register-page`: campo "Telefone (opcional)" → "Telefone / WhatsApp" com `Validators.required` + mensagem de erro
    - [x] 14.12 `checkout`: campo "Telefone / WhatsApp" ganha `Validators.required` + `invalid-feedback`
  - Verificação e documentação
    - [x] 14.13 `dotnet test`/`ng test` completos (69+20 backend, 28 frontend); verificado via API: cadastro/pedido/contato sem telefone rejeitados com 400 e mensagem clara; pedido válido com telefone é aceito (200) e a falha de envio (sem credencial configurada) fica isolada no outbox (`Attempts`/`Error`, até 5 tentativas), sem afetar a criação do pedido
    - [ ] 14.14 **Bloqueado até o administrador criar a conta Meta WhatsApp Business Cloud API, obter `AccessToken`/`PhoneNumberId` e ter os 5 templates aprovados** — só então é possível verificar o envio real de ponta a ponta; até lá, o `WhatsAppNotificationSender` está implementado e verificado até a chamada HTTP (falha limpa e isolada quando não configurado), mas nenhuma mensagem real foi enviada ainda
    - [ ] 14.15 Atualizar `README.md` (RF29) e remover a nota "proposto" de `spec/requirements.md`/`spec/design.md` — **fazer só depois de 14.14**, quando o envio real for confirmado
