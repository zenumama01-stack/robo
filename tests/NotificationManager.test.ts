// Mock external dependencies before importing the module
    NotificationChannel: {}
import { NotificationManager } from '../NotificationManager';
const mockedLogStatus = vi.mocked(LogStatus);
describe('NotificationManager', () => {
    describe('SendScheduledJobNotification', () => {
        it('should call LogStatus with user ID', async () => {
                'user-123',
                { Subject: 'Test Subject', Body: 'Test body content here', Priority: 'Normal', Metadata: {} },
                ['Email']
            expect(mockedLogStatus).toHaveBeenCalledWith(
                expect.stringContaining('user-123')
        it('should log the subject', async () => {
                'user-456',
                { Subject: 'Important Notification', Body: 'Body text for the notification here', Priority: 'High', Metadata: {} },
                ['InApp']
                expect.stringContaining('Important Notification')
        it('should log the channels', async () => {
                'user-789',
                { Subject: 'Test', Body: 'Test body content is here', Priority: 'Normal', Metadata: {} },
                ['Email', 'InApp']
                expect.stringContaining('Email, InApp')
        it('should handle Email channel', async () => {
                'user-abc',
                { Subject: 'Email Test', Body: 'This is an email body with sufficient content to show', Priority: 'Normal', Metadata: {} },
            // Should log Email-specific message
                expect.stringContaining('[Email]')
        it('should handle InApp channel', async () => {
                'user-def',
                { Subject: 'InApp Test', Body: 'In-app body content of this notification', Priority: 'Normal', Metadata: {} },
            // Should log InApp-specific message
                expect.stringContaining('[InApp]')
        it('should handle both Email and InApp channels', async () => {
                'user-ghi',
                { Subject: 'Both Channels', Body: 'Body for both channels is contained here', Priority: 'High', Metadata: {} },
            const emailCall = mockedLogStatus.mock.calls.find(
                (c: string[]) => c[0].includes('[Email]')
            const inAppCall = mockedLogStatus.mock.calls.find(
                (c: string[]) => c[0].includes('[InApp]')
            expect(emailCall).toBeDefined();
            expect(inAppCall).toBeDefined();
        it('should log priority', async () => {
                'user-jkl',
                { Subject: 'Priority Test', Body: 'Priority test body content here', Priority: 'High', Metadata: {} },
                expect.stringContaining('High')
        it('should handle empty channels array', async () => {
            // Should not throw when no channels provided
                NotificationManager.SendScheduledJobNotification(
                    'user-mno',
                    { Subject: 'No Channels', Body: 'No channels body content here', Priority: 'Normal', Metadata: {} },
            ).resolves.toBeUndefined();
        it('should not throw for any input combination', async () => {
                    { Subject: '', Body: '', Priority: 'Normal', Metadata: {} },
