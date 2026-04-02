    MJArtifactTypeEntity
 * Caching of metadata for components and related data
export class ArtifactMetadataEngine extends BaseEngine<ArtifactMetadataEngine> {
    public static get Instance(): ArtifactMetadataEngine {
       return super.getInstance<ArtifactMetadataEngine>();
    private _artifactTypes: MJArtifactTypeEntity[];
                PropertyName: "_artifactTypes",
     * Finds an artifact type on a case-insensitive match of name
    public FindArtifactType(name: string): MJArtifactTypeEntity | undefined {
        if (!this._artifactTypes || !name) {
        const match = this._artifactTypes.find(c => c.Name.trim().toLowerCase() === name.trim().toLowerCase());
import { randomBytes } from 'node:crypto';
import fs = require('node:fs/promises');
const IS_CI = !!process.env.CI;
const ARTIFACT_DIR = path.join(__dirname, '..', 'artifacts');
async function ensureArtifactDir (): Promise<void> {
  if (!IS_CI) {
  await fs.mkdir(ARTIFACT_DIR, { recursive: true });
export async function createArtifact (
  data: Buffer
  await ensureArtifactDir();
  await fs.writeFile(path.join(ARTIFACT_DIR, fileName), data);
export async function createArtifactWithRandomId (
  makeFileName: (id: string) => string,
  const randomId = randomBytes(12).toString('hex');
  const fileName = makeFileName(randomId);
  await createArtifact(fileName, data);
