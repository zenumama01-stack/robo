import { ActionResult } from '../action-test-harness/action-test-harness.component';
 * A dialog wrapper component for the Action Test Harness.
 * This component provides a modal dialog experience without Kendo dependencies.
 * <mj-action-test-harness-dialog
 *   [Action]="myAction"
 *   [ActionParams]="myParams"
 *   (Close)="onDialogClose()"
 *   (ExecutionComplete)="onExecutionComplete($event)">
 * </mj-action-test-harness-dialog>
    selector: 'mj-action-test-harness-dialog',
    templateUrl: './action-test-harness-dialog.component.html',
    styleUrls: ['./action-test-harness-dialog.component.css']
export class ActionTestHarnessDialogComponent implements OnInit {
    private _action!: MJActionEntity;
    private _actionParams: MJActionParamEntity[] = [];
    set Action(value: MJActionEntity) {
        this._action = value;
    get Action(): MJActionEntity {
        return this._action;
    set ActionParams(value: MJActionParamEntity[]) {
        this._actionParams = value || [];
    get ActionParams(): MJActionParamEntity[] {
        return this._actionParams;
    @Output() ExecutionComplete = new EventEmitter<ActionResult>();
        // Any initialization if needed
        // Only close if clicking directly on backdrop, not on dialog content
            this.OnClose();
    public OnExecutionComplete(result: ActionResult): void {
        this.ExecutionComplete.emit(result);
