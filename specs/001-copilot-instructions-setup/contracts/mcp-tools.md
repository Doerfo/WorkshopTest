# MCP Tool Contracts

This document defines the MCP tool interfaces for the Copilot Instructions Setup server.

## Tools

### 1. list-available-technologies

Lists all technologies that the MCP server has baseline instructions or company guidelines for.

**Method Signature**:
```csharp
[McpServerTool("list-available-technologies")]
[Description("Returns a list of all technologies supported by the MCP server, including both baseline instructions from awesome-copilot and company-specific guidelines")]
public async Task<List<TechnologyInfo>> ListAvailableTechnologies(
    CancellationToken cancellationToken = default)
```

**Input**: None

**Output**: `List<TechnologyInfo>`
```json
[
  {
    "name": "csharp",
    "displayName": "C#",
    "hasBaseline": true,
    "hasGuideline": true,
    "detectionConfidence": null
  },
  {
    "name": "typescript",
    "displayName": "TypeScript",
    "hasBaseline": true,
    "hasGuideline": true,
    "detectionConfidence": null
  }
]
```

**Errors**:
- `InternalError`: If cache refresh fails and no cached data available

---

### 2. detect-project-technologies

Analyzes a project directory to detect technologies used.

**Method Signature**:
```csharp
[McpServerTool("detect-project-technologies")]
[Description("Analyzes a project directory structure to automatically detect which technologies and frameworks are being used")]
public async Task<TechnologyDetectionResult> DetectProjectTechnologies(
    [Description("Absolute path to the project directory to analyze")]
    string projectPath,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `projectPath` (string, required): Absolute path to project directory

**Output**: `TechnologyDetectionResult`
```json
{
  "detectedTechnologies": [
    {
      "name": "csharp",
      "displayName": "C#",
      "hasBaseline": true,
      "hasGuideline": true,
      "detectionConfidence": "High"
    },
    {
      "name": "typescript",
      "displayName": "TypeScript",
      "hasBaseline": true,
      "hasGuideline": false,
      "detectionConfidence": "Medium"
    }
  ],
  "indicators": {
    "csharp": ["SampleMcpServer.csproj", "Program.cs", "*.cs files (5 found)"],
    "typescript": ["tsconfig.json"]
  },
  "ambiguousTechnologies": ["typescript"],
  "projectPath": "/workspaces/WorkshopTest",
  "analyzedAt": "2025-12-07T10:30:00Z"
}
```

**Errors**:
- `InvalidParams`: If `projectPath` doesn't exist or is not a directory
- `InternalError`: If file system access fails

---

### 3. get-baseline-instruction

Retrieves baseline instruction content from the awesome-copilot repository for a specific technology.

**Method Signature**:
```csharp
[McpServerTool("get-baseline-instruction")]
[Description("Fetches the baseline instruction file for a specific technology from the GitHub awesome-copilot repository")]
public async Task<BaselineInstruction?> GetBaselineInstruction(
    [Description("Technology name (lowercase, e.g., 'csharp', 'typescript', 'react')")]
    string technology,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `technology` (string, required): Technology name (must match awesome-copilot repository naming)

**Output**: `BaselineInstruction` or `null` if not found
```json
{
  "technology": "csharp",
  "fileName": "csharp.instructions.md",
  "content": "---\ndescription: C# coding guidelines...\n---\n\n# C# Instructions\n\n...",
  "sourceUrl": "https://raw.githubusercontent.com/github/awesome-copilot/main/instructions/csharp.instructions.md",
  "retrievedAt": "2025-12-07T10:30:00Z",
  "sha": "abc123def456..."
}
```

**Errors**:
- `InvalidParams`: If `technology` is empty or contains invalid characters
- `InternalError`: If GitHub API call fails and no cached baseline available

**Notes**:
- Returns `null` if technology not found in awesome-copilot repository
- Uses cached version if available and not expired
- Fallback to cached version if GitHub API unavailable

---

### 4. get-company-guideline

Retrieves company-specific guideline content for a technology.

**Method Signature**:
```csharp
[McpServerTool("get-company-guideline")]
[Description("Retrieves all company-specific guideline files for a specific technology from the MCP server's Guidelines directory")]
public async Task<List<GuidelineContent>> GetCompanyGuideline(
    [Description("Technology name (lowercase, e.g., 'csharp', 'typescript', 'react')")]
    string technology,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `technology` (string, required): Technology name

**Output**: `List<GuidelineContent>` (empty list if no guidelines found)
```json
[
  {
    "technology": "csharp",
    "aspect": "testing",
    "filePath": "/app/Guidelines/csharp-testing.md",
    "content": "---\ntechnology: csharp\naspect: testing\n---\n\n# C# Testing Guidelines\n\n...",
    "frontmatter": {
      "technology": "csharp",
      "aspect": "testing"
    }
  },
  {
    "technology": "csharp",
    "aspect": null,
    "filePath": "/app/Guidelines/csharp.md",
    "content": "# General C# Guidelines\n\n...",
    "frontmatter": null
  }
]
```

**Errors**:
- `InvalidParams`: If `technology` is empty or invalid
- `InternalError`: If guideline file exists but cannot be read

**Notes**:
- Returns empty list if no guidelines found (not an error)
- Returns all guideline files matching the technology (e.g., `csharp.md`, `csharp-testing.md`)

---

### 5. create-repository-instruction-file

Creates or updates the repository-wide `.github/copilot-instructions.md` file.

**Method Signature**:
```csharp
[McpServerTool("create-repository-instruction-file")]
[Description("Creates or updates the repository-wide .github/copilot-instructions.md file with general coding standards and tech stack overview")]
public async Task<InstructionFileResult> CreateRepositoryInstructionFile(
    [Description("Absolute path to the project directory")]
    string projectPath,
    
    [Description("List of technologies detected in the project")]
    List<string> technologies,
    
    [Description("Whether to update the file if it already exists (creates backup if true)")]
    bool updateExisting = false,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `projectPath` (string, required): Absolute path to project
- `technologies` (List<string>, required): List of technology names
- `updateExisting` (bool, optional, default: false): Whether to overwrite existing file

**Output**: `InstructionFileResult`
```json
{
  "filePath": "/workspaces/WorkshopTest/.github/copilot-instructions.md",
  "technology": null,
  "status": "Created",
  "backupPath": null,
  "bytesWritten": 2048,
  "message": "Repository-wide instruction file created successfully"
}
```

**Errors**:
- `InvalidParams`: If `projectPath` doesn't exist or `technologies` is empty
- `InternalError`: If file cannot be written (permissions, disk space)

**Notes**:
- Creates `.github/` directory if it doesn't exist
- If file exists and `updateExisting=false`, returns status `Skipped`
- If file exists and `updateExisting=true`, creates backup with timestamp

---

### 6. create-technology-instruction-file

Creates or updates a technology-specific instruction file in `.github/instructions/`.

**Method Signature**:
```csharp
[McpServerTool("create-technology-instruction-file")]
[Description("Creates or updates a technology-specific instruction file in .github/instructions/ directory with merged baseline and company guidelines")]
public async Task<InstructionFileResult> CreateTechnologyInstructionFile(
    [Description("Absolute path to the project directory")]
    string projectPath,
    
    [Description("Technology name (lowercase)")]
    string technology,
    
    [Description("Baseline instruction content from awesome-copilot (null if not available)")]
    BaselineInstruction? baseline,
    
    [Description("List of company guideline contents for this technology (empty if none)")]
    List<GuidelineContent> guidelines,
    
    [Description("Whether to update the file if it already exists (creates backup if true)")]
    bool updateExisting = false,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `projectPath` (string, required): Absolute path to project
- `technology` (string, required): Technology name
- `baseline` (BaselineInstruction?, optional): Baseline content (null if not available)
- `guidelines` (List<GuidelineContent>, required): Company guidelines (can be empty)
- `updateExisting` (bool, optional, default: false): Whether to overwrite existing file

**Output**: `InstructionFileResult`
```json
{
  "filePath": "/workspaces/WorkshopTest/.github/instructions/csharp.instructions.md",
  "technology": "csharp",
  "status": "Updated",
  "backupPath": "/workspaces/WorkshopTest/.github/instructions/csharp.instructions.md.20251207103000.backup",
  "bytesWritten": 4096,
  "message": "Merged 1 baseline + 2 guidelines, created backup"
}
```

**Errors**:
- `InvalidParams`: If `projectPath` doesn't exist, `technology` is empty, or both `baseline` and `guidelines` are null/empty
- `InternalError`: If file cannot be written or merge fails

**Notes**:
- Creates `.github/instructions/` directory if it doesn't exist
- Merges baseline and guidelines using deduplication algorithm (company overrides baseline)
- If file exists and `updateExisting=false`, returns status `Skipped`
- Warns if both baseline and guidelines are missing

---

### 7. refresh-cache

Manually refreshes the awesome-copilot repository cache.

**Method Signature**:
```csharp
[McpServerTool("refresh-cache")]
[Description("Manually refreshes the cache of awesome-copilot repository structure, useful after repository updates")]
public async Task<string> RefreshCache(
    CancellationToken cancellationToken = default)
```

**Input**: None

**Output**: Success message (string)
```json
"Cache refreshed successfully. Found 15 baseline instruction files."
```

**Errors**:
- `InternalError`: If GitHub API call fails

**Notes**:
- Forces immediate cache refresh regardless of TTL
- Returns count of discovered baseline files

---

## Prompts

### setup-copilot-instructions

Agent-mode prompt that orchestrates the entire setup workflow.

**Method Signature**:
```csharp
[McpServerPrompt("setup-copilot-instructions")]
[Description("Orchestrates the complete workflow to analyze a project and set up GitHub Copilot instruction files with baselines and company guidelines")]
public async Task<string> SetupCopilotInstructions(
    [Description("Absolute path to the project directory to set up")]
    string projectPath,
    
    [Description("Whether to update existing instruction files (true) or skip if they exist (false)")]
    bool updateExisting = false,
    
    CancellationToken cancellationToken = default)
```

**Input**:
- `projectPath` (string, required): Absolute path to project
- `updateExisting` (bool, optional, default: false): Update existing files

**Output**: Workflow instructions for the agent (string)

**Workflow Steps** (defined in prompt response):
1. Call `list-available-technologies` to get supported technologies
2. Call `detect-project-technologies` with `projectPath`
3. If no technologies detected, prompt user to select from available list
4. For each detected technology:
   - Call `get-baseline-instruction` with technology name
   - Call `get-company-guideline` with technology name
5. Call `create-repository-instruction-file` with detected technologies
6. For each technology, call `create-technology-instruction-file` with baseline and guidelines
7. Collect all results and generate summary report with:
   - Technologies covered
   - Files created/updated/skipped
   - Warnings (missing baselines, network errors)
   - Next steps for user

**Notes**:
- Runs in agent mode - AI assistant invokes tools autonomously
- Handles user interaction for ambiguous technology detection
- Provides detailed progress and error reporting

---

## Error Codes

Standard MCP error codes used by all tools:

| Code | Name | Usage |
|------|------|-------|
| -32600 | InvalidRequest | Malformed tool invocation |
| -32602 | InvalidParams | Invalid parameter values (wrong type, missing required, out of range) |
| -32603 | InternalError | Server-side failures (file I/O, network errors, GitHub API failures) |

---

## Rate Limiting

**GitHub API**:
- Unauthenticated: 60 requests/hour
- Authenticated: 5000 requests/hour
- Caching mitigates rate limiting for repeated technology lookups

**MCP Tools**:
- No artificial rate limiting
- Performance constrained by file I/O and network calls

---

## Versioning

**Contract Version**: 1.0  
**MCP Protocol Version**: 2024-11-05  
**Breaking Changes**: None (initial version)

---

**Contracts Complete** ✅  
All MCP tools and prompts fully specified with inputs, outputs, and error handling.
