 * @fileoverview Credential Dialog Component
 * A dialog wrapper for the CredentialEditPanelComponent that provides
 * a convenient way to create or edit credentials in a modal dialog.
import { Component, Input, Output, EventEmitter, ChangeDetectorRef, ChangeDetectionStrategy, ViewChild, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { MJCredentialEntity, MJCredentialTypeEntity } from '@memberjunction/core-entities';
import { CredentialEditPanelComponent } from '../panels/credential-edit-panel/credential-edit-panel.component';
 * Configuration options for opening the credential dialog
export interface CredentialDialogOptions {
    /** The credential to edit, or null for creating a new credential */
    credential?: MJCredentialEntity | null;
    /** Pre-selected credential type ID for new credentials */
    preselectedTypeId?: string;
    /** Pre-selected category ID for new credentials */
    preselectedCategoryId?: string;
    /** Dialog title override */
    /** Dialog width in pixels */
 * Result returned when the dialog closes
export interface CredentialDialogResult {
    /** Whether a credential was saved or deleted */
    /** The saved credential entity (if save was successful) */
    credential?: MJCredentialEntity;
    /** The ID of the deleted credential (if delete was successful) */
    deletedId?: string;
    /** The action that was taken */
    action: 'saved' | 'deleted' | 'cancelled';
    selector: 'mj-credential-dialog',
                <div class="loading-backdrop">
                    [credential]="Credential"
                    [credentialTypes]="credentialTypes"
                    [isOpen]="Visible"
                    (saved)="onSaved($event)"
                    (deleted)="onDeleted($event)"
                </mj-credential-edit-panel>
        .loading-backdrop {
export class CredentialDialogComponent implements OnInit, OnChanges {
    @Input() Credential: MJCredentialEntity | null = null;
    @Input() PreselectedTypeId: string | undefined;
    @Input() PreselectedCategoryId: string | undefined;
    @Input() Title: string | undefined;
    @Input() Width = 600;
    @Output() close = new EventEmitter<CredentialDialogResult>();
    private _dialogTitle = 'Credential';
        if (this.Visible) {
            this.loadCredentialTypes();
        if (changes['Visible'] && changes['Visible'].currentValue === true) {
            // When dialog becomes visible, load credential types and initialize the panel
            this.loadCredentialTypes().then(() => {
                // Wait for next change detection cycle to ensure panel is rendered
                // (panel is only rendered when IsLoading is false)
                // Use setTimeout to ensure ViewChild is resolved after render
                        this.editPanel.open(this.Credential, this.PreselectedTypeId, this.PreselectedCategoryId);
    public get dialogTitle(): string {
        if (this.Title) {
            return this.Title;
        return this.Credential?.ID ? 'Edit Credential' : 'Create Credential';
     * Opens the dialog with the specified options
    public async open(options?: CredentialDialogOptions): Promise<void> {
        this.Credential = options?.credential ?? null;
        this.PreselectedTypeId = options?.preselectedTypeId;
        this.PreselectedCategoryId = options?.preselectedCategoryId;
        this.Title = options?.title;
        if (options?.width) {
            this.Width = options.width;
        this.Visible = true;
        await this.loadCredentialTypes();
        // Initialize the edit panel after types are loaded
            await this.editPanel.open(this.Credential, this.PreselectedTypeId, this.PreselectedCategoryId);
    private async loadCredentialTypes(): Promise<void> {
        if (this.credentialTypes.length > 0) {
            return; // Already loaded
            const result = await rv.RunView<MJCredentialTypeEntity>({
                this.credentialTypes = result.Results;
    public onSaved(credential: MJCredentialEntity): void {
        this.close.emit({
            action: 'saved'
    public onDeleted(credentialId: string): void {
            deletedId: credentialId,
            action: 'deleted'
            action: 'cancelled'
