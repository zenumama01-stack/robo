 * Permission levels for list sharing
 * Maps to the PermissionLevel values in ResourcePermission table
export type ListPermissionLevel = 'View' | 'Edit' | 'Owner';
 * Information about a list share
export interface ListShareInfo {
   * The ResourcePermission record ID
  shareId: string;
   * The List ID being shared
   * Type of recipient
  type: 'User' | 'Role';
   * User or Role ID
  recipientId: string;
   * Name of the user or role
  recipientName: string;
   * Email (only for users)
  recipientEmail?: string;
   * Permission level granted
  permissionLevel: ListPermissionLevel;
   * Status of the share (Approved, Pending, Rejected)
  status: 'Approved' | 'Pending' | 'Rejected';
   * When sharing starts (optional)
  startSharingAt?: Date;
   * When sharing ends (optional)
  endSharingAt?: Date;
 * Result of a share operation
export interface ListShareResult {
   * Whether the operation succeeded
   * The share ID (ResourcePermission ID) if successful
  shareId?: string;
   * Message describing the result
 * Recipient for sharing (used in autocomplete)
export interface ShareRecipient {
   * Display name
 * Summary of sharing status for a list
export interface ListSharingSummary {
   * List ID
   * Total number of shares
  totalShares: number;
   * Number of user shares
  userShares: number;
   * Number of role shares
  roleShares: number;
   * Is this list shared with the current user (not owner)
  isSharedWithMe: boolean;
   * Has the current user shared this list with others
  isSharedByMe: boolean;
 * Permission capabilities based on role
export interface ListPermissions {
   * Can view the list contents
   * Can add/remove items from the list
  canEditItems: boolean;
   * Can edit list properties (name, description, category)
  canEditProperties: boolean;
   * Can share the list with others
   * Can delete the list
   * The effective permission level
  effectiveLevel: ListPermissionLevel | 'None';
   * Whether the user is the owner
 * Configuration for the share dialog
export interface ListShareDialogConfig {
   * The list being shared
   * List name for display
   * Current user ID
  currentUserId: string;
   * Whether the current user is the owner
 * Result from the share dialog
export interface ListShareDialogResult {
   * Action taken
   * Shares that were added
  sharesAdded: ListShareInfo[];
   * Shares that were updated
  sharesUpdated: ListShareInfo[];
   * Shares that were removed
  sharesRemoved: string[];
