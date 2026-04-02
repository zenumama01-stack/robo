import { EntityVectorSyncer } from "@memberjunction/ai-vector-sync";
import { MJEntityDocumentEntity } from "@memberjunction/core-entities";
 * Action that vectorizes entities by creating and storing vector embeddings for entity documents.
 * This action processes one or more entities and their associated documents to generate
 * searchable vector representations for AI-powered semantic search and retrieval.
 * // Vectorize a single entity
 *   ActionName: 'Vectorize Entity',
 *     Name: 'EntityNames',
 * // Vectorize multiple entities
 *     Value: ['Customers', 'Orders', 'Products']
@RegisterClass(BaseAction, "__VectorizeEntity")
export class VectorizeEntityAction extends BaseAction {
     * Executes the vectorization process for specified entities.
     *   - EntityNames: A string, comma-separated string, or array of entity names to vectorize
     *   - ContextUser: The user context for permissions and logging
     *   - Success: true if all entities were vectorized successfully
     *   - Message: Combined messages from all vectorization operations
     *   - ResultCode: "SUCCESS" if all succeeded, "FAILED" if any failed
     * @throws Never throws directly - all errors are caught and returned in the result
        const entityNamesParam: ActionParam | undefined = params.Params.find(p => p.Name.trim().toLowerCase() === 'entitynames');
        let entityNames: string[] = [];
        if(entityNamesParam && entityNamesParam.Value){
            if(Array.isArray(entityNamesParam.Value)){
                entityNames = entityNamesParam.Value;
            else if(entityNamesParam.Value.includes(',')){
                entityNames = entityNamesParam.Value.split(',');
                entityNames = [entityNamesParam.Value];
        LogStatus(`VectorizeEntityAction: Entities to vectorize: ${entityNames.join(', ')}`);
        await vectorizer.Config(false, params.ContextUser);
        const entityDocuments: MJEntityDocumentEntity[] = await vectorizer.GetActiveEntityDocuments(entityNames);
        let results: ActionResultSimple[] = await Promise.all(entityDocuments.map(async (entityDocument: MJEntityDocumentEntity) => {
                await vectorizer.VectorizeEntity({
            Success: results.every(r => r.Success),
            Message: results.map(r => r.Message).join('\n'),
            ResultCode: results.every(r => r.Success) ? "SUCCESS" : "FAILED"            
