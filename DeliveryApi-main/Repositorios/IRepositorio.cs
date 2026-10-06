namespace DeliveryApi.Repositorios;

public interface IRepositorio<TEntidade, TId>
    where TEntidade : class
    where TId : notnull
{
    void Adicionar(TEntidade entidade);

    TEntidade? ObterPorId(TId id);

    IReadOnlyList<TEntidade> ListarTodos();
}
