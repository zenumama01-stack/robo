import { UserInfo, RunView, LogStatus, RunViewResult } from "@memberjunction/core";
import { MJEntityDocumentEntity, MJEntityDocumentTypeEntity } from "@memberjunction/core-entities";
 * Simple caching class to load all Entity Documents and related data at once into memory
export class EntityDocumentCache {
    private static _instance: EntityDocumentCache;
    private _cache: { [key: string]: MJEntityDocumentEntity } = {};
    private _typeCache: { [key: string]: MJEntityDocumentTypeEntity } = {};
        // load up the cache
        this._cache = {};
        this._typeCache = {};
    public static get Instance(): EntityDocumentCache {
        if(!EntityDocumentCache._instance){
            EntityDocumentCache._instance = new EntityDocumentCache();
        return EntityDocumentCache._instance;
    public get IsLoaded(): boolean {
        return this._loaded;
    protected Cache(): { [key: string]: MJEntityDocumentEntity } {
        return this._cache;
    protected TypeCache(): { [key: string]: MJEntityDocumentTypeEntity } {
        return this._typeCache;
    public GetDocument(EntityDocumentID: string): MJEntityDocumentEntity | null {
        let document: MJEntityDocumentEntity = this._cache[EntityDocumentID];
        if (!document) {
            LogStatus(`EntityDocumentCache.GetDocument: Cache miss for EntityDocumentID: ${EntityDocumentID}`);
        return document || null;
    public GetFirstActiveDocumentForEntityByID(EntityID: string): MJEntityDocumentEntity | null {
        let documentType: MJEntityDocumentTypeEntity | null = this.GetDocumentTypeByName('Record Duplicate');
        if(!documentType){
        return Object.values(this._cache).find((ed: MJEntityDocumentEntity) => {
            ed.EntityID === EntityID && ed.Status === 'Active' && ed.TypeID === documentType.ID;
    public GetFirstActiveDocumentForEntityByName(EntityName: string): MJEntityDocumentEntity | null {
            ed.Entity === EntityName && ed.Status === 'Active' && ed.TypeID === documentType.ID;
    public GetDocumentByName(EntityDocumentName: string): MJEntityDocumentEntity | null {
        const toLower = EntityDocumentName.trim().toLowerCase();
        let document: MJEntityDocumentEntity = Object.values(this._cache).find((ed: MJEntityDocumentEntity) => {
            ed.Name.trim().toLowerCase() === toLower;
            LogStatus(`EntityDocumentCache.GetDocumentByName: Cache miss for EntityDocumentName: ${EntityDocumentName}`);
    public GetDocumentType(EntityDocumentTypeID: string): MJEntityDocumentTypeEntity | null {
        let documentType: MJEntityDocumentTypeEntity = this._typeCache[EntityDocumentTypeID];
        if (!documentType) {
            LogStatus(`EntityDocumentCache.GetDocument: Cache miss for EntityDocumentID: ${EntityDocumentTypeID}`);
        return documentType;
    public GetDocumentTypeByName(EntityDocumentTypeName: string): MJEntityDocumentTypeEntity | null {
        const toLower = EntityDocumentTypeName.trim().toLowerCase();
        let documentType: MJEntityDocumentTypeEntity = Object.values(this._typeCache).find((edt: MJEntityDocumentTypeEntity) => edt.Name.trim().toLowerCase() === toLower);
            LogStatus(`EntityDocumentCache.GetDocumentByName: Cache miss for EntityDocumentName: ${EntityDocumentTypeName}`);
        return documentType;;
    public SetCurrentUser(user: UserInfo) {
        this._contextUser = user;
    public async Refresh(forceRefresh: boolean, ContextUser?: UserInfo) {
        if(!forceRefresh && this._loaded){
        LogStatus('Refreshing Entity Document Cache');
        // now load up the cache with all the entity documents
        const results: RunViewResult[] = await rv.RunViews([
                EntityName: "MJ: Entity Documents",
                EntityName: "MJ: Entity Document Types",
        ], ContextUser || this._contextUser);
        if (results[0] && results[0].Success) {
            for (const entityDocument of results[0].Results) {
                this._cache[entityDocument.ID] = entityDocument;
        if (results[1] && results[1].Success) {
            for (const entityDocumentType of results[1].Results) {
                this._typeCache[entityDocumentType.ID] = entityDocumentType;
