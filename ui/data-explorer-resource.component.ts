import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy, ViewChild, ViewContainerRef } from '@angular/core';
import { BaseResourceComponent, NavigationService, DashboardConfig } from '@memberjunction/ng-shared';
import { ResourceData, MJDashboardEntity } from '@memberjunction/core-entities';
import { DataExplorerDashboardComponent } from './data-explorer-dashboard.component';
import { DataExplorerFilter } from './models/explorer-state.interface';
 * Resource component for the Data Explorer.
 * Wraps DataExplorerDashboardComponent as a BaseResourceComponent for use
 * in application nav items with ResourceType: "Custom".
@RegisterClass(BaseResourceComponent, 'DataExplorerResource')
    selector: 'mj-data-explorer-resource',
        <div class="data-explorer-resource-container">
            <mj-data-explorer-dashboard
                [entityFilter]="entityFilter"
                [contextName]="contextName"
                [contextIcon]="contextIcon"
                (OpenEntityRecord)="onOpenEntityRecord($event)">
            </mj-data-explorer-dashboard>
        .data-explorer-resource-container {
export class DataExplorerResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public entityFilter: DataExplorerFilter | null = null;
    public contextName: string | null = null;
    public contextIcon: string | null = null;
    @ViewChild(DataExplorerDashboardComponent) dataExplorer!: DataExplorerDashboardComponent;
    private _dataLoaded = false;
    // Data Property Override
    override set Data(value: ResourceData) {
        super.Data = value;
        if (!this._dataLoaded) {
            this._dataLoaded = true;
            this.loadConfiguration();
    override get Data(): ResourceData {
        return super.Data;
        // Configuration loaded via Data setter
        return data.Name || 'Data';
        return 'fa-solid fa-table-cells';
    private loadConfiguration(): void {
        const data = this.Data;
        if (!data) {
        const config = data.Configuration || {};
        // Extract configuration options
        this.entityFilter = config['entityFilter'] as DataExplorerFilter || null;
        this.contextName = config['appName'] as string || null;
        this.contextIcon = config['appIcon'] as string || null;
        // Setup LoadCompleteEvent after view initializes
            if (this.dataExplorer) {
                this.dataExplorer.LoadCompleteEvent = () => {
                // Initialize with minimal config (no database dashboard)
                const dashboardConfig: DashboardConfig = {
                    dashboard: null as unknown as MJDashboardEntity,
                    userState: {}
                this.dataExplorer.Config = dashboardConfig;
                this.dataExplorer.Refresh();
    // Event Handlers
    public onOpenEntityRecord(event: { EntityName: string; RecordPKey: CompositeKey }): void {
        if (event && event.EntityName && event.RecordPKey) {
            this.navigationService.OpenEntityRecord(event.EntityName, event.RecordPKey);
