using SampleMcpServer.Tools;
using SampleMcpServer.Services;
using SampleMcpServer.Prompts;

var builder = WebApplication.CreateBuilder(args);

// Register NotesService as singleton
builder.Services.AddSingleton<NotesService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<NotesTools>()
    .WithPrompts<NotesPrompts>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();