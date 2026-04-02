const mockAxiosGet = vi.hoisted(() => vi.fn());
    get: mockAxiosGet,
  class MockBaseVideoGenerator {
  class MockVideoResult {
    videoId: string = '';
  class MockAvatarInfo {
    gender: string = '';
    previewImageUrl: string = '';
    previewVideoUrl: string = '';
    BaseVideoGenerator: MockBaseVideoGenerator,
    AvatarVideoParams: class {},
    VideoResult: MockVideoResult,
    AvatarInfo: MockAvatarInfo,
import { HeyGenVideoGenerator } from '../index';
describe('HeyGenVideoGenerator', () => {
  let video: HeyGenVideoGenerator;
    video = new HeyGenVideoGenerator('test-heygen-key');
      expect(video).toBeInstanceOf(HeyGenVideoGenerator);
    it('should set default API URLs', () => {
      const inst = video as unknown as Record<string, string>;
      expect(inst['_generateUrl']).toBe('https://api.heygen.com/v2/video/generate');
      expect(inst['_avatarsUrl']).toBe('https://api.heygen.com/v2/avatars');
  /* ---- CreateAvatarVideo request building ---- */
  describe('CreateAvatarVideo', () => {
    it('should build correct API request and return videoId on success', async () => {
        data: { data: { video_id: 'vid-123' } },
        avatarId: 'avatar1',
        scale: 1.0,
        avatarStyle: 'normal',
        outputWidth: 1920,
        outputHeight: 1080,
      const result = await video.CreateAvatarVideo(params as never);
        'https://api.heygen.com/v2/video/generate',
          video_inputs: expect.arrayContaining([
              character: expect.objectContaining({ avatar_id: 'avatar1' }),
              voice: expect.objectContaining({ audio_asset_id: 'audio1' }),
              background: expect.objectContaining({ image_asset_id: 'img1' }),
          dimension: { width: 1920, height: 1080 },
          headers: expect.objectContaining({ 'X-Api-Key': 'test-heygen-key' }),
      mockAxiosPost.mockRejectedValueOnce(new Error('HeyGen API error'));
      expect(result.errorMessage).toContain('HeyGen API error');
  /* ---- GetAvatars ---- */
  describe('GetAvatars', () => {
    it('should parse avatar list from API response', async () => {
      mockAxiosGet.mockResolvedValueOnce({
            avatars: [
                avatar_id: 'a1',
                avatar_name: 'Alice',
                gender: 'female',
                preview_image_url: 'https://img/alice.png',
                preview_video_url: 'https://vid/alice.mp4',
                avatar_id: 'a2',
                avatar_name: 'Bob',
                gender: 'male',
                preview_image_url: 'https://img/bob.png',
                preview_video_url: 'https://vid/bob.mp4',
      const avatars = await video.GetAvatars();
      expect(avatars).toHaveLength(2);
      expect(avatars[0].id).toBe('a1');
      expect(avatars[0].name).toBe('Alice');
      expect(avatars[0].gender).toBe('female');
      expect(avatars[1].id).toBe('a2');
      mockAxiosGet.mockRejectedValueOnce(new Error('API error'));
      expect(avatars).toEqual([]);
      const methods = await video.GetSupportedMethods();
      expect(methods).toContain('CreateAvatarVideo');
      expect(methods).toContain('GetAvatars');
  /* ---- CreateVideoTranslation ---- */
  describe('CreateVideoTranslation', () => {
      await expect(video.CreateVideoTranslation({} as never)).rejects.toThrow('Method not implemented.');
