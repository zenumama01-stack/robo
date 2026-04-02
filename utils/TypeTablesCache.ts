import { MJEntityRelationshipDisplayComponentEntity } from "../generated/entity_subclasses";
 * Simple cache for commonly used type tables across the system that are outside of what we have in the Metadata object
export class TypeTablesCache extends BaseEngine<TypeTablesCache> {
                EntityName: 'MJ: Entity Relationship Display Components',
                PropertyName: '_EntityRelationshipDisplayComponents',
    public static get Instance(): TypeTablesCache {
        return super.getInstance<TypeTablesCache>();
    public get EntityRelationshipDisplayComponents() {
        return this._EntityRelationshipDisplayComponents;
    private _EntityRelationshipDisplayComponents: MJEntityRelationshipDisplayComponentEntity[] = [];
