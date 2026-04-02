"""Version info for notebook."""
# Copyright (c) Jupyter Development Team.
# Distributed under the terms of the Modified BSD License.
# Use "hatch version xx.yy.zz" to handle version changes
__version__ = "7.6.0a4"
# PEP440 version parser
_version_regex = re.compile(
    r"""
  (?P<major>\d+)
  \.
  (?P<minor>\d+)
  (?P<micro>\d+)
  (?P<releaselevel>((a|b|rc|\.dev)))?
  (?P<serial>\d+)?
  """,
    re.VERBOSE,
_version_fields = _version_regex.match(__version__).groupdict()  # type:ignore[union-attr]
VersionInfo = namedtuple("VersionInfo", ["major", "minor", "micro", "releaselevel", "serial"])  # noqa: PYI024
version_info = VersionInfo(
    *[
        for field in (
            int(_version_fields["major"]),
            int(_version_fields["minor"]),
            int(_version_fields["micro"]),
            _version_fields["releaselevel"] or "",
            _version_fields["serial"] or "",
__title__ = "openai"
__version__ = "2.30.0"  # x-release-please-version
Version module for spotdl.
__version__ = "4.4.3"
"""Version information for langchain-anthropic."""
__version__ = "1.4.0"
# This file is protected via CODEOWNERS
__version__ = "1.26.20"
__version__ = version = '2.6.3'
__version_tuple__ = version_tuple = (2, 6, 3)
# This file is imported from __init__.py and exec'd from setup.py
__version__ = "1.3.1"
# Master version for Pillow
__version__ = "12.1.1"
This module defines the version.
__version__ = "1.0.4"
"""Utility to compare (NumPy) version strings.
The NumpyVersion class allows properly comparing numpy version strings.
The LooseVersion and StrictVersion classes that distutils provides don't
work; they don't recognize anything like alpha/beta/rc/dev versions.
__all__ = ['NumpyVersion']
class NumpyVersion():
    """Parse and compare numpy version strings.
    NumPy has the following versioning scheme (numbers given are examples; they
    can be > 9 in principle):
    - Released version: '1.8.0', '1.8.1', etc.
    - Alpha: '1.8.0a1', '1.8.0a2', etc.
    - Beta: '1.8.0b1', '1.8.0b2', etc.
    - Release candidates: '1.8.0rc1', '1.8.0rc2', etc.
    - Development versions: '1.8.0.dev-f1234afa' (git commit hash appended)
    - Development versions after a1: '1.8.0a1.dev-f1234afa',
                                     '1.8.0b2.dev-f1234afa',
                                     '1.8.1rc1.dev-f1234afa', etc.
    - Development versions (no git hash available): '1.8.0.dev-Unknown'
    Comparing needs to be done against a valid version string or other
    `NumpyVersion` instance. Note that all development versions of the same
    (pre-)release compare equal.
    .. versionadded:: 1.9.0
    vstring : str
        NumPy version string (``np.__version__``).
    >>> from numpy.lib import NumpyVersion
    >>> if NumpyVersion(np.__version__) < '1.7.0':
    ...     print('skip')
    >>> # skip
    >>> NumpyVersion('1.7')  # raises ValueError, add ".0"
    ValueError: Not a valid numpy version string
    def __init__(self, vstring):
        ver_main = re.match(r'\d+\.\d+\.\d+', vstring)
        if not ver_main:
            raise ValueError("Not a valid numpy version string")
        self.version = ver_main.group()
        self.major, self.minor, self.bugfix = [int(x) for x in
            self.version.split('.')]
        if len(vstring) == ver_main.end():
            self.pre_release = 'final'
            alpha = re.match(r'a\d', vstring[ver_main.end():])
            beta = re.match(r'b\d', vstring[ver_main.end():])
            rc = re.match(r'rc\d', vstring[ver_main.end():])
            pre_rel = [m for m in [alpha, beta, rc] if m is not None]
            if pre_rel:
                self.pre_release = pre_rel[0].group()
                self.pre_release = ''
        self.is_devversion = bool(re.search(r'.dev', vstring))
    def _compare_version(self, other):
        """Compare major.minor.bugfix"""
        if self.major == other.major:
            if self.minor == other.minor:
                if self.bugfix == other.bugfix:
                    vercmp = 0
                elif self.bugfix > other.bugfix:
                    vercmp = 1
                    vercmp = -1
            elif self.minor > other.minor:
        elif self.major > other.major:
        return vercmp
    def _compare_pre_release(self, other):
        """Compare alpha/beta/rc/final."""
        if self.pre_release == other.pre_release:
        elif self.pre_release == 'final':
        elif other.pre_release == 'final':
        elif self.pre_release > other.pre_release:
    def _compare(self, other):
        if not isinstance(other, (str, NumpyVersion)):
            raise ValueError("Invalid object to compare with NumpyVersion.")
            other = NumpyVersion(other)
        vercmp = self._compare_version(other)
        if vercmp == 0:
            # Same x.y.z version, check for alpha/beta/rc
            vercmp = self._compare_pre_release(other)
                # Same version and same pre-release, check if dev version
                if self.is_devversion is other.is_devversion:
                elif self.is_devversion:
        return self._compare(other) < 0
        return self._compare(other) <= 0
        return self._compare(other) == 0
        return self._compare(other) != 0
        return self._compare(other) > 0
        return self._compare(other) >= 0
        return "NumpyVersion(%s)" % self.vstring
# This file must be kept very simple, because it is consumed from several
# places -- it is imported by h11/__init__.py, execfile'd by setup.py, etc.
# We use a simple scheme:
#   1.0.0 -> 1.0.0+dev -> 1.1.0 -> 1.1.0+dev
# where the +dev versions are never released into the wild, they're just what
# we stick into the VCS in between releases.
# This is compatible with PEP 440:
#   http://legacy.python.org/dev/peps/pep-0440/
# via the use of the "local suffix" "+dev", which is disallowed on index
# servers and causes 1.0.0+dev to sort after plain 1.0.0, which is what we
# want. (Contrast with the special suffix 1.0.0.dev, which sorts *before*
# 1.0.0.)
__version__ = "0.16.0"
__version__ = version = '2026.2.0'
__version_tuple__ = version_tuple = (2026, 2, 0)
# file generated by setuptools_scm
__version__ = version = '2.9.0.post0'
__version_tuple__ = version_tuple = (2, 9, 0)
__version__ = "2.20.0"  # x-release-please-version
import importlib_metadata
    version = importlib_metadata.version("litellm")
    version = "unknown"
__version__ = version = '0.86.2'
__version_tuple__ = version_tuple = (0, 86, 2)
