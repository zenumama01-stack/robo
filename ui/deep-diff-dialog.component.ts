 * Dialog wrapper component for the deep diff viewer
  selector: 'mj-deep-diff-dialog',
  templateUrl: './deep-diff-dialog.component.html',
  styleUrls: ['./deep-diff-dialog.component.css']
export class DeepDiffDialogComponent {
  @Input() oldValue: any;
  @Input() newValue: any;
  @Input() title: string = 'Deep Diff Analysis';
  @Input() showSummary: boolean = true;
  @Input() showUnchanged: boolean = false;
  @Input() expandAll: boolean = false;
  @Input() maxDepth: number = 10;
  @Input() maxStringLength: number = 100;
  @Input() treatNullAsUndefined: boolean = false;
  @Input() width: string = '80%';
  @Input() height: string = '80vh';
  public get dialogWidth(): string {
    return this.isMaximized ? '95vw' : this.width;
  public get dialogHeight(): string {
    return this.isMaximized ? '95vh' : this.height;
