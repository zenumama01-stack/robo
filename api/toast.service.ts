export type ToastType = 'success' | 'error' | 'warning' | 'info';
export interface Toast {
export class ToastService {
  public toasts$: Observable<Toast[]> = this.toastsSubject.asObservable();
   * Show a toast notification
    // Auto-dismiss after duration
        this.dismiss(toast.id);
   * Show a success toast
   * Show an error toast
   * Show a warning toast
   * Show an info toast
   * Dismiss a specific toast by ID
   * Clear all toasts
