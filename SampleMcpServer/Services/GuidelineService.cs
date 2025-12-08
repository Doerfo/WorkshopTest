using SampleMcpServer.Models;
using System.Text.RegularExpressions;

namespace SampleMcpServer.Services;

/// <summary>
/// Manages company-specific guideline files from the Guidelines directory.
/// </summary>
public class GuidelineService : IGuidelineService
{
    private readonly ILogger<GuidelineService> _logger;
    private readonly string _guidelinesPath;

    public GuidelineService(
        IWebHostEnvironment environment,
        ILogger<GuidelineService> logger)
    {
        _logger = logger;
        _guidelinesPath = Path.Combine(environment.ContentRootPath, "Guidelines");
        
        // Create Guidelines directory if it doesn't exist
        if (!Directory.Exists(_guidelinesPath))
        {
            Directory.CreateDirectory(_guidelinesPath);
            _logger.LogInformation("Created Guidelines directory at {Path}", _guidelinesPath);
        }
    }

    public async Task<List<GuidelineContent>> GetGuidelinesAsync(
        string technology, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving guidelines for technology: {Technology}", technology);

        if (!Directory.Exists(_guidelinesPath))
        {
            _logger.LogWarning("Guidelines directory does not exist: {Path}", _guidelinesPath);
            return [];
        }

        var normalizedTech = technology.ToLowerInvariant();
        var guidelines = new List<GuidelineContent>();

        // Pattern: {technology}[-{aspect}].md
        var pattern = $"{normalizedTech}*.md";
        var files = Directory.GetFiles(_guidelinesPath, pattern, SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                var guideline = await LoadGuidelineFileAsync(file, normalizedTech, cancellationToken);
                if (guideline != null)
                {
                    guidelines.Add(guideline);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load guideline file: {FilePath}", file);
            }
        }

        _logger.LogInformation("Found {Count} guidelines for {Technology}", guidelines.Count, technology);
        return guidelines;
    }

    public async Task<Dictionary<string, List<GuidelineContent>>> DiscoverAllGuidelinesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Discovering all guidelines in {Path}", _guidelinesPath);

        var allGuidelines = new Dictionary<string, List<GuidelineContent>>();

        if (!Directory.Exists(_guidelinesPath))
        {
            _logger.LogWarning("Guidelines directory does not exist");
            return allGuidelines;
        }

        var files = Directory.GetFiles(_guidelinesPath, "*.md", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                
                // Extract technology name (before optional dash)
                var match = Regex.Match(fileName, @"^([a-z]+)(-.*)?$");
                if (!match.Success)
                {
                    _logger.LogWarning("Guideline file does not match naming convention: {FileName}", fileName);
                    continue;
                }

                var technology = match.Groups[1].Value;
                var guideline = await LoadGuidelineFileAsync(file, technology, cancellationToken);
                
                if (guideline != null)
                {
                    if (!allGuidelines.ContainsKey(technology))
                    {
                        allGuidelines[technology] = [];
                    }
                    allGuidelines[technology].Add(guideline);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process guideline file: {FilePath}", file);
            }
        }

        _logger.LogInformation("Discovered guidelines for {Count} technologies", allGuidelines.Count);
        return allGuidelines;
    }

    private async Task<GuidelineContent?> LoadGuidelineFileAsync(
        string filePath, 
        string expectedTechnology,
        CancellationToken cancellationToken)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        // Extract technology and aspect from filename
        var match = Regex.Match(fileName, @"^([a-z]+)(?:-(.+))?$");
        if (!match.Success)
        {
            _logger.LogWarning("Invalid guideline filename format: {FileName}", fileName);
            return null;
        }

        var technology = match.Groups[1].Value;
        var aspect = match.Groups[2].Success ? match.Groups[2].Value : null;

        // Validate technology matches expected
        if (technology != expectedTechnology)
        {
            return null;
        }

        // Parse frontmatter if present
        var frontmatter = ParseFrontmatter(content);

        return new GuidelineContent(
            Technology: technology,
            Aspect: aspect,
            FilePath: filePath,
            Content: content,
            Frontmatter: frontmatter
        );
    }

    private Dictionary<string, string>? ParseFrontmatter(string content)
    {
        var match = Regex.Match(content, @"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline);
        if (!match.Success)
        {
            return null;
        }

        var frontmatter = new Dictionary<string, string>();
        var yaml = match.Groups[1].Value;
        
        foreach (var line in yaml.Split('\n'))
        {
            var kvMatch = Regex.Match(line, @"^\s*([^:]+):\s*(.+)\s*$");
            if (kvMatch.Success)
            {
                var key = kvMatch.Groups[1].Value.Trim();
                var value = kvMatch.Groups[2].Value.Trim();
                frontmatter[key] = value;
            }
        }

        return frontmatter.Count > 0 ? frontmatter : null;
    }
}
