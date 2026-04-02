import { BaseEngine, BaseEnginePropertyConfig, IMetadataProvider, IStartupSink, RegisterForStartup, UserInfo } from "@memberjunction/core";
import { DashboardEntityExtended } from "../custom/DashboardEntityExtended";
    MJDashboardCategoryLinkEntity
 * Represents the effective permissions a user has on a dashboard
export interface DashboardUserPermissions {
    /** The dashboard ID these permissions apply to */
    DashboardID: string;
    /** Whether the user can view the dashboard */
    /** Whether the user can modify the dashboard */
    /** Whether the user can delete the dashboard */
    /** Whether the user can share the dashboard with others */
    CanShare: boolean;
    /** Whether the user is the owner of the dashboard */
    IsOwner: boolean;
    /** Source of the permission: 'owner', 'direct', 'category', or 'none' */
    PermissionSource: 'owner' | 'direct' | 'category' | 'none';
 * Represents the effective permissions a user has on a dashboard category
export interface DashboardCategoryUserPermissions {
    /** The category ID these permissions apply to */
    /** Whether the user can view dashboards in the category */
    /** Whether the user can modify dashboards in the category */
    /** Whether the user can add/remove dashboards in the category */
    CanAddRemove: boolean;
    /** Whether the user can share the category with others */
    /** Whether the user is the owner of the category */
    /** Source of the permission: 'owner', 'direct', or 'none' */
    PermissionSource: 'owner' | 'direct' | 'none';
 * Caching of metadata for dashboards and related data, including permission management
export class DashboardEngine extends BaseEngine<DashboardEngine> {
    public static get Instance(): DashboardEngine {
       return super.getInstance<DashboardEngine>();
    private _dashboards: DashboardEntityExtended[] = [];
    private _partTypes: MJDashboardPartTypeEntity[] = [];
    private _dashboardUserPreferences: MJDashboardUserPreferenceEntity[] = [];
    private _dashboardCategories: MJDashboardCategoryEntity[] = [];
    private _dashboardUserStates: MJDashboardUserStateEntity[] = [];
    private _dashboardPermissions: MJDashboardPermissionEntity[] = [];
    private _dashboardCategoryPermissions: MJDashboardCategoryPermissionEntity[] = [];
    private _dashboardCategoryLinks: MJDashboardCategoryLinkEntity[] = [];
                EntityName: "MJ: Dashboard Part Types",
                PropertyName: "_partTypes",
                EntityName: 'MJ: Dashboards',
                PropertyName: "_dashboards",
                PropertyName: "_dashboardUserPreferences",
                EntityName: 'MJ: Dashboard Categories',
                PropertyName: "_dashboardCategories",
                EntityName: 'MJ: Dashboard User States',
                PropertyName: "_dashboardUserStates",
                EntityName: 'MJ: Dashboard Permissions',
                PropertyName: "_dashboardPermissions",
                EntityName: 'MJ: Dashboard Category Permissions',
                PropertyName: "_dashboardCategoryPermissions",
                EntityName: 'MJ: Dashboard Category Links',
                PropertyName: "_dashboardCategoryLinks",
    public get Dashboards(): DashboardEntityExtended[] {
    public get DashboardPartTypes(): MJDashboardPartTypeEntity[] {
        return this._partTypes;
    public get DashboardUserPreferences(): MJDashboardUserPreferenceEntity[] {
        return this._dashboardUserPreferences;
    public get DashboardCategories(): MJDashboardCategoryEntity[] {
        return this._dashboardCategories;
    public get DashboardUserStates(): MJDashboardUserStateEntity[] {
        return this._dashboardUserStates;
    public get DashboardPermissions(): MJDashboardPermissionEntity[] {
    public get DashboardCategoryPermissions(): MJDashboardCategoryPermissionEntity[] {
        return this._dashboardCategoryPermissions;
    public get DashboardCategoryLinks(): MJDashboardCategoryLinkEntity[] {
        return this._dashboardCategoryLinks;
    // Permission Checking Methods
     * Gets the effective permissions for a user on a specific dashboard.
     * Permission priority: Owner > Direct Permission > Category Permission
     * @param dashboardId - The ID of the dashboard
     * @param userId - The ID of the user to check permissions for
     * @returns The effective permissions for the user on this dashboard
    public GetDashboardPermissions(dashboardId: string, userId: string): DashboardUserPermissions {
        const dashboard = this._dashboards.find(d => d.ID === dashboardId);
        // Default: no permissions
        const noPermissions: DashboardUserPermissions = {
            DashboardID: dashboardId,
            CanShare: false,
            IsOwner: false,
            PermissionSource: 'none'
            return noPermissions;
        // Check if user is the owner - owners have full permissions
        if (dashboard.UserID === userId) {
        // Check for direct dashboard permission
        const directPermission = this._dashboardPermissions.find(
            p => p.DashboardID === dashboardId && p.UserID === userId
        if (directPermission) {
                CanRead: directPermission.CanRead,
                CanEdit: directPermission.CanEdit,
                CanDelete: directPermission.CanDelete,
                CanShare: directPermission.CanShare,
                PermissionSource: 'direct'
        // Check for category-level permission (if dashboard has a category)
        if (dashboard.CategoryID) {
            const categoryPermission = this.GetCategoryPermissions(dashboard.CategoryID, userId);
            if (categoryPermission.PermissionSource !== 'none') {
                    CanRead: categoryPermission.CanRead,
                    CanEdit: categoryPermission.CanEdit,
                    // Category permissions don't grant delete on individual dashboards
                    PermissionSource: 'category'
     * Gets the effective permissions for a user on a specific dashboard category.
     * @param categoryId - The ID of the category
     * @returns The effective permissions for the user on this category
    public GetCategoryPermissions(categoryId: string, userId: string): DashboardCategoryUserPermissions {
        const category = this._dashboardCategories.find(c => c.ID === categoryId);
        const noPermissions: DashboardCategoryUserPermissions = {
            CategoryID: categoryId,
            CanAddRemove: false,
        if (category.UserID === userId) {
                CanAddRemove: true,
        // Check for direct category permission
        const directPermission = this._dashboardCategoryPermissions.find(
            p => p.DashboardCategoryID === categoryId && p.UserID === userId
                CanAddRemove: directPermission.CanAddRemove,
     * Checks if a user can read/view a dashboard
     * @returns true if the user can read the dashboard
    public CanUserReadDashboard(dashboardId: string, userId: string): boolean {
        return this.GetDashboardPermissions(dashboardId, userId).CanRead;
     * Checks if a user can edit a dashboard
     * @returns true if the user can edit the dashboard
    public CanUserEditDashboard(dashboardId: string, userId: string): boolean {
        return this.GetDashboardPermissions(dashboardId, userId).CanEdit;
     * Checks if a user can delete a dashboard
     * @returns true if the user can delete the dashboard
    public CanUserDeleteDashboard(dashboardId: string, userId: string): boolean {
        return this.GetDashboardPermissions(dashboardId, userId).CanDelete;
     * Checks if a user can share a dashboard
     * @returns true if the user can share the dashboard
    public CanUserShareDashboard(dashboardId: string, userId: string): boolean {
        return this.GetDashboardPermissions(dashboardId, userId).CanShare;
     * Gets all dashboards the user has read access to (owned or shared)
     * @returns Array of dashboards the user can read
    public GetAccessibleDashboards(userId: string): DashboardEntityExtended[] {
        return this._dashboards.filter(dashboard =>
            this.CanUserReadDashboard(dashboard.ID, userId)
     * Gets all dashboard category links for a user (shared dashboards linked to their folder structure)
     * @returns Array of category links for the user
    public GetUserCategoryLinks(userId: string): MJDashboardCategoryLinkEntity[] {
        return this._dashboardCategoryLinks.filter(link => link.UserID === userId);
     * Gets the dashboards shared with a specific user (excludes owned dashboards)
     * @returns Array of dashboards shared with the user
    public GetSharedDashboards(userId: string): DashboardEntityExtended[] {
        return this._dashboards.filter(dashboard => {
            // Exclude owned dashboards
            // Check if user has read permission
            return this.CanUserReadDashboard(dashboard.ID, userId);
     * Gets the users a dashboard is shared with and their permissions
     * @returns Array of permission records for this dashboard
    public GetDashboardShares(dashboardId: string): MJDashboardPermissionEntity[] {
        return this._dashboardPermissions.filter(p => p.DashboardID === dashboardId);
     * Gets the users a category is shared with and their permissions
     * @returns Array of permission records for this category
    public GetCategoryShares(categoryId: string): MJDashboardCategoryPermissionEntity[] {
        return this._dashboardCategoryPermissions.filter(p => p.DashboardCategoryID === categoryId);
     * Checks if a user can access a category (view it in their category list).
     * A user can access a category if they:
     * 1. Own the category (category.UserID === userId)
     * 2. Have direct permission on the category
     * @returns true if the user can access the category
    public CanUserAccessCategory(categoryId: string, userId: string): boolean {
        const permissions = this.GetCategoryPermissions(categoryId, userId);
        return permissions.PermissionSource !== 'none';
     * Gets all categories the user has access to (owned or shared).
     * Only returns categories the user explicitly owns or has permissions on.
     * @returns Array of categories the user can access
    public GetAccessibleCategories(userId: string): MJDashboardCategoryEntity[] {
        return this._dashboardCategories.filter(category =>
            this.CanUserAccessCategory(category.ID, userId)
