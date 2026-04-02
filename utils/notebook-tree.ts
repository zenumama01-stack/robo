import { TabBarSvg } from '@jupyterlab/ui-components';
import { TabPanel } from '@lumino/widgets';
import { INotebookTree } from './token';
 * The widget added in main area of the tree view.
export class NotebookTreeWidget extends TabPanel implements INotebookTree {
   * Constructor of the NotebookTreeWidget.
    super({
      tabPlacement: 'top',
      renderer: TabBarSvg.defaultRenderer,
    this.addClass('jp-TreePanel');
