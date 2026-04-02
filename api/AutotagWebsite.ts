import { AutotagBase } from '../../Core';
import { AutotagBaseEngine, ContentSourceParams } from '../../Engine';
import * as cheerio from 'cheerio';
import { URL } from 'url';
@RegisterClass(AutotagBase, 'AutotagWebsite')
export class AutotagWebsite extends AutotagBase {
    protected CrawlOtherSitesInTopLevelDomain: boolean;
    protected CrawlSitesInLowerLevelDomain: boolean;
    protected MaxDepth: number;
    protected RootURL: string;
    protected URLPattern: string;
    protected visitedURLs: Set<string>;
        this.visitedURLs = new Set<string>();
        this.contentSourceTypeID = await this.engine.setSubclassContentSourceType('Website', this.contextUser);
        const contentSources: MJContentSourceEntity[] = await this.engine.getAllContentSources(this.contextUser, this.contentSourceTypeID);
        const contentItemsToProcess: MJContentItemEntity[] = await this.SetContentItemsToProcess(contentSources);
     * Given a content source, retrieve all content items associated with the content sources. 
     * The content items are then processed to determine if they have been modified since the last time they were processed or if they are new content items.
                // All content items associated with the content source
                const startURL: string = contentSourceParams.URL;
                // root url should be set to this.RootURL if it exists, otherwise it should be set to the base path of the startURL. 
                const rootURL: string = this.RootURL ? this.RootURL : this.getBasePath(startURL);
                // regex should be set to this.URLPattern if it exists, otherwise it should be set to match any URL.
                const regex: RegExp = this.URLPattern && new RegExp(this.URLPattern) || new RegExp('.*');
                const allContentItemLinks: string[] = await this.getAllLinksFromContentSource(startURL, rootURL, regex);
                const contentItems: MJContentItemEntity[] = await this.SetNewAndModifiedContentItems(allContentItemLinks, contentSourceParams, this.contextUser);
                console.error(`Failed to process content source: ${contentSource.Get('Name')}`);
     * Given a list of content item links, check if the content item already exists in the database. 
     * If the content item exists, check if the content item has been modified since the last time it was processed.
     * If the content item does not exist, create a new content item and add it to the list of content items to process.
     * @param contentItemLinks 
     * @param contentSourceParams 
    protected async SetNewAndModifiedContentItems(contentItemLinks: string[], contentSourceParams: ContentSourceParams, contextUser: UserInfo): Promise<MJContentItemEntity[]> { 
        const addedContentItems: MJContentItemEntity[] = [];
        for (const contentItemLink of contentItemLinks) {
                const newHash = await this.engine.getChecksumFromURL(contentItemLink);
                const results = await rv.RunViews<MJContentItemEntity>([
                        ExtraFilter: `Checksum = '${newHash}'`,
                        ExtraFilter: `ContentSourceID = '${contentSourceParams.contentSourceID}' AND URL = '${contentItemLink}'`,
                ], this.contextUser)
                const contentItemResultsWithChecksum = results[0]
                const contentItemResultsWithURL = results[1]
                if (contentItemResultsWithChecksum.Success && contentItemResultsWithChecksum.Results.length) {
                    // We found the checksum so this content item has not changed since we last accessed it, do nothing
                else if (contentItemResultsWithURL.Success && contentItemResultsWithURL.Results.length) {
                    // This content item already exists, update the hash and last updated date
                    const contentItemResult: MJContentItemEntity = contentItemResultsWithURL.Results[0]; 
                        // This content item has changed since we last access it, update the hash and last updated date
                        contentItem.Text = await this.parseWebPage(contentItemLink)
                        addedContentItems.push(contentItem); // Content item was modified, add to list
                    contentItem.Name = this.getPathName(contentItemLink) // Will get overwritten by title later if it exists
                    contentItem.Checksum = await this.engine.getChecksumFromURL(contentItemLink)
                    contentItem.URL = contentItemLink
                    addedContentItems.push(contentItem); // Content item was added, add to list
                }catch (e) {
                    console.log(e)
        return addedContentItems;
    public async fetchPageContent(url: string): Promise<string> {
        const { data } = await axios.get(url);
    public getTextWithLineBreaks(element: any, $: cheerio.CheerioAPI): string {
        const children = $(element).contents();
            const el = children[i];
            if (el.type === 'text') {
                text += $(el).text().trim() + ' ';
            } else if (el.type === 'tag') {
                text += '\n' + this.getTextWithLineBreaks(el, $) + '\n';
     * Given a URL, this function extracts text from a webpage. 
     * @param url 
     * @returns The text extracted from the webpage
    public async parseWebPage(url: string): Promise<string> {
            const pageContent: string = await this.fetchPageContent(url);
            const $ = cheerio.load(pageContent);
            const text: string = this.getTextWithLineBreaks($('body')[0], $);
            console.error(`Error processing ${url}:`, error);
     * Given a root URL that corresponds to a content source, retrieve all the links in accordance to the crawl settings. 
     * If the crawl settings are set to crawl other sites in the top level domain, then all links in the top level domain will be retrieved.
     * If the crawl settings are set to crawl sites in lower level domains, then function is recursively called to retrieve all links in the lower level domains.
    protected async getAllLinksFromContentSource(url: string, rootURL: string, regex: RegExp): Promise<string[]> {
            await this.getLowerLevelLinks(url, rootURL, this.MaxDepth, new Set<string>(), regex);
            await this.getTopLevelLinks(url, this.getBasePath(url));
            return Array.from(this.visitedURLs);
            console.error(`Failed to get links from ${url}`);
     * For a given URL, retrieves all other links at that top level domain.
     * @param rootURL 
     * @param visitedURLs 
    protected async getTopLevelLinks(url: string, rootURL: string): Promise<void> {
        if (!this.CrawlOtherSitesInTopLevelDomain) {
            this.visitedURLs.add(url);
        // If we have already visited this URL, return an empty array
        if (this.visitedURLs.has(url) || !await this.urlIsValid(url) || this.isHighestDomain(url)) {
            const $ = cheerio.load(data);
            // Get all links on the page for the current URL
            $('a').each((_, element) => {
                const link = $(element).attr('href');
                    const newURL = new URL(link, url).href;
                    if (newURL.startsWith(rootURL) && !this.visitedURLs.has(newURL)) {
                        this.visitedURLs.add(newURL);
            await this.delay(1000); // Delay to prevent rate limiting
     * Simple check to see if the URL is at the highest level domain.
    protected isHighestDomain(url: string): boolean {
            const parsedURL: URL = new URL(url);
            return parsedURL.pathname === '/' || parsedURL.pathname === '';
            console.error(`Invalid URL for same level parsing: ${url}`);
    protected getBasePath(url: string): string {
        const pathSegments: string[] = parsedURL.pathname.split('/').filter(segment => segment);
        if (pathSegments.length > 0) {
            pathSegments.pop(); //Remove last segment so that we are in the same level domain
        const basePath = parsedURL.origin + '/' + pathSegments.join('/');
    // Creates a URL from input string and returns the path name in the form abc.com/xyz
    protected getPathName(url: string): string {
            const path = parsedURL.origin + '/' + pathSegments.join('/');
            return path
     * For a given URL, retrieves all links at lower level domains up to the specified crawl depth.
     * @param crawlDepth 
    protected async getLowerLevelLinks(url: string, rootURL: string, crawlDepth: number, scrapedURLs: Set<string>, regex: RegExp): Promise<Set<string>> {
            console.log(`Scraping ${url}`);
            if (scrapedURLs.has(url) || await this.urlIsValid(url) === false || crawlDepth < 0 || !this.CrawlSitesInLowerLevelDomain) {
                return new Set<string>();
            let combinedLinks = new Set<string>(); // Combined links from the current URL and all lower level URLs
            const extractedLinks = new Set<string>(); // Links extracted from the input URL
                    if (newURL.startsWith(rootURL) && newURL !== url && !this.visitedURLs.has(newURL) && regex.test(newURL)) {
                        extractedLinks.add(newURL);
            scrapedURLs.add(url);
            // If we are at the depth limit, return the current set of URLs and don't recurse
            if (crawlDepth === 0) {
                return extractedLinks;
            for (const subLink of extractedLinks) {
                //console.log(`Adding ${subLink}`);
                const lowerLevelLinks = await this.getLowerLevelLinks(subLink, rootURL, crawlDepth-1, scrapedURLs, regex);
                combinedLinks = new Set<string>([...extractedLinks, ...lowerLevelLinks]);
            return combinedLinks;
    protected async delay(ms: number) {
        return new Promise( resolve => setTimeout(resolve, ms) );
