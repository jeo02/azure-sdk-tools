// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import type { ExecutorRegistry } from "@microsoft/vally";
import { TempMockExecutor } from "./executor/temp-mock-executor.js";

/**
 * Plugin entry point — vally calls this to discover the executor.
 */
export function register(registry: ExecutorRegistry): void {
    registry.register(new TempMockExecutor());
}

export { TempMockExecutor } from "./executor/temp-mock-executor.js";
