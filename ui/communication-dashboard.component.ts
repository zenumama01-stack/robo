import { Component, AfterViewInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { BaseDashboard } from '@memberjunction/ng-shared';
interface CommunicationDashboardState {
    activeTab: string;
    selector: 'mj-communication-dashboard',
    templateUrl: './communication-dashboard.component.html',
    styleUrls: ['./communication-dashboard.component.css'],
@RegisterClass(BaseDashboard, 'CommunicationDashboard')
export class CommunicationDashboardComponent extends BaseDashboard implements AfterViewInit, OnDestroy {
    public isRefreshing = false;
    public activeTab = 'monitor';
    public selectedIndex = 0;
    private visitedTabs = new Set<string>();
    public navigationItems: string[] = ['monitor', 'logs', 'providers', 'templates', 'runs', 'settings'];
    private stateChangeSubject = new Subject<CommunicationDashboardState>();
        return "Communications";
        this.visitedTabs.add(this.activeTab);
        this.stateChangeSubject.complete();
    public onTabChange(tabId: string): void {
        this.activeTab = tabId;
        const index = this.navigationItems.indexOf(tabId);
        this.selectedIndex = index >= 0 ? index : 0;
            SharedService.Instance.InvokeManualResize();
        this.visitedTabs.add(tabId);
    public hasVisited(tabId: string): boolean {
        return this.visitedTabs.has(tabId);
        }, 1000);
        this.stateChangeSubject.pipe(
            debounceTime(50)
        ).subscribe(state => {
            this.UserStateChanged.emit(state);
        const state: CommunicationDashboardState = {
            activeTab: this.activeTab
        this.stateChangeSubject.next(state);
    public loadUserState(state: Partial<CommunicationDashboardState>): void {
        if (state.activeTab) {
            this.activeTab = state.activeTab;
            const index = this.navigationItems.indexOf(state.activeTab);
            this.visitedTabs.add(state.activeTab);
    initDashboard(): void {
            console.error('Error initializing Communication dashboard:', error);
            this.Error.emit(new Error('Failed to initialize Communication dashboard. Please try again.'));
    loadData(): void {
        if (this.Config?.userState) {
                    this.loadUserState(this.Config.userState);
    public getCurrentTabLabel(): string {
        const tabIndex = this.navigationItems.indexOf(this.activeTab);
        const labels = ['Monitor', 'Logs', 'Providers', 'Templates', 'Runs', 'Settings'];
        return tabIndex >= 0 ? labels[tabIndex] : 'Communication Management';
