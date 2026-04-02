 * Unit tests for the Gmail provider.
 * Tests: credential resolution, email content creation, supported operations.
    GMAIL_CLIENT_ID: 'env-client-id',
    GMAIL_CLIENT_SECRET: 'env-client-secret',
    GMAIL_REDIRECT_URI: 'http://localhost:3000/callback',
    GMAIL_REFRESH_TOKEN: 'env-refresh-token',
    GMAIL_SERVICE_ACCOUNT_EMAIL: 'service@example.com',
const { mockSend, mockMessagesList, mockMessagesGet } = vi.hoisted(() => ({
  mockSend: vi.fn(),
  mockMessagesList: vi.fn(),
  mockMessagesGet: vi.fn(),
vi.mock('googleapis', () => {
  class FakeOAuth2 {
    setCredentials = vi.fn();
          OAuth2: FakeOAuth2,
        gmail: vi.fn().mockReturnValue({
          users: {
            getProfile: vi.fn().mockResolvedValue({ data: { emailAddress: 'test@gmail.com' } }),
              send: mockSend,
              list: mockMessagesList,
              get: mockMessagesGet,
              modify: vi.fn().mockResolvedValue({}),
              trash: vi.fn().mockResolvedValue({}),
              delete: vi.fn().mockResolvedValue({}),
              list: vi.fn().mockResolvedValue({ data: { labels: [] } }),
            drafts: {
              create: vi.fn().mockResolvedValue({ status: 200, data: { id: 'draft-1' } }),
import { GmailProvider } from '../GmailProvider';
import type { ProcessedMessage, MessageResult } from '@memberjunction/communication-types';
describe('GmailProvider', () => {
  let provider: GmailProvider;
    provider = new GmailProvider();
    it('should include all Gmail-supported operations', () => {
      expect(ops).toContain('DeleteMessage');
      expect(ops).toContain('ListFolders');
      expect(ops).toContain('MarkAsRead');
      expect(ops).toContain('ArchiveMessage');
      expect(ops).toContain('SearchMessages');
      expect(ops).toContain('ListAttachments');
      expect(ops).toContain('DownloadAttachment');
    it('should send an email successfully', async () => {
      mockSend.mockResolvedValue({ status: 200, statusText: 'OK' });
        FromName: 'Sender',
        ProcessedSubject: 'Test Subject',
        ProcessedBody: 'Hello World',
      } as unknown as ProcessedMessage;
      const result = await provider.SendSingleMessage(message);
      expect(mockSend).toHaveBeenCalled();
    it('should handle non-success status from Gmail API', async () => {
      mockSend.mockResolvedValue({ status: 500, statusText: 'Internal Server Error' });
    it('should handle send errors gracefully', async () => {
      mockSend.mockRejectedValue(new Error('API quota exceeded'));
      expect(result.Error).toContain('API quota exceeded');
    it('should return failure when required credentials are missing', async () => {
      // SendSingleMessage catches the error internally
      const result = await provider.SendSingleMessage(message, {
        disableEnvironmentFallback: true,
      expect(result.Error).toContain('Missing required credential');
  describe('ReplyToMessage', () => {
    it('should return error when MessageID is missing', async () => {
      const result = await provider.ReplyToMessage({
        MessageID: '',
        Message: {} as ProcessedMessage,
      expect(result.ErrorMessage).toContain('Message ID not provided');
  describe('ForwardMessage', () => {
      const result = await provider.ForwardMessage({
        ToRecipients: ['r@example.com'],
