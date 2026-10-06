using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - ENCAPSULAMENTO E ABSTRAÇÃO:
/// Modela a entidade Cliente. O telefone atua como identificador único natural no sistema.
/// O estado interno é protegido: o telefone é imutável após a criação e os dados são validados.
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único do cliente no sistema.
    /// É somente leitura (sem setter), garantindo imutabilidade de identidade.
    /// </summary>
    public string Telefone { get; }

    /// <summary>
    /// Nome completo do cliente, protegido por encapsulamento (private set).
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Endereço completo para entrega de pedidos.
    /// </summary>
    public string Endereco { get; private set; }

    public Cliente(string nome, string telefone, string endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new DominioException("O telefone do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(endereco))
            throw new DominioException("O endereço do cliente é obrigatório.");

        Nome = nome.Trim();
        Telefone = telefone.Trim();
        Endereco = endereco.Trim();
    }
}
