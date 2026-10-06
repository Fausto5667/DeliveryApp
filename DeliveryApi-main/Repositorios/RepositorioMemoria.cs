using System.Collections.Concurrent;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Repositorios;

/// <summary>
/// Esqueleto do armazenamento em memória (Template Method): a lógica de guardar com segurança entre threads
/// e recusar identificadores duplicados fica aqui, e cada repositório concreto só diz qual é o
/// identificador da entidade e em que ordem listá-las.
/// </summary>
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

        // TryAdd é atômico: evita que dois cadastros simultâneos com o mesmo id passem pela verificação.
        if (!_dados.TryAdd(id, entidade))
            throw new ConflitoException($"Já existe um registro com o identificador '{id}'.");
    }

    public TEntidade? ObterPorId(TId id) =>
        _dados.TryGetValue(id, out var entidade) ? entidade : null;

    public IReadOnlyList<TEntidade> ListarTodos() =>
        Ordenar(_dados.Values).ToList().AsReadOnly();
}
