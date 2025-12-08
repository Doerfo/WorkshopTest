# Quickstart Guide: Copilot Instructions Setup

**Feature**: Automated GitHub Copilot instruction file setup  
**Branch**: `001-copilot-instructions-setup`

This guide helps you get started with setting up GitHub Copilot instructions for your projects using the MCP server.

---

## Prerequisites

- .NET 10 SDK installed
- GitHub Copilot or compatible MCP client
- Project to set up (any programming language/framework)

---

## Quick Start (5 minutes)

### 1. Start the MCP Server

```bash
cd SampleMcpServer
dotnet run
```

The server starts with both stdio and HTTP transports available.

### 2. Invoke the Setup Prompt

From your MCP client (GitHub Copilot, Claude Desktop, etc.):

```
Use the setup-copilot-instructions prompt to set up my project at /path/to/my/project
```

### 3. Follow the Agent Workflow

The agent will:
1. ✅ Detect technologies in your project
2. ✅ Fetch baseline instructions from awesome-copilot
3. ✅ Merge with company guidelines (if available)
4. ✅ Create instruction files:
   - `.github/copilot-instructions.md` (repository-wide)
   - `.github/instructions/{technology}.instructions.md` (per technology)

### 4. Review Generated Files

Check the created instruction files:

```bash
# Repository-wide instructions
cat .github/copilot-instructions.md

# Technology-specific instructions
ls -la .github/instructions/
cat .github/instructions/csharp.instructions.md
```

---

## Usage Examples

### Example 1: Set Up a New .NET Project

**Project Structure**:
```
MyProject/
├── MyProject.csproj
├── Program.cs
└── Services/
    └── DataService.cs
```

**Command**:
```
Set up Copilot instructions for /workspace/MyProject
```

**Result**:
```
✅ Detected technologies: C#, .NET
✅ Retrieved baseline: csharp.instructions.md from awesome-copilot
✅ Applied company guidelines: csharp-testing.md
✅ Created:
   - .github/copilot-instructions.md (repository-wide)
   - .github/instructions/csharp.instructions.md (merged baseline + guidelines)
```

---

### Example 2: Full-Stack Web App (React + Node.js)

**Project Structure**:
```
WebApp/
├── frontend/
│   ├── package.json (with react dependency)
│   └── src/
│       └── components/
├── backend/
│   ├── package.json (with express)
│   └── server.js
```

**Command**:
```
Set up Copilot instructions for /workspace/WebApp
```

**Result**:
```
✅ Detected technologies: TypeScript, React, Node.js, JavaScript
✅ Retrieved baselines: typescript, react, nodejs, javascript
✅ Created:
   - .github/copilot-instructions.md (lists all 4 technologies)
   - .github/instructions/typescript.instructions.md
   - .github/instructions/react.instructions.md
   - .github/instructions/nodejs.instructions.md
   - .github/instructions/javascript.instructions.md
```

---

### Example 3: Update Existing Instructions

**Scenario**: You already have instruction files but want to refresh with latest baselines.

**Command**:
```
Update Copilot instructions for /workspace/MyProject with updateExisting=true
```

**Result**:
```
✅ Detected existing files
✅ Created backups:
   - .github/copilot-instructions.md.20251207103000.backup
   - .github/instructions/csharp.instructions.md.20251207103000.backup
✅ Updated with latest baselines and guidelines
```

---

### Example 4: Project with Unsupported Technology

**Scenario**: Project uses a technology not in awesome-copilot or company guidelines.

**Command**:
```
Set up Copilot instructions for /workspace/ElixirProject
```

**Interaction**:
```
Agent: No technologies automatically detected. Please select from available:
  - csharp
  - typescript
  - javascript
  - python
  - java
  - react
  - angular
  - vue
  - golang
  - rust

You: elixir (or skip)

Agent: ⚠️ Warning: Technology 'elixir' not found in awesome-copilot
✅ Created minimal instruction file at .github/instructions/elixir.instructions.md
ℹ️ Please manually customize this file with Elixir-specific guidelines
```

---

## Manual Tool Invocation

If you prefer to control each step manually instead of using the agent-mode prompt:

### Step 1: List Available Technologies

```
Invoke list-available-technologies tool
```

**Output**: List of all supported technologies

### Step 2: Detect Technologies in Your Project

```
Invoke detect-project-technologies tool with projectPath=/workspace/MyProject
```

**Output**: Detected technologies with confidence levels

### Step 3: Get Baseline for a Technology

```
Invoke get-baseline-instruction tool with technology=csharp
```

**Output**: Baseline instruction content from awesome-copilot

### Step 4: Get Company Guidelines

```
Invoke get-company-guideline tool with technology=csharp
```

**Output**: List of company guideline files

### Step 5: Create Repository-Wide File

```
Invoke create-repository-instruction-file tool with:
  - projectPath=/workspace/MyProject
  - technologies=["csharp", "typescript"]
  - updateExisting=false
```

**Output**: File creation result

### Step 6: Create Technology-Specific Files

```
Invoke create-technology-instruction-file tool with:
  - projectPath=/workspace/MyProject
  - technology=csharp
  - baseline=<baseline content>
  - guidelines=<guideline content list>
  - updateExisting=false
```

**Output**: File creation result with merge details

---

## Adding Company Guidelines

### 1. Create Guideline File

Create a markdown file in the MCP server's `Guidelines/` directory:

**File**: `SampleMcpServer/Guidelines/csharp-testing.md`

```markdown
---
description: Company-specific C# testing guidelines
technology: csharp
aspect: testing
---

# C# Testing Standards

## Unit Testing

- All public methods must have unit tests
- Use xUnit test framework
- Test coverage minimum: 80%

## Naming Conventions

- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

## Examples

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange, Act, Assert...
    }
}
```
```

### 2. Restart MCP Server

```bash
# Stop server (Ctrl+C)
# Start again
dotnet run
```

### 3. Run Setup Again

The new guideline will be automatically discovered and included in the next setup.

---

## Troubleshooting

### Issue: No Technologies Detected

**Cause**: Project structure doesn't match detection patterns

**Solution**: 
1. Check if standard files exist (e.g., `.csproj`, `package.json`)
2. Manually select technologies when prompted
3. Or create minimal indicator files

### Issue: GitHub API Rate Limit

**Cause**: Too many requests to awesome-copilot repository

**Solution**:
- Wait for rate limit to reset (shown in error message)
- Server uses cached baselines automatically
- Or authenticate GitHub requests (future enhancement)

### Issue: Permission Denied Writing Files

**Cause**: No write access to project directory

**Solution**:
- Check file permissions on project directory
- Ensure `.github/` directory is writable
- Run with appropriate user permissions

### Issue: Existing Files Not Updated

**Cause**: `updateExisting` flag not set

**Solution**:
- Use `updateExisting=true` in prompt or tool invocation
- Or manually delete existing files first

---

## Configuration

### GitHub API Settings

Edit `appsettings.json`:

```json
{
  "GitHub": {
    "Repository": "github/awesome-copilot",
    "InstructionsPath": "instructions",
    "CacheDurationHours": 24,
    "UserAgent": "SampleMcpServer/1.0"
  }
}
```

### Cache Settings

Edit `appsettings.json`:

```json
{
  "Cache": {
    "AwesomeCopilotTtlHours": 24,
    "EnablePersistence": false
  }
}
```

### Logging

Edit `appsettings.Development.json` for development:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SampleMcpServer.Services": "Debug",
      "SampleMcpServer.Tools": "Debug"
    }
  }
}
```

---

## Best Practices

### 1. Technology Detection

- ✅ Keep standard project files (`.csproj`, `package.json`) in root
- ✅ Use conventional directory structures
- ❌ Don't rely on ambiguous indicators

### 2. Company Guidelines

- ✅ One guideline file per technology-aspect combination
- ✅ Use clear, specific aspect names (testing, security, performance)
- ✅ Follow markdown format with frontmatter
- ❌ Don't create overlapping guidelines (causes confusion)

### 3. Updating Instructions

- ✅ Review backup files before deleting
- ✅ Update guidelines incrementally (test with one technology first)
- ✅ Use version control for instruction files
- ❌ Don't blindly overwrite custom modifications

### 4. Multi-Technology Projects

- ✅ Organize by clear technology boundaries
- ✅ Use path-specific applyTo patterns in instruction files
- ✅ Keep repository-wide file focused on general standards
- ❌ Don't mix frontend/backend guidelines in same file

---

## Next Steps

After setting up Copilot instructions:

1. **Test Copilot Suggestions**: Verify that Copilot respects the instructions
2. **Iterate on Guidelines**: Refine company-specific rules based on team feedback
3. **Automate in CI/CD**: Consider adding setup check in CI pipeline
4. **Share Across Projects**: Use same MCP server for multiple projects
5. **Contribute Back**: Submit useful baselines to awesome-copilot repository

---

## Support

- **Issues**: Report bugs or request features in repository
- **Documentation**: See [plan.md](plan.md), [data-model.md](data-model.md), [contracts/](contracts/)
- **Examples**: Check `/examples` directory for sample projects

---

**Quickstart Complete** ✅  
You're ready to use the Copilot Instructions Setup MCP server!
