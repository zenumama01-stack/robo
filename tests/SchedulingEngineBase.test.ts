    protected async Load(_configs: unknown[], _provider?: unknown, _forceRefresh?: boolean, _contextUser?: unknown) {
      // no-op for tests
  MJScheduledJobTypeEntity: class {},
  MJScheduledJobRunEntity: class {},
import { SchedulingEngineBase } from '../SchedulingEngineBase';
describe('SchedulingEngineBase', () => {
  let engine: SchedulingEngineBase;
    engine = new SchedulingEngineBase();
    it('should have empty arrays for all metadata collections', () => {
      expect(engine.ScheduledJobTypes).toEqual([]);
      expect(engine.ScheduledJobs).toEqual([]);
      expect(engine.ScheduledJobRuns).toEqual([]);
    it('should have default polling interval of 10 seconds', () => {
      expect(engine.ActivePollingInterval).toBe(10000);
  describe('GetJobTypeByName', () => {
    it('should return undefined when no job types loaded', () => {
      expect(engine.GetJobTypeByName('Agent')).toBeUndefined();
  describe('GetJobTypeByDriverClass', () => {
      expect(engine.GetJobTypeByDriverClass('AgentDriver')).toBeUndefined();
  describe('GetJobsByType', () => {
    it('should return empty array when no jobs loaded', () => {
      expect(engine.GetJobsByType('type-id')).toEqual([]);
  describe('GetRunsForJob', () => {
    it('should return empty array when no runs loaded', () => {
      expect(engine.GetRunsForJob('job-id')).toEqual([]);
  describe('UpdatePollingInterval', () => {
    it('should set interval to null when no jobs', () => {
      // ScheduledJobs starts empty
      expect(engine.ActivePollingInterval).toBeNull();
