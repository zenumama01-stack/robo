import { BaseEntity, SimpleEmbeddingResult } from "@memberjunction/core";
import { ComponentEntityExtended } from "@memberjunction/core-entities";
export class ComponentEntityExtended_Server extends ComponentEntityExtended  {
        await this.GenerateEmbeddingsByFieldName([
                fieldName: "FunctionalRequirements", 
                vectorFieldName: "FunctionalRequirementsVector", 
                modelFieldName: "FunctionalRequirementsVectorEmbeddingModelID" 
                fieldName: "TechnicalDesign", 
                vectorFieldName: "TechnicalDesignVector", 
                modelFieldName: "TechnicalDesignVectorEmbeddingModelID" 
        const saveResult: boolean = await super.Save();
     * Simple proxy to local helper method for embeddings. Needed for BaseEntity sub-classes that want to use embeddings built into BaseEntity
