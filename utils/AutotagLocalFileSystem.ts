import { UserInfo, Metadata, RunView } from "@memberjunction/core";
@RegisterClass(AutotagBase, 'AutotagLocalFileSystem')
export class AutotagLocalFileSystem extends AutotagBase {
    static _openAI: OpenAI;
    public getContextUser(): UserInfo | null {
        return this.contextUser;
     * Implemented abstract method from the AutotagBase class. that runs the entire autotagging process. This method is the entry point for the autotagging process.
     * It initializes the connection, retrieves the content sources corresponding to the content source type, sets the content items that we want to process, 
     * extracts and processes the text, and sets the results in the database.
        this.contentSourceTypeID = await this.engine.setSubclassContentSourceType('Local File System', this.contextUser);
    * Implemented abstract method from the AutotagBase class. Given a list of content sources, this method should return a list 
            // First check that the directory exists
            if (fs.existsSync(contentSource.URL)) {
                const contentSourceParams = await this.setContentSourceParams(contentSource);
                const lastRunDate: Date = await this.engine.getContentSourceLastRunDate(contentSourceParams.contentSourceID, this.contextUser)
                // Traverse through all the files in the directory
                        console.log(`No content items found to process for content source: ${contentSource.Get('Name')}`);
                throw new Error('Invalid last run date');
                console.log(`Invalid Content Source ${contentSource.Name}`);
    public async setContentSourceParams(contentSource: MJContentSourceEntity) { 
        return contentSourceParams;
     * Given a content source and last run date, recursively traverse through the directory and return a 
     * list of content source items that have been modified or added after the last run date.
     * @param contentSource 
     * @param lastRunDate 
    public async SetNewAndModifiedContentItems(contentSourceParams: ContentSourceParams, lastRunDate: Date, contextUser: UserInfo): Promise<MJContentItemEntity[]> {
        const contentItems: MJContentItemEntity[] = []
        let contentSourcePath = contentSourceParams.URL
        const filesAndDirs = fs.readdirSync(contentSourcePath)
        for (const file of filesAndDirs) {
            const filePath = path.join(contentSourcePath, file)
            const stats = fs.statSync(filePath)
            if (stats.isDirectory()) {
                contentSourceParams.URL = filePath
                await this.SetNewAndModifiedContentItems(contentSourceParams, lastRunDate, contextUser)
            else if (stats.isFile()) {
                const modifiedDate = new Date(stats.mtime.toUTCString())
                const changedDate = new Date(stats.ctime.toUTCString())
                if (changedDate > lastRunDate) {
                    // The file has been added, create a new record for this file
                    const contentItem = await this.setAddedContentItem(filePath, contentSourceParams);
                    contentItems.push(contentItem); // Content item was added, add to list
                else if (modifiedDate > lastRunDate) {
                    // The file's contents has been, update the record for this file 
                    const contentItem = await this.setModifiedContentItem(filePath, contentSourceParams);
    public async setAddedContentItem(filePath: string, contentSourceParams: ContentSourceParams): Promise<MJContentItemEntity> { 
        const text = await this.engine.parseFileFromPath(filePath);
        contentItem.URL = contentSourceParams.URL
    public async setModifiedContentItem(filePath: string, contentSourceParams: ContentSourceParams): Promise<MJContentItemEntity> {
        const contentItemID: string = await this.engine.getContentItemIDFromURL(contentSourceParams, this.contextUser);
        await contentItem.Load(contentItemID);
