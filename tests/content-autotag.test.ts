vi.mock('@memberjunction/core-actions', () => ({
    VectorizeEntityAction: class VectorizeEntityAction {
        protected async InternalRunAction(): Promise<unknown> {
vi.mock('@memberjunction/content-autotagging', () => ({
    AutotagLocalFileSystem: class AutotagLocalFileSystem {
        async Autotag(): Promise<void> {}
    AutotagRSSFeed: class AutotagRSSFeed {
    AutotagWebsite: class AutotagWebsite {
import { AutotagAndVectorizeContentAction } from '../generic/content-autotag-and-vectorize.action';
describe('AutotagAndVectorizeContentAction', () => {
    let action: AutotagAndVectorizeContentAction;
        action = new AutotagAndVectorizeContentAction();
    it('should be an instance of the class', () => {
        expect(action).toBeInstanceOf(AutotagAndVectorizeContentAction);
    describe('InternalRunAction', () => {
        it('should throw when Autotag param is missing', async () => {
                Params: [{ Name: 'Vectorize', Value: 1, Type: 'Input' }],
                ContextUser: {}
                (action as unknown as Record<string, (p: unknown) => Promise<unknown>>).InternalRunAction(params)
            ).rejects.toThrow('Autotag and Vectorize params are required.');
        it('should throw when Vectorize param is missing', async () => {
                Params: [{ Name: 'Autotag', Value: 1, Type: 'Input' }],
        it('should return success when both params present but disabled', async () => {
                    { Name: 'Autotag', Value: 0, Type: 'Input' },
                    { Name: 'Vectorize', Value: 0, Type: 'Input' }
            const result = await (action as unknown as Record<string, (p: unknown) => Promise<{ Success: boolean; ResultCode: string }>>).InternalRunAction(params);
        it('should run autotagging when Autotag is 1', async () => {
                    { Name: 'Autotag', Value: 1, Type: 'Input' },
