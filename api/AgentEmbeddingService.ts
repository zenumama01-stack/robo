import { AIAgentEntityExtended, AIModelEntityExtended } from '@memberjunction/ai-core-plus';
import { AgentMatchResult, AgentEmbeddingMetadata } from '../types/AgentMatchResult';
 * Utility service for agent embedding operations.
 * Uses agent data already loaded by AIEngine - no separate initialization needed.
 * const entries = await AgentEmbeddingService.GenerateAgentEmbeddings(
 *   agents,
 * // Find similar agents
 * const matches = await AgentEmbeddingService.FindSimilarAgents(
 *   "Handle customer support",
export class AgentEmbeddingService {
     * Generate embeddings for agents using provided embedding function.
     * @param agents - Array of agents (already loaded by AIEngine)
    public static async GenerateAgentEmbeddings(
        agents: AIAgentEntityExtended[],
    ): Promise<VectorEntry<AgentEmbeddingMetadata>[]> {
        const entries: VectorEntry<AgentEmbeddingMetadata>[] = [];
                // Create embedding text from agent name and description
                const embeddingText = this.createEmbeddingText(agent);
                    LogError(`Failed to generate embedding for agent ${agent.Name}`);
                    key: agent.ID,
                        systemPrompt: '',  // SystemPrompt not available on extended class
                        typeName: agent.Type || '',
                        status: agent.Status || 'Active',
                        invocationMode: agent.InvocationMode || 'Any',
                        defaultArtifactType: agent.DefaultArtifactType || undefined,
                        parentId: agent.ParentID || undefined
                LogError(`Error generating embedding for agent ${agent.Name}: ${error instanceof Error ? error.message : String(error)}`);
                // Continue with other agents
     * Find agents similar to a given task description.
     * @param vectorService - Vector service containing agent embeddings
     * @param taskDescription - The task description to match against agent capabilities
     * @returns Array of matching agents sorted by similarity score (highest first)
    public static async FindSimilarAgents(
        vectorService: SimpleVectorService<AgentEmbeddingMetadata>,
            // Map to AgentMatchResult format
                agentId: r.key,
                agentName: r.metadata?.name || 'Unknown',
                systemPrompt: r.metadata?.systemPrompt,
                typeName: r.metadata?.typeName,
                invocationMode: r.metadata?.invocationMode,
                defaultArtifactType: r.metadata?.defaultArtifactType,
                parentId: r.metadata?.parentId
            LogError(`Error finding similar agents: ${error instanceof Error ? error.message : String(error)}`);
     * Find agents similar to a specific agent.
     * @param agentId - The ID of the agent to find similar agents to
     * @returns Array of matching agents (excludes the source agent)
     * @throws Error if agent not found in embeddings
    public static FindRelatedAgents(
    ): AgentMatchResult[] {
        if (!vectorService.Has(agentId)) {
            throw new Error(`Agent ${agentId} not found in embeddings`);
            LogError(`Error finding related agents: ${error instanceof Error ? error.message : String(error)}`);
     * Get the similarity score between two specific agents.
     * @param agentId1 - First agent ID
     * @param agentId2 - Second agent ID
     * @throws Error if either agent is not found
        agentId1: string,
        agentId2: string
        if (!vectorService.Has(agentId1)) {
            throw new Error(`Agent ${agentId1} not found in embeddings`);
        if (!vectorService.Has(agentId2)) {
            throw new Error(`Agent ${agentId2} not found in embeddings`);
        return vectorService.Similarity(agentId1, agentId2);
     * Create the embedding text from agent properties.
    private static createEmbeddingText(agent: AIAgentEntityExtended): string {
        // Weight the agent name more heavily by repeating it
            agent.Name,
            agent.Name,  // Repeat for emphasis
            agent.Description || ''
