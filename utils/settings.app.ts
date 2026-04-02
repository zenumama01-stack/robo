export class SettingsApp implements IApp {
  Id = 'settings';
  Name = 'Settings';
  Icon = 'fa-solid fa-gear';
  Route = '/settings';
  Color = '#616161'; // Gray - neutral, utility
  private currentRoute = '';
    // Track route changes to update breadcrumbs
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.currentRoute = event.urlAfterRedirects;
    return 'breadcrumb';
    return []; // Not used for breadcrumb navigation
    const breadcrumbs: Breadcrumb[] = [];
    if (this.currentRoute.includes('/settings/profile')) {
      breadcrumbs.push(
        { Label: 'User Preferences', Route: '/settings' },
        { Label: 'Profile', Route: undefined }
    } else if (this.currentRoute.includes('/settings/notifications')) {
        { Label: 'Notifications', Route: undefined }
    } else if (this.currentRoute.includes('/settings/appearance')) {
        { Label: 'Appearance', Route: undefined }
    } else if (this.currentRoute === '/settings' || this.currentRoute === '/settings/') {
      breadcrumbs.push({ Label: 'User Preferences', Route: undefined });
    return breadcrumbs;
    return false; // Use default search
    // Not implemented
    console.log('Settings handling route:', segments);
