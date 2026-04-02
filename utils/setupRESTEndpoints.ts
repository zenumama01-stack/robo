import { RESTEndpointHandler } from './RESTEndpointHandler.js';
export const ___REST_API_BASE_PATH = '/api/v1';
 * Configuration options for REST API endpoints
export interface RESTApiOptions {
     * Whether to enable REST API endpoints (default: false)
 * Default REST API configuration
export const DEFAULT_REST_API_OPTIONS: RESTApiOptions = {
 * Adds the REST API endpoints to an existing Express application
 * @param app The Express application to add the endpoints to
 * @param options Configuration options for REST API
 * @param authMiddleware Optional authentication middleware to use
export function setupRESTEndpoints(
    app: express.Application,
    options?: Partial<RESTApiOptions>,
    authMiddleware?: express.RequestHandler
    // Merge with default options
    const config = { ...DEFAULT_REST_API_OPTIONS, ...options };
    // Skip setup if REST API is disabled
    if (!config.enabled) {
        console.log('REST API endpoints are disabled');
    const basePath = ___REST_API_BASE_PATH;
    // Create REST endpoint handler with entity and schema filters
    const restHandler = new RESTEndpointHandler({
        includeEntities: config.includeEntities ? config.includeEntities.map(e => e.toLowerCase()) : undefined,
        excludeEntities: config.excludeEntities ? config.excludeEntities.map(e => e.toLowerCase()) : undefined,
        includeSchemas: config.includeSchemas ? config.includeSchemas.map(s => s.toLowerCase()) : undefined,
        excludeSchemas: config.excludeSchemas ? config.excludeSchemas.map(s => s.toLowerCase()) : undefined
    // Mount REST API at the specified base path with authentication
    // This must come AFTER OAuth routes so they take precedence
    if (authMiddleware) {
        app.use(basePath, authMiddleware, restHandler.getRouter());
        app.use(basePath, restHandler.getRouter());
    console.log(`REST API endpoints have been set up at ${basePath}`);
