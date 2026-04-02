import { MJActionExecutionLogEntity, MJActionEntity, MJUserEntity } from '@memberjunction/core-entities';
import { Metadata, CompositeKey } from '@memberjunction/core';
import { MJActionExecutionLogFormComponent } from '../../generated/Entities/MJActionExecutionLog/mjactionexecutionlog.form.component';
interface ActionParameter {
    Value: unknown;
    Type: 'Input' | 'Output' | 'Both';
@RegisterClass(BaseFormComponent, 'MJ: Action Execution Logs')
    selector: 'mj-action-execution-log-form',
    templateUrl: './action-execution-log-form.component.html',
    styleUrls: ['./action-execution-log-form.component.css']
export class ActionExecutionLogFormComponentExtended extends MJActionExecutionLogFormComponent implements OnInit {
    public record!: MJActionExecutionLogEntity;
    public action: MJActionEntity | null = null;
    public user: MJUserEntity | null = null;
    // Parameter counts for visibility
    public hasInputParams = false;
    public hasOutputParams = false;
    public hasBothParams = false;
    // Loading states
    public isLoadingAction = false;
    public isLoadingUser = false;
    // Formatted JSON fields
    public formattedParams: string = '';
    public formattedMessage: string = '';
    public formattedInputParams: string = '';
    public formattedOutputParams: string = '';
    public formattedBothParams: string = '';
    public expandedSections = {
        execution: true,
        input: true,
        output: true,
        inputParams: true,
        outputParams: true,
        bothParams: true,
            // Load related data
                this.loadAction(),
                this.loadUser()
            // Format JSON fields
            this.formatJSONFields();
    private async loadAction() {
        if (!this.record.ActionID) return;
        this.isLoadingAction = true;
            this.action = await md.GetEntityObject<MJActionEntity>('MJ: Actions');
            if (this.action) {
                await this.action.Load(this.record.ActionID);
            console.error('Error loading action:', error);
            this.isLoadingAction = false;
    private async loadUser() {
        if (!this.record.UserID) return;
        this.isLoadingUser = true;
            this.user = await md.GetEntityObject<MJUserEntity>('MJ: Users');
            if (this.user) {
                await this.user.Load(this.record.UserID);
            console.error('Error loading user:', error);
            this.isLoadingUser = false;
    private formatJSONFields() {
        // Format Params with recursive JSON parsing
        if (this.record.Params) {
                const parsed = JSON.parse(this.record.Params);
                this.formattedParams = JSON.stringify(recursivelyParsed, null, 2);
                // Format parameter-specific views if params is an array of ActionParameter objects
                if (Array.isArray(recursivelyParsed)) {
                    this.formatParameterSections(recursivelyParsed as ActionParameter[]);
                this.formattedParams = this.record.Params;
        // Format Message field with recursive JSON parsing
        if (this.record.Message) {
                const parsed = JSON.parse(this.record.Message);
                this.formattedMessage = JSON.stringify(recursivelyParsed, null, 2);
                this.formattedMessage = this.record.Message;
    private formatParameterSections(params: ActionParameter[]) {
        // Reset visibility flags
        this.hasInputParams = false;
        this.hasOutputParams = false;
        this.hasBothParams = false;
        // Arrays to collect parameters by type
        const inputParams: ActionParameter[] = [];
        const outputParams: ActionParameter[] = [];
        const bothParams: ActionParameter[] = [];
        // Sort parameters by type
        for (const param of params) {
                case 'Input':
                    inputParams.push(param);
                    this.hasInputParams = true;
                case 'Output':
                    outputParams.push(param);
                    this.hasOutputParams = true;
                case 'Both':
                    bothParams.push(param);
                    this.hasBothParams = true;
        // Format input parameters
        if (inputParams.length > 0) {
            this.formattedInputParams = JSON.stringify(inputParams, null, 2);
        // Format output parameters
            this.formattedOutputParams = JSON.stringify(outputParams, null, 2);
        // Format both parameters
        if (bothParams.length > 0) {
            this.formattedBothParams = JSON.stringify(bothParams, null, 2);
    // Navigation
    navigateToAction() {
        if (this.record.ActionID) {
            this.navigateToEntity('MJ: Actions', this.record.ActionID);
    navigateToUser() {
        if (this.record.UserID) {
            this.navigateToEntity('MJ: Users', this.record.UserID);
    // UI Helpers
    getExecutionDuration(): number {
        if (!this.record.StartedAt || !this.record.EndedAt) return 0;
        return new Date(this.record.EndedAt).getTime() - new Date(this.record.StartedAt).getTime();
    formatDuration(ms: number): string {
        if (ms < 3600000) return `${(ms / 60000).toFixed(1)}m`;
        return `${(ms / 3600000).toFixed(1)}h`;
    getResultCodeColor(): string {
        const code = this.record.ResultCode?.toLowerCase();
        if (code === 'success' || code === 'ok' || code === 'completed' || code === '200') {
    getResultCodeIcon(): string {
    // Save handlers for JSON fields
    async saveParams() {
        if (!this.EditMode) return;
            // Validate JSON
            JSON.parse(this.formattedParams);
            this.record.Params = this.formattedParams;
            await this.record.Save();
            console.error('Invalid JSON in Params field:', e);
            // Could show notification here
    async saveMessage() {
            JSON.parse(this.formattedMessage);
            this.record.Message = this.formattedMessage;
            console.error('Invalid JSON in Message field:', e);
    async copyToClipboard(text: string) {
            await navigator.clipboard.writeText(text);
            // Could show a toast notification here
            console.error('Failed to copy to clipboard:', err);
