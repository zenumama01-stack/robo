 * Represents a system validation issue with its severity level
export interface SystemValidationIssue {
  details?: string;
  severity: 'error' | 'warning' | 'info';
  help?: string;
 * Service for checking system validation issues and displaying error messages to users
export class SystemValidationService {
  private _validationIssues = new BehaviorSubject<SystemValidationIssue[]>([]);
  public validationIssues$: Observable<SystemValidationIssue[]> = this._validationIssues.asObservable();
  constructor() { }
   * Adds a new validation issue to the list
  public addIssue(issue: Omit<SystemValidationIssue, 'timestamp'>): void {
      const newIssue: SystemValidationIssue = {
        ...issue,
      const currentIssues = this._validationIssues.getValue();
      // Don't add duplicates with the same ID
      if (!currentIssues.some(i => i.id === issue.id)) {
        this._validationIssues.next([...currentIssues, newIssue]);
        LogError(`System Validation Issue: ${issue.message} (${issue.id})`);
      console.error('Error adding validation issue', err);
   * Removes a validation issue by id
  public removeIssue(id: string): void {
      const updatedIssues = currentIssues.filter(issue => issue.id !== id);
      if (updatedIssues.length !== currentIssues.length) {
        this._validationIssues.next(updatedIssues);
      console.error('Error removing validation issue', err);
   * Clears all validation issues
  public clearIssues(): void {
    this._validationIssues.next([]);
   * Checks if there are any validation issues with error severity
  public hasErrors(): boolean {
    return this._validationIssues.getValue().some(issue => issue.severity === 'error');
