import { RunViewParams } from '@memberjunction/core';
  selector: 'mj-single-search-result',
  templateUrl: './single-search-result.component.html',
  styleUrls: ['./single-search-result.component.css']
export class SingleSearchResultComponent {
  @Input() public entity: string = '';
  @Input() public searchInput: string = '';
  public get params(): RunViewParams {
    const p: RunViewParams = {
      EntityName: this.entity,
      ExtraFilter: "ID IS NOT NULL", // temporary hack as ExtraFilter is required for dynamic views
      UserSearchString: this.searchInput,
