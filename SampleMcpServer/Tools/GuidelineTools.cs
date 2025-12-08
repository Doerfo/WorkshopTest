using ModelContextProtocol.Server;
using SampleMcpServer.Models;
using SampleMcpServer.Services;
using System.ComponentModel;

namespace SampleMcpServer.Tools;

/// <summary>
/// MCP tools for retrieving company-specific guidelines.
/// </summary>
public class GuidelineTools
{
    [McpServerTool]
    [Description("Retrieves all company-specific guideline files for a specific technology from the MCP server's Guidelines directory")]
    public async Task<List<GuidelineContent>> GetCompanyGuideline(
        [Description("Technology name (lowercase, e.g., 'csharp', 'typescript', 'react')")]
        string technology,
        IGuidelineService guidelineService,
        ILogger<GuidelineTools> logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(technology))
        {
            throw new ArgumentException("technology parameter is required and cannot be empty", nameof(technology));
        }

        logger.LogInformation("Retrieving company guidelines for technology: {Technology}", technology);

        try
        {
            var guidelines = await guidelineService.GetGuidelinesAsync(technology, cancellationToken);
            
            logger.LogInformation("Found {Count} company guidelines for {Technology}", 
                guidelines.Count, technology);
            
            return guidelines;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve company guidelines for {Technology}", technology);
            throw new InvalidOperationException($"Failed to retrieve company guidelines: {ex.Message}", ex);
        }
    }
}
