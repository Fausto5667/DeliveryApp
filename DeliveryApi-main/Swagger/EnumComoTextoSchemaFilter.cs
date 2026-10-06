using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DeliveryApi.Swagger;

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
