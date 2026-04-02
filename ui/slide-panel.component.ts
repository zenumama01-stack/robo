import { SlidePanelMode } from '../types';
    selector: 'mj-slide-panel',
    templateUrl: './slide-panel.component.html',
    styleUrls: ['./slide-panel.component.css'],
export class MjSlidePanelComponent implements OnInit, OnDestroy {
    @Input() Mode: SlidePanelMode = 'slide';
    @Input() Title = '';
    @Input() MinWidthPx = 400;
    @Input() MaxWidthRatio = 0.92;
    /** Initial width in pixels. Defaults to 65% of viewport for slide, 800px for dialog. */
    set WidthPx(value: number) {
        this._widthPx = value;
    get WidthPx(): number {
        return this._widthPx;
    private _widthPx = 0;
        private elRef: ElementRef
        if (this._widthPx === 0) {
            this._widthPx = this.Mode === 'dialog'
                ? 800
                : Math.max(this.MinWidthPx, Math.min(window.innerWidth * 0.65, 1000));
        if (!this.Resizable || this.Mode === 'dialog') return;
        const maxWidth = viewportWidth * this.MaxWidthRatio;
        this._widthPx = Math.max(this.MinWidthPx, Math.min(maxWidth, viewportWidth - event.clientX));
