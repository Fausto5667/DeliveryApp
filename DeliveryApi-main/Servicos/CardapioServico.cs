using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dominio.Fabricas;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

public class CardapioServico
{
    private readonly IRepositorio<ItemCardapio, int> _repositorio;
    private readonly object _trava = new();
    private int _proximoCodigo = 1;

    public CardapioServico(IRepositorio<ItemCardapio, int> repositorio)
    {
        _repositorio = repositorio;
    }

    public ItemCardapio Adicionar(NovoItemCardapio dados)
    {
        lock (_trava)
        {
            var item = FabricaItemCardapio.Criar(
                dados.Categoria, _proximoCodigo, dados.Nome, dados.PrecoBase,
                dados.TempoPreparo, dados.VolumeMl, dados.Gelada);

            _repositorio.Adicionar(item);
            _proximoCodigo++;
            return item;
        }
    }

    public IReadOnlyList<ItemCardapio> Listar() => _repositorio.ListarTodos();

    /// <summary>Busca por trecho do nome, sem diferenciar maiúsculas de minúsculas.</summary>
    public IReadOnlyList<ItemCardapio> BuscarPorNome(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            throw new DominioException("Informe um termo de busca.");

        var trecho = termo.Trim();

        return _repositorio.ListarTodos()
            .Where(item => item.Nome.Contains(trecho, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public ItemCardapio ObterPorCodigo(int codigo) =>
        _repositorio.ObterPorId(codigo)
            ?? throw new NaoEncontradoException($"Item com código {codigo} não encontrado no cardápio.");
}
