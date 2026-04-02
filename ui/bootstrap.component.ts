 * MemberJunction Bootstrap Component
 * Minimal placeholder component for future fully encapsulated bootstrap.
 * Currently applications should use MJInitializationService directly in their app.component.
import { Component, Inject, OnInit } from '@angular/core';
import { SetProductionStatus } from '@memberjunction/core';
import { MJEnvironmentConfig, MJ_ENVIRONMENT } from './bootstrap.types';
  standalone: false,
  selector: 'mj-bootstrap',
    <div class="mj-bootstrap-container">
      <ng-content></ng-content>
    .mj-bootstrap-container {
export class MJBootstrapComponent implements OnInit {
    @Inject(MJ_ENVIRONMENT) private environment: MJEnvironmentConfig
  ngOnInit() {
    console.log('🚀 MemberJunction Bootstrap');
    SetProductionStatus(this.environment.production);
