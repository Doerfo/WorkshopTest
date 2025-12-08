# Phase 0: Research - Copilot Instructions Setup MCP Server

**Date**: December 7, 2025  
**Branch**: `001-copilot-instructions-setup`

## Research Tasks

This document consolidates research findings to resolve all technical unknowns identified in the Technical Context section of [plan.md](plan.md).

---

## 1. Technology Detection Patterns

**Research Question**: What are the reliable indicators for detecting mainstream technologies in project structures?

### Decision: File-based pattern matching with multiple indicators per technology

### Rationale:
- Different technologies have distinct, recognizable file patterns
- Multiple indicators increase detection accuracy (avoid false positives)
- Standard project structures are consistent across the industry
- File existence checks are fast and reliable

### Implementation Approach:

| Technology | Primary Indicators | Secondary Indicators |
|------------|-------------------|---------------------|
| **C#** | `*.csproj`, `*.cs` files | `*.sln`, `global.json` |
| **TypeScript** | `tsconfig.json`, `*.ts` files | `package.json` with typescript dependency |
| **JavaScript** | `package.json`, `*.js` files | `.eslintrc`, `webpack.config.js` |
| **Python** | `requirements.txt`, `*.py` files | `pyproject.toml`, `setup.py`, `Pipfile` |
| **Java** | `pom.xml`, `build.gradle`, `*.java` | `settings.gradle`, `gradlew` |
| **React** | `package.json` with react dependency | `*.jsx`, `*.tsx` files |
| **Angular** | `angular.json`, `package.json` with @angular/* | `*.component.ts` files |
| **Vue** | `package.json` with vue dependency | `*.vue` files, `vue.config.js` |
| **.NET** | `*.csproj` with SDK="Microsoft.NET.Sdk" | `global.json` with .NET version |
| **Node.js** | `package.json` with "main" or "bin" | `index.js`, `server.js` |
| **Go** | `go.mod`, `*.go` files | `go.sum` |
| **Rust** | `Cargo.toml`, `*.rs` files | `Cargo.lock` |

### Alternatives Considered:
- **File content analysis**: Too slow, requires parsing
- **Git history analysis**: Unreliable, not all projects use git
- **Manual user specification**: Defeats automation purpose
- **AI-based detection**: Too complex, unnecessary for well-defined patterns

---

## 2. GitHub Awesome-Copilot Repository Structure

**Research Question**: How is the awesome-copilot repository organized, and what's the best way to retrieve baseline instructions?

### Decision: Use GitHub REST API with pattern-based discovery and local caching

### Rationale:
- GitHub REST API provides reliable, structured access to repository contents
- Caching repository structure minimizes API calls and improves performance
- Pattern-based discovery handles repository reorganizations gracefully
- API responses include file paths, SHA hashes for cache validation

### Repository Structure (as of December 2025):
```
github/awesome-copilot/
└── instructions/
    ├── README.md
    ├── csharp.instructions.md
    ├── typescript.instructions.md
    ├── javascript.instructions.md
    ├── python.instructions.md
    ├── java.instructions.md
    ├── react.instructions.md
    ├── angular.instructions.md
    ├── vue.instructions.md
    ├── golang.instructions.md
    ├── rust.instructions.md
    └── [additional technologies]
```

### Implementation Approach:
1. **Initial Cache**: On first run, fetch directory listing from `GET /repos/github/awesome-copilot/contents/instructions`
2. **Parse Response**: Extract file names matching pattern `*.instructions.md`
3. **Store Mapping**: Cache technology name → file path (e.g., "csharp" → "instructions/csharp.instructions.md")
4. **Retrieve Content**: Use `GET /repos/github/awesome-copilot/contents/{path}` to fetch specific instruction files
5. **Cache Duration**: Cache structure for 24 hours, refresh on startup or manual request
6. **Pattern Discovery**: If expected file not found, search directory for similar patterns

### GitHub API Endpoints:
- **List directory**: `https://api.github.com/repos/github/awesome-copilot/contents/instructions`
- **Get file**: `https://api.github.com/repos/github/awesome-copilot/contents/instructions/{filename}.instructions.md`
- **Rate Limiting**: 60 requests/hour (unauthenticated), 5000/hour (authenticated)
- **Response Format**: JSON with `name`, `path`, `download_url`, `sha` fields

### Alternatives Considered:
- **Git clone entire repository**: Too heavy, unnecessary for read-only access
- **Static bundled instructions**: Becomes outdated, defeats purpose of baseline retrieval
- **Web scraping**: Fragile, violates best practices
- **GraphQL API**: More complex, REST API sufficient for this use case

---

## 3. Company Guideline Storage and Management

**Research Question**: How should company-specific guidelines be stored and accessed within the MCP server?

### Decision: Markdown files in `Guidelines/` directory with technology-name-based naming convention

### Rationale:
- Markdown is human-readable and easy to edit
- File-based storage requires no external dependencies
- Technology-name prefix enables automatic matching
- Version control friendly (can be committed to git)
- Simple to add, modify, or remove guidelines

### Naming Convention:
```
Guidelines/
├── csharp-testing.md          # C# testing guidelines
├── csharp-security.md         # C# security guidelines
├── typescript.md              # General TypeScript guidelines
├── angular.md                 # Angular-specific guidelines
├── react-hooks.md             # React hooks best practices
└── python-type-hints.md       # Python type hinting guidelines
```

**Pattern**: `{technology}[-{aspect}].md`
- Technology name matches detection results (lowercase)
- Optional aspect suffix for multiple guidelines per technology
- All guidelines for a technology are merged during instruction generation

### File Format:
```markdown
---
description: Company-specific C# testing guidelines
technology: csharp
aspect: testing
---

# C# Testing Guidelines

[Guideline content...]
```

### Discovery and Loading:
1. **Scan Guidelines directory** for `*.md` files on service startup
2. **Parse frontmatter** to extract technology and aspect metadata
3. **Index by technology**: Create mapping of technology → list of guideline files
4. **Load on demand**: Read file content when generating instructions for specific technology
5. **Hot reload**: Watch directory for changes (optional, for development)

### Alternatives Considered:
- **Database storage**: Overkill, adds unnecessary dependency
- **JSON/YAML files**: Less human-readable than markdown
- **Embedded resources**: Harder to update without recompiling
- **External service**: Adds network dependency and complexity

---

## 4. Content Merging and Deduplication Strategy

**Research Question**: How should baseline instructions and company guidelines be merged with automatic deduplication?

### Decision: Section-based merging with semantic deduplication using content hashing

### Rationale:
- Instruction files are structured with markdown sections (headers)
- Section-level merging preserves organization
- Content hashing detects duplicates regardless of formatting differences
- Company guidelines override baseline (per clarification)
- Maintains readability and structure of final output

### Merging Algorithm:

```
1. Parse baseline instruction into sections (split by ## headers)
2. Parse company guidelines into sections
3. Create section map: header → content
4. For each section in baseline:
   a. Calculate content hash (normalized text, ignore whitespace)
   b. Check if company guideline has section with same header
   c. If yes: Replace baseline section with company section (company override)
   d. If no: Check content hash against all company sections
   e. If hash match found: Skip baseline section (duplicate)
   f. If no match: Include baseline section
5. Add all company guideline sections not yet included
6. Reconstruct markdown with frontmatter + merged sections
```

### Content Normalization for Hashing:
- Convert to lowercase
- Remove extra whitespace (collapse multiple spaces/newlines)
- Remove markdown formatting characters (**, *, `, etc.)
- Remove code block delimiters (```)
- Trim leading/trailing whitespace
- Hash using SHA256

### Section Detection:
- **Level 2 headers** (`##`) define sections
- **Level 1 header** (`#`) is file title (preserve from baseline or company)
- **Frontmatter** (YAML) merged separately (company overrides baseline)

### Output Structure:
```markdown
---
description: [Company description if provided, else baseline description]
applyTo: [Merged glob patterns from both sources]
technology: [Technology name]
---

# [Title from company guideline if provided, else baseline]

## Section 1
[Content - company version if overlap, else baseline]

## Section 2
[Content - only from company if unique]

## Section 3
[Content - only from baseline if unique]
```

### Alternatives Considered:
- **Line-by-line diff**: Too granular, loses context
- **Full replacement**: Loses valuable baseline content
- **Manual merge markers**: Requires user intervention
- **AI-based semantic merging**: Overcomplicated, introduces uncertainty

---

## 5. MCP Agent-Mode Prompt Design

**Research Question**: How should the setup prompt be structured to run in agent mode and orchestrate multiple tools?

### Decision: Multi-turn conversational prompt with clear workflow steps and tool invocation guidance

### Rationale:
- Agent mode allows prompt to invoke tools autonomously
- Clear workflow steps guide the LLM through complex process
- Tool descriptions enable correct parameter passing
- Conversation allows user interaction for ambiguous cases (technology selection)
- Structured approach ensures consistent execution

### Prompt Structure:

```csharp
[McpServerPrompt("setup-copilot-instructions")]
[Description("Analyzes a project and sets up GitHub Copilot instruction files with baseline from awesome-copilot and company guidelines")]
public async Task<string> SetupCopilotInstructions(
    [Description("Absolute path to the project directory to analyze")]
    string projectPath,
    
    [Description("Whether to update existing instruction files (true) or skip if they exist (false)")]
    bool updateExisting = false,
    
    CancellationToken cancellationToken = default)
{
    // Prompt returns instructions for the agent to follow
    return """
    You are setting up GitHub Copilot instruction files for a project. Follow these steps:
    
    1. DETECT TECHNOLOGIES
       - Invoke 'list-available-technologies' to see what the MCP server supports
       - Invoke 'detect-project-technologies' with projectPath='{projectPath}'
       - If no technologies detected, ask user to select from available list
    
    2. RETRIEVE CONTENT
       - For each detected technology:
         - Invoke 'get-baseline-instruction' with technology name
         - Invoke 'get-company-guideline' with technology name
    
    3. CHECK EXISTING FILES
       - Check if {projectPath}/.github/copilot-instructions.md exists
       - Check if {projectPath}/.github/instructions/*.instructions.md exist
       - If exists and updateExisting=false, warn and skip
       - If exists and updateExisting=true, proceed with backup
    
    4. GENERATE INSTRUCTIONS
       - Invoke 'create-repository-instruction-file' with merged content
       - For each technology, invoke 'create-technology-instruction-file'
    
    5. REPORT RESULTS
       - List files created/updated
       - List technologies covered
       - Note any warnings (missing baselines, skipped files)
    """;
}
```

### Tool Invocation Pattern:
Each tool is independently invocable, and the agent orchestrates them based on the prompt workflow:

1. **list-available-technologies**: Returns list of supported technologies
2. **detect-project-technologies**: Analyzes project directory, returns detected technologies
3. **get-baseline-instruction**: Fetches baseline from awesome-copilot for one technology
4. **get-company-guideline**: Retrieves company guideline for one technology
5. **create-repository-instruction-file**: Writes `.github/copilot-instructions.md`
6. **create-technology-instruction-file**: Writes `.github/instructions/{technology}.instructions.md`

### Alternatives Considered:
- **Single monolithic tool**: Less flexible, harder to test individual components
- **Synchronous workflow**: Blocks on long operations (GitHub API calls)
- **No agent mode**: Requires user to manually invoke each tool in sequence
- **Scripted workflow**: Less adaptive to errors or user preferences

---

## 6. Error Handling and Fallback Strategies

**Research Question**: How should the system handle various failure scenarios?

### Decision: Graceful degradation with user warnings and partial success

### Rationale:
- Network failures shouldn't completely block setup
- Missing baselines shouldn't prevent using company guidelines
- Partial success is better than total failure
- Users should be informed of limitations clearly

### Error Scenarios and Handling:

| Scenario | Handling Strategy |
|----------|------------------|
| **GitHub API unavailable** | Use cached baselines if available; warn user; continue with guidelines only |
| **Technology not in awesome-copilot** | Warn user; create instruction file with guidelines only; suggest manual baseline |
| **Company guideline not found** | Continue with baseline only; log info message |
| **Both baseline and guideline missing** | Create minimal instruction file with technology name; warn user to customize |
| **Project path doesn't exist** | Throw `McpProtocolException` with `InvalidParams` error code |
| **No write permission** | Throw `McpProtocolException` with `InternalError` and clear message |
| **Existing file, updateExisting=false** | Skip file creation; warn in summary report |
| **GitHub rate limit exceeded** | Use cached baselines; warn user; retry after rate limit reset |
| **Malformed guideline file** | Log warning; skip that guideline; continue with others |
| **Merge conflict (rare)** | Apply company-first rule; log the override |

### Logging Strategy:
- **Trace**: Detailed operation steps (for debugging)
- **Debug**: Tool invocations, file operations
- **Information**: Successful completions, cache hits
- **Warning**: Fallback scenarios, skipped operations, missing content
- **Error**: Failures requiring user intervention, exceptions

### Alternatives Considered:
- **Fail-fast approach**: Poor user experience, blocks partial success
- **Silent failures**: Users unaware of issues
- **Retry with exponential backoff**: Overkill for GitHub API (has rate limit headers)
- **Queue failed operations**: Adds complexity without clear benefit

---

## 7. Caching Strategy for Awesome-Copilot Repository

**Research Question**: What caching approach balances freshness with performance?

### Decision: In-memory cache with 24-hour TTL and startup refresh

### Rationale:
- Repository structure changes infrequently (weeks/months)
- In-memory cache is fast and simple
- 24-hour TTL balances freshness and performance
- Startup refresh ensures recent data after server restart
- No external dependencies (Redis, etc.)

### Cache Implementation:

```csharp
public class AwesomeCopilotCacheService : IAwesomeCopilotCacheService
{
    private readonly record struct CacheEntry(
        Dictionary<string, string> TechnologyFiles, // tech name -> file path
        DateTimeOffset CachedAt
    );
    
    private CacheEntry? _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
    
    public async Task<Dictionary<string, string>> GetTechnologyFilesAsync(
        CancellationToken cancellationToken)
    {
        if (_cache is { } cache && 
            DateTimeOffset.UtcNow - cache.CachedAt < _cacheDuration)
        {
            return cache.TechnologyFiles; // Cache hit
        }
        
        // Cache miss or expired - refresh from GitHub
        var files = await FetchFromGitHubAsync(cancellationToken);
        _cache = new CacheEntry(files, DateTimeOffset.UtcNow);
        return files;
    }
}
```

### Cache Invalidation:
- **Time-based**: 24-hour TTL
- **Manual**: Expose tool to force cache refresh if needed
- **On error**: Don't invalidate cache on GitHub error (use stale cache)

### Persistence:
- **Not persisted**: Cache is in-memory only
- **Rationale**: Simple implementation, 24-hour refresh is acceptable
- **Future enhancement**: Could add optional file-based persistence

### Alternatives Considered:
- **No caching**: Too many GitHub API calls, poor performance
- **Permanent cache**: Baselines become stale
- **File-based cache**: Adds file I/O complexity
- **Redis/external cache**: Overkill, adds infrastructure dependency

---

## Summary of Decisions

| Topic | Decision | Key Rationale |
|-------|----------|---------------|
| Technology Detection | File-based pattern matching | Fast, reliable, industry-standard patterns |
| GitHub Access | REST API with caching | Reliable, structured, handles rate limits |
| Guideline Storage | Markdown files in Guidelines/ | Human-readable, version-control friendly |
| Content Merging | Section-based with content hashing | Preserves structure, detects duplicates |
| Agent Prompt | Multi-turn workflow with tool orchestration | Flexible, handles user interaction |
| Error Handling | Graceful degradation with warnings | Partial success better than total failure |
| Caching | In-memory with 24-hour TTL | Simple, fast, acceptable freshness |

**All NEEDS CLARIFICATION items from Technical Context have been resolved.**

---

**Phase 0 Complete** ✅  
Ready to proceed to Phase 1: Design & Contracts
