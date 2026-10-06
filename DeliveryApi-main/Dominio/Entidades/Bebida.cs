using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>Bebida: item com volume declarado em mililitros. Não passa pela cozinha.</summary>
public class Bebida : ItemCardapio
{
    public int VolumeMl { get; private set; }

    public Bebida(int codigo, string nome, decimal precoBase, int volumeMl)
        : base(codigo, nome, precoBase)
    {
        if (volumeMl <= 0)
            throw new DominioException("O volume da bebida deve ser maior que zero ml.");

        if (volumeMl > RegrasDeNegocio.VolumeMaximoMl)
            throw new DominioException($"O volume da bebida não pode ultrapassar {RegrasDeNegocio.VolumeMaximoMl} ml.");

        VolumeMl = volumeMl;
    }

    public override CategoriaItem Categoria => CategoriaItem.Bebida;

    public override string ObterDetalhes() => $"{VolumeMl} ml";
}
