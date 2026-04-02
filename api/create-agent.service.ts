import { Injectable, ComponentRef, ApplicationRef, createComponent, EnvironmentInjector } from '@angular/core';
import { CreateAgentConfig, CreateAgentResult } from '../components/create-agent-panel.component';
import { CreateAgentDialogComponent } from '../components/create-agent-dialog.component';
import { CreateAgentSlideInComponent } from '../components/create-agent-slidein.component';
 * Result returned when the create agent dialog/slide-in closes.
export interface CreateAgentDialogResult {
    /** The result from the panel (if agent was created) */
    Result?: CreateAgentResult;
    /** Whether the dialog was closed without creating */
    Cancelled: boolean;
 * Service for programmatically opening create agent dialogs and slide-ins.
 * This service dynamically creates and manages the dialog/slide-in components,
 * handling their lifecycle and emitting results via observables.
 * constructor(private createAgentService: CreateAgentService) {}
 * openCreateDialog() {
 *     this.createAgentService.OpenDialog({ Title: 'Create Agent' })
 *         .subscribe(result => {
 *             if (!result.Cancelled && result.Result) {
 *                 console.log('Agent created:', result.Result.Agent);
 *             }
 *         });
 * openCreateSlideIn() {
 *     this.createAgentService.OpenSlideIn({
 *         ParentAgentId: 'abc123',
 *         ParentAgentName: 'My Agent'
 *     }).subscribe(result => {
 *         // Handle result
export class CreateAgentService {
    private dialogRef: ComponentRef<CreateAgentDialogComponent> | null = null;
    private slideInRef: ComponentRef<CreateAgentSlideInComponent> | null = null;
        private injector: EnvironmentInjector
     * Opens the create agent dialog.
     * @param config Configuration options for the dialog
     * @returns Observable that emits when the dialog closes
    public OpenDialog(config: CreateAgentConfig = {}): Observable<CreateAgentDialogResult> {
        // Close any existing dialog
        const resultSubject = new Subject<CreateAgentDialogResult>();
        // Create component dynamically
        this.dialogRef = createComponent(CreateAgentDialogComponent, {
            environmentInjector: this.injector
        // Set inputs
        this.dialogRef.instance.Config = config;
        // Listen for events
        this.dialogRef.instance.Created.subscribe((result: CreateAgentResult) => {
            resultSubject.next({ Result: result, Cancelled: false });
        this.dialogRef.instance.Closed.subscribe(() => {
            // Only emit cancelled if we haven't already emitted a result
                resultSubject.next({ Cancelled: true });
        // Attach to DOM
        this.appRef.attachView(this.dialogRef.hostView);
        document.body.appendChild(this.dialogRef.location.nativeElement);
        this.dialogRef.changeDetectorRef.detectChanges();
     * Opens the create agent slide-in panel.
     * @param config Configuration options for the slide-in
     * @returns Observable that emits when the slide-in closes
    public OpenSlideIn(config: CreateAgentConfig = {}): Observable<CreateAgentDialogResult> {
        // Close any existing slide-in
        this.closeSlideIn();
        this.slideInRef = createComponent(CreateAgentSlideInComponent, {
        this.slideInRef.instance.Config = config;
        this.slideInRef.instance.Created.subscribe((result: CreateAgentResult) => {
        this.slideInRef.instance.Closed.subscribe(() => {
        this.appRef.attachView(this.slideInRef.hostView);
        document.body.appendChild(this.slideInRef.location.nativeElement);
        this.slideInRef.changeDetectorRef.detectChanges();
     * Convenience method to open dialog for creating a sub-agent.
     * @param parentAgentId The ID of the parent agent
     * @param parentAgentName The name of the parent agent (for display)
    public OpenSubAgentDialog(parentAgentId: string, parentAgentName: string): Observable<CreateAgentDialogResult> {
        return this.OpenDialog({
            ParentAgentId: parentAgentId,
            ParentAgentName: parentAgentName,
            Title: 'Create Sub-Agent'
     * Convenience method to open slide-in for creating a sub-agent.
    public OpenSubAgentSlideIn(parentAgentId: string, parentAgentName: string): Observable<CreateAgentDialogResult> {
        return this.OpenSlideIn({
     * Closes the currently open dialog (if any).
    public CloseDialog(): void {
     * Closes the currently open slide-in (if any).
    public CloseSlideIn(): void {
     * Closes all open dialogs and slide-ins.
    public CloseAll(): void {
     * Returns true if a dialog is currently open.
    public get IsDialogOpen(): boolean {
     * Returns true if a slide-in is currently open.
    public get IsSlideInOpen(): boolean {
        return this.slideInRef !== null;
    private closeDialog(): void {
            this.appRef.detachView(this.dialogRef.hostView);
            this.dialogRef.destroy();
    private closeSlideIn(): void {
        if (this.slideInRef) {
            this.appRef.detachView(this.slideInRef.hostView);
            this.slideInRef.destroy();
            this.slideInRef = null;
