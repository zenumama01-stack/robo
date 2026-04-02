  selector: 'app-notifications',
      <h2>Notification Settings</h2>
        <div class="setting-info">
          <h4>Email Notifications</h4>
          <p>Receive email updates about your activity</p>
        <label class="toggle">
          <input type="checkbox" checked>
          <span class="slider"></span>
          <h4>Push Notifications</h4>
          <p>Get notified about important updates</p>
          <input type="checkbox">
          <h4>Daily Digest</h4>
          <p>Receive a daily summary email</p>
    .notifications-settings {
      .setting-info {
    .toggle {
      input {
        &:checked + .slider {
          background-color: #1976d2;
          &:before {
            transform: translateX(22px);
      .slider {
        transition: 0.2s;
export class NotificationsComponent {}
