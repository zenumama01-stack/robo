import { IMetadataProvider } from '@memberjunction/core';
 * Enhanced dialog wrapper component for the data context viewer
  selector: 'mj-data-context-dialog',
  templateUrl: './ng-data-context-dialog.component.html',
  styleUrls: ['./ng-data-context-dialog.component.css']
export class DataContextDialogComponent {
  @Output() dialogClosed = new EventEmitter();
  @Input() dataContextId!: string;
  @Input() dataContextName?: string;
  public isMaximized: boolean = false;
  public get dialogWidth(): number {
    return this.isMaximized ? window.innerWidth * 0.95 : 900;
  public get dialogHeight(): number {
    return this.isMaximized ? window.innerHeight * 0.95 : 700;
    this.dialogClosed.emit();
  toggleMaximize(): void {
