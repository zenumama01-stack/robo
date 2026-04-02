SOURCE_ROOT = os.path.dirname(os.path.dirname(__file__))
cmd = "npm"
if sys.platform == "win32":
    cmd += ".cmd"
args = [cmd, "run",
    "--prefix",
    SOURCE_ROOT
    ] + sys.argv[1:]
    subprocess.check_output(args, stderr=subprocess.STDOUT)
    error_msg = "NPM script '{}' failed with code '{}':\n".format(sys.argv[2], e.returncode)
    print(error_msg + e.output.decode('utf8'))
    sys.exit(e.returncode)
