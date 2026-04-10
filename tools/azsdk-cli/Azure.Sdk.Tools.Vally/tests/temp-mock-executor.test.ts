// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { describe, it, expect } from "vitest";
import { TempMockExecutor } from "../src/executor/temp-mock-executor.js";

describe("TempMockExecutor", () => {
    const executor = new TempMockExecutor();

    it("has the correct name", () => {
        expect(executor.name).toBe("temp-mock");
    });

    it("returns a trajectory with the stimulus prompt echoed", async () => {
        const stimulus = { name: "test-case", prompt: "Do something useful" };
        const options = { workDir: "/tmp/test", timeout: 5000 };

        const trajectory = await executor.execute(stimulus, options);

        expect(trajectory.output).toBe("Mock response for: Do something useful");
        expect(trajectory.stimulus).toBe(stimulus);
        expect(trajectory.workDir).toBe("/tmp/test");
    });

    it("populates metadata correctly", async () => {
        const stimulus = { name: "meta-test", prompt: "Hello" };
        const options = { workDir: "/tmp/test", timeout: 5000, model: "my-model" };

        const trajectory = await executor.execute(stimulus, options);

        expect(trajectory.metadata.executor).toBe("temp-mock");
        expect(trajectory.metadata.model).toBe("my-model");
        expect(trajectory.metadata.startedAt).toBeInstanceOf(Date);
        expect(trajectory.metadata.completedAt).toBeInstanceOf(Date);
    });

    it("defaults model to temp-mock-model when not specified", async () => {
        const stimulus = { name: "default-model", prompt: "Test" };
        const options = { workDir: "/tmp/test", timeout: 5000 };

        const trajectory = await executor.execute(stimulus, options);

        expect(trajectory.metadata.model).toBe("temp-mock-model");
    });

    it("produces zero-cost metrics", async () => {
        const stimulus = { name: "metrics-test", prompt: "Check metrics" };
        const options = { workDir: "/tmp/test", timeout: 5000 };

        const trajectory = await executor.execute(stimulus, options);

        expect(trajectory.metrics.tokenUsage.totalTokens).toBe(0);
        expect(trajectory.metrics.toolCallCount).toBe(0);
        expect(trajectory.metrics.errorCount).toBe(0);
        expect(trajectory.metrics.turnCount).toBe(1);
    });

    it("emits 4 events per execution (turn_start, user_message, assistant_message, turn_end)", async () => {
        const stimulus = { name: "events-test", prompt: "Events" };
        const options = { workDir: "/tmp/test", timeout: 5000 };

        const trajectory = await executor.execute(stimulus, options);

        expect(trajectory.events).toHaveLength(4);
        expect(trajectory.events.map((e) => e.type)).toEqual([
            "turn_start",
            "user_message",
            "assistant_message",
            "turn_end",
        ]);
    });

    it("shutdown is a no-op", async () => {
        await expect(executor.shutdown()).resolves.toBeUndefined();
    });
});
