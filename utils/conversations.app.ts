import { IApp, NavItem, Breadcrumb } from '../../core/models/app.interface';
import { ShellService } from '../../core/services/shell.service';
export class ConversationsApp implements IApp {
  Id = 'conversations';
  Name = 'Conversations';
  Icon = 'fa-solid fa-comments';
  Route = '/conversations';
  Color = '#1976d2'; // Blue - communication, trust
  private shellService!: ShellService;
  constructor(private router: Router) {}
  Initialize(shellService: ShellService): void {
    this.shellService = shellService;
  GetNavigationType(): 'list' | 'breadcrumb' {
    return 'list';
  GetNavItems(): NavItem[] {
        Label: 'Chat',
        Route: '/conversations/chat',
        Icon: 'fa-solid fa-message'
        Label: 'Collections',
        Route: '/conversations/collections',
        Icon: 'fa-solid fa-folder-open',
        Badge: 3
        Label: 'Tasks',
        Route: '/conversations/tasks',
        Icon: 'fa-solid fa-check-square',
        Badge: 12
  GetBreadcrumbs(): Breadcrumb[] {
    return []; // Not used for list navigation
  CanHandleSearch(): boolean {
  OnSearchRequested(query: string): void {
    console.log('Conversations app searching for:', query);
    // Custom search within conversations
  RequestNewTab(title: string, route: string, data?: any): void {
    if (this.shellService) {
        AppId: this.Id,
        Title: title,
        Route: route,
  HandleRoute(segments: string[]): void {
    // App handles its own internal routing
    console.log('Conversations handling route:', segments);
