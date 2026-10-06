namespace DeliveryApi.Dominio.Excecoes;

/// <summary>Entidade solicitada não existe. Vira HTTP 404 na API.</summary>
public class NaoEncontradoException : DominioException
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
