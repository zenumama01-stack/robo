import { Component, Input, Output, EventEmitter, ChangeDetectorRef, ChangeDetectionStrategy, OnInit, OnDestroy, HostListener } from '@angular/core';
import { MJCredentialEntity, MJCredentialTypeEntity, MJCredentialCategoryEntity } from '@memberjunction/core-entities';
    // JSON Schema constraint properties
    enum?: string[];       // List of allowed values
    const?: unknown;           // Fixed immutable value
    default?: unknown;         // Pre-filled value
    format?: string;       // Format validation (uri, email, date, etc.)
    pattern?: string;      // Regex pattern
    minLength?: number;    // Minimum string length
    maxLength?: number;    // Maximum string length
    minimum?: number;      // Minimum numeric value
    maximum?: number;      // Maximum numeric value
interface CredentialValues {
    [key: string]: string | number | boolean;
    selector: 'mj-credential-edit-panel',
    templateUrl: './credential-edit-panel.component.html',
    styleUrls: ['./credential-edit-panel.component.css'],
export class CredentialEditPanelComponent implements OnInit, OnDestroy {
    @Input() credential: MJCredentialEntity | null = null;
    @Input() credentialTypes: MJCredentialTypeEntity[] = [];
    @Output() saved = new EventEmitter<MJCredentialEntity>();
    public selectedTypeId = '';
    public selectedCategoryId = '';
    public isActive = true;
    public isDefault = false;
    public expiresAt: Date | null = null;
    // Dynamic credential values based on type schema
    public credentialValues: CredentialValues = {};
    public schemaFields: FieldSchemaProperty[] = [];
    public showSecretFields: Set<string> = new Set();
    public get selectedType(): MJCredentialTypeEntity | null {
        return this.credentialTypes.find(t => t.ID === this.selectedTypeId) || null;
        return this.isNew ? 'Create Credential' : 'Edit Credential';
        if (!this.name.trim() || !this.selectedTypeId) {
        for (const field of this.schemaFields) {
            if (field.required && !this.credentialValues[field.name]) {
    public async open(credential: MJCredentialEntity | null, preselectedTypeId?: string, preselectedCategoryId?: string): Promise<void> {
        this.credential = credential;
        this.isNew = !credential || !credential.ID;
        if (credential && credential.ID) {
            // Edit mode - populate from existing credential
            this.populateFromCredential(credential);
            // Create mode with optional preselections
            if (preselectedTypeId) {
                this.selectedTypeId = preselectedTypeId;
                this.onTypeChange();
            if (preselectedCategoryId) {
                this.selectedCategoryId = preselectedCategoryId;
        this.selectedTypeId = '';
        this.selectedCategoryId = '';
        this.isActive = true;
        this.isDefault = false;
        this.expiresAt = null;
        this.credentialValues = {};
        this.schemaFields = [];
        this.showSecretFields.clear();
    private populateFromCredential(credential: MJCredentialEntity): void {
        this.name = credential.Name || '';
        this.description = credential.Description || '';
        this.selectedTypeId = credential.CredentialTypeID || '';
        this.selectedCategoryId = credential.CategoryID || '';
        this.isActive = credential.IsActive;
        this.isDefault = credential.IsDefault;
        this.expiresAt = credential.ExpiresAt ? new Date(credential.ExpiresAt) : null;
        // Parse the type schema
        // Parse stored values and filter out "undefined" strings
            if (credential.Values) {
                const parsedValues = JSON.parse(credential.Values) as CredentialValues;
                // Filter out "undefined" string values that may have been stored
                for (const [key, value] of Object.entries(parsedValues)) {
                    if (value !== 'undefined' && value !== undefined && value !== null) {
                        this.credentialValues[key] = value;
            console.error('Error parsing credential values:', e);
    public onTypeChange(): void {
        const type = this.selectedType;
        if (!type || !type.FieldSchema) {
            const schema = JSON.parse(type.FieldSchema) as {
                properties?: Record<string, Record<string, unknown>>;
                required?: string[]
            this.schemaFields = Object.entries(properties).map(([name, prop]) => {
                const field: FieldSchemaProperty = {
                    order: typeof prop.order === 'number' ? prop.order : 999
                // Extract JSON Schema constraint properties
                if ('enum' in prop && Array.isArray(prop.enum)) {
                    field.enum = prop.enum as string[];
                if ('const' in prop) {
                    field.const = prop.const;
                if ('default' in prop) {
                    field.default = prop.default;
                if ('format' in prop) {
                    field.format = prop.format as string;
                if ('pattern' in prop) {
                    field.pattern = prop.pattern as string;
                if ('minLength' in prop) {
                    field.minLength = prop.minLength as number;
                if ('maxLength' in prop) {
                    field.maxLength = prop.maxLength as number;
                if ('minimum' in prop) {
                    field.minimum = prop.minimum as number;
                if ('maximum' in prop) {
                    field.maximum = prop.maximum as number;
            // Sort by order
            this.schemaFields.sort((a, b) => a.order - b.order);
            // Initialize any missing values with defaults or const values
                if (!(field.name in this.credentialValues)) {
                    // Priority: const > default > empty
                    if (field.const !== undefined) {
                        this.credentialValues[field.name] = String(field.const);
                    } else if (field.default !== undefined) {
                        this.credentialValues[field.name] = String(field.default);
                        this.credentialValues[field.name] = '';
            console.error('Error parsing field schema:', e);
    public toggleSecretVisibility(fieldName: string): void {
        if (this.showSecretFields.has(fieldName)) {
            this.showSecretFields.delete(fieldName);
            this.showSecretFields.add(fieldName);
    public isSecretVisible(fieldName: string): boolean {
        return this.showSecretFields.has(fieldName);
            MJNotificationService.Instance.CreateSimpleNotification('Please fill in all required fields', 'warning', 3000);
        // Validate all fields against schema constraints
        const validationErrors = this.validateAllFields();
        if (validationErrors.length > 0) {
            const errorMessage = validationErrors.length === 1
                ? validationErrors[0]
                : `Validation errors:\n${validationErrors.map(e => `• ${e}`).join('\n')}`;
            MJNotificationService.Instance.CreateSimpleNotification(errorMessage, 'error', 5000);
            let entity: MJCredentialEntity;
                entity = await this._metadata.GetEntityObject<MJCredentialEntity>('MJ: Credentials');
                entity = this.credential!;
            // Set all fields
            entity.CredentialTypeID = this.selectedTypeId;
            entity.CategoryID = this.selectedCategoryId || null;
            entity.IsActive = this.isActive;
            entity.IsDefault = this.isDefault;
            entity.ExpiresAt = this.expiresAt;
            // Clean credential values before saving (remove "undefined" strings and empty non-required fields)
            const cleanedValues = this.cleanCredentialValues();
            entity.Values = JSON.stringify(cleanedValues);
                    `Credential "${entity.Name}" ${action} successfully`,
                console.error('Credential save failed:', errorMessage, entity.LatestResult);
                    `Failed to save credential: ${errorMessage}`,
                    8000
            console.error('Error saving credential:', error);
                'Error saving credential',
    public async deleteCredential(): Promise<void> {
        if (this.isNew || !this.credential) return;
        const confirmed = confirm(`Are you sure you want to delete "${this.credential.Name}"? This action cannot be undone.`);
            const success = await this.credential.Delete();
                    `Credential "${this.credential.Name}" deleted successfully`,
                this.deleted.emit(this.credential.ID);
                const errorMessage = this.credential.LatestResult?.CompleteMessage || this.credential.LatestResult?.Message || 'Unknown error';
                console.error('Credential delete failed:', errorMessage, this.credential.LatestResult);
                    `Failed to delete credential: ${errorMessage}`,
                'Error deleting credential',
        this.credential = null;
    public onBackdropClick(): void {
        // Backdrop click disabled - panel only closeable via Cancel button or ESC key
        // This prevents accidental closes when clicking outside the panel
    public onEscapeKey(event: Event): void {
        if (this.isOpen && !this.isSaving) {
    public get groupedCredentialTypes(): Array<{ category: string; types: MJCredentialTypeEntity[] }> {
        for (const type of this.credentialTypes) {
            const category = type.Category || 'Other';
        return Array.from(grouped.entries()).map(([category, types]) => ({ category, types }));
    public getTypeIcon(type: MJCredentialTypeEntity): string {
        return type.IconClass || iconMap[type.Category] || 'fa-solid fa-key';
    public getTypeColor(type: MJCredentialTypeEntity): string {
        return colorMap[type.Category] || '#6366f1';
    public onValueChange(fieldName: string, value: string): void {
        this.credentialValues[fieldName] = value;
    public formatDateForInput(date: Date | null): string {
        return d.toISOString().split('T')[0];
    public onExpiresAtChange(value: string): void {
        this.expiresAt = value ? new Date(value) : null;
     * Validates a field value against format constraints.
    private validateFormat(fieldTitle: string, value: string, format: string): string | null {
        if (!value || !format) return null;
            case 'uri':
                    new URL(value);
                    return `${fieldTitle} must be a valid URL`;
                return emailRegex.test(value) ? null : `${fieldTitle} must be a valid email`;
                return isNaN(Date.parse(value)) ? `${fieldTitle} must be a valid date` : null;
            case 'date-time':
                return isNaN(Date.parse(value)) ? `${fieldTitle} must be a valid date-time` : null;
                return uuidRegex.test(value) ? null : `${fieldTitle} must be a valid UUID`;
     * Validates all credential fields against their schema constraints.
     * Returns array of error messages (empty if all valid).
    private validateAllFields(): string[] {
            const value = this.credentialValues[field.name];
            const stringValue = String(value || '');
            // Skip validation for const fields (they're auto-populated and read-only)
            // Required field validation (already handled by canSave, but included for completeness)
            if (field.required && !stringValue) {
                errors.push(`${field.title} is required`);
            // Skip further validation if field is empty and not required
            if (!stringValue) {
            // Enum validation
            if (field.enum && field.enum.length > 0) {
                if (!field.enum.includes(stringValue)) {
                    errors.push(`${field.title} must be one of: ${field.enum.join(', ')}`);
            // Format validation
            if (field.format) {
                const formatError = this.validateFormat(field.title, stringValue, field.format);
                if (formatError) {
                    errors.push(formatError);
            // Pattern validation
            if (field.pattern) {
                    const regex = new RegExp(field.pattern);
                    if (!regex.test(stringValue)) {
                        errors.push(`${field.title} does not match required pattern`);
                    console.error(`Invalid regex pattern for ${field.name}:`, e);
            // Length validation
            if (field.minLength !== undefined && stringValue.length < field.minLength) {
                errors.push(`${field.title} must be at least ${field.minLength} characters`);
            if (field.maxLength !== undefined && stringValue.length > field.maxLength) {
                errors.push(`${field.title} must be no more than ${field.maxLength} characters`);
            // Numeric range validation (if type is number)
            if (field.type === 'number') {
                const numValue = Number(value);
                if (!isNaN(numValue)) {
                    if (field.minimum !== undefined && numValue < field.minimum) {
                        errors.push(`${field.title} must be at least ${field.minimum}`);
                    if (field.maximum !== undefined && numValue > field.maximum) {
                        errors.push(`${field.title} must be no more than ${field.maximum}`);
     * Cleans credential values before saving by removing:
     * - "undefined" string values
     * - Empty strings for non-required fields
     * - null and undefined values
    private cleanCredentialValues(): CredentialValues {
        const cleaned: CredentialValues = {};
            // Skip "undefined" strings
            if (value === 'undefined') {
            // Skip null and undefined
            // Skip empty strings for non-required fields (but keep empty strings for required fields)
            if (value === '' && !field.required) {
            // Keep all other values
            cleaned[field.name] = value;
