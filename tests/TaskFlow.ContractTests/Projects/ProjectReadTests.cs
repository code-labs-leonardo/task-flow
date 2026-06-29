using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Projects;

public class ProjectReadTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public ProjectReadTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Projetos_Retorna200_ComMetadadosDePaginacao()
    {
        await ApiHelpers.CreateProjectAsync(_client, "Projeto Lista 1");
        await ApiHelpers.CreateProjectAsync(_client, "Projeto Lista 2");

        var response = await _client.GetAsync("/projetos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.True(body.Items.Count >= 2);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(100, body.PageSize);
        Assert.True(body.TotalItems >= 2);
        Assert.True(body.TotalPages >= 1);
    }

    [Fact]
    public async Task GET_Projetos_Retorna200_FiltroPorStatusActive()
    {
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto Filtro Active");
        await _client.PatchAsJsonAsync($"/projetos/{project.Id}", new { status = "archived" });

        var response = await _client.GetAsync("/projetos?status=active");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.All(body.Items, p => Assert.Equal("active", p.Status));
    }

    [Fact]
    public async Task GET_ProjetosById_Retorna200_EValidaSchema()
    {
        var created = await ApiHelpers.CreateProjectAsync(_client, "Projeto Get By Id", "Descrição");

        var response = await _client.GetAsync($"/projetos/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("Projeto Get By Id", body.Name);
        Assert.Equal("Descrição", body.Description);
        Assert.Equal("active", body.Status);

        await SchemaValidator.ValidateAsync("ProjectResponse", response);
    }

    [Fact]
    public async Task GET_ProjetosById_Retorna404_QuandoNaoExiste()
    {
        var response = await _client.GetAsync($"/projetos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
