  selector: 'app-appearance',
      <h2>Appearance Settings</h2>
        <label>Theme</label>
        <select class="form-control">
          <option>Light</option>
          <option>Dark</option>
          <option>Auto</option>
        <label>Font Size</label>
          <option>Small</option>
          <option selected>Medium</option>
          <option>Large</option>
        <label>Accent Color</label>
        <div class="color-options">
          <div class="color-option" style="background: #1976d2;" title="Blue"></div>
          <div class="color-option" style="background: #388e3c;" title="Green"></div>
          <div class="color-option" style="background: #d32f2f;" title="Red"></div>
          <div class="color-option" style="background: #7b1fa2;" title="Purple"></div>
    .color-options {
        border-color: #424242;
export class AppearanceComponent {}
