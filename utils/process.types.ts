export class ProcessRunParams {
    sourceID: string;
    numItemsProcessed: number;
export class ContentItemProcessParams {
    modelID: string;
    minTags: number;
    maxTags: number;
    contentItemID: string;
    contentTypeID: string;
    contentFileTypeID: string;
export class ContentItemProcessResults {
    author: string[];
    publicationDate: Date;
    content_text: string;
    processStartTime: Date;
    processEndTime: Date;
export interface JsonObject {
