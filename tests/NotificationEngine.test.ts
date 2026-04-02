 * Unit tests for the Communication/notifications package.
 * Tests: delivery channel resolution, type lookup, error handling.
const { mockUserInfoEngineInstance } = vi.hoisted(() => ({
  mockUserInfoEngineInstance: {
    NotificationTypes: [] as Array<{ ID: string; Name: string; AllowUserPreference: boolean; DefaultInApp: boolean; DefaultEmail: boolean; DefaultSMS: boolean; EmailTemplateID: string | null; SMSTemplateID: string | null }>,
    GetUserPreferenceForType: vi.fn().mockReturnValue(null),
  const mockLoad = vi.fn().mockResolvedValue(undefined);
    Load = mockLoad;
    TryThrowIfNotLoaded() {
      if (!this._loaded) {
        // For testing, just mark as loaded
      GetEntityObject: <T>() => Promise<T>;
        this.GetEntityObject = vi.fn().mockResolvedValue({
          NotificationTypeID: '',
          Title: '',
          Message: '',
          Unread: true,
          ResourceTypeID: null,
          ResourceRecordID: null,
          ResourceConfiguration: null,
          ID: 'notif-1',
      static Provider = {};
  MJUserNotificationTypeEntity: class {},
  MJUserNotificationPreferenceEntity: class {},
    Instance: mockUserInfoEngineInstance,
      Templates: [],
      SendSingleMessage: vi.fn().mockResolvedValue({ Success: true }),
    ContextData: Record<string, unknown> = {};
  UserCache: {
      Users: [
        { ID: 'user-1', Email: 'user@example.com', Name: 'Test User' },
import { NotificationEngine } from '../NotificationEngine';
import type { SendNotificationParams } from '../types';
describe('NotificationEngine', () => {
  let engine: NotificationEngine;
  const contextUser = { ID: 'user-1' } as InstanceType<typeof import('@memberjunction/core').UserInfo>;
    engine = new NotificationEngine();
    // Override internal _loaded state
    (engine as unknown as Record<string, boolean>)['_loaded'] = true;
    // Set up default notification types
    mockUserInfoEngineInstance.NotificationTypes = [
        ID: 'type-1',
        Name: 'Agent Completion',
        AllowUserPreference: true,
        DefaultInApp: true,
        DefaultEmail: false,
        DefaultSMS: false,
        EmailTemplateID: null,
        SMSTemplateID: null,
        ID: 'type-2',
        Name: 'System Alert',
        AllowUserPreference: false,
        DefaultEmail: true,
        EmailTemplateID: 'tmpl-email-1',
  describe('SendNotification - type lookup', () => {
    it('should fail when notification type not found', async () => {
      const params: SendNotificationParams = {
        typeNameOrId: 'NonExistentType',
        title: 'Test',
        message: 'Test message',
      const result = await engine.SendNotification(params, contextUser);
      expect(result.errors).toContain('Notification type not found: NonExistentType');
    it('should find notification type by name (case-insensitive)', async () => {
        typeNameOrId: 'agent completion',
        title: 'Done!',
        message: 'Your agent is done',
      expect(result.deliveryChannels.inApp).toBe(true);
    it('should find notification type by ID', async () => {
        typeNameOrId: 'type-1',
  describe('SendNotification - delivery channel resolution', () => {
    it('should use type defaults when no user preference exists', async () => {
        typeNameOrId: 'Agent Completion',
        message: 'Test',
      expect(result.deliveryChannels.email).toBe(false);
      expect(result.deliveryChannels.sms).toBe(false);
    it('should use forceDeliveryChannels when specified', async () => {
        forceDeliveryChannels: { inApp: false, email: true, sms: true },
      expect(result.deliveryChannels.inApp).toBe(false);
      expect(result.deliveryChannels.email).toBe(true);
      expect(result.deliveryChannels.sms).toBe(true);
    it('should disable all channels when user has opted out', async () => {
      mockUserInfoEngineInstance.GetUserPreferenceForType.mockReturnValue({
        Enabled: false,
        InAppEnabled: null,
        EmailEnabled: null,
        SMSEnabled: null,
    it('should respect user preferences when allowed', async () => {
        Enabled: true,
        InAppEnabled: false,
        EmailEnabled: true,
    it('should use type defaults when AllowUserPreference is false', async () => {
        typeNameOrId: 'System Alert',
        title: 'Alert!',
        message: 'System message',
      // System Alert: DefaultInApp=true, DefaultEmail=true, DefaultSMS=false
