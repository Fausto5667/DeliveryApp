namespace DeliveryApi.Dominio.Excecoes;

/// <summary>Violação de invariante ou regra de negócio. Vira HTTP 400 na API.</summary>
public class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem)
    {
    }
}
