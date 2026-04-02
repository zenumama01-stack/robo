import { FlowNode, FlowConnection, FlowConnectionStyle, FlowNodeTypeConfig, FlowNodePort } from '../interfaces/flow-types';
/** Picker item shape for Actions with optional icon */
export interface ActionPickerItem { ID: string; Name: string; IconClass?: string | null; }
/** Picker item shape for Agents with optional icon and logo */
export interface AgentPickerItem { ID: string; Name: string; IconClass?: string | null; LogoURL?: string | null; }
/** Step types mapped to visual configuration */
export const AGENT_STEP_TYPE_CONFIGS: FlowNodeTypeConfig[] = [
    Type: 'Action',
    Label: 'Action',
    Icon: 'fa-bolt',
    Color: '#3B82F6',
    Category: 'Steps',
    DefaultPorts: [
      { ID: 'input', Direction: 'input', Side: 'top', Multiple: true },
      { ID: 'output', Direction: 'output', Side: 'bottom', Multiple: true }
    Type: 'Prompt',
    Label: 'Prompt',
    Icon: 'fa-comment-dots',
    Color: '#8B5CF6',
    Type: 'Sub-Agent',
    Label: 'Sub-Agent',
    Icon: 'fa-robot',
    Color: '#10B981',
    Type: 'ForEach',
    Label: 'For Each',
    Icon: 'fa-arrows-repeat',
    Color: '#F59E0B',
    Category: 'Loops',
    Type: 'While',
    Label: 'While',
    Icon: 'fa-rotate',
    Color: '#F97316',
 * Transforms MJ AIAgentStep/Path entities to/from generic FlowNode/FlowConnection models.
export class AgentFlowTransformerService {
  /** Convert MJ step entities to generic FlowNodes */
  StepsToNodes(
    actions?: ActionPickerItem[],
    agents?: AgentPickerItem[]
  ): FlowNode[] {
    return steps.map(step => this.StepToNode(step, actions, agents));
  /** Convert MJ path entities to generic FlowConnections */
  PathsToConnections(paths: MJAIAgentStepPathEntity[]): FlowConnection[] {
    return paths.map(path => this.pathToConnection(path, paths));
  /** Build the subtitle for a step based on its configured action/prompt/agent */
  BuildStepSubtitle(step: MJAIAgentStepEntity): string {
    switch (step.StepType) {
        return step.Action ? `Action: ${step.Action}` : 'No action selected';
        return step.Prompt ? `Prompt: ${step.Prompt}` : 'No prompt selected';
        return step.SubAgent ? `Agent: ${step.SubAgent}` : 'No sub-agent selected';
        return this.buildLoopSubtitle(step, 'For Each');
        return this.buildLoopSubtitle(step, 'While');
        return step.StepType;
  /** Apply FlowNode position changes back to a step entity */
  ApplyNodePosition(step: MJAIAgentStepEntity, node: FlowNode): void {
    step.PositionX = Math.round(node.Position.X);
    step.PositionY = Math.round(node.Position.Y);
    if (node.Size) {
      step.Width = Math.round(node.Size.Width);
      step.Height = Math.round(node.Size.Height);
  /** Map FlowNode status from step entity status */
  MapStepStatus(stepStatus: string): FlowNode['Status'] {
    switch (stepStatus) {
      case 'Active': return 'default';
      case 'Disabled': return 'disabled';
      case 'Pending': return 'pending';
      default: return 'default';
   * Returns a short human-readable message describing what's missing,
   * or null if the step is fully configured.
  BuildConfigWarningMessage(step: MJAIAgentStepEntity): string | null {
        return !step.ActionID ? 'No action selected' : null;
        return !step.PromptID ? 'No prompt selected' : null;
        return !step.SubAgentID ? 'No sub-agent selected' : null;
        return this.buildLoopWarningMessage(step);
  private buildLoopWarningMessage(step: MJAIAgentStepEntity): string | null {
    const bodyType = step.LoopBodyType;
    if (!bodyType) return 'No loop body type selected';
    switch (bodyType) {
      case 'Action': return !step.ActionID ? 'No action selected for loop body' : null;
      case 'Prompt': return !step.PromptID ? 'No prompt selected for loop body' : null;
      case 'Sub-Agent': return !step.SubAgentID ? 'No sub-agent selected for loop body' : null;
   * Returns true when a step is missing its required configuration reference
   * (e.g., an Action step with no ActionID, a Prompt step with no PromptID).
  IsStepMissingConfiguration(step: MJAIAgentStepEntity): boolean {
        return !step.ActionID;
        return !step.PromptID;
        return !step.SubAgentID;
        return this.isLoopBodyMissingReference(step);
  private isLoopBodyMissingReference(step: MJAIAgentStepEntity): boolean {
    if (!bodyType) return true; // No body type selected at all
      case 'Action': return !step.ActionID;
      case 'Prompt': return !step.PromptID;
      case 'Sub-Agent': return !step.SubAgentID;
   * Resolve the best icon and optional logo URL for a step based on its
   * configured action/agent and available picker data.
   * Resolution chain:
   * - Action step: action's IconClass -> step-type fallback
   * - Sub-Agent step: agent's LogoURL (stored in Data) -> agent's IconClass -> step-type fallback
   * - Other steps: step-type icon
  ResolveStepIcon(
    step: MJAIAgentStepEntity,
  ): { Icon: string; LogoURL?: string | null } {
    const fallbackIcon = this.getIconForType(step.StepType);
    if (step.StepType === 'Action' && step.ActionID && actions) {
      const action = actions.find(a => a.ID === step.ActionID);
      return { Icon: action?.IconClass || fallbackIcon };
    if (step.StepType === 'Sub-Agent' && step.SubAgentID && agents) {
      const agent = agents.find(a => a.ID === step.SubAgentID);
        return { Icon: agent.IconClass || fallbackIcon, LogoURL: agent.LogoURL };
      return { Icon: agent?.IconClass || fallbackIcon };
    // For loop steps, resolve based on loop body type
    if ((step.StepType === 'ForEach' || step.StepType === 'While') && step.LoopBodyType) {
      return this.resolveLoopBodyIcon(step, actions, agents);
    return { Icon: fallbackIcon };
  private resolveLoopBodyIcon(
    if (step.LoopBodyType === 'Action' && step.ActionID && actions) {
      if (action?.IconClass) return { Icon: fallbackIcon }; // Loop keeps its own icon; body icon handled separately
    if (step.LoopBodyType === 'Sub-Agent' && step.SubAgentID && agents) {
      if (agent?.LogoURL) return { Icon: fallbackIcon, LogoURL: agent.LogoURL };
  // ── Public Conversion Methods ───────────────────────────────
  /** Convert a single MJ step entity to a FlowNode (public for direct use when adding nodes) */
  StepToNode(
  ): FlowNode {
    const stepId = step.ID;
    const ports: FlowNodePort[] = [
        ID: `${stepId}-input`,
        Direction: 'input',
        Side: 'top',
        Multiple: true,
        Disabled: step.StartingStep === true
        ID: `${stepId}-output`,
        Direction: 'output',
        Side: 'bottom',
        Multiple: true
    // Show warning status when the step is missing its required configuration,
    // unless the step is explicitly disabled (respect the user's intent).
    const baseStatus = this.MapStepStatus(step.Status);
    const warningMessage = (baseStatus !== 'disabled') ? this.BuildConfigWarningMessage(step) : null;
    const effectiveStatus = warningMessage ? 'warning' : baseStatus;
    // Build loop-specific data for ForEach/While nodes
    const data: Record<string, unknown> = { StepEntityID: stepId };
    if (step.StepType === 'ForEach' || step.StepType === 'While') {
      this.populateLoopData(step, data, actions, agents);
    // Resolve icon (and optional logo URL) from picker data
    const resolved = this.ResolveStepIcon(step, actions, agents);
    if (resolved.LogoURL) {
      data['LogoURL'] = resolved.LogoURL;
      ID: stepId,
      Type: step.StepType,
      Label: step.Name,
      Subtitle: this.BuildStepSubtitle(step),
      Icon: resolved.Icon,
      Status: effectiveStatus,
      StatusMessage: warningMessage ?? undefined,
      IsStartNode: step.StartingStep === true,
      Position: {
        X: step.PositionX ?? 0,
        Y: step.PositionY ?? 0
      Size: {
        Width: step.Width ?? 220,
        Height: step.Height ?? 100
      Ports: ports,
  private pathToConnection(path: MJAIAgentStepPathEntity, allPaths: MJAIAgentStepPathEntity[]): FlowConnection {
    const hasCondition = path.Condition != null && path.Condition.trim().length > 0;
    const isAlwaysPath = !hasCondition;
    // Analyze sibling paths from the same origin step
    const siblingPaths = allPaths.filter(p => p.OriginStepID === path.OriginStepID);
    const isOnlyPath = siblingPaths.length === 1;
    const unconditionalSiblings = siblingPaths.filter(
      p => !p.Condition || p.Condition.trim().length === 0
    // Flag as ambiguous when 2+ unconditional paths exist from the same source.
    // Multiple unconditional paths are always ambiguous because only the
    // highest-priority one will execute — regardless of whether they have descriptions.
    const hasAmbiguousAlways = isAlwaysPath && unconditionalSiblings.length > 1;
    // Build label, icon, and visual style
    const visual = this.buildPathVisuals(path, hasCondition, isOnlyPath, hasAmbiguousAlways);
      SourceNodeID: path.OriginStepID,
      SourcePortID: `${path.OriginStepID}-output`,
      TargetNodeID: path.DestinationStepID,
      TargetPortID: `${path.DestinationStepID}-input`,
      Label: visual.label,
      LabelIcon: visual.labelIcon,
      LabelIconColor: visual.labelIconColor,
      LabelDetail: visual.labelDetail,
      Condition: path.Condition ?? undefined,
      Priority: path.Priority,
      Style: visual.style,
      Color: visual.color,
      Data: {
        PathEntityID: path.ID,
        IsAlwaysPath: !hasCondition,
        HasAmbiguousAlways: hasAmbiguousAlways
  private buildPathVisuals(
    path: MJAIAgentStepPathEntity,
    hasCondition: boolean,
    isOnlyPath: boolean,
    hasAmbiguousAlways: boolean
  ): { label?: string; labelIcon?: string; labelIconColor?: string; labelDetail?: string; color: string; style: FlowConnectionStyle } {
    // Conditional path — amber dashed with condition text
    if (hasCondition) {
        label: path.Description || path.Condition!,
        color: '#f59e0b',
        style: 'dashed'
    // Sole unconditional (only exit path) — neutral dark slate with Default indicator
    if (isOnlyPath) {
        label: path.Description || 'Default',
        labelIcon: 'fa-circle-check',
        labelIconColor: '#16a34a',
        color: '#64748b',
        style: 'solid'
    // Ambiguous: multiple unconditional paths from the same step
    if (hasAmbiguousAlways) {
        labelIcon: 'fa-triangle-exclamation',
        labelIconColor: '#ef4444',
        labelDetail: 'Duplicate default paths: only the highest-priority one will execute',
        color: '#ef4444',
    // Valid default path (unconditional among conditional siblings) — forest green with checkmark
      color: '#16a34a',
  /** Populate loop-specific display data on the node's Data payload */
  private populateLoopData(
    data: Record<string, unknown>,
    data['LoopBodyType'] = bodyType ?? null;
    data['LoopBodyName'] = bodyType ? this.resolveLoopBodyName(step) : null;
    data['LoopBodyIcon'] = bodyType ? this.resolveLoopBodySpecificIcon(step, actions, agents) : null;
    data['LoopBodyColor'] = bodyType ? this.getBodyTypeColor(bodyType) : null;
    data['LoopIterationSummary'] = this.BuildLoopIterationSummary(step);
    // Store logo URL for loop body sub-agents
    if (bodyType === 'Sub-Agent' && step.SubAgentID && agents) {
        data['LoopBodyLogoURL'] = agent.LogoURL;
    const config = this.parseLoopConfig(step);
      data['MaxIterations'] = config['maxIterations'] ?? null;
      data['LoopItemVariable'] = config['itemVariable'] ?? null;
  /** Resolve the best icon for a loop body, checking picker data first */
  private resolveLoopBodySpecificIcon(
    const fallback = this.getBodyTypeIcon(bodyType ?? '');
    if (bodyType === 'Action' && step.ActionID && actions) {
      return action?.IconClass || fallback;
      return agent?.IconClass || fallback;
    return fallback;
  /** Get icon for a loop body type */
  private getBodyTypeIcon(bodyType: string): string {
      case 'Action': return 'fa-bolt';
      case 'Prompt': return 'fa-comment-dots';
      case 'Sub-Agent': return 'fa-robot';
      default: return 'fa-circle-nodes';
  /** Get color for a loop body type */
  private getBodyTypeColor(bodyType: string): string {
      case 'Action': return '#3B82F6';
      case 'Prompt': return '#8B5CF6';
      case 'Sub-Agent': return '#10B981';
      default: return '#6B7280';
  private getIconForType(stepType: string): string {
    const config = AGENT_STEP_TYPE_CONFIGS.find(c => c.Type === stepType);
    return config?.Icon ?? 'fa-circle-nodes';
  private buildLoopSubtitle(step: MJAIAgentStepEntity, prefix: string): string {
    if (!bodyType) return `${prefix} (no body type)`;
    const bodyName = this.resolveLoopBodyName(step);
    return bodyName ? `${prefix} → ${bodyName}` : `${prefix} → ${bodyType}`;
  /** Resolve the display name for the loop body operation */
  private resolveLoopBodyName(step: MJAIAgentStepEntity): string | null {
    switch (step.LoopBodyType) {
      case 'Action': return step.Action ?? null;
      case 'Prompt': return step.Prompt ?? null;
      case 'Sub-Agent': return step.SubAgent ?? null;
  /** Parse the Configuration JSON, returning null on failure */
  private parseLoopConfig(step: MJAIAgentStepEntity): Record<string, unknown> | null {
    if (!step.Configuration) return null;
      return JSON.parse(step.Configuration) as Record<string, unknown>;
  /** Build a short iteration summary for display on loop nodes */
  BuildLoopIterationSummary(step: MJAIAgentStepEntity): string {
    if (step.StepType === 'ForEach') {
      const collection = config?.['collectionPath'] as string | undefined;
      return collection ? `over ${collection}` : 'over collection';
    if (step.StepType === 'While') {
      const condition = config?.['condition'] as string | undefined;
      return condition ? `while ${this.truncateCondition(condition)}` : 'while condition';
  private truncateCondition(condition: string): string {
    const maxLen = 30;
    if (condition.length <= maxLen) return condition;
    return condition.substring(0, maxLen) + '...';
