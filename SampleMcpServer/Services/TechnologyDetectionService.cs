using SampleMcpServer.Models;
using System.Text.Json;

namespace SampleMcpServer.Services;

/// <summary>
/// Detects technologies in a project using file-based pattern matching.
/// </summary>
public class TechnologyDetectionService : ITechnologyDetectionService
{
    private readonly ILogger<TechnologyDetectionService> _logger;
    
    // Technology detection patterns
    private static readonly Dictionary<string, TechnologyPattern> Patterns = new()
    {
        ["csharp"] = new("C#", ["*.csproj", "*.cs"], ["*.sln", "global.json"]),
        ["typescript"] = new("TypeScript", ["tsconfig.json", "*.ts"], ["package.json:typescript"]),
        ["javascript"] = new("JavaScript", ["package.json", "*.js"], [".eslintrc*", "webpack.config.js"]),
        ["python"] = new("Python", ["requirements.txt", "*.py"], ["pyproject.toml", "setup.py", "Pipfile"]),
        ["java"] = new("Java", ["pom.xml", "build.gradle", "*.java"], ["settings.gradle", "gradlew"]),
        ["react"] = new("React", ["package.json:react"], ["*.jsx", "*.tsx"]),
        ["angular"] = new("Angular", ["angular.json"], ["*.component.ts"]),
        ["vue"] = new("Vue", ["package.json:vue"], ["*.vue", "vue.config.js"]),
        ["golang"] = new("Go", ["go.mod", "*.go"], ["go.sum"]),
        ["rust"] = new("Rust", ["Cargo.toml", "*.rs"], ["Cargo.lock"]),
        ["dotnet"] = new(".NET", ["*.csproj:Microsoft.NET.Sdk"], ["global.json"])
    };

    public TechnologyDetectionService(ILogger<TechnologyDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<TechnologyDetectionResult> DetectTechnologiesAsync(
        string projectPath, 
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException($"Project path does not exist: {projectPath}");
        }

        _logger.LogInformation("Detecting technologies in {ProjectPath}", projectPath);

        var detected = new List<TechnologyInfo>();
        var indicators = new Dictionary<string, List<string>>();
        var ambiguous = new List<string>();

        foreach (var (techName, pattern) in Patterns)
        {
            var foundIndicators = new List<string>();
            
            // Check primary indicators
            foreach (var primaryPattern in pattern.PrimaryIndicators)
            {
                var found = await CheckPatternAsync(projectPath, primaryPattern, cancellationToken);
                if (found != null)
                {
                    foundIndicators.Add(found);
                }
            }

            // Check secondary indicators
            var secondaryFound = 0;
            foreach (var secondaryPattern in pattern.SecondaryIndicators)
            {
                var found = await CheckPatternAsync(projectPath, secondaryPattern, cancellationToken);
                if (found != null)
                {
                    foundIndicators.Add(found);
                    secondaryFound++;
                }
            }

            // Determine confidence
            string? confidence = null;
            if (foundIndicators.Count > 0)
            {
                if (foundIndicators.Count >= pattern.PrimaryIndicators.Count)
                {
                    confidence = "High";
                }
                else if (secondaryFound > 0)
                {
                    confidence = "Medium";
                    ambiguous.Add(techName);
                }
                else
                {
                    confidence = "Low";
                    ambiguous.Add(techName);
                }

                detected.Add(new TechnologyInfo(
                    Name: techName,
                    DisplayName: pattern.DisplayName,
                    HasBaseline: false, // Will be filled by caller
                    HasGuideline: false, // Will be filled by caller
                    DetectionConfidence: confidence
                ));

                indicators[techName] = foundIndicators;
            }
        }

        _logger.LogInformation("Detected {Count} technologies: {Technologies}", 
            detected.Count, string.Join(", ", detected.Select(t => t.Name)));

        return new TechnologyDetectionResult(
            DetectedTechnologies: detected,
            Indicators: indicators,
            AmbiguousTechnologies: ambiguous,
            ProjectPath: projectPath,
            AnalyzedAt: DateTimeOffset.UtcNow
        );
    }

    private async Task<string?> CheckPatternAsync(
        string projectPath, 
        string pattern, 
        CancellationToken cancellationToken)
    {
        // Handle special patterns like "package.json:react"
        if (pattern.Contains(':'))
        {
            var parts = pattern.Split(':');
            var fileName = parts[0];
            var keyword = parts[1];
            
            var filePath = Path.Combine(projectPath, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    var content = await File.ReadAllTextAsync(filePath, cancellationToken);
                    if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{fileName} (contains {keyword})";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read {FilePath}", filePath);
                }
            }
            return null;
        }

        // Handle glob patterns
        if (pattern.Contains('*'))
        {
            try
            {
                var files = Directory.GetFiles(
                    projectPath, 
                    pattern, 
                    SearchOption.AllDirectories);
                
                if (files.Length > 0)
                {
                    return $"{pattern} ({files.Length} found)";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search for pattern {Pattern}", pattern);
            }
            return null;
        }

        // Handle exact file names
        var exactPath = Path.Combine(projectPath, pattern);
        if (File.Exists(exactPath))
        {
            return pattern;
        }

        return null;
    }

    private record TechnologyPattern(
        string DisplayName,
        List<string> PrimaryIndicators,
        List<string> SecondaryIndicators
    );
}
