/* eslint-disable @typescript-eslint/no-explicit-any */
import { EntitySaveOptions, Metadata } from '@memberjunction/core';
import { AppContext, Arg, Ctx, Int, Query, Resolver, UserPayload } from '@memberjunction/server';
import { MJUserView_, MJUserViewResolverBase } from '../generated/generated.js';
import { UserResolver } from './UserResolver.js';
import { MJUserViewEntity, UserViewEntityExtended } from '@memberjunction/core-entities';
export class UserViewResolver extends MJUserViewResolverBase {
  async UserViewsByUserID(@Arg('UserID', () => Int) UserID: number, @Ctx() { providers, userPayload }: AppContext) {
    return await this.findBy(provider, 'MJ: User Views', { UserID }, userPayload.userRecord);
  async DefaultViewByUserAndEntity(
    @Arg('UserID', () => Int) UserID: number,
    @Arg('EntityID', () => Int) EntityID: number,
    return await this.findBy(provider, 'MJ: User Views', { UserID, EntityID, IsDefault: true }, userPayload.userRecord);
  async CurrentUserDefaultViewByEntityID(@Arg('EntityID', () => Int) EntityID: number, @Ctx() context: AppContext) {
    const provider = GetReadOnlyProvider(context.providers, {allowFallbackToReadWrite: true})    
    return await this.findBy(provider, 'MJ: User Views', {
      UserID: await this.getCurrentUserID(context),
      IsDefault: true,
  protected async getCurrentUserID(context: AppContext): Promise<number> {
    const userResolver = new UserResolver();
    const user = await userResolver.UserByEmail(context.userPayload.email, context);
    return user.ID;
  async CurrentUserUserViewsByEntityID(@Arg('EntityID', () => Int) EntityID: number, @Ctx() context: AppContext) {
    return this.findBy(provider, 'MJ: User Views', { UserID: await this.getCurrentUserID(context), EntityID}, context.userPayload.userRecord);
  async UpdateWhereClause(@Arg('ID', () => String) ID: string, @Ctx() { userPayload, providers }: AppContext) {
    // in this query we want to update the uesrView record in the DB with a new where clause
    // this should normally not be a factor but we have this exposed in the GraphQL API so that
    // a dev can force the update if desired from the client. The normal path is just to update
    // filter state which in turn will be used to update the where clause in the entity sub-class.
    const u = this.GetUserFromPayload(userPayload);
    const viewEntity = <UserViewEntityExtended>await p.GetEntityObject('MJ: User Views', u);
    await viewEntity.Load(ID);
    viewEntity.UpdateWhereClause();
    if (await viewEntity.Save()) {
      return viewEntity.GetAll();
      throw new Error('Failed to update where clause');
export default UserViewResolver;
