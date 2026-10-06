using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Servicos;

namespace DeliveryApi.Dtos;

/// <summary>
/// Os três campos opcionais dependem do Tipo; a regra de qual é obrigatório para cada tipo fica no domínio.
/// </summary>
public record CriarItemRequest(
    [Required(ErrorMessage = "O tipo do item é obrigatório (Prato, Bebida ou Sobremesa).")]
    CategoriaItem? Tipo,

    [Required(ErrorMessage = "O nome do item é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve conter entre 2 e 100 caracteres.")]
    string Nome,

    [Range(0.01, 10000.0, ErrorMessage = "O preço base deve estar entre 0,01 e 10.000,00.")]
    decimal PrecoBase,

    int? TempoPreparo,
    int? VolumeMl,
    bool? Gelada
)
{
    public NovoItemCardapio ParaComando() =>
        new(Tipo!.Value, Nome, PrecoBase, TempoPreparo, VolumeMl, Gelada);
}

public record ItemCardapioResponse(
    int Codigo,
    string Nome,
    decimal PrecoBase,
    CategoriaItem Categoria,
    string Detalhes,
    int TempoPreparoMinutos
)
{
    /// <summary>Qualquer subtipo de ItemCardapio é convertido da mesma forma, sem if/switch de tipo.</summary>
    public static ItemCardapioResponse DeDominio(ItemCardapio item) => new(
        item.Codigo,
        item.Nome,
        item.PrecoBase,
        item.Categoria,
        item.ObterDetalhes(),
        item.TempoPreparoMinutos
    );
}
