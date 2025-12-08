<!--
Sync Impact Report:
Version: Initial → 1.0.0
Modified Principles: N/A (initial ratification)
Added Sections: 
  - I. MCP Tool Architecture
  - II. AI Discoverability
  - III. Dependency Injection
  - IV. Modern .NET Conventions
  - Technology Stack
  - Development Standards
Removed Sections: N/A
Templates Status:
  ✅ .specify/templates/plan-template.md - reviewed, no updates needed
  ✅ .specify/templates/spec-template.md - reviewed, no updates needed
  ✅ .specify/templates/tasks-template.md - reviewed, no updates needed
  ✅ .specify/templates/checklist-template.md - reviewed, no updates needed
  ✅ .specify/templates/agent-file-template.md - reviewed, no updates needed
  ✅ .github/copilot-instructions.md - reviewed, already contains MCP-specific guidance
  ✅ README.md - reviewed, aligned with constitution principles
Follow-up TODOs: None
-->

# .NET MCP Server Constitution

## Core Principles

### I. MCP Tool Architecture

All features MUST be exposed as MCP tools using the ModelContextProtocol.Server library.
Implementation requirements:
- Tools MUST use the `[McpServerTool]` attribute on methods
- Prompts MUST use the `[McpServerPrompt]` attribute on methods
- Tool classes MUST be registered in `Program.cs` using `.WithTools<TToolClass>()`
- Prompt classes MUST be registered in `Program.cs` using `.WithPrompts<TPromptClass>()`
- Both stdio and HTTP transports MUST be configured for maximum compatibility

**Rationale**: MCP is the integration contract with AI assistants. Consistent use of the official
SDK ensures protocol compliance and reduces integration friction.

### II. AI Discoverability

Every tool and prompt MUST have comprehensive `[Description]` attributes from
`System.ComponentModel` on both the method and all parameters.

Requirements:
- Method descriptions MUST explain what the tool does and what it returns
- Parameter descriptions MUST specify expected format, constraints, and examples
- Descriptions MUST be clear enough for an LLM to understand usage without source code access
- Return types MUST be simple (string, int) or JSON-serializable objects

**Rationale**: AI agents rely entirely on descriptions to understand tool capabilities. Poor
descriptions lead to incorrect tool invocation and degraded user experience.

### III. Dependency Injection

Services MUST use Microsoft.Extensions.DependencyInjection for all dependencies.

Requirements:
- Services with in-memory state MUST be registered as Singleton
- Transient services MUST be stateless
- Tool methods MUST accept dependencies as method parameters (parameter injection)
- Constructor injection MAY be used for class-based tools
- External dependencies (HttpClient, configuration) MUST be injected, never newed up

**Rationale**: DI enables testability, promotes loose coupling, and is the standard .NET pattern
for managing service lifetimes and dependencies.

### IV. Modern .NET Conventions

Code MUST follow C# 13 and .NET 10 best practices.

Requirements:
- Use nullable reference types (enabled by default)
- Use implicit usings (enabled by default)
- Prefer records for immutable data models
- Prefer primary constructors for simple classes
- Use PascalCase for public members, camelCase for parameters and local variables
- Use async/await for I/O-bound operations with `CancellationToken` support
- Follow .NET naming conventions (IService interfaces, ServiceImpl implementations)

**Rationale**: Modern C# features improve code clarity, safety, and maintainability. Consistency
reduces cognitive load and makes the codebase accessible to any .NET developer.

## Technology Stack

**Framework**: .NET 10 SDK with C# 13
**Core Dependencies**:
- ModelContextProtocol (version 0.5.0-preview.1 or later)
- ModelContextProtocol.AspNetCore (for HTTP transport)
- Microsoft.Extensions.Hosting (for DI and lifecycle)

**Project Structure**:
- `Program.cs`: Server configuration and startup
- `Tools/`: MCP tool implementations
- `Prompts/`: MCP prompt implementations (create as needed)
- `Services/`: Business logic and service classes (create as needed)
- `appsettings.json`: Production configuration
- `appsettings.Development.json`: Development configuration

**Testing** (when required):
- Unit tests in separate test project
- Use xUnit or NUnit
- Mock external dependencies using Moq or NSubstitute

## Development Standards

**Logging**:
- MUST configure logging to stderr to avoid interfering with stdio transport
- Use `LogToStandardErrorThreshold = LogLevel.Trace` in console logger options
- Use structured logging with proper log levels (Trace, Debug, Information, Warning, Error, Critical)
- Never write to stdout (reserved for MCP protocol)

**Error Handling**:
- Use `McpProtocolException` for protocol-level errors with appropriate `McpErrorCode` values
- Validate input parameters and throw `McpProtocolException` with `McpErrorCode.InvalidParams`
- Implement try-catch for external service calls with meaningful error messages
- Always accept and propagate `CancellationToken` in async methods

**Configuration**:
- Production settings in `appsettings.json`
- Development overrides in `appsettings.Development.json`
- Sensitive values MUST use user secrets or environment variables
- Never commit secrets to appsettings files

**Security**:
- Validate all input parameters
- Sanitize data before external calls
- Consider rate limiting for resource-intensive tools
- Document security implications in tool descriptions

## Governance

This constitution supersedes all other development practices and guidelines. All code changes,
pull requests, and feature implementations MUST comply with these principles.

**Amendment Process**:
1. Proposed changes MUST document rationale and impact
2. Version MUST increment per semantic versioning (MAJOR.MINOR.PATCH)
3. All dependent templates and documentation MUST be updated to maintain consistency
4. Sync Impact Report MUST be generated and reviewed

**Compliance**:
- All PRs MUST be reviewed for constitutional compliance before merge
- Violations MUST be justified in writing or corrected
- Unjustified complexity MUST be rejected
- Reference `.github/copilot-instructions.md` for runtime development guidance

**Version**: 1.0.0 | **Ratified**: 2025-12-07 | **Last Amended**: 2025-12-07
