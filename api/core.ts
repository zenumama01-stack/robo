 * Mock for @memberjunction/core
 * Used in unit tests to avoid database dependencies
 * Note: These mocks are simplified versions. Use type assertions (as UserInfo)
 * when passing to functions that expect the real UserInfo type.
// Use 'any' base to allow type compatibility with the real UserInfo
export class UserInfo {
    Email: string;
    // Add common properties that real UserInfo has
    Roles: string[];
    UserRoles: unknown[];
    ApplicationID: string | null;
    EmployeeID: string | null;
        this.ID = (data?.ID as string) || 'mock-user-id';
        this.Name = (data?.Name as string) || 'Mock User';
        this.Email = (data?.Email as string) || 'mock@example.com';
        this.IsActive = (data?.IsActive as boolean) ?? true;
        this.Type = (data?.Type as string) || 'User';
        this.Roles = (data?.Roles as string[]) || [];
        this.UserRoles = (data?.UserRoles as unknown[]) || [];
        this.ApplicationID = (data?.ApplicationID as string) || null;
        this.EmployeeID = (data?.EmployeeID as string) || null;
    // Add common methods that real UserInfo has
    get UserRolesList(): string[] {
        return this.Roles;
// Mock RunView results storage - tests can configure this
let mockRunViewResults: Map<string, { Success: boolean; Results: unknown[]; ErrorMessage?: string }> = new Map();
export function setMockRunViewResult(entityName: string, result: { Success: boolean; Results: unknown[]; ErrorMessage?: string }) {
    mockRunViewResults.set(entityName, result);
export function clearMockRunViewResults() {
    mockRunViewResults.clear();
export class RunView {
    async RunView<T>(params: {
        EntityName: string;
        ExtraFilter?: string;
        OrderBy?: string;
        ResultType?: string;
        Fields?: string[];
    }, _contextUser?: UserInfo): Promise<{ Success: boolean; Results: T[]; ErrorMessage?: string }> {
        const result = mockRunViewResults.get(params.EntityName);
        if (result) {
            return result as { Success: boolean; Results: T[]; ErrorMessage?: string };
        // Default: return empty results
        return { Success: true, Results: [] };
    async RunViews(params: Array<{
    }>, _contextUser?: UserInfo): Promise<Array<{ Success: boolean; Results: unknown[] }>> {
        return params.map(p => {
            const result = mockRunViewResults.get(p.EntityName);
            return result || { Success: true, Results: [] };
// Mock entity storage - tests can configure created entities
let mockEntities: Map<string, unknown> = new Map();
export function setMockEntity(entityName: string, entity: unknown) {
    mockEntities.set(entityName, entity);
export function clearMockEntities() {
    mockEntities.clear();
export class Metadata {
    async GetEntityObject<T>(entityName: string, _contextUser?: UserInfo): Promise<T> {
        const entity = mockEntities.get(entityName);
            return entity as T;
        // Return a basic mock entity with all commonly used methods
        const mockData: Record<string, unknown> = { ID: 'mock-entity-id' };
            ID: 'mock-entity-id',
            GetAll: vi.fn().mockReturnValue(mockData),
            // Support setting any property
            set(key: string, value: unknown) { mockData[key] = value; }
        } as unknown as T;
// Re-export for convenience
export { mockRunViewResults, mockEntities };
let mockLoadData: Map<string, unknown[]> = new Map();
let mockEntityObjects: Map<string, unknown> = new Map();
let mockEntityInfos: Array<{ ID: string; Name: string }> = [
    { ID: 'cred-entity-id', Name: 'MJ: Credentials' }
export function setMockLoadData(entityName: string, data: unknown[]): void {
    mockLoadData.set(entityName, data);
export function setMockEntityObject(entityName: string, entity: unknown): void {
    mockEntityObjects.set(entityName, entity);
export function setMockEntityInfos(infos: Array<{ ID: string; Name: string }>): void {
    mockEntityInfos = infos;
export function clearAllCredentialMocks(): void {
    mockLoadData.clear();
    mockEntityObjects.clear();
    mockEntityInfos = [{ ID: 'cred-entity-id', Name: 'MJ: Credentials' }];
    _instances.clear();
// ---- BaseEngine singleton tracking ----
const _instances: Map<string, unknown> = new Map();
export function resetEngineInstances(): void {
// ---- BaseEngine ----
export class BaseEngine<_T> {
    protected _isLoaded = false;
    protected async Load(
        params: Array<{ PropertyName: string; EntityName: string; CacheLocal?: boolean }>,
        _contextUser?: unknown
            const data = mockLoadData.get(param.EntityName) || [];
            (this as Record<string, unknown>)[param.PropertyName] = data;
    protected TryThrowIfNotLoaded(): void {
        if (!this._isLoaded) {
            throw new Error('Engine has not been initialized. Call Config() first.');
    protected async RefreshItem(_propertyName: string): Promise<void> {
    protected static getInstance<U>(): U {
        const key = this.name || 'default';
        if (!_instances.has(key)) {
            _instances.set(key, new (this as unknown as new () => U)());
        return _instances.get(key) as U;
// ---- Metadata ----
    async GetEntityObject<T>(entityName: string, _contextUser?: unknown): Promise<T> {
        const entity = mockEntityObjects.get(entityName);
            GetAll: vi.fn().mockReturnValue({}),
    EntityByName(name: string): { ID: string; Name: string } | undefined {
        return mockEntityInfos.find(e =>
            e.Name?.trim().toLowerCase() === name.trim().toLowerCase()
// ---- UserInfo ----
    constructor(_provider?: unknown, data?: Record<string, unknown>) {
// ---- Types ----
export type IMetadataProvider = {
    Entities: Array<{ ID: string; Name: string }>;
export type EntityInfo = {
// ---- Logging ----
export const LogError = vi.fn();
export const LogStatus = vi.fn();
import { Arch, archFromString, ArchType, AsyncTaskManager } from "builder-util"
import { AllPublishOptions, CancellationToken, Nullish } from "builder-util-runtime"
// https://github.com/YousefED/typescript-json-schema/issues/80
export type Publish = AllPublishOptions | Array<AllPublishOptions> | null
export type TargetConfigType = Array<string | TargetConfiguration> | string | TargetConfiguration | null
export interface TargetConfiguration {
   * The target name. e.g. `snap`.
  readonly target: string
   * The arch or list of archs.
  readonly arch?: Array<ArchType> | ArchType
export class Platform {
  static MAC = new Platform("mac", "mac", "darwin")
  static LINUX = new Platform("linux", "linux", "linux")
  static WINDOWS = new Platform("windows", "win", "win32")
    public name: string,
    public buildConfigurationKey: string,
    public nodeName: NodeJS.Platform
  toString() {
    return this.name
  createTarget(type?: string | Array<string> | null, ...archs: Array<Arch>): Map<Platform, Map<Arch, Array<string>>> {
    if (type == null && (archs == null || archs.length === 0)) {
      return new Map([[this, new Map()]])
    const archToType = new Map()
    for (const arch of archs == null || archs.length === 0 ? [archFromString(process.arch)] : archs) {
      archToType.set(arch, type == null ? [] : Array.isArray(type) ? type : [type])
    return new Map([[this, archToType]])
  static current(): Platform {
    return Platform.fromString(process.platform)
  static fromString(name: string): Platform {
    name = name.toLowerCase()
      case Platform.MAC.nodeName:
      case Platform.MAC.name:
      case Platform.WINDOWS.nodeName:
      case Platform.WINDOWS.name:
      case Platform.WINDOWS.buildConfigurationKey:
      case Platform.LINUX.nodeName:
        throw new Error(`Unknown platform: ${name}`)
export abstract class Target {
  abstract readonly outDir: string
  abstract readonly options: TargetSpecificOptions | Nullish
  // use only for tasks that cannot be executed in parallel (such as signing on windows and hdiutil on macOS due to file locking)
  readonly buildQueueManager = new AsyncTaskManager(new CancellationToken())
  protected constructor(
    readonly name: string,
    readonly isAsyncSupported: boolean = true
  async checkOptions(): Promise<any> {
  abstract build(appOutDir: string, arch: Arch): Promise<any>
  async finishBuild(): Promise<any> {
    await this.buildQueueManager.awaitTasks()
export interface TargetSpecificOptions {
   The [artifact file name template](./configuration.md#artifact-file-name-template).
  readonly artifactName?: string | null
  publish?: Publish
export const DEFAULT_TARGET = "default"
export const DIR_TARGET = "dir"
export type CompressionLevel = "store" | "normal" | "maximum"
export interface BeforeBuildContext {
  readonly appDir: string
  readonly electronVersion: string
  readonly platform: Platform
  readonly arch: string
export interface SourceRepositoryInfo {
  type?: string
  domain?: string
  user: string
  project: string
