SOURCE_ROOT = os.path.abspath(os.path.dirname(os.path.dirname(__file__)))
  args = parse_args()
  chromedriver_name = {
    'darwin': 'chromedriver',
    'win32': 'chromedriver.exe',
    'linux': 'chromedriver',
    'linux2': 'chromedriver'
  chromedriver_path = os.path.join(
    args.source_root, args.build_dir, chromedriver_name[sys.platform])
  with subprocess.Popen([chromedriver_path],
                        universal_newlines=True) as proc:
      output = proc.stdout.readline()
      returncode = 0
      proc.terminate()
  match = re.search(
    '^Starting ChromeDriver [0-9]+.[0-9]+.[0-9]+.[0-9]+ .* on port [0-9]+$',
  if match is None:
    returncode = 1
  if returncode == 0:
    print('ok ChromeDriver is able to be initialized.')
  return returncode
    parser=argparse.ArgumentParser(description='Test ChromeDriver')
    parser.add_argument('--source-root',
                        default=SOURCE_ROOT,
                        required=False)
    parser.add_argument('--build-dir',
                        default=None,
                        required=True)
