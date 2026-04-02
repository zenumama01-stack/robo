import { Ctx, Field, Int, ObjectType, PubSub, PubSubEngine, Query, Resolver, Root, Subscription } from 'type-graphql';
import { Public } from '../directives/index.js';
export class Color {
  @Public()
  createdZ: string;
export class ColorNotification {
export interface ColorNotificationPayload {
@Resolver(Color)
export class ColorResolver {
  @Subscription(() => ColorNotification, { topics: 'COLOR' })
  colorSubscription(@Root() { message }: ColorNotificationPayload): ColorNotification {
    return { message, date: new Date() };
  @Query(() => [Color])
  async colors(@Ctx() _ctx: AppContext, @PubSub() pubSub: PubSubEngine) {
    const createdZ = new Date().toISOString();
    pubSub.publish('COLOR', {
      message: 'Colors were requested!',
      { ID: 1, name: 'Red', createdZ },
      { ID: 2, name: 'Orange', createdZ },
      { ID: 3, name: 'Yellow', createdZ },
      { ID: 4, name: 'Green', createdZ },
      { ID: 5, name: 'Blue', createdZ },
      { ID: 6, name: 'Purple', createdZ },
