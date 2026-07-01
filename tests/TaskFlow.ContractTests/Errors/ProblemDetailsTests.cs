using System.Net.Http.Json;

namespace TaskFlow.ContractTests.Errors;

public class ProblemDetailsTests : IClassFixture<TaskFlowFactory>
{
    private readonly HttpClient _client;

    public ProblemDetailsTests(TaskFlowFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Retorna404_ComProblemDetailsCorreto()
    {
        var response = await _client.GetAsync($"/projetos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal(404, body.Status);
        Assert.Equal("Recurso não encontrado", body.Title);
        Assert.NotNull(body.Detail);
        Assert.NotEmpty(body.Detail);
    }

    [Fact]
    public async Task Retorna422_ComProblemDetailsCorreto()
    {
        // Regra 4: criar tarefa em projeto arquivado → 422
        var project = await ApiHelpers.CreateProjectAsync(_client, "Projeto PD 422");
        await _client.PatchAsJsonAsync($"/projetos/{project.Id}", new { status = "archived" });

        var response = await _client.PostAsJsonAsync(
            $"/projetos/{project.Id}/tarefas",
            new { title = "Tarefa", priority = "low" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal(422, body.Status);
        Assert.Equal("Erro de regra de negócio", body.Title);
        Assert.NotNull(body.Detail);
        Assert.NotEmpty(body.Detail);
    }

    [Fact]
    public async Task Retorna400_ComValidationProblemDetailsECamposDeErro()
    {
        var response = await _client.PostAsJsonAsync("/projetos", new
        {
            name = new string('x', 101)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsDto>(ApiHelpers.Json);
        Assert.NotNull(body);
        Assert.Equal(400, body.Status);
        Assert.Equal("Erro de validação", body.Title);
        Assert.NotNull(body.Errors);
        Assert.True(body.Errors.Count > 0);
        Assert.True(body.Errors.ContainsKey("name"));
    }
}
