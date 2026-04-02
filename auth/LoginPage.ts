 * @fileoverview Login Page HTML Templates for OAuth Proxy
 * Provides HTML templates for the login UI during OAuth flows.
 * These pages provide a better user experience than raw redirects.
 * @module @memberjunction/ai-mcp-server/auth/LoginPage
 * Options for rendering the login page.
export interface LoginPageOptions {
  /** Name of the MCP client requesting access */
  clientName?: string;
  /** Provider name for display (e.g., "Microsoft", "Google") */
  providerName?: string;
  /** URL to redirect to when user clicks continue */
  continueUrl: string;
  /** Resource name (e.g., "MemberJunction MCP Server") */
  resourceName?: string;
  /** Custom logo URL (optional) */
  logoUrl?: string;
 * Options for rendering the error page.
export interface ErrorPageOptions {
  /** Error title */
  /** Error message */
  /** Whether to show a "try again" button */
  showRetry?: boolean;
  /** URL to retry (if showRetry is true) */
  retryUrl?: string;
 * Options for rendering the success page.
export interface SuccessPageOptions {
  /** Client name that received access */
  /** Message to show (e.g., "You can close this window") */
 * Renders the login consent page.
 * This page is shown when a user arrives at the authorization endpoint
 * to provide context about what they're authorizing.
export function renderLoginPage(options: LoginPageOptions): string {
    clientName = 'An application',
    providerName = 'your identity provider',
    continueUrl,
    resourceName = 'MemberJunction MCP Server',
    logoUrl,
  const logoHtml = logoUrl
    ? `<img src="${escapeHtml(logoUrl)}" alt="Logo" class="logo" />`
    : `<div class="logo-text">MJ</div>`;
  <title>Sign In - ${escapeHtml(resourceName)}</title>
    .card {
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      max-width: 420px;
      padding: 2.5rem;
    .logo {
      width: 80px;
      height: 80px;
      margin-bottom: 1.5rem;
      object-fit: cover;
    .logo-text {
      margin: 0 auto 1.5rem;
      font-size: 2rem;
      font-size: 1.5rem;
      margin-bottom: 0.5rem;
      font-size: 0.95rem;
      margin-bottom: 2rem;
    .client-name {
    .permissions {
    .permissions h3 {
      font-size: 0.85rem;
      margin-bottom: 0.75rem;
    .permissions ul {
    .permissions li {
      font-size: 0.9rem;
      padding: 0.5rem 0;
    .permissions li::before {
      content: "✓";
      color: #28a745;
      margin-right: 0.75rem;
    .btn {
      padding: 0.875rem 1.5rem;
      font-size: 1rem;
      transition: transform 0.15s, box-shadow 0.15s;
    .btn-primary {
    .btn-primary:hover {
      box-shadow: 0 5px 20px rgba(102, 126, 234, 0.4);
    .btn-primary:active {
    .security-note {
      font-size: 0.8rem;
    .security-note a {
    ${logoHtml}
    <h1>Sign In Required</h1>
    <p class="subtitle">
      <span class="client-name">${escapeHtml(clientName)}</span> wants to access
      <span class="client-name">${escapeHtml(resourceName)}</span>
    <div class="permissions">
      <h3>This will allow the application to:</h3>
        <li>View your profile information</li>
        <li>Access MemberJunction on your behalf</li>
        <li>Execute MCP tools using your permissions</li>
    <a href="${escapeHtml(continueUrl)}" class="btn btn-primary">
      Continue with ${escapeHtml(providerName)}
      You'll be redirected to ${escapeHtml(providerName)} to sign in securely.
 * Renders an error page.
export function renderErrorPage(options: ErrorPageOptions): string {
  const { title, message, showRetry = false, retryUrl } = options;
  const retryButton = showRetry && retryUrl
    ? `<a href="${escapeHtml(retryUrl)}" class="btn btn-primary">Try Again</a>`
  <title>${escapeHtml(title)} - MemberJunction MCP Server</title>
      box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
    .icon-error {
      color: #dc2626;
      margin-bottom: 1rem;
      padding: 0.875rem 2rem;
    <div class="icon-error">⚠️</div>
    <h1>${escapeHtml(title)}</h1>
    ${retryButton}
 * Renders a success page.
export function renderSuccessPage(options: SuccessPageOptions): string {
    clientName = 'The application',
    message = 'You can now close this window and return to your application.',
  <title>Authentication Successful - MemberJunction MCP Server</title>
    .icon-success {
      font-size: 2.5rem;
      color: #059669;
      margin-top: 1rem;
    <div class="icon-success">✓</div>
    <h1>Authentication Successful!</h1>
      <span class="client-name">${escapeHtml(clientName)}</span> has been granted access.
 * Escapes HTML special characters.
