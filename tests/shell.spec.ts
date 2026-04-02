import { INotebookShell, NotebookShell } from '@jupyter-notebook/application';
import { Widget } from '@lumino/widgets';
describe('Shell for notebooks', () => {
  let shell: INotebookShell;
    shell = new NotebookShell();
    Widget.attach(shell, document.body);
    shell.dispose();
  describe('#constructor()', () => {
    it('should create a LabShell instance', () => {
      expect(shell).toBeInstanceOf(NotebookShell);
    it('should make some areas empty initially', () => {
      ['main', 'left', 'right', 'menu'].forEach((area) => {
        const widgets = Array.from(shell.widgets(area as INotebookShell.Area));
        expect(widgets.length).toEqual(0);
    it('should have the skip link widget in the top area initially', () => {
      const widgets = Array.from(shell.widgets('top'));
      expect(widgets.length).toEqual(1);
  describe('#widgets()', () => {
    it('should add widgets to main area', () => {
      const widget = new Widget();
      shell.add(widget, 'main');
      const widgets = Array.from(shell.widgets('main'));
      expect(widgets).toEqual([widget]);
    it('should be empty and console.error if area does not exist', () => {
      const spy = jest.spyOn(console, 'error');
      const jupyterFrontEndShell = shell as JupyterFrontEnd.IShell;
      expect(Array.from(jupyterFrontEndShell.widgets('fake'))).toHaveLength(0);
      expect(spy).toHaveBeenCalled();
  describe('#currentWidget', () => {
    it('should be the current widget in the shell main area', () => {
      expect(shell.currentWidget).toBe(null);
      widget.node.tabIndex = -1;
      widget.id = 'foo';
      expect(shell.currentWidget).toBe(widget);
  describe('#add(widget, "top")', () => {
    it('should add a widget to the top area', () => {
      shell.add(widget, 'top');
      expect(widgets.length).toBeGreaterThan(0);
    it('should accept options', () => {
      shell.add(widget, 'top', { rank: 10 });
  describe('#add(widget, "main")', () => {
    it('should add a widget to the main area', () => {
  describe('#add(widget, "left")', () => {
    it('should add a widget to the left area', () => {
      shell.add(widget, 'left');
      const widgets = Array.from(shell.widgets('left'));
  describe('#add(widget, "right")', () => {
    it('should add a widget to the right area', () => {
      shell.add(widget, 'right');
      const widgets = Array.from(shell.widgets('right'));
describe('Shell for tree view', () => {
    it('should add widgets to existing areas', () => {
    it('should throw an exception if a fake area does not exist', () => {
