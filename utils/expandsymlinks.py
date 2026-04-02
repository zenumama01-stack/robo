#      http://www.apache.org/licenses/LICENSE-2.0
"""Copy files from source to dest expanding symlinks along the way.
from shutil import copytree
# Ignore these files and directories when copying over files into the snapshot.
IGNORE = set([".hg", "httplib2", "oauth2", "simplejson", "static"])
# In addition to the above files also ignore these files and directories when
# copying over samples into the snapshot.
IGNORE_IN_SAMPLES = set(["googleapiclient", "oauth2client", "uritemplate"])
parser.add_argument("--source", default=".", help="Directory name to copy from.")
parser.add_argument("--dest", default="snapshot", help="Directory name to copy to.")
def _ignore(path, names):
    retval = set()
    if path != ".":
        retval = retval.union(IGNORE_IN_SAMPLES.intersection(names))
    retval = retval.union(IGNORE.intersection(names))
    return retval
    copytree(FLAGS.source, FLAGS.dest, symlinks=True, ignore=_ignore)
