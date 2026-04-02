import { BaseEntity, EntitySaveOptions, Metadata, RunView, LogError, IMetadataProvider } from "@memberjunction/core";
import { MJArtifactVersionEntity, MJArtifactEntity, MJArtifactTypeEntity, MJArtifactVersionAttributeEntity, ArtifactExtractor } from "@memberjunction/core-entities";
@RegisterClass(BaseEntity, "MJ: Artifact Versions")
export class ArtifactVersionExtended extends MJArtifactVersionEntity {
    private _pendingAttributes: any[] | null = null;
    private _loadedAttributes: MJArtifactVersionAttributeEntity[] | null = null;
     * Gets the attributes for this artifact version
     * Returns loaded attributes if available, empty array otherwise
     * Use InnerLoad() to automatically load attributes from database
     * After Save(), returns the newly created attributes from extraction
    public get Attributes(): MJArtifactVersionAttributeEntity[] {
        return this._loadedAttributes || [];
     * Override InnerLoad to automatically load attributes after loading the version
    public override async InnerLoad(compositeKey: any, EntityRelationshipsToLoad?: string[]): Promise<boolean> {
        // Load the base entity first
        const loaded = await super.InnerLoad(compositeKey, EntityRelationshipsToLoad);
        // Automatically load attributes for this version
                ExtraFilter: `ArtifactVersionID='${this.ID}'`,
                this._loadedAttributes = result.Results;
                this._loadedAttributes = [];
            LogError(`Error loading attributes for artifact version ${this.ID}: ${error instanceof Error ? error.message : error}`);
     * Overrides base save method to:
     * 1) Automatically calculate the ContentHash field whenever the Content field is changed
     * 2) Automatically extract attributes from the Content field using the ExtractRules from the ArtifactType
        const contentDirty = this.IsSaved === false || this.Fields.find(f => f.Name === "Content")?.Dirty;
        if (contentDirty && this.Content) {
                // 1. Calculate ContentHash using SHA-256
                this.ContentHash = this.CalculateContentHash(this.Content);
                // 2. Extract attributes (sets Name/Description and stores pending attributes)
                await this.ExtractAndSaveAttributes();
                LogError(`Error in ArtifactVersionExtended.Save pre-processing: ${error instanceof Error ? error.message : error}`);
        // Save the main entity
        const saveResult = await super.Save(options);
        // After successful save, create ArtifactVersionAttribute records
        if (saveResult && this._pendingAttributes && this._pendingAttributes.length > 0) {
                const savedAttributes = await this.SaveAttributeRecords(this._pendingAttributes);
                // Populate loaded attributes with the newly created ones
                this._loadedAttributes = savedAttributes;
                this._pendingAttributes = null; // Clear pending attributes
                LogError(`Error saving ArtifactVersionAttribute records: ${error instanceof Error ? error.message : error}`);
                // Don't fail the save if attribute creation fails
     * Calculates SHA-256 hash of content
     * @param content - The content to hash
    protected CalculateContentHash(content: string): string {
        return createHash('sha256').update(content, 'utf8').digest('hex');
     * Extracts attributes from Content using ArtifactType ExtractRules and saves them
    protected async ExtractAndSaveAttributes(): Promise<void> {
        if (!this.Content) {
            return; // Nothing to extract
        // Load the parent Artifact to get TypeID
        const artifact = await md.GetEntityObject<MJArtifactEntity>('MJ: Artifacts', this.ContextCurrentUser);
        const loadedArtifact = await artifact.Load(this.ArtifactID);
        if (!loadedArtifact) {
            throw new Error(`Failed to load Artifact with ID: ${this.ArtifactID}`);
        // Load the ArtifactType hierarchy (child to parent)
        const artifactTypeChain = await this.LoadArtifactTypeHierarchy(artifact.TypeID);
        if (artifactTypeChain.length === 0) {
            return; // No extract rules to apply
        // Resolve extract rules with inheritance
        const extractRules = ArtifactExtractor.ResolveExtractRules(artifactTypeChain);
        if (extractRules.length === 0) {
            return; // No rules defined
        // Extract attributes
        const extractionResult = await ArtifactExtractor.ExtractAttributes({
            content: this.Content,
            extractRules: extractRules,
            throwOnError: false, // Don't throw - continue with partial results
            verbose: false
        // Set Name and Description from standard properties
        const nameValue = ArtifactExtractor.GetStandardProperty(extractionResult.attributes, 'name');
        if (nameValue !== null && typeof nameValue === 'string') {
            this.Name = nameValue;
        const descriptionValue = ArtifactExtractor.GetStandardProperty(extractionResult.attributes, 'description');
        if (descriptionValue !== null && typeof descriptionValue === 'string') {
            this.Description = descriptionValue;
        // We need to save the ArtifactVersion first to get an ID before saving attributes
        // So we'll do this in a post-save hook. For now, store the extraction result
        // so we can save attributes after the main save completes
        this._pendingAttributes = extractionResult.attributes;
     * Saves extracted attributes as ArtifactVersionAttribute records
     * Updates existing attributes, creates new ones, and deletes removed ones
     * @param attributes - Extracted attributes to save
     * @returns Array of saved attribute entities
    protected async SaveAttributeRecords(attributes: any[]): Promise<MJArtifactVersionAttributeEntity[]> {
        if (!this.ID) {
            throw new Error('Cannot save attributes: ArtifactVersion ID is not set');
        if (!attributes || attributes.length === 0) {
        const operations: Promise<boolean>[] = [];
        const savedAttributes: MJArtifactVersionAttributeEntity[] = [];
        // Load existing attributes for this version
        const existingAttrsResult = await rv.RunView<MJArtifactVersionAttributeEntity>({
        const existingAttrs = existingAttrsResult?.Success ? (existingAttrsResult.Results || []) : [];
        const existingAttrMap = new Map(existingAttrs.map(attr => [attr.Name, attr]));
        // Serialize attributes for storage
        const serialized = ArtifactExtractor.SerializeForStorage(attributes);
        // Track which attributes we've processed
        const processedNames = new Set<string>();
        // Update existing or create new attributes
        for (const attrData of serialized) {
            processedNames.add(attrData.name);
            const existingAttr = existingAttrMap.get(attrData.name);
            // Null value means delete the attribute if it exists
            if (attrData.value === null || attrData.value === undefined) {
                if (existingAttr) {
                    operations.push(existingAttr.Delete());
            // Update existing attribute
                existingAttr.Type = attrData.type;
                existingAttr.Value = attrData.value;
                existingAttr.StandardProperty = (attrData.standardProperty as 'name' | 'description' | 'displayMarkdown' | 'displayHtml' | null) || null;
                operations.push(existingAttr.Save());
                savedAttributes.push(existingAttr);
                // Create new attribute
                const newAttr = await md.GetEntityObject<MJArtifactVersionAttributeEntity>(
                    'MJ: Artifact Version Attributes',
                newAttr.NewRecord();
                newAttr.ArtifactVersionID = this.ID;
                newAttr.Name = attrData.name;
                newAttr.Type = attrData.type;
                newAttr.Value = attrData.value;
                newAttr.StandardProperty = (attrData.standardProperty as 'name' | 'description' | 'displayMarkdown' | 'displayHtml' | null) || null;
                operations.push(newAttr.Save());
                savedAttributes.push(newAttr);
        // Delete attributes that are no longer in the serialized data
        for (const [name, attr] of existingAttrMap) {
            if (!processedNames.has(name)) {
                operations.push(attr.Delete());
        // Execute all operations in parallel
        await Promise.all(operations);
        return savedAttributes;
     * Loads the ArtifactType hierarchy from child to root parent
     * @param typeId - Starting ArtifactType ID
     * @returns Array of ArtifactType entities from child to parent
    protected async LoadArtifactTypeHierarchy(typeId: string): Promise<MJArtifactTypeEntity[]> {
        const hierarchy: MJArtifactTypeEntity[] = [];
        let currentTypeId: string | null = typeId;
        // Walk up the hierarchy (max 10 levels to prevent infinite loops)
        let maxDepth = 10;
        while (currentTypeId && maxDepth > 0) {
            const artifactType = await md.GetEntityObject<MJArtifactTypeEntity>(
                'MJ: Artifact Types',
            const loaded = await artifactType.Load(currentTypeId);
                break; // Type not found, stop traversal
            hierarchy.push(artifactType);
            currentTypeId = artifactType.ParentID; // Move to parent
            maxDepth--;
        return hierarchy;
