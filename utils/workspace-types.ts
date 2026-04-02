 * Workspace Initializer - Type Definitions
 * Extracted from MJExplorer AppComponent to make workspace initialization reusable
export interface WorkspaceEnvironment {
export interface WorkspaceInitResult {
  error?: WorkspaceInitError;
export interface WorkspaceInitError {
  type: 'no_roles' | 'no_access' | 'token_expired' | 'network' | 'unknown';
