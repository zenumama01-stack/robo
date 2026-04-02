import { ResourcePermissionEngine } from "./ResourcePermissionEngine";
export class ResourceData {
    constructor(data: any = null) {
            this.ID = data.ID;
            this.Name = data.Name;
            this.ResourceTypeID = data.ResourceTypeID;
            this.ResourceRecordID = data.ResourceRecordID;
            this.Configuration = data.Configuration;
    public ID!: number;
    public Name!: string;
    public ResourceTypeID!: string;
    public ResourceRecordID!: any; 
    public Configuration: any;
     * Returns the name of the resource type based on the ResourceTypeID
    public get ResourceType(): string {
        return rt ? rt.Name : '';
    public get ResourceIcon(): string {
        return rt && rt.Icon ? rt.Icon : '';
