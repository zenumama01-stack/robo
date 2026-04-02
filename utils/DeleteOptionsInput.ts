import { Field, InputType } from "type-graphql";
 * GraphQL InputType for entity delete operations.
 * SYNC REQUIREMENTS - Changes here require updates to:
 * 1. @memberjunction/core - EntityDeleteOptions class in interfaces.ts
 * 2. @memberjunction/graphql-dataprovider - Delete method default options in graphQLDataProvider.ts
 * 3. @memberjunction/graphql-dataprovider - DeleteQueryOptionsInput interface in graphQLSystemUserClient.ts
 * TESTING: Schema sync tests in GraphQLDataProvider will fail if these files drift out of sync.
 * Run: cd packages/GraphQLDataProvider && npm run test
 * @see packages/MJCore/src/generic/interfaces.ts - EntityDeleteOptions class
 * @see packages/GraphQLDataProvider/src/__tests__/schema-sync.test.ts - Automated sync verification
export class DeleteOptionsInput {
    SkipEntityAIActions: boolean;
    SkipEntityActions: boolean;
     * When set to true, the delete operation will BYPASS Validate() and the actual
     * process of deleting the record from the database but WILL invoke any associated
     * actions (AI Actions, Entity Actions, etc...).
    ReplayOnly: boolean;
    IsParentEntityDelete: boolean;
