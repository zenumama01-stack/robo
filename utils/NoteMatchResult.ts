import { MJAIAgentNoteEntity } from '@memberjunction/core-entities';
 * Metadata stored with each note embedding in the vector service.
export interface NoteEmbeddingMetadata {
    agentId: string | null;
    noteText: string;
    noteEntity: MJAIAgentNoteEntity;
 * Result from semantic search for similar notes.
export interface NoteMatchResult {
    note: MJAIAgentNoteEntity;
