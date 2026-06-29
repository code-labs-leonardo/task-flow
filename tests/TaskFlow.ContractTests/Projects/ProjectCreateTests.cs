using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Projects;

public class ProjectCreateTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public ProjectCreateTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Projetos_Retorna201_ComDadosValidos()
    {
        var response = await _client.PostAsJsonAsync("/projetos", new
        {
            name = "Projeto Alpha",
            description = "Descrição do projeto"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProjectDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("Projeto Alpha", body.Name);
        Assert.Equal("Descrição do projeto", body.Description);
        Assert.Equal("active", body.Status);
        Assert.True(body.CreatedAt > DateTime.MinValue);

        await SchemaValidator.ValidateAsync("ProjectResponse", response);
    }

    [Fact]
    public async Task POST_Projetos_Retorna400_QuandoNameAusente()
    {
        var response = await _client.PostAsJsonAsync("/projetos", new
        {
            description = "Projeto sem nome"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Projetos_Retorna400_QuandoNameExcedeMaximo()
    {
        var response = await _client.PostAsJsonAsync("/projetos", new
        {
            name = new string('x', 101)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
