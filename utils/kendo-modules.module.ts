 * Kendo UI Modules Bundle
 * Consolidates all Kendo UI Angular modules into a single import.
 * This reduces boilerplate in application module files and provides
 * a consistent set of Kendo UI components across all MemberJunction applications.
 * import { MJKendoModule } from '@memberjunction/ng-kendo-modules';
 *     MJKendoModule,  // ← Replaces 11 individual Kendo imports
 *     // ... other modules
// Import all Kendo UI Angular modules
import { IconsModule } from '@progress/kendo-angular-icons';
import { NotificationModule } from '@progress/kendo-angular-notification';
 * MJKendoModule - Consolidated bundle of all Kendo UI Angular modules
 * Re-exports all commonly used Kendo UI modules to reduce boilerplate
 * in application module files.
 * @module MJKendoModule
    // Grid and Data
    // Layout and Structure
    IconsModule,
    // Form Controls
    // Dialogs and Notifications
    NotificationModule
export class MJKendoModule {}
