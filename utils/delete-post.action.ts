 * Action to delete a post from Buffer
@RegisterClass(BaseAction, 'BufferDeletePostAction')
export class BufferDeletePostAction extends BufferBaseAction {
     * Delete a Buffer post
            const updateId = this.getParamValue(Params, 'UpdateID');
            if (!updateId) {
                throw new Error('UpdateID is required');
            // Delete the update
            const success = await this.deleteUpdate(updateId);
                updateId: updateId,
                deleted: success,
                deletedAt: new Date()
            const deletedParam = outputParams.find(p => p.Name === 'Deleted');
            if (deletedParam) deletedParam.Value = success;
                    Message: `Successfully deleted Buffer post ${updateId}`,
                    ResultCode: 'DELETE_FAILED',
                    Message: `Failed to delete Buffer post ${updateId}`,
                Message: `Failed to delete post: ${errorMessage}`,
                Name: 'UpdateID',
                Name: 'Deleted',
        return 'Deletes a pending or sent post from Buffer';
