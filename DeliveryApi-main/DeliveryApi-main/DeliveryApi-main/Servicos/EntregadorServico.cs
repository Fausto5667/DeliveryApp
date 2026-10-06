using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>
/// Camada de Serviço responsável pela gestão de entregadores da frota.
/// Controla o cadastro e identificação dos operadores logísticos.
/// </summary>
public class EntregadorServico
{
    private readonly IRepositorio<Entregador> _repositorio;
    private int _contadorId = 0;

    public EntregadorServico(IRepositorio<Entregador> repositorio)
    {
        _repositorio = repositorio;
    }

    private int ProximoId() => Interlocked.Increment(ref _contadorId);

    public Entregador Cadastrar(string nome, TipoVeiculo veiculo)
    {
        var entregador = new Entregador(ProximoId(), nome, veiculo);
        _repositorio.Adicionar(entregador);
        return entregador;
    }

    public IReadOnlyList<Entregador> Listar()
    {
        return _repositorio.ListarTodos();
    }

    public Entregador ObterPorId(int id)
    {
        var entregador = _repositorio.ObterPorId(id);
        if (entregador is null)
            throw new NaoEncontradoException($"Entregador com identificador {id} não encontrado.");

        return entregador;
    }
}
