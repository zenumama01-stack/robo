import { ViewContainerRef } from '@angular/core';
 * Defines a single menu item in the user context menu
export interface UserMenuItem {
    /** Unique identifier for the menu item */
    /** Display text for the menu item */
    /** Font Awesome icon class (e.g., 'fa-solid fa-gear') */
    /** Optional icon/text color (CSS color value) */
    /** Optional CSS class for custom styling */
    cssClass?: string;
    /** Group ID for organizing items (items with same group have dividers between groups) */
    group: 'primary' | 'developer' | 'system' | 'danger' | string;
    /** Sort order within group (lower = higher in menu) */
    /** Whether item requires Developer role to be visible */
    developerOnly: boolean;
    /** Whether the item is currently visible (dynamic control) */
    /** Whether the item is currently enabled/clickable */
    /** Optional tooltip text */
    tooltip?: string;
    /** Optional keyboard shortcut hint */
    shortcut?: string;
 * Defines a separator/divider in the menu
export interface UserMenuDivider {
    type: 'divider';
    group: string;
 * Union type for all menu element types
export type UserMenuElement = UserMenuItem | UserMenuDivider;
 * Type guard to check if element is a divider
export function isUserMenuDivider(element: UserMenuElement): element is UserMenuDivider {
    return 'type' in element && element.type === 'divider';
 * Type guard to check if element is a menu item
export function isUserMenuItem(element: UserMenuElement): element is UserMenuItem {
    return !('type' in element);
 * Minimal application info needed by user menu
export interface ApplicationInfoRef {
 * Interface for workspace manager operations needed by user menu
export interface WorkspaceManagerRef {
    GetConfiguration: () => unknown;
 * Interface for auth service operations needed by user menu
export interface AuthServiceRef {
    logout: () => void | Promise<void>;
 * Context passed to menu handlers
export interface UserMenuContext {
    /** Current authenticated user info */
    /** Full user entity with all fields */
    userEntity: MJUserEntity;
    /** Reference to shell component for advanced operations */
    shell: Record<string, unknown>;
    /** View container for opening dialogs/modals */
    viewContainerRef: ViewContainerRef;
    /** Whether user has Developer role */
    isDeveloper: boolean;
    /** Whether developer mode is currently enabled (can be toggled) */
    developerModeEnabled: boolean;
    /** Current application context */
    currentApplication: ApplicationInfoRef | null;
    /** Workspace manager for layout operations */
    workspaceManager: WorkspaceManagerRef;
    /** Auth service for logout operations */
    authService: AuthServiceRef;
    /** Function to open settings dialog */
    openSettings: () => void;
 * Result of a menu item click handler
export interface UserMenuActionResult {
    /** Whether the action succeeded */
    /** Whether to close the menu after action */
    closeMenu: boolean;
    /** Optional message to display (toast notification) */
 * Options for menu configuration
export interface UserMenuOptions {
    /** Whether to show the user name in menu header */
    showUserName: boolean;
    /** Whether to show the user email in menu header */
    showUserEmail: boolean;
    /** Menu position relative to avatar */
    menuPosition: 'below-left' | 'below-right';
    /** Animation style for menu opening */
    animationStyle: 'fade' | 'slide' | 'none';
