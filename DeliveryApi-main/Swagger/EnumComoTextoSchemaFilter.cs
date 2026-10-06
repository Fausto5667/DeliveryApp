using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DeliveryApi.Swagger;

/// <summary>
/// A API lê e escreve enums como texto ("Moto") e não aceita números. Sem este filtro o Swagger
/// mostraria os enums como inteiros e o "Try it out" enviaria valores que a API recusa.
/// </summary>
public class EnumComoTextoSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var tipo = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!tipo.IsEnum)
            return;

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames(tipo)
            .Select(nome => (IOpenApiAny)new OpenApiString(nome))
            .ToList();
    }
}
