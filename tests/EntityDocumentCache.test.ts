import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
const { mockRunViews } = vi.hoisted(() => {
    mockRunViews: vi.fn(),
    RunView = vi.fn();
  MJEntityDocumentTypeEntity: vi.fn(),
import { EntityDocumentCache } from '../models/EntityDocumentCache';
describe('EntityDocumentCache', () => {
    // Reset singleton between tests
    (EntityDocumentCache as unknown as { _instance: undefined })._instance = undefined;
    it('should return singleton instance', () => {
      const instance1 = EntityDocumentCache.Instance;
      const instance2 = EntityDocumentCache.Instance;
      expect(instance1).toBe(instance2);
  describe('IsLoaded', () => {
    it('should be false initially', () => {
      const cache = EntityDocumentCache.Instance;
      expect(cache.IsLoaded).toBe(false);
  describe('GetDocument', () => {
    it('should return null for cache miss', () => {
      const result = cache.GetDocument('non-existent-id');
  describe('GetDocumentType', () => {
      const result = cache.GetDocumentType('non-existent-type-id');
  describe('GetDocumentByName', () => {
      const result = cache.GetDocumentByName('Non Existent');
  describe('GetDocumentTypeByName', () => {
      const result = cache.GetDocumentTypeByName('Non Existent');
  describe('GetFirstActiveDocumentForEntityByID', () => {
    it('should return null when no document type found', () => {
      const result = cache.GetFirstActiveDocumentForEntityByID('entity-1');
  describe('GetFirstActiveDocumentForEntityByName', () => {
      const result = cache.GetFirstActiveDocumentForEntityByName('Contacts');
  describe('SetCurrentUser', () => {
    it('should set the current user without errors', () => {
      cache.SetCurrentUser(mockUser);
      // No error thrown means success
  describe('Refresh', () => {
    it('should load documents and types from database', async () => {
      const mockDocuments = [
        { ID: 'doc-1', Name: 'Doc 1', EntityID: 'entity-1', Status: 'Active' },
        { ID: 'doc-2', Name: 'Doc 2', EntityID: 'entity-2', Status: 'Active' },
      const mockTypes = [
        { ID: 'type-1', Name: 'Record Duplicate' },
      mockRunViews.mockResolvedValue([
        { Success: true, Results: mockDocuments },
        { Success: true, Results: mockTypes },
      await cache.Refresh(true);
      expect(cache.IsLoaded).toBe(true);
      expect(cache.GetDocument('doc-1')).toBe(mockDocuments[0]);
      expect(cache.GetDocument('doc-2')).toBe(mockDocuments[1]);
      expect(cache.GetDocumentType('type-1')).toBe(mockTypes[0]);
    it('should skip refresh when already loaded and not forced', async () => {
        { Success: true, Results: [{ ID: 'doc-1' }] },
        { Success: true, Results: [] },
      expect(mockRunViews).toHaveBeenCalledTimes(1);
      // Second call without force should be skipped
      await cache.Refresh(false);
    it('should re-refresh when forced', async () => {
      expect(mockRunViews).toHaveBeenCalledTimes(2);
    it('should handle failed results gracefully', async () => {
        { Success: false, Results: [] },
    it('should use context user when provided', async () => {
      await cache.Refresh(true, mockUser);
      expect(mockRunViews).toHaveBeenCalledWith(
        expect.arrayContaining([
          expect.objectContaining({ EntityName: 'MJ: Entity Documents' }),
          expect.objectContaining({ EntityName: 'MJ: Entity Document Types' }),
        mockUser
  describe('GetDocumentTypeByName after Refresh', () => {
    it('should find document type by name (case insensitive)', async () => {
      const result = cache.GetDocumentTypeByName('record duplicate');
      expect(result).toBe(mockTypes[0]);
