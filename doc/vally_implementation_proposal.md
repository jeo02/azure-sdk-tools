# Vally Implementation Proposal

## Overview

This proposal outlines how we will adopt [Vally](https://vally.dev) as the evaluation framework for the Azure SDK CLI, replacing the current `Azure.Sdk.Tools.Cli.Evaluations` and `Azure.Sdk.Tools.Cli.Benchmarks` projects. Vally gives us a unified pipeline, built-in skill linting, grader taxonomy, and multi-trial scoring — all things we built partially or planned to build ourselves.

## What We're Replacing

| Current                                | Replacement                          |
|----------------------------------------|--------------------------------------|
| `Azure.Sdk.Tools.Cli.Evaluations`      | Vally evals for MCP tools            |
| `Azure.Sdk.Tools.Cli.Benchmarks`       | Vally evals (outer-loop / nightly)   |
| Custom .NET eval framework + NUnit     | Vally CLI + YAML eval specs          |
| Custom benchmark runner + report gen   | Vally scoring + multi-trial metrics  |

## Priority System

Evals are tagged by priority to control when they run:

| Priority | When it runs       | What it covers                                    |
|----------|--------------------|---------------------------------------------------|
| **p0**   | Every PR (gate CI) | Linting + fast static/complex-static graders      |
| **p1**   | Nightly (schedule) | Longer-running evals, real agent execution         |
| **p2**   | Local only         | Exploratory, expensive, or experimental evals      |

## CI Structure

### PR Gate (p0)

Runs on every pull request. Must be fast and deterministic — no LLM calls, no real agent execution.

- `vally lint .` — skill spec compliance, file reference validation, orphan detection
- `vally eval --suite ci-gate` — static and complex-static graders only

### Nightly (p0 + p1)

Runs on a schedule. Includes everything from the PR gate plus longer-running evals.

- `vally eval --suite nightly` — includes p0 and p1 evals
- Real agent execution via Copilot SDK executor
- Multi-trial runs for statistical confidence

### Local (p0 + p1 + p2)

Everything, including exploratory evals that shouldn't run in CI.

- `vally eval --suite local`

## Two Separate Configurations

Skills and MCP tools live in different places, have different CI pipelines, and test different things. Each gets its own Vally configuration.

### 1. MCP Tools (`tools/azsdk-cli/`)

Replaces the current Evaluations and Benchmarks projects. Eval files live alongside tool directories under the existing `evals/` area.

```
tools/azsdk-cli/
├── .vally.yaml
└── evals/
    ├── ReleasePlan/
    │   ├── eval.yaml
    │   └── trigger.eval.yaml
    ├── Pipeline/
    │   └── trigger.eval.yaml
    ├── TypeSpec/
    │   └── eval.yaml
    └── GitHub/
        └── trigger.eval.yaml
```

**Base configuration (`.vally.yaml`):**

```yaml
# Vally configuration for Azure SDK CLI MCP tool evaluations
# See: https://vally.dev/reference/vally-config

paths:
  skills: ["skills/"]
  evals: ["evals/"]
  results: results/
  evalFilenames: ["eval.yaml", "*.eval.yaml"]

environments:
  azsdk-mcp:
    mcpServers:
      azure-sdk-mcp:
        type: stdio
        command: dotnet
        args: ["run", "--project", "../../tools/azsdk-cli/Azure.Sdk.Tools.Cli", "--", "start"]
        timeout: 30000
        env:
          AZSDKTOOLS_AGENT_TESTING: "false"
          AZSDKTOOLS_COLLECT_TELEMETRY: "false"

# p0: Gate CI, p1: Nightly, p2: Exploratory/Only local
suites:
  ci-gate:
    filter: { priority: [p0] }
  nightly:
    filter: { priority: [p0, p1] }
  local:
    filter: { priority: [p0, p1, p2] }
```

### 2. Skills (`.github/skills/`)

Skills currently live under `.github/skills/` and are synced across repositories. Evaluations for each skill live alongside the skill definition.

```
.github/skills/
├── .vally.yaml
├── prepare-release-plan/
│   └── evals/
│       ├── eval.yaml
│       └── trigger.eval.yaml
├── pipeline-troubleshooting/
│   └── evals/
│       └── eval.yaml
├── sdk-release/
│   └── evals/
│       └── trigger.eval.yaml
└── skill-authoring/
    └── evals/
        └── eval.yaml
```

The skills Vally config should be **shareable** so it can run in any repository that syncs these skills. The CI setup for skills will leverage the existing `eng/common/pipelines/templates/` infrastructure.

## Eval File Naming Convention

Eval files must be named either:

- `eval.yaml` — the default for a single eval file per directory
- `*.eval.yaml` — for splitting evals by concern (e.g. `trigger.eval.yaml`, `grounding.eval.yaml`)

This prevents cramming too many stimuli into a single file and keeps evals focused and reviewable.

## Pipeline Integration

We leverage the existing pipeline infrastructure under `eng/common/pipelines/templates/` and the `tools/azsdk-cli/ci.yml` pipeline.

### PR Gate Step

```yaml
- script: |
    npx vally lint .
    npx vally eval --suite ci-gate
  displayName: 'Vally Evals (PR Gate)'
```

### Nightly Step

```yaml
- script: |
    npx vally eval --suite nightly
  displayName: 'Vally Evals (Nightly)'
```

## Migration Path

1. **Set up Vally configs** — Create `.vally.yaml` for both MCP tools and skills.
2. **Convert existing eval scenarios** — Translate current `trigger.eval.yaml` files and C# scenario definitions into Vally stimulus format.
3. **Wire up CI** — Replace the current `dotnet test --filter 'Category==Evals'` and benchmark runner steps with Vally CLI calls.
4. **Validate parity** — Run both old and new in parallel until confident in coverage.
5. **Remove old projects** — Delete `Azure.Sdk.Tools.Cli.Evaluations` and `Azure.Sdk.Tools.Cli.Benchmarks` once Vally is fully adopted.

## Open Items

- **Mock executor** — Vally's mock executor is planned but not yet implemented. This is critical for deterministic p0 CI runs that don't make real API calls. Until available, p0 evals are limited to lint + static graders.
- **Custom grader plugins** — We want to experiment with our own graders (e.g. tool-selection accuracy). The plugin system for external graders is not yet on the Vally roadmap but would be valuable.
- **Skills CI sharing** — The skills Vally config needs to work across repos. We need to decide how the config is synced alongside skills.
