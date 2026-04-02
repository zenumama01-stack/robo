    AfterViewInit
import { PanelConfig, DashboardPanel } from '../../models/dashboard-types';
 * Result from the edit part dialog
export interface EditPartDialogResult {
    /** The updated panel configuration */
    /** Updated title */
    /** Updated icon (optional) */
 * Generic dialog for editing dashboard part configuration.
 * This dialog dynamically loads the appropriate config panel based on the
 * DashboardPartType.ConfigDialogClass field, using MJGlobal.ClassFactory.
 * This allows new part types to be added without modifying this dialog -
 * just create a config panel, register it, and set the ConfigDialogClass.
    selector: 'mj-edit-part-dialog',
    templateUrl: './edit-part-dialog.component.html',
    styleUrls: ['./edit-part-dialog.component.css']
export class EditPartDialogComponent implements OnDestroy, AfterViewInit {
     * Whether the dialog is visible
    set Visible(value: boolean) {
        if (value && !previous) {
            this.onDialogOpened();
            // Delay loadConfigPanel to next tick to ensure all inputs are set
            // Angular doesn't guarantee input order, so PartType might not be set yet
            if (this.viewInitialized) {
        } else if (!value && previous) {
            this.onDialogClosed();
     * The part type being configured
    @Input() PartType: MJDashboardPartTypeEntity | null = null;
     * The panel being edited
    @Input() Panel: DashboardPanel | null = null;
     * The current configuration
    @Input() Config: PanelConfig | null = null;
     * Emitted when configuration is saved
    @Output() Saved = new EventEmitter<EditPartDialogResult>();
     * Emitted when dialog is cancelled
     * Container for dynamically loaded config panel
     * Reference to the dynamically created config panel component
     * Track validity from the embedded panel
    public IsValid = false;
     * Store the latest result from the panel
    private latestResult: ConfigPanelResult | null = null;
     * Error message if panel failed to load
    public LoadError: string | null = null;
     * Whether the panel is loading
            this.loadConfigPanel();
     * Called when dialog opens
    private onDialogOpened(): void {
        this.IsValid = false;
        this.latestResult = null;
        this.LoadError = null;
     * Called when dialog closes
    private onDialogClosed(): void {
        this.LoadError = null; // Clear any previous error
        console.log('[EditPartDialog] loadConfigPanel() called');
        console.log('[EditPartDialog] PartType:', this.PartType?.Name);
        console.log('[EditPartDialog] ConfigDialogClass:', this.PartType?.ConfigDialogClass);
        if (!this.PartType?.ConfigDialogClass) {
            // No config class specified - this is okay for simple part types
            // Just enable the save button with default config
            console.log('[EditPartDialog] No ConfigDialogClass specified, using default');
            this.IsValid = true;
        if (!this.configPanelContainer) {
            // Container not ready yet - will be called again from ngAfterViewInit
            console.log('[EditPartDialog] Container not ready yet');
            console.log('[EditPartDialog] Attempting to create instance via ClassFactory:', this.PartType.ConfigDialogClass);
                this.PartType.ConfigDialogClass
            console.log('[EditPartDialog] ClassFactory returned:', panelInstance);
                this.LoadError = `Could not create config panel: ${this.PartType.ConfigDialogClass}`;
                console.error('[EditPartDialog]', this.LoadError);
                this.IsValid = true; // Allow saving with default config
            // We need to find the Angular component class that was registered
            console.log('[EditPartDialog] Component class:', componentClass.name);
            console.log('[EditPartDialog] Component created successfully');
            panel.partType = this.PartType;
            panel.panel = this.Panel;
            panel.config = this.Config;
            this.LoadError = null; // Ensure error is cleared on success
            this.LoadError = `Failed to load config panel: ${error instanceof Error ? error.message : String(error)}`;
            console.error('[EditPartDialog]', this.LoadError, error);
     * Handle configuration changes from the embedded panel
        this.latestResult = result;
        this.IsValid = result.isValid;
     * Get the dialog title based on part type
    public getDialogTitle(): string {
        const partName = this.PartType?.Name || 'Part';
        return this.Panel ? `Configure ${partName}` : `Add ${partName}`;
     * Get the part type icon
        return this.PartType?.Icon || 'fa-solid fa-puzzle-piece';
     * Save the configuration
    public save(): void {
        if (!this.IsValid || !this.latestResult) {
        this.Saved.emit({
            Config: this.latestResult.config,
            Title: this.latestResult.title,
            Icon: this.latestResult.icon
     * Get save button text
    public getSaveButtonText(): string {
        return this.Panel ? 'Save Changes' : 'Add Part';
