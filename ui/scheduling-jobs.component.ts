  JobStatistics,
  selector: 'app-scheduling-jobs',
  templateUrl: './scheduling-jobs.component.html',
  styleUrls: ['./scheduling-jobs.component.css'],
export class SchedulingJobsComponent implements OnInit, OnDestroy {
  public Jobs: JobStatistics[] = [];
  public FilteredJobs: JobStatistics[] = [];
  // Slideout state
  public SlideoutOpen = false;
  public SlideoutMode: 'create' | 'edit' = 'create';
  public SelectedJob: JobStatistics | null = null;
  public SlideoutWidth = 620;
  // Resize state
  private isResizing = false;
  public TypeFilter = '';
  public StatusOptions = ['', 'Active', 'Paused', 'Disabled', 'Pending', 'Expired'];
  public TypeOptions: string[] = [''];
  // Settings keys
  private static readonly PANEL_WIDTH_KEY = 'Scheduling.SlideoutPanelWidth';
  private static readonly SEARCH_STATE_KEY = 'Scheduling.JobsSearchState';
  private typeSubject = new BehaviorSubject<string>('');
      this.schedulingService.jobStatistics$.subscribe(jobs => {
        this.Jobs = jobs;
        this.updateTypeOptions();
      const widthStr = UserInfoEngine.Instance.GetSetting(SchedulingJobsComponent.PANEL_WIDTH_KEY);
      if (widthStr) {
        const width = parseInt(widthStr, 10);
        if (!isNaN(width) && width >= 400 && width <= 900) {
          this.SlideoutWidth = width;
      const stateStr = UserInfoEngine.Instance.GetSetting(SchedulingJobsComponent.SEARCH_STATE_KEY);
        if (state.typeFilter) this.TypeFilter = state.typeFilter;
      console.warn('[SchedulingJobs] Failed to load user settings:', error);
        typeFilter: this.TypeFilter
        SchedulingJobsComponent.SEARCH_STATE_KEY,
      console.warn('[SchedulingJobs] Failed to persist search state:', error);
      if (this.initialState['typeFilter']) this.TypeFilter = this.initialState['typeFilter'] as string;
        this.typeSubject.pipe(distinctUntilChanged())
    this.typeSubject.next(this.TypeFilter);
  public OnTypeFilterChange(type: string): void {
    this.TypeFilter = type;
    this.typeSubject.next(type);
  // ── Resize ──────────────────────────────────────────────
  public OnResizeStart(event: MouseEvent): void {
    this.isResizing = true;
    this.resizeStartWidth = this.SlideoutWidth;
    this.SlideoutWidth = Math.max(400, Math.min(900, this.resizeStartWidth + delta));
    this.isResizing = false;
      SchedulingJobsComponent.PANEL_WIDTH_KEY,
      String(this.SlideoutWidth)
  public OpenCreateSlideout(): void {
    this.SelectedJob = null;
    this.SlideoutMode = 'create';
    this.SlideoutOpen = true;
  public OpenEditSlideout(job: JobStatistics): void {
    this.SelectedJob = job;
    this.SlideoutMode = 'edit';
  public CloseSlideout(): void {
    this.SlideoutOpen = false;
  public OnSlideoutSaved(): void {
    this.CloseSlideout();
  public async ToggleJobStatus(job: JobStatistics, event: MouseEvent): Promise<void> {
    const newStatus = job.status === 'Active' ? 'Paused' : 'Active';
    await this.schedulingService.updateJobStatus(job.jobId, newStatus as 'Active' | 'Paused');
  public OpenEntityRecord(job: JobStatistics): void {
    compositeKey.LoadFromSingleKeyValuePair('ID', job.jobId);
      case 'Paused': return 'status-paused';
      case 'Pending': return 'status-pending';
      case 'Expired': return 'status-expired';
  public GetTypeIcon(typeName: string): string {
    const lower = typeName.toLowerCase();
  public FormatDate(date: Date | undefined): string {
    let filtered = [...this.Jobs];
      filtered = filtered.filter(j =>
        j.jobName.toLowerCase().includes(term) ||
        j.jobType.toLowerCase().includes(term) ||
        (j.description && j.description.toLowerCase().includes(term))
      filtered = filtered.filter(j => j.status === this.StatusFilter);
    if (this.TypeFilter) {
      filtered = filtered.filter(j => j.jobType === this.TypeFilter);
    this.FilteredJobs = filtered;
  private updateTypeOptions(): void {
    const types = new Set(this.Jobs.map(j => j.jobType));
    this.TypeOptions = ['', ...Array.from(types).sort()];
