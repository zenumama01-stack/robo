import axios, { AxiosRequestConfig, Method } from "axios";
 * Action that makes HTTP requests with full control over headers, authentication, and request options
 * // Simple GET request
 *   ActionName: 'HTTP Request',
 * // POST request with JSON body
 *     Value: 'POST'
 *     Name: 'Body',
 *     Value: { name: 'John Doe', email: 'john@example.com' }
 *     Value: { 'Content-Type': 'application/json' }
 * // Request with authentication
 *     Value: 'https://api.example.com/protected'
 *     Name: 'Authentication',
 *       type: 'bearer',
 *       token: 'your-api-token'
@RegisterClass(BaseAction, "HTTP Request")
export class HTTPRequestAction extends BaseAction {
     * Makes an HTTP request with configurable options
     *   - URL: Target URL (required)
     *   - Method: HTTP method (GET, POST, PUT, DELETE, etc.) - default: GET
     *   - Headers: Object with request headers
     *   - Body: Request body (string or object)
     *   - BodyType: "json" | "form" | "text" | "binary" - default: "json"
     *   - Authentication: Auth config object { type: 'basic'|'bearer', username?, password?, token? }
     *   - FollowRedirects: Boolean - default: true
     *   - MaxRedirects: Number - default: 5
     *   - ValidateStatus: Function string to validate response status
     *   - ResponseType: "json" | "text" | "arraybuffer" | "stream" - default: "json"
     * @returns Response object with status, headers, and body
            const method = (this.getParamValue(params, 'method') || 'GET').toUpperCase() as Method;
            const body = JSONParamHelper.getJSONParam(params, 'body');
            const bodyType = this.getParamValue(params, 'bodytype') || 'json';
            const authentication = JSONParamHelper.getJSONParam(params, 'authentication');
            const followRedirects = this.getBooleanParam(params, 'followredirects', true);
            const maxRedirects = this.getNumericParam(params, 'maxredirects', 5);
            const responseType = this.getParamValue(params, 'responsetype') || 'json';
            // Validate URL
                headers: { ...headers },
                maxRedirects: followRedirects ? maxRedirects : 0,
                responseType: responseType as any,
                validateStatus: () => true // We'll handle status validation ourselves
            // Handle authentication
            if (authentication) {
                const authResult = this.configureAuthentication(config, authentication);
                if (!authResult.success) {
                        Message: authResult.error,
                        ResultCode: "AUTH_CONFIG_ERROR"
            // Handle request body
                const bodyResult = this.configureRequestBody(config, body, bodyType);
                if (!bodyResult.success) {
                        Message: bodyResult.error,
                        ResultCode: "BODY_CONFIG_ERROR"
            // Make request
            const response = await axios(config);
            // Prepare response data
            let responseData = response.data;
            if (responseType === 'arraybuffer' && Buffer.isBuffer(responseData)) {
                responseData = responseData.toString('base64');
                Value: responseData
            // Check if request was successful (2xx status)
                    headers: response.headers,
                    data: responseData,
                    requestUrl: url,
                    requestMethod: method
                Message: `HTTP request failed: ${error instanceof Error ? error.message : String(error)}`,
     * Configure authentication for the request
    private configureAuthentication(config: AxiosRequestConfig, auth: any): { success: boolean; error?: string } {
        if (!auth.type) {
            return { success: false, error: "Authentication type is required" };
        switch (auth.type.toLowerCase()) {
            case 'basic':
                if (!auth.username || !auth.password) {
                    return { success: false, error: "Basic auth requires username and password" };
                config.auth = {
                    username: auth.username,
                    password: auth.password
            case 'bearer':
                if (!auth.token) {
                    return { success: false, error: "Bearer auth requires token" };
                config.headers = config.headers || {};
                config.headers['Authorization'] = `Bearer ${auth.token}`;
            case 'apikey':
                if (!auth.key || !auth.value) {
                    return { success: false, error: "API key auth requires key name and value" };
                if (auth.location === 'query') {
                    config.params = config.params || {};
                    config.params[auth.key] = auth.value;
                    config.headers[auth.key] = auth.value;
                return { success: false, error: `Unsupported authentication type: ${auth.type}` };
     * Configure request body based on type
    private configureRequestBody(config: AxiosRequestConfig, body: any, bodyType: string): { success: boolean; error?: string } {
        switch (bodyType.toLowerCase()) {
                if (!config.headers!['Content-Type']) {
                    config.headers!['Content-Type'] = 'application/json';
                if (typeof body === 'object') {
                    const formData = new URLSearchParams();
                    for (const [key, value] of Object.entries(body)) {
                        formData.append(key, String(value));
                    config.data = formData.toString();
                        config.headers!['Content-Type'] = 'application/x-www-form-urlencoded';
                    return { success: false, error: "Form body type requires an object" };
                config.data = String(body);
                    config.headers!['Content-Type'] = 'text/plain';
            case 'binary':
                if (typeof body === 'string') {
                    // Assume base64 encoded
                    config.data = Buffer.from(body, 'base64');
                    config.headers!['Content-Type'] = 'application/octet-stream';
                return { success: false, error: `Unsupported body type: ${bodyType}` };
