import { BaseLMSAction } from '../base/base-lms.action';
import { LearnWorldsBaseAction } from '../providers/learnworlds/learnworlds-base.action';
// Concrete subclass for testing BaseLMSAction
class TestLMSAction extends BaseLMSAction {
    protected lmsProvider = 'TestLMS';
    protected integrationName = 'TestLMS';
describe('BaseLMSAction', () => {
    let action: TestLMSAction;
        action = new TestLMSAction();
    describe('calculateProgressPercentage', () => {
        it('should calculate percentage correctly', () => {
            expect(action['calculateProgressPercentage'](75, 100)).toBe(75);
            expect(action['calculateProgressPercentage'](1, 3)).toBe(33);
            expect(action['calculateProgressPercentage'](0, 0)).toBe(0);
            expect(action['calculateProgressPercentage'](50, 50)).toBe(100);
    describe('mapEnrollmentStatus', () => {
        it('should map active statuses', () => {
            expect(action['mapEnrollmentStatus']('active')).toBe('active');
        it('should map completed statuses', () => {
            expect(action['mapEnrollmentStatus']('completed')).toBe('completed');
            expect(action['mapEnrollmentStatus']('finished')).toBe('completed');
        it('should map expired statuses', () => {
            expect(action['mapEnrollmentStatus']('expired')).toBe('expired');
        it('should map suspended statuses', () => {
            expect(action['mapEnrollmentStatus']('suspended')).toBe('suspended');
            expect(action['mapEnrollmentStatus']('paused')).toBe('suspended');
            expect(action['mapEnrollmentStatus']('inactive')).toBe('suspended');
            expect(action['mapEnrollmentStatus']('pending')).toBe('unknown');
            expect(action['mapEnrollmentStatus']('review')).toBe('unknown');
    describe('formatLMSDate', () => {
            expect(action['formatLMSDate'](date)).toBe('2024-06-15T10:30:00.000Z');
    describe('parseLMSDate', () => {
            const result = action['parseLMSDate']('2024-06-15T10:30:00Z');
    describe('buildLMSErrorMessage', () => {
            expect(action['buildLMSErrorMessage']('GetCourse', 'Not found'))
                .toBe('LMS operation failed: GetCourse. Not found');
            const result = action['buildLMSErrorMessage']('GetCourse', 'Failed', new Error('timeout'));
    describe('getCommonLMSParams', () => {
            const params = action['getCommonLMSParams']();
            process.env['BIZAPPS_TESTLMS_COMP1_API_KEY'] = 'key123';
            delete process.env['BIZAPPS_TESTLMS_COMP1_API_KEY'];
            const params = [{ Name: 'CourseID', Value: 'c1', Type: 'Input' as const }];
            expect(action['getParamValue'](params, 'CourseID')).toBe('c1');
describe('LearnWorldsBaseAction', () => {
    class TestLearnWorldsAction extends LearnWorldsBaseAction {
    let action: TestLearnWorldsAction;
        action = new TestLearnWorldsAction();
    describe('parseLearnWorldsDate', () => {
            const result = action['parseLearnWorldsDate']('2024-06-15T10:30:00Z');
            const result = action['parseLearnWorldsDate'](1718444400);
    describe('formatLearnWorldsDate', () => {
            expect(action['formatLearnWorldsDate'](date)).toBe('2024-06-15T10:30:00.000Z');
    describe('mapUserStatus', () => {
        it('should map active status', () => {
            expect(action['mapUserStatus']('active')).toBe('active');
        it('should map inactive status', () => {
            expect(action['mapUserStatus']('inactive')).toBe('inactive');
        it('should map suspended/blocked statuses', () => {
            expect(action['mapUserStatus']('suspended')).toBe('suspended');
            expect(action['mapUserStatus']('blocked')).toBe('suspended');
        it('should return inactive for unknown statuses', () => {
            expect(action['mapUserStatus']('unknown')).toBe('inactive');
    describe('mapLearnWorldsEnrollmentStatus', () => {
        it('should return completed when completed is true', () => {
            expect(action['mapLearnWorldsEnrollmentStatus']({ completed: true, active: true })).toBe('completed');
        it('should return expired when expired is true', () => {
            expect(action['mapLearnWorldsEnrollmentStatus']({ expired: true, active: true })).toBe('expired');
        it('should return suspended when suspended is true', () => {
            expect(action['mapLearnWorldsEnrollmentStatus']({ suspended: true, active: false })).toBe('suspended');
        it('should return suspended when not active', () => {
            expect(action['mapLearnWorldsEnrollmentStatus']({ active: false })).toBe('suspended');
        it('should return active when active and not completed/expired/suspended', () => {
            expect(action['mapLearnWorldsEnrollmentStatus']({ active: true })).toBe('active');
    describe('calculateProgress', () => {
        it('should extract progress data', () => {
            const result = action['calculateProgress']({
                percentage: 75,
                completed_units: 15,
                total_units: 20,
                time_spent: 3600
            expect(result.percentage).toBe(75);
            expect(result.completedUnits).toBe(15);
            expect(result.totalUnits).toBe(20);
            expect(result.timeSpent).toBe(3600);
        it('should default missing values to 0', () => {
            const result = action['calculateProgress']({});
            expect(result.percentage).toBe(0);
            expect(result.completedUnits).toBe(0);
            expect(result.totalUnits).toBe(0);
            expect(result.timeSpent).toBe(0);
