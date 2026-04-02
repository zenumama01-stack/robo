import { FacebookBaseAction, FacebookComment } from '../facebook-base.action';
 * Responds to comments on Facebook posts or other comments.
 * Supports replying to top-level comments and nested replies.
@RegisterClass(BaseAction, 'FacebookRespondToCommentsAction')
export class FacebookRespondToCommentsAction extends FacebookBaseAction {
        return 'Responds to comments on Facebook posts, pages, or other comments with text replies or reactions';
                Name: 'CommentID',
                Name: 'ResponseText',
                Name: 'AttachmentURL',
                Name: 'LikeComment',
                Name: 'HideComment',
                Name: 'DeleteComment',
                Name: 'PrivateReply',
            const commentId = this.getParamValue(Params, 'CommentID');
            if (!commentId) {
                Message: 'CommentID is required',
            const responseText = this.getParamValue(Params, 'ResponseText') as string;
            const attachmentUrl = this.getParamValue(Params, 'AttachmentURL') as string;
            const likeComment = this.getParamValue(Params, 'LikeComment') as boolean;
            const hideComment = this.getParamValue(Params, 'HideComment') as boolean;
            const deleteComment = this.getParamValue(Params, 'DeleteComment') as boolean;
            const privateReply = this.getParamValue(Params, 'PrivateReply') as boolean;
            const pageId = this.getParamValue(Params, 'PageID') as string;
            // Validate that at least one action is specified
            if (!responseText && !attachmentUrl && !likeComment && !hideComment && !deleteComment) {
                Message: 'At least one action (ResponseText, AttachmentURL, LikeComment, HideComment, or DeleteComment) is required',
                ResultCode: 'MISSING_ACTION'
            // Get appropriate access token
            let accessToken = this.getAccessToken();
            if (pageId) {
                // Use page access token for page actions
                accessToken = await this.getPageAccessToken(pageId);
            LogStatus(`Processing comment ${commentId}...`);
            // Get comment details first
            const commentDetails = await this.getCommentDetails(commentId, accessToken!);
            if (!commentDetails) {
                Message: 'Comment not found or access denied',
                ResultCode: 'NOT_FOUND'
            const results: any = {
                commentId,
                originalComment: {
                    message: commentDetails.message,
                    from: commentDetails.from,
                    createdTime: commentDetails.created_time
                actions: []
            // Handle delete action first (if specified)
            if (deleteComment) {
                    await this.deleteCommentAction(commentId, accessToken!);
                    results.actions.push({ action: 'delete', success: true });
                    LogStatus(`Deleted comment ${commentId}`);
                    // If deleted, no other actions can be performed
                Message: 'Comment deleted successfully',
                    LogError(`Failed to delete comment: ${error}`);
                    results.actions.push({ 
                        action: 'delete', 
                        error: error instanceof Error ? error.message : 'Unknown error' 
            // Handle hide action
            if (hideComment) {
                    await this.hideCommentAction(commentId, accessToken!, true);
                    results.actions.push({ action: 'hide', success: true });
                    LogStatus(`Hidden comment ${commentId}`);
                    LogError(`Failed to hide comment: ${error}`);
                        action: 'hide', 
            // Handle like action
            if (likeComment) {
                    await this.likeCommentAction(commentId, accessToken!);
                    results.actions.push({ action: 'like', success: true });
                    LogStatus(`Liked comment ${commentId}`);
                    LogError(`Failed to like comment: ${error}`);
                        action: 'like', 
            // Handle reply action
            if (responseText || attachmentUrl) {
                    const replyResult = await this.replyToComment(
                        responseText,
                        attachmentUrl,
                        privateReply,
                        accessToken!
                        action: privateReply ? 'private_reply' : 'reply', 
                        replyId: replyResult.id,
                        message: responseText
                    LogStatus(`Replied to comment ${commentId}`);
                    LogError(`Failed to reply to comment: ${error}`);
            // Check if any actions succeeded
            const successfulActions = results.actions.filter((a: any) => a.success);
            if (successfulActions.length === 0) {
                    Message: 'All requested actions failed',
                Message: `Successfully completed ${successfulActions.length} of ${results.actions.length} actions`,
            LogError(`Failed to respond to Facebook comment: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get comment details
    private async getCommentDetails(commentId: string, accessToken: string): Promise<FacebookComment | null> {
                `${this.apiBaseUrl}/${commentId}`,
                        access_token: accessToken,
                        fields: 'id,message,created_time,from,like_count,comment_count,parent'
            LogError(`Failed to get comment details: ${error}`);
     * Reply to a comment
    private async replyToComment(
        commentId: string,
        message: string | null,
        attachmentUrl: string | null,
        privateReply: boolean,
        const endpoint = privateReply 
            ? `${this.apiBaseUrl}/${commentId}/private_replies`
            : `${this.apiBaseUrl}/${commentId}/comments`;
        const data: any = {};
            data.message = message;
        if (attachmentUrl) {
            data.attachment_url = attachmentUrl;
        const response = await axios.post(endpoint, data, {
                access_token: accessToken
     * Like a comment
    private async likeCommentAction(commentId: string, accessToken: string): Promise<void> {
            `${this.apiBaseUrl}/${commentId}/likes`,
     * Hide or unhide a comment
    private async hideCommentAction(commentId: string, accessToken: string, hide: boolean): Promise<void> {
                is_hidden: hide
     * Delete a comment
    private async deleteCommentAction(commentId: string, accessToken: string): Promise<void> {
        await axios.delete(
