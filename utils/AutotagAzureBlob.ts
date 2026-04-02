import { CloudStorageBase } from "../generic/CloudStorageBase";
import { BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { MJContentItemEntity } from "@memberjunction/core-entities";
import path from "path";
export class AutotagAzureBlob extends CloudStorageBase {
    private blobServiceClient: BlobServiceClient;
    private containerClient: ContainerClient;
    private connectionString: string;
    private containerName: string;
    constructor(connectionString: string, containerName: string) { 
        this.connectionString = connectionString
        this.containerName = containerName
     * Implemented abstract method from the CloudStorageBase class for cloud storage authentication. This method authenticates the user to the Azure Blob Storage.
    public async Authenticate(): Promise<void> {
            this.blobServiceClient = BlobServiceClient.fromConnectionString(this.connectionString);
            for await (const container of this.blobServiceClient.listContainers()) {
                console.log(`Container: ${container.name}`);
            this.containerClient = this.blobServiceClient.getContainerClient(this.containerName);
            console.error(error)
            throw new Error('Error authenticating to Azure Blob Storage')
    public async SetNewAndModifiedContentItems(contentSourceParams: ContentSourceParams, lastRunDate: Date, contextUser: UserInfo, prefix=''): Promise<MJContentItemEntity[]> {
        for await (const blob of this.containerClient.listBlobsFlat()) {
            const filePath = path.join(this.containerName, blob.name)
            if (blob.properties.createdOn && blob.properties.createdOn > lastRunDate) {
                // The file has been created, add a new record for this file
                const contentItem = await md.GetEntityObject<MJContentItemEntity>('MJ: Content Items', contextUser)
                const text = await this.extractText(blob.name)
                contentItem.ContentSourceID = contentSourceParams.contentSourceID
                contentItem.Name = blob.name
                contentItem.Description = await this.engine.getContentItemDescription(contentSourceParams, contextUser)
                contentItem.URL = filePath
                contentItem.ContentTypeID = contentSourceParams.ContentTypeID
                contentItem.ContentSourceTypeID =  contentSourceParams.ContentSourceTypeID
                contentItem.ContentFileTypeID = contentSourceParams.ContentFileTypeID
                contentItem.Checksum = await this.engine.getChecksumFromText(text)
                contentItem.Text = text
                await contentItem.Save()
                contentItemsToProcess.push(contentItem)
            else if (blob.properties.lastModified && blob.properties.lastModified > lastRunDate) {
                // The file has been modified, update the record for this file
                const contentItemID = await this.engine.getContentItemIDFromURL(contentSourceParams, contextUser)
                await contentItem.Load(contentItemID)
                contentItem.Save()
        return contentItemsToProcess
    public async extractText(file: string): Promise<string> {
        const blockBlobClient = this.containerClient.getBlockBlobClient(file)
        const downloadBlockBlobResponse = await blockBlobClient.download()
        const document: Buffer = await this.streamToBuffer(downloadBlockBlobResponse.readableStreamBody)
        const text: string = await this.engine.parsePDF(document)
    public async streamToBuffer(readableStream: NodeJS.ReadableStream): Promise<Buffer> {
            readableStream.on("data", (data) => {
                chunks.push(data instanceof Buffer ? data : Buffer.from(data));
            readableStream.on("end", () => {
                resolve(Buffer.concat(chunks as unknown as Uint8Array[]));
            readableStream.on("error", reject);
