using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

public abstract class ItemCardapio
{
    public int Codigo { get; private set; }
    public string Nome { get; private set; }
    public decimal PrecoBase { get; private set; }

    protected ItemCardapio(int codigo, string nome, decimal precoBase)
    {
        if (codigo <= 0)
            throw new DominioException("O código do item deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do item é obrigatório.");

        var preco = Math.Round(precoBase, 2, MidpointRounding.AwayFromZero);
        if (preco <= 0)
            throw new DominioException("O preço base deve ser maior que zero.");

        if (preco > RegrasDeNegocio.PrecoMaximoItem)
            throw new DominioException($"O preço base não pode ultrapassar {RegrasDeNegocio.PrecoMaximoItem:N2}.");

        Codigo = codigo;
        Nome = nome.Trim();
        PrecoBase = preco;
    }

    public abstract CategoriaItem Categoria { get; }

    public abstract string ObterDetalhes();

    public virtual int TempoPreparoMinutos => 0;
}
