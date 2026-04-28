// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Sdk.Tools.Cli.Models;
using Azure.Sdk.Tools.Cli.Models.Responses.ReleasePlan;

namespace Azure.Sdk.Tools.Mock.Handlers.ReleasePlan;

/// <summary>
/// Mock handler for azsdk_get_sdk_pull_request_link.
/// Switches on language — returns a PR link response for known languages, default otherwise.
/// </summary>
public class GetSdkPullRequestLinkHandler : IMockToolHandler
{
    public string ToolName => "azsdk_get_sdk_pull_request_link";

    public CommandResponse Handle(Dictionary<string, object?>? arguments)
    {
        var language = arguments?.GetValueOrDefault("language")?.ToString() ?? "";

        return language.ToLowerInvariant() switch
        {
            ".net" => CompletedPrLinkResponse(language),
            _ => MockToolFactory.GetDefaultResponse()
        };
    }

    private static ReleaseWorkflowResponse CompletedPrLinkResponse(string language) => new()
    {
        Language = SdkLanguageHelpers.GetSdkLanguage(language),
        Status = "Completed",
        TypeSpecProject = "specification/contosowidgetmanager/Contoso.WidgetManager",
        Details =
        [
            $"SDK pull request created for {language}",
            $"PR URL: https://github.com/Azure/azure-sdk-for-net/pull/45001"
        ]
    };
}
