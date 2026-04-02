import { Mention, MentionParseResult } from '../models/conversation-state.model';
import { AIAgentEntityExtended, ConversationUtility } from '@memberjunction/ai-core-plus';
 * Service for parsing @mentions from message text
 * Supports both JSON format and legacy text format
export class MentionParserService {
  // Regex to match JSON mentions: @{type:"agent",id:"uuid",name:"Name",configId:"uuid",config:"High"}
  private readonly JSON_MENTION_REGEX = /@\{[^}]+\}/g;
  // Regex to match legacy @mentions - supports names with spaces if quoted: @"Agent Name" or @AgentName
  private readonly LEGACY_MENTION_REGEX = /@"([^"]+)"|@(\S+)/g;
   * Parse mentions from message text
   * @param text The message text to parse
   * @param availableAgents List of available agents for matching
   * @param availableUsers List of available users for matching (optional)
   * @returns Parsed mentions with agent and user separation
  parseMentions(
    availableAgents: AIAgentEntityExtended[],
    availableUsers?: UserInfo[]
  ): MentionParseResult {
    const mentions: Mention[] = [];
    // First, try to parse JSON mentions (new format)
    const jsonMatches = Array.from(text.matchAll(this.JSON_MENTION_REGEX));
    for (const match of jsonMatches) {
        // Extract JSON string (remove @ prefix)
        const jsonStr = match[0].substring(1); // Remove '@'
        const mentionData = JSON.parse(jsonStr);
        if (mentionData.type && mentionData.id && mentionData.name) {
          const mention: Mention = {
            type: mentionData.type,
            id: mentionData.id,
            name: mentionData.name
          // Add configuration if present (for agents)
          if (mentionData.configId) {
            mention.configurationId = mentionData.configId;
          mentions.push(mention);
        console.warn('Failed to parse JSON mention:', match[0], error);
        // Continue to next match
    // If no JSON mentions found, fall back to legacy text format
    if (mentions.length === 0) {
      const legacyMatches = Array.from(text.matchAll(this.LEGACY_MENTION_REGEX));
      for (const match of legacyMatches) {
        // Extract the mention name (either quoted or unquoted)
        const mentionName = match[1] || match[2];
        if (!mentionName) continue;
        // Try to match against agents first
        const agent = this.findAgent(mentionName, availableAgents);
          mentions.push({
            name: agent.Name || 'Unknown'
        // Try to match against users
        if (availableUsers) {
          const user = this.findUser(mentionName, availableUsers);
              name: user.Name
    // Extract first agent mention and all user mentions
    const agentMention = mentions.find(m => m.type === 'agent') || null;
    const userMentions = mentions.filter(m => m.type === 'user');
      mentions,
      agentMention,
      userMentions
   * Find an agent by name (case-insensitive)
   * Uses exact match or starts-with match (no contains match to avoid ambiguity)
  private findAgent(name: string, agents: AIAgentEntityExtended[]): AIAgentEntityExtended | null {
    // Remove trailing punctuation and trim
    const cleanName = name.replace(/[.,;!?]+$/, '').trim();
    const lowerName = cleanName.toLowerCase();
    let agent = agents.find(a => (a.Name?.toLowerCase() || '') === lowerName);
      return agent;
    // Try starts with match
    agent = agents.find(a => (a.Name?.toLowerCase() || '').startsWith(lowerName));
    // Note: Removed "contains" match to avoid ambiguous matches
    // e.g., "@Agent" would match multiple agents like "Marketing Agent", "Data Agent"
    // Future: Could add LLM-based disambiguation as fallback
   * Find a user by name (case-insensitive)
   * Uses exact match, email match, or starts-with match (no contains match to avoid ambiguity)
  private findUser(name: string, users: UserInfo[]): UserInfo | null {
    const lowerName = name.toLowerCase().trim();
    let user = users.find(u => u.Name.toLowerCase() === lowerName);
    if (user) return user;
    // Try email match
    user = users.find(u => u.Email?.toLowerCase() === lowerName);
    user = users.find(u => u.Name.toLowerCase().startsWith(lowerName));
    return user || null;
    // Note: Removed "contains" match for consistency with agent matching
   * Validate mentions - check if all mentions are valid
   * Returns array of invalid mention names
   * Supports both JSON and legacy mention formats
  validateMentions(
    const invalidMentions: string[] = [];
    // Check JSON mentions first
    if (jsonMatches.length > 0) {
          const jsonStr = match[0].substring(1);
          const mentionName = mentionData.name;
          if (mentionData.type === 'agent') {
            const isAgent = this.findAgent(mentionName, availableAgents) !== null;
            if (!isAgent) {
              invalidMentions.push(mentionName);
          } else if (mentionData.type === 'user') {
            const isUser = availableUsers ? this.findUser(mentionName, availableUsers) !== null : false;
            if (!isUser) {
          // Invalid JSON mention
          invalidMentions.push(match[0]);
      // Fall back to legacy format
      const matches = Array.from(text.matchAll(this.LEGACY_MENTION_REGEX));
      for (const match of matches) {
        if (!isAgent && !isUser) {
    return invalidMentions;
   * Extract all mention names from text (raw strings)
  extractMentionNames(text: string): string[] {
      return jsonMatches.map(match => {
          return mentionData.name;
    return matches.map(match => match[1] || match[2]).filter(Boolean);
   * Replace mentions in text with formatted versions
   * Example: "@agent" -> "@Agent Name" (with proper casing)
  formatMentions(
    mentions: Mention[]
    let formattedText = text;
    for (const mention of mentions) {
      // Find the mention in the text and replace with proper name
      const patterns = [
        new RegExp(`@"${mention.name}"`, 'gi'),
        new RegExp(`@${mention.name.replace(/\s+/g, '\\s*')}`, 'gi')
      for (const pattern of patterns) {
        formattedText = formattedText.replace(pattern, `@${mention.name}`);
    return formattedText;
   * Convert a message with JSON-encoded mentions to plain text.
   * Replaces @{...} JSON mentions with simple @Name format.
   * This is a wrapper around ConversationUtility.ToPlainText() for Angular injection.
   * @param text - The message text containing JSON mentions
   * @param agents - Optional array of agents for name lookup
   * @param users - Optional array of users for name lookup
   * @returns Plain text with mentions converted to @Name format
   * // Input: '@{"type":"agent","id":"123","name":"Sage"} help me'
   * // Output: '@Sage help me'
  toPlainText(
    agents?: AIAgentEntityExtended[],
    // Convert agents to the AgentInfo format expected by ConversationUtility
    const agentInfos = agents?.map(a => ({
      Name: a.Name || 'Unknown'
    // Convert users to the UserInfo format expected by ConversationUtility
    const userInfos = users?.map(u => ({
      ID: u.ID,
      Name: u.Name
    return ConversationUtility.ToPlainText(text, agentInfos, userInfos);
