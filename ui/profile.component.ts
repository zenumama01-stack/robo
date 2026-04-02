  selector: 'app-profile',
    <div class="profile-settings">
      <h2>Profile Settings</h2>
        <label>Full Name</label>
        <input type="text" value="John Doe" class="form-control">
        <input type="email" value="john.doe@example.com" class="form-control">
        <label>Role</label>
        <input type="text" value="Developer" class="form-control" readonly>
      <button class="save-btn">Save Changes</button>
    .profile-settings {
        &:readonly {
export class ProfileComponent {}
