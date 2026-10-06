using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>Sobremesa: item que pode ser servido gelado ou natural. Não passa pela cozinha.</summary>
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
