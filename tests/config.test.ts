 * Unit tests for AICLI config
import { describe, it, expect, vi } from 'vitest';
// Mock cosmiconfig
vi.mock('cosmiconfig', () => ({
    cosmiconfig: () => ({
        search: vi.fn().mockResolvedValue(null),
// Mock dotenv
vi.mock('dotenv', () => ({
    default: { config: vi.fn() },
import type { AICliConfig } from '../config';
describe('AICliConfig', () => {
    it('should accept a full config object', () => {
        const config: AICliConfig = {
            dbHost: 'localhost',
            dbDatabase: 'testdb',
            dbPort: 1433,
            dbUsername: 'sa',
            dbPassword: 'password',
            coreSchema: '__mj',
            aiSettings: {
                defaultTimeout: 30000,
                outputFormat: 'json',
                logLevel: 'debug',
                enableChat: true,
                chatHistoryLimit: 50,
        expect(config.dbHost).toBe('localhost');
        expect(config.aiSettings?.outputFormat).toBe('json');
    it('should accept a minimal config object', () => {
        const config: AICliConfig = {};
        expect(config.dbHost).toBeUndefined();
        expect(config.aiSettings).toBeUndefined();
    it('should accept partial aiSettings', () => {
                outputFormat: 'compact',
        expect(config.aiSettings?.outputFormat).toBe('compact');
        expect(config.aiSettings?.defaultTimeout).toBeUndefined();
describe('loadAIConfig', () => {
    it('should throw when no config found', async () => {
        const { loadAIConfig } = await import('../config');
        await expect(loadAIConfig()).rejects.toThrow('No mj.config.cjs configuration found');
// Mock cosmiconfig and other deps before importing config
    cosmiconfigSync: vi.fn().mockReturnValue({
        search: vi.fn().mockReturnValue(null)
    SeverityType: { Info: 'Info', Warning: 'Warning', Critical: 'Critical' }
                CreateInstance: vi.fn().mockReturnValue(null)
vi.mock('@memberjunction/config', () => ({
    mergeConfigs: vi.fn((...configs: unknown[]) => Object.assign({}, ...configs)),
    parseBooleanEnv: vi.fn((value: string | undefined, defaultValue: boolean) => {
import { parseBooleanEnv } from '@memberjunction/config';
describe('Config Types', () => {
    describe('parseBooleanEnv (utility function)', () => {
        it('should return default for undefined input', () => {
            expect(parseBooleanEnv(undefined, true)).toBe(true);
            expect(parseBooleanEnv(undefined, false)).toBe(false);
        it('should parse "true" string', () => {
            expect(parseBooleanEnv('true', false)).toBe(true);
        it('should parse "false" string', () => {
            expect(parseBooleanEnv('false', true)).toBe(false);
describe('Config Schema Shapes', () => {
    it('should define SettingInfo with name and value', () => {
        const setting = { name: 'testSetting', value: 'testValue' };
        expect(setting.name).toBe('testSetting');
        expect(setting.value).toBe('testValue');
    it('should define LogInfo with log, logFile, and console', () => {
        const logInfo = { log: true, logFile: 'output.log', console: true };
        expect(logInfo.log).toBe(true);
        expect(logInfo.logFile).toBe('output.log');
    it('should define CommandInfo with required fields', () => {
        const command = {
            workingDirectory: '/tmp',
            command: 'npm',
            args: ['run', 'build'],
            when: 'after'
        expect(command.command).toBe('npm');
        expect(command.args).toHaveLength(2);
        expect(command.timeout).toBe(30000);
    it('should define OutputInfo with type and directory', () => {
            type: 'SQL',
            directory: '/output/sql',
            appendOutputCode: true,
            options: [{ name: 'schemaName', value: '__mj' }]
        expect(output.type).toBe('SQL');
        expect(output.directory).toBe('/output/sql');
        expect(output.options).toHaveLength(1);
    it('should define CustomSQLScript with when and scriptFile', () => {
        const script = {
            scriptFile: 'init.sql'
        expect(script.when).toBe('before-all');
        expect(script.scriptFile).toBe('init.sql');
// Mock all dependencies
  RunReport: vi.fn(),
  RunQuery: vi.fn(),
  SetProvider: vi.fn(),
  StartupManager: {
      Startup: vi.fn().mockResolvedValue(true),
      RaiseEvent: vi.fn(),
vi.mock('../graphQLDataProvider', () => {
  class MockGraphQLDataProvider {
    Config = vi.fn().mockResolvedValue(true);
  class MockGraphQLProviderConfigData {
    WSURL: string;
    constructor(token: string, url: string, wsurl: string) {
      this.Token = token;
      this.WSURL = wsurl;
    GraphQLDataProvider: MockGraphQLDataProvider,
    GraphQLProviderConfigData: MockGraphQLProviderConfigData,
import { setupGraphQLClient } from '../config';
import { GraphQLProviderConfigData } from '../graphQLDataProvider';
import { SetProvider, StartupManager } from '@memberjunction/core';
describe('setupGraphQLClient', () => {
  it('should create a new GraphQLDataProvider', async () => {
      'test-token',
      'http://localhost:4000',
      'ws://localhost:4000',
      async () => 'token',
    const result = await setupGraphQLClient(config);
  it('should call SetProvider', async () => {
    expect(SetProvider).toHaveBeenCalled();
  it('should call StartupManager.Startup', async () => {
    expect(StartupManager.Instance.Startup).toHaveBeenCalled();
  it('should raise LoggedIn event', async () => {
    expect(MJGlobal.Instance.RaiseEvent).toHaveBeenCalled();
