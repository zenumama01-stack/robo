 * Inspired by: https://github.com/jupyterlab/jupyterlab/blob/master/buildutils/src/patch-release.ts
  .description('Create a patch release')
    // Make sure we can patch release.
    const pyVersion = getPythonVersion();
      pyVersion.includes('a') ||
      pyVersion.includes('b') ||
      pyVersion.includes('rc')
      throw new Error('Can only make a patch release from a final version');
    // Run pre-bump actions.
    // Patch the python version
    utils.run('hatch version patch');
    // Version the changed
    let cmd =
      'jlpm run lerna version patch --no-push --force-publish --no-git-tag-version';
    const commit = options.skipCommit !== true;
