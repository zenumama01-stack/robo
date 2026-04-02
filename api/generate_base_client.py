from __future__ import absolute_import, division, print_function, unicode_literals
cmdline_desc = """\
Runs Stone to generate Python types and client for the Dropbox client. 
_cmdline_parser = argparse.ArgumentParser(description=cmdline_desc)
_cmdline_parser.add_argument(
    '-v',
    '--verbose',
    help='Print debugging statements.',
    nargs='*',
    help='Path to API specifications. Each must have a .stone extension.',
    """The entry point for the program."""
    args = _cmdline_parser.parse_args()
    verbose = args.verbose
    if args.spec:
        specs = args.spec
        # If no specs were specified, default to the spec submodule.
        specs = glob.glob('spec/*.stone')  # Arbitrary sorting
        specs.sort()
    specs = [os.path.join(os.getcwd(), s) for s in specs]
    dropbox_pkg_path = os.path.abspath(
        os.path.join(os.path.dirname(sys.argv[0]), 'dropbox'))
    if verbose:
        print('Dropbox package path: %s' % dropbox_pkg_path)
        print('Generating Python types')
        (['python', '-m', 'stone.cli', 'python_types', dropbox_pkg_path] +
         specs + ['-a', 'host', '-a', 'style', '-a', 'auth'] +
         ['--', '-r', 'dropbox.dropbox_client.Dropbox.{ns}_{route}', '-p', 'dropbox']))
        print('Generating Python client')
    o = subprocess.check_output(
        (['python', '-m', 'stone.cli', 'python_client', dropbox_pkg_path] +
         specs + ['-a', 'host', '-a', 'style', '-a', 'auth', '-a', 'scope'] +
             '--',
              '-w', 'user,app,noauth',
              '-m', 'base',
              '-c', 'DropboxBase',
              '-t', 'dropbox',
              '-a', 'scope'
         ]))
    if o:
        print('Output:', o)
             '-w', 'team',
             '-m', 'base_team',
             '-c', 'DropboxTeamBase',
