# Guia de Implementação: API Delivery (C# / .NET 8)

Projeto de POO, etapa 1. Persistência **em memória**. Este documento foi escrito para ser entregue a desenvolvedores humanos e a agentes de IA. Cada tarefa é pequena, tem entregável claro e critério de aceite.

---

## 0. Regras para quem for executar (humano ou IA)

1. Execute as tarefas **na ordem**. Cada fase depende da anterior.
2. Faça **uma tarefa por vez** e confirme com `dotnet build` antes de seguir.
3. **Não coloque regra de negócio em Controller.** Controller só recebe, chama o Service e devolve a resposta.
4. **Não exponha entidades de domínio diretamente** nas respostas. Use DTOs.
5. Toda propriedade de entidade tem `private set`. Toda coleção interna é exposta como `IReadOnlyList<T>`.
6. Nenhuma exceção pode derrubar a API. O middleware global (Fase 7) converte tudo em resposta HTTP controlada.
7. Nomes de classes, métodos e comentários em **português**, para a defesa oral.
8. Comente **por que** cada conceito de POO foi aplicado (veja o mapa na seção 1).

---

## 1. Mapa dos conceitos de POO

| Conceito | Onde aparece | Justificativa de domínio |
| --- | --- | --- |
| **Abstração** | `ItemCardapio` modela o que todo item tem em comum | Cardápio tem pratos, bebidas e sobremesas |
| **Classe abstrata** | `ItemCardapio` (não pode ser instanciada) | "Item genérico" não existe no cardápio real |
| **Herança** | `Prato`, `Bebida`, `Sobremesa` herdam de `ItemCardapio` | Relação "é um" real; cada um tem atributo próprio |
| **Polimorfismo** | `ObterDetalhes()` (abstract) e `TempoPreparoMinutos` (virtual) sobrescritos | Pedido trata todos como `ItemCardapio` sem `if` de tipo |
| **Encapsulamento** | `private set`, validação nos construtores, `Ocupar()/Liberar()`, `Avancar()` | Estado inválido é impossível de criar |
| **Associação** | `Pedido` → `Cliente`, `Pedido` → `Entregador`, `ItemPedido` → `ItemCardapio` | Objetos independentes que se relacionam |

> Já preparado para as próximas aulas: `ItemPedido` dentro de `Pedido` é **composição** (não existe sem o pedido). Repositórios atrás de interfaces (`IRepositorio<T>`) preparam o conceito de **interface**.

---

## 2. Modelagem de dados

### 2.1 Enums

```
TipoVeiculo   : Moto, Bicicleta, Carro
StatusPedido  : Recebido, EmPreparo, Pronto, EmRota, Entregue
```

### 2.2 Diagrama de classes (texto)

```
            <<abstract>>
           ItemCardapio
  +Codigo : int (único)
  +Nome : string
  +PrecoBase : decimal
  +Categoria : string        (abstract)
  +ObterDetalhes() : string  (abstract)
  +TempoPreparoMinutos : int (virtual, padrão 0)
        ▲         ▲          ▲
      Prato     Bebida    Sobremesa
  +TempoPreparo +VolumeMl  +Gelada : bool
   (override    
   TempoPreparoMinutos)

Cliente                    Entregador
  +Telefone (único)          +Id : int
  +Nome                      +Nome
  +Endereco                  +Veiculo : TipoVeiculo
                             +Disponivel : bool
                             +Ocupar() / Liberar()

Pedido
  +Id : int
  +Cliente : Cliente                 (associação)
  +Entregador : Entregador           (associação)
  +DistanciaKm : decimal
  +Itens : IReadOnlyList<ItemPedido> (composição)
  +Status : StatusPedido
  +CriadoEm / EntregueEm : DateTime
  +Subtotal, Frete, Total : decimal
  +TempoPreparoMinutos : int         (maior entre os itens)
  +AdicionarItem(item, qtd)
  +Avancar()

ItemPedido
  +Item : ItemCardapio               (associação)
  +Quantidade : int
  +Subtotal : decimal
```

### 2.3 Regras numéricas (constantes, em uma classe `RegrasDeNegocio`)

| Regra | Valor |
| --- | --- |
| Frete grátis a partir de (subtotal) | R$ 80,00 |
| Taxa por km: Bicicleta | R$ 1,00 |
| Taxa por km: Moto | R$ 1,50 |
| Taxa por km: Carro | R$ 2,00 |
| Frete | `taxaPorKm(veículo) × distânciaKm`; se `Subtotal >= 80` então `0` |

---

## 3. Estrutura de pastas

```
DeliveryApi/
├── Program.cs
├── Dominio/
│   ├── Enums/            TipoVeiculo.cs, StatusPedido.cs
│   ├── Entidades/        ItemCardapio.cs, Prato.cs, Bebida.cs, Sobremesa.cs,
│   │                     Cliente.cs, Entregador.cs, Pedido.cs, ItemPedido.cs
│   ├── Excecoes/         DominioException.cs, NaoEncontradoException.cs
│   └── RegrasDeNegocio.cs
├── Repositorios/         IRepositorio.cs, RepositorioMemoria.cs (ou um por entidade)
├── Servicos/             CardapioServico.cs, ClienteServico.cs,
│                         EntregadorServico.cs, PedidoServico.cs, RelatorioServico.cs
├── Dtos/                 (requests e responses)
├── Controllers/          CardapioController.cs, ClientesController.cs,
│                         EntregadoresController.cs, PedidosController.cs,
│                         RelatoriosController.cs
└── Middleware/           TratamentoErrosMiddleware.cs
```

---

## 4. Plano de execução

### FASE 1: Projeto base

**Tarefa 1.1:** Criar a solução.

```bash
dotnet new webapi -n DeliveryApi --use-controllers
cd DeliveryApi
```

**Tarefa 1.2:** Criar as pastas da seção 3 e apagar o `WeatherForecast` de exemplo. **Aceite:** `dotnet run` sobe e o Swagger abre.

---

### FASE 2: Exceções e enums

**Tarefa 2.1:** Criar os enums da seção 2.1.

**Tarefa 2.2:** Criar as exceções de domínio.

```csharp
public class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem) { }
}
public class NaoEncontradoException : DominioException
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}
```

**Tarefa 2.3:** Criar `RegrasDeNegocio` com as constantes da seção 2.3.

**Aceite:** compila.

---

### FASE 3: Hierarquia do cardápio (Abstração, Herança, Polimorfismo, Encapsulamento)

**Tarefa 3.1:** Classe abstrata `ItemCardapio`.

```csharp
public abstract class ItemCardapio
{
    public int Codigo { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoBase { get; private set; }

    protected ItemCardapio(int codigo, string nome, decimal precoBase)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do item é obrigatório.");
        if (precoBase <= 0)
            throw new DominioException("O preço base deve ser maior que zero.");
        Codigo = codigo; Nome = nome.Trim(); PrecoBase = precoBase;
    }

    public abstract string Categoria { get; }       // polimorfismo
    public abstract string ObterDetalhes();         // polimorfismo
    public virtual int TempoPreparoMinutos => 0;    // sobrescrito só por Prato
}
```

**Tarefa 3.2:** `Prato : ItemCardapio` com `TempoPreparo` (minutos, > 0). Sobrescreve `Categoria => "Prato"`, `ObterDetalhes()` (ex.: `"Preparo: 25 min"`) e `TempoPreparoMinutos`.

**Tarefa 3.3:** `Bebida : ItemCardapio` com `VolumeMl` (> 0). `Categoria => "Bebida"`, `ObterDetalhes()` → `"350 ml"`.

**Tarefa 3.4:** `Sobremesa : ItemCardapio` com `Gelada` (bool). `Categoria => "Sobremesa"`, `ObterDetalhes()` → `"Gelada"` ou `"Natural"`.

**Regras:** construtor de cada filha chama `base(...)` e valida o próprio atributo. Nada de `public set`.

**Aceite:**

- `new ItemCardapio(...)` **não compila**.
- `Prato` com tempo 0 lança `DominioException`.
- Uma `List<ItemCardapio>` com os 3 tipos chama `ObterDetalhes()` e cada um responde diferente, sem `if`/`switch` de tipo.

---

### FASE 4: Cliente e Entregador (Encapsulamento)

**Tarefa 4.1:** `Cliente(nome, telefone, endereco)`. Todos obrigatórios (não vazios). `Telefone` é o identificador único e é **somente leitura** (sem setter).

**Tarefa 4.2:** `Entregador(id, nome, veiculo)`.

- `Disponivel` começa `true`, com `private set`.
- `Ocupar()`: se `!Disponivel` lança `DominioException("Entregador indisponível.")`; senão `Disponivel = false`.
- `Liberar()`: `Disponivel = true`.

**Aceite:** chamar `Ocupar()` duas vezes seguidas lança exceção na segunda.

---

### FASE 5: Pedido (Associação, Composição, máquina de estados)

**Tarefa 5.1:** `ItemPedido(ItemCardapio item, int quantidade)`. Quantidade > 0. Propriedade `Subtotal => Item.PrecoBase * Quantidade`.

**Tarefa 5.2:** `Pedido`, campos e criação.

- Construtor recebe `id, cliente, entregador, distanciaKm`. Valida não nulos e `distanciaKm > 0`.
- Lista privada `_itens` exposta como `IReadOnlyList<ItemPedido>`.
- `Status` inicia em `Recebido`. `CriadoEm = DateTime.Now`.

**Tarefa 5.3:** `AdicionarItem(ItemCardapio item, int qtd)`. Só permitido com status `Recebido`; senão lança `DominioException`.

**Tarefa 5.4:** Cálculos.

```csharp
public decimal Subtotal => _itens.Sum(i => i.Subtotal);
public int TempoPreparoMinutos => _itens.Any() ? _itens.Max(i => i.Item.TempoPreparoMinutos) : 0; // paralelo: maior tempo
public decimal Frete => Subtotal >= RegrasDeNegocio.MetaFreteGratis
    ? 0m
    : RegrasDeNegocio.TaxaPorKm(Entregador.Veiculo) * DistanciaKm;
public decimal Total => Subtotal + Frete;
```

**Tarefa 5.5:** Método `Confirmar()`, chamado pelo serviço depois de montar os itens: se não há itens lança `DominioException("Pedido sem itens.")`; senão chama `Entregador.Ocupar()`.

**Tarefa 5.6:** `Avancar()`: move o status para o próximo da sequência **sem pular etapas**. `Recebido → EmPreparo → Pronto → EmRota → Entregue`.

- Se já está `Entregue`, lança `DominioException("Pedido já foi entregue.")`.
- Ao chegar em `Entregue`: registra `EntregueEm = DateTime.Now` e chama `Entregador.Liberar()`.

**Aceite:**

- Pedido com prato de 10 min e prato de 25 min → `TempoPreparoMinutos == 25`.
- Subtotal ≥ 80 → `Frete == 0`.
- Entregador ocupado não consegue ser usado em um segundo pedido.
- Tentar avançar um pedido entregue lança exceção.

---

### FASE 6: Repositórios e serviços (em memória)

**Tarefa 6.1:** Interface genérica e implementação em memória (prepara o conceito de interface).

```csharp
public interface IRepositorio<T>
{
    void Adicionar(T entidade);
    T? ObterPorId(int id);
    IReadOnlyList<T> ListarTodos();
}
```

Para `Cliente`, a chave é o telefone (string), então faça um repositório próprio ou use `Dictionary<string, Cliente>`. Registre todos como **Singleton** no `Program.cs` (senão os dados somem a cada requisição).

**Tarefa 6.2:** `CardapioServico`.

- `Adicionar(...)` gera o `Codigo` sequencial (contador interno) e instancia `Prato`, `Bebida` ou `Sobremesa`.
- `Listar()` retorna todos. Se vazio, retorna lista vazia (sem erro).
- `BuscarPorNome(string termo)`:
  - termo vazio → `DominioException("Informe um termo de busca.")`
  - `Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)`

**Tarefa 6.3:** `ClienteServico`. Cadastrar, rejeitando telefone duplicado com `DominioException`. Listar.

**Tarefa 6.4:** `EntregadorServico`. Cadastrar e listar (mostra o campo `Disponivel`).

**Tarefa 6.5:** `PedidoServico`.

- `Criar(telefoneCliente, entregadorId, distanciaKm, itens[])`:
  1. busca cliente (não achou → `NaoEncontradoException`)
  2. busca entregador (não achou → `NaoEncontradoException`)
  3. cria `Pedido`, adiciona cada item (código inexistente → `NaoEncontradoException`)
  4. `Confirmar()` (valida itens e ocupa o entregador)
  5. salva e retorna
- `Avancar(id)`.
- `ListarPorStatus(StatusPedido status)`: retorna vazio se não houver nenhum.

**Tarefa 6.6:** `RelatorioServico.Gerar()` retorna:

- **Itens mais vendidos:** agrupar `ItemPedido` por item, somar quantidades, ordenar decrescente (top 5).
- **Tempo médio de entrega:** média de `(EntregueEm − CriadoEm)` dos pedidos entregues, em minutos. Sem pedidos entregues → `0` com mensagem.
- **Faturamento total:** soma de `Total` dos pedidos com status `Entregue`.

---

### FASE 7: DTOs, controllers e tratamento global de erros

**Tarefa 7.1:** DTOs de entrada (records) com validação por atributos (`[Required]`, `[Range]`):

- `CriarItemRequest { Tipo ("prato"|"bebida"|"sobremesa"), Nome, PrecoBase, TempoPreparo?, VolumeMl?, Gelada? }`
- `CriarClienteRequest { Nome, Telefone, Endereco }`
- `CriarEntregadorRequest { Nome, Veiculo }`
- `CriarPedidoRequest { TelefoneCliente, EntregadorId, DistanciaKm, Itens: [{ CodigoItem, Quantidade }] }`

**Tarefa 7.2:** DTOs de saída. Item: `Codigo, Nome, PrecoBase, Categoria, Detalhes`. Pedido: `Id, Status, Cliente, Entregador, Itens (com subtotal), Subtotal, Frete, Total, TempoPreparoMinutos`.

**Tarefa 7.3:** Endpoints (um para cada item do menu do enunciado):

| Menu | Método e rota |
| --- | --- |
| 1 | `POST /api/cardapio` |
| 2 | `GET /api/cardapio` |
| 3 | `GET /api/cardapio/buscar?nome=...` |
| 4 | `POST /api/clientes` |
| 5 | `POST /api/entregadores` |
| 6 | `POST /api/pedidos` |
| 7 | `PATCH /api/pedidos/{id}/avancar` |
| 8 | `GET /api/pedidos?status=EmPreparo` |
| 9 | `GET /api/relatorios/gerencial` |

Extras úteis: `GET /api/clientes`, `GET /api/entregadores`. Configure o JSON para aceitar enums como texto: `.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))`.

**Tarefa 7.4:** `TratamentoErrosMiddleware`, registrado antes dos controllers.

- `NaoEncontradoException` → **404**
- `DominioException` → **400** (ou **409** para entregador indisponível e telefone duplicado)
- qualquer outra `Exception` → **500** com mensagem genérica (sem stack trace)
- corpo sempre em JSON: `{ "erro": "mensagem clara" }`

**Aceite da fase:** nenhuma requisição derruba a aplicação, mesmo com corpo vazio, ID inexistente ou enum inválido.

---

### FASE 8: Dados iniciais e testes manuais

**Tarefa 8.1:** Opcional, mas recomendado. Popular no startup 2 pratos, 2 bebidas, 1 sobremesa, 1 cliente e 2 entregadores para facilitar a demonstração.

**Tarefa 8.2:** Roteiro de teste obrigatório (use o Swagger ou um arquivo `.http`):

1. Criar prato "Lasanha" (R$ 45, 30 min), prato "Salada" (R$ 20, 10 min), bebida e sobremesa.
2. `GET /api/cardapio` lista os 4 itens.
3. Buscar `?nome=LASA` encontra "Lasanha" (parcial e sem diferenciar maiúsculas).
4. Buscar `?nome=` vazio → 400 com mensagem clara.
5. Cadastrar cliente e entregador (moto).
6. Criar pedido com Lasanha + Salada → `TempoPreparoMinutos = 30`.
7. Criar **segundo** pedido com o mesmo entregador → **409** (ocupado).
8. Criar pedido com lista de itens vazia → **400**.
9. `PATCH avancar` 4 vezes até `Entregue`. A 5ª tentativa → 400.
10. Após `Entregue`, o entregador volta a ficar disponível.
11. Pedido com subtotal ≥ 80 → `Frete = 0`.
12. `GET /api/pedidos?status=Pronto` sem nenhum pedido nesse status → lista vazia, status 200.
13. `GET /api/relatorios/gerencial` mostra mais vendidos, tempo médio e faturamento.

---

## 5. Divisão sugerida entre integrantes (módulos individuais)

O enunciado exige que cada aluno domine um módulo e responda na arguição oral.

| Integrante | Módulo | Fases |
| --- | --- | --- |
| A | **Regras de domínio**: hierarquia do cardápio, exceções, enums | 2 e 3 |
| B | **Domínio de pessoas e pedido**: Cliente, Entregador, Pedido, máquina de estados | 4 e 5 |
| C | **Serviços e repositórios**: lógica de aplicação, busca, relatório | 6 |
| D | **API e testes**: DTOs, controllers, middleware, roteiro de teste | 1, 7 e 8 |

> Cada integrante deve saber explicar **por que** a herança foi usada em `ItemCardapio` e não em `Entregador`, e como o polimorfismo elimina `if` de tipo.

---

## 6. Checklist de pronto

- [ ] `ItemCardapio` é abstrata e não instanciável
- [ ] Três subclasses com atributo próprio e `override` de membros abstratos/virtuais
- [ ] Nenhum `if (item is Prato)` no código
- [ ] Entidades sem `public set`; coleções somente leitura
- [ ] Máquina de estados sem pular etapas
- [ ] Entregador ocupado não é alocado
- [ ] Frete por veículo e distância, grátis acima da meta
- [ ] Tempo de preparo do pedido = maior tempo entre os pratos
- [ ] Busca parcial sem diferenciar maiúsculas/minúsculas
- [ ] Middleware global; nenhuma exceção chega crua ao cliente
- [ ] Os 9 endpoints funcionando e o roteiro de teste passando
- [ ] Comentários explicando cada conceito de POO aplicado
