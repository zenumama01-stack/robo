export class MJGlobalProperty
    key: unknown;
export class MJEvent
    component!: IMJComponent;
    event!: MJEventType;
    eventCode?: string;
    args: any; // Intentionally any - event payload varies by event type
export interface IMJComponent
export const MJEventType = {
    ComponentRegistered: 'ComponentRegistered',
    ComponentUnregistered: 'ComponentUnregistered',
    ComponentEvent: 'ComponentEvent',
    LoggedIn: 'LoggedIn',
    LoggedOut: 'LoggedOut',
    LoginFailed: 'LoginFailed',
    LogoutFailed: 'LogoutFailed',
    ManualResizeRequest: 'ManualResizeRequest',
    DisplaySimpleNotificationRequest: 'DisplaySimpleNotificationRequest',
export type MJEventType = typeof MJEventType[keyof typeof MJEventType];
export type DisplaySimpleNotificationRequestData = {
    style?: "none" | "success" | "error" | "warning" | "info"
    ResourceTypeID?: number
    ResourceRecordID?: number
    ResourceConfiguration?: string
    DisplayDuration?: number
