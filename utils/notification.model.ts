 * Notification types supported by the system
export type NotificationType = 'message' | 'artifact' | 'agent_process' | 'task';
 * Notification priority levels
export type NotificationPriority = 'low' | 'normal' | 'high' | 'urgent';
 * Represents a notification for a conversation
export interface ConversationNotification {
  unreadMessageCount: number;
  lastReadMessageTimestamp: Date | null;
  lastMessageTimestamp: Date | null;
  hasNewArtifacts: boolean;
  hasActiveAgentProcesses: boolean;
  newArtifactCount: number;
  activeAgentProcessCount: number;
  lastNotificationTimestamp: Date;
  highestPriority: NotificationPriority;
 * Represents a single notification item
export interface NotificationItem {
  type: NotificationType;
  priority: NotificationPriority;
  isRead: boolean;
 * User notification preferences
export interface NotificationPreferences {
  enableSound: boolean;
  enableDesktopNotifications: boolean;
  enableInAppNotifications: boolean;
  showBadges: boolean;
  muteUntil?: Date;
  mutedConversations: string[];
 * Notification state for persistence
export interface NotificationState {
  conversations: Record<string, ConversationNotification>;
  preferences: NotificationPreferences;
  lastUpdated: Date;
 * Badge display configuration
export interface BadgeConfig {
  show: boolean;
  type?: 'count' | 'dot' | 'pulse' | 'new';
  priority?: NotificationPriority;
  animate?: boolean;
 * Activity indicator configuration
export interface ActivityIndicatorConfig {
  type: 'agent' | 'processing' | 'typing';
 * Event emitted when notification state changes
export interface NotificationChangeEvent {
  action: 'added' | 'read' | 'cleared';
 * Default notification preferences
export const DEFAULT_NOTIFICATION_PREFERENCES: NotificationPreferences = {
  enableSound: false,
  enableDesktopNotifications: false,
  enableInAppNotifications: true,
  showBadges: true,
  mutedConversations: []
