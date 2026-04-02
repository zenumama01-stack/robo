import { Resolver, Mutation, Arg, Ctx, ObjectType, Field } from 'type-graphql';
import { LogError, LogStatus, Metadata, RunView } from '@memberjunction/core';
export class TemplateRunResult {
export class RunTemplateResolver extends ResolverBase {
    @Mutation(() => TemplateRunResult)
    async RunTemplate(
        @Arg('templateId') templateId: string,
        @Arg('contextData', { nullable: true }) contextData?: string
    ): Promise<TemplateRunResult> {
        // Check API key scope authorization for template execution
        await this.CheckAPIKeyScopeAuthorization('template:execute', templateId, userPayload);
            LogStatus(`=== RUNNING TEMPLATE FOR ID: ${templateId} ===`);
            // Parse context data (JSON string)
            let data = {};
            if (contextData) {
                    data = JSON.parse(contextData);
                        error: `Invalid JSON in context data: ${(parseError as Error).message}`,
            // Load the template entity
            const templateEntity = await p.GetEntityObject<TemplateEntityExtended>('MJ: Templates', currentUser);
            await templateEntity.Load(templateId);
            if (!templateEntity.IsSaved) {
                    error: `Template with ID ${templateId} not found`,
            // Load template content (get the first/highest priority content)
            const templateContentResult = await rv.RunView<MJTemplateContentEntity>({
                ExtraFilter: `TemplateID = '${templateId}'`,
            if (!templateContentResult.Results || templateContentResult.Results.length === 0) {
                    error: `No template content found for template ${templateEntity.Name}`,
            // Configure and render the template
            await TemplateEngineServer.Instance.Config(true /*always refresh to get latest templates*/, currentUser);
            const result = await TemplateEngineServer.Instance.RenderTemplate(
                templateEntity, 
                templateContentResult.Results[0], 
                true // skip validation for execution
                LogStatus(`=== TEMPLATE RUN COMPLETED FOR: ${templateEntity.Name} (${executionTime}ms) ===`);
                    output: result.Output,
                LogError(`Template run failed for ${templateEntity.Name}: ${result.Message}`);
                    error: result.Message,
            LogError(`Template run failed:`, undefined, error);
