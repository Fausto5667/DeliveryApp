namespace DeliveryApi.Dominio.Excecoes;

/// <summary>
/// A operação é válida em si, mas conflita com o estado atual do sistema (entregador ocupado,
/// telefone já cadastrado, pedido já entregue). Vira HTTP 409 na API.
/// Existe como tipo próprio para o middleware não depender do texto das mensagens.
/// </summary>
public class ConflitoException : DominioException
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
