async function deleteDraft (releaseId: string, targetRepo: ElectronReleaseRepo) {
    const result = await octokit.repos.getRelease({
      repo: targetRepo,
      release_id: parseInt(releaseId, 10)
    if (!result.data.draft) {
      console.log(`${fail} published releases cannot be deleted.`);
      await octokit.repos.deleteRelease({
        release_id: result.data.id
    console.log(`${pass} successfully deleted draft with id ${releaseId} from ${targetRepo}`);
    console.error(`${fail} couldn't delete draft with id ${releaseId} from ${targetRepo}: `, err);
async function deleteTag (tag: string, targetRepo: ElectronReleaseRepo) {
    await octokit.git.deleteRef({
      ref: `tags/${tag}`
    console.log(`${pass} successfully deleted tag ${tag} from ${targetRepo}`);
    console.log(`${fail} couldn't delete tag ${tag} from ${targetRepo}: `, err);
type CleanOptions = {
  releaseID?: string;
  tag: string;
export async function cleanReleaseArtifacts ({ releaseID, tag }: CleanOptions) {
  const releaseId = releaseID && releaseID.length > 0 ? releaseID : null;
  const isNightly = tag.includes('nightly');
  if (releaseId) {
    if (isNightly) {
      await deleteDraft(releaseId, NIGHTLY_REPO);
      // We only need to delete the Electron tag since the
      // nightly tag is only created at publish-time.
      await deleteTag(tag, ELECTRON_REPO);
      const deletedElectronDraft = await deleteDraft(releaseId, ELECTRON_REPO);
      // don't delete tag unless draft deleted successfully
      if (deletedElectronDraft) {
      deleteTag(tag, ELECTRON_REPO),
      deleteTag(tag, NIGHTLY_REPO)
  console.log(`${pass} failed release artifact cleanup complete`);
