  selector: 'app-collections',
    <div class="collections-container">
      <h2>Collections</h2>
      <div class="collections-grid">
        <div class="collection-card" *ngFor="let collection of collections">
          <i [class]="collection.icon"></i>
          <h3>{{ collection.name }}</h3>
          <p>{{ collection.itemCount }} items</p>
    .collections-container {
    .collections-grid {
    .collection-card {
export class CollectionsComponent {
  collections = [
    { name: 'Q4 Reports', icon: 'fa-solid fa-folder', itemCount: 12 },
    { name: 'Client Feedback', icon: 'fa-solid fa-folder', itemCount: 8 },
    { name: 'Design Assets', icon: 'fa-solid fa-folder', itemCount: 24 }
