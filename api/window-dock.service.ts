import { Injectable, Component, ComponentRef, ApplicationRef, Injector, createComponent } from '@angular/core';
export interface DockItem {
    windowId: string;
    iconUrl?: string;
    restoreCallback: () => void;
    progress?: number; // 0-100 for progress indicator
    selector: 'mj-window-dock',
    imports: [],
        <div class="window-dock">
            @for (item of dockItems; track item.windowId) {
                <div class="dock-item" 
                     [title]="item.title"
                     (click)="restoreWindow(item)">
                    <span class="dock-item-label">{{ getTruncatedTitle(item.title) }}</span>
        .window-dock {
            border: 1px solid #d0d8e0;
        .dock-item {
                background-color: rgba(255, 255, 255, 0.8);
                color: #2c5282;
            .dock-item-label {
                color: #4a5568;
            .dock-item-progress {
            .dock-item-progress-bar {
                background: #0076B6; /* MJ blue color */
            /* Pulse animation for indeterminate progress (50%) */
            .dock-item-progress-bar[style*="width: 50%"] {
                animation: pulse-progress 1.5s ease-in-out infinite;
        @keyframes pulse-progress {
            0% { opacity: 0.6; width: 30%; }
            50% { opacity: 1; width: 70%; }
            100% { opacity: 0.6; width: 30%; }
export class WindowDockComponent {
    dockItems: DockItem[] = [];
    addItem(item: DockItem) {
        this.dockItems.push(item);
    removeItem(windowId: string) {
        this.dockItems = this.dockItems.filter(item => item.windowId !== windowId);
    restoreWindow(item: DockItem) {
        item.restoreCallback();
        this.removeItem(item.windowId);
    getTruncatedTitle(title: string): string {
        // Remove "Test: " prefix for the label to save space
        const cleanTitle = title.startsWith('Test: ') ? title.substring(6) : title;
        // Truncate to first 8 characters for the label
        return cleanTitle.length > 8 ? cleanTitle.substring(0, 8) + '...' : cleanTitle;
export class WindowDockService {
    private dockComponent?: ComponentRef<WindowDockComponent>;
    private ensureDockExists() {
        if (!this.dockComponent) {
            // Create dock component
            this.dockComponent = createComponent(WindowDockComponent, {
                elementInjector: this.injector
            document.body.appendChild(this.dockComponent.location.nativeElement);
            this.appRef.attachView(this.dockComponent.hostView);
    addWindow(windowId: string, title: string, icon?: string, restoreCallback?: () => void, iconUrl?: string, progress?: number) {
        this.ensureDockExists();
        if (this.dockComponent) {
            this.dockComponent.instance.addItem({
                windowId,
                iconUrl,
                restoreCallback: restoreCallback || (() => {}),
    removeWindow(windowId: string) {
            this.dockComponent.instance.removeItem(windowId);
            // If no more items, remove the dock
            if (this.dockComponent.instance.dockItems.length === 0) {
                this.appRef.detachView(this.dockComponent.hostView);
                this.dockComponent.destroy();
                this.dockComponent = undefined;
    updateWindowProgress(windowId: string, progress: number | undefined) {
            const item = this.dockComponent.instance.dockItems.find(i => i.windowId === windowId);
                item.progress = progress;
