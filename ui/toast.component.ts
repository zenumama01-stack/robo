import { ToastService, Toast } from '../../services/toast.service';
  selector: 'mj-toast',
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css'],
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
export class ToastComponent implements OnInit, OnDestroy {
  public toasts: Toast[] = [];
  private subscription: Subscription | null = null;
  constructor(private toastService: ToastService) {}
    this.subscription = this.toastService.toasts$.subscribe(toasts => {
      this.toasts = toasts;
   * Get the icon class for a toast type
  public getIconClass(type: string): string {
      case 'warning':
      case 'info':
        return 'fa-solid fa-circle-info';
   * Dismiss a toast
  public dismiss(toastId: string): void {
    this.toastService.dismiss(toastId);
   * Track toasts by ID for performance
  public trackByToastId(index: number, toast: Toast): string {
