import d3CloudModule from 'd3-cloud';
import { WordItem, CloudLayout, SVGActionResult, ViewBox, Branding } from './shared/svg-types';
// Handle d3-cloud module export
const d3Cloud = (d3CloudModule as any).default || d3CloudModule;
 * Action that generates SVG word clouds and tag bars from weighted word lists.
 * This action provides server-side SVG generation for text visualizations, designed for
 * AI agents and workflows to create publication-quality word clouds from data.
 * // Word cloud example
 *   ActionName: 'Create SVG Word Cloud',
 *     { Name: 'CloudType', Value: 'cloud' },
 *     { Name: 'Words', Value: JSON.stringify([
 *       { text: 'TypeScript', weight: 100 },
 *       { text: 'JavaScript', weight: 80 },
 *       { text: 'React', weight: 60 },
 *       { text: 'Node.js', weight: 50 }
 *     { Name: 'MaxWords', Value: '50' },
 *     { Name: 'Rotation', Value: 'few' },
 * // Tag bar example
 *     { Name: 'CloudType', Value: 'tagbar' },
 *       { text: 'AI', weight: 67 },
 *       { text: 'Data', weight: 45 },
 *       { text: 'API', weight: 32 }
 *     { Name: 'MaxWords', Value: '10' }
@RegisterClass(BaseAction, '__CreateSVGWordCloud')
export class CreateSVGWordCloudAction extends BaseAction {
     * Generates an SVG word cloud from the provided data and configuration
     *   - CloudType: Type of visualization ('cloud' | 'tagbar')
     *   - Words: JSON array of {text, weight} objects
     *   - MaxWords: Maximum number of words to display (default: 50)
     *   - Rotation: Rotation strategy ('none' | 'few' | 'mixed') for clouds
     *   - MinFont: Minimum font size (default: 10)
     *   - MaxFont: Maximum font size (default: 80)
     *   - Title: Visualization title (optional)
            const cloudTypeParam = this.getParamValue(params, 'CloudType');
            const cloudType = cloudTypeParam ? this.ensureString(cloudTypeParam, 'CloudType').toLowerCase() : 'cloud';
            if (!['cloud', 'tagbar'].includes(cloudType)) {
                    Message: 'CloudType must be "cloud" or "tagbar"',
                    ResultCode: 'INVALID_CLOUD_TYPE',
            // Parse words
            const wordsParam = this.getParamValue(params, 'Words');
            if (!wordsParam) {
                    Message: 'Words parameter is required',
            let words: WordItem[] = this.parseJSON<WordItem[]>(wordsParam, 'Words');
            const maxWords = parseInt(this.ensureString(this.getParamValue(params, 'MaxWords') || '50', 'MaxWords'));
            // Sort by weight descending and limit
            words = words.sort((a, b) => b.weight - a.weight).slice(0, maxWords);
            // Generate visualization based on type
            switch (cloudType) {
                case 'cloud':
                    svg = await this.renderWordCloud(params, words, viewBox, branding, title, seed, warnings);
                case 'tagbar':
                    svg = await this.renderTagBar(words, viewBox, branding, title);
                        Message: `Unsupported cloud type: ${cloudType}`,
                    wordCount: words.length,
                    requestedMaxWords: maxWords,
                Message: `Failed to generate word cloud: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: 'WORD_CLOUD_GENERATION_FAILED',
     * Renders a word cloud using d3-cloud
    private async renderWordCloud(
        words: WordItem[],
            // Parse cloud-specific parameters
            const rotation = this.getParamValue(params, 'Rotation') || 'few';
            const minFont = parseInt(this.getParamValue(params, 'MinFont') || '10');
            const maxFont = parseInt(this.getParamValue(params, 'MaxFont') || '80');
            // Calculate viewBox
            // Calculate font sizes using log scale
            const minWeight = Math.min(...words.map((w) => w.weight));
            const maxWeight = Math.max(...words.map((w) => w.weight));
            const logMin = Math.log(minWeight || 1);
            const logMax = Math.log(maxWeight || 1);
            const wordsWithSize = words.map((w) => {
                const logWeight = Math.log(w.weight || 1);
                const normalizedSize = logMax > logMin ? (logWeight - logMin) / (logMax - logMin) : 0.5;
                const fontSize = minFont + normalizedSize * (maxFont - minFont);
                    text: w.text,
                    size: fontSize,
                    weight: w.weight,
            // Determine rotation angles based on strategy
            const getRotation = (): number => {
                switch (rotation) {
                    case 'few':
                        return random() < 0.5 ? 0 : -90;
                    case 'mixed':
                        return Math.floor(random() * 4) * 45 - 90;
            // Create d3-cloud layout
            return new Promise<string>((resolve, reject) => {
                    const layout = (d3Cloud as any)()
                        .size([vb.contentWidth, vb.contentHeight])
                        .words(wordsWithSize)
                        .padding(5)
                        .rotate(getRotation)
                        .font(getFontSpec(branding.font).family)
                        .fontSize((d) => d.size)
                        .random(random)
                        .spiral('archimedean')
                        .on('end', (layoutWords) => {
                                // Check if any words didn't fit
                                const fittedWords = layoutWords.filter((w) => w.x != null && w.y != null);
                                if (fittedWords.length < wordsWithSize.length) {
                                    warnings.push(`${wordsWithSize.length - fittedWords.length} words did not fit in the layout`);
                                // Create SVG - STEP 1
                                let doc;
                                    doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'wordcloud');
                                    throw new Error(`[STEP 1: createSVG] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Get SVG element - STEP 2
                                let svg;
                                    svg = doc.querySelector('svg')!;
                                    if (!svg) {
                                        throw new Error('SVG element not found in document');
                                    throw new Error(`[STEP 2: querySelector] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Add accessibility - STEP 3
                                        throw new Error(`[STEP 3: addA11y] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Add styles - STEP 4
                                    throw new Error(`[STEP 4: addStyles] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Get palette - STEP 5
                                let palette;
                                    palette = getPalette(branding.palette);
                                    throw new Error(`[STEP 5: getPalette] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Create container group - STEP 6
                                    if (!ns) {
                                        throw new Error('SVG namespaceURI is null');
                                    container.setAttribute('transform', `translate(${vb.x + vb.contentWidth / 2}, ${vb.y + vb.contentHeight / 2})`);
                                    // Render words - STEP 7
                                    fittedWords.forEach((word, i) => {
                                            text.setAttribute('transform', `translate(${word.x || 0}, ${word.y || 0}) rotate(${word.rotate || 0})`);
                                            text.setAttribute('font-family', getFontSpec(branding.font).family);
                                            text.setAttribute('font-size', String(word.size));
                                            text.setAttribute('font-weight', String(word.size > 40 ? 'bold' : 'normal'));
                                            // Get color from palette
                                            const color = getColorForIndex(i, branding.palette);
                                            text.setAttribute('fill', color);
                                            // Add subtle stroke for better visibility
                                            if (word.size > 30) {
                                                text.setAttribute('stroke', palette.background);
                                                text.setAttribute('stroke-width', '0.5');
                                                text.setAttribute('paint-order', 'stroke fill');
                                            text.textContent = word.text;
                                            container.appendChild(text);
                                            throw new Error(`[STEP 7: render word ${i} "${word.text}"] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                    throw new Error(`[STEP 6: create container] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Add title if present - STEP 8
                                        throw new Error(`[STEP 8: addTitle] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                                // Sanitize and resolve - STEP 9
                                    const result = SVGUtils.sanitizeSVG(svg.outerHTML);
                                    throw new Error(`[STEP 9: sanitizeSVG] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
                    layout.start();
                    reject(new Error(`[d3-cloud layout setup] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`));
            throw new Error(`[renderWordCloud outer] ${error instanceof Error ? error.message : String(error)}\nStack: ${error instanceof Error ? error.stack : 'N/A'}`);
     * Renders a tag bar visualization
    private async renderTagBar(words: WordItem[], viewBox: ViewBox, branding: Branding, title: string): Promise<string> {
        const doc = SVGUtils.createSVG(viewBox.width, viewBox.height, 'tagbar');
        // Calculate bar dimensions
        const barHeight = 40;
        const spacing = 10;
        const availableHeight = vb.contentHeight - (title ? 40 : 0);
        const maxBars = Math.floor(availableHeight / (barHeight + spacing));
        const displayWords = words.slice(0, maxBars);
        // Normalize bar widths based on weights
        const maxWeight = Math.max(...displayWords.map((w) => w.weight));
        const minBarWidth = 100;
        // Create container group
        container.setAttribute('transform', `translate(${vb.x}, ${vb.y + (title ? 40 : 0)})`);
        // Render bars
        displayWords.forEach((word, i) => {
            const yPos = i * (barHeight + spacing);
            const barWidth = minBarWidth + ((word.weight / maxWeight) * (vb.contentWidth - minBarWidth));
            g.setAttribute('transform', `translate(0, ${yPos})`);
            // Bar rectangle
            // Label text
            text.setAttribute('x', '10');
            text.setAttribute('y', String(barHeight / 2));
            text.setAttribute('fill', '#FFF');
            // Weight text
            const weightText = doc.createElementNS(ns, 'text');
            weightText.setAttribute('x', String(barWidth - 10));
            weightText.setAttribute('y', String(barHeight / 2));
            weightText.setAttribute('text-anchor', 'end');
            weightText.setAttribute('dominant-baseline', 'middle');
            weightText.setAttribute('font-family', font.family);
            weightText.setAttribute('font-size', String(font.size - 2));
            weightText.setAttribute('fill', '#FFF');
            weightText.setAttribute('opacity', '0.9');
            weightText.textContent = String(word.weight);
            g.appendChild(weightText);
            this.addTitle(doc, svg, title, vb.width, font);
