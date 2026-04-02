import { net, systemPreferences } from 'electron/utility';
  if (e.data === 'Hello from parent!') {
    process.parentPort.postMessage('Hello from child!');
