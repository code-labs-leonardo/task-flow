using System.Net.Http.Json;

namespace TaskFlow.ContractTests;

internal static class ApiHelpers
{
    internal static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    internal static async Task<ProjectDto> CreateProjectAsync(
        HttpClient client, string name = "Projeto Teste", string? description = null)
    {
        var response = await client.PostAsJsonAsync("/projetos", new { name, description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectDto>(Json))!;
    }

    internal static async Task<TaskDto> CreateTaskAsync(
        HttpClient client, Guid projectId,
        string title = "Tarefa Teste", string priority = "low", string? description = null)
    {
        var response = await client.PostAsJsonAsync(
            $"/projetos/{projectId}/tarefas",
            new { title, description, priority });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>(Json))!;
    }

    internal static async Task<TaskDto> UpdateTaskAsync(
        HttpClient client, Guid taskId, object body)
    {
        var response = await client.PatchAsJsonAsync($"/tarefas/{taskId}", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskDto>(Json))!;
    }
}
