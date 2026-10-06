using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>
/// Camada de Serviço para gestão do acervo do cardápio.
/// Orquestra a criação polimórfica dos itens e consulta por critérios parciais.
/// </summary>
public class CardapioServico
{
    private readonly IRepositorio<ItemCardapio> _repositorio;
    private int _contadorCodigo = 0;

    public CardapioServico(IRepositorio<ItemCardapio> repositorio)
    {
        _repositorio = repositorio;
    }

    private int ProximoCodigo() => Interlocked.Increment(ref _contadorCodigo);

    public ItemCardapio AdicionarPrato(string nome, decimal precoBase, int tempoPreparo)
    {
        var prato = new Prato(ProximoCodigo(), nome, precoBase, tempoPreparo);
        _repositorio.Adicionar(prato);
        return prato;
    }

    public ItemCardapio AdicionarBebida(string nome, decimal precoBase, int volumeMl)
    {
        var bebida = new Bebida(ProximoCodigo(), nome, precoBase, volumeMl);
        _repositorio.Adicionar(bebida);
        return bebida;
    }

    public ItemCardapio AdicionarSobremesa(string nome, decimal precoBase, bool gelada)
    {
        var sobremesa = new Sobremesa(ProximoCodigo(), nome, precoBase, gelada);
        _repositorio.Adicionar(sobremesa);
        return sobremesa;
    }

    public IReadOnlyList<ItemCardapio> Listar()
    {
        return _repositorio.ListarTodos();
    }

    public IReadOnlyList<ItemCardapio> BuscarPorNome(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            throw new DominioException("Informe um termo de busca.");

        var todos = _repositorio.ListarTodos();
        return todos
            .Where(item => item.Nome.Contains(termo.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public ItemCardapio ObterPorCodigo(int codigo)
    {
        var item = _repositorio.ObterPorId(codigo);
        if (item is null)
            throw new NaoEncontradoException($"Item com código {codigo} não encontrado no cardápio.");

        return item;
    }
}
