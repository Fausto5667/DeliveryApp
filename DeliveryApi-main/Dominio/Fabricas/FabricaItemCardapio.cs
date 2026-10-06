using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Fabricas;

/// <summary>
/// Único ponto do sistema que precisa conhecer as subclasses concretas de ItemCardapio.
/// Depois da criação, todo o resto trabalha com a abstração.
/// </summary>
public static class FabricaItemCardapio
{
    public static ItemCardapio Criar(
        CategoriaItem categoria,
        int codigo,
        string nome,
        decimal precoBase,
        int? tempoPreparo,
        int? volumeMl,
        bool? gelada) => categoria switch
    {
        CategoriaItem.Prato => new Prato(codigo, nome, precoBase,
            tempoPreparo ?? throw new DominioException("O tempo de preparo é obrigatório para pratos.")),

        CategoriaItem.Bebida => new Bebida(codigo, nome, precoBase,
            volumeMl ?? throw new DominioException("O volume em ml é obrigatório para bebidas.")),

        CategoriaItem.Sobremesa => new Sobremesa(codigo, nome, precoBase,
            gelada ?? throw new DominioException("Informe se a sobremesa é gelada.")),

        _ => throw new DominioException($"Categoria de item inválida: {categoria}.")
    };
}
