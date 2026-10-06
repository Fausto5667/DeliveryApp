using Xunit;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dominio.Frete;
using DeliveryApi.Repositorios;
using DeliveryApi.Servicos;

namespace DeliveryApi.Tests;

public class ServicosTests
{
    private (CardapioServico, ClienteServico, EntregadorServico, PedidoServico, RelatorioServico) CriarAmbiente()
    {
        var repoItens = new RepositorioItemCardapioMemoria();
        var repoClientes = new RepositorioClienteMemoria();
        var repoEntregadores = new RepositorioEntregadorMemoria();
        var repoPedidos = new RepositorioPedidoMemoria();

        var freteBase = new CalculadoraFretePorVeiculo();
        var freteEstrategia = new CalculadoraFreteGratisAcimaDaMeta(freteBase, 50.00m);

        var cardapio = new CardapioServico(repoItens);
        var clientes = new ClienteServico(repoClientes);
        var entregadores = new EntregadorServico(repoEntregadores);
        var pedidos = new PedidoServico(repoPedidos, repoClientes, repoEntregadores, repoItens, freteEstrategia);
        var relatorios = new RelatorioServico(repoPedidos);

        return (cardapio, clientes, entregadores, pedidos, relatorios);
    }

    [Fact]
    public void ClienteServico_CadastraENaoPermiteTelefoneDuplicado()
    {
        var (_, clientes, _, _, _) = CriarAmbiente();

        var c1 = clientes.Cadastrar("Maria Souza", "(11) 98765-4321", "Rua das Flores, 123");
        Assert.Equal("11987654321", c1.Telefone);

        var erro = Assert.Throws<ConflitoException>(() =>
            clientes.Cadastrar("Maria Clone", "11 98765-4321", "Outro Endereço, 456"));
        Assert.Contains("Já existe um cliente cadastrado com este telefone", erro.Message);
    }

    [Fact]
    public void ClienteServico_ObterPorTelefone_EncontraFormatadoOuNao()
    {
        var (_, clientes, _, _, _) = CriarAmbiente();
        clientes.Cadastrar("Carlos Mendes", "11988887777", "Av Central, 50");

        var c = clientes.ObterPorTelefone("(11) 98888-7777");
        Assert.Equal("Carlos Mendes", c.Nome);

        var erro = Assert.Throws<NaoEncontradoException>(() => clientes.ObterPorTelefone("11900000000"));
        Assert.Contains("não encontrado", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CardapioServico_AdicionaEBuscaPorTermo()
    {
        var (cardapio, _, _, _, _) = CriarAmbiente();

        cardapio.Adicionar(new NovoItemCardapio(CategoriaItem.Prato, "Lasanha Bolonhesa", 42.00m, 25, null, null));
        cardapio.Adicionar(new NovoItemCardapio(CategoriaItem.Bebida, "Suco de Laranja", 9.00m, null, 400, null));
        cardapio.Adicionar(new NovoItemCardapio(CategoriaItem.Sobremesa, "Pudim de Leite", 12.00m, null, null, true));

        var buscaLasanha = cardapio.BuscarPorNome("lasanha");
        Assert.Single(buscaLasanha);
        Assert.Equal("Lasanha Bolonhesa", buscaLasanha[0].Nome);

        var todos = cardapio.Listar();
        Assert.Equal(3, todos.Count);
    }

    [Fact]
    public void PedidoServico_SimulacaoNaoCriaPedidoNemOcupaEntregador()
    {
        var (cardapio, clientes, entregadores, pedidos, _) = CriarAmbiente();

        var prato = cardapio.Adicionar(new NovoItemCardapio(CategoriaItem.Prato, "Hambúrguer Gourmet", 35.00m, 20, null, null));
        clientes.Cadastrar("Lucas", "11999998888", "Rua 1, 10");
        var motoboy = entregadores.Cadastrar("Pedro Motoboy", TipoVeiculo.Moto);

        var dados = new NovoPedido(
            "11999998888",
            motoboy.Id,
            4.0m,
            new[] { new ItemSolicitado(prato.Codigo, 1) });

        var simulacao = pedidos.Simular(dados);

        // Subtotal = 35.00, Frete Moto: 4.0 km * 1.50 = 6.00. Total = 41.00
        Assert.Equal(35.00m, simulacao.Subtotal);
        Assert.Equal(6.00m, simulacao.Frete);
        Assert.Equal(41.00m, simulacao.Total);
        Assert.Equal(20, simulacao.TempoPreparoMinutos);

        // Garante que nenhum pedido foi criado e entregador continua disponível
        Assert.Empty(pedidos.Listar());
        Assert.True(motoboy.Disponivel);
    }

    [Fact]
    public void PedidoServico_FluxoCompleto_FreteGratis_OcupacaoELiberacaoDeEntregador()
    {
        var (cardapio, clientes, entregadores, pedidos, relatorios) = CriarAmbiente();

        var prato = cardapio.Adicionar(new NovoItemCardapio(CategoriaItem.Prato, "Combo Pizza Grande", 60.00m, 30, null, null));
        clientes.Cadastrar("Beatriz", "11912345678", "Rua das Palmeiras, 99");
        var entregador = entregadores.Cadastrar("Diego", TipoVeiculo.Carro);

        // 1. Criar pedido acima de 50 -> Frete Grátis
        var pedido = pedidos.Criar(new NovoPedido(
            "11912345678",
            entregador.Id,
            5.0m,
            new[] { new ItemSolicitado(prato.Codigo, 1) }));

        Assert.Equal(StatusPedido.Recebido, pedido.Status);
        Assert.Equal(60.00m, pedido.Subtotal);
        Assert.Equal(0m, pedido.Frete);
        Assert.Equal(60.00m, pedido.Total);
        Assert.False(entregador.Disponivel, "Entregador alocado para o pedido fica reservado/ocupado.");

        // 2. Tentar alocar entregador ocupado para novo pedido deve lançar ConflitoException
        var erroOcupado = Assert.Throws<ConflitoException>(() =>
            pedidos.Criar(new NovoPedido(
                "11912345678",
                entregador.Id,
                2.0m,
                new[] { new ItemSolicitado(prato.Codigo, 1) })));
        Assert.Contains("indisponível", erroOcupado.Message, StringComparison.OrdinalIgnoreCase);

        // 3. Avançar: Recebido -> EmPreparo
        var pEmPreparo = pedidos.Avancar(pedido.Id);
        Assert.Equal(StatusPedido.EmPreparo, pEmPreparo.Status);

        // 4. Avançar: EmPreparo -> Pronto
        var pPronto = pedidos.Avancar(pedido.Id);
        Assert.Equal(StatusPedido.Pronto, pPronto.Status);

        // 5. Avançar: Pronto -> EmRota
        var pEmRota = pedidos.Avancar(pedido.Id);
        Assert.Equal(StatusPedido.EmRota, pEmRota.Status);

        // 6. Avançar: EmRota -> Entregue
        var pEntregue = pedidos.Avancar(pedido.Id);
        Assert.Equal(StatusPedido.Entregue, pEntregue.Status);
        Assert.True(entregador.Disponivel, "Entregador deve ser LIBERADO após a entrega ser concluída.");

        // 7. Não pode avançar após Entregue
        Assert.Throws<ConflitoException>(() => pedidos.Avancar(pedido.Id));

        // Relatório deve computar o faturamento e o ranking
        var relatorio = relatorios.Gerar();
        Assert.Equal(1, relatorio.TotalPedidosEntregues);
        Assert.Equal(60.00m, relatorio.FaturamentoTotal);
        Assert.Single(relatorio.ItensMaisVendidos);
        Assert.Equal("Combo Pizza Grande", relatorio.ItensMaisVendidos[0].Nome);
    }

    [Fact]
    public void RepositorioMemoria_ThreadSafety_SuportaConcorrenciaMassiva()
    {
        var repo = new RepositorioClienteMemoria();
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(1, 151, i =>
        {
            try
            {
                var cliente = new Cliente($"Cliente {i}", $"119{i:D8}", $"Rua Teste {i}");
                repo.Adicionar(cliente);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.Equal(150, repo.ListarTodos().Count);
    }
}
