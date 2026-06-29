using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Tasks;

public class TaskDeleteTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public TaskDeleteTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DELETE_Tarefas_Retorna204_QuandoTarefaEhPending()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto delete pending");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa delete pending");

        var response = await _client.DeleteAsync($"/tarefas/{task.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_Tarefas_Retorna404_QuandoTarefaNaoExiste()
    {
        var response = await _client.DeleteAsync($"/tarefas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_Tarefas_Retorna422_QuandoTarefaEstaEmAndamento()
    {
        // Regra 2: tarefa in_progress não pode ser excluída
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto delete in_progress");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa delete in_progress");
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });

        var response = await _client.DeleteAsync($"/tarefas/{task.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_Tarefas_Retorna422_QuandoTarefaEstaConcluida()
    {
        // Regra 2: tarefa done não pode ser excluída
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto delete done");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id, "Tarefa delete done");
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "done" });

        var response = await _client.DeleteAsync($"/tarefas/{task.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
