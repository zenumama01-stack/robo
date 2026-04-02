 * Public API Surface of @memberjunction/ng-event-abstract-submission
// Module
export * from './module';
export * from './dashboards/events-dashboard/events-dashboard.component';
export * from './forms/event-form/event-form.component';
export * from './forms/submission-form/submission-form.component';
export * from './forms/speaker-form/speaker-form.component';
export * from './services/event.service';
export * from './services/submission.service';
export * from './services/speaker.service';
export * from  './dashboards/events-dashboard/events-dashboard.component'; * Public API Surface of @memberjunction/ng-bootstrap
export * from './lib/bootstrap.module';
export * from './lib/bootstrap.component';
export * from './lib/bootstrap.types';
export * from './lib/services/initialization.service';
export * from './lib/components/auth-shell.component';
 * Pre-built class registrations manifest for all @memberjunction/* Angular packages.
 * Importing this module prevents tree-shaking of MJ's dynamically registered classes.
 * Generated at MJ build time and ships with the package.
export * from './generated/mj-class-registrations'; * Public API Surface
// Core exports
export * from './lib/mjexplorer-auth-base.service';
export * from './lib/auth-services.module';
export { RedirectComponent } from './lib/redirect.component';
// Type exports - v3.0.0 standardized types
export * from './lib/auth-types';
// Interface exports
export * from './lib/IAuthProvider';
export * from './lib/AngularAuthProviderFactory';
// Provider implementations
export * from './lib/providers/mjexplorer-msal-provider.service';
export * from './lib/providers/mjexplorer-auth0-provider.service';
export * from './lib/providers/mjexplorer-okta-provider.service'; * @memberjunction/ng-base-application
 * BaseApplication class system for app-centric navigation in MemberJunction Explorer.
 * Provides extensible application classes, workspace configuration types, and state management.
export * from './lib/base-application';
export * from './lib/application-manager';
export * from './lib/workspace-state-manager';
export * from './lib/golden-layout-manager';
export * from './lib/tab.service';
// Interfaces
export * from './lib/interfaces/nav-item.interface';
export * from './lib/interfaces/tab-request.interface';
export * from './lib/interfaces/workspace-configuration.interface';
 * Public API Surface of ng-compare-records
export * from './lib/generated/generated-forms.module';
export * from './lib/custom/custom-forms.module';
// Export Agent Dialog components and service
export { NewAgentDialogComponent } from './lib/custom/AIAgents/new-agent-dialog.component';
export { NewAgentDialogService } from './lib/custom/AIAgents/new-agent-dialog.service';
// Export Flow Agent components
export { FlowAgentFormSectionComponent } from './lib/custom/AIAgents/FlowAgentType/flow-agent-form-section.component';
// NOTE: Flow editor components have moved to @memberjunction/ng-flow-editor
// NOTE: Action Test Harness components have moved to @memberjunction/ng-actions
 * Public API Surface of dashboards
export * from './EntityAdmin/entity-admin-dashboard.component';
export * from './ComponentStudio';
export * from './Scheduling/scheduling-dashboard.component';
export * from './Testing/testing-dashboard.component';
export * from './DataExplorer';
export * from './Communication/communication-dashboard.component';
export * from './Credentials';
export * from './SystemDiagnostics';
export * from './Lists';
// Export AI components as resources (BaseResourceComponent-based)
  PerformanceHeatmapComponent
} from './AI/index';
// Export Actions components as resources (BaseResourceComponent-based)
  // Action Explorer components
  // State service
  ActionExplorerStateService
} from './Actions';
// Re-export Actions ExecutionMonitoringComponent with alias to avoid conflict with AI version
export { ExecutionMonitoringComponent as ActionsExecutionMonitoringComponent } from './Actions/components/execution-monitoring.component';
// Export Scheduling components as resources (BaseResourceComponent-based)
  SchedulingActivityResourceComponent
} from './Scheduling/components';
// Export Communication components as resources
  CommunicationMonitorResourceComponent
} from './Communication/communication-monitor-resource.component';
  CommunicationLogsResourceComponent
} from './Communication/communication-logs-resource.component';
  CommunicationProvidersResourceComponent
} from './Communication/communication-providers-resource.component';
  CommunicationRunsResourceComponent
} from './Communication/communication-runs-resource.component';
  CommunicationTemplatesResourceComponent
} from './Communication/communication-templates-resource.component';
// Export Testing components as resources (BaseResourceComponent-based)
  TestingExplorerResourceComponent
} from './Testing/components';
// Query Browser
  QueryBrowserResourceComponent
} from './QueryBrowser/query-browser-resource.component';
// Dashboard Browser
  DashboardBrowserResourceComponent
} from './DashboardBrowser/dashboard-browser-resource.component';
  UserSharePermission,
  ShareDialogResult
} from './DashboardBrowser/dashboard-share-dialog.component';
// Home Application and Dashboard
export { HomeApplication } from './Home/home-application';
export { HomeDashboardComponent } from './Home/home-dashboard.component';
// API Keys
export { APIKeysResourceComponent } from './APIKeys/api-keys-resource.component';
export { APIKeyCreateDialogComponent, APIKeyCreateResult } from './APIKeys/api-key-create-dialog.component';
export { APIKeyEditPanelComponent } from './APIKeys/api-key-edit-panel.component';
export { APIKeyListComponent, APIKeyFilter } from './APIKeys/api-key-list.component';
export { APIApplicationsPanelComponent } from './APIKeys/api-applications-panel.component';
export { APIScopesPanelComponent } from './APIKeys/api-scopes-panel.component';
export { APIUsagePanelComponent } from './APIKeys/api-usage-panel.component';
// MCP (Model Context Protocol)
export * from './MCP';
} from './VersionHistory';
export * from './lib/module';
export * from './lib/entity-form-dialog/entity-form-dialog.component'
export * from './lib/entity-selector-with-grid/entity-selector-with-grid.component'
export * from './lib/grid/entity-permissions-grid.component'
 * Public API Surface of @memberjunction/ng-explorer-app
export * from './lib/explorer-app.module';
export * from './lib/explorer-app.component';
export * from './lib/generic/form-toolbar';
export * from './lib/generic/resource-container-component';
export * from './lib/resource-wrappers/dashboard-resource.component'
export * from './lib/dashboard-preferences-dialog/dashboard-preferences-dialog.component'
export * from './lib/resource-wrappers/record-resource.component'
// export * from './lib/resource-wrappers/resource-wrappers-loader'
export * from './lib/resource-wrappers/search-results-resource.component'
export * from './lib/resource-wrappers/view-resource.component'
export * from './lib/resource-wrappers/list-detail-resource.component'
export * from './lib/resource-wrappers/chat-conversations-resource.component'
export * from './lib/resource-wrappers/artifact-resource.component'
// Command Palette (only component and service, no module)
export * from './lib/command-palette/command-palette.component';
export * from './lib/command-palette/command-palette.service';
// New Shell Module (New Explorer UX)
export * from './lib/shell/shell.module'
export * from './lib/shell/shell.component'
export * from './lib/single-record/single-record.component'
export * from './lib/single-search-result/single-search-result.component'
export * from './lib/single-dashboard/single-dashboard.component'
export * from './lib/single-dashboard/Components/add-item/add-item.component'
export * from './lib/single-dashboard/Components/delete-item/delete-item.component'
export * from './lib/single-dashboard/Components/edit-dashboard/edit-dashboard.component'
export * from './lib/single-list-detail/single-list-detail.component'
export * from './lib/user-profile/user-profile.component'
export * from './lib/user-notifications/user-notifications.component';
export * from './lib/guards/auth-guard.service';
export * from './lib/guards/entities.guard';
export * from './lib/single-query/single-query.component'
export * from './lib/resource-wrappers/query-resource.component'
// Validation services
export * from './lib/services/system-validation.service'
export * from './lib/services/startup-validation.service'
export * from './lib/system-validation/system-validation-banner.component'
// User Menu Plugin System
export * from './lib/user-menu'
// OAuth Module
export * from './lib/oauth/oauth.module'
export * from './lib/oauth/oauth-callback.component'
// Routing Module - must be imported directly in root app module
export * from './app-routing.module'
 * Public API Surface of @memberjunction/ng-explorer-modules
export * from './lib/explorer-modules.module';
// Re-export commonly used exports from bundled modules for convenience
export { SystemValidationBannerComponent } from '@memberjunction/ng-explorer-core';
export { SharedService } from '@memberjunction/ng-shared';
// Main dashboard components
export * from './lib/settings/settings.component';
export * from './lib/sql-logging/sql-logging.component';
export * from './lib/user-management/user-management.component';
export * from './lib/role-management/role-management.component';
export * from './lib/application-management/application-management.component';
export * from './lib/entity-permissions/entity-permissions.component';
export * from './lib/user-profile-settings/user-profile-settings.component';
export * from './lib/notification-preferences/notification-preferences.component';
// Settings sub-pages
export * from './lib/general-settings/general-settings.component';
export * from './lib/account-info/account-info.component';
export * from './lib/appearance-settings/appearance-settings.component';
export * from './lib/application-settings/application-settings.component';
export * from './lib/shared/settings-card.component';
// Dialog components
export * from './lib/role-management/role-dialog/role-dialog.component';
export * from './lib/user-management/user-dialog/user-dialog.component';
export * from './lib/entity-permissions/permission-dialog/permission-dialog.component';
export * from './lib/application-management/application-dialog/application-dialog.component';
export * from './lib/user-app-config/user-app-config.component';
 * Public API Surface of @memberjunction/ng-kendo-modules
export * from './lib/kendo-modules.module';
 * Public API Surface of ng-link-directives
export * from './lib/ng-link-directives.module';
export * from './lib/ng-base-link';
export * from './lib/ng-email-link'; 
export * from './lib/ng-web-link'; 
export * from './lib/ng-field-link'; 
 * Public API Surface of ng-user-view-grid
export * from './lib/ng-list-detail-grid.component';export * from './lib/urlPipe';
export * from './lib/base-dashboard';
export * from './lib/simpleTextFormat';
export * from './lib/shared.service';
export * from './lib/base-resource-component'
export * from './lib/base-navigation-component';
export * from './lib/navigation.service';
export * from './lib/navigation.interfaces';
export * from './lib/title.service';
export * from './lib/developer-mode.service';
export { SYSTEM_APP_ID } from './lib/navigation.service';
// Re-export from ng-shared-generic for backwards compatibility
export * from '@memberjunction/ng-shared-generic';
export * from './lib/simple-record-list/simple-record-list.component';
 * Public API Surface of @memberjunction/ng-workspace-initializer
export * from './lib/models/workspace-types';
export * from './lib/services/workspace-initializer.service';
export * from './lib/workspace-initializer.module';
 * Public API Surface of @memberjunction/ng-testing
export * from './lib/testing.module';
export * from './lib/models/testing.models';
export * from './lib/models/evaluation.types';
// Components
export * from './lib/components/test-feedback-dialog.component';
export * from './lib/components/test-run-dialog.component';
export * from './lib/components/widgets/test-status-badge.component';
export * from './lib/components/widgets/score-indicator.component';
export * from './lib/components/widgets/cost-display.component';
export * from './lib/components/widgets/test-results-matrix.component';
export * from './lib/components/widgets/evaluation-badge.component';
export * from './lib/components/widgets/evaluation-mode-toggle.component';
export * from './lib/components/widgets/review-status-indicator.component';
export * from './lib/components/widgets/execution-context.component';
export * from './lib/services/testing-dialog.service';
export * from './lib/services/testing-execution.service';
export * from './lib/services/evaluation-preferences.service';
// Prevent tree shaking of components
import './lib/action-gallery.component';
// Public API Surface
export * from './lib/action-gallery.component';
export * from './lib/action-gallery-dialog.service';
 * @memberjunction/ng-actions
 * A reusable Angular module for testing and running MemberJunction Actions.
 * This package has no Kendo dependencies and can be used in any Angular application.
export { ActionsModule } from './lib/actions.module';
// Action Test Harness
    ActionTestHarnessComponent,
    ActionParamValue,
    ActionResult
} from './lib/action-test-harness/action-test-harness.component';
// Action Test Harness Dialog
    ActionTestHarnessDialogComponent
} from './lib/action-test-harness-dialog/action-test-harness-dialog.component';
// Action Param Dialog
    ActionParamDialogComponent,
    ActionParamDialogResult
} from './lib/action-param-dialog/action-param-dialog.component';
// Action Result Code Dialog
    ActionResultCodeDialogComponent,
    ActionResultCodeDialogResult
} from './lib/action-result-code-dialog/action-result-code-dialog.component';
 * Public API Surface of @memberjunction/ng-agents
export * from './lib/agents.module';
export * from './lib/services/agent-permissions.service';
export * from './lib/services/create-agent.service';
// Permissions Components
export * from './lib/components/agent-permissions-panel.component';
export * from './lib/components/agent-permissions-dialog.component';
export * from './lib/components/agent-permissions-slideover.component';
// Create Agent Components
export * from './lib/components/create-agent-panel.component';
export * from './lib/components/create-agent-dialog.component';
export * from './lib/components/create-agent-slidein.component';
import './lib/ai-test-harness.component';
import './lib/ai-test-harness-dialog.component';
import './lib/ai-test-harness-window.component';
import './lib/test-harness-custom-window.component';
export * from './lib/ai-test-harness.component';
export * from './lib/ai-test-harness-dialog.component';
export * from './lib/ai-test-harness-window.component';
export * from './lib/test-harness-custom-window.component';
export * from './lib/test-harness-window.service';
export * from './lib/test-harness-window-manager.service';
export * from './lib/ai-test-harness-dialog.service';
export * from './lib/agent-execution-monitor.component';
export * from './lib/agent-execution-node.component';
export * from './lib/json-viewer-window.component';
 * Public API Surface of @memberjunction/ng-artifacts
export * from './lib/artifacts.module';
export * from './lib/services/artifact-icon.service';
export * from './lib/interfaces/artifact-viewer-plugin.interface';
// Base component
export * from './lib/components/base-artifact-viewer.component';
// Artifact type plugin viewer (loads appropriate plugin based on DriverClass)
export * from './lib/components/artifact-type-plugin-viewer.component';
// Artifact viewer UI components
export * from './lib/components/artifact-viewer-panel.component';
export * from './lib/components/artifact-version-history.component';
export * from './lib/components/artifact-message-card.component';
// Plugin components
export * from './lib/components/plugins/json-artifact-viewer.component';
export * from './lib/components/plugins/code-artifact-viewer.component';
export * from './lib/components/plugins/markdown-artifact-viewer.component';
export * from './lib/components/plugins/html-artifact-viewer.component';
export * from './lib/components/plugins/svg-artifact-viewer.component';
export * from './lib/components/plugins/component-artifact-viewer.component';
 * @memberjunction/ng-base-forms
 * Modern form components for rendering and editing MemberJunction entity records.
 * Provides configurable toolbar, inline-editing fields, collapsible panels with
 * IS-A inheritance support, and event-driven navigation.
 * Zero Explorer dependencies - usable in any Angular application.
 * Uses only native HTML elements with custom styling (no PrimeNG dependency).
export * from './lib/types/form-types';
export * from './lib/types/navigation-events';
export * from './lib/types/toolbar-config';
export * from './lib/types/form-events';
export * from './lib/base-record-component';
export * from './lib/base-form-component';
export * from './lib/base-form-section-component';
export * from './lib/base-form-section-info';
export * from './lib/form-state.interface';
export * from './lib/form-state.service';
export * from './lib/toolbar/form-toolbar.component';
export * from './lib/field/form-field.component';
export * from './lib/panel/collapsible-panel.component';
export * from './lib/container/record-form-container.component';
export * from './lib/section-manager/section-manager.component';
export * from './lib/section-loader-component';
export * from './lib/explorer-entity-data-grid.component';
export * from './lib/isa-related-panel/isa-hierarchy-utils';
export * from './lib/isa-related-panel/isa-related-card.component';
export * from './lib/isa-related-panel/isa-related-panel.component';
export * from './base-angular-component';export * from './lib/chat/chat.component';
export * from './lib/ng-code-editor.component';
export * from './lib/ng-code-editor.module';
export * from './lib/toolbar-config';
 * Public API Surface of ng-container-directives
export * from './lib/ng-container-directives.module';
export * from './lib/ng-fill-container-directive';
export * from './lib/ng-container-directive'; 
 * Public API Surface of @memberjunction/ng-conversations
export * from './lib/conversations.module';
export * from './lib/models/conversation-state.model';
export * from './lib/models/notification.model';
export * from './lib/models/lazy-artifact-info';
export * from './lib/models/conversation-complete-query.model';
export * from './lib/models/navigation-request.model';
// Services - State
export * from './lib/services/data-cache.service';
export * from './lib/services/conversation-data.service';
export * from './lib/services/artifact-state.service';
export * from './lib/services/agent-state.service';
export * from './lib/services/conversation-agent.service';
export * from './lib/services/active-tasks.service';
export * from './lib/services/conversation-streaming.service';
export * from './lib/services/dialog.service';
export * from './lib/services/export.service';
export * from './lib/services/notification.service';
export * from './lib/services/toast.service';
export * from './lib/services/mention-parser.service';
export * from './lib/services/mention-autocomplete.service';
export * from './lib/services/collection-permission.service';
export * from './lib/services/artifact-permission.service';
export * from './lib/services/artifact-use-tracking.service';
export * from './lib/services/collection-state.service';
export * from './lib/services/conversation-attachment.service';
export * from './lib/services/ui-command-handler.service';
export * from './lib/components/workspace/conversation-workspace.component';
export * from './lib/components/navigation/conversation-navigation.component';
export * from './lib/components/sidebar/conversation-sidebar.component';
export * from './lib/components/conversation/conversation-list.component';
export * from './lib/components/conversation/conversation-chat-area.component';
export * from './lib/components/conversation/conversation-empty-state.component';
export * from './lib/components/message/message-item.component';
export * from './lib/components/message/message-list.component';
export * from './lib/components/message/message-input.component';
export * from './lib/components/message/message-input-box.component';
export * from './lib/components/message/conversation-message-rating.component';
export * from './lib/components/mention/mention-dropdown.component';
export * from './lib/components/mention/mention-editor.component';
export * from './lib/components/collection/collection-tree.component';
export * from './lib/components/collection/collection-view.component';
export * from './lib/components/collection/collections-full-view.component';
export * from './lib/components/collection/collection-artifact-card.component';
export * from './lib/components/collection/artifact-collection-picker-modal.component';
export * from './lib/components/collection/collection-share-modal.component';
export * from './lib/components/artifact/artifact-share-modal.component';
export * from './lib/components/project/project-selector.component';
export * from './lib/components/project/project-form-modal.component';
export * from './lib/components/task/tasks-full-view.component';
export * from './lib/components/tasks/task-widget.component';
export * from './lib/components/agent/agent-process-panel.component';
export * from './lib/components/agent/active-agent-indicator.component';
export * from './lib/components/active-tasks/active-tasks-panel.component';
export * from './lib/components/share/share-modal.component';
export * from './lib/components/notification/notification-badge.component';
export * from './lib/components/notification/activity-indicator.component';
export * from './lib/components/toast/toast.component';
export * from './lib/components/global-tasks/global-tasks-panel.component';
export * from './lib/components/attachment/image-viewer.component';
 * Public API Surface for @memberjunction/ng-credentials
 * This package provides reusable Angular components for credential management:
 * - Panel components for embedding in existing UIs
 * - Dialog components for modal credential editing
 * - Service for programmatic dialog access
export * from './lib/credentials.module';
// Panel components
export * from './lib/panels/credential-edit-panel/credential-edit-panel.component';
export * from './lib/panels/credential-type-edit-panel/credential-type-edit-panel.component';
export * from './lib/panels/credential-category-edit-panel/credential-category-edit-panel.component';
export * from './lib/dialogs/credential-dialog.component';
export * from './lib/services/credential-dialog.service';
 * Public API Surface of @memberjunction/ng-dashboard-viewer
export * from './lib/dashboard-viewer.module';
// Main Component
export * from './lib/dashboard-viewer/dashboard-viewer.component';
// Dashboard Browser Component
export * from './lib/dashboard-browser/dashboard-browser.component';
// Breadcrumb Component
export * from './lib/breadcrumb/dashboard-breadcrumb.component';
// Generic Dialogs
export * from './lib/dialogs/add-panel-dialog/add-panel-dialog.component';
export * from './lib/dialogs/edit-part-dialog/edit-part-dialog.component';
export * from './lib/config-dialogs/confirm-dialog.component';
// Base Classes for Extensibility
export * from './lib/config-panels/base-config-panel';
export * from './lib/parts/base-dashboard-part';
// Config Panels (pluggable form components loaded via ClassFactory)
export * from './lib/config-panels/weburl-config-panel.component';
export * from './lib/config-panels/view-config-panel.component';
export * from './lib/config-panels/query-config-panel.component';
export * from './lib/config-panels/artifact-config-panel.component';
// Runtime Part Components (pluggable renderers loaded via ClassFactory)
export * from './lib/parts/weburl-part.component';
export * from './lib/parts/view-part.component';
export * from './lib/parts/query-part.component';
export * from './lib/parts/artifact-part.component';
// Types and Models
export * from './lib/models/dashboard-types';
export * from './lib/services/golden-layout-wrapper.service';
export * from './lib/ng-data-context-dialog.component';
export * from './lib/ng-data-context.component';
export * from './lib/deep-diff.component';
export * from './lib/deep-diff-dialog.component';
export * from './lib/preview.component';
export * from './lib/window.component';
 * Public API Surface of @memberjunction/ng-entity-relationship-diagram
export * from './lib/entity-relationship-diagram.module';
// Interfaces and types
export * from './lib/interfaces/erd-types';
export * from './lib/components/erd-diagram.component';
export * from './lib/components/mj-entity-erd.component';
export * from './lib/components/entity-details/entity-details.component';
export * from './lib/components/entity-filter-panel/entity-filter-panel.component';
export * from './lib/components/erd-composite/erd-composite.component';
// Utilities for MJ entity transformation
export * from './lib/utils/entity-to-erd-adapter';
 * @memberjunction/ng-entity-viewer
 * Angular components for viewing entity data in multiple formats.
 * Provides grid (AG Grid) and card views with filtering, selection, and shared data management.
// Types and Interfaces
export * from './lib/types';
export * from './lib/entity-cards/entity-cards.component';
export * from './lib/entity-viewer/entity-viewer.component';
export * from './lib/entity-record-detail-panel/entity-record-detail-panel.component';
export * from './lib/pill/pill.component';
export * from './lib/pagination/pagination.component';
// Entity Data Grid (modern AG Grid component with Before/After events)
export * from './lib/entity-data-grid/entity-data-grid.component';
export * from './lib/entity-data-grid/models/grid-types';
export * from './lib/entity-data-grid/events/grid-events';
// View Config Panel (sliding panel for view configuration)
export * from './lib/view-config-panel/view-config-panel.component';
// Aggregate Panel (card-based aggregate display)
export * from './lib/aggregate-panel/aggregate-panel.component';
// Aggregate Setup Dialog (3-mode dialog for configuring aggregates)
export * from './lib/aggregate-setup-dialog/aggregate-setup-dialog.component';
// Confirm Dialog (generic reusable confirmation dialog)
export * from './lib/confirm-dialog/confirm-dialog.component';
// Quick Save Dialog (focused view save modal)
export * from './lib/quick-save-dialog/quick-save-dialog.component';
// View Header (inline name edit, modified badge, save/revert actions)
export * from './lib/view-header/view-header.component';
// Duplicate View Dialog (modal for duplicating views with custom name)
export * from './lib/duplicate-view-dialog/duplicate-view-dialog.component';
// Shared View Warning Dialog (warning when saving shared views)
export * from './lib/shared-view-warning-dialog/shared-view-warning-dialog.component';
export * from './lib/utils/highlight.util';
export * from './lib/utils/record.util';
 * Public API Surface for @memberjunction/ng-export-service
export * from './lib/export.service';
export * from './lib/export-dialog.component';
// NOTE: For export types (ExportFormat, ExportOptions, etc.), import directly from @memberjunction/export-engine
export * from './lib/category-tree/category-tree';
export * from './lib/files-grid/files-grid';
export * from './lib/file-browser/file-browser.component';
export * from './lib/file-browser/file-browser-demo.component';
export * from './lib/file-browser/file-browser-resource.component';
 * @memberjunction/ng-filter-builder
 * A modern, intuitive filter builder component for Angular applications.
 * Creates complex boolean filter expressions with Kendo-compatible JSON output.
 * @packageDocumentation
export { FilterBuilderModule } from './lib/filter-builder.module';
export { FilterBuilderComponent } from './lib/filter-builder/filter-builder.component';
export { FilterGroupComponent } from './lib/filter-group/filter-group.component';
export { FilterRuleComponent } from './lib/filter-rule/filter-rule.component';
  FilterOperator,
  FilterLogic,
  FilterDescriptor,
  FilterValueOption,
  FilterBuilderConfig,
  isCompositeFilter,
  isSimpleFilter,
  createEmptyFilter,
  createFilterRule
} from './lib/types/filter.types';
// Operators
  OperatorInfo,
  STRING_OPERATORS,
  NUMBER_OPERATORS,
  BOOLEAN_OPERATORS,
  DATE_OPERATORS,
  LOOKUP_OPERATORS,
  getOperatorsForType,
  getOperatorInfo,
  operatorRequiresValue
} from './lib/types/operators';
export * from './lib/find-record.component';
export * from './lib/dialog.component';
 * Public API Surface of @memberjunction/ng-flow-editor
export * from './lib/flow-editor.module';
export * from './lib/interfaces/flow-types';
export * from './lib/services/flow-state.service';
export * from './lib/services/flow-layout.service';
// Generic Components
export * from './lib/components/flow-editor.component';
export * from './lib/components/flow-node.component';
export * from './lib/components/flow-palette.component';
export * from './lib/components/flow-toolbar.component';
export * from './lib/components/flow-status-bar.component';
// Agent Editor Components
export * from './lib/agent-editor/flow-agent-editor.component';
export * from './lib/agent-editor/agent-properties-panel.component';
export * from './lib/agent-editor/agent-step-list.component';
export * from './lib/agent-editor/agent-flow-transformer.service';
export * from './lib/join-grid/join-grid.component';
 * Public API Surface for @memberjunction/ng-list-management
export * from './lib/components/list-management-dialog/list-management-dialog.component';
export * from './lib/components/list-share-dialog/list-share-dialog.component';
export * from './lib/services/list-management.service';
export * from './lib/services/list-sharing.service';
export * from './lib/models/list-management.models';
export * from './lib/models/list-sharing.models';
// Public API Surface of @memberjunction/ng-markdown
export * from './lib/markdown.module';
export * from './lib/components/markdown.component';
export * from './lib/services/markdown.service';
// Extensions (for advanced customization)
export * from './lib/extensions/collapsible-headings.extension';
export * from './lib/extensions/code-copy.extension';
export * from './lib/extensions/svg-renderer.extension';
export * from './lib/types/markdown.types';
export * from './lib/notifications.service'; * Public API Surface of ng-query-grid
 * @deprecated This package is deprecated. Use `@memberjunction/ng-query-viewer` instead.
 * The new package provides better features including state persistence, parameter persistence,
 * entity linking, and auto-run capability.
export * from './lib/ng-query-grid.component';
export * from './lib/ng-query-grid.module';
 * Public API Surface of @memberjunction/ng-query-viewer
export * from './lib/query-viewer.module';
export * from './lib/query-data-grid/query-data-grid.component';
export * from './lib/query-parameter-form/query-parameter-form.component';
export * from './lib/query-viewer/query-viewer.component';
export * from './lib/query-row-detail/query-row-detail.component';
export * from './lib/query-info-panel/query-info-panel.component';
export * from './lib/query-data-grid/models/query-grid-types';
 * @fileoverview Public API Surface of @memberjunction/ng-react
 * This file exports all public APIs from the Angular React integration library.
export * from './lib/components/mj-react-component.component';
export * from './lib/services/script-loader.service';
export * from './lib/services/react-bridge.service';
export * from './lib/services/angular-adapter.service';
export * from './lib/config/react-debug.config';
export * from './lib/ng-record-changes.component';
export * from './lib/ng-record-changes.module';
export * from './lib/record-selector.component';
export * from './lib/resource-permissions.component';
export * from './lib/available-resources.component';
export * from './lib/request-access.component';
 * Public API Surface for @memberjunction/ng-shared-generic
export * from './lib/recent-access.service';
export * from './lib/loading/loading.component';
export * from './lib/tab-strip/tab-strip.component';
export * from './lib/tab-body/tab-body.component';
export * from './lib/tab/tab.component';
export * from './lib/tab.base'
 * Public API Surface of @memberjunction/ng-tasks
export * from './lib/ng-tasks.module';
export * from './lib/components/task.component';
export * from './lib/components/simple-task-viewer.component';
export * from './lib/components/gantt-task-viewer.component';
export * from './lib/models/task-view.models';
 * @fileoverview Public API surface for @memberjunction/ng-timeline
 * This module exports all public types, classes, and components for the
 * MJ Timeline component. The component works with both MemberJunction
 * BaseEntity objects and plain JavaScript objects.
// Core types and interfaces
// Event interfaces (BeforeX/AfterX pattern)
export * from './lib/events';
// TimelineGroup class
export * from './lib/timeline-group';
// Component and Module
export * from './lib/component/timeline.component';
 * Public API Surface of @memberjunction/ng-trees
export * from './lib/ng-trees.module';
export * from './lib/tree/tree.component';
export * from './lib/tree-dropdown/tree-dropdown.component';
export * from './lib/models/tree-types';
export * from './lib/events/tree-events';
 * Public API Surface of ng-user-avatar
export * from './lib/user-avatar.service';
export * from './lib/versions.module';
export * from './lib/panel/slide-panel.component';
export * from './lib/record-micro-view/record-micro-view.component';
export * from './lib/label-create/label-create.component';
export * from './lib/label-detail/label-detail.component';
export * from  './dashboards/events-dashboard/events-dashboard.component'; * Public API Surface of @memberjunction/ng-bootstrap
export * from './generated/mj-class-registrations'; * Public API Surface
export * from './lib/providers/mjexplorer-okta-provider.service'; * @memberjunction/ng-base-application
export * from './lib/ng-list-detail-grid.component';export * from './lib/urlPipe';
export * from './base-angular-component';export * from './lib/chat/chat.component';
export * from './lib/notifications.service'; * Public API Surface of ng-query-grid
 * Public API Surface of @memberjunction/ng-bootstrap
export * from './generated/mj-class-registrations'; * Public API Surface
export * from './lib/providers/mjexplorer-okta-provider.service'; * @memberjunction/ng-base-application
export * from './lib/ng-list-detail-grid.component';export * from './lib/urlPipe';
export * from './base-angular-component';export * from './lib/chat/chat.component';
export * from './lib/notifications.service'; * Public API Surface of ng-query-grid
export * from  './dashboards/events-dashboard/events-dashboard.component';