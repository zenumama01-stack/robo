import { Component, EventEmitter, Output } from '@angular/core';
import { AUDIO_MODEL_REGISTRY, type AudioModelConfig, type AudioModelDefinition } from '../../ai/audio-model-registry';
  selector: 'app-model-config',
  imports: [FormsModule],
    <div class="model-config-panel">
      <h2>Configure Voice Chat Pipeline</h2>
      <p class="description">Select models for each stage of the voice chat pipeline</p>
      <div class="model-selectors">
        <div class="model-selector">
          <label for="stt-select">
            <span class="label-icon">🎤</span>
            <span class="label-text">Speech Recognition (STT):</span>
          <select id="stt-select" [(ngModel)]="SelectedSTT">
            @for (model of AvailableSTTModels; track model.Id) {
              <option [value]="model.Id">
                {{ model.Name }} (~{{ model.ApproxSizeMB }} MB)
                @if (model.RequiresWebGPU) { · WebGPU }
          <p class="model-description">Converts your speech to text</p>
        <div class="arrow">↓</div>
          <label for="llm-select">
            <span class="label-icon">🧠</span>
            <span class="label-text">Language Model:</span>
          <select id="llm-select" [(ngModel)]="SelectedLLM">
            @for (model of AvailableLLMModels; track model.Id) {
          <p class="model-description">Generates intelligent responses</p>
          <label for="tts-select">
            <span class="label-icon">🔊</span>
            <span class="label-text">Text-to-Speech (TTS):</span>
          <select id="tts-select" [(ngModel)]="SelectedTTS">
            @for (model of AvailableTTSModels; track model.Id) {
          <p class="model-description">Converts text back to speech</p>
      <div class="total-section">
        <div class="total-size">
          <strong>Total Download Size:</strong> ~{{ TotalSizeMB }} MB
        <p class="download-note">
          @if (TotalSizeMB < 500) {
            ⚡ Fast download - should load in under a minute
          } @else if (TotalSizeMB < 1500) {
            ⏱️ Medium download - may take 1-2 minutes
            🐌 Large download - may take 2-5 minutes on slow connections
      <button (click)="OnStartClick()" class="start-btn">
        🚀 Start Voice Chat
      <div class="info-section">
        <p>💡 <strong>Privacy First:</strong> All models run locally in your browser. No data is sent to servers.</p>
        <p>💾 <strong>One-Time Download:</strong> Models are cached after first load for instant future use.</p>
    .model-config-panel {
      margin: 40px auto;
      box-shadow: 0 8px 24px rgba(0,0,0,0.1);
    .description {
      margin: 0 0 32px 0;
    .model-selectors {
    .model-selector {
    label {
    .label-icon {
    select {
      border: 2px solid #e0e0e0;
      transition: border-color 0.2s;
    select:hover {
    select:focus {
    .model-description {
      padding-left: 28px;
    .arrow {
      margin: 8px 0;
    .total-section {
    .total-size {
    .download-note {
    .start-btn {
      padding: 16px 32px;
    .start-btn:hover {
    .start-btn:active {
    .info-section {
      margin-top: 24px;
      padding-top: 24px;
    .info-section p {
export class ModelConfigComponent {
  @Output() StartChat = new EventEmitter<AudioModelConfig>();
  SelectedSTT = AUDIO_MODEL_REGISTRY.STT[0].Id;
  SelectedLLM = AUDIO_MODEL_REGISTRY.LLM[0].Id;
  SelectedTTS = AUDIO_MODEL_REGISTRY.TTS[0].Id;
  AvailableSTTModels = AUDIO_MODEL_REGISTRY.STT;
  AvailableLLMModels = AUDIO_MODEL_REGISTRY.LLM;
  AvailableTTSModels = AUDIO_MODEL_REGISTRY.TTS;
  get TotalSizeMB(): number {
    const stt = this.AvailableSTTModels.find(m => m.Id === this.SelectedSTT);
    const llm = this.AvailableLLMModels.find(m => m.Id === this.SelectedLLM);
    const tts = this.AvailableTTSModels.find(m => m.Id === this.SelectedTTS);
    return (stt?.ApproxSizeMB ?? 0) + (llm?.ApproxSizeMB ?? 0) + (tts?.ApproxSizeMB ?? 0);
  OnStartClick(): void {
    const config: AudioModelConfig = {
      STT: this.AvailableSTTModels.find(m => m.Id === this.SelectedSTT)!,
      LLM: this.AvailableLLMModels.find(m => m.Id === this.SelectedLLM)!,
      TTS: this.AvailableTTSModels.find(m => m.Id === this.SelectedTTS)!,
    this.StartChat.emit(config);
