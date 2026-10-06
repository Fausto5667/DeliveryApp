using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - ENCAPSULAMENTO:
/// Representa o entregador logístico.
/// A transição de estado de disponibilidade é estritamente controlada pelos métodos
/// Ocupar() e Liberar(), impedindo que o objeto fique em estado inconsistente.
/// Nota de Design POO: O tipo de veículo é modelado como uma propriedade (associação com Enum)
/// e não como herança (ex: EntregadorCarro), pois o veículo é uma modalidade do serviço,
/// não uma especialização do ser humano 'Entregador'.
/// </summary>
public class Entregador
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public TipoVeiculo Veiculo { get; private set; }

    /// <summary>
    /// Indicador de disponibilidade com encapsulamento estrito (private set).
    /// </summary>
    public bool Disponivel { get; private set; }

    public Entregador(int id, string nome, TipoVeiculo veiculo)
    {
        if (id <= 0)
            throw new DominioException("O identificador do entregador deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do entregador é obrigatório.");

        Id = id;
        Nome = nome.Trim();
        Veiculo = veiculo;
        Disponivel = true;
    }

    /// <summary>
    /// Aloca o entregador para uma entrega.
    /// Garante o invariante de que um entregador já ocupado não pode ser alocado.
    /// </summary>
    public void Ocupar()
    {
        if (!Disponivel)
            throw new DominioException("Entregador indisponível.");

        Disponivel = false;
    }

    /// <summary>
    /// Libera o entregador para novas rotas após a finalização da entrega do pedido.
    /// </summary>
    public void Liberar()
    {
        Disponivel = true;
    }
}
