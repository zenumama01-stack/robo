 * Action that executes GraphQL queries and mutations
 * // Simple query
 *   ActionName: 'GraphQL Query',
 *     Name: 'Endpoint',
 *     Value: 'https://api.example.com/graphql'
 *     Value: `
 *       query GetUser($id: ID!) {
 *         user(id: $id) {
 *           id
 *           name
 *           email
 *     `
 *     Value: { id: '123' }
 * // Mutation with headers
 *       mutation CreateUser($input: CreateUserInput!) {
 *         createUser(input: $input) {
 *       input: {
 *         name: 'John Doe',
 *         email: 'john@example.com'
 *       'Authorization': 'Bearer token123'
@RegisterClass(BaseAction, "GraphQL Query")
export class GraphQLQueryAction extends BaseAction {
     * Executes a GraphQL query or mutation
     *   - Endpoint: GraphQL endpoint URL (required)
     *   - Query: GraphQL query/mutation string (required)
     *   - Variables: Variables object for the query (optional)
     *   - Headers: Additional headers for the request (optional)
     *   - OperationName: For multi-operation documents (optional)
     *   - Timeout: Request timeout in milliseconds - default: 30000
     * @returns GraphQL response data
            const endpoint = this.getParamValue(params, 'endpoint');
            const query = this.getParamValue(params, 'query');
            const variables = JSONParamHelper.getJSONParam(params, 'variables');
            const headers = JSONParamHelper.getJSONParam(params, 'headers') || {};
            const operationName = this.getParamValue(params, 'operationname');
            if (!endpoint) {
                    Message: "Endpoint parameter is required",
                    ResultCode: "MISSING_ENDPOINT"
                    Message: "Query parameter is required",
                    ResultCode: "MISSING_QUERY"
            // Validate endpoint URL
                new URL(endpoint);
                    Message: `Invalid endpoint URL: ${endpoint}`,
                    ResultCode: "INVALID_URL"
            // Prepare GraphQL request body
            const requestBody: any = { query };
            if (variables) {
                requestBody.variables = variables;
            if (operationName) {
                requestBody.operationName = operationName;
            const requestHeaders = {
                ...headers
            // Make GraphQL request
            const response = await axios.post(endpoint, requestBody, {
                headers: requestHeaders,
            // Check for HTTP errors
            if (response.status < 200 || response.status >= 300) {
                    Message: `HTTP error ${response.status}: ${response.statusText}`,
            // Check for GraphQL errors
            const responseData = response.data;
            const hasErrors = responseData.errors && responseData.errors.length > 0;
            if (responseData.data) {
                    Name: 'Data',
                    Value: responseData.data
            if (responseData.errors) {
                    Value: responseData.errors
            if (responseData.extensions) {
                    Name: 'Extensions',
                    Value: responseData.extensions
            // Determine success based on GraphQL response
            if (hasErrors && !responseData.data) {
                // Only errors, no data - complete failure
                    Message: `GraphQL errors: ${JSON.stringify(responseData.errors, null, 2)}`,
                    ResultCode: "GRAPHQL_ERROR"
            } else if (hasErrors && responseData.data) {
                // Partial success - has both data and errors
                    ResultCode: "PARTIAL_SUCCESS",
                        message: "GraphQL query completed with errors",
                        data: responseData.data,
                        errors: responseData.errors,
                        extensions: responseData.extensions
                // Complete success
                        message: "GraphQL query executed successfully",
                Message: `GraphQL request failed: ${error instanceof Error ? error.message : String(error)}`,
