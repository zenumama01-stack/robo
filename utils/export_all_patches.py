def export_patches(target, dry_run):
  git.export_patches(
    dry_run=dry_run,
    grep=target.get('grep'),
    out_dir=target.get('patch_dir'),
    repo=repo
def export_config(config, dry_run):
  for target in config:
    export_patches(target, dry_run)
  parser = argparse.ArgumentParser(description='Export Electron patches')
  parser.add_argument("-d", "--dry-run",
    help="Checks whether the exported patches need to be updated.",
    default=False, action='store_true')
  configs = parse_args().config
  dry_run = parse_args().dry_run
  for config_json in configs:
    export_config(json.load(config_json), dry_run)
