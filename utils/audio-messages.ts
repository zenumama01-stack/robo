import type { AudioModelConfig } from './audio-model-registry';
// ── Audio Turn Data ────────────────────────────────────
export interface AudioTurn {
  Transcription: string;
  LLMResponse: string;
  AudioBlob: Blob;
  Timestamp: Date;
// ── Requests (main → worker) ───────────────────────────
export interface AudioLoadRequest {
  Type: 'audio:load';
  Config: AudioModelConfig;
export interface AudioProcessRequest {
  Type: 'audio:process';
  AudioData: Float32Array;  // Pre-processed audio from main thread
export interface AudioAbortRequest {
  Type: 'audio:abort';
// ── Responses (worker → main) ──────────────────────────
export interface AudioLoadProgress {
  Stage: 'stt' | 'llm' | 'tts';
  Progress: number;
  File?: string;
export interface AudioReady {
export interface TranscriptionResponse {
  Type: 'transcription';
export interface LLMTokenResponse {
  Type: 'llm-token';
export interface AudioReadyResponse {
  Type: 'audio-ready';
export interface TurnCompleteResponse {
  Type: 'turn-complete';
  Turn: AudioTurn;
export interface AudioErrorResponse {
  Stage: 'stt' | 'llm' | 'tts' | 'audio-capture' | 'init';
// ── Union Types ────────────────────────────────────────
export type AudioWorkerRequest =
  | AudioLoadRequest
  | AudioProcessRequest
  | AudioAbortRequest;
export type AudioWorkerResponse =
  | AudioLoadProgress
  | AudioReady
  | TranscriptionResponse
  | LLMTokenResponse
  | AudioReadyResponse
  | TurnCompleteResponse
  | AudioErrorResponse;
