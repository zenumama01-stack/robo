import { EntityPermissionType, Metadata, FieldValueCollection, EntitySaveOptions, RunView } from '@memberjunction/core';
import { MJFileEntity, MJFileStorageProviderEntity, MJFileStorageAccountEntity } from '@memberjunction/core-entities';
  AppContext,
  Arg,
  Ctx,
  DeleteOptionsInput,
  Field,
  FieldResolver,
  InputType,
  Int,
  Mutation,
  ObjectType,
  PubSub,
  PubSubEngine,
  Query,
  Resolver,
} from '@memberjunction/server';
  createDownloadUrl,
  createUploadUrl,
  deleteObject,
  moveObject,
  copyObject,
  listObjects,
  copyObjectBetweenProviders,
  searchAcrossAccounts,
  AccountSearchResult,
  AccountSearchInput,
  UserContextOptions,
  ExtendedUserContextOptions,
  initializeDriverWithAccountCredentials,
} from '@memberjunction/storage';
import { CreateMJFileInput, MJFileResolver as FileResolverBase, MJFile_, UpdateMJFileInput } from '../generated/generated.js';
export class CreateUploadURLInput {
export class CreateFilePayload {
  @Field(() => MJFile_)
  File: MJFile_;
  UploadUrl: string;
  NameExists: boolean;
export class FileExt extends MJFile_ {
  DownloadUrl: string;
export class StorageObjectMetadata {
export class StorageListResult {
  @Field(() => [StorageObjectMetadata])
export class ListStorageObjectsInput {
  AccountID: string;
  Prefix: string;
  Delimiter?: string;
export class CreatePreAuthDownloadUrlInput {
  ObjectName: string;
export class CreatePreAuthUploadUrlInput {
export class CreatePreAuthUploadUrlPayload {
export class DeleteStorageObjectInput {
export class MoveStorageObjectInput {
export class CopyStorageObjectInput {
  SourceName: string;
  DestinationName: string;
export class CreateDirectoryInput {
export class CopyObjectBetweenAccountsInput {
  SourceAccountID: string;
  DestinationAccountID: string;
  SourcePath: string;
  DestinationPath: string;
export class CopyObjectBetweenAccountsPayload {
export class SearchAcrossAccountsInput {
  AccountIDs: string[];
  MaxResultsPerAccount?: number;
  FileTypes?: string[];
  SearchContent?: boolean;
export class FileSearchResultPayload {
export class AccountSearchResultPayload {
  @Field(() => [FileSearchResultPayload])
  results: FileSearchResultPayload[];
export class SearchAcrossAccountsPayload {
  @Field(() => [AccountSearchResultPayload])
  accountResults: AccountSearchResultPayload[];
export class FileResolver extends FileResolverBase {
   * Builds UserContextOptions for storage operations that may require OAuth authentication.
   * This passes the current user's ID and context to allow the storage utilities to
   * load user-specific OAuth tokens for providers like Google Drive.
  private buildUserContext(context: AppContext): UserContextOptions {
    const user = this.GetUserFromPayload(context.userPayload);
      userID: user.ID,
      contextUser: user,
   * Builds ExtendedUserContextOptions that includes the account entity for enterprise model.
   * This is required for OAuth providers using the Credential Engine to decrypt credentials.
  private buildExtendedUserContext(context: AppContext, accountEntity: MJFileStorageAccountEntity): ExtendedUserContextOptions {
   * Loads a FileStorageAccount and its associated FileStorageProvider.
   * This is the standard way to get provider information in the enterprise model.
   * @param context The AppContext containing provider info
   * @returns Object containing both the account and provider entities
  private async loadAccountAndProvider(
    context: AppContext,
  ): Promise<{ account: MJFileStorageAccountEntity; provider: MJFileStorageProviderEntity }> {
    const md = GetReadOnlyProvider(context.providers, { allowFallbackToReadWrite: true });
    // Load the account entity
    const account = await md.GetEntityObject<MJFileStorageAccountEntity>('MJ: File Storage Accounts', user);
    const loaded = await account.Load(accountId);
      throw new Error(`Storage account with ID ${accountId} not found`);
    // Load the provider entity from the account's ProviderID
    const provider = await md.GetEntityObject<MJFileStorageProviderEntity>('MJ: File Storage Providers', user);
    await provider.Load(account.ProviderID);
  @Mutation(() => CreateFilePayload)
  async CreateFile(@Arg('input', () => CreateMJFileInput) input: CreateMJFileInput, @Ctx() context: AppContext, @PubSub() pubSub: PubSubEngine) {
    // Check to see if there's already an object with that name
    const provider = GetReadOnlyProvider(context.providers, { allowFallbackToReadWrite: true });
    const fileEntity = await provider.GetEntityObject<MJFileEntity>('MJ: Files', user);
    const providerEntity = await provider.GetEntityObject<MJFileStorageProviderEntity>('MJ: File Storage Providers', user);
    fileEntity.CheckPermissions(EntityPermissionType.Create, true);
    const [sameName] = await this.findBy(provider, 'MJ: Files', { Name: input.Name, ProviderID: input.ProviderID }, context.userPayload.userRecord);
    const NameExists = Boolean(sameName);
    const success = fileEntity.NewRecord(FieldValueCollection.FromObject({ ...input, Status: 'Pending' }));
    // If there's a problem creating the file record, the base resolver will return null
    // Create the upload URL and get the record updates (provider key, content type, etc)
    const userContext = this.buildUserContext(context);
    const { updatedInput, UploadUrl } = await createUploadUrl(providerEntity, fileEntity, userContext);
    // Save the file record with the updated input
    fileEntity.SetMany(mapper.ReverseMapFields({ ...updatedInput }), true, true);
    const File = mapper.MapFields({ ...fileEntity.GetAll() });
    return { File, UploadUrl, NameExists };
  @FieldResolver(() => String)
  async DownloadUrl(@Root() file: MJFile_, @Ctx() context: AppContext) {
    const fileEntity = await md.GetEntityObject<MJFileEntity>('MJ: Files', user);
    fileEntity.CheckPermissions(EntityPermissionType.Read, true);
    const providerEntity = await md.GetEntityObject<MJFileStorageProviderEntity>('MJ: File Storage Providers', user);
    await providerEntity.Load(file.ProviderID);
    const url = await createDownloadUrl(providerEntity, file.ProviderKey ?? file.Name, userContext);
  async UpdateFile(@Arg('input', () => UpdateMJFileInput) input: UpdateMJFileInput, @Ctx() context: AppContext, @PubSub() pubSub: PubSubEngine) {
    // if the name is changing, rename the target object as well
    await fileEntity.Load(input.ID);
    if (fileEntity.Name !== input.Name) {
      await providerEntity.Load(fileEntity.ProviderID);
      const success = await moveObject(providerEntity, fileEntity.Name, input.Name, userContext);
        throw new Error('Error updating object name');
    const updatedFile = await super.UpdateMJFile(input, context, pubSub);
    return updatedFile;
  async DeleteFile(
    @PubSub() pubSub: PubSubEngine,
    const userInfo = this.GetUserFromPayload(context.userPayload);
    const fileEntity = await md.GetEntityObject<MJFileEntity>('MJ: Files', userInfo);
    await fileEntity.Load(ID);
    if (!fileEntity) {
    fileEntity.CheckPermissions(EntityPermissionType.Delete, true);
    // Only delete the object from the provider if it's actually been uploaded
    if (fileEntity.Status === 'Uploaded') {
      const providerEntity = await md.GetEntityObject<MJFileStorageProviderEntity>('MJ: File Storage Providers', userInfo);
      await deleteObject(providerEntity, fileEntity.ProviderKey ?? fileEntity.Name, userContext);
    return super.DeleteMJFile(ID, options, context, pubSub);
  @Query(() => StorageListResult)
  async ListStorageObjects(@Arg('input', () => ListStorageObjectsInput) input: ListStorageObjectsInput, @Ctx() context: AppContext) {
    console.log('[FileResolver] ListStorageObjects called with:', {
      AccountID: input.AccountID,
      Prefix: input.Prefix,
      Delimiter: input.Delimiter,
    // Load the account and its provider
    const { account, provider: providerEntity } = await this.loadAccountAndProvider(input.AccountID, context);
    console.log('[FileResolver] Provider loaded:', {
      Name: providerEntity.Name,
      ServerDriverKey: providerEntity.ServerDriverKey,
      HasConfiguration: !!providerEntity.Get('Configuration'),
    // Check permissions - user must have read access to Files entity
    // Call the storage provider to list objects with extended user context (includes account for credential lookup)
    const userContext = this.buildExtendedUserContext(context, account);
    const result = await listObjects(providerEntity, input.Prefix, input.Delimiter || '/', userContext);
    console.log('[FileResolver] listObjects result:', {
      objectsCount: result.objects.length,
      prefixesCount: result.prefixes.length,
      objects: result.objects.map((o) => ({ name: o.name, isDirectory: o.isDirectory })),
      prefixes: result.prefixes,
    // Convert Date objects to ISO strings for GraphQL
    const objects = result.objects.map((obj) => ({
      lastModified: obj.lastModified.toISOString(),
      objects,
  @Query(() => String)
  async CreatePreAuthDownloadUrl(@Arg('input', () => CreatePreAuthDownloadUrlInput) input: CreatePreAuthDownloadUrlInput, @Ctx() context: AppContext) {
    // Create download URL with extended user context (includes account for credential lookup)
    const downloadUrl = await createDownloadUrl(providerEntity, input.ObjectName, userContext);
    return downloadUrl;
  @Mutation(() => CreatePreAuthUploadUrlPayload)
  async CreatePreAuthUploadUrl(@Arg('input', () => CreatePreAuthUploadUrlInput) input: CreatePreAuthUploadUrlInput, @Ctx() context: AppContext) {
    // Create upload URL with extended user context (includes account for credential lookup)
    const { UploadUrl, updatedInput } = await createUploadUrl(
        ID: '', // Not needed for direct upload
        Name: input.ObjectName,
        ProviderID: providerEntity.ID,
        ContentType: input.ContentType,
      userContext,
    // Extract ProviderKey if it exists (spread into updatedInput by createUploadUrl)
    const providerKey = (updatedInput as { ProviderKey?: string }).ProviderKey;
      UploadUrl,
      ProviderKey: providerKey,
  @Mutation(() => Boolean)
  async DeleteStorageObject(@Arg('input', () => DeleteStorageObjectInput) input: DeleteStorageObjectInput, @Ctx() context: AppContext) {
    console.log('[FileResolver] DeleteStorageObject called:', input);
      providerID: providerEntity.ID,
      providerName: providerEntity.Name,
      serverDriverKey: providerEntity.ServerDriverKey,
    console.log('[FileResolver] Permissions checked, calling deleteObject...');
    // Delete the object with extended user context (includes account for credential lookup)
    const success = await deleteObject(providerEntity, input.ObjectName, userContext);
    console.log('[FileResolver] deleteObject returned:', success);
  async MoveStorageObject(@Arg('input', () => MoveStorageObjectInput) input: MoveStorageObjectInput, @Ctx() context: AppContext) {
    // Move the object with extended user context (includes account for credential lookup)
    const success = await moveObject(providerEntity, input.OldName, input.NewName, userContext);
  async CopyStorageObject(@Arg('input', () => CopyStorageObjectInput) input: CopyStorageObjectInput, @Ctx() context: AppContext) {
    // Check permissions - copying requires both read (source) and create (destination)
    // Copy the object with extended user context (includes account for credential lookup)
    const success = await copyObject(providerEntity, input.SourceName, input.DestinationName, userContext);
  async CreateDirectory(@Arg('input', () => CreateDirectoryInput) input: CreateDirectoryInput, @Ctx() context: AppContext) {
    const { account: accountEntity, provider: providerEntity } = await this.loadAccountAndProvider(input.AccountID, context);
    // Initialize driver with account-based credentials from Credential Engine
    const driver = await initializeDriverWithAccountCredentials({
    const success = await driver.CreateDirectory(input.Path);
  @Mutation(() => CopyObjectBetweenAccountsPayload)
  async CopyObjectBetweenAccounts(
    @Arg('input', () => CopyObjectBetweenAccountsInput) input: CopyObjectBetweenAccountsInput,
  ): Promise<CopyObjectBetweenAccountsPayload> {
    console.log('[FileResolver] CopyObjectBetweenAccounts called:', {
      sourceAccountID: input.SourceAccountID,
      destinationAccountID: input.DestinationAccountID,
      sourcePath: input.SourcePath,
      destinationPath: input.DestinationPath,
    // Load the source account and its provider
    const { account: sourceAccount, provider: sourceProviderEntity } = await this.loadAccountAndProvider(input.SourceAccountID, context);
    // Load the destination account and its provider
    const { account: destAccount, provider: destProviderEntity } = await this.loadAccountAndProvider(input.DestinationAccountID, context);
    // Perform the cross-provider copy with extended user context (includes account for credential lookup)
    const sourceUserContext = this.buildExtendedUserContext(context, sourceAccount);
    const destUserContext = this.buildExtendedUserContext(context, destAccount);
    const result = await copyObjectBetweenProviders(sourceProviderEntity, destProviderEntity, input.SourcePath, input.DestinationPath, {
      sourceUserContext,
      destinationUserContext: destUserContext,
    console.log('[FileResolver] CopyObjectBetweenAccounts result:', result);
      message: result.message,
      bytesTransferred: result.bytesTransferred,
      sourceAccount: sourceAccount.Name,
      destinationAccount: destAccount.Name,
      sourcePath: result.sourcePath,
      destinationPath: result.destinationPath,
  @Query(() => SearchAcrossAccountsPayload)
  async SearchAcrossAccounts(
    @Arg('input', () => SearchAcrossAccountsInput) input: SearchAcrossAccountsInput,
  ): Promise<SearchAcrossAccountsPayload> {
    console.log('[FileResolver] SearchAcrossAccounts called:', {
      accountIDs: input.AccountIDs,
      query: input.Query,
      maxResultsPerAccount: input.MaxResultsPerAccount,
      fileTypes: input.FileTypes,
      searchContent: input.SearchContent,
    // Check permissions - searching requires read access
    // Load all requested account entities in a single query
    const quotedIDs = input.AccountIDs.map((id) => `'${id}'`).join(', ');
    const accountResult = await rv.RunView<MJFileStorageAccountEntity>(
        ExtraFilter: `ID IN (${quotedIDs})`,
    if (!accountResult.Success) {
      throw new Error(`Failed to load storage accounts: ${accountResult.ErrorMessage}`);
    const accountEntities = accountResult.Results;
    if (accountEntities.length === 0) {
      throw new Error('No valid storage accounts found for the provided IDs');
    // Log any accounts that weren't found
    if (accountEntities.length < input.AccountIDs.length) {
      const foundIDs = new Set(accountEntities.map((a) => a.ID));
      const missingIDs = input.AccountIDs.filter((id) => !foundIDs.has(id));
      console.warn(`[FileResolver] Accounts not found: ${missingIDs.join(', ')}`);
    // Load providers for all accounts
    const providerIDs = [...new Set(accountEntities.map((a) => a.ProviderID))];
    const quotedProviderIDs = providerIDs.map((id) => `'${id}'`).join(', ');
    const providerResult = await rv.RunView<MJFileStorageProviderEntity>(
        ExtraFilter: `ID IN (${quotedProviderIDs})`,
    if (!providerResult.Success) {
      throw new Error(`Failed to load storage providers: ${providerResult.ErrorMessage}`);
    for (const provider of providerResult.Results) {
      providerMap.set(provider.ID, provider);
    // Build account/provider pairs for the search
    const accountInputs: AccountSearchInput[] = [];
    for (const account of accountEntities) {
        accountInputs.push({ accountEntity: account, providerEntity: provider });
    // Execute the search across all accounts with account-based credentials
    const result = await searchAcrossAccounts(accountInputs, input.Query, {
    console.log('[FileResolver] SearchAcrossAccounts result:', {
      totalResultsReturned: result.totalResultsReturned,
      successfulAccounts: result.successfulAccounts,
      failedAccounts: result.failedAccounts,
    // Convert results to GraphQL payload format
    const accountResults: AccountSearchResultPayload[] = result.accountResults.map((ar: AccountSearchResult) => ({
      accountID: ar.accountID,
      results: ar.results.map((r: FileSearchResult) => ({
        objectId: r.objectId,
      nextPageToken: ar.nextPageToken,
      accountResults,
