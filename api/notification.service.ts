import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject, Observable, Subject, fromEvent } from 'rxjs';
import { filter, map, shareReplay } from 'rxjs/operators';
  NotificationItem,
  NotificationPreferences,
  NotificationState,
  NotificationChangeEvent,
} from '../models/notification.model';
 * Service for managing notifications across conversations
 * Provides real-time notification tracking, persistence, and cross-tab synchronization
export class NotificationService {
  private readonly STORAGE_KEY = 'mj_conversation_notifications';
  private readonly STORAGE_EVENT_KEY = 'mj_notification_change';
  private _notifications$ = new BehaviorSubject<Record<string, ConversationNotification>>({});
  private _preferences$ = new BehaviorSubject<NotificationPreferences>(DEFAULT_NOTIFICATION_PREFERENCES);
  private _notificationItems$ = new BehaviorSubject<NotificationItem[]>([]);
  private _changeEvents$ = new Subject<NotificationChangeEvent>();
  public readonly notifications$ = this._notifications$.asObservable();
  public readonly preferences$ = this._preferences$.asObservable();
  public readonly notificationItems$ = this._notificationItems$.asObservable();
  public readonly changeEvents$ = this._changeEvents$.asObservable();
  // Derived observables
  public readonly totalUnreadCount$: Observable<number> = this.notifications$.pipe(
    map(notifications => {
      return Object.values(notifications).reduce(
        (sum, notif) => sum + notif.unreadMessageCount,
  public readonly hasAnyNotifications$: Observable<boolean> = this.totalUnreadCount$.pipe(
    map(count => count > 0),
  constructor(private ngZone: NgZone) {
    this.setupStorageListener();
   * Gets notification state for a specific conversation
  getConversationNotification(conversationId: string): ConversationNotification | null {
    return this._notifications$.value[conversationId] || null;
   * Gets notification observable for a specific conversation
  getConversationNotification$(conversationId: string): Observable<ConversationNotification | null> {
    return this.notifications$.pipe(
      map(notifications => notifications[conversationId] || null),
   * Gets badge configuration for a conversation
  getBadgeConfig(conversationId: string): BadgeConfig {
    const notification = this.getConversationNotification(conversationId);
    if (!notification) {
      return { show: false };
    const prefs = this._preferences$.value;
    if (!prefs.showBadges || prefs.mutedConversations.includes(conversationId)) {
    // Check if muted temporarily
    if (prefs.muteUntil && new Date() < prefs.muteUntil) {
    const totalNotifications =
      notification.unreadMessageCount +
      notification.newArtifactCount +
      notification.activeAgentProcessCount;
    if (totalNotifications === 0) {
    // Determine badge type based on notification content
    let type: BadgeConfig['type'] = 'count';
    let animate = false;
    if (notification.activeAgentProcessCount > 0) {
      type = 'pulse';
      animate = true;
    } else if (notification.newArtifactCount > 0) {
      type = 'new';
      count: totalNotifications,
      priority: notification.highestPriority,
      animate
   * Gets badge configuration observable for a conversation
  getBadgeConfig$(conversationId: string): Observable<BadgeConfig> {
      map(() => this.getBadgeConfig(conversationId)),
   * Tracks a new message in a conversation
  trackNewMessage(conversationId: string, messageTimestamp: Date, priority: NotificationPriority = 'normal'): void {
    const notifications = { ...this._notifications$.value };
    const existing = notifications[conversationId];
      notifications[conversationId] = {
        unreadMessageCount: existing.unreadMessageCount + 1,
        lastMessageTimestamp: messageTimestamp,
        highestPriority: this.getHigherPriority(existing.highestPriority, priority)
      notifications[conversationId] = this.createNewNotification(conversationId, {
        unreadMessageCount: 1,
        highestPriority: priority
    this.updateNotifications(notifications);
    this.emitChangeEvent(conversationId, 'message', 'added');
    this.playNotificationSound();
   * Tracks multiple new messages at once (batch operation)
  trackNewMessages(conversationId: string, count: number, latestTimestamp: Date, priority: NotificationPriority = 'normal'): void {
        unreadMessageCount: existing.unreadMessageCount + count,
        lastMessageTimestamp: latestTimestamp,
        unreadMessageCount: count,
   * Tracks a new artifact notification
  trackNewArtifact(conversationId: string): void {
        newArtifactCount: existing.newArtifactCount + 1,
        lastNotificationTimestamp: new Date()
        newArtifactCount: 1
    this.emitChangeEvent(conversationId, 'artifact', 'added');
   * Tracks an active agent process
  trackAgentProcess(conversationId: string, isActive: boolean): void {
      const countChange = isActive ? 1 : -1;
        hasActiveAgentProcesses: isActive ? true : (existing.activeAgentProcessCount - 1) > 0,
        activeAgentProcessCount: Math.max(0, existing.activeAgentProcessCount + countChange),
    } else if (isActive) {
        hasActiveAgentProcesses: true,
        activeAgentProcessCount: 1
    this.emitChangeEvent(conversationId, 'agent_process', isActive ? 'added' : 'cleared');
   * Marks a conversation as read (clears unread message count)
  markConversationAsRead(conversationId: string): void {
    if (existing && existing.unreadMessageCount > 0) {
        unreadMessageCount: 0,
        lastReadMessageTimestamp: new Date()
      this.emitChangeEvent(conversationId, 'message', 'read');
   * Clears artifact notifications for a conversation
  clearArtifactNotifications(conversationId: string): void {
    if (existing && (existing.hasNewArtifacts || existing.newArtifactCount > 0)) {
        hasNewArtifacts: false,
        newArtifactCount: 0
      this.emitChangeEvent(conversationId, 'artifact', 'cleared');
   * Clears all notifications for a conversation
  clearAllNotifications(conversationId: string): void {
    if (notifications[conversationId]) {
      delete notifications[conversationId];
      this.emitChangeEvent(conversationId, 'message', 'cleared');
   * Clears all notifications across all conversations
  clearAllNotificationsGlobal(): void {
    this.updateNotifications({});
   * Updates notification preferences
  updatePreferences(preferences: Partial<NotificationPreferences>): void {
    const updated = { ...current, ...preferences };
   * Mutes a conversation
  muteConversation(conversationId: string): void {
    if (!prefs.mutedConversations.includes(conversationId)) {
      this.updatePreferences({
        mutedConversations: [...prefs.mutedConversations, conversationId]
   * Unmutes a conversation
  unmuteConversation(conversationId: string): void {
      mutedConversations: prefs.mutedConversations.filter(id => id !== conversationId)
   * Checks if a conversation is muted
  isConversationMuted(conversationId: string): boolean {
    return this._preferences$.value.mutedConversations.includes(conversationId);
   * Requests desktop notification permission
  async requestDesktopPermission(): Promise<boolean> {
    if (!('Notification' in window)) {
      console.warn('Desktop notifications not supported');
    if (Notification.permission === 'granted') {
    if (Notification.permission !== 'denied') {
      const permission = await Notification.requestPermission();
      return permission === 'granted';
   * Shows a desktop notification
  showDesktopNotification(title: string, body: string, conversationId: string): void {
    if (!this._preferences$.value.enableDesktopNotifications) {
    if ('Notification' in window && Notification.permission === 'granted') {
      const notification = new Notification(title, {
        icon: '/assets/icons/notification-icon.png',
        badge: '/assets/icons/badge-icon.png',
        tag: conversationId,
        requireInteraction: false
      notification.onclick = () => {
        window.focus();
        notification.close();
  // Private helper methods
  private createNewNotification(
    overrides?: Partial<ConversationNotification>
  ): ConversationNotification {
      lastMessageTimestamp: null,
      newArtifactCount: 0,
      highestPriority: 'normal',
  private updateNotifications(notifications: Record<string, ConversationNotification>): void {
    this._notifications$.next(notifications);
    this.broadcastChange();
  private emitChangeEvent(conversationId: string, type: NotificationType, action: 'added' | 'read' | 'cleared'): void {
    this._changeEvents$.next({
  private getHigherPriority(current: NotificationPriority, newPriority: NotificationPriority): NotificationPriority {
    const priorityOrder: NotificationPriority[] = ['low', 'normal', 'high', 'urgent'];
    const currentIndex = priorityOrder.indexOf(current);
    const newIndex = priorityOrder.indexOf(newPriority);
    return newIndex > currentIndex ? newPriority : current;
  private playNotificationSound(): void {
    if (!this._preferences$.value.enableSound) {
    // Create and play a subtle notification sound
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
    const oscillator = audioContext.createOscillator();
    const gainNode = audioContext.createGain();
    oscillator.connect(gainNode);
    gainNode.connect(audioContext.destination);
    oscillator.frequency.value = 800;
    oscillator.type = 'sine';
    gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);
    oscillator.start(audioContext.currentTime);
    oscillator.stop(audioContext.currentTime + 0.1);
  private loadFromStorage(): void {
      const stored = localStorage.getItem(this.STORAGE_KEY);
        const state: NotificationState = JSON.parse(stored);
        // Restore notifications
        this._notifications$.next(state.conversations || {});
        // Restore preferences
          ...DEFAULT_NOTIFICATION_PREFERENCES,
          ...state.preferences
      console.error('Error loading notification state from storage:', error);
  private saveToStorage(): void {
      const state: NotificationState = {
        conversations: this._notifications$.value,
        preferences: this._preferences$.value,
        lastUpdated: new Date()
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
      console.error('Error saving notification state to storage:', error);
  private broadcastChange(): void {
      // Broadcast to other tabs via storage event
        data: this._notifications$.value
      localStorage.setItem(this.STORAGE_EVENT_KEY, JSON.stringify(event));
      console.error('Error broadcasting notification change:', error);
  private setupStorageListener(): void {
    // Listen for changes from other tabs
      fromEvent<StorageEvent>(window, 'storage')
          filter(event => event.key === this.STORAGE_KEY || event.key === this.STORAGE_EVENT_KEY)
