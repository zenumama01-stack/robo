import { ActionableCommand } from '@memberjunction/ai-core-plus';
 * Component for displaying actionable command buttons
 * These are actions the user can trigger after an agent completes a task
  selector: 'mj-actionable-commands',
  templateUrl: './actionable-commands.component.html',
  styleUrls: ['./actionable-commands.component.css']
export class ActionableCommandsComponent {
  @Input() commands: ActionableCommand[] = [];
  @Input() isLastMessage: boolean = false;
  @Input() isConversationOwner: boolean = false;
  @Output() commandExecuted = new EventEmitter<ActionableCommand>();
   * Check if component should be visible
  public get isVisible(): boolean {
      this.isLastMessage &&
      this.isConversationOwner &&
      this.commands &&
      this.commands.length > 0
   * Handle command button click
  public onCommandClick(command: ActionableCommand): void {
      this.commandExecuted.emit(command);
   * Get button theme color based on command type
  public getButtonTheme(command: ActionableCommand): 'base' | 'primary' | 'secondary' | 'tertiary' | 'info' | 'success' | 'warning' | 'error' | 'dark' | 'light' | 'inverse' {
    // Use different themes for different command types
      return 'primary';
    } else if (command.type === 'open:url') {
      return 'info';
    return 'base';
   * Track by function for commands list
  public trackByCommand(index: number, command: ActionableCommand): string {
    return `${command.type}_${index}`;
