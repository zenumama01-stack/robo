def npm(*npm_args):
    call_args = [__get_executable_name()] + list(npm_args)
    subprocess.check_call(call_args)
def __get_executable_name():
    executable = 'npm'
        executable += '.cmd'
    return executable
    npm(*sys.argv[1:])
