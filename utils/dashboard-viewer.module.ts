// MJ Shared Components
// MJ Trees for hierarchical selection
import { NgTreesModule } from '@memberjunction/ng-trees';
// MJ Entity Viewer for displaying entity data in grid/cards/timeline
// MJ Query Viewer for displaying query results
// MJ Artifacts for displaying conversation artifacts
import { DashboardViewerComponent } from './dashboard-viewer/dashboard-viewer.component';
import { AddPanelDialogComponent } from './dialogs/add-panel-dialog/add-panel-dialog.component';
import { EditPartDialogComponent } from './dialogs/edit-part-dialog/edit-part-dialog.component';
// Confirm Dialog (generic utility)
import { ConfirmDialogComponent } from './config-dialogs/confirm-dialog.component';
// Config Panels (pluggable form components for each part type)
import { WebURLConfigPanelComponent } from './config-panels/weburl-config-panel.component';
import { ViewConfigPanelComponent } from './config-panels/view-config-panel.component';
import { QueryConfigPanelComponent } from './config-panels/query-config-panel.component';
import { ArtifactConfigPanelComponent } from './config-panels/artifact-config-panel.component';
// Runtime Part Components (pluggable renderers for each part type)
import { WebURLPartComponent } from './parts/weburl-part.component';
import { ViewPartComponent } from './parts/view-part.component';
import { QueryPartComponent } from './parts/query-part.component';
import { ArtifactPartComponent } from './parts/artifact-part.component';
// Dashboard Browser Component (generic, no routing dependencies)
import { DashboardBrowserComponent } from './dashboard-browser/dashboard-browser.component';
// Breadcrumb Component (reusable navigation component)
import { DashboardBreadcrumbComponent } from './breadcrumb/dashboard-breadcrumb.component';
        DashboardBrowserComponent,
        DashboardBreadcrumbComponent,
        AddPanelDialogComponent,
        EditPartDialogComponent,
        ConfirmDialogComponent,
        // Config Panels (pluggable form components)
        // These are registered with @RegisterClass and loaded dynamically via ClassFactory
        // Runtime Part Components (pluggable renderers)
        ArtifactPartComponent
        NgTreesModule,
        ArtifactsModule
        // Config Panels - exported for potential direct use
        // Runtime Part Components - exported for potential direct use
export class DashboardViewerModule { }
