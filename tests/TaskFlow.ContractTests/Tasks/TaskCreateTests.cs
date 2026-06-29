using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Tasks;

public class TaskCreateTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public TaskCreateTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Tarefas_Retorna201_ComDadosValidos()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client);

        var response = await _client.PostAsJsonAsync(
            $"/projetos/{project.Id}/tarefas",
            new { title = "Implementar feature", priority = "high", description = "Descrição da tarefa" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TaskDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("Implementar feature", body.Title);
        Assert.Equal("high", body.Priority);
        Assert.Equal("pending", body.Status);
        Assert.Equal(project.Id, body.ProjectId);
        Assert.Null(body.CompletedAt);
        Assert.True(body.CreatedAt > DateTime.MinValue);

        await SchemaValidator.ValidateAsync("TaskResponse", response);
    }

    [Fact]
    public async Task POST_Tarefas_Retorna404_QuandoProjetoNaoExiste()
    {
        var response = await _client.PostAsJsonAsync(
            $"/projetos/{Guid.NewGuid()}/tarefas",
            new { title = "Tarefa Órfã", priority = "low" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_Tarefas_Retorna422_EmProjetoArquivado()
    {
        // Regra 4: não é permitido criar tarefa em projeto arquivado
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Arquivado");
        await _client.PatchAsJsonAsync($"/projetos/{project.Id}", new { status = "archived" });

        var response = await _client.PostAsJsonAsync(
            $"/projetos/{project.Id}/tarefas",
            new { title = "Tarefa Bloqueada", priority = "low" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
