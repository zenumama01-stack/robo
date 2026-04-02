import { BaseEngine, Metadata, UserInfo, LogStatus, RunView, EntityInfo, LogError, IMetadataProvider, BaseEnginePropertyConfig } from '@memberjunction/core';
import { MJListDetailEntityType, MJListEntity, MJListEntityType, MJRecommendationEntity, MJRecommendationProviderEntity, MJRecommendationRunEntity } from '@memberjunction/core-entities';
import { RecommendationProviderBase } from './ProviderBase';
import { RecommendationRequest, RecommendationResult } from './generic/types';
 * Engine class to be used for running all recommendation requests
export class RecommendationEngineBase extends BaseEngine<RecommendationEngineBase> {
  private _RecommendationProviders: MJRecommendationProviderEntity[] = [];
  public static get Instance(): RecommendationEngineBase {
    return super.getInstance<RecommendationEngineBase>();
  public get RecommendationProviders(): MJRecommendationProviderEntity[] {
    return this._RecommendationProviders;
        PropertyName: '_RecommendationProviders',
        EntityName: 'MJ: Recommendation Providers',
  public async Recommend<T>(request: RecommendationRequest<T>): Promise<RecommendationResult> {
    super.TryThrowIfNotLoaded();
    let provider: MJRecommendationProviderEntity = request.Provider;
    if (!provider){
      if(this.RecommendationProviders.length == 0) {
        throw new Error('No recommendation provider provider and no provider found in metadata');
        provider = this.RecommendationProviders[0];
    LogStatus(`Recommendation Engine is using provider: ${provider.Name}`);
    // get the driver
    const driver = MJGlobal.Instance.ClassFactory.CreateInstance<RecommendationProviderBase>(RecommendationProviderBase, provider.Name);
    if(!driver) {
      throw new Error(`Could not find driver for provider: ${provider.Name}`);
    const recommendations: MJRecommendationEntity[] = await this.GetRecommendationEntities(request);
    LogStatus(`Processing ${recommendations.length} recommendations`);
    if(recommendations.length == 0) {
        ErrorMessage: 'No records found to get recommendations for',
      } as any;
    request.Recommendations = recommendations;
    // load the run
    const recommendationRunEntity = await new Metadata().GetEntityObject<MJRecommendationRunEntity>('MJ: Recommendation Runs', request.CurrentUser);
    recommendationRunEntity.NewRecord();
    // update status for current run
    recommendationRunEntity.Status = 'In Progress';
    recommendationRunEntity.RecommendationProviderID = provider.ID;
    recommendationRunEntity.StartDate = new Date();
    recommendationRunEntity.RunByUserID = request.CurrentUser ? request.CurrentUser.ID : super.ContextUser.ID;
    const saveResult: boolean = await recommendationRunEntity.Save();
    if(!saveResult) {
      LogStatus(`Error saving RecommendationRun entity: `, undefined, recommendationRunEntity.LatestResult);
      throw new Error('Error creating Recommendation Run entity');
    if(request.CreateErrorList){
      const errorList: MJListEntity | null = await this.CreateRecommendationErrorList(recommendationRunEntity.ID, recommendations[0].SourceEntityID, request.CurrentUser);
      if(errorList){
        request.ErrorListID = errorList.ID;
    request.RunID = recommendationRunEntity.ID;
    const recommendResult: RecommendationResult = await driver.Recommend(request);
    recommendationRunEntity.Status = recommendResult.Success ? 'Completed' : 'Error';
    recommendationRunEntity.Description = recommendResult.ErrorMessage;
    const postRunSaveResult: boolean = await recommendationRunEntity.Save();
    if(!postRunSaveResult) {
      throw new Error('Error updating Recommendation Run entity');
    return recommendResult;
  private async GetRecommendationEntities(request: RecommendationRequest): Promise<MJRecommendationEntity[]> {
    if(request.Recommendations){
      const invalidEntities: MJRecommendationEntity[] = request.Recommendations.filter((r) => !this.IsNullOrUndefined(r.RecommendationRunID) || r.IsSaved);
      if(invalidEntities.length > 0){
        throw new Error(`Recommendation entities must be new, not saved and have their RecommendationRunID not set. Invalid entities: ${invalidEntities.map((r) => r.ID).join(',')}`);
      return request.Recommendations;
    else if(request.ListID){
      return await this.GetRecommendationsByListID(request.ListID, request.CurrentUser);
    else if(request.EntityAndRecordsInfo){
      const entityName = request.EntityAndRecordsInfo.EntityName;
      const recordIDs = request.EntityAndRecordsInfo.RecordIDs;
        throw new Error('Entity name is required in EntityAndRecordsInfo');
      if(!recordIDs){
        throw new Error('RecordIDs are required in EntityAndRecordsInfo');
      return await this.GetRecommendationsByRecordIDs(entityName, recordIDs, request.CurrentUser);
  private async GetRecommendationsByListID(listID: string, currentUser?: UserInfo): Promise<MJRecommendationEntity[]> {
    const rvListDetailsResult = await rv.RunViews([
      { /* Getting the List to get the entity name */
        EntityName: 'MJ: Lists',
        ExtraFilter: `ID = '${listID}'`,
      { /* Getting the List Details to get the record IDs */
        EntityName: 'MJ: List Details',
        ExtraFilter: `ListID = '${listID}'`,
        IgnoreMaxRows: true,
    ], currentUser);
    const listViewResult = rvListDetailsResult[0];
    if(!listViewResult.Success) {
      throw new Error(`Error getting list with ID: ${listID}: ${listViewResult.ErrorMessage}`);
    if(listViewResult.Results.length == 0) {
      throw new Error(`No list found with ID: ${listID}`);
    const list: MJListEntityType = listViewResult.Results[0];
    const entityName: string = list.Entity;
    LogStatus(`Getting recommendations for list: ${list.Name}. Entity: ${entityName}`);
    const entityID: string = list.EntityID;
    const entity: EntityInfo = md.Entities.find((e) => e.ID == entityID);
    const needsQuotes: string = entity.FirstPrimaryKey.NeedsQuotes? "'" : '';
    const listDetailsResult = rvListDetailsResult[1];
    if(!listDetailsResult.Success) {
      throw new Error(`Error getting list details for listID: ${listID}: ${listDetailsResult.ErrorMessage}`);
    //list is empty, just exit early
    if(listDetailsResult.Results.length == 0) {
    const recordIDs: string = listDetailsResult.Results.map((ld: MJListDetailEntityType) => `${needsQuotes}${ld.RecordID}${needsQuotes}`).join(',');
    const rvEntityResult = await rv.RunView({
      ExtraFilter: `${entity.FirstPrimaryKey.Name} IN (${recordIDs})`,
    if(!rvEntityResult.Success) {
      throw new Error(`Error getting entity records for listID: ${listID}: ${rvEntityResult.ErrorMessage}`);
    let recommendations: MJRecommendationEntity[] = [];
    for(const entity of rvEntityResult.Results) {
      const recommendationEntity: MJRecommendationEntity = await md.GetEntityObject<MJRecommendationEntity>('MJ: Recommendations', currentUser);
      recommendationEntity.NewRecord();
      recommendationEntity.SourceEntityID = entityID;
      recommendationEntity.SourceEntityRecordID = entity.ID;
      recommendations.push(recommendationEntity);
    return recommendations;
  private async GetRecommendationsByRecordIDs(entityName: string, recordIDs: Array<string | number>, currentUser?: UserInfo): Promise<MJRecommendationEntity[]> {
    const entity: EntityInfo = md.Entities.find((e) => e.Name.toLowerCase() == entityName.toLowerCase());
    if(!entity) {
      throw new Error(`Unable to get recommendations by entity info: Entity not found with name: ${entityName}`);
    LogStatus(`Getting recommendations for entity: ${entityName}`);
    const needsQuotes: string = entity.FirstPrimaryKey.NeedsQuotes ? "'" : '';
    const recordIDsFilter: string = recordIDs.map((id) => `${needsQuotes}${id}${needsQuotes}`).join(',');
      ExtraFilter: `${entity.FirstPrimaryKey.Name} IN (${recordIDsFilter})`,
      throw new Error(`Error getting entity records for entity: ${entityName}: ${rvEntityResult.ErrorMessage}`);
      recommendationEntity.SourceEntityID = entity.ID;
  private async CreateRecommendationErrorList(recommendationRunID: string, entityID: string, currentUser?: UserInfo): Promise<MJListEntity | null> {
    const list: MJListEntity = await md.GetEntityObject<MJListEntity>('MJ: Lists', currentUser);
    list.Name = `Recommendation Run ${recommendationRunID} Errors`;
    list.EntityID = entityID;
    list.UserID = currentUser ? currentUser.ID : super.ContextUser.ID;
    const saveResult: boolean = await list.Save();
      LogError(`Error saving Recommendation Error List entity: `, undefined, list.LatestResult);
      LogStatus(`Error list created for recommendation run: ${recommendationRunID}. List ID: ${list.ID}`);
  private IsNullOrUndefined(value: unknown): boolean {
    return value === null || value === undefined;
import { BaseCommunicationProvider, CommunicationEngineBase, CreateDraftResult, Message, MessageRecipient, MessageResult, ProviderCredentialsBase } from "@memberjunction/communication-types";
import { MJCommunicationRunEntity } from "@memberjunction/core-entities";
import { LogError, LogStatus, UserInfo } from "@memberjunction/core";
import { ProcessedMessageServer } from "./BaseProvider";
export class CommunicationEngine extends CommunicationEngineBase {
    public static get Instance(): CommunicationEngine {
        return super.getInstance<CommunicationEngine>();    
      * Gets an instance of the class for the specified provider. The provider must be one of the providers that are configured in the system.
      * @param providerName 
     public GetProvider(providerName: string): BaseCommunicationProvider {
        if (!this.Loaded){
            throw new Error(`Metadata not loaded. Call Config() before accessing metadata.`);
        const instance = MJGlobal.Instance.ClassFactory.CreateInstance<BaseCommunicationProvider>(BaseCommunicationProvider, providerName);
        if (instance) {
            // make sure the class we got back is NOT an instance of the base class, that is the default behavior of CreateInstance if we 
            // dont have a registration for the class we are looking for
            if (instance.constructor.name === 'BaseCommunicationProvider'){
                throw new Error(`Provider ${providerName} not found.`);
                return instance; // we got a valid instance of the sub-class we were looking for
      * Sends multiple messages using the specified provider. The provider must be one of the providers that are configured in the
      * system.
      * @param providerName - Name of the communication provider to use
      * @param providerMessageTypeName - Type of message to send
      * @param message - Base message (To will be replaced with each recipient)
      * @param recipients - Array of recipients to send to
      * @param previewOnly - If true, only preview without sending
      *                      Provider-specific credential object (e.g., SendGridCredentials).
      *                      Set `credentials.disableEnvironmentFallback = true` to require explicit credentials.
     public async SendMessages(
        providerName: string,
        providerMessageTypeName: string,
        message: Message,
        recipients: MessageRecipient[],
        previewOnly: boolean = false,
     ): Promise<MessageResult[]> {
        const run = await this.StartRun();
        if (!run)
            throw new Error(`Failed to start communication run.`);
        const results: MessageResult[] = [];
        for (const r of recipients) {
            const messageCopy = new Message(message);
            messageCopy.To = r.To;
            messageCopy.ContextData = r.ContextData;
            const result = await this.SendSingleMessage(providerName, providerMessageTypeName, messageCopy, run, previewOnly, credentials);
        if (!await this.EndRun(run))
            throw new Error(`Failed to end communication run.`);
      * Sends a single message using the specified provider. The provider must be one of the providers that are configured in the system.
      * @param message - The message to send
      * @param run - Optional communication run entity for logging
     public async SendSingleMessage(
        run?: MJCommunicationRunEntity,
        previewOnly?: boolean,
     ): Promise<MessageResult> {
        const provider = this.GetProvider(providerName);
        const providerEntity = this.Providers.find((p) => p.Name === providerName);
        if (!providerEntity){
        if (!message.MessageType) {
            // find the message type
            const providerMessageType = providerEntity.MessageTypes.find((pmt) => pmt.Name.trim().toLowerCase() === providerMessageTypeName.trim().toLowerCase());
            if (!providerMessageType){
                throw new Error(`Provider message type ${providerMessageTypeName} not found.`);
            message.MessageType = providerMessageType;
        // now, process the message
        const processedMessage = new ProcessedMessageServer(message);
        const processResult = await processedMessage.Process(false, this.ContextUser);
        if (processResult.Success) {
            if (previewOnly) {
                return { Success: true, Error: '', Message: processedMessage };
                const log = await this.StartLog(processedMessage, run);
                if (log) {
                    const sendResult = await provider.SendSingleMessage(processedMessage, credentials);
                    log.Status = sendResult.Success ? 'Complete' : 'Failed';
                    log.ErrorMessage = sendResult.Error;
                    if (!await log.Save()){
                        throw new Error(`Failed to complete log for message: ${log.LatestResult?.Message}`);
                        return sendResult;
                    throw new Error(`Failed to start log for message.`);
            throw new Error(`Failed to process message: ${processResult.Message}`);
      * Creates a draft message using the specified provider
      * @param message - The message to save as a draft
      * @param providerName - Name of the provider to use
      * @param contextUser - Optional user context for server-side operations
      *                      Provider-specific credential object (e.g., MSGraphCredentials).
     public async CreateDraft(
     ): Promise<CreateDraftResult> {
             if (!this.Loaded) {
                     ErrorMessage: 'Metadata not loaded. Call Config() before creating drafts.'
             // Get provider instance
                     ErrorMessage: `Provider ${providerName} not found`
             // Check if provider supports drafts
             const providerEntity = this.Providers.find(p => p.Name === providerName);
             if (!providerEntity?.SupportsDrafts) {
                     ErrorMessage: `Provider ${providerName} does not support creating drafts`
             // Process message (render templates)
             const processResult = await processedMessage.Process(false, contextUser || this.ContextUser);
             if (!processResult.Success) {
                     ErrorMessage: `Failed to process message: ${processResult.Message}`
             // Create draft via provider
             const result = await provider.CreateDraft({
                 Message: processedMessage,
                 ContextData: message.ContextData
             }, credentials);
                 LogStatus(`Draft created successfully via ${providerName}. Draft ID: ${result.DraftID}`);
                 LogError(`Failed to create draft via ${providerName}`, undefined, result.ErrorMessage);
             const errorMessage = error instanceof Error ? error.message : 'Error creating draft';
             LogError('Error creating draft', undefined, error);
                 ErrorMessage: errorMessage
import { BaseEngine, BaseEnginePropertyConfig, LogError, UserInfo, BaseEntity, IMetadataProvider } from "@memberjunction/core";
import { MJLibraryEntity, MJLibraryItemEntity } from "@memberjunction/core-entities";
 * Represents a single item within a library/package that is used to provide documentation for the MemberJunction system. For example a library would be something like
 * @memberjunction/core and an item within that library might be the BaseEntity or BaseEngine class.
@RegisterClass(BaseEntity, "MJ: Library Items")
export class LibraryItemEntityExtended extends MJLibraryItemEntity {
    HTMLContent: string;
    public get TypeURLSegment(): string {
        switch(this.Type){
            case 'Class':
                return "classes"
            case "Interface":
                return "interfaces";
            case "Function":
                return "functions";
            case 'Module':
                return "modules";
                return "types";
            case 'Variable':
                return "variables";
                throw new Error("Unknown type " + this.Type);
@RegisterClass(BaseEntity, "MJ: Libraries")
export class LibraryEntityExtended extends MJLibraryEntity {
    private _items: LibraryItemEntityExtended[] = [];
    public get Items(): LibraryItemEntityExtended[] {
        return this._items;
 * Provides utility functionality for documentation of the MemberJunction system using external website content from the MemberJunction project.
export class DocumentationEngine extends BaseEngine<DocumentationEngine> {
    public static get Instance(): DocumentationEngine {
        return super.getInstance<DocumentationEngine>();
    private _Libraries: LibraryEntityExtended[] = [];
    private _LibraryItems: LibraryItemEntityExtended[] = [];
                EntityName: 'MJ: Libraries',
                PropertyName: '_Libraries',
                EntityName: 'MJ: Library Items',
                PropertyName: '_LibraryItems',
    private _baseURL: string = 'https://memberjunction.github.io/MJ/';
        // Load the items for each library using the comma delimited list of included items in the Library metadata
        for (const library of this.Libraries) {
            const items: LibraryItemEntityExtended[] = this.LibraryItems.filter((item: LibraryItemEntityExtended) => item.LibraryID === library.ID);
                // lib code name is replace all instances of @ . and / or \ in the library name with _
                const URLSegment = library.Name.replace(/[@.\/\\]/g, '_');
                item.URL = `${this._baseURL}${item.TypeURLSegment}/${URLSegment}.${item.Name}.html`;
                item.HTMLContent = await this.GetContent(item.URL);
                library.Items.push(item);
    protected async GetContent(url: string, rootSelector?: string): Promise<string> {
        const html = await this.fetchDocumentation(url);
        return this.parseDocumentation(html, rootSelector);
    protected async fetchDocumentation(url: string): Promise<string> {
            const response = await axios.get(url);
                return 'No content found';
            return "Error fetching content"
    protected  parseDocumentation(html: string, rootSelector?: string): string {
        const document = dom.window.document;
        const content = document.querySelector('div.col-content')?.innerHTML;
        return content || 'No relevant content found';
    public get Libraries(): LibraryEntityExtended[] {
        return this._Libraries;
     public get LibraryItems(): LibraryItemEntityExtended[] {
        return this._LibraryItems;
