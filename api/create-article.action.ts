import { LinkedInBaseAction, LinkedInShareData, LinkedInArticle } from '../linkedin-base.action';
 * Action to create a LinkedIn article (long-form content)
@RegisterClass(BaseAction, 'LinkedInCreateArticleAction')
export class LinkedInCreateArticleAction extends LinkedInBaseAction {
     * Create a LinkedIn article
            const title = this.getParamValue(Params, 'Title');
            const coverImage = this.getParamValue(Params, 'CoverImage');
            const authorType = this.getParamValue(Params, 'AuthorType') || 'personal'; // 'personal' or 'organization'
            const organizationId = this.getParamValue(Params, 'OrganizationID');
            const visibility = this.getParamValue(Params, 'Visibility') || 'PUBLIC';
            const publishImmediately = this.getParamValue(Params, 'PublishImmediately') !== false; // Default true
                throw new Error('Title is required');
            // Determine author URN
            let authorUrn: string;
            if (authorType === 'organization') {
                if (!organizationId) {
                    // Get first admin organization if not specified
                    const orgs = await this.getAdminOrganizations();
                    if (orgs.length === 0) {
                        throw new Error('No organizations found. Please specify OrganizationID.');
                    authorUrn = orgs[0].urn;
                    LogStatus(`Using organization: ${orgs[0].name}`);
                    authorUrn = `urn:li:organization:${organizationId}`;
                // Personal article
                authorUrn = await this.getCurrentUserUrn();
            // Upload cover image if provided
            let coverImageUrn: string | undefined;
            if (coverImage) {
                LogStatus('Uploading cover image...');
                const coverImageFile = coverImage as MediaFile;
                const uploadedUrns = await this.uploadMedia([coverImageFile]);
                coverImageUrn = uploadedUrns[0];
            // Create article share data
            const articleShareData: LinkedInShareData = {
                author: authorUrn,
                lifecycleState: publishImmediately ? 'PUBLISHED' : 'DRAFT',
                            text: description || `Check out my new article: ${title}`
                        shareMediaCategory: 'ARTICLE',
                        media: [{
                            status: 'READY' as const,
                            media: '', // Article URL will be filled after creation
                                text: title
                            description: description ? {
                                text: description
                    'com.linkedin.ugc.MemberNetworkVisibility': visibility as any
            // Format content for LinkedIn article
            const formattedContent = this.formatArticleContent(content);
            // Create the article
            LogStatus('Creating LinkedIn article...');
            // Note: LinkedIn's v2 API has limited article creation support
            // Full article creation typically requires using the Publishing Platform API
            // This implementation creates an article-style share with rich content
            // For now, we'll create a rich media share that looks like an article
            const articlePost = await this.createRichMediaShare(
                authorUrn,
                formattedContent,
                coverImageUrn,
                visibility,
                publishImmediately
            const articleParam = outputParams.find(p => p.Name === 'Article');
            if (articleParam) articleParam.Value = articlePost;
            const articleIdParam = outputParams.find(p => p.Name === 'ArticleID');
            if (articleIdParam) articleIdParam.Value = articlePost.id;
                Message: `Successfully created LinkedIn article: ${title}`,
                Message: `Failed to create LinkedIn article: ${errorMessage}`,
     * Format article content for LinkedIn
    private formatArticleContent(content: string): string {
        // LinkedIn articles support basic HTML formatting
        // Convert markdown-style formatting to LinkedIn-compatible format
        let formatted = content;
        // Convert markdown headers to bold text
        formatted = formatted.replace(/^### (.+)$/gm, '\n**$1**\n');
        formatted = formatted.replace(/^## (.+)$/gm, '\n**$1**\n');
        formatted = formatted.replace(/^# (.+)$/gm, '\n**$1**\n');
        // Convert markdown bold
        formatted = formatted.replace(/\*\*(.+?)\*\*/g, '**$1**');
        // Convert markdown italic
        formatted = formatted.replace(/\*(.+?)\*/g, '_$1_');
        // Add line breaks for paragraphs
        formatted = formatted.replace(/\n\n/g, '\n\n');
        // Truncate if too long (LinkedIn has character limits)
        const maxLength = 3000; // LinkedIn's limit for share commentary
        if (formatted.length > maxLength) {
            formatted = formatted.substring(0, maxLength - 3) + '...';
     * Create a rich media share that resembles an article
    private async createRichMediaShare(
        authorUrn: string,
        content: string,
        description?: string,
        coverImageUrn?: string,
        visibility: string = 'PUBLIC',
        publishImmediately: boolean = true
            // Create a rich share with article-like formatting
            const shareData: LinkedInShareData = {
                            text: `📝 ${title}\n\n${content}`
                        shareMediaCategory: coverImageUrn ? 'IMAGE' : 'NONE',
                        media: coverImageUrn ? [{
                            media: coverImageUrn,
                        }] : undefined
            const postId = await this.createShare(shareData);
            // Return article-like object
                id: postId,
                title: title,
                description: description,
                coverImage: coverImageUrn,
                publishedAt: publishImmediately ? new Date().toISOString() : null,
                visibility: visibility,
                url: `https://www.linkedin.com/feed/update/${postId}/`
            LogError(`Failed to create rich media share: ${error instanceof Error ? error.message : 'Unknown error'}`);
                Name: 'CoverImage',
                Name: 'AuthorType',
                Value: 'personal' // 'personal' or 'organization'
                Name: 'OrganizationID',
                Name: 'Visibility',
                Value: 'PUBLIC' // 'PUBLIC', 'CONNECTIONS', 'LOGGED_IN', 'CONTAINER'
                Name: 'PublishImmediately',
                Name: 'Article',
                Name: 'ArticleID',
        return 'Creates a LinkedIn article (long-form content) with title, content, and optional cover image. Note: Uses rich media shares to simulate article functionality due to API limitations.';
