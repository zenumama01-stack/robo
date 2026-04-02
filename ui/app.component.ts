import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`,
  styles: []
export class AppComponent {
  title = 'transformers-demo';
 * MemberJunction Explorer - Application Component
 * MJ 3.0 Minimal App Shell Pattern:
 * Reduced from 158 lines to 12 lines by using @memberjunction/ng-explorer-app
  template: '<mj-explorer-app />'
export class AppComponent {}import { HeaderComponent } from './shell/header/header.component';
import { TabContainerComponent } from './shell/tab-container/tab-container.component';
import { ShellService } from './core/services/shell.service';
import { ConversationsApp } from './apps/conversations/conversations.app';
import { SettingsApp } from './apps/settings/settings.app';
import { CrmApp } from './apps/crm/crm.app';
  imports: [HeaderComponent, TabContainerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
export class AppComponent implements OnInit {
    private shellService: ShellService,
    private conversationsApp: ConversationsApp,
    private settingsApp: SettingsApp,
    private crmApp: CrmApp,
    // Register apps
    this.conversationsApp.Initialize(this.shellService);
    this.settingsApp.Initialize(this.shellService);
    this.crmApp.Initialize(this.shellService);
    this.shellService.RegisterApp(this.conversationsApp);
    this.shellService.RegisterApp(this.settingsApp);
    this.shellService.RegisterApp(this.crmApp);
    // Create initial tab only if no tabs exist (prevent duplicates on refresh)
    if (!this.shellService.HasTabs()) {
      this.shellService.OpenTab({
        AppId: 'conversations',
        Title: 'Chat',
        Route: '/conversations/chat'
    // Set initial active app
    this.shellService.SetActiveApp('conversations');
export class AppComponent {}import { HeaderComponent } from './shell/header/header.component';
export class AppComponent {}import { HeaderComponent } from './shell/header/header.component';
