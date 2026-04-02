 * Testing Runs Resource - displays test execution history and monitoring
@RegisterClass(BaseResourceComponent, 'TestingRunsResource')
  selector: 'mj-testing-runs-resource',
      <app-testing-runs></app-testing-runs>
export class TestingRunsResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    return 'Runs';
