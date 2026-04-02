 * Testing Explorer Resource - displays interactive test and suite browser
@RegisterClass(BaseResourceComponent, 'TestingExplorerResource')
  selector: 'mj-testing-explorer-resource',
      <app-testing-explorer></app-testing-explorer>
export class TestingExplorerResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    return 'Explorer';
    return 'fa-solid fa-compass';
