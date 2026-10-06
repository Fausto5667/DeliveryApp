namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - HERANÇA:
/// Sobremesa "é um" ItemCardapio. Especializa o cardápio com
/// a indicação booleana de serviço refrigerado/gelado ou natural.
/// </summary>
public class Sobremesa : ItemCardapio
{
    /// <summary>
    /// CONCEITO POO - ENCAPSULAMENTO:
    /// Flag que indica se o doce deve ser mantido e servido gelado.
    /// </summary>
    public bool Gelada { get; private set; }

    public Sobremesa(int codigo, string nome, decimal precoBase, bool gelada)
        : base(codigo, nome, precoBase)
    {
        Gelada = gelada;
    }

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Sobrescreve a categoria abstrata da classe base.
    /// </summary>
    public override string Categoria => "Sobremesa";

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Fornece o detalhamento próprio de uma sobremesa sem condicionais na camada consumidora.
    /// </summary>
    public override string ObterDetalhes() => Gelada ? "Gelada" : "Natural";
}
