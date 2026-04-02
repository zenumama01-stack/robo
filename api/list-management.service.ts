  RecordMembershipInfo,
  CreateListConfig
} from '../models/list-management.models';
interface CacheEntry<T> {
  data: T;
 * Service for managing list operations including data loading, caching,
 * and batch add/remove operations.
export class ListManagementService {
  private readonly CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes
  // Cache storage
  private listCache = new Map<string, CacheEntry<MJListEntity[]>>();
  private categoryCache: CacheEntry<MJListCategoryEntity[]> | null = null;
  private membershipCache = new Map<string, CacheEntry<Map<string, string[]>>>();
  // Loading state subjects
   * Get lists for a specific entity, with optional caching
  async getListsForEntity(
    entityId: string,
    forceRefresh: boolean = false
  ): Promise<MJListEntity[]> {
    const cacheKey = `${entityId}_${userId || 'all'}`;
    if (!forceRefresh) {
      const cached = this.listCache.get(cacheKey);
      if (cached && this.isCacheValid(cached)) {
        return cached.data;
      let filter = `EntityID = '${entityId}'`;
        filter += ` AND UserID = '${userId}'`;
        const lists = result.Results || [];
        this.listCache.set(cacheKey, {
          data: lists,
        return lists;
        console.error('Failed to load lists:', result.ErrorMessage);
   * Get all list categories
  async getListCategories(forceRefresh: boolean = false): Promise<MJListCategoryEntity[]> {
    if (!forceRefresh && this.categoryCache && this.isCacheValid(this.categoryCache)) {
      return this.categoryCache.data;
      this.categoryCache = {
        data: result.Results || [],
   * Get membership information for a set of records
   * Returns a Map where key is listId and value is array of recordIds that are members
  async getRecordMembership(
    recordIds: string[]
  ): Promise<Map<string, string[]>> {
    const cacheKey = `${entityId}_${recordIds.sort().join(',')}`;
    const cached = this.membershipCache.get(cacheKey);
      // Get all list details for these records
        ExtraFilter: `RecordID IN (${recordIdFilter})`,
      const membership = new Map<string, string[]>();
          const existingRecords = membership.get(detail.ListID) || [];
          if (!existingRecords.includes(detail.RecordID)) {
            existingRecords.push(detail.RecordID);
          membership.set(detail.ListID, existingRecords);
      this.membershipCache.set(cacheKey, {
        data: membership,
      return membership;
   * Get lists that contain a specific record
  async getListsForRecord(
    recordId: string
    if (!detailsResult.Success || !detailsResult.Results || detailsResult.Results.length === 0) {
    const listIds = [...new Set(detailsResult.Results.map((d: MJListDetailEntity) => d.ListID))];
    // Get the lists filtered by entity
      ExtraFilter: `ID IN (${listIdFilter}) AND EntityID = '${entityId}'`,
    return listsResult.Success ? (listsResult.Results || []) : [];
   * Get item count for a list
  async getListItemCount(listId: string): Promise<number> {
      ExtraFilter: `ListID = '${listId}'`,
    return result.Success ? result.TotalRowCount : 0;
   * Build view models for lists with membership information
  async buildListViewModels(
    lists: MJListEntity[],
    recordIds: string[],
    membership: Map<string, string[]>
  ): Promise<ListItemViewModel[]> {
    const viewModels: ListItemViewModel[] = [];
    // Get item counts for all lists in one batch query
    // Get counts grouped by list - using a regular query since we need aggregation
    const countsResult = await rv.RunView<MJListDetailEntity>({
      ExtraFilter: listIds.length > 0 ? `ListID IN (${listIdFilter})` : '1=0',
    // Build count map
    const countMap = new Map<string, number>();
    if (countsResult.Success && countsResult.Results) {
      for (const detail of countsResult.Results) {
        const current = countMap.get(detail.ListID) || 0;
        countMap.set(detail.ListID, current + 1);
      const memberRecordIds = membership.get(list.ID) || [];
      const membershipCount = memberRecordIds.length;
      const totalSelected = recordIds.length;
      viewModels.push({
        itemCount: countMap.get(list.ID) || 0,
        membershipCount,
        totalSelectedRecords: totalSelected,
        isFullMember: membershipCount === totalSelected && totalSelected > 0,
        isPartialMember: membershipCount > 0 && membershipCount < totalSelected,
        isNotMember: membershipCount === 0,
        lastUpdated: list.Get('__mj_UpdatedAt') as Date || new Date(),
        category: undefined, // Can be populated separately if needed
        isSelectedForAdd: false,
    return viewModels;
   * Add records to one or more lists
  async addRecordsToLists(
    skipDuplicates: boolean = true
  ): Promise<BatchOperationResult> {
    console.log(`[ListManagementService] addRecordsToLists called:`);
    console.log(`  - listIds (${listIds.length}):`, listIds);
    console.log(`  - recordIds (${recordIds.length}):`, recordIds);
    console.log(`  - skipDuplicates:`, skipDuplicates);
      success: 0,
      skipped: 0,
    // Get existing membership to skip duplicates
    let existingMembership = new Map<string, Set<string>>();
      const existing = await rv.RunView<MJListDetailEntity>({
        ExtraFilter: `ListID IN (${listIdFilter}) AND RecordID IN (${recordIdFilter})`,
      if (existing.Success && existing.Results) {
        for (const detail of existing.Results) {
          const recordSet = existingMembership.get(detail.ListID) || new Set();
          recordSet.add(detail.RecordID);
          existingMembership.set(detail.ListID, recordSet);
    // Build list of records to add (excluding duplicates)
    const recordsToAdd: Array<{listId: string, recordId: string}> = [];
      const existingRecords = existingMembership.get(listId) || new Set();
        if (skipDuplicates && existingRecords.has(recordId)) {
          result.skipped++;
          recordsToAdd.push({ listId, recordId });
      console.log(`[ListManagementService] No records to add (all skipped as duplicates)`);
    for (const { listId, recordId } of recordsToAdd) {
        const listDetail = await md.GetEntityObject<MJListDetailEntity>('MJ: List Details', md.CurrentUser);
          console.error(`[ListManagementService] Failed to queue record ${recordId} for list ${listId}:`, listDetail.LatestResult?.Message);
          result.errors.push(`Failed to add record ${recordId} to list ${listId}: ${listDetail.LatestResult?.Message || 'Unknown error'}`);
        console.error(`[ListManagementService] Exception adding record:`, error);
        result.errors.push(`Error adding record ${recordId} to list ${listId}: ${errorMessage}`);
    console.log(`[ListManagementService] Submitting transaction with ${recordsToAdd.length} records...`);
      result.success = recordsToAdd.length - result.errors.length;
      result.failed = result.errors.length;
      console.log(`[ListManagementService] Transaction succeeded. Added ${result.success} records.`);
      result.failed = recordsToAdd.length;
      result.success = 0;
      console.error(`[ListManagementService] Transaction failed`);
      result.errors.push('Transaction failed to submit');
    // Invalidate membership cache
    this.membershipCache.clear();
    console.log(`[ListManagementService] addRecordsToLists final result:`, result);
   * Remove records from one or more lists
  async removeRecordsFromLists(
    // Find existing list details to delete
      result.errors.push('Failed to query existing list details');
    // Delete each matching detail
    for (const detail of existingResult.Results || []) {
          result.success++;
          result.failed++;
          result.errors.push(`Failed to remove record ${detail.RecordID} from list ${detail.ListID}`);
        result.errors.push(`Error removing record: ${errorMessage}`);
    // Invalidate caches
  async createList(config: CreateListConfig): Promise<MJListEntity | null> {
      const list = await md.GetEntityObject<MJListEntity>('MJ: Lists');
      list.Name = config.name;
      list.Description = config.description || '';
      list.EntityID = config.entityId;
      list.UserID = md.CurrentUser.ID;
        list.CategoryID = config.categoryId;
        // Invalidate list cache
        this.invalidateListCache(config.entityId);
   * Check if a cache entry is still valid
  private isCacheValid<T>(entry: CacheEntry<T>): boolean {
    const now = new Date().getTime();
    const entryTime = entry.timestamp.getTime();
    return (now - entryTime) < this.CACHE_TTL_MS;
   * Invalidate cache for a specific entity or all caches
  invalidateCache(entityId?: string): void {
      this.invalidateListCache(entityId);
      this.listCache.clear();
      this.categoryCache = null;
   * Invalidate list cache for a specific entity
  private invalidateListCache(entityId: string): void {
    // Remove all cache entries that start with this entityId
    for (const key of this.listCache.keys()) {
      if (key.startsWith(entityId)) {
        this.listCache.delete(key);
