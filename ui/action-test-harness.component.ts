import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { MJActionEntity, MJActionParamEntity, UserInfoEngine } from '@memberjunction/core-entities';
// Setting key prefix for action run input caching
const ACTION_INPUT_CACHE_PREFIX = '__ACTION_DASHBOARD__action-run-inputs/';
export interface ActionParamValue {
export interface ActionResult {
    ResultCode?: string;
    ResultData?: unknown;
    selector: 'mj-action-test-harness',
    templateUrl: './action-test-harness.component.html',
    styleUrls: ['./action-test-harness.component.css']
export class ActionTestHarnessComponent implements OnInit {
    @ViewChild('resultsSection') ResultsSectionRef!: ElementRef<HTMLDivElement>;
    private _isVisible = false;
    // Input properties with getter/setters
        if (value && this._actionParams.length > 0) {
            this.initializeParamValues();
        if (this._action && value) {
    set IsVisible(value: boolean) {
    get IsVisible(): boolean {
    @Output() VisibilityChange = new EventEmitter<boolean>();
    // Public state properties
    public ParamValues: ActionParamValue[] = [];
    public IsExecuting = false;
    public ExecutionResult: ActionResult | null = null;
    public ExecutionError: string | null = null;
    public ExecutionTime = 0;
    public ShowRawResult = false;
    public SkipActionLog = false;
    public InputsCollapsed = false;
    private initializeParamValues(): void {
        this.ParamValues = this._actionParams
            .filter(p => {
                // Sort required params first, then by name
                if (a.IsRequired !== b.IsRequired) {
            .map(param => ({
                Param: param,
                Value: this.getDefaultValue(param)
        // Load cached values for this action
        this.loadCachedInputs();
     * Load cached input values from UserInfoEngine
    private loadCachedInputs(): void {
        if (!this._action?.ID) return;
            const cacheKey = `${ACTION_INPUT_CACHE_PREFIX}${this._action.ID}`;
            const cachedJson = UserInfoEngine.Instance.GetSetting(cacheKey);
            if (cachedJson) {
                const cachedValues = JSON.parse(cachedJson) as Record<string, unknown>;
                // Apply cached values to matching parameters
                for (const paramValue of this.ParamValues) {
                    const paramName = paramValue.Param.Name;
                    if (paramName in cachedValues) {
                        paramValue.Value = cachedValues[paramName];
            // Silently ignore cache load errors - just use defaults
            console.warn('Action Test Harness: Failed to load cached inputs', error);
     * Save current input values to cache using debounced setting
    private saveCachedInputs(): void {
            const values: Record<string, unknown> = {};
                values[paramValue.Param.Name] = paramValue.Value;
            // Use debounced setting to avoid excessive saves during rapid typing
            UserInfoEngine.Instance.SetSettingDebounced(cacheKey, JSON.stringify(values));
            // Silently ignore cache save errors
            console.warn('Action Test Harness: Failed to save cached inputs', error);
    private getDefaultValue(param: MJActionParamEntity): unknown {
        if (param.DefaultValue) {
            return this.parseDefaultValue(param.DefaultValue, param.ValueType);
        // Return appropriate empty value based on type
        if (param.IsArray) {
    private parseDefaultValue(defaultValue: string, valueType: string): unknown {
            // Try to parse as JSON first
            return JSON.parse(defaultValue);
            // If not JSON, return as string for scalar types
            if (valueType === 'Scalar') {
    public GetInputType(param: MJActionParamEntity): string {
        if (param.IsArray || param.ValueType === 'Simple Object' || param.ValueType === 'BaseEntity Sub-Class') {
            return 'textarea';
        // Try to infer from default value or name
        const value = param.DefaultValue?.toLowerCase() || param.Name.toLowerCase();
        if (value.includes('date') || value.includes('time')) {
            return 'datetime-local';
        if (value === 'true' || value === 'false') {
            return 'checkbox';
        if (!isNaN(Number(value))) {
    public OnParamValueChange(paramValue: ActionParamValue, event: Event): void {
        const target = event.target as HTMLInputElement | HTMLTextAreaElement;
        const inputType = this.GetInputType(paramValue.Param);
        if (inputType === 'checkbox') {
            paramValue.Value = (target as HTMLInputElement).checked;
        } else if (inputType === 'number') {
            paramValue.Value = (target as HTMLInputElement).valueAsNumber;
        } else if (inputType === 'textarea') {
            // For complex types, try to parse as JSON
                paramValue.Value = JSON.parse(target.value);
                paramValue.Error = undefined;
                // If not valid JSON, keep as string
                paramValue.Value = target.value;
                if (paramValue.Param.ValueType !== 'Scalar') {
                    paramValue.Error = 'Invalid JSON format';
        // Cache the inputs for this action (debounced)
        this.saveCachedInputs();
    public GetParamDisplayValue(paramValue: ActionParamValue): string {
        if (paramValue.Value === null || paramValue.Value === undefined) {
        if (typeof paramValue.Value === 'object') {
            return JSON.stringify(paramValue.Value, null, 2);
        return String(paramValue.Value);
    public ValidateParams(): boolean {
        let isValid = true;
            // Check required fields
            if (paramValue.Param.IsRequired) {
                if (paramValue.Value === null || paramValue.Value === undefined ||
                    (typeof paramValue.Value === 'string' && paramValue.Value.trim() === '') ||
                    (Array.isArray(paramValue.Value) && paramValue.Value.length === 0)) {
                    paramValue.Error = 'This field is required';
                    isValid = false;
            // Check for JSON parse errors
            if (paramValue.Error) {
    public async ExecuteAction(): Promise<void> {
        if (!this.ValidateParams()) {
                params[paramValue.Param.Name] = paramValue.Value;
            // Execute the action using GraphQL
                mutation RunAction($input: RunActionInput!) {
                    RunAction(input: $input) {
                        Message
                        ResultCode
                        ResultData
            // Get GraphQL data provider from Metadata
            const graphQLProvider = Metadata.Provider as GraphQLDataProvider;
            // Convert params to ActionParamInput array format
            const actionParams = this.ParamValues.map(paramValue => {
                // Determine the actual data type for the Type field
                let dataType = 'string'; // default
                if (paramValue.Param.ValueType === 'Scalar') {
                    // For scalar, check the actual value type
                    if (typeof paramValue.Value === 'boolean') {
                        dataType = 'boolean';
                    } else if (typeof paramValue.Value === 'number') {
                        dataType = 'number';
                        dataType = 'string';
                } else if (paramValue.Param.ValueType === 'Simple Object') {
                    dataType = 'object';
                } else if (paramValue.Param.IsArray) {
                    dataType = 'array';
                    Name: paramValue.Param.Name,
                    Value: paramValue.Value === null || paramValue.Value === undefined
                        : typeof paramValue.Value === 'object'
                            ? JSON.stringify(paramValue.Value)
                            : String(paramValue.Value),
                    Type: dataType
                    ActionID: this._action.ID,
                    SkipActionLog: this.SkipActionLog
                result = await graphQLProvider.ExecuteGQL(query, variables);
            } catch (gqlError: unknown) {
                const error = gqlError as { message?: string; networkError?: unknown; graphQLErrors?: unknown };
                console.error('Action Test Harness: GraphQL execution failed', {
                    error: gqlError,
                    message: error?.message,
                    networkError: error?.networkError,
                    graphQLErrors: error?.graphQLErrors
                throw gqlError;
            this.ExecutionTime = Date.now() - startTime;
            if (result?.RunAction) {
                this.ExecutionResult = result.RunAction;
                // If result is false/failed, it might still have an error in the data
                if (!this.ExecutionResult?.Success && this.ExecutionResult?.Message) {
                    console.warn('Action Test Harness: Action failed with message', this.ExecutionResult.Message);
                // Emit the execution complete event
                if (this.ExecutionResult) {
                    this.ExecutionComplete.emit(this.ExecutionResult);
                console.error('Action Test Harness: No RunAction in result', {
                    resultType: typeof result,
                    resultStringified: JSON.stringify(result)
                throw new Error('No result returned from action execution');
            const err = error as { message?: string; stack?: string; constructor?: { name?: string } };
            this.ExecutionError = err.message || 'An unknown error occurred';
            console.error('Action Test Harness: Caught error during action execution', {
                message: err?.message,
                stack: err?.stack,
                type: err?.constructor?.name
            // Auto-collapse inputs and scroll to results
            this.InputsCollapsed = true;
            // Scroll to results section after a small delay to allow DOM update
                this.ScrollToResults();
    private ScrollToResults(): void {
        if (this.ResultsSectionRef?.nativeElement) {
            this.ResultsSectionRef.nativeElement.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
    public ToggleInputsCollapsed(): void {
        this.InputsCollapsed = !this.InputsCollapsed;
    public ClearResults(): void {
        this.ExecutionTime = 0;
    public ResetParams(): void {
        this.ClearResults();
    public CopyResultToClipboard(): void {
        if (!this.ExecutionResult) return;
        const resultText = JSON.stringify(this.ExecutionResult, null, 2);
        navigator.clipboard.writeText(resultText).catch(() => {
            // Failed to copy to clipboard
    public CopyResultDataToClipboard(): void {
        if (!this.ExecutionResult?.ResultData) return;
        const resultDataText = this.FormatResultData(this.ExecutionResult.ResultData);
        navigator.clipboard.writeText(resultDataText).catch(() => {
    public GetResultIcon(): string {
        if (!this.ExecutionResult) return '';
        return this.ExecutionResult.Success ? 'fa-check-circle' : 'fa-times-circle';
    public GetResultColor(): string {
        return this.ExecutionResult.Success ? '#28a745' : '#dc3545';
    public GetOutputParams(): MJActionParamEntity[] {
        return this._actionParams.filter(p => p.Type === 'Output' || p.Type === 'Both');
    public GetOutputParamValue(paramName: string): string {
        if (!this.ExecutionResult?.ResultData) return 'null';
        const data = this.ExecutionResult.ResultData as Record<string, unknown>;
        return this.FormatResultData(data[paramName]);
    public FormatResultData(data: unknown): string {
        if (data === null || data === undefined) return 'null';
        if (typeof data === 'object') {
            return JSON.stringify(data, null, 2);
        return String(data);
