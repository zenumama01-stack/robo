import { RouterLink } from '@angular/router';
  selector: 'app-home',
  imports: [RouterLink],
    <div class="home-container">
      <header class="home-header">
        <h1>Transformers.js AI Demo</h1>
        <p class="subtitle">Run AI models entirely in your browser — no server, no API costs, complete privacy</p>
      </header>
      <div class="mode-cards">
        <a routerLink="/text-chat" class="mode-card">
          <div class="icon">💬</div>
          <h2>Text Chat</h2>
          <p>Interactive text conversation with language models</p>
          <ul class="features">
            <li>SmolLM2 & Phi models</li>
            <li>Token streaming</li>
            <li>WebGPU acceleration</li>
        <a routerLink="/audio-chat" class="mode-card">
          <div class="icon">🎤</div>
          <h2>Audio Chat</h2>
          <p>Full voice-to-voice AI assistant pipeline</p>
            <li>Speech-to-Text (Whisper)</li>
            <li>Language Model</li>
            <li>Text-to-Speech (SpeechT5)</li>
      <footer class="home-footer">
        <p>
          <strong>All processing happens on your device.</strong>
          Models are downloaded once and cached locally.
        <p class="tech-stack">
          Built with <a href="https://huggingface.co/docs/transformers.js" target="_blank">Transformers.js</a>
          + <a href="https://angular.dev" target="_blank">Angular 18</a>
      </footer>
      font-family: system-ui, -apple-system, sans-serif;
    .home-container {
    .home-header {
      margin-bottom: 60px;
    .home-header h1 {
      text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
    .subtitle {
      opacity: 0.95;
      max-width: 600px;
    .mode-cards {
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 30px;
    .mode-card {
      box-shadow: 0 8px 16px rgba(0,0,0,0.15);
    .mode-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0,0,0,0.2);
    .mode-card .icon {
    .mode-card h2 {
    .mode-card > p {
    .features {
      list-style: none;
      padding-top: 16px;
    .features li {
      padding: 8px 0;
      color: #555;
    .features li:before {
      content: '✓';
      width: 20px;
      height: 20px;
      line-height: 20px;
      margin-right: 10px;
    .home-footer {
    .home-footer p {
      margin: 12px 0;
    .tech-stack {
      opacity: 0.8;
    .tech-stack a {
      text-decoration: underline;
        font-size: 36px;
export class HomeComponent {}
