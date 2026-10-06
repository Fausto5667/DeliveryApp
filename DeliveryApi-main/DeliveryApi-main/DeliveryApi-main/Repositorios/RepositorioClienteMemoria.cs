using System.Collections.Concurrent;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - POLIMORFISMO DE INTERFACE:
/// Implementação em memória do repositório de clientes indexado por telefone.
/// </summary>
public class RepositorioClienteMemoria : IClienteRepositorio
{
    private readonly ConcurrentDictionary<string, Cliente> _clientes = new();

    public void Adicionar(Cliente cliente)
    {
        _clientes[cliente.Telefone] = cliente;
    }

    public Cliente? ObterPorTelefone(string telefone)
    {
        _clientes.TryGetValue(telefone.Trim(), out var cliente);
        return cliente;
    }

    public IReadOnlyList<Cliente> ListarTodos()
    {
        return _clientes.Values.OrderBy(c => c.Nome).ToList().AsReadOnly();
    }
}
