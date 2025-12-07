# MCP Tools Contract: Notes Management

**Feature**: In-Memory Notes Store  
**Type**: MCP Tools (6 operations)  
**Protocol**: Model Context Protocol (MCP)

## Overview

This contract defines the 6 MCP tools that expose note management operations to AI assistants and MCP clients. Each tool is a callable function with typed parameters and return values.

---

## Tool 1: AddNote

**Purpose**: Create a new note with automatic ID and timestamp generation

**Parameters**:
- `title` (string, required): The title of the note
  - Must not be empty or whitespace-only
  - Example: "Meeting Ideas"
  
- `content` (string, required): The body content of the note
  - Must not be empty or whitespace-only
  - Example: "Discuss Q4 roadmap, review team capacity"

**Returns**: Formatted string
- Success: "Added note '{title}' with ID {id} ✅"
- Error: "{error message} ❌"

**Behavior**:
1. Validate title and content are not empty
2. Generate unique 8-character ID
3. Record current UTC timestamp as CreatedAt
4. Set UpdatedAt to null
5. Store note in memory
6. Return success message with ID

**Example**:
```
Input: 
  title: "Meeting Ideas"
  content: "Discuss Q4 roadmap"

Output:
  "Added note 'Meeting Ideas' with ID a3f7b2c1 ✅"
```

**Error Cases**:
- Empty title: "Title cannot be empty ❌"
- Empty content: "Content cannot be empty ❌"

---

## Tool 2: GetNote

**Purpose**: Retrieve a specific note by its ID

**Parameters**:
- `id` (string, required): The unique identifier of the note
  - Must be a valid note ID that exists in the store
  - Example: "a3f7b2c1"

**Returns**: Formatted string
- Success: Multi-line formatted note with all fields
- Error: "Note with ID '{id}' not found ❌"

**Behavior**:
1. Validate ID is not empty
2. Lookup note in store by ID
3. Format note details for display
4. Return formatted output

**Example**:
```
Input:
  id: "a3f7b2c1"

Output:
  "📝 Note a3f7b2c1
  
  Title: Meeting Ideas
  Content: Discuss Q4 roadmap, review team capacity
  
  Created: 2025-12-07 14:30 UTC
  Updated: Never"
```

**Error Cases**:
- Empty ID: "Note ID cannot be empty ❌"
- Not found: "Note with ID 'xyz999' not found ❌"

---

## Tool 3: ListNotes

**Purpose**: Retrieve all notes ordered by creation date (newest first)

**Parameters**: None

**Returns**: Formatted string
- Success: Numbered list of all notes with ID, title, and timestamps
- Empty: "No notes found. Create your first note! 📝"

**Behavior**:
1. Retrieve all notes from store
2. Order by CreatedAt descending (newest first)
3. Format as numbered list
4. Return formatted output

**Example**:
```
Output:
  "📝 All Notes (3 total)
  
  1. [7d8e9f10] Code Review: Auth Module
     Created: 2025-12-07 16:00 UTC | Updated: Never
  
  2. [a3f7b2c1] Q4 Planning Meeting
     Created: 2025-12-07 14:30 UTC | Updated: 2025-12-07 16:45 UTC
  
  3. [5b6c7d8e] Daily Standup Notes
     Created: 2025-12-07 09:15 UTC | Updated: Never"
```

**Edge Cases**:
- No notes: "No notes found. Create your first note! 📝"

---

## Tool 4: SearchNotes

**Purpose**: Find notes where title or content contains the search query (case-insensitive)

**Parameters**:
- `query` (string, required): The search term
  - Case-insensitive matching
  - Searches both title and content fields
  - Example: "roadmap"

**Returns**: Formatted string
- Success: "Found {count} note(s) matching '{query}' 🔍" + formatted list
- No matches: "No notes found matching '{query}' 🔍"

**Behavior**:
1. Retrieve all notes from store
2. Filter where title OR content contains query (case-insensitive)
3. Order results by CreatedAt descending
4. Format as numbered list with match context
5. Return formatted output

**Example**:
```
Input:
  query: "roadmap"

Output:
  "Found 2 note(s) matching 'roadmap' 🔍
  
  1. [a3f7b2c1] Q4 Planning Meeting
     Content: Discuss Q4 **roadmap**, review team capacity...
     Created: 2025-12-07 14:30 UTC
  
  2. [9c1d2e3f] Product Vision
     Title: 2025 Product **Roadmap** Draft
     Created: 2025-12-06 11:20 UTC"
```

**Edge Cases**:
- Empty query: Returns all notes (same as ListNotes)
- No matches: "No notes found matching '{query}' 🔍"
- Special characters: Treated as literal characters in search

---

## Tool 5: UpdateNote

**Purpose**: Modify an existing note's title and/or content

**Parameters**:
- `id` (string, required): The unique identifier of the note to update
  - Must exist in store
  - Example: "a3f7b2c1"

- `title` (string, optional): New title for the note
  - If provided, must not be empty or whitespace-only
  - If null/not provided, existing title is preserved
  - Example: "Q4 Planning Meeting"

- `content` (string, optional): New content for the note
  - If provided, must not be empty or whitespace-only
  - If null/not provided, existing content is preserved
  - Example: "Discuss Q4 roadmap, review team capacity, assign action items"

**Returns**: Formatted string
- Success: "Updated note '{title}' (ID: {id}) ✅"
- Error: "{error message} ❌"

**Behavior**:
1. Validate ID exists in store
2. Validate any provided title/content not empty
3. Update specified fields (preserve others)
4. Preserve CreatedAt timestamp
5. Set UpdatedAt to current UTC timestamp
6. Return success message

**Example**:
```
Input:
  id: "a3f7b2c1"
  title: "Q4 Planning Meeting"
  content: null (preserve existing)

Output:
  "Updated note 'Q4 Planning Meeting' (ID: a3f7b2c1) ✅"
```

**Error Cases**:
- Not found: "Note with ID '{id}' not found ❌"
- Empty title: "Title cannot be empty ❌"
- Empty content: "Content cannot be empty ❌"
- No changes: "At least one of title or content must be provided ❌"

---

## Tool 6: DeleteNote

**Purpose**: Remove a note from the store permanently

**Parameters**:
- `id` (string, required): The unique identifier of the note to delete
  - Must exist in store
  - Example: "a3f7b2c1"

**Returns**: Formatted string
- Success: "Deleted note '{title}' (ID: {id}) ✅"
- Error: "Note with ID '{id}' not found ❌"

**Behavior**:
1. Validate ID exists in store
2. Retrieve note title for confirmation message
3. Remove note from store
4. Return success message

**Example**:
```
Input:
  id: "a3f7b2c1"

Output:
  "Deleted note 'Meeting Ideas' (ID: a3f7b2c1) ✅"
```

**Error Cases**:
- Empty ID: "Note ID cannot be empty ❌"
- Not found: "Note with ID '{id}' not found ❌"

---

## MCP Integration

**Tool Registration**:
- All tools marked with `[McpServerTool]` attribute
- All parameters have `[Description]` attributes for AI discoverability
- Each tool method has `[Description]` attribute explaining purpose

**Parameter Injection**:
- Tools receive `NotesService` instance via dependency injection
- MCP SDK automatically resolves and injects service parameter

**Error Handling**:
- Validation errors throw `McpProtocolException` with `InvalidParams` code
- Service errors throw `McpProtocolException` with `InternalError` code
- All exceptions include user-friendly error messages

**Return Format**:
- All tools return formatted strings (not objects)
- Use emojis for visual indicators (✅ ❌ 📝 🔍)
- Multi-line formatting for readability in chat interfaces

---

## Contract Validation

| Requirement | Tool Coverage |
|-------------|---------------|
| FR-004 | AddNote |
| FR-005 | GetNote |
| FR-006 | ListNotes |
| FR-007 | SearchNotes |
| FR-008 | UpdateNote |
| FR-009 | DeleteNote |
| FR-010 | All 6 tools defined |
| FR-012 | Error responses for invalid IDs |
| FR-013 | Empty results handled gracefully |
| FR-015 | Partial updates supported |

---

## Tools Contract Complete ✅

All 6 MCP tools fully specified with parameters, return values, behavior, and error handling.
