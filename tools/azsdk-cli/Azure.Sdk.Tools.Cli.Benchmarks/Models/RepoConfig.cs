// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Azure.Sdk.Tools.Cli.Benchmarks.Models;

/// <summary>
/// Configuration for a GitHub repository used in benchmarks.
/// </summary>
public class RepoConfig
{
    /// <summary>
    /// The owner of the repository (e.g., "Azure").
    /// </summary>
    public required string Owner { get; init; }

    /// <summary>
    /// The name of the repository (e.g., "azure-rest-api-specs").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional override for the owner, used when working with a fork (e.g., "chrisradek").
    /// </summary>
    public string? ForkOwner { get; init; }

    /// <summary>
    /// The branch, tag, or commit SHA to use. Defaults to "main".
    /// </summary>
    public string Ref { get; init; } = "main";

    /// <summary>
    /// Returns <see cref="ForkOwner"/> if set, otherwise <see cref="Owner"/>.
    /// </summary>
    public string EffectiveOwner => ForkOwner ?? Owner;

    /// <summary>
    /// Optional list of directory paths to sparse checkout.
    /// When set, only these directories (plus root-level files) will be materialized in the worktree.
    /// When null or empty, the full repository is checked out.
    /// Uses git sparse-checkout in cone mode.
    /// </summary>
    public string[]? SparseCheckoutPaths { get; init; }

    /// <summary>
    /// The HTTPS clone URL for the repository.
    /// </summary>
    public string CloneUrl => $"https://github.com/{EffectiveOwner}/{Name}.git";

    /// <summary>
    /// Returns a new <see cref="RepoConfig"/> with the specified ref, preserving all other properties.
    /// </summary>
    public RepoConfig WithRef(string newRef) => new()
    {
        Owner = Owner,
        Name = Name,
        ForkOwner = ForkOwner,
        Ref = newRef,
        SparseCheckoutPaths = SparseCheckoutPaths
    };

    /// <summary>
    /// Parses a repo string in the format "Owner/Name:Ref", "Owner/Name", or "Name".
    /// </summary>
    /// <returns>A tuple of (owner, name, gitRef) where owner and gitRef may be null.</returns>
    public static (string? Owner, string Name, string? Ref) ParseRepoString(string input)
    {
        var match = Regex.Match(input, @"^(?:(?<owner>[^/:\s]+)/)?(?<name>[^/:\s]+)(?::(?<ref>.+))?$");
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid repo format: '{input}'. Expected 'Owner/Name:Ref', 'Owner/Name', or 'Name'.", nameof(input));
        }

        var owner = match.Groups["owner"].Success ? match.Groups["owner"].Value : null;
        var name = match.Groups["name"].Value;
        var gitRef = match.Groups["ref"].Success ? match.Groups["ref"].Value : null;

        return (owner, name, gitRef);
    }
}
