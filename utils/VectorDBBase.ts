import { BaseRequestParams, BaseResponse, CreateIndexParams, 
        EditIndexParams, IndexList, UpdateOptions, 
        VectorRecord } from "./record";
import { QueryOptions } from './query.types';
export abstract class VectorDBBase {
            throw new Error('API key cannot be empty');
    //Union types to allow the sub class implementing the functions to mark them as async or not
    abstract listIndexes(): IndexList | Promise<IndexList>;
    abstract getIndex(params: BaseRequestParams): BaseResponse | Promise<BaseResponse>;
    abstract createIndex(params: CreateIndexParams): BaseResponse | Promise<BaseResponse>;
    abstract deleteIndex(params: BaseRequestParams): BaseResponse | Promise<BaseResponse>;
    abstract editIndex(params: EditIndexParams): BaseResponse  | Promise<BaseResponse>;
    abstract queryIndex(params: QueryOptions): BaseResponse | Promise<BaseResponse>;
    abstract createRecord(record: VectorRecord): BaseResponse | Promise<BaseResponse>;
    abstract createRecords(record: VectorRecord[]): BaseResponse  | Promise<BaseResponse>;
    abstract getRecord(param: BaseRequestParams): BaseResponse  | Promise<BaseResponse>;
    abstract getRecords(params: BaseRequestParams): BaseResponse  | Promise<BaseResponse>;
    abstract updateRecord(record: UpdateOptions): BaseResponse  | Promise<BaseResponse>;
    abstract updateRecords(records: UpdateOptions): BaseResponse  | Promise<BaseResponse>;
    abstract deleteRecord(record: VectorRecord): BaseResponse  | Promise<BaseResponse>;
    abstract deleteRecords(records: VectorRecord[]): BaseResponse  | Promise<BaseResponse>;
