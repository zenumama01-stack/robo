import { Injectable, ViewContainerRef, ComponentRef } from '@angular/core';
import { WindowService, WindowRef, WindowSettings } from '@progress/kendo-angular-dialog';
import { AITestHarnessWindowComponent, AITestHarnessWindowData } from './ai-test-harness-window.component';
export interface TestResult {
 * Uses Kendo Window component which provides maximize and close functionality.
 * Note: Kendo Window created via WindowService does not include a minimize button
 * in the titlebar. The window supports maximize/restore and close actions only.
 * To add minimize functionality, a custom window component with custom titlebar
 * would need to be implemented.
export class TestHarnessWindowService {
    private openWindows = new Map<string, WindowRef>();
        const data: AITestHarnessWindowData = {
        return this.openWindow(data, options.viewContainerRef);
        console.log('🎯 openPromptTestHarness called with options:', options);
        console.log('📌 promptRunId:', options.promptRunId);
            promptRunId: options.promptRunId,
        console.log('📦 Final AITestHarnessWindowData:', data);
     * Opens a test harness window
    private openWindow(data: AITestHarnessWindowData, viewContainerRef?: ViewContainerRef): Observable<TestResult> {
            title: data.title || 'AI Test Harness',
            content: AITestHarnessWindowComponent,
            width: this.convertToNumber(data.width) || 1200,
            height: this.convertToNumber(data.height) || 800,
            state: 'default',
            cssClass: 'test-harness-window-wrapper'
            windowSettings.appendTo = viewContainerRef;
        const windowRef = this.windowService.open(windowSettings);
        this.openWindows.set(windowId, windowRef);
        // Pass data to the component
        const componentRef = windowRef.content as ComponentRef<AITestHarnessWindowComponent>;
        if (componentRef && componentRef.instance) {
                // Don't complete the observable here, as the window.result will handle it
        // Handle window close via close button
        windowRef.result.subscribe({
                    error: error.message || 'Test failed'
        const windowRef = this.openWindows.get(windowId);
        if (windowRef) {
                windowRef.close();
                console.error('Error closing window:', error);
                // Force delete from map even if close fails
        this.openWindows.forEach((windowRef, id) => {
        this.openWindows.clear();
     * Helper method to convert string dimensions to pixel numbers
