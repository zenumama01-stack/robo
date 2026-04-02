import { LogError, LogStatus, CompositeKey } from '@memberjunction/core';
import { MJAIConfigurationEntity, MJAIConfigurationParamEntity, ResourceData } from '@memberjunction/core-entities';
interface ConfigurationWithParams extends MJAIConfigurationEntity {
  params?: MJAIConfigurationParamEntity[];
  compressionPrompt?: AIPromptEntityExtended | null;
  summarizationPrompt?: AIPromptEntityExtended | null;
 * AI Configuration Resource - displays AI system configuration management
@RegisterClass(BaseResourceComponent, 'AIConfigResource')
  selector: 'app-system-configuration',
  templateUrl: './system-configuration.component.html',
  styleUrls: ['./system-configuration.component.css']
export class SystemConfigurationComponent extends BaseResourceComponent implements OnInit {
  public configurations: ConfigurationWithParams[] = [];
  public filteredConfigurations: ConfigurationWithParams[] = [];
  public allParams: MJAIConfigurationParamEntity[] = [];
  public allPrompts: AIPromptEntityExtended[] = [];
  public currentFilters: SystemConfigFilter = {
  public selectedConfig: ConfigurationWithParams | null = null;
  // Stats
  public totalConfigs = 0;
  public activeConfigs = 0;
  public defaultConfig: ConfigurationWithParams | null = null;
      const configs = AIEngineBase.Instance.Configurations;
      const params = AIEngineBase.Instance.ConfigurationParams;
      // Create extended configurations with associated data
      this.configurations = configs.map(config => {
        const extended = config as ConfigurationWithParams;
        extended.params = params.filter(p => p.ConfigurationID === config.ID);
        extended.isExpanded = false;
        // Find linked prompts
        if (config.DefaultPromptForContextCompressionID) {
          extended.compressionPrompt = prompts.find(p => p.ID === config.DefaultPromptForContextCompressionID) || null;
        if (config.DefaultPromptForContextSummarizationID) {
          extended.summarizationPrompt = prompts.find(p => p.ID === config.DefaultPromptForContextSummarizationID) || null;
      this.allParams = params;
      this.allPrompts = prompts;
      // Calculate stats
      this.totalConfigs = this.configurations.length;
      this.activeConfigs = this.configurations.filter(c => c.Status === 'Active').length;
      this.defaultConfig = this.configurations.find(c => c.IsDefault) || null;
      LogStatus('AI Configurations loaded successfully');
      this.error = 'Failed to load AI configurations. Please try again.';
      LogError('Error loading AI configurations', undefined, error);
  public toggleExpanded(config: ConfigurationWithParams): void {
    config.isExpanded = !config.isExpanded;
  public onFiltersChange(filters: SystemConfigFilter): void {
    let filtered = [...this.configurations];
      filtered = filtered.filter(config =>
        config.Name.toLowerCase().includes(searchTerm) ||
        (config.Description || '').toLowerCase().includes(searchTerm) ||
        (config.params?.some(p => p.Name.toLowerCase().includes(searchTerm) ||
          (p.Description || '').toLowerCase().includes(searchTerm)))
      filtered = filtered.filter(config => config.Status === this.currentFilters.status);
    // Apply default configuration filter
    if (this.currentFilters.isDefault !== 'all') {
      const isDefault = this.currentFilters.isDefault === 'true';
      filtered = filtered.filter(config => config.IsDefault === isDefault);
    this.filteredConfigurations = this.applySorting(filtered);
   * Sort the configurations by the specified column
  private applySorting(configs: ConfigurationWithParams[]): ConfigurationWithParams[] {
    return configs.sort((a, b) => {
      let valueA: string | number | boolean | null | undefined;
      let valueB: string | number | boolean | null | undefined;
        case 'Parameters':
          valueA = a.params?.length || 0;
          valueB = b.params?.length || 0;
        case 'Updated':
          valueA = a.__mj_UpdatedAt ? new Date(a.__mj_UpdatedAt).getTime() : 0;
          valueB = b.__mj_UpdatedAt ? new Date(b.__mj_UpdatedAt).getTime() : 0;
      // Handle numeric comparison
      if (typeof valueA === 'number' && typeof valueB === 'number') {
        const comparison = valueA - valueB;
      // Handle string/other comparison
      const comparison = strA.localeCompare(strB);
  public onOpenConfiguration(config: ConfigurationWithParams): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: config.ID }]);
    this.navigationService.OpenEntityRecord('MJ: AI Configurations', compositeKey);
  public onOpenPrompt(promptId: string): void {
  public onOpenParam(param: MJAIConfigurationParamEntity): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: param.ID }]);
    this.navigationService.OpenEntityRecord('MJ: AI Configuration Params', compositeKey);
   * Show the detail panel for a configuration
  public showConfigDetails(config: ConfigurationWithParams, event?: Event): void {
    this.selectedConfig = config;
    // Delay clearing selectedConfig for smoother animation
        this.selectedConfig = null;
  public openConfigFromPanel(): void {
    if (this.selectedConfig) {
      this.onOpenConfiguration(this.selectedConfig);
      case 'Preview': return 'status-preview';
      case 'Inactive': return 'status-inactive';
      default: return 'status-unknown';
  public getStatusIcon(status: string): string {
      case 'Active': return 'fa-solid fa-circle-check';
      case 'Preview': return 'fa-solid fa-flask';
      case 'Inactive': return 'fa-solid fa-circle-pause';
      case 'Deprecated': return 'fa-solid fa-triangle-exclamation';
      default: return 'fa-solid fa-circle-question';
      case 'string': return 'fa-solid fa-font';
      case 'date': return 'fa-solid fa-calendar';
      case 'object': return 'fa-solid fa-brackets-curly';
      default: return 'fa-solid fa-code';
  public formatParamValue(param: MJAIConfigurationParamEntity): string {
    if (!param.Value) return '(not set)';
        return param.Value === 'true' ? 'Yes' : 'No';
          return JSON.stringify(JSON.parse(param.Value), null, 2).substring(0, 50) + '...';
          return param.Value.substring(0, 50) + '...';
        return param.Value.length > 50 ? param.Value.substring(0, 50) + '...' : param.Value;
  public formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
  async GetResourceDisplayName(_data: ResourceData): Promise<string> {
    return 'AI Configuration';
  async GetResourceIconClass(_data: ResourceData): Promise<string> {
    return 'fa-solid fa-sliders';
