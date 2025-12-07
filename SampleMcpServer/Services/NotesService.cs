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
    /// Validates that a string value is not null, empty, or whitespace.
    /// </summary>
    private static void ValidateNotEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
    }

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

        // Validate new values if provided
        if (title is not null) ValidateNotEmpty(title, nameof(title));
        if (content is not null) ValidateNotEmpty(content, nameof(content));

        var updated = existing with
        {
            Title = title?.Trim() ?? existing.Title,
            Content = content?.Trim() ?? existing.Content,
            UpdatedAt = DateTime.UtcNow
        };

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
}
