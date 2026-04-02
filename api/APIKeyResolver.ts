import { Resolver, Mutation, Arg, Ctx } from "type-graphql";
import { MJAPIKeyScopeEntity } from "@memberjunction/core-entities";
import { AppContext } from "../types.js";
import { ResolverBase } from "../generic/ResolverBase.js";
 * Input type for creating a new API key
export class CreateAPIKeyInput {
     * Human-readable label for the API key
     * Optional description of what the key is used for
     * Optional expiration date for the key
    @Field(() => Date, { nullable: true })
     * Optional array of scope IDs to assign to this key
    @Field(() => [String], { nullable: true })
 * Result type for API key creation
 * Returns the raw key ONCE - it cannot be recovered after this
export class CreateAPIKeyResult {
     * Whether the key was created successfully
     * The raw API key - show this to the user ONCE
     * This cannot be recovered after the initial response
     * The database ID of the created API key
     * Error message if creation failed
 * Result type for API key revocation
export class RevokeAPIKeyResult {
     * Whether the key was revoked successfully
     * Error message if revocation failed
 * Resolver for API key operations
 * Handles secure server-side API key generation
export class APIKeyResolver extends ResolverBase {
     * Creates a new API key with proper server-side cryptographic hashing.
     * This mutation:
     * 1. Generates a cryptographically secure API key using APIKeyEngine
     * 2. Stores only the SHA-256 hash in the database (never the raw key)
     * 3. Returns the raw key ONCE - it cannot be recovered after this call
     * 4. Optionally assigns scope permissions to the key
     * @param input The creation parameters
     * @param ctx The GraphQL context with authenticated user
     * @returns The raw key (show once!) and database ID
    @Mutation(() => CreateAPIKeyResult)
    async CreateAPIKey(
        @Arg("input") input: CreateAPIKeyInput,
        @Ctx() ctx: AppContext
        // Check API key scope authorization for API key creation
        await this.CheckAPIKeyScopeAuthorization('apikey:create', '*', ctx.userPayload);
            // Get the authenticated user
            const user = ctx.userPayload.userRecord;
                    Error: "User is not authenticated"
            // Use APIKeyEngine to create the API key with proper server-side crypto
            const result = await apiKeyEngine.CreateAPIKey(
                    UserId: user.ID,
                    Label: input.Label,
                    Description: input.Description,
                    ExpiresAt: input.ExpiresAt
                    Error: result.Error || "Failed to create API key"
            // Save scope associations if provided
            if (input.ScopeIDs && input.ScopeIDs.length > 0 && result.APIKeyId) {
                await this.saveScopeAssociations(result.APIKeyId, input.ScopeIDs, user);
                RawKey: result.RawKey,
                APIKeyID: result.APIKeyId
            LogError(`Error in CreateAPIKey resolver: ${error.message}`);
                Error: `Error creating API key: ${error.message}`
     * This uses APIKeyEngine.RevokeAPIKey() for consistency.
     * @returns Success status
    @Mutation(() => RevokeAPIKeyResult)
    async RevokeAPIKey(
        @Arg("apiKeyId") apiKeyId: string,
    ): Promise<RevokeAPIKeyResult> {
        // Check API key scope authorization for API key revocation
        await this.CheckAPIKeyScopeAuthorization('apikey:revoke', apiKeyId, ctx.userPayload);
            const result = await apiKeyEngine.RevokeAPIKey(apiKeyId, user);
                    Error: "Failed to revoke API key. It may not exist or you may not have permission."
            LogError(`Error in RevokeAPIKey resolver: ${error.message}`);
                Error: `Error revoking API key: ${error.message}`
     * Saves scope associations for the newly created API key
     * @param apiKeyId The ID of the created API key
     * @param scopeIds Array of scope IDs to associate
     * @param user The context user
    private async saveScopeAssociations(
        scopeIds: string[],
        for (const scopeId of scopeIds) {
                const keyScope = await md.GetEntityObject<MJAPIKeyScopeEntity>(
                    'MJ: API Key Scopes',
                keyScope.APIKeyID = apiKeyId;
                keyScope.ScopeID = scopeId;
                LogError(`Error saving scope association for API key ${apiKeyId}, scope ${scopeId}: ${error}`);
                // Continue with other scopes even if one fails
