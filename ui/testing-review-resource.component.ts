 * Testing Review Resource - displays human-in-the-loop review workflow
@RegisterClass(BaseResourceComponent, 'TestingReviewResource')
  selector: 'mj-testing-review-resource',
      <app-testing-review></app-testing-review>
export class TestingReviewResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    return 'Review';
    return 'fa-solid fa-clipboard-check';
