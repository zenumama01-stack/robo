 * Action for moving/renaming a file object within a storage provider.
 *   ActionName: 'Move Storage Object',
 *     Value: 'Box.com'
 *     Value: 'temp/file.txt'
 *     Value: 'permanent/file.txt'
@RegisterClass(BaseAction, "File Storage: Move Object")
export class MoveObjectAction extends BaseFileStorageAction {
     * Move/rename a file object
     *   - Success: Boolean indicating if the move succeeded
            // Execute the move operation
            const success: boolean = await driver!.MoveObject(sourceObjectName, destinationObjectName);
                operation: 'MoveObject',
                `Move object operation failed: ${error instanceof Error ? error.message : String(error)}`,
