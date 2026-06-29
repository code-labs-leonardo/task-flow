using TaskFlow.Api.Middleware;
using TaskFlow.Application;
using TaskFlow.Infra.Persistence;
using TaskFlow.Infra.Persistence.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskFlowWriteDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public partial class Program { }
