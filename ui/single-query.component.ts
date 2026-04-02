import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { LogError, Metadata, RunQuery } from '@memberjunction/core';
import { QueryGridComponent } from '@memberjunction/ng-query-grid';
  selector: 'mj-single-query',
  templateUrl: './single-query.component.html',
  styleUrls: ['./single-query.component.css']
export class SingleQueryComponent implements OnInit {
  @Input() queryId!: string;  
  @ViewChild('theQuery', { static: true }) theQuery!: QueryGridComponent;
  public queryEntity!: MJQueryEntity;
  public queryData!: any[];
    console.log('SingleQueryComponent - Raw queryId received:', this.queryId);
    console.log('SingleQueryComponent - queryId type:', typeof this.queryId);
    // Clean any quotes that might have been added upstream
    if (this.queryId && typeof this.queryId === 'string') {
      this.queryId = this.queryId.replace(/^['"]|['"]$/g, '');
    console.log('SingleQueryComponent - Cleaned queryId:', this.queryId);
    this.doLoad();
  async doLoad(): Promise<void> {
      // get info on the report we are loading
      this.queryEntity = await md.GetEntityObject<MJQueryEntity>('MJ: Queries', md.CurrentUser);
      // Use the clean ID without quotes for GraphQL/entity loading
      const loadResult = await this.queryEntity.Load(this.queryId);
      if(!loadResult) {
        LogError(`Failed to load query with ID: ${this.queryId}`, undefined, this.queryEntity.LatestResult);
      const runReport = new RunQuery();
      // RunQuery also should use the clean ID
      const result = await runReport.RunQuery({QueryID: this.queryId});
      if (result && result.Success && result.Results.length > 0) {
        this.queryData = result.Results;
