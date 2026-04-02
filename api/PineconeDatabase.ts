import { pineconeDefaultIndex } from '../config';
import { error } from 'console';
import { RegisterClass } from '@memberjunction/global'
import { FetchResponse, Index, Pinecone, QueryOptions } from '@pinecone-database/pinecone';
import { BaseRequestParams, BaseResponse, CreateIndexParams, EditIndexParams, IndexDescription, IndexList, QueryResponse, RecordMetadata, VectorDBBase, VectorRecord } from '@memberjunction/ai-vectordb';
export type BaseMetadata = {
    RecordID: string
    Entity: string,
    TemplateID: string,
@RegisterClass(VectorDBBase, "PineconeDatabase")
export class PineconeDatabase extends VectorDBBase {
    private _pinecone: Pinecone;
    private _defaultIndex: Index<RecordMetadata> = null;
    constructor(apiKey: string){
        this._pinecone = new Pinecone({
        throw new Error('Method not implemented.');
    get pinecone(): Pinecone { return this._pinecone; }
    public async getIndexDescription(params: BaseRequestParams): Promise<IndexDescription> {
        const description: IndexDescription = await this.pinecone.describeIndex(params.id);
    public async getDefaultIndex(): Promise<Index<RecordMetadata>> {
        if(this._defaultIndex){
            return this._defaultIndex;
        if(pineconeDefaultIndex){
            let defaultIndex = this.pinecone.Index(pineconeDefaultIndex);
            if(defaultIndex){
                this._defaultIndex = defaultIndex;
                return defaultIndex;
        const indexList = await this.listIndexes();
        if(indexList && indexList.indexes && indexList.indexes.length > 0){
            const indexName: string = indexList.indexes[0].name;
            this._defaultIndex = this.pinecone.index(indexName);
        LogStatus("Attempted to fetch default index but none were found");
    public async listIndexes(): Promise<IndexList> {
        const indexes: IndexList = await this.pinecone.listIndexes();
        return indexes;
     * If an indexName is not provided, this will use the default index name
     * defined in the environment variables instead.
    public getIndex(params?: BaseRequestParams): BaseResponse {
        const name: string = params?.id || pineconeDefaultIndex;
        if(!name){
            throw new Error("id not found in params and PINECONE_DEFAULT_INDEX not found in env variables");
        const result: BaseResponse = {
            message: "",
            data: this.pinecone.Index(name)
     * The minimum required configuration to create an index is the index `name`, `dimension`, and `spec`.
     * ```js
     * await pinecone.createIndex({ name: 'my-index', dimension: 128, spec: { serverless: { cloud: 'aws', region: 'us-west-2' }}})
    public async createIndex(options: CreateIndexParams): Promise<BaseResponse> {
            const result = await this.pinecone.createIndex({
                name: options.id,
                dimension: options.dimension,
                metric: options.metric,
                spec: options.additionalParams
            return this.wrapSuccessResponse(result);
            LogError("Error creating index", undefined, ex);
            return this.wrapFailureResponse();
    public async deleteIndex(params: BaseRequestParams): Promise<BaseResponse> {
            await this.pinecone.deleteIndex(params.id);
            return this.wrapSuccessResponse(null);
            LogError("Error deleting index", undefined, ex);
    public async editIndex(params: EditIndexParams): Promise<BaseResponse> {
       throw new error("Method not implemented");
    public async queryIndex(params: QueryOptions): Promise<BaseResponse> {
            let index: Index = this.getIndex().data;
            let result: QueryResponse = await index.query(params);
            LogError("Error querying index", undefined, ex);
    public async createRecord(params: VectorRecord): Promise<BaseResponse> {
            let records: VectorRecord[] = [params];
            let result = await this.createRecords(records);
            LogError("Error creating record", undefined, ex);
    public async createRecords(records: VectorRecord[]): Promise<BaseResponse> {
            const index: Index = await this.getIndex().data;
            let result = await index.upsert(records);
            console.log(ex);
    public async getRecord(params: BaseRequestParams): Promise<BaseResponse> {
            let result = await this.getRecords(params);
            LogError("Error getting record", undefined, ex);
    public async getRecords(params: BaseRequestParams): Promise<BaseResponse> {
            const index: Index = this.getIndex().data;
            const fetchResult: FetchResponse = await index.fetch(params.data);
            return this.wrapSuccessResponse(fetchResult);
            LogError("Error getting records", undefined, ex);
    public async updateRecord(params: BaseRequestParams): Promise<BaseResponse> {
            let result = index.update(params.data);
            LogError("Error updating record", undefined, ex);
    public async updateRecords(params: BaseRequestParams): Promise<BaseResponse> {
        throw new Error("Method not implemented");
    public async deleteRecord(record: VectorRecord): Promise<BaseResponse> {
            let result = index.deleteOne(record.id);
            LogError("Error deleting record", undefined, ex);
    public async deleteRecords(records: VectorRecord[]): Promise<BaseResponse> {
            const IDMap: string[] = records.map((record: VectorRecord) => record.id);
            let result = index.deleteMany(IDMap);
    public async deleteAllRecords(params: BaseRequestParams): Promise<BaseResponse> {
            if(params?.data){
                await index.namespace(params?.data).deleteAll();
                index.deleteAll();
            LogError("Error deleting all records", undefined, ex);
    private wrapSuccessResponse(data: any): BaseResponse {
            message: null,
    private wrapFailureResponse(message?: string): BaseResponse {
            message: message || "An error occurred",
