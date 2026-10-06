namespace DeliveryApi.Dominio.Excecoes;

/// <summary>
/// Exceção de domínio lançada quando uma entidade solicitada não é localizada.
/// Herda de DominioException, permitindo tratamento polimórfico de exceções de negócio.
/// </summary>
public class NaoEncontradoException : DominioException
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
