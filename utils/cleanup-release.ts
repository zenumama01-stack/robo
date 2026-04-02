import { cleanReleaseArtifacts } from '../release-artifact-cleanup';
const { values: { tag: _tag, releaseID } } = parseArgs({
    tag: {
    releaseID: {
      default: ''
if (!_tag) {
  console.error('Missing --tag argument');
const tag = _tag;
cleanReleaseArtifacts({
  releaseID,
  tag
