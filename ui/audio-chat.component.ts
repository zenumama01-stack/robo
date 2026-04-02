  Component,
  OnInit,
  ViewChild,
  ElementRef,
  DestroyRef,
  inject,
  ChangeDetectorRef,
} from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AudioService } from '../ai/audio.service';
import type { AudioTurn } from '../ai/audio-messages';
import { ModelConfigComponent } from './model-config/model-config.component';
import type { AudioModelConfig } from '../ai/audio-model-registry';
  selector: 'app-audio-chat',
  imports: [DecimalPipe, DatePipe, RouterLink, ModelConfigComponent],
  template: `
    <!-- Model configuration screen -->
    @if (!IsReady && !IsLoading) {
      <div class="config-container">
        <div class="back-link">
          <a routerLink="/home">← Back to Home</a>
        <app-model-config (StartChat)="OnStartChat($event)"></app-model-config>
    <!-- Loading overlay -->
    @if (IsLoading) {
      <div class="loading-overlay">
        <div class="loading-content">
          <div class="spinner"></div>
          <p>Loading models... {{ LoadProgress | number:'1.0-0' }}%</p>
          <div class="progress-bar">
            <div class="progress-fill" [style.width.%]="LoadProgress"></div>
          <p class="loading-hint">
            First load downloads models (~{{ TotalSizeMB }} MB).<br>
            They're cached locally for instant loads afterward.
          </p>
    <!-- Audio chat interface -->
    @if (IsReady) {
      <div class="audio-chat-container">
        <div class="chat-header">
            <a routerLink="/home" class="back-btn">← Home</a>
            <h2>Voice Chat</h2>
            <span class="status-badge" [class.recording]="IsRecording" [class.processing]="IsProcessing">
              @if (IsRecording) {
                🎤 Recording...
              } @else if (IsProcessing) {
                ⚙️ {{ CurrentStageLabel }}
              } @else {
                ✓ Ready
        <div class="turns-list" #turnsContainer>
          @for (turn of Turns; track $index) {
            <div class="turn">
              <div class="user-message">
                <div class="label">You said:</div>
                <div class="text">{{ turn.Transcription }}</div>
              <div class="ai-message">
                <div class="label">AI responded:</div>
                <div class="text">{{ turn.LLMResponse }}</div>
                @if (turn.AudioBlob) {
                  <audio [src]="GetAudioUrl(turn.AudioBlob)" controls></audio>
              <div class="timestamp">{{ turn.Timestamp | date:'short' }}</div>
          <!-- Current processing turn -->
          @if (IsProcessing) {
            <div class="turn processing">
              @if (CurrentTranscription) {
                  <div class="text">{{ CurrentTranscription }}</div>
              @if (CurrentLLMResponse) {
                  <div class="label">AI is responding:</div>
                  <div class="text">{{ CurrentLLMResponse }}<span class="cursor">&#x2588;</span></div>
          @if (!IsRecording && !IsProcessing && Turns.length === 0) {
            <div class="empty-state">
              <div class="empty-icon">🎤</div>
              <p>Click "Start Recording" to begin your voice conversation</p>
        <div class="control-area">
          @if (!IsRecording && !IsProcessing) {
            <button (click)="StartRecording()" class="record-btn">
              🎤 Start Recording
            <p class="hint">Speak clearly and keep messages under 30 seconds</p>
          } @else if (IsRecording) {
            <button (click)="StopRecording()" class="stop-btn">
              ⏹ Stop Recording
            <p class="hint recording-hint">Recording... Click stop when finished</p>
            <button (click)="Abort()" class="abort-btn">
              🛑 Cancel
            <p class="hint">Processing your message...</p>
    @if (ErrorMessage) {
      <div class="error-overlay">
        <div class="error-box">
          <h3>⚠️ Error</h3>
          <p>{{ ErrorMessage }}</p>
          <button (click)="DismissError()">Dismiss</button>
  `,
  styles: [`
      height: 100vh;
      font-family: system-ui, sans-serif;
      background: #f5f5f5;
    .config-container {
    .back-link {
      margin: 0 auto 16px auto;
    .back-link a {
      transition: opacity 0.2s;
    .back-link a:hover {
    .loading-overlay {
    .loading-content {
      max-width: 400px;
    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid rgba(255,255,255,0.3);
      border-top-color: white;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 20px;
    .progress-bar {
      height: 8px;
      background: rgba(255,255,255,0.3);
      margin: 16px 0;
    .progress-fill {
      transition: width 0.3s;
    .loading-hint {
      margin-top: 16px;
    .audio-chat-container {
    .chat-header {
      padding: 16px 20px;
    .back-btn {
    .back-btn:hover {
    .chat-header h2 {
      background: rgba(255,255,255,0.2);
    .status-badge.recording {
      background: #ef4444;
      animation: pulse 1.5s ease-in-out infinite;
    .status-badge.processing {
      background: #3b82f6;
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.7; }
    .turns-list {
      overflow-y: auto;
      padding: 80px 20px;
      color: #999;
    .empty-icon {
    .turn {
    .turn.processing {
      border: 2px dashed #667eea;
      background: #f0f4ff;
    .user-message, .ai-message {
    .user-message:last-child, .ai-message:last-child {
    .label {
    .text {
      color: #333;
      word-break: break-word;
    .cursor {
      animation: blink 0.7s step-end infinite;
    @keyframes blink {
      50% { opacity: 0; }
    audio {
    .timestamp {
    .control-area {
      border-top: 1px solid #e0e0e0;
      box-shadow: 0 -2px 8px rgba(0,0,0,0.05);
    .record-btn, .stop-btn, .abort-btn {
      padding: 16px 48px;
      border-radius: 50px;
      transition: transform 0.2s, box-shadow 0.2s;
    .record-btn {
    .record-btn:hover {
    .stop-btn {
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
    .stop-btn:hover {
      box-shadow: 0 6px 16px rgba(239, 68, 68, 0.4);
    .abort-btn {
      background: #6b7280;
      box-shadow: 0 4px 12px rgba(107, 114, 128, 0.3);
    .abort-btn:hover {
      box-shadow: 0 6px 16px rgba(107, 114, 128, 0.4);
    .hint {
      margin: 12px 0 0 0;
    .hint.recording-hint {
    .error-overlay {
      position: fixed;
      background: rgba(0,0,0,0.5);
      z-index: 1000;
    .error-box {
      margin: 20px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.2);
    .error-box h3 {
      margin: 0 0 16px 0;
    .error-box p {
      margin: 0 0 20px 0;
    .error-box button {
      padding: 10px 24px;
      background: #667eea;
        flex-basis: 100%;
  `]
export class AudioChatComponent implements OnInit {
  @ViewChild('turnsContainer') TurnsContainer!: ElementRef;
  protected readonly audioService = inject(AudioService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly sanitizer = inject(DomSanitizer);
  IsLoading = false;
  IsReady = false;
  IsRecording = false;
  IsProcessing = false;
  LoadProgress = 0;
  CurrentStage: 'idle' | 'transcribing' | 'generating' | 'synthesizing' = 'idle';
  CurrentTranscription = '';
  CurrentLLMResponse = '';
  Turns: AudioTurn[] = [];
  ErrorMessage = '';
  TotalSizeMB = 0;
  // Audio recording
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  get CurrentStageLabel(): string {
    switch (this.CurrentStage) {
      case 'transcribing': return 'Listening...';
      case 'generating': return 'Thinking...';
      case 'synthesizing': return 'Speaking...';
      default: return 'Processing...';
  ngOnInit(): void {
    this.subscribeToAudioService();
  OnStartChat(config: AudioModelConfig): void {
    this.TotalSizeMB = config.STT.ApproxSizeMB + config.LLM.ApproxSizeMB + config.TTS.ApproxSizeMB;
    this.audioService.Initialize(config);
  async StartRecording(): Promise<void> {
      const stream = await navigator.mediaDevices.getUserMedia({
        audio: {
          sampleRate: 16000,
          channelCount: 1,
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true,
      this.mediaRecorder = new MediaRecorder(stream, {
        mimeType: 'audio/webm',
      this.audioChunks = [];
      this.mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          this.audioChunks.push(event.data);
      this.mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
          // Process audio in main thread (Web Audio API available here)
          const audioData = await this.processAudioBlob(audioBlob);
          this.audioService.ProcessAudio(audioData);
          console.error('Audio processing failed:', err);
          this.ErrorMessage = 'Failed to process audio. Please try again.';
        stream.getTracks().forEach(track => track.stop());
      this.mediaRecorder.start();
      this.IsRecording = true;
      console.error('Microphone access denied:', err);
      this.ErrorMessage = 'Microphone access denied. Please allow microphone access in your browser settings and try again.';
  StopRecording(): void {
    if (this.mediaRecorder && this.IsRecording) {
      this.mediaRecorder.stop();
      this.IsRecording = false;
    this.audioService.Abort();
    this.CurrentTranscription = '';
    this.CurrentLLMResponse = '';
  GetAudioUrl(blob: Blob | null): SafeUrl {
    if (!blob) return '';
    return this.sanitizer.bypassSecurityTrustUrl(URL.createObjectURL(blob));
  DismissError(): void {
    this.ErrorMessage = '';
   * Process audio blob in main thread using Web Audio API
   * Converts to Float32Array for Whisper model
  private async processAudioBlob(audioBlob: Blob): Promise<Float32Array> {
    // Convert Blob to ArrayBuffer
    const arrayBuffer = await audioBlob.arrayBuffer();
    // Decode audio using Web Audio API (only available in main thread)
    const audioContext = new AudioContext();
    const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
    // Resample to 16kHz mono if needed
    let audioData: Float32Array;
    if (audioBuffer.sampleRate !== 16000 || audioBuffer.numberOfChannels !== 1) {
      // Create offline context for resampling
      const targetSampleRate = 16000;
      const duration = audioBuffer.duration;
      const targetLength = Math.ceil(duration * targetSampleRate);
      const offlineContext = new OfflineAudioContext(1, targetLength, targetSampleRate);
      const source = offlineContext.createBufferSource();
      source.buffer = audioBuffer;
      source.connect(offlineContext.destination);
      source.start();
      const resampledBuffer = await offlineContext.startRendering();
      audioData = resampledBuffer.getChannelData(0);
      // Already correct format
      audioData = audioBuffer.getChannelData(0);
    // Whisper expects exactly 30 seconds of audio at 16kHz
    const targetSamples = 16000 * 30; // 480,000 samples
    const currentSamples = audioData.length;
    if (currentSamples < targetSamples) {
      // Pad with silence
      const paddedData = new Float32Array(targetSamples);
      paddedData.set(audioData);
      return paddedData;
    } else if (currentSamples > targetSamples) {
      // Truncate to 30 seconds
      return audioData.slice(0, targetSamples);
    return audioData;
  private subscribeToAudioService(): void {
    this.audioService.IsLoading$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(v => this.IsLoading = v);
    this.audioService.IsReady$
      .subscribe(v => this.IsReady = v);
    this.audioService.IsProcessing$
      .subscribe(v => this.IsProcessing = v);
    this.audioService.LoadProgress$
      .subscribe(v => this.LoadProgress = v);
    this.audioService.CurrentStage$
      .subscribe(v => this.CurrentStage = v);
    this.audioService.Transcription$
      .subscribe(text => {
        this.CurrentTranscription = text;
    this.audioService.LLMToken$
      .subscribe(token => {
        this.CurrentLLMResponse += token;
    this.audioService.TurnComplete$
      .subscribe(() => {
        Promise.resolve().then(() => {
          this.Turns = this.audioService.GetHistory();
          this.ScrollToBottom();
    this.audioService.Error$
      .subscribe(err => {
        this.ErrorMessage = err;
  private ScrollToBottom(): void {
      const el = this.TurnsContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
