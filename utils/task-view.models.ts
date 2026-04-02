 * View mode for task display
export type TaskViewMode = 'simple' | 'gantt';
 * Gantt bar data for Frappe Gantt
export interface GanttTask {
  dependencies?: string;
  custom_class?: string;
