import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
 * Displays read-only account information for the current user.
 * Shows name, email, role, creation date, and last login.
  selector: 'mj-account-info',
  templateUrl: './account-info.component.html',
  styleUrls: ['./account-info.component.css']
export class AccountInfoComponent implements OnInit {
  CurrentUser: MJUserEntity | null = null;
  constructor(private cdr: ChangeDetectorRef, private ngZone: NgZone) {}
    await this.LoadAccountInfo();
  private async LoadAccountInfo(): Promise<void> {
      const userInfo = md.CurrentUser;
      // Load full user entity for additional details
      const user = await md.GetEntityObject<MJUserEntity>('MJ: Users');
      const loaded = await user.Load(userInfo.ID);
        this.CurrentUser = user;
        this.ErrorMessage = 'Unable to load account information.';
      this.ErrorMessage = 'Failed to load account information.';
      console.error('Error loading account info:', error);
   * Formats a date for display
  FormatDate(date: Date | null | undefined): string {
