def main(argv):
  os.chdir(argv[1])
  p = subprocess.Popen(argv[2:])
  return p.wait()
  sys.exit(main(sys.argv))
