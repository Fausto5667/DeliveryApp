using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>
/// Camada de Serviço responsável pelo gerenciamento de clientes.
/// Garante o controle de unicidade por telefone e regras cadastrais.
/// </summary>
public class ClienteServico
{
    private readonly IClienteRepositorio _repositorio;

    public ClienteServico(IClienteRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public Cliente Cadastrar(string nome, string telefone, string endereco)
    {
        var existente = _repositorio.ObterPorTelefone(telefone);
        if (existente is not null)
            throw new DominioException("Já existe um cliente cadastrado com este telefone.");

        var cliente = new Cliente(nome, telefone, endereco);
        _repositorio.Adicionar(cliente);
        return cliente;
    }

    public IReadOnlyList<Cliente> Listar()
    {
        return _repositorio.ListarTodos();
    }

    public Cliente ObterPorTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DominioException("O telefone do cliente deve ser informado.");

        var cliente = _repositorio.ObterPorTelefone(telefone);
        if (cliente is null)
            throw new NaoEncontradoException($"Cliente com telefone '{telefone}' não encontrado.");

        return cliente;
    }
}
