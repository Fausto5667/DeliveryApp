using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

public class EntregadorServico
{
    private readonly IRepositorio<Entregador, int> _repositorio;
    private readonly object _trava = new();
    private int _proximoId = 1;

    public EntregadorServico(IRepositorio<Entregador, int> repositorio)
    {
        _repositorio = repositorio;
    }

    public Entregador Cadastrar(string nome, TipoVeiculo veiculo)
    {
        lock (_trava)
        {
            var entregador = new Entregador(_proximoId, nome, veiculo);
            _repositorio.Adicionar(entregador);
            _proximoId++;
            return entregador;
        }
    }

    public IReadOnlyList<Entregador> Listar() => _repositorio.ListarTodos();

    public Entregador ObterPorId(int id) =>
        _repositorio.ObterPorId(id)
            ?? throw new NaoEncontradoException($"Entregador com identificador {id} não encontrado.");
}
