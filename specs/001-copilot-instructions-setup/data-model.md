# Phase 1: Data Model - Copilot Instructions Setup MCP Server

**Date**: December 7, 2025  
**Branch**: `001-copilot-instructions-setup`

This document defines the data structures, entities, and their relationships for the Copilot Instructions Setup feature.

---

## Core Entities

### 1. TechnologyInfo

Represents metadata about a detected or supported technology.

**Properties**:
- `Name` (string): Technology identifier (lowercase, e.g., "csharp", "typescript", "react")
- `DisplayName` (string): Human-readable name (e.g., "C#", "TypeScript", "React")
- `HasBaseline` (bool): Whether awesome-copilot repository has baseline for this technology
- `HasGuideline` (bool): Whether company has guideline file for this technology
- `DetectionConfidence` (string?): Confidence level for detected technologies ("High", "Medium", "Low")

**C# Implementation**:
```csharp
public record TechnologyInfo(
    string Name,
    string DisplayName,
    bool HasBaseline,
    bool HasGuideline,
    string? DetectionConfidence = null
);
```

**Validation Rules**:
- `Name` must be non-empty, lowercase, alphanumeric (with hyphens allowed)
- `DisplayName` must be non-empty
- `DetectionConfidence` must be one of: "High", "Medium", "Low", or null

**Relationships**:
- One-to-many with `GuidelineContent` (one technology can have multiple guideline files)
- One-to-one with `BaselineInstruction` (one baseline per technology)

---

### 2. GuidelineContent

Represents content from a company guideline file.

**Properties**:
- `Technology` (string): Technology this guideline applies to
- `Aspect` (string?): Specific aspect of technology (e.g., "testing", "security"), null for general guidelines
- `FilePath` (string): Absolute path to the guideline markdown file
- `Content` (string): Full markdown content of the guideline
- `Frontmatter` (Dictionary<string, string>?): YAML frontmatter metadata if present

**C# Implementation**:
```csharp
public record GuidelineContent(
    string Technology,
    string? Aspect,
    string FilePath,
    string Content,
    Dictionary<string, string>? Frontmatter = null
);
```

**Validation Rules**:
- `Technology` must match a valid technology name
- `FilePath` must exist and be readable
- `Content` must be valid markdown
- `Frontmatter` if present, must be valid YAML

**Relationships**:
- Many-to-one with `TechnologyInfo` (multiple guidelines per technology)
- Used by `InstructionMergeService` to create merged instructions

---

### 3. BaselineInstruction

Represents baseline instruction content from the awesome-copilot repository.

**Properties**:
- `Technology` (string): Technology this baseline applies to
- `FileName` (string): Original filename in awesome-copilot repo (e.g., "csharp.instructions.md")
- `Content` (string): Full markdown content
- `SourceUrl` (string): GitHub raw content URL
- `RetrievedAt` (DateTimeOffset): When this baseline was fetched
- `Sha` (string?): Git SHA hash for cache validation

**C# Implementation**:
```csharp
public record BaselineInstruction(
    string Technology,
    string FileName,
    string Content,
    string SourceUrl,
    DateTimeOffset RetrievedAt,
    string? Sha = null
);
```

**Validation Rules**:
- `Technology` must match filename (e.g., "csharp" matches "csharp.instructions.md")
- `Content` must be non-empty markdown
- `SourceUrl` must be valid GitHub raw content URL
- `Sha` if present, must be 40-character hex string (Git SHA-1)

**Relationships**:
- One-to-one with `TechnologyInfo`
- Source for `InstructionMergeService`

---

### 4. InstructionFileResult

Represents the result of creating or updating an instruction file.

**Properties**:
- `FilePath` (string): Absolute path where instruction file was written
- `Technology` (string?): Technology name (null for repository-wide file)
- `Status` (FileOperationStatus): Status of the operation
- `BackupPath` (string?): Path to backup file if existing file was updated
- `BytesWritten` (long): Size of written file in bytes
- `Message` (string?): Human-readable status message or warning

**C# Implementation**:
```csharp
public enum FileOperationStatus
{
    Created,
    Updated,
    Skipped,
    Failed
}

public record InstructionFileResult(
    string FilePath,
    string? Technology,
    FileOperationStatus Status,
    string? BackupPath = null,
    long BytesWritten = 0,
    string? Message = null
);
```

**Validation Rules**:
- `FilePath` must be absolute path
- `Technology` must be null for repository-wide file, non-empty for technology-specific
- `BackupPath` if present, must exist if Status is Updated
- `BytesWritten` must be >= 0

**Relationships**:
- Output from `InstructionFileTools`
- Collected in `SetupSummary` for reporting

---

### 5. SetupSummary

Aggregates results from the entire setup process.

**Properties**:
- `ProjectPath` (string): Path to project that was analyzed
- `DetectedTechnologies` (List<TechnologyInfo>): All technologies found/selected
- `FileResults` (List<InstructionFileResult>): All file operations performed
- `Warnings` (List<string>): Non-fatal issues encountered
- `ExecutionTime` (TimeSpan): Total time taken for setup
- `CompletedAt` (DateTimeOffset): When setup finished

**C# Implementation**:
```csharp
public record SetupSummary(
    string ProjectPath,
    List<TechnologyInfo> DetectedTechnologies,
    List<InstructionFileResult> FileResults,
    List<string> Warnings,
    TimeSpan ExecutionTime,
    DateTimeOffset CompletedAt
)
{
    public int FilesCreated => FileResults.Count(r => r.Status == FileOperationStatus.Created);
    public int FilesUpdated => FileResults.Count(r => r.Status == FileOperationStatus.Updated);
    public int FilesSkipped => FileResults.Count(r => r.Status == FileOperationStatus.Skipped);
    public int FilesFailed => FileResults.Count(r => r.Status == FileOperationStatus.Failed);
}
```

**Validation Rules**:
- `ProjectPath` must be absolute path
- `DetectedTechnologies` must not be empty
- `ExecutionTime` must be positive

**Relationships**:
- Aggregates multiple `InstructionFileResult` instances
- References multiple `TechnologyInfo` instances
- Returned by setup prompt as final result

---

### 6. MergedInstructionContent

Represents the final merged content before writing to file.

**Properties**:
- `Technology` (string): Technology name
- `Title` (string): File/section title
- `Description` (string): Description for frontmatter
- `ApplyToPatterns` (List<string>): Glob patterns for `applyTo` frontmatter
- `Sections` (List<InstructionSection>): Merged content sections
- `SourceSummary` (string): Human-readable summary of content sources

**C# Implementation**:
```csharp
public record InstructionSection(
    string Header,
    string Content,
    string Source // "baseline", "company-guideline", "both"
);

public record MergedInstructionContent(
    string Technology,
    string Title,
    string Description,
    List<string> ApplyToPatterns,
    List<InstructionSection> Sections,
    string SourceSummary
)
{
    public string ToMarkdown()
    {
        var sb = new StringBuilder();
        
        // Frontmatter
        sb.AppendLine("---");
        sb.AppendLine($"description: {Description}");
        sb.AppendLine($"applyTo: '{string.Join(",", ApplyToPatterns)}'");
        sb.AppendLine("---");
        sb.AppendLine();
        
        // Title
        sb.AppendLine($"# {Title}");
        sb.AppendLine();
        
        // Sections
        foreach (var section in Sections)
        {
            sb.AppendLine($"## {section.Header}");
            sb.AppendLine();
            sb.AppendLine(section.Content);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
}
```

**Validation Rules**:
- `Technology` must be valid technology name
- `Sections` must not be empty
- `ApplyToPatterns` must contain at least one valid glob pattern

**Relationships**:
- Produced by `InstructionMergeService`
- Consumed by `InstructionFileTools` to write files

---

### 7. TechnologyDetectionResult

Represents the output of project analysis.

**Properties**:
- `DetectedTechnologies` (List<TechnologyInfo>): Technologies found in project
- `Indicators` (Dictionary<string, List<string>>): Technology → list of detected indicators
- `AmbiguousTechnologies` (List<string>): Technologies with weak detection confidence
- `ProjectPath` (string): Path that was analyzed
- `AnalyzedAt` (DateTimeOffset): When analysis was performed

**C# Implementation**:
```csharp
public record TechnologyDetectionResult(
    List<TechnologyInfo> DetectedTechnologies,
    Dictionary<string, List<string>> Indicators,
    List<string> AmbiguousTechnologies,
    string ProjectPath,
    DateTimeOffset AnalyzedAt
);
```

**Validation Rules**:
- `ProjectPath` must exist and be a directory
- Each technology in `DetectedTechnologies` must have at least one indicator
- `AmbiguousTechnologies` must be subset of `DetectedTechnologies`

**Relationships**:
- Output from `TechnologyDetectionService`
- Input to guideline and baseline retrieval

---

## State Transitions

### File Operation Status Flow

```
Initial State
    ↓
[Check if file exists]
    ↓
┌─────────────┬──────────────┐
│ Not exists  │ Exists       │
↓             ↓              ↓
Create file   [Check updateExisting flag]
↓             ↓              ↓
Created       ┌──────────┬───────────┐
              │ true     │ false     │
              ↓          ↓           ↓
              Create     Skip        
              backup     operation   
              ↓                      ↓
              Update file            Skipped
              ↓
              Updated
```

### Setup Workflow Data Flow

```
1. Project Analysis
   Input: projectPath (string)
   Output: TechnologyDetectionResult
   
2. Technology Selection (if ambiguous)
   Input: TechnologyDetectionResult, available technologies
   Output: List<TechnologyInfo> (user-confirmed)
   
3. Content Retrieval (parallel per technology)
   Input: TechnologyInfo
   Output: BaselineInstruction?, GuidelineContent[]
   
4. Content Merging (per technology)
   Input: BaselineInstruction?, GuidelineContent[]
   Output: MergedInstructionContent
   
5. File Writing (per technology + repository-wide)
   Input: MergedInstructionContent, projectPath, updateExisting
   Output: InstructionFileResult
   
6. Summary Generation
   Input: All InstructionFileResult[], TechnologyInfo[], warnings
   Output: SetupSummary
```

---

## Validation & Constraints

### Cross-Entity Validation

1. **Technology Consistency**: 
   - Technology names must be consistent across all entities
   - Case-sensitive matching (all lowercase)

2. **File Path Integrity**:
   - All file paths must be absolute
   - Backup paths must be in same directory as original
   - Instruction files must be in `.github/` or `.github/instructions/`

3. **Content Integrity**:
   - Merged content must not exceed reasonable size (e.g., 100KB per file)
   - Markdown must be valid (basic syntax check)

4. **Timestamp Consistency**:
   - `RetrievedAt` must be <= `CompletedAt`
   - `AnalyzedAt` must be <= `CompletedAt`

### Business Rules

1. **One Repository-Wide File**: Only one `.github/copilot-instructions.md` per project
2. **One File Per Technology**: Only one `.github/instructions/{technology}.instructions.md` per technology
3. **Backup Naming**: Backups use format `{original-name}.{timestamp}.backup`
4. **Technology Names**: Must match awesome-copilot repository naming (lowercase, kebab-case)

---

## Summary

**Total Entities**: 7 core records
**Key Relationships**: Technology → Guidelines (1:many), Technology → Baseline (1:1)
**Validation Points**: File paths, technology names, content size, timestamp order
**State Management**: In-memory during execution, no persistent storage required

All entities use immutable records (C# record types) for thread safety and clarity.

**Phase 1: Data Model Complete** ✅  
Ready to proceed to contracts and quickstart documentation.
