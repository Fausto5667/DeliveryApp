namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - INTERFACE:
/// Define o contrato abstrato de persistência genérico para entidades com identificador numérico.
/// Desacopla a camada de serviço da implementação concreta de armazenamento (seja em memória, banco relacional, etc.).
/// </summary>
/// <typeparam name="T">Tipo da entidade gerenciada.</typeparam>
public interface IRepositorio<T>
{
    void Adicionar(T entidade);
    T? ObterPorId(int id);
    IReadOnlyList<T> ListarTodos();
}
