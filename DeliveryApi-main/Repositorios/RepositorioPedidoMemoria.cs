using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

public class RepositorioPedidoMemoria : RepositorioMemoria<Pedido, int>
{
    protected override int ObterId(Pedido pedido) => pedido.Id;

    protected override IEnumerable<Pedido> Ordenar(IEnumerable<Pedido> pedidos) =>
        pedidos.OrderBy(p => p.Id);
}
