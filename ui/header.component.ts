import { Component, OnInit, HostListener } from '@angular/core';
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
export class HeaderComponent implements OnInit {
  activeApp: IApp | null = null;
  navItems: NavItem[] = [];
  breadcrumbs: Breadcrumb[] = [];
  navigationType: 'list' | 'breadcrumb' | null = null;
  showUserMenu = false;
  showAppSwitcher = false; // TODO: Implement app switcher dropdown
  constructor(private shellService: ShellService) {}
    this.shellService.GetActiveApp().subscribe(app => {
        this.navigationType = app.GetNavigationType();
        if (this.navigationType === 'list') {
          this.navItems = app.GetNavItems();
          this.breadcrumbs = app.GetBreadcrumbs();
          this.navItems = [];
  OnSearch(): void {
      this.shellService.HandleSearch(this.searchQuery);
  ToggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  OnNavigate(route: string): void {
    this.shellService.Navigate(route);
  isNavItemActive(route: string): boolean {
    // Check if there's an active tab with this route
    const activeTabs = this.shellService['tabs$'].value;
    const activeTabId = this.shellService['activeTabId$'].value;
    const activeTab = activeTabs.find(t => t.Id === activeTabId);
    return activeTab?.Route === route;
  ToggleAppSwitcher(): void {
    this.showAppSwitcher = !this.showAppSwitcher;
    const apps = this.GetAllApps();
    console.log('App switcher toggled:', this.showAppSwitcher, 'Apps:', apps.map(a => a.Name));
    return this.shellService.GetAllApps();
  SwitchToApp(app: IApp): void {
    this.showAppSwitcher = false;
    // Don't do anything if already on this app
    if (this.activeApp?.Id === app.Id) {
    // SetActiveApp now handles opening default tab if needed
    this.shellService.SetActiveApp(app.Id);
  OnClickOutside(event: MouseEvent): void {
    const clickedInside = target.closest('.app-switcher-container');
    if (!clickedInside && this.showAppSwitcher) {
    // Also close user menu if clicked outside
    const clickedUserMenu = target.closest('.user-menu');
    if (!clickedUserMenu && this.showUserMenu) {
      this.showUserMenu = false;
