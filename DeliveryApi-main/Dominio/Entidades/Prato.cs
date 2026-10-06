using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>Prato: item que passa pela cozinha e por isso tem tempo de preparo.</summary>
public class Prato : ItemCardapio
{
    public int TempoPreparo { get; private set; }

    public Prato(int codigo, string nome, decimal precoBase, int tempoPreparo)
        : base(codigo, nome, precoBase)
    {
        if (tempoPreparo <= 0)
            throw new DominioException("O tempo de preparo do prato deve ser maior que zero minutos.");

        if (tempoPreparo > RegrasDeNegocio.TempoPreparoMaximoMinutos)
            throw new DominioException($"O tempo de preparo não pode ultrapassar {RegrasDeNegocio.TempoPreparoMaximoMinutos} minutos.");

        TempoPreparo = tempoPreparo;
    }

    public override CategoriaItem Categoria => CategoriaItem.Prato;

    public override int TempoPreparoMinutos => TempoPreparo;

    public override string ObterDetalhes() => $"Preparo: {TempoPreparo} min";
}
