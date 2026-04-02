import { FormState, FormSectionState, DEFAULT_FORM_STATE, DEFAULT_SECTION_STATE } from './form-state.interface';
const SETTING_KEY_PREFIX = 'Form.State.';
 * Service for managing form state persistence per entity.
 * State is stored in the User Settings entity and shared reactively
 * across all form instances for the same entity.
export class FormStateService {
    /** Cache of BehaviorSubjects per entity name */
    private stateCache = new Map<string, BehaviorSubject<FormState>>();
    /** Track which entities have been loaded from DB */
    private loadedEntities = new Set<string>();
    /** Track loading promises to prevent duplicate loads */
    private loadingPromises = new Map<string, Promise<void>>();
     * Get the observable state for an entity.
     * Automatically loads from User Settings on first access.
     * @returns Observable of the form state
    getState$(entityName: string): Observable<FormState> {
        return this.getOrCreateSubject(entityName).asObservable();
     * Get the current state value for an entity.
     * @returns Current form state
    getCurrentState(entityName: string): FormState {
        return this.getOrCreateSubject(entityName).value;
     * Initialize state for an entity by loading from User Settings.
     * Call this when a form component initializes.
    async initializeState(entityName: string): Promise<FormState> {
        // If already loaded, return current state
        if (this.loadedEntities.has(entityName)) {
            return this.getCurrentState(entityName);
        // If currently loading, wait for that promise
        const existingPromise = this.loadingPromises.get(entityName);
        if (existingPromise) {
            await existingPromise;
        const loadPromise = this.loadState(entityName);
        this.loadingPromises.set(entityName, loadPromise);
            await loadPromise;
            this.loadedEntities.add(entityName);
            this.loadingPromises.delete(entityName);
     * Get section state, returning defaults if section doesn't exist yet.
     * @param sectionKey The section key
     * @returns Section state with defaults applied
    getSectionState(entityName: string, sectionKey: string): FormSectionState {
        const state = this.getCurrentState(entityName);
        return state.sections[sectionKey] || { ...DEFAULT_SECTION_STATE };
     * Check if a section is expanded.
     * @param defaultExpanded Optional default value to use when no persisted state exists (defaults to DEFAULT_SECTION_STATE.isExpanded)
     * @returns True if expanded
    isSectionExpanded(entityName: string, sectionKey: string, defaultExpanded?: boolean): boolean {
        const sectionState = state.sections[sectionKey];
        if (sectionState) {
            return sectionState.isExpanded;
        // No persisted state - use provided default or fall back to DEFAULT_SECTION_STATE
        return defaultExpanded !== undefined ? defaultExpanded : DEFAULT_SECTION_STATE.isExpanded;
     * Set section expanded state.
     * @param isExpanded Whether the section is expanded
    setSectionExpanded(entityName: string, sectionKey: string, isExpanded: boolean): void {
        this.updateSectionState(entityName, sectionKey, { isExpanded });
     * Get the persisted panel height for a section.
     * @returns Panel height in pixels, or undefined if no custom height is set
    getSectionPanelHeight(entityName: string, sectionKey: string): number | undefined {
        return state.sections[sectionKey]?.panelHeight;
     * Set the panel height for a section (persisted to User Settings).
     * @param height Panel height in pixels
    setSectionPanelHeight(entityName: string, sectionKey: string, height: number): void {
        this.updateSectionState(entityName, sectionKey, { panelHeight: height });
     * Toggle section expanded state.
    toggleSection(entityName: string, sectionKey: string): void {
        const current = this.isSectionExpanded(entityName, sectionKey);
        this.setSectionExpanded(entityName, sectionKey, !current);
     * Get form width mode.
     * @returns Width mode ('centered' or 'full-width')
    getWidthMode(entityName: string): 'centered' | 'full-width' {
        return this.getCurrentState(entityName).widthMode;
     * Set form width mode.
     * @param widthMode The width mode
    setWidthMode(entityName: string, widthMode: 'centered' | 'full-width'): void {
        const subject = this.getOrCreateSubject(entityName);
        const currentState = subject.value;
        const newState: FormState = {
            ...currentState,
            widthMode
        subject.next(newState);
        this.queueSave(entityName);
     * Toggle form width mode between centered and full-width.
    toggleWidthMode(entityName: string): void {
        const current = this.getWidthMode(entityName);
        this.setWidthMode(entityName, current === 'centered' ? 'full-width' : 'centered');
     * Get showEmptyFields preference for an entity.
     * @returns Whether to show empty fields
    getShowEmptyFields(entityName: string): boolean {
        return this.getCurrentState(entityName).showEmptyFields;
     * Set showEmptyFields preference for an entity.
     * @param show Whether to show empty fields
    setShowEmptyFields(entityName: string, show: boolean): void {
            showEmptyFields: show
     * Expand all sections for an entity.
     * @param sectionKeys Array of all section keys to expand
    expandAllSections(entityName: string, sectionKeys: string[]): void {
        const newSections = { ...currentState.sections };
        for (const key of sectionKeys) {
            newSections[key] = {
                ...DEFAULT_SECTION_STATE,
                ...newSections[key],
        subject.next({ ...currentState, sections: newSections });
     * Collapse all sections for an entity.
     * @param sectionKeys Array of all section keys to collapse
    collapseAllSections(entityName: string, sectionKeys: string[]): void {
     * Reset state to defaults for an entity.
    resetToDefaults(entityName: string): void {
        subject.next({ ...DEFAULT_FORM_STATE });
     * Get the custom section order for an entity.
     * @returns Array of section keys in user's preferred order, or undefined if using default order
    getSectionOrder(entityName: string): string[] | undefined {
        return this.getCurrentState(entityName).sectionOrder;
     * Set the custom section order for an entity.
     * @param sectionOrder Array of section keys in the desired order
    setSectionOrder(entityName: string, sectionOrder: string[]): void {
            sectionOrder
     * Reset the section order to default (removes custom ordering).
    resetSectionOrder(entityName: string): void {
        const { sectionOrder: _, ...stateWithoutOrder } = currentState;
        subject.next(stateWithoutOrder as FormState);
     * Check if a custom section order exists for an entity.
     * @returns True if custom order exists
    hasCustomSectionOrder(entityName: string): boolean {
        const order = this.getSectionOrder(entityName);
        return order !== undefined && order.length > 0;
     * Get the count of expanded sections.
     * @param sectionKeys Array of section keys to check
     * @returns Number of expanded sections
    getExpandedCount(entityName: string, sectionKeys: string[]): number {
        return sectionKeys.filter(key => {
            const section = state.sections[key];
            return section ? section.isExpanded : DEFAULT_SECTION_STATE.isExpanded;
    // -------------------- Private Methods --------------------
     * Get or create the BehaviorSubject for an entity.
    private getOrCreateSubject(entityName: string): BehaviorSubject<FormState> {
        let subject = this.stateCache.get(entityName);
            subject = new BehaviorSubject<FormState>({ ...DEFAULT_FORM_STATE });
            this.stateCache.set(entityName, subject);
        return subject;
     * Update a single section's state.
    private updateSectionState(entityName: string, sectionKey: string, updates: Partial<FormSectionState>): void {
        const currentSection = currentState.sections[sectionKey] || { ...DEFAULT_SECTION_STATE };
            sections: {
                ...currentState.sections,
                [sectionKey]: {
                    ...currentSection,
                    ...updates
     * Generate the User Settings key for an entity.
    private getSettingKey(entityName: string): string {
        return `${SETTING_KEY_PREFIX}${entityName}`;
     * Load state from User Settings using UserInfoEngine for cached access.
    private async loadState(entityName: string): Promise<void> {
            const settingKey = this.getSettingKey(entityName);
                const savedState = JSON.parse(setting.Value) as Partial<FormState>;
                subject.next({ ...DEFAULT_FORM_STATE, ...savedState });
            console.warn(`Failed to load form state for ${entityName}:`, error);
            // Keep default state on error
     * Queue a debounced save using UserInfoEngine's centralized debounce.
    private queueSave(entityName: string): void {
        UserInfoEngine.Instance.SetSettingDebounced(settingKey, JSON.stringify(state));
