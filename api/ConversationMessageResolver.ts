 * @fileoverview Utility for resolving conversation message references in agent mappings.
 * Provides flexible syntax for accessing conversation messages in action input mappings
 * and sub-agent downstream mappings. Supports filtering by role, selecting by position,
 * and extracting specific properties.
import { ChatMessage } from '@memberjunction/ai';
 * Utility class for resolving conversation message references.
 * Supports a flexible syntax for accessing conversation messages:
 * - `conversation.all` - All messages
 * - `conversation.user.last` - Last user message
 * - `conversation.ai.first` - First assistant message
 * - `conversation.all.last[3]` - Last 3 messages
 * - `conversation.user.last[5]` - Last 5 user messages
 * - `conversation.user.last.content` - Content of last user message
 * const messages = [
 *   { role: 'user', content: 'Hello' },
 *   { role: 'assistant', content: 'Hi there!' },
 *   { role: 'user', content: 'How are you?' }
 * ];
 * // Get last user message
 * const last = ConversationMessageResolver.resolve('conversation.user.last', messages);
 * // Returns: { role: 'user', content: 'How are you?' }
 * // Get last 2 messages
 * const lastTwo = ConversationMessageResolver.resolve('conversation.all.last[2]', messages);
 * // Returns: [{ role: 'assistant', ... }, { role: 'user', ... }]
 * // Get content of last user message
 * const content = ConversationMessageResolver.resolve('conversation.user.last.content', messages);
 * // Returns: 'How are you?'
export class ConversationMessageResolver {
     * Resolves conversation message references.
     * Syntax: `conversation.<role>.<position>[count].<property>`
     * - `<role>`: all, user, assistant (or ai as alias), system
     * - `<position>`: last, first
     * - `[count]`: Optional number in brackets - returns that many messages
     * - `<property>`: Optional property path to extract from message(s)
     * @param conversationPath - The conversation reference path to resolve
     * @param conversationMessages - Array of chat messages to resolve from
     * @returns Resolved message(s) or property value(s), or undefined if invalid path
    public static resolve(
        conversationPath: string,
        conversationMessages: ChatMessage[]
    ): ChatMessage | ChatMessage[] | any | undefined {
        // Parse: conversation.<role>.<position>[count].<property>
        const parts = conversationPath.split('.');
        if (parts[0]?.toLowerCase() !== 'conversation') {
        // Extract role (all, user, assistant/ai, system)
        const roleSpec = parts[1]?.toLowerCase();
        if (!roleSpec) {
        // If just "conversation.all", return all messages
        if (roleSpec === 'all' && parts.length === 2) {
            return conversationMessages;
        // Filter by role
        const roleFilter = this.getRoleFilter(roleSpec);
        const filteredMessages = roleFilter
            ? conversationMessages.filter(roleFilter)
            : conversationMessages;
        // Extract position and count
        const positionSpec = parts[2];
        if (!positionSpec) {
            // Just "conversation.user" - return all of that role
            return filteredMessages;
        // Parse position and optional count: "last[3]" or "first[2]"
        const match = positionSpec.match(/^(first|last)(?:\[(\d+)\])?$/i);
        if (!match) {
            // Not a position spec, might be a property on the array
            // e.g., "conversation.all.length"
            return this.getValueFromPath(filteredMessages, positionSpec);
        const position = match[1].toLowerCase();
        const count = match[2] ? parseInt(match[2], 10) : 1;
        // Get messages based on position
        let result: ChatMessage | ChatMessage[] | undefined;
        if (position === 'last') {
            const selected = filteredMessages.slice(-count);
            result = count === 1 ? selected[0] : selected;
        } else if (position === 'first') {
            const selected = filteredMessages.slice(0, count);
        if (result === undefined) {
        // Check if there's a property path to extract
        if (parts.length > 3) {
            const propertyPath = parts.slice(3).join('.');
            if (Array.isArray(result)) {
                // Map property over array
                return result.map(item => this.getValueFromPath(item, propertyPath));
                // Get property from single object
                return this.getValueFromPath(result, propertyPath);
     * Gets the role filter function for a given role specification.
     * @param roleSpec - Role specification (all, user, assistant, ai, system)
     * @returns Filter function or null for 'all'
    private static getRoleFilter(roleSpec: string): ((msg: ChatMessage) => boolean) | null {
        switch (roleSpec) {
                return null; // No filtering
            case 'user':
                return (msg) => msg.role === 'user';
            case 'assistant':
            case 'ai':
                return (msg) => msg.role === 'assistant';
            case 'system':
                return (msg) => msg.role === 'system';
     * Checks if a mapping value is a conversation reference.
     * @param value - The value to check
     * @returns True if the value is a conversation reference
    public static isConversationReference(value: string): boolean {
        return typeof value === 'string' &&
               value.trim().toLowerCase().startsWith('conversation.');
     * @param obj - The object to extract value from
     * @param path - The property path (e.g., "user.name" or "items[0].title")
     * @returns The value at the path, or undefined if not found
    private static getValueFromPath(obj: any, path: string): any {
