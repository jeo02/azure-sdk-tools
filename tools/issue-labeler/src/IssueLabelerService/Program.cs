// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureRagService;
using Hubbup.MikLabelModel;
using Azure.Identity;
using System;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;

var credential = new DefaultAzureCredential();

var builder = new ConfigurationBuilder();
builder.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri("https://gh-triage-app-config-test.azconfig.io"), credential);
});

var config2 = builder.Build();

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        var config = context.Configuration;
        services.AddSingleton(config);
        services.AddSingleton(config2);

        services.AddSingleton<ChatClient>(sp =>
        {
            var openAIEndpoint = new Uri(config["OpenAIEndpoint"]);
            
            var openAIClient = new AzureOpenAIClient(openAIEndpoint, credential);
            var modelName = config["OpenAIModelName"];
            return openAIClient.GetChatClient(modelName);
        });

        services.AddSingleton<SearchIndexClient>(sp =>
        {
            var searchEndpoint = new Uri(config["SearchEndpoint"]);
            return new SearchIndexClient(searchEndpoint, credential);
        });

        services.AddSingleton<TriageRag>();
        services.AddSingleton<IModelHolderFactoryLite, ModelHolderFactoryLite>();
        services.AddSingleton<ILabelerLite, LabelerLite>();
    })
    .Build();

host.Run();
