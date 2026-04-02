export type SettingsResponse = {
    enabledFeatures: unknown[];
export type BettyResponse = {
    conversationId: number;
    response: string;
    references: BettyReference[];
export type BettyReference = {
    link: string;
