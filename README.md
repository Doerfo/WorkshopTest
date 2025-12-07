# .NET MCP Server Workshop Template

A comprehensive template for building Model Context Protocol (MCP) servers using .NET. This project demonstrates how to create a modern MCP server with both stdio and HTTP transport capabilities, enabling seamless integration with AI assistants and development tools.

## Features

- **MCP Server Implementation**: Full-featured MCP server built on .NET
- **Dual Transport Support**:
  - **stdio Transport**: For local process communication and integration with development tools
  - **HTTP Transport**: For network-based communication and web service integration
- **Modern .NET Architecture**: Leverages .NET 10 SDK features and best practices
- **VS Code Integration**: Seamless setup for use with Visual Studio Code and GitHub Copilot
- **Extensible Design**: Easy to customize and extend with your own tools and resources

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK**: Download and install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
  - Verify installation: `dotnet --version` (should show 10.x or higher)
- **Visual Studio Code**: Download from [code.visualstudio.com](https://code.visualstudio.com/)
- **GitHub Copilot Extension**: Install from the VS Code marketplace
  - Requires an active GitHub Copilot subscription

## Getting Started

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/Doerfo/WorkshopTemplate.git
   cd WorkshopTemplate
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

### Running the Server

#### Using stdio Transport

Run the server with stdio transport for local integration:

```bash
dotnet run --project <YourProjectName>
```

#### Using HTTP Transport

Run the server with HTTP transport for network access:

```bash
dotnet run --project <YourProjectName> -- --transport http --port 5000
```

The server will be accessible at `http://localhost:5000`.

## Configuration

### VS Code MCP Configuration

To integrate this MCP server with VS Code and GitHub Copilot, create or update the MCP configuration file:

**Location**: `~/.vscode/mcp.json` (Linux/macOS) or `%USERPROFILE%\.vscode\mcp.json` (Windows)

**Configuration**:

```json
{
  "mcpServers": {
    "dotnet-mcp-server": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/WorkshopTemplate/<YourProjectName>"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Configuration Options**:

- **command**: The executable to run (`dotnet`)
- **args**: Arguments passed to the command (includes the project path)
- **env**: Environment variables for the server process

### Alternative: HTTP Transport Configuration

For HTTP-based communication:

```json
{
  "mcpServers": {
    "dotnet-mcp-server-http": {
      "url": "http://localhost:5000",
      "transport": "http"
    }
  }
}
```

## Project Structure

```
WorkshopTemplate/
├── .gitignore          # .NET-specific ignore patterns
├── README.md           # This file
└── <YourProject>/      # Your MCP server implementation
    ├── Program.cs      # Main entry point
    ├── *.csproj        # Project configuration
    └── ...             # Additional source files
```

## Development Tips

- **Hot Reload**: Use `dotnet watch run` for automatic reloading during development
- **Debugging**: Attach the VS Code debugger to troubleshoot issues
- **Logging**: Configure logging levels in `appsettings.json` or environment variables
- **Testing**: Create unit tests in a separate test project

## Additional Resources

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [GitHub Copilot Documentation](https://docs.github.com/copilot)

## License

This template is provided as-is for educational and development purposes.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.