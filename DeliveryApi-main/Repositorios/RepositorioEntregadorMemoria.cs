using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

public class RepositorioEntregadorMemoria : RepositorioMemoria<Entregador, int>
{
    protected override int ObterId(Entregador entregador) => entregador.Id;

    protected override IEnumerable<Entregador> Ordenar(IEnumerable<Entregador> entregadores) =>
        entregadores.OrderBy(e => e.Id);
}
