<!--
  SYNC IMPACT REPORT
  ==================
  Version Change: INITIAL → 1.0.0
  Type: MINOR (Initial constitution establishment)
  
  Changes Made:
  - Initial constitution created for .NET MCP Server Workshop project
  - Established 6 core principles for MCP server development
  - Added Technology Stack section with .NET 10 and MCP SDK requirements
  - Added Development Workflow section with workshop learning objectives
  
  Principles Established:
  1. ModelContextProtocol SDK Integration (NEW)
  2. AI Discoverability Through Attributes (NEW)
  3. Dependency Injection for State Management (NEW)
  4. User-Friendly Tool Responses (NEW)
  5. Workshop-Focused Simplicity (NEW)
  6. Modern .NET Patterns (NEW)
  
  Template Consistency Status:
  ✅ plan-template.md - Reviewed, compatible with constitution principles
  ✅ spec-template.md - Reviewed, user story structure aligns with workshop objectives
  ✅ tasks-template.md - Reviewed, task organization supports incremental learning
  ⚠️  checklist-template.md - Generic template, no updates needed
  ⚠️  agent-file-template.md - Generic template, no updates needed
  
  Follow-up Actions:
  - None required at this time
  - Future amendments should follow semantic versioning guidelines
  - Consider adding testing principles if TDD is introduced to workshop
  
  Date: 2025-12-07
-->

# .NET MCP Server Workshop Constitution

## Core Principles

### I. ModelContextProtocol SDK Integration
All MCP functionality MUST use the official ModelContextProtocol.Server library with declarative attributes.

**Requirements**:
- Use `[McpServerTool]` attribute to expose tool methods
- Use `[McpServerPrompt]` attribute to expose prompt methods  
- Configure server using `AddMcpServer()` builder pattern
- Support both stdio and HTTP transports for maximum compatibility
- Register tools and prompts through DI container using `WithTools<T>()` and `WithPrompts<T>()`

**Rationale**: Declarative attributes make MCP capabilities immediately discoverable by AI assistants and ensure consistency with MCP specification. The official SDK handles protocol complexity, allowing developers to focus on business logic.

### II. AI Discoverability Through Attributes
All tools and prompts MUST include comprehensive `[Description]` attributes from `System.ComponentModel` namespace.

**Requirements**:
- Every tool method MUST have a `[Description]` attribute explaining its purpose
- Every tool parameter MUST have a `[Description]` attribute explaining expected input
- Descriptions MUST be clear, concise, and explain what the tool does and what it returns
- Descriptions MUST help LLMs understand when and how to use the tool appropriately

**Rationale**: LLMs discover and invoke tools based on descriptions. Clear, comprehensive descriptions are the primary mechanism for AI assistants to understand tool capabilities and select appropriate tools for user requests.

### III. Dependency Injection for State Management
Services managing state MUST be registered through dependency injection with appropriate lifetimes.

**Requirements**:
- In-memory state services MUST be registered as Singleton
- Use constructor injection for class-based tools and prompts
- Use parameter injection for static tool methods (automatically resolved by MCP SDK)
- Follow Microsoft.Extensions.DependencyInjection patterns
- Inject framework services (McpServer, HttpClient, CancellationToken) as method parameters

**Rationale**: DI provides predictable service lifetimes, enables testability, and ensures single source of truth for in-memory state. Singleton registration guarantees state persistence across tool invocations within server lifetime.

### IV. User-Friendly Tool Responses
Tool return values MUST be formatted for human readability with visual indicators.

**Requirements**:
- Return formatted strings rather than raw data structures when appropriate
- Use emojis to indicate status (✅ success, ❌ error, ℹ️ info, 📝 data, etc.)
- Provide clear, actionable messages for error conditions
- Structure multi-item responses with bullets or numbering
- Include context in responses (e.g., "Added note 'Meeting Notes' ✅" vs just "Success")

**Rationale**: Workshop focuses on building delightful user experiences. Formatted responses with visual indicators make tool output more engaging and easier to understand in chat interfaces, demonstrating best practices for production MCP servers.

### V. Workshop-Focused Simplicity
Implementation MUST prioritize learning objectives over production-ready complexity.

**Requirements**:
- Keep implementation simple and focused on core MCP concepts
- Avoid unnecessary abstractions, patterns, or frameworks beyond MCP SDK
- Use in-memory storage (no databases) for state management
- Minimize external dependencies
- Code MUST be readable and well-commented for educational purposes
- Demonstrate one clear way to accomplish each task

**Rationale**: This is a workshop template designed for learning. Complexity obscures learning objectives. Simple, focused implementations help developers understand MCP concepts without distraction.

### VI. Modern .NET Patterns
Code MUST follow current .NET conventions and leverage modern C# language features.

**Requirements**:
- Use C# 13 features where appropriate (primary constructors, collection expressions, etc.)
- Follow .NET naming conventions (PascalCase for public members, camelCase for parameters)
- Enable nullable reference types (already configured in project)
- Use implicit usings (already configured in project)
- Prefer async/await for I/O operations with CancellationToken support
- Use record types for immutable data structures
- Implement proper error handling with McpProtocolException

**Rationale**: Demonstrates idiomatic modern .NET development. Workshop participants learn current best practices alongside MCP concepts. Modern language features reduce boilerplate and improve code clarity.

## Technology Stack

**Required Dependencies**:
- .NET 10 SDK (latest framework with C# 13 support)
- ModelContextProtocol SDK version 0.5.0-preview.1 or later
- ModelContextProtocol.AspNetCore for HTTP transport support

**Hosting & Transport**:
- ASP.NET Core minimal APIs for HTTP endpoint hosting
- Stdio transport for local process communication (VS Code integration)
- HTTP transport for network-based access and testing

**Project Structure**:
```
SampleMcpServer/
├── Program.cs              # Server configuration and startup
├── Tools/                  # MCP tool implementations
├── Prompts/               # MCP prompt implementations (create as needed)
├── Services/              # Business logic and service classes
├── appsettings.json       # Production configuration
└── appsettings.Development.json  # Development configuration
```

## Development Workflow

**Workshop Learning Objectives**:
1. Understand MCP protocol basics and server configuration
2. Implement tools using declarative attributes
3. Practice dependency injection for service management
4. Create user-friendly, AI-discoverable interfaces
5. Test MCP servers with GitHub Copilot and other MCP clients

**Development Practices**:
- Log to stderr to avoid interfering with stdio transport (use `LogToStandardErrorThreshold = LogLevel.Trace`)
- Test tools individually before integration
- Use structured logging for debugging without polluting stdout
- Validate tool descriptions for clarity and completeness
- Follow security best practices when accessing external resources

**Code Quality Standards**:
- Methods should be focused and single-purpose
- Use meaningful names that clearly indicate function
- Keep methods small (single responsibility principle)
- Add XML documentation comments for public APIs
- Group related tools into logical classes in Tools/ directory

## Governance

This constitution establishes the foundational principles and standards for the .NET MCP Server Workshop project. All code contributions, feature specifications, and implementation plans MUST align with these principles.

**Amendment Process**:
- Constitution amendments require clear justification and impact analysis
- Version numbering follows semantic versioning:
  - MAJOR: Backward incompatible governance changes or principle removals
  - MINOR: New principles added or materially expanded guidance
  - PATCH: Clarifications, wording improvements, non-semantic refinements
- All amendments MUST include Sync Impact Report documenting changes to dependent templates and documentation

**Compliance & Review**:
- All feature specifications MUST pass constitution check before implementation
- Plans MUST document any complexity exceptions with justification
- Templates in `.specify/templates/` MUST remain consistent with constitution principles
- Refer to `.github/copilot-instructions.md` for runtime development guidance

**Complexity Justification**:
When principles are violated, teams MUST document:
1. Which principle is violated and why
2. What simpler alternative was considered
3. Why the simpler alternative is insufficient for the specific use case

**Version**: 1.0.0 | **Ratified**: 2025-12-07 | **Last Amended**: 2025-12-07
