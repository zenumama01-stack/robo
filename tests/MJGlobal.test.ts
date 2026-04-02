import { MJGlobal } from '../Global';
import { ClassFactory } from '../ClassFactory';
import { ObjectCache } from '../ObjectCache';
import { MJEvent, MJEventType, IMJComponent, MJGlobalProperty } from '../interface';
import { GetGlobalObjectStore } from '../util';
import { firstValueFrom, take, toArray } from 'rxjs';
 * Helper to clear the MJGlobal singleton from the global object store
 * so each test starts with a clean slate.
function clearMJGlobalSingleton(): void {
    // Remove all singleton keys
    for (const key of Object.keys(g)) {
      if (key.startsWith('___SINGLETON__')) {
describe('MJGlobal', () => {
    clearMJGlobalSingleton();
  describe('Singleton behavior', () => {
    it('should return the same instance on repeated calls to Instance', () => {
      const instance1 = MJGlobal.Instance;
      const instance2 = MJGlobal.Instance;
    it('should be stored in the global object store', () => {
      const instance = MJGlobal.Instance;
      expect(g).not.toBeNull();
      expect(g!['___SINGLETON__MJGlobal']).toBe(instance);
    it('should expose a GlobalKey property', () => {
      expect(instance.GlobalKey).toBe('___SINGLETON__MJGlobal');
  describe('ClassFactory getter', () => {
    it('should return an instance of ClassFactory', () => {
      const cf = MJGlobal.Instance.ClassFactory;
      expect(cf).toBeInstanceOf(ClassFactory);
    it('should return the same ClassFactory on repeated access', () => {
      const cf1 = MJGlobal.Instance.ClassFactory;
      const cf2 = MJGlobal.Instance.ClassFactory;
      expect(cf1).toBe(cf2);
  describe('ObjectCache getter', () => {
    it('should return an instance of ObjectCache', () => {
      const cache = MJGlobal.Instance.ObjectCache;
      expect(cache).toBeInstanceOf(ObjectCache);
    it('should return the same ObjectCache on repeated access', () => {
      const cache1 = MJGlobal.Instance.ObjectCache;
      const cache2 = MJGlobal.Instance.ObjectCache;
      expect(cache1).toBe(cache2);
  describe('Properties getter', () => {
    it('should return an array', () => {
      const props = MJGlobal.Instance.Properties;
      expect(Array.isArray(props)).toBe(true);
    it('should start as empty', () => {
      expect(MJGlobal.Instance.Properties.length).toBe(0);
    it('should allow adding properties directly to the array', () => {
      const prop = new MJGlobalProperty();
      prop.key = 'testKey';
      prop.value = 'testValue';
      MJGlobal.Instance.Properties.push(prop);
      expect(MJGlobal.Instance.Properties.length).toBe(1);
      expect(MJGlobal.Instance.Properties[0].key).toBe('testKey');
  describe('RegisterComponent', () => {
    it('should register a component without error', () => {
      const component: IMJComponent = {};
      expect(() => MJGlobal.Instance.RegisterComponent(component)).not.toThrow();
    it('should accumulate multiple registered components', () => {
      const mjg = MJGlobal.Instance;
      const comp1: IMJComponent = {};
      const comp2: IMJComponent = {};
      mjg.RegisterComponent(comp1);
      mjg.RegisterComponent(comp2);
      // We cannot inspect _components directly, but we can verify
      // the method does not throw after multiple registrations
  describe('RaiseEvent and GetEventListener', () => {
    it('should deliver events to non-replay listeners', async () => {
      const received: MJEvent[] = [];
      const listener = mjg.GetEventListener(false);
      const subscription = listener.subscribe((event) => {
        received.push(event);
      const event: MJEvent = {
        component: {} as IMJComponent,
        eventCode: 'test-code',
        args: { data: 42 },
      mjg.RaiseEvent(event);
      expect(received.length).toBe(1);
      expect(received[0].eventCode).toBe('test-code');
      expect(received[0].args.data).toBe(42);
    it('should deliver events to replay listeners', async () => {
        event: MJEventType.LoggedIn,
        eventCode: 'login',
      // Raise event BEFORE subscribing
      // Now subscribe with replay - should receive the event
      const replayListener = mjg.GetEventListener(true);
      const subscription = replayListener.subscribe((evt) => {
        received.push(evt);
      // ReplaySubject should have replayed the event
      expect(received[0].event).toBe(MJEventType.LoggedIn);
    it('should not replay events for non-replay listeners that subscribe after event', () => {
        eventCode: 'before-subscribe',
      const subscription = listener.subscribe((evt) => {
      // Non-replay should NOT get the event raised before subscription
      expect(received.length).toBe(0);
    it('should deliver multiple events in order', () => {
      const received: string[] = [];
        received.push(event.eventCode ?? '');
      mjg.RaiseEvent({ component: {} as IMJComponent, event: MJEventType.ComponentEvent, eventCode: 'first', args: null });
      mjg.RaiseEvent({ component: {} as IMJComponent, event: MJEventType.ComponentEvent, eventCode: 'second', args: null });
      mjg.RaiseEvent({ component: {} as IMJComponent, event: MJEventType.ComponentEvent, eventCode: 'third', args: null });
      expect(received).toEqual(['first', 'second', 'third']);
    it('should deliver events to both replay and non-replay listeners simultaneously', () => {
      const nonReplayReceived: MJEvent[] = [];
      const replayReceived: MJEvent[] = [];
      const nonReplaySub = mjg.GetEventListener(false).subscribe((e) => nonReplayReceived.push(e));
      const replaySub = mjg.GetEventListener(true).subscribe((e) => replayReceived.push(e));
        eventCode: 'dual',
      expect(nonReplayReceived.length).toBe(1);
      expect(replayReceived.length).toBe(1);
      nonReplaySub.unsubscribe();
      replaySub.unsubscribe();
  describe('Reset', () => {
    it('should clear registered components', () => {
      mjg.RegisterComponent({} as IMJComponent);
      mjg.Reset();
      // After reset, the internal _components array is empty.
      // We verify by checking that new event listeners work properly.
      const sub = mjg.GetEventListener(false).subscribe((e) => received.push(e));
    it('should create new event subjects so old subscriptions stop receiving', () => {
      const oldReceived: MJEvent[] = [];
      const oldSub = mjg.GetEventListener(false).subscribe((e) => oldReceived.push(e));
      // Raise event after reset
      mjg.RaiseEvent({
        eventCode: 'after-reset',
      // Old subscription should NOT receive the new event
      // because the subject was replaced
      expect(oldReceived.length).toBe(0);
      oldSub.unsubscribe();
    it('should allow new subscriptions to work after reset', () => {
        eventCode: 'post-reset',
      expect(received[0].eventCode).toBe('post-reset');
    it('should clear replay buffer after reset', () => {
      // Raise event before reset
        eventCode: 'before-reset',
      // Subscribe with replay after reset
      const sub = mjg.GetEventListener(true).subscribe((e) => received.push(e));
      // Should NOT receive the pre-reset event
  describe('GetGlobalObjectStore (via BaseSingleton)', () => {
    it('should return a non-null global object store', () => {
      const store = MJGlobal.Instance.GetGlobalObjectStore();
      expect(store).not.toBeNull();
