 * Custom Nunjucks loader for file-based prompts
 * Pattern adapted from Templates package's TemplateEntityLoader
export class PromptFileLoader extends nunjucks.Loader {
  public async: true = true;
  private prompts: Map<string, string> = new Map();
  constructor(private promptsDir: string) {
   * Load all prompt files from directory
  public async loadAll(): Promise<void> {
    const files = await fs.readdir(this.promptsDir);
      if (file.endsWith('.md') || file.endsWith('.txt')) {
        const promptName = path.basename(file, path.extname(file));
        const filePath = path.join(this.promptsDir, file);
        const content = await fs.readFile(filePath, 'utf-8');
        this.prompts.set(promptName, content);
   * Required by Nunjucks Loader - provides template source
  public getSource(name: string, callback: any): void {
    const content = this.prompts.get(name);
      callback(null, {
        src: content,
        path: name,
        noCache: true
      callback(new Error(`Prompt not found: ${name}`));
