    selector: 'mj-json-viewer-window',
        <div class="json-dialog-content">
            <div class="json-dialog-header">
                <button class="custom-button icon-only" (click)="copyJsonContent()" title="Copy JSON">
            <div class="json-dialog-body">
                    [(ngModel)]="jsonContent"
        .json-dialog-content {
        .json-dialog-header {
        .json-dialog-body {
export class JsonViewerWindowComponent {
    @Input() jsonContent: string = '';
    copyJsonContent() {
        if (this.jsonContent) {
            navigator.clipboard.writeText(this.jsonContent).then(() => {
