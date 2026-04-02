import { IRunReportProvider, RunReportResult } from './interfaces';
export type RunReportParams = {
    ReportID: string
 * Class used to run a report and return the results.
export class RunReport  {
    private _provider: IRunReportProvider;
     * Optionally, you can pass in a provider to use for running the report. If you dont pass in a provider, the static provider will be used.
    constructor(provider: IRunReportProvider | null = null) {
     * Returns the provider to be used for this instance, if one was passed in. Otherwise, it returns the static provider.
    public get ProviderToUse(): IRunReportProvider {
        return this._provider || RunReport.Provider;
        return this.ProviderToUse.RunReport(params, contextUser);
    private static _globalProviderKey: string = 'MJ_RunReportProvider';
    public static get Provider(): IRunReportProvider {
            return g[RunReport._globalProviderKey];
    public static set Provider(value: IRunReportProvider) {
            g[RunReport._globalProviderKey] = value;
