using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Projects;

public class ProjectUpdateTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public ProjectUpdateTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PATCH_Projetos_Retorna404_QuandoProjetoNaoExiste()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/projetos/{Guid.NewGuid()}",
            new { name = "Novo Nome" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Projetos_Retorna422_AoArquivarComTarefaEmAndamento()
    {
        // Regra 1: não pode arquivar projeto com tarefa in_progress
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Para Arquivar 1");
        var task = await ApiHelpers.CreateTaskAsync(_client, project.Id);
        await ApiHelpers.UpdateTaskAsync(_client, task.Id, new { status = "in_progress" });

        var response = await _client.PatchAsJsonAsync(
            $"/projetos/{project.Id}",
            new { status = "archived" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_Projetos_Retorna200_AoArquivarSemTarefasEmAndamento()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Para Arquivar 2");

        var response = await _client.PatchAsJsonAsync(
            $"/projetos/{project.Id}",
            new { status = "archived" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal("archived", body.Status);
    }

    [Fact]
    public async Task PATCH_Projetos_Retorna200_AtualizandoNomeEDescricao_EValidaSchema()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Nome Original");

        var response = await _client.PatchAsJsonAsync(
            $"/projetos/{project.Id}",
            new { name = "Nome Atualizado", description = "Nova descrição" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal("Nome Atualizado", body.Name);
        Assert.Equal("Nova descrição", body.Description);
        Assert.Equal("active", body.Status);

        await SchemaValidator.ValidateAsync("ProjectResponse", response);
    }
}
