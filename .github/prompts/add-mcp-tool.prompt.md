---
name: add-mcp-tool
description: Add a new MCP tool to the server based on a description
agent: agent
tools:
  ['vscode', 'execute', 'read', 'agent', 'edit', 'search', 'web', 'microsoft-docs/*', 'todo']
---

# Add MCP Tool

Create a new MCP Server tool based on the following description:

{{ input }}

## Requirements

1. **Tool Class**: Create or update the appropriate Tools class with `[McpServerToolType]`
2. **Tool Method**: Add a method with `[McpServerTool]` and clear `[Description]` attributes
3. **Parameters**: Define parameters with `[Description]` for AI discoverability
4. **Return Format**: Return user-friendly formatted strings (use emojis for status)
5. **Error Handling**: Include proper validation and error messages
6. **Service Integration**: If needed, inject and use appropriate services

## Code Style

- Use async/await for operations that may take time
- Follow C# naming conventions (PascalCase for methods)
- Keep tools focused on a single responsibility
- Use descriptive parameter names

Generate the complete implementation and update any necessary files (Program.cs for DI registration if new services are needed).