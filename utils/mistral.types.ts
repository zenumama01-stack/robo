export interface ModelPermission {
    object: 'model_permission';
    allow_create_engine: boolean;
    allow_sampling: boolean;
    allow_logprobs: boolean;
    allow_search_indices: boolean;
    allow_view: boolean;
    allow_fine_tuning: boolean;
    organization: string;
    group: string | null;
    is_blocking: boolean;
export interface Model {
    object: 'model';
    owned_by: string;
    root: string | null;
    parent: string | null;
    permission: ModelPermission[];
export interface ListModelsResponse {
    object: 'list';
    data: Model[];
export interface TokenUsage {
export type ChatCompletetionRequest = {
    messages: ChatMessage[],
    temperature?: number,
    max_tokens?: number,
    top_p?: number,
    random_seed?: number,
    stream?: boolean,
    safe_prompt?: boolean,
    response_format?: any
export interface ChatCompletionResponseChoice {
export interface ChatCompletionResponseChunkChoice {
export interface ChatCompletionResponse {
    object: 'chat.completion';
    choices: ChatCompletionResponseChoice[];
    usage: TokenUsage;
export interface ChatCompletionResponseChunk {
    object: 'chat.completion.chunk';
    choices: ChatCompletionResponseChunkChoice[];
export interface Embedding {
    object: 'embedding';
    embedding: number[];
export interface EmbeddingResponse {
    data: Embedding[];
