namespace SampleMcpServer.Models;

/// <summary>
/// Represents metadata about a detected or supported technology.
/// </summary>
/// <param name="Name">Technology identifier (lowercase, e.g., "csharp", "typescript", "react")</param>
/// <param name="DisplayName">Human-readable name (e.g., "C#", "TypeScript", "React")</param>
/// <param name="HasBaseline">Whether awesome-copilot repository has baseline for this technology</param>
/// <param name="HasGuideline">Whether company has guideline file for this technology</param>
/// <param name="DetectionConfidence">Confidence level for detected technologies ("High", "Medium", "Low")</param>
public record TechnologyInfo(
    string Name,
    string DisplayName,
    bool HasBaseline,
    bool HasGuideline,
    string? DetectionConfidence = null
);
