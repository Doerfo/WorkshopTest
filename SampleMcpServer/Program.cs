using SampleMcpServer.Prompts;
using SampleMcpServer.Services;
using SampleMcpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to stderr to avoid interfering with stdio transport
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register foundational services
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IAwesomeCopilotCacheService, AwesomeCopilotCacheService>();
builder.Services.AddSingleton<ITechnologyDetectionService, TechnologyDetectionService>();
builder.Services.AddSingleton<IInstructionMergeService, InstructionMergeService>();
builder.Services.AddSingleton<IGuidelineService, GuidelineService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TechnologyDiscoveryTools>()
    .WithTools<BaselineInstructionTools>()
    .WithTools<InstructionFileTools>()
    .WithTools<GuidelineTools>()
    .WithPrompts<SetupCopilotInstructionsPrompt>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();