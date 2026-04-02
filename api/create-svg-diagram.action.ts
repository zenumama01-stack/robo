import dagre from '@dagrejs/dagre';
import * as d3Hierarchy from 'd3-hierarchy';
import { JSDOM } from 'jsdom';
    FlowNode,
    FlowEdge,
    FlowLayout,
    OrgNode,
    ERTable,
    ERRelation,
    SVGActionResult,
    ViewBox,
    Accessibility,
    Branding,
} from './shared/svg-types';
import { getPalette, generateCSS, getFontSpec } from './shared/svg-theming';
 * Action that generates SVG diagrams including flowcharts, org charts, and ER diagrams.
 * This action provides server-side SVG generation for structured diagrams, designed for
 * AI agents and workflows to create publication-quality visualizations from structured data.
 * // Flowchart example
 *   ActionName: 'Create SVG Diagram',
 *     { Name: 'DiagramType', Value: 'flow' },
 *     { Name: 'Nodes', Value: JSON.stringify([
 *       { id: '1', kind: 'start', label: 'Start' },
 *       { id: '2', kind: 'process', label: 'Process Data' },
 *       { id: '3', kind: 'end', label: 'End' }
 *     { Name: 'Edges', Value: JSON.stringify([
 *       { from: '1', to: '2', label: 'Begin' },
 *       { from: '2', to: '3' }
 *     { Name: 'Direction', Value: 'TB' },
 *     { Name: 'Width', Value: '800' },
 *     { Name: 'Height', Value: '600' }
 * // Org chart example
 *     { Name: 'DiagramType', Value: 'org' },
 *     { Name: 'Nodes', Value: JSON.stringify({
 *       id: '1',
 *       label: 'CEO',
 *       role: 'Chief Executive Officer',
 *       children: [
 *         { id: '2', label: 'CTO', role: 'Technology' },
 *         { id: '3', label: 'CFO', role: 'Finance' }
 *     }) }
@RegisterClass(BaseAction, '__CreateSVGDiagram')
export class CreateSVGDiagramAction extends BaseAction {
     * Generates an SVG diagram from the provided data and configuration
     *   - DiagramType: Type of diagram ('flow' | 'org' | 'er')
     *   - Nodes: JSON array or object of nodes
     *   - Edges: JSON array of edges (for flow and ER diagrams)
     *   - Direction: Layout direction ('TB' | 'LR' | 'RL' | 'BT') for flow diagrams
     *   - Width: Diagram width in pixels (optional, default: 800)
     *   - Height: Diagram height in pixels (optional, default: 600)
     *   - Title: Diagram title (optional)
     *   - Palette: Color palette ('mjDefault' | 'gray' | 'pastel' | 'highContrast')
     *   - Seed: Random seed for deterministic layouts (optional)
     * @returns A promise resolving to an SVGActionResult with:
     *   - svg: The SVG XML string
     *   - width: Diagram width in pixels
     *   - height: Diagram height in pixels
            const diagramTypeParam = this.getParamValue(params, 'DiagramType');
            if (!diagramTypeParam) {
                    Message: 'DiagramType parameter is required (flow, org, or er)',
                    ResultCode: 'MISSING_PARAMETERS',
            const diagramType = this.ensureString(diagramTypeParam, 'DiagramType').toLowerCase();
            const seed = parseInt(this.ensureString(this.getParamValue(params, 'Seed') || '0', 'Seed'));
                palette: { type: 'named', name: paletteName as any },
                padding: 40,
            // Generate diagram based on type
            switch (diagramType) {
                case 'flow':
                    svg = await this.renderFlowDiagram(params, viewBox, branding, title, seed);
                case 'org':
                    svg = await this.renderOrgChart(params, viewBox, branding, title);
                case 'er':
                    svg = await this.renderERDiagram(params, viewBox, branding, title, seed);
                        Message: `Unsupported diagram type: ${diagramType}. Supported types: flow, org, er`,
                        ResultCode: 'INVALID_DIAGRAM_TYPE',
                Message: `Failed to generate diagram: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: 'DIAGRAM_GENERATION_FAILED',
     * Renders a flowchart using dagre layout
    private async renderFlowDiagram(
        seed: number
        // Parse nodes and edges
        const nodesParam = this.getParamValue(params, 'Nodes');
        const edgesParam = this.getParamValue(params, 'Edges');
        if (!nodesParam) {
            throw new Error('Nodes parameter is required for flow diagrams');
        const nodes: FlowNode[] = this.parseJSON<FlowNode[]>(nodesParam, 'Nodes');
        const edges: FlowEdge[] = edgesParam ? this.parseJSON<FlowEdge[]>(edgesParam, 'Edges') : [];
        const directionParam = this.getParamValue(params, 'Direction');
        const direction = (directionParam ? this.ensureString(directionParam, 'Direction') : 'TB') as FlowLayout['direction'];
        // Create dagre graph
        const g = new dagre.graphlib.Graph();
        g.setGraph({
            rankdir: direction,
            nodesep: 50,
            ranksep: 80,
            marginx: 40,
            marginy: 40,
        g.setDefaultEdgeLabel(() => ({}));
        // Add nodes to graph with dimensions based on kind
        for (const node of nodes) {
            const dims = this.getNodeDimensions(node);
            g.setNode(node.id, {
                ...node,
                width: node.width || dims.width,
                height: node.height || dims.height,
        // Add edges to graph
        for (const edge of edges) {
            g.setEdge(edge.from, edge.to, edge);
        // Run dagre layout
        dagre.layout(g);
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'flow');
        // Add accessibility
            SVGUtils.addA11y(svg, {
                ariaRole: 'img',
        // Add styles
        const css = generateCSS(branding);
        SVGUtils.addStyles(svg, css);
        // Get palette for colors
        // Render nodes
        for (const nodeId of g.nodes()) {
            const node = g.node(nodeId) as FlowNode & { x: number; y: number; width: number; height: number };
            this.renderFlowNode(doc, svg, node, palette);
        // Render edges
        for (const edgeObj of g.edges()) {
            const edge = g.edge(edgeObj) as FlowEdge & { points: Array<{ x: number; y: number }> };
            this.renderFlowEdge(doc, svg, edge, palette);
        // Add title if present
            this.addTitle(doc, svg, title, vb.width, getFontSpec(branding.font));
        // Sanitize and return
     * Gets dimensions for flow node based on kind
    private getNodeDimensions(node: FlowNode): { width: number; height: number } {
        const labelWidth = SVGUtils.estimateTextWidth(node.label, 14);
        switch (node.kind) {
            case 'start':
            case 'end':
                return { width: 120, height: 60 };
            case 'decision':
                return { width: Math.max(160, labelWidth + 40), height: 100 };
            case 'process':
            case 'input':
            case 'output':
            case 'subprocess':
                return { width: Math.max(140, labelWidth + 40), height: 70 };
                return { width: 140, height: 70 };
     * Renders a single flow node
    private renderFlowNode(
        node: FlowNode & { x: number; y: number; width: number; height: number },
        palette: { foreground: string; categorical: string[] }
        g.setAttribute('id', node.id);
        g.setAttribute('transform', `translate(${node.x}, ${node.y})`);
        const colorIndex = this.getNodeColorIndex(node.kind);
        const fillColor = palette.categorical[colorIndex % palette.categorical.length];
        // Render shape based on kind
                this.renderEllipse(doc, g, node.width, node.height, fillColor);
                this.renderDiamond(doc, g, node.width, node.height, fillColor);
                this.renderRoundedRect(doc, g, node.width, node.height, fillColor, 10);
                // Add double border for subprocess
                const innerRect = doc.createElementNS(ns, 'path');
                innerRect.setAttribute('d', SVGUtils.roundedRectPath(-node.width / 2 + 5, -node.height / 2 + 5, node.width - 10, node.height - 10, 8));
                innerRect.setAttribute('fill', 'none');
                innerRect.setAttribute('stroke', palette.foreground);
                innerRect.setAttribute('stroke-width', '1.5');
                g.appendChild(innerRect);
                this.renderRoundedRect(doc, g, node.width, node.height, fillColor, 5);
        // Add label
        text.setAttribute('class', 'node-text');
        text.setAttribute('y', '0');
        text.textContent = node.label;
        svg.appendChild(g);
     * Gets color index for node kind
    private getNodeColorIndex(kind: string): number {
        const kindMap: Record<string, number> = {
            start: 0,
            end: 1,
            process: 2,
            decision: 3,
            input: 4,
            output: 5,
            subprocess: 6,
        return kindMap[kind] || 2;
     * Renders an ellipse shape
    private renderEllipse(doc: Document, g: Element, width: number, height: number, fill: string): void {
        const ns = g.namespaceURI!;
        const ellipse = doc.createElementNS(ns, 'ellipse');
        ellipse.setAttribute('cx', '0');
        ellipse.setAttribute('cy', '0');
        ellipse.setAttribute('rx', String(width / 2));
        ellipse.setAttribute('ry', String(height / 2));
        ellipse.setAttribute('fill', fill);
        ellipse.setAttribute('stroke', '#333');
        ellipse.setAttribute('stroke-width', '2');
        g.appendChild(ellipse);
     * Renders a diamond shape
    private renderDiamond(doc: Document, g: Element, width: number, height: number, fill: string): void {
        const w = width / 2;
        const h = height / 2;
        const d = `M 0,${-h} L ${w},0 L 0,${h} L ${-w},0 Z`;
        path.setAttribute('fill', fill);
        path.setAttribute('stroke', '#333');
        g.appendChild(path);
     * Renders a rounded rectangle
    private renderRoundedRect(doc: Document, g: Element, width: number, height: number, fill: string, radius: number): void {
        path.setAttribute('d', SVGUtils.roundedRectPath(-width / 2, -height / 2, width, height, radius));
     * Renders a flow edge
    private renderFlowEdge(
        edge: FlowEdge & { points: Array<{ x: number; y: number }> },
        palette: { foreground: string }
        // Create path from points
        const pathData = edge.points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x},${p.y}`).join(' ');
        path.setAttribute('d', pathData);
        path.setAttribute('class', 'edge-line');
        if (edge.dashed) {
            path.setAttribute('stroke-dasharray', '5,5');
        // Add arrow marker
        const markerId = SVGUtils.addArrowMarker(svg, 'arrow-end', palette.foreground);
        path.setAttribute('marker-end', markerId);
        // Add label if present
        if (edge.label) {
            const midPoint = edge.points[Math.floor(edge.points.length / 2)];
            text.setAttribute('x', String(midPoint.x));
            text.setAttribute('y', String(midPoint.y - 5));
            text.setAttribute('class', 'edge-label');
            text.textContent = edge.label;
     * Renders an org chart using d3-hierarchy
    private async renderOrgChart(params: RunActionParams, viewBox: ViewBox, branding: Branding, title: string): Promise<string> {
        // Parse org tree
            throw new Error('Nodes parameter is required for org charts');
        const rootNode: OrgNode = this.parseJSON<OrgNode>(nodesParam, 'Nodes');
        // Create hierarchy
        const hierarchy = d3Hierarchy.hierarchy(rootNode);
        // Create tree layout
        const treeLayout = d3Hierarchy.tree<OrgNode>().size([vb.contentWidth, vb.contentHeight]);
        // Calculate layout
        const treeData = treeLayout(hierarchy);
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'org');
        // Get palette
        // Create container group with offset for padding
        const container = doc.createElementNS(svg.namespaceURI!, 'g');
        container.setAttribute('transform', `translate(${vb.x}, ${vb.y})`);
        svg.appendChild(container);
        // Render links first (so they appear behind nodes)
        treeData.links().forEach((link) => {
            this.renderOrgLink(doc, container, link, palette);
        treeData.descendants().forEach((node) => {
            this.renderOrgNode(doc, container, node, palette);
     * Renders an org chart link
    private renderOrgLink(
        link: d3Hierarchy.HierarchyPointLink<OrgNode>,
        // Create elbow connector
        const d = `M ${link.source.x},${link.source.y + 35}
                   L ${link.source.x},${(link.source.y + link.target.y) / 2}
                   L ${link.target.x},${(link.source.y + link.target.y) / 2}
                   L ${link.target.x},${link.target.y - 35}`;
        path.setAttribute('fill', 'none');
        path.setAttribute('stroke', palette.foreground);
        path.setAttribute('opacity', '0.6');
        container.appendChild(path);
     * Renders an org chart node
    private renderOrgNode(
        node: d3Hierarchy.HierarchyPointNode<OrgNode>,
        const boxWidth = 180;
        const boxHeight = 70;
        // Determine color based on depth
        const colorIndex = node.depth % palette.categorical.length;
        const fillColor = palette.categorical[colorIndex];
        // Render box
        rect.setAttribute('x', String(-boxWidth / 2));
        rect.setAttribute('y', String(-boxHeight / 2));
        rect.setAttribute('width', String(boxWidth));
        rect.setAttribute('height', String(boxHeight));
        rect.setAttribute('fill', fillColor);
        rect.setAttribute('stroke', '#333');
        rect.setAttribute('stroke-width', '2');
        rect.setAttribute('rx', '8');
        if (node.data.highlight) {
            rect.setAttribute('stroke', '#FFD700');
            rect.setAttribute('stroke-width', '4');
        // Add avatar circle if avatarUrl present
        if (node.data.avatarUrl) {
            const avatar = doc.createElementNS(ns, 'circle');
            avatar.setAttribute('cx', '0');
            avatar.setAttribute('cy', String(-boxHeight / 2 + 20));
            avatar.setAttribute('r', '15');
            avatar.setAttribute('fill', '#FFF');
            avatar.setAttribute('stroke', '#333');
            g.appendChild(avatar);
        // Add name
        const nameText = doc.createElementNS(ns, 'text');
        nameText.setAttribute('y', node.data.avatarUrl ? '5' : '-8');
        nameText.setAttribute('text-anchor', 'middle');
        nameText.setAttribute('font-weight', 'bold');
        nameText.setAttribute('font-size', '14');
        nameText.setAttribute('fill', '#FFF');
        nameText.textContent = node.data.label;
        g.appendChild(nameText);
        // Add role if present
        if (node.data.role) {
            const roleText = doc.createElementNS(ns, 'text');
            roleText.setAttribute('y', node.data.avatarUrl ? '20' : '8');
            roleText.setAttribute('text-anchor', 'middle');
            roleText.setAttribute('font-size', '12');
            roleText.setAttribute('fill', '#FFF');
            roleText.setAttribute('opacity', '0.9');
            roleText.textContent = node.data.role;
            g.appendChild(roleText);
        container.appendChild(g);
     * Renders an ER diagram using dagre layout
    private async renderERDiagram(
        // Parse tables and relations
            throw new Error('Nodes parameter is required for ER diagrams');
        const tables: ERTable[] = this.parseJSON<ERTable[]>(nodesParam, 'Nodes');
        const relations: ERRelation[] = edgesParam ? this.parseJSON<ERRelation[]>(edgesParam, 'Edges') : [];
            rankdir: 'TB',
            nodesep: 60,
        // Add tables as nodes
        for (const table of tables) {
            const height = 40 + table.attrs.length * 25; // Header + rows
            const maxAttrWidth = Math.max(...table.attrs.map((a) => SVGUtils.estimateTextWidth(`${a.name}: ${a.type || ''}`, 12)));
            const width = Math.max(200, maxAttrWidth + 80);
            g.setNode(table.id, {
                ...table,
        // Add relations as edges
        for (const rel of relations) {
            g.setEdge(rel.from, rel.to, rel);
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'er');
        // Render tables
        for (const tableId of g.nodes()) {
            const table = g.node(tableId) as ERTable & { x: number; y: number; width: number; height: number };
            this.renderERTable(doc, svg, table, palette);
        // Render relations
            const rel = g.edge(edgeObj) as ERRelation & { points: Array<{ x: number; y: number }> };
            this.renderERRelation(doc, svg, rel, palette);
     * Renders an ER table
    private renderERTable(
        table: ERTable & { x: number; y: number; width: number; height: number },
        palette: { foreground: string; categorical: string[]; background: string }
        g.setAttribute('id', table.id);
        g.setAttribute('transform', `translate(${table.x - table.width / 2}, ${table.y - table.height / 2})`);
        // Render table box
        rect.setAttribute('x', '0');
        rect.setAttribute('y', '0');
        rect.setAttribute('width', String(table.width));
        rect.setAttribute('height', String(table.height));
        rect.setAttribute('fill', palette.background);
        rect.setAttribute('stroke', palette.foreground);
        // Render header
        const headerRect = doc.createElementNS(ns, 'rect');
        headerRect.setAttribute('x', '0');
        headerRect.setAttribute('y', '0');
        headerRect.setAttribute('width', String(table.width));
        headerRect.setAttribute('height', '40');
        headerRect.setAttribute('fill', palette.categorical[0]);
        g.appendChild(headerRect);
        // Header text
        const headerText = doc.createElementNS(ns, 'text');
        headerText.setAttribute('x', String(table.width / 2));
        headerText.setAttribute('y', '25');
        headerText.setAttribute('text-anchor', 'middle');
        headerText.setAttribute('font-weight', 'bold');
        headerText.setAttribute('font-size', '14');
        headerText.setAttribute('fill', '#FFF');
        headerText.textContent = table.name;
        g.appendChild(headerText);
        // Render attributes
        let yOffset = 40;
        for (let i = 0; i < table.attrs.length; i++) {
            const attr = table.attrs[i];
            // Alternating row colors
            if (i % 2 === 1) {
                const rowRect = doc.createElementNS(ns, 'rect');
                rowRect.setAttribute('x', '0');
                rowRect.setAttribute('y', String(yOffset));
                rowRect.setAttribute('width', String(table.width));
                rowRect.setAttribute('height', '25');
                rowRect.setAttribute('fill', '#F5F5F5');
                g.appendChild(rowRect);
            // Attribute text
            const attrText = doc.createElementNS(ns, 'text');
            attrText.setAttribute('x', '10');
            attrText.setAttribute('y', String(yOffset + 17));
            attrText.setAttribute('font-size', '12');
            attrText.setAttribute('fill', palette.foreground);
            let label = attr.name;
            if (attr.pk) label = '🔑 ' + label;
            else if (attr.fk) label = '🔗 ' + label;
            if (attr.type) label += `: ${attr.type}`;
            attrText.textContent = label;
            g.appendChild(attrText);
            yOffset += 25;
     * Renders an ER relation
    private renderERRelation(
        rel: ERRelation & { points: Array<{ x: number; y: number }> },
        const pathData = rel.points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x},${p.y}`).join(' ');
        // Add cardinality label if present
        if (rel.label) {
            const midPoint = rel.points[Math.floor(rel.points.length / 2)];
            text.setAttribute('font-size', '11');
            text.textContent = rel.label;
     * Adds a title to the SVG
    private addTitle(doc: Document, svg: SVGElement, title: string, width: number, font: { family: string; size: number }): void {
        text.setAttribute('y', '25');
        text.setAttribute('font-size', String(font.size + 4));
     * Helper to ensure a parameter value is a string, with type conversion and validation
        // Convert numbers and booleans to strings
        // For objects/arrays, reject with descriptive error
        const param = params.Params.find((p) => p.Name.trim().toLowerCase() === paramName.toLowerCase());
