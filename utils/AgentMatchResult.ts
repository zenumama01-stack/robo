 * Result from agent similarity matching
export interface AgentMatchResult {
     * The agent's unique ID
     * The agent's name
     * The agent's description
     * Cosine similarity score (0-1, where 1 = perfect match)
     * Agent's system prompt (optional)
     * Agent type name (optional)
    typeName?: string;
     * Agent status (optional)
     * Agent invocation mode - controls how agent can be called (optional)
     * Values: 'Any', 'Top-Level', 'Sub-Agent'
    invocationMode?: string;
     * Name of the default artifact type this agent produces (optional)
     * NULL if agent doesn't produce artifacts (orchestration/utility agents)
     * Examples: "Research Content", "Report", "Diagram"
    defaultArtifactType?: string;
     * Parent agent ID for child agents (optional)
     * NULL if this is a top-level agent
 * Metadata about an agent for embedding purposes
export interface AgentEmbeddingMetadata {
     * Agent invocation mode - controls how agent can be called
     * Name of the default artifact type this agent produces
     * NULL if agent doesn't produce artifacts
     * Parent agent ID for child agents
