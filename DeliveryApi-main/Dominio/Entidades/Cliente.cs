using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// Cliente do delivery. O telefone é o identificador único e fica guardado só com dígitos,
/// para que "(11) 99999-0000" e "11999990000" sejam reconhecidos como o mesmo cliente.
/// </summary>
public class Cliente
{
    public string Telefone { get; }
    public string Nome { get; private set; }
    public string Endereco { get; private set; }

    public Cliente(string nome, string telefone, string endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(endereco))
            throw new DominioException("O endereço do cliente é obrigatório.");

        Telefone = NormalizarTelefone(telefone);
        Nome = nome.Trim();
        Endereco = endereco.Trim();
    }

    /// <summary>
    /// Aceita apenas dígitos e os separadores usuais ( ) - + . e espaço, e devolve somente os dígitos.
    /// </summary>
    public static string NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DominioException("O telefone do cliente é obrigatório.");

        var digitos = new List<char>();
        foreach (var caractere in telefone.Trim())
        {
            if (caractere is >= '0' and <= '9')
                digitos.Add(caractere);
            else if (!(char.IsWhiteSpace(caractere) || caractere is '(' or ')' or '-' or '+' or '.'))
                throw new DominioException("O telefone deve conter apenas números, espaços, parênteses, hífen ou +.");
        }

        if (digitos.Count < RegrasDeNegocio.TelefoneMinimoDigitos || digitos.Count > RegrasDeNegocio.TelefoneMaximoDigitos)
            throw new DominioException(
                $"O telefone deve ter entre {RegrasDeNegocio.TelefoneMinimoDigitos} e {RegrasDeNegocio.TelefoneMaximoDigitos} dígitos.");

        return new string(digitos.ToArray());
    }
}
