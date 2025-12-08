using SampleMcpServer.Models;

namespace SampleMcpServer.Services;

/// <summary>
/// Service for managing company-specific guideline files.
/// </summary>
public interface IGuidelineService
{
    /// <summary>
    /// Retrieves all company guidelines for a specific technology.
    /// </summary>
    Task<List<GuidelineContent>> GetGuidelinesAsync(
        string technology, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Discovers all available guideline files.
    /// </summary>
    Task<Dictionary<string, List<GuidelineContent>>> DiscoverAllGuidelinesAsync(
        CancellationToken cancellationToken = default);
}
