import { GraphQLDataProvider } from './graphQLDataProvider';
import { gql } from 'graphql-request';
// Client-side params / result types
 * Progress update received during label creation.
export interface CreateVersionLabelProgress {
    /** Current lifecycle step */
    Step: 'initializing' | 'walking_dependencies' | 'capturing_snapshots' | 'finalizing';
    /** Human-readable description of what's happening */
    /** Estimated completion percentage (0–100) */
    /** Number of records processed so far */
    RecordsProcessed?: number;
    /** Total records to process */
    TotalRecords?: number;
    /** Entity currently being processed */
    CurrentEntity?: string;
 * Parameters for creating a version label via the server-side VersionHistoryEngine.
export interface CreateVersionLabelParams {
    /** Optional longer description */
    /** Scope of the label: System, Entity, or Record (default: Record) */
    Scope?: 'System' | 'Entity' | 'Record';
    /** The target entity name (required for Entity/Record scope) */
    /** The record's primary key pairs (required for Record scope). Key = field name, Value = field value. */
    RecordKeys?: Array<{ Key: string; Value: string }>;
    /** Optional parent label ID for grouping */
    /** Optional external system reference */
    ExternalSystemID?: string;
    /** Whether to include dependent records when scope is Record (default: true) */
    IncludeDependencies?: boolean;
    /** Maximum depth of dependency graph traversal */
    MaxDepth?: number;
    /** Entity names to exclude from dependency traversal */
    ExcludeEntities?: string[];
     * Optional callback invoked with progress updates during label creation.
     * Requires an active PushStatusUpdates subscription (handled automatically).
    OnProgress?: (progress: CreateVersionLabelProgress) => void;
 * Result returned from the CreateVersionLabel mutation.
export interface CreateVersionLabelResult {
    LabelID?: string;
    LabelName?: string;
    ItemsCaptured?: number;
    SyntheticSnapshotsCreated?: number;
    CaptureErrors?: Array<{
// GraphQL Client
 * Client for executing Version History operations through GraphQL.
 * This class provides an easy way to create version labels with proper
 * server-side snapshot capture from a client application.
 * const vhClient = new GraphQLVersionHistoryClient(graphQLProvider);
 * const result = await vhClient.CreateLabel({
 *   Name: 'Before Refactor',
 *   Scope: 'Record',
 *   EntityName: 'MJ: AI Prompts',
 *   RecordKeys: [{ Key: 'ID', Value: recordId }],
 *   IncludeDependencies: true,
 *   console.log(`Created label ${result.LabelID} with ${result.ItemsCaptured} items`);
export class GraphQLVersionHistoryClient {
     * Create a version label with full server-side snapshot capture.
     * This invokes the VersionHistoryEngine on the server which:
     * 1. Creates the VersionLabel record
     * 2. Captures snapshots (VersionLabelItems) based on scope
     * 3. Updates the label with item count and duration metrics
     * If `params.OnProgress` is provided, subscribes to PushStatusUpdates
     * for real-time progress during the operation.
    public async CreateLabel(params: CreateVersionLabelParams): Promise<CreateVersionLabelResult> {
        let subscription: { unsubscribe: () => void } | undefined;
            if (params.OnProgress) {
                            if (parsed.resolver === 'VersionHistoryResolver' &&
                                parsed.type === 'CreateLabelProgress' &&
                                parsed.data) {
                                params.OnProgress!(parsed.data as CreateVersionLabelProgress);
                            // Ignore parse errors on progress messages
                mutation CreateVersionLabel($input: CreateVersionLabelInput!, $sessionId: String) {
                    CreateVersionLabel(input: $input, sessionId: $sessionId) {
                        LabelID
                        LabelName
                        ItemsCaptured
                        SyntheticSnapshotsCreated
                        CaptureErrors {
                input: this.buildInput(params),
                sessionId: this._dataProvider.sessionId,
            return this.processResult(result);
            const msg = e instanceof Error ? e.message : String(e);
            LogError(`GraphQLVersionHistoryClient.CreateLabel error: ${msg}`);
            return { Success: false, Error: msg };
    private buildInput(params: CreateVersionLabelParams): Record<string, unknown> {
        const input: Record<string, unknown> = {
            Name: params.Name,
        if (params.Description != null) input.Description = params.Description;
        if (params.Scope != null) input.Scope = params.Scope;
        if (params.EntityName != null) input.EntityName = params.EntityName;
        if (params.ParentID != null) input.ParentID = params.ParentID;
        if (params.ExternalSystemID != null) input.ExternalSystemID = params.ExternalSystemID;
        if (params.IncludeDependencies != null) input.IncludeDependencies = params.IncludeDependencies;
        if (params.MaxDepth != null) input.MaxDepth = params.MaxDepth;
        if (params.ExcludeEntities != null) input.ExcludeEntities = params.ExcludeEntities;
        if (params.RecordKeys && params.RecordKeys.length > 0) {
            input.RecordKeys = params.RecordKeys.map(kv => ({
                Key: kv.Key,
                Value: kv.Value,
    private processResult(result: Record<string, unknown>): CreateVersionLabelResult {
        const data = result?.CreateVersionLabel as Record<string, unknown> | undefined;
            return { Success: false, Error: 'Invalid response from server.' };
        const captureErrors = Array.isArray(data.CaptureErrors)
            ? (data.CaptureErrors as Array<Record<string, string>>).map(e => ({
                EntityName: e.EntityName ?? '',
                RecordID: e.RecordID ?? '',
                ErrorMessage: e.ErrorMessage ?? '',
            Success: data.Success as boolean,
            LabelID: data.LabelID as string | undefined,
            LabelName: data.LabelName as string | undefined,
            ItemsCaptured: data.ItemsCaptured as number | undefined,
            SyntheticSnapshotsCreated: data.SyntheticSnapshotsCreated as number | undefined,
            Error: data.Error as string | undefined,
            CaptureErrors: captureErrors,
