import { AutotagBaseEngine, ContentSourceParams } from "../../Engine";
import { UserInfo, Metadata, RunView, BaseEntity } from "@memberjunction/core";
@RegisterClass(AutotagBase, 'AutotagEntity')
export class AutotagEntity extends AutotagBase {
    private engine: AutotagBaseEngine;
    private EntityName: string
    private EntityFields: string[]
        this.contentSourceTypeID = await this.engine.setSubclassContentSourceType('Entity', this.contextUser);
            // If content source parameters were provided, set them. Otherwise, use the default values.
            const contentSourceParamsMap = await this.engine.getContentSourceParams(contentSource, this.contextUser);
            if (contentSourceParamsMap) {
                // Override defaults with content source specific params
                contentSourceParamsMap.forEach((value, key) => {
                    if (key in this) {
                        (this as any)[key] = value;
            const contentItems: MJContentItemEntity[] = await this.getNewOrModifiedContentItems(contentSourceParams, this.contextUser);
            if (contentItems && contentItems.length > 0) {
                // No content items found to process
                console.log(`No content items found to process for content source: ${contentSource.Name}`);
    public async getNewOrModifiedContentItems(contentSourceParams: ContentSourceParams, contextUser: UserInfo): Promise<MJContentItemEntity[]> {
        const lastRunDate: Date = await this.engine.getContentSourceLastRunDate(contentSourceParams.contentSourceID, contextUser)
        const lastRunDateISOString: string = lastRunDate.toISOString();
        // Get all entities that have been added/updated since the last run date
        const results = await rv.RunView<BaseEntity>({
            Fields: this.EntityFields,
            ExtraFilter: `__mj_UpdatedAt > '${lastRunDateISOString}'`,
        const entityResults: BaseEntity[] = results.Results
        if (results.Results && results.Results.length > 0) {
            const contentItems: MJContentItemEntity[] = await this.setContentItemsFromEntityResults(entityResults, contentSourceParams, lastRunDate);
            return contentItems;
    public async setContentItemsFromEntityResults(results: BaseEntity[], contentSourceParams: ContentSourceParams, lastRunDate: Date): Promise<MJContentItemEntity[]> {
        const contentItems: MJContentItemEntity[] = [];
            const lastUpdatedAt = result.Get('__mj_UpdatedAt');
            if (lastUpdatedAt > lastRunDate) {
                const contentItem = await this.setNewContentItem(result, contentSourceParams);
                if (contentItem){
                    contentItems.push(contentItem);
    public async setNewContentItem(item: BaseEntity, contentSourceParams: ContentSourceParams): Promise<MJContentItemEntity> {
        const text = this.getTextFromEntityResult(item);
        const contentItem = await md.GetEntityObject<MJContentItemEntity>('MJ: Content Items', this.contextUser);
        contentItem.NewRecord();
        contentItem.ContentSourceID = contentSourceParams.contentSourceID;
        contentItem.Name = contentSourceParams.name
        contentItem.Description = await this.engine.getContentItemDescription(contentSourceParams, this.contextUser)
        contentItem.ContentSourceTypeID = contentSourceParams.ContentSourceTypeID
        contentItem.Text = text;
        contentItem.Checksum = await this.engine.getChecksumFromText(text);
        contentItem.URL = contentSourceParams.URL;
        if(await contentItem.Save()){
            return contentItem;
            throw new Error('Failed to save content item');
    public getTextFromEntityResult(result: BaseEntity): string {
        let text = ''
        // For every field in the field list, add the field's name and value to the text in the format FieldName: FieldValue
        for (const field of this.EntityFields) {
            const fieldVal = result.Get(field)
            text += `${field}: ${fieldVal}\n`
