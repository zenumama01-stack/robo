if (process.argv.length < 6) {
  console.log('Usage: upload-to-github filePath fileName releaseId');
const filePath = process.argv[2];
const fileName = process.argv[3];
const releaseId = parseInt(process.argv[4], 10);
const releaseVersion = process.argv[5];
if (isNaN(releaseId)) {
  throw new TypeError('Provided release ID was not a valid integer');
const getHeaders = (filePath: string, fileName: string) => {
  const extension = fileName.split('.').pop();
  if (!extension) {
    throw new Error(`Failed to get headers for extensionless file: ${fileName}`);
  console.log(`About to get size of ${filePath}`);
  const size = fs.statSync(filePath).size;
  console.log(`Got size of ${filePath}: ${size}`);
  const options: Record<string, string> = {
    json: 'text/json',
    zip: 'application/zip',
    txt: 'text/plain',
    ts: 'application/typescript'
    'content-type': options[extension],
    'content-length': size
  return releaseVersion.indexOf('nightly') > 0 ? NIGHTLY_REPO : ELECTRON_REPO;
let retry = 0;
let octokit = new Octokit({
  authStrategy: createGitHubTokenStrategy(targetRepo),
  log: console
function uploadToGitHub () {
  console.log(`in uploadToGitHub for ${filePath}, ${fileName}`);
  const fileData = fs.createReadStream(filePath);
  console.log(`in uploadToGitHub, created readstream for ${filePath}`);
  octokit.repos.uploadReleaseAsset({
    headers: getHeaders(filePath, fileName),
    data: fileData as any,
    release_id: releaseId
  }).then(() => {
    console.log(`Successfully uploaded ${fileName} to GitHub.`);
    process.exit();
    if (retry < 4) {
      console.log(`Error uploading ${fileName} to GitHub, will retry.  Error was:`, err);
      // Reset octokit in case it cached an auth error somehow
      octokit = new Octokit({
      octokit.repos.listReleaseAssets({
        release_id: releaseId,
      }).then(assets => {
        console.log('Got list of assets for existing release:');
        console.log(JSON.stringify(assets.data, null, '  '));
        const existingAssets = assets.data.filter(asset => asset.name === fileName);
          console.log(`${fileName} already exists; will delete before retrying upload.`);
          octokit.repos.deleteReleaseAsset({
          }).catch((deleteErr) => {
            console.log(`Failed to delete existing asset ${fileName}.  Error was:`, deleteErr);
          }).then(uploadToGitHub);
          console.log(`Current asset ${fileName} not found in existing assets; retrying upload.`);
          uploadToGitHub();
      }).catch((getReleaseErr) => {
        console.log('Fatal: Unable to get current release assets via getRelease!  Error was:', getReleaseErr);
        process.exitCode = 1;
      console.log(`Error retrying uploading ${fileName} to GitHub:`, err);
