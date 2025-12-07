# Feature Specification: In-Memory Notes Store

**Feature Branch**: `001-notes-store`  
**Created**: December 7, 2025  
**Status**: Draft  
**Input**: User description: "Build an in-memory notes store for the CustomMcpServer"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Quick Note Capture (Priority: P1)

A user needs to quickly capture thoughts, ideas, or reminders without worrying about organization or structure. The system should automatically generate a unique identifier and timestamp for each note.

**Why this priority**: Core value proposition - enables immediate capture of information without friction. This is the fundamental use case that all other features build upon.

**Independent Test**: Can be fully tested by creating a note with title and content, then retrieving it by ID. Delivers immediate value as a basic note-taking system.

**Acceptance Scenarios**:

1. **Given** the notes store is empty, **When** a user adds a note with title "Meeting Ideas" and content "Discuss Q4 roadmap", **Then** the system creates a note with a unique ID, stores the title and content, and records the current timestamp as both created and updated time
2. **Given** a note exists, **When** a user retrieves the note by its ID, **Then** the system returns the note with ID, title, content, created timestamp, and updated timestamp
3. **Given** multiple notes exist, **When** a user adds a new note, **Then** the new note receives a unique ID that doesn't conflict with existing notes

---

### User Story 2 - Browse and Search Notes (Priority: P2)

A user needs to find specific information among their notes by either browsing all notes or searching for specific keywords in titles or content.

**Why this priority**: Essential for notes to be useful beyond initial capture. Without search and list capabilities, notes become write-only, limiting their value.

**Independent Test**: Can be fully tested by creating multiple notes, listing them to verify order (newest first), and searching for specific terms. Delivers value as a searchable knowledge base.

**Acceptance Scenarios**:

1. **Given** multiple notes exist created at different times, **When** a user lists all notes, **Then** the system returns notes ordered by creation date with newest first
2. **Given** notes contain various titles and content, **When** a user searches for "roadmap", **Then** the system returns all notes where either title or content contains "roadmap" (case-insensitive)
3. **Given** a search query matches no notes, **When** a user performs the search, **Then** the system returns an empty result set
4. **Given** the notes store is empty, **When** a user lists all notes, **Then** the system returns an empty list

---

### User Story 3 - Update and Manage Notes (Priority: P3)

A user needs to modify existing notes to correct information, add details, or remove outdated notes.

**Why this priority**: Important for maintaining note accuracy and relevance over time, but users can work around this by creating new notes if update/delete isn't available initially.

**Independent Test**: Can be fully tested by creating a note, updating its title and/or content, verifying the updated timestamp changed, and deleting notes. Delivers value as a complete note management system.

**Acceptance Scenarios**:

1. **Given** a note exists, **When** a user updates the note's title and/or content, **Then** the system saves the changes and updates the "updated timestamp" to the current time while preserving the original "created timestamp"
2. **Given** a note exists, **When** a user deletes the note by ID, **Then** the system removes the note and it no longer appears in lists or searches
3. **Given** a user attempts to update a non-existent note ID, **When** the update is attempted, **Then** the system indicates the note was not found
4. **Given** a user attempts to delete a non-existent note ID, **When** the delete is attempted, **Then** the system indicates the note was not found

---

### User Story 4 - Guided Note Creation (Priority: P4)

A user working through an AI assistant wants structured prompts that help create well-organized notes for specific purposes (meeting notes, code reviews, quick thoughts).

**Why this priority**: Enhances user experience and note quality, but not essential for basic functionality. Provides convenience and structure for common use cases.

**Independent Test**: Can be fully tested by invoking each prompt template and verifying it guides the user through appropriate note creation. Delivers value as an intelligent note-taking assistant.

**Acceptance Scenarios**:

1. **Given** a user needs to capture a quick thought, **When** they use the QuickNote prompt, **Then** the system captures the content and generates an appropriate title automatically
2. **Given** a user wants an overview, **When** they use the SummarizeNotes prompt, **Then** the system provides a summary of all existing notes
3. **Given** a user needs to find notes about a topic, **When** they use the FindTopicNotes prompt with a search term, **Then** the system returns relevant notes matching that topic
4. **Given** a user is preparing for a meeting, **When** they use the MeetingNotes prompt, **Then** the system provides a structured template with fields for date, attendees, agenda, discussion, and action items
5. **Given** a user is documenting a code review, **When** they use the CodeReviewNote prompt, **Then** the system provides a structured template with fields for file/component, reviewer, findings, and recommendations

---

### Edge Cases

- What happens when a user attempts to retrieve a note with an ID that doesn't exist?
- How does the system handle searches with special characters or very long search queries?
- What happens when a user tries to create a note with empty title or content?
- How does the system handle concurrent operations (multiple adds, updates, or deletes happening simultaneously)?
- What happens when the in-memory store reaches system memory limits?
- How does the system handle notes with very large content (e.g., 10MB of text)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST store notes in memory with each note containing: unique ID, title, content, created timestamp, and updated timestamp
- **FR-002**: System MUST automatically generate unique IDs for new notes without requiring user input
- **FR-003**: System MUST automatically record created and updated timestamps using the current system time
- **FR-004**: System MUST provide an AddNote operation that accepts title and content and returns the created note with its generated ID and timestamps
- **FR-005**: System MUST provide a GetNote operation that accepts a note ID and returns the complete note if it exists
- **FR-006**: System MUST provide a ListNotes operation that returns all notes ordered by created timestamp (newest first)
- **FR-007**: System MUST provide a SearchNotes operation that accepts a query string and returns notes where either title or content contains the query (case-insensitive matching)
- **FR-008**: System MUST provide an UpdateNote operation that accepts a note ID and new title and/or content, updates the note, and updates the "updated timestamp" while preserving the "created timestamp"
- **FR-009**: System MUST provide a DeleteNote operation that accepts a note ID and removes the note from storage
- **FR-010**: System MUST expose six MCP tools: AddNote, GetNote, ListNotes, SearchNotes, UpdateNote, and DeleteNote
- **FR-011**: System MUST expose five MCP prompts: QuickNote (auto-generate title), SummarizeNotes (overview of all notes), FindTopicNotes (search by topic), MeetingNotes (structured meeting template), and CodeReviewNote (structured code review template)
- **FR-012**: System MUST return appropriate error responses when operations reference non-existent note IDs
- **FR-013**: System MUST handle empty search results gracefully by returning an empty collection
- **FR-014**: System MUST maintain note ordering consistency when multiple notes are created in quick succession
- **FR-015**: System MUST allow partial updates where users can update only title, only content, or both in the UpdateNote operation

### Key Entities

- **Note**: Represents a single note with auto-generated unique identifier, user-provided title and content, and system-managed timestamps for creation and last update. Notes are independent entities with no relationships to other entities.
- **NoteID**: Unique identifier for each note, automatically generated by the system upon note creation, used for retrieval, update, and deletion operations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new note and retrieve it by ID in under 2 seconds
- **SC-002**: Search operations return results within 1 second for stores containing up to 10,000 notes
- **SC-003**: List operations return properly ordered notes (newest first) within 1 second for stores containing up to 10,000 notes
- **SC-004**: Update operations complete within 1 second and correctly preserve the original created timestamp
- **SC-005**: All six MCP tools (AddNote, GetNote, ListNotes, SearchNotes, UpdateNote, DeleteNote) are accessible and functional through the MCP protocol
- **SC-006**: All five MCP prompts (QuickNote, SummarizeNotes, FindTopicNotes, MeetingNotes, CodeReviewNote) provide appropriate guidance and execute successfully
- **SC-007**: 100% of operations on non-existent note IDs return clear error messages indicating the note was not found
- **SC-008**: Case-insensitive search successfully finds notes regardless of query case (e.g., "roadmap", "ROADMAP", "RoadMap" all return the same results)
- **SC-009**: System handles concurrent operations without data corruption or lost updates
- **SC-010**: Users can successfully complete the full note lifecycle (create, read, update, delete) for a single note in under 30 seconds
