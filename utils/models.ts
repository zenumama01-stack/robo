import { UserInfo } from "@memberjunction/core";
export type RasaResponse<T = Record<string, any>> = {
    code: number,
    status_code: number,
        errors: string,
        next_link: string,
        record_count: number,
        request: string,
        response_time: number,
        timestamp: number,
        token_expiration: number,
        total_community_count: number | null,
        total_query_count: number | null,
    request?: Record<string, any>,
    results: T[],
export type RasaTokenResponse = {
    'rasa-token': string
export type GetEmbeddingParams = {
     * Access token received from the /tokens endpoint
    AccessToken: string,
     * Identifier for an entity
    EntityID: string,
     * Entity type
    EntityType: 'article' | 'person' | 'session' | 'others',
     * Data source for locating the entity
    Source: 'rasa' | 'mj.pinecone'
     * Optionally exclude embedding results in the API response
    ExcludeEmbeddings?: boolean
export type GetEmbeddingResponse = {
    created: string,
    engine: string,
    type: string,
    vector_id: string,
    version: string
export type RecommendationResponse = {
    version: string, 
    score: number,
    vector_id: string
export type GetRecommendationParams = {
    Options: RecommendContextData, 
    VectorID: string,
    ErrorListID: string,
    CurrentUser?: UserInfo
export type GetRecommendationResults = {
    Recommendations: RecommendationResponse[] | null,
    ErrorMessage?: string
export type RecommendContextData = {
    EntityDocumentID: string,
    TypeMap: Record<string, string>
    filters: { type: string, max_results: number } []
export type MSGraphGetResponse<T> = {
    "@odata.context": string,
    value: T
export type GetMessagesContextDataParams = {
     * The email address of the service account to use. If not provide,
     * defaults to the AZURE_ACCOUNT_EMAIL config variable
    Email?: string
     * If true, messages will be returned with the body stripped of HTLM tags
    ReturnAsPlainText?: boolean;
     * If true, messages will be marked as read after being processed
    MarkAsRead?: boolean;
     * Filter to use in the MS Graph request to fetch messages. Defaults to fetching messages not marked as read.
     * Number of messages to return, defaults to 10.
    Top?: number;
