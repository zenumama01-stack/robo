import { existsSync } from 'fs';
import { resolve } from 'path';
describe('@memberjunction/ng-action-gallery', () => {
  it('should have a public API entry point', () => {
    const pkgRoot = resolve(__dirname, '../..');
    const hasPublicApi = existsSync(resolve(pkgRoot, 'src/public-api.ts'));
    const hasIndex = existsSync(resolve(pkgRoot, 'src/index.ts'));
    expect(hasPublicApi || hasIndex).toBe(true);
  it('should have a package.json with correct name', () => {
    const pkg = require(resolve(pkgRoot, 'package.json'));
    expect(pkg.name).toBe('@memberjunction/ng-action-gallery');
describe('@memberjunction/ng-actions', () => {
    expect(pkg.name).toBe('@memberjunction/ng-actions');
describe('@memberjunction/ng-agents', () => {
    expect(pkg.name).toBe('@memberjunction/ng-agents');
describe('@memberjunction/ng-artifacts', () => {
    expect(pkg.name).toBe('@memberjunction/ng-artifacts');
describe('@memberjunction/ng-chat', () => {
    expect(pkg.name).toBe('@memberjunction/ng-chat');
describe('@memberjunction/ng-container-directives', () => {
    expect(pkg.name).toBe('@memberjunction/ng-container-directives');
describe('@memberjunction/ng-credentials', () => {
    expect(pkg.name).toBe('@memberjunction/ng-credentials');
describe('@memberjunction/ng-dashboard-viewer', () => {
    expect(pkg.name).toBe('@memberjunction/ng-dashboard-viewer');
describe('@memberjunction/ng-data-context', () => {
    expect(pkg.name).toBe('@memberjunction/ng-data-context');
describe('@memberjunction/ng-deep-diff', () => {
    expect(pkg.name).toBe('@memberjunction/ng-deep-diff');
describe('@memberjunction/ng-entity-communications', () => {
    expect(pkg.name).toBe('@memberjunction/ng-entity-communications');
describe('@memberjunction/ng-file-storage', () => {
    expect(pkg.name).toBe('@memberjunction/ng-file-storage');
describe('@memberjunction/ng-find-record', () => {
    expect(pkg.name).toBe('@memberjunction/ng-find-record');
describe('@memberjunction/ng-flow-editor', () => {
    expect(pkg.name).toBe('@memberjunction/ng-flow-editor');
describe('@memberjunction/ng-generic-dialog', () => {
    expect(pkg.name).toBe('@memberjunction/ng-generic-dialog');
describe('@memberjunction/ng-join-grid', () => {
    expect(pkg.name).toBe('@memberjunction/ng-join-grid');
describe('@memberjunction/ng-notifications', () => {
    expect(pkg.name).toBe('@memberjunction/ng-notifications');
describe('@memberjunction/ng-query-grid', () => {
    expect(pkg.name).toBe('@memberjunction/ng-query-grid');
describe('@memberjunction/ng-query-viewer', () => {
    expect(pkg.name).toBe('@memberjunction/ng-query-viewer');
describe('@memberjunction/ng-record-selector', () => {
    expect(pkg.name).toBe('@memberjunction/ng-record-selector');
describe('@memberjunction/ng-resource-permissions', () => {
    expect(pkg.name).toBe('@memberjunction/ng-resource-permissions');
describe('@memberjunction/ng-shared-generic', () => {
    expect(pkg.name).toBe('@memberjunction/ng-shared-generic');
describe('@memberjunction/ng-tabstrip', () => {
    expect(pkg.name).toBe('@memberjunction/ng-tabstrip');
describe('@memberjunction/ng-user-avatar', () => {
    expect(pkg.name).toBe('@memberjunction/ng-user-avatar');
  ComponentSpec,
  BuildComponentCompleteCode,
  BuildComponentCode,
describe('InteractiveComponents exports', () => {
  it('should export ComponentSpec class', () => {
    expect(ComponentSpec).toBeDefined();
    const spec = new ComponentSpec();
    expect(spec).toBeInstanceOf(ComponentSpec);
  it('should export BuildComponentCompleteCode function', () => {
    expect(typeof BuildComponentCompleteCode).toBe('function');
  it('should export BuildComponentCode function', () => {
    expect(typeof BuildComponentCode).toBe('function');
  EntityFieldValueInfo: class {},
  SimpleEntityInfo: class {
  SimpleEntityFieldInfo: class {
import * as index from '../index';
describe('SkipTypes exports', () => {
  it('should export utility functions', () => {
    expect(typeof index.MapEntityFieldInfoToSkipEntityFieldInfo).toBe('function');
    expect(typeof index.MapEntityFieldValueInfoToSkipEntityFieldValueInfo).toBe('function');
    expect(typeof index.MapEntityRelationshipInfoToSkipEntityRelationshipInfo).toBe('function');
    expect(typeof index.skipEntityHasField).toBe('function');
    expect(typeof index.skipEntityGetField).toBe('function');
    expect(typeof index.skipEntityGetFieldNameSet).toBe('function');
    expect(typeof index.MapEntityInfoToSkipEntityInfo).toBe('function');
    expect(typeof index.MapEntityInfoArrayToSkipEntityInfoArray).toBe('function');
    expect(typeof index.MapSkipEntityInfoToEntityInfo).toBe('function');
    expect(typeof index.MapSkipEntityFieldInfoToEntityFieldInfo).toBe('function');
    expect(typeof index.MapSimpleEntityInfoToSkipEntityInfo).toBe('function');
    expect(typeof index.MapSimpleEntityInfoArrayToSkipEntityInfoArray).toBe('function');
    expect(typeof index.MapSkipEntityInfoToSimpleEntityInfo).toBe('function');
    expect(typeof index.MapSimpleEntityFieldInfoToSkipEntityFieldInfo).toBe('function');
    expect(typeof index.MapSkipEntityFieldInfoToSimpleEntityFieldInfo).toBe('function');
