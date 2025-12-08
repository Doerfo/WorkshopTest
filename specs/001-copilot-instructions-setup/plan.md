# Implementation Plan: Copilot Instructions Setup MCP Server

**Branch**: `001-copilot-instructions-setup` | **Date**: December 7, 2025 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-copilot-instructions-setup/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

The MCP server will provide an automated setup capability for GitHub Copilot instruction files. It will analyze project technology stacks, retrieve baseline instructions from the awesome-copilot GitHub repository, merge company-specific guidelines, and generate properly structured instruction files. The implementation uses .NET 10 with ModelContextProtocol.Server library, exposing MCP tools for technology discovery, guideline retrieval, and baseline instruction fetching. A setup prompt running in agent mode orchestrates the entire workflow by invoking these tools to detect technologies, fetch appropriate content, merge with deduplication, and write instruction files to the target project.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: 
- ModelContextProtocol (0.5.0-preview.1)
- ModelContextProtocol.AspNetCore (0.5.0-preview.1)
- Microsoft.Extensions.Hosting
- System.Net.Http (for GitHub API calls)

**Storage**: 
- File system (for reading/writing instruction files in target projects)
- In-memory cache (for awesome-copilot repository file names)
- Local guideline files (markdown files in `Guidelines/` directory)

**Testing**: xUnit with Moq for mocking external dependencies (GitHub API, file system)

**Target Platform**: Cross-platform (.NET 10 runtime) - MCP server runs via stdio or HTTP transport

**Project Type**: MCP Server - exposes tools and prompts for AI assistant integration

**Performance Goals**: 
- Technology detection: <5 seconds for typical project
- Baseline retrieval from GitHub: <10 seconds per technology
- Complete setup workflow: <2 minutes (per SC-001)

**Constraints**: 
- Must not pollute stdout (MCP protocol requirement)
- Must handle network failures gracefully
- File operations must preserve existing user content (backups)

**Scale/Scope**: 
- Support 15-20 mainstream technologies initially
- Handle projects with 1-10 technologies simultaneously
- Cache awesome-copilot repository structure to minimize GitHub API calls

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Initial Check (Before Phase 0) ✅

#### I. MCP Tool Architecture ✅
- **Compliant**: All features exposed as MCP tools with `[McpServerTool]` attribute
- **Prompts**: Setup prompt uses `[McpServerPrompt]` attribute and runs in agent mode
- **Registration**: Tools registered in `Program.cs` using `.WithTools<>()` and `.WithPrompts<>()`
- **Transports**: Both stdio and HTTP configured

#### II. AI Discoverability ✅
- **Compliant**: All tools and prompts have comprehensive `[Description]` attributes
- **Tool descriptions**: Explain what each tool does, what it returns
- **Parameter descriptions**: Specify format, constraints, examples for technology names, file paths
- **Return types**: JSON-serializable objects for technology lists, string content for files

#### III. Dependency Injection ✅
- **Compliant**: Services use Microsoft.Extensions.DependencyInjection
- **Singleton services**: Cache service for repository structure (in-memory state)
- **Parameter injection**: HttpClient injected into tool methods
- **No newing up**: HttpClient, ILogger, configuration injected

#### IV. Modern .NET Conventions ✅
- **Compliant**: C# 13 and .NET 10 best practices followed
- **Nullable reference types**: Enabled
- **Async/await**: All I/O operations (GitHub API, file system) with CancellationToken
- **Records**: Used for technology info, guideline metadata
- **Naming**: PascalCase for public, camelCase for parameters

**GATE STATUS**: ✅ **PASSED** - No violations. All constitution principles satisfied.

---

### Post-Phase 1 Re-check ✅

#### I. MCP Tool Architecture ✅
- **Design Review**: 7 tools + 1 prompt defined in contracts
- **Separation**: Clear separation between Tools/, Services/, Models/
- **Compliance**: All tools follow `[McpServerTool]` pattern with descriptions

#### II. AI Discoverability ✅
- **Contract Review**: All parameters have detailed descriptions with examples
- **Return Types**: All use records (TechnologyInfo, BaselineInstruction, etc.)
- **Documentation**: Complete contracts in contracts/mcp-tools.md

#### III. Dependency Injection ✅
- **Service Interfaces**: All services have interfaces (ITechnologyDetectionService, etc.)
- **Lifetimes**: Singleton for cache, Transient for stateless services
- **Parameter Injection**: HttpClient, ILogger injected in tool methods

#### IV. Modern .NET Conventions ✅
- **Data Model**: Uses records exclusively for immutability
- **Async Patterns**: All I/O operations async with CancellationToken
- **Naming**: Follows .NET conventions throughout

**FINAL GATE STATUS**: ✅ **PASSED** - Design maintains constitutional compliance.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
SampleMcpServer/
├── Program.cs                          # MCP server configuration and startup
├── Prompts/
│   └── SetupCopilotInstructionsPrompt.cs  # Agent-mode setup prompt
├── Tools/
│   ├── TechnologyDiscoveryTools.cs     # List available technologies, detect project stack
│   ├── GuidelineTools.cs               # Retrieve company guideline content
│   ├── BaselineInstructionTools.cs     # Retrieve awesome-copilot baseline content
│   └── InstructionFileTools.cs         # Write/merge instruction files
├── Services/
│   ├── ITechnologyDetectionService.cs  # Interface for project analysis
│   ├── TechnologyDetectionService.cs   # Detect technologies from project structure
│   ├── IAwesomeCopilotCacheService.cs  # Interface for repository cache
│   ├── AwesomeCopilotCacheService.cs   # Cache awesome-copilot repo structure
│   ├── IGitHubService.cs               # Interface for GitHub API calls
│   ├── GitHubService.cs                # Fetch files from awesome-copilot repo
│   ├── IGuidelineService.cs            # Interface for guideline access
│   ├── GuidelineService.cs             # Load local guideline files
│   ├── IInstructionMergeService.cs     # Interface for content merging
│   └── InstructionMergeService.cs      # Merge baseline + guidelines with deduplication
├── Models/
│   ├── TechnologyInfo.cs               # Record for technology metadata
│   ├── GuidelineContent.cs             # Record for guideline file content
│   ├── BaselineInstruction.cs          # Record for awesome-copilot content
│   └── InstructionFileResult.cs        # Record for generated file results
├── Guidelines/                          # Company-specific guideline files
│   ├── csharp-testing.md
│   ├── typescript.md
│   ├── angular.md
│   └── react.md
└── appsettings.json                     # GitHub API settings, cache configuration

Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── TechnologyDetectionServiceTests.cs
│   │   ├── InstructionMergeServiceTests.cs
│   │   └── GitHubServiceTests.cs
│   └── Tools/
│       └── TechnologyDiscoveryToolsTests.cs
└── IntegrationTests/
    └── SetupWorkflowTests.cs            # End-to-end setup prompt execution
```

**Structure Decision**: Single MCP server project with clear separation of concerns. Tools directory contains MCP-exposed functionality, Services directory contains business logic, Models contains data structures. Guidelines directory stores company-specific markdown files. This follows the constitution's required project structure while maintaining MCP-specific organization (Tools/ and Prompts/ directories).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations** - Constitution check passed. No complexity justification needed.


---

## Phase Completion Summary

### ✅ Phase 0: Research (Complete)
- **Output**: [research.md](research.md)
- **Decisions Made**: 7 key technical decisions
- **Status**: All NEEDS CLARIFICATION items resolved

**Key Decisions**:
1. Technology Detection: File-based pattern matching
2. GitHub Access: REST API with 24-hour caching
3. Guideline Storage: Markdown files in Guidelines/ directory
4. Content Merging: Section-based with content hashing
5. Agent Prompt: Multi-turn workflow with tool orchestration
6. Error Handling: Graceful degradation with warnings
7. Caching: In-memory with TTL

### ✅ Phase 1: Design & Contracts (Complete)
- **Outputs**: 
  - [data-model.md](data-model.md) - 7 core entities defined
  - [contracts/mcp-tools.md](contracts/mcp-tools.md) - 7 tools + 1 prompt specified
  - [quickstart.md](quickstart.md) - User documentation with examples
- **Agent Context**: Updated `.github/agents/copilot-instructions.md`
- **Status**: Constitution re-check passed

**Deliverables**:
- Data model with immutable records
- Complete MCP tool contracts with I/O specs
- Quickstart guide with usage examples
- Updated agent context file

### 🔄 Next Phase: Implementation (/speckit.tasks)
- Generate task breakdown from this plan
- Create implementation tasks for Phase 2
- Begin coding tools, services, and prompts

---

## Implementation Readiness

**Constitutional Compliance**: ✅ All principles satisfied  
**Technical Unknowns**: ✅ All resolved in research phase  
**Data Model**: ✅ Complete with validation rules  
**API Contracts**: ✅ All 8 endpoints fully specified  
**Documentation**: ✅ Quickstart guide with examples  

**Ready for Phase 2: Implementation** ✅

---

**Planning Complete** | Branch: `001-copilot-instructions-setup` | Next: `/speckit.tasks`
