let nextItemID = 1;
const hiddenProperties = Symbol('hidden touch bar props');
const extendConstructHook = (target: any, hook: Function) => {
  const existingHook = target._hook;
  target._hook = function () {
    if (existingHook) existingHook.call(this);
    hook.call(this);
const ImmutableProperty = <T extends TouchBarItem<any>>(def: (config: T extends TouchBarItem<infer C> ? C : never, setInternalProp: <K extends keyof T>(k: K, v: T[K]) => void) => any) => (target: T, propertyKey: keyof T) => {
  extendConstructHook(target, function (this: T) {
    (this as any)[hiddenProperties][propertyKey] = def((this as any)._config, (k, v) => {
      (this as any)[hiddenProperties][k] = v;
  Object.defineProperty(target, propertyKey, {
    get: function () {
      return this[hiddenProperties][propertyKey];
    set: function () {
      throw new Error(`Cannot override property ${name}`);
    configurable: false
const LiveProperty = <T extends TouchBarItem<any>>(def: (config: T extends TouchBarItem<infer C> ? C : never) => any, onMutate?: (self: T, newValue: any) => void) => (target: T, propertyKey: keyof T) => {
    (this as any)[hiddenProperties][propertyKey] = def((this as any)._config);
    if (onMutate) onMutate((this as any), (this as any)[hiddenProperties][propertyKey]);
    set: function (value) {
      if (onMutate) onMutate((this as any), value);
      this[hiddenProperties][propertyKey] = value;
      this.emit('change', this);
abstract class TouchBarItem<ConfigType> extends EventEmitter {
  @ImmutableProperty(() => `${nextItemID++}`) id!: string;
  abstract type: string;
  abstract onInteraction: Function | null;
  child?: TouchBar;
  private _parents: { id: string; type: string }[] = [];
  private _config!: ConfigType;
  constructor (config: ConfigType) {
    this._config = this._config || config || {} as ConfigType;
    (this as any)[hiddenProperties] = {};
    const hook = (this as any)._hook;
    if (hook) hook.call(this);
    delete (this as any)._hook;
  public _addParent (item: TouchBarItem<any>) {
    const existing = this._parents.some(test => test.id === item.id);
      this._parents.push({
        type: item.type
  public _removeParent (item: TouchBarItem<any>) {
    this._parents = this._parents.filter(test => test.id !== item.id);
class TouchBarButton extends TouchBarItem<Electron.TouchBarButtonConstructorOptions> implements Electron.TouchBarButton {
  @ImmutableProperty(() => 'button')
    type!: string;
  @LiveProperty<TouchBarButton>(config => config.label)
    label!: string;
  @LiveProperty<TouchBarButton>(config => config.accessibilityLabel)
    accessibilityLabel!: string;
  @LiveProperty<TouchBarButton>(config => config.backgroundColor)
    backgroundColor!: string;
  @LiveProperty<TouchBarButton>(config => config.icon)
    icon!: Electron.NativeImage;
  @LiveProperty<TouchBarButton>(config => config.iconPosition)
    iconPosition!: Electron.TouchBarButton['iconPosition'];
  @LiveProperty<TouchBarButton>(config => typeof config.enabled !== 'boolean' ? true : config.enabled)
    enabled!: boolean;
  @ImmutableProperty<TouchBarButton>(({ click: onClick }) => typeof onClick === 'function' ? () => onClick() : null)
    onInteraction!: Function | null;
class TouchBarColorPicker extends TouchBarItem<Electron.TouchBarColorPickerConstructorOptions> implements Electron.TouchBarColorPicker {
  @ImmutableProperty(() => 'colorpicker')
  @LiveProperty<TouchBarColorPicker>(config => config.availableColors)
    availableColors!: string[];
  @LiveProperty<TouchBarColorPicker>(config => config.selectedColor)
    selectedColor!: string;
  @ImmutableProperty<TouchBarColorPicker>(({ change: onChange }, setInternalProp) => typeof onChange === 'function'
    ? (details: { color: string }) => {
        setInternalProp('selectedColor', details.color);
        onChange(details.color);
    : null)
class TouchBarGroup extends TouchBarItem<Electron.TouchBarGroupConstructorOptions> implements Electron.TouchBarGroup {
  @ImmutableProperty(() => 'group')
  @LiveProperty<TouchBarGroup>(config => config.items instanceof TouchBar ? config.items : new TouchBar(config.items), (self, newChild: TouchBar) => {
    if (self.child) {
      for (const item of self.child.orderedItems) {
        item._removeParent(self);
    for (const item of newChild.orderedItems) {
      item._addParent(self);
    child!: TouchBar;
  onInteraction = null;
class TouchBarLabel extends TouchBarItem<Electron.TouchBarLabelConstructorOptions> implements Electron.TouchBarLabel {
  @ImmutableProperty(() => 'label')
  @LiveProperty<TouchBarLabel>(config => config.label)
  @LiveProperty<TouchBarLabel>(config => config.accessibilityLabel)
  @LiveProperty<TouchBarLabel>(config => config.textColor)
    textColor!: string;
class TouchBarPopover extends TouchBarItem<Electron.TouchBarPopoverConstructorOptions> implements Electron.TouchBarPopover {
  @ImmutableProperty(() => 'popover')
  @LiveProperty<TouchBarPopover>(config => config.label)
  @LiveProperty<TouchBarPopover>(config => config.icon)
  @LiveProperty<TouchBarPopover>(config => config.showCloseButton)
    showCloseButton!: boolean;
  @LiveProperty<TouchBarPopover>(config => config.items instanceof TouchBar ? config.items : new TouchBar(config.items), (self, newChild: TouchBar) => {
class TouchBarSlider extends TouchBarItem<Electron.TouchBarSliderConstructorOptions> implements Electron.TouchBarSlider {
  @ImmutableProperty(() => 'slider')
  @LiveProperty<TouchBarSlider>(config => config.label)
  @LiveProperty<TouchBarSlider>(config => config.minValue)
    minValue!: number;
  @LiveProperty<TouchBarSlider>(config => config.maxValue)
    maxValue!: number;
  @LiveProperty<TouchBarSlider>(config => config.value)
    value!: number;
  @ImmutableProperty<TouchBarSlider>(({ change: onChange }, setInternalProp) => typeof onChange === 'function'
    ? (details: { value: number }) => {
        setInternalProp('value', details.value);
        onChange(details.value);
class TouchBarSpacer extends TouchBarItem<Electron.TouchBarSpacerConstructorOptions> implements Electron.TouchBarSpacer {
  @ImmutableProperty(() => 'spacer')
  @ImmutableProperty<TouchBarSpacer>(config => config.size)
    size!: Electron.TouchBarSpacer['size'];
class TouchBarSegmentedControl extends TouchBarItem<Electron.TouchBarSegmentedControlConstructorOptions> implements Electron.TouchBarSegmentedControl {
  @ImmutableProperty(() => 'segmented_control')
  @LiveProperty<TouchBarSegmentedControl>(config => config.segmentStyle)
    segmentStyle!: Electron.TouchBarSegmentedControl['segmentStyle'];
  @LiveProperty<TouchBarSegmentedControl>(config => config.segments || [])
    segments!: Electron.SegmentedControlSegment[];
  @LiveProperty<TouchBarSegmentedControl>(config => config.selectedIndex)
    selectedIndex!: number;
  @LiveProperty<TouchBarSegmentedControl>(config => config.mode)
    mode!: Electron.TouchBarSegmentedControl['mode'];
  @ImmutableProperty<TouchBarSegmentedControl>(({ change: onChange }, setInternalProp) => typeof onChange === 'function'
    ? (details: { selectedIndex: number, isSelected: boolean }) => {
        setInternalProp('selectedIndex', details.selectedIndex);
        onChange(details.selectedIndex, details.isSelected);
class TouchBarScrubber extends TouchBarItem<Electron.TouchBarScrubberConstructorOptions> implements Electron.TouchBarScrubber {
  @ImmutableProperty(() => 'scrubber')
  @LiveProperty<TouchBarScrubber>(config => config.items)
    items!: Electron.ScrubberItem[];
  @LiveProperty<TouchBarScrubber>(config => config.selectedStyle || null)
    selectedStyle!: Electron.TouchBarScrubber['selectedStyle'];
  @LiveProperty<TouchBarScrubber>(config => config.overlayStyle || null)
    overlayStyle!: Electron.TouchBarScrubber['overlayStyle'];
  @LiveProperty<TouchBarScrubber>(config => config.showArrowButtons || false)
    showArrowButtons!: boolean;
  @LiveProperty<TouchBarScrubber>(config => config.mode || 'free')
    mode!: Electron.TouchBarScrubber['mode'];
  @LiveProperty<TouchBarScrubber>(config => typeof config.continuous === 'undefined' ? true : config.continuous)
    continuous!: boolean;
  @ImmutableProperty<TouchBarScrubber>(({ select: onSelect, highlight: onHighlight }) => typeof onSelect === 'function' || typeof onHighlight === 'function'
    ? (details: { type: 'select'; selectedIndex: number } | { type: 'highlight'; highlightedIndex: number }) => {
        if (details.type === 'select') {
          if (onSelect) onSelect(details.selectedIndex);
          if (onHighlight) onHighlight(details.highlightedIndex);
class TouchBarOtherItemsProxy extends TouchBarItem<null> implements Electron.TouchBarOtherItemsProxy {
  @ImmutableProperty(() => 'other_items_proxy') type!: string;
const escapeItemSymbol = Symbol('escape item');
class TouchBar extends EventEmitter implements Electron.TouchBar {
  // Bind a touch bar to a window
  static _setOnWindow (touchBar: TouchBar | Electron.TouchBarConstructorOptions['items'], window: Electron.BaseWindow) {
    if (window._touchBar != null) {
      window._touchBar._removeFromWindow(window);
    if (!touchBar) {
      window._setTouchBarItems([]);
    if (Array.isArray(touchBar)) {
      touchBar = new TouchBar({ items: touchBar });
    touchBar._addToWindow(window);
  private windowListeners = new Map<number, Function>();
  private items = new Map<string, TouchBarItem<any>>();
  orderedItems: TouchBarItem<any>[] = [];
  constructor (options: Electron.TouchBarConstructorOptions) {
      throw new Error('Must specify options object as first argument');
    let { items, escapeItem } = options;
      items = [];
    this.escapeItem = (escapeItem as any) || null;
    const registerItem = (item: TouchBarItem<any>) => {
      this.items.set(item.id, item);
      item.on('change', this.changeListener);
      if (item.child instanceof TouchBar) {
        for (const child of item.child.orderedItems) {
          registerItem(child);
    let hasOtherItemsProxy = false;
    const idSet = new Set();
      if (!(item instanceof TouchBarItem)) {
        throw new TypeError('Each item must be an instance of TouchBarItem');
      if (item.type === 'other_items_proxy') {
        if (!hasOtherItemsProxy) {
          hasOtherItemsProxy = true;
          throw new Error('Must only have one OtherItemsProxy per TouchBar');
      if (!idSet.has(item.id)) {
        idSet.add(item.id);
        throw new Error('Cannot add a single instance of TouchBarItem multiple times in a TouchBar');
    // register in separate loop after all items are validated
    for (const item of (items as TouchBarItem<any>[])) {
      this.orderedItems.push(item);
      registerItem(item);
  private changeListener = (item: TouchBarItem<any>) => {
    this.emit('change', item.id, item.type);
  private [escapeItemSymbol]: TouchBarItem<unknown> | null = null;
  set escapeItem (item: TouchBarItem<unknown> | null) {
    if (item != null && !(item instanceof TouchBarItem)) {
      throw new Error('Escape item must be an instance of TouchBarItem');
    const escapeItem = this.escapeItem;
    if (escapeItem) {
      escapeItem.removeListener('change', this.changeListener);
    this[escapeItemSymbol] = item;
    if (this.escapeItem != null) {
      this.escapeItem.on('change', this.changeListener);
    this.emit('escape-item-change', item);
  get escapeItem (): TouchBarItem<unknown> | null {
    return this[escapeItemSymbol];
  _addToWindow (window: Electron.BaseWindow) {
    const { id } = window;
    // Already added to window
    if (this.windowListeners.has(id)) return;
    window._touchBar = this;
    const changeListener = (itemID: string) => {
      window._refreshTouchBarItem(itemID);
    this.on('change', changeListener);
    const escapeItemListener = (item: Electron.TouchBarItemType | null) => {
      window._setEscapeTouchBarItem(item ?? {});
    this.on('escape-item-change', escapeItemListener);
    const interactionListener = (_: any, itemID: string, details: any) => {
      let item = this.items.get(itemID);
      if (item == null && this.escapeItem != null && this.escapeItem.id === itemID) {
        item = this.escapeItem;
      if (item != null && item.onInteraction != null) {
        item.onInteraction(details);
    window.on('-touch-bar-interaction', interactionListener);
    const removeListeners = () => {
      this.removeListener('change', changeListener);
      this.removeListener('escape-item-change', escapeItemListener);
      window.removeListener('-touch-bar-interaction', interactionListener);
      window.removeListener('closed', removeListeners);
      window._touchBar = null;
      this.windowListeners.delete(id);
      const unregisterItems = (items: TouchBarItem<any>[]) => {
          item.removeListener('change', this.changeListener);
            unregisterItems(item.child.orderedItems);
      unregisterItems(this.orderedItems);
      if (this.escapeItem) {
        this.escapeItem.removeListener('change', this.changeListener);
    window.once('closed', removeListeners);
    this.windowListeners.set(id, removeListeners);
    window._setTouchBarItems(this.orderedItems);
    escapeItemListener(this.escapeItem);
  _removeFromWindow (window: Electron.BaseWindow) {
    const removeListeners = this.windowListeners.get(window.id);
    if (removeListeners != null) removeListeners();
  static TouchBarButton = TouchBarButton;
  static TouchBarColorPicker = TouchBarColorPicker;
  static TouchBarGroup = TouchBarGroup;
  static TouchBarLabel = TouchBarLabel;
  static TouchBarPopover = TouchBarPopover;
  static TouchBarSlider = TouchBarSlider;
  static TouchBarSpacer = TouchBarSpacer;
  static TouchBarSegmentedControl = TouchBarSegmentedControl;
  static TouchBarScrubber = TouchBarScrubber;
  static TouchBarOtherItemsProxy = TouchBarOtherItemsProxy;
export default TouchBar;
