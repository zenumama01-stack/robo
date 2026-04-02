import { EntityInfo, Metadata } from "@memberjunction/core";
import { ChangeDetectionResult, ExternalChangeDetectorEngine } from "@memberjunction/external-change-detection";
 * This class provides a simple wrapper to execute the External Change Detection process.
 * Possible params:
 *  EntityList - a comma separated list of entity names to detect changes for. If not provided, all eligible entities will be processed.
@RegisterClass(BaseAction, "__RunExternalChangeDetection")
export class ExternalChangeDetectionAction extends BaseAction {
        const entityListParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'entitylist');
        await ExternalChangeDetectorEngine.Instance.Config(false, params.ContextUser);
        let changes: ChangeDetectionResult;
        if (entityListParam && entityListParam.Value && entityListParam.Value.length > 0) {
            const entityNames = entityListParam.Value.split(',');
            const entities: EntityInfo[] = entityNames.map(entityName => md.EntityByName(entityName));
            changes = await ExternalChangeDetectorEngine.Instance.DetectChangesForEntities(entities);
            changes = await ExternalChangeDetectorEngine.Instance.DetectChangesForAllEligibleEntities();
        if (changes && changes.Success) {
            // attempt to replay the changes
            if (await ExternalChangeDetectorEngine.Instance.ReplayChanges(changes.Changes)) {
                    Message: "Changes detected and replayed successfully.",
                    Message: "Failed to replay changes.",
                Message: changes.ErrorMessage,
