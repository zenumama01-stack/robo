import { Metadata, KeyValuePair, CompositeKey, UserInfo } from '@memberjunction/core';
  CompositeKeyInputType,
  CompositeKeyOutputType,
import { MJUserFavorite_, MJUserFavoriteResolverBase } from '../generated/generated.js';
// INPUT TYPE for User Favorite Queries
export class UserFavoriteSearchParams {
  CompositeKey: CompositeKeyInputType;
export class UserFavoriteSetParams {
  IsFavorite: boolean;
export class UserFavoriteResult {
  EntityID: number;
export class UserFavoriteResolver extends MJUserFavoriteResolverBase {
  @Query(() => [MJUserFavorite_])
  async UserFavoritesByUserID(@Arg('UserID', () => Int) UserID: number, @Ctx() { providers, userPayload }: AppContext) {
    const provider = GetReadOnlyProvider(providers, {allowFallbackToReadWrite: true})    
    return await this.findBy(provider, 'MJ: User Favorites', { UserID }, userPayload.userRecord);
  async UserFavoriteSearchByParams(@Arg('params', () => Int) params: UserFavoriteSearchParams, @Ctx() { providers, userPayload }: AppContext) {
    return await this.findBy(provider, 'MJ: User Favorites', params, userPayload.userRecord);
  @Query(() => UserFavoriteResult)
  async GetRecordFavoriteStatus(@Arg('params', () => UserFavoriteSearchParams) params: UserFavoriteSearchParams, @Ctx() {providers, userPayload}: AppContext) {
    const p = GetReadOnlyProvider(providers, {allowFallbackToReadWrite: true});
    const pk = new CompositeKey(params.CompositeKey.KeyValuePairs);
    const e = p.Entities.find((e) => e.ID === params.EntityID);
        UserID: params.UserID,
        IsFavorite: await p.GetRecordFavoriteStatus(params.UserID, e.Name, pk, userPayload.userRecord),
    else throw new Error(`Entity ID:${params.EntityID} not found`);
  @Mutation(() => UserFavoriteResult)
  async SetRecordFavoriteStatus(@Arg('params', () => UserFavoriteSetParams) params: UserFavoriteSetParams, @Ctx() { userPayload, providers }: AppContext) {
    const u = UserCache.Users.find((u) => u.ID === userPayload.userRecord.ID);
      await p.SetRecordFavoriteStatus(params.UserID, e.Name, pk, params.IsFavorite, u);
        CompositeKey: params.CompositeKey,
        IsFavorite: params.IsFavorite,
    } else throw new Error(`Entity ID:${params.EntityID} not found`);
export default UserFavoriteResolver;
