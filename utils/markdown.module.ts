import { MarkdownComponent } from './components/markdown.component';
import { MarkdownService } from './services/markdown.service';
 * MemberJunction Markdown Module
 * A lightweight Angular module for rendering markdown content with:
 * - Prism.js syntax highlighting
 * - Mermaid diagram support
 * - Copy-to-clipboard for code blocks
 * - Collapsible heading sections
 * - GitHub-style alerts
 * - Heading anchor IDs
 * import { MarkdownModule } from '@memberjunction/ng-markdown';
 *   imports: [MarkdownModule]
 * Then in your template:
 * <mj-markdown [data]="markdownContent"></mj-markdown>
 * Note: This module does NOT use forRoot(). Simply import it in any module
 * where you need markdown rendering. The MarkdownService is provided at root
 * level for efficient sharing across the application.
    MarkdownComponent
    // MarkdownService is providedIn: 'root', so no need to provide here
    // This ensures a single instance is shared across the app
export class MarkdownModule { }
