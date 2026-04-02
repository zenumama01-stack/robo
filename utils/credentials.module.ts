 * @fileoverview Credentials Module
 * Angular module providing reusable credential management components.
 * Includes panels and dialogs for creating and editing credentials,
 * credential types, and credential categories.
import { CredentialEditPanelComponent } from './panels/credential-edit-panel/credential-edit-panel.component';
import { CredentialTypeEditPanelComponent } from './panels/credential-type-edit-panel/credential-type-edit-panel.component';
import { CredentialCategoryEditPanelComponent } from './panels/credential-category-edit-panel/credential-category-edit-panel.component';
import { CredentialDialogComponent } from './dialogs/credential-dialog.component';
        // Panels
        CredentialEditPanelComponent,
        CredentialTypeEditPanelComponent,
        CredentialCategoryEditPanelComponent,
        // Dialogs
        CredentialDialogComponent
export class CredentialsModule { }
