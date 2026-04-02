 * Request to open a new tab
export interface TabRequest {
  /** ID of the application this tab belongs to */
  /** Display title for the tab */
  Title: string;
  /** Route to load in the tab (optional if ResourceType is provided) */
  /** Resource type for resource-based tabs */
  /** Resource type ID for matching existing tabs (legacy) */
  ResourceTypeId?: string;
  /** Resource record ID for matching existing tabs */
  ResourceRecordId?: string;
  /** Whether this tab should be pinned (permanent) */
  IsPinned?: boolean;
  /** Tab-specific configuration */
  Configuration?: Record<string, unknown>;
