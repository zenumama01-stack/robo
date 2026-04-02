import { GraphQLDataProvider } from "./graphQLDataProvider";
import { gql } from "graphql-request";
import { ComponentSpec } from "@memberjunction/interactive-component-types";
 * Parameters for getting a component from registry
export interface GetRegistryComponentParams {
     * Registry name (globally unique)
    registryName: string;
     * Component version (optional, defaults to 'latest')
     * Optional hash for caching - if provided and matches, returns null
 * Response from GetRegistryComponent with hash and caching metadata
export interface ComponentSpecWithHash {
     * The component specification (undefined if not modified)
    specification?: ComponentSpec | string; // Can be either parsed object or JSON string
    notModified: boolean;
     * Optional message from server
 * Parameters for searching registry components
export interface SearchRegistryComponentsParams {
     * Optional registry name filter
 * Search result for registry components
export interface RegistryComponentSearchResult {
 * Dependency tree for a component
export interface ComponentDependencyTree {
    dependencies?: ComponentDependencyTree[];
 * Input parameters for sending component feedback
     * Component version (optional)
     * Registry name (optional - for registry-specific feedback)
     * Rating (typically 0-5 scale)
     * User comments (optional)
 * Response from sending component feedback
     * ID of the created feedback record (if available)
     * Error message if submission failed
 * Client for executing Component Registry operations through GraphQL.
 * This class provides an easy way to fetch components from external registries
 * through the MJ API server, which handles authentication and caching.
 * The GraphQLComponentRegistryClient follows the same naming convention as other GraphQL clients
 * in the MemberJunction ecosystem, such as GraphQLAIClient and GraphQLActionClient.
 * // Create the client
 * const registryClient = new GraphQLComponentRegistryClient(graphQLProvider);
 * // Get a component from a registry
 * const component = await registryClient.GetRegistryComponent({
 *   registryName: "MJ",
 *   namespace: "core/ui",
 *   name: "DataGrid",
 *   version: "1.0.0"
 * // Search for components
 * const searchResult = await registryClient.SearchRegistryComponents({
 *   query: "dashboard",
 *   type: "dashboard",
 *   limit: 10
export class GraphQLComponentRegistryClient {
     * The GraphQLDataProvider instance used to execute GraphQL requests
    private _dataProvider: GraphQLDataProvider;
     * Creates a new GraphQLComponentRegistryClient instance.
     * @param dataProvider The GraphQL data provider to use for queries
    constructor(dataProvider: GraphQLDataProvider) {
        this._dataProvider = dataProvider;
     * Get a specific component from a registry.
     * This method fetches a component specification from an external registry
     * through the MJ API server. The server handles authentication with the
     * registry and may cache the result for performance.
     * @param params The parameters for getting the component
     * @returns A Promise that resolves to a ComponentSpec
     *   version: "2.0.0"
     * console.log('Component:', component.name);
     * console.log('Description:', component.description);
     * console.log('Code:', component.code);
    public async GetRegistryComponent(params: GetRegistryComponentParams): Promise<ComponentSpec | null> {
            // Build the query - specification is now a JSON string
            const query = gql`
                query GetRegistryComponent(
                    $registryName: String!,
                    $namespace: String!,
                    $name: String!,
                    $version: String,
                    $hash: String
                    GetRegistryComponent(
                        registryName: $registryName,
                        namespace: $namespace,
                        name: $name,
                        version: $version,
                        hash: $hash
                        hash
                        notModified
                        specification
            // Prepare variables
            const variables: Record<string, any> = {
                registryName: params.registryName,
                namespace: params.namespace,
                name: params.name
            if (params.version !== undefined) {
                variables.version = params.version;
            if (params.hash !== undefined) {
                variables.hash = params.hash;
            const result = await this._dataProvider.ExecuteGQL(query, variables);
            // Handle new response structure with hash
            if (result && result.GetRegistryComponent) {
                const response = result.GetRegistryComponent as ComponentSpecWithHash;
                // If not modified and no specification, return null (client should use cache)
                if (response.notModified && !response.specification) {
                // Parse the JSON string specification if available
                if (response.specification) {
                    // If it's already an object, return it
                    if (typeof response.specification === 'object') {
                        return response.specification as ComponentSpec;
                    // Otherwise parse the JSON string
                        return JSON.parse(response.specification) as ComponentSpec;
                        LogError(`Failed to parse component specification: ${e}`);
            throw new Error(`Failed to get registry component: ${e instanceof Error ? e.message : 'Unknown error'}`);
     * Get a component from registry with hash and caching metadata.
     * Returns the full response including hash and notModified flag.
     * @param params - Parameters for fetching the component
     * @returns Full response with specification, hash, and caching metadata
     * const response = await client.GetRegistryComponentWithHash({
     *   registryName: 'MJ',
     *   namespace: 'core/ui',
     *   name: 'DataGrid',
     *   version: '1.0.0',
     *   hash: 'abc123...'
     * if (response.notModified) {
     *   // Use cached version
     *   // Use response.specification
    public async GetRegistryComponentWithHash(params: GetRegistryComponentParams): Promise<ComponentSpecWithHash> {
            // Build the query - same as GetRegistryComponent
            // Return the full response with parsed specification
                const response = result.GetRegistryComponent;
                let spec: ComponentSpec | undefined;
                        spec = JSON.parse(response.specification) as ComponentSpec;
                        LogError(`Failed to parse component specification in GetRegistryComponentWithHash: ${e}`);
                        spec = undefined;
                    hash: response.hash,
                    notModified: response.notModified,
                    message: response.message
                } as ComponentSpecWithHash;
            // Return empty response if nothing found
                specification: undefined,
                hash: '',
                notModified: false,
                message: 'Component not found'
            throw new Error(`Failed to get registry component with hash: ${e instanceof Error ? e.message : 'Unknown error'}`);
     * Search for components in registries.
     * This method searches for components across one or more registries
     * based on the provided criteria. Results are paginated for performance.
     * @param params The search parameters
     * @returns A Promise that resolves to a RegistryComponentSearchResult
     *   tags: ["analytics", "reporting"],
     *   limit: 20,
     *   offset: 0
     * console.log(`Found ${searchResult.total} components`);
     * searchResult.components.forEach(component => {
     *   console.log(`- ${component.name}: ${component.description}`);
    public async SearchRegistryComponents(params: SearchRegistryComponentsParams): Promise<RegistryComponentSearchResult> {
                query SearchRegistryComponents($params: SearchRegistryComponentsInput!) {
                    SearchRegistryComponents(params: $params) {
                        components
                        total
            const result = await this._dataProvider.ExecuteGQL(query, { params });
            // Return the search result with parsed components
            if (result && result.SearchRegistryComponents) {
                const searchResult = result.SearchRegistryComponents;
                    components: searchResult.components.map((json: string) => JSON.parse(json) as ComponentSpec),
                    total: searchResult.total,
                    offset: searchResult.offset,
                    limit: searchResult.limit
                } as RegistryComponentSearchResult;
                components: [],
                limit: params.limit || 10
            throw new Error(`Failed to search registry components: ${e instanceof Error ? e.message : 'Unknown error'}`);
     * Resolve the dependency tree for a component.
     * This method fetches the complete dependency tree for a component,
     * including all transitive dependencies. The server handles circular
     * dependency detection and marks them appropriately.
     * @param registryId The registry ID
     * @param componentId The component ID
     * @returns A Promise that resolves to a ComponentDependencyTree
     * const dependencyTree = await registryClient.ResolveComponentDependencies(
     *   "mj-central",
     *   "component-123"
     * console.log(`Component has ${dependencyTree.totalCount} total dependencies`);
     * if (dependencyTree.circular) {
     *   console.warn('Circular dependency detected!');
    public async ResolveComponentDependencies(
        registryId: string,
        componentId: string
    ): Promise<ComponentDependencyTree | null> {
                query ResolveComponentDependencies(
                    $registryId: String!,
                    $componentId: String!
                    ResolveComponentDependencies(
                        registryId: $registryId,
                        componentId: $componentId
                        componentId
                        namespace
                        version
                        circular
                        dependencies {
            const result = await this._dataProvider.ExecuteGQL(query, {
                registryId,
            // Return the dependency tree
            if (result && result.ResolveComponentDependencies) {
                return result.ResolveComponentDependencies as ComponentDependencyTree;
            throw new Error(`Failed to resolve component dependencies: ${e instanceof Error ? e.message : 'Unknown error'}`);
     * Check if a specific version of a component exists in a registry.
     * @param params The parameters for checking component existence
     * @returns A Promise that resolves to true if the component exists, false otherwise
     * const exists = await registryClient.ComponentExists({
     *   registryId: "mj-central",
     * if (exists) {
     *   console.log('Component is available');
    public async ComponentExists(params: GetRegistryComponentParams): Promise<boolean> {
            const component = await this.GetRegistryComponent(params);
            return component !== null;
            // If we get an error, assume the component doesn't exist
     * Get the latest version of a component.
     * @param namespace The component namespace
     * @param name The component name
     * @returns A Promise that resolves to the latest version string or null
     * const latestVersion = await registryClient.GetLatestVersion(
     *   "core/ui",
     *   "DataGrid"
     * console.log(`Latest version: ${latestVersion}`);
    public async GetLatestVersion(
        registryName: string,
        namespace: string,
            const component = await this.GetRegistryComponent({
                registryName,
                version: 'latest'
            return component?.version || null;
     * Send feedback for a component.
     * This is a registry-agnostic method that allows submitting feedback
     * for any component from any registry. The feedback can include ratings,
     * comments, and associations with conversations, reports, or dashboards.
     * @param params The feedback parameters
     * @returns A Promise that resolves to a ComponentFeedbackResponse
     * const response = await registryClient.SendComponentFeedback({
     *   componentName: 'DataGrid',
     *   componentNamespace: 'core/ui',
     *   componentVersion: '1.0.0',
     *   rating: 5,
     *   feedbackType: 'feature-request',
     *   comments: 'Would love to see export to Excel functionality',
     *   conversationID: 'conv-123'
     * if (response.success) {
     *   console.log('Feedback submitted successfully!');
     *   if (response.feedbackID) {
     *     console.log(`Feedback ID: ${response.feedbackID}`);
     *   console.error('Feedback submission failed:', response.error);
    public async SendComponentFeedback(params: ComponentFeedbackParams): Promise<ComponentFeedbackResponse> {
            // Build the mutation
                mutation SendComponentFeedback($feedback: ComponentFeedbackInput!) {
                    SendComponentFeedback(feedback: $feedback) {
                        feedbackID
            const result = await this._dataProvider.ExecuteGQL(mutation, { feedback: params });
            // Return the response
            if (result && result.SendComponentFeedback) {
                return result.SendComponentFeedback as ComponentFeedbackResponse;
                error: 'No response from server'
                error: e instanceof Error ? e.message : 'Unknown error'
