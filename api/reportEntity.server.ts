import { BaseEntity, Metadata, RunView, IMetadataProvider } from "@memberjunction/core";
import { MJReportEntity, MJReportSnapshotEntity, MJReportVersionEntity } from "@memberjunction/core-entities";
import { SkipAPIAnalysisCompleteResponse } from "@memberjunction/skip-types";
export class ReportEntity_Server extends MJReportEntity  {
     * The server side Report Entity sub-class has a simple logic that will create a snapshot of the report when it is first created and each time it is modified, but only
     * if it is either newly created or if the Configuration field has changed.
            // first let's get the actual parsed object for the Configuration field
            const config = this.Configuration && this.Configuration.length > 0 ? SafeJSONParse(this.Configuration) : null;
            const oldTextConfig = this.GetFieldByName('Configuration')?.OldValue;
            const oldConfig = oldTextConfig && oldTextConfig.length > 0 ? SafeJSONParse(oldTextConfig) : null;
            if ( (config && oldConfig) || (config && !this.IsSaved) ) {
                let createSnapshot = false;
                let createReportVersion = false;
                    // existing record
                    // we nave a configuration and an old configuration, so we can compare them, cast them to their strong types first
                    const castedConfig = config as SkipAPIAnalysisCompleteResponse;
                    const castedOldConfig = oldConfig as SkipAPIAnalysisCompleteResponse;
                    // Next, we need to see if the data context has changed at all. We do this by comparing the # of items in the data context. a Data context's items are immutable and we only add new Data Context Items
                    // so we can simply check to see if we have new data context items or not
                    const dataContextChanged = Object.keys(castedConfig.dataContext || {})?.length !== Object.keys(castedOldConfig.dataContext || {})?.length;
                    // next up, we check to see if anything in the execution results changed. The simple way to do that is convert the execution results back to a string and compare those strings
                    const executionResultsChanged = JSON.stringify(castedConfig.executionResults) !== JSON.stringify(castedOldConfig.executionResults);
                    // next up we check to see if the analysis changed, which would also represent a "data" change
                    const analysisChanged = castedConfig.analysis !== castedOldConfig.analysis;
                    // finally, we check to see if the configuration itself has OUTSIDE of the data elements we just checked. We do that by deleting those properties from those objects, converting to strings
                    // and comparing them
                    delete castedConfig.dataContext;
                    delete castedConfig.analysis;
                    delete castedConfig.executionResults;
                    delete castedOldConfig.dataContext;
                    delete castedOldConfig.executionResults;
                    delete castedOldConfig.analysis;
                    // we can now compare the two objects
                    const configChanged = JSON.stringify(castedConfig) !== JSON.stringify(castedOldConfig);
                    // now our logic is this - if the DATA changed, but the configuration did NOT change, that means we want to create a new SNAPSHOT of the report - which is the data changing side
                    // if the configuration changed at all we wnat to create a new Report Version. Sometimes both changd and we do both
                    createSnapshot = dataContextChanged || executionResultsChanged || analysisChanged;
                    createReportVersion = configChanged;
                    // we have a new record set createSnapshot to true and createReportVersion to true
                    createSnapshot = true;
                    createReportVersion = true;
                const wasNewRecord: boolean = !this.IsSaved;
                if (saveResult && (createSnapshot || createReportVersion)) {
                    // here we either have a new record or the configuration has changed, so we need to create a snapshot of the report
                    if (createReportVersion) {
                        const reportVersion = await md.GetEntityObject<MJReportVersionEntity>('MJ: Report Versions', this.ContextCurrentUser);
                        reportVersion.ReportID = this.ID;
                        // const get highest new version number already in DB
                        let newVersionNumber: number = 1;
                                Fields: ['VersionNumber'],
                                EntityName: 'MJ: Report Versions',
                                ExtraFilter: `ReportID = '${this.ID}'`,
                            if (rvResult.Success && rvResult.Results.length > 0) {
                                newVersionNumber = parseInt(rvResult.Results[0].VersionNumber) + 1;
                                newVersionNumber = 1; // this is an error state as we previously had a saved report, BUT there were no saved versions for the report, so we are going to use 1 here, but let's log a warning
                                console.warn('ReportEntity_Server.Save(): No report versions found for report ID:', this.ID);
                                console.warn('ReportEntity_Server.Save(): Using version number 1 for new report version');
                        reportVersion.Name = this.Name; // copy current name to the new version
                        reportVersion.Description = this.Description; // copy current description to the new version
                        reportVersion.VersionNumber = newVersionNumber;
                        reportVersion.Configuration = JSON.stringify(this.Configuration);
                        success = success && await reportVersion.Save();
                            console.error('ReportEntity_Server.Save(): Error saving report version\n' + reportVersion.LatestResult.CompleteMessage);
                    if (createSnapshot) {
                        const snapshot = await md.GetEntityObject<MJReportSnapshotEntity>('MJ: Report Snapshots', this.ContextCurrentUser);
                        snapshot.ReportID = this.ID;
                        snapshot.UserID = this.ContextCurrentUser.ID;
                        // in the snapshot entity the ResultSet column is the Configuration column from the Report entity
                        snapshot.ResultSet = JSON.stringify(this.Configuration);
                        success = success && await snapshot.Save();
                // no configuration or no parseable configuration, so we just call super.Save();
                return super.Save();
            console.error('Error in ReportEntity_Server.Save():', e);
