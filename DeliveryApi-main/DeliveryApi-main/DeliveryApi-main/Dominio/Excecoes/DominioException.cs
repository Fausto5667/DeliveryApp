namespace DeliveryApi.Dominio.Excecoes;

/// <summary>
/// Exceção base para violações de invariantes e regras de negócio do domínio.
/// Permite o encapsulamento de erros de validação sem vazar exceções genéricas de infraestrutura.
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem)
    {
    }
}
