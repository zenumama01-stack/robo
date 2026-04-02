import { Component, Input, Output, EventEmitter, OnInit, ViewChild, OnDestroy, AfterViewInit, Renderer2, ElementRef, ChangeDetectorRef } from '@angular/core';
import { WindowComponent } from '@progress/kendo-angular-dialog';
export interface CustomWindowData {
    selector: 'mj-test-harness-custom-window',
            #kendoWindow
            [title]="windowTitle"
            [width]="width"
            [height]="height"
            [top]="windowTop"
            [left]="windowLeft"
            [minWidth]="isMinimized ? 400 : 800"
            [minHeight]="isMinimized ? 60 : 600"
            [resizable]="!isMinimized"
            [state]="windowState"
            [class.minimized-window]="isMinimized"
            (close)="onClose()"
            (resize)="onWindowResize()">
                <div class="window-title">
                    @if (mode === 'agent' && agent?.LogoURL) {
                        <img [src]="agent?.LogoURL" class="title-logo" alt="Agent logo" />
                    } @else if (mode === 'agent') {
                        <i class="fa-solid fa-robot title-icon"></i>
                        <i class="fa-solid fa-comment-dots title-icon"></i>
                    <span>{{ windowTitle }}</span>
                <div class="window-actions">
                        (click)="minimize()"
                        title="Minimize"
                        class="window-action-btn">
                        <i class="fa-solid fa-window-minimize"></i>
                        (click)="toggleMaximize()"
                        [title]="isMaximized ? 'Restore' : 'Maximize'"
                        <i class="fa-solid" [class.fa-window-maximize]="!isMaximized" [class.fa-window-restore]="isMaximized"></i>
                        (click)="closeButtonClick()"
                        (runOpened)="onRunOpened($event)">
        /* Remove animation for snappy performance
        ::ng-deep .window-transition {
            transition: all 0.6s ease-in-out !important;
        ::ng-deep .minimized-window {
            .k-window-content {
            .k-window-titlebar {
        .window-title {
            .title-logo {
                object-fit: contain;
        .window-actions {
        .window-action-btn {
                background-color: rgba(0, 0, 0, 0.08);
        /* Ensure Kendo Window content wrapper maintains proper height */
        ::ng-deep .k-window-content {
export class TestHarnessCustomWindowComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('kendoWindow', { static: false }) kendoWindow!: WindowComponent;
    @ViewChild(AITestHarnessComponent, { static: false }) testHarness?: AITestHarnessComponent;
    @Input() data: CustomWindowData = {};
    @Output() minimizeWindow = new EventEmitter<void>();
    @Output() restoreWindow = new EventEmitter<void>();
    @Output() executionStateChange = new EventEmitter<{ windowId?: string; isExecuting: boolean }>();
    windowTop: number = 100;
    windowLeft: number = 100;
    windowState: 'default' | 'minimized' | 'maximized' = 'default';
    isMaximized = false;
    isMinimized = false;
    // Store original dimensions for restore
    private originalWidth: number = 1200;
    private originalHeight: number = 800;
    private originalTop: number = 100;
    private originalLeft: number = 100;
    // Minimized dimensions
    private readonly MINIMIZED_WIDTH = 400;
    private readonly MINIMIZED_HEIGHT = 60;
        // Store original dimensions
        this.originalWidth = this.width;
        this.originalHeight = this.height;
        // Calculate centered position
        const windowWidth = window.innerWidth;
        const windowHeight = window.innerHeight;
        this.windowLeft = Math.max(0, (windowWidth - this.width) / 2);
        this.windowTop = Math.max(0, (windowHeight - this.height) / 2);
        // Store original position
        this.originalLeft = this.windowLeft;
        this.originalTop = this.windowTop;
                    this.windowTitle = this.data.title || `Test: ${this.agent.Name}`;
                    this.windowTitle = this.data.title || `Test: ${this.prompt.Name}`;
        // When the Kendo Window's close event fires (from the default X button),
        // emit our closeWindow event to notify the window manager
    closeButtonClick() {
        // When our custom close button is clicked, emit the close event
        // This will trigger the window to close and call our onClose handler
        if (this.kendoWindow) {
            this.kendoWindow.close.emit();
            // If kendoWindow is not available, directly emit the close event
    onStateChange(state: 'default' | 'minimized' | 'maximized') {
        this.windowState = state;
        this.isMaximized = state === 'maximized';
        this.isMinimized = state === 'minimized';
        // Handle restore from minimized
        if (this.isMinimized && state !== 'minimized') {
            this.restoreFromMinimized();
        // Adjust content height on any state change
        if (state !== 'minimized') {
            setTimeout(() => this.adjustContentHeight(), 100);
    onWindowResize() {
        // Handle Kendo Window resize event
        if (!this.isMinimized) {
            // Use a small delay to ensure the DOM has updated
            setTimeout(() => this.adjustContentHeight(), 50);
    onRunOpened(event: { runId: string; runType: 'agent' | 'prompt' }) {
        // Auto-minimize the test harness window when a run is opened
        this.minimize();
    minimize() {
            // Store current dimensions before minimizing
            // Hide the window when minimized (dock will show icon)
            this.isMinimized = true;
            this.minimizeWindow.emit();
            // Hide the window element
            let windowElement = this.elementRef.nativeElement.querySelector('.k-window');
            if (!windowElement && this.elementRef.nativeElement.closest) {
                windowElement = this.elementRef.nativeElement.closest('.k-window');
            if (windowElement) {
                this.renderer.setStyle(windowElement, 'display', 'none');
    restoreFromMinimized() {
        this.width = this.originalWidth;
        this.height = this.originalHeight;
        this.windowTop = this.originalTop;
        this.windowLeft = this.originalLeft;
        this.windowState = 'default';
        this.isMinimized = false;
        // Show the window element
            this.renderer.setStyle(windowElement, 'display', 'block');
        // Update position after state change
            this.updateWindowPosition();
            // Force Angular change detection
            // Force the Kendo Window to recalculate its internal dimensions
                // Trigger resize event to fix content sizing
                // Use the shared method to adjust content height
                this.adjustContentHeight();
        // Emit restore event
        this.restoreWindow.emit();
    toggleMaximize() {
        if (this.isMinimized) {
            // First restore from minimized, then maximize
                this.windowState = 'maximized';
                this.isMaximized = true;
            }, 600);
            this.windowState = this.isMaximized ? 'default' : 'maximized';
            this.isMaximized = !this.isMaximized;
        // Set initial position if needed
        // Set up execution tracking
        this.setupExecutionTracking();
        // Set up window resize listener
        this.setupResizeListener();
    private adjustContentHeight() {
        // Ensure the window content wrapper has proper height
        const contentWrapper = this.elementRef.nativeElement.querySelector('.k-window-content');
        if (contentWrapper && this.kendoWindow) {
            // Get the actual window element to check current dimensions
            const windowElement = this.elementRef.nativeElement.querySelector('.k-window') ||
                                 this.elementRef.nativeElement.closest('.k-window');
            // Update our tracked dimensions if the window has been resized
                const rect = windowElement.getBoundingClientRect();
                if (rect.width > 0) this.width = rect.width;
                if (rect.height > 0) this.height = rect.height;
            // Calculate the actual content height (window height minus titlebar and some padding)
            const windowHeight = this.height;
            const titlebarHeight = 40; // Titlebar height
            const bottomPadding = 10; // Extra space to prevent clipping
            const contentHeight = windowHeight - titlebarHeight - bottomPadding;
            this.renderer.setStyle(contentWrapper, 'height', `${contentHeight}px`);
            this.renderer.setStyle(contentWrapper, 'display', 'flex');
            this.renderer.setStyle(contentWrapper, 'flex-direction', 'column');
        // Also update the test harness container
        const testHarnessContainer = this.elementRef.nativeElement.querySelector('mj-ai-test-harness');
        if (testHarnessContainer) {
            this.renderer.setStyle(testHarnessContainer, 'flex', '1');
            this.renderer.setStyle(testHarnessContainer, 'height', '100%');
            this.renderer.setStyle(testHarnessContainer, 'overflow', 'hidden');
    private setupResizeListener() {
        // Debounced resize handler
        let resizeTimeout: any;
        const handleResize = () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
        // Listen for window resize events
        // Store the handler for cleanup
        (this as any).resizeHandler = handleResize;
        // Note: Kendo Window resize events are handled by the (resize) event binding in the template
    private setupExecutionTracking() {
        // Use a timer to check the test harness execution state
        // This is needed because the test harness doesn't emit events for execution state changes
            // Initial state
            let lastExecutingState = false;
            // Check execution state periodically
            const checkInterval = setInterval(() => {
                if (this.testHarness && this.testHarness.isExecuting !== lastExecutingState) {
                    lastExecutingState = this.testHarness.isExecuting;
                    this.executionStateChange.emit({ isExecuting: lastExecutingState });
            // Store interval for cleanup
            (this as any).executionCheckInterval = checkInterval;
        // Clean up execution tracking interval
        if ((this as any).executionCheckInterval) {
            clearInterval((this as any).executionCheckInterval);
        // Clean up resize listener
        if ((this as any).resizeHandler) {
            window.removeEventListener('resize', (this as any).resizeHandler);
        // Ensure window is properly closed and cleaned up
    private updateWindowPosition() {
        // Find the window element - it might be in the parent if we're inside a wrapper
        if (windowElement && (this.windowTop !== undefined || this.windowLeft !== undefined)) {
            if (this.windowTop !== undefined) {
                this.renderer.setStyle(windowElement, 'top', `${this.windowTop}px`);
            if (this.windowLeft !== undefined) {
                this.renderer.setStyle(windowElement, 'left', `${this.windowLeft}px`);
