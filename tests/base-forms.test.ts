 * Tests for base-forms package:
 * - BaseFormSectionInfo
 * - FormState interfaces and defaults
 * - FormStateService (state management logic)
  ContentChild: () => () => {},
  ContentChildren: () => () => {},
    CurrentUser = { ID: 'user-123' };
  EntityFieldInfo: class {},
      UserSettings: [],
      SetSettingDebounced: vi.fn(),
// ======================= BaseFormSectionInfo =======================
describe('BaseFormSectionInfo', () => {
  let BaseFormSectionInfo: typeof import('../base-form-section-info').BaseFormSectionInfo;
    const mod = await import('../base-form-section-info');
    BaseFormSectionInfo = mod.BaseFormSectionInfo;
  it('should create with required parameters', () => {
    const info = new BaseFormSectionInfo('details', 'Details');
    expect(info.sectionKey).toBe('details');
    expect(info.sectionName).toBe('Details');
    expect(info.isExpanded).toBe(false);
    expect(info.rowCount).toBeUndefined();
    expect(info.metadata).toBeUndefined();
  it('should create with all parameters', () => {
    const info = new BaseFormSectionInfo('related', 'Related Records', true, 42, { custom: true });
    expect(info.sectionKey).toBe('related');
    expect(info.sectionName).toBe('Related Records');
    expect(info.isExpanded).toBe(true);
    expect(info.rowCount).toBe(42);
    expect(info.metadata).toEqual({ custom: true });
  it('should default isExpanded to false', () => {
    const info = new BaseFormSectionInfo('section', 'Section');
// ======================= FormState defaults =======================
describe('FormState defaults', () => {
  it('should have expected default values', async () => {
    const { DEFAULT_FORM_STATE, DEFAULT_SECTION_STATE } = await import('../form-state.interface');
    expect(DEFAULT_FORM_STATE.sections).toEqual({});
    expect(DEFAULT_FORM_STATE.showEmptyFields).toBe(false);
    expect(DEFAULT_FORM_STATE.widthMode).toBe('centered');
    expect(DEFAULT_FORM_STATE.sectionOrder).toBeUndefined();
    expect(DEFAULT_SECTION_STATE.isExpanded).toBe(true);
// ======================= FormStateService =======================
describe('FormStateService', () => {
  let service: InstanceType<typeof import('../form-state.service').FormStateService>;
    const mod = await import('../form-state.service');
    service = new mod.FormStateService();
  describe('getCurrentState', () => {
    it('should return default state for unknown entity', () => {
      const state = service.getCurrentState('MJTestEntity');
      expect(state.sections).toEqual({});
      expect(state.showEmptyFields).toBe(false);
      expect(state.widthMode).toBe('centered');
  describe('getSectionState', () => {
    it('should return default section state for unknown section', () => {
      const sectionState = service.getSectionState('MJTestEntity', 'details');
      expect(sectionState.isExpanded).toBe(true);
  describe('isSectionExpanded', () => {
    it('should return default when no persisted state', () => {
      expect(service.isSectionExpanded('MJTestEntity', 'details')).toBe(true);
    it('should respect custom default when no persisted state', () => {
      expect(service.isSectionExpanded('MJTestEntity', 'details', false)).toBe(false);
    it('should return persisted value when state exists', () => {
      service.setSectionExpanded('MJTestEntity', 'details', false);
      expect(service.isSectionExpanded('MJTestEntity', 'details')).toBe(false);
  describe('setSectionExpanded', () => {
    it('should update section expanded state', () => {
      service.setSectionExpanded('MJTestEntity', 'details', true);
  describe('toggleSection', () => {
    it('should toggle section expanded state', () => {
      // Default is expanded (true)
      expect(service.isSectionExpanded('MJTestEntity', 'sec1')).toBe(true);
      service.toggleSection('MJTestEntity', 'sec1');
      expect(service.isSectionExpanded('MJTestEntity', 'sec1')).toBe(false);
  describe('widthMode', () => {
    it('should default to centered', () => {
      expect(service.getWidthMode('MJTestEntity')).toBe('centered');
    it('should set width mode', () => {
      service.setWidthMode('MJTestEntity', 'full-width');
      expect(service.getWidthMode('MJTestEntity')).toBe('full-width');
    it('should toggle width mode', () => {
      service.toggleWidthMode('MJTestEntity');
  describe('showEmptyFields', () => {
      expect(service.getShowEmptyFields('MJTestEntity')).toBe(false);
    it('should set showEmptyFields', () => {
      service.setShowEmptyFields('MJTestEntity', true);
      expect(service.getShowEmptyFields('MJTestEntity')).toBe(true);
  describe('expandAllSections / collapseAllSections', () => {
    it('should expand all sections', () => {
      const keys = ['sec1', 'sec2', 'sec3'];
      // Collapse some first
      service.setSectionExpanded('MJTestEntity', 'sec1', false);
      service.setSectionExpanded('MJTestEntity', 'sec2', false);
      service.expandAllSections('MJTestEntity', keys);
      keys.forEach(key => {
        expect(service.isSectionExpanded('MJTestEntity', key)).toBe(true);
    it('should collapse all sections', () => {
      service.collapseAllSections('MJTestEntity', keys);
        expect(service.isSectionExpanded('MJTestEntity', key)).toBe(false);
  describe('sectionOrder', () => {
    it('should return undefined by default', () => {
      expect(service.getSectionOrder('MJTestEntity')).toBeUndefined();
    it('should set custom section order', () => {
      const order = ['sec3', 'sec1', 'sec2'];
      service.setSectionOrder('MJTestEntity', order);
      expect(service.getSectionOrder('MJTestEntity')).toEqual(order);
    it('should detect custom section order', () => {
      expect(service.hasCustomSectionOrder('MJTestEntity')).toBe(false);
      service.setSectionOrder('MJTestEntity', ['sec1', 'sec2']);
      expect(service.hasCustomSectionOrder('MJTestEntity')).toBe(true);
    it('should reset section order', () => {
      service.resetSectionOrder('MJTestEntity');
  describe('getExpandedCount', () => {
    it('should count expanded sections', () => {
      // Default is all expanded
      expect(service.getExpandedCount('MJTestEntity', keys)).toBe(3);
      expect(service.getExpandedCount('MJTestEntity', keys)).toBe(2);
    it('should reset all state for entity', () => {
      service.resetToDefaults('MJTestEntity');
      expect(service.getCurrentState('MJTestEntity').sections).toEqual({});
  describe('getState$ (observable)', () => {
    it('should emit state changes', async () => {
      const states: Array<{ widthMode: string }> = [];
      const sub = service.getState$('MJTestEntity').subscribe(s => states.push(s));
      // Initial state emission
      expect(states.length).toBeGreaterThanOrEqual(1);
      expect(states[0].widthMode).toBe('centered');
      expect(states[states.length - 1].widthMode).toBe('full-width');
