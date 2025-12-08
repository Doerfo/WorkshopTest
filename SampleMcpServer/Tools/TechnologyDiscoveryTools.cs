using ModelContextProtocol.Server;
using SampleMcpServer.Models;
using SampleMcpServer.Services;
using System.ComponentModel;

namespace SampleMcpServer.Tools;

/// <summary>
/// MCP tools for discovering available technologies and detecting them in projects.
/// </summary>
public class TechnologyDiscoveryTools
{
    [McpServerTool]
    [Description("Returns a list of all technologies supported by the MCP server, including both baseline instructions from awesome-copilot and company-specific guidelines")]
    public async Task<List<TechnologyInfo>> ListAvailableTechnologies(
        IAwesomeCopilotCacheService cacheService,
        IGuidelineService guidelineService,
        ILogger<TechnologyDiscoveryTools> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing available technologies");

        try
        {
            var technologyFiles = await cacheService.GetTechnologyFilesAsync(cancellationToken);
            var allGuidelines = await guidelineService.DiscoverAllGuidelinesAsync(cancellationToken);
            
            // Combine technologies from baselines and guidelines
            var allTechnologies = technologyFiles.Keys
                .Union(allGuidelines.Keys)
                .Distinct()
                .ToList();

            var technologies = allTechnologies.Select(tech => new TechnologyInfo(
                Name: tech,
                DisplayName: FormatDisplayName(tech),
                HasBaseline: technologyFiles.ContainsKey(tech),
                HasGuideline: allGuidelines.ContainsKey(tech),
                DetectionConfidence: null
            )).ToList();

            logger.LogInformation("Found {Count} available technologies", technologies.Count);
            return technologies;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list available technologies");
            throw new InvalidOperationException("Failed to retrieve available technologies from cache", ex);
        }
    }

    [McpServerTool]
    [Description("Analyzes a project directory structure to automatically detect which technologies and frameworks are being used")]
    public async Task<TechnologyDetectionResult> DetectProjectTechnologies(
        [Description("Absolute path to the project directory to analyze")]
        string projectPath,
        ITechnologyDetectionService detectionService,
        IAwesomeCopilotCacheService cacheService,
        IGuidelineService guidelineService,
        ILogger<TechnologyDiscoveryTools> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Detecting technologies in project: {ProjectPath}", projectPath);

        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new ArgumentException("projectPath parameter is required and cannot be empty", nameof(projectPath));
        }

        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
        }

        try
        {
            // Detect technologies
            var result = await detectionService.DetectTechnologiesAsync(projectPath, cancellationToken);

            // Enhance with baseline and guideline availability info
            var technologyFiles = await cacheService.GetTechnologyFilesAsync(cancellationToken);
            var allGuidelines = await guidelineService.DiscoverAllGuidelinesAsync(cancellationToken);
            
            var enhancedTechnologies = result.DetectedTechnologies.Select(tech =>
                tech with
                {
                    HasBaseline = technologyFiles.ContainsKey(tech.Name),
                    HasGuideline = allGuidelines.ContainsKey(tech.Name)
                }
            ).ToList();

            logger.LogInformation("Detected {Count} technologies ({Baselines} with baselines, {Guidelines} with guidelines)", 
                enhancedTechnologies.Count,
                enhancedTechnologies.Count(t => t.HasBaseline),
                enhancedTechnologies.Count(t => t.HasGuideline));

            return result with { DetectedTechnologies = enhancedTechnologies };
        }
        catch (Exception ex) when (ex is not ArgumentException and not DirectoryNotFoundException)
        {
            logger.LogError(ex, "Failed to detect technologies in {ProjectPath}", projectPath);
            throw new InvalidOperationException($"Technology detection failed: {ex.Message}", ex);
        }
    }

    private static string FormatDisplayName(string name)
    {
        return name switch
        {
            "csharp" => "C#",
            "dotnet" => ".NET",
            "typescript" => "TypeScript",
            "javascript" => "JavaScript",
            "golang" => "Go",
            _ => char.ToUpper(name[0]) + name[1..]
        };
    }
}
