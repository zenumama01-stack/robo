 * Modal for creating and editing collections
  selector: 'mj-collection-form-modal',
        [title]="collection?.ID ? 'Edit Collection' : 'New Collection'"
        [minWidth]="300">
        <div class="collection-form">
              placeholder="Collection name"
              #nameInput
              (keydown.enter)="onSave()">
              rows="3">
          @if (parentCollection) {
              <label class="form-label">Parent Collection</label>
                <span>{{ parentCollection.Name }}</span>
            {{ isSaving ? 'Saving...' : 'Save' }}
    .collection-form {
export class CollectionFormModalComponent implements OnChanges {
  @Input() collection?: MJCollectionEntity;
  @Input() parentCollection?: MJCollectionEntity;
  @Output() saved = new EventEmitter<MJCollectionEntity>();
    if (changes['collection'] || changes['isOpen']) {
      if (this.isOpen && this.collection) {
        this.formData.name = this.collection.Name || '';
        this.formData.description = this.collection.Description || '';
      } else if (this.isOpen && !this.collection) {
        this.formData = { name: '', description: '' };
    return this.formData.name.trim().length > 0;
      // Validate permissions before saving
      if (this.collection) {
        // Editing existing collection - need Edit permission
        if (this.collection.OwnerID && this.collection.OwnerID !== this.currentUser.ID) {
            this.collection.ID,
            this.errorMessage = 'You do not have Edit permission for this collection.';
      } else if (this.parentCollection) {
        // Creating child collection - need Edit permission on parent
        if (this.parentCollection.OwnerID && this.parentCollection.OwnerID !== this.currentUser.ID) {
            this.parentCollection.ID,
            this.errorMessage = 'You do not have Edit permission for the parent collection.';
      const collection = this.collection ||
        await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
      collection.Name = this.formData.name.trim();
      collection.Description = this.formData.description.trim() || null;
      // Set owner and parent relationship if creating new collection
      if (!this.collection) {
        if (this.parentCollection) {
          // Child collection inherits parent's owner to maintain permission hierarchy
          collection.ParentID = this.parentCollection.ID;
          collection.OwnerID = this.parentCollection.OwnerID || this.currentUser.ID;
          // Root collection - current user becomes owner
        // Updating existing collection's parent
        // If creating new collection, set up permissions
            // Child collection - copy all permissions from parent (including owner)
            // Root collection - create owner permission for current user
        this.toastService.success(
          this.collection ? 'Collection updated successfully' : 'Collection created successfully'
        this.saved.emit(collection);
        this.errorMessage = collection.LatestResult?.Message || 'Failed to save collection';
      console.error('Error saving collection:', error);
