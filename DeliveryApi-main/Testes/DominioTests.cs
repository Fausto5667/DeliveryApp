using Xunit;
using DeliveryApi.Dominio;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dominio.Fabricas;
using DeliveryApi.Dominio.Frete;

namespace DeliveryApi.Tests;

public class ItemCardapioTests
{
    [Fact]
    public void ItemCardapio_EhAbstrataEPratoBebidaSobremesaHerdamDela()
    {
        Assert.True(typeof(ItemCardapio).IsAbstract);
        Assert.True(typeof(Prato).IsSubclassOf(typeof(ItemCardapio)));
        Assert.True(typeof(Bebida).IsSubclassOf(typeof(ItemCardapio)));
        Assert.True(typeof(Sobremesa).IsSubclassOf(typeof(ItemCardapio)));
    }

    [Fact]
    public void Polimorfismo_CadaSubclasseResponderDiferenteSemCheckagemDeTipo()
    {
        var itens = new List<ItemCardapio>
        {
            new Prato(1, "Lasanha", 45m, 30),
            new Bebida(2, "Suco", 8m, 350),
            new Sobremesa(3, "Pudim", 12m, true),
            new Sobremesa(4, "Brigadeiro", 5m, false)
        };

        Assert.Equal(
            new[] { "Preparo: 30 min", "350 ml", "Gelada", "Natural" },
            itens.Select(i => i.ObterDetalhes()));

        Assert.Equal(
            new[] { CategoriaItem.Prato, CategoriaItem.Bebida, CategoriaItem.Sobremesa, CategoriaItem.Sobremesa },
            itens.Select(i => i.Categoria));

        Assert.Equal(new[] { 30, 0, 0, 0 }, itens.Select(i => i.TempoPreparoMinutos));
    }

    [Theory]
    [InlineData(0, "Nome", 10, "código")]
    [InlineData(-1, "Nome", 10, "código")]
    [InlineData(1, "", 10, "nome")]
    [InlineData(1, "   ", 10, "nome")]
    [InlineData(1, "Nome", 0, "preço")]
    [InlineData(1, "Nome", -5, "preço")]
    [InlineData(1, "Nome", 0.004, "preço")]
    [InlineData(1, "Nome", 10001, "preço")]
    public void Construtor_RejeitaDadosInvalidos(int codigo, string nome, double preco, string trechoMensagem)
    {
        var erro = Assert.Throws<DominioException>(() => new Sobremesa(codigo, nome, (decimal)preco, true));
        Assert.Contains(trechoMensagem, erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nome_EAparadoEPrecoArredondadoParaCentavos()
    {
        var item = new Sobremesa(1, "  Pudim  ", 10.005m, true);

        Assert.Equal("Pudim", item.Nome);
        Assert.Equal(10.01m, item.PrecoBase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(601)]
    public void Prato_RejeitaTempoDePreparoInvalido(int tempo)
    {
        Assert.Throws<DominioException>(() => new Prato(1, "Prato", 10m, tempo));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-350)]
    [InlineData(10001)]
    public void Bebida_RejeitaVolumeInvalido(int volume)
    {
        Assert.Throws<DominioException>(() => new Bebida(1, "Suco", 10m, volume));
    }

    [Fact]
    public void Fabrica_CriaCadaCategoriaEExigeOCampoEspecifico()
    {
        Assert.IsType<Prato>(FabricaItemCardapio.Criar(CategoriaItem.Prato, 1, "P", 10m, 10, null, null));
        Assert.IsType<Bebida>(FabricaItemCardapio.Criar(CategoriaItem.Bebida, 2, "B", 10m, null, 300, null));
        Assert.IsType<Sobremesa>(FabricaItemCardapio.Criar(CategoriaItem.Sobremesa, 3, "S", 10m, null, null, false));

        Assert.Throws<DominioException>(() => FabricaItemCardapio.Criar(CategoriaItem.Prato, 1, "P", 10m, null, null, null));
        Assert.Throws<DominioException>(() => FabricaItemCardapio.Criar(CategoriaItem.Bebida, 1, "B", 10m, null, null, null));
        Assert.Throws<DominioException>(() => FabricaItemCardapio.Criar(CategoriaItem.Sobremesa, 1, "S", 10m, null, null, null));
        Assert.Throws<DominioException>(() => FabricaItemCardapio.Criar((CategoriaItem)99, 1, "X", 10m, 1, 1, true));
    }
}

public class ClienteEEntregadorTests
{
    [Theory]
    [InlineData("(11) 99999-0000", "11999990000")]
    [InlineData("11 99999 0000", "11999990000")]
    [InlineData("+55 (11) 99999-0000", "5511999990000")]
    [InlineData("  1199999.0000 ", "11999990000")]
    public void Telefone_ENormalizadoParaDigitos(string informado, string esperado)
    {
        Assert.Equal(esperado, Cliente.NormalizarTelefone(informado));
        Assert.Equal(esperado, new Cliente("Ana", informado, "Rua A, 10").Telefone);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1199999-000a")]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("11999990000; DROP TABLE")]
    public void Telefone_InvalidoEhRejeitado(string? telefone)
    {
        Assert.Throws<DominioException>(() => Cliente.NormalizarTelefone(telefone));
    }

    [Theory]
    [InlineData("", "11999990000", "Rua A")]
    [InlineData("Ana", "11999990000", "")]
    [InlineData("  ", "11999990000", "Rua A")]
    public void Cliente_RejeitaCamposObrigatoriosVazios(string nome, string telefone, string endereco)
    {
        Assert.Throws<DominioException>(() => new Cliente(nome, telefone, endereco));
    }

    [Fact]
    public void Entregador_ComecaDisponivelEOcuparDuasVezesGeraConflito()
    {
        var entregador = new Entregador(1, "Carlos", TipoVeiculo.Moto);
        Assert.True(entregador.Disponivel);

        entregador.Ocupar();
        Assert.False(entregador.Disponivel);

        Assert.Throws<ConflitoException>(entregador.Ocupar);

        entregador.Liberar();
        Assert.True(entregador.Disponivel);
    }

    [Fact]
    public void Entregador_RejeitaVeiculoForaDaEnumeracaoENomeVazio()
    {
        Assert.Throws<DominioException>(() => new Entregador(1, "Carlos", (TipoVeiculo)42));
        Assert.Throws<DominioException>(() => new Entregador(1, " ", TipoVeiculo.Carro));
        Assert.Throws<DominioException>(() => new Entregador(0, "Carlos", TipoVeiculo.Carro));
    }

    [Fact]
    public void Entregador_OcuparEmParaleloAlocaNoMaximoUmaVez()
    {
        var entregador = new Entregador(1, "Carlos", TipoVeiculo.Moto);
        var sucessos = 0;

        Parallel.For(0, 200, _ =>
        {
            try
            {
                entregador.Ocupar();
                Interlocked.Increment(ref sucessos);
            }
            catch (ConflitoException)
            {
            }
        });

        Assert.Equal(1, sucessos);
    }
}

public class FreteTests
{
    private static readonly ICalculadoraFrete Calculadora =
        new CalculadoraFreteGratisAcimaDaMeta(new CalculadoraFretePorVeiculo());

    [Theory]
    [InlineData(TipoVeiculo.Bicicleta, 10, 10.00)]
    [InlineData(TipoVeiculo.Moto, 10, 15.00)]
    [InlineData(TipoVeiculo.Carro, 10, 20.00)]
    [InlineData(TipoVeiculo.Moto, 3.333, 5.00)]
    public void Frete_DependeDoVeiculoEDaDistancia(TipoVeiculo veiculo, double km, double esperado)
    {
        Assert.Equal((decimal)esperado, Calculadora.Calcular(veiculo, (decimal)km, 20m));
    }

    [Theory]
    [InlineData(79.99, false)]
    [InlineData(80.00, false)]
    [InlineData(80.01, true)]
    [InlineData(500, true)]
    public void Frete_EGratisSomenteQuandoOValorSuperaAMeta(double valorPedido, bool gratis)
    {
        var frete = Calculadora.Calcular(TipoVeiculo.Carro, 10m, (decimal)valorPedido);

        Assert.Equal(gratis, frete == 0m);
    }

    [Fact]
    public void Frete_VeiculoInvalidoEhRejeitado()
    {
        Assert.Throws<DominioException>(() => new CalculadoraFretePorVeiculo().Calcular((TipoVeiculo)9, 1m, 1m));
    }
}

public class PedidoTests
{
    private static readonly ICalculadoraFrete Frete =
        new CalculadoraFreteGratisAcimaDaMeta(new CalculadoraFretePorVeiculo());

    private static Cliente NovoCliente() => new("Ana", "11999990000", "Rua A, 10");

    private static Entregador NovoEntregador(TipoVeiculo veiculo = TipoVeiculo.Moto) => new(1, "Carlos", veiculo);

    private static Pedido NovoPedido(Entregador? entregador = null, decimal km = 10m) =>
        new(1, NovoCliente(), entregador ?? NovoEntregador(), km);

    [Fact]
    public void TempoDePreparo_EOMaiorTempoENaoASoma()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Prato(1, "Salada", 20m, 10), 1);
        pedido.AdicionarItem(new Prato(2, "Lasanha", 45m, 25), 1);
        pedido.AdicionarItem(new Bebida(3, "Suco", 8m, 300), 2);

        Assert.Equal(25, pedido.TempoPreparoMinutos);
    }

    [Fact]
    public void TempoDePreparo_SemPratosEZero()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);

        Assert.Equal(0, pedido.TempoPreparoMinutos);
        Assert.Equal(0, NovoPedido().TempoPreparoMinutos);
    }

    [Fact]
    public void Subtotal_SomaOsSubtotaisDeCadaItem()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Prato(1, "Lasanha", 45.50m, 25), 2);
        pedido.AdicionarItem(new Bebida(2, "Suco", 8m, 300), 3);

        Assert.Equal(91m + 24m, pedido.Subtotal);
        Assert.Equal(new[] { 91m, 24m }, pedido.Itens.Select(i => i.Subtotal));
    }

    [Fact]
    public void ItemRepetido_AcumulaQuantidadeEmUmaUnicaLinha()
    {
        var pedido = NovoPedido();
        var suco = new Bebida(1, "Suco", 8m, 300);

        pedido.AdicionarItem(suco, 2);
        pedido.AdicionarItem(suco, 3);

        var linha = Assert.Single(pedido.Itens);
        Assert.Equal(5, linha.Quantidade);
    }

    [Fact]
    public void Quantidade_InvalidaOuAcimaDoLimiteEhRejeitada()
    {
        var pedido = NovoPedido();
        var suco = new Bebida(1, "Suco", 8m, 300);

        Assert.Throws<DominioException>(() => pedido.AdicionarItem(suco, 0));
        Assert.Throws<DominioException>(() => pedido.AdicionarItem(suco, -1));
        Assert.Throws<DominioException>(() => pedido.AdicionarItem(suco, 101));
        Assert.Throws<DominioException>(() => pedido.AdicionarItem(null!, 1));

        pedido.AdicionarItem(suco, 100);
        Assert.Throws<DominioException>(() => pedido.AdicionarItem(suco, 1));
    }

    [Fact]
    public void Confirmar_SemItensEhRejeitadoENaoOcupaOEntregador()
    {
        var entregador = NovoEntregador();
        var pedido = NovoPedido(entregador);

        var erro = Assert.Throws<DominioException>(() => pedido.Confirmar(Frete));

        Assert.Equal("Pedido sem itens.", erro.Message);
        Assert.True(entregador.Disponivel);
    }

    [Fact]
    public void Confirmar_OcupaOEntregadorEFixaOFrete()
    {
        var entregador = NovoEntregador(TipoVeiculo.Moto);
        var pedido = NovoPedido(entregador, km: 10m);
        pedido.AdicionarItem(new Prato(1, "Salada", 20m, 10), 1);

        pedido.Confirmar(Frete);

        Assert.False(entregador.Disponivel);
        Assert.Equal(15m, pedido.Frete);
        Assert.Equal(35m, pedido.Total);
        Assert.True(pedido.Confirmado);
    }

    [Fact]
    public void Confirmar_FreteGratisQuandoSubtotalSuperaAMeta()
    {
        var pedido = NovoPedido(km: 30m);
        pedido.AdicionarItem(new Prato(1, "Banquete", 90m, 40), 1);

        pedido.Confirmar(Frete);

        Assert.Equal(0m, pedido.Frete);
        Assert.Equal(90m, pedido.Total);
    }

    [Fact]
    public void Confirmar_EntregadorOcupadoGeraConflitoENaoAlteraOPedido()
    {
        var entregador = NovoEntregador();
        var primeiro = NovoPedido(entregador);
        primeiro.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);
        primeiro.Confirmar(Frete);

        var segundo = new Pedido(2, NovoCliente(), entregador, 5m);
        segundo.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);

        Assert.Throws<ConflitoException>(() => segundo.Confirmar(Frete));
        Assert.False(segundo.Confirmado);
        Assert.Equal(0m, segundo.Frete);
    }

    [Fact]
    public void Pedido_ConfirmadoNaoAceitaNovosItensNemNovaConfirmacao()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);
        pedido.Confirmar(Frete);

        Assert.Throws<DominioException>(() => pedido.AdicionarItem(new Bebida(2, "Agua", 3m, 500), 1));
        Assert.Throws<DominioException>(() => pedido.Confirmar(Frete));
    }

    [Fact]
    public void Avancar_PercorreAsEtapasEmOrdemELiberaOEntregadorNaEntrega()
    {
        var entregador = NovoEntregador();
        var pedido = NovoPedido(entregador);
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);
        pedido.Confirmar(Frete);

        var percorridos = new List<StatusPedido> { pedido.Status };
        for (var i = 0; i < 4; i++)
        {
            pedido.Avancar();
            percorridos.Add(pedido.Status);
        }

        Assert.Equal(
            new[] { StatusPedido.Recebido, StatusPedido.EmPreparo, StatusPedido.Pronto, StatusPedido.EmRota, StatusPedido.Entregue },
            percorridos);

        Assert.True(entregador.Disponivel);
        Assert.NotNull(pedido.EntregueEm);
        Assert.Null(pedido.ProximoStatus);
    }

    [Fact]
    public void Avancar_EntregadorSoELiberadoNaEntrega()
    {
        var entregador = NovoEntregador();
        var pedido = NovoPedido(entregador);
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);
        pedido.Confirmar(Frete);

        pedido.Avancar();
        pedido.Avancar();
        pedido.Avancar();

        Assert.Equal(StatusPedido.EmRota, pedido.Status);
        Assert.False(entregador.Disponivel);
    }

    [Fact]
    public void Avancar_AposEntregueGeraConflito()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);
        pedido.Confirmar(Frete);
        for (var i = 0; i < 4; i++) pedido.Avancar();

        var erro = Assert.Throws<ConflitoException>(pedido.Avancar);

        Assert.Equal("Pedido já foi entregue.", erro.Message);
        Assert.Equal(StatusPedido.Entregue, pedido.Status);
    }

    [Fact]
    public void Avancar_PedidoNaoConfirmadoEhRejeitado()
    {
        Assert.Throws<DominioException>(() => NovoPedido().Avancar());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(501)]
    public void Pedido_RejeitaDistanciaInvalida(double km)
    {
        Assert.Throws<DominioException>(() => NovoPedido(km: (decimal)km));
    }

    [Fact]
    public void Pedido_RejeitaClienteOuEntregadorNulos()
    {
        Assert.Throws<DominioException>(() => new Pedido(1, null!, NovoEntregador(), 1m));
        Assert.Throws<DominioException>(() => new Pedido(1, NovoCliente(), null!, 1m));
        Assert.Throws<DominioException>(() => new Pedido(0, NovoCliente(), NovoEntregador(), 1m));
    }

    [Fact]
    public void Itens_NaoPodemSerAlteradosPorFora()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem(new Bebida(1, "Suco", 8m, 300), 1);

        Assert.IsNotType<List<ItemPedido>>(pedido.Itens);
        Assert.Throws<NotSupportedException>(() => ((IList<ItemPedido>)pedido.Itens).Add(null!));
    }

    [Fact]
    public void SequenciaDeStatus_CobreOsCincoEstadosSemPular()
    {
        var atual = (StatusPedido?)StatusPedido.Recebido;
        var sequencia = new List<StatusPedido>();

        while (atual.HasValue)
        {
            sequencia.Add(atual.Value);
            atual = atual.Value.ObterProximo();
        }

        Assert.Equal(Enum.GetValues<StatusPedido>().OrderBy(s => (int)s), sequencia);
    }

    [Fact]
    public void RegrasDeNegocio_TarifasEMetaConferemComOEnunciado()
    {
        Assert.Equal(80m, RegrasDeNegocio.MetaFreteGratis);
        Assert.Equal(1.00m, RegrasDeNegocio.TaxaPorKm(TipoVeiculo.Bicicleta));
        Assert.Equal(1.50m, RegrasDeNegocio.TaxaPorKm(TipoVeiculo.Moto));
        Assert.Equal(2.00m, RegrasDeNegocio.TaxaPorKm(TipoVeiculo.Carro));
    }
}
