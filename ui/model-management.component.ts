import { Subject, BehaviorSubject } from 'rxjs';
import { MJAIVendorEntity, MJAIModelTypeEntity, ResourceData, UserInfoEngine } from '@memberjunction/core-entities';
import { SharedService, BaseResourceComponent, NavigationService } from '@memberjunction/ng-shared';
interface ModelDisplayData extends AIModelEntityExtended {
  VendorName?: string;
  VendorID?: string; // Add this since we're using it for filtering
  ModelTypeName?: string;
  PowerRankDisplay?: string;
  SpeedRankDisplay?: string;
  CostRankDisplay?: string;
 * User preferences for the Model Management dashboard
interface ModelManagementUserPreferences {
  showFilters: boolean;
  selectedVendor: string;
  selectedType: string;
  selectedStatus: string;
  sortBy: string;
 * AI Models Resource - displays AI model management
@RegisterClass(BaseResourceComponent, 'AIModelsResource')
  selector: 'app-model-management',
  templateUrl: './model-management.component.html',
  styleUrls: ['./model-management.component.css']
export class ModelManagementComponent extends BaseResourceComponent implements OnInit, OnDestroy {
  private readonly USER_SETTINGS_KEY = 'AI.Models.UserPreferences';
  // View state
  public showFilters = true;
  public expandedModelId: string | null = null;
  // Data - Keep as AIModelEntityExtended to preserve getters
  public models: AIModelEntityExtended[] = [];
  public filteredModels: AIModelEntityExtended[] = [];
  public vendors: MJAIVendorEntity[] = [];
  public modelTypes: MJAIModelTypeEntity[] = [];
  // Filtering
  public searchTerm = '';
  private searchSubject = new BehaviorSubject<string>('');
  public selectedVendor = 'all';
  public selectedType = 'all';
  public selectedStatus = 'all';
  public powerRankRange = { min: 0, max: 10 };
  public speedRankRange = { min: 0, max: 10 };
  public costRankRange = { min: 0, max: 10 };
  public selectedModel: ModelDisplayData | null = null;
  public sortBy = 'name';
  public sortOptions = [
    { value: 'name', label: 'Name' },
    { value: 'vendor', label: 'Vendor' },
    { value: 'type', label: 'Type' },
    { value: 'powerRank', label: 'Power Rank' },
    { value: 'speedRank', label: 'Speed Rank' },
    { value: 'costRank', label: 'Cost Rank' },
    { value: 'created', label: 'Created Date' },
    { value: 'updated', label: 'Updated Date' }
  // Max rank values calculated from all models
  public maxPowerRank = 10;
  public maxSpeedRank = 10;
  public maxCostRank = 10;
  // Loading messages
  public loadingMessages = [
    'Loading AI models...',
    'Fetching vendor information...',
    'Calculating rankings...',
    'Almost ready...'
  public currentLoadingMessage = this.loadingMessages[0];
  private loadingMessageIndex = 0;
  private loadingMessageInterval: any;
    this.setupSearchListener();
    this.startLoadingMessages();
    if (this.loadingMessageInterval) {
      clearInterval(this.loadingMessageInterval);
  private setupSearchListener(): void {
    this.searchSubject.pipe(
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
        const prefs = JSON.parse(savedPrefs) as ModelManagementUserPreferences;
        this.applyUserPreferencesFromStorage(prefs);
      console.warn('[ModelManagement] Failed to load user preferences:', error);
  private applyUserPreferencesFromStorage(prefs: ModelManagementUserPreferences): void {
    if (prefs.showFilters !== undefined) {
      this.showFilters = prefs.showFilters;
    if (prefs.searchTerm) {
      this.searchTerm = prefs.searchTerm;
    if (prefs.selectedVendor) {
      this.selectedVendor = prefs.selectedVendor;
    if (prefs.selectedType) {
      this.selectedType = prefs.selectedType;
    if (prefs.selectedStatus) {
      this.selectedStatus = prefs.selectedStatus;
    if (prefs.sortBy) {
      this.sortBy = prefs.sortBy;
  private getCurrentPreferences(): ModelManagementUserPreferences {
      showFilters: this.showFilters,
      searchTerm: this.searchTerm,
      selectedVendor: this.selectedVendor,
      selectedType: this.selectedType,
      selectedStatus: this.selectedStatus,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
      console.warn('[ModelManagement] Failed to persist user preferences:', error);
  private startLoadingMessages(): void {
    this.loadingMessageInterval = setInterval(() => {
      this.loadingMessageIndex = (this.loadingMessageIndex + 1) % this.loadingMessages.length;
      this.currentLoadingMessage = this.loadingMessages[this.loadingMessageIndex];
    }, 2000);
  private async loadInitialData(): Promise<void> {
      // Get cached data from AIEngineBase
      const models = AIEngineBase.Instance.Models;
      this.vendors = AIEngineBase.Instance.Vendors;
      this.modelTypes = AIEngineBase.Instance.ModelTypes;
      // Log summary data
      // Create lookup maps
      const vendorMap = new Map(this.vendors.map(v => [v.ID, v.Name]));
      const typeMap = new Map(this.modelTypes.map(t => [t.ID, t.Name]));
      // Transform models to display format
      this.models = models.map((model) => {
        // Find vendor ID by matching vendor name
        let vendorId: string | undefined;
        if (model.Vendor) {
          const vendor = this.vendors.find(v => v.Name === model.Vendor);
          vendorId = vendor?.ID;
        // Don't spread the model - it loses getter properties!
        // Instead, augment the model with display properties
        const modelWithDisplay = model as ModelDisplayData;
        modelWithDisplay.VendorID = vendorId;
        modelWithDisplay.VendorName = model.Vendor || 'No Vendor';
        modelWithDisplay.ModelTypeName = model.AIModelTypeID ? typeMap.get(model.AIModelTypeID) || 'Unknown' : 'No Type';
      // Calculate max values for each rank type from ALL models
      this.maxPowerRank = Math.max(...this.models.map(m => m.PowerRank || 0), 10);
      this.maxSpeedRank = Math.max(...this.models.map(m => m.SpeedRank || 0), 10);
      this.maxCostRank = Math.max(...this.models.map(m => m.CostRank || 0), 10);
      // Update filter ranges based on actual max values
      this.powerRankRange = { min: 0, max: this.maxPowerRank };
      this.speedRankRange = { min: 0, max: this.maxSpeedRank };
      this.costRankRange = { min: 0, max: this.maxCostRank };
      this.filteredModels = [...this.models];
      this.sortModels();
      console.error('Error loading model data:', error);
      this.sharedService.CreateSimpleNotification('Error loading models', 'error', 3000);
  public formatRank(rank: number | null, rankType?: 'power' | 'speed' | 'cost'): string {
    if (rank === null) return 'N/A';
    // Determine which max value to use
    let maxValue = 10;
    if (rankType === 'power') {
      maxValue = this.maxPowerRank;
    } else if (rankType === 'speed') {
      maxValue = this.maxSpeedRank;
    } else if (rankType === 'cost') {
      maxValue = this.maxCostRank;
    return `${rank}/${maxValue}`;
    if (state.viewMode) this.viewMode = state.viewMode;
    if (state.showFilters !== undefined) this.showFilters = state.showFilters;
    if (state.searchTerm) this.searchTerm = state.searchTerm;
    if (state.selectedVendor) this.selectedVendor = state.selectedVendor;
    if (state.selectedType) this.selectedType = state.selectedType;
    if (state.selectedStatus) this.selectedStatus = state.selectedStatus;
    if (state.sortBy) this.sortBy = state.sortBy;
    if (state.powerRankRange) this.powerRankRange = state.powerRankRange;
    if (state.speedRankRange) this.speedRankRange = state.speedRankRange;
    if (state.costRankRange) this.costRankRange = state.costRankRange;
  public onSearchChange(value: string): void {
  public toggleFilters(): void {
    this.showFilters = !this.showFilters;
    this.expandedModelId = null;
  public toggleModelExpansion(modelId: string): void {
    this.expandedModelId = this.expandedModelId === modelId ? null : modelId;
  public applyFilters(): void {
    this.filteredModels = this.models.filter(m => {
      const model = m as ModelDisplayData;
      if (this.searchTerm) {
        const searchLower = this.searchTerm.toLowerCase();
          model.Name?.toLowerCase().includes(searchLower) ||
          model.Description?.toLowerCase().includes(searchLower) ||
          model.VendorName?.toLowerCase().includes(searchLower) ||
          model.ModelTypeName?.toLowerCase().includes(searchLower);
      // Vendor filter
      if (this.selectedVendor !== 'all' && model.VendorID !== this.selectedVendor) {
      if (this.selectedType !== 'all' && model.AIModelTypeID !== this.selectedType) {
      if (this.selectedStatus !== 'all') {
        const isActive = model.IsActive === true;
        if (this.selectedStatus === 'active' && !isActive) return false;
        if (this.selectedStatus === 'inactive' && isActive) return false;
      // Rank filters
      if (model.PowerRank !== null && (model.PowerRank < this.powerRankRange.min || model.PowerRank > this.powerRankRange.max)) {
      if (model.SpeedRank !== null && (model.SpeedRank < this.speedRankRange.min || model.SpeedRank > this.speedRankRange.max)) {
      if (model.CostRank !== null && (model.CostRank < this.costRankRange.min || model.CostRank > this.costRankRange.max)) {
  private sortModels(): void {
    this.filteredModels.sort((a, b) => {
      const modelA = a as ModelDisplayData;
      const modelB = b as ModelDisplayData;
      let comparison = 0;
      switch (this.sortBy) {
        case 'name':
          comparison = (modelA.Name || '').localeCompare(modelB.Name || '');
        case 'vendor':
          comparison = (modelA.VendorName || '').localeCompare(modelB.VendorName || '');
        case 'type':
          comparison = (modelA.ModelTypeName || '').localeCompare(modelB.ModelTypeName || '');
        case 'powerRank':
          comparison = (modelA.PowerRank || 0) - (modelB.PowerRank || 0);
        case 'speedRank':
          comparison = (modelA.SpeedRank || 0) - (modelB.SpeedRank || 0);
        case 'costRank':
          comparison = (modelA.CostRank || 0) - (modelB.CostRank || 0);
        case 'created':
          comparison = new Date(modelA.__mj_CreatedAt).getTime() - new Date(modelB.__mj_CreatedAt).getTime();
        case 'updated':
          comparison = new Date(modelA.__mj_UpdatedAt).getTime() - new Date(modelB.__mj_UpdatedAt).getTime();
          comparison = 0;
  public onVendorChange(vendorId: string): void {
    this.selectedVendor = vendorId;
  public onTypeChange(typeId: string): void {
    this.selectedType = typeId;
  public onStatusChange(status: string): void {
    this.selectedStatus = status;
  public onSortChange(sortBy: string): void {
    if (this.sortBy === sortBy) {
      this.sortBy = sortBy;
  public async toggleModelStatus(model: ModelDisplayData, event: Event): Promise<void> {
      model.IsActive = !model.IsActive;
      if (await model.Save()) {
        this.sharedService.CreateSimpleNotification(
          `Model ${model.IsActive ? 'activated' : 'deactivated'} successfully`,
        // Revert on failure
        throw new Error('Failed to save model status');
      console.error('Error toggling model status:', error);
      this.sharedService.CreateSimpleNotification('Error updating model status', 'error', 3000);
  public openModel(modelId: string): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: modelId }]);
    this.navigationService.OpenEntityRecord('MJ: AI Models', compositeKey);
   * Show the detail panel for a model
  public showModelDetails(model: AIModelEntityExtended, event?: Event): void {
    this.selectedModel = model as ModelDisplayData;
    // Delay clearing selectedModel for smoother animation
        this.selectedModel = null;
  public openModelFromPanel(): void {
    if (this.selectedModel) {
      this.openModel(this.selectedModel.ID);
  public async createNewModel(): Promise<void> {
      const newModel = await md.GetEntityObject<AIModelEntityExtended>('MJ: AI Models');
      if (newModel) {
        newModel.Name = 'New AI Model';
        newModel.IsActive = true;
        if (await newModel.Save()) {
          const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: newModel.ID }]);
          // Reload the data
          await this.loadInitialData();
      this.sharedService.CreateSimpleNotification('Error creating model', 'error', 3000);
  public getModelIcon(model: ModelDisplayData): string {
    const typeName = model.ModelTypeName?.toLowerCase();
    if (typeName?.includes('chat') || typeName?.includes('conversation')) {
    } else if (typeName?.includes('image') || typeName?.includes('vision')) {
      return 'fa-solid fa-image';
    } else if (typeName?.includes('audio') || typeName?.includes('speech')) {
      return 'fa-solid fa-microphone';
    } else if (typeName?.includes('embed')) {
      return 'fa-solid fa-vector-square';
    return 'fa-solid fa-microchip';
  public getRankClass(rank: number | null, rankType?: 'power' | 'speed' | 'cost'): string {
    if (rank === null || rank === 0) return 'rank-none';
    // Calculate percentage of max
    const percentage = (rank / maxValue) * 100;
    if (percentage >= 70) return 'rank-high';
    if (percentage >= 40) return 'rank-medium';
    return 'rank-low';
  public get hasActiveFilters(): boolean {
    return this.searchTerm !== '' || 
           this.selectedVendor !== 'all' || 
           this.selectedType !== 'all' || 
           this.selectedStatus !== 'all' ||
           this.powerRankRange.min > 0 ||
           this.powerRankRange.max < this.maxPowerRank ||
           this.speedRankRange.min > 0 ||
           this.speedRankRange.max < this.maxSpeedRank ||
           this.costRankRange.min > 0 ||
           this.costRankRange.max < this.maxCostRank;
  public clearFilters(): void {
    this.searchTerm = '';
    this.selectedVendor = 'all';
    this.selectedType = 'all';
    this.selectedStatus = 'all';
    this.searchSubject.next('');
  public formatTokenLimit(limit: number): string {
    if (limit >= 1000000) {
      return Math.floor(limit / 1000000) + 'M';
    } else if (limit >= 1000) {
      return Math.floor(limit / 1000) + 'K';
    return limit.toString();
  public validateAndApplyRankFilters(rankType: 'power' | 'speed' | 'cost'): void {
    // Get the appropriate range and max value based on type
    let range = rankType === 'power' ? this.powerRankRange :
                 rankType === 'speed' ? this.speedRankRange :
                 this.costRankRange;
    let maxValue = rankType === 'power' ? this.maxPowerRank :
                   rankType === 'speed' ? this.maxSpeedRank :
                   this.maxCostRank;
    // Ensure min is not greater than max
    if (range.min > range.max) {
      // Swap the values
      const temp = range.min;
      range.min = range.max;
      range.max = temp;
    // Ensure values are within bounds
    range.min = Math.max(0, Math.min(maxValue, range.min));
    range.max = Math.max(0, Math.min(maxValue, range.max));
    // Apply the filters
  // BaseResourceComponent abstract method implementations
    return 'Models';
