function handleFocusBlur () {
  // Note that while Chromium content APIs have observer for focus/blur, they
  // unfortunately do not work for webview.
  window.addEventListener('focus', () => {
    ipcRendererInternal.send(IPC_MESSAGES.GUEST_VIEW_MANAGER_FOCUS_CHANGE, true);
  window.addEventListener('blur', () => {
    ipcRendererInternal.send(IPC_MESSAGES.GUEST_VIEW_MANAGER_FOCUS_CHANGE, false);
export function webViewInit (webviewTag: boolean, isWebView: boolean) {
  // Don't allow recursive `<webview>`.
  if (webviewTag && !isWebView) {
    const guestViewInternal = require('@electron/internal/renderer/web-view/guest-view-internal') as typeof guestViewInternalModule;
    if (process.contextIsolated) {
      v8Util.setHiddenValue(window, 'guestViewInternal', guestViewInternal);
      setupWebView({
        guestViewInternal,
        allowGuestViewElementDefinition: webFrame.allowGuestViewElementDefinition,
        setIsWebView: iframe => v8Util.setHiddenValue(iframe, 'isWebView', true)
    // Report focus/blur events of webview to browser.
    handleFocusBlur();
