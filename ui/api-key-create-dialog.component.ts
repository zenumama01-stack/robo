import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';
import { GraphQLDataProvider, GraphQLEncryptionClient } from '@memberjunction/graphql-dataprovider';
/** Scope selection item */
interface ScopeItem {
/** Grouped scopes by category */
    scopes: ScopeItem[];
/** Result of key creation */
export interface APIKeyCreateResult {
    rawKey?: string;
 * Dialog for creating new API keys
 * Shows the raw key only once - it cannot be recovered after closing
    selector: 'mj-api-key-create-dialog',
    templateUrl: './api-key-create-dialog.component.html',
    styleUrls: ['./api-key-create-dialog.component.css']
export class APIKeyCreateDialogComponent implements OnInit {
    @Input() Visible = false;
    @Output() VisibleChange = new EventEmitter<boolean>();
    @Output() Created = new EventEmitter<APIKeyCreateResult>();
    @Output() Closed = new EventEmitter<void>();
    // Form fields
    public Label = '';
    public Description = '';
    public ExpiresAt: Date | null = null;
    public NeverExpires = true;
    // Expiration presets
    public ExpirationPresets = [
        { label: '30 days', days: 30 },
        { label: '90 days', days: 90 },
        { label: '1 year', days: 365 },
        { label: 'Custom', days: -1 }
    public SelectedPreset: { label: string; days: number } | null = null;
    // Scopes
    public IsLoadingScopes = true;
    public IsCreating = false;
    public Step: 'configure' | 'scopes' | 'success' = 'configure';
    public RawApiKey = '';
    public KeyCopied = false;
    public Error = '';
        this.loadScopes();
     * Handle escape key to close dialog
    @HostListener('document:keydown.escape')
    public onEscapeKey(): void {
        if (this.Visible && !this.IsCreating) {
     * Load available scopes from cached data.
    private loadScopes(): void {
        this.IsLoadingScopes = true;
            const scopes = base.Scopes;
                    // This is a root scope - use its UIConfig for the category
            // Group scopes by category
            const categoryMap = new Map<string, ScopeItem[]>();
                const category = scope.Category || 'Other';
                categoryMap.get(category)!.push({
                    selected: false
            // Build categories with UI config from root scopes
            this.ScopeCategories = Array.from(categoryMap.entries())
                .sort(([a], [b]) => a.localeCompare(b))
                .map(([name, scopeItems]) => {
                        scopes: scopeItems.sort((a, b) => a.scope.Name.localeCompare(b.scope.Name)),
                        allSelected: false
            console.error('Error loading scopes:', error);
            this.IsLoadingScopes = false;
     * Handle expiration preset selection
    public onPresetSelect(preset: { label: string; days: number }): void {
        this.SelectedPreset = preset;
        this.NeverExpires = false;
        if (preset.days > 0) {
            const date = new Date();
            date.setDate(date.getDate() + preset.days);
            this.ExpiresAt = date;
            this.ExpiresAt = null;
     * Handle never expires toggle
    public onNeverExpiresChange(): void {
        if (this.NeverExpires) {
            this.SelectedPreset = null;
    public toggleCategory(category: ScopeCategory): void {
        category.allSelected = !category.allSelected;
        for (const item of category.scopes) {
            item.selected = category.allSelected;
     * Update category allSelected state
        category.allSelected = category.scopes.every(s => s.selected);
     * Get selected scope count
        return this.ScopeCategories.reduce((sum, cat) =>
            sum + cat.scopes.filter(s => s.selected).length, 0);
     * Proceed to scopes step
    public goToScopes(): void {
        if (!this.Label.trim()) {
            this.Error = 'Please enter a label for the API key';
        this.Error = '';
        this.Step = 'scopes';
     * Go back to configure step
    public goBack(): void {
        this.Step = 'configure';
     * Create the API key using server-side cryptographic hashing
    public async createKey(): Promise<void> {
        this.IsCreating = true;
            // Get selected scope IDs
            const selectedScopeIds = this.ScopeCategories
                .flatMap(cat => cat.scopes)
                .filter(s => s.selected)
                .map(s => s.scope.ID);
            // Get the GraphQL provider and create the encryption client
            const provider = Metadata.Provider as GraphQLDataProvider;
            const encryptionClient = new GraphQLEncryptionClient(provider);
            // Call the server to create the API key with proper crypto hashing
            const result = await encryptionClient.CreateAPIKey({
                Label: this.Label.trim(),
                Description: this.Description.trim() || undefined,
                ExpiresAt: this.NeverExpires ? undefined : (this.ExpiresAt || undefined),
                ScopeIDs: selectedScopeIds.length > 0 ? selectedScopeIds : undefined
            if (result.Success && result.RawKey) {
                this.RawApiKey = result.RawKey;
                this.Step = 'success';
                this.Created.emit({
                    rawKey: result.RawKey
                this.Error = result.Error || 'Failed to create API key. Please try again.';
            console.error('Error creating API key:', error);
            this.Error = 'An error occurred while creating the API key.';
            this.IsCreating = false;
     * Copy the API key to clipboard
    public async copyKey(): Promise<void> {
            await navigator.clipboard.writeText(this.RawApiKey);
            this.KeyCopied = true;
            setTimeout(() => this.KeyCopied = false, 3000);
            console.error('Failed to copy key:', error);
     * Close the dialog
    public close(): void {
        this.Visible = false;
        this.VisibleChange.emit(false);
        this.Closed.emit();
     * Reset form state
    private reset(): void {
        this.Label = '';
        this.Description = '';
        this.NeverExpires = true;
        this.RawApiKey = '';
        this.KeyCopied = false;
        // Reset scope selections
        for (const category of this.ScopeCategories) {
            category.expanded = false;
            category.allSelected = false;
                scope.selected = false;
     * Get minimum date for expiration (tomorrow)
    public getMinDate(): Date {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        return tomorrow;
