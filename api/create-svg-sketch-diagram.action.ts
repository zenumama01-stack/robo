import rough from 'roughjs';
    ERAttribute,
 * Action that generates hand-drawn/sketch-style SVG diagrams using Rough.js.
 * Provides informal, whiteboard-style visualizations for brainstorming and presentations.
 * This action supports the same diagram types as Create SVG Diagram but renders them
 * with a sketchy, hand-drawn aesthetic using Rough.js.
 * // Sketch flowchart
 *   ActionName: 'Create SVG Sketch Diagram',
 *       { id: '2', kind: 'process', label: 'Process' },
 *       { from: '1', to: '2' },
 *     { Name: 'Roughness', Value: '1.5' },
 *     { Name: 'FillStyle', Value: 'cross-hatch' }
@RegisterClass(BaseAction, '__CreateSVGSketchDiagram')
export class CreateSVGSketchDiagramAction extends BaseAction {
     * Generates a hand-drawn style SVG diagram from the provided data and configuration
     *   - Roughness: Hand-drawn wobble intensity (0.5-3.0, default: 1.5)
     *   - FillStyle: Fill pattern ('solid' | 'hachure' | 'cross-hatch' | 'dots', default: 'hachure')
     *   - Bowing: Curve intensity for lines (0-5, default: 1)
     *   - EnableTooltips: Enable interactive tooltips (optional)
     *   - EnablePanZoom: Enable pan and zoom (optional)
     *   - WrapWithContainer: Wrap in scrollable container (optional)
            // Parse Rough.js parameters
            const roughness = parseFloat(this.ensureString(this.getParamValue(params, 'Roughness') || '1.5', 'Roughness'));
            const fillStyle = this.ensureString(this.getParamValue(params, 'FillStyle') || 'hachure', 'FillStyle');
            const bowing = parseFloat(this.ensureString(this.getParamValue(params, 'Bowing') || '1', 'Bowing'));
            // Rough.js options
            const roughOptions = {
                roughness,
                bowing,
                fillStyle: fillStyle as 'solid' | 'hachure' | 'cross-hatch' | 'dots',
                strokeWidth: 2,
                fillWeight: 0.5,
                    svg = await this.renderSketchFlow(params, viewBox, branding, title, roughOptions, enableTooltips, enablePanZoom, showZoomControls);
                    svg = await this.renderSketchOrg(params, viewBox, branding, title, roughOptions, enableTooltips, enablePanZoom, showZoomControls);
                    svg = await this.renderSketchER(params, viewBox, branding, title, roughOptions, enableTooltips, enablePanZoom, showZoomControls);
                Message: `Failed to generate sketch diagram: ${error instanceof Error ? error.message : String(error)}`,
     * Renders a sketch-style flowchart
    private async renderSketchFlow(
        roughOptions: any,
        const direction = (directionParam ? this.ensureString(directionParam, 'Direction') : 'TB') as 'TB' | 'LR' | 'RL' | 'BT';
        // Create dagre graph for layout
        // Add nodes with dimensions
        // Add edges
        // Run layout
        // Create SVG with JSDOM
        const dom = new JSDOM(`<svg xmlns="http://www.w3.org/2000/svg" width="${viewBox.width}" height="${viewBox.height}" viewBox="0 0 ${viewBox.width} ${viewBox.height}"></svg>`);
        const doc = dom.window.document;
        // Initialize Rough.js with SVG
        const rc = rough.svg(svg);
        // Create container
        // Render edges first (so they appear behind nodes)
            // Draw rough line through points
            if (edge.points && edge.points.length >= 2) {
                const pathPoints: [number, number][] = edge.points.map(p => [p.x, p.y]);
                const roughLine = rc.linearPath(pathPoints, {
                    ...roughOptions,
                    stroke: palette.foreground,
                    fill: 'none',
                    strokeWidth: edge.dashed ? 1.5 : 2,
                contentContainer.appendChild(roughLine);
                // Add edge label if present
                    contentContainer.appendChild(text);
            const g_elem = doc.createElementNS(ns, 'g');
            g_elem.setAttribute('data-node-id', node.id);
                g_elem.setAttribute('data-tooltip', node.label);
            // Draw shape based on kind
                    // Ellipse
                    const ellipse = rc.ellipse(node.x, node.y, node.width, node.height, {
                        fill: fillColor,
                        stroke: '#333',
                    g_elem.appendChild(ellipse);
                    // Diamond
                    const w = node.width / 2;
                    const h = node.height / 2;
                    const diamond = rc.polygon([
                        [node.x, node.y - h],
                        [node.x + w, node.y],
                        [node.x, node.y + h],
                        [node.x - w, node.y]
                    ], {
                    g_elem.appendChild(diamond);
                    // Rectangle
                    const rect = rc.rectangle(
                        node.x - node.width / 2,
                        node.y - node.height / 2,
                        node.width,
                        node.height,
                    g_elem.appendChild(rect);
            text.setAttribute('x', String(node.x));
            text.setAttribute('y', String(node.y));
            text.setAttribute('font-size', '12');
            g_elem.appendChild(text);
            contentContainer.appendChild(g_elem);
        // Add interactivity
     * Renders a sketch-style org chart
    private async renderSketchOrg(
        // Initialize Rough.js
        contentContainer.setAttribute('transform', `translate(${vb.x}, ${vb.y})`);
            const points: [number, number][] = [
                [link.source.x, link.source.y + 35],
                [link.source.x, (link.source.y + link.target.y) / 2],
                [link.target.x, (link.source.y + link.target.y) / 2],
                [link.target.x, link.target.y - 35],
            const roughPath = rc.linearPath(points, {
            contentContainer.appendChild(roughPath);
            g_elem.setAttribute('data-node-id', node.data.id);
                g_elem.setAttribute('data-tooltip', `${node.data.label}${node.data.role ? ' - ' + node.data.role : ''}`);
            // Draw sketchy box
            const box = rc.rectangle(
                node.x - boxWidth / 2,
                node.y - boxHeight / 2,
                boxWidth,
                boxHeight,
                    stroke: node.data.highlight ? '#FFD700' : '#333',
                    strokeWidth: node.data.highlight ? 3 : 2,
            g_elem.appendChild(box);
            nameText.setAttribute('x', String(node.x));
            nameText.setAttribute('y', String(node.y - (node.data.role ? 8 : 0)));
            g_elem.appendChild(nameText);
            // Add role
                roleText.setAttribute('x', String(node.x));
                roleText.setAttribute('y', String(node.y + 8));
                g_elem.appendChild(roleText);
     * Renders a sketch-style ER diagram
    private async renderSketchER(
            const height = 40 + table.attrs.length * 25;
            if (rel.points && rel.points.length >= 2) {
                const pathPoints: [number, number][] = rel.points.map(p => [p.x, p.y]);
                    strokeLineDash: [5, 5],
            g_elem.setAttribute('data-node-id', table.id);
                g_elem.setAttribute('data-tooltip', `${table.name} (${table.attrs.length} attributes)`);
            // Draw outer box
            const outerBox = rc.rectangle(
                table.x - table.width / 2,
                table.y - table.height / 2,
                table.width,
                table.height,
                    fill: palette.background,
            g_elem.appendChild(outerBox);
            // Draw header box
            const headerBox = rc.rectangle(
                40,
                    fill: palette.categorical[0],
            g_elem.appendChild(headerBox);
            // Add table name
            nameText.setAttribute('x', String(table.x));
            nameText.setAttribute('y', String(table.y - table.height / 2 + 25));
            nameText.textContent = table.name;
            let yOffset = table.y - table.height / 2 + 40;
            for (const attr of table.attrs) {
                attrText.setAttribute('x', String(table.x - table.width / 2 + 10));
                g_elem.appendChild(attrText);
     * Helper to ensure string type
            `Received ${typeof value}.`
     * Helper to get parameter value by name
