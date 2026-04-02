import { MJAIAgentNoteEntity } from "@memberjunction/core-entities";
 * Server-side extension of MJAIAgentNoteEntity that auto-generates embeddings
 * when the Note field changes, following the MJQueryEntity pattern.
export class AIAgentNoteEntityExtended extends MJAIAgentNoteEntity {
            // Check if Note field has changed
            const noteField = this.GetFieldByName('Note');
            const shouldGenerateEmbedding = !this.IsSaved || noteField.Dirty;
            // Generate embedding for Note field if needed
            if (shouldGenerateEmbedding && this.Note && this.Note.trim().length > 0) {
                await this.GenerateEmbeddingByFieldName("Note", "EmbeddingVector", "EmbeddingModelID");
                // now let the AIEngine know that the note has been updated so it can have the latest vector in memory
                AIEngine.Instance.AddOrUpdateSingleNoteEmbedding(this);
            } else if (!this.Note || this.Note.trim().length === 0) {
                // Clear embedding if note is empty
            // Save using parent
            LogError('Failed to save AI Agent Note:', e);
