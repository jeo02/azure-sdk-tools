// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import type {
    Executor,
    ExecutorOptions,
    Stimulus,
    Trajectory,
    TrajectoryEvent,
} from "@microsoft/vally";

/**
 * A lightweight mock executor for local testing and CI.
 *
 * Returns a deterministic trajectory without calling any LLM or
 * spawning external processes, making it fast (sub-millisecond)
 * and free of network dependencies.
 */
export class TempMockExecutor implements Executor {
    name = "temp-mock";

    async execute(stimulus: Stimulus, options: ExecutorOptions): Promise<Trajectory> {
        const startedAt = new Date();

        const events: TrajectoryEvent[] = [
            {
                type: "turn_start",
                timestamp: startedAt,
                data: { turnId: "turn-0" },
            },
            {
                type: "user_message",
                timestamp: startedAt,
                data: {
                    content: stimulus.prompt,
                    agent_mode: undefined,
                },
            },
            {
                type: "assistant_message",
                timestamp: startedAt,
                data: {
                    content: `Mock response for: ${stimulus.prompt}`,
                },
            },
            {
                type: "turn_end",
                timestamp: startedAt,
                data: { turnId: "turn-0" },
            },
        ];

        const completedAt = new Date();
        const wallTimeMs = completedAt.getTime() - startedAt.getTime();

        return {
            id: crypto.randomUUID(),
            stimulus,
            events,
            output: `Mock response for: ${stimulus.prompt}`,
            workDir: options.workDir,
            metadata: {
                startedAt,
                completedAt,
                model: options.model ?? "temp-mock-model",
                executor: this.name,
                sessionID: crypto.randomUUID(),
                skillsLoaded: [],
            },
            metrics: {
                tokenUsage: {
                    inputTokens: 0,
                    outputTokens: 0,
                    totalTokens: 0,
                    cacheReadTokens: 0,
                    cacheWriteTokens: 0,
                    callCount: 0,
                    byModel: {},
                },
                toolCallCount: 0,
                toolCallBreakdown: {},
                skillActivationCount: 0,
                skillActivationBreakdown: {},
                turnCount: 1,
                wallTimeMs,
                errorCount: 0,
            },
        };
    }

    async shutdown(): Promise<void> {
        // No resources to release.
    }
}
