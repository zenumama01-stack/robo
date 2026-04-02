import { RSSItem } from './RSS.types';
import Parser from 'rss-parser'
@RegisterClass(AutotagBase, 'AutotagRSSFeed')
export class AutotagRSSFeed extends AutotagBase {
    protected getContextUser(): UserInfo {
        this.contentSourceTypeID = await this.engine.setSubclassContentSourceType('RSS Feed', this.contextUser);
        const contentSources = await this.engine.getAllContentSources(this.contextUser, this.contentSourceTypeID);
        const contentItemsToProcess = await this.SetContentItemsToProcess(contentSources);
            const allRSSItems: RSSItem[] = await this.parseRSSFeed(contentSourceParams.URL);
            const contentItems: MJContentItemEntity[] = await this.SetNewAndModifiedContentItems(allRSSItems, contentSourceParams)
    public async SetNewAndModifiedContentItems(allRSSItems: RSSItem[], contentSourceParams: ContentSourceParams): Promise<MJContentItemEntity[]> {
        const contentItemsToProcess: MJContentItemEntity[] = [];
        for (const RSSContentItem of allRSSItems) {
                ExtraFilter: `ContentSourceID = '${contentSourceParams.contentSourceID}' AND (URL = '${RSSContentItem.link}' OR Description = '${RSSContentItem.description}')`, // According to the RSS spec, all items must contain either a title or a description.
            }, this.contextUser)
                const contentItemResult = <MJContentItemEntity> results.Results[0];
                // This content item already exists, check the last hash to see if it has been modified
                const lastStoredHash: string = contentItemResult.Checksum
                const newHash: string = await this.getChecksumFromRSSItem(RSSContentItem, this.contextUser)
                if (lastStoredHash !== newHash) {
                    // This content item has been modified
                    contentItem.Load(contentItemResult.ID);
                    contentItem.Checksum = newHash
                    contentItem.Text = JSON.stringify(RSSContentItem)
                    await contentItem.Save();
                    contentItemsToProcess.push(contentItem); // Content item was modified, add to list
                // This content item does not exist, add it
                contentItem.Description = RSSContentItem.description || await this.engine.getContentItemDescription(contentSourceParams, this.contextUser)
                contentItem.Checksum = await this.getChecksumFromRSSItem(RSSContentItem, this.contextUser)
                contentItem.URL = RSSContentItem.link || contentSourceParams.URL
                contentItemsToProcess.push(contentItem); // Content item was added, add to list
    public async parseRSSFeed(url: string): Promise<RSSItem[]> {
            if(await this.urlIsValid(url)) {
                const RSSItems: RSSItem[] = []
                const parser = new Parser();
                const feed = await parser.parseURL(url);
                const items = feed.items;
                // Map each item to an RSSItem object and add it to the RSSItems array
                items.forEach(async (item: any) => {
                    const rssItem = new RSSItem();
                    rssItem.title = item.title ?? '';
                    rssItem.link = item.link ?? '';
                    rssItem.description = item.description ?? '';
                    rssItem.pubDate = item.pubDate ?? '';
                    rssItem.guid = item.guid ?? '';
                    rssItem.category = item.category ?? '';
                    const content = item['content:encoded'] ?? item['content'] ?? '';
                    rssItem.content = await this.engine.parseHTML(content);
                    rssItem.author = item.author ?? '';
                    rssItem.comments = item.comments ?? '';
                    rssItem.source = item.source ?? '';
                    RSSItems.push(rssItem);
                return RSSItems
                throw new Error(`Invalid URL: ${url}`);
            console.error('Error fetching RSS feed:', error);
    protected async urlIsValid(url: string): Promise<boolean> {
            const response = await axios.head(url);
            return response.status === 200;
            console.error(`Invalid URL: ${url}`);
    public async getChecksumFromRSSItem(RSSContentItem: RSSItem, contextUser: UserInfo): Promise<string> {
        const hash = crypto.createHash('sha256').update(JSON.stringify(RSSContentItem)).digest('hex')
