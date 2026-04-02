 * Explorer Modules Bundle
 * Consolidates all MemberJunction Explorer modules and Kendo UI modules into a single import.
 * This dramatically reduces boilerplate in application module files and provides
 * a consistent set of MJ functionality across all applications.
 * import { MJExplorerModulesBundle } from '@memberjunction/ng-explorer-modules';
 *     MJExplorerModulesBundle,  // ← Replaces 20+ individual module imports
 *     // ... your app-specific modules
// Import all MemberJunction Explorer modules
import { ExplorerCoreModule, ShellModule } from '@memberjunction/ng-explorer-core';
import { CoreGeneratedFormsModule } from '@memberjunction/ng-core-entity-forms';
import { WorkspaceInitializerModule } from '@memberjunction/ng-workspace-initializer';
// Import consolidated Kendo modules
import { MJKendoModule } from '@memberjunction/ng-kendo-modules';
 * MJExplorerModulesBundle - Consolidated bundle of all MJ Explorer and Kendo UI modules
 * Re-exports all commonly used MJ Explorer modules and Kendo UI modules to drastically
 * reduce boilerplate in application module files.
 * This module provides:
 * - All MemberJunction Explorer functionality (forms, grids, directives, shell)
 * - All Kendo UI components (grid, layout, inputs, dialogs, etc.)
 * - Workspace initialization
 * - User settings and preferences
 * @module MJExplorerModulesBundle
    // MemberJunction Explorer Modules
    ExplorerCoreModule,
    CoreGeneratedFormsModule,
    WorkspaceInitializerModule,
    // Kendo UI Modules (consolidated)
    MJKendoModule
export class MJExplorerModulesBundle {}
