namespace DeliveryApi.Repositorios;

/// <summary>
/// Contrato de armazenamento das entidades. Os serviços dependem apenas desta interface, então trocar
/// o armazenamento em memória por um banco de dados não exige alterar nenhuma regra de negócio.
/// O telefone do cliente é o seu identificador, por isso o tipo da chave é genérico.
/// </summary>
public interface IRepositorio<TEntidade, TId>
    where TEntidade : class
    where TId : notnull
{
    /// <summary>Lança ConflitoException se já existir uma entidade com o mesmo identificador.</summary>
    void Adicionar(TEntidade entidade);

    TEntidade? ObterPorId(TId id);

    IReadOnlyList<TEntidade> ListarTodos();
}
