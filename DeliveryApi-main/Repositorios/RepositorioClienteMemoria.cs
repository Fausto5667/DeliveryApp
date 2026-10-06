using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

public class RepositorioClienteMemoria : RepositorioMemoria<Cliente, string>
{
    protected override string ObterId(Cliente cliente) => cliente.Telefone;

    protected override IEnumerable<Cliente> Ordenar(IEnumerable<Cliente> clientes) =>
        clientes.OrderBy(c => c.Nome, StringComparer.CurrentCultureIgnoreCase);
}
