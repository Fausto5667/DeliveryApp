using System.Net.Mime;
using System.Text.Json;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Middleware;

/// <summary>
/// Middleware global para tratamento centralizado de exceções operacionais e de domínio.
/// Garante que nenhuma falha cause interrupção abrupta da API e padroniza as respostas de erro em JSON:
/// { "erro": "mensagem clara" }
/// </summary>
public class TratamentoErrosMiddleware
{
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
        catch (NaoEncontradoException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado: {Mensagem}", ex.Message);
            await EscreverRespostaErroAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (DominioException ex)
        {
            _logger.LogWarning(ex, "Violação de regra de negócio/domínio: {Mensagem}", ex.Message);

            // Mapeia conflitos operacionais (ex: entregador ocupado/indisponível ou telefone duplicado) para 409 Conflict
            int statusCode = EhConflito(ex.Message)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            await EscreverRespostaErroAsync(context, statusCode, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogWarning(ex, "Requisição mal formatada: {Mensagem}", ex.Message);
            await EscreverRespostaErroAsync(context, StatusCodes.Status400BadRequest, "Formato de requisição inválido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado no servidor.");
            await EscreverRespostaErroAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno no servidor. Tente novamente mais tarde."
            );
        }
    }

    private static bool EhConflito(string mensagem)
    {
        var msg = mensagem.ToLowerInvariant();
        return msg.Contains("indisponível")
            || msg.Contains("indisponivel")
            || msg.Contains("já existe")
            || msg.Contains("ja existe")
            || msg.Contains("duplicado");
    }

    private static async Task EscreverRespostaErroAsync(HttpContext context, int statusCode, string mensagem)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new { erro = mensagem };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
