using System.ComponentModel;
using ModelContextProtocol.Server;
using SampleMcpServer.Services;
using SampleMcpServer.Models;

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
            return $"{ex.Message} ❌";
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
                return $"Note with ID '{id}' not found ❌";

            return FormatNote(note);
        }
        catch (ArgumentException ex)
        {
            return $"{ex.Message} ❌";
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
                return $"Note with ID '{id}' not found ❌";

            return $"Updated note '{updated.Title}' (ID: {updated.Id}) ✅";
        }
        catch (ArgumentException ex)
        {
            return $"{ex.Message} ❌";
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
                return $"Note with ID '{id}' not found ❌";

            notesService.DeleteNote(id);
            return $"Deleted note '{note.Title}' (ID: {note.Id}) ✅";
        }
        catch (ArgumentException ex)
        {
            return $"{ex.Message} ❌";
        }
    }

    private static string FormatNote(NoteItem note)
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
