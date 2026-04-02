 * Analysis-related types for DBAutoDoc
import { TableDefinition } from './state.js';
export interface DependencyGraph {
  nodes: Map<string, TableNode>;
  levels: TableNode[][];
export interface TableNode {
  fullName: string;
  dependsOn: TableNode[];
  dependents: TableNode[];
  tableDefinition?: TableDefinition;
export interface BackpropagationTrigger {
  sourceTable: string;
  targetTable: string;
  insight: string;
export interface ConvergenceResult {
  converged: boolean;
  iterationsPerformed: number;
export interface AnalysisMetrics {
  tablesAnalyzed: number;
  columnsAnalyzed: number;
  averageConfidence: number;
  lowConfidenceCount: number;
  backpropagationCount: number;
  iterationCount: number;
export interface TableAnalysisContext {
  columns: any[];
  dependsOn: any[];
  dependents: any[];
  sampleData: any[];
  parentDescriptions?: ParentTableDescription[];
  userNotes?: string;
  seedContext?: any;
  allTables?: Array<{ schema: string; name: string }>;
export interface ParentTableDescription {
export interface AnalysisResult {
  tableDescription?: string;
  columnDescriptions?: ColumnDescriptionResult[];
  inferredBusinessDomain?: string;
export interface ColumnDescriptionResult {
