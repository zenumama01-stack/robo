import { CodeNameFromString, EntityFieldValueListType, EntityInfo, Metadata, SeverityType, TypeScriptTypeFromSQLType } from '@memberjunction/core';
import { makeDir } from '../Misc/util';
import { MJActionEntity, MJActionLibraryEntity } from '@memberjunction/core-entities';
import { ActionEntityServerEntity } from '@memberjunction/core-entities-server';
import { logError, logMessage, logStatus } from './status_logging';
import { mkdirSync } from 'fs';
 * Base class for generating entity sub-classes, you can sub-class this class to modify/extend your own entity sub-class generator logic
export class ActionSubClassGeneratorBase {
    protected getAllActionLibrariesAndUsedItems(actions: ActionEntityExtended[]) {
        // get all of the libraries from the combination of distinct libraries from all of the actions we have here
        const allActionLibraries: {Library: string, LibraryID: string, ItemsUsedArray: string[]}[] = [];
            action.Libraries.forEach(lib => {
                if (!allActionLibraries.find(l => l.LibraryID === lib.LibraryID)) {
                    allActionLibraries.push({
                        Library: lib.Library,
                        LibraryID: lib.LibraryID,
                        ItemsUsedArray: lib.ItemsUsed && lib.ItemsUsed.length > 0 ? lib.ItemsUsed.split(',').map(item => item.trim()) : []
                    // lib already in array, make sure the ItemsUsed for this paritcular Action are merged in to the ItemsUsed array in the entry
                    // in the allActionLibraries array element
                    const existingLib = allActionLibraries.find(l => l.LibraryID === lib.LibraryID);
                    if(existingLib && lib.ItemsUsed && lib.ItemsUsed.length > 0) {
                        const itemsUsed = lib.ItemsUsed.split(',').map(item => item.trim());
                        if(itemsUsed.length > 0) {
                            itemsUsed.forEach(item => {
                                if (!existingLib.ItemsUsedArray.includes(item)) {
                                    existingLib.ItemsUsedArray.push(item);
        return allActionLibraries;
    public async generateActions(actions: ActionEntityExtended[], directory: string): Promise<boolean> {
            const actionFilePath = path.join(directory, 'action_subclasses.ts');
            // Sort actions alphabetically by name for consistent output across CodeGen runs
            // This prevents git diffs from showing random reordering when no actual changes occurred
            const sortedActions = [...actions].sort((a, b) => a.Name.localeCompare(b.Name));
            const allActionLibraries = this.getAllActionLibrariesAndUsedItems(sortedActions);
            const actionHeader = `/*************************************************
${allActionLibraries.map(lib => `import { ${lib.ItemsUsedArray.map(item => item).join(', ')} } from "${lib.Library}";`).join('\n')}
            let sCode: string = "";
            for (const action of sortedActions) {
                sCode += await this.generateSingleAction(action, directory);
            let actionCode = actionHeader + sCode;
            // Note: LoadGeneratedActions() stub function has been removed.
            // Tree-shaking prevention is now handled by the pre-built class registration manifest system.
            mkdirSync(directory, { recursive: true });
            fs.writeFileSync(actionFilePath, actionCode);
            logError(`Error generating actions`, e);
     * description: Generate a single Action
     * @param action 
     * @param directory 
    public async generateSingleAction(action: MJActionEntity, directory: string): Promise<string> {
        if (action.Status !== 'Active' || action.CodeApprovalStatus !=='Approved' || action.Type !== 'Generated') {
            // either the action is not active, not approved, or is NOT a Generated action, so skip it
            const codeName = CodeNameFromString(action.Name);
            const actionClassName = codeName + '_Action';
            // replace all \n with \t\t\n
            const generatedCode = action.Code ? action.Code.replace(/\n/g, '\n\t\t') : 'throw new Error("Action not yet implemented")';
            const codeComments = action.CodeComments ? action.CodeComments.replace(/\n/g, '\n\t\t') : '';
            const codeCommentsInserted = codeComments ? `/*\n\t\t${codeComments}\n\t*/` : '';
            const actionCode = `
 * ${action.Name}
 * User Prompt: ${action.UserPrompt}${action.UserComments ? "\n * User Comments: " + action.UserComments : ""}
@RegisterClass(BaseAction, "${action.Name}")
export class ${actionClassName} extends BaseAction {
    ${codeCommentsInserted}
        ${generatedCode}
            return actionCode;
            logError(`Error generating action ${action.Name}`, e);
            throw e
