using System.ComponentModel;
using ModelContextProtocol.Server;
using SampleMcpServer.Services;

namespace SampleMcpServer.Prompts;

/// <summary>
/// MCP prompts for guided note creation workflows.
/// </summary>
internal class NotesPrompts
{
    [McpServerPrompt]
    [Description("Quickly capture a thought with auto-generated title")]
    public string QuickNote(
        NotesService notesService,
        [Description("The thought or information to capture")] string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Content cannot be empty. Please provide the thought you want to capture. ❌";

        var title = GenerateTitle(content);
        
        return $"""
            I'll capture this as a quick note:
            
            Title: {title}
            Content: {content}
            
            Would you like me to create this note, or would you prefer a different title?
            
            💡 Say 'create it' to save, or suggest a different title.
            """;
    }

    [McpServerPrompt]
    [Description("Get an overview of all notes with statistics and recent items")]
    public string SummarizeNotes(NotesService notesService)
    {
        var allNotes = notesService.ListNotes().ToList();
        
        if (!allNotes.Any())
            return "You don't have any notes yet. Create your first note to get started! 📝";

        var now = DateTime.UtcNow;
        var createdToday = allNotes.Count(n => n.CreatedAt.Date == now.Date);
        var updatedToday = allNotes.Count(n => n.UpdatedAt.HasValue && n.UpdatedAt.Value.Date == now.Date);
        var neverUpdated = allNotes.Count(n => !n.UpdatedAt.HasValue);

        // Extract common topics from titles
        var words = allNotes
            .SelectMany(n => n.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length > 3) // Filter short words
            .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"• {g.Key} ({g.Count()} notes)")
            .ToList();

        var commonTopics = words.Any() 
            ? string.Join("\n  ", words)
            : "No common topics identified";

        // Get 5 most recent notes
        var recentNotes = allNotes.Take(5).ToList();
        var recentList = string.Join("\n  ", recentNotes.Select((n, i) =>
        {
            var updated = n.UpdatedAt.HasValue
                ? $", updated {FormatRelativeTime(n.UpdatedAt.Value, now)}"
                : "";
            return $"{i + 1}. [{n.Id}] {n.Title} ({FormatRelativeTime(n.CreatedAt, now)}{updated})";
        }));

        return $"""
            📊 Notes Summary
            
            Total Notes: {allNotes.Count}
            Created today: {createdToday}
            Updated today: {updatedToday}
            Never updated: {neverUpdated}
            
            Common Topics:
            {commonTopics}
            
            Recent Notes:
            {recentList}
            
            💡 Use SearchNotes to find specific topics, or GetNote to read a full note.
            """;
    }

    [McpServerPrompt]
    [Description("Search for notes on a specific topic with relevance context")]
    public string FindTopicNotes(
        NotesService notesService,
        [Description("The topic or subject to search for")] string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "Please provide a topic to search for. ❌";

        var results = notesService.SearchNotes(topic).ToList();
        
        if (!results.Any())
            return $"No notes found about '{topic}'. Try a different search term or create a new note on this topic. 🔍";

        var output = $"🔍 Notes about '{topic}'\n\nFound {results.Count} relevant note(s):\n\n";
        
        for (int i = 0; i < results.Count; i++)
        {
            var note = results[i];
            var titleMatch = note.Title.Contains(topic, StringComparison.OrdinalIgnoreCase);
            var relevance = titleMatch ? "High (title match)" : "Medium (content match)";
            var star = titleMatch ? "⭐ " : "";

            // Extract snippet showing where topic appears
            var matchContext = titleMatch
                ? $"Title match + \"{GetSnippet(note.Content, topic)}\""
                : $"\"{GetSnippet(note.Content, topic)}\"";

            var updated = note.UpdatedAt.HasValue
                ? $", updated {note.UpdatedAt.Value:yyyy-MM-dd HH:mm} UTC"
                : "";

            output += $"{i + 1}. {star}[{note.Id}] {note.Title}\n";
            output += $"   Match: {matchContext}\n";
            output += $"   Created: {note.CreatedAt:yyyy-MM-dd HH:mm} UTC{updated}\n";
            output += $"   Relevance: {relevance}\n\n";
        }

        output += "💡 Use GetNote [id] to read the full note, or refine your search.";
        
        return output;
    }

    [McpServerPrompt]
    [Description("Create structured meeting notes with template guidance")]
    public string MeetingNotes(
        [Description("Name or purpose of the meeting (optional)")] string? meetingName = null)
    {
        var name = string.IsNullOrWhiteSpace(meetingName) 
            ? "[Meeting Name]" 
            : meetingName;

        return $"""
            I'll help you create meeting notes for '{name}'.
            
            Here's the template structure:
            
            📅 Date: {DateTime.UtcNow:yyyy-MM-dd}
            👥 Attendees: [List participants]
            
            📋 Agenda:
            • [Agenda item 1]
            • [Agenda item 2]
            
            💬 Discussion:
            [Key points discussed]
            
            ✅ Decisions:
            • [Decision 1]
            • [Decision 2]
            
            📌 Action Items:
            • [ ] [Task] - [Owner] - [Due date]
            
            💡 Fill in the details and I'll help you create the note. You can provide:
            - Who attended
            - What was discussed
            - Key decisions made
            - Action items and owners
            
            Then use AddNote to save the meeting notes with your completed template.
            """;
    }

    [McpServerPrompt]
    [Description("Create structured code review notes with checklist")]
    public string CodeReviewNote(
        [Description("The file, component, or module being reviewed")] string fileOrComponent)
    {
        if (string.IsNullOrWhiteSpace(fileOrComponent))
            return "Please specify the file or component being reviewed. ❌";

        return $"""
            I'll help you document the code review for '{fileOrComponent}'.
            
            Here's the template structure:
            
            👤 Reviewer: [Your name]
            📅 Date: {DateTime.UtcNow:yyyy-MM-dd}
            🎯 Purpose: [Why this code is being reviewed]
            
            🔍 Findings:
            • [Issue or observation 1]
            • [Issue or observation 2]
            • [Question or concern]
            
            💡 Recommendations:
            • [Suggested improvement 1]
            • [Suggested improvement 2]
            
            ✅ Status: [Approved / Approved with changes / Needs rework]
            
            💡 Fill in the details:
            - Who performed the review
            - Purpose of the review
            - Issues or observations found
            - Recommendations for improvement
            - Approval status
            
            Then use AddNote with title "Code Review: {fileOrComponent}" to save the review.
            """;
    }

    /// <summary>
    /// Generates a title from content using smart extraction.
    /// </summary>
    private static string GenerateTitle(string content)
    {
        var trimmed = content.Trim();
        
        // Short content: use first 5 words
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 10)
        {
            var title = string.Join(" ", words.Take(5));
            return title.Length > 50 ? title[..47] + "..." : title;
        }

        // Single sentence: use entire sentence (max 50 chars)
        var sentences = trimmed.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries);
        if (sentences.Length == 1)
        {
            return trimmed.Length > 50 ? trimmed[..47] + "..." : trimmed;
        }

        // Multiple sentences: use first sentence
        var firstSentence = sentences[0].Trim();
        return firstSentence.Length > 50 ? firstSentence[..47] + "..." : firstSentence;
    }

    /// <summary>
    /// Formats a relative time string (e.g., "16:00 UTC", "Yesterday 11:20 UTC").
    /// </summary>
    private static string FormatRelativeTime(DateTime time, DateTime now)
    {
        var diff = now - time;
        
        if (diff.TotalHours < 24 && time.Date == now.Date)
            return $"{time:HH:mm} UTC";
        
        if (diff.TotalDays < 2 && time.Date == now.Date.AddDays(-1))
            return $"Yesterday, {time:HH:mm} UTC";
        
        return $"{time:yyyy-MM-dd HH:mm} UTC";
    }

    /// <summary>
    /// Extracts a snippet from content showing where the topic appears.
    /// </summary>
    private static string GetSnippet(string content, string topic)
    {
        var index = content.IndexOf(topic, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
            return content.Length > 50 ? content[..47] + "..." : content;

        // Get surrounding context (30 chars before and after)
        var start = Math.Max(0, index - 30);
        var length = Math.Min(content.Length - start, 80);
        var snippet = content.Substring(start, length);

        // Highlight the topic with **bold**
        var highlightedSnippet = snippet.Replace(
            topic, 
            $"**{topic}**", 
            StringComparison.OrdinalIgnoreCase);

        return start > 0 ? "..." + highlightedSnippet : highlightedSnippet;
    }
}
