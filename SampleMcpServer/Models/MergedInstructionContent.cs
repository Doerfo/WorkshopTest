using System.Text;

namespace SampleMcpServer.Models;

/// <summary>
/// Represents a single section in an instruction file.
/// </summary>
/// <param name="Header">Section header (without ## prefix)</param>
/// <param name="Content">Section content</param>
/// <param name="Source">Source of content: "baseline", "company-guideline", or "both"</param>
public record InstructionSection(
    string Header,
    string Content,
    string Source
);

/// <summary>
/// Represents the final merged content before writing to file.
/// </summary>
/// <param name="Technology">Technology name</param>
/// <param name="Title">File/section title</param>
/// <param name="Description">Description for frontmatter</param>
/// <param name="ApplyToPatterns">Glob patterns for applyTo frontmatter</param>
/// <param name="Sections">Merged content sections</param>
/// <param name="SourceSummary">Human-readable summary of content sources</param>
public record MergedInstructionContent(
    string Technology,
    string Title,
    string Description,
    List<string> ApplyToPatterns,
    List<InstructionSection> Sections,
    string SourceSummary
)
{
    /// <summary>
    /// Converts the merged content to markdown format with frontmatter.
    /// </summary>
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
};
