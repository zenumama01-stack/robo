import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
// MemberJunction
import { BaseFormsModule } from '@memberjunction/ng-base-forms';
import { ContainerDirectivesModule } from '@memberjunction/ng-container-directives';
// Dashboards
import { EventsDashboardComponent } from './dashboards/events-dashboard/events-dashboard.component';
// Forms
import { EventFormComponent } from './forms/event-form/event-form.component';
import { SubmissionFormComponent } from './forms/submission-form/submission-form.component';
import { SpeakerFormComponent } from './forms/speaker-form/speaker-form.component';
// Services
import { EventService } from './services/event.service';
import { SubmissionService } from './services/submission.service';
import { SpeakerService } from './services/speaker.service';
@NgModule({
  declarations: [
    EventsDashboardComponent,
    EventFormComponent,
    SubmissionFormComponent,
    SpeakerFormComponent
  imports: [
    CommonModule,
    FormsModule,
    BaseFormsModule,
    ContainerDirectivesModule
  providers: [
    EventService,
    SubmissionService,
    SpeakerService
  exports: [
export class EventAbstractSubmissionModule { }
import { IndicatorsModule } from '@progress/kendo-angular-indicators';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { LayoutModule, TabStripModule, PanelBarModule } from '@progress/kendo-angular-layout';
import { EntityAdminDashboardComponent } from './EntityAdmin/entity-admin-dashboard.component';
// ERDCompositeComponent, EntityFilterPanelComponent, EntityDetailsComponent are now in
// @memberjunction/ng-entity-relationship-diagram and imported via EntityRelationshipDiagramModule
import { ModelManagementComponent } from './AI/components/models/model-management.component';
import { PromptManagementComponent } from './AI/components/prompts/prompt-management.component';
import { PromptFilterPanelComponent } from './AI/components/prompts/prompt-filter-panel.component';
import { AgentConfigurationComponent } from './AI/components/agents/agent-configuration.component';
import { AgentFilterPanelComponent } from './AI/components/agents/agent-filter-panel.component';
import { AgentEditorComponent } from './AI/components/agents/agent-editor.component';
import { ExecutionMonitoringComponent } from './AI/components/execution-monitoring.component';
import { SystemConfigurationComponent } from './AI/components/system/system-configuration.component';
import { SystemConfigFilterPanelComponent } from './AI/components/system/system-config-filter-panel.component';
import { ActionsOverviewComponent } from './Actions/components/actions-overview.component';
import { ExecutionMonitoringComponent as ActionsExecutionMonitoringComponent } from './Actions/components/execution-monitoring.component';
import { ScheduledActionsComponent } from './Actions/components/scheduled-actions.component';
import { CodeManagementComponent } from './Actions/components/code-management.component';
import { EntityIntegrationComponent } from './Actions/components/entity-integration.component';
import { SecurityPermissionsComponent } from './Actions/components/security-permissions.component';
import { ActionsListViewComponent } from './Actions/components/actions-list-view.component';
import { ExecutionsListViewComponent } from './Actions/components/executions-list-view.component';
import { CategoriesListViewComponent } from './Actions/components/categories-list-view.component';
// Action Explorer Components
  ActionTreePanelComponent,
  ActionToolbarComponent,
  ActionBreadcrumbComponent,
  ActionCardComponent,
  ActionListItemComponent,
  NewCategoryPanelComponent,
  NewActionPanelComponent
} from './Actions/components/explorer';
import { MarkdownModule } from '@memberjunction/ng-markdown';
import { NavigationModule } from '@progress/kendo-angular-navigation';
import { CodeEditorModule } from '@memberjunction/ng-code-editor';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { ModelPromptPriorityMatrixComponent } from './AI/components/prompts/model-prompt-priority-matrix.component';
import { PromptVersionControlComponent } from './AI/components/prompts/prompt-version-control.component';
import { ActionGalleryModule } from '@memberjunction/ng-action-gallery';
import { AITestHarnessModule } from '@memberjunction/ng-ai-test-harness';
import { MemberJunctionCoreEntityFormsModule } from '@memberjunction/ng-core-entity-forms';
import { MJNotificationsModule } from '@memberjunction/ng-notifications';
// AI Instrumentation Components
import { KPICardComponent } from './AI/components/widgets/kpi-card.component';
import { LiveExecutionWidgetComponent } from './AI/components/widgets/live-execution-widget.component';
import { TimeSeriesChartComponent } from './AI/components/charts/time-series-chart.component';
import { PerformanceHeatmapComponent } from './AI/components/charts/performance-heatmap.component';
import { AIInstrumentationService } from './AI/services/ai-instrumentation.service';
// Component Studio Components
import { ComponentStudioDashboardComponent } from './ComponentStudio/component-studio-dashboard.component';
import { TextImportDialogComponent } from './ComponentStudio/components/text-import-dialog.component';
import { ArtifactSelectionDialogComponent } from './ComponentStudio/components/artifact-selection-dialog.component';
import { ArtifactLoadDialogComponent } from './ComponentStudio/components/artifact-load-dialog.component';
import { ComponentBrowserComponent } from './ComponentStudio/components/browser/component-browser.component';
import { ComponentPreviewComponent } from './ComponentStudio/components/workspace/component-preview.component';
import { EditorTabsComponent } from './ComponentStudio/components/workspace/editor-tabs.component';
import { SpecEditorComponent } from './ComponentStudio/components/editors/spec-editor.component';
import { CodeEditorPanelComponent } from './ComponentStudio/components/editors/code-editor-panel.component';
import { RequirementsEditorComponent } from './ComponentStudio/components/editors/requirements-editor.component';
import { DataRequirementsEditorComponent } from './ComponentStudio/components/editors/data-requirements-editor.component';
import { AIAssistantPanelComponent } from './ComponentStudio/components/ai-assistant/ai-assistant-panel.component';
import { NewComponentDialogComponent } from './ComponentStudio/components/new-component-dialog/new-component-dialog.component';
import { SaveVersionDialogComponent } from './ComponentStudio/components/save-version-dialog/save-version-dialog.component';
import { ExcelExportModule } from '@progress/kendo-angular-excel-export';
import { MJReactModule } from '@memberjunction/ng-react';
import { SplitterModule } from '@progress/kendo-angular-layout';
// Scheduling Dashboard Components
import { SchedulingDashboardComponent } from './Scheduling/scheduling-dashboard.component';
import { SchedulingOverviewComponent } from './Scheduling/components/scheduling-overview.component';
import { SchedulingJobsComponent } from './Scheduling/components/scheduling-jobs.component';
import { SchedulingActivityComponent } from './Scheduling/components/scheduling-activity.component';
import { JobSlideoutComponent } from './Scheduling/components/job-slideout.component';
import { SchedulingOverviewResourceComponent } from './Scheduling/components/scheduling-overview-resource.component';
import { SchedulingJobsResourceComponent } from './Scheduling/components/scheduling-jobs-resource.component';
import { SchedulingActivityResourceComponent } from './Scheduling/components/scheduling-activity-resource.component';
import { SchedulingInstrumentationService } from './Scheduling/services/scheduling-instrumentation.service';
// Testing Dashboard Components
import { TestingDashboardComponent } from './Testing/testing-dashboard.component';
import { TestingDashboardTabComponent } from './Testing/components/testing-dashboard-tab.component';
import { TestingRunsComponent } from './Testing/components/testing-runs.component';
import { TestingAnalyticsComponent } from './Testing/components/testing-analytics.component';
import { TestingReviewComponent } from './Testing/components/testing-review.component';
import { TestingDashboardTabResourceComponent } from './Testing/components/testing-dashboard-tab-resource.component';
import { TestingRunsResourceComponent } from './Testing/components/testing-runs-resource.component';
import { TestingAnalyticsResourceComponent } from './Testing/components/testing-analytics-resource.component';
import { TestingReviewResourceComponent } from './Testing/components/testing-review-resource.component';
import { TestingExplorerComponent } from './Testing/components/testing-explorer.component';
import { TestingExplorerResourceComponent } from './Testing/components/testing-explorer-resource.component';
import { SuiteTreeComponent, SuiteTreeNodeComponent } from './Testing/components/widgets/suite-tree.component';
import { OracleBreakdownTableComponent } from './Testing/components/widgets/oracle-breakdown-table.component';
import { TestRunDetailPanelComponent } from './Testing/components/widgets/test-run-detail-panel.component';
import { TestingInstrumentationService } from './Testing/services/testing-instrumentation.service';
import { TestingModule } from '@memberjunction/ng-testing';
// Data Explorer Dashboard Components
import { DataExplorerDashboardComponent } from './DataExplorer/data-explorer-dashboard.component';
import { DataExplorerResourceComponent } from './DataExplorer/data-explorer-resource.component';
import { NavigationPanelComponent as ExplorerNavigationPanelComponent } from './DataExplorer/components/navigation-panel/navigation-panel.component';
import { ViewSelectorComponent } from './DataExplorer/components/view-selector/view-selector.component';
// ViewConfigPanelComponent now imported from @memberjunction/ng-entity-viewer via EntityViewerModule
import { FilterDialogComponent } from './DataExplorer/components/filter-dialog/filter-dialog.component';
import { ExplorerStateService } from './DataExplorer/services/explorer-state.service';
// Home Dashboard Components
import { HomeDashboardComponent } from './Home/home-dashboard.component';
import { ExplorerSettingsModule } from '@memberjunction/ng-explorer-settings';
import { FilterBuilderModule } from '@memberjunction/ng-filter-builder';
import { ExportServiceModule } from '@memberjunction/ng-export-service';
import { CommunicationDashboardComponent } from './Communication/communication-dashboard.component';
import { CommunicationMonitorResourceComponent } from './Communication/communication-monitor-resource.component';
import { CommunicationLogsResourceComponent } from './Communication/communication-logs-resource.component';
import { CommunicationProvidersResourceComponent } from './Communication/communication-providers-resource.component';
import { CommunicationRunsResourceComponent } from './Communication/communication-runs-resource.component';
import { CommunicationTemplatesResourceComponent } from './Communication/communication-templates-resource.component';
// Credentials Dashboard Components
import { CredentialsDashboardComponent } from './Credentials/credentials-dashboard.component';
import { CredentialsOverviewResourceComponent } from './Credentials/components/credentials-overview-resource.component';
import { CredentialsListResourceComponent } from './Credentials/components/credentials-list-resource.component';
import { CredentialsTypesResourceComponent } from './Credentials/components/credentials-types-resource.component';
import { CredentialsCategoriesResourceComponent } from './Credentials/components/credentials-categories-resource.component';
import { CredentialsAuditResourceComponent } from './Credentials/components/credentials-audit-resource.component';
import { GroupByPipe } from './Credentials/pipes/group-by.pipe';
// Credentials Module from generic package (panels and dialogs)
import { CredentialsModule } from '@memberjunction/ng-credentials';
// System Diagnostics Components
import { SystemDiagnosticsComponent } from './SystemDiagnostics/system-diagnostics.component';
// Lists Dashboard Components
import { ListsMyListsResource } from './Lists/components/lists-my-lists-resource.component';
import { ListsBrowseResource } from './Lists/components/lists-browse-resource.component';
import { ListsCategoriesResource } from './Lists/components/lists-categories-resource.component';
import { ListsOperationsResource } from './Lists/components/lists-operations-resource.component';
import { VennDiagramComponent } from './Lists/components/venn-diagram/venn-diagram.component';
import { ListSetOperationsService } from './Lists/services/list-set-operations.service';
// Query Browser Components
import { QueryBrowserResourceComponent } from './QueryBrowser/query-browser-resource.component';
// Dashboard Browser Components (Coming Soon Placeholder)
import { DashboardBrowserResourceComponent } from './DashboardBrowser/dashboard-browser-resource.component';
import { DashboardShareDialogComponent } from './DashboardBrowser/dashboard-share-dialog.component';
// Query Viewer Module
import { QueryViewerModule } from '@memberjunction/ng-query-viewer';
// Dashboard Viewer Module
import { DashboardViewerModule } from '@memberjunction/ng-dashboard-viewer';
// API Keys Dashboard Components
import { APIKeysResourceComponent } from './APIKeys/api-keys-resource.component';
import { APIKeyCreateDialogComponent } from './APIKeys/api-key-create-dialog.component';
import { APIKeyEditPanelComponent } from './APIKeys/api-key-edit-panel.component';
import { APIKeyListComponent } from './APIKeys/api-key-list.component';
import { APIApplicationsPanelComponent } from './APIKeys/api-applications-panel.component';
import { APIScopesPanelComponent } from './APIKeys/api-scopes-panel.component';
import { APIUsagePanelComponent } from './APIKeys/api-usage-panel.component';
// Shared Pipes Module
import { SharedPipesModule } from './shared/shared-pipes.module';
// MCP Dashboard Module
import { MCPModule } from './MCP';
// Actions Module (test harness, dialogs)
import { ActionsModule } from '@memberjunction/ng-actions';
// Agents Module (create agent dialogs and slide-ins)
import { AgentsModule } from '@memberjunction/ng-agents';
// Version History Dashboard Components
import { VersionHistoryLabelsResourceComponent } from './VersionHistory/components/labels-resource.component';
import { VersionHistoryDiffResourceComponent } from './VersionHistory/components/diff-resource.component';
import { VersionHistoryRestoreResourceComponent } from './VersionHistory/components/restore-resource.component';
import { VersionHistoryGraphResourceComponent } from './VersionHistory/components/graph-resource.component';
import { VersionsModule } from '@memberjunction/ng-versions';
    // ERDCompositeComponent, EntityFilterPanelComponent, EntityDetailsComponent now in Generic package
    PromptFilterPanelComponent,
    AgentFilterPanelComponent,
    AgentEditorComponent,
    SystemConfigFilterPanelComponent,
    ActionsExecutionMonitoringComponent,
    ActionsListViewComponent,
    ExecutionsListViewComponent,
    CategoriesListViewComponent,
    NewActionPanelComponent,
    ModelPromptPriorityMatrixComponent,
    PromptVersionControlComponent,
    KPICardComponent,
    LiveExecutionWidgetComponent,
    TimeSeriesChartComponent,
    PerformanceHeatmapComponent,
    TextImportDialogComponent,
    ArtifactSelectionDialogComponent,
    ArtifactLoadDialogComponent,
    ComponentBrowserComponent,
    ComponentPreviewComponent,
    EditorTabsComponent,
    SpecEditorComponent,
    CodeEditorPanelComponent,
    RequirementsEditorComponent,
    DataRequirementsEditorComponent,
    AIAssistantPanelComponent,
    NewComponentDialogComponent,
    SaveVersionDialogComponent,
    SchedulingOverviewComponent,
    SchedulingJobsComponent,
    SchedulingActivityComponent,
    JobSlideoutComponent,
    TestingDashboardTabComponent,
    TestingRunsComponent,
    TestingAnalyticsComponent,
    TestingReviewComponent,
    TestingExplorerComponent,
    SuiteTreeComponent,
    SuiteTreeNodeComponent,
    OracleBreakdownTableComponent,
    TestRunDetailPanelComponent,
    ExplorerNavigationPanelComponent,
    ViewSelectorComponent,
    // ViewConfigPanelComponent now comes from EntityViewerModule
    FilterDialogComponent,
    // Communication Dashboard Components
    // Credentials Dashboard Components (panels now come from CredentialsModule)
    GroupByPipe,
    VennDiagramComponent,
    // Dashboard Browser Components
    DashboardShareDialogComponent,
    APIKeyCreateDialogComponent,
    APIKeyEditPanelComponent,
    APIKeyListComponent,
    APIApplicationsPanelComponent,
    APIScopesPanelComponent,
    APIUsagePanelComponent,
    VersionHistoryGraphResourceComponent
    IndicatorsModule,
    ContainerDirectivesModule,
    NavigationModule,
    MemberJunctionCoreEntityFormsModule,
    ExcelExportModule,
    MJReactModule,
    MJNotificationsModule,
    ExplorerSettingsModule,
    FilterBuilderModule,
    ExportServiceModule,
    QueryViewerModule,
    DashboardViewerModule,
    MCPModule,
    CredentialsModule,
    SharedPipesModule,
    AgentsModule,
    MarkdownModule,
    VersionsModule
    AIInstrumentationService,
    SchedulingInstrumentationService,
    TestingInstrumentationService,
    ExplorerStateService,
    ListSetOperationsService
    // Export AI components (now BaseResourceComponent-based)
    // Export Actions components (now BaseResourceComponent-based)
    // Export Action Explorer components
    // Export Scheduling resource components
    // Export Testing resource components
    // Export Data Explorer Dashboard and Resource
    // Export Home Dashboard
    // Export Communication Dashboard
    // Export Credentials Dashboard (panels re-exported via CredentialsModule)
    // MCP Dashboard Module (re-exports its components)
export class DashboardsModule { }// Kendo UI Angular imports
import { DialogsModule } from "@progress/kendo-angular-dialog";
import { EntityFormDialogComponent } from './entity-form-dialog/entity-form-dialog.component';
    EntityFormDialogComponent
export class EntityFormDialogModule { }import { EntityPermissionsGridComponent } from './grid/entity-permissions-grid.component';
import { EntityPermissionsSelectorWithGridComponent } from './entity-selector-with-grid/entity-selector-with-grid.component';
    EntityPermissionsGridComponent,
    EntityPermissionsSelectorWithGridComponent
    SharedGenericModule
export class EntityPermissionsModule { }import { RouterModule, RouteReuseStrategy } from '@angular/router';
import { SystemValidationService } from './lib/services/system-validation.service';
import { StartupValidationService } from './lib/services/startup-validation.service';
import { DialogsModule } from '@progress/kendo-angular-dialog';
import { ExcelModule, GridModule, PDFModule } from '@progress/kendo-angular-grid';
import { LabelModule } from '@progress/kendo-angular-label';
import { LayoutModule, TabStripModule, CardModule, AvatarModule } from '@progress/kendo-angular-layout';
import { ListViewModule } from '@progress/kendo-angular-listview';
import { ProgressBarModule } from "@progress/kendo-angular-progressbar";
import { DragDropModule } from '@angular/cdk/drag-drop';
// MJ
import { FileStorageModule } from '@memberjunction/ng-file-storage';
import { QueryGridModule } from '@memberjunction/ng-query-grid';
import { RecordChangesModule } from '@memberjunction/ng-record-changes';
import { EntityFormDialogModule } from '@memberjunction/ng-entity-form-dialog';
import { RecordSelectorModule } from '@memberjunction/ng-record-selector';
import { ResourcePermissionsModule } from '@memberjunction/ng-resource-permissions';
import { ListDetailGridModule } from '@memberjunction/ng-list-detail-grid';
// Local Components
import { ConversationsModule } from '@memberjunction/ng-conversations';
import { DashboardsModule } from '@memberjunction/ng-dashboards';
import { ArtifactsModule } from '@memberjunction/ng-artifacts';
import { MemberJunctionSharedModule } from '@memberjunction/ng-shared';
import { FormToolbarComponent } from './lib/generic/form-toolbar';
import { ResourceContainerComponent } from './lib/generic/resource-container-component';
import { DashboardPreferencesDialogComponent } from './lib/dashboard-preferences-dialog/dashboard-preferences-dialog.component';
import { DashboardResource } from './lib/resource-wrappers/dashboard-resource.component';
import { QueryResource } from './lib/resource-wrappers/query-resource.component';
import { EntityRecordResource } from './lib/resource-wrappers/record-resource.component';
import { SearchResultsResource } from './lib/resource-wrappers/search-results-resource.component';
import { UserViewResource } from './lib/resource-wrappers/view-resource.component';
import { AddItemComponent } from './lib/single-dashboard/Components/add-item/add-item.component';
import { DeleteItemComponent } from './lib/single-dashboard/Components/delete-item/delete-item.component';
import { EditDashboardComponent } from './lib/single-dashboard/Components/edit-dashboard/edit-dashboard.component';
import { SingleDashboardComponent } from './lib/single-dashboard/single-dashboard.component';
import { SingleQueryComponent } from './lib/single-query/single-query.component';
import { SingleRecordComponent } from './lib/single-record/single-record.component';
import { SingleSearchResultComponent } from './lib/single-search-result/single-search-result.component';
import { UserNotificationsComponent } from './lib/user-notifications/user-notifications.component';
import { UserProfileComponent } from './lib/user-profile/user-profile.component';
import { AppRoutingModule, CustomReuseStrategy } from './app-routing.module';
import { GenericDialogModule } from '@memberjunction/ng-generic-dialog';
import {SingleListDetailComponent} from './lib/single-list-detail/single-list-detail.component';
import { ListDetailResource } from './lib/resource-wrappers/list-detail-resource.component';
import { ChatConversationsResource } from './lib/resource-wrappers/chat-conversations-resource.component';
import { ChatCollectionsResource } from './lib/resource-wrappers/chat-collections-resource.component';
import { ChatTasksResource } from './lib/resource-wrappers/chat-tasks-resource.component';
import { ArtifactResource } from './lib/resource-wrappers/artifact-resource.component';
import { NotificationsResource } from './lib/resource-wrappers/notifications-resource.component';
    FormToolbarComponent,
    OAuthCallbackComponent,
    ResourceContainerComponent,
    SingleSearchResultComponent,
    SingleQueryComponent,
    UserProfileComponent,
    SingleDashboardComponent,
    AddItemComponent,
    DeleteItemComponent,
    EditDashboardComponent,
    UserNotificationsComponent,
    SingleListDetailComponent,
    ChatCollectionsResource,
    ChatTasksResource,
    NotificationsResource,
    DashboardPreferencesDialogComponent,
    AppRoutingModule,
    ExcelModule,
    PDFModule,
    LabelModule,
    RecordChangesModule,
    ListViewModule,
    QueryGridModule,
    MemberJunctionSharedModule,
    ConversationsModule,
    DashboardsModule,
    FileStorageModule,
    EntityFormDialogModule,
    RecordSelectorModule,
    ResourcePermissionsModule,
    GenericDialogModule,
    ProgressBarModule,
    DragDropModule,
    CardModule,
    AvatarModule,
    AITestHarnessModule, // [3.0] TO DO TO-DO Need to verify this works correctly!
    ArtifactsModule,
    ListDetailGridModule
    DashboardPreferencesDialogComponent 
    { provide: RouteReuseStrategy, useClass: CustomReuseStrategy },
    SystemValidationService,
    StartupValidationService
export class ExplorerCoreModule {}
import { WindowModule } from '@progress/kendo-angular-dialog';
import { EntityPermissionsModule } from '@memberjunction/ng-entity-permissions';
import { SimpleRecordListModule } from '@memberjunction/ng-simple-record-list';
import { JoinGridModule } from '@memberjunction/ng-join-grid';
// Shared module
import { SharedSettingsModule } from './shared/shared-settings.module';
// Main settings container
import { SettingsComponent } from './settings/settings.component';
// User-facing components
import { UserProfileSettingsComponent } from './user-profile-settings/user-profile-settings.component';
import { UserAppConfigComponent } from './user-app-config/user-app-config.component';
import { NotificationPreferencesComponent } from './notification-preferences/notification-preferences.component';
// New user settings components
import { GeneralSettingsComponent } from './general-settings/general-settings.component';
import { AccountInfoComponent } from './account-info/account-info.component';
import { ApplicationSettingsComponent } from './application-settings/application-settings.component';
import { AppearanceSettingsComponent } from './appearance-settings/appearance-settings.component';
// Admin components (used by Admin app dashboards)
import { SqlLoggingComponent } from './sql-logging/sql-logging.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { RoleManagementComponent } from './role-management/role-management.component';
import { ApplicationManagementComponent } from './application-management/application-management.component';
import { EntityPermissionsComponent } from './entity-permissions/entity-permissions.component';
// Admin dialog components
import { RoleDialogComponent } from './role-management/role-dialog/role-dialog.component';
import { UserDialogComponent } from './user-management/user-dialog/user-dialog.component';
import { PermissionDialogComponent } from './entity-permissions/permission-dialog/permission-dialog.component';
import { ApplicationDialogComponent } from './application-management/application-dialog/application-dialog.component';
    UserProfileSettingsComponent,
    UserAppConfigComponent,
    NotificationPreferencesComponent,
    GeneralSettingsComponent,
    AccountInfoComponent,
    ApplicationSettingsComponent,
    AppearanceSettingsComponent,
    RoleDialogComponent,
    UserDialogComponent,
    PermissionDialogComponent,
    ApplicationDialogComponent
    EntityPermissionsModule,
    SimpleRecordListModule,
    SharedSettingsModule,
    WindowModule
export class ExplorerSettingsModule { }
// MemberJunction modules
import { ListDetailGridComponent } from './lib/ng-list-detail-grid.component';
    ListDetailGridComponent
export class ListDetailGridModule { }
import { URLPipe } from './lib/urlPipe';
import { SimpleTextFormatPipe } from './lib/simpleTextFormat';
    URLPipe,
    SimpleTextFormatPipe
    NotificationModule,
    MJNotificationsModule
export class MemberJunctionSharedModule { }// LOCAL
import { SimpleRecordListComponent } from './simple-record-list/simple-record-list.component';
    SimpleRecordListComponent
export class SimpleRecordListModule { }import { TooltipsModule } from '@progress/kendo-angular-tooltip';
import { ActionGalleryComponent } from './lib/action-gallery.component';
import { ActionGalleryDialogService } from './lib/action-gallery-dialog.service';
    ActionGalleryComponent
    // Kendo UI
    TooltipsModule,
    ActionGalleryDialogService
export class ActionGalleryModule { }// MemberJunction imports
import { AITestHarnessComponent } from './lib/ai-test-harness.component';
import { AITestHarnessDialogComponent } from './lib/ai-test-harness-dialog.component';
import { AITestHarnessWindowComponent } from './lib/ai-test-harness-window.component';
import { TestHarnessCustomWindowComponent } from './lib/test-harness-custom-window.component';
import { AgentExecutionMonitorComponent } from './lib/agent-execution-monitor.component';
import { ExecutionNodeComponent } from './lib/agent-execution-node.component';
import { JsonViewerWindowComponent } from './lib/json-viewer-window.component';
import { WindowDockService } from './lib/window-dock.service';
import { AITestHarnessDialogService } from './lib/ai-test-harness-dialog.service';
import { TestHarnessWindowService } from './lib/test-harness-window.service';
import { TestHarnessWindowManagerService } from './lib/test-harness-window-manager.service';
    AITestHarnessComponent,
    AITestHarnessDialogComponent,
    AITestHarnessWindowComponent,
    TestHarnessCustomWindowComponent,
    JsonViewerWindowComponent,
    AgentExecutionMonitorComponent,
    ExecutionNodeComponent
    JsonViewerWindowComponent
    AITestHarnessDialogService,
    TestHarnessWindowService,
    TestHarnessWindowManagerService,
    WindowDockService
export class AITestHarnessModule { }import { MjFormToolbarComponent } from './lib/toolbar/form-toolbar.component';
import { MjFormFieldComponent } from './lib/field/form-field.component';
import { MjCollapsiblePanelComponent } from './lib/panel/collapsible-panel.component';
import { MjRecordFormContainerComponent } from './lib/container/record-form-container.component';
import { MjSectionManagerComponent } from './lib/section-manager/section-manager.component';
import { SectionLoaderComponent } from './lib/section-loader-component';
import { ExplorerEntityDataGridComponent } from './lib/explorer-entity-data-grid.component';
import { MjIsaRelatedCardComponent } from './lib/isa-related-panel/isa-related-card.component';
import { MjIsaRelatedPanelComponent } from './lib/isa-related-panel/isa-related-panel.component';
 * BaseFormsModule - Form components and base classes for rendering and editing MemberJunction entity records.
 * - **MjRecordFormContainerComponent** (`<mj-record-form-container>`): Top-level container
 *   with sticky toolbar, section state management, and content slots.
 * - **MjFormToolbarComponent** (`<mj-form-toolbar>`): Configurable toolbar with read/edit mode
 *   actions, IS-A hierarchy breadcrumb, section controls, and inline delete dialog.
 * - **MjCollapsiblePanelComponent** (`<mj-collapsible-panel>`): Collapsible section with
 *   drag-to-reorder, search filtering, and inherited/related variants.
 * - **MjFormFieldComponent** (`<mj-form-field>`): Entity field renderer with clean read-only
 *   display and modern edit-mode inputs (native HTML with custom select and autocomplete).
 * - **SectionLoaderComponent** (`<mj-form-section>`): Dynamic section loader via ClassFactory.
 * - **ExplorerEntityDataGridComponent** (`<mj-explorer-entity-data-grid>`): Data grid wrapper
 *   for related entity sections with Navigate event support.
 * All navigation actions are emitted as events via {@link FormNavigationEvent}.
 * The host application subscribes and maps to its own routing system.
    MjFormToolbarComponent,
    MjFormFieldComponent,
    MjCollapsiblePanelComponent,
    MjRecordFormContainerComponent,
    MjSectionManagerComponent,
    SectionLoaderComponent,
    ExplorerEntityDataGridComponent,
    MjIsaRelatedCardComponent,
    MjIsaRelatedPanelComponent
    EntityViewerModule
export class BaseFormsModule { }
// Markdown
import { ChatComponent } from './chat/chat.component';
    ChatComponent
    MarkdownModule
export class ChatModule { }import { DataContextComponent } from './ng-data-context.component';
import { DataContextDialogComponent } from './ng-data-context-dialog.component';
    DataContextComponent,
    DataContextDialogComponent
export class DataContextModule { }import { DeepDiffDialogComponent } from './deep-diff-dialog.component';
import { DeepDiffComponent } from './deep-diff.component';
    DeepDiffComponent,
    DeepDiffDialogComponent
export class DeepDiffModule { }import { ListBoxModule } from '@progress/kendo-angular-listbox';
import { EntityCommunicationsPreviewWindowComponent } from './lib/window.component';
import { EntityCommunicationsPreviewComponent } from './lib/preview.component';
    EntityCommunicationsPreviewComponent,
    EntityCommunicationsPreviewWindowComponent
    ListBoxModule,
export class EntityCommunicationsModule { }import { AgGridModule } from 'ag-grid-angular';
import { TimelineModule } from '@memberjunction/ng-timeline';
import { EntityCardsComponent } from './lib/entity-cards/entity-cards.component';
import { EntityViewerComponent } from './lib/entity-viewer/entity-viewer.component';
import { EntityRecordDetailPanelComponent } from './lib/entity-record-detail-panel/entity-record-detail-panel.component';
import { PillComponent } from './lib/pill/pill.component';
import { PaginationComponent } from './lib/pagination/pagination.component';
import { EntityDataGridComponent } from './lib/entity-data-grid/entity-data-grid.component';
import { ViewConfigPanelComponent } from './lib/view-config-panel/view-config-panel.component';
import { AggregatePanelComponent } from './lib/aggregate-panel/aggregate-panel.component';
import { AggregateSetupDialogComponent } from './lib/aggregate-setup-dialog/aggregate-setup-dialog.component';
import { ConfirmDialogComponent } from './lib/confirm-dialog/confirm-dialog.component';
import { QuickSaveDialogComponent } from './lib/quick-save-dialog/quick-save-dialog.component';
import { ViewHeaderComponent } from './lib/view-header/view-header.component';
import { DuplicateViewDialogComponent } from './lib/duplicate-view-dialog/duplicate-view-dialog.component';
import { SharedViewWarningDialogComponent } from './lib/shared-view-warning-dialog/shared-view-warning-dialog.component';
 * EntityViewerModule - Provides components for viewing entity data
 * This module exports:
 * - EntityViewerComponent: Composite component with grid/cards toggle, server-side filtering/sorting/pagination
 * - EntityDataGridComponent: Modern AG Grid-based grid with Before/After cancelable events
 * - EntityCardsComponent: Card-based view with standalone or parent-managed data
 * - EntityRecordDetailPanelComponent: Detail panel for displaying single record information
 * - PillComponent: Semantic color pills for categorical values
 * - PaginationComponent: Beautiful "Load More" pagination with progress indicator
 * - ViewConfigPanelComponent: Sliding panel for configuring view settings (columns, sort, filters)
 * import { EntityViewerModule } from '@memberjunction/ng-entity-viewer';
 *   imports: [EntityViewerModule]
 * export class MyModule { }
    EntityCardsComponent,
    EntityRecordDetailPanelComponent,
    PillComponent,
    PaginationComponent,
    AggregatePanelComponent,
    AggregateSetupDialogComponent,
    QuickSaveDialogComponent,
    ViewHeaderComponent,
    DuplicateViewDialogComponent,
    SharedViewWarningDialogComponent
    AgGridModule,
    TimelineModule,
    ExportServiceModule
export class EntityViewerModule { }
import { ExportDialogComponent } from './export-dialog.component';
import { ExportService } from './export.service';
    ExportDialogComponent
    ExportService
export class ExportServiceModule { }
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MenusModule } from '@progress/kendo-angular-menu';
import { CategoryTreeComponent } from './category-tree/category-tree';
import { FileUploadComponent } from './file-upload/file-upload';
import { FilesGridComponent } from './files-grid/files-grid';
import { FileBrowserComponent } from './file-browser/file-browser.component';
import { FileBrowserDemoComponent } from './file-browser/file-browser-demo.component';
import { FileBrowserResource } from './file-browser/file-browser-resource.component';
import { StorageProvidersListComponent } from './file-browser/storage-providers-list.component';
import { FolderTreeComponent } from './file-browser/folder-tree.component';
import { FileGridComponent } from './file-browser/file-grid.component';
    CategoryTreeComponent,
    FilesGridComponent,
    FileUploadComponent,
    FileBrowserComponent,
    FileBrowserDemoComponent,
    StorageProvidersListComponent,
    FolderTreeComponent,
    FileGridComponent
    BrowserModule,
    BrowserAnimationsModule,
    MenusModule,
    FileBrowserResource
export class FileStorageModule {}
import { FindRecordComponent } from './lib/find-record.component';
import { FindRecordDialogComponent } from './lib/dialog.component';
    FindRecordComponent,
    FindRecordDialogComponent
    InputsModule
export class FindRecordModule { }import { GenericDialogComponent } from './lib/dialog.component';
    GenericDialogComponent
export class GenericDialogModule { }import { JoinGridComponent } from './join-grid/join-grid.component';
    JoinGridComponent
export class JoinGridModule { }// Kendo UI imports
import { ListManagementDialogComponent } from './components/list-management-dialog/list-management-dialog.component';
import { ListShareDialogComponent } from './components/list-share-dialog/list-share-dialog.component';
import { ListManagementService } from './services/list-management.service';
import { ListSharingService } from './services/list-sharing.service';
 * Module providing list management components for MemberJunction.
 * Import this module to use the list management dialog:
 * import { ListManagementModule } from '@memberjunction/ng-list-management';
 *   imports: [ListManagementModule]
 * Then use in templates:
 * <mj-list-management-dialog
 *   [config]="dialogConfig"
 *   [visible]="showDialog"
 *   (complete)="onComplete($event)"
 *   (cancel)="onCancel()">
 * </mj-list-management-dialog>
    ListManagementDialogComponent,
    ListShareDialogComponent
    ListManagementService,
    ListSharingService
export class ListManagementModule { }
export class MJNotificationsModule { } * @fileoverview Angular module for React component integration in MemberJunction.
 * Provides components and services for hosting React components within Angular applications.
import { MJReactComponent } from './components/mj-react-component.component';
import { ScriptLoaderService } from './services/script-loader.service';
import { ReactBridgeService } from './services/react-bridge.service';
import { AngularAdapterService } from './services/angular-adapter.service';
 * Angular module that provides React component hosting capabilities.
 * Import this module to use React components within your Angular application.
 * import { MJReactModule } from '@memberjunction/ng-react';
 *   imports: [MJReactModule]
    MJReactComponent
    ScriptLoaderService,
    ReactBridgeService,
    AngularAdapterService
export class MJReactModule { }import { RecordSelectorComponent } from './lib/record-selector.component';
import { RecordSelectorDialogComponent } from './lib/dialog.component';
    RecordSelectorComponent,
    RecordSelectorDialogComponent
    ListBoxModule
export class RecordSelectorModule { }import { ResourcePermissionsComponent } from './lib/resource-permissions.component';
import { AvailableResourcesComponent } from './lib/available-resources.component';
import { RequestResourceAccessComponent } from './lib/request-access.component';
    ResourcePermissionsComponent,
    AvailableResourcesComponent,
    RequestResourceAccessComponent
export class ResourcePermissionsModule { }import { LoadingComponent } from './loading/loading.component';
    LoadingComponent
export class SharedGenericModule { }
import { MJTabStripComponent } from './tab-strip/tab-strip.component';
import { MJTabBodyComponent } from './tab-body/tab-body.component';
import { MJTabComponent } from './tab/tab.component';
    MJTabStripComponent,
    MJTabBodyComponent,
    MJTabComponent
export class MJTabStripModule { } * @fileoverview Angular module for the MJ Timeline component.
 * This module provides a rich, responsive timeline component that works with
 * both MemberJunction BaseEntity objects and plain JavaScript objects.
 * @module @memberjunction/ng-timeline
import { TimelineComponent } from './component/timeline.component';
 * Angular module that provides the MJ Timeline component.
 * import { TimelineModule } from '@memberjunction/ng-timeline';
 *   imports: [TimelineModule]
    TimelineComponent
export class TimelineModule { }
export class DashboardsModule { }// Kendo UI Angular imports
export class EntityFormDialogModule { }import { EntityPermissionsGridComponent } from './grid/entity-permissions-grid.component';
export class EntityPermissionsModule { }import { RouterModule, RouteReuseStrategy } from '@angular/router';
export class MemberJunctionSharedModule { }// LOCAL
export class SimpleRecordListModule { }import { TooltipsModule } from '@progress/kendo-angular-tooltip';
export class ActionGalleryModule { }// MemberJunction imports
export class AITestHarnessModule { }import { MjFormToolbarComponent } from './lib/toolbar/form-toolbar.component';
export class ChatModule { }import { DataContextComponent } from './ng-data-context.component';
export class DataContextModule { }import { DeepDiffDialogComponent } from './deep-diff-dialog.component';
export class DeepDiffModule { }import { ListBoxModule } from '@progress/kendo-angular-listbox';
export class EntityCommunicationsModule { }import { AgGridModule } from 'ag-grid-angular';
export class FindRecordModule { }import { GenericDialogComponent } from './lib/dialog.component';
export class GenericDialogModule { }import { JoinGridComponent } from './join-grid/join-grid.component';
export class JoinGridModule { }// Kendo UI imports
export class MJNotificationsModule { } * @fileoverview Angular module for React component integration in MemberJunction.
export class MJReactModule { }import { RecordSelectorComponent } from './lib/record-selector.component';
export class RecordSelectorModule { }import { ResourcePermissionsComponent } from './lib/resource-permissions.component';
export class ResourcePermissionsModule { }import { LoadingComponent } from './loading/loading.component';
export class MJTabStripModule { } * @fileoverview Angular module for the MJ Timeline component.
export class DashboardsModule { }// Kendo UI Angular imports
export class EntityFormDialogModule { }import { EntityPermissionsGridComponent } from './grid/entity-permissions-grid.component';
export class EntityPermissionsModule { }import { RouterModule, RouteReuseStrategy } from '@angular/router';
export class MemberJunctionSharedModule { }// LOCAL
export class SimpleRecordListModule { }import { TooltipsModule } from '@progress/kendo-angular-tooltip';
export class ActionGalleryModule { }// MemberJunction imports
export class AITestHarnessModule { }import { MjFormToolbarComponent } from './lib/toolbar/form-toolbar.component';
export class ChatModule { }import { DataContextComponent } from './ng-data-context.component';
export class DataContextModule { }import { DeepDiffDialogComponent } from './deep-diff-dialog.component';
export class DeepDiffModule { }import { ListBoxModule } from '@progress/kendo-angular-listbox';
export class EntityCommunicationsModule { }import { AgGridModule } from 'ag-grid-angular';
export class FindRecordModule { }import { GenericDialogComponent } from './lib/dialog.component';
export class GenericDialogModule { }import { JoinGridComponent } from './join-grid/join-grid.component';
export class JoinGridModule { }// Kendo UI imports
export class MJNotificationsModule { } * @fileoverview Angular module for React component integration in MemberJunction.
export class MJReactModule { }import { RecordSelectorComponent } from './lib/record-selector.component';
export class RecordSelectorModule { }import { ResourcePermissionsComponent } from './lib/resource-permissions.component';
export class ResourcePermissionsModule { }import { LoadingComponent } from './loading/loading.component';
export class MJTabStripModule { } * @fileoverview Angular module for the MJ Timeline component.
