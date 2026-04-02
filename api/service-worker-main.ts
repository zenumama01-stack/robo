const { ServiceWorkerMain } = process._linkedBinding('electron_browser_service_worker_main');
Object.defineProperty(ServiceWorkerMain.prototype, 'ipc', {
    const ipc = new IpcMainImpl();
    Object.defineProperty(this, 'ipc', { value: ipc });
    return ipc;
ServiceWorkerMain.prototype.send = function (channel, ...args) {
  if (typeof channel !== 'string') {
    throw new TypeError('Missing required channel argument');
    return this._send(false /* internal */, channel, args);
    console.error('Error sending from ServiceWorkerMain: ', e);
ServiceWorkerMain.prototype.startTask = function () {
  // TODO(samuelmaddock): maybe make timeout configurable in the future
  const hasTimeout = false;
  const { id, ok } = this._startExternalRequest(hasTimeout);
    throw new Error('Unable to start service worker task.');
    end: () => this._finishExternalRequest(id)
module.exports = ServiceWorkerMain;
