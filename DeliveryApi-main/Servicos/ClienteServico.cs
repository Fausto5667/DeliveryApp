using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

public class ClienteServico
{
    private readonly IRepositorio<Cliente, string> _repositorio;

    public ClienteServico(IRepositorio<Cliente, string> repositorio)
    {
        _repositorio = repositorio;
    }

    public Cliente Cadastrar(string nome, string telefone, string endereco)
    {
        var cliente = new Cliente(nome, telefone, endereco);

        if (_repositorio.ObterPorId(cliente.Telefone) is not null)
            throw new ConflitoException("Já existe um cliente cadastrado com este telefone.");

        _repositorio.Adicionar(cliente);
        return cliente;
    }

    public IReadOnlyList<Cliente> Listar() => _repositorio.ListarTodos();

    public Cliente ObterPorTelefone(string telefone)
    {
        var chave = Cliente.NormalizarTelefone(telefone);

        return _repositorio.ObterPorId(chave)
            ?? throw new NaoEncontradoException($"Cliente com telefone '{telefone}' não encontrado.");
    }
}
