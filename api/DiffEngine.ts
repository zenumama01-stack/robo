import { MJVersionLabelEntity, MJVersionLabelItemEntityType } from '@memberjunction/core-entities';
    DiffResult,
    DiffSummary,
    EntityDiffGroup,
    RecordDiff,
    RecordDiffType,
    RecordSnapshot,
    ENTITY_VERSION_LABEL_ITEMS,
    ENTITY_VERSION_LABELS,
    ENTITY_RECORD_CHANGES,
    sqlEquals,
    loadRecordChangeSnapshot,
    loadEntityById,
} from './constants';
 * Map key: "entityId::recordId" → RecordChangeID
type SnapshotIndex = Map<string, string>;
 * Compares state between two version labels, or between a label and the
 * current live state, producing a structured diff grouped by entity.
export class DiffEngine {
     * Compare two version labels.
    public async DiffLabels(
        fromLabelId: string,
        toLabelId: string,
    ): Promise<DiffResult> {
        // Short-circuit: identical labels produce an empty diff
        if (fromLabelId === toLabelId) {
            const label = await loadEntityById<MJVersionLabelEntity>(ENTITY_VERSION_LABELS, fromLabelId, contextUser);
            const labelName = label ? label.Name : fromLabelId;
                FromLabelID: fromLabelId,
                FromLabelName: labelName,
                ToLabelID: toLabelId,
                ToLabelName: labelName,
                Summary: { TotalRecordsChanged: 0, TotalRecordsAdded: 0, TotalRecordsModified: 0, TotalRecordsDeleted: 0, EntitiesAffected: 0 },
                EntityDiffs: [],
        const [fromLabel, toLabel] = await Promise.all([
            loadEntityById<MJVersionLabelEntity>(ENTITY_VERSION_LABELS, fromLabelId, contextUser),
            loadEntityById<MJVersionLabelEntity>(ENTITY_VERSION_LABELS, toLabelId, contextUser),
        if (!fromLabel) throw new Error(`Version label '${fromLabelId}' not found`);
        if (!toLabel) throw new Error(`Version label '${toLabelId}' not found`);
        const [fromIndex, toIndex] = await Promise.all([
            this.buildSnapshotIndex(fromLabelId, contextUser),
            this.buildSnapshotIndex(toLabelId, contextUser),
        const entityDiffs = await this.computeDiffs(fromIndex, toIndex, contextUser);
        const summary = this.computeSummary(entityDiffs);
            FromLabelName: fromLabel.Name,
            ToLabelName: toLabel.Name,
            Summary: summary,
            EntityDiffs: entityDiffs,
     * Compare a version label to the current live state.
     * For each record in the label, we find its current latest RecordChange
     * and compare. Records that exist in the current state but not in the
     * label are reported as Added; records in the label but not currently
     * existing are reported as Deleted.
    public async DiffLabelToCurrentState(
        labelId: string,
        const label = await loadEntityById<MJVersionLabelEntity>(ENTITY_VERSION_LABELS, labelId, contextUser);
        if (!label) throw new Error(`Version label '${labelId}' not found`);
        const fromIndex = await this.buildSnapshotIndex(labelId, contextUser);
        // Build a "current" index: for each record in the from index, find
        // its most recent RecordChange
        const currentIndex = await this.buildCurrentIndex(fromIndex, contextUser);
        const entityDiffs = await this.computeDiffs(fromIndex, currentIndex, contextUser);
            FromLabelID: labelId,
            FromLabelName: label.Name,
            ToLabelID: null,
            ToLabelName: null,
     * Get the snapshot of a specific record at a specific label.
    public async GetRecordSnapshotAtLabel(
    ): Promise<RecordSnapshot | null> {
        const filter = [
            sqlEquals('VersionLabelID', labelId),
            sqlEquals('EntityID', entityInfo.ID),
            sqlEquals('RecordID', recordId),
        ].join(' AND ');
            EntityName: ENTITY_VERSION_LABEL_ITEMS,
            Fields: ['ID', 'RecordChangeID', 'EntityID', 'RecordID'],
        if (!result.Success || result.Results.length === 0) return null;
        const item = result.Results[0];
        return this.buildSnapshotFromRecordChange(
            item.RecordChangeID,
    // -----------------------------------------------------------------------
     * Build an index from VersionLabelItems: key → RecordChangeID.
    private async buildSnapshotIndex(labelId: string, contextUser: UserInfo): Promise<SnapshotIndex> {
            ExtraFilter: sqlEquals('VersionLabelID', labelId),
        const index: SnapshotIndex = new Map();
            LogError(`DiffEngine: Failed to load label items for ${labelId}: ${result.ErrorMessage}`);
            const key = `${item.EntityID}::${item.RecordID}`;
            index.set(key, item.RecordChangeID);
     * Build a "current state" index by finding the latest RecordChange for
     * each record in the fromIndex.
    private async buildCurrentIndex(
        fromIndex: SnapshotIndex,
    ): Promise<SnapshotIndex> {
        const currentIndex: SnapshotIndex = new Map();
        // Group by entityId for efficient batch queries
        const byEntity = this.groupKeysByEntity(fromIndex);
        for (const [entityId, recordIds] of byEntity) {
                const latestId = await this.findLatestRecordChange(rv, entityId, recordId, contextUser);
                if (latestId) {
                    currentIndex.set(`${entityId}::${recordId}`, latestId);
        return currentIndex;
     * Group composite keys from a SnapshotIndex by entityId.
    private groupKeysByEntity(index: SnapshotIndex): Map<string, string[]> {
        const byEntity = new Map<string, string[]>();
        for (const compositeKey of index.keys()) {
            const [entityId, recordId] = compositeKey.split('::');
            if (!byEntity.has(entityId)) byEntity.set(entityId, []);
            byEntity.get(entityId)!.push(recordId);
     * Find the latest RecordChange ID for a given entity + record.
    private async findLatestRecordChange(
            sqlEquals('EntityID', entityId),
            EntityName: ENTITY_RECORD_CHANGES,
     * Compute diffs between two snapshot indices.
    private async computeDiffs(
        toIndex: SnapshotIndex,
    ): Promise<EntityDiffGroup[]> {
        const allKeys = new Set([...fromIndex.keys(), ...toIndex.keys()]);
        // Group diffs by entity
        const entityGroups = new Map<string, { entityId: string; records: RecordDiff[] }>();
        for (const compositeKey of allKeys) {
            const fromChangeId = fromIndex.get(compositeKey) ?? null;
            const toChangeId = toIndex.get(compositeKey) ?? null;
            const entityName = this.resolveEntityName(md, entityId);
            if (!entityGroups.has(entityId)) {
                entityGroups.set(entityId, { entityId, records: [] });
            const diff = await this.diffSingleRecord(
                entityName, recordId, fromChangeId, toChangeId, contextUser
            entityGroups.get(entityId)!.records.push(diff);
        return this.buildEntityDiffGroups(entityGroups, md);
     * Resolve an entity name from metadata by ID, with fallback.
    private resolveEntityName(md: Metadata, entityId: string): string {
        return entityInfo?.Name ?? `Unknown(${entityId})`;
     * Convert the grouped map into an array of EntityDiffGroup,
     * filtering out groups with no actual changes.
    private buildEntityDiffGroups(
        entityGroups: Map<string, { entityId: string; records: RecordDiff[] }>,
    ): EntityDiffGroup[] {
        const result: EntityDiffGroup[] = [];
        for (const [entityId, group] of entityGroups) {
            const addedCount = group.records.filter(r => r.DiffType === 'Added').length;
            const modifiedCount = group.records.filter(r => r.DiffType === 'Modified').length;
            const deletedCount = group.records.filter(r => r.DiffType === 'Deleted').length;
            // Skip groups with no changes
            if (addedCount === 0 && modifiedCount === 0 && deletedCount === 0) continue;
                Records: group.records.filter(r => r.DiffType !== 'Unchanged'),
                AddedCount: addedCount,
                ModifiedCount: modifiedCount,
                DeletedCount: deletedCount,
     * Diff a single record between two RecordChange snapshots.
    private async diffSingleRecord(
        fromChangeId: string | null,
        toChangeId: string | null,
    ): Promise<RecordDiff> {
        // Record exists only in "to" → Added
        if (!fromChangeId && toChangeId) {
            return this.buildAddedDiff(entityName, recordId, toChangeId, contextUser);
        // Record exists only in "from" → Deleted
        if (fromChangeId && !toChangeId) {
            return this.buildDeletedDiff(entityName, recordId, fromChangeId, contextUser);
        // Same RecordChange ID → Unchanged
        if (fromChangeId === toChangeId) {
            return this.buildUnchangedDiff(entityName, recordId);
        // Both exist but differ → compare field-by-field
        return this.buildModifiedDiff(entityName, recordId, fromChangeId!, toChangeId!, contextUser);
     * Build a diff result for a record that was added.
    private async buildAddedDiff(
        toChangeId: string,
        const toSnapshot = await loadRecordChangeSnapshot(toChangeId, contextUser);
            DiffType: 'Added',
            FromSnapshot: null,
            ToSnapshot: toSnapshot,
     * Build a diff result for a record that was deleted.
    private async buildDeletedDiff(
        fromChangeId: string,
        const fromSnapshot = await loadRecordChangeSnapshot(fromChangeId, contextUser);
            DiffType: 'Deleted',
            FromSnapshot: fromSnapshot,
            ToSnapshot: null,
     * Build a diff result for an unchanged record.
    private buildUnchangedDiff(entityName: string, recordId: string): RecordDiff {
            DiffType: 'Unchanged',
     * Build a diff result for a modified record by comparing snapshots field-by-field.
    private async buildModifiedDiff(
        const [fromSnapshot, toSnapshot] = await Promise.all([
            loadRecordChangeSnapshot(fromChangeId, contextUser),
            loadRecordChangeSnapshot(toChangeId, contextUser),
        const fieldChanges = this.compareSnapshots(fromSnapshot, toSnapshot);
        const diffType: RecordDiffType = fieldChanges.length > 0 ? 'Modified' : 'Unchanged';
            DiffType: diffType,
            FieldChanges: fieldChanges,
     * Build a RecordSnapshot from a RecordChange entry, using the shared
     * loadRecordChangeSnapshot utility and wrapping the result.
    private async buildSnapshotFromRecordChange(
        recordChangeId: string,
        const result = await rv.RunView<{ ID: string; ChangedAt: string }>({
            ExtraFilter: sqlEquals('ID', recordChangeId),
            Fields: ['ID', 'ChangedAt'],
        const changedAt = result.Results[0].ChangedAt;
        const parsed = await loadRecordChangeSnapshot(recordChangeId, contextUser);
        if (!parsed) return null;
            ChangedAt: new Date(changedAt),
            FullRecordJSON: parsed,
     * Compare two parsed JSON snapshots field-by-field.
    private compareSnapshots(
        fromSnapshot: Record<string, unknown> | null,
        toSnapshot: Record<string, unknown> | null
    ): FieldChange[] {
        if (!fromSnapshot || !toSnapshot) return [];
        const allFields = new Set([...Object.keys(fromSnapshot), ...Object.keys(toSnapshot)]);
        for (const field of allFields) {
            // Skip internal timestamp fields
            if (field.startsWith('__mj_')) continue;
            const oldVal = fromSnapshot[field];
            const newVal = toSnapshot[field];
            if (this.valuesEqual(oldVal, newVal)) continue;
            const changeType = this.classifyFieldChange(oldVal, newVal);
            changes.push({ FieldName: field, OldValue: oldVal, NewValue: newVal, ChangeType: changeType });
     * Classify a field change as Added, Removed, or Modified.
    private classifyFieldChange(
        oldVal: unknown,
        newVal: unknown
    ): 'Added' | 'Modified' | 'Removed' {
        if (oldVal === undefined || oldVal === null) return 'Added';
        if (newVal === undefined || newVal === null) return 'Removed';
        return 'Modified';
     * Equality check for field values.
     * Handles null, identity, Date instances, and falls back to string comparison.
    private valuesEqual(a: unknown, b: unknown): boolean {
        if (a === b) return true;
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        // Date instance comparison
        if (a instanceof Date && b instanceof Date) {
            return a.getTime() === b.getTime();
        // String comparison fallback
        return String(a) === String(b);
     * Compute summary statistics from entity diffs.
    private computeSummary(entityDiffs: EntityDiffGroup[]): DiffSummary {
        let modified = 0;
        for (const group of entityDiffs) {
            added += group.AddedCount;
            modified += group.ModifiedCount;
            deleted += group.DeletedCount;
            TotalRecordsChanged: added + modified + deleted,
            TotalRecordsAdded: added,
            TotalRecordsModified: modified,
            TotalRecordsDeleted: deleted,
            EntitiesAffected: entityDiffs.length,
