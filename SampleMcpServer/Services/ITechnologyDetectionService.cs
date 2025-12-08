using SampleMcpServer.Models;

namespace SampleMcpServer.Services;

/// <summary>
/// Service for detecting technologies used in a project.
/// </summary>
public interface ITechnologyDetectionService
{
    /// <summary>
    /// Analyzes a project directory to detect which technologies are being used.
    /// </summary>
    Task<TechnologyDetectionResult> DetectTechnologiesAsync(
        string projectPath, 
        CancellationToken cancellationToken = default);
}
