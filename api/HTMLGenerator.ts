 * Generates interactive HTML documentation with Mermaid ERD
export interface HTMLGeneratorOptions {
  includeMermaid?: boolean;
export class HTMLGenerator {
   * Generate interactive HTML documentation
    options: HTMLGeneratorOptions = {}
    const htmlLines: string[] = [];
    // HTML header
    this.appendHtmlHeader(htmlLines, state);
    // Body content
    htmlLines.push('<body>');
    htmlLines.push('<div class="container">');
    // Header section
    this.appendHeaderSection(htmlLines, state);
    // Sidebar with navigation
    htmlLines.push('<div class="main-wrapper">');
    this.appendSidebar(htmlLines, state);
    // Main content
    htmlLines.push('<div class="main-content">');
    // Analysis summary
    this.appendAnalysisSummary(htmlLines, state);
    // Database context
    this.appendDatabaseContext(htmlLines, state);
    // Searchable table/column list
    this.appendSearchableList(htmlLines, state, options);
    // Schemas with ERD and tables
    this.appendSchemas(htmlLines, state, options);
    htmlLines.push('</div>'); // main-content
    htmlLines.push('</div>'); // main-wrapper
    htmlLines.push('</div>'); // container
    htmlLines.push('</body>');
    htmlLines.push('</html>');
    return htmlLines.join('\n');
  private appendHtmlHeader(lines: string[], state: DatabaseDocumentation): void {
    lines.push('<!DOCTYPE html>');
    lines.push('<html lang="en">');
    lines.push('<head>');
    lines.push('<meta charset="UTF-8">');
    lines.push('<meta name="viewport" content="width=device-width, initial-scale=1.0">');
    lines.push(`<title>Database Documentation - ${state.database.name}</title>`);
    this.appendStyles(lines);
    lines.push('</head>');
  private appendStyles(lines: string[]): void {
    lines.push('<style>');
    lines.push(`
        background-color: #f5f7fa;
      header {
      header h1 {
      header .subtitle {
      header .meta {
      .main-wrapper {
        padding: 1.5rem 0;
        max-height: calc(100vh - 200px);
      .sidebar h3 {
        padding: 0.5rem 1.5rem;
      .sidebar a {
        padding: 0.6rem 1.5rem;
      .sidebar a:hover {
        border-left-color: #667eea;
        font-size: 1.8rem;
        border-bottom: 2px solid #e0e0e0;
        color: #764ba2;
        font-size: 1.3rem;
        margin-bottom: 0.8rem;
      .summary-grid {
        background: linear-gradient(135deg, #667eea15 0%, #764ba215 100%);
        border-left: 4px solid #667eea;
      .summary-card strong {
        margin-bottom: 0.3rem;
      .summary-card .value {
        font-size: 1.6rem;
        padding: 0.8rem;
      .search-result {
      .search-result:hover {
      .search-result .table-name {
      .search-result .column-name {
      table td {
        padding: 0.3rem 0.8rem;
      .confidence-high {
      .confidence-medium {
        background-color: #fff3cd;
      .confidence-low {
        background-color: #f8d7da;
      .schema-section {
      .schema-title {
      .mermaid {
      .mermaid svg {
      .relationship-info {
      .relationship-info strong {
        padding: 0.25rem 0.6rem;
      .tag.pk {
        background-color: #667eea;
      .tag.fk {
        background-color: #764ba2;
      .tag.notnull {
        background-color: #ff6b6b;
          max-height: auto;
    lines.push('</style>');
  private appendHeaderSection(lines: string[], state: DatabaseDocumentation): void {
    lines.push('<header>');
    lines.push(`<h1>📊 ${this.escapeHtml(state.database.name)}</h1>`);
    lines.push(`<div class="subtitle">Database Documentation</div>`);
    lines.push('<div class="meta">');
    lines.push(`<span><strong>Server:</strong> ${this.escapeHtml(state.database.server)}</span>`);
    lines.push(`<span><strong>Generated:</strong> ${new Date().toLocaleDateString()}</span>`);
    lines.push('</div>');
    lines.push('</header>');
  private appendSidebar(lines: string[], state: DatabaseDocumentation): void {
    lines.push('<aside class="sidebar">');
    lines.push('<h3>📑 Schemas</h3>');
      lines.push(`<a href="#schema-${this.toAnchor(schema.name)}">${this.escapeHtml(schema.name)}</a>`);
    lines.push('</aside>');
  private appendAnalysisSummary(lines: string[], state: DatabaseDocumentation): void {
    lines.push('<section id="summary">');
    lines.push('<h2>📈 Analysis Summary</h2>');
    const summary = state.summary;
    lines.push('<div class="summary-grid">');
    lines.push(`<div class="summary-card">
      <strong>Total Schemas</strong>
      <div class="value">${summary.totalSchemas}</div>
    </div>`);
      <strong>Total Tables</strong>
      <div class="value">${summary.totalTables}</div>
      <strong>Total Columns</strong>
      <div class="value">${summary.totalColumns}</div>
      <strong>Total Iterations</strong>
      <div class="value">${summary.totalIterations}</div>
      <strong>Tokens Used</strong>
      <div class="value">${summary.totalTokens.toLocaleString()}</div>
      <strong>Estimated Cost</strong>
      <div class="value">$${summary.estimatedCost.toFixed(2)}</div>
      lines.push('<h3>Latest Analysis Run</h3>');
      lines.push(`<p><strong>Status:</strong> ${this.escapeHtml(lastRun.status)}</p>`);
      lines.push(`<p><strong>Model:</strong> ${this.escapeHtml(lastRun.modelUsed)} (${this.escapeHtml(lastRun.vendor)})</p>`);
      lines.push(`<p><strong>Temperature:</strong> ${lastRun.temperature}</p>`);
    lines.push('</section>');
  private appendDatabaseContext(lines: string[], state: DatabaseDocumentation): void {
    if (!state.seedContext) {
    lines.push('<section id="context">');
    lines.push('<h2>💡 Database Context</h2>');
    if (state.seedContext.overallPurpose) {
      lines.push(`<p><strong>Purpose:</strong> ${this.escapeHtml(state.seedContext.overallPurpose)}</p>`);
    if (state.seedContext.industryContext) {
      lines.push(`<p><strong>Industry:</strong> ${this.escapeHtml(state.seedContext.industryContext)}</p>`);
    if (state.seedContext.businessDomains && state.seedContext.businessDomains.length > 0) {
      lines.push('<p><strong>Business Domains:</strong> ');
      const domains = state.seedContext.businessDomains.map(d => this.escapeHtml(d)).join(', ');
      lines.push(domains + '</p>');
  private appendSearchableList(
    lines: string[],
    options: HTMLGeneratorOptions
    lines.push('<section id="search">');
    lines.push('<h2>🔍 Search Tables & Columns</h2>');
    lines.push('<div class="search-box">');
    lines.push('<input type="text" id="search-input" placeholder="Search for tables or columns...">');
    lines.push('<div id="search-results" class="search-results"></div>');
    // Generate search data
    lines.push('<script>');
    lines.push('const searchData = [');
        lines.push(`  {`);
        lines.push(`    type: 'table',`);
        lines.push(`    schema: '${this.escapeJson(schema.name)}',`);
        lines.push(`    name: '${this.escapeJson(table.name)}',`);
        lines.push(`    description: '${this.escapeJson(table.description || '')}',`);
        lines.push(`    anchor: 'table-${this.toAnchor(table.name)}'`);
        lines.push(`  },`);
        // Add columns
          lines.push(`    type: 'column',`);
          lines.push(`    table: '${this.escapeJson(table.name)}',`);
          lines.push(`    name: '${this.escapeJson(column.name)}',`);
          lines.push(`    dataType: '${this.escapeJson(column.dataType)}',`);
          lines.push(`    description: '${this.escapeJson(column.description || '')}',`);
    this.appendSearchScript(lines);
    lines.push('</script>');
  private appendSearchScript(lines: string[]): void {
      const searchInput = document.getElementById('search-input');
      const searchResults = document.getElementById('search-results');
      searchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();
          searchResults.innerHTML = '';
        const results = searchData.filter(item =>
          item.name.toLowerCase().includes(query) ||
          item.description.toLowerCase().includes(query)
        ).slice(0, 50);
          searchResults.innerHTML = '<div class="no-results">No results found</div>';
        searchResults.innerHTML = results.map(item => {
          if (item.type === 'table') {
            return \`<div class="search-result" onclick="document.getElementById('\${item.anchor}').scrollIntoView({behavior: 'smooth'})">
              <div class="table-name">\${item.schema}.\${item.name}</div>
              <div class="column-name">\${item.description || 'No description'}</div>
            </div>\`;
              <div class="table-name">\${item.table}.\${item.name}</div>
              <div class="column-name">\${item.dataType} - \${item.description || 'No description'}</div>
  private appendSchemas(
      lines.push(`<div class="schema-section" id="schema-${this.toAnchor(schema.name)}">`);
      lines.push(`<div class="schema-title">${this.escapeHtml(schema.name)}</div>`);
      if (schema.description) {
        lines.push(`<p>${this.escapeHtml(schema.description)}</p>`);
      if (schema.inferredPurpose) {
        lines.push(`<p><strong>Purpose:</strong> ${this.escapeHtml(schema.inferredPurpose)}</p>`);
      if (schema.businessDomains && schema.businessDomains.length > 0) {
        lines.push(`<p><strong>Business Domains:</strong> ${schema.businessDomains.map(d => this.escapeHtml(d)).join(', ')}</p>`);
      // Entity Relationship Diagram
      lines.push('<h3>Entity Relationship Diagram</h3>');
      lines.push('<div class="mermaid">');
      lines.push(this.generateMermaidERD(schema));
      // Tables
      lines.push('<h3>Tables</h3>');
        lines.push(`<div id="table-${this.toAnchor(table.name)}">`);
        lines.push(`<h4>${this.escapeHtml(table.name)}</h4>`);
        if (table.description) {
          lines.push(`<p>${this.escapeHtml(table.description)}</p>`);
        lines.push(`<p><strong>Row Count:</strong> ${table.rowCount.toLocaleString()}</p>`);
        if (table.dependencyLevel !== undefined) {
          lines.push(`<p><strong>Dependency Level:</strong> ${table.dependencyLevel}</p>`);
        // Confidence
            const confidence = latest.confidence * 100;
            const confClass = confidence >= 80 ? 'confidence-high' : confidence >= 60 ? 'confidence-medium' : 'confidence-low';
            lines.push(`<p><span class="confidence-indicator ${confClass}">Confidence: ${confidence.toFixed(0)}%</span></p>`);
        if ((table.dependsOn && table.dependsOn.length > 0) || (table.dependents && table.dependents.length > 0)) {
          lines.push('<div class="relationship-info">');
          if (table.dependsOn && table.dependsOn.length > 0) {
            lines.push('<strong>Depends On:</strong>');
            lines.push('<ul>');
              lines.push(`<li><a href="#table-${this.toAnchor(dep.table)}">${dep.schema}.${dep.table}</a> (via ${dep.column})</li>`);
            lines.push('</ul>');
          if (table.dependents && table.dependents.length > 0) {
            lines.push('<strong>Referenced By:</strong>');
            for (const dep of table.dependents) {
              lines.push(`<li><a href="#table-${this.toAnchor(dep.table)}">${dep.schema}.${dep.table}</a></li>`);
        // Columns table
        lines.push('<table>');
        lines.push('<thead><tr><th>Column</th><th>Type</th><th>Description</th></tr></thead>');
        lines.push('<tbody>');
          const flags = [];
          if (column.isPrimaryKey) flags.push('<span class="tag pk">PK</span>');
          if (column.isForeignKey) flags.push('<span class="tag fk">FK</span>');
          if (!column.isNullable) flags.push('<span class="tag notnull">NOT NULL</span>');
          const typeInfo = flags.length > 0 ? `${column.dataType} ${flags.join(' ')}` : column.dataType;
          const description = column.description || '';
          lines.push(`<tr>`);
          lines.push(`<td>${this.escapeHtml(column.name)}</td>`);
          lines.push(`<td>${typeInfo}</td>`);
          lines.push(`<td>${this.escapeHtml(description)}</td>`);
          lines.push(`</tr>`);
        lines.push('</tbody>');
        lines.push('</table>');
    // Add Mermaid script
    lines.push('<script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"><\/script>');
    lines.push('<script>mermaid.initialize({ startOnLoad: true, theme: "default" }); mermaid.contentLoaded();<\/script>');
  private generateMermaidERD(schema: any): string {
    lines.push('erDiagram');
    // Add entities with their columns
      lines.push(`    ${table.name} {`);
        const type = column.dataType.replace(/\s+/g, '_');
        const constraints = [];
        if (column.isPrimaryKey) constraints.push('PK');
        if (column.isForeignKey) constraints.push('FK');
        if (!column.isNullable) constraints.push('NOT_NULL');
        const constraintStr = constraints.length > 0 ? ` "${constraints.join(',')}"` : '';
        lines.push(`        ${type} ${column.name}${constraintStr}`);
      lines.push('    }');
    // Add relationships
          lines.push(`    ${dep.table} ||--o{ ${table.name} : "has"`);
  private toAnchor(text: string): string {
    return text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
    const map: { [key: string]: string } = {
    return text.replace(/[&<>"']/g, (char) => map[char]);
  private escapeJson(text: string): string {
    return text.replace(/\\/g, '\\\\').replace(/'/g, "\\'").replace(/\n/g, '\\n').replace(/\r/g, '\\r');
