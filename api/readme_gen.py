# Copyright 2024 Google LLC
"""Generates READMEs using configuration defined in yaml."""
import yaml
jinja_env = jinja2.Environment(
    trim_blocks=True,
    loader=jinja2.FileSystemLoader(
        os.path.abspath(os.path.join(os.path.dirname(__file__), "templates"))
README_TMPL = jinja_env.get_template("README.tmpl.rst")
def get_help(file):
    return subprocess.check_output(["python", file, "--help"]).decode()
    parser.add_argument("source")
    parser.add_argument("--destination", default="README.rst")
    source = os.path.abspath(args.source)
    root = os.path.dirname(source)
    destination = os.path.join(root, args.destination)
    jinja_env.globals["get_help"] = get_help
    with io.open(source, "r") as f:
        config = yaml.load(f)
    # This allows get_help to execute in the right directory.
    os.chdir(root)
    output = README_TMPL.render(config)
    with io.open(destination, "w") as f:
        f.write(output)
