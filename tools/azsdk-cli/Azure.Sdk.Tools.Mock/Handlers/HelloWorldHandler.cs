// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Sdk.Tools.Mock.Handlers;

/// <summary>
/// Example custom handler for the azsdk_hello_world tool.
/// Demonstrates argument-based switching to return different mock responses
/// depending on the input, making the mock server more flexible for testing.
/// </summary>
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
