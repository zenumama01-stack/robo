 * @fileoverview Registry provider interfaces and implementations for component loading
 * Metadata about a component from a registry
export interface RegistryComponentMetadata {
  properties?: ComponentSpec['properties'];
  events?: ComponentSpec['events'];
  libraries?: ComponentSpec['libraries'];
  dependencies?: ComponentSpec['dependencies'];
  sourceRegistryID?: string | null;
  isLocal: boolean;
  lastFetched?: Date;
  cacheDuration?: number;
 * Response from fetching a component from a registry
export interface RegistryComponentResponse {
  metadata: RegistryComponentMetadata;
 * Interface for registry providers that can fetch components
export interface RegistryProvider {
   * Name of the registry provider
   * Fetch a component from the registry
   * @param version - Component version (optional, defaults to latest)
   * @returns Component metadata and specification
  fetchComponent(
    version?: string
  ): Promise<RegistryComponentResponse>;
   * Check if a component exists in the registry
   * @param version - Component version (optional)
   * @returns True if component exists
  componentExists?(
   * Get available versions of a component
   * @returns Array of available versions
  getComponentVersions?(
    namespace: string
  ): Promise<string[]>;
 * Search filters for registry queries
export interface RegistrySearchFilters {
 * Dependency information for a component
export interface ComponentDependencyInfo {
  location: 'embedded' | 'registry';
 * Tree structure for dependency resolution
