 * Core interfaces for the shell/app architecture
export interface IApp {
  Color: string; // App signature color for visual identification
  // Navigation style
  GetNavigationType(): 'list' | 'breadcrumb';
  // List navigation (tabs/options)
  GetNavItems(): NavItem[];
  // Breadcrumb navigation
  GetBreadcrumbs(): Breadcrumb[];
  // Chrome extension points
  CanHandleSearch(): boolean;
  OnSearchRequested(query: string): void;
  // Tab management
  RequestNewTab(title: string, route: string, data?: any): void;
  HandleRoute(segments: string[]): void;
  Badge?: number;
export interface Breadcrumb {
  Route?: string; // null/undefined = current page (not clickable)
  AppId: string;
  Data?: any;
export interface TabState {
  IsPermanent?: boolean; // VSCode-style: false = temporary (gets replaced), true = permanent (stays)
  Color?: string; // App signature color for visual identification
