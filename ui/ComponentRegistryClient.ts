  ComponentRegistryClientConfig,
  GetComponentParams,
  SearchComponentsParams,
  ComponentSearchResult,
  ComponentResponse,
  ResolvedVersion,
  RegistryInfo,
  Namespace,
  DependencyTree,
  RegistryError,
  RegistryErrorCode,
  RetryPolicy,
  ComponentFeedbackParams,
  ComponentFeedbackResponse
 * Client for interacting with Component Registry Servers
export class ComponentRegistryClient {
  private config: ComponentRegistryClientConfig;
  private defaultTimeout = 30000; // 30 seconds
  private defaultRetryPolicy: RetryPolicy = {
    initialDelay: 1000,
    maxDelay: 10000,
    backoffMultiplier: 2
  constructor(config: ComponentRegistryClientConfig) {
      timeout: config.timeout || this.defaultTimeout,
      retryPolicy: config.retryPolicy || this.defaultRetryPolicy
    // Ensure baseUrl doesn't end with slash
    if (this.config.baseUrl.endsWith('/')) {
      this.config.baseUrl = this.config.baseUrl.slice(0, -1);
   * Get a specific component from the registry (backward compatible)
  async getComponent(params: GetComponentParams): Promise<ComponentSpec> {
    const response = await this.getComponentWithHash(params);
    if (!response.specification) {
      throw new RegistryError(
        `Component ${params.namespace}/${params.name} returned without specification`,
        RegistryErrorCode.UNKNOWN
    return response.specification;
   * Get a specific component from the registry with hash support
   * Returns ComponentResponse which includes hash and notModified flag
  async getComponentWithHash(params: GetComponentParams): Promise<ComponentResponse> {
    const { namespace, name, version = 'latest', hash, userEmail } = params;
    if (version !== 'latest') {
      queryParams.append('version', version);
    if (hash) {
      queryParams.append('hash', hash);
    if (userEmail) {
      queryParams.append('userEmail', userEmail);
    const queryString = queryParams.toString();
    const path = `/api/v1/components/${encodeURIComponent(namespace)}/${encodeURIComponent(name)}${queryString ? `?${queryString}` : ''}`;
      const response = await this.makeRequest('GET', path);
      // Handle 304 Not Modified response
      if (response && typeof response === 'object' && 'message' in response && response.message === 'Not modified') {
          ...response,
          notModified: true
        } as ComponentResponse;
      return response as ComponentResponse;
      if (error instanceof RegistryError) {
        `Failed to get component ${namespace}/${name}@${version}`,
        RegistryErrorCode.UNKNOWN,
   * Search for components in the registry
  async searchComponents(params: SearchComponentsParams): Promise<ComponentSearchResult> {
    if (params.registry) queryParams.append('registry', params.registry);
    if (params.namespace) queryParams.append('namespace', params.namespace);
    if (params.query) queryParams.append('q', params.query);
    if (params.type) queryParams.append('type', params.type);
    if (params.tags) params.tags.forEach(tag => queryParams.append('tag', tag));
    if (params.limit !== undefined) queryParams.append('limit', params.limit.toString());
    if (params.offset !== undefined) queryParams.append('offset', params.offset.toString());
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortDirection) queryParams.append('sortDirection', params.sortDirection);
    const path = `/api/v1/components/search?${queryParams.toString()}`;
      return response as ComponentSearchResult;
        'Failed to search components',
   * Resolve a version range to a specific version
  async resolveVersion(params: {
    registry: string;
    versionRange: string;
  }): Promise<ResolvedVersion> {
    const { registry, namespace, name, versionRange } = params;
    const path = `/api/v1/components/${encodeURIComponent(registry)}/${encodeURIComponent(namespace)}/${encodeURIComponent(name)}/versions/resolve`;
      const response = await this.makeRequest('POST', path, {
        versionRange
      return response as ResolvedVersion;
        `Failed to resolve version for ${registry}/${namespace}/${name}`,
   * Get information about a registry
  async getRegistryInfo(registry: string): Promise<RegistryInfo> {
    const path = `/api/v1/registries/${encodeURIComponent(registry)}`;
      return response as RegistryInfo;
        `Failed to get registry info for ${registry}`,
   * List namespaces in a registry
  async listNamespaces(registry: string): Promise<Namespace[]> {
    const path = `/api/v1/registries/${encodeURIComponent(registry)}/namespaces`;
      return response as Namespace[];
        `Failed to list namespaces for registry ${registry}`,
   * Resolve dependencies for a component
  async resolveDependencies(componentId: string): Promise<DependencyTree> {
    const path = `/api/v1/components/${encodeURIComponent(componentId)}/dependencies`;
      return response as DependencyTree;
        `Failed to resolve dependencies for component ${componentId}`,
   * Health check for the registry server
  async ping(): Promise<boolean> {
    const path = '/api/v1/health';
      await this.makeRequest('GET', path);
   * Submit feedback for a component
    const path = '/api/v1/feedback';
      const response = await this.makeRequest('POST', path, params);
      return response as ComponentFeedbackResponse;
        // Return structured error response
      // Return generic error response
        error: error instanceof Error ? error.message : 'Failed to submit feedback'
   * Make an HTTP request with retry logic
  private async makeRequest(
    path: string,
    retryCount = 0
    const url = `${this.config.baseUrl}${path}`;
    const headers: HeadersInit = {
      ...this.config.headers
    if (this.config.apiKey) {
      headers['Authorization'] = `Bearer ${this.config.apiKey}`;
    // Create abort controller for timeout
    const timeoutId = setTimeout(() => controller.abort(), this.config.timeout!);
        body: body ? JSON.stringify(body) : undefined,
      // Handle response - include 304 Not Modified as success
      if (response.ok || response.status === 304) {
        if (contentType && contentType.includes('application/json')) {
          return await response.json();
        return await response.text();
      // Handle errors
      let errorMessage = `Request failed with status ${response.status}`;
      let errorCode = RegistryErrorCode.UNKNOWN;
      let errorDetails: any;
        const errorBody = await response.json();
        if (errorBody.message) errorMessage = errorBody.message;
        if (errorBody.code) errorCode = this.mapErrorCode(errorBody.code);
        errorDetails = errorBody;
        // If response isn't JSON, use status text
        errorMessage = response.statusText || errorMessage;
      // Map HTTP status to error codes
      if (!errorDetails || !errorDetails.code) {
        switch (response.status) {
            errorCode = RegistryErrorCode.UNAUTHORIZED;
            errorCode = RegistryErrorCode.FORBIDDEN;
            errorCode = RegistryErrorCode.COMPONENT_NOT_FOUND;
            errorCode = RegistryErrorCode.RATE_LIMITED;
            errorCode = RegistryErrorCode.SERVER_ERROR;
      throw new RegistryError(errorMessage, errorCode, response.status, errorDetails);
      if (error.name === 'AbortError') {
          `Request timeout after ${this.config.timeout}ms`,
          RegistryErrorCode.TIMEOUT
      // Handle network errors
      if (error instanceof TypeError && error.message.includes('fetch')) {
          'Network error: Unable to connect to registry',
          RegistryErrorCode.NETWORK_ERROR,
      // If it's already a RegistryError, check if we should retry
        if (this.shouldRetry(error, retryCount)) {
          const delay = this.getRetryDelay(retryCount);
          await this.sleep(delay);
          return this.makeRequest(method, path, body, retryCount + 1);
        'Unexpected error during request',
   * Determine if a request should be retried
  private shouldRetry(error: RegistryError, retryCount: number): boolean {
    if (retryCount >= this.config.retryPolicy!.maxRetries) {
    // Retry on network errors, timeouts, and server errors
    const retryableCodes = [
      RegistryErrorCode.TIMEOUT,
      RegistryErrorCode.SERVER_ERROR
    return retryableCodes.includes(error.code);
   * Calculate retry delay with exponential backoff
  private getRetryDelay(retryCount: number): number {
    const policy = this.config.retryPolicy!;
    const delay = policy.initialDelay! * Math.pow(policy.backoffMultiplier!, retryCount);
    return Math.min(delay, policy.maxDelay!);
   * Sleep for specified milliseconds
   * Map error codes from server to client enum
  private mapErrorCode(code: string): RegistryErrorCode {
    const codeMap: Record<string, RegistryErrorCode> = {
      'COMPONENT_NOT_FOUND': RegistryErrorCode.COMPONENT_NOT_FOUND,
      'UNAUTHORIZED': RegistryErrorCode.UNAUTHORIZED,
      'FORBIDDEN': RegistryErrorCode.FORBIDDEN,
      'RATE_LIMITED': RegistryErrorCode.RATE_LIMITED,
      'INVALID_VERSION': RegistryErrorCode.INVALID_VERSION,
      'INVALID_NAMESPACE': RegistryErrorCode.INVALID_NAMESPACE,
      'NETWORK_ERROR': RegistryErrorCode.NETWORK_ERROR,
      'TIMEOUT': RegistryErrorCode.TIMEOUT,
      'SERVER_ERROR': RegistryErrorCode.SERVER_ERROR,
      'INVALID_RESPONSE': RegistryErrorCode.INVALID_RESPONSE
    return codeMap[code] || RegistryErrorCode.UNKNOWN;
