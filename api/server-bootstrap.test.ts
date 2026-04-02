vi.mock('@memberjunction/server', () => ({
    serve: vi.fn().mockResolvedValue(undefined),
    MJServerOptions: class {},
                codeGeneration: {
                    packages: {
                        entities: { name: '@test/generated-entities' },
                        actions: { name: '@test/generated-actions' },
            filepath: '/test/mj.config.cjs',
import { createMJServer, MJServerConfig } from '../index';
import { serve } from '@memberjunction/server';
describe('ServerBootstrap', () => {
    describe('createMJServer', () => {
        it('should call cosmiconfigSync with mj module name', async () => {
            await createMJServer();
            expect(cosmiconfigSync).toHaveBeenCalledWith('mj', expect.objectContaining({
                searchStrategy: 'global',
        it('should call serve with default resolver paths when none provided', async () => {
            expect(serve).toHaveBeenCalledWith(
                    expect.stringContaining('generated.{js,ts}'),
                expect.objectContaining({}),
        it('should call serve with custom resolver paths when provided', async () => {
            const customPaths = ['./custom/**/*Resolver.ts'];
            await createMJServer({ resolverPaths: customPaths });
                customPaths,
        it('should call beforeStart hook if provided', async () => {
            const beforeStart = vi.fn();
            await createMJServer({ beforeStart });
            expect(beforeStart).toHaveBeenCalled();
        it('should call afterStart hook if provided', async () => {
            const afterStart = vi.fn();
            await createMJServer({ afterStart });
            expect(afterStart).toHaveBeenCalled();
        it('should pass restApiOptions to serve', async () => {
            const restApiOptions = { enabled: true };
            await createMJServer({ restApiOptions } as MJServerConfig);
                expect.objectContaining({ restApiOptions }),
        it('should use custom configPath when provided', async () => {
                filepath: '/custom/path/mj.config.cjs',
            (cosmiconfigSync as ReturnType<typeof vi.fn>).mockReturnValue({ search: mockSearch });
            await createMJServer({ configPath: '/custom/path' });
    describe('MJServerConfig interface', () => {
        it('should accept empty config object', async () => {
            const config: MJServerConfig = {};
            expect(config.configPath).toBeUndefined();
            expect(config.resolverPaths).toBeUndefined();
            expect(config.beforeStart).toBeUndefined();
            expect(config.afterStart).toBeUndefined();
        it('should accept all optional properties', () => {
            const config: MJServerConfig = {
                configPath: '/test/config',
                resolverPaths: ['./resolvers/**/*.ts'],
                beforeStart: async () => {},
                afterStart: async () => {},
            expect(config.configPath).toBe('/test/config');
            expect(config.resolverPaths).toHaveLength(1);
