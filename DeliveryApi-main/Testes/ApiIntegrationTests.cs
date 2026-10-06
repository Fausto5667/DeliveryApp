using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DeliveryApi.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(AppContext.BaseDirectory);
        }).CreateClient();
    }

    [Fact]
    public async Task Cardapio_FluxoCompletoApi_CadastrarListarBuscar()
    {
        // 1. Cadastra um Prato
        var novoPrato = new CriarItemRequest(
            Tipo: CategoriaItem.Prato,
            Nome: "Risoto de Alho-Poró",
            PrecoBase: 38.50m,
            TempoPreparo: 25,
            VolumeMl: null,
            Gelada: null);

        var respostaPost = await _client.PostAsJsonAsync("/api/cardapio", novoPrato, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);
        var itemCriado = await respostaPost.Content.ReadFromJsonAsync<ItemCardapioResponse>(JsonOptions);
        Assert.NotNull(itemCriado);
        Assert.Equal("Risoto de Alho-Poró", itemCriado.Nome);
        Assert.Equal(CategoriaItem.Prato, itemCriado.Categoria);

        // 2. Cadastra uma Bebida
        var novaBebida = new CriarItemRequest(
            Tipo: CategoriaItem.Bebida,
            Nome: "Refrigerante Cola",
            PrecoBase: 7.00m,
            TempoPreparo: null,
            VolumeMl: 350,
            Gelada: null);

        var respostaBebida = await _client.PostAsJsonAsync("/api/cardapio", novaBebida, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, respostaBebida.StatusCode);

        // 3. Cadastra uma Sobremesa
        var novaSobremesa = new CriarItemRequest(
            Tipo: CategoriaItem.Sobremesa,
            Nome: "Mousse de Maracujá",
            PrecoBase: 14.00m,
            TempoPreparo: null,
            VolumeMl: null,
            Gelada: true);

        var respostaSobremesa = await _client.PostAsJsonAsync("/api/cardapio", novaSobremesa, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, respostaSobremesa.StatusCode);

        // 4. Listar
        var respostaListar = await _client.GetAsync("/api/cardapio");
        Assert.Equal(HttpStatusCode.OK, respostaListar.StatusCode);
        var lista = await respostaListar.Content.ReadFromJsonAsync<List<ItemCardapioResponse>>(JsonOptions);
        Assert.NotNull(lista);
        Assert.True(lista.Count >= 3);

        // 5. Buscar por nome
        var respostaBusca = await _client.GetAsync("/api/cardapio/buscar?nome=risoto");
        Assert.Equal(HttpStatusCode.OK, respostaBusca.StatusCode);
        var busca = await respostaBusca.Content.ReadFromJsonAsync<List<ItemCardapioResponse>>(JsonOptions);
        Assert.NotNull(busca);
        Assert.Contains(busca, i => i.Nome.Contains("Risoto", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Clientes_CadastrarComTelefoneInvalido_Retorna400BadRequest()
    {
        var requisicaoInvalida = new CriarClienteRequest(
            Nome: "Cliente Errado",
            Telefone: "123", // Menos de 10 dígitos numéricos
            Endereco: "Rua Valida, 100");

        var resposta = await _client.PostAsJsonAsync("/api/clientes", requisicaoInvalida, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Entregadores_CadastrarEListar()
    {
        var requisicao = new CriarEntregadorRequest(
            Nome: "Entregador Piloto",
            Veiculo: TipoVeiculo.Moto);

        var respostaPost = await _client.PostAsJsonAsync("/api/entregadores", requisicao, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, respostaPost.StatusCode);
        var entregador = await respostaPost.Content.ReadFromJsonAsync<EntregadorResponse>(JsonOptions);
        Assert.NotNull(entregador);
        Assert.Equal(TipoVeiculo.Moto, entregador.Veiculo);
        Assert.True(entregador.Disponivel);

        var respostaListar = await _client.GetAsync("/api/entregadores");
        Assert.Equal(HttpStatusCode.OK, respostaListar.StatusCode);
        var lista = await respostaListar.Content.ReadFromJsonAsync<List<EntregadorResponse>>(JsonOptions);
        Assert.NotNull(lista);
        Assert.Contains(lista, e => e.Id == entregador.Id);
    }

    [Fact]
    public async Task Pedido_FluxoCompletoPelaApiComAvancoDeStatusERelatorios()
    {
        // 1. Cadastra Cliente
        var tel = $"119{Random.Shared.Next(10000000, 99999999)}";
        var clienteResp = await _client.PostAsJsonAsync("/api/clientes", new CriarClienteRequest("Joana", tel, "Rua dos Pinheiros, 500"), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, clienteResp.StatusCode);

        // 2. Cadastra Entregador
        var entregadorResp = await _client.PostAsJsonAsync("/api/entregadores", new CriarEntregadorRequest("Marcos", TipoVeiculo.Bicicleta), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, entregadorResp.StatusCode);
        var entregador = await entregadorResp.Content.ReadFromJsonAsync<EntregadorResponse>(JsonOptions);
        Assert.NotNull(entregador);

        // 3. Cadastra Prato
        var pratoResp = await _client.PostAsJsonAsync("/api/cardapio", new CriarItemRequest(CategoriaItem.Prato, "Nhoque ao Sugo", 32.00m, 18, null, null), JsonOptions);
        Assert.Equal(HttpStatusCode.Created, pratoResp.StatusCode);
        var prato = await pratoResp.Content.ReadFromJsonAsync<ItemCardapioResponse>(JsonOptions);
        Assert.NotNull(prato);

        // 4. Simulação de Pedido
        var simulacaoReq = new CriarPedidoRequest(
            TelefoneCliente: tel,
            EntregadorId: entregador.Id,
            DistanciaKm: 3.0m,
            Itens: new List<ItemPedidoRequest> { new(prato.Codigo, 3) }); // Subtotal = 96.00 (> 80 -> Frete Grátis)

        var respostaSimulacao = await _client.PostAsJsonAsync("/api/pedidos/simulacao", simulacaoReq, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, respostaSimulacao.StatusCode);
        var simulacao = await respostaSimulacao.Content.ReadFromJsonAsync<SimulacaoPedidoResponse>(JsonOptions);
        Assert.NotNull(simulacao);
        Assert.Equal(96.00m, simulacao.Subtotal);
        Assert.Equal(0m, simulacao.Frete);
        Assert.Equal(96.00m, simulacao.Total);
        Assert.True(simulacao.FreteGratis);

        // 5. Criação do Pedido Real
        var criarPedidoReq = new CriarPedidoRequest(
            TelefoneCliente: tel,
            EntregadorId: entregador.Id,
            DistanciaKm: 3.0m,
            Itens: new List<ItemPedidoRequest> { new(prato.Codigo, 3) });

        var respostaPedido = await _client.PostAsJsonAsync("/api/pedidos", criarPedidoReq, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, respostaPedido.StatusCode);
        var pedido = await respostaPedido.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.NotNull(pedido);
        Assert.Equal(StatusPedido.Recebido, pedido.Status);
        Assert.Equal(96.00m, pedido.Total);

        // 6. Avanço: Recebido -> EmPreparo
        var avanco1 = await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null);
        Assert.Equal(HttpStatusCode.OK, avanco1.StatusCode);
        var p1 = await avanco1.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.Equal(StatusPedido.EmPreparo, p1!.Status);

        // 7. Avanço: EmPreparo -> Pronto
        var avanco2 = await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null);
        Assert.Equal(HttpStatusCode.OK, avanco2.StatusCode);
        var p2 = await avanco2.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.Equal(StatusPedido.Pronto, p2!.Status);

        // 8. Avanço: Pronto -> EmRota
        var avanco3 = await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null);
        Assert.Equal(HttpStatusCode.OK, avanco3.StatusCode);
        var p3 = await avanco3.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.Equal(StatusPedido.EmRota, p3!.Status);

        // 9. Avanço: EmRota -> Entregue
        var avanco4 = await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null);
        Assert.Equal(HttpStatusCode.OK, avanco4.StatusCode);
        var p4 = await avanco4.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);
        Assert.Equal(StatusPedido.Entregue, p4!.Status);

        // 10. Relatórios
        var relatorioResp = await _client.GetAsync("/api/relatorios");
        Assert.Equal(HttpStatusCode.OK, relatorioResp.StatusCode);
        var relatorio = await relatorioResp.Content.ReadFromJsonAsync<RelatorioGerencialResponse>(JsonOptions);
        Assert.NotNull(relatorio);
        Assert.True(relatorio.TotalPedidosEntregues >= 1);
        Assert.True(relatorio.FaturamentoTotal >= 64.00m);
    }

    [Fact]
    public async Task Cardapio_Validacoes_PrecoInvalidoENomeVazio_RetornaBadRequest()
    {
        var itemPrecoNegativo = new CriarItemRequest(
            Tipo: CategoriaItem.Prato,
            Nome: "Prato Teste Inválido",
            PrecoBase: -10.00m,
            TempoPreparo: 15,
            VolumeMl: null,
            Gelada: null);

        var respPreco = await _client.PostAsJsonAsync("/api/cardapio", itemPrecoNegativo, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, respPreco.StatusCode);

        var itemNomeVazio = new CriarItemRequest(
            Tipo: CategoriaItem.Bebida,
            Nome: "   ",
            PrecoBase: 8.00m,
            TempoPreparo: null,
            VolumeMl: 300,
            Gelada: null);

        var respNome = await _client.PostAsJsonAsync("/api/cardapio", itemNomeVazio, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, respNome.StatusCode);
    }

    [Fact]
    public async Task Clientes_Validacoes_TelefoneDuplicado_RetornaBadRequest()
    {
        var tel = "118888" + Random.Shared.Next(1000, 9999);
        var cliente1 = new CriarClienteRequest("Cliente Teste 1", tel, "Rua Um, 100");
        var resp1 = await _client.PostAsJsonAsync("/api/clientes", cliente1, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);

        // Tentativa de cadastrar o mesmo telefone
        var clienteDuplicado = new CriarClienteRequest("Outro Cliente", tel, "Rua Dois, 200");
        var resp2 = await _client.PostAsJsonAsync("/api/clientes", clienteDuplicado, JsonOptions);
        Assert.Equal(HttpStatusCode.Conflict, resp2.StatusCode);
    }

    [Fact]
    public async Task Pedidos_Validacoes_ClienteOuEntregadorInexistente_RetornaErro()
    {
        // 1. Cadastra um item válido para compor o pedido
        var itemReq = new CriarItemRequest(
            Tipo: CategoriaItem.Bebida,
            Nome: "Água Mineral " + Guid.NewGuid().ToString()[..6],
            PrecoBase: 5.00m,
            TempoPreparo: null,
            VolumeMl: 500,
            Gelada: null);

        var itemResp = await _client.PostAsJsonAsync("/api/cardapio", itemReq, JsonOptions);
        var item = await itemResp.Content.ReadFromJsonAsync<ItemCardapioResponse>(JsonOptions);
        Assert.NotNull(item);

        // Tentativa de pedido com entregador ID 9999999 (inexistente)
        var pedidoEntregadorInvalido = new CriarPedidoRequest(
            TelefoneCliente: "11999999999",
            EntregadorId: 9999999,
            DistanciaKm: 4.0m,
            Itens: new List<ItemPedidoRequest> { new(item.Codigo, 1) });

        var respEntregador = await _client.PostAsJsonAsync("/api/pedidos", pedidoEntregadorInvalido, JsonOptions);
        Assert.True(respEntregador.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pedidos_CicloDeVida_NaoPermiteAvancarPedidoJaEntregue()
    {
        // Cria cliente
        var tel = "117777" + Random.Shared.Next(1000, 9999);
        await _client.PostAsJsonAsync("/api/clientes", new CriarClienteRequest("Cliente Ciclo", tel, "Rua Ciclo, 50"), JsonOptions);

        // Cria entregador
        var entResp = await _client.PostAsJsonAsync("/api/entregadores", new CriarEntregadorRequest("Entregador Ciclo", TipoVeiculo.Moto), JsonOptions);
        var entregador = await entResp.Content.ReadFromJsonAsync<EntregadorResponse>(JsonOptions);

        // Cria item
        var itemResp = await _client.PostAsJsonAsync("/api/cardapio", new CriarItemRequest(
            CategoriaItem.Sobremesa, "Pudim " + Guid.NewGuid().ToString()[..6], 12.00m, null, null, true), JsonOptions);
        var item = await itemResp.Content.ReadFromJsonAsync<ItemCardapioResponse>(JsonOptions);

        // Cria pedido
        var pedidoResp = await _client.PostAsJsonAsync("/api/pedidos", new CriarPedidoRequest(
            tel, entregador!.Id, 2.0m, new List<ItemPedidoRequest> { new(item!.Codigo, 1) }), JsonOptions);
        var pedido = await pedidoResp.Content.ReadFromJsonAsync<PedidoResponse>(JsonOptions);

        // Avança até Entregue
        await _client.PatchAsync($"/api/pedidos/{pedido!.Id}/avancar", null); // EmPreparo
        await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null); // Pronto
        await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null); // EmRota
        await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null); // Entregue

        // Tentativa de avançar além do estado final
        var avancoExtra = await _client.PatchAsync($"/api/pedidos/{pedido.Id}/avancar", null);
        Assert.Equal(HttpStatusCode.Conflict, avancoExtra.StatusCode);
    }
}
