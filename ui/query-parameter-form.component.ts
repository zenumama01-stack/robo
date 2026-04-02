import { QueryInfo, QueryParameterInfo } from '@memberjunction/core';
import { QueryParameterValues } from '../query-data-grid/models/query-grid-types';
 * A single parameter field in the form
interface ParameterField {
    info: QueryParameterInfo;
    value: string | number | boolean | Date | string[] | null;
    touched: boolean;
 * A slide-in form for entering query parameters before execution.
 * - Dynamic form generation from QueryParameterInfo metadata
 * - Type-appropriate input controls (text, number, date, checkbox, multi-select)
 * - Validation with helpful error messages
 * - Sample values as placeholders
 * - Description tooltips
 * <mj-query-parameter-form
 *   [QueryInfo]="query"
 *   [InitialValues]="savedParams"
 *   [IsOpen]="showParams"
 *   (ParametersSubmit)="onRunQuery($event)"
 *   (Close)="showParams = false">
 * </mj-query-parameter-form>
    selector: 'mj-query-parameter-form',
    templateUrl: './query-parameter-form.component.html',
    styleUrls: ['./query-parameter-form.component.css'],
                animate('150ms ease-out', style({ opacity: 1 }))
                animate('100ms ease-in', style({ opacity: 0 }))
export class QueryParameterFormComponent implements OnInit, OnChanges, OnDestroy {
     * The query metadata containing parameter definitions
     * Initial values to populate the form (e.g., from saved state)
    @Input() InitialValues: QueryParameterValues = {};
     * Whether the panel is open/visible
     * Panel width in pixels
    @Input() PanelWidth: number = 400;
     * Whether to show the overlay backdrop
     * Fired when user submits the form with valid parameters
    @Output() ParametersSubmit = new EventEmitter<QueryParameterValues>();
     * Fired when panel is closed
    public Fields: ParameterField[] = [];
    public HasRequiredParams: boolean = false;
    public AllRequiredFilled: boolean = false;
        this.buildForm();
        if (changes['QueryInfo'] || changes['InitialValues']) {
    // Form Building
    private buildForm(): void {
        if (!this.QueryInfo) {
            this.Fields = [];
            this.HasRequiredParams = false;
        const parameters = this.QueryInfo.Parameters || [];
        this.Fields = parameters.map(param => {
            const initialValue = this.getInitialValue(param);
                info: param,
                value: initialValue,
                touched: false
        this.HasRequiredParams = this.Fields.some(f => f.info.IsRequired);
        this.validateAllFields();
    private getInitialValue(param: QueryParameterInfo): string | number | boolean | Date | string[] | null {
        // Check initial values first
        if (this.InitialValues && this.InitialValues[param.Name] !== undefined) {
            return this.InitialValues[param.Name];
        // Fall back to default value
        if (param.DefaultValue !== null && param.DefaultValue !== undefined) {
            return this.parseDefaultValue(param);
        // Type-specific defaults
    private parseDefaultValue(param: QueryParameterInfo): string | number | boolean | Date | string[] | null {
        const defaultValue = param.DefaultValue;
        if (defaultValue === null || defaultValue === undefined) return null;
                    return Number(defaultValue);
                    return defaultValue.toLowerCase() === 'true' || defaultValue === '1';
                    return new Date(defaultValue);
    // Value Handling
    public OnValueChange(field: ParameterField, value: unknown): void {
        field.value = value as ParameterField['value'];
        field.touched = true;
        this.validateField(field);
        this.updateAllRequiredFilled();
    public OnDateChange(field: ParameterField, value: Date | null): void {
        field.value = value;
    public OnCheckboxChange(field: ParameterField, event: Event): void {
        field.value = input.checked;
    private validateField(field: ParameterField): void {
        field.error = null;
        const value = field.value;
        const param = field.info;
        // Required check
        if (param.IsRequired) {
                field.error = 'This field is required';
            if (param.Type === 'array' && Array.isArray(value) && value.length === 0) {
                field.error = 'At least one value is required';
        // Type validation
                    if (isNaN(Number(value))) {
                        field.error = 'Must be a valid number';
                    if (value instanceof Date && isNaN(value.getTime())) {
                        field.error = 'Must be a valid date';
    private validateAllFields(): void {
        for (const field of this.Fields) {
    private updateAllRequiredFilled(): void {
        this.AllRequiredFilled = this.Fields
            .filter(f => f.info.IsRequired)
            .every(f => {
                if (value === null || value === undefined || value === '') return false;
                if (f.info.Type === 'array' && Array.isArray(value) && value.length === 0) return false;
                return f.error === null;
    public get HasErrors(): boolean {
        return this.Fields.some(f => f.error !== null);
    public get CanSubmit(): boolean {
        return !this.HasErrors && this.AllRequiredFilled;
    // Form Submission
    public Submit(): void {
        // Mark all fields as touched
        if (!this.CanSubmit) {
        // Build parameter values object
        const values: QueryParameterValues = {};
            if (field.value !== null && field.value !== undefined) {
                values[field.info.Name] = field.value;
        this.ParametersSubmit.emit(values);
    public Cancel(): void {
    public GetInputType(param: QueryParameterInfo): string {
    public GetPlaceholder(param: QueryParameterInfo): string {
        if (param.SampleValue) {
            return `e.g., ${param.SampleValue}`;
        return param.Description || '';
    public TrackByField(index: number, field: ParameterField): string {
        return field.info.Name;
    // Type-Safe Value Getters for Template
    public GetStringValue(value: ParameterField['value']): string {
    public GetNumberValue(value: ParameterField['value']): number | null {
    public GetDateValue(value: ParameterField['value']): string {
            // Format as YYYY-MM-DD for HTML date input
            return value.toISOString().split('T')[0];
            // Try to parse and format
    public GetBooleanValue(value: ParameterField['value']): boolean {
    public GetArrayDisplayValue(value: ParameterField['value']): string {
            return value.join(', ');
    // Input Event Handlers
    public OnInputChange(field: ParameterField, event: Event): void {
        this.OnValueChange(field, input.value);
    public OnNumberInputChange(field: ParameterField, event: Event): void {
        const value = input.value === '' ? null : Number(input.value);
        this.OnValueChange(field, value);
    public OnDateInputChange(field: ParameterField, event: Event): void {
        const value = input.value ? new Date(input.value) : null;
        this.OnDateChange(field, value);
    public OnArrayInputChange(field: ParameterField, event: Event): void {
        const value = input.value
            ? input.value.split(',').map(s => s.trim()).filter(s => s)
