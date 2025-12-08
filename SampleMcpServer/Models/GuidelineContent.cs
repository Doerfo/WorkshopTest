namespace SampleMcpServer.Models;

/// <summary>
/// Represents content from a company guideline file.
/// </summary>
/// <param name="Technology">Technology this guideline applies to</param>
/// <param name="Aspect">Specific aspect of technology (e.g., "testing", "security"), null for general guidelines</param>
/// <param name="FilePath">Absolute path to the guideline markdown file</param>
/// <param name="Content">Full markdown content of the guideline</param>
/// <param name="Frontmatter">YAML frontmatter metadata if present</param>
public record GuidelineContent(
    string Technology,
    string? Aspect,
    string FilePath,
    string Content,
    Dictionary<string, string>? Frontmatter = null
);
