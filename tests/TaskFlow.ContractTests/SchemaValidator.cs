using NJsonSchema;

namespace TaskFlow.ContractTests;

// Os schemas são derivados dos componentes do openapi.yaml.
// A abordagem inline é necessária porque Microsoft.OpenApi 2.0 não inclui um reader YAML
// nativo — apenas OpenApiJsonReader está disponível. Usar inline schemas evita adicionar
// um parser YAML como dependência extra ao projeto de testes.
internal static class SchemaValidator
{
    private const string ProjectResponseSchema = """
        {
            "type": "object",
            "required": ["id", "name", "status", "createdAt"],
            "properties": {
                "id":          { "type": "string", "format": "uuid" },
                "name":        { "type": "string", "maxLength": 100 },
                "description": { "type": ["string", "null"] },
                "status":      { "type": "string", "enum": ["active", "archived"] },
                "createdAt":   { "type": "string", "format": "date-time" }
            }
        }
        """;

    private const string TaskResponseSchema = """
        {
            "type": "object",
            "required": ["id", "title", "status", "priority", "createdAt", "projectId"],
            "properties": {
                "id":          { "type": "string", "format": "uuid" },
                "title":       { "type": "string", "maxLength": 200 },
                "description": { "type": ["string", "null"] },
                "status":      { "type": "string", "enum": ["pending", "in_progress", "done"] },
                "priority":    { "type": "string", "enum": ["low", "medium", "high"] },
                "createdAt":   { "type": "string", "format": "date-time" },
                "completedAt": { "type": ["string", "null"], "format": "date-time" },
                "projectId":   { "type": "string", "format": "uuid" }
            }
        }
        """;

    private static readonly Dictionary<string, string> _schemaJsons = new()
    {
        ["ProjectResponse"] = ProjectResponseSchema,
        ["TaskResponse"]    = TaskResponseSchema
    };

    private static readonly Lazy<Dictionary<string, JsonSchema>> _schemas =
        new(
            () => _schemaJsons.ToDictionary(
                kvp => kvp.Key,
                kvp => JsonSchema.FromJsonAsync(kvp.Value).GetAwaiter().GetResult()),
            LazyThreadSafetyMode.ExecutionAndPublication);

    internal static async Task ValidateAsync(string schemaName, HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (!_schemas.Value.TryGetValue(schemaName, out var schema))
            throw new KeyNotFoundException(
                $"Schema '{schemaName}' não disponível. " +
                $"Disponíveis: {string.Join(", ", _schemas.Value.Keys)}");

        var errors = schema.Validate(json);
        Assert.True(
            errors.Count == 0,
            $"Resposta não está em conformidade com o schema '{schemaName}':\n" +
            string.Join('\n', errors.Select(e => $"  [{e.Path}] {e.Kind}")));
    }
}
