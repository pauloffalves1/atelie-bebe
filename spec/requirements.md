# Requirements Document — Ateliê Bebê

## Introdução

O Ateliê Bebê é uma plataforma de e-commerce e gestão de encomendas para um ateliê de enxovais e itens artesanais de bebê. Este documento formaliza, no padrão *Spec-Driven Development* (user story + critérios de aceite em EARS — Easy Approach to Requirements Syntax), os requisitos do sistema **tal como construído**. Ele complementa (não substitui) `README.md`, que mantém a tabela de requisitos numerados (RF01–RF25, RNF01–RNF08) usada como referência rápida — cada requisito abaixo cita o(s) RF/RNF correspondente(s) para rastreabilidade.

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
5. QUANDO um visitante acessa um produto por slug válido, O SISTEMA DEVE exibir nome, categoria, preço, descrição, imagem e estoque disponível.
6. SE o slug não corresponder a nenhum produto, ENTÃO O SISTEMA DEVE responder com 404 e a UI DEVE exibir uma página "produto não encontrado" com um link de volta à loja.
7. O SISTEMA NUNCA DEVE incluir produtos com `Active = false` nas listagens públicas (loja, destaque, busca por categoria).

---

## Requisito 2: Carrinho e checkout de loja

**User Story:** Como visitante ou cliente, quero adicionar produtos a um carrinho e finalizar a compra, para receber os itens escolhidos.

**Rastreamento:** RF05, RF21, RF25, RNF07

**Acceptance Criteria**
1. QUANDO um usuário adiciona um produto ao carrinho, O CLIENTE (frontend) DEVE limitar a quantidade ao estoque disponível do produto (`min(quantidade solicitada, stock)`).
2. QUANDO o carrinho é persistido, O CLIENTE DEVE gravá-lo em `localStorage`, sobrevivendo a recarregamentos de página.
3. QUANDO um usuário confirma o checkout, O SISTEMA DEVE criar um pedido do tipo `Loja` com um item por produto do carrinho.
4. PARA CADA item do pedido de loja, O SISTEMA DEVE reservar o estoque do produto correspondente (`Product.Reserve`) na mesma operação de criação do pedido.
5. SE a quantidade solicitada de um item exceder o estoque disponível do produto, ENTÃO O SISTEMA DEVE rejeitar a criação do pedido inteiro com um erro de domínio, sem reservar estoque parcialmente.
6. SE o pedido de loja não tiver nenhum item, ENTÃO O SISTEMA DEVE rejeitá-lo (`ConflictException`/`DomainException`).
7. QUANDO a requisição de checkout parte de um cliente autenticado, O SISTEMA DEVE vincular o pedido criado ao `CustomerId` desse cliente.
8. QUANDO a requisição de checkout parte de um visitante não autenticado, O SISTEMA DEVE aceitar o pedido mesmo assim, com `CustomerId = null`.
9. A gravação do pedido e o registro do evento `OrderCreatedDomainEvent` na tabela de outbox DEVEM ocorrer na mesma transação de banco de dados.
10. QUANDO o pedido está no status `Recebido`, O SISTEMA PODE aceitar adição de itens; APÓS o pedido sair do status `Recebido`, O SISTEMA DEVE rejeitar qualquer tentativa de adicionar ou alterar itens.

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

**User Story:** Como administrador, quero cadastrar, editar e controlar a visibilidade e o estoque dos produtos, para manter o catálogo atualizado.

**Rastreamento:** RF12, RF13, RF14, RF15, RF16

**Acceptance Criteria**
1. QUANDO um administrador lista produtos, O SISTEMA DEVE incluir tanto ativos quanto inativos (diferente da listagem pública).
2. QUANDO um administrador cadastra um novo produto, O SISTEMA DEVE gerar um slug a partir do nome; SE o slug colidir com um existente, ENTÃO O SISTEMA DEVE adicionar um sufixo aleatório para garantir unicidade.
3. SE nome, slug OU categoria estiverem vazios, OU o estoque informado for negativo, ENTÃO O SISTEMA DEVE rejeitar a criação/edição do produto.
4. QUANDO um administrador edita os dados de um produto, O SISTEMA DEVE atualizar nome, descrição, preço, categoria, imagem e destaque sem alterar o estoque (o estoque tem endpoint próprio).
5. QUANDO um administrador ajusta o estoque manualmente, O SISTEMA DEVE rejeitar valores negativos.
6. QUANDO o estoque de um produto (por ajuste manual ou reserva) chega a 3 unidades ou menos, O SISTEMA DEVE emitir um evento de estoque baixo.
7. QUANDO um administrador ativa ou inativa um produto, O SISTEMA DEVE refletir imediatamente essa mudança na visibilidade da loja pública.

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
1. O painel DEVE exibir: total de pedidos, pedidos em aberto, receita total, receita do mês, total de produtos, produtos com estoque baixo, total de clientes, distribuição de pedidos por status e os pedidos mais recentes.
2. Pedidos com status `Cancelado` NÃO DEVEM ser contabilizados em nenhuma métrica de receita nem na contagem de "pedidos em aberto".
3. "Pedidos em aberto" DEVE contar pedidos em `Recebido`, `EmProducao`, `Pronto` ou `Enviado`.
4. "Receita do mês" DEVE somar apenas pedidos criados a partir do primeiro dia do mês corrente, calculado em UTC.
5. "Produtos com estoque baixo" DEVE contar apenas produtos ativos com estoque ≤ 3.

---

## Requisito 11: Comportamentos automáticos do sistema

**User Story:** Como sistema, preciso reagir automaticamente a eventos de negócio (estoque, pedidos, mensagens), para manter consistência de dados e manter os envolvidos informados, sem depender de ação manual.

**Rastreamento:** RF21, RF22, RF23, RF24, RNF06, RNF07

**Acceptance Criteria**
1. QUANDO uma entidade de domínio levanta um evento (criação de pedido, mudança de status, cadastro de cliente, estoque baixo, mensagem de contato recebida), O SISTEMA DEVE gravar esse evento na tabela de outbox na MESMA transação que originou a mudança de estado.
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
