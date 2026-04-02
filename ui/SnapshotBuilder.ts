import { MJRecordChangeEntity, MJVersionLabelItemEntity } from '@memberjunction/core-entities';
import { CaptureError, CaptureResult, CreateLabelProgressCallback, DependencyNode, WalkOptions } from './types';
import { DependencyGraphWalker } from './DependencyGraphWalker';
    buildCompositeKeyFromRecord,
    escapeSqlString,
/** Result of capturing a single record. */
interface CaptureItemResult {
    WasSynthetic: boolean;
/** A record change lookup entry from batched queries. */
interface RecordChangeLookup {
// SnapshotBuilder
 * Captures the current state of records into VersionLabelItem entries,
 * linking each record to its most recent RecordChange snapshot.
 * ## Batched approach
 * When capturing a dependency graph, the builder:
 * 1. Groups all nodes by EntityID
 * 2. For each entity group, runs ONE batched query to find the latest
 *    RecordChange for all records in the group
 * 3. Creates synthetic snapshots for records with no change history
 * 4. Creates VersionLabelItem entries for all records
 * This reduces hundreds/thousands of individual RunView calls down to
 * a handful of batched queries (one per unique entity type).
export class SnapshotBuilder {
    private Walker = new DependencyGraphWalker();
     * Capture a snapshot of a single record and optionally its full dependency graph.
    public async CaptureRecord(
        includeDependencies: boolean,
        walkOptions: WalkOptions,
        onProgress?: CreateLabelProgressCallback
    ): Promise<CaptureResult> {
        if (includeDependencies) {
            return this.captureWithDependencies(labelId, entityName, recordKey, walkOptions, contextUser, onProgress);
        return this.captureSingleRecordAsResult(labelId, entityName, recordKey, contextUser);
     * Capture all records of a given entity type.
    public async CaptureEntity(
            return this.failResult(labelId, `Entity '${escapeSqlString(entityName)}' not found`);
            return this.failResult(labelId, `Failed to load records for entity '${escapeSqlString(entityName)}': ${result.ErrorMessage}`);
        const errors: CaptureError[] = [];
        let itemsCaptured = 0;
        let syntheticCount = 0;
        for (const record of result.Results) {
            const key = buildCompositeKeyFromRecord(entityInfo, record);
            const captureItemResult = await this.captureSingleRecord(labelId, entityName, key, contextUser, record);
            if (captureItemResult.Success) {
                itemsCaptured++;
                if (captureItemResult.WasSynthetic) syntheticCount++;
                    ErrorMessage: captureItemResult.Error ?? 'Unknown error',
        const errorSuffix = errors.length > 0 ? ` (${errors.length} errors)` : '';
        LogStatus(`VersionHistory: Captured ${itemsCaptured} records for entity '${entityName}' into label ${labelId}${errorSuffix}`);
            LabelID: labelId,
            ItemsCaptured: itemsCaptured,
            SyntheticSnapshotsCreated: syntheticCount,
     * Capture all tracked entities (System scope).
    public async CaptureSystem(
        const trackedEntities = md.Entities.filter(e => e.TrackRecordChanges);
        const allErrors: CaptureError[] = [];
        let totalCaptured = 0;
        let totalSynthetic = 0;
        for (const entityInfo of trackedEntities) {
            const entityResult = await this.CaptureEntity(labelId, entityInfo.Name, contextUser);
            totalCaptured += entityResult.ItemsCaptured;
            totalSynthetic += entityResult.SyntheticSnapshotsCreated;
            allErrors.push(...entityResult.Errors);
        LogStatus(`VersionHistory: System capture complete. ${totalCaptured} records across ${trackedEntities.length} entities.`);
            Success: allErrors.length === 0,
            ItemsCaptured: totalCaptured,
            SyntheticSnapshotsCreated: totalSynthetic,
            Errors: allErrors,
    // Batched Dependency Capture
     * Walk the dependency graph and capture all nodes using batched queries.
     * 1. Walk the dependency graph to get all nodes
     * 2. Batch-lookup the latest RecordChange for all nodes (grouped by entity)
     * 3. Create synthetic snapshots for nodes without a RecordChange
     * 4. Create VersionLabelItem entries for all nodes
    private async captureWithDependencies(
        // Step 1: Walk the dependency graph
        this.emitProgress(onProgress, {
            Step: 'walking_dependencies',
            Message: `Walking dependency graph for ${entityName}...`,
            Percentage: 10,
        const root = await this.Walker.WalkDependents(entityName, recordKey, walkOptions, contextUser);
        const flatNodes = this.Walker.FlattenTopological(root);
        // Step 2: Batch-lookup RecordChanges for all nodes
            Step: 'capturing_snapshots',
            Message: `Found ${flatNodes.length} records. Looking up change history...`,
            Percentage: 30,
            TotalRecords: flatNodes.length,
            RecordsProcessed: 0,
        const changeLookup = await this.batchLookupLatestChanges(flatNodes, contextUser);
        // Steps 3 & 4: Process each node — create synthetics where needed, then label items
        const totalNodes = flatNodes.length;
        // Track current entity for progress reporting
        let lastEntityName = '';
        for (let i = 0; i < flatNodes.length; i++) {
            const node = flatNodes[i];
            // Emit progress periodically (every 5 items or on entity change)
            if (node.EntityName !== lastEntityName || i % 5 === 0) {
                lastEntityName = node.EntityName;
                const pct = 35 + Math.round((i / totalNodes) * 55); // 35–90%
                    Message: `Capturing ${node.EntityName}...`,
                    Percentage: pct,
                    RecordsProcessed: i,
                    TotalRecords: totalNodes,
                    CurrentEntity: node.EntityName,
            const result = await this.captureNodeWithLookup(
                labelId, node, changeLookup, contextUser
                if (result.WasSynthetic) syntheticCount++;
                    EntityName: node.EntityName,
                    RecordID: node.RecordID,
                    ErrorMessage: result.Error ?? 'Unknown error',
        // Final snapshot progress
            Message: `Captured ${itemsCaptured} records`,
            Percentage: 90,
            RecordsProcessed: totalNodes,
    /** Safely invoke the progress callback if provided. */
    private emitProgress(
        callback: CreateLabelProgressCallback | undefined,
        update: { Step: import('./types').CreateLabelStep; Message: string; Percentage: number; RecordsProcessed?: number; TotalRecords?: number; CurrentEntity?: string }
                callback(update);
                // Never let a callback error break the capture
    // Batched RecordChange Lookup
     * Batch-lookup the latest RecordChange for a list of dependency nodes.
     * Groups nodes by EntityID, then runs ONE query per entity group with an
     * IN clause for all RecordIDs. Returns a map keyed by "entityId::recordId"
     * with the latest RecordChange ID and timestamp.
     * This replaces N individual RunView calls with ~K calls (K = unique entity types).
    private async batchLookupLatestChanges(
        nodes: DependencyNode[],
    ): Promise<Map<string, RecordChangeLookup>> {
        const lookup = new Map<string, RecordChangeLookup>();
        // Group nodes by EntityID
        const entityGroups = this.groupNodesByEntity(nodes);
        // For each entity group, run a single batched query
        for (const [entityId, recordIds] of entityGroups.entries()) {
            const groupResults = await this.lookupChangesForEntityGroup(
                entityId, recordIds, contextUser
            // Merge results into the main lookup
            for (const [key, value] of groupResults.entries()) {
                lookup.set(key, value);
        return lookup;
     * Group dependency nodes by EntityID, collecting unique RecordIDs per entity.
     * Returns a Map of entityId → Set of recordIds.
    private groupNodesByEntity(nodes: DependencyNode[]): Map<string, Set<string>> {
        const groups = new Map<string, Set<string>>();
            const entityId = node.EntityInfo.ID;
            if (!groups.has(entityId)) {
                groups.set(entityId, new Set());
            groups.get(entityId)!.add(node.RecordID);
     * Query Record Changes for a single entity group.
     * Uses an IN clause to find the latest change for each record in one query.
     * Returns a Map of "entityId::recordId" → RecordChangeLookup.
    private async lookupChangesForEntityGroup(
        const results = new Map<string, RecordChangeLookup>();
        const recordIdArray = Array.from(recordIds);
        // SQL Server has a practical limit on IN clause size (~2100 params).
        // Batch into chunks if needed.
        const chunkSize = 500;
        for (let i = 0; i < recordIdArray.length; i += chunkSize) {
            const chunk = recordIdArray.slice(i, i + chunkSize);
            const chunkResults = await this.lookupChangesChunk(entityId, chunk, contextUser);
            for (const [key, value] of chunkResults.entries()) {
                results.set(key, value);
     * Query Record Changes for a chunk of RecordIDs within a single entity.
     * Loads all matching changes ordered by RecordID and ChangedAt DESC,
     * then picks the latest change per record.
    private async lookupChangesChunk(
        const escapedIds = recordIds.map(id => `'${escapeSqlString(id)}'`).join(', ');
        const filter = `${sqlEquals('EntityID', entityId)} AND RecordID IN (${escapedIds})`;
                OrderBy: 'RecordID, ChangedAt DESC',
                Fields: ['ID', 'RecordID', 'ChangedAt'],
                LogError(`SnapshotBuilder: Batch RecordChange lookup failed: ${result.ErrorMessage}`);
            // Pick the first (most recent) change per RecordID
                const recordId = String(row['RecordID']);
                const key = `${entityId}::${recordId}`;
                // Only take the first occurrence (most recent due to ORDER BY)
                if (!results.has(key)) {
                    results.set(key, {
                        RecordChangeID: String(row['ID']),
                        ChangedAt: new Date(String(row['ChangedAt'])),
            LogError(`SnapshotBuilder: Error in batch RecordChange lookup: ${msg}`);
    // Node Capture (with batched lookup)
     * Capture a single dependency node using the pre-built change lookup.
     * If no existing RecordChange is found, creates a synthetic snapshot.
    private async captureNodeWithLookup(
        node: DependencyNode,
        changeLookup: Map<string, RecordChangeLookup>,
    ): Promise<CaptureItemResult> {
            const lookupKey = `${node.EntityInfo.ID}::${node.RecordID}`;
            const existingChange = changeLookup.get(lookupKey);
            let recordChangeId: string;
            let wasSynthetic = false;
            if (existingChange) {
                recordChangeId = existingChange.RecordChangeID;
                // No existing change — create a synthetic snapshot
                const syntheticChange = await this.createSyntheticSnapshot(
                    node.EntityInfo, node.RecordID, node.RecordData, contextUser
                if (!syntheticChange) {
                    return { Success: false, WasSynthetic: false, Error: 'Failed to create synthetic snapshot' };
                recordChangeId = syntheticChange.ID;
                wasSynthetic = true;
            // Create the VersionLabelItem
            return this.createLabelItem(labelId, node.EntityInfo, node.RecordID, recordChangeId, wasSynthetic, contextUser);
            LogError(`SnapshotBuilder: Error capturing ${node.EntityName}/${node.RecordID}: ${msg}`);
            return { Success: false, WasSynthetic: false, Error: msg };
    // Single Record Capture (non-batched, for CaptureEntity / single record)
     * Capture a single record without batching. Used by CaptureEntity and
     * the non-dependency CaptureRecord path.
    private async captureSingleRecord(
        existingRecordData?: Record<string, unknown>
                return { Success: false, WasSynthetic: false, Error: `Entity '${entityName}' not found` };
            const recordId = recordKey.ToConcatenatedString();
            // Look up the latest RecordChange for this record
            const latestChange = await this.findLatestRecordChange(entityInfo.ID, recordId, contextUser);
            if (latestChange) {
                recordChangeId = String(latestChange['ID']);
                const recordData = existingRecordData ?? await this.loadCurrentRecord(entityName, recordKey, contextUser);
                if (!recordData || Object.keys(recordData).length === 0) {
                    return { Success: false, WasSynthetic: false, Error: 'Record not found or empty' };
                const syntheticChange = await this.createSyntheticSnapshot(entityInfo, recordId, recordData, contextUser);
            return this.createLabelItem(labelId, entityInfo, recordId, recordChangeId, wasSynthetic, contextUser);
            LogError(`SnapshotBuilder: Error capturing ${entityName}/${recordKey.ToConcatenatedString()}: ${msg}`);
     * Wrapper for captureSingleRecord that returns a CaptureResult.
    private async captureSingleRecordAsResult(
        const result = await this.captureSingleRecord(labelId, entityName, recordKey, contextUser);
                ItemsCaptured: 1,
                SyntheticSnapshotsCreated: result.WasSynthetic ? 1 : 0,
                Errors: [],
            ItemsCaptured: 0,
            SyntheticSnapshotsCreated: 0,
            Errors: [{ EntityName: entityName, RecordID: recordKey.ToConcatenatedString(), ErrorMessage: result.Error ?? 'Unknown error' }],
    // Shared Helpers
     * Find the most recent RecordChange for a single entity + record.
     * Used by the non-batched capture path.
    ): Promise<Record<string, unknown> | null> {
        const filter = `${sqlEquals('EntityID', entityId)} AND ${sqlEquals('RecordID', recordId)}`;
     * Create a synthetic RecordChange entry of Type='Snapshot' for a record
     * that has no prior change history.
    private async createSyntheticSnapshot(
    ): Promise<MJRecordChangeEntity | null> {
            const change = await md.GetEntityObject<MJRecordChangeEntity>(ENTITY_RECORD_CHANGES, contextUser);
            change.EntityID = entityInfo.ID;
            change.RecordID = recordId;
            change.UserID = contextUser.ID;
            change.Type = 'Snapshot';
            change.Source = 'Internal';
            change.ChangesJSON = '{}';
            change.ChangesDescription = 'Snapshot captured for version label';
            change.FullRecordJSON = JSON.stringify(recordData);
            change.Status = 'Complete';
            const saved = await change.Save();
                LogError('SnapshotBuilder: Failed to save synthetic snapshot');
            return change;
            LogError(`SnapshotBuilder: Error creating synthetic snapshot: ${msg}`);
     * Create a VersionLabelItem linking a record to its RecordChange.
    private async createLabelItem(
        wasSynthetic: boolean,
        const item = await md.GetEntityObject<MJVersionLabelItemEntity>(ENTITY_VERSION_LABEL_ITEMS, contextUser);
        item.VersionLabelID = labelId;
        item.RecordChangeID = recordChangeId;
        item.EntityID = entityInfo.ID;
        item.RecordID = recordId;
        const saved = await item.Save();
            return { Success: false, WasSynthetic: wasSynthetic, Error: 'Failed to save VersionLabelItem' };
        return { Success: true, WasSynthetic: wasSynthetic };
     * Load the current state of a single record.
    private async loadCurrentRecord(
    /** Create a failure CaptureResult. */
    private failResult(labelId: string, errorMessage: string): CaptureResult {
            Errors: [{ EntityName: '', RecordID: '', ErrorMessage: errorMessage }],
