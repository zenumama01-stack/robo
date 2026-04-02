 * Action to reorder posts in a Buffer profile's queue
@RegisterClass(BaseAction, 'BufferReorderQueueAction')
export class BufferReorderQueueAction extends BufferBaseAction {
     * Reorder posts in the queue
            const updateIds = this.getParamValue(Params, 'UpdateIDs');
            const offset = this.getParamValue(Params, 'Offset');
            if (!updateIds || !Array.isArray(updateIds) || updateIds.length === 0) {
                throw new Error('UpdateIDs array is required with at least one update ID');
            // Reorder the updates
            const result = await this.reorderUpdates(profileId, updateIds, offset);
            // Get the new queue order to confirm
            const pendingPosts = await this.getUpdates(profileId, 'pending', {
                count: updateIds.length + (offset || 0) + 10
            // Find the reordered posts in the new queue
            const reorderedPosts = pendingPosts.updates?.filter((update: any) => 
                updateIds.includes(update.id)
                reorderedCount: updateIds.length,
                offset: offset || 0,
                newOrder: updateIds,
                success: result.success === true,
                newPositions: reorderedPosts.map((post: any, index: number) => ({
                    id: post.id,
                    position: index + (offset || 0),
                    scheduledFor: post.due_at ? new Date(post.due_at * 1000) : null,
                    text: post.text?.substring(0, 100) + (post.text?.length > 100 ? '...' : '')
            const reorderedParam = outputParams.find(p => p.Name === 'ReorderedPosts');
            if (reorderedParam) reorderedParam.Value = reorderedPosts;
                Message: `Successfully reordered ${updateIds.length} posts in Buffer queue`,
                Message: `Failed to reorder queue: ${errorMessage}`,
                Name: 'UpdateIDs',
                Name: 'ReorderedPosts',
        return 'Reorders posts in a Buffer profile\'s queue, allowing you to change the posting schedule order';
