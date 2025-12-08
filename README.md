# Copilot Instructions Setup MCP Server

An MCP server that automates the setup of GitHub Copilot instruction files by analyzing project technology stacks, fetching baseline instructions from the awesome-copilot repository, and merging them with company-specific guidelines.

## Features

- **Automated Technology Detection**: Analyzes project structure to identify technologies (C#, TypeScript, React, Python, etc.)
- **Baseline Instructions**: Fetches best practice instructions from [github/awesome-copilot](https://github.com/github/awesome-copilot)
- **Company Guidelines**: Merges organizational coding standards with baseline instructions
- **Smart Deduplication**: Automatically deduplicates content with company guidelines taking precedence
- **Backup Management**: Creates timestamped backups when updating existing instruction files
- **MCP Integration**: Works seamlessly with GitHub Copilot and other MCP-compatible clients

## Quick Start

### 1. Build and Run
```bash
cd SampleMcpServer
dotnet build
dotnet run
```

### 2. Configure in VS Code

Add to your `~/.vscode/mcp.json`:

```json
{
  "mcpServers": {
    "copilot-instructions-setup": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/workspaces/WorkshopTest/SampleMcpServer"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### 3. Use the Setup Prompt

In GitHub Copilot Chat or any MCP client:

```
Use the setup-copilot-instructions prompt to set up my project at /path/to/my/project
```

The agent will:
1. ✅ Detect technologies (C#, TypeScript, React, etc.)
2. ✅ Fetch baselines from awesome-copilot
3. ✅ Merge with company guidelines  
4. ✅ Create instruction files:
   - `.github/copilot-instructions.md` (repository-wide)
   - `.github/instructions/{technology}.instructions.md` (per technology)

## Available Tools

The MCP server exposes these tools:

- **ListAvailableTechnologies**: List all supported technologies
- **DetectProjectTechnologies**: Analyze project to detect tech stack
- **GetBaselineInstruction**: Fetch baseline from awesome-copilot
- **GetCompanyGuideline**: Retrieve company-specific guidelines
- **CreateRepositoryInstructionFile**: Generate `.github/copilot-instructions.md`
- **CreateTechnologyInstructionFile**: Generate technology-specific files
- **RefreshCache**: Manually refresh awesome-copilot cache

## Adding Company Guidelines

Create markdown files in `SampleMcpServer/Guidelines/`:

**Naming Convention**: `{technology}[-{aspect}].md`

**Example**: `SampleMcpServer/Guidelines/csharp-testing.md`

```markdown
---
description: Company-specific C# testing guidelines
technology: csharp
aspect: testing
---

# C# Testing Standards

- Use xUnit framework
- 80% code coverage minimum
- Follow Arrange-Act-Assert pattern

[... more content ...]
```

Guidelines are automatically discovered and merged during setup!

## Configuration

Edit `appsettings.json` to customize:

```json
{
  "GitHub": {
    "Repository": "github/awesome-copilot",
    "InstructionsPath": "instructions",
    "CacheDurationHours": 24
  }
}
```

## Project Structure

```
WorkshopTest/
├── SampleMcpServer/
│   ├── Program.cs              # MCP server configuration
│   ├── Models/                 # Data models (TechnologyInfo, BaselineInstruction, etc.)
│   ├── Services/               # Business logic services
│   │   ├── TechnologyDetectionService.cs
│   │   ├── GitHubService.cs
│   │   ├── GuidelineService.cs
│   │   └── InstructionMergeService.cs
│   ├── Tools/                  # MCP tools
│   │   ├── TechnologyDiscoveryTools.cs
│   │   ├── BaselineInstructionTools.cs
│   │   ├── GuidelineTools.cs
│   │   └── InstructionFileTools.cs
│   ├── Prompts/                # MCP prompts
│   │   └── SetupCopilotInstructionsPrompt.cs
│   ├── Guidelines/             # Company-specific guidelines
│   │   ├── csharp-testing.md
│   │   ├── typescript.md
│   │   └── react.md
│   └── appsettings.json        # Configuration
└── specs/
    └── 001-copilot-instructions-setup/  # Feature specification
```

## Technology Detection

The server automatically detects:

- **C#**: `*.csproj`, `*.cs` files
- **TypeScript**: `tsconfig.json`, `*.ts` files  
- **JavaScript**: `package.json`, `*.js` files
- **Python**: `requirements.txt`, `*.py` files
- **Java**: `pom.xml`, `build.gradle`
- **React**: `package.json` with react dependency
- **Angular**: `angular.json`
- **Vue**: `package.json` with vue dependency
- **Go**: `go.mod`, `*.go` files
- **Rust**: `Cargo.toml`, `*.rs` files

## Example Workflow

```
1. User: "Setup Copilot instructions for my project"
2. MCP Agent invokes DetectProjectTechnologies
   → Finds C# and TypeScript
3. Agent invokes GetBaselineInstruction for each
   → Fetches csharp.instructions.md and typescript.instructions.md
4. Agent invokes GetCompanyGuideline for each
   → Loads csharp-testing.md and typescript.md from Guidelines/
5. Agent invokes CreateTechnologyInstructionFile
   → Merges baseline + guidelines
   → Writes .github/instructions/csharp.instructions.md
   → Writes .github/instructions/typescript.instructions.md
6. Agent creates .github/copilot-instructions.md
7. Result: Project now has tailored Copilot instructions!
```

## For More Details

See the complete documentation in `specs/001-copilot-instructions-setup/`:
- [quickstart.md](specs/001-copilot-instructions-setup/quickstart.md) - Detailed usage guide
- [plan.md](specs/001-copilot-instructions-setup/plan.md) - Technical implementation plan
- [contracts/mcp-tools.md](specs/001-copilot-instructions-setup/contracts/mcp-tools.md) - Full API contracts

## License

This template is provided as-is for educational and development purposes.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.