using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Tasks;

public class TaskListTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public TaskListTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Tarefas_Retorna200_ComMetadadosDePaginacao()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Lista Tarefas");
        await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa 1", "low");
        await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa 2", "high");

        var response = await _client.GetAsync($"/projetos/{project.Id}/tarefas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedTaskDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(100, body.PageSize);
        Assert.Equal(2, body.TotalItems);
        Assert.Equal(1, body.TotalPages);
    }

    [Fact]
    public async Task GET_Tarefas_Retorna404_QuandoProjetoNaoExiste()
    {
        var response = await _client.GetAsync($"/projetos/{Guid.NewGuid()}/tarefas");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_Tarefas_Retorna200_FiltroPorStatus()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Filtro Status Tarefas");
        var task1 = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa pending", "low");
        var task2 = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa in_progress", "medium");
        await ApiHelpers.UpdateTaskAsync(_client, task2.Id, new { status = "in_progress" });

        var response = await _client.GetAsync($"/projetos/{project.Id}/tarefas?status=pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedTaskDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.All(body.Items, t => Assert.Equal("pending", t.Status));
        Assert.DoesNotContain(body.Items, t => t.Id == task2.Id);
    }

    [Fact]
    public async Task GET_Tarefas_Retorna200_FiltroPorPrioridade()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Filtro Prioridade");
        var taskLow  = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa low",  "low");
        var taskHigh = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa high", "high");

        var response = await _client.GetAsync($"/projetos/{project.Id}/tarefas?priority=high");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedTaskDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.All(body.Items, t => Assert.Equal("high", t.Priority));
        Assert.DoesNotContain(body.Items, t => t.Id == taskLow.Id);
        Assert.Contains(body.Items, t => t.Id == taskHigh.Id);
    }
}
