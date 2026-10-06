namespace DeliveryApi.Dominio.Excecoes;

public class ConflitoException : DominioException
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
