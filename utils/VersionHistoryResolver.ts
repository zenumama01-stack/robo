import { Resolver, Mutation, Arg, Ctx, ObjectType, Field, Int, InputType, PubSub, PubSubEngine } from 'type-graphql';
import { LogError, LogStatus, CompositeKey, KeyValuePair } from '@memberjunction/core';
import { VersionHistoryEngine } from '@memberjunction/version-history';
import { CreateLabelParams, CreateLabelProgressUpdate, VersionLabelScope } from '@memberjunction/version-history';
import { KeyValuePairInput } from '../generic/KeyValuePairInput.js';
// GraphQL types
export class CaptureErrorOutput {
export class CreateLabelResultOutput {
    @Field(() => [CaptureErrorOutput], { nullable: true })
    CaptureErrors?: CaptureErrorOutput[];
export class CreateVersionLabelInput {
    RecordKeys?: KeyValuePairInput[];
export class VersionHistoryResolver extends ResolverBase {
    @Mutation(() => CreateLabelResultOutput)
    async CreateVersionLabel(
        @Arg('input') input: CreateVersionLabelInput,
        @Arg('sessionId', { nullable: true }) sessionId: string,
    ): Promise<CreateLabelResultOutput> {
        const contextUser = this.GetUserFromPayload(context.userPayload);
            return { Success: false, Error: 'Unable to determine user context.' };
            const engine = new VersionHistoryEngine();
            // Build CompositeKey from input key pairs if provided
            let recordKey: CompositeKey | undefined;
            if (input.RecordKeys && input.RecordKeys.length > 0) {
                recordKey = new CompositeKey(
                    input.RecordKeys.map(kv => ({
                        FieldName: kv.Key,
                        Value: kv.Value ?? ''
                    } as KeyValuePair))
            // Build progress callback that publishes to PubSub
            const resolvedSessionId = sessionId ?? context.userPayload.sessionId;
            const onProgress = this.buildProgressCallback(pubSub, resolvedSessionId);
            const params: CreateLabelParams = {
                Name: input.Name,
                Scope: (input.Scope as VersionLabelScope) ?? 'Record',
                EntityName: input.EntityName,
                RecordKey: recordKey,
                ParentID: input.ParentID,
                ExternalSystemID: input.ExternalSystemID,
                IncludeDependencies: input.IncludeDependencies,
                MaxDepth: input.MaxDepth,
                ExcludeEntities: input.ExcludeEntities,
                OnProgress: onProgress,
            const result = await engine.CreateLabel(params, contextUser);
            LogStatus(`VersionHistory resolver: Label '${input.Name}' created with ${result.CaptureResult.ItemsCaptured} items.`);
                Success: result.CaptureResult.Success,
                LabelID: result.Label.ID,
                LabelName: result.Label.Name,
                ItemsCaptured: result.CaptureResult.ItemsCaptured,
                SyntheticSnapshotsCreated: result.CaptureResult.SyntheticSnapshotsCreated,
                CaptureErrors: result.CaptureResult.Errors.map(e => ({
                    EntityName: e.EntityName,
                    RecordID: e.RecordID,
                    ErrorMessage: e.ErrorMessage,
            LogError(`VersionHistory resolver: CreateLabel failed: ${msg}`);
     * Build a progress callback that publishes CreateLabelProgress messages
     * to the PubSub system for real-time client consumption.
    private buildProgressCallback(
        sessionId: string
    ): (progress: CreateLabelProgressUpdate) => void {
        return (progress: CreateLabelProgressUpdate) => {
                    resolver: 'VersionHistoryResolver',
                    type: 'CreateLabelProgress',
                    data: progress,
