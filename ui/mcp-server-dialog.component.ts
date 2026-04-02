 * @fileoverview MCP Server Dialog Component
 * Dialog for creating and editing MCP server configurations.
import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { MCPServerData } from '../mcp-dashboard.component';
 * Transport type options
export const TRANSPORT_TYPES = [
    { value: 'StreamableHTTP', label: 'Streamable HTTP', description: 'HTTP-based transport with streaming support' },
    { value: 'SSE', label: 'Server-Sent Events', description: 'SSE-based transport for real-time updates' },
    { value: 'Stdio', label: 'Standard I/O', description: 'For local subprocess communication' },
    { value: 'WebSocket', label: 'WebSocket', description: 'Full-duplex WebSocket connection' }
 * Auth type options
export const AUTH_TYPES = [
    { value: 'None', label: 'None', description: 'No authentication required' },
    { value: 'Bearer', label: 'Bearer Token', description: 'Authorization header with Bearer token' },
    { value: 'APIKey', label: 'API Key', description: 'API key in custom header' },
    { value: 'OAuth2', label: 'OAuth 2.0', description: 'OAuth 2.0 authentication flow' },
    { value: 'Basic', label: 'Basic Auth', description: 'Username and password authentication' },
    { value: 'Custom', label: 'Custom', description: 'Custom authentication scheme' }
export interface ServerDialogResult {
    server?: MCPServerData;
 * MCP Server Dialog Component
    selector: 'mj-mcp-server-dialog',
    templateUrl: './mcp-server-dialog.component.html',
    styleUrls: ['./mcp-server-dialog.component.css']
export class MCPServerDialogComponent implements OnInit, OnChanges {
    @Input() server: MCPServerData | null = null;
    @Output() close = new EventEmitter<ServerDialogResult>();
    public serverForm: FormGroup;
    public transportTypes = TRANSPORT_TYPES;
    public authTypes = AUTH_TYPES;
        return !!this.server?.ID;
        return this.IsEditMode ? 'Edit MCP Server' : 'Add MCP Server';
    public get SelectedTransportType(): string {
        return this.serverForm?.get('TransportType')?.value ?? 'StreamableHTTP';
    public get RequiresURL(): boolean {
        const transport = this.SelectedTransportType;
        return transport === 'StreamableHTTP' || transport === 'SSE' || transport === 'WebSocket';
    public get RequiresCommand(): boolean {
        return this.SelectedTransportType === 'Stdio';
    public get SelectedAuthType(): string {
        return this.serverForm?.get('DefaultAuthType')?.value ?? 'None';
    public get IsOAuth2(): boolean {
        return this.SelectedAuthType === 'OAuth2';
        this.serverForm = this.createForm();
        if (changes['server'] || changes['visible']) {
            TransportType: ['StreamableHTTP', Validators.required],
            ServerURL: [''],
            Command: [''],
            CommandArgs: [''],
            DefaultAuthType: ['None', Validators.required],
            RateLimitPerMinute: [null, [Validators.min(0)]],
            RateLimitPerHour: [null, [Validators.min(0)]],
            RequestTimeoutMs: [60000, [Validators.min(1000), Validators.max(600000)]],
            Status: ['Active'],
            OAuthIssuerURL: [''],
            OAuthScopes: [''],
            OAuthMetadataCacheTTLMinutes: [1440, [Validators.min(5)]],
            OAuthClientID: [''],
            OAuthClientSecretEncrypted: ['']
        if (this.server) {
            this.serverForm.patchValue({
                Name: this.server.Name,
                Description: this.server.Description ?? '',
                TransportType: this.server.TransportType,
                ServerURL: this.server.ServerURL ?? '',
                Command: this.server.Command ?? '',
                CommandArgs: '',  // Would need to load from entity
                DefaultAuthType: this.server.DefaultAuthType,
                RateLimitPerMinute: this.server.RateLimitPerMinute,
                RateLimitPerHour: this.server.RateLimitPerHour,
                RequestTimeoutMs: 60000,
                Status: this.server.Status,
                OAuthIssuerURL: this.server.OAuthIssuerURL ?? '',
                OAuthScopes: this.server.OAuthScopes ?? '',
                OAuthMetadataCacheTTLMinutes: this.server.OAuthMetadataCacheTTLMinutes ?? 1440,
                OAuthClientID: this.server.OAuthClientID ?? '',
                OAuthClientSecretEncrypted: this.server.OAuthClientSecretEncrypted ?? ''
            this.serverForm.reset({
                ServerURL: '',
                Command: '',
                CommandArgs: '',
                RateLimitPerMinute: null,
                RateLimitPerHour: null,
                OAuthIssuerURL: '',
                OAuthScopes: '',
                OAuthMetadataCacheTTLMinutes: 1440,
                OAuthClientID: '',
                OAuthClientSecretEncrypted: ''
        this.updateValidators();
    private updateValidators(): void {
        const urlControl = this.serverForm.get('ServerURL');
        const commandControl = this.serverForm.get('Command');
        const oauthIssuerControl = this.serverForm.get('OAuthIssuerURL');
        if (this.RequiresURL) {
            urlControl?.setValidators([Validators.required]);
            commandControl?.clearValidators();
        } else if (this.RequiresCommand) {
            commandControl?.setValidators([Validators.required]);
            urlControl?.clearValidators();
        // OAuth2 requires an issuer URL
        if (this.IsOAuth2) {
            oauthIssuerControl?.setValidators([Validators.required]);
            oauthIssuerControl?.clearValidators();
        urlControl?.updateValueAndValidity();
        commandControl?.updateValueAndValidity();
        oauthIssuerControl?.updateValueAndValidity();
    public onTransportTypeChange(): void {
    public onAuthTypeChange(): void {
        if (this.serverForm.invalid) {
            this.serverForm.markAllAsTouched();
            if (this.IsEditMode && this.server) {
                await entity.InnerLoad(new CompositeKey([{ FieldName: 'ID', Value: this.server.ID }]));
            const formValue = this.serverForm.value;
            entity.TransportType = formValue.TransportType;
            entity.ServerURL = formValue.ServerURL || null;
            entity.Command = formValue.Command || null;
            entity.CommandArgs = formValue.CommandArgs || null;
            entity.DefaultAuthType = formValue.DefaultAuthType;
            entity.RateLimitPerMinute = formValue.RateLimitPerMinute || null;
            entity.RateLimitPerHour = formValue.RateLimitPerHour || null;
            entity.RequestTimeoutMs = formValue.RequestTimeoutMs || 60000;
            // OAuth configuration fields (only set if OAuth2 is selected)
            if (formValue.DefaultAuthType === 'OAuth2') {
                entity.OAuthIssuerURL = formValue.OAuthIssuerURL || null;
                entity.OAuthScopes = formValue.OAuthScopes || null;
                entity.OAuthMetadataCacheTTLMinutes = formValue.OAuthMetadataCacheTTLMinutes || 1440;
                entity.OAuthClientID = formValue.OAuthClientID || null;
                entity.OAuthClientSecretEncrypted = formValue.OAuthClientSecretEncrypted || null;
                // Clear OAuth fields when not using OAuth2
                entity.OAuthIssuerURL = null;
                entity.OAuthScopes = null;
                entity.OAuthMetadataCacheTTLMinutes = null;
                entity.OAuthClientID = null;
                entity.OAuthClientSecretEncrypted = null;
                throw new Error('Failed to save server');
            this.ErrorMessage = `Failed to save: ${error instanceof Error ? error.message : String(error)}`;
        const control = this.serverForm.get(controlName);
