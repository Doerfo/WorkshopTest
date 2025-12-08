namespace SampleMcpServer.Models;

/// <summary>
/// Represents the output of project analysis.
/// </summary>
/// <param name="DetectedTechnologies">Technologies found in project</param>
/// <param name="Indicators">Technology → list of detected indicators</param>
/// <param name="AmbiguousTechnologies">Technologies with weak detection confidence</param>
/// <param name="ProjectPath">Path that was analyzed</param>
/// <param name="AnalyzedAt">When analysis was performed</param>
public record TechnologyDetectionResult(
    List<TechnologyInfo> DetectedTechnologies,
    Dictionary<string, List<string>> Indicators,
    List<string> AmbiguousTechnologies,
    string ProjectPath,
    DateTimeOffset AnalyzedAt
);
