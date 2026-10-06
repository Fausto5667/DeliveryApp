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
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
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

builder.Services.AddSingleton<IRepositorio<ItemCardapio, int>, RepositorioItemCardapioMemoria>();
builder.Services.AddSingleton<IRepositorio<Cliente, string>, RepositorioClienteMemoria>();
builder.Services.AddSingleton<IRepositorio<Entregador, int>, RepositorioEntregadorMemoria>();
builder.Services.AddSingleton<IRepositorio<Pedido, int>, RepositorioPedidoMemoria>();

builder.Services.AddSingleton<ICalculadoraFrete>(
    new CalculadoraFreteGratisAcimaDaMeta(new CalculadoraFretePorVeiculo()));

builder.Services.AddSingleton<CardapioServico>();
builder.Services.AddSingleton<ClienteServico>();
builder.Services.AddSingleton<EntregadorServico>();
builder.Services.AddSingleton<PedidoServico>();
builder.Services.AddSingleton<RelatorioServico>();

var app = builder.Build();

app.UseMiddleware<TratamentoErrosMiddleware>();

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
