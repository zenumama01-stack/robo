#!/usr/bin/python
# Copyright 2014 Google Inc. All Rights Reserved.
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#     http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""Create documentation for generate API surfaces.
Command-line tool that creates documentation for all APIs listed in discovery.
The documentation is generated from a combination of the discovery document and
the generated API surface itself.
from __future__ import print_function
__author__ = "jcgregorio@google.com (Joe Gregorio)"
import uritemplate
from googleapiclient.discovery import DISCOVERY_URI, build_from_document
from googleapiclient.http import build_http
DISCOVERY_DOC_DIR = (
    pathlib.Path(__file__).resolve().parent
    / "googleapiclient"
    / "discovery_cache"
    / "documents"
CSS = """<style>
body, h1, h2, h3, div, span, p, pre, a {
  font-style: inherit;
  font-size: 100%;
  vertical-align: baseline;
pre, code {
  font-family: Monaco, 'DejaVu Sans Mono', 'Bitstream Vera Sans Mono', 'Lucida Console', monospace;
  margin-top: 0.5em;
h1, h2, h3, p {
  font-family: Arial, sans serif;
  border-bottom: solid #CCC 1px;
.toc_element {
.firstline {
  margin-left: 2 em;
.method  {
  border: solid 1px #CCC;
  background: #EEE;
.details {
METHOD_TEMPLATE = """<div class="method">
    <code class="details" id="$name">$name($params)</code>
  <pre>$doc</pre>
COLLECTION_LINK = """<p class="toc_element">
  <code><a href="$href">$name()</a></code>
<p class="firstline">Returns the $name Resource.</p>
METHOD_LINK = """<p class="toc_element">
  <code><a href="#$name">$name($params)</a></code></p>
<p class="firstline">$firstline</p>"""
BASE = pathlib.Path(__file__).resolve().parent / "docs" / "dyn"
# Obtain the discovery index and artifacts from googleapis/discovery-artifact-manager
DIRECTORY_URI = "https://raw.githubusercontent.com/googleapis/discovery-artifact-manager/master/discoveries/index.json"
DISCOVERY_URI_TEMPLATE = "https://raw.githubusercontent.com/googleapis/discovery-artifact-manager/master/discoveries/{api}.{apiVersion}.json"
    "--discovery_uri_template",
    default=DISCOVERY_URI_TEMPLATE,
    help="URI Template for discovery.",
    "--discovery_uri",
    default="",
    help=(
        "URI of discovery document. If supplied then only "
        "this API will be documented."
    "--directory_uri",
    default=DIRECTORY_URI,
    help=("URI of directory document. Unused if --discovery_uri" " is supplied."),
    "--dest", default=BASE, help="Directory name to write documents into."
def safe_version(version):
    """Create a safe version of the verion string.
    Needed so that we can distinguish between versions
    and sub-collections in URIs. I.e. we don't want
    adsense_v1.1 to refer to the '1' collection in the v1
    version of the adsense api.
      version: string, The version string.
      The string with '.' replaced with '_'.
    return version.replace(".", "_")
def unsafe_version(version):
    """Undoes what safe_version() does.
    See safe_version() for the details.
      version: string, The safe version string.
      The string with '_' replaced with '.'.
    return version.replace("_", ".")
def method_params(doc):
    """Document the parameters of a method.
      doc: string, The method's docstring.
      The method signature as a string.
    doclines = doc.splitlines()
    if "Args:" in doclines:
        begin = doclines.index("Args:")
        if "Returns:" in doclines[begin + 1 :]:
            end = doclines.index("Returns:", begin)
            args = doclines[begin + 1 : end]
            args = doclines[begin + 1 :]
        parameters = []
        sorted_parameters = []
        pname = None
        desc = ""
        def add_param(pname, desc):
            if pname is None:
            if "(required)" not in desc:
                pname = pname + "=None"
                parameters.append(pname)
                # required params should be put straight into sorted_parameters
                # to maintain order for positional args
                sorted_parameters.append(pname)
        for line in args:
            m = re.search(r"^\s+([a-zA-Z0-9_]+): (.*)", line)
                desc += line
            add_param(pname, desc)
            pname = m.group(1)
            desc = m.group(2)
        sorted_parameters.extend(sorted(parameters))
        sorted_parameters = ", ".join(sorted_parameters)
        sorted_parameters = ""
    return sorted_parameters
def method(name, doc):
    """Documents an individual method.
      name: string, Name of the method.
      doc: string, The methods docstring.
    import html
    params = method_params(doc)
    doc = html.escape(doc)
    return string.Template(METHOD_TEMPLATE).substitute(
        name=name, params=params, doc=doc
def breadcrumbs(path, root_discovery):
    """Create the breadcrumb trail to this page of documentation.
      path: string, Dot separated name of the resource.
      root_discovery: Deserialized discovery document.
      HTML with links to each of the parent resources of this resource.
    parts = path.split(".")
    crumbs = []
    accumulated = []
    for i, p in enumerate(parts):
        prefix = ".".join(accumulated)
        # The first time through prefix will be [], so we avoid adding in a
        # superfluous '.' to prefix.
        if prefix:
            prefix += "."
        display = p
        if i == 0:
            display = root_discovery.get("title", display)
        crumbs.append('<a href="{}.html">{}</a>'.format(prefix + p, display))
        accumulated.append(p)
    return " . ".join(crumbs)
def document_collection(resource, path, root_discovery, discovery, css=CSS):
    """Document a single collection in an API.
      resource: Collection or service being documented.
      discovery: Deserialized discovery document, but just the portion that
        describes the resource.
      css: string, The CSS to include in the generated file.
    collections = []
    methods = []
    resource_name = path.split(".")[-2]
    html = [
        "<html><body>",
        css,
        "<h1>%s</h1>" % breadcrumbs(path[:-1], root_discovery),
        "<h2>Instance Methods</h2>",
    # Which methods are for collections.
    for name in dir(resource):
        if not name.startswith("_") and callable(getattr(resource, name)):
            if hasattr(getattr(resource, name), "__is_resource__"):
                collections.append(name)
                methods.append(name)
    # TOC
    if collections:
        for name in collections:
                href = path + name + ".html"
                html.append(
                    string.Template(COLLECTION_LINK).substitute(href=href, name=name)
    if methods:
        for name in methods:
                doc = getattr(resource, name).__doc__
                firstline = doc.splitlines()[0]
                    string.Template(METHOD_LINK).substitute(
                        name=name, params=params, firstline=firstline
        html.append("<h3>Method Details</h3>")
            dname = name.rsplit("_")[0]
            html.append(method(name, getattr(resource, name).__doc__))
    html.append("</body></html>")
    return "\n".join(html)
def document_collection_recursive(
    root_discovery,
    discovery,
    doc_destination_dir,
    artifact_destination_dir=DISCOVERY_DOC_DIR,
    html = document_collection(resource, path, root_discovery, discovery)
    f = open(pathlib.Path(doc_destination_dir).joinpath(path + "html"), "w")
    f.write(html)
    f.close()
            not name.startswith("_")
            and callable(getattr(resource, name))
            and hasattr(getattr(resource, name), "__is_resource__")
            and discovery != {}
            collection = getattr(resource, name)()
            document_collection_recursive(
                path + name + ".",
                discovery["resources"].get(dname, {}),
                artifact_destination_dir,
def document_api(
    """Document the given API.
        name (str): Name of the API.
        version (str): Version of the API.
        uri (str): URI of the API's discovery document
        doc_destination_dir (str): relative path where the reference
            documentation should be saved.
        artifact_destination_dir (Optional[str]): relative path where the discovery
            artifacts should be saved.
    http = build_http()
    resp, content = http.request(uri)
        discovery = json.loads(content)
        service = build_from_document(discovery)
        doc_name = "{}.{}.json".format(name, version)
        discovery_file_path = artifact_destination_dir / doc_name
        revision = None
        pathlib.Path(discovery_file_path).touch(exist_ok=True)
        # Write discovery artifact to disk if revision equal or newer
        with open(discovery_file_path, "r+") as f:
                json_data = json.load(f)
                revision = json_data["revision"]
            except json.JSONDecodeError:
            if revision is None or discovery["revision"] >= revision:
                # Reset position to the beginning
                f.seek(0)
                # Write the changes to disk
                json.dump(discovery, f, indent=0, sort_keys=True)
                # Truncate anything left as it's not needed
                f.truncate()
    elif resp.status == 404:
            "Warning: {} {} not found. HTTP Code: {}".format(name, version, resp.status)
            "Warning: {} {} could not be built. HTTP Code: {}".format(
                name, version, resp.status
        "{}_{}.".format(name, safe_version(version)),
def document_api_from_discovery_document(
    discovery_url, doc_destination_dir, artifact_destination_dir=DISCOVERY_DOC_DIR
      discovery_url (str): URI of discovery document.
      artifact_destination_dir (str): relative path where the discovery
    response, content = http.request(discovery_url)
    name = discovery["version"]
    version = safe_version(discovery["version"])
        "{}_{}.".format(name, version),
def generate_all_api_documents(
    directory_uri=DIRECTORY_URI,
    doc_destination_dir=BASE,
    discovery_uri_template=DISCOVERY_URI_TEMPLATE,
    """Retrieve discovery artifacts and fetch reference documentations
    for all apis listed in the public discovery directory.
    args:
        directory_uri (Optional[str]): uri of the public discovery directory.
        doc_destination_dir (Optional[str]): relative path where the reference
        discovery_uri_template (Optional[str]): URI template of the API's discovery
            document.
    api_directory = collections.defaultdict(list)
    resp, content = http.request(directory_uri)
        directory = json.loads(content)["items"]
        for api in directory:
            uri = uritemplate.expand(
                discovery_uri_template or api["discoveryRestUrl"],
                {"api": api["name"], "apiVersion": api["version"]},
            document_api(
                api["name"],
                api["version"],
            api_directory[api["name"]].append(api["version"])
        # sort by api name and version number
        for api in api_directory:
            api_directory[api] = sorted(api_directory[api])
        api_directory = collections.OrderedDict(
            sorted(api_directory.items(), key=lambda x: x[0])
        markdown = []
        for api, versions in api_directory.items():
            markdown.append("## %s" % api)
            for version in versions:
                markdown.append(
                    "* [%s](http://googleapis.github.io/google-api-python-client/docs/dyn/%s_%s.html)"
                    % (version, api, safe_version(version))
            markdown.append("\n")
        with open(doc_destination_dir / "index.md", "w") as f:
            markdown = "\n".join(markdown)
            f.write(markdown)
        sys.exit("Failed to load the discovery document.")
    FLAGS = parser.parse_args(sys.argv[1:])
    if FLAGS.discovery_uri:
        document_api_from_discovery_document(
            discovery_url=FLAGS.discovery_uri,
            doc_destination_dir=FLAGS.dest,
        generate_all_api_documents(
            directory_uri=FLAGS.directory_uri,
            discovery_uri_template=FLAGS.discovery_uri_template,
../describe.py