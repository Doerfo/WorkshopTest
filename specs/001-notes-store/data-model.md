# Data Model: In-Memory Notes Store

**Feature Branch**: `001-notes-store`  
**Created**: December 7, 2025  
**Phase**: Phase 1 - Design & Contracts

## Purpose

Define the data entities, their relationships, validation rules, and state management for the in-memory notes store. This document provides the foundation for implementation without prescribing specific code.

---

## Entities

### NoteItem

Represents a single note with metadata.

**Attributes**:
- **Id** (string, required): Unique identifier, 8 characters from GUID (e.g., "abc12345")
  - Generated automatically by system
  - Immutable once created
  - Used for retrieval, updates, and deletion

- **Title** (string, required): User-provided note title
  - Mutable (can be updated)
  - Display text for note identification
  - Searchable field

- **Content** (string, required): User-provided note body
  - Mutable (can be updated)
  - Main note text
  - Searchable field

- **CreatedAt** (DateTime, required): Timestamp when note was created
  - Set automatically by system on creation
  - Immutable (never changes)
  - UTC timezone recommended
  - Used for ordering (newest first)

- **UpdatedAt** (DateTime, nullable): Timestamp when note was last updated
  - Null on initial creation
  - Set automatically by system on update operations
  - UTC timezone recommended
  - Distinguishes between "never updated" and "recently updated"

**Invariants**:
- Id must be unique across all notes
- CreatedAt must be set on creation and never change
- UpdatedAt must be null or >= CreatedAt
- If UpdatedAt is not null, an update operation occurred

---

## Relationships

**None** - NoteItem is a standalone entity with no relationships to other entities.

Each note is independent. No hierarchies, no tags, no categories per workshop simplicity principle.

---

## Validation Rules

### On Create (AddNote)

1. **Title**:
   - Must not be null or empty string
   - Must not be only whitespace
   - Error if invalid: "Title cannot be empty"

2. **Content**:
   - Must not be null or empty string
   - Must not be only whitespace
   - Error if invalid: "Content cannot be empty"

3. **Id**:
   - Generated automatically (8-char GUID prefix)
   - Must not collide with existing IDs (GUID ensures this)

4. **CreatedAt**:
   - Set automatically to current UTC time

5. **UpdatedAt**:
   - Set to null on creation

### On Update (UpdateNote)

1. **Id**:
   - Must exist in store
   - Error if not found: "Note with ID '{id}' not found ❌"

2. **Title** (if provided):
   - Same validation as create
   - If not provided (null), preserve existing title

3. **Content** (if provided):
   - Same validation as create
   - If not provided (null), preserve existing content

4. **UpdatedAt**:
   - Set automatically to current UTC time on any update

5. **CreatedAt**:
   - Must be preserved from original note (immutable)

### On Retrieve (GetNote)

1. **Id**:
   - Must not be null or empty
   - Must exist in store
   - Error if invalid: "Note ID cannot be empty"
   - Error if not found: "Note with ID '{id}' not found ❌"

### On Delete (DeleteNote)

1. **Id**:
   - Same validation as retrieve
   - Note is removed from store if exists
   - Error if not found: "Note with ID '{id}' not found ❌"

### On Search (SearchNotes)

1. **Query**:
   - May be empty (returns all notes)
   - Case-insensitive matching
   - Searches both title and content fields

### On List (ListNotes)

- No validation needed
- Returns empty collection if no notes exist

---

## State Transitions

### NoteItem Lifecycle

```
[Not Created] 
    |
    | AddNote(title, content)
    | - Generate ID
    | - Set CreatedAt = Now(UTC)
    | - Set UpdatedAt = null
    v
[Created]
    |
    ├─> GetNote(id) ────────────> [Retrieved] (no state change)
    |
    ├─> UpdateNote(id, ...) ────> [Updated]
    |   - Preserve Id                |
    |   - Preserve CreatedAt         |
    |   - Update Title/Content       |
    |   - Set UpdatedAt = Now(UTC) <─┘
    |                                 |
    |   (can update multiple times) ─┘
    |
    └─> DeleteNote(id) ──────────> [Deleted]
```

**State Properties**:
- **Created**: Note exists with CreatedAt set, UpdatedAt = null
- **Updated**: Note exists with CreatedAt set, UpdatedAt ≠ null
- **Deleted**: Note no longer exists in store (terminal state)

---

## Storage Model

### In-Memory Store

**Structure**: Dictionary-like key-value store
- **Key**: Note ID (string)
- **Value**: NoteItem entity

**Properties**:
- **Thread-safe**: Supports concurrent reads and writes
- **Volatile**: Data lost on server restart (in-memory only)
- **Performance**: O(1) lookup by ID, O(n) for search and list

### Operations

1. **Add**:
   - Insert new key-value pair
   - Key must be unique (enforced by dictionary)

2. **Get**:
   - Lookup by key
   - Return value if exists, error if not

3. **Update**:
   - Lookup by key
   - Replace value with updated entity
   - Preserve immutable fields

4. **Delete**:
   - Remove key-value pair
   - Error if key doesn't exist

5. **List**:
   - Retrieve all values
   - Order by CreatedAt descending (newest first)

6. **Search**:
   - Retrieve all values
   - Filter by query (title or content contains)
   - Order by CreatedAt descending (newest first)

---

## Ordering and Sorting

**Default Order**: Newest first (CreatedAt descending)

Applied to:
- ListNotes: Returns all notes newest first
- SearchNotes: Returns matching notes newest first

**Rationale**:
- Most recent notes are typically most relevant
- Consistent ordering across all multi-result operations
- Specified in FR-006 and user acceptance scenarios

---

## Concurrency Considerations

### Thread Safety Requirements

1. **Multiple Concurrent Adds**:
   - Each gets unique ID (GUID ensures uniqueness)
   - All succeed without conflicts

2. **Concurrent Read + Write**:
   - Reads don't block writes
   - Writes don't block reads
   - Eventually consistent (read may see old or new value)

3. **Concurrent Updates to Same Note**:
   - Last write wins
   - No merge logic (workshop simplicity)
   - Both updates succeed, last one persists

4. **Concurrent Delete + Read**:
   - Read may succeed or fail depending on timing
   - Acceptable for workshop scope

**Strategy**: Use thread-safe collection (ConcurrentDictionary in .NET)
- Provides lock-free reads
- Atomic updates for add/update/delete
- No manual locking required

---

## Data Examples

### Newly Created Note
```json
{
  "Id": "a3f7b2c1",
  "Title": "Meeting Ideas",
  "Content": "Discuss Q4 roadmap, review team capacity, plan sprint goals",
  "CreatedAt": "2025-12-07T14:30:00Z",
  "UpdatedAt": null
}
```

### Updated Note
```json
{
  "Id": "a3f7b2c1",
  "Title": "Q4 Planning Meeting",
  "Content": "Discuss Q4 roadmap, review team capacity, plan sprint goals, assign action items",
  "CreatedAt": "2025-12-07T14:30:00Z",
  "UpdatedAt": "2025-12-07T16:45:00Z"
}
```

### Multiple Notes (Ordered)
```json
[
  {
    "Id": "7d8e9f10",
    "Title": "Code Review: Auth Module",
    "Content": "Reviewed authentication flow, found security issue in token validation",
    "CreatedAt": "2025-12-07T16:00:00Z",
    "UpdatedAt": null
  },
  {
    "Id": "a3f7b2c1",
    "Title": "Q4 Planning Meeting",
    "Content": "Discuss Q4 roadmap, review team capacity, plan sprint goals",
    "CreatedAt": "2025-12-07T14:30:00Z",
    "UpdatedAt": "2025-12-07T16:45:00Z"
  },
  {
    "Id": "5b6c7d8e",
    "Title": "Daily Standup Notes",
    "Content": "Blocked on API docs, need design review by EOD",
    "CreatedAt": "2025-12-07T09:15:00Z",
    "UpdatedAt": null
  }
]
```

---

## Mapping to Functional Requirements

| Requirement | Data Model Support |
|-------------|-------------------|
| FR-001 | NoteItem with all 5 attributes defined |
| FR-002 | Id attribute with auto-generation strategy |
| FR-003 | CreatedAt and UpdatedAt with automatic timestamps |
| FR-004 | Add operation with validation rules |
| FR-005 | Get operation with ID lookup |
| FR-006 | List operation with CreatedAt ordering |
| FR-007 | Search operation with case-insensitive filtering |
| FR-008 | Update operation with timestamp preservation |
| FR-009 | Delete operation with removal logic |
| FR-012 | Validation rules for non-existent IDs |
| FR-013 | Search/List return empty collections when no matches |
| FR-014 | Ordering consistency via CreatedAt |
| FR-015 | Partial update support (null checks for title/content) |

---

## Implementation Notes

**This document is technology-agnostic**. Implementation in .NET will use:
- C# record type for NoteItem (immutability, value equality)
- ConcurrentDictionary<string, NoteItem> for storage
- DateTime.UtcNow for timestamps
- LINQ for search and ordering
- with-expressions for updates

See [research.md](./research.md) for technology-specific decisions.

---

## Data Model Complete ✅

All entities defined with attributes, validation rules, state transitions, and storage semantics. Ready for contract generation (Phase 1 continued).
