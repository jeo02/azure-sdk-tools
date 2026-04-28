// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Sdk.Tools.Cli.Models;
using Azure.Sdk.Tools.Cli.Models.Responses.ReleasePlan;

namespace Azure.Sdk.Tools.Mock.Handlers.ReleasePlan;

/// <summary>
/// Mock handler for azsdk_run_generate_sdk.
/// Switches on language — returns a queued pipeline response for known languages, default otherwise.
/// </summary>
public class RunGenerateSdkHandler : IMockToolHandler
{
    public string ToolName => "azsdk_run_generate_sdk";

    public CommandResponse Handle(Dictionary<string, object?>? arguments)
    {
        var language = arguments?.GetValueOrDefault("language")?.ToString() ?? "";

        return language.ToLowerInvariant() switch
        {
            ".net" => QueuedPipelineResponse(language),
            _ => MockToolFactory.GetDefaultResponse()
        };
    }

    private static ReleaseWorkflowResponse QueuedPipelineResponse(string language) => new()
    {
        Language = SdkLanguageHelpers.GetSdkLanguage(language),
        Status = "Queued",
        TypeSpecProject = "specification/contosowidgetmanager/Contoso.WidgetManager",
        Details =
        [
            $"SDK generation pipeline triggered for {language}",
            $"Pipeline build ID: 90001",
            "Monitor status using azsdk_get_pipeline_status"
        ]
    };
}
