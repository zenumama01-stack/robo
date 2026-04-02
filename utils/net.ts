import { ClientRequest } from '@electron/internal/common/api/net-client-request';
import { app, IncomingMessage, session } from 'electron/main';
import type { ClientRequestConstructorOptions } from 'electron/main';
const { isOnline } = process._linkedBinding('electron_common_net');
export function request (options: ClientRequestConstructorOptions | string, callback?: (message: IncomingMessage) => void) {
    throw new Error('net module can only be used after app is ready');
  return new ClientRequest(options, callback);
export function fetch (input: RequestInfo, init?: RequestInit): Promise<Response> {
  return session.defaultSession.fetch(input, init);
export function resolveHost (host: string, options?: Electron.ResolveHostOptions): Promise<Electron.ResolvedHost> {
  return session.defaultSession.resolveHost(host, options);
exports.isOnline = isOnline;
Object.defineProperty(exports, 'online', {
  get: () => isOnline()
import type { ClientRequestConstructorOptions, IncomingMessage } from 'electron/utility';
const { isOnline, resolveHost } = process._linkedBinding('electron_common_net');
  return fetchWithSession(input, init, undefined, request);
exports.resolveHost = resolveHost;
