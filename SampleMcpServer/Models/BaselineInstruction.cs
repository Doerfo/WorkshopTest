namespace SampleMcpServer.Models;

/// <summary>
/// Represents baseline instruction content from the awesome-copilot repository.
/// </summary>
/// <param name="Technology">Technology this baseline applies to</param>
/// <param name="FileName">Original filename in awesome-copilot repo (e.g., "csharp.instructions.md")</param>
/// <param name="Content">Full markdown content</param>
/// <param name="SourceUrl">GitHub raw content URL</param>
/// <param name="RetrievedAt">When this baseline was fetched</param>
/// <param name="Sha">Git SHA hash for cache validation</param>
public record BaselineInstruction(
    string Technology,
    string FileName,
    string Content,
    string SourceUrl,
    DateTimeOffset RetrievedAt,
    string? Sha = null
);
