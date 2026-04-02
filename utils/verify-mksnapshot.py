import glob
from lib.util import get_electron_branding, rm_rf, scoped_cwd
SNAPSHOT_SOURCE = os.path.join(SOURCE_ROOT, 'spec', 'fixtures', 'testsnap.js')
    with scoped_cwd(app_path):
      if args.snapshot_files_dir is None:
        snapshot_filename = os.path.join(app_path, 'mksnapshot_args')
        with open(snapshot_filename, encoding='utf-8') as file_in:
          mkargs = file_in.read().splitlines()
        print('running: ' + ' '.join(mkargs + [ SNAPSHOT_SOURCE ]))
        subprocess.check_call(mkargs + [ SNAPSHOT_SOURCE ], cwd=app_path)
        print('ok mksnapshot successfully created snapshot_blob.bin.')
        context_snapshot = 'v8_context_snapshot.bin'
        if platform.system() == 'Darwin':
          if os.environ.get('TARGET_ARCH') == 'arm64':
            context_snapshot = 'v8_context_snapshot.arm64.bin'
            context_snapshot = 'v8_context_snapshot.x86_64.bin'
        context_snapshot_path = os.path.join(app_path, context_snapshot)
        gen_binary = get_binary_path('v8_context_snapshot_generator', \
                                    app_path)
        genargs = [ gen_binary, \
                  f'--output_file={context_snapshot_path}' ]
        print('running: ' + ' '.join(genargs))
        subprocess.check_call(genargs)
        print('ok v8_context_snapshot_generator successfully created ' \
              + context_snapshot)
        if args.create_snapshot_only:
        gen_bin_path = os.path.join(args.snapshot_files_dir, '*.bin')
        generated_bin_files = glob.glob(gen_bin_path)
        for bin_file in generated_bin_files:
          shutil.copy2(bin_file, app_path)
      test_path = os.path.join(SOURCE_ROOT, 'spec', 'fixtures', \
                               'snapshot-items-available')
        bin_files = glob.glob(os.path.join(app_path, '*.bin'))
        app_dir = os.path.join(app_path, f'{PRODUCT_NAME}.app')
        electron = os.path.join(app_dir, 'Contents', 'MacOS', PRODUCT_NAME)
        bin_out_path = os.path.join(app_dir, 'Contents', 'Frameworks',
                  f'{PROJECT_NAME} Framework.framework',
                  'Resources')
        for bin_file in bin_files:
          shutil.copy2(bin_file, bin_out_path)
      print('running: ' + ' '.join([electron, test_path]))
      subprocess.check_call([electron, test_path])
      print('ok successfully used custom snapshot.')
    print('not ok an error was encountered while testing mksnapshot.')
    print('Other error')
  print(f'Returning with error code: {returncode}')
# Create copy of app to install custom snapshot
  print('Creating copy of app for testing')
                          + '-mksnapshot-test')
def get_binary_path(binary_name, root_path):
    binary_path = os.path.join(root_path, f'{binary_name}.exe')
    binary_path = os.path.join(root_path, binary_name)
  return binary_path
  parser = argparse.ArgumentParser(description='Test mksnapshot')
  parser.add_argument('--create-snapshot-only',
                      help='Just create snapshot files, but do not run test',
  parser.add_argument('--snapshot-files-dir',
                      help='Directory containing snapshot files to use \
                          for testing',
