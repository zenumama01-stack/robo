 * @fileoverview CSS Styles for OAuth UI Pages
 * Provides consistent MemberJunction branding for OAuth flow pages:
 * - Login page
 * - Consent page
 * - Error pages
 * @module @memberjunction/ai-mcp-server/auth/styles
 * Returns the CSS styles for OAuth UI pages.
 * Uses MemberJunction brand colors and modern design patterns.
export function getOAuthStyles(): string {
      /* MemberJunction brand colors - matching MJExplorer */
      --mj-primary: #0076B6;
      --mj-primary-hover: #005a9e;
      --mj-navy: #092340;
      --mj-light-blue: #AAE7FD;
      --mj-secondary: #AAA;
      --mj-secondary-hover: #888;
      --mj-success: #28a745;
      --mj-error: #dc3545;
      --mj-warning: #ffc107;
      --mj-background: #F4F4F4;
      --mj-card-bg: #ffffff;
      --mj-text: #092340;
      --mj-text-muted: #AAA;
      --mj-border: #D9D9D9;
      --mj-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
      --mj-border-radius: 1rem;
      background: var(--mj-background);
      color: var(--mj-text);
      max-width: 480px;
      background: var(--mj-card-bg);
      border-radius: var(--mj-border-radius);
      box-shadow: var(--mj-shadow);
    .logo-svg {
      max-width: 56px;
      max-height: 32px;
      margin: 0 auto 1rem auto;
      color: var(--mj-navy);
      font-size: 1.125rem;
      color: var(--mj-text-muted);
      letter-spacing: 0.05em;
    .user-info {
      padding: 0.75rem 1rem;
    .user-info p {
    .client-info {
    .client-info p {
      font-size: 0.9375rem;
    .scope-section {
    .scope-description {
    /* Full Access Section - Special treatment for full_access scope */
    .full-access-section {
      padding-bottom: 1rem;
      border-bottom: 2px solid var(--mj-border);
    .full-access-item {
      gap: 0.75rem;
      background: linear-gradient(135deg, #fef3c7, #fffbeb);
    .full-access-item:hover {
      background: linear-gradient(135deg, #fde68a, #fef3c7);
      border-color: #d97706;
    .full-access-item input[type="checkbox"] {
      width: 1.25rem;
      height: 1.25rem;
      margin-top: 0.25rem;
      accent-color: #f59e0b;
    .full-access-content {
    .full-access-icon {
    .full-access-text {
    .full-access-label {
    .full-access-desc {
      font-size: 0.8125rem;
      color: #a16207;
    .clear-all-btn {
      gap: 0.5rem;
      margin-top: 0.75rem;
      padding: 0.5rem 1rem;
      border: 1px solid var(--mj-border);
    .clear-all-btn:hover {
    /* Legacy grant-all styles - kept for backwards compatibility */
    .grant-all-section {
      border-bottom: 1px solid var(--mj-border);
    .grant-all-item {
      background: linear-gradient(135deg, #e6f4fb, #f0faff);
      border: 2px solid var(--mj-light-blue);
    .grant-all-item:hover {
      background: linear-gradient(135deg, #cce9f7, #e0f4ff);
      border-color: var(--mj-primary);
    .grant-all-item input[type="checkbox"] {
      margin-top: 0.125rem;
      accent-color: var(--mj-primary);
    .grant-all-label {
    .grant-all-desc {
    .scope-category {
      margin-bottom: 1.25rem;
    .category-header {
      padding: 0.75rem;
      transition: background 0.2s ease;
    .category-header:hover {
      background: #e8e8e8;
    .category-header i {
    .category-toggle {
      font-size: 0.625rem;
      transition: transform 0.2s ease;
      width: 0.75rem;
    .category-name {
    .category-count {
      font-size: 0.75rem;
    .category-select-btn {
      padding: 0.25rem 0.5rem;
      font-size: 0.6875rem;
      color: var(--mj-primary);
      border: 1px solid var(--mj-primary);
      border-radius: 0.25rem;
      margin-left: 0.25rem;
    .category-select-btn:first-of-type {
      margin-left: auto;
    .category-select-btn:hover {
      background: var(--mj-primary);
    .scope-list {
      padding-top: 0.5rem;
    /* Prefix group container */
    .prefix-group {
      border-left: 3px solid var(--mj-border);
      padding-left: 0.75rem;
    .prefix-group.orphaned {
      border-left-color: var(--mj-text-muted);
    /* Scope children container */
    .scope-children {
      margin-left: 1.5rem;
      margin-top: 0.5rem;
      gap: 0.375rem;
      border-left: 2px solid var(--mj-border);
    .scope-item {
    .scope-item:hover {
      background: var(--mj-light-blue);
    /* Parent scope styling */
    .scope-item.scope-parent {
    .scope-item.scope-parent:hover {
      background: #f8fafc;
    .scope-item.scope-parent .scope-content {
    /* Child scope styling */
    .scope-item.scope-child {
      padding: 0.5rem 0.75rem;
      background: #fafafa;
    .scope-item.scope-child:hover {
    /* Disabled state for implied scopes */
    .scope-item input[type="checkbox"]:disabled {
    .scope-item:has(input:disabled) {
      background: #f0f0f0;
    .scope-item:has(input:disabled):hover {
    .scope-item input[type="checkbox"] {
      width: 1.125rem;
      height: 1.125rem;
    .scope-name {
      font-family: 'SF Mono', Monaco, 'Consolas', monospace;
    .scope-desc {
    .scope-children-note {
    /* Parent scope header with collapsible children */
    .parent-scope-header {
    .parent-scope-header:hover {
    .parent-scope-header input[type="checkbox"] {
    .parent-scope-header .scope-content {
    .children-toggle {
      width: 1.5rem;
      height: 1.5rem;
    .children-toggle:hover {
    /* Single parent in category - more compact styling */
    .prefix-group.single-in-category {
      border-left: none;
    .prefix-group.single-in-category .parent-scope-header {
    .prefix-group.single-in-category .parent-scope-header:hover {
    /* Integrated parent checkbox in category header */
    .category-header.integrated-parent {
      gap: 0.5rem 0.75rem;
    .category-header.integrated-parent .category-toggle {
    .integrated-parent-checkbox {
    .integrated-parent-checkbox input[type="checkbox"] {
    .integrated-parent-desc {
      margin-left: 1.25rem;
      margin-top: -0.25rem;
    /* Single-parent category scope list - show children directly */
    .scope-category.single-parent .scope-list {
      padding-left: 1.25rem;
    .scope-category.single-parent .scope-list .scope-children {
      margin-left: 0;
    .button-group {
      padding: 0.75rem 1.5rem;
      min-height: 44px;
      background: var(--mj-primary-hover);
      box-shadow: 0 4px 8px rgba(0, 118, 182, 0.3);
    .btn-secondary {
    .btn-secondary:hover {
      padding-top: 1rem;
      border-top: 1px solid var(--mj-border);
    .success-message {
      padding: 1rem 0;
    .success-message p {
    .error-message {
    .error-message p {
      border: 2px solid #fecaca;
    .error-box h2 {
      color: var(--mj-error);
    .provider-list {
      margin: 1.5rem 0;
    .provider-btn {
      border: 2px solid var(--mj-border);
    .provider-btn:hover {
    .provider-icon {
    .retry-link {
    .retry-link:hover {
    @media (max-width: 480px) {
        padding: 1.5rem;
