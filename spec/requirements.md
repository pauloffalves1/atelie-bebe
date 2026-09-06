# Requirements Document — Ateliê Layette Baby

## Introdução

O Ateliê Layette Baby é uma plataforma de e-commerce e gestão de encomendas para um ateliê especializado em fraldas de ombro e boca bordadas (individuais ou em kit) — a técnica de bordado (à mão ou computadorizado) varia por produto, descrita individualmente no catálogo em vez de assumida globalmente. Este documento formaliza, no padrão *Spec-Driven Development* (user story + critérios de aceite em EARS — Easy Approach to Requirements Syntax), os requisitos do sistema **tal como construído**. Ele complementa (não substitui) `README.md`, que mantém a tabela de requisitos numerados (RF01–RF26, RNF01–RNF08) usada como referência rápida — cada requisito abaixo cita o(s) RF/RNF correspondente(s) para rastreabilidade.

Quatro atores participam do sistema: **Visitante** (não autenticado), **Cliente** (autenticado com papel `customer`), **Administrador** (autenticado com papel `admin`) e **Sistema** (comportamentos automáticos, sem ator humano direto).

---

## Requisito 1: Navegação pelo catálogo público

**User Story:** Como visitante, quero navegar pelo catálogo de produtos, para conhecer as peças disponíveis antes de decidir comprar.

**Rastreamento:** RF01, RF02, RF03, RF04

**Acceptance Criteria**
1. QUANDO um visitante acessa a loja SEM filtro de categoria, O SISTEMA DEVE listar todos os produtos com `Active = true`.
2. QUANDO um visitante seleciona uma categoria, O SISTEMA DEVE retornar apenas produtos ativos dessa categoria.
3. QUANDO um visitante acessa a home, O SISTEMA DEVE exibir os produtos com `Featured = true`.
4. QUANDO um visitante solicita a lista de categorias, O SISTEMA DEVE retornar as categorias distintas presentes no catálogo.
5. QUANDO um visitante acessa um produto por slug válido, O SISTEMA DEVE exibir nome, categoria, preço, descrição e imagem.
6. SE o slug não corresponder a nenhum produto, ENTÃO O SISTEMA DEVE responder com 404 e a UI DEVE exibir uma página "produto não encontrado" com um link de volta à loja.
7. O SISTEMA NUNCA DEVE incluir produtos com `Active = false` nas listagens públicas (loja, destaque, busca por categoria).
8. O catálogo é especializado exclusivamente em fraldas de ombro e boca — as únicas categorias vendidas são "Kit Ombro e Boca", "Fralda de Ombro" e "Fralda de Boca". O SISTEMA DEVE remover qualquer produto fora dessas categorias (ex.: restaurado de um backup antigo com o catálogo genérico anterior) em vez de exibi-lo.

---

## Requisito 2: Carrinho e checkout de loja

**User Story:** Como visitante ou cliente, quero adicionar produtos a um carrinho e finalizar a compra, para receber os itens escolhidos.

**Rastreamento:** RF05, RF25, RNF07

**Acceptance Criteria**
1. QUANDO o carrinho é persistido, O CLIENTE DEVE gravá-lo em `localStorage`, sobrevivendo a recarregamentos de página.
2. QUANDO um usuário confirma o checkout, O SISTEMA DEVE criar um pedido do tipo `Loja` com um item por produto do carrinho.
3. SE o pedido de loja não tiver nenhum item, ENTÃO O SISTEMA DEVE rejeitá-lo (`ConflictException`/`DomainException`).
4. QUANDO a requisição de checkout parte de um cliente autenticado, O SISTEMA DEVE vincular o pedido criado ao `CustomerId` desse cliente.
5. QUANDO a requisição de checkout parte de um visitante não autenticado, O SISTEMA DEVE aceitar o pedido mesmo assim, com `CustomerId = null`.
6. A gravação do pedido e o registro do evento `OrderCreatedDomainEvent` na tabela de outbox DEVEM ocorrer na mesma transação de banco de dados.
7. QUANDO o pedido está no status `Recebido`, O SISTEMA PODE aceitar adição de itens; APÓS o pedido sair do status `Recebido`, O SISTEMA DEVE rejeitar qualquer tentativa de adicionar ou alterar itens.
8. Não há controle de estoque: todo produto é fabricado sob encomenda, então a quantidade escolhida pelo cliente nunca é limitada por disponibilidade prévia (ver nota sobre a remoção do Requisito 7/RF15/RF21/RF22 abaixo).
11. QUANDO o usuário digita um CEP com 8 dígitos no campo de endereço do checkout, O CLIENTE (frontend) DEVE consultar a API pública ViaCEP e, em caso de sucesso, preencher automaticamente rua, bairro, cidade e estado — mantendo os campos editáveis para ajuste manual.
12. SE o CEP informado não for encontrado pela ViaCEP, ENTÃO O CLIENTE DEVE exibir uma mensagem de erro no campo de CEP, sem apagar os demais campos do endereço.
13. O token de autenticação do cliente/administrador NUNCA DEVE ser enviado em requisições a domínios de terceiros (ex.: ViaCEP) — apenas para a própria API do backend.

---

## Requisito 3: Contato e encomenda personalizada via WhatsApp

**User Story:** Como visitante ou cliente, quero enviar uma dúvida geral ou uma solicitação de encomenda personalizada, para iniciar uma conversa direta com o ateliê.

**Rastreamento:** RF06

**Acceptance Criteria**
1. A página de contato DEVE apresentar um único formulário cobrindo tanto dúvidas gerais quanto encomendas personalizadas, alternados por um controle "É uma encomenda personalizada?".
2. QUANDO o alternador de encomenda personalizada está ativo, A UI DEVE exibir campos adicionais: tipo de peça, tamanho, tecido, cor e nome para bordar (opcional).
3. QUANDO o usuário envia o formulário com nome e mensagem preenchidos, O CLIENTE (frontend) DEVE montar uma mensagem de texto com os dados informados e abrir `https://wa.me/<número-do-ateliê>?text=<mensagem-codificada>` em uma nova aba.
4. SE o campo nome OU o campo mensagem estiverem vazios, ENTÃO O SISTEMA NÃO DEVE abrir o WhatsApp e DEVE exibir a mensagem de validação correspondente no campo afetado.
5. QUANDO e-mail ou telefone são informados, O SISTEMA DEVE incluí-los ao final da mensagem montada; QUANDO não são informados, O SISTEMA NÃO DEVE incluir essas linhas.
6. QUANDO um cliente autenticado abre a página de contato, O SISTEMA DEVE pré-preencher automaticamente os campos nome e e-mail com os dados da conta.
7. Este fluxo NÃO DEVE realizar nenhuma chamada à API do backend — nenhuma mensagem de contato nem pedido é persistido a partir desta tela.
8. A rota antiga `/encomenda-personalizada` DEVE redirecionar para `/contato`, preservando links e favoritos existentes.

---

## Requisito 4: Consulta e confirmação de pedidos

**User Story:** Como visitante, cliente ou administrador, quero consultar o status e os detalhes de um pedido, para acompanhar seu andamento.

**Rastreamento:** RF07, RF08

**Acceptance Criteria**
1. QUANDO qualquer usuário (autenticado ou não) consulta um pedido por ID válido, O SISTEMA DEVE retornar seus detalhes (status, itens, total, dados do cliente) sem exigir autenticação.
2. SE o ID do pedido não existir, ENTÃO O SISTEMA DEVE responder com 404.
3. QUANDO um cliente autenticado acessa "Minha conta", O SISTEMA DEVE listar somente os pedidos vinculados ao seu `CustomerId`, ordenados do mais recente para o mais antigo.
4. O total exibido de um pedido DEVE ser sempre recalculado como a soma de `preço unitário × quantidade` de cada item no momento da leitura — nunca um valor armazenado.

---

## Requisito 5: Conta de cliente

**User Story:** Como visitante, quero criar uma conta e fazer login, para acompanhar meus pedidos e agilizar futuras compras.

**Rastreamento:** RF09, RF10, RNF02

**Acceptance Criteria**
1. QUANDO um visitante se cadastra com um e-mail já usado por outra conta, O SISTEMA DEVE rejeitar o cadastro com um erro de conflito.
2. SE a senha informada no cadastro tiver menos de 6 caracteres, ENTÃO O SISTEMA DEVE rejeitar o cadastro.
3. QUANDO um cadastro é aceito, O SISTEMA DEVE armazenar a senha apenas como hash (BCrypt), nunca em texto plano.
4. QUANDO um cadastro é concluído com sucesso, O SISTEMA DEVE autenticar automaticamente o novo cliente e retornar um token JWT com papel `customer`.
5. QUANDO um cliente faz login com e-mail e senha corretos, O SISTEMA DEVE retornar um token JWT válido.
6. SE o e-mail não existir OU a senha estiver incorreta, ENTÃO O SISTEMA DEVE responder com 401 e a mensagem genérica "E-mail ou senha inválidos" em ambos os casos, sem revelar qual dos dois está errado.

---

## Requisito 6: Autenticação de administrador

**User Story:** Como administrador, quero fazer login no painel administrativo, para gerenciar produtos, pedidos e mensagens.

**Rastreamento:** RF11, RNF03

**Acceptance Criteria**
1. QUANDO um administrador faz login com credenciais corretas, O SISTEMA DEVE retornar um token JWT com papel `admin`.
2. SE as credenciais estiverem incorretas, ENTÃO O SISTEMA DEVE responder com 401 e a mesma mensagem genérica usada no login de cliente.
3. O SISTEMA NÃO DEVE expor nenhuma rota pública de autocadastro de administrador — o único admin é criado por semeadura na inicialização do banco.
4. QUANDO uma requisição a uma rota `/api/admin/*` (exceto o próprio login) não apresenta um token JWT válido com papel `admin`, O SISTEMA DEVE responder com 401/403.
5. QUANDO o usuário acessa uma rota `/admin/*` no frontend sem sessão de admin válida, O GUARD de rota DEVE redirecioná-lo para `/admin/login`.

---

## Requisito 7: Gestão de produtos (administrador)

**User Story:** Como administrador, quero cadastrar, editar e controlar a visibilidade dos produtos, para manter o catálogo atualizado.

**Rastreamento:** RF12, RF13, RF14, RF16

> **Nota (removido):** este ateliê não mantém estoque físico — todo produto é fabricado sob encomenda a partir da compra. Os antigos RF15 ("ajustar estoque"), RF21 ("reservar estoque no pedido") e RF22 ("evento de estoque baixo") foram removidos do sistema; os números RF15/RF21/RF22 ficam propositalmente vagos na tabela do README em vez de renumerados, para não invalidar referências antigas.

**Acceptance Criteria**
1. QUANDO um administrador lista produtos, O SISTEMA DEVE incluir tanto ativos quanto inativos (diferente da listagem pública).
2. QUANDO um administrador cadastra um novo produto, O SISTEMA DEVE gerar um slug a partir do nome; SE o slug colidir com um existente, ENTÃO O SISTEMA DEVE adicionar um sufixo aleatório para garantir unicidade.
3. SE nome, slug OU categoria estiverem vazios, ENTÃO O SISTEMA DEVE rejeitar a criação/edição do produto.
4. QUANDO um administrador edita os dados de um produto, O SISTEMA DEVE atualizar nome, descrição, preço, categoria, imagem e destaque.
5. QUANDO um administrador ativa ou inativa um produto, O SISTEMA DEVE refletir imediatamente essa mudança na visibilidade da loja pública.

---

## Requisito 8: Gestão de pedidos (administrador)

**User Story:** Como administrador, quero listar pedidos e atualizar seus status, para conduzir o fluxo de produção e entrega.

**Rastreamento:** RF17, RF18

**Acceptance Criteria**
1. QUANDO um administrador lista pedidos sem filtro, O SISTEMA DEVE retornar todos, ordenados do mais recente para o mais antigo.
2. QUANDO um administrador filtra por status, O SISTEMA DEVE retornar somente pedidos naquele status.
3. A transição de status DEVE seguir estritamente o mapa: `Recebido → EmProducao → Pronto → Enviado → Entregue`, com `Recebido`, `EmProducao` e `Pronto` também podendo transicionar para `Cancelado`.
4. SE uma transição solicitada não constar no mapa de transições permitidas a partir do status atual, ENTÃO O SISTEMA DEVE rejeitá-la com um erro de domínio.
5. `Entregue` e `Cancelado` SÃO estados terminais — nenhuma transição posterior DEVE ser aceita a partir deles.
6. QUANDO uma transição de status é aceita, O SISTEMA DEVE emitir `OrderStatusChangedDomainEvent` para notificar o cliente.

---

## Requisito 9: Mensagens de contato (administrador)

**User Story:** Como administrador, quero consultar mensagens de contato recebidas, para responder dúvidas de clientes.

**Rastreamento:** RF19

**Acceptance Criteria**
1. QUANDO uma mensagem de contato é submetida via `POST /api/contact` (canal reservado, não usado pela UI pública atual — ver Requisito 3), O SISTEMA DEVE persisti-la e emitir `ContactMessageReceivedDomainEvent`.
2. QUANDO um administrador lista mensagens de contato, O SISTEMA DEVE retorná-las ordenadas da mais recente para a mais antiga.
3. Mensagens de contato NÃO DEVEM ficar visíveis a nenhum usuário fora do papel `admin`.

---

## Requisito 10: Painel administrativo (dashboard)

**User Story:** Como administrador, quero ver um resumo consolidado do negócio, para acompanhar a saúde operacional do ateliê de relance.

**Rastreamento:** RF20

**Acceptance Criteria**
1. O painel DEVE exibir: total de pedidos, pedidos em aberto, receita total, receita do mês, total de produtos, total de clientes, distribuição de pedidos por status e os pedidos mais recentes.
2. Pedidos com status `Cancelado` NÃO DEVEM ser contabilizados em nenhuma métrica de receita nem na contagem de "pedidos em aberto".
3. "Pedidos em aberto" DEVE contar pedidos em `Recebido`, `EmProducao`, `Pronto` ou `Enviado`.
4. "Receita do mês" DEVE somar apenas pedidos criados a partir do primeiro dia do mês corrente, calculado em UTC.

---

## Requisito 11: Comportamentos automáticos do sistema

**User Story:** Como sistema, preciso reagir automaticamente a eventos de negócio (pedidos, clientes, mensagens), para manter consistência de dados e manter os envolvidos informados, sem depender de ação manual.

**Rastreamento:** RF23, RF24, RNF06, RNF07

**Acceptance Criteria**
1. QUANDO uma entidade de domínio levanta um evento (criação de pedido, mudança de status, cadastro de cliente, mensagem de contato recebida), O SISTEMA DEVE gravar esse evento na tabela de outbox na MESMA transação que originou a mudança de estado.
2. Um processo em segundo plano DEVE consultar mensagens pendentes da outbox a cada 5 segundos, em lotes de até 20.
3. QUANDO o despacho de uma mensagem de outbox falha, O SISTEMA DEVE incrementar seu contador de tentativas e registrar o erro, sem interromper o processamento das demais mensagens.
4. QUANDO uma mensagem de outbox atinge 5 tentativas malsucedidas, O SISTEMA NÃO DEVE mais tentar reprocessá-la automaticamente.
5. O disparo de uma notificação NUNCA DEVE bloquear a resposta HTTP da requisição que originou o evento.

---

## Requisito 12: Requisitos não funcionais transversais

**Rastreamento:** RNF01, RNF02, RNF03, RNF04, RNF05, RNF08

**Acceptance Criteria**
1. A API DEVE expor um contrato RESTful documentado via OpenAPI em ambiente de desenvolvimento.
2. Toda senha (cliente e administrador) DEVE ser armazenada apenas como hash — nunca em texto plano, nem em logs.
3. Toda rota administrativa DEVE exigir um JWT válido com papel `admin`; toda rota exclusiva de cliente DEVE exigir um JWT válido com papel `customer`.
4. QUANDO uma exceção não tratada ocorre, O SISTEMA DEVE responder com HTTP 500 e uma mensagem genérica ao cliente, registrando os detalhes apenas no log do servidor — nunca na resposta.
5. A interface DEVE ser responsiva e inteiramente localizada em português brasileiro (pt-BR), incluindo rotas, rótulos, mensagens de validação e dados de exemplo.
6. Nenhum segredo de assinatura de token (JWT) DEVE ser versionado em texto plano no repositório — deve residir em `dotnet user-secrets` (dev) ou variável de ambiente/cofre (produção).

---

## Requisito 13: Paginação de listagens

**Rastreamento:** RF26

**User Story:** Como visitante ou administrador, quero navegar por listas longas em páginas menores, para que a tela carregue rápido e a navegação não fique poluída conforme o catálogo, as encomendas e as mensagens crescem.

**Escopo confirmado:** `/loja` (catálogo público), `/admin/produtos`, `/admin/encomendas`, `/admin/mensagens`. Fora de escopo por ora: produtos em destaque na home, lista de categorias, "Minhas encomendas" do cliente.

**Acceptance Criteria**
1. QUANDO um visitante acessa `/loja`, O SISTEMA DEVE exibir no máximo **12 produtos por página**, com controles para avançar/voltar página.
2. QUANDO um administrador acessa `/admin/produtos`, `/admin/encomendas` ou `/admin/mensagens`, O SISTEMA DEVE exibir no máximo **20 itens por página** em cada uma, com os mesmos controles de navegação.
3. As chamadas `GET /api/products`, `GET /api/admin/products`, `GET /api/admin/orders` e `GET /api/admin/contact` DEVEM aceitar os parâmetros de consulta `page` (1-based, padrão 1) e `pageSize` (padrão conforme item 1/2, com um teto máximo de 100 para evitar abuso).
4. A resposta dessas chamadas DEVE trazer, além dos itens da página, o total de itens (`totalItems`) e o total de páginas (`totalPages`), em um envelope consistente reutilizado pelas quatro listagens.
5. QUANDO o filtro de categoria (loja) OU de status (encomendas) muda, O SISTEMA DEVE retornar à página 1 automaticamente.
6. SE `page` solicitado for maior que `totalPages`, ENTÃO O SISTEMA DEVE retornar uma lista de itens vazia (não um erro), mantendo `totalItems`/`totalPages` corretos.
7. A ordenação dentro de cada listagem (mais recente primeiro para encomendas/mensagens; ordem atual para produtos) DEVE ser preservada — a paginação apenas recorta a lista já ordenada, nunca reordena.
8. Os controles de paginação no frontend DEVEM refletir a página atual e o total de páginas, e desabilitar "Anterior"/"Próxima" nos limites (primeira/última página).
9. Trocar de página NÃO DEVE exigir recarregar a aplicação inteira — apenas uma nova chamada à API e atualização da lista renderizada.

---

## Requisito 14: Produtos exclusivos por cliente

**User Story:** Como administrador, quero cadastrar produtos que só determinados clientes podem ver e encomendar (ex.: kit berço, carrinho, lençol), para oferecer itens sob consulta ou de catálogo estendido sem torná-los públicos.

**Rastreamento:** RF27.

**Acceptance Criteria**
1. Um produto PODE ser associado a zero, um ou vários clientes (relação N:N). Um produto sem nenhum cliente associado é considerado **público** — o comportamento atual (visível a todos) não muda.
2. Um produto com um ou mais clientes associados é considerado **exclusivo** e NÃO DEVE aparecer nas listagens (`/loja`, categorias, destaque, busca) para visitantes não autenticados nem para clientes a quem ele não foi associado.
3. QUANDO um cliente autenticado ao qual o produto foi associado acessa `/loja`, O SISTEMA DEVE incluir esse produto (e sua categoria, no filtro) misturado aos produtos públicos, na mesma listagem.
4. QUANDO um administrador cadastra ou edita um produto, O SISTEMA DEVE permitir selecionar quais clientes (dentre os já cadastrados) têm acesso a ele, a partir de uma lista de clientes existente.
5. `GET /api/products` (loja pública) DEVE aceitar autenticação opcional: SE a requisição não trouxer um token válido, ENTÃO O SISTEMA DEVE retornar apenas produtos públicos; SE trouxer um token de cliente válido, ENTÃO O SISTEMA DEVE incluir também os produtos exclusivos associados àquele cliente.
6. `GET /api/products/{slug}` (detalhe de produto) DEVE aplicar a mesma regra de visibilidade — SE o produto for exclusivo e o visitante/cliente não tiver acesso, ENTÃO O SISTEMA DEVE responder 404, como se o produto não existisse.
7. As listagens administrativas (`GET /api/admin/products`) DEVEM continuar mostrando todos os produtos (públicos e exclusivos, de todos os clientes), independentemente da regra de visibilidade pública.

---

## Requisito 15: Personalização de bordado em todos os produtos

> **Histórico:** este requisito nasceu restrito a produtos exclusivos (critério 2 original: "NÃO DEVE ser oferecida para produtos públicos"). O cliente pediu explicitamente para estender a todos os produtos da loja; o critério 2 abaixo substitui essa restrição.

**User Story:** Como cliente, quero informar quais letras devem ser bordadas e em quantas peças ao comprar qualquer produto, para receber o item personalizado conforme pedido — toda peça do ateliê é bordada sob encomenda.

**Rastreamento:** RF28.

**Acceptance Criteria**
1. QUANDO um cliente adiciona um produto ao carrinho, O SISTEMA DEVE oferecer um campo de texto para as letras/inscrição a bordar, além da quantidade — inclusive um alfabeto clicável (A-Z, espaço, apagar, limpar) que escreve no mesmo campo.
2. O texto de bordado É OBRIGATÓRIO para todo produto, exclusivo ou público — não há mais grade de produto com botão de "adicionar rápido" sem personalização; a única forma de comprar é pela página de detalhe do produto, informando o bordado.
3. QUANDO o mesmo produto é adicionado ao carrinho com um texto de bordado DIFERENTE do já presente, O SISTEMA DEVE tratá-lo como um item de carrinho separado (não somar à quantidade do item com bordado diferente); QUANDO adicionado com o MESMO texto de bordado, O SISTEMA DEVE somar à quantidade desse item.
4. A quantidade de um item do carrinho representa o número de peças que recebem aquele mesmo texto de bordado.
5. QUANDO o pedido é criado, O SISTEMA DEVE persistir o texto de bordado de cada item no campo `OptionsJson` do `OrderItem` correspondente.
6. QUANDO um administrador visualiza o detalhe de uma encomenda, O SISTEMA DEVE exibir o texto de bordado de cada item que o possuir.

---

## Requisito 16: Notificações por WhatsApp

> **Status:** proposto — depende de configuração externa (conta Meta WhatsApp Business Cloud API) que o administrador ainda não criou.

**User Story:** Como cliente, quero receber as notificações do site (confirmação de pedido, mudança de status, boas-vindas, confirmação de contato) por WhatsApp em vez de e-mail, para acompanhar tudo no canal que já uso no dia a dia.

**Rastreamento:** RF29.

**Acceptance Criteria**
1. QUANDO um pedido de loja ou encomenda personalizada é criado, O SISTEMA DEVE enviar ao cliente uma mensagem de WhatsApp confirmando o recebimento do pedido.
2. QUANDO o status de um pedido muda (ex.: "Recebido" → "Em produção" → "Pronto" → "Enviado" → "Entregue"), O SISTEMA DEVE enviar ao cliente uma mensagem de WhatsApp informando o novo status.
3. QUANDO um cliente cria uma conta, O SISTEMA DEVE enviar uma mensagem de WhatsApp de boas-vindas.
4. QUANDO alguém envia o formulário de contato (`POST /api/contact`), O SISTEMA DEVE enviar uma mensagem de WhatsApp confirmando o recebimento.
5. Como o telefone é agora o canal de entrega das notificações, o SISTEMA DEVE exigir telefone/WhatsApp: (a) no cadastro de cliente (`POST /api/auth/register`); (b) no checkout de loja (`POST /api/orders/store`); (c) na encomenda personalizada (`POST /api/orders/custom`); (d) no formulário de contato (`POST /api/contact`, que ganha um campo de telefone que hoje não existe). Um pedido/cadastro/contato sem telefone É REJEITADO com uma mensagem de erro clara.
6. O envio por WhatsApp usa a Meta WhatsApp Business Cloud API oficial (Graph API `POST /{phone-number-id}/messages`), autenticada por um token de acesso configurado via `dotnet user-secrets` (nunca commitado), no mesmo padrão já usado para `Jwt:Secret`.
7. Cada tipo de notificação corresponde a um *message template* pré-aprovado pela Meta (a Cloud API exige template aprovado para mensagens iniciadas pela empresa fora da janela de 24h de atendimento) — o sistema não tenta enviar texto livre para essas notificações automáticas.
8. QUANDO o envio de uma notificação por WhatsApp falha (credenciais ausentes/inválidas, número inválido, template não aprovado, etc.), O SISTEMA NÃO DEVE afetar a operação que originou o evento (criar pedido, mudar status, cadastrar cliente) — a falha fica registrada no outbox (`Attempts`/`Error`) e é reprocessada nas tentativas seguintes, como já ocorre hoje para qualquer falha de notificação.
9. Este requisito substitui o canal de notificação simulado (log) por um canal real — mas o mecanismo de outbox/at-least-once/retry (Requisito de eventos de domínio já implementado) não muda.

---

## Requisito 17: CPF no cadastro de cliente

**User Story:** Como ateliê, quero registrar o CPF de cada cliente no cadastro, para ter um identificador fiscal único de cada pessoa que compra ou encomenda peças.

**Rastreamento:** RF30.

**Acceptance Criteria**
1. QUANDO um visitante se cadastra (`POST /api/auth/register`), O SISTEMA DEVE exigir um CPF válido (11 dígitos, dígitos verificadores corretos pelo algoritmo padrão da Receita Federal) — um cadastro sem CPF ou com CPF inválido É REJEITADO com uma mensagem de erro clara.
2. O SISTEMA DEVE aceitar o CPF formatado (`000.000.000-00`) ou apenas os dígitos, normalizando para armazenamento.
3. O SISTEMA NÃO DEVE permitir duas contas de cliente com o mesmo CPF — um cadastro com um CPF já usado por outra conta É REJEITADO.
4. Contas de cliente já existentes antes deste requisito, que não têm CPF registrado, PERMANECEM válidas (o campo fica em branco para elas) — este requisito não é retroativo.

---

## Requisito 18: Listagem de clientes no admin

**User Story:** Como administrador, quero ver a lista de todos os clientes cadastrados, para consultar quem já tem conta no ateliê e seus dados de contato.

**Rastreamento:** RF31.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer uma tela administrativa (`/admin/clientes`) listando nome, e-mail, telefone, CPF e data de cadastro de todos os clientes.
2. QUANDO um cliente não tem telefone ou CPF registrado (conta anterior ao Requisito 17), O SISTEMA DEVE exibir um traço (`—`) no lugar do dado ausente, sem quebrar a listagem.
3. Esta tela reaproveita o endpoint `GET /api/admin/customers` já existente (usado pelo seletor de clientes exclusivos do Requisito 14) — não pagina, pelo mesmo motivo que o seletor precisa da lista completa de uma vez.

---

## Requisito 19: CPF obrigatório no checkout

**User Story:** Como ateliê, quero registrar o CPF do cliente também nos pedidos (não só no cadastro de conta), para ter um identificador fiscal único mesmo de quem finaliza a compra sem já ter esse dado salvo.

**Rastreamento:** RF32.

**Acceptance Criteria**
1. QUANDO um pedido de loja é criado (`POST /api/orders/store`), O SISTEMA DEVE exigir um CPF válido (mesma validação de formato/dígitos verificadores do Requisito 17) — um pedido sem CPF ou com CPF inválido É REJEITADO com uma mensagem de erro clara.
2. A mesma exigência vale para a encomenda personalizada (`POST /api/orders/custom`).
3. O formulário de checkout (`checkout.html`/`.ts`) ganha um campo "CPF" obrigatório, mesmo padrão visual e de validação do campo equivalente no cadastro (Requisito 17).
4. Pedidos criados antes deste requisito, que não têm CPF registrado, PERMANECEM válidos (o campo fica em branco para eles) — este requisito não é retroativo.

---

## Requisito 20: Login ou cadastro obrigatório para finalizar a compra

**User Story:** Como ateliê, quero que o cliente esteja autenticado ao finalizar a compra, para vincular cada pedido a uma conta e não depender de checkout como convidado.

**Rastreamento:** RF33.

**Acceptance Criteria**
1. QUANDO um visitante não autenticado tenta acessar `/checkout`, O SISTEMA DEVE redirecioná-lo para a tela de login (`/entrar`), preservando a URL de destino (`returnUrl`).
2. A tela de login exibe um link para a tela de cadastro (`/cadastro`) e vice-versa, preservando o `returnUrl` entre as duas.
3. QUANDO o `returnUrl` aponta para `/checkout`, AMBAS as telas (login e cadastro) DEVEM exibir uma mensagem contextual explicando que a autenticação é para finalizar o pedido.
4. QUANDO o login ou o cadastro é concluído com sucesso, O SISTEMA DEVE navegar o cliente para o `returnUrl` (ou para `/minha-conta` na ausência de um) em vez do destino fixo anterior.
5. Este requisito remove o checkout como convidado — o carrinho (mantido em `localStorage`, independente de autenticação) permanece intacto durante o desvio para login/cadastro.

---

## Requisito 21: Pré-preenchimento de dados no checkout

**User Story:** Como cliente que já tem conta e já fez pedido antes, quero que o checkout venha com meus dados e endereço já preenchidos, para não ter que redigitar tudo de novo a cada compra.

**Rastreamento:** RF34.

**Acceptance Criteria**
1. QUANDO um cliente autenticado abre `/checkout`, O SISTEMA DEVE preencher automaticamente nome, e-mail, telefone e CPF com os dados salvos na conta (`GET /api/auth/me`).
2. QUANDO esse cliente já tem pelo menos um pedido anterior com endereço de entrega registrado, O SISTEMA DEVE preencher automaticamente CEP, rua, número, complemento, bairro, cidade e estado com o endereço do pedido mais recente que tiver essa informação (`GET /api/orders/mine`, já ordenado do mais recente para o mais antigo).
3. Todos os campos pré-preenchidos PERMANECEM editáveis — o cliente pode alterar qualquer um antes de confirmar o pedido.
4. QUANDO o cliente não tem telefone/CPF salvos na conta, ou nenhum pedido anterior com endereço, os campos correspondentes ficam em branco (comportamento atual, sem erro).

---

## Requisito 22: Cálculo de frete no checkout

**User Story:** Como ateliê, quero que o checkout calcule uma estimativa de frete com base no destino do pedido, para que o cliente veja o custo total (produtos + envio) antes de confirmar, em vez de descobrir o frete só depois pelo WhatsApp.

**Rastreamento:** RF35.

**Acceptance Criteria**
1. QUANDO o cliente preenche ou tem preenchido automaticamente o estado (UF) de entrega no checkout, O SISTEMA DEVE calcular um frete estimado com base nesse estado e na quantidade total de itens no carrinho.
2. O cálculo NÃO usa a API oficial dos Correios (exigiria contrato/credenciais que o ateliê não possui) — é uma estimativa por faixa de região (SP, Sul/Sudeste, Centro-Oeste/Nordeste, Norte) com acréscimo por item adicional, mais uma margem de 50% sobre a tarifa estimada dos Correios (cobre embalagem/postagem), calculada inteiramente no frontend.
3. O SISTEMA DEVE exibir, no resumo do pedido durante o checkout, o subtotal dos produtos, o frete estimado e o total (soma dos dois) separadamente.
4. QUANDO o pedido é confirmado, O SISTEMA DEVE persistir o valor do frete (`Orders.ShippingCostAmount`) e o total do pedido (`Order.Total`) passa a ser subtotal dos itens + frete, refletido em toda tela que exibe o total do pedido (confirmação, "Minhas encomendas", admin).
5. Pedidos criados antes deste requisito, que não têm frete registrado, PERMANECEM válidos com frete zero (`Order.ShippingCost` não é retroativo).

---

## Requisito 23: CPF mascarado nas telas administrativas

**User Story:** Como ateliê, quero que o CPF do cliente apareça mascarado nas telas administrativas, para reduzir a exposição desse dado sensível (LGPD) no dia a dia de quem opera o painel.

**Rastreamento:** RF36.

**Acceptance Criteria**
1. QUANDO um administrador visualiza a listagem de clientes (`/admin/clientes`) ou o detalhe de uma encomenda (`/admin/encomendas/:id`), O SISTEMA DEVE exibir o CPF mascarado, no formato `***.XXX.XXX-**` (oculta o primeiro bloco e os dígitos verificadores, mostra só o bloco do meio).
2. QUANDO o cliente não tem CPF registrado, O SISTEMA DEVE continuar exibindo um traço (`—`), sem tentar mascarar um valor inexistente.
3. Este requisito é só de exibição — o CPF continua armazenado por completo no banco e retornado sem máscara pela API (`GET /api/admin/customers`, `GET /api/admin/orders`); a máscara é aplicada no frontend, no momento de renderizar essas duas telas.

---

## Requisito 24: Imagens do site editáveis pelo admin

**User Story:** Como ateliê, quero trocar a foto principal da página inicial e a foto da página "Sobre" direto pelo painel administrativo, para não precisar pedir um deploy de código toda vez que eu tiver uma foto nova.

**Rastreamento:** RF37.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer uma tela administrativa (`/admin/imagens`) com um slot para a imagem principal da página inicial e um slot para a imagem da página "Sobre", cada um com a prévia atual e um botão para enviar um arquivo novo.
2. QUANDO o administrador envia um arquivo para um desses slots, O SISTEMA DEVE salvar o arquivo em disco (fora da pasta de publicação, para sobreviver a um redeploy) e associá-lo à chave daquele slot (`home-hero` ou `about`).
3. QUANDO a página inicial ou a página "Sobre" carregam, O SISTEMA DEVE exibir a imagem mais recente enviada para o slot correspondente; SE nenhuma imagem foi enviada ainda, exibe a imagem estática padrão que já existe hoje.
4. Formatos aceitos: JPG, PNG e WEBP; tamanho máximo de 8MB — um upload fora desses limites é rejeitado com uma mensagem de erro clara.

---

## Requisito 25: Upload de foto do produto no admin

**User Story:** Como ateliê, quero enviar a foto de um produto como arquivo ao cadastrar ou editar um produto, em vez de precisar descobrir e colar uma URL de imagem.

**Rastreamento:** RF38.

**Acceptance Criteria**
1. O formulário de produto do admin (`/admin/produtos/novo`, `/admin/produtos/:id/editar`) DEVE oferecer, ao lado do campo "URL da imagem", um botão para enviar um arquivo diretamente.
2. QUANDO o administrador envia um arquivo, O SISTEMA DEVE salvar o arquivo e preencher automaticamente o campo "URL da imagem" com o endereço salvo, mantendo a pré-visualização atualizada.
3. O campo "URL da imagem" continua editável manualmente — o upload é uma forma alternativa de preenchê-lo, não substitui a opção de colar uma URL existente.
4. Mesmos limites de formato/tamanho do Requisito 24 (JPG/PNG/WEBP, até 8MB).

---

## Requisito 26: Galeria gerenciável pelo admin

**User Story:** Como ateliê, quero adicionar e remover fotos da galeria pública direto pelo painel administrativo, para manter a galeria atualizada sem depender de deploy de código.

**Rastreamento:** RF39.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer uma tela administrativa (`/admin/galeria`) que lista todas as fotos da galeria, com um botão para adicionar uma foto nova (upload de arquivo) e um botão para remover cada foto existente.
2. QUANDO uma foto é adicionada, O SISTEMA DEVE salvá-la e fazê-la aparecer na página pública `/galeria` (mais recente primeiro).
3. QUANDO uma foto é removida, O SISTEMA DEVE apagar o registro E o arquivo salvo em disco, e ela deixa de aparecer em `/galeria` imediatamente.
4. QUANDO não há nenhuma foto cadastrada ainda, a página pública `/galeria` exibe um conjunto de imagens de exemplo (placeholder), para a página não ficar vazia antes do primeiro upload.
5. Mesmos limites de formato/tamanho do Requisito 24 (JPG/PNG/WEBP, até 8MB).
