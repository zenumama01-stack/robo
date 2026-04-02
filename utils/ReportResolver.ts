import { EntitySaveOptions, IRunReportProvider, Metadata, RunReport } from '@memberjunction/core';
import { Arg, Ctx, Field, Int, Mutation, ObjectType, Query, Resolver } from 'type-graphql';
import { MJConversationDetailEntity, MJReportEntity } from '@memberjunction/core-entities';
export class RunReportResultType {
export class CreateReportResultType {
  ReportName: string;
@Resolver(RunReportResultType)
export class ReportResolverExtended extends ResolverBase {
  @Query(() => RunReportResultType)
  async GetReportData(@Arg('ReportID', () => String) ReportID: string, @Ctx() context: AppContext): Promise<RunReportResultType> {
    // Check API key scope authorization for report run
    await this.CheckAPIKeyScopeAuthorization('report:run', ReportID, context.userPayload);
    const rp = new RunReport(provider as unknown as IRunReportProvider);
    const result = await rp.RunReport({ ReportID: ReportID });
      ReportID: ReportID,
      RowCount: result.RowCount,
      ExecutionTime: result.ExecutionTime,
   * This mutation will create a new report from a conversation detail ID
  @Mutation(() => CreateReportResultType)
  async CreateReportFromConversationDetailID(
    @Arg('ConversationDetailID', () => String) ConversationDetailID: string,
    @Ctx() { dataSource, userPayload, providers }: AppContext
  ): Promise<CreateReportResultType> {
    // Check API key scope authorization for report creation (uses entity:create for Reports entity)
    await this.CheckAPIKeyScopeAuthorization('entity:create', 'Reports', userPayload);
      const u = UserCache.Users.find((u) => u.Email?.trim().toLowerCase() === userPayload?.email?.trim().toLowerCase());
      if (!u) throw new Error('Unable to find user');
      const cde = md.Entities.find((e) => e.Name === 'MJ: Conversation Details');
      if (!cde) throw new Error('Unable to find Conversation Details Entity metadata');
      const cd = md.Entities.find((e) => e.Name === 'MJ: Conversations');
      if (!cd) throw new Error('Unable to find Conversations Entity metadata');
                      cd.Message, cd.ConversationID, c.DataContextID
                      ${cde.SchemaName}.${cde.BaseView} cd
                      ${cd.SchemaName}.${cd.BaseView} c
                      cd.ConversationID = c.ID
                      cd.ID='${ConversationDetailID}'`;
      if (!result || !result.recordset || result.recordset.length === 0) throw new Error('Unable to retrieve converation details');
      const skipData = <SkipAPIAnalysisCompleteResponse>JSON.parse(result.recordset[0].Message);
      const report = await md.GetEntityObject<MJReportEntity>('MJ: Reports', u);
      report.NewRecord();
      // support the legacy report title as old conversation details had a reportTitle property
      // but the new SkipData object has a title property, so favor the title property
      const title = skipData.title ? skipData.title : skipData.reportTitle ? skipData.reportTitle : 'Untitled Report';
      report.Name = title;
      report.Description = skipData.userExplanation ? skipData.userExplanation : '';
      report.ConversationID = result.recordset[0].ConversationID;
      report.ConversationDetailID = ConversationDetailID;
      const dc: DataContext = new DataContext();
      await dc.LoadMetadata(result.recordset[0].DataContextID, u);
      const newDataContext = await DataContext.Clone(dc, false, u);
      if (!newDataContext) throw new Error('Error cloning data context');
      report.DataContextID = newDataContext.ID;
      // next, strip out the messags from the SkipData object to put them into our Report Configuration as we dont need to store that information as we have a
      // link back to the conversation and conversation detail, skipData can be modified as it is a copy of the data doesn't affect the original source
      skipData.messages = [];
      report.Configuration = JSON.stringify(skipData);
      report.SharingScope = 'None';
      report.UserID = u.ID;
      if (await report.Save()) {
          ReportID: report.ID,
          ReportName: report.Name,
          ReportID: '',
          ReportName: '',
          ErrorMessage: 'Unable to save new report',
      const err = z.object({ message: z.string() }).safeParse(ex);
        ErrorMessage: 'Unable to create new report: ' + err.success ? err.data.message : String(ex),
