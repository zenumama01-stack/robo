interface CredentialsDashboardState {
    selector: 'mj-credentials-dashboard',
    templateUrl: './credentials-dashboard.component.html',
    styleUrls: ['./credentials-dashboard.component.css'],
@RegisterClass(BaseDashboard, 'CredentialsDashboard')
export class CredentialsDashboardComponent extends BaseDashboard implements AfterViewInit, OnDestroy {
    public activeTab = 'overview';
    // Counts for badges
    public credentialCount = 0;
    public typeCount = 0;
    // Track visited tabs for lazy loading
    // Navigation items
    public navigationItems: string[] = ['overview', 'credentials', 'types', 'categories', 'audit'];
    public tabLabels: Record<string, string> = {
        'overview': 'Overview',
        'credentials': 'Credentials',
        'types': 'Credential Types',
        'categories': 'Categories',
        'audit': 'Audit Trail'
    private stateChangeSubject = new Subject<CredentialsDashboardState>();
        return "Credentials";
        this.loadCounts();
    private async loadCounts(): Promise<void> {
            // Load credential count
            const credResult = await rv.RunView({
            if (credResult.Success) {
                this.credentialCount = credResult.RowCount;
            // Load type count
                EntityName: 'MJ: Credential Types',
            if (typeResult.Success) {
                this.typeCount = typeResult.RowCount;
            console.error('Error loading counts:', error);
        const state: CredentialsDashboardState = {
    public loadUserState(state: Partial<CredentialsDashboardState>): void {
            console.error('Error initializing Credentials dashboard:', error);
            this.Error.emit(new Error('Failed to initialize Credentials dashboard. Please try again.'));
        return this.tabLabels[this.activeTab] || 'Credential Management';
