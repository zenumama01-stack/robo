from setuptools import setup
scriptFolder = os.path.dirname(os.path.realpath(__file__))
os.chdir(scriptFolder)
# Find version info from module (without importing the module):
with open('pyautogui/__init__.py', 'r') as fd:
    version = re.search(r'^__version__\s*=\s*[\'"]([^\'"]*)[\'"]',
                        fd.read(), re.MULTILINE).group(1)
# Use the README.md content for the long description:
with io.open("README.md", encoding="utf-8") as fileObj:
    long_description = fileObj.read()
setup(
    name='PyAutoGUI',
    version=version,
    url='https://github.com/asweigart/pyautogui',
    author='Al Sweigart',
    author_email='al@inventwithpython.com',
    description=('PyAutoGUI lets Python control the mouse and keyboard, and other GUI automation tasks. For Windows, macOS, and Linux, on Python 3 and 2.'),
    long_description=long_description,
    long_description_content_type="text/markdown",
    license='BSD',
    packages=['pyautogui'],
    test_suite='tests',
    install_requires=['pyobjc-core;platform_system=="Darwin"',
                      'pyobjc-framework-quartz;platform_system=="Darwin"',
                      'python3-Xlib;platform_system=="Linux" and python_version>="3.0"',
                      'python-xlib;platform_system=="Linux" and python_version<"3.0"',
                      'pymsgbox',
                      'pytweening>=1.0.4',
                      'pyscreeze>=0.1.21',
                      'pygetwindow>=0.0.5',
                      'mouseinfo'],
    keywords="gui automation test testing keyboard mouse cursor click press keystroke control",
    classifiers=[
        'Development Status :: 4 - Beta',
        'Environment :: Win32 (MS Windows)',
        'Environment :: X11 Applications',
        'Environment :: MacOS X',
        'Intended Audience :: Developers',
        'License :: OSI Approved :: BSD License',
        'Operating System :: OS Independent',
        'Programming Language :: Python',
        'Programming Language :: Python :: 3',
        'Programming Language :: Python :: 3.1',
        'Programming Language :: Python :: 3.2',
        'Programming Language :: Python :: 3.3',
        'Programming Language :: Python :: 3.4',
        'Programming Language :: Python :: 3.5',
        'Programming Language :: Python :: 3.6',
        'Programming Language :: Python :: 3.7',
        'Programming Language :: Python :: 3.8',
        'Programming Language :: Python :: 3.9',
        'Programming Language :: Python :: 3.10',
        'Programming Language :: Python :: 3.11',
SRC = os.path.abspath(os.path.dirname(__file__))
def get_version():
    with open(os.path.join(SRC, 'instaloader/__init__.py')) as f:
            m = re.match("__version__ = '(.*)'", line)
            if m:
                return m.group(1)
    raise SystemExit("Could not find version string.")
if sys.version_info < (3, 9):
    sys.exit('Instaloader requires Python >= 3.9.')
requirements = ['requests>=2.25']
optional_requirements = {
    'browser_cookie3': ['browser_cookie3>=0.19.1'],
keywords = (['instagram', 'instagram-scraper', 'instagram-client', 'instagram-feed', 'downloader', 'videos', 'photos',
             'pictures', 'instagram-user-photos', 'instagram-photos', 'instagram-metadata', 'instagram-downloader',
             'instagram-stories'])
# NOTE that many of the values defined in this file are duplicated on other places, such as the
    name='instaloader',
    version=get_version(),
    packages=['instaloader'],
    package_data={'instaloader': ['py.typed']},
    url='https://instaloader.github.io/',
    license='MIT',
    author='Alexander Graf, André Koch-Kramer',
    author_email='mail@agraf.me, koch-kramer@web.de',
    description='Download pictures (or videos) along with their captions and other metadata '
                'from Instagram.',
    long_description=open(os.path.join(SRC, 'README.rst')).read(),
    install_requires=requirements,
    python_requires='>=3.9',
    extras_require=optional_requirements,
    entry_points={'console_scripts': ['instaloader=instaloader.__main__:main']},
    zip_safe=False,
    keywords=keywords,
        'Development Status :: 5 - Production/Stable',
        'Environment :: Console',
        'Intended Audience :: End Users/Desktop',
        'License :: OSI Approved :: MIT License',
        'Programming Language :: Python :: 3.12',
        'Programming Language :: Python :: 3.13',
        'Programming Language :: Python :: 3.14',
        'Programming Language :: Python :: 3 :: Only',
        'Topic :: Internet',
        'Topic :: Multimedia :: Graphics'
# setup.py shim for use with versions of JupyterLab that require
# it for extensions.
__import__("setuptools").setup()
"""Setup script for Google API Python client.
Also installs included versions of third party libraries, if those libraries
are not already installed.
if sys.version_info < (3, 7):
    print("google-api-python-client requires python3 version >= 3.7.", file=sys.stderr)
packages = ["apiclient", "googleapiclient", "googleapiclient/discovery_cache"]
install_requires = [
    "httplib2>=0.19.0,<1.0.0",
    # NOTE: Maintainers, please do not require google-auth>=2.x.x
    # Until this issue is closed
    # https://github.com/googleapis/google-cloud-python/issues/10566
    "google-auth>=1.32.0,<3.0.0,!=2.24.0,!=2.25.0",
    "google-auth-httplib2>=0.2.0, <1.0.0",
    # NOTE: Maintainers, please do not require google-api-core>=2.x.x
    "google-api-core >= 1.31.5, <3.0.0,!=2.0.*,!=2.1.*,!=2.2.*,!=2.3.0",
    "uritemplate>=3.0.1,<5",
package_root = os.path.abspath(os.path.dirname(__file__))
readme_filename = os.path.join(package_root, "README.md")
with io.open(readme_filename, encoding="utf-8") as readme_file:
    readme = readme_file.read()
version = {}
with open(os.path.join(package_root, "googleapiclient/version.py")) as fp:
    exec(fp.read(), version)
version = version["__version__"]
    name="google-api-python-client",
    description="Google API Client Library for Python",
    long_description=readme,
    author="Google LLC",
    author_email="googleapis-packages@google.com",
    url="https://github.com/googleapis/google-api-python-client/",
    install_requires=install_requires,
    python_requires=">=3.7",
    packages=packages,
    package_data={"googleapiclient": ["discovery_cache/documents/*.json"]},
    license="Apache 2.0",
    keywords="google api client",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.7",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Programming Language :: Python :: 3.12",
        "Programming Language :: Python :: 3.13",
        "Programming Language :: Python :: 3.14",
        "Development Status :: 5 - Production/Stable",
        "Intended Audience :: Developers",
        "License :: OSI Approved :: Apache Software License",
        "Operating System :: OS Independent",
        "Topic :: Internet :: WWW/HTTP",
# Don't import unicode_literals because of a bug in py2 setuptools
# where package_data is expected to be str and not unicode.
# Ensure setuptools is available
    # Try to use ez_setup, but if not, continue anyway. The import is known
    # to fail when installing from a tar.gz.
    print('Could not import ez_setup', file=sys.stderr)
dbx_mod_path = os.path.join(os.path.dirname(__file__), 'dropbox/dropbox_client.py')
line = '= "UNKNOWN"'
for line in open(dbx_mod_path):
    if line.startswith('__version__'):
version = eval(line.split('=', 1)[1].strip())  # pylint: disable=eval-used
install_reqs = [
    'requests>=2.16.2',
    'six >= 1.12.0',
    'stone>=2,<=3.3.9',
setup_requires = [
    # Pin pytest-runner to 5.2.0, since 5.3.0 uses `find_namespaces` directive, not supported in
    # Python 2.7
    'pytest-runner==5.2.0',
# WARNING: This imposes limitations on test/requirements.txt such that the
# full Pip syntax is not supported. See also
# <http://stackoverflow.com/questions/14399534/>.
test_reqs = []
with open('test/requirements.txt') as f:
    test_reqs += f.read().splitlines()
with codecs.open('README.rst', encoding='utf-8') as f:
    README = f.read()
dist = setup(
    name='dropbox',
    install_requires=install_reqs,
    setup_requires=setup_requires,
    tests_require=test_reqs,
    packages=['dropbox'],
    author_email='dev-platform@dropbox.com',
    author='Dropbox',
    description='Official Dropbox API Client',
    license='MIT License',
    long_description=README,
    url='http://www.dropbox.com/developers',
    project_urls={
        'Source': 'https://github.com/dropbox/dropbox-sdk-python',
    # From <https://pypi.python.org/pypi?%3Aaction=list_classifiers>
        'Programming Language :: Python :: 2.7',
        'Programming Language :: Python :: Implementation :: CPython',
        'Programming Language :: Python :: Implementation :: PyPy',
        'Topic :: Software Development :: Libraries :: Python Modules',
from __future__ import with_statement
from codecs import open
from os import path
here = path.abspath(path.dirname(__file__))
with open(path.join(here, 'DESCRIPTION.rst'), encoding='utf-8') as f:
    long_description = f.read()
with open(path.join(here, 'src/onedrivesdk/version.txt'), encoding='utf-8') as f:
    version = f.read()
    package_list = ['onedrivesdk',
                    'onedrivesdk.request',
                    'onedrivesdk.model',
                    'onedrivesdk.extensions',
                    'onedrivesdk.helpers']
    if sys.version_info >= (3, 4):
        base_dir = 'python3'
        package_list.append('onedrivesdk.version_bridge')
        base_dir = 'python2'
        name='onedrivesdk',
        description='Official Python OneDrive SDK for interfacing with OneDrive APIs',
        url='http://dev.onedrive.com',
        author='Microsoft',
        author_email='',
            'Operating System :: POSIX',
            'Operating System :: Microsoft :: Windows',
            'Operating System :: MacOS :: MacOS X',
        keywords='onedrive sdk microsoft',
        packages=package_list,
        package_dir={'onedrivesdk': 'src/onedrivesdk',
                     'onedrivesdk.request': 'src/' + base_dir + '/request'},
        package_data={'onedrivesdk': ['version.txt']},
        install_requires=['requests>=2.6.1'],
        extras_require={
            "samples": ["Pillow"],
            "tests": ["Mock"]
        test_suite='testonedrivesdk'
# Copyright (c) 2020-2023, PyInstaller Development Team.
# This assists in creating an ``.egg`` package for use with the ``test_pkg_resources_provider``. To build the package,
# execute ``python setup.py bdist_egg``.
setuptools.setup(
    name='pyi_pkgres_testpkg',
    version='1.0.0',
    setup_requires="setuptools >= 40.0.0",
    author='PyInstaller development team',
    packages=setuptools.find_packages(),
    package_data={
        "pyi_pkgres_testpkg": [
            "subpkg1/data/*.txt",
            "subpkg1/data/*.md",
            "subpkg1/data/*.rst",
            "subpkg1/data/extra/*.json",
            "subpkg3/*.json",
    name='pyi_example_package',
    packages=['pyi_example_package'],
    entry_points={'pyinstaller40': ['hooks = pyi_example_package.__init__:get_hook_dirs']}
# This assists in creating a ``.egg`` package for use with testing ``collect_submodules``.
# To do so, execute ``python setup.py bdist_egg``.
from setuptools import setup, find_packages
    name='hookutils_egg',
    zip_safe=True,
    packages=find_packages(),
    # Manually include the fake extension modules for testing. They are not automatically included.
    package_data={'hookutils_package': ['pyextension.*']},
    name="nspkg",
    version="1.0",
    namespace_packages=['nspkg', 'nspkg.nssubpkg'],
    packages=['nspkg', 'nspkg.nssubpkg'],
    package_dir = {'': 'src'},
#                   ,*++++++*,                ,*++++++*,
#                *++.        .+++          *++.        .++*
#              *+*     ,++++*   *+*      *+*   ,++++,     *+*
#             ,+,   .++++++++++* ,++,,,,*+, ,++++++++++.   *+,
#             *+.  .++++++++++++..++    *+.,++++++++++++.  .+*
#             .+*   ++++++++++++.*+,    .+*.++++++++++++   *+,
#              .++   *++++++++* ++,      .++.*++++++++*   ++,
#               ,+++*.    . .*++,          ,++*.      .*+++*
#              *+,   .,*++**.                  .**++**.   ,+*
#             .+*                                          *+,
#             *+.                   Coqui                  .+*
#             *+*              +++   TTS  +++              *+*
#             .+++*.            .          .             *+++.
#              ,+* *+++*...                       ...*+++* *+,
#               .++.    .""""+++++++****+++++++"""".     ++.
#                 ,++.                                .++,
#                   .++*                            *++.
#                       *+++,                  ,+++*
#                           .,*++++::::::++++*,.
#                                  ``````
from packaging.version import Version
import setuptools.command.build_py
import setuptools.command.develop
from Cython.Build import cythonize
from setuptools import Extension, find_packages, setup
python_version = sys.version.split()[0]
if Version(python_version) < Version("3.9") or Version(python_version) >= Version("3.12"):
    raise RuntimeError("TTS requires python >= 3.9 and < 3.12 " "but your Python version is {}".format(sys.version))
cwd = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(cwd, "TTS", "VERSION")) as fin:
    version = fin.read().strip()
class build_py(setuptools.command.build_py.build_py):  # pylint: disable=too-many-ancestors
        setuptools.command.build_py.build_py.run(self)
class develop(setuptools.command.develop.develop):
        setuptools.command.develop.develop.run(self)
# The documentation for this feature is in server/README.md
package_data = ["TTS/server/templates/*"]
def pip_install(package_name):
    subprocess.call([sys.executable, "-m", "pip", "install", package_name])
requirements = open(os.path.join(cwd, "requirements.txt"), "r").readlines()
with open(os.path.join(cwd, "requirements.notebooks.txt"), "r") as f:
    requirements_notebooks = f.readlines()
with open(os.path.join(cwd, "requirements.dev.txt"), "r") as f:
    requirements_dev = f.readlines()
with open(os.path.join(cwd, "requirements.ja.txt"), "r") as f:
    requirements_ja = f.readlines()
requirements_all = requirements_dev + requirements_notebooks + requirements_ja
with open("README.md", "r", encoding="utf-8") as readme_file:
    README = readme_file.read()
exts = [
    Extension(
        name="TTS.tts.utils.monotonic_align.core",
        sources=["TTS/tts/utils/monotonic_align/core.pyx"],
    name="TTS",
    url="https://github.com/coqui-ai/TTS",
    author="Eren Gölge",
    author_email="egolge@coqui.ai",
    description="Deep learning for Text to Speech by Coqui.",
    license="MPL-2.0",
    # cython
    include_dirs=numpy.get_include(),
    ext_modules=cythonize(exts, language_level=3),
    # ext_modules=find_cython_extensions(),
    # package
    include_package_data=True,
    packages=find_packages(include=["TTS"], exclude=["*.tests", "*tests.*", "tests.*", "*tests", "tests"]),
        "TTS": [
            "VERSION",
        "Documentation": "https://github.com/coqui-ai/TTS/wiki",
        "Tracker": "https://github.com/coqui-ai/TTS/issues",
        "Repository": "https://github.com/coqui-ai/TTS",
        "Discussions": "https://github.com/coqui-ai/TTS/discussions",
    cmdclass={
        "build_py": build_py,
        "develop": develop,
        # 'build_ext': build_ext
        "all": requirements_all,
        "dev": requirements_dev,
        "notebooks": requirements_notebooks,
        "ja": requirements_ja,
    python_requires=">=3.9.0, <3.12",
    entry_points={"console_scripts": ["tts=TTS.bin.synthesize:main", "tts-server = TTS.server.server:main"]},
        "Programming Language :: Python",
        "Development Status :: 3 - Alpha",
        "Intended Audience :: Science/Research",
        "Operating System :: POSIX :: Linux",
        "License :: OSI Approved :: Mozilla Public License 2.0 (MPL 2.0)",
        "Topic :: Software Development",
        "Topic :: Software Development :: Libraries :: Python Modules",
        "Topic :: Multimedia :: Sound/Audio :: Speech",
        "Topic :: Multimedia :: Sound/Audio",
        "Topic :: Multimedia",
        "Topic :: Scientific/Engineering :: Artificial Intelligence",
# from distutils.core import setup
# from Cython.Build import cythonize
# import numpy
# setup(name='monotonic_align',
#       ext_modules=cythonize("core.pyx"),
#       include_dirs=[numpy.get_include()])
# Figure out environment for cross-compile
vosk_source = os.getenv("VOSK_SOURCE", os.path.abspath(os.path.join(os.path.dirname(__file__),
    "..")))
system = os.environ.get('VOSK_SYSTEM', platform.system())
architecture = os.environ.get('VOSK_ARCHITECTURE', platform.architecture()[0])
machine = os.environ.get('VOSK_MACHINE', platform.machine())
# Copy precompmilled libraries
for lib in glob.glob(os.path.join(vosk_source, "src/lib*.*")):
    print ("Adding library", lib)
    shutil.copy(lib, "vosk")
# Create OS-dependent, but Python-independent wheels.
    from wheel.bdist_wheel import bdist_wheel
    cmdclass = {}
    class bdist_wheel_tag_name(bdist_wheel):
        def get_tag(self):
            abi = 'none'
            if system == 'Darwin':
                oses = 'macosx_10_6_universal2'
            elif system == 'Windows' and architecture == '32bit':
                oses = 'win32'
            elif system == 'Windows' and architecture == '64bit':
                oses = 'win_amd64'
            elif system == 'Linux' and machine == 'aarch64' and architecture == '64bit':
                oses = 'manylinux2014_aarch64'
            elif system == 'Linux':
                oses = 'linux_' + machine
                raise TypeError("Unknown build environment")
            return 'py3', abi, oses
    cmdclass = {'bdist_wheel': bdist_wheel_tag_name}
with open("README.md", "rb") as fh:
    long_description = fh.read().decode("utf-8")
    name="vosk",
    version="0.3.75",
    author="Alpha Cephei Inc",
    author_email="contact@alphacephei.com",
    description="Offline open source speech recognition API based on Kaldi and Vosk",
    url="https://github.com/alphacep/vosk-api",
    package_data = {'vosk': ['*.so', '*.dll', '*.dyld']},
    entry_points = {
        'console_scripts': ['vosk-transcriber=vosk.transcriber.cli:main'],
        'License :: OSI Approved :: Apache Software License',
        'Operating System :: POSIX :: Linux',
        'Topic :: Software Development :: Libraries :: Python Modules'
    cmdclass=cmdclass,
    python_requires='>=3',
    zip_safe=False, # Since we load so file from the filesystem, we can not run from zip file
    setup_requires=['cffi>=1.0', 'requests', 'tqdm', 'srt', 'websockets'],
    install_requires=['cffi>=1.0', 'requests', 'tqdm', 'srt', 'websockets'],
    cffi_modules=['vosk_builder.py:ffibuilder'],
def configuration(parent_package="", top_path=None):
    from numpy.distutils.misc_util import Configuration
    config = Configuration("array_api", parent_package, top_path)
    config.add_subpackage("tests")
    from numpy.distutils.core import setup
    setup(configuration=configuration)
def configuration(parent_package='',top_path=None):
    config = Configuration('compat', parent_package, top_path)
    config.add_subpackage('tests')
Provide python-space access to the functions exposed in numpy/__init__.pxd
for testing.
macros = [("NPY_NO_DEPRECATED_API", 0)]
checks = Extension(
    "checks",
    sources=[os.path.join('.', "checks.pyx")],
    include_dirs=[np.get_include()],
    define_macros=macros,
extensions = [checks]
    ext_modules=cythonize(extensions)
Build an example package using the limited Python C API.
from setuptools import setup, Extension
macros = [("NPY_NO_DEPRECATED_API", 0), ("Py_LIMITED_API", "0x03060000")]
limited_api = Extension(
    "limited_api",
    sources=[os.path.join('.', "limited_api.c")],
extensions = [limited_api]
    ext_modules=extensions
    config = Configuration('distutils', parent_package, top_path)
    config.add_subpackage('command')
    config.add_subpackage('fcompiler')
    config.add_data_files('site.cfg')
    config.add_data_files('mingw/gfortran_vs2003_hack.c')
    config.add_data_dir('checks')
    config.add_data_files('*.pyi')
    config.make_config_py()
    from numpy.distutils.core      import setup
setup.py for installing F2PY
   pip install .
Copyright 2001-2005 Pearu Peterson all rights reserved,
Pearu Peterson <pearu@cens.ioc.ee>
Permission to use, modify, and distribute this software is given under the
terms of the NumPy License.
NO WARRANTY IS EXPRESSED OR IMPLIED.  USE AT YOUR OWN RISK.
$Revision: 1.32 $
$Date: 2005/01/30 17:22:14 $
Pearu Peterson
from __version__ import version
def configuration(parent_package='', top_path=None):
    config = Configuration('f2py', parent_package, top_path)
    config.add_subpackage('_backends')
    config.add_data_dir('tests/src')
    config.add_data_files(
        'src/fortranobject.c',
        'src/fortranobject.h',
        '_backends/meson.build.template',
    config = configuration(top_path='')
    config = config.todict()
    config['classifiers'] = [
        'Intended Audience :: Science/Research',
        'License :: OSI Approved :: NumPy License',
        'Natural Language :: English',
        'Programming Language :: C',
        'Programming Language :: Fortran',
        'Topic :: Scientific/Engineering',
        'Topic :: Software Development :: Code Generators',
    setup(version=version,
          description="F2PY - Fortran to Python Interface Generator",
          author="Pearu Peterson",
          author_email="pearu@cens.ioc.ee",
          maintainer="Pearu Peterson",
          maintainer_email="pearu@cens.ioc.ee",
          license="BSD",
          platforms="Unix, Windows (mingw|cygwin), Mac OSX",
          long_description="""\
The Fortran to Python Interface Generator, or F2PY for short, is a
command line tool (f2py) for generating Python C/API modules for
wrapping Fortran 77/90/95 subroutines, accessing common blocks from
Python, and calling Python functions from Fortran (call-backs).
Interfacing subroutines/data from Fortran 90/95 modules is supported.""",
          url="https://numpy.org/doc/stable/f2py/",
          keywords=['Fortran', 'f2py'],
          **config)
    config = Configuration('lib', parent_package, top_path)
    config.add_data_dir('tests/data')
    config = Configuration('ma', parent_package, top_path)
    config = configuration(top_path='').todict()
    setup(**config)
    config = Configuration('matrixlib', parent_package, top_path)
    config = Configuration('polynomial', parent_package, top_path)
    config = Configuration('testing', parent_package, top_path)
    config.add_subpackage('_private')
    config.add_data_files('_private/*.pyi')
    setup(maintainer="NumPy Developers",
          maintainer_email="numpy-dev@numpy.org",
          description="NumPy test module",
          url="https://www.numpy.org",
          license="NumPy License (BSD Style)",
          configuration=configuration,
    config = Configuration('typing', parent_package, top_path)
    config = Configuration('_typing', parent_package, top_path)
Setup configuration for Jarvis Voice Assistant
Install with: pip install -e .
or: python setup.py install
# Read requirements
requirements_file = Path(__file__).parent / "requirements.txt"
requirements = [line.strip() for line in requirements_file.read_text().split('\n') if line.strip() and not line.startswith('#')]
# Read README
readme_file = Path(__file__).parent / "README_JARVIS.md"
long_description = readme_file.read_text() if readme_file.exists() else ""
    name="jarvis-voice-assistant",
    version="2.0.0",
    author="Zia",
    description="Advanced Voice Assistant with Beautiful UI",
    url="https://github.com/yourusername/jarvis",
    license="MIT",
    python_requires=">=3.8",
    entry_points={
        'console_scripts': [
            'jarvis=Jarvis-Desktop-Voice-Assistant.main_ui:main',
        "Operating System :: Microsoft :: Windows",
        "Intended Audience :: End Users/Desktop",
    keywords="voice assistant AI chatbot speech recognition",
