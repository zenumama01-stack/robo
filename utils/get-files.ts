export async function getFiles (
  test: ((file: string) => boolean) = (_: string) => true // eslint-disable-line @typescript-eslint/no-unused-vars
  return fs.promises.readdir(dir)
    .then(files => files.map(file => path.join(dir, file)))
    .then(files => files.filter(file => test(file)));
