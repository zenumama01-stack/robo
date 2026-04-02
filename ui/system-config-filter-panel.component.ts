interface SystemConfigFilter {
  isDefault: string;
  selector: 'mj-system-config-filter-panel',
  templateUrl: './system-config-filter-panel.component.html',
  styleUrls: ['./system-config-filter-panel.component.css']
export class SystemConfigFilterPanelComponent implements OnInit {
  @Input() configurations: MJAIConfigurationEntity[] = [];
  @Input() filteredConfigurations: MJAIConfigurationEntity[] = [];
  @Input() filters: SystemConfigFilter = {
    isDefault: 'all'
  @Output() filtersChange = new EventEmitter<SystemConfigFilter>();
  public isDefaultOptions = [
    { text: 'All Configurations', value: 'all' },
    { text: 'Default Only', value: 'true' },
    { text: 'Non-Default Only', value: 'false' }
    // Initialize component
