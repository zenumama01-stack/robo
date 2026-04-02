export interface SubAgentSelectorResult {
  selectedAgents: AIAgentEntityExtended[];
  createNew: boolean;
export interface SubAgentSelectorConfig {
  multiSelect: boolean;
  selectedAgentIds: string[];
  showCreateNew: boolean;
  parentAgentId: string; // To exclude from selection
export interface AgentDisplayItem extends AIAgentEntityExtended {
 * Sub-Agent Selector Dialog for selecting agents to convert to sub-agents.
 * Only shows agents with NULL ParentID (root agents) that can become sub-agents.
  selector: 'mj-sub-agent-selector-dialog',
  templateUrl: './sub-agent-selector-dialog.component.html',
  styleUrls: ['./sub-agent-selector-dialog.component.css']
export class SubAgentSelectorDialogComponent implements OnInit, OnDestroy {
  config!: SubAgentSelectorConfig;
  public result = new Subject<SubAgentSelectorResult | null>();
  allAgents$ = new BehaviorSubject<AgentDisplayItem[]>([]);
  filteredAgents$ = new BehaviorSubject<AgentDisplayItem[]>([]);
  selectedAgents$ = new BehaviorSubject<Set<string>>(new Set());
  selectedTypeId$ = new BehaviorSubject<string>('all');
    return this.selectedAgents$.value.size;
  get totalAgentCount(): number {
    return this.allAgents$.value.length;
    return this.filteredAgents$.value.length;
    this.preselectExistingAgents();
      await this.loadAgentsAndTypes();
  private async loadAgentsAndTypes() {
    // Load both agents and types in a single batch for better performance
      // Root agents (index 0)
        ExtraFilter: `ParentID IS NULL AND ID != '${this.config.parentAgentId}' AND Status = 'Active' AND (ExposeAsAction = 0 OR ExposeAsAction IS NULL)`,
      // Agent types (index 1)
    // Process root agents (index 0)
    if (results[0].Success) {
      const agents: AgentDisplayItem[] = (results[0].Results || []).map(agent => ({
        ...agent.GetAll(),
        typeName: agent.Type || 'Default'
      } as AgentDisplayItem));
      this.allAgents$.next(agents);
    // Process agent types (index 1)
    if (results[1].Success) {
      this.agentTypes$.next(results[1].Results || []);
      this.allAgents$,
        startWith('')
      this.selectedTypeId$
    ).subscribe(([agents, searchTerm, typeId]) => {
      this.filterAgents(agents, searchTerm || '', typeId);
  private filterAgents(agents: AgentDisplayItem[], searchTerm: string, typeId: string) {
    let filtered = [...agents];
    if (typeId !== 'all') {
      filtered = filtered.filter(agent => agent.TypeID === typeId);
      filtered = filtered.filter(agent =>
        (agent.Name && agent.Name.toLowerCase().includes(term)) ||
        (agent.Description && agent.Description.toLowerCase().includes(term)) ||
        (agent.typeName && agent.typeName.toLowerCase().includes(term))
    this.filteredAgents$.next(filtered);
  private preselectExistingAgents() {
    if (this.config.selectedAgentIds.length > 0) {
      const selected = new Set(this.config.selectedAgentIds);
      this.selectedAgents$.next(selected);
      // Update agent selection state
      const agents = this.allAgents$.value;
        agent.selected = selected.has(agent.ID);
  selectType(typeId: string) {
    this.selectedTypeId$.next(typeId);
  toggleAgentSelection(agent: AgentDisplayItem) {
    const selected = this.selectedAgents$.value;
    // Find the agent and toggle its selection
    const agentToUpdate = agents.find(a => a.ID === agent.ID);
    if (agentToUpdate) {
      agentToUpdate.selected = !agentToUpdate.selected;
      if (agentToUpdate.selected) {
        if (!this.config.multiSelect) {
          // Single select mode - clear other selections
          selected.clear();
          agents.forEach(a => {
            if (a.ID !== agent.ID) {
              a.selected = false;
        selected.add(agent.ID);
        selected.delete(agent.ID);
      this.selectedAgents$.next(new Set(selected));
      // Update filtered agents to reflect selection state
      const filtered = this.filteredAgents$.value;
      const filteredAgent = filtered.find(a => a.ID === agent.ID);
      if (filteredAgent) {
        filteredAgent.selected = agentToUpdate.selected;
  getAgentIcon(agent: AgentDisplayItem): string {
    if (agent.IconClass) {
      return agent.IconClass;
  getAgentStatusColor(agent: AgentDisplayItem): string {
    switch (agent.Status) {
    this.result.next({
      selectedAgents: [],
  async addSelectedAgents() {
    const selectedIds = this.selectedAgents$.value;
    const allAgents = this.allAgents$.value;
    // Get the selected agent display items
    const selectedDisplayItems = allAgents
      .filter(agent => selectedIds.has(agent.ID));
    // Convert AgentDisplayItem to AIAgentEntityExtended by casting (they have the same structure)
    const selectedAgents: AIAgentEntityExtended[] = selectedDisplayItems.map(item => item as AIAgentEntityExtended);
      selectedAgents,
      createNew: false
