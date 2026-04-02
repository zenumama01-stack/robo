import { BaseEntity, EntityDeleteOptions, IMetadataProvider, LogError, Metadata } from "@memberjunction/core";
import { RegisterClass, ValidationErrorInfo, ValidationResult } from "@memberjunction/global";
import { MJDashboardEntity } from "../generated/entity_subclasses";
import { DashboardEngine } from "../engines/dashboards";
@RegisterClass(BaseEntity, 'MJ: Dashboards')
export class DashboardEntityExtended extends MJDashboardEntity  {
    public NewRecord(): boolean {
            super.NewRecord();
            const defaultConfigDetails = {
                columns: 4,
                rowHeight: 150,
            const configJSON = JSON.stringify(defaultConfigDetails);
            this.Set("UIConfigDetails", configJSON);
            if(md.CurrentUser){
                this.Set("UserID", md.CurrentUser.ID);
        catch(error) {
            LogError("Error in NewRecord: ");
     * Override Validate to check dashboard permissions before save.
     * For new records, user must be authenticated.
     * For existing records, user must have edit permission.
        // Run base validation first
        // Check permission for save operation
        const md =  this.ProviderToUse as any as IMetadataProvider
        const currentUser = this.ContextCurrentUser || md.CurrentUser;
                'Permission',
                'You must be logged in to save a dashboard',
        // For existing records (not new), check edit permission
        if (this.IsSaved) {
            const permissions = DashboardEngine.Instance.GetDashboardPermissions(this.ID, currentUser.ID);
                    'You do not have permission to edit this dashboard',
                    this.ID
     * Override Delete to check dashboard permissions before deletion.
     * User must have delete permission (typically only owners can delete).
    public override async Delete(options?: EntityDeleteOptions): Promise<boolean> {
        const md = this.ProviderToUse as any as IMetadataProvider;
            LogError('Cannot delete dashboard: User not authenticated');
        // Check delete permission
        if (!permissions.CanDelete) {
            LogError(`User ${currentUser.ID} does not have permission to delete dashboard ${this.ID}`);
        // Permission granted, proceed with delete
