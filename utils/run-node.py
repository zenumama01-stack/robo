  # Proxy all args to node script
  script = os.path.join(SOURCE_ROOT, sys.argv[1])
  subprocess.check_call(['node', script] + [str(x) for x in sys.argv[2:]])
