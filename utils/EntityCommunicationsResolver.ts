import { RunViewByIDInput } from '../generic/RunViewResolver.js';
import { EntityCommunicationsEngine } from '@memberjunction/entity-communications-server';
import { GraphQLJSONObject } from 'graphql-type-json';
export class CommunicationProviderMessageType {
  CommunicationProviderID: number;
  CommunicationBaseMessageTypeID: number;
  AdditionalAttributes: string;
  _mj_CreatedAt: Date;
  _mj_UpdatedAt: Date;
  CommunicationProvider?: string;
  CommunicationBaseMessageType?: string;
export class TemplateInputType {
  CategoryID?: number;
  UserID: number;
export class CommunicationMessageInput {
  @Field(() => CommunicationProviderMessageType)
  public MessageType: CommunicationProviderMessageType;
  @Field(() => TemplateInputType, { nullable: true })
  public BodyTemplate?: TemplateInputType;
  public HTMLBodyTemplate?: TemplateInputType;
  public SubjectTemplate?: TemplateInputType;
  @Field(() => GraphQLJSONObject, { nullable: true })
export class RunEntityCommunicationResultType {
  public Results?: any;
@Resolver(RunEntityCommunicationResultType)
export class ReportResolver extends ResolverBase {
  @Query(() => RunEntityCommunicationResultType)
  async RunEntityCommunicationByViewID(
    @Arg('entityID', () => String) entityID: string,
    @Arg('runViewByIDInput', () => RunViewByIDInput) runViewByIDInput: RunViewByIDInput,
    @Arg('providerName', () => String) providerName: string,
    @Arg('providerMessageTypeName', () => String) providerMessageTypeName: string,
    @Arg('message', () => CommunicationMessageInput) message: CommunicationMessageInput,
    @Arg('previewOnly', () => Boolean) previewOnly: boolean,
    @Arg('includeProcessedMessages', () => Boolean) includeProcessedMessages: boolean,
  ): Promise<RunEntityCommunicationResultType> {
    // Check API key scope authorization for communication send
    await this.CheckAPIKeyScopeAuthorization('communication:send', entityID, userPayload);
      await EntityCommunicationsEngine.Instance.Config(false, userPayload.userRecord);
      const newMessage = new Message(message as unknown as Message);
      await TemplateEngineServer.Instance.Config(false, userPayload.userRecord);
      // for the templates, replace the values from the input with the objects from the Template Engine we have here
      if (newMessage.BodyTemplate) {
        newMessage.BodyTemplate = TemplateEngineServer.Instance.FindTemplate(newMessage.BodyTemplate.Name);
      if (newMessage.HTMLBodyTemplate) {
        newMessage.HTMLBodyTemplate = TemplateEngineServer.Instance.FindTemplate(newMessage.HTMLBodyTemplate.Name);
      if (newMessage.SubjectTemplate) {
        newMessage.SubjectTemplate = TemplateEngineServer.Instance.FindTemplate(newMessage.SubjectTemplate.Name);
      const params: EntityCommunicationParams = {
        RunViewParams: <RunViewParams>runViewByIDInput,
        ProviderName: providerName,
        ProviderMessageTypeName: providerMessageTypeName,
        Message: newMessage,
        PreviewOnly: previewOnly,
        IncludeProcessedMessages: includeProcessedMessages,
      const result = await EntityCommunicationsEngine.Instance.RunEntityCommunication(params);
        ErrorMessage: result.ErrorMessage,
        Results: includeProcessedMessages && result.Results ? { Results: result.Results } : undefined,
      const { message } = z
        .object({ message: z.string() })
        .catch({ message: JSON.stringify(e) })
        .parse(e);
        ErrorMessage: message,
