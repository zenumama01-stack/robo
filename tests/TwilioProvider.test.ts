 * Unit tests for the Twilio provider.
 * Tests: SMS construction, channel detection, credential resolution, error handling.
const { mockMessagesCreate, mockMessagesList, mockMessageFetch } = vi.hoisted(() => ({
  mockMessagesCreate: vi.fn(),
  mockMessageFetch: vi.fn(),
// Mock config.ts directly to avoid dotenv import issues
  TWILIO_ACCOUNT_SID: 'test-account-sid',
  TWILIO_AUTH_TOKEN: 'test-auth-token',
  TWILIO_PHONE_NUMBER: '+15551234567',
  TWILIO_WHATSAPP_NUMBER: '+15559876543',  // Provider adds whatsapp: prefix
  TWILIO_FACEBOOK_PAGE_ID: 'fb-page-123',  // Provider adds messenger: prefix
vi.mock('twilio', () => ({
  default: vi.fn().mockReturnValue({
      create: mockMessagesCreate,
  Twilio: class {},
// env-var not needed since we're mocking config.ts directly
import { TwilioProvider } from '../TwilioProvider';
describe('TwilioProvider', () => {
  let provider: TwilioProvider;
    provider = new TwilioProvider();
    it('should support messaging operations but not CreateDraft', () => {
      expect(ops).not.toContain('CreateDraft');
      To: '+15559998888',
      ProcessedBody: 'Hello via SMS',
      ProcessedSubject: '',
    it('should send SMS successfully', async () => {
      mockMessagesCreate.mockResolvedValue({ sid: 'SM123' });
      expect(mockMessagesCreate).toHaveBeenCalledWith(expect.objectContaining({
        body: 'Hello via SMS',
        from: '+15551234567',
        to: '+15559998888',
    it('should return failure when recipient not specified', async () => {
      const result = await provider.SendSingleMessage(
        createMessage({ To: '' } as unknown as Partial<ProcessedMessage>)
      expect(result.Error).toContain('Recipient not specified');
    it('should detect WhatsApp channel from recipient prefix', async () => {
      mockMessagesCreate.mockResolvedValue({ sid: 'WA123' });
      await provider.SendSingleMessage(
        createMessage({ To: 'whatsapp:+15559998888' } as unknown as Partial<ProcessedMessage>)
        from: 'whatsapp:+15559876543',
        to: 'whatsapp:+15559998888',
    it('should detect Messenger channel from recipient prefix', async () => {
      mockMessagesCreate.mockResolvedValue({ sid: 'MSG123' });
        createMessage({ To: 'messenger:user123' } as unknown as Partial<ProcessedMessage>)
        from: 'messenger:fb-page-123',
        to: 'messenger:user123',
    it('should include media URLs when provided', async () => {
      mockMessagesCreate.mockResolvedValue({ sid: 'SM456' });
        createMessage({
          ContextData: { mediaUrls: ['https://example.com/image.jpg'] },
        } as unknown as Partial<ProcessedMessage>)
        mediaUrl: ['https://example.com/image.jpg'],
    it('should handle Twilio API errors gracefully', async () => {
      mockMessagesCreate.mockRejectedValue(new Error('Authentication Error'));
      expect(result.Error).toContain('Authentication Error');
    it('should send successfully with per-request credentials', async () => {
      mockMessagesCreate.mockResolvedValue({ sid: 'SM789' });
      const result = await provider.SendSingleMessage(createMessage(), {
        accountSid: 'custom-sid',
        authToken: 'custom-token',
        phoneNumber: '+15550001111',
      // The mock twilio constructor is called with custom creds internally
      expect(mockMessagesCreate).toHaveBeenCalled();
    it('should return unsupported error', async () => {
  describe('GetMessages', () => {
    it('should fetch messages', async () => {
      mockMessagesList.mockResolvedValue([
        { from: '+15551111111', to: '+15552222222', body: 'Hello', sid: 'SM001' },
      const result = await provider.GetMessages({
        NumMessages: 10,
        Identifier: '+15552222222',
      } as Parameters<typeof provider.GetMessages>[0]);
      expect(result.Messages).toHaveLength(1);
      expect(result.Messages[0].Body).toBe('Hello');
      expect(result.Messages[0].ExternalSystemRecordID).toBe('SM001');
    it('should handle fetch errors', async () => {
      mockMessagesList.mockRejectedValue(new Error('Rate limited'));
      expect(result.ErrorMessage).toContain('Rate limited');
