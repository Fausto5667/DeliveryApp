# Documentação Técnica e Acadêmica do Sistema de Delivery
**Disciplina:** Programação Orientada a Objetos (POO)  
**Professor:** Prof. Esp. Geison Mancuzo  
**Projeto:** Sistema de Delivery de Restaurante  
**Linguagem & Plataforma:** C# / .NET 8 (ASP.NET Core Web API + xUnit + Frontend SPA Vanilla)  

---

## 1. Visão Geral do Sistema e Arquitetura

O sistema foi desenvolvido como uma solução corporativa e acadêmica completa para o gerenciamento de um restaurante com serviço de entrega em domicílio (*delivery*). Diferente de implementações mínimas ou acadêmicas superficiais, a arquitetura do projeto adota **separação estrita de responsabilidades**, **Domain-Driven Design (DDD) tático**, **Clean Architecture** e aplicação fundamentada dos pilares da **Programação Orientada a Objetos**.

### 1.1 Diagrama de Camadas

```
┌───────────────────────────────────────────────────────────┐
│                       FRONTEND SPA                        │
│     (HTML5 Semântico, CSS3 Moderno, JavaScript Vanilla)   │
└─────────────────────────────┬─────────────────────────────┘
                              │ HTTP / JSON REST
                              ▼
┌───────────────────────────────────────────────────────────┐
│                    CAMADA DE API / REST                   │
│   Controllers Enxutas, DTOs Tipados, Middleware Global    │
└─────────────────────────────┬─────────────────────────────┘
                              │ Injeção de Dependência
                              ▼
┌───────────────────────────────────────────────────────────┐
│                    CAMADA DE SERVIÇOS                     │
│      Orquestração de Casos de Uso e Regras de Negócio     │
└─────────────────────────────┬─────────────────────────────┘
                              │
               ┌──────────────┴──────────────┐
               ▼                             ▼
┌──────────────────────────────┐ ┌──────────────────────────┐
│     CAMADA DE REPOSITÓRIOS   │ │     CAMADA DE DOMÍNIO    │
│  IRepositorio<T, TId>        │ │  Entidades, Enums,       │
│  RepositorioMemoria ThreadSafe│ │  Classes Abstratas,      │
└──────────────────────────────┘ │  Strategy, Fábricas      │
                                 └──────────────────────────┘
```

---

## 2. Mapeamento Completo dos Conceitos de POO

O sistema foi desenhado para que **cada conceito de POO resolva um problema legítimo do domínio gastronômico e logístico**, sem artifícios teóricos ou estruturas forçadas.

---

### 2.1 Abstração
- **Definição:** A abstração consiste em isolar os aspectos essenciais de um objeto para o contexto da aplicação, descartando detalhes acidentais ou irrelevantes.
- **Onde está aplicada no projeto:**
  1. [`ItemCardapio`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemCardapio.cs): Modela a essência de qualquer item alimentício comercializado (código identificador, nome comercial, preço base e categoria), abstraindo como o item é preparado ou servido.
  2. [`Cliente`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Cliente.cs): Abstrai os dados estritamente necessários para o atendimento e a entrega (nome, telefone identificador único normalizado e endereço de entrega).
  3. [`Entregador`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Entregador.cs): Abstrai o colaborador logístico em termos de sua modalidade de transporte (`TipoVeiculo`) e seu estado operacional (`Disponivel`).
  4. [`Pedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Pedido.cs): Abstrai o ciclo de venda, agrupando cliente solicitante, entregador encarregado, itens solicitados, taxas e máquina de estados.

---

### 2.2 Classe Abstrata
- **Definição:** Uma classe que não pode ser instanciada diretamente (`abstract class`). Serve como molde e base contratual para especializações concretas, podendo conter métodos com implementação e métodos abstratos sem corpo.
- **Onde está aplicada no projeto:**
  - [`ItemCardapio`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemCardapio.cs) é declarada como:
    ```csharp
    public abstract class ItemCardapio
    ```
- **Por que faz sentido no domínio:**
  - No mundo real de um restaurante, ninguém pede "um item genérico de cardápio"; pede-se especificamente um **Prato**, uma **Bebida** ou uma **Sobremesa**. Portanto, impedir `new ItemCardapio(...)` protege a integridade conceitual do sistema.
- **Membros Abstratos e Virtuais:**
  - `public abstract string ObterDetalhes();` -> Cada especialização é obrigada a descrever suas propriedades exclusivas.
  - `public abstract CategoriaItem Categoria { get; }` -> Garante a tipagem forte do item via Enum.
  - `public virtual int TempoPreparoMinutos => 0;` -> Fornece comportamento padrão neutro para bebidas e sobremesas, permitindo que pratos sobrescrevam com seu tempo real.

---

### 2.3 Herança
- **Definição:** Mecanismo pelo qual uma classe filha (subclasse) herda atributos e comportamentos de uma classe mãe (superclasse), promovendo reuso de código e especialização de comportamento.
- **Onde está aplicada no projeto:**
  1. **Hierarquia do Cardápio:**
     - [`Prato`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Prato.cs) herda de `ItemCardapio`. Adiciona a propriedade encapsulada `TempoPreparoMinutos` e validações de tempo de cozinha.
     - [`Bebida`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Bebida.cs) herda de `ItemCardapio`. Adiciona a propriedade `VolumeMl` e validações de volume líquido.
     - [`Sobremesa`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Sobremesa.cs) herda de `ItemCardapio`. Adiciona a indicação booleana `Gelada` (refrigerada vs. temperatura ambiente).
  2. **Hierarquia de Exceções de Domínio:**
     - `System.Exception` -> [`DominioException`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Excecoes/DominioException.cs)
     - `DominioException` -> [`NaoEncontradoException`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Excecoes/NaoEncontradoException.cs) (mapeada para HTTP 404).
     - `DominioException` -> [`ConflitoException`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Excecoes/ConflitoException.cs) (mapeada para HTTP 409).
  3. **Hierarquia de Repositórios (Template Method):**
     - [`RepositorioMemoria<T, TId>`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Repositorios/RepositorioMemoria.cs) é a classe base genérica e thread-safe.
     - `RepositorioClienteMemoria`, `RepositorioEntregadorMemoria`, `RepositorioItemCardapioMemoria` e `RepositorioPedidoMemoria` herdam dessa base, especializando apenas a extração da chave identificadora e a ordenação de listagem.

---

### 2.4 Encapsulamento
- **Definição:** Proteção do estado interno de um objeto contra modificações indevidas externas, expondo apenas operações seguras e validadas por meio de métodos e propriedades públicas.
- **Onde está aplicado no projeto:**
  1. **Propriedades com Setters Privados (`private set`):**
     - Em [`Cliente`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Cliente.cs), [`Entregador`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Entregador.cs), [`ItemCardapio`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemCardapio.cs) e [`Pedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Pedido.cs), nenhuma propriedade pode ser alterada diretamente de fora por atribuição arbitrária (`pedido.Status = ...` não compila fora da classe).
  2. **Coleções Imutáveis:**
     - A lista `_itens` de `Pedido` é privada (`private readonly List<ItemPedido> _itens`). O acesso externo é exposto estritamente como `public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();`. Ninguém fora de `Pedido` consegue executar `.Add()` ou `.Clear()`.
  3. **Invariantes nos Construtores:**
     - Nenhum objeto entra em estado inválido na aplicação. Se um cliente for instanciado sem endereço ou com telefone inconsistente, ou se um item tiver preço negativo, uma `DominioException` é lançada imediatamente no construtor.
  4. **Métodos de Mutação de Estado com Regras de Negócio:**
     - `entregador.Ocupar()` e `entregador.Liberar()`: garantem que um entregador ocupado não possa ser reocupado (lança `ConflitoException`).
     - `pedido.AdicionarItem(...)`, `pedido.Confirmar(...)`, `pedido.Avancar()`: garantem que a máquina de estados não pule etapas e que o pedido não seja alterado após confirmado.

---

### 2.5 Polimorfismo
- **Definição:** Capacidade de objetos de diferentes tipos responderem à mesma mensagem ou invocação de método de formas distintas e adequadas à sua especialização.
- **Onde está aplicado no projeto:**
  1. **Polimorfismo por Sobrescrita de Método (`override`):**
     - Ao percorrer uma lista heterogênea `List<ItemCardapio>`, o sistema invoca `item.ObterDetalhes()`:
       - Se for `Prato`: retorna `"Preparo: 30 min"`.
       - Se for `Bebida`: retorna `"350 ml"`.
       - Se for `Sobremesa`: retorna `"Gelada"` ou `"Natural"`.
     - Nenhum `if (item is Prato)` ou `switch` é executado na camada de consumo (princípio Open/Closed do SOLID).
  2. **Polimorfismo no Tempo de Cozinha:**
     - `item.TempoPreparoMinutos`:
       - Em `Prato`, retorna o tempo real informado (ex: `25`).
       - Em `Bebida` e `Sobremesa`, retorna `0` pela implementação herdada.
     - O cálculo em `Pedido.TempoPreparoMinutos => _itens.Max(i => i.Item.TempoPreparoMinutos)` opera de forma 100% polimórfica e paralela.
  3. **Polimorfismo de Interface (Strategy Pattern):**
     - [`ICalculadoraFrete.Calcular(...)`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Frete/ICalculadoraFrete.cs): invocado uniformemente por `Pedido`, respondendo polimorficamente seja pela calculadora de tarifas por veículo (`CalculadoraFretePorVeiculo`), seja pelo decorador de gratuidade por meta mínima (`CalculadoraFreteGratisAcimaDaMeta`).

---

### 2.6 Associação
- **Definição:** Relacionamento estrutural entre classes independentes que se comunicam para realizar uma ação, sem vínculo de dependência de ciclo de vida.
- **Onde está aplicada no projeto:**
  - [`Pedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Pedido.cs) mantém associação com [`Cliente`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Cliente.cs) e com [`Entregador`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Entregador.cs):
    ```csharp
    public Cliente Cliente { get; private set; }
    public Entregador Entregador { get; private set; }
    ```
  - **Justificativa do Domínio:** O cliente existe antes do pedido e continuará existindo depois que o pedido for finalizado. Da mesma forma, o entregador pertence à frota do restaurante e atende múltiplos pedidos ao longo do tempo.

---

### 2.7 Composição
- **Definição:** Relacionamento todo-parte ("tem um") onde a existência da parte é estritamente subordinada ao todo. Se o objeto pai for destruído, suas partes deixam de ter sentido e são destruídas com ele.
- **Onde está aplicada no projeto:**
  - [`Pedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/Pedido.cs) e [`ItemPedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemPedido.cs):
    ```csharp
    private readonly List<ItemPedido> _itens = new();
    ```
  - **Justificativa do Domínio:** Uma linha de pedido (`ItemPedido` - que guarda a quantidade solicitada de um item naquele momento específico) nasce de dentro do pedido e morre com o pedido. Não existe uma linha de pedido "solta" no restaurante sem pertencer a um pedido concreto.

---

### 2.8 Agregação
- **Definição:** Relacionamento todo-parte onde a parte pode existir de forma autônoma e independente do todo. O objeto pai apenas agrupa ou faz referência a objetos que já possuem ciclo de vida próprio.
- **Onde está aplicada no projeto:**
  1. [`ItemPedido`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemPedido.cs) e [`ItemCardapio`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Entidades/ItemCardapio.cs):
     - `ItemPedido` agrega uma referência a um `ItemCardapio`. O item do cardápio existe no menu do restaurante antes de qualquer pedido e permanece inalterado mesmo que o pedido seja cancelado ou arquivado.
  2. [`CalculadoraFreteGratisAcimaDaMeta`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Frete/CalculadoraFreteGratisAcimaDaMeta.cs) e [`ICalculadoraFrete`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Frete/ICalculadoraFrete.cs):
     - A calculadora decoradora agrega uma instância interna de `ICalculadoraFrete`, delegando a ela o cálculo tarifário padrão caso a meta de gratuidade não seja atingida.

---

### 2.9 Interfaces
- **Definição:** Contrato formal abstrato que especifica o conjunto de métodos e propriedades que uma classe deve implementar, desacoplando o código de implementações concretas e viabilizando inversão de dependência (DIP do SOLID).
- **Onde estão aplicadas no projeto:**
  1. [`IRepositorio<T, TId>`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Repositorios/IRepositorio.cs):
     - Contrato genérico com operações `Adicionar(T)`, `ObterPorId(TId)`, `ListarTodos()` e `Remover(TId)`.
     - Permite que a camada de serviços interaja com os repositórios sem saber se os dados estão armazenados em memória (ConcurrentDictionary), em um banco relacional SQL ou NoSQL.
  2. [`ICalculadoraFrete`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Dominio/Frete/ICalculadoraFrete.cs):
     - Define o contrato `decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido);`.
     - Desacopla a entidade `Pedido` do algoritmo matemático e das políticas de promoção de frete.

---

## 3. Padrões de Projeto (Design Patterns)

| Padrão | Classe / Interface | Papel no Domínio do Delivery |
|---|---|---|
| **Strategy** | `ICalculadoraFrete`, `CalculadoraFretePorVeiculo`, `CalculadoraFreteGratisAcimaDaMeta` | Permite alternar políticas de frete em tempo de execução sem alterar a classe `Pedido`. |
| **Factory Method** | `FabricaItemCardapio` | Centraliza a instanciação polimórfica de itens do cardápio (`Prato`, `Bebida`, `Sobremesa`) a partir da categoria recebida. |
| **Decorator** | `CalculadoraFreteGratisAcimaDaMeta` | Envolve a calculadora de tarifas agregada adicionando a verificação de gratuidade sem modificar o algoritmo de cálculo de quilometragem. |
| **Repository** | `IRepositorio<T, TId>`, `RepositorioMemoria<T, TId>` | Abstrai a camada de armazenamento com operações CRUD thread-safe e tipadas. |
| **State Machine** | `StatusPedido`, `StatusPedidoExtensoes`, `Pedido.Avancar()` | Garante a progressão linear estrita do ciclo de vida do pedido: `Recebido -> EmPreparo -> Pronto -> EmRota -> Entregue`. |

---

## 4. Regras de Negócio e Robustez Operacional

Todas as **37 regras de negócio** estipuladas no escopo acadêmico foram rigorosamente implementadas e testadas:

1. **Gestão de Itens Gastronômicos:**
   - Todo item possui código único positivo, nome não vazio aparado (trim) e preço base em reais maior que zero e menor que R$ 10.000,00.
   - Pratos: tempo de preparo obrigatório entre 1 e 600 minutos.
   - Bebidas: volume em ml obrigatório entre 1 e 10.000 ml.
   - Sobremesas: indicador booleano explícito se é servida gelada/refrigerada ou em temperatura ambiente.
2. **Cadastro e Unicidade de Clientes:**
   - Telefone é a chave identificadora primária única.
   - Normalização automática (apenas dígitos numéricos, com validação de 10 ou 11 dígitos padrão Brasil com DDD).
   - Tentativa de cadastrar cliente com telefone já existente lança `ConflitoException` (HTTP 409).
3. **Frota e Logística de Entregadores:**
   - Modalidades de veículo restritas ao enum `TipoVeiculo` (`Moto`, `Bicicleta`, `Carro`).
   - Um entregador ocupado não pode assumir novos pedidos. A ocupação ocorre na confirmação do pedido e a liberação é automática na transição para `Entregue`.
4. **Cálculo de Frete e Gratuidade:**
   - Tarifas por veículo:
     - Bicicleta: R$ 1,00 por km.
     - Moto: R$ 1,50 por km.
     - Carro: R$ 2,00 por km.
   - Meta mínima de frete grátis: subtotal de itens > R$ 80,00 concede frete grátis automático (R$ 0,00).
5. **Cozinha Paralela:**
   - A preparação dos pratos na cozinha ocorre em paralelo. Portanto, o tempo estimado de preparo do pedido é determinado pelo **item de maior tempo** (`_itens.Max(i => i.Item.TempoPreparoMinutos)`), e não pela soma ingênua dos pratos. Bebidas e sobremesas têm tempo zero.
6. **Máquina de Estados Sequencial Estrita:**
   - Ciclo obrigatório: `Recebido` (1) ➔ `EmPreparo` (2) ➔ `Pronto` (3) ➔ `EmRota` (4) ➔ `Entregue` (5).
   - É proibido pular etapas ou regredir status. Ao atingir `Entregue`, o pedido registra o carimbo de data/hora (`EntregueEm`) e libera o entregador.
7. **Busca Parcial Case-Insensitive:**
   - A rota de consulta do cardápio pesquisa por correspondência de substring ignorando diferenças entre maiúsculas, minúsculas e espaços excedentes.
8. **Relatório Gerencial:**
   - Faturamento total: soma apenas pedidos no status `Entregue` (a receita só se realiza na entrega confirmada).
   - Tempo médio de entrega: calculado com base na diferença real entre `CriadoEm` e `EntregueEm`.
   - Ranking: lista os 5 itens com maior quantidade vendida.

---

## 5. Arquitetura da Camada de Serviços e Controllers Enxutas

Para manter a separação correta de responsabilidades e atender ao princípio de Responsabilidade Única (SRP):
- **Controllers NÃO possuem regras de negócio:**
  - As classes [`CardapioController`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Controllers/CardapioController.cs), [`ClientesController`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Controllers/ClientesController.cs), [`EntregadoresController`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Controllers/EntregadoresController.cs), [`PedidosController`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Controllers/PedidosController.cs) e [`RelatoriosController`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Controllers/RelatoriosController.cs) têm função estritamente de orquestração de protocolo HTTP: recebem DTOs, acionam as Services correspondentes e devolvem códigos de status HTTP apropriados (`200 OK`, `201 Created`, etc.).
- **Services detêm a lógica de aplicação:**
  - [`CardapioServico`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Servicos/CardapioServico.cs)
  - [`ClienteServico`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Servicos/ClienteServico.cs)
  - [`EntregadorServico`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Servicos/EntregadorServico.cs)
  - [`PedidoServico`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Servicos/PedidoServico.cs)
  - [`RelatorioServico`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Servicos/RelatorioServico.cs)

---

## 6. Cobertura de Testes Automatizados (xUnit)

O projeto possui uma suíte com **76 testes automatizados** cobrindo todos os níveis da aplicação:

1. **Testes de Domínio ([`DominioTests.cs`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Testes/DominioTests.cs)):**
   - Herança e natureza abstrata de `ItemCardapio`.
   - Polimorfismo de `ObterDetalhes()` e `TempoPreparoMinutos`.
   - Invariantes de construtor (preços negativos, nomes vazios, volumes inválidos).
   - Fórmulas de frete e meta de gratuidade.
   - Ocupação e liberação de entregadores.
   - Máquina de estados do pedido e proteção contra modificação após confirmação.
2. **Testes de Serviços e Concorrência ([`ServicosTests.cs`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Testes/ServicosTests.cs)):**
   - Unicidade de telefone para clientes e entregadores.
   - Simulação de frete sem alteração de estado nem bloqueio de entregador.
   - Fluxo completo de criação e avanço de pedidos até a entrega.
   - Teste de concorrência massiva (`Parallel.For` com 150 threads simultâneas) validando o thread-safety do `RepositorioMemoria`.
3. **Testes de Integração de API HTTP ([`ApiIntegrationTests.cs`](file:///c:/Users/faust/Desktop/Fatec/TrabalhoGeison/DeliveryApi/DeliveryApi-main/Testes/ApiIntegrationTests.cs)):**
   - Execução de ponta a ponta via `WebApplicationFactory<Program>`, testando o pipeline completo: serialização JSON de enums em texto, respostas padronizadas do middleware, códigos de status HTTP e contratos de resposta.

---

## 7. Roteiro de Apresentação e Defesa Acadêmica

Para a defesa perante o professor e banca avaliadora, o grupo pode estruturar a apresentação da seguinte maneira:

1. **Abertura (Visão do Domínio):**
   - Demonstrar que o sistema não é um CRUD genérico, mas um ecossistema real de delivery com cozinha paralela, logística multimodal e regras de frete dinâmico.
2. **Defesa do Tópico 1 (Conceitos Fundamentais):**
   - *Onde está a Abstração?* Em `ItemCardapio`, `Cliente`, `Entregador` e `Pedido`.
   - *Onde está a Classe Abstrata?* Em `public abstract class ItemCardapio`, impedindo itens genéricos no restaurante.
   - *Onde está a Herança?* Em `Prato`, `Bebida` e `Sobremesa` derivando de `ItemCardapio`, herdando código base e adicionando atributos específicos.
   - *Onde está o Encapsulamento?* Setters privados, coleções imutáveis `IReadOnlyList`, construtores validando todas as entradas e métodos de mutação controlada (`Ocupar()`, `Avancar()`).
   - *Onde está o Polimorfismo?* Na chamada `item.ObterDetalhes()` e `item.TempoPreparoMinutos`, onde cada subclasse responde de forma própria sem verificações de tipo na camada consumidora; e em `ICalculadoraFrete`.
   - *Onde estão Associação, Composição e Agregação?*
     - Associação: `Pedido` associa-se a `Cliente` e `Entregador` (entidades autônomas).
     - Composição: `Pedido` compõe suas linhas `ItemPedido` (nascem e morrem juntas).
     - Agregação: `ItemPedido` agrega `ItemCardapio` (o item do menu existe independentemente).
   - *Onde estão as Interfaces?* `IRepositorio<T, TId>` para persistência e `ICalculadoraFrete` para cálculo de entrega.
3. **Demonstração Prática:**
   - Executar os testes via terminal: `dotnet test DeliveryApi-main\DeliveryApi.csproj` (76 testes aprovados).
   - Abrir o frontend no navegador e demonstrar o cadastro no cardápio, a simulação instantânea de frete (com frete grátis para pedidos acima de R$ 80,00), a ocupação em tempo real do entregador e o avanço sequencial do status do pedido até a entrega e o relatório gerencial.

---

## 8. Como Executar o Projeto

1. **Executar a suíte de testes:**
   ```powershell
   dotnet test DeliveryApi-main\DeliveryApi.csproj
   ```
2. **Executar a API Backend:**
   ```powershell
   dotnet run --project DeliveryApi-main\DeliveryApi.csproj
   ```
   *A API estará disponível em `http://localhost:5173` (e a interface Swagger em `http://localhost:5173/swagger`).*
3. **Acessar o Front-end:**
   Basta abrir `http://localhost:5173` no navegador (servido diretamente pelo backend) ou abrir o arquivo `frontend/index.html` no navegador.
