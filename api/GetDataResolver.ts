import { Arg, Ctx, Field, InputType, ObjectType, Query } from 'type-graphql';
import { GetReadOnlyDataSource, GetReadOnlyProvider } from '../util.js';
export class GetDataInputType {
export class GetDataOutputType {
     * Each query's results will be converted to JSON and returned as a string
export class SimpleEntityResultType {
    @Field(() => [SimpleEntityOutputType])
    Results: SimpleEntityOutputType[];
export class SimpleEntityOutputType {
    @Field(() => [SimpleEntityFieldOutputType])
    Fields: SimpleEntityFieldOutputType[];
export class SimpleEntityFieldOutputType {
 * General purpose resolver for fetching different kinds of data payloads for SYSTEM users only.
export class GetDataResolver {
     * This query will sync the specified items with the existing system. Items will be processed in order and the results of each operation will be returned in the Results array within the return value.
     * @param items - an array of ActionItemInputType objects that specify the action to be taken on the specified entity with the specified primary key and the JSON representation of the field values. 
     * @param token - the short-lived access token that is required to perform this operation.
    @Query(() => GetDataOutputType)
    async GetData(
    @Arg('input', () => GetDataInputType) input: GetDataInputType,
    @Ctx() context: AppContext
    ): Promise<GetDataOutputType> {
            LogStatus(`GetDataResolver.GetData() ---- IMPORTANT - temporarily using the same connection as rest of the server, we need to separately create a READ ONLY CONNECTION and pass that in 
                       the AppContext so we can use that special connection here to ensure we are using a lower privileged connection for this operation to prevent mutation from being possible.`);
            LogStatus(`${JSON.stringify(input)}`);
            // validate the token
            if (!isTokenValid(input.Token)) {
                throw new Error(`Token ${input.Token} is not valid or has expired`);
            // Use the read-only connection for executing queries
            const readOnlyDataSource = GetReadOnlyDataSource(context.dataSources, {allowFallbackToReadWrite: false})
            if (!readOnlyDataSource) {
                throw new Error('Read-only data source not found');
            // Execute all queries in parallel, but execute each individual query in its own try catch block so that if one fails, the others can still be processed
            // and also so that we can capture the error message for each query and return it
            const results = await Promise.allSettled(
                input.Queries.map(async (query) => {
                        const request = new sql.Request(readOnlyDataSource);
                        const result = await request.query(query);
                        return { result: result.recordset, error: null };
                        // Extract clean SQL error message
                        let errorMessage = err instanceof Error ? err.message : String(err);
                        // SQL Server errors often have additional context in properties
                        // Try to get more context from the error object
                        if (err && typeof err === 'object') {
                            const sqlError = err as any;
                            // SQL Server error objects may have a 'procName' with the problematic token
                            // or 'lineNumber' and other useful properties
                            if (sqlError.precedingErrors && sqlError.precedingErrors.length > 0) {
                                // Sometimes the error has preceding errors with better context
                                const precedingError = sqlError.precedingErrors[0];
                                if (precedingError.message) {
                                    errorMessage = precedingError.message;
                            // Enhance error message with line number if available
                            if (sqlError.lineNumber && sqlError.lineNumber > 0) {
                                errorMessage = `${errorMessage} (Line ${sqlError.lineNumber})`;
                        // SQL Server errors often have the format "message\nstatement"
                        // Extract just the first line which contains the actual error
                        const firstLine = errorMessage.split('\n')[0];
                        if (firstLine) {
                            errorMessage = firstLine.trim();
                        return { result: null, error: errorMessage };
            // Extract results and errors from the promises
            const processedResults = results.map((res) => res.status === "fulfilled" ? res.value.result : null);
            const errorMessages = results.map((res) => res.status === "fulfilled" ? res.value.error : res.reason);
            // record the use of the token
            const returnVal = { Success: errorMessages.filter((e) => e !== null).length === 0, 
                                Results: processedResults.map((r) => JSON.stringify(r)), 
                                Queries: input.Queries, 
                                ErrorMessages: errorMessages }
            recordTokenUse(input.Token, {request: input, results: returnVal});
            // Success below is derived from having no errorMessages, check that array
            return returnVal;
            return { Success: false, ErrorMessages: [typeof err === 'string' ? err : (err as any).message], Results: [], Queries: input.Queries };
    @Query(() => SimpleEntityResultType)
    async GetAllEntities(
    ): Promise<SimpleEntityResultType> {
            const md = GetReadOnlyProvider(context.providers);
            const result = md.Entities.map((e) => {
                    Description: e.Description, 
                    BaseView: e.BaseView,
                    CodeName: e.CodeName,
                    ClassName: e.ClassName,
                    Fields: e.Fields.map((f) => {
                            MaxLength: f.MaxLength,
            return { Success: true, Results: result };
            return { Success: false, ErrorMessage: typeof err === 'string' ? err : (err as any).message, Results: [] };
export class TokenUseLog {
    UsedAt: Date;
    UsePayload: any;
 * Used to track all active access tokens that are requested by anyone within the server to be able to send to external services that can
 * in turn call back to the GetDataResolver to get data from the server. This is an extra security layer to ensure that tokens are short 
 * lived compared to the system level API key which rotates but less frequently.
export class GetDataAccessToken {
    RequstedAt: Date;
     * Can be used to store any payload to identify who requested the creation of the token, for example Skip might use this to put in a conversation ID to know which conversation a request is coming back for.
    RequestorPayload: any;
    TokenUses: TokenUseLog[];
const __accessTokens: GetDataAccessToken[] = [];
const __defaultTokenLifeSpan = 1000 * 60 * 5; // 5 minutes  
export function registerAccessToken(token?: string, lifeSpan: number = __defaultTokenLifeSpan, requestorPayload?: any): GetDataAccessToken {
    const tokenToUse = token || uuidv4();
    if (tokenExists(tokenToUse)) {
        // should never happen if we used the uuidv4() function but could happen if someone tries to use a custom token
        throw new Error(`Token ${tokenToUse} already exists`);
    const newToken = new GetDataAccessToken();
    newToken.Token = tokenToUse;
    newToken.ExpiresAt = new Date(new Date().getTime() + lifeSpan);
    newToken.RequstedAt = new Date();
    newToken.RequestorPayload = requestorPayload;
    __accessTokens.push(newToken);
    return newToken;
export function deleteAccessToken(token: string) {
    const index = __accessTokens.findIndex((t) => t.Token === token);
        __accessTokens.splice(index, 1);
        throw new Error(`Token ${token} does not exist`);
export function tokenExists(token: string) {
    return __accessTokens.find((t) => t.Token === token) !== undefined;
export function isTokenValid(token: string) {
    const t = __accessTokens.find((t) => t.Token === token);
    if (t) {
        return t.ExpiresAt > new Date();
export function recordTokenUse(token: string, usePayload: any) {
        if (!t.TokenUses) {
            t.TokenUses = [];
        t.TokenUses.push({ Token: token, UsedAt: new Date(), UsePayload: usePayload });
