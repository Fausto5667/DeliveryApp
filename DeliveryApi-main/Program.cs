using System.Text.Json.Serialization;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Frete;
using DeliveryApi.Middleware;
using DeliveryApi.Repositorios;
using DeliveryApi.Servicos;
using DeliveryApi.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums trafegam como texto; números são recusados para não criar valores fora da enumeração.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação e de JSON usam o mesmo formato { "erro": "..." } dos demais erros da API.
        options.InvalidModelStateResponseFactory = contexto =>
            new BadRequestObjectResult(new { erro = RespostasDeErro.MontarMensagem(contexto.ModelState) });
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SchemaFilter<EnumComoTextoSchemaFilter>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .SetIsOriginAllowed(origem => Uri.TryCreate(origem, UriKind.Absolute, out var uri) && uri.IsLoopback)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Repositórios e serviços são singletons: os dados vivem em memória e precisam sobreviver entre requisições.
builder.Services.AddSingleton<IRepositorio<ItemCardapio, int>, RepositorioItemCardapioMemoria>();
builder.Services.AddSingleton<IRepositorio<Cliente, string>, RepositorioClienteMemoria>();
builder.Services.AddSingleton<IRepositorio<Entregador, int>, RepositorioEntregadorMemoria>();
builder.Services.AddSingleton<IRepositorio<Pedido, int>, RepositorioPedidoMemoria>();

// Frete = tarifa por veículo, com a regra de gratuidade acima da meta envolvendo a tarifa.
builder.Services.AddSingleton<ICalculadoraFrete>(
    new CalculadoraFreteGratisAcimaDaMeta(new CalculadoraFretePorVeiculo()));

builder.Services.AddSingleton<CardapioServico>();
builder.Services.AddSingleton<ClienteServico>();
builder.Services.AddSingleton<EntregadorServico>();
builder.Services.AddSingleton<PedidoServico>();
builder.Services.AddSingleton<RelatorioServico>();

var app = builder.Build();

app.UseMiddleware<TratamentoErrosMiddleware>();

// Respostas sem corpo (rota inexistente, método ou tipo de conteúdo não suportado) também saem em JSON.
app.UseStatusCodePages(contexto =>
    TratamentoErrosMiddleware.EscreverErroAsync(
        contexto.HttpContext,
        contexto.HttpContext.Response.StatusCode,
        RespostasDeErro.MensagemParaStatus(contexto.HttpContext.Response.StatusCode)));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

// Serve o front-end na mesma origem da API (http://localhost:5173), o que dispensa configurar CORS e servidor à parte.
var pastaFrontend = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend"));
if (Directory.Exists(pastaFrontend))
{
    var arquivosFrontend = new PhysicalFileProvider(pastaFrontend);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = arquivosFrontend });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = arquivosFrontend });
}

app.MapControllers();

app.Run();

public partial class Program;