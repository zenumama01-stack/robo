 * Unit tests for the SendGrid provider.
 * Tests: email construction, parameter mapping, error handling, unsupported operations.
const { mockSgSend, mockSetApiKey } = vi.hoisted(() => ({
  mockSgSend: vi.fn(),
  mockSetApiKey: vi.fn(),
vi.mock('@sendgrid/mail', () => ({
    setApiKey: mockSetApiKey,
    send: mockSgSend,
      if (!creds[key]) throw new Error(`${provider}: Missing required credential: ${key}`);
// The config.ts module reads from process.env, let's mock it
  __API_KEY: 'env-sendgrid-key',
import { SendGridProvider } from '../SendGridProvider';
import type { ProcessedMessage } from '@memberjunction/communication-types';
describe('SendGridProvider', () => {
  let provider: SendGridProvider;
    provider = new SendGridProvider();
    it('should only support SendSingleMessage', () => {
      expect(ops).toEqual(['SendSingleMessage']);
    const createMessage = (overrides: Partial<ProcessedMessage> = {}): ProcessedMessage => ({
      FromName: 'Test Sender',
      CCRecipients: ['cc@example.com'],
      BCCRecipients: ['bcc@example.com'],
      ProcessedSubject: 'Test Email',
      ProcessedBody: 'Plain text body',
      ProcessedHTMLBody: '<p>HTML body</p>',
      SendAt: undefined,
      Headers: {},
      ...overrides,
    } as unknown as ProcessedMessage);
    it('should send email successfully', async () => {
      mockSgSend.mockResolvedValue([{ statusCode: 202, body: 'Accepted' }]);
      const result = await provider.SendSingleMessage(createMessage());
      expect(mockSetApiKey).toHaveBeenCalledWith('env-sendgrid-key');
      expect(mockSgSend).toHaveBeenCalledWith(expect.objectContaining({
        to: 'recipient@example.com',
        from: { email: 'sender@example.com', name: 'Test Sender' },
        cc: ['cc@example.com'],
        bcc: ['bcc@example.com'],
        subject: 'Test Email',
        text: 'Plain text body',
        html: '<p>HTML body</p>',
    it('should disable subscription tracking', async () => {
      mockSgSend.mockResolvedValue([{ statusCode: 202, body: '' }]);
      await provider.SendSingleMessage(createMessage());
          subscriptionTracking: { enable: false },
    it('should convert SendAt to unix timestamp', async () => {
      const sendAt = new Date('2025-06-15T12:00:00Z');
      await provider.SendSingleMessage(createMessage({ SendAt: sendAt } as unknown as Partial<ProcessedMessage>));
        sendAt: Math.floor(sendAt.getTime() / 1000),
    it('should handle API errors gracefully', async () => {
      mockSgSend.mockRejectedValue(new Error('Bad Request'));
      expect(result.Error).toContain('Bad Request');
    it('should handle non-success status codes', async () => {
      mockSgSend.mockResolvedValue([{ statusCode: 400, body: 'Invalid', toString: () => 'Error 400' }]);
    it('should use per-request credentials when provided', async () => {
      await provider.SendSingleMessage(createMessage(), {
        apiKey: 'SG.custom-key',
      expect(mockSetApiKey).toHaveBeenCalledWith('SG.custom-key');
  describe('unsupported operations', () => {
    it('GetMessages should throw', async () => {
        provider.GetMessages({} as Parameters<typeof provider.GetMessages>[0])
      ).rejects.toThrow('does not support fetching messages');
    it('ForwardMessage should throw', () => {
      expect(
        () => provider.ForwardMessage({} as Parameters<typeof provider.ForwardMessage>[0])
      ).toThrow('does not support forwarding');
    it('ReplyToMessage should throw', () => {
        () => provider.ReplyToMessage({} as Parameters<typeof provider.ReplyToMessage>[0])
      ).toThrow('does not support replying');
    it('CreateDraft should return failure', async () => {
      const result = await provider.CreateDraft({} as Parameters<typeof provider.CreateDraft>[0]);
      expect(result.ErrorMessage).toContain('does not support');
