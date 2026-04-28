// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Sdk.Tools.Cli.Models;

namespace Azure.Sdk.Tools.Mock.Handlers;

/// <summary>
/// Example custom handler for the azsdk_hello_world tool.
/// Demonstrates argument-based switching to return different mock responses
/// depending on the input, making the mock server more flexible for testing.
/// </summary>
public class HelloWorldHandler : IMockToolHandler
{
    public string ToolName => "azsdk_hello_world";

    public DefaultCommandResponse Handle(Dictionary<string, object?>? arguments)
    {
        var message = arguments?.GetValueOrDefault("message")?.ToString() ?? "world";

        return message.ToLowerInvariant() switch
        {
            "error" => new DefaultCommandResponse
            {
                Message = "Simulated error for testing",
                ResponseError = "MOCK_ERROR"
            },
            "slow" => new DefaultCommandResponse
            {
                Message = "Simulated slow response",
                Duration = 30000
            },
            _ => MockToolFactory.GetDefaultResponse()
        };
    }
}
