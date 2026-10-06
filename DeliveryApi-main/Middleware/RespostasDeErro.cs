using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DeliveryApi.Middleware;

/// <summary>Monta mensagens de erro em português a partir de falhas de validação e de leitura do JSON.</summary>
public static class RespostasDeErro
{
    public static string MontarMensagem(ModelStateDictionary estado)
    {
        var mensagens = new List<string>();

        foreach (var (campo, entrada) in estado)
        {
            foreach (var erro in entrada.Errors)
            {
                mensagens.Add(EhMensagemTecnica(erro) ? TraduzirErroTecnico(campo) : erro.ErrorMessage);
            }
        }

        return mensagens.Count > 0
            ? string.Join(" ", mensagens.Distinct())
            : "Requisição inválida.";
    }

    public static string MensagemParaStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida.",
        StatusCodes.Status404NotFound => "Recurso não encontrado.",
        StatusCodes.Status405MethodNotAllowed => "Método HTTP não permitido para este recurso.",
        StatusCodes.Status415UnsupportedMediaType => "Tipo de conteúdo não suportado. Envie o corpo como application/json.",
        _ => "Não foi possível processar a requisição."
    };

    // Mensagens geradas pelo próprio framework (em inglês) para JSON malformado ou valores de tipo errado.
    private static bool EhMensagemTecnica(ModelError erro) =>
        erro.Exception is not null
        || string.IsNullOrWhiteSpace(erro.ErrorMessage)
        || erro.ErrorMessage.StartsWith("The JSON", StringComparison.Ordinal)
        || erro.ErrorMessage.StartsWith("The value", StringComparison.Ordinal)
        || erro.ErrorMessage.StartsWith("A non-empty request body", StringComparison.Ordinal)
        || erro.ErrorMessage.StartsWith("The input does not contain", StringComparison.Ordinal)
        || erro.ErrorMessage.Contains("is invalid", StringComparison.Ordinal)
        || erro.ErrorMessage.Contains("is not valid", StringComparison.Ordinal);

    private static string TraduzirErroTecnico(string campo)
    {
        var nome = campo.TrimStart('$', '.');

        return string.IsNullOrWhiteSpace(nome) || nome == "request"
            ? "Corpo da requisição ausente ou com JSON inválido."
            : $"Valor inválido para o campo '{nome}'.";
    }
}
