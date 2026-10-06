using System.Collections.Concurrent;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Repositorios;

public abstract class RepositorioMemoria<TEntidade, TId> : IRepositorio<TEntidade, TId>
    where TEntidade : class
    where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TEntidade> _dados = new();

    protected abstract TId ObterId(TEntidade entidade);

    protected abstract IEnumerable<TEntidade> Ordenar(IEnumerable<TEntidade> entidades);

    public void Adicionar(TEntidade entidade)
    {
        var id = ObterId(entidade);

        if (!_dados.TryAdd(id, entidade))
            throw new ConflitoException($"Já existe um registro com o identificador '{id}'.");
    }

    public TEntidade? ObterPorId(TId id) =>
        _dados.TryGetValue(id, out var entidade) ? entidade : null;

    public IReadOnlyList<TEntidade> ListarTodos() =>
        Ordenar(_dados.Values).ToList().AsReadOnly();
}
