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
2. O cálculo NÃO usa a API oficial dos Correios (exigiria contrato/credenciais que o ateliê não possui) — é uma estimativa por faixa de região (SP, Sul/Sudeste, Centro-Oeste/Nordeste, Norte) com acréscimo por item adicional, calculada inteiramente no frontend, sem nenhuma margem adicional sobre a tarifa estimada.
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

---

## Requisito 27: Pagamento online no checkout (PagBank)

**User Story:** Como ateliê, quero oferecer PIX e cartão de crédito como formas de pagamento no checkout, para que o cliente pague no ato da compra em vez de combinar o pagamento por fora.

**Rastreamento:** RF40.

**Acceptance Criteria**
1. QUANDO um pedido de loja é criado (`POST /api/orders/store`) E o gateway de pagamento está configurado (`PagBank:Token` presente), O SISTEMA DEVE criar um checkout hospedado no PagBank para o valor total do pedido e devolver a URL de pagamento na resposta (`PaymentUrl`).
2. QUANDO o gateway de pagamento NÃO está configurado, O SISTEMA DEVE criar o pedido normalmente, sem `PaymentUrl` e sem erro — o comportamento é idêntico ao de antes deste requisito.
3. O frontend, ao receber uma `PaymentUrl` na resposta do checkout, DEVE redirecionar o navegador do cliente para essa URL (a página de pagamento hospedada pelo PagBank) em vez de ir direto para a confirmação do pedido.
4. O SISTEMA DEVE expor um endpoint de webhook (`POST /api/payments/pagbank/webhook`) que, ao ser chamado pelo PagBank, reconsulta o status do pagamento diretamente na API do PagBank (nunca confia no status vindo no corpo da notificação) e atualiza `Orders.PaymentStatus` do pedido correspondente (localizado pelo `reference_id` devolvido nessa consulta, que é o id do pedido).
5. O SISTEMA DEVE tratar como aprovado (`PaymentStatus = Pago`) apenas quando a cobrança mais recente do pedido está com status `PAID` no PagBank, e como recusado (`PaymentStatus = Recusado`) quando está `DECLINED` ou `CANCELED`; qualquer outro status (`AUTHORIZED`, `IN_ANALYSIS`, `WAITING`, etc.) NÃO altera o `PaymentStatus` atual do pedido, que permanece `Pendente`.
6. UMA VEZ que um pedido está com `PaymentStatus = Pago`, o SISTEMA NÃO DEVE rebaixá-lo para `Recusado` ou `Pendente` em razão de uma notificação de webhook posterior, duplicada ou fora de ordem (idempotência).
7. O endpoint de webhook DEVE sempre responder HTTP 200, mesmo quando a notificação vem malformada, sem id reconhecível, referenciando um checkout (em vez de um pedido pago) ou um pedido inexistente — para que o PagBank não fique retentando indefinidamente uma notificação que nunca vai ser processável.
8. `PaymentStatus` (`Pendente` | `Pago` | `Recusado`) é independente do status de produção/entrega do pedido (`Order.Status`) — um pedido pode estar `EmProducao` com pagamento ainda `Pendente`, por exemplo.
9. A página de pagamento hospedada DEVE oferecer apenas Pix e cartão de crédito — nenhum outro meio de pagamento que o PagBank ofereça (boleto, débito, carteira digital, etc.) DEVE aparecer como opção no checkout criado.

---

## Requisito 28: Gestão de pagamento das encomendas no admin

**User Story:** Como ateliê, quero ver rapidamente quais encomendas já foram pagas e poder gerar um novo link de pagamento para as que não foram, para acompanhar o fluxo de caixa e resolver casos de cliente que abandonou o pagamento ou cujo pedido foi criado antes do gateway estar configurado.

**Rastreamento:** RF41.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer, na listagem de encomendas do admin (`/admin/encomendas`), um filtro por status de pagamento (`Pendente` | `Pago` | `Recusado`), independente do filtro por status de produção/entrega já existente, e exibir o status de pagamento de cada encomenda na própria linha da tabela.
2. QUANDO o administrador abre o detalhe de uma encomenda cujo `PaymentStatus` NÃO é `Pago`, O SISTEMA DEVE oferecer um botão "Gerar link de pagamento".
3. QUANDO o administrador clica nesse botão, O SISTEMA DEVE criar uma nova preferência de pagamento no PagBank para o valor total da encomenda e apresentar a URL resultante com um botão para copiar o link e outro para abrir a página de pagamento em uma nova aba.
4. SE o gateway de pagamento não estiver configurado, O SISTEMA DEVE rejeitar a geração do link com uma mensagem de erro clara, em vez de falhar silenciosamente ou gerar uma URL inválida.
5. SE a encomenda já estiver com `PaymentStatus = Pago`, O SISTEMA NÃO DEVE oferecer a opção de gerar um novo link de pagamento — não faz sentido cobrar de novo por um pedido já pago.
6. Gerar um novo link de pagamento NÃO DEVE alterar o `PaymentStatus` atual da encomenda nem seus dados persistidos — é só a criação de uma preferência adicional no PagBank; a confirmação de pagamento continua acontecendo exclusivamente pelo webhook (Requisito 27).

---

## Requisito 29: SEO das páginas públicas

**User Story:** Como ateliê, quero que as páginas do site tenham título, descrição e prévia de compartilhamento adequados, para aparecer melhor no Google e ter um link bonito quando compartilhado no WhatsApp/Instagram.

**Rastreamento:** RF42.

**Acceptance Criteria**
1. QUANDO qualquer página pública carrega, O SISTEMA DEVE definir um `<title>` específico da página, uma meta description, tags Open Graph (`og:title`, `og:description`, `og:type`, `og:url`, `og:image`, `og:site_name`) e Twitter Card, além de um link `canonical` apontando para a URL absoluta da página.
2. A página de detalhe do produto DEVE usar `og:type=product` e a foto do próprio produto (resolvida para uma URL absoluta) como `og:image`; as demais páginas usam `og:type=website` e uma imagem padrão do site.
3. O SISTEMA DEVE expor `GET /api/sitemap.xml`, gerado a cada requisição a partir das páginas fixas (`/`, `/loja`, `/sobre`, `/galeria`, `/contato`) e de todo produto ativo e público (`/produto/:slug`), refletindo sempre o catálogo atual.
4. O SISTEMA DEVE expor um `robots.txt` que aponta o diretivo `Sitemap:` para `/api/sitemap.xml` e bloqueia o rastreamento de áreas administrativas e de conta (`/admin`, `/checkout`, `/minha-conta`, `/entrar`, `/cadastro`, `/pedido/`, `/pagamento-simulado/`).

---

## Requisito 30: Estrutura de analytics (Google Analytics / Meta Pixel)

**User Story:** Como ateliê, quero poder acompanhar de onde vêm minhas vendas quando eu criar as contas de Google Analytics e Meta Pixel, sem precisar de mais desenvolvimento depois.

**Rastreamento:** RF43.

**Acceptance Criteria**
1. SE `environment.analytics.googleAnalyticsId` estiver preenchido, O SISTEMA DEVE carregar o script do Google Analytics (GA4) e registrar uma visualização de página a cada navegação de rota.
2. SE `environment.analytics.metaPixelId` estiver preenchido, O SISTEMA DEVE carregar o script do Meta Pixel e registrar um evento `PageView` a cada navegação de rota.
3. QUANDO nenhum dos dois IDs está preenchido (padrão, até o ateliê criar as contas), O SISTEMA NÃO DEVE carregar nenhum script de terceiro nem fazer nenhuma requisição de rastreamento.

---

## Requisito 31: Busca de produtos na loja

**User Story:** Como cliente, quero buscar um produto pelo nome na loja, para encontrá-lo rapidamente sem precisar navegar pelas categorias.

**Rastreamento:** RF44.

**Acceptance Criteria**
1. A página da loja (`/loja`) DEVE oferecer um campo de busca por texto; ao digitar, o SISTEMA DEVE filtrar os produtos cujo nome contém o texto digitado (sem diferenciar maiúsculas/minúsculas), com um debounce para não buscar a cada tecla digitada.
2. O termo de busca DEVE ser refletido na URL (`?busca=`) e ser combinável com o filtro de categoria (`?categoria=`) já existente — os dois filtros se aplicam juntos.
3. QUALQUER mudança no termo de busca DEVE reiniciar a paginação para a página 1, do mesmo jeito que mudar de categoria já faz.
4. QUANDO nenhum produto corresponde à busca, O SISTEMA DEVE exibir uma mensagem indicando que nada foi encontrado para aquele termo.

---

## Requisito 32: Avaliações de produtos

**User Story:** Como cliente que já comprou um produto, quero deixar uma avaliação com nota e comentário, para compartilhar minha experiência com outros compradores; como ateliê, quero que essas avaliações apareçam na página do produto para gerar confiança em novos clientes.

**Rastreamento:** RF45.

**Acceptance Criteria**
1. O SISTEMA DEVE permitir que um cliente autenticado avalie um produto (nota de 1 a 5 estrelas, comentário opcional) SOMENTE SE ele tiver pelo menos um pedido (qualquer status) contendo aquele produto como item.
2. QUANDO o cliente já avaliou aquele produto anteriormente, O SISTEMA DEVE recusar uma nova avaliação para o mesmo par cliente/produto, com uma mensagem clara.
3. QUANDO uma avaliação é criada, O SISTEMA DEVE publicá-la imediatamente na página do produto, sem exigir aprovação prévia do administrador.
4. A página do produto DEVE exibir a nota média e o total de avaliações ao lado do nome do produto, e a lista completa de avaliações (nome do cliente, nota, comentário, data) mais abaixo.
5. QUANDO o visitante não está autenticado, ou está autenticado mas nunca comprou aquele produto, O SISTEMA NÃO DEVE oferecer o formulário de avaliação — a lista de avaliações existentes continua visível normalmente.

---

## Requisito 33: Backup automático do banco de dados

**User Story:** Como ateliê, quero que o banco de dados de produção tenha backup automático, para não perder pedidos e cadastros de clientes se o servidor tiver um problema.

**Rastreamento:** RNF09.

**Acceptance Criteria**
1. O SISTEMA DEVE ter um script (`server/ops/backup-db.sh`) que gera um backup consistente do banco SQLite (via `sqlite3 .backup`, nunca uma cópia de arquivo crua) e o compacta.
2. O backup DEVE ser salvo fora da pasta de publicação, para sobreviver a um `dotnet publish` de deploy.
3. O SISTEMA DEVE manter apenas os 10 backups mais recentes, removendo automaticamente os mais antigos, para não esgotar o espaço em disco indefinidamente.
4. A instalação (agendamento via cron) É um passo manual único documentado no `README.md`, não algo que a aplicação faz sozinha.
5. O SISTEMA DEVE ter um script (`server/ops/sync-offsite.sh`) que sincroniza os backups locais para um remoto fora do servidor (Google Drive via `rclone`), para sobreviver à perda do VPS inteiro (não só do arquivo do banco).
6. O SISTEMA DEVE ter um script (`server/ops/sync-uploads-offsite.sh`) que sincroniza a pasta de imagens enviadas pelo admin (produtos, galeria, site) para o mesmo remoto fora do servidor, já que essas imagens não fazem parte do banco de dados e por isso não são cobertas pelos critérios 1-5.

---

## Requisito 34: Edição de dados do cliente pelo admin

**User Story:** Como administrador, quero poder corrigir o nome, e-mail, CPF ou telefone de um cliente cadastrado, para consertar erros de digitação ou atualizar dados desatualizados sem depender do próprio cliente.

**Rastreamento:** RF46.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer uma tela (`/admin/clientes/:id/editar`) com os campos nome, e-mail, CPF e telefone pré-preenchidos com os dados atuais do cliente.
2. QUANDO o administrador salva com um e-mail já usado por **outra** conta, O SISTEMA DEVE rejeitar com um erro de conflito; o mesmo vale para CPF.
3. QUANDO o administrador salva sem alterar e-mail/CPF (mesmo valor da própria conta), O SISTEMA NÃO DEVE rejeitar por conflito consigo mesma.
4. A senha do cliente NÃO É um campo editável nesta tela.

---

## Requisito 35: Notificações por e-mail

**User Story:** Como ateliê, quero que os clientes recebam e-mail de confirmação de pedido e mudança de status, para ter um canal de comunicação que não dependa da aprovação do WhatsApp Business pela Meta.

**Rastreamento:** RF47.

**Acceptance Criteria**
1. QUANDO um pedido é criado, o status de um pedido muda, um cliente se cadastra, ou uma mensagem de contato é recebida, O SISTEMA DEVE tentar enviar um e-mail correspondente via Resend, além da notificação por WhatsApp já existente.
2. SE o envio de e-mail falhar ou `Resend:ApiKey` não estiver configurado, O SISTEMA NÃO DEVE deixar de tentar enviar a notificação por WhatsApp (e vice-versa) — os dois canais são independentes.
3. SE `Resend:ApiKey` não estiver configurado, O SISTEMA NÃO DEVE expor esse erro ao cliente que originou o evento — a falha fica só no log do servidor.

---

## Requisito 36: Exportação de encomendas em CSV

**User Story:** Como ateliê, quero exportar a lista de encomendas em CSV, para levar os dados de vendas para a contabilidade ou uma planilha própria.

**Rastreamento:** RF48.

**Acceptance Criteria**
1. A tela `/admin/encomendas` DEVE oferecer um botão "Exportar CSV" que baixa um arquivo CSV com todas as encomendas que correspondem aos filtros de status/pagamento ativos no momento (não só a página atual visível).
2. O arquivo CSV DEVE abrir corretamente no Excel em português (separador `;`, acentuação preservada).

---

## Requisito 37: Galeria de fotos por produto

**User Story:** Como ateliê, quero adicionar mais de uma foto a um produto, para mostrar detalhes do bordado e ângulos diferentes da peça.

**Rastreamento:** RF49.

**Acceptance Criteria**
1. O formulário de produto do admin (em modo edição) DEVE oferecer uma seção de galeria onde o administrador pode enviar novas fotos, ver as já cadastradas e remover qualquer uma antes de salvar.
2. QUANDO o administrador salva a galeria, O SISTEMA DEVE substituir o conjunto anterior pelo conjunto atual, na ordem em que aparecem na tela.
3. A página pública do produto DEVE exibir a foto de capa mais as fotos da galeria como miniaturas clicáveis, trocando a foto em destaque ao clicar em uma miniatura.
4. QUANDO o produto não tem nenhuma foto de galeria, a página do produto DEVE continuar mostrando só a foto de capa, sem miniaturas.

---

## Requisito 38: Notificação do ateliê em cada novo pedido

**User Story:** Como ateliê, quero ser avisado automaticamente sempre que um pedido novo chegar, para não depender de checar o painel manualmente.

**Rastreamento:** RF50.

**Acceptance Criteria**
1. QUANDO um pedido é criado (loja ou personalizado), O SISTEMA DEVE, além de notificar o cliente, enviar um alerta ao ateliê pelos mesmos canais já existentes (e-mail via Resend, WhatsApp via Meta Cloud API), configurados em `AdminNotification:Email`/`AdminNotification:Phone`.
2. Uma falha ou ausência de configuração no alerta do ateliê NÃO DEVE impedir a notificação do cliente, nem vice-versa — os canais continuam totalmente independentes entre si.

---

## Requisito 39: Redefinição de senha do cliente

**User Story:** Como cliente, quero poder redefinir minha senha se eu esquecer, sem depender de criar uma conta nova.

**Rastreamento:** RF51.

**Acceptance Criteria**
1. O SISTEMA DEVE oferecer uma tela (`/esqueci-senha`) onde o cliente informa o e-mail e recebe um link de redefinição por e-mail, caso a conta exista.
2. A resposta dessa solicitação NÃO DEVE revelar se o e-mail informado está ou não cadastrado — o comportamento observável é o mesmo nos dois casos.
3. O link de redefinição DEVE ser de uso único e expirar após 1 hora; uma segunda tentativa de uso do mesmo link, ou um link expirado, DEVE ser rejeitada com uma mensagem genérica.
4. A tela de redefinição (`/redefinir-senha?token=...`) DEVE exigir a nova senha (mínimo 6 caracteres) e sua confirmação, sem exigir a senha atual.
5. O SISTEMA NUNCA DEVE persistir o token em texto plano — apenas seu hash, de forma que um vazamento do banco não seja suficiente para redefinir senhas de clientes.

---

## Requisito 40: Código de rastreio da encomenda

**User Story:** Como ateliê, quero anexar o código de rastreio de uma encomenda enviada, para o cliente conseguir acompanhar a entrega.

**Rastreamento:** RF52.

**Acceptance Criteria**
1. O painel administrativo de uma encomenda DEVE permitir que o administrador informe (ou limpe) um código de rastreio livre, independente do status atual do pedido.
2. QUANDO um código de rastreio está preenchido, a página pública do pedido (`/pedido/:id`) DEVE exibi-lo ao cliente.
3. QUANDO não há código de rastreio, a página do pedido NÃO DEVE exibir nenhuma menção a rastreio.

---

## Requisito 41: Exclusão de conta pelo cliente (LGPD)

**User Story:** Como cliente, quero poder solicitar a exclusão da minha conta, para exercer meu direito de remoção de dados pessoais.

**Rastreamento:** RF53.

**Acceptance Criteria**
1. A tela "Minha conta" DEVE oferecer uma opção de exclusão de conta, exigindo a senha atual como confirmação antes de prosseguir.
2. SE a senha informada estiver incorreta, O SISTEMA DEVE rejeitar a exclusão sem alterar nada.
3. SE o cliente não tiver nenhuma encomenda registrada, O SISTEMA DEVE remover a conta por completo do banco de dados.
4. SE o cliente tiver ao menos uma encomenda registrada, O SISTEMA DEVE anonimizar os dados pessoais da conta (nome, e-mail, telefone, CPF) e invalidar o login, em vez de remover a conta — o histórico de encomendas (que guarda sua própria cópia dos dados no momento da compra) DEVE permanecer intacto e visível ao administrador.
5. Uma conta anonimizada NUNCA DEVE conseguir autenticar novamente, mesmo com a senha antiga.
6. Após a exclusão bem-sucedida (removida ou anonimizada), O SISTEMA DEVE encerrar a sessão do cliente no navegador.

---

## Requisito 42: Registro das mensagens de contato no painel administrativo

**User Story:** Como ateliê, quero ver no painel administrativo as mensagens enviadas pelo formulário de contato, mesmo continuando a conversa pelo WhatsApp.

**Rastreamento:** RF54.

**Acceptance Criteria**
1. QUANDO o formulário de contato/encomenda é enviado, O SISTEMA DEVE registrar a mensagem (via `POST /api/contact`) além de abrir a conversa no WhatsApp — as duas ações DEVEM acontecer para o mesmo envio.
2. SE o registro da mensagem falhar por qualquer motivo, O SISTEMA NÃO DEVE impedir nem atrasar a abertura do WhatsApp.
3. A mensagem registrada DEVE aparecer em `/admin/mensagens`.
4. QUANDO o cliente não informa e-mail no formulário, O SISTEMA DEVE ainda assim conseguir registrar a mensagem, sem exigir esse campo do usuário.

---

## Requisito 43: Promoções por produto com período determinado

**User Story:** Como ateliê, quero aplicar descontos por tempo limitado em um ou vários produtos, para fazer promoções sazonais sem precisar mexer no preço base.

**Rastreamento:** RF55.

**Acceptance Criteria**
1. O SISTEMA DEVE permitir configurar, por produto, um desconto percentual (1 a 99%) com data/hora de início e fim.
2. O preço promocional DEVE se aplicar automaticamente enquanto o instante atual estiver dentro da janela configurada, e deixar de se aplicar automaticamente fora dela — sem exigir nenhuma ação manual do administrador para ativar ou desativar.
3. O SISTEMA DEVE permitir remover uma promoção configurada, voltando o produto ao preço normal imediatamente.
4. O preço efetivamente cobrado ao criar um pedido de loja DEVE ser o preço promocional quando a promoção estiver ativa, ignorando qualquer preço enviado pelo cliente na requisição.
5. O SISTEMA DEVE permitir aplicar a mesma promoção (desconto + período) a vários produtos selecionados de uma vez.
6. A vitrine pública e a página do produto DEVEM exibir o preço original riscado, o preço promocional e o percentual de desconto quando a promoção estiver ativa.

---

## Requisito 44: Exportação de CSV com detalhes por item

**User Story:** Como ateliê, quero que a exportação de encomendas mostre o bordado, a quantidade e a cor de cada item, para conseguir organizar a produção.

**Rastreamento:** RF56.

**Acceptance Criteria**
1. O CSV exportado DEVE ter uma linha por item de pedido (não uma linha por pedido) — pedidos com vários itens geram várias linhas, repetindo os dados do pedido.
2. Cada linha DEVE incluir o nome do produto, a quantidade, o texto bordado e a cor da linha de bordado daquele item específico, quando presentes.
3. Um pedido sem nenhum item (caso não deva ocorrer na prática) DEVE ainda assim gerar exatamente uma linha, com as colunas de item em branco.

---

## Requisito 45: Endereço completo no cadastro do cliente

**User Story:** Como cliente, quero informar meu endereço completo já no cadastro, para não precisar redigitar tudo no primeiro checkout.

**Rastreamento:** RF57.

**Acceptance Criteria**
1. O formulário de cadastro DEVE coletar CEP, rua, número, complemento, bairro, cidade e estado.
2. QUANDO o cliente digita um CEP válido, O SISTEMA DEVE preencher automaticamente rua, bairro, cidade e estado via ViaCEP, mantendo os campos editáveis.
3. O endereço informado no cadastro DEVE ficar salvo na conta do cliente, consultável e editável posteriormente (inclusive pelo administrador).
4. Contas criadas antes deste requisito DEVEM continuar válidas mesmo sem endereço preenchido.

---

## Requisito 46: Cor da linha de bordado

**User Story:** Como cliente, quero escolher a cor da linha usada no bordado, além do texto, para personalizar completamente a peça.

**Rastreamento:** RF58.

**Acceptance Criteria**
1. A página de detalhe do produto DEVE oferecer uma seleção de cor de linha de bordado, a partir de uma paleta fixa, junto do campo de texto para bordar.
2. O SISTEMA DEVE exigir que uma cor seja escolhida antes de permitir adicionar o produto ao carrinho, da mesma forma que já exige o texto do bordado.
3. Itens do carrinho com a mesma combinação de produto, texto e cor DEVEM ser tratados como a mesma linha (quantidades somadas); combinações diferentes DEVEM gerar linhas separadas.
4. A cor escolhida DEVE ser exibida no carrinho, na confirmação do pedido e no detalhe administrativo da encomenda.

---

## Requisito 47: Cupons de desconto

**User Story:** Como ateliê, quero criar cupons de desconto que o cliente digita no checkout, para campanhas de marketing pontuais (boas-vindas, datas comemorativas, etc.).

**Rastreamento:** RF59.

**Acceptance Criteria**
1. O SISTEMA DEVE permitir que o administrador crie um cupom com código, percentual de desconto (1 a 99%), validade opcional e limite de usos opcional.
2. QUANDO o cliente informa um código de cupom no checkout, O SISTEMA DEVE validar o cupom (existe, ativo, dentro da validade, dentro do limite de usos) antes de confirmar o pedido, mostrando o valor do desconto sem ainda contabilizar o uso.
3. O desconto DEVE incidir apenas sobre o subtotal dos itens, nunca sobre o frete, e nunca DEVE deixar o total do pedido negativo.
4. Um cupom que se torna inválido entre a validação e a confirmação do pedido (expirou, esgotou usos, foi desativado) DEVE ser rejeitado na confirmação, mesmo que a validação anterior tenha aprovado.
5. QUANDO um pedido é confirmado com um cupom válido, O SISTEMA DEVE incrementar o contador de usos do cupom.
6. O administrador DEVE poder desativar um cupom a qualquer momento, independente de validade ou limite de usos.

---

## Requisito 48: Proteção contra força bruta

**User Story:** Como ateliê, quero que tentativas repetidas de adivinhar uma senha ou código sejam bloqueadas automaticamente, para reduzir o risco de invasão de contas.

**Rastreamento:** RF60.

**Acceptance Criteria**
1. O SISTEMA DEVE limitar a 5 requisições por minuto, por combinação de IP do cliente e rota, os endpoints de login (cliente e admin), redefinição de senha, exclusão de conta e validação de cupom.
2. A partir da 6ª requisição no mesmo minuto, o endpoint DEVE responder `429 Too Many Requests` sem processar a requisição.
3. O limite de um endpoint NÃO DEVE afetar o limite de outro endpoint para o mesmo cliente — tentativas malsucedidas no login do admin, por exemplo, não podem bloquear o login de um cliente distinto.
4. Em produção, atrás de um proxy reverso, o SISTEMA DEVE identificar o IP real do cliente (via cabeçalho encaminhado pelo proxy confiável), não o IP do próprio proxy.

---

## Requisito 49: Health check para monitoramento

**User Story:** Como ateliê, quero um jeito automático de saber se o site caiu, para não depender de um cliente avisar que o site não abre.

**Rastreamento:** RF61.

**Acceptance Criteria**
1. O SISTEMA DEVE expor uma rota pública (`GET /api/health`) que responde sucesso somente quando a API está no ar e consegue se conectar ao banco de dados.
2. QUANDO o banco de dados está inacessível, a rota DEVE responder com um status de falha, mesmo que o processo da API continue rodando.

---

## Requisito 50: Métricas adicionais no painel administrativo

**User Story:** Como ateliê, quero ver ticket médio, produtos mais vendidos e a evolução das vendas no painel, para entender o desempenho do negócio sem precisar exportar dados.

**Rastreamento:** RF62.

**Acceptance Criteria**
1. O painel administrativo DEVE exibir o ticket médio dos pedidos (excluindo cancelados).
2. O painel DEVE exibir os 5 produtos mais vendidos por quantidade, com a receita gerada por cada um.
3. O painel DEVE exibir um gráfico com a receita diária dos últimos 30 dias.
4. Essas métricas DEVEM ser calculadas a partir dos mesmos dados já usados para os indicadores existentes, sem exigir uma consulta adicional ao banco de dados.

---

## Requisito 51: Páginas de Termos de Uso e Política de Privacidade

**User Story:** Como ateliê, quero páginas públicas de Termos de Uso e Política de Privacidade, para deixar claras as regras de compra/personalização e como os dados dos clientes são tratados.

**Rastreamento:** RF63.

**Acceptance Criteria**
1. O SISTEMA DEVE disponibilizar uma página pública de Termos de Uso em `/termos-de-uso`.
2. O SISTEMA DEVE disponibilizar uma página pública de Política de Privacidade em `/politica-de-privacidade`.
3. O rodapé de toda página pública DEVE conter links para as duas páginas.
4. A Política de Privacidade DEVE descrever o comportamento real de exclusão/anonimização de conta já implementado (Requisito 41), não apenas uma promessa genérica.

---

## Requisito 52: Dados estruturados (JSON-LD) nos produtos

**User Story:** Como ateliê, quero que os produtos tenham dados estruturados no formato reconhecido por buscadores, para melhorar a chance de aparecer com rich snippets nos resultados de busca.

**Rastreamento:** RF64.

**Acceptance Criteria**
1. A página de detalhe de cada produto DEVE incluir um bloco `<script type="application/ld+json">` no formato schema.org `Product`, com nome, descrição, imagem, preço e disponibilidade.
2. QUANDO o produto tem avaliações, o JSON-LD DEVE incluir `aggregateRating` (nota média e total de avaliações).
3. QUANDO o visitante navega para qualquer outra página, o bloco JSON-LD do produto anterior NÃO DEVE permanecer na página.

---

## Requisito 53: Verificação de e-mail no cadastro

**User Story:** Como ateliê, quero confirmar que o e-mail informado no cadastro pertence de fato ao cliente, para reduzir cadastros com e-mail inválido/alheio e melhorar a entregabilidade das notificações.

**Rastreamento:** RF65.

**Acceptance Criteria**
1. QUANDO um cliente se cadastra, O SISTEMA DEVE enviar um e-mail com um link de confirmação de uso único, válido por 24 horas.
2. QUANDO o cliente acessa o link de confirmação com um token válido, O SISTEMA DEVE marcar o e-mail da conta como verificado.
3. QUANDO o token é inválido, expirado ou já usado, O SISTEMA DEVE rejeitar a confirmação sem alterar o estado da conta.
4. O cliente autenticado DEVE poder solicitar o reenvio do e-mail de confirmação a qualquer momento pela própria conta; a solicitação NÃO DEVE falhar de forma visível se a conta já estiver verificada.
5. QUANDO um cliente altera o e-mail da própria conta, O SISTEMA DEVE marcar o e-mail como não verificado novamente.
6. A anonimização de conta (Requisito 41) DEVE limpar também o estado de verificação de e-mail.

---

## Requisito 54: Otimização automática de imagens enviadas

**User Story:** Como ateliê, quero que as fotos que eu envio sejam otimizadas automaticamente, para o site carregar rápido sem eu precisar editar cada imagem antes de enviar.

**Rastreamento:** RF66.

**Acceptance Criteria**
1. QUANDO uma imagem é enviada (produto, galeria ou foto do site) com largura ou altura maior que 1600px, O SISTEMA DEVE redimensioná-la para no máximo 1600px no maior lado, preservando a proporção original.
2. QUANDO uma imagem enviada já é menor que 1600px nos dois lados, O SISTEMA NÃO DEVE aumentá-la (sem upscale).
3. O SISTEMA DEVE recomprimir toda imagem salva (JPEG/WEBP com perda controlada, PNG sem perda) para reduzir o tamanho do arquivo final.
4. O formato do arquivo (extensão) DEVE permanecer o mesmo enviado pelo administrador.

---

## Requisito 55: Favoritos e aviso de reposição

**User Story:** Como cliente, quero marcar produtos como favoritos para não perdê-los de vista, e ser avisado se um deles voltar a ficar disponível depois de pausado.

**Rastreamento:** RF67.

**Acceptance Criteria**
1. O cliente autenticado DEVE poder favoritar um produto e, posteriormente, desfavoritá-lo.
2. Favoritar um produto já favoritado, ou desfavoritar um produto que não está favoritado, NÃO DEVE resultar em erro (operação idempotente).
3. O cliente DEVE poder ver a lista de todos os seus produtos favoritados (`/favoritos`).
4. QUANDO um produto favoritado por um ou mais clientes passa de inativo para ativo, O SISTEMA DEVE enviar um e-mail a cada cliente que o favoritou, avisando que voltou a ficar disponível.
5. Reativar um produto que já estava ativo, ou desativá-lo, NÃO DEVE disparar esse aviso.

---

## Requisito 56: Lembrete de carrinho abandonado

**User Story:** Como ateliê, quero avisar um cliente que deixou itens no carrinho sem finalizar a compra, para recuperar vendas que quase aconteceram.

**Rastreamento:** RF68.

**Acceptance Criteria**
1. QUANDO um cliente autenticado altera o carrinho, O SISTEMA DEVE salvar uma cópia dos itens no servidor, associada à conta do cliente.
2. QUANDO o carrinho de um cliente autenticado fica vazio, O SISTEMA DEVE remover essa cópia em vez de manter uma cópia vazia.
3. QUANDO uma cópia de carrinho fica sem nenhuma atualização por um período configurado de inatividade e nunca recebeu um lembrete, O SISTEMA DEVE enviar um e-mail ao cliente listando os produtos deixados no carrinho.
4. O SISTEMA NÃO DEVE enviar mais de um lembrete para o mesmo período de abandono de um carrinho.
5. QUANDO o cliente volta a alterar o carrinho depois de já ter recebido um lembrete, O SISTEMA DEVE permitir um novo lembrete se esse carrinho for abandonado de novo depois.
6. O carrinho de um cliente não autenticado NÃO DEVE ser rastreado no servidor.

---

## Requisito 57: Consentimento de cookies

**User Story:** Como ateliê, quero pedir consentimento antes de carregar cookies de análise, para respeitar a privacidade do visitante e a legislação aplicável.

**Rastreamento:** RF69.

**Acceptance Criteria**
1. O SISTEMA DEVE exibir um aviso de cookies ao visitante que ainda não fez uma escolha.
2. QUANDO o visitante aceita, O SISTEMA DEVE carregar os scripts de análise configurados (se houver) e não exibir o aviso novamente.
3. QUANDO o visitante recusa, O SISTEMA NÃO DEVE carregar nenhum script de análise, e não deve exibir o aviso novamente.
4. A escolha do visitante DEVE ser lembrada entre visitas (mesmo navegador).

---

## Requisito 58: Fotos em avaliações de produto

**User Story:** Como cliente, quero anexar uma foto à minha avaliação, para mostrar como a peça personalizada ficou de verdade.

**Rastreamento:** RF70.

**Acceptance Criteria**
1. O cliente elegível a avaliar um produto DEVE poder, opcionalmente, anexar uma foto à avaliação antes de enviá-la.
2. A foto enviada DEVE passar pela mesma otimização (redimensionamento/compressão) aplicada a qualquer outra imagem do sistema.
3. Uma avaliação sem foto DEVE continuar funcionando normalmente (a foto é opcional, não obrigatória).
4. A foto DEVE aparecer junto com a avaliação na listagem pública do produto.

---

## Requisito 59: Log de auditoria administrativo

**User Story:** Como ateliê, quero saber quem fez qual alteração no painel administrativo e quando, para ter responsabilização caso mais de uma pessoa tenha acesso.

**Rastreamento:** RF71.

**Acceptance Criteria**
1. O SISTEMA DEVE registrar quem (qual admin), o quê (que ação) e quando, para as principais ações administrativas: alterações de produto, mudança de status de pedido, criação/ativação de cupom, edição de cliente e login administrativo.
2. O administrador DEVE poder consultar esse histórico, paginado, do mais recente para o mais antigo.
3. O registro de auditoria NÃO DEVE poder ser editado ou apagado pela interface administrativa.

---

## Requisito 60: Autenticação de dois fatores para administrador

**User Story:** Como ateliê, quero exigir um segundo fator de autenticação no login administrativo, para reduzir o risco de invasão do painel caso a senha vaze.

**Rastreamento:** RF72.

**Acceptance Criteria**
1. O administrador DEVE poder ativar a autenticação de dois fatores (TOTP) na própria conta, escaneando ou digitando manualmente uma chave em um aplicativo autenticador.
2. A ativação SÓ DEVE se efetivar após o administrador confirmar um código válido gerado a partir dessa chave.
3. QUANDO a autenticação de dois fatores está ativa, O SISTEMA DEVE exigir um código válido, além de e-mail e senha corretos, para completar o login.
4. Um código inválido ou expirado NÃO DEVE permitir o login.
5. O administrador DEVE poder desativar a autenticação de dois fatores, mediante confirmação da senha atual.

---

## Requisito 61: Teste automatizado de ponta a ponta do fluxo de compra

**User Story:** Como ateliê, quero um teste automatizado que simule uma compra real, para detectar quebras no fluxo mais crítico do site antes que um cliente de verdade perceba.

**Rastreamento:** RNF10.

**Acceptance Criteria**
1. O SISTEMA DEVE ter um teste automatizado que simula, num navegador real, um visitante personalizando um produto, criando uma conta, preenchendo o checkout e confirmando o pedido.
2. O teste DEVE rodar contra o backend real (não simulado/mockado), verificando a integração de ponta a ponta.
3. O teste DEVE terminar confirmando que a página de pedido confirmado foi exibida.

---

## Requisito 62: Newsletter

**User Story:** Como ateliê, quero capturar e-mails de visitantes interessados em novidades, para poder anunciar promoções e lançamentos por e-mail no futuro.

**Rastreamento:** RF73.

**Acceptance Criteria**
1. O visitante DEVE poder se inscrever para receber novidades por e-mail, informando apenas o e-mail.
2. Inscrever um e-mail já inscrito, ou reinscrever um e-mail que havia se descadastrado, NÃO DEVE resultar em erro.
3. O administrador DEVE poder consultar a lista de inscritos ativos.
4. O administrador DEVE poder exportar a lista de inscritos ativos em CSV.
5. O SISTEMA NÃO PRECISA enviar campanhas de e-mail marketing — apenas capturar e exportar a lista para uso em uma ferramenta externa.

---

## Requisito 63: Cache de imagens e assets estáticos

**User Story:** Como ateliê, quero que fotos e arquivos do site sejam guardados em cache pelo navegador do visitante, para o site carregar mais rápido em visitas repetidas e reduzir a carga no servidor.

**Rastreamento:** RNF11.

**Acceptance Criteria**
1. O SISTEMA DEVE fornecer a configuração de cabeçalhos de cache para imagens enviadas (nome de arquivo imutável — nunca muda de conteúdo uma vez criado).
2. O SISTEMA DEVE fornecer a configuração de cabeçalhos de cache para os arquivos JS/CSS do build do frontend (nome com hash de conteúdo).
3. O arquivo `index.html` NÃO DEVE ser cacheado, para que uma nova versão do site seja sempre servida corretamente após um deploy.
4. A instalação da configuração de cache é um passo manual documentado (a configuração do Nginx na VPS não é gerenciada por este repositório), assim como o backup do banco (Requisito 33).
5. Uma CDN (ex. Cloudflare) na frente do domínio é uma melhoria opcional adicional, documentada mas não aplicada automaticamente — exige trocar os servidores de nome do domínio, uma decisão e execução do ateliê, não do sistema.
