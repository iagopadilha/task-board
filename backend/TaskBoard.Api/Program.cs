using TaskBoard.Api.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);
builder.AddPreApiConfig();

var app = builder.Build();
app.AddPosApiConfig();

app.Run();

// Exposes the entry point to WebApplicationFactory<Program> in integration tests. Usually this is hidden/interla
public partial class Program;