import * as d3Force from 'd3-force';
    GraphNode,
    GraphEdge,
    NetworkLayoutType,
    PhysicsParams,
    DecisionNode,
    NodeShape,
 * Action that generates SVG network graphs including force-directed layouts, decision trees, and radial networks.
 * This action provides server-side SVG generation for network visualizations, designed for
 * AI agents and workflows to create publication-quality graphs from structured data.
 * // Force-directed network example
 *   ActionName: 'Create SVG Network',
 *     { Name: 'NetworkType', Value: 'force' },
 *       { id: '1', label: 'Node A', group: 'A' },
 *       { id: '2', label: 'Node B', group: 'B' },
 *       { id: '3', label: 'Node C', group: 'A' }
 *       { source: '1', target: '2', weight: 1, directed: true },
 *       { source: '2', target: '3', weight: 0.5 }
 *     { Name: 'ShowLabels', Value: 'true' },
 *     { Name: 'ShowLegend', Value: 'true' }
 * // Decision tree example
 *     { Name: 'NetworkType', Value: 'tree' },
 *       id: 'root',
 *       label: 'Decision',
 *         { id: '1', label: 'Option A', value: 0.6 },
 *         { id: '2', label: 'Option B', value: 0.4 }
@RegisterClass(BaseAction, '__CreateSVGNetwork')
export class CreateSVGNetworkAction extends BaseAction {
     * Generates an SVG network graph from the provided data and configuration
     *   - NetworkType: Type of network ('force' | 'tree' | 'radial')
     *   - Edges: JSON array of edges (for force and radial)
     *   - Physics: Physics parameters (charge, linkDistance, iterations)
     *   - ShowLabels: Show node labels (default: true)
     *   - ShowLegend: Show group legend (default: false)
     *   - NodeShape: Shape for tree nodes ('rect' | 'circle' | 'pill')
     *   - Width: Width in pixels (optional, default: 800)
     *   - Height: Height in pixels (optional, default: 600)
     *   - Title: Network title (optional)
     *   - Palette: Color palette name (optional)
            const networkTypeParam = this.getParamValue(params, 'NetworkType');
            if (!networkTypeParam) {
                    Message: 'NetworkType parameter is required. Must be "force", "tree", or "radial"',
            const networkType = this.ensureString(networkTypeParam, 'NetworkType').toLowerCase();
            if (!['force', 'tree', 'radial'].includes(networkType)) {
                    Message: 'NetworkType must be "force", "tree", or "radial"',
                    ResultCode: 'INVALID_NETWORK_TYPE',
            const seed = parseInt(this.ensureString(this.getParamValue(params, 'Seed') || String(Date.now()), 'Seed'));
            const showLabelsParam = this.getParamValue(params, 'ShowLabels');
            const showLabels = showLabelsParam ? this.ensureString(showLabelsParam, 'ShowLabels').toLowerCase() !== 'false' : true;
            // Parse interactivity parameters
            const enableTooltipsParam = this.getParamValue(params, 'EnableTooltips');
            const enableTooltips = enableTooltipsParam ? this.ensureString(enableTooltipsParam, 'EnableTooltips').toLowerCase() === 'true' : false;
            const enablePanZoomParam = this.getParamValue(params, 'EnablePanZoom');
            const enablePanZoom = enablePanZoomParam ? this.ensureString(enablePanZoomParam, 'EnablePanZoom').toLowerCase() === 'true' : false;
            const showZoomControlsParam = this.getParamValue(params, 'ShowZoomControls');
            const showZoomControls = showZoomControlsParam ? this.ensureString(showZoomControlsParam, 'ShowZoomControls').toLowerCase() === 'true' : false;
            const wrapWithContainerParam = this.getParamValue(params, 'WrapWithContainer');
            const wrapWithContainer = wrapWithContainerParam ? this.ensureString(wrapWithContainerParam, 'WrapWithContainer').toLowerCase() === 'true' : false;
            const maxContainerWidth = parseInt(this.ensureString(this.getParamValue(params, 'MaxContainerWidth') || '1200', 'MaxContainerWidth'));
            const maxContainerHeight = parseInt(this.ensureString(this.getParamValue(params, 'MaxContainerHeight') || '800', 'MaxContainerHeight'));
            // Generate network based on type
            switch (networkType) {
                case 'force':
                    svg = await this.renderForceNetwork(params, viewBox, branding, title, seed, showLabels, showLegend, enableTooltips, enablePanZoom, showZoomControls, warnings);
                case 'tree':
                    svg = await this.renderDecisionTree(params, viewBox, branding, title, showLabels, enableTooltips, enablePanZoom, showZoomControls);
                case 'radial':
                    svg = await this.renderRadialNetwork(params, viewBox, branding, title, seed, showLabels, enableTooltips, enablePanZoom, showZoomControls, warnings);
                        Message: `Unsupported network type: ${networkType}`,
            // Apply scroll container wrapping if requested
            if (wrapWithContainer) {
                svg = SVGUtils.wrapWithScrollContainer(svg, maxContainerWidth, maxContainerHeight);
                warnings: warnings.length > 0 ? warnings : undefined,
                Message: `Failed to generate network: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: 'NETWORK_GENERATION_FAILED',
     * Renders a force-directed network using d3-force
    private async renderForceNetwork(
        seed: number,
        showLabels: boolean,
        showLegend: boolean,
        enableTooltips: boolean,
        enablePanZoom: boolean,
        showZoomControls: boolean,
        warnings: string[]
            throw new Error('Nodes parameter is required for force networks');
        const nodes: GraphNode[] = this.parseJSON<GraphNode[]>(nodesParam, 'Nodes');
        const edges: GraphEdge[] = edgesParam ? this.parseJSON<GraphEdge[]>(edgesParam, 'Edges') : [];
        // Parse physics parameters
        const physicsParam = this.getParamValue(params, 'Physics');
        const physics: PhysicsParams = physicsParam ? this.parseJSON<PhysicsParams>(physicsParam, 'Physics') : {};
        const charge = physics.charge || -300;
        const linkDistance = physics.linkDistance || 100;
        const iterations = physics.iterations || 300;
        // Warn if graph is too large
        if (nodes.length > 500) {
            warnings.push(`Large graph with ${nodes.length} nodes may have performance issues`);
        // Create seeded random generator
        const random = SVGUtils.seededRandom(seed);
        // Initialize node positions randomly
        const simNodes = nodes.map((n) => ({
            ...n,
            x: vb.x + random() * vb.contentWidth,
            y: vb.y + random() * vb.contentHeight,
            vx: 0,
            vy: 0,
        const simulation = d3Force
            .forceSimulation(simNodes)
            .force(
                'link',
                d3Force
                    .forceLink(edges)
                    .id((d: any) => d.id)
                    .distance(linkDistance)
            .force('charge', d3Force.forceManyBody().strength(charge))
            .force('center', d3Force.forceCenter(vb.x + vb.contentWidth / 2, vb.y + vb.contentHeight / 2))
            .force('collide', d3Force.forceCollide(20))
            .stop();
        // Run simulation headless
        for (let i = 0; i < iterations; i++) {
            simulation.tick();
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'force-network');
        // Build group-to-color mapping
        const groups = [...new Set(nodes.map((n) => n.group).filter((g): g is string => g != null))];
        const groupColorMap = new Map(groups.map((g, i) => [g, getColorForIndex(i, branding.palette)]));
        // Wrap content in pan/zoom container if needed
        const contentContainer = doc.createElementNS(ns, 'g');
        contentContainer.setAttribute('id', enablePanZoom ? 'pan-zoom-container' : 'content-container');
        svg.appendChild(contentContainer);
        const edgesGroup = doc.createElementNS(ns, 'g');
        edgesGroup.setAttribute('id', 'edges');
        contentContainer.appendChild(edgesGroup);
        for (const edge of edges as any[]) {
            const sourceSim = simNodes.find((n) => n.id === edge.source.id || n.id === edge.source);
            const targetSim = simNodes.find((n) => n.id === edge.target.id || n.id === edge.target);
            if (!sourceSim || !targetSim) continue;
            line.setAttribute('x1', String(sourceSim.x));
            line.setAttribute('y1', String(sourceSim.y));
            line.setAttribute('x2', String(targetSim.x));
            line.setAttribute('y2', String(targetSim.y));
            line.setAttribute('stroke-width', String((edge.weight || 1) * 2));
            line.setAttribute('opacity', '0.6');
            if (edge.directed) {
                const markerId = SVGUtils.addArrowMarker(svg, 'arrow-force', palette.foreground);
                line.setAttribute('marker-end', markerId);
            edgesGroup.appendChild(line);
        const nodesGroup = doc.createElementNS(ns, 'g');
        nodesGroup.setAttribute('id', 'nodes');
        contentContainer.appendChild(nodesGroup);
        for (const node of simNodes) {
            g.setAttribute('data-node-id', node.id);
            // Add tooltip data if enabled
            if (enableTooltips) {
                const tooltipText = `${node.label || node.id}${node.group ? ' (' + node.group + ')' : ''}`;
                g.setAttribute('data-tooltip', tooltipText);
            const radius = node.size || 10;
            const color = node.group ? groupColorMap.get(node.group) || palette.categorical[0] : palette.categorical[0];
            circle.setAttribute('r', String(radius));
            circle.setAttribute('fill', color);
            circle.setAttribute('stroke', '#FFF');
            circle.setAttribute('class', 'node-circle');
            g.appendChild(circle);
            if (showLabels && node.label) {
                text.setAttribute('y', String(radius + 15));
            nodesGroup.appendChild(g);
        if (showLegend && groups.length > 0) {
            this.addLegend(doc, svg, groups, groupColorMap, vb, getFontSpec(branding.font));
        // Add interactivity features
            SVGUtils.addTooltipSupport(svg, doc);
        if (enablePanZoom) {
            const panZoomScript = doc.createElementNS(ns, 'script');
            panZoomScript.setAttribute('type', 'text/javascript');
            panZoomScript.textContent = SVGUtils.generatePanZoomScript('pan-zoom-container', 0.5, 3, showZoomControls).replace(/<script[^>]*>|<\/script>/gi, '');
            svg.appendChild(panZoomScript);
     * Renders a decision tree using d3-hierarchy
    private async renderDecisionTree(
        showZoomControls: boolean
        // Parse tree
            throw new Error('Nodes parameter is required for decision trees');
        const rootNode: DecisionNode = this.parseJSON<DecisionNode>(nodesParam, 'Nodes');
        const nodeShapeParam = this.getParamValue(params, 'NodeShape');
        const nodeShape = (nodeShapeParam ? this.ensureString(nodeShapeParam, 'NodeShape') : 'rect') as NodeShape;
        const treeLayout = d3Hierarchy.tree<DecisionNode>().size([vb.contentWidth, vb.contentHeight]);
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'decision-tree');
        const container = doc.createElementNS(ns, 'g');
        // Render links
            // Diagonal path
            const d = `M ${link.source.x},${link.source.y}
                       C ${link.source.x},${(link.source.y + link.target.y) / 2}
                         ${link.target.x},${(link.source.y + link.target.y) / 2}
                         ${link.target.x},${link.target.y}`;
            // Render shape based on nodeShape
            switch (nodeShape) {
                case 'circle':
                    circle.setAttribute('r', '25');
                    circle.setAttribute('fill', fillColor);
                    circle.setAttribute('stroke', '#333');
                case 'pill':
                    const pill = doc.createElementNS(ns, 'path');
                    pill.setAttribute('d', SVGUtils.roundedRectPath(-40, -20, 80, 40, 20));
                    pill.setAttribute('fill', fillColor);
                    pill.setAttribute('stroke', '#333');
                    pill.setAttribute('stroke-width', '2');
                    g.appendChild(pill);
                default: // rect
                    rect.setAttribute('x', '-30');
                    rect.setAttribute('y', '-20');
                    rect.setAttribute('width', '60');
                    rect.setAttribute('height', '40');
                    rect.setAttribute('rx', '5');
            if (showLabels) {
                labelText.setAttribute('y', node.data.value != null ? '-5' : '0');
                labelText.setAttribute('dominant-baseline', 'middle');
                labelText.setAttribute('font-size', '11');
                labelText.setAttribute('font-weight', 'bold');
                labelText.setAttribute('fill', '#FFF');
                labelText.textContent = node.data.label;
                g.appendChild(labelText);
                // Add value if present
                if (node.data.value != null) {
                    valueText.setAttribute('y', '8');
                    valueText.setAttribute('font-size', '9');
                    valueText.setAttribute('fill', '#FFF');
                    valueText.setAttribute('opacity', '0.9');
                    valueText.textContent = String(node.data.value);
                    g.appendChild(valueText);
     * Renders a radial network using d3-force with radial constraint
    private async renderRadialNetwork(
            throw new Error('Nodes parameter is required for radial networks');
        // Identify central node (first node or node with most connections)
        const degreeMap = new Map<string, number>();
            degreeMap.set(edge.source, (degreeMap.get(edge.source) || 0) + 1);
            degreeMap.set(edge.target, (degreeMap.get(edge.target) || 0) + 1);
        const centralNodeId = nodes.length > 0 ? [...degreeMap.entries()].sort((a, b) => b[1] - a[1])[0]?.[0] || nodes[0].id : nodes[0].id;
        // Initialize node positions
        const simNodes = nodes.map((n, i) => ({
            x: centerX + (random() - 0.5) * 100,
            y: centerY + (random() - 0.5) * 100,
            isCentral: n.id === centralNodeId,
        // Create force simulation with radial force
                    .distance(80)
            .force('charge', d3Force.forceManyBody().strength(-200))
                'radial',
                d3Force.forceRadial<any>(
                    (d) => (d.isCentral ? 0 : 150),
                    centerX,
                    centerY
            .force('collide', d3Force.forceCollide(25))
        for (let i = 0; i < 300; i++) {
        // Create SVG (reuse force network rendering logic)
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'radial-network');
        svg.appendChild(edgesGroup);
            line.setAttribute('opacity', '0.4');
        svg.appendChild(nodesGroup);
            const radius = node.isCentral ? 20 : node.size || 12;
            const color = node.isCentral ? palette.categorical[4] : palette.categorical[0];
            circle.setAttribute('stroke-width', node.isCentral ? '3' : '2');
                text.setAttribute('font-size', node.isCentral ? '12' : '10');
                text.setAttribute('font-weight', node.isCentral ? 'bold' : 'normal');
     * Adds a legend to the SVG
        groups: string[],
        groupColorMap: Map<string, string>,
        vb: { width: number; height: number; x: number; y: number },
        font: { family: string; size: number }
        legendGroup.setAttribute('id', 'legend');
        legendGroup.setAttribute('transform', `translate(${vb.width - 150}, 50)`);
        groups.forEach((group, i) => {
            circle.setAttribute('cx', '10');
            circle.setAttribute('cy', '10');
            circle.setAttribute('r', '8');
            circle.setAttribute('fill', groupColorMap.get(group)!);
            text.setAttribute('x', '25');
            text.setAttribute('y', '15');
            text.setAttribute('font-size', String(font.size - 2));
            text.textContent = group;
