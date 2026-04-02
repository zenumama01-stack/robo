def npx(*npx_args):
    npx_env = os.environ.copy()
    npx_env['npm_config_yes'] = 'true'
    call_args = [__get_executable_name()] + list(npx_args)
    subprocess.check_call(call_args, env=npx_env)
    executable = 'npx'
    npx(*sys.argv[1:])
