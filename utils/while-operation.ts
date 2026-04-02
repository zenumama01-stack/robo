 * @fileoverview While loop configuration for AI Agent execution.
 * This module contains the WhileOperation interface used by all agent types
 * for conditional iteration. Flow agents convert AIAgentStep configuration
 * Universal While loop configuration used by all agent types.
export interface WhileOperation {
    /** Boolean expression evaluated before each iteration */
    condition: string;
    /** Variable name for attempt context (default: "attempt") */
    /** Maximum iterations (undefined=100, 0=unlimited, >0=limit) */
