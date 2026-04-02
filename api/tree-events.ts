 * Tree Event System for @memberjunction/ng-trees
 * Follows the Before/After cancelable event pattern from entity-data-grid.
 * Before events can be canceled, After events are informational.
import { TreeNode, TreeBranchConfig, TreeLeafConfig } from '../models/tree-types';
export type TreeComponentRef = unknown;
 * Base class for all tree events
export class TreeEventArgs {
    /** The tree component that raised the event */
    readonly Tree: TreeComponentRef;
    readonly Timestamp: Date;
    constructor(tree: TreeComponentRef) {
        this.Tree = tree;
        this.Timestamp = new Date();
export class CancelableTreeEventArgs extends TreeEventArgs {
    Cancel: boolean = false;
    CancelReason?: string;
        super(tree);
 * Base class for node-related events
export class NodeEventArgs extends TreeEventArgs {
    /** The node involved in the event */
    readonly Node: TreeNode;
    constructor(tree: TreeComponentRef, node: TreeNode) {
        this.Node = node;
 * Base class for cancelable node events
export class CancelableNodeEventArgs extends NodeEventArgs {
        super(tree, node);
// Node Selection Events
 * Fired before a node is selected - can be canceled
export class BeforeNodeSelectEventArgs extends CancelableNodeEventArgs {
    readonly IsAdditive: boolean;
    readonly CurrentSelection: TreeNode[];
        tree: TreeComponentRef,
        node: TreeNode,
        currentSelection: TreeNode[]
        this.IsAdditive = isAdditive;
        this.CurrentSelection = [...currentSelection];
 * Fired after a node is selected
export class AfterNodeSelectEventArgs extends NodeEventArgs {
    readonly WasAdditive: boolean;
    readonly NewSelection: TreeNode[];
    readonly PreviousSelection: TreeNode[];
        newSelection: TreeNode[],
        previousSelection: TreeNode[]
        this.WasAdditive = wasAdditive;
        this.NewSelection = [...newSelection];
        this.PreviousSelection = [...previousSelection];
 * Fired before a node is deselected - can be canceled
export class BeforeNodeDeselectEventArgs extends CancelableNodeEventArgs {
 * Fired after a node is deselected
export class AfterNodeDeselectEventArgs extends NodeEventArgs {
// Node Expand/Collapse Events
 * Fired before a node is expanded - can be canceled
export class BeforeNodeExpandEventArgs extends CancelableNodeEventArgs {
 * Fired after a node is expanded
export class AfterNodeExpandEventArgs extends NodeEventArgs {
    /** Number of visible children after expansion */
    readonly ChildCount: number;
    constructor(tree: TreeComponentRef, node: TreeNode, childCount: number) {
        this.ChildCount = childCount;
 * Fired before a node is collapsed - can be canceled
export class BeforeNodeCollapseEventArgs extends CancelableNodeEventArgs {
 * Fired after a node is collapsed
export class AfterNodeCollapseEventArgs extends NodeEventArgs {
// Node Click Events
 * Fired before a node click is processed - can be canceled
export class BeforeNodeClickEventArgs extends CancelableNodeEventArgs {
    readonly MouseEvent: MouseEvent;
    constructor(tree: TreeComponentRef, node: TreeNode, mouseEvent: MouseEvent) {
        this.MouseEvent = mouseEvent;
 * Fired after a node click
export class AfterNodeClickEventArgs extends NodeEventArgs {
 * Fired before a node double-click - can be canceled
export class BeforeNodeDoubleClickEventArgs extends CancelableNodeEventArgs {
 * Fired after a node double-click
export class AfterNodeDoubleClickEventArgs extends NodeEventArgs {
 * Fired before data load - can be canceled or configs modified
export class BeforeDataLoadEventArgs extends CancelableTreeEventArgs {
    /** The branch configuration that will be used */
    readonly BranchConfig: TreeBranchConfig;
    /** The leaf configuration that will be used (if any) */
    readonly LeafConfig?: TreeLeafConfig;
    /** Set to modify the extra filter for branches */
    ModifiedBranchFilter?: string;
    /** Set to modify the extra filter for leaves */
    ModifiedLeafFilter?: string;
        branchConfig: TreeBranchConfig,
        leafConfig?: TreeLeafConfig
        this.BranchConfig = { ...branchConfig };
        this.LeafConfig = leafConfig ? { ...leafConfig } : undefined;
export class AfterDataLoadEventArgs extends TreeEventArgs {
    readonly Success: boolean;
    /** Total number of branch nodes loaded */
    readonly BranchCount: number;
    /** Total number of leaf nodes loaded */
    readonly LeafCount: number;
    /** Total number of all nodes */
    readonly TotalNodes: number;
    readonly LoadTimeMs: number;
    readonly Error?: string;
        branchCount: number,
        leafCount: number,
        this.BranchCount = branchCount;
        this.LeafCount = leafCount;
        this.TotalNodes = branchCount + leafCount;
        this.LoadTimeMs = loadTimeMs;
// Search Events
 * Fired before search is applied - can be canceled or modified
export class BeforeSearchEventArgs extends CancelableTreeEventArgs {
    /** The search text entered */
    readonly SearchText: string;
    /** Set to modify the search text */
    ModifiedSearchText?: string;
    constructor(tree: TreeComponentRef, searchText: string) {
        this.SearchText = searchText;
 * Fired after search is applied
export class AfterSearchEventArgs extends TreeEventArgs {
    /** The search text that was used */
    /** Number of nodes matching the search */
    readonly MatchCount: number;
    /** Number of branch nodes matching */
    readonly BranchMatchCount: number;
    /** Number of leaf nodes matching */
    readonly LeafMatchCount: number;
    /** The matching nodes */
    readonly MatchedNodes: TreeNode[];
        searchText: string,
        matchedNodes: TreeNode[]
        this.MatchedNodes = [...matchedNodes];
        this.MatchCount = matchedNodes.length;
        this.BranchMatchCount = matchedNodes.filter(n => n.Type === 'branch').length;
        this.LeafMatchCount = matchedNodes.filter(n => n.Type === 'leaf').length;
 * Fired when search is cleared
export class SearchClearedEventArgs extends TreeEventArgs {
    /** The previous search text */
    readonly PreviousSearchText: string;
    constructor(tree: TreeComponentRef, previousSearchText: string) {
        this.PreviousSearchText = previousSearchText;
// Dropdown Events
 * Fired before dropdown opens - can be canceled
export class BeforeDropdownOpenEventArgs extends CancelableTreeEventArgs {
 * Fired after dropdown opens
export class AfterDropdownOpenEventArgs extends TreeEventArgs {
    /** The position where dropdown was rendered */
    readonly Position: 'above' | 'below';
    constructor(tree: TreeComponentRef, position: 'above' | 'below') {
 * Fired before dropdown closes - can be canceled
export class BeforeDropdownCloseEventArgs extends CancelableTreeEventArgs {
    /** Reason for closing */
    readonly Reason: 'selection' | 'escape' | 'outsideClick' | 'programmatic';
    constructor(tree: TreeComponentRef, reason: 'selection' | 'escape' | 'outsideClick' | 'programmatic') {
        this.Reason = reason;
 * Fired after dropdown closes
export class AfterDropdownCloseEventArgs extends TreeEventArgs {
    /** Reason dropdown was closed */
// Keyboard Navigation Events
 * Fired before keyboard navigation - can be canceled
export class BeforeKeyboardNavigateEventArgs extends CancelableTreeEventArgs {
    /** The keyboard event */
    readonly KeyboardEvent: KeyboardEvent;
    /** The key that was pressed */
    readonly Key: string;
    /** The currently focused node (if any) */
    readonly CurrentNode: TreeNode | null;
    /** The node that will be focused */
    readonly TargetNode: TreeNode | null;
        keyboardEvent: KeyboardEvent,
        currentNode: TreeNode | null,
        targetNode: TreeNode | null
        this.KeyboardEvent = keyboardEvent;
        this.Key = keyboardEvent.key;
        this.CurrentNode = currentNode;
        this.TargetNode = targetNode;
 * Fired after keyboard navigation
export class AfterKeyboardNavigateEventArgs extends TreeEventArgs {
    /** The previously focused node */
    readonly PreviousNode: TreeNode | null;
    /** The newly focused node */
        previousNode: TreeNode | null,
        currentNode: TreeNode | null
        this.PreviousNode = previousNode;
