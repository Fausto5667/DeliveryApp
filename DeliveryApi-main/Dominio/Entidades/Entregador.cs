using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

public class Entregador
{
    private readonly object _trava = new();

    public int Id { get; private set; }
    public string Nome { get; private set; }
    public TipoVeiculo Veiculo { get; private set; }
    public bool Disponivel { get; private set; }

    public Entregador(int id, string nome, TipoVeiculo veiculo)
    {
        if (id <= 0)
            throw new DominioException("O identificador do entregador deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do entregador é obrigatório.");

        if (!Enum.IsDefined(veiculo))
            throw new DominioException("Modalidade de veículo inválida. Use Moto, Bicicleta ou Carro.");

        Id = id;
        Nome = nome.Trim();
        Veiculo = veiculo;
        Disponivel = true;
    }

    public void Ocupar()
    {
        lock (_trava)
        {
            if (!Disponivel)
                throw new ConflitoException("Entregador indisponível.");

            Disponivel = false;
        }
    }

    public void Liberar()
    {
        lock (_trava)
        {
            Disponivel = true;
        }
    }
}
