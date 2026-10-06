namespace DeliveryApi.Dominio.Excecoes;

public class NaoEncontradoException : DominioException
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
