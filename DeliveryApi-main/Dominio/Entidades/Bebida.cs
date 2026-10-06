using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - HERANÇA:
/// Bebida "é um" ItemCardapio. Especializa a classe base agregando
/// a declaração do volume em mililitros.
/// </summary>
public class Bebida : ItemCardapio
{
    /// <summary>
    /// CONCEITO POO - ENCAPSULAMENTO:
    /// Volume líquido declarado em mililitros (ml).
    /// </summary>
    public int VolumeMl { get; private set; }

    public Bebida(int codigo, string nome, decimal precoBase, int volumeMl)
        : base(codigo, nome, precoBase)
    {
        if (volumeMl <= 0)
            throw new DominioException("O volume da bebida deve ser maior que zero ml.");

        VolumeMl = volumeMl;
    }

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Sobrescreve a categoria abstrata para identificar a especialização.
    /// </summary>
    public override string Categoria => "Bebida";

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Retorna os detalhes específicos da bebida formatados.
    /// </summary>
    public override string ObterDetalhes() => $"{VolumeMl} ml";
}
