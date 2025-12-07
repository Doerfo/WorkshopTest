# MCP Prompts Contract: Guided Note Interactions

**Feature**: In-Memory Notes Store  
**Type**: MCP Prompts (5 guided interactions)  
**Protocol**: Model Context Protocol (MCP)

## Overview

This contract defines the 5 MCP prompts that provide guided, structured interactions for common note-taking scenarios. Prompts help users create well-organized notes through AI-assisted templates and workflows.

---

## Prompt 1: QuickNote

**Purpose**: Quickly capture a thought with auto-generated title

**Parameters**:
- `content` (string, required): The thought or information to capture
  - Can be a single sentence or multiple paragraphs
  - Example: "Need to update the API documentation before Friday's release. Currently missing examples for authentication endpoints."

**Returns**: Interactive prompt string

**Behavior**:
1. Analyze content to generate appropriate title
   - Use first 5-7 words if content is short
   - Extract main topic if content is longer
   - Capitalize appropriately
2. Present suggested title and content to user
3. Ask for confirmation or title adjustment
4. Once confirmed, call AddNote tool to create the note

**Title Generation Strategy**:
- Short content (< 10 words): Use first 5 words
- Single sentence: Use entire sentence (max 50 chars)
- Multiple sentences: Extract subject from first sentence
- Fallback: "Quick Note - [timestamp]"

**Example Interaction**:
```
User: Use QuickNote with "Need to update the API docs before Friday"

AI Response:
  "I'll capture this as a quick note:
  
  Title: Need to update the API docs
  Content: Need to update the API docs before Friday
  
  Would you like me to create this note, or would you prefer a different title?"

User: "Create it"

AI: [Calls AddNote("Need to update the API docs", "Need to update the API docs before Friday")]
  "Added note 'Need to update the API docs' with ID x7y8z9a1 ✅"
```

**MCP Integration**:
- Marked with `[McpServerPrompt]` attribute
- Description: "Quickly capture a thought with auto-generated title"
- Can invoke AddNote tool within the same conversation

---

## Prompt 2: SummarizeNotes

**Purpose**: Get an overview of all notes with key statistics and recent items

**Parameters**: None

**Returns**: Summary string with statistics and previews

**Behavior**:
1. Call ListNotes to retrieve all notes
2. Calculate statistics:
   - Total count
   - Recently created (last 24 hours)
   - Recently updated (last 24 hours)
   - Never updated count
3. Identify most common topics (frequent words in titles)
4. Show preview of 5 most recent notes
5. Format as readable summary

**Example Interaction**:
```
User: "Summarize my notes"

AI: [Calls SummarizeNotes prompt]

Output:
  "📊 Notes Summary
  
  Total Notes: 12
  Created today: 4
  Updated today: 2
  Never updated: 8
  
  Common Topics:
  • Meeting (5 notes)
  • Code Review (3 notes)
  • Roadmap (2 notes)
  
  Recent Notes:
  1. [7d8e9f10] Code Review: Auth Module (16:00 UTC)
  2. [a3f7b2c1] Q4 Planning Meeting (14:30 UTC, updated 16:45)
  3. [5b6c7d8e] Daily Standup Notes (09:15 UTC)
  4. [9c1d2e3f] Product Vision (Yesterday, 11:20 UTC)
  5. [2a3b4c5d] Sprint Retrospective (Yesterday, 15:30 UTC)
  
  Use SearchNotes to find specific topics, or GetNote to read a full note."
```

**MCP Integration**:
- Marked with `[McpServerPrompt]` attribute
- Description: "Get an overview of all notes with statistics and recent items"
- Calls ListNotes internally or via sampling

---

## Prompt 3: FindTopicNotes

**Purpose**: Search for notes on a specific topic with relevance context

**Parameters**:
- `topic` (string, required): The topic or subject to search for
  - Example: "authentication", "Q4 planning", "code review"

**Returns**: Search results with context and relevance indicators

**Behavior**:
1. Call SearchNotes with the topic
2. For each result, show:
   - Note title and ID
   - Match context (snippet showing where topic appears)
   - Relevance indicator (title match > content match)
   - Timestamps
3. Sort by relevance then by date
4. Provide next steps (read full note, refine search)

**Example Interaction**:
```
User: "Find notes about authentication"

AI: [Calls FindTopicNotes with topic="authentication"]

Output:
  "🔍 Notes about 'authentication'
  
  Found 3 relevant notes:
  
  1. ⭐ [7d8e9f10] Code Review: Auth Module
     Match: Title match + "Reviewed **authentication** flow, found security issue"
     Created: 2025-12-07 16:00 UTC
     Relevance: High (title match)
  
  2. [2f3a4b5c] API Documentation Update
     Match: "Currently missing examples for **authentication** endpoints"
     Created: 2025-12-07 10:30 UTC
     Relevance: Medium (content match)
  
  3. [8g9h1i2j] Security Best Practices
     Match: "Always validate **authentication** tokens before processing"
     Created: 2025-12-05 14:15 UTC
     Relevance: Medium (content match)
  
  Use GetNote [id] to read the full note, or refine your search."
```

**MCP Integration**:
- Marked with `[McpServerPrompt]` attribute
- Description: "Search for notes on a specific topic with relevance context"
- Calls SearchNotes internally

---

## Prompt 4: MeetingNotes

**Purpose**: Create structured meeting notes with standard template

**Parameters**:
- `meetingName` (string, optional): Name or purpose of the meeting
  - If not provided, prompts user for it
  - Example: "Q4 Planning Session"

**Returns**: Interactive template-filling workflow

**Behavior**:
1. Request meeting name if not provided
2. Present structured template with fields:
   - Meeting name (title)
   - Date and time
   - Attendees
   - Agenda items
   - Discussion notes
   - Decisions made
   - Action items (who, what, when)
3. Guide user through filling template
4. Once complete, call AddNote with formatted content

**Template Structure**:
```
Title: [Meeting Name]

Content:
📅 Date: [Auto-filled with today's date]
👥 Attendees: [User provides]

📋 Agenda:
• [User provides items]

💬 Discussion:
[User provides notes]

✅ Decisions:
• [User provides]

📌 Action Items:
• [ ] [Task] - [Owner] - [Due date]
```

**Example Interaction**:
```
User: "Create meeting notes for Q4 Planning"

AI: [Calls MeetingNotes with meetingName="Q4 Planning Session"]

Output:
  "I'll help you create meeting notes for 'Q4 Planning Session'.
  
  📅 Date: December 7, 2025
  
  Who attended the meeting?"

User: "Alice, Bob, Charlie"

AI: "Great! What were the main agenda items?"

User: "Review roadmap, discuss team capacity, assign Q4 goals"

AI: "What were the key points discussed?"

[... continues template filling ...]

AI: [Calls AddNote with formatted template]
  "Created meeting notes 'Q4 Planning Session' with ID m3e4t5i6 ✅"
```

**MCP Integration**:
- Marked with `[McpServerPrompt]` attribute
- Description: "Create structured meeting notes with template guidance"
- Can invoke AddNote tool when template is complete

---

## Prompt 5: CodeReviewNote

**Purpose**: Create structured code review notes with standard checklist

**Parameters**:
- `fileOrComponent` (string, required): The file, component, or module being reviewed
  - Example: "AuthenticationModule.cs", "User API endpoints"

**Returns**: Interactive template-filling workflow

**Behavior**:
1. Present code review template with fields:
   - File/component name (title)
   - Reviewer
   - Review date
   - Purpose/changes reviewed
   - Findings (issues, concerns, questions)
   - Recommendations
   - Approval status
2. Guide user through filling template
3. Once complete, call AddNote with formatted content

**Template Structure**:
```
Title: Code Review: [File/Component]

Content:
👤 Reviewer: [User provides]
📅 Date: [Auto-filled]
🎯 Purpose: [User provides]

🔍 Findings:
• [User provides issues/observations]

💡 Recommendations:
• [User provides suggestions]

✅ Status: [Approved / Approved with changes / Needs rework]
```

**Example Interaction**:
```
User: "Create code review note for AuthenticationModule.cs"

AI: [Calls CodeReviewNote with fileOrComponent="AuthenticationModule.cs"]

Output:
  "I'll help you document the code review for 'AuthenticationModule.cs'.
  
  👤 Who performed the review?"

User: "Alice"

AI: "What was the purpose of this review?"

User: "Security audit before production deployment"

AI: "What findings or issues did you identify?"

User: "Token validation missing expiry check, password hashing uses weak algorithm"

AI: "What are your recommendations?"

User: "Add expiry validation, upgrade to bcrypt for password hashing, add unit tests for auth flow"

AI: "What's the approval status? (Approved / Approved with changes / Needs rework)"

User: "Needs rework"

AI: [Calls AddNote with formatted template]
  "Created code review note 'Code Review: AuthenticationModule.cs' with ID c0d3r3v1 ✅"
```

**MCP Integration**:
- Marked with `[McpServerPrompt]` attribute
- Description: "Create structured code review notes with checklist"
- Can invoke AddNote tool when template is complete

---

## Prompt Design Principles

### User Guidance
- Prompts ask questions to gather information
- Provide examples when requesting input
- Confirm information before creating notes
- Offer to revise or edit before finalizing

### Template Consistency
- Use emojis for visual section headers (📅 📋 💬 ✅ etc.)
- Clear section structure for easy scanning
- Markdown formatting for readability
- Consistent field names across similar templates

### AI Interaction
- Prompts can call tools (AddNote, SearchNotes, etc.)
- Can use sampling to interact with user across multiple turns
- Provide helpful next steps after completion
- Error handling with graceful fallbacks

---

## MCP Integration

**Prompt Registration**:
- All prompts marked with `[McpServerPrompt]` attribute
- Each prompt has `[Description]` attribute explaining purpose
- Parameters have `[Description]` attributes for clarity

**Tool Invocation**:
- Prompts can invoke NotesTools methods
- Use MCP sampling for multi-turn conversations
- Coordinate between prompt guidance and tool execution

**Return Format**:
- Prompts return guidance strings, not objects
- Use formatting (emojis, bullets, structure) for clarity
- Provide clear next steps for user

---

## Contract Validation

| Requirement | Prompt Coverage |
|-------------|-----------------|
| FR-011 | All 5 prompts defined |
| QuickNote | Auto-title generation ✅ |
| SummarizeNotes | Overview with statistics ✅ |
| FindTopicNotes | Topic search with context ✅ |
| MeetingNotes | Structured meeting template ✅ |
| CodeReviewNote | Structured code review template ✅ |

---

## Prompts Contract Complete ✅

All 5 MCP prompts fully specified with parameters, behavior, templates, and interaction flows.
