import { Component, OnInit, inject, ViewContainerRef } from '@angular/core';
import { MJActionEntity, MJActionParamEntity, MJActionResultCodeEntity, MJActionCategoryEntity, MJActionExecutionLogEntity, MJActionLibraryEntity, MJLibraryEntity } from '@memberjunction/core-entities';
import { MJActionFormComponent } from '../../generated/Entities/MJAction/mjaction.form.component';
import { ActionParamDialogComponent, ActionResultCodeDialogComponent } from '@memberjunction/ng-actions';
@RegisterClass(BaseFormComponent, 'MJ: Actions')
    selector: 'mj-action-form',
    templateUrl: './action-form.component.html',
    styleUrls: ['./action-form.component.css']
export class ActionFormComponentExtended extends MJActionFormComponent implements OnInit {
    public record!: MJActionEntity;
    public category: MJActionCategoryEntity | null = null;
    public actionParams: MJActionParamEntity[] = [];
    public resultCodes: MJActionResultCodeEntity[] = [];
    public recentExecutions: MJActionExecutionLogEntity[] = [];
    public actionLibraries: MJActionLibraryEntity[] = [];
    public libraries: MJLibraryEntity[] = [];
    // Cached filtered params
    private _inputParams: MJActionParamEntity[] = [];
    private _outputParams: MJActionParamEntity[] = [];
    // Track params to delete
    private paramsToDelete: MJActionParamEntity[] = [];
    // Track result codes to delete
    private resultCodesToDelete: MJActionResultCodeEntity[] = [];
    public isLoadingParams = false;
    public isLoadingResultCodes = false;
    public isLoadingExecutions = false;
    public isLoadingLibraries = false;
        overview: true,
        code: true,
        params: true,
        resultCodes: true,
        execution: false,
        configuration: false
    // Test harness state
    // Execution stats
    public executionStats = {
        successRate: 0,
        avgDuration: 0,
        lastRun: null as Date | null
    // Code editor config
    public codeLanguage = 'typescript';
    public showCodeComments = false;
                this.loadCategory(),
                this.loadActionParams(),
                this.loadResultCodes(),
                this.loadRecentExecutions(),
                this.loadActionLibraries(),
                this.loadExecutionStats()
     * Override InternalSaveRecord to handle Action and related ActionParams in a transaction
     * This follows the same pattern as AIAgentFormComponent
            // Set transaction group on the Action record
            // Save the Action record first
            const actionSaved = await this.record.Save();
            if (!actionSaved) {
                console.error('Failed to save Action record');
                this.sharedService.CreateSimpleNotification('Failed to save Action record', 'error', 5000);
            // Process all pending records (params and result codes to save or delete)
            for (const pendingRecord of this.PendingRecords) {
                if (pendingRecord.entityObject.EntityInfo.Name === 'MJ: Action Params') {
                    const param = pendingRecord.entityObject as MJActionParamEntity;
                    // Ensure ActionID is set for new params
                    if (!param.ActionID) {
                        param.ActionID = this.record.ID;
                    param.TransactionGroup = transactionGroup;
                    if (pendingRecord.action === 'save') {
                        const saved = await param.Save();
                            console.error('Failed to save parameter:', param.Name);
                    } else if (pendingRecord.action === 'delete') {
                        const deleted = await param.Delete();
                            console.error('Failed to delete parameter:', param.Name);
                } else if (pendingRecord.entityObject.EntityInfo.Name === 'MJ: Action Result Codes') {
                    const resultCode = pendingRecord.entityObject as MJActionResultCodeEntity;
                    // Ensure ActionID is set for new result codes
                    if (!resultCode.ActionID) {
                        resultCode.ActionID = this.record.ID;
                    resultCode.TransactionGroup = transactionGroup;
                        const saved = await resultCode.Save();
                            console.error('Failed to save result code:', resultCode.ResultCode);
                        const deleted = await resultCode.Delete();
                            console.error('Failed to delete result code:', resultCode.ResultCode);
            // Submit the transaction
                // Clear pending records after successful save
                this.paramsToDelete = [];
                this.resultCodesToDelete = [];
                // Reload params and result codes to get updated data
                    this.loadResultCodes()
                // Show success message
                this.sharedService.CreateSimpleNotification('Action and related records saved successfully', 'success', 3000);
            console.error('Error saving Action and parameters:', error);
            this.sharedService.CreateSimpleNotification('Error saving Action: ' + error, 'error', 5000);
    private async loadCategory() {
        if (!this.record.CategoryID) return;
            this.category = await md.GetEntityObject<MJActionCategoryEntity>('MJ: Action Categories');
            if (this.category) {
                await this.category.Load(this.record.CategoryID);
            // Error loading category
    private async loadActionParams() {
        this.isLoadingParams = true;
                ExtraFilter: `ActionID='${this.record.ID}'`,
                ResultType: 'entity_object'  // This ensures we get proper entity instances
                this.actionParams = result.Results || [];
                // Update cached filtered params - trim and lowercase Type values to handle any whitespace and case
                this._inputParams = this.actionParams.filter(p => {
                    const type = p.Type?.trim().toLowerCase();
                    return type === 'input' || type === 'both';
                this._outputParams = this.actionParams.filter(p => {
                    return type === 'output' || type === 'both';
                // Failed to load action params
                this.actionParams = [];
                this._inputParams = [];
                this._outputParams = [];
            // Error loading action params
            this.isLoadingParams = false;
    private async loadResultCodes() {
        this.isLoadingResultCodes = true;
            const result = await rv.RunView<MJActionResultCodeEntity>({
                OrderBy: 'IsSuccess DESC, ResultCode',
                this.resultCodes = result.Results || [];
                // Failed to load result codes
                this.resultCodes = [];
            // Error loading result codes
            this.isLoadingResultCodes = false;
    private async loadRecentExecutions() {
        this.isLoadingExecutions = true;
            const result = await rv.RunView<MJActionExecutionLogEntity>({
                EntityName: 'MJ: Action Execution Logs',
                MaxRows: 10 
                this.recentExecutions = result.Results || [];
                // Failed to load executions
                this.recentExecutions = [];
            // Error loading executions
            this.isLoadingExecutions = false;
    private async loadActionLibraries() {
        this.isLoadingLibraries = true;
            const result = await rv.RunView<MJActionLibraryEntity>({
                OrderBy: 'Library' 
                this.actionLibraries = result.Results || [];
                // Load library details
                if (this.actionLibraries.length > 0) {
                    const libraryIds = this.actionLibraries.map(al => al.LibraryID).filter(id => id);
                    this.libraries = [];
                    for (const libId of libraryIds) {
                        const lib = await md.GetEntityObject<MJLibraryEntity>('MJ: Libraries');
                        if (lib && libId) {
                            await lib.Load(libId);
                            this.libraries.push(lib);
            // Error loading libraries
            this.isLoadingLibraries = false;
    private async loadExecutionStats() {
            // Load ALL executions for accurate statistics
                const allExecutions = result.Results;
                this.executionStats.totalRuns = allExecutions.length;
                // Calculate success rate based on result codes
                const successfulRuns = allExecutions.filter(e => {
                    const resultCode = this.resultCodes.find(rc => rc.ResultCode === e.ResultCode);
                    return resultCode?.IsSuccess || false;
                this.executionStats.successRate = this.executionStats.totalRuns > 0 
                    ? (successfulRuns.length / this.executionStats.totalRuns) * 100 
                // Calculate average duration from ALL completed executions
                const completedExecutions = allExecutions.filter(e => e.StartedAt && e.EndedAt);
                if (completedExecutions.length > 0) {
                    const totalDuration = completedExecutions.reduce((sum, e) => {
                        const duration = new Date(e.EndedAt!).getTime() - new Date(e.StartedAt).getTime();
                        // Use absolute value to handle any swapped dates
                        return sum + Math.abs(duration);
                    this.executionStats.avgDuration = totalDuration / completedExecutions.length;
                // Get last run date from most recent execution
                this.executionStats.lastRun = new Date(allExecutions[0].StartedAt);
            // Error loading execution stats
    // UI Helper Methods
        switch (this.record.Status) {
            case 'Disabled': return '#dc3545';
            case 'Active': return 'fa-check-circle';
            case 'Pending': return 'fa-clock';
            case 'Disabled': return 'fa-ban';
            default: return 'fa-question-circle';
    getTypeColor(): string {
        return this.record.Type === 'Generated' ? '#6f42c1' : '#007bff';
    getTypeIcon(): string {
        return this.record.Type === 'Generated' ? 'fa-robot' : 'fa-code';
    getApprovalStatusColor(): string {
        switch (this.record.CodeApprovalStatus) {
            case 'Approved': return '#28a745';
            case 'Rejected': return '#dc3545';
    getApprovalStatusIcon(): string {
            case 'Approved': return 'fa-check-circle';
            case 'Rejected': return 'fa-times-circle';
    getParamTypeIcon(type: string): string {
            case 'Input': return 'fa-sign-in-alt';
            case 'Output': return 'fa-sign-out-alt';
            case 'Both': return 'fa-exchange-alt';
    getParamTypeColor(type: string): string {
            case 'Input': return '#007bff';
            case 'Output': return '#28a745';
            case 'Both': return '#6f42c1';
        if (!date) return 'Never';
        const d = typeof date === 'string' ? new Date(date) : date;
        const diff = now.getTime() - d.getTime();
        if (diff < 60000) return 'Just now';
        if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`;
        if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`;
        if (diff < 604800000) return `${Math.floor(diff / 86400000)}d ago`;
    navigateToCategory() {
        if (this.record.CategoryID) {
            this.navigateToEntity('MJ: Action Categories', this.record.CategoryID);
    navigateToExecution(executionId: string) {
        this.navigateToEntity('MJ: Action Execution Logs', executionId);
    navigateToLibrary(libraryId: string) {
        this.navigateToEntity('MJ: Libraries', libraryId);
    openTestHarness() {
        if (!this.record || !this.record.ID || !this.record.IsSaved || this.record.Status !== 'Active') {
            // Cannot open test harness: Action must be saved and active
        this.showTestHarness = true;
     * Event handler for test harness visibility changes
    async regenerateCode() {
        this.record.ForceCodeGeneration = true;
        // Reload related data after save
        await this.loadResultCodes();
    toggleCodeComments() {
        this.showCodeComments = !this.showCodeComments;
    async approveCode() {
        this.record.CodeApprovalStatus = 'Approved';
        this.record.CodeApprovedAt = new Date();
        // Note: CodeApprovedByUserID would be set server-side
    async rejectCode() {
        this.record.CodeApprovalStatus = 'Rejected';
            // Could add a notification here
            // Failed to copy
    // Helper methods for template filtering
    getInputParams(): MJActionParamEntity[] {
        // Sort by IsRequired (required first) then by Name
        return this._inputParams.sort((a, b) => {
            if (a.IsRequired === b.IsRequired) {
                return (a.Name || '').localeCompare(b.Name || '');
            return a.IsRequired ? -1 : 1;
    getOutputParams(): MJActionParamEntity[] {
        // Sort by Name
        return this._outputParams.sort((a, b) => (a.Name || '').localeCompare(b.Name || ''));
    isExecutionSuccess(execution: MJActionExecutionLogEntity): boolean {
        const code = execution.ResultCode?.toLowerCase();
        // First check if we have a result code definition
        const resultCode = this.resultCodes.find(rc => rc.ResultCode === execution.ResultCode);
        if (resultCode) {
            return resultCode.IsSuccess;
        // Fallback to common success patterns if no result code defined
        return code === 'success' || code === 'ok' || code === 'completed' || code === '200';
    getExecutionDuration(execution: MJActionExecutionLogEntity): number {
        if (!execution.EndedAt) return 0;
        const startTime = new Date(execution.StartedAt).getTime();
        const endTime = new Date(execution.EndedAt).getTime();
        const duration = endTime - startTime;
        // Return absolute value to handle timezone mismatches
        return Math.abs(duration);
    getSuccessRateColor(): string {
        const rate = this.executionStats.successRate;
        if (rate >= 80) return '#28a745'; // green
        if (rate >= 60) return '#ffc107'; // yellow
        return '#dc3545'; // red
    // Parameter management methods
    async addParameter(type: 'Input' | 'Output' | 'Both') {
        if (!this.EditMode || !this.record.IsSaved) return;
        const newParam = await md.GetEntityObject<MJActionParamEntity>('MJ: Action Params');
        newParam.ActionID = this.record.ID;
        newParam.Name = '';
        newParam.Type = type;
        newParam.ValueType = 'Scalar';
        newParam.IsRequired = false;
        newParam.IsArray = false;
            content: ActionParamDialogComponent,
            width: 500,
            appendTo: this.viewContainerRef
        const dialog = dialogRef.content.instance;
        dialog.param = newParam;
        dialog.isNew = true;
        dialog.editMode = true;
        dialogRef.result.subscribe(result => {
            if (result && (result as any).save) {
                // The dialog has already modified the newParam entity directly
                // New entities are automatically dirty (IsSaved = false)
                // Add to local array
                this.actionParams.push(newParam);
                // Add to pending records for saving
                    entityObject: newParam,
                // Update the filtered arrays
                this.updateParamArrays();
    async editParameter(param: MJActionParamEntity) {
        dialog.param = param;
        dialog.isNew = false;
        dialog.editMode = this.EditMode;
            if (result && (result as any).save && this.EditMode) {
                // Param will be dirty from property changes in dialog
                // Ensure it's in pending records if modified
                if (param.Dirty) {
                    const exists = this.PendingRecords.some(pr => 
                        pr.entityObject === param && pr.action === 'save'
                    if (!exists) {
                            entityObject: param,
                // Update the local arrays
    onParamClick(param: MJActionParamEntity, event: Event) {
        // Prevent event bubbling if clicking on edit/delete buttons
        const target = event.target as HTMLElement;
        if (target.closest('.param-edit-btn') || target.closest('.param-delete-btn')) {
        // Show the parameter dialog
        this.editParameter(param);
    private async updateParamArrays() {
        // Update cached filtered params - exclude deleted items
        const activeParams = this.actionParams.filter(p => !this.paramsToDelete || !this.paramsToDelete.includes(p));
        this._inputParams = activeParams.filter(p => {
        this._outputParams = activeParams.filter(p => {
    // Override to populate pending records with our action params and result codes
        // Preserve existing pending records before base class clears them
        const currentPendingRecords = [...this.PendingRecords];
        // Call parent to handle child components
            // Only re-add if it's an Action Param or Result Code (avoid duplicates)
            if (record.entityObject.EntityInfo.Name === 'MJ: Action Params' || 
                record.entityObject.EntityInfo.Name === 'MJ: Action Result Codes') {
                    pr.entityObject === record.entityObject
        // Add action params that need saving
        for (const param of this.actionParams) {
            if (!param.IsSaved || param.Dirty) {
                // Check if not already in pending records
                    pr.entityObject === param
        // Add params marked for deletion
        for (const param of this.paramsToDelete) {
            if (param.IsSaved) {
        // Add result codes that need saving
        for (const resultCode of this.resultCodes) {
            if (!resultCode.IsSaved || resultCode.Dirty) {
                    pr.entityObject === resultCode
                        entityObject: resultCode,
        // Add result codes marked for deletion
        for (const resultCode of this.resultCodesToDelete) {
            if (resultCode.IsSaved) {
     * Gets the action's display icon
     * Falls back to default cog icon if no IconClass is set
        return this.record?.IconClass || 'fa-solid fa-cog';
    // Result Code management methods
    async addResultCode() {
        const newResultCode = await md.GetEntityObject<MJActionResultCodeEntity>('MJ: Action Result Codes');
        newResultCode.ActionID = this.record.ID;
        newResultCode.ResultCode = '';
        newResultCode.Description = '';
        newResultCode.IsSuccess = false;
            content: ActionResultCodeDialogComponent,
        dialog.resultCode = newResultCode;
                this.resultCodes.push(newResultCode);
                    entityObject: newResultCode,
    async editResultCode(resultCode: MJActionResultCodeEntity) {
        dialog.resultCode = resultCode;
                if (resultCode.Dirty) {
                        pr.entityObject === resultCode && pr.action === 'save'
    onResultCodeClick(resultCode: MJActionResultCodeEntity, event: Event) {
        if (target.closest('.result-edit-btn') || target.closest('.result-delete-btn')) {
        // Show the result code dialog
        this.editResultCode(resultCode);
     * Delete a result code (marks for deletion on save)
    deleteResultCode(resultCode: MJActionResultCodeEntity) {
        // Remove from main array
        const index = this.resultCodes.indexOf(resultCode);
            this.resultCodes.splice(index, 1);
        // Handle pending records
            // Add to deletion list for saved result codes
            this.resultCodesToDelete.push(resultCode);
            // Add to pending records for deletion
            // For unsaved result codes, just remove from pending records
            const pendingIndex = this.PendingRecords.findIndex(pr => 
            if (pendingIndex >= 0) {
                this.PendingRecords.splice(pendingIndex, 1);
     * Delete a parameter (marks for deletion on save)
    deleteParameter(param: MJActionParamEntity) {
        const index = this.actionParams.indexOf(param);
            this.actionParams.splice(index, 1);
            // Add to deletion list for saved params
            this.paramsToDelete.push(param);
            // For unsaved params, just remove from pending records
        // Update filtered arrays
