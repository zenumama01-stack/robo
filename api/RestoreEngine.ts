    MJVersionLabelItemEntityType,
    RestoreItemResult,
    RestoreOptions,
    RestoreResult,
    RestoreStatus,
    VersionLabelScope,
import { LabelManager } from './LabelManager';
import { SnapshotBuilder } from './SnapshotBuilder';
    ENTITY_VERSION_LABEL_RESTORES,
    sqlNotIn,
    buildPrimaryKeyForLoad,
/** Batch size for progress update writes — only persist every N items. */
const PROGRESS_UPDATE_INTERVAL = 10;
 * Restores records to the state captured by a version label.
 * Key behaviors:
 * - Automatically creates a "Pre-Restore" safety label before making changes
 * - Restores in topological order (parents before children) based on entity
 *   relationships to maintain referential integrity
 * - Tracks progress and errors in a VersionLabelRestore audit record
 * - Supports dry-run mode for previewing changes without applying them
export class RestoreEngine {
    private LabelMgr = new LabelManager();
    private SnapshotBldr = new SnapshotBuilder();
     * Restore records to the state captured by a version label.
    public async RestoreToLabel(
        options: RestoreOptions,
    ): Promise<RestoreResult> {
        // Load the target label
        const labelName = label.Name;
        const labelScope = label.Scope;
        LogStatus(`VersionHistory: Starting restore to label '${labelName}' (${labelId})`);
        // Load all label items to restore
        const items = await this.loadLabelItems(labelId, resolvedOptions, contextUser);
            LogStatus('VersionHistory: No items to restore');
            return this.emptyResult(resolvedOptions.DryRun);
        // Create pre-restore safety label
        let preRestoreLabelId: string | null = null;
        if (resolvedOptions.CreatePreRestoreLabel && !resolvedOptions.DryRun) {
            preRestoreLabelId = await this.createPreRestoreLabel(labelName, labelScope, items, contextUser);
        // Create the restore audit record
        let restoreAuditId: string | null = null;
        if (!resolvedOptions.DryRun) {
            restoreAuditId = await this.createRestoreAuditRecord(
                labelId,
                items.length,
                preRestoreLabelId,
        // Sort items by entity dependency order and process them
        const sortedItems = this.sortByDependencyOrder(items);
        const { details, restoredCount, failedCount, skippedCount } =
            await this.processRestoreItems(sortedItems, resolvedOptions.DryRun, restoreAuditId, contextUser);
        // Finalize the audit record
        const finalStatus = this.determineFinalStatus(restoredCount, failedCount, items.length);
        if (restoreAuditId && !resolvedOptions.DryRun) {
            await this.finalizeRestoreAudit(restoreAuditId, finalStatus, failedCount, details, contextUser);
        // Mark the label as restored
        if (!resolvedOptions.DryRun && finalStatus !== 'Error') {
            await this.LabelMgr.MarkLabelRestored(labelId, contextUser);
        LogStatus(`VersionHistory: Restore complete. ${restoredCount} restored, ${failedCount} failed, ${skippedCount} skipped.`);
            RestoreID: restoreAuditId ?? 'dry-run',
            PreRestoreLabelID: preRestoreLabelId,
            Status: finalStatus,
            RestoredCount: restoredCount,
            FailedCount: failedCount,
            SkippedCount: skippedCount,
            Details: details,
     * Process all restore items in order, tracking progress and updating the
     * audit record in batches.
    private async processRestoreItems(
        sortedItems: MJVersionLabelItemEntityType[],
        dryRun: boolean,
        restoreAuditId: string | null,
    ): Promise<RestoreProgressTotals> {
        const details: RestoreItemResult[] = [];
        for (let i = 0; i < sortedItems.length; i++) {
            const result = await this.restoreSingleItem(sortedItems[i], dryRun, contextUser);
            details.push(result);
            switch (result.Status) {
                case 'Restored':
            // Batch progress updates — only write every N items or on the last item
            const isLastItem = i === sortedItems.length - 1;
            const isBatchBoundary = (i + 1) % PROGRESS_UPDATE_INTERVAL === 0;
            if (restoreAuditId && !dryRun && (isBatchBoundary || isLastItem)) {
                await this.updateRestoreProgress(restoreAuditId, restoredCount, failedCount, contextUser);
        return { details, restoredCount, failedCount, skippedCount };
     * Load all VersionLabelItems for a label, optionally filtered.
        options: Required<RestoreOptions>,
        let extraFilter = sqlEquals('VersionLabelID', labelId);
        // Apply entity exclusion
        if (options.SkipEntities && options.SkipEntities.length > 0) {
            const excludeIds = options.SkipEntities
                .map(name => md.EntityByName(name)?.ID)
                .filter((id): id is string => id != null);
            if (excludeIds.length > 0) {
                extraFilter += ` AND ${sqlNotIn('EntityID', excludeIds)}`;
            LogError(`RestoreEngine: Failed to load label items: ${result.ErrorMessage}`);
        let items = result.Results;
        // Apply selected records filter
        if (options.Scope === 'Selected' && options.SelectedRecords && options.SelectedRecords.length > 0) {
            items = this.filterBySelectedRecords(items, options.SelectedRecords);
     * Filter label items to only include those matching a set of selected records.
    private filterBySelectedRecords(
        items: MJVersionLabelItemEntityType[],
        selectedRecords: Array<{ EntityName: string; RecordID: string }>
    ): MJVersionLabelItemEntityType[] {
        const selectedSet = new Set(
            selectedRecords.map(s => `${s.EntityName}::${s.RecordID}`)
            const entityInfo = md.Entities.find(e => e.ID === item.EntityID);
            const entityName = entityInfo?.Name ?? '';
            return selectedSet.has(`${entityName}::${item.RecordID}`);
     * Sort label items by entity dependency order so parents are restored
     * before their children.
    private sortByDependencyOrder(items: MJVersionLabelItemEntityType[]): MJVersionLabelItemEntityType[] {
        // Build a map of entityId -> dependency level
        const levelMap = new Map<string, number>();
        const computeLevel = (entityId: string): number => {
            if (levelMap.has(entityId)) return levelMap.get(entityId)!;
            if (visited.has(entityId)) return 0; // Cycle — break it
                levelMap.set(entityId, 0);
            // Find all FK fields pointing to other entities
            let maxParentLevel = -1;
                if (field.RelatedEntityID && field.RelatedEntityID !== entityId) {
                    const parentLevel = computeLevel(field.RelatedEntityID);
                    maxParentLevel = Math.max(maxParentLevel, parentLevel);
            const level = maxParentLevel + 1;
            levelMap.set(entityId, level);
        // Compute levels for all entities in the item set
        const entityIds = new Set(items.map(i => i.EntityID));
        for (const entityId of entityIds) {
            computeLevel(entityId);
        // Sort: lower level (parents) first
            const levelA = levelMap.get(a.EntityID) ?? 0;
            const levelB = levelMap.get(b.EntityID) ?? 0;
            return levelA - levelB;
     * Restore a single record to its labeled state.
    private async restoreSingleItem(
        item: MJVersionLabelItemEntityType,
    ): Promise<RestoreItemResult> {
        const entityInfo = this.resolveEntityInfo(item.EntityID);
            return this.failedItemResult(`Unknown(${item.EntityID})`, item.RecordID,
                `Entity with ID '${item.EntityID}' not found in metadata`);
            const snapshotData = await loadRecordChangeSnapshot(item.RecordChangeID, contextUser);
            if (!snapshotData) {
                return this.failedItemResult(entityInfo.Name, item.RecordID,
                    'Could not load snapshot from RecordChange');
            if (dryRun) {
                return { EntityName: entityInfo.Name, RecordID: item.RecordID, Status: 'Restored' };
            return await this.applyRestore(entityInfo, item.RecordID, snapshotData, contextUser);
            LogError(`RestoreEngine: Error restoring ${entityInfo.Name}/${item.RecordID}: ${msg}`);
            return this.failedItemResult(entityInfo.Name, item.RecordID, msg);
     * Resolve entity metadata by ID.
    private resolveEntityInfo(entityId: string): EntityInfo | null {
        return md.Entities.find(e => e.ID === entityId) ?? null;
     * Apply the snapshot data to an existing or new entity and save it.
    private async applyRestore(
        snapshotData: Record<string, unknown>,
        const entity = await this.loadOrCreateEntity(entityInfo, recordId, contextUser);
            return this.failedItemResult(entityInfo.Name, recordId, 'Could not load entity for restore');
        const changed = this.applySnapshotToEntity(entity, snapshotData, entityInfo);
        if (!changed) {
            return { EntityName: entityInfo.Name, RecordID: recordId, Status: 'Skipped' };
            return this.failedItemResult(entityInfo.Name, recordId, 'Save failed');
        return { EntityName: entityInfo.Name, RecordID: recordId, Status: 'Restored' };
     * Build a failed RestoreItemResult.
    private failedItemResult(entityName: string, recordId: string, errorMessage: string): RestoreItemResult {
        return { EntityName: entityName, RecordID: recordId, Status: 'Failed', ErrorMessage: errorMessage };
     * Load an existing entity record, or create a new one if it doesn't exist.
    private async loadOrCreateEntity(
    ): Promise<BaseEntity | null> {
        const entity = await md.GetEntityObject<BaseEntity>(entityInfo.Name, contextUser);
        // Try to load existing record using the entity's actual primary key
            const key = buildPrimaryKeyForLoad(entityInfo, recordId);
            const loaded = await entity.InnerLoad(key);
            if (loaded) return entity;
            // Record doesn't exist — fall through to create
        // Record doesn't exist; return fresh entity for insert
        return await md.GetEntityObject<BaseEntity>(entityInfo.Name, contextUser);
     * Apply snapshot data to an entity, setting each field from the snapshot.
     * Returns true if any field was changed.
    private applySnapshotToEntity(
        let anyChanged = false;
            // Skip read-only, primary key, and system fields
            if (field.ReadOnly) continue;
            if (field.IsPrimaryKey) continue;
            if (field.Name.startsWith('__mj_')) continue;
            const snapshotValue = snapshotData[field.Name];
            if (snapshotValue === undefined) continue;
            const currentValue = entity.Get(field.Name);
            if (!this.valuesDiffer(currentValue, snapshotValue)) continue;
            entity.Set(field.Name, snapshotValue);
            anyChanged = true;
        return anyChanged;
     * Check if two values are meaningfully different.
    private valuesDiffer(a: unknown, b: unknown): boolean {
        if (a === b) return false;
        if (a == null && b == null) return false;
        return String(a) !== String(b);
     * Create a "Pre-Restore" safety label capturing current state of all
     * records that will be affected by the restore.
    private async createPreRestoreLabel(
        targetLabelName: string,
        targetLabelScope: VersionLabelScope,
        const label = await this.LabelMgr.CreateLabel({
            Name: `Pre-Restore: ${targetLabelName} (${new Date().toISOString()})`,
            Description: `Automatic safety snapshot created before restoring to label '${targetLabelName}'`,
            Scope: targetLabelScope,
        const preRestoreLabelId = label.ID;
        // Capture current state of each record that will be restored
            const key = new CompositeKey([{
                FieldName: entityInfo.FirstPrimaryKey.Name,
                Value: item.RecordID,
            await this.SnapshotBldr.CaptureRecord(
                entityInfo.Name,
                false, // Don't walk dependencies — we already have the full item list
        LogStatus(`VersionHistory: Created pre-restore safety label (${preRestoreLabelId})`);
        return preRestoreLabelId;
     * Create a VersionLabelRestore audit record.
    private async createRestoreAuditRecord(
        totalItems: number,
        preRestoreLabelId: string | null,
        const restore = await md.GetEntityObject<MJVersionLabelRestoreEntity>(ENTITY_VERSION_LABEL_RESTORES, contextUser);
        restore.VersionLabelID = labelId;
        restore.Status = 'In Progress';
        restore.UserID = contextUser.ID;
        restore.TotalItems = totalItems;
        restore.CompletedItems = 0;
        restore.FailedItems = 0;
        if (preRestoreLabelId) {
            restore.PreRestoreLabelID = preRestoreLabelId;
        const saved = await restore.Save();
            throw new Error('Failed to create restore audit record');
        return restore.ID;
     * Update restore progress counters.
    private async updateRestoreProgress(
        restoreId: string,
        completedItems: number,
        failedItems: number,
            const restore = await loadEntityById<MJVersionLabelRestoreEntity>(ENTITY_VERSION_LABEL_RESTORES, restoreId, contextUser);
            if (!restore) return;
            restore.CompletedItems = completedItems;
            restore.FailedItems = failedItems;
            await restore.Save();
            // Non-critical — progress update failure shouldn't stop the restore
            LogError(`RestoreEngine: Failed to update restore progress for ${restoreId}: ${msg}`);
     * Finalize the restore audit record.
    private async finalizeRestoreAudit(
        status: RestoreStatus,
        failedCount: number,
        details: RestoreItemResult[],
            restore.Status = status;
            restore.EndedAt = new Date();
                const errorItems = details
                    .filter(d => d.Status === 'Failed')
                    .map(d => `${d.EntityName}/${d.RecordID}: ${d.ErrorMessage ?? 'Unknown'}`)
                restore.ErrorLog = errorItems;
            LogError(`RestoreEngine: Error finalizing audit record: ${msg}`);
     * Determine the overall status based on counts.
    private determineFinalStatus(
        restored: number,
        failed: number,
        _total: number
    ): RestoreStatus {
        if (failed === 0) return 'Complete';
        if (restored === 0) return 'Error';
        return 'Partial';
     * Apply defaults to restore options.
    private resolveDefaults(options: RestoreOptions): Required<RestoreOptions> {
            DryRun: options.DryRun ?? false,
            Scope: options.Scope ?? 'Full',
            SelectedRecords: options.SelectedRecords ?? [],
            SkipEntities: options.SkipEntities ?? [],
            CreatePreRestoreLabel: options.CreatePreRestoreLabel ?? true,
     * Create an empty result (used when there's nothing to restore).
    private emptyResult(dryRun: boolean): RestoreResult {
            RestoreID: dryRun ? 'dry-run' : '',
            PreRestoreLabelID: null,
            RestoredCount: 0,
            FailedCount: 0,
            SkippedCount: 0,
            Details: [],
 * Progress totals returned from the item-processing loop.
interface RestoreProgressTotals {
    details: RestoreItemResult[];
    restoredCount: number;
    failedCount: number;
    skippedCount: number;
