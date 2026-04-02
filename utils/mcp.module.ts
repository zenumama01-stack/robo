 * @fileoverview MCP Management Module
 * Angular module for MCP (Model Context Protocol) server management.
 * Provides components for managing servers, connections, tools, and viewing logs.
// Kendo UI Modules
import { DialogModule } from '@progress/kendo-angular-dialog';
// MemberJunction Modules
import { SharedPipesModule } from '../shared/shared-pipes.module';
// MCP Components
import { MCPDashboardComponent } from './mcp-dashboard.component';
import { MCPResourceComponent } from './mcp-resource.component';
import { MCPFilterPanelComponent } from './mcp-filter-panel.component';
import { MCPServerDialogComponent } from './components/mcp-server-dialog.component';
import { MCPConnectionDialogComponent } from './components/mcp-connection-dialog.component';
import { MCPTestToolDialogComponent } from './components/mcp-test-tool-dialog.component';
import { MCPLogDetailPanelComponent } from './components/mcp-log-detail-panel.component';
        MCPResourceComponent,
        MCPFilterPanelComponent,
        MCPTestToolDialogComponent,
        MCPLogDetailPanelComponent
        DialogModule,
        SharedPipesModule
export class MCPModule { }
