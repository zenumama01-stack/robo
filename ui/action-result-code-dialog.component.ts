export interface ActionResultCodeDialogResult {
    ResultCode: MJActionResultCodeEntity;
 * A dialog component for editing action result codes.
 * This component provides a form for creating and editing action result codes.
 * <mj-action-result-code-dialog
 *   [ResultCode]="myResultCode"
 * </mj-action-result-code-dialog>
    selector: 'mj-action-result-code-dialog',
    templateUrl: './action-result-code-dialog.component.html',
    styleUrls: ['./action-result-code-dialog.component.css']
export class ActionResultCodeDialogComponent implements OnInit {
    private _resultCode!: MJActionResultCodeEntity;
    set ResultCode(value: MJActionResultCodeEntity) {
        this._resultCode = value;
            this.loadResultCodeValues();
    get ResultCode(): MJActionResultCodeEntity {
        return this._resultCode;
    @Output() Close = new EventEmitter<ActionResultCodeDialogResult>();
    public Code = '';
    public IsSuccess = false;
    private loadResultCodeValues(): void {
        if (this._resultCode) {
            this.Code = this._resultCode.ResultCode || '';
            this.Description = this._resultCode.Description || '';
            this.IsSuccess = this._resultCode.IsSuccess || false;
        // Update the result code entity with form values
        this._resultCode.ResultCode = this.Code;
        this._resultCode.Description = this.Description;
        this._resultCode.IsSuccess = this.IsSuccess;
        this.Close.emit({ ResultCode: this._resultCode, Save: true });
        this.Close.emit({ ResultCode: this._resultCode, Save: false });
    public get CanSave(): boolean {
        return !!this.Code && this.Code.trim().length > 0;
