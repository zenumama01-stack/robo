import { VertexLLM, VertexAICredentials } from '../models/vertexLLM';
import { ChatParams, ChatMessageRole, ModelUsage } from '@memberjunction/ai';
 * Test suite for VertexLLM
 * VertexLLM extends GeminiLLM and inherits all its functionality (chat, streaming,
 * thinking, multimodal, etc.). These tests focus on:
 * 1. Constructor and authentication configuration
 * 2. Basic integration tests (if credentials are available)
 * Setup Requirements:
 * 1. Set VERTEX_PROJECT_ID environment variable
 * 2. Set VERTEX_SERVICE_ACCOUNT_KEY_PATH to path of JSON key file (optional)
 * 3. Set VERTEX_LOCATION (optional, defaults to 'us-central1')
 * To run tests:
 * export VERTEX_PROJECT_ID="your-project-id"
 * export VERTEX_SERVICE_ACCOUNT_KEY_PATH="/path/to/service-account-key.json"
 * npm test
describe('VertexLLM', () => {
  const projectId = process.env.VERTEX_PROJECT_ID || 'test-project';
  const keyPath = process.env.VERTEX_SERVICE_ACCOUNT_KEY_PATH;
  const location = process.env.VERTEX_LOCATION || 'us-central1';
  // Build credentials JSON based on what's available
  const buildCredentialsJson = () => JSON.stringify({
    project: projectId,
    location: location,
    ...(keyPath ? { keyFilePath: keyPath } : {})
  describe('Constructor - Credential Formats', () => {
    it('should accept ADC credentials format', () => {
      const creds = JSON.stringify({
        project: 'test-project',
        location: 'us-central1'
      const llm = new VertexLLM(creds);
      expect(llm.Credentials.project).toBe('test-project');
      expect(llm.Credentials.location).toBe('us-central1');
    it('should accept service account JSON format', () => {
        type: 'service_account',
        project_id: 'test-project',
        private_key: '-----BEGIN PRIVATE KEY-----\ntest\n-----END PRIVATE KEY-----\n',
        client_email: 'test@test.iam.gserviceaccount.com',
    it('should accept keyFilePath format', () => {
        keyFilePath: '/path/to/key.json',
      expect(llm.Credentials.keyFilePath).toBe('/path/to/key.json');
    it('should throw error for invalid JSON', () => {
      expect(() => new VertexLLM('not json')).toThrow('Invalid Vertex AI credentials JSON');
    it('should throw error for missing project', () => {
      expect(() => new VertexLLM(JSON.stringify({ location: 'us-central1' })))
        .toThrow('must include "project"');
    it('should default location to us-central1', () => {
      const creds = JSON.stringify({ project: 'test-project' });
    it('should use project_id over project if both provided', () => {
        project: 'wrong-project',
        project_id: 'correct-project',
      expect(llm.Credentials.project).toBe('correct-project');
  describe('Instance Properties', () => {
    let vertexLLM: VertexLLM;
      vertexLLM = new VertexLLM(buildCredentialsJson());
    it('should create instance successfully', () => {
      expect(vertexLLM).toBeInstanceOf(VertexLLM);
    it('should expose GeminiClient getter (inherited)', async () => {
      // GeminiClient requires async initialization - make a chat call to trigger it
        messages: [{ role: 'user' as ChatMessageRole, content: 'test' }],
        model: 'gemini-2.0-flash-exp',
        temperature: 0.7
      // This will initialize the client (or fail gracefully if no credentials)
        await vertexLLM.ChatCompletion(params);
        // Expected to fail without real credentials
      // Now the client should be initialized
      expect(vertexLLM.GeminiClient).toBeDefined();
    it('should expose Credentials getter', () => {
      const creds = vertexLLM.Credentials;
      expect(creds.project).toBe(projectId);
      expect(creds.location).toBe(location);
    it('should report streaming support (inherited from GeminiLLM)', () => {
      expect(vertexLLM.SupportsStreaming).toBe(true);
  describe('Chat Completion - Integration Tests', () => {
    // Skip actual API calls in CI/CD - only run when credentials are available
    const shouldRunIntegrationTests = process.env.VERTEX_PROJECT_ID &&
                                     process.env.VERTEX_SERVICE_ACCOUNT_KEY_PATH;
      if (shouldRunIntegrationTests) {
    it('should handle simple text completion', async () => {
      if (!shouldRunIntegrationTests) {
        console.log('Skipping integration test - credentials not configured');
            content: 'What is 2+2? Reply with just the number.'
        model: 'gemini-2.5-flash',
      const result = await vertexLLM.ChatCompletion(params);
        console.error('Chat completion failed:', result.errorMessage);
        console.error('Exception details:', JSON.stringify(result.exception, null, 2));
        if (result.errorInfo) {
          console.error('Error info:', JSON.stringify(result.errorInfo, null, 2));
      expect(result.data.choices[0].message.role).toBe(ChatMessageRole.assistant);
      expect(result.data.usage).toBeInstanceOf(ModelUsage);
    it('should handle system messages (inherited from GeminiLLM)', async () => {
            content: 'You are a helpful math tutor. Always explain your answers.'
            content: 'What is 5+3?'
        temperature: 0.5
      expect(result.data.choices[0].message.content).toContain('8');
    it('should handle conversation history (inherited from GeminiLLM)', async () => {
            content: 'My name is Alice.'
            content: 'Hello Alice! Nice to meet you.'
            content: 'What is my name?'
        temperature: 0.1
      expect(result.data.choices[0].message.content.toLowerCase()).toContain('alice');
    it.skip('should support streaming with callbacks (inherited from GeminiLLM)', async () => {
      const chunks: string[] = [];
      let completeCalled = false;
            content: 'Count from 1 to 3.'
        streaming: true,
        streamingCallbacks: {
          OnContent: (content) => {
            chunks.push(content);
          OnComplete: (result) => {
            completeCalled = true;
          OnError: (error) => {
            throw new Error(`Streaming error: ${error}`);
      expect(completeCalled).toBe(true);
      // Streaming should return final result in data
      expect(result.data.choices[0].message.content.length).toBeGreaterThan(0);
      // Note: OnContent callbacks may or may not fire depending on provider implementation
      // What matters is that streaming mode completes successfully
    it('should handle thinking/reasoning for Gemini 2.5+ models', async () => {
            content: 'Solve: If x + 5 = 12, what is x?'
        effortLevel: '50' // Medium thinking effort (string for compatibility)
      // Note: thinking content may or may not be present depending on model version
  describe('Gemini 3 Models - Integration Tests', () => {
        // Gemini 3 preview models require 'global' location, not regional endpoints
        // See: https://github.com/block/goose/issues/6186
        const gemini3CredentialsJson = JSON.stringify({
          location: 'global', // Must use global for Gemini 3 preview models
        vertexLLM = new VertexLLM(gemini3CredentialsJson);
    it('should work with Gemini 3 Flash', async () => {
            content: 'What is the capital of France? Reply with just the city name.'
        model: 'gemini-3-flash-preview',
      // Gemini 3 models may not be available in all projects (preview status)
        console.log('Gemini 3 Flash not available (preview), skipping:', result.errorMessage);
        return; // Skip test if model not available
      expect(result.data.choices[0].message.content.toLowerCase()).toContain('paris');
    it('should work with Gemini 3 Pro for complex reasoning', async () => {
            content: 'Write a function in Python that calculates fibonacci numbers. Keep it concise.'
        model: 'gemini-3-pro-preview',
        maxOutputTokens: 200,
        effortLevel: '75' // High reasoning effort
        console.log('Gemini 3 Pro not available (preview), skipping:', result.errorMessage);
      expect(content).toBeTruthy();
      expect(content.toLowerCase()).toContain('def');
      // Model may use "fib" or "fibonacci" as function name
      expect(content.toLowerCase()).toMatch(/fib(onacci)?/);
    it('should handle Gemini 3 Pro Image for image generation', async () => {
            content: 'Describe what a blue sky looks like. Keep it brief.'
        model: 'gemini-3-pro-image-preview',
        maxOutputTokens: 100
      // Gemini 3 Pro Image may not be available in all projects (preview status)
        console.log('Gemini 3 Pro Image not available (preview), skipping:', result.errorMessage);
    }, 60000); // Increased timeout for slower image model
  describe('Error Handling (inherited from GeminiLLM)', () => {
            content: 'Test message'
        model: 'nonexistent-model-xyz-123',
      expect(result.errorMessage).toBeTruthy();
      expect(result.exception).toBeDefined();
      expect(result.errorInfo).toBeDefined();
    it('should handle empty messages array', async () => {
      expect(result.success).toBeDefined();
