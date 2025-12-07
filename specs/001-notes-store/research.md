# Research: In-Memory Notes Store

**Feature Branch**: `001-notes-store`  
**Created**: December 7, 2025  
**Phase**: Phase 0 - Outline & Research

## Purpose

Document technical decisions, research findings, and rationale for implementation choices. This resolves all "NEEDS CLARIFICATION" items from Technical Context and validates technology choices against feature requirements.

## Research Topics

### 1. ID Generation Strategy

**Decision**: Use 8-character prefix from GUID (e.g., "abc12345")

**Rationale**:
- GUIDs provide uniqueness without collision risk
- 8 characters provide sufficient uniqueness for workshop scope (16^8 = 4.3 billion combinations)
- Shorter IDs are user-friendly and easier to reference in chat interfaces
- .NET's `Guid.NewGuid().ToString("N")[..8]` provides simple implementation
- No database sequence needed (in-memory only)

**Alternatives Considered**:
- **Full GUID (32 chars)**: Too verbose for user-facing tool responses, harder to read in chat
- **Sequential integers**: Requires thread-safe counter, predictable IDs less secure, but simpler
- **NanoID or similar**: External dependency violates workshop simplicity principle

**Implementation**:
```csharp
string GenerateId() => Guid.NewGuid().ToString("N")[..8];
```

---

### 2. Thread-Safe Dictionary Access

**Decision**: Use `ConcurrentDictionary<string, NoteItem>` for thread-safe in-memory storage

**Rationale**:
- ASP.NET Core handles concurrent requests, dictionary access must be thread-safe
- `ConcurrentDictionary<TKey, TValue>` provides lock-free reads and atomic updates
- No manual locking code needed (simpler than `Dictionary + lock`)
- Standard .NET collection, no external dependencies
- Excellent performance for read-heavy workloads (list, search, get operations)

**Alternatives Considered**:
- **Dictionary + lock**: More verbose, requires careful lock management, error-prone
- **ReaderWriterLockSlim**: Over-engineered for workshop scope, adds complexity
- **Immutable collections**: Requires replacing entire dictionary on updates, memory overhead

**Implementation**:
```csharp
private readonly ConcurrentDictionary<string, NoteItem> _notes = new();
```

---

### 3. Note Data Model

**Decision**: Use C# 13 record type with positional parameters

**Record Definition**:
```csharp
public record NoteItem(
    string Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

**Rationale**:
- Records provide immutability by default (value equality, with-expressions for updates)
- Positional syntax is concise and readable
- `DateTime` for timestamps (UTC recommended in implementation)
- `UpdatedAt` is nullable to distinguish "never updated" from "updated at same time as created"
- No validation logic in model (simplicity principle - validate in service layer)

**Alternatives Considered**:
- **Class with properties**: More verbose, mutable by default, requires manual equality
- **Struct**: Value type semantics inappropriate for reference-stored entities
- **DateTimeOffset**: More correct for distributed systems, but overkill for workshop scope

**Update Pattern**:
```csharp
var updated = existingNote with { Title = newTitle, UpdatedAt = DateTime.UtcNow };
```

---

### 4. Search Implementation

**Decision**: LINQ with case-insensitive `Contains` for simple in-memory search

**Rationale**:
- Requirement: Search title and content, case-insensitive
- LINQ provides readable, functional approach: `.Where(n => n.Title.Contains(query, OrdinalIgnoreCase) || n.Content.Contains(query, OrdinalIgnoreCase))`
- `StringComparison.OrdinalIgnoreCase` for case-insensitive matching
- Performance acceptable for <10,000 notes (per success criteria)
- No indexing needed for workshop scope

**Alternatives Considered**:
- **Full-text search library (Lucene.NET)**: Massive overkill, external dependency, violates simplicity
- **Regex**: More powerful but slower and more complex than needed
- **ToLower() comparison**: Works but less efficient than OrdinalIgnoreCase

**Implementation**:
```csharp
public IEnumerable<NoteItem> Search(string query)
{
    return _notes.Values
        .Where(n => n.Title.Contains(query, StringComparison.OrdinalIgnoreCase) 
                 || n.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(n => n.CreatedAt);
}
```

---

### 5. MCP Tool Return Types

**Decision**: Return formatted strings with emojis for all tools

**Rationale**:
- Constitution Principle IV: User-friendly tool responses with visual indicators
- Strings are directly displayable in chat interfaces (GitHub Copilot, Claude Desktop)
- JSON objects require LLM to format for display (extra token cost, slower)
- Emojis provide visual status indicators (✅ ❌ 📝 🔍)

**Response Patterns**:
- **Success**: "Added note 'Meeting Ideas' with ID abc12345 ✅"
- **Error**: "Note with ID 'xyz999' not found ❌"
- **List**: Numbered list with titles, IDs, timestamps
- **Search**: "Found 3 notes matching 'roadmap' 🔍" + formatted list

**Alternatives Considered**:
- **Return NoteItem objects**: Requires LLM to format, less user-friendly
- **Return JSON strings**: Valid but less readable than natural language
- **Return complex DTOs**: Over-engineered for workshop scope

---

### 6. MCP Prompt Design Patterns

**Decision**: Prompts return structured templates as formatted strings

**Prompt Types**:

1. **QuickNote**: Capture thought with auto-generated title
   - Takes: content only
   - Returns: Suggested title + prompt to confirm/edit

2. **SummarizeNotes**: Overview of all notes
   - Takes: no parameters
   - Returns: Count, recent notes preview, topics mentioned

3. **FindTopicNotes**: Search by topic with context
   - Takes: topic string
   - Returns: Search results with relevance context

4. **MeetingNotes**: Structured meeting template
   - Takes: meeting name (optional)
   - Returns: Template with fields for date, attendees, agenda, discussion, actions

5. **CodeReviewNote**: Structured code review template
   - Takes: file/component name
   - Returns: Template with fields for reviewer, findings, recommendations

**Rationale**:
- Prompts guide users through structured note creation
- Templates provide consistency for common use cases
- Auto-title generation (QuickNote) uses simple heuristics (first 5 words, or first sentence)
- All prompts can call NotesTools to create notes after gathering information

**Implementation Approach**:
```csharp
[McpServerPrompt]
[Description("Quickly capture a thought with auto-generated title")]
public string QuickNote(
    [Description("The content to capture")] string content)
{
    // Generate title from first 5 words or first sentence
    var title = GenerateTitle(content);
    return $"I'll create a note with:\nTitle: {title}\nContent: {content}\n\nWould you like to proceed or change the title?";
}
```

---

### 7. Error Handling Strategy

**Decision**: Use `McpProtocolException` with appropriate error codes for MCP-level errors

**Error Categories**:

1. **Not Found** (FR-012): Note ID doesn't exist
   - Throw `McpProtocolException` with `McpErrorCode.InvalidParams`
   - Message: "Note with ID '{id}' not found ❌"

2. **Invalid Input**: Empty title/content, invalid search query
   - Validate in tool methods before calling service
   - Throw `McpProtocolException` with `McpErrorCode.InvalidParams`
   - Message: Clear explanation of what's invalid

3. **Service Errors**: Unexpected dictionary operations
   - Catch and wrap as `McpProtocolException` with `McpErrorCode.InternalError`
   - Log to stderr for debugging

**Rationale**:
- `McpProtocolException` is the MCP SDK's expected exception type
- Error codes help clients understand error category
- Clear messages make debugging easier for workshop participants
- Logging to stderr doesn't interfere with stdio transport

**Implementation**:
```csharp
public string GetNote(string id)
{
    if (string.IsNullOrWhiteSpace(id))
        throw new McpProtocolException(McpErrorCode.InvalidParams, "Note ID cannot be empty");
    
    if (!_notes.TryGetValue(id, out var note))
        throw new McpProtocolException(McpErrorCode.InvalidParams, $"Note with ID '{id}' not found ❌");
    
    return FormatNote(note);
}
```

---

### 8. Dependency Injection Configuration

**Decision**: Register NotesService as Singleton

**Registration in Program.cs**:
```csharp
builder.Services.AddSingleton<NotesService>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<NotesTools>()
    .WithPrompts<NotesPrompts>();
```

**Rationale**:
- In-memory storage requires singleton lifetime (single shared instance)
- Scoped would create new service per request (losing notes between calls)
- Transient would create new service per injection (completely broken)
- NotesService has no per-request state, safe as singleton

**Injection Pattern**:
```csharp
// Tools and Prompts use parameter injection
[McpServerTool]
public string AddNote(
    NotesService notesService,  // Injected by MCP SDK
    string title,
    string content)
{
    return notesService.AddNote(title, content);
}
```

---

## Best Practices Validation

### .NET 10 & C# 13 Features
✅ Record types for immutable data  
✅ Nullable reference types enabled  
✅ String interpolation for formatting  
✅ Pattern matching for validation  
✅ LINQ for collection operations  
✅ Collection expressions (e.g., `[]` for empty arrays)

### MCP SDK Integration
✅ `[McpServerTool]` attributes  
✅ `[McpServerPrompt]` attributes  
✅ `[Description]` attributes on all tools, prompts, and parameters  
✅ Builder pattern with `AddMcpServer()`, `WithTools<T>()`, `WithPrompts<T>()`  
✅ Both stdio and HTTP transport support

### Workshop Simplicity
✅ No external dependencies beyond MCP SDK  
✅ Single service class (NotesService)  
✅ Standard collections (ConcurrentDictionary)  
✅ No complex patterns (no repository, no CQRS, no event sourcing)  
✅ Clear code structure for educational purposes

---

## Performance Validation

**Success Criteria Check**:

| Criterion | Target | Implementation | Status |
|-----------|--------|----------------|--------|
| SC-001 | Create + retrieve < 2s | Dictionary add + lookup O(1) | ✅ Pass |
| SC-002 | Search < 1s for 10K notes | LINQ Where O(n), acceptable for 10K | ✅ Pass |
| SC-003 | List < 1s for 10K notes | Values + OrderBy O(n log n), acceptable | ✅ Pass |
| SC-004 | Update < 1s, preserve timestamps | Dictionary update O(1) + with-expression | ✅ Pass |
| SC-009 | Thread-safe concurrent access | ConcurrentDictionary lock-free | ✅ Pass |

**Memory Estimation**:
- NoteItem: ~200 bytes (strings + DateTime + overhead)
- 10,000 notes: ~2 MB + dictionary overhead ~4 MB = ~6 MB total
- Well under 100 MB constraint ✅

---

## Technology Stack Summary

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| Runtime | .NET | 10.0 | Latest framework, C# 13 support |
| MCP SDK | ModelContextProtocol | 0.5.0-preview.1 | Official MCP implementation |
| MCP Transport | ModelContextProtocol.AspNetCore | 0.5.0-preview.1 | HTTP + stdio support |
| Hosting | ASP.NET Core | 10.0 (built-in) | Minimal API hosting |
| Storage | ConcurrentDictionary | Built-in | Thread-safe in-memory |
| ID Generation | GUID | Built-in | Unique 8-char prefixes |
| Search | LINQ | Built-in | Case-insensitive contains |

**Zero external dependencies** beyond what's already in SampleMcpServer.csproj ✅

---

## Open Questions / Future Enhancements

### Out of Scope (Workshop Simplicity)
❌ Persistence (database, files) - In-memory only per requirements  
❌ Tags/categories - Not in functional requirements  
❌ Note sharing/collaboration - Not in functional requirements  
❌ Full-text search indexing - LINQ sufficient for scope  
❌ Note versioning/history - Not in functional requirements  
❌ Authentication/authorization - Single-user workshop scope  

### Edge Cases Acknowledged
⚠️ **Memory limits**: No enforcement - trust OS memory limits  
⚠️ **Large content (10MB)**: No size validation - accept as-is  
⚠️ **Empty title/content**: Will validate and return clear error  
⚠️ **Special characters in search**: String.Contains handles naturally  
⚠️ **Very long search queries**: No length limit, acceptable performance  
⚠️ **Concurrent operations**: ConcurrentDictionary handles safely  

---

## Research Complete ✅

All technical decisions documented. All NEEDS CLARIFICATION items resolved. Ready to proceed to Phase 1: Design.
