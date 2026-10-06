using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Entidades;

public class Sobremesa : ItemCardapio
{
    public bool Gelada { get; private set; }

    public Sobremesa(int codigo, string nome, decimal precoBase, bool gelada)
        : base(codigo, nome, precoBase)
    {
        Gelada = gelada;
    }

    public override CategoriaItem Categoria => CategoriaItem.Sobremesa;

    public override string ObterDetalhes() => Gelada ? "Gelada" : "Natural";
}
