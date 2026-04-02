from lib.util import get_depot_tools_executable
# Helper to run gn format on multiple files
# (gn only formats a single file at a time)
  new_env = os.environ.copy()
  new_env['DEPOT_TOOLS_WIN_TOOLCHAIN'] = '0'
  new_env['CHROMIUM_BUILDTOOLS_PATH'] = os.path.realpath(
    os.path.join(SOURCE_ROOT, '..', 'buildtools')
  gn_path = get_depot_tools_executable('gn')
  for gn_file in sys.argv[1:]:
      [gn_path, 'format', gn_file],
      env=new_env
