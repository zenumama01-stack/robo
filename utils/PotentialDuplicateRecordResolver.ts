import { Arg, Ctx, Field, Float, InputType, Int, ObjectType, Query, Resolver } from 'type-graphql';
  PotentialDuplicate,
  PotentialDuplicateResult,
import { CompositeKeyInputType, CompositeKeyOutputType, KeyValuePairOutputType } from '../generic/KeyInputOutputTypes.js';
export class PotentialDuplicateRequestType extends PotentialDuplicateRequest {
  declare EntityID: string;
  declare RecordIDs: CompositeKey[];
  declare EntityDocumentID: string;
  declare ProbabilityScore: number;
  declare ListID: string;
export class PotentialDuplicateType extends PotentialDuplicate {
  @Field(() => Float)
  declare KeyValuePairs: KeyValuePairOutputType[];
export class PotentialDuplicateResultType extends PotentialDuplicateResult {
  @Field(() => [PotentialDuplicateType])
  declare Duplicates: PotentialDuplicateType[];
  RecordPrimaryKeys: CompositeKey;
  declare DuplicateRunDetailMatchRecordIDs: string[];
export class PotentialDuplicateResponseType extends PotentialDuplicateResponse {
  declare Status: 'Inprogress' | 'Success' | 'Error';
  declare ErrorMessage?: string;
  @Field(() => [PotentialDuplicateResultType])
  declare PotentialDuplicateResult: PotentialDuplicateResult[];
@Resolver(PotentialDuplicateResponseType)
export class DuplicateRecordResolver {
  @Query(() => PotentialDuplicateResponseType)
  async GetRecordDuplicates(
    @Arg('params') params: PotentialDuplicateRequestType
  ): Promise<PotentialDuplicateResponseType> {
    const user = UserCache.Instance.Users.find((u) => u.Email.trim().toLowerCase() === userPayload.email.trim().toLowerCase());
      throw new Error(`User ${userPayload.email} not found in UserCache`);
    const result = await md.GetRecordDuplicates(params, user);
