 * Action that retrieves a list of configured file storage accounts.
 * This action returns storage accounts that are configured in the enterprise model,
 * along with their associated provider information. Storage accounts link to
 * providers (Google Drive, Dropbox, etc.) and credentials managed at the org level.
 * This is useful for AI agents to discover what storage accounts are available
 * before attempting to search or access files.
 * // Get all available storage accounts
 *   ActionName: 'List Storage Accounts',
 *   Params: []
 * // Get only accounts that support search
 *     Name: 'SearchSupportedOnly',
@RegisterClass(BaseAction, "List Storage Accounts")
export class ListStorageAccountsAction extends BaseFileStorageAction {
        // Optional parameter to filter only accounts whose provider supports search
        const searchSupportedOnly = this.getBooleanParam(params, "searchsupportedonly", false);
            // Load accounts and providers in parallel
            const [accountsResult, providersResult] = await rv.RunViews([
                    ExtraFilter: 'IsActive=1',
            ], params.ContextUser);
            if (!accountsResult.Success) {
                    `Failed to retrieve storage accounts: ${accountsResult.ErrorMessage}`,
                    "QUERY_FAILED"
            if (!providersResult.Success) {
                    `Failed to retrieve storage providers: ${providersResult.ErrorMessage}`,
            const accounts = accountsResult.Results as MJFileStorageAccountEntity[] || [];
            const providers = providersResult.Results as MJFileStorageProviderEntity[] || [];
            // Create provider lookup map
            const providerMap = new Map<string, MJFileStorageProviderEntity>();
            providers.forEach(p => providerMap.set(p.ID, p));
            const availableAccounts: Array<{
                ProviderName: string;
                ProviderType: string;
                SupportsSearch: boolean;
                HasCredential: boolean;
                const provider = providerMap.get(account.ProviderID);
                    continue; // Skip accounts with inactive/missing providers
                const supportsSearch = provider.Get('SupportsSearch') ?? false;
                // Skip if filtering for search-only and this provider doesn't support it
                if (searchSupportedOnly && !supportsSearch) {
                availableAccounts.push({
                    Name: account.Name,
                    Description: account.Description || '',
                    ProviderName: provider.Name,
                    ProviderType: provider.ServerDriverKey,
                    SupportsSearch: supportsSearch,
                    HasCredential: !!account.CredentialID
            // Calculate counts
            const searchSupportedCount = availableAccounts.filter(a => a.SupportsSearch).length;
            const totalCount = availableAccounts.length;
            // Create detailed result message
            let message = `Found ${totalCount} storage account(s)`;
            if (searchSupportedOnly) {
                message += ` with search support`;
                message += ` (${searchSupportedCount} support search)`;
            // Add account details to message for LLM visibility
            message += '\n\nAvailable Storage Accounts:';
            for (const account of availableAccounts) {
                message += `\n- ${account.Name} (${account.ProviderName})`;
                if (account.SupportsSearch) {
                    message += ' - Supports Search';
            // Build output parameters array
                    Name: 'Accounts',
                    Value: availableAccounts,
                    Value: totalCount,
                    Name: 'SearchSupportedCount',
                    Value: searchSupportedCount,
                `Failed to list storage accounts: ${errorMessage}`,
                "LIST_FAILED"
