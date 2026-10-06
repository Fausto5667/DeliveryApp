using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - HERANÇA:
/// Prato "é um" ItemCardapio. Herda código, nome e preço base,
/// e especializa o conceito com seu tempo específico de confecção na cozinha.
/// </summary>
public class Prato : ItemCardapio
{
    /// <summary>
    /// CONCEITO POO - ENCAPSULAMENTO:
    /// Tempo de preparo em minutos na cozinha, imutável externamente após construção.
    /// </summary>
    public int TempoPreparo { get; private set; }

    public Prato(int codigo, string nome, decimal precoBase, int tempoPreparo)
        : base(codigo, nome, precoBase)
    {
        if (tempoPreparo <= 0)
            throw new DominioException("O tempo de preparo do prato deve ser maior que zero minutos.");

        TempoPreparo = tempoPreparo;
    }

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO (sobrescrita de membro abstrato):
    /// Identifica dinamicamente a categoria específica deste item.
    /// </summary>
    public override string Categoria => "Prato";

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Sobrescreve a propriedade virtual da classe base para fornecer o tempo real de preparo.
    /// </summary>
    public override int TempoPreparoMinutos => TempoPreparo;

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Fornece a representação textual dos detalhes técnicos exclusivos de um prato.
    /// </summary>
    public override string ObterDetalhes() => $"Preparo: {TempoPreparo} min";
}
