  js2c = sys.argv[1]
  root = sys.argv[2]
  natives = sys.argv[3]
  js_source_files = sys.argv[4:]
  subprocess.check_call(
    [js2c, natives] + js_source_files + ['--only-js', "--root", root])
  sys.exit(main())