  DEFAULT_NOTIFICATION_PREFERENCES
} from '../lib/models/notification.model';
  NotificationType,
  NotificationPriority,
  ConversationNotification,
  BadgeConfig,
  NotificationPreferences
describe('DEFAULT_NOTIFICATION_PREFERENCES', () => {
  it('should have sound disabled by default', () => {
    expect(DEFAULT_NOTIFICATION_PREFERENCES.enableSound).toBe(false);
  it('should have desktop notifications disabled by default', () => {
    expect(DEFAULT_NOTIFICATION_PREFERENCES.enableDesktopNotifications).toBe(false);
  it('should have in-app notifications enabled by default', () => {
    expect(DEFAULT_NOTIFICATION_PREFERENCES.enableInAppNotifications).toBe(true);
  it('should have badges enabled by default', () => {
    expect(DEFAULT_NOTIFICATION_PREFERENCES.showBadges).toBe(true);
  it('should have empty muted conversations list', () => {
    expect(DEFAULT_NOTIFICATION_PREFERENCES.mutedConversations).toEqual([]);
describe('Notification types', () => {
  it('should support all notification types', () => {
    const types: NotificationType[] = ['message', 'artifact', 'agent_process', 'task'];
  it('should support all priority levels', () => {
    const priorities: NotificationPriority[] = ['low', 'normal', 'high', 'urgent'];
    expect(priorities).toHaveLength(4);
describe('ConversationNotification shape', () => {
  it('should be constructable with required fields', () => {
    const notification: ConversationNotification = {
      conversationId: 'conv-1',
      unreadMessageCount: 5,
      lastReadMessageTimestamp: null,
      lastMessageTimestamp: new Date(),
      hasNewArtifacts: true,
      hasActiveAgentProcesses: false,
      newArtifactCount: 2,
      activeAgentProcessCount: 0,
      lastNotificationTimestamp: new Date(),
      highestPriority: 'high'
    expect(notification.conversationId).toBe('conv-1');
    expect(notification.unreadMessageCount).toBe(5);
    expect(notification.hasNewArtifacts).toBe(true);
describe('BadgeConfig shape', () => {
  it('should represent hidden badge', () => {
    const badge: BadgeConfig = { show: false };
    expect(badge.show).toBe(false);
    expect(badge.count).toBeUndefined();
  it('should represent visible badge with count', () => {
    const badge: BadgeConfig = {
      count: 3,
      type: 'count',
      priority: 'normal',
      animate: false
    expect(badge.show).toBe(true);
    expect(badge.count).toBe(3);
    expect(badge.type).toBe('count');
