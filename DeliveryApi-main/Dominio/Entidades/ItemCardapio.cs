using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - ABSTRAÇÃO E CLASSE ABSTRATA:
/// Modela a essência comum a todo item de cardápio (código, nome e preço base).
/// É marcada como 'abstract' porque não existe no mundo real um "item de cardápio genérico";
/// o cliente sempre pede um Prato, uma Bebida ou uma Sobremesa.
/// </summary>
public abstract class ItemCardapio
{
    /// <summary>
    /// CONCEITO POO - ENCAPSULAMENTO:
    /// As propriedades possuem 'private set', garantindo que o estado interno do objeto
    /// só seja definido e alterado através de métodos controlados e do construtor validado.
    /// </summary>
    public int Codigo { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoBase { get; private set; }

    /// <summary>
    /// Construtor protegido: valida as invariantes obrigatórias de qualquer item antes de criá-lo.
    /// </summary>
    protected ItemCardapio(int codigo, string nome, decimal precoBase)
    {
        if (codigo <= 0)
            throw new DominioException("O código do item deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do item é obrigatório.");

        if (precoBase <= 0)
            throw new DominioException("O preço base deve ser maior que zero.");

        Codigo = codigo;
        Nome = nome.Trim();
        PrecoBase = precoBase;
    }

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Cada subclasse deve declarar a sua própria categoria de forma polimórfica,
    /// eliminando checagens manuais de tipo na aplicação.
    /// </summary>
    public abstract string Categoria { get; }

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Método abstrato que força cada especialização a formatar seus próprios detalhes específicos.
    /// </summary>
    public abstract string ObterDetalhes();

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Propriedade virtual: por padrão, itens de cardápio não possuem tempo de preparo de cozinha (0 min).
    /// Apenas pratos culinários sobrescrevem esta propriedade.
    /// </summary>
    public virtual int TempoPreparoMinutos => 0;
}
