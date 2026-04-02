@RegisterClass(BaseResourceComponent, 'SearchResultsResource')
    selector: 'mj-search-results-resource',
    template: `<mj-single-search-result [entity]="Data.Configuration.Entity" [searchInput]="Data.Configuration.SearchInput" (loadComplete)="NotifyLoadComplete()" (loadStarted)="NotifyLoadStarted()"></mj-single-search-result>`
export class SearchResultsResource extends BaseResourceComponent implements OnInit {
        return `Search (${data.Configuration.Entity}): ${data.Configuration.SearchInput}`;
