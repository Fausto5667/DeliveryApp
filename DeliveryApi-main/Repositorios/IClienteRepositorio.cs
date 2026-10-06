using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - INTERFACE:
/// Contrato específico para a entidade Cliente, cuja chave natural única é o Telefone (string).
/// </summary>
public interface IClienteRepositorio
{
    void Adicionar(Cliente cliente);
    Cliente? ObterPorTelefone(string telefone);
    IReadOnlyList<Cliente> ListarTodos();
}
