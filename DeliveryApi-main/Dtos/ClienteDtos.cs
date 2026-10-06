using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Dtos;

public record CriarClienteRequest(
    [Required(ErrorMessage = "O nome completo do cliente é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve conter entre 2 e 150 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
    [StringLength(30, ErrorMessage = "O telefone deve ter no máximo 30 caracteres.")]
    string Telefone,

    [Required(ErrorMessage = "O endereço de entrega é obrigatório.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "O endereço deve conter entre 5 e 200 caracteres.")]
    string Endereco
);

public record ClienteResponse(
    string Nome,
    string Telefone,
    string Endereco
)
{
    public static ClienteResponse DeDominio(Cliente cliente) => new(
        cliente.Nome,
        cliente.Telefone,
        cliente.Endereco
    );
}
