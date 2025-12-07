using SampleMcpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();