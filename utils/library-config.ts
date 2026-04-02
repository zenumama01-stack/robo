 * Configuration for external libraries used in React components
export interface ExternalLibraryConfig {
  /** Unique identifier for the library */
  /** Library name (e.g., 'lodash') */
  /** Display name for UI (e.g., 'Lodash') */
  /** Library category */
  category: 'core' | 'runtime' | 'ui' | 'charting' | 'utility';
  /** Global variable name when loaded (e.g., '_' for lodash) */
  /** Library version */
  /** CDN URL for the library JavaScript */
  cdnUrl: string;
  /** Optional CDN URL for library CSS */
  cdnCssUrl?: string;
  /** Library description */
  /** Instructions for AI when using this library */
  aiInstructions?: string;
  /** Example usage code */
  exampleUsage?: string;
  /** Whether the library is enabled */
  isEnabled: boolean;
  /** Whether this is a core library (always loaded) */
  isCore: boolean;
  /** Whether this is runtime-only (not exposed to generated components) */
  isRuntimeOnly?: boolean;
export interface LibraryConfigurationMetadata {
  lastUpdated: string;
export interface LibraryConfiguration {
  libraries: ExternalLibraryConfig[];
  metadata: LibraryConfigurationMetadata;
 * Library loading options
export interface LibraryLoadOptions {
  /** Skip loading if already loaded */
  skipIfLoaded?: boolean;
  /** Timeout for loading (ms) */
  /** Filter to specific categories */
  categories?: Array<ExternalLibraryConfig['category']>;
  /** Exclude runtime-only libraries */
  excludeRuntimeOnly?: boolean;
