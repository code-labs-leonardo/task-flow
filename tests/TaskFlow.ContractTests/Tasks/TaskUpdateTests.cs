using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Tasks;

public class TaskUpdateTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public TaskUpdateTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna404_QuandoTarefaNaoExiste()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{Guid.NewGuid()}",
            new { title = "Novo título" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna400_QuandoCompletedAtEnviado()
    {
        // Regra 3: completedAt não pode ser informado manualmente
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto completedAt");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa completedAt");

        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{task.Id}",
            new { completedAt = "2024-01-01T00:00:00Z" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Tarefas_PreencheCompletedAt_AoTransicionarParaDone()
    {
        // Regra 3: completedAt preenchido automaticamente pela API ao concluir
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto auto done");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa auto done");
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });

        var done = await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "done" });

        Assert.Equal("done", done.Status);
        Assert.NotNull(done.CompletedAt);
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna422_AoPularEtapaInProgress()
    {
        // Regra 5: pending → done não é permitido (deve passar por in_progress)
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto pular etapa");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa pular etapa");

        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{task.Id}",
            new { status = "done" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna422_AoRetrocederDoneParaPending()
    {
        // Regra 5: não é permitido retroceder o status
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto retroceder done");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa retroceder done");
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "done" });

        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{task.Id}",
            new { status = "pending" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna200_EValidaSchema()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto update schema");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Título original", "low");

        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{task.Id}",
            new { title = "Título atualizado", priority = "medium" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TaskDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal("Título atualizado", body.Title);
        Assert.Equal("medium", body.Priority);

        await SchemaValidator.ValidateAsync("TaskResponse", response);
    }

    [Fact]
    public async Task PATCH_Tarefas_Retorna422_AoRetrocederInProgressParaPending()
    {
        // Regra 5: retrocesso de in_progress para pending também não é permitido
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto retroceder in_progress");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa retroceder in_progress");
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });

        var response = await _client.PatchAsJsonAsync(
            $"/tarefas/{task.Id}",
            new { status = "pending" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
