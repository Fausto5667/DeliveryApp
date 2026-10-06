using System.Text.Json.Serialization;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Middleware;
using DeliveryApi.Repositorios;
using DeliveryApi.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:3000",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IRepositorio<ItemCardapio>, RepositorioItemCardapioMemoria>();
builder.Services.AddSingleton<IClienteRepositorio, RepositorioClienteMemoria>();
builder.Services.AddSingleton<IRepositorio<Entregador>, RepositorioEntregadorMemoria>();
builder.Services.AddSingleton<IRepositorio<Pedido>, RepositorioPedidoMemoria>();

builder.Services.AddSingleton<CardapioServico>();
builder.Services.AddSingleton<ClienteServico>();
builder.Services.AddSingleton<EntregadorServico>();
builder.Services.AddSingleton<PedidoServico>();
builder.Services.AddSingleton<RelatorioServico>();

var app = builder.Build();

app.UseMiddleware<TratamentoErrosMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();