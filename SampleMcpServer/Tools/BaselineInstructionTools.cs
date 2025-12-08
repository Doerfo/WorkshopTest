using ModelContextProtocol.Server;
using SampleMcpServer.Models;
using SampleMcpServer.Services;
using System.ComponentModel;

namespace SampleMcpServer.Tools;

/// <summary>
/// MCP tools for retrieving baseline instructions from awesome-copilot repository.
/// </summary>
public class BaselineInstructionTools
{
    [McpServerTool]
    [Description("Fetches the baseline instruction file for a specific technology from the GitHub awesome-copilot repository")]
    public async Task<BaselineInstruction?> GetBaselineInstruction(
        [Description("Technology name (lowercase, e.g., 'csharp', 'typescript', 'react')")]
        string technology,
        IGitHubService githubService,
        IAwesomeCopilotCacheService cacheService,
        ILogger<BaselineInstructionTools> logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(technology))
        {
            throw new ArgumentException("technology parameter is required and cannot be empty", nameof(technology));
        }

        logger.LogInformation("Fetching baseline instruction for technology: {Technology}", technology);

        try
        {
            // Get technology file mapping from cache
            var technologyFiles = await cacheService.GetTechnologyFilesAsync(cancellationToken);
            
            if (!technologyFiles.TryGetValue(technology.ToLowerInvariant(), out var filePath))
            {
                logger.LogWarning("No baseline instruction found for technology: {Technology}", technology);
                return null;
            }

            // Fetch file content from GitHub
            var content = await githubService.GetFileContentAsync(
                "github",
                "awesome-copilot",
                filePath,
                cancellationToken);

            var fileName = Path.GetFileName(filePath);
            var sourceUrl = $"https://raw.githubusercontent.com/github/awesome-copilot/main/{filePath}";

            logger.LogInformation("Successfully fetched baseline for {Technology}", technology);

            return new BaselineInstruction(
                Technology: technology.ToLowerInvariant(),
                FileName: fileName,
                Content: content,
                SourceUrl: sourceUrl,
                RetrievedAt: DateTimeOffset.UtcNow,
                Sha: null
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch baseline instruction for {Technology}", technology);
            throw new InvalidOperationException($"Failed to retrieve baseline instruction: {ex.Message}", ex);
        }
    }

    [McpServerTool]
    [Description("Manually refreshes the cache of awesome-copilot repository structure, useful after repository updates")]
    public async Task<string> RefreshCache(
        IAwesomeCopilotCacheService cacheService,
        ILogger<BaselineInstructionTools> logger,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Manually refreshing cache");

        try
        {
            await cacheService.RefreshCacheAsync(cancellationToken);
            var files = await cacheService.GetTechnologyFilesAsync(cancellationToken);
            
            var message = $"Cache refreshed successfully. Found {files.Count} baseline instruction files.";
            logger.LogInformation(message);
            
            return message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh cache");
            throw new InvalidOperationException("Failed to refresh cache", ex);
        }
    }
}
