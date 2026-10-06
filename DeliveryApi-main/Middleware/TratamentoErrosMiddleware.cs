using System.Net.Mime;
using System.Text.Json;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Middleware;

/// <summary>
/// Converte qualquer exceção em uma resposta HTTP controlada no formato { "erro": "mensagem" }:
/// NaoEncontradoException 404, ConflitoException 409, DominioException 400 e o restante 500
/// (sem expor detalhes internos). Nenhuma falha derruba a aplicação.
/// </summary>
public class TratamentoErrosMiddleware
{
    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoErrosMiddleware> _logger;

    public TratamentoErrosMiddleware(RequestDelegate next, ILogger<TratamentoErrosMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Falha após o início da resposta; não é possível enviar JSON de erro.");
                throw;
            }

            var (status, mensagem) = Classificar(ex);
            await EscreverErroAsync(context, status, mensagem);
        }
    }

    // A ordem importa: NaoEncontrado e Conflito herdam de DominioException e precisam ser testados antes.
    private (int Status, string Mensagem) Classificar(Exception ex)
    {
        switch (ex)
        {
            case NaoEncontradoException:
                _logger.LogInformation("Recurso não encontrado: {Mensagem}", ex.Message);
                return (StatusCodes.Status404NotFound, ex.Message);

            case ConflitoException:
                _logger.LogInformation("Conflito de regra de negócio: {Mensagem}", ex.Message);
                return (StatusCodes.Status409Conflict, ex.Message);

            case DominioException:
                _logger.LogInformation("Regra de negócio violada: {Mensagem}", ex.Message);
                return (StatusCodes.Status400BadRequest, ex.Message);

            case BadHttpRequestException requisicaoInvalida:
                _logger.LogWarning("Requisição inválida: {Mensagem}", requisicaoInvalida.Message);
                return (requisicaoInvalida.StatusCode, "Requisição inválida.");

            default:
                _logger.LogError(ex, "Erro interno não tratado.");
                return (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno no servidor. Tente novamente mais tarde.");
        }
    }

    public static async Task EscreverErroAsync(HttpContext context, int status, string mensagem)
    {
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { erro = mensagem }, OpcoesJson));
    }
}
