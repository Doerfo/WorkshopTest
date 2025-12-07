---
description: Instructions for building Model Context Protocol (MCP) servers using the C# SDK
applyTo: '**/*.cs, **/*.csproj'
---

# SampleMcpServer Development

This is a Model Context Protocol (MCP) server built with .NET 10 and C# 13.

## Tech Stack

- **.NET 10 SDK**: Latest .NET framework with modern C# features
- **C# 13**: Latest language features including implicit usings and nullable reference types
- **ModelContextProtocol SDK**: Version 0.5.0-preview.1
- **ModelContextProtocol.AspNetCore**: Version 0.5.0-preview.1 for HTTP transport
- **ASP.NET Core**: Minimal API for HTTP endpoint hosting

## Project Structure

```
SampleMcpServer/
├── Program.cs              # Server configuration and startup
├── Tools/                  # MCP tool implementations
│   └── RandomNumberTools.cs
├── Services/              # Business logic and service classes (create as needed)
├── Prompts/               # MCP prompt implementations (create as needed)
├── appsettings.json       # Production configuration
└── appsettings.Development.json  # Development configuration
```

## Instructions

### Package Management
- Use the ModelContextProtocol NuGet package (prerelease): `dotnet add package ModelContextProtocol --prerelease`
- Use ModelContextProtocol.AspNetCore for HTTP-based MCP servers
- Use ModelContextProtocol.Core for minimal dependencies (client-only or low-level server APIs)

### Server Configuration
- Configure services using `builder.Services.AddMcpServer()`
- Use `WithStdioServerTransport()` for stdio transport (local process communication)
- Use `WithHttpTransport()` for HTTP transport (network-based communication)
- Register tools using `WithTools<TToolClass>()` or `WithToolsFromAssembly()` for auto-discovery
- Register prompts using `WithPrompts<TPromptClass>()` or `WithPromptsFromAssembly()`
- Map the MCP endpoint using `app.MapMcp("/mcp")` in minimal API setup

### Logging
- Always configure logging to stderr to avoid interfering with stdio transport
- Use `LogToStandardErrorThreshold = LogLevel.Trace` in console logger options
- Use structured logging for debugging without polluting stdout

### Tools
- Use the `[McpServerTool]` attribute on methods to expose them as tools
- Use the `[Description]` attribute from `System.ComponentModel` to document tools and parameters
- Tool methods can be synchronous or async (return `Task` or `Task<T>`)
- Return simple types (string, int, etc.) or complex objects that can be serialized to JSON
- Support dependency injection in tool methods - inject `McpServer`, `HttpClient`, or other services as parameters
- Use `CancellationToken` parameters in async tools for proper cancellation support
- Always include comprehensive descriptions for tools and parameters to help LLMs understand their purpose

### Prompts
- Expose prompts using `[McpServerPrompt]` on methods
- Use `[Description]` attribute to document prompts and their parameters
- Prompt methods should return prompt templates or structured responses

### Dependency Injection
- Structure projects with Microsoft.Extensions.Hosting for proper DI and lifecycle management
- Register services in `Program.cs` using `builder.Services.Add*()` methods
- Use constructor injection for class-based tools and prompts
- Use parameter injection for static tool methods

### Error Handling
- Use `McpProtocolException` for protocol-level errors with appropriate `McpErrorCode` values
- Validate input parameters and throw `McpProtocolException` with `McpErrorCode.InvalidParams` for invalid inputs
- Implement proper error handling and return meaningful error messages
- Use try-catch blocks for external service calls and resource access

### Async/Await Patterns
- Prefer async/await for I/O-bound operations
- Use `ConfigureAwait(false)` in library code (not necessary in ASP.NET Core)
- Always accept and pass through `CancellationToken` parameters
- Use `Task<T>` return types for async methods

## Best Practices

### Tool Design
- Keep tool methods focused and single-purpose
- Use meaningful tool names that clearly indicate their function
- Provide detailed descriptions that explain what the tool does, what parameters it expects, and what it returns
- Organize related tools into logical classes in the `Tools/` directory
- Consider security implications when exposing tools that access external resources

### Code Quality
- Enable nullable reference types (already configured in project)
- Use implicit usings (already configured in project)
- Follow C# naming conventions (PascalCase for public members, camelCase for parameters)
- Use XML documentation comments for public APIs
- Keep methods small and focused (single responsibility principle)

### Testing
- Test MCP servers using the `McpClient` from the same SDK or any compliant MCP client
- Create unit tests in a separate test project
- Test tools individually before integrating with LLMs
- Mock external dependencies for isolated testing

### Configuration
- Use `appsettings.json` for production configuration
- Use `appsettings.Development.json` for development-specific settings
- Store sensitive values in user secrets or environment variables, never in appsettings files
- Use the built-in DI container to manage service lifetimes and dependencies

## Common Patterns

### Basic Tool

```csharp
[McpServerTool]
[Description("Description of what the tool does")]
public int ToolName(
    [Description("Parameter description")] string param)
{
    return 42;
}
```

### Async Tool with Dependency Injection

```csharp
[McpServerTool]
[Description("Fetches data from a URL")]
public async Task<string> FetchData(
    HttpClient httpClient,
    [Description("The URL to fetch")] string url,
    CancellationToken cancellationToken)
{
    return await httpClient.GetStringAsync(url, cancellationToken);
}
```

### Tool with Sampling (LLM Interaction)

```csharp
[McpServerTool]
[Description("Analyzes content using the client's LLM")]
public async Task<string> Analyze(
    McpServer server,
    [Description("Content to analyze")] string content,
    CancellationToken cancellationToken)
{
    var messages = new ChatMessage[]
    {
        new(ChatRole.User, $"Analyze this: {content}")
    };
    return await server.AsSamplingChatClient()
        .GetResponseAsync(messages, cancellationToken: cancellationToken);
}
```

### Adding a New Service

1. Create the service interface and implementation in `Services/`
2. Register the service in `Program.cs`: `builder.Services.AddSingleton<IMyService, MyService>()`
3. Inject the service into tool methods or constructors

### Adding a New Tool

1. Create a new class in the `Tools/` directory
2. Add tool methods with `[McpServerTool]` and `[Description]` attributes
3. Register the tool class in `Program.cs`: `builder.Services.WithTools<MyToolClass>()`

## MCP Server Conventions

- Tool descriptions should be clear, concise, and explain the purpose and expected outcome
- Parameter descriptions should specify expected format, constraints, and examples when helpful
- Use default parameter values for optional parameters
- Return structured data when possible to make results easier to parse
- Follow consistent naming patterns across related tools
- Version your tools if breaking changes are introduced
