# Quickstart Guide: In-Memory Notes Store

**Feature Branch**: `001-notes-store`  
**Created**: December 7, 2025  
**Audience**: Developers implementing the feature

## Overview

This guide provides step-by-step implementation instructions for the in-memory notes store feature. Follow these steps to add note management capabilities to the SampleMcpServer.

**What You'll Build**:
- In-memory note storage with CRUD operations
- 6 MCP tools for note management
- 5 MCP prompts for guided interactions
- Thread-safe concurrent access

**Time Estimate**: 2-3 hours

---

## Prerequisites

✅ .NET 10 SDK installed  
✅ SampleMcpServer project exists and builds successfully  
✅ Familiarity with C# records and dependency injection  
✅ Understanding of MCP tool/prompt attributes

**Verify Setup**:
```bash
cd /workspaces/WorkshopTest/SampleMcpServer
dotnet build
```

Expected: Build succeeds with no errors

---

## Step 1: Create the Note Model

**File**: `SampleMcpServer/Models/NoteItem.cs` (create new directory and file)

**What**: Define the data structure for notes

**Why**: Immutable record type ensures data integrity and provides value equality

**Implementation**:
```csharp
namespace SampleMcpServer.Models;

/// <summary>
/// Represents a single note with metadata.
/// </summary>
/// <param name="Id">Unique 8-character identifier</param>
/// <param name="Title">User-provided note title</param>
/// <param name="Content">User-provided note content</param>
/// <param name="CreatedAt">Timestamp when note was created (UTC)</param>
/// <param name="UpdatedAt">Timestamp when note was last updated (UTC), null if never updated</param>
public record NoteItem(
    string Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

**Key Points**:
- Record type provides immutability and value equality
- `UpdatedAt` is nullable (null = never updated)
- All timestamps should use UTC
- No validation logic in the model (handled by service)

---

## Step 2: Implement NotesService

**File**: `SampleMcpServer/Services/NotesService.cs` (create new directory and file)

**What**: Core business logic for note storage and operations

**Why**: Singleton service maintains in-memory state across all tool invocations

**Implementation**:

```csharp
using System.Collections.Concurrent;
using SampleMcpServer.Models;

namespace SampleMcpServer.Services;

/// <summary>
/// In-memory note storage service with thread-safe CRUD operations.
/// </summary>
public class NotesService
{
    private readonly ConcurrentDictionary<string, NoteItem> _notes = new();

    /// <summary>
    /// Generates a unique 8-character ID from a GUID.
    /// </summary>
    private static string GenerateId() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Adds a new note with auto-generated ID and timestamp.
    /// </summary>
    public NoteItem AddNote(string title, string content)
    {
        ValidateNotEmpty(title, nameof(title));
        ValidateNotEmpty(content, nameof(content));

        var note = new NoteItem(
            Id: GenerateId(),
            Title: title.Trim(),
            Content: content.Trim(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );

        _notes[note.Id] = note;
        return note;
    }

    /// <summary>
    /// Retrieves a note by ID.
    /// </summary>
    public NoteItem? GetNote(string id)
    {
        ValidateNotEmpty(id, "Note ID");
        return _notes.TryGetValue(id, out var note) ? note : null;
    }

    /// <summary>
    /// Lists all notes ordered by creation date (newest first).
    /// </summary>
    public IEnumerable<NoteItem> ListNotes()
    {
        return _notes.Values
            .OrderByDescending(n => n.CreatedAt);
    }

    /// <summary>
    /// Searches notes where title or content contains the query (case-insensitive).
    /// </summary>
    public IEnumerable<NoteItem> SearchNotes(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ListNotes();

        return _notes.Values
            .Where(n => n.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || n.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.CreatedAt);
    }

    /// <summary>
    /// Updates an existing note's title and/or content.
    /// </summary>
    public NoteItem? UpdateNote(string id, string? title = null, string? content = null)
    {
        ValidateNotEmpty(id, "Note ID");

        if (title is null && content is null)
            throw new ArgumentException("At least one of title or content must be provided");

        if (!_notes.TryGetValue(id, out var existing))
            return null;

        var updated = existing with
        {
            Title = title?.Trim() ?? existing.Title,
            Content = content?.Trim() ?? existing.Content,
            UpdatedAt = DateTime.UtcNow
        };

        if (title is not null) ValidateNotEmpty(title, nameof(title));
        if (content is not null) ValidateNotEmpty(content, nameof(content));

        _notes[id] = updated;
        return updated;
    }

    /// <summary>
    /// Deletes a note by ID.
    /// </summary>
    public bool DeleteNote(string id)
    {
        ValidateNotEmpty(id, "Note ID");
        return _notes.TryRemove(id, out _);
    }

    private static void ValidateNotEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
    }
}
```

**Key Points**:
- `ConcurrentDictionary` ensures thread-safety
- `GenerateId()` creates unique 8-character IDs
- Validation throws `ArgumentException` (will be caught by tools)
- `with` expressions create updated records immutably
- `TryGetValue`, `TryRemove` avoid exceptions for not found

---

## Step 3: Implement MCP Tools

**File**: `SampleMcpServer/Tools/NotesTools.cs` (create file in existing directory)

**What**: MCP tool implementations that wrap NotesService methods

**Why**: Expose service operations to AI assistants through MCP protocol

**Implementation**:

```csharp
using System.ComponentModel;
using ModelContextProtocol.Server;
using SampleMcpServer.Services;

namespace SampleMcpServer.Tools;

/// <summary>
/// MCP tools for note management operations.
/// </summary>
internal class NotesTools
{
    [McpServerTool]
    [Description("Creates a new note with auto-generated ID and timestamp")]
    public string AddNote(
        NotesService notesService,
        [Description("The title of the note")] string title,
        [Description("The content/body of the note")] string content)
    {
        try
        {
            var note = notesService.AddNote(title, content);
            return $"Added note '{note.Title}' with ID {note.Id} ✅";
        }
        catch (ArgumentException ex)
        {
            throw new McpProtocolException(McpErrorCode.InvalidParams, $"{ex.Message} ❌");
        }
    }

    [McpServerTool]
    [Description("Retrieves a specific note by its unique ID")]
    public string GetNote(
        NotesService notesService,
        [Description("The unique identifier of the note to retrieve")] string id)
    {
        try
        {
            var note = notesService.GetNote(id);
            if (note is null)
                throw new McpProtocolException(McpErrorCode.InvalidParams, $"Note with ID '{id}' not found ❌");

            return FormatNote(note);
        }
        catch (ArgumentException ex)
        {
            throw new McpProtocolException(McpErrorCode.InvalidParams, $"{ex.Message} ❌");
        }
    }

    [McpServerTool]
    [Description("Lists all notes ordered by creation date (newest first)")]
    public string ListNotes(NotesService notesService)
    {
        var notes = notesService.ListNotes().ToList();
        if (!notes.Any())
            return "No notes found. Create your first note! 📝";

        var output = $"📝 All Notes ({notes.Count} total)\n\n";
        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            var updated = note.UpdatedAt.HasValue
                ? $"Updated: {note.UpdatedAt.Value:yyyy-MM-dd HH:mm} UTC"
                : "Updated: Never";
            output += $"{i + 1}. [{note.Id}] {note.Title}\n   Created: {note.CreatedAt:yyyy-MM-dd HH:mm} UTC | {updated}\n\n";
        }
        return output;
    }

    [McpServerTool]
    [Description("Searches for notes where title or content contains the query (case-insensitive)")]
    public string SearchNotes(
        NotesService notesService,
        [Description("The search term to find in note titles or content")] string query)
    {
        var results = notesService.SearchNotes(query).ToList();
        if (!results.Any())
            return $"No notes found matching '{query}' 🔍";

        var output = $"Found {results.Count} note(s) matching '{query}' 🔍\n\n";
        for (int i = 0; i < results.Count; i++)
        {
            var note = results[i];
            output += $"{i + 1}. [{note.Id}] {note.Title}\n   Created: {note.CreatedAt:yyyy-MM-dd HH:mm} UTC\n\n";
        }
        return output;
    }

    [McpServerTool]
    [Description("Updates an existing note's title and/or content")]
    public string UpdateNote(
        NotesService notesService,
        [Description("The unique identifier of the note to update")] string id,
        [Description("New title for the note (optional, preserves existing if not provided)")] string? title = null,
        [Description("New content for the note (optional, preserves existing if not provided)")] string? content = null)
    {
        try
        {
            var updated = notesService.UpdateNote(id, title, content);
            if (updated is null)
                throw new McpProtocolException(McpErrorCode.InvalidParams, $"Note with ID '{id}' not found ❌");

            return $"Updated note '{updated.Title}' (ID: {updated.Id}) ✅";
        }
        catch (ArgumentException ex)
        {
            throw new McpProtocolException(McpErrorCode.InvalidParams, $"{ex.Message} ❌");
        }
    }

    [McpServerTool]
    [Description("Permanently deletes a note by its ID")]
    public string DeleteNote(
        NotesService notesService,
        [Description("The unique identifier of the note to delete")] string id)
    {
        try
        {
            var note = notesService.GetNote(id);
            if (note is null)
                throw new McpProtocolException(McpErrorCode.InvalidParams, $"Note with ID '{id}' not found ❌");

            notesService.DeleteNote(id);
            return $"Deleted note '{note.Title}' (ID: {note.Id}) ✅";
        }
        catch (ArgumentException ex)
        {
            throw new McpProtocolException(McpErrorCode.InvalidParams, $"{ex.Message} ❌");
        }
    }

    private static string FormatNote(Models.NoteItem note)
    {
        var updated = note.UpdatedAt.HasValue
            ? $"{note.UpdatedAt.Value:yyyy-MM-dd HH:mm} UTC"
            : "Never";

        return $"""
            📝 Note {note.Id}
            
            Title: {note.Title}
            Content: {note.Content}
            
            Created: {note.CreatedAt:yyyy-MM-dd HH:mm} UTC
            Updated: {updated}
            """;
    }
}
```

**Key Points**:
- NotesService injected via parameter (MCP SDK handles DI)
- All tools have `[Description]` attributes for AI discoverability
- Return formatted strings with emojis (✅ ❌ 📝 🔍)
- Catch `ArgumentException` and convert to `McpProtocolException`
- Use helper method `FormatNote()` for consistent formatting

---

## Step 4: Implement MCP Prompts

**File**: `SampleMcpServer/Prompts/NotesPrompts.cs` (create new directory and file)

**What**: Guided interactions for common note-taking scenarios

**Why**: Provide structured templates and auto-generation for better UX

**Implementation** (simplified for workshop):

```csharp
using System.ComponentModel;
using ModelContextProtocol.Server;
using SampleMcpServer.Services;

namespace SampleMcpServer.Prompts;

/// <summary>
/// MCP prompts for guided note creation workflows.
/// </summary>
internal class NotesPrompts
{
    [McpServerPrompt]
    [Description("Quickly capture a thought with auto-generated title")]
    public string QuickNote(
        [Description("The content to capture as a quick note")] string content)
    {
        var title = GenerateTitle(content);
        return $"""
            I'll create a quick note with:
            
            Title: {title}
            Content: {content}
            
            Would you like me to create this note, or would you prefer a different title?
            """;
    }

    [McpServerPrompt]
    [Description("Get an overview of all notes with statistics")]
    public string SummarizeNotes(NotesService notesService)
    {
        var notes = notesService.ListNotes().ToList();
        if (!notes.Any())
            return "You have no notes yet. Create your first note to get started! 📝";

        var recentlyCreated = notes.Count(n => n.CreatedAt > DateTime.UtcNow.AddDays(-1));
        var recentlyUpdated = notes.Count(n => n.UpdatedAt.HasValue && n.UpdatedAt > DateTime.UtcNow.AddDays(-1));
        var neverUpdated = notes.Count(n => !n.UpdatedAt.HasValue);

        var preview = string.Join("\n", notes.Take(5).Select((n, i) =>
            $"{i + 1}. [{n.Id}] {n.Title} ({n.CreatedAt:yyyy-MM-dd HH:mm} UTC)"));

        return $"""
            📊 Notes Summary
            
            Total Notes: {notes.Count}
            Created today: {recentlyCreated}
            Updated today: {recentlyUpdated}
            Never updated: {neverUpdated}
            
            Recent Notes:
            {preview}
            
            Use SearchNotes to find specific topics, or GetNote to read a full note.
            """;
    }

    [McpServerPrompt]
    [Description("Search for notes on a specific topic with context")]
    public string FindTopicNotes(
        NotesService notesService,
        [Description("The topic or subject to search for")] string topic)
    {
        var results = notesService.SearchNotes(topic).ToList();
        if (!results.Any())
            return $"No notes found about '{topic}'. Try a different search term or create a new note. 🔍";

        var formatted = string.Join("\n\n", results.Select((n, i) =>
        {
            var relevance = n.Title.Contains(topic, StringComparison.OrdinalIgnoreCase) ? "⭐ High (title match)" : "Medium (content match)";
            return $"{i + 1}. [{n.Id}] {n.Title}\n   Created: {n.CreatedAt:yyyy-MM-dd HH:mm} UTC\n   Relevance: {relevance}";
        }));

        return $"""
            🔍 Notes about '{topic}'
            
            Found {results.Count} relevant note(s):
            
            {formatted}
            
            Use GetNote [id] to read the full note.
            """;
    }

    [McpServerPrompt]
    [Description("Create structured meeting notes with template")]
    public string MeetingNotes(
        [Description("Name or purpose of the meeting")] string meetingName)
    {
        var template = $"""
            I'll help you create meeting notes for '{meetingName}'.
            
            Here's a template to fill out:
            
            📅 Date: {DateTime.UtcNow:yyyy-MM-dd}
            👥 Attendees: [Who attended?]
            
            📋 Agenda:
            • [What topics were discussed?]
            
            💬 Discussion:
            [Key points from the discussion]
            
            ✅ Decisions:
            • [What decisions were made?]
            
            📌 Action Items:
            • [ ] [Task] - [Owner] - [Due date]
            
            Please provide the information and I'll create the note for you.
            """;
        return template;
    }

    [McpServerPrompt]
    [Description("Create structured code review notes with checklist")]
    public string CodeReviewNote(
        [Description("The file, component, or module being reviewed")] string fileOrComponent)
    {
        var template = $"""
            I'll help you document the code review for '{fileOrComponent}'.
            
            Here's a template to fill out:
            
            👤 Reviewer: [Who performed the review?]
            📅 Date: {DateTime.UtcNow:yyyy-MM-dd}
            🎯 Purpose: [Why was this code reviewed?]
            
            🔍 Findings:
            • [What issues or observations were found?]
            
            💡 Recommendations:
            • [What changes are suggested?]
            
            ✅ Status: [Approved / Approved with changes / Needs rework]
            
            Please provide the details and I'll create the code review note.
            """;
        return template;
    }

    private static string GenerateTitle(string content)
    {
        var words = content.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 5)
            return content.Trim();

        return string.Join(' ', words.Take(5)) + "...";
    }
}
```

**Key Points**:
- Prompts return guidance strings (templates, summaries)
- Can inject NotesService to access data
- Use emojis for visual structure (📊 📝 🔍 etc.)
- `GenerateTitle()` helper for QuickNote auto-generation
- Templates guide users through structured note creation

---

## Step 5: Register Services and Tools

**File**: `SampleMcpServer/Program.cs` (modify existing file)

**What**: Register NotesService and wire up tools/prompts

**Why**: Dependency injection and MCP integration

**Changes**:

```csharp
using SampleMcpServer.Tools;
using SampleMcpServer.Prompts;
using SampleMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Register NotesService as singleton
builder.Services.AddSingleton<NotesService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<NotesTools>()           // Add this line
    .WithPrompts<NotesPrompts>();      // Add this line

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
```

**Key Points**:
- `AddSingleton<NotesService>()` ensures single shared instance
- `WithTools<NotesTools>()` registers all 6 MCP tools
- `WithPrompts<NotesPrompts>()` registers all 5 MCP prompts
- Order matters: register service before configuring MCP server

---

## Step 6: Build and Test

**Build**:
```bash
cd /workspaces/WorkshopTest/SampleMcpServer
dotnet build
```

Expected: Build succeeds with no errors

**Run**:
```bash
dotnet run
```

Expected: Server starts and listens (stdio and HTTP)

**Test with MCP Client** (GitHub Copilot, Claude Desktop, etc.):

1. **List available tools**: Should show AddNote, GetNote, ListNotes, SearchNotes, UpdateNote, DeleteNote

2. **Create a note**:
   ```
   Use AddNote with title "Test Note" and content "This is a test"
   ```
   Expected: "Added note 'Test Note' with ID xxxxxxxx ✅"

3. **List notes**:
   ```
   Use ListNotes
   ```
   Expected: Shows the test note with ID, timestamps

4. **Search notes**:
   ```
   Use SearchNotes with query "test"
   ```
   Expected: Finds and displays the test note

5. **Update note** (use ID from step 2):
   ```
   Use UpdateNote with id "xxxxxxxx", title "Updated Test"
   ```
   Expected: "Updated note 'Updated Test' (ID: xxxxxxxx) ✅"

6. **Get note**:
   ```
   Use GetNote with id "xxxxxxxx"
   ```
   Expected: Shows full note details with updated timestamp

7. **Delete note**:
   ```
   Use DeleteNote with id "xxxxxxxx"
   ```
   Expected: "Deleted note 'Updated Test' (ID: xxxxxxxx) ✅"

8. **Try prompts**:
   ```
   Use QuickNote with content "Remember to update documentation"
   Use SummarizeNotes
   Use FindTopicNotes with topic "documentation"
   Use MeetingNotes with meetingName "Daily Standup"
   Use CodeReviewNote with fileOrComponent "NotesService.cs"
   ```

---

## Troubleshooting

### Build Errors

**Error**: `The type or namespace name 'Models' does not exist`
- **Fix**: Ensure you created the `Models/` directory and `NoteItem.cs` file

**Error**: `The type or namespace name 'NotesService' could not be found`
- **Fix**: Ensure you created `Services/NotesService.cs` and namespace is correct

### Runtime Errors

**Error**: `No service for type 'NotesService' has been registered`
- **Fix**: Add `builder.Services.AddSingleton<NotesService>();` in Program.cs

**Error**: Tools not appearing in MCP client
- **Fix**: Ensure `WithTools<NotesTools>()` is called in Program.cs
- **Fix**: Rebuild and restart the server

### Tool Invocation Errors

**Error**: "Note with ID 'xxx' not found ❌"
- **Cause**: Note doesn't exist or wrong ID
- **Fix**: Use ListNotes to see all note IDs

**Error**: "Title cannot be empty ❌"
- **Cause**: Empty or whitespace-only title/content
- **Fix**: Provide non-empty values

---

## Next Steps

✅ **Feature Complete!** You've implemented:
- In-memory note storage with thread-safe access
- 6 MCP tools for full CRUD operations
- 5 MCP prompts for guided interactions
- Proper error handling and user-friendly responses

**Enhancements** (optional):
- Add note categories/tags
- Implement note persistence (save to file)
- Add note export (markdown, JSON)
- Create unit tests for NotesService
- Add more structured prompt templates

**Learn More**:
- [MCP Documentation](https://modelcontextprotocol.io)
- [.NET ModelContextProtocol SDK](https://github.com/microsoft/modelcontextprotocol-dotnet)
- [Constitution Principles](../../.specify/memory/constitution.md)

---

## Summary

You successfully implemented an in-memory notes store with:
- **NotesService**: Thread-safe storage with CRUD operations
- **6 MCP Tools**: AddNote, GetNote, ListNotes, SearchNotes, UpdateNote, DeleteNote
- **5 MCP Prompts**: QuickNote, SummarizeNotes, FindTopicNotes, MeetingNotes, CodeReviewNote
- **User-Friendly UX**: Emojis, formatted responses, clear error messages

The implementation follows all constitution principles:
✅ MCP SDK integration with attributes  
✅ AI discoverability through descriptions  
✅ Dependency injection for state management  
✅ User-friendly tool responses  
✅ Workshop-focused simplicity  
✅ Modern .NET patterns

**Total Files Created**: 4 (NoteItem.cs, NotesService.cs, NotesTools.cs, NotesPrompts.cs)  
**Total Files Modified**: 1 (Program.cs)  
**Total Lines of Code**: ~500
