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
