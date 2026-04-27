# Mock MCP Server

The `--mock` flag starts the CLI as a mock MCP server that exposes the same tools as the real CLI but returns canned responses instead of executing real logic. Useful for benchmarking, evaluation, and integration testing without requiring live Azure services or dependencies.

## How It Works

When started with `mcp --mock`, the CLI takes a separate startup path that:

1. **Reflects** over all tool classes in `SharedOptions.ToolsList` to find methods marked with `[McpServerTool]`
2. **Creates** real `McpServerTool` instances to capture full metadata (name, description, input schema)
3. **Wraps** each tool in a `MockMcpServerTool` that intercepts all calls and routes them through a `MockToolFactory`
4. **Bypasses** all real service registrations (no Azure, GitHub, DevOps, or AI service dependencies)

When a tool is called:
- If a custom `IMockToolHandler` exists for that tool name → the handler produces the response
- Otherwise → a default success response is returned:
  ```json
  { "message": "Mock response for <tool_name>", "operation_status": "Succeeded" }
  ```

The MCP client sees the exact same tool list and schemas as the real CLI — only the responses differ.

## Running the Mock Server

### From the command line

```bash
dotnet run --project tools/azsdk-cli/Azure.Sdk.Tools.Cli -- mcp --mock
```

### As an MCP server in VS Code

Add to `.vscode/mcp.json`:

```json
{
  "servers": {
    "azure-sdk-mock": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "./tools/azsdk-cli/Azure.Sdk.Tools.Cli", "--configuration", "Debug", "--", "mcp", "--mock"]
    }
  }
}
```

## Adding a Custom Handler

To provide a tool-specific mock response instead of the default, create a class that implements `IMockToolHandler` in the `Mock/` directory.

### Step 1: Create the handler

```csharp
// Mock/MyToolHandler.cs
namespace Azure.Sdk.Tools.Cli.Mock;

public class MyToolHandler : IMockToolHandler
{
    // Must match the [McpServerTool(Name = "...")] value from the real tool
    public string ToolName => "azsdk_my_tool";

    public object Handle(Dictionary<string, object?>? arguments)
    {
        return new
        {
            message = "Custom mock response",
            operation_status = "Succeeded"
        };
    }
}
```

### Step 2: That's it

The `MockToolFactory` auto-discovers all `IMockToolHandler` implementations in the assembly at startup. No registration code is needed.

## Argument-Based Switching

Handlers receive the arguments passed by the MCP client. You can switch on these arguments to return different responses, making the mock flexible enough to simulate success, failure, and edge cases from a single handler.

### Example: `HelloWorldHandler`

The `azsdk_hello_world` handler demonstrates this pattern:

```csharp
public class HelloWorldHandler : IMockToolHandler
{
    public string ToolName => "azsdk_hello_world";

    public object Handle(Dictionary<string, object?>? arguments)
    {
        var message = arguments?.GetValueOrDefault("message")?.ToString() ?? "world";

        return message.ToLowerInvariant() switch
        {
            "error" => new
            {
                message = "Simulated error for testing",
                operation_status = "Failed",
                error_code = "MOCK_ERROR"
            },
            "slow" => new
            {
                message = "Simulated slow response",
                operation_status = "Succeeded",
                duration = 30000
            },
            _ => new
            {
                message = $"Hello, {message}!",
                operation_status = "Succeeded",
                duration = 1
            }
        };
    }
}
```

This lets callers control the mock behavior through input:
- `{"message": "error"}` → simulates a failure
- `{"message": "slow"}` → simulates a slow operation
- `{"message": "Alice"}` → normal success response

Use this pattern in any handler to test how your integration handles different scenarios without changing the mock server code.
