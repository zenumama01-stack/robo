import { KeyValuePair } from "@memberjunction/core";
import { Field, InputType, ObjectType } from "type-graphql";
export class KeyValuePairInputType {
  @Field(() => String)
export class KeyValuePairOutputType {
export class CompositeKeyInputType {
  @Field(() => [KeyValuePairInputType])
export class CompositeKeyOutputType {
  @Field(() => [KeyValuePairOutputType])
