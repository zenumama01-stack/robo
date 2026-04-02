import { BaseEntity, Metadata, CompositeKey, RunView, UserInfo, EntityInfo, RunViewResult, LogError } from "@memberjunction/core";
import { MJVectorDatabaseEntity } from "@memberjunction/core-entities";
import { PageRecordsParams } from "../generic/VectorCore.types";
import { AIModelEntityExtended } from "@memberjunction/ai-core-plus";
export class VectorBase {
    _runView: RunView;
    _metadata: Metadata;
    _currentUser: UserInfo;
        this._runView = new RunView();
        this._currentUser = this._metadata.CurrentUser;
    public get Metadata(): Metadata { return this._metadata; }
    public get RunView(): RunView { return this._runView; }
    public get CurrentUser(): UserInfo { return this._currentUser; }
    public set CurrentUser(user: UserInfo) { this._currentUser = user; }
    protected async GetRecordsByEntityID(entityID: string, recordIDs?: CompositeKey[]): Promise<BaseEntity[]> {
        const entity = md.Entities.find(e => e.ID === entityID);
        if (!entity){
            throw new Error(`Entity with ID ${entityID} not found.`);
        const rvResult = await this._runView.RunView<BaseEntity>({
            ExtraFilter: recordIDs ? this.BuildExtraFilter(recordIDs): undefined,
            IgnoreMaxRows: true
        }, this.CurrentUser);
        if(!rvResult.Success){
            throw new Error(rvResult.ErrorMessage);
        return rvResult.Results;
    protected async PageRecordsByEntityID<T>(params: PageRecordsParams): Promise<T[]> {
        const entity: EntityInfo | undefined = this.Metadata.Entities.find((e) => e.ID === params.EntityID);
          throw new Error(`Entity with ID ${params.EntityID} not found.`);
        const rvResult: RunViewResult<T> = await this._runView.RunView<T>({
            ResultType: params.ResultType,
            MaxRows: params.PageSize,
            StartRow: Math.max(0, (params.PageNumber - 1) * params.PageSize),
            ExtraFilter: params.Filter
        if (!rvResult.Success) {
    protected BuildExtraFilter(CompositeKey: CompositeKey[]): string {
        return CompositeKey.map((keyValue) => {
            return keyValue.KeyValuePairs.map((keys) => {
                return `${keys.FieldName} = '${keys.Value}'`;
            }).join(" AND ");
        }).join("\n OR ");
    protected GetAIModel(id?: string): AIModelEntityExtended {
        let model: AIModelEntityExtended;
        if(id){
            model = AIEngine.Instance.Models.find(m => m.AIModelType === "Embeddings" && m.ID === id);
            model = AIEngine.Instance.Models.find(m => m.AIModelType === "Embeddings");
        if(!model){
            throw new Error("No AI Model Entity found");
    protected GetVectorDatabase(id?: string): MJVectorDatabaseEntity {
        if(AIEngine.Instance.VectorDatabases.length > 0){
                let vectorDB = AIEngine.Instance.VectorDatabases.find(vd => vd.ID === id);
                if(vectorDB){
                    return vectorDB;
                return AIEngine.Instance.VectorDatabases[0];
        throw new Error("No Vector Database Entity found");
    protected async RunViewForSingleValue<T extends BaseEntity>(entityName: string, extraFilter: string): Promise<T | null> {
        const rvResult = await this._runView.RunView({
            ExtraFilter: extraFilter,
        if(rvResult.Success){
            return rvResult.RowCount > 0 ? rvResult.Results[0] as T: null;
            LogError(rvResult.ErrorMessage);
     * Saving an Entity in any vector related package needs the CurrentUser property to be set on the entity
     * So this is a simple wrapper to set it before saving
    protected async SaveEntity(entity: BaseEntity): Promise<boolean> {
        entity.ContextCurrentUser = this.CurrentUser;
        return await entity.Save();
