using SampleMcpServer.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SampleMcpServer.Services;

/// <summary>
/// Merges baseline instructions with company guidelines using section-based deduplication.
/// </summary>
public class InstructionMergeService : IInstructionMergeService
{
    private readonly ILogger<InstructionMergeService> _logger;

    public InstructionMergeService(ILogger<InstructionMergeService> logger)
    {
        _logger = logger;
    }

    public async Task<MergedInstructionContent> MergeAsync(
        BaselineInstruction? baseline,
        List<GuidelineContent> guidelines,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Merging baseline and {Count} guidelines", guidelines.Count);

        if (baseline == null && guidelines.Count == 0)
        {
            throw new InvalidOperationException("Cannot merge: both baseline and guidelines are empty");
        }

        var technology = baseline?.Technology ?? guidelines.First().Technology;
        var sections = new Dictionary<string, InstructionSection>();
        var contentHashes = new HashSet<string>();

        // Parse baseline into sections
        if (baseline != null)
        {
            var baselineSections = await ParseSectionsAsync(baseline.Content, "baseline", cancellationToken);
            foreach (var section in baselineSections)
            {
                var hash = ComputeContentHash(section.Content);
                if (contentHashes.Add(hash))
                {
                    sections[section.Header] = section;
                }
            }
            _logger.LogDebug("Parsed {Count} sections from baseline", baselineSections.Count);
        }

        // Parse and merge company guidelines (company overrides baseline)
        foreach (var guideline in guidelines)
        {
            var guidelineSections = await ParseSectionsAsync(guideline.Content, "company-guideline", cancellationToken);
            
            foreach (var section in guidelineSections)
            {
                var hash = ComputeContentHash(section.Content);
                
                // Company guideline overrides baseline for same header
                if (sections.ContainsKey(section.Header))
                {
                    _logger.LogDebug("Company guideline overriding baseline section: {Header}", section.Header);
                    sections[section.Header] = section with { Source = "company-guideline" };
                    contentHashes.Add(hash);
                }
                // Add if content is unique (not already present)
                else if (contentHashes.Add(hash))
                {
                    sections[section.Header] = section;
                }
                else
                {
                    _logger.LogDebug("Skipping duplicate content in section: {Header}", section.Header);
                }
            }
            
            _logger.LogDebug("Merged {Count} sections from guideline: {FilePath}", 
                guidelineSections.Count, guideline.FilePath);
        }

        var title = ExtractTitle(baseline?.Content) ?? 
                    ExtractTitle(guidelines.FirstOrDefault()?.Content) ?? 
                    $"{FormatTechnologyName(technology)} Instructions";

        var description = ExtractDescription(baseline?.Content) ?? 
                         ExtractDescription(guidelines.FirstOrDefault()?.Content) ?? 
                         $"{FormatTechnologyName(technology)} coding guidelines";

        var applyToPatterns = ExtractApplyToPatterns(baseline?.Content, guidelines) ?? 
                             [$"**/*.{GetFileExtension(technology)}"];

        var sourceSummary = BuildSourceSummary(baseline, guidelines);

        var merged = new MergedInstructionContent(
            Technology: technology,
            Title: title,
            Description: description,
            ApplyToPatterns: applyToPatterns,
            Sections: sections.Values.ToList(),
            SourceSummary: sourceSummary
        );

        _logger.LogInformation("Merge complete: {SectionCount} sections, {SourceSummary}", 
            sections.Count, sourceSummary);

        return merged;
    }

    private async Task<List<InstructionSection>> ParseSectionsAsync(
        string content, 
        string source, 
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // For async consistency

        var sections = new List<InstructionSection>();
        
        // Remove frontmatter
        var withoutFrontmatter = Regex.Replace(content, @"^---\s*\n.*?\n---\s*\n", "", RegexOptions.Singleline);
        
        // Split by level 2 headers (##)
        var matches = Regex.Matches(withoutFrontmatter, @"^##\s+(.+?)$\s*(.*?)(?=^##\s+|$)", 
            RegexOptions.Multiline | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var header = match.Groups[1].Value.Trim();
            var sectionContent = match.Groups[2].Value.Trim();

            if (!string.IsNullOrWhiteSpace(sectionContent))
            {
                sections.Add(new InstructionSection(
                    Header: header,
                    Content: sectionContent,
                    Source: source
                ));
            }
        }

        return sections;
    }

    private string ComputeContentHash(string content)
    {
        // Normalize content for hashing
        var normalized = content.ToLowerInvariant()
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
        
        // Remove markdown formatting
        normalized = Regex.Replace(normalized, @"[*`_#]+", "");
        normalized = Regex.Replace(normalized, @"\s+", " ");

        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private string? ExtractTitle(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        var match = Regex.Match(content, @"^#\s+(.+?)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private string? ExtractDescription(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        var match = Regex.Match(content, @"description:\s*(.+?)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private List<string>? ExtractApplyToPatterns(string? baselineContent, List<GuidelineContent> guidelines)
    {
        // Try baseline first
        if (!string.IsNullOrWhiteSpace(baselineContent))
        {
            var match = Regex.Match(baselineContent, @"applyTo:\s*['""](.+?)['""]", RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Groups[1].Value.Split(',').Select(p => p.Trim()).ToList();
            }
        }

        // Try guidelines
        foreach (var guideline in guidelines)
        {
            if (guideline.Frontmatter != null && guideline.Frontmatter.TryGetValue("applyTo", out var pattern))
            {
                return pattern.Split(',').Select(p => p.Trim()).ToList();
            }
        }

        return null;
    }

    private string BuildSourceSummary(BaselineInstruction? baseline, List<GuidelineContent> guidelines)
    {
        var parts = new List<string>();
        
        if (baseline != null)
        {
            parts.Add("baseline from awesome-copilot");
        }

        if (guidelines.Count > 0)
        {
            parts.Add($"{guidelines.Count} company guideline{(guidelines.Count > 1 ? "s" : "")}");
        }

        return string.Join(" + ", parts);
    }

    private string FormatTechnologyName(string technology)
    {
        return technology switch
        {
            "csharp" => "C#",
            "dotnet" => ".NET",
            "typescript" => "TypeScript",
            "javascript" => "JavaScript",
            "golang" => "Go",
            _ => char.ToUpper(technology[0]) + technology[1..]
        };
    }

    private string GetFileExtension(string technology)
    {
        return technology switch
        {
            "csharp" => "cs",
            "typescript" => "ts",
            "javascript" => "js",
            "python" => "py",
            "java" => "java",
            "golang" => "go",
            "rust" => "rs",
            _ => technology
        };
    }
}
