import { Injectable } from '@angular/core';
import { Metadata, RunView } from '@memberjunction/core';
import { EventEntity } from 'mj_generatedentities';
@Injectable({
  providedIn: 'root'
export class EventService {
  async getEvents(): Promise<EventEntity[]> {
    return result.Success ? (result.Results || []) : [];
  async getEventById(id: string): Promise<EventEntity | null> {
    const event = await md.GetEntityObject('Events') as unknown as EventEntity;
    const loaded = await event.Load(id);
    return loaded ? event : null;
  async createEvent(): Promise<EventEntity> {
    return await md.GetEntityObject('Events') as unknown as EventEntity;
  async getEventStatistics(): Promise<{
    upcomingEvents: number;
    pastEvents: number;
  }> {
    const events = await this.getEvents();
      totalEvents: events.length,
      upcomingEvents: events.filter(e => new Date(e.StartDate) > now).length,
      pastEvents: events.filter(e => new Date(e.EndDate) < now).length
