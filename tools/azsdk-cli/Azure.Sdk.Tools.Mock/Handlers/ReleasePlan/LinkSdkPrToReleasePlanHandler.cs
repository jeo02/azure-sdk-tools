// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Sdk.Tools.Cli.Models;
using Azure.Sdk.Tools.Cli.Models.Responses.ReleasePlan;

namespace Azure.Sdk.Tools.Mock.Handlers.ReleasePlan;

/// <summary>
/// Mock handler for azsdk_link_sdk_pull_request_to_release_plan.
/// Switches on language — returns a linked response for known languages, default otherwise.
/// </summary>
public class LinkSdkPrToReleasePlanHandler : IMockToolHandler
{
    public string ToolName => "azsdk_link_sdk_pull_request_to_release_plan";

    public CommandResponse Handle(Dictionary<string, object?>? arguments)
    {
        var language = arguments?.GetValueOrDefault("language")?.ToString() ?? "";
        var prUrl = arguments?.GetValueOrDefault("pullRequestUrl")?.ToString() ?? "";

        return language.ToLowerInvariant() switch
        {
            ".net" => LinkedPrResponse(language, prUrl),
            _ => MockToolFactory.GetDefaultResponse()
        };
    }

    private static ReleaseWorkflowResponse LinkedPrResponse(string language, string prUrl) => new()
    {
        Language = SdkLanguageHelpers.GetSdkLanguage(language),
        Status = "Linked",
        TypeSpecProject = "specification/contosowidgetmanager/Contoso.WidgetManager",
        Details =
        [
            $"SDK pull request linked to release plan for {language}",
            $"PR: {prUrl}"
        ]
    };
}
