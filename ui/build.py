from collections import OrderedDict
SENTINEL = "dL7pKGdnNz796PbbjQWNKmHXBZaB9tsX"
#ifndef ELECTRON_FUSES_H_
#define ELECTRON_FUSES_H_
#if defined(WIN32)
#define FUSE_EXPORT __declspec(dllexport)
#define FUSE_EXPORT __attribute__((visibility("default")))
namespace electron::fuses {
extern const volatile char kFuseWire[];
{getters}
}  // namespace electron::fuses
#endif  // ELECTRON_FUSES_H_
TEMPLATE_CC = """
#include "electron/fuses.h"
#include "base/dcheck_is_on.h"
#if DCHECK_IS_ON()
#include "base/command_line.h"
#include <string>
const volatile char kFuseWire[] = { /* sentinel */ {sentinel}, /* fuse_version */ {fuse_version}, /* fuse_wire_length */ {fuse_wire_length}, /* fuse_wire */ {initial_config}};
}  // namespace electron:fuses
with open(os.path.join(dir_path, "fuses.json5"), 'r') as f:
  fuse_defaults = json.loads(''.join(line for line in f.readlines() if not line.strip()[0] == "/"), object_pairs_hook=OrderedDict)
fuse_version = fuse_defaults['_version']
del fuse_defaults['_version']
del fuse_defaults['_schema']
del fuse_defaults['_comment']
if fuse_version >= pow(2, 8):
  raise Exception("Fuse version can not exceed one byte in size")
fuses = fuse_defaults.keys()
initial_config = ""
getters_h = ""
getters_cc = ""
index = len(SENTINEL) + 1
for fuse in fuses:
  index += 1
  initial_config += fuse_defaults[fuse]
  name = ''.join(word.title() for word in fuse.split('_'))
  getters_h += "FUSE_EXPORT bool Is{name}Enabled();\n".replace("{name}", name)
  getters_cc += """
bool Is{name}Enabled() {
  // RunAsNode is checked so early that base::CommandLine isn't yet
  // initialized, so guard here to avoid a CHECK.
  if (base::CommandLine::InitializedForCurrentProcess()) {
    base::CommandLine* command_line = base::CommandLine::ForCurrentProcess();
    if (command_line->HasSwitch("{switch_name}")) {
      std::string switch_value = command_line->GetSwitchValueASCII("{switch_name}");
      return switch_value == "1";
  return kFuseWire[{index}] == '1';
""".replace("{name}", name).replace("{switch_name}", f"set-fuse-{fuse.lower()}").replace("{index}", str(index))
def c_hex(n):
  s = hex(n)[2:]
  return "0x" + s.rjust(2, '0')
def hex_arr(s):
  arr = []
  for char in s:
    arr.append(c_hex(ord(char)))
  return ",".join(arr)
header = TEMPLATE_H.replace("{getters}", getters_h.strip())
impl = TEMPLATE_CC.replace("{sentinel}", hex_arr(SENTINEL))
impl = impl.replace("{fuse_version}", c_hex(fuse_version))
impl = impl.replace("{fuse_wire_length}", c_hex(len(fuses)))
impl = impl.replace("{initial_config}", hex_arr(initial_config))
impl = impl.replace("{getters}", getters_cc.strip())
with open(sys.argv[1], 'w') as f:
  f.write(header)
with open(sys.argv[2], 'w') as f:
  f.write(impl)
from importlib.metadata import metadata
import PyInstaller.__main__  # type: ignore
import pykakasi
import ytmusicapi
from spotdl._version import __version__
LOCALES_PATH = str((Path(ytmusicapi.__file__).parent / "locales"))
PYKAKASI_PATH = str((Path(pykakasi.__file__).parent / "data"))
YTDLP_PATH = str(Path(yt_dlp.__file__).parent / "__pyinstaller")
# Read modules from pyproject.toml
modules = set(
    module.split(" ")[0] for module in metadata("spotdl").get_all("Requires-Dist", [])
PyInstaller.__main__.run(
        "spotdl/__main__.py",
        "--onefile",
        "--add-data",
        f"{LOCALES_PATH}{os.pathsep}ytmusicapi/locales",
        f"{PYKAKASI_PATH}{os.pathsep}pykakasi/data",
        f"--additional-hooks-dir={YTDLP_PATH}",
        "--name",
        f"spotdl-{__version__}-{sys.platform}",
        "--console",
        *(f"--collect-all={module}" for module in modules),
#! /usr/bin/env python
# encoding: utf-8
# WARNING! Do not edit! https://waf.io/book/index.html#_obtaining_the_waf_file
import os, sys, errno, re, shutil, stat
    import cPickle
    import pickle as cPickle
from waflib import Node, Runner, TaskGen, Utils, ConfigSet, Task, Logs, Options, Context, Errors
CACHE_DIR = 'c4che'
CACHE_SUFFIX = '_cache.py'
INSTALL = 1337
UNINSTALL = -1337
SAVED_ATTRS = 'root node_sigs task_sigs imp_sigs raw_deps node_deps'.split()
CFG_FILES = 'cfg_files'
POST_AT_ONCE = 0
POST_LAZY = 1
PROTOCOL = -1
if sys.platform == 'cli':
    PROTOCOL = 0
class BuildContext(Context.Context):
    '''executes the build'''
    cmd = 'build'
    variant = ''
    def __init__(self, **kw):
        super(BuildContext, self).__init__(**kw)
        self.is_install = 0
        self.top_dir = kw.get('top_dir', Context.top_dir)
        self.out_dir = kw.get('out_dir', Context.out_dir)
        self.run_dir = kw.get('run_dir', Context.run_dir)
        self.launch_dir = Context.launch_dir
        self.post_mode = POST_LAZY
        self.cache_dir = kw.get('cache_dir')
        if not self.cache_dir:
            self.cache_dir = os.path.join(self.out_dir, CACHE_DIR)
        self.all_envs = {}
        self.node_sigs = {}
        self.task_sigs = {}
        self.imp_sigs = {}
        self.node_deps = {}
        self.raw_deps = {}
        self.task_gen_cache_names = {}
        self.jobs = Options.options.jobs
        self.targets = Options.options.targets
        self.keep = Options.options.keep
        self.progress_bar = Options.options.progress_bar
        self.deps_man = Utils.defaultdict(list)
        self.current_group = 0
        self.groups = []
        self.group_names = {}
        for v in SAVED_ATTRS:
            if not hasattr(self, v):
                setattr(self, v, {})
    def get_variant_dir(self):
        if not self.variant:
            return self.out_dir
        return os.path.join(self.out_dir, os.path.normpath(self.variant))
    variant_dir = property(get_variant_dir, None)
    def __call__(self, *k, **kw):
        kw['bld'] = self
        ret = TaskGen.task_gen(*k, **kw)
        self.add_to_group(ret, group=kw.get('group'))
        raise Errors.WafError('build contexts cannot be copied')
    def load_envs(self):
        node = self.root.find_node(self.cache_dir)
        if not node:
            raise Errors.WafError('The project was not configured: run "waf configure" first!')
        lst = node.ant_glob('**/*%s' % CACHE_SUFFIX, quiet=True)
        if not lst:
            raise Errors.WafError('The cache directory is empty: reconfigure the project')
        for x in lst:
            name = x.path_from(node).replace(CACHE_SUFFIX, '').replace('\\', '/')
            env = ConfigSet.ConfigSet(x.abspath())
            self.all_envs[name] = env
            for f in env[CFG_FILES]:
                newnode = self.root.find_resource(f)
                if not newnode or not newnode.exists():
                    raise Errors.WafError('Missing configuration file %r, reconfigure the project!' % f)
    def init_dirs(self):
        if not (os.path.isabs(self.top_dir) and os.path.isabs(self.out_dir)):
        self.path = self.srcnode = self.root.find_dir(self.top_dir)
        self.bldnode = self.root.make_node(self.variant_dir)
        self.bldnode.mkdir()
    def execute(self):
        self.restore()
        if not self.all_envs:
            self.load_envs()
        self.execute_build()
    def execute_build(self):
        Logs.info("Waf: Entering directory `%s'", self.variant_dir)
        self.recurse([self.run_dir])
        self.pre_build()
        self.timer = Utils.Timer()
            self.compile()
            if self.progress_bar == 1 and sys.stderr.isatty():
                c = self.producer.processed or 1
                m = self.progress_line(c, c, Logs.colors.BLUE, Logs.colors.NORMAL)
                Logs.info(m, extra={'stream': sys.stderr, 'c1': Logs.colors.cursor_off, 'c2': Logs.colors.cursor_on})
            Logs.info("Waf: Leaving directory `%s'", self.variant_dir)
            self.producer.bld = None
            del self.producer
        self.post_build()
    def restore(self):
            env = ConfigSet.ConfigSet(os.path.join(self.cache_dir, 'build.config.py'))
        except EnvironmentError:
            if env.version < Context.HEXVERSION:
                raise Errors.WafError('Project was configured with a different version of Waf, please reconfigure it')
            for t in env.tools:
                self.setup(**t)
        dbfn = os.path.join(self.variant_dir, Context.DBFILE)
            data = Utils.readf(dbfn, 'rb')
        except (EnvironmentError, EOFError):
            Logs.debug('build: Could not load the build cache %s (missing)', dbfn)
                Node.pickle_lock.acquire()
                Node.Nod3 = self.node_class
                    data = cPickle.loads(data)
                    Logs.debug('build: Could not pickle the build cache %s: %r', dbfn, e)
                    for x in SAVED_ATTRS:
                        setattr(self, x, data.get(x, {}))
                Node.pickle_lock.release()
        self.init_dirs()
    def store(self):
            data[x] = getattr(self, x)
        db = os.path.join(self.variant_dir, Context.DBFILE)
            x = cPickle.dumps(data, PROTOCOL)
        Utils.writef(db + '.tmp', x, m='wb')
            st = os.stat(db)
            os.remove(db)
            if not Utils.is_win32:
                os.chown(db + '.tmp', st.st_uid, st.st_gid)
        except (AttributeError, OSError):
        os.rename(db + '.tmp', db)
    def compile(self):
        Logs.debug('build: compile()')
        self.producer = Runner.Parallel(self, self.jobs)
        self.producer.biter = self.get_build_iterator()
            self.producer.start()
            if self.is_dirty():
                self.store()
        if self.producer.error:
            raise Errors.BuildError(self.producer.error)
    def is_dirty(self):
        return self.producer.dirty
    def setup(self, tool, tooldir=None, funs=None):
        if isinstance(tool, list):
            for i in tool:
                self.setup(i, tooldir)
        module = Context.load_tool(tool, tooldir)
        if hasattr(module, "setup"):
            module.setup(self)
    def get_env(self):
            return self.all_envs[self.variant]
            return self.all_envs['']
    def set_env(self, val):
        self.all_envs[self.variant] = val
    env = property(get_env, set_env)
    def add_manual_dependency(self, path, value):
        if not path:
            raise ValueError('Invalid input path %r' % path)
        if isinstance(path, Node.Node):
            node = path
        elif os.path.isabs(path):
            node = self.root.find_resource(path)
            node = self.path.find_resource(path)
            raise ValueError('Could not find the path %r' % path)
            self.deps_man[node].extend(value)
            self.deps_man[node].append(value)
    def launch_node(self):
            return self.p_ln
            self.p_ln = self.root.find_dir(self.launch_dir)
    def hash_env_vars(self, env, vars_lst):
        if not env.table:
            env = env.parent
            if not env:
                return Utils.SIG_NIL
        idx = str(id(env)) + str(vars_lst)
            cache = self.cache_env
            cache = self.cache_env = {}
                return self.cache_env[idx]
        lst = [env[a] for a in vars_lst]
        cache[idx] = ret = Utils.h_list(lst)
        Logs.debug('envhash: %s %r', Utils.to_hex(ret), lst)
    def get_tgen_by_name(self, name):
        cache = self.task_gen_cache_names
        if not cache:
            for g in self.groups:
                for tg in g:
                        cache[tg.name] = tg
            return cache[name]
            raise Errors.WafError('Could not find a task generator for the name %r' % name)
    def progress_line(self, idx, total, col1, col2):
        if not sys.stderr.isatty():
        n = len(str(total))
        Utils.rot_idx += 1
        ind = Utils.rot_chr[Utils.rot_idx % 4]
        pc = (100. * idx) / total
        fs = "[%%%dd/%%d][%%s%%2d%%%%%%s][%s][" % (n, ind)
        left = fs % (idx, total, col1, pc, col2)
        right = '][%s%s%s]' % (col1, self.timer, col2)
        cols = Logs.get_term_cols() - len(left) - len(right) + 2 * len(col1) + 2 * len(col2)
        if cols < 7:
            cols = 7
        ratio = ((cols * idx) // total) - 1
        bar = ('=' * ratio + '>').ljust(cols)
        msg = Logs.indicator % (left, bar, right)
    def declare_chain(self, *k, **kw):
        return TaskGen.declare_chain(*k, **kw)
    def pre_build(self):
        for m in getattr(self, 'pre_funs', []):
            m(self)
    def post_build(self):
        for m in getattr(self, 'post_funs', []):
    def add_pre_fun(self, meth):
            self.pre_funs.append(meth)
            self.pre_funs = [meth]
    def add_post_fun(self, meth):
            self.post_funs.append(meth)
            self.post_funs = [meth]
    def get_group(self, x):
        if not self.groups:
            self.add_group()
            return self.groups[self.current_group]
        if x in self.group_names:
            return self.group_names[x]
        return self.groups[x]
    def add_to_group(self, tgen, group=None):
        assert (isinstance(tgen, TaskGen.task_gen) or isinstance(tgen, Task.Task))
        tgen.bld = self
        self.get_group(group).append(tgen)
    def get_group_name(self, g):
        if not isinstance(g, list):
            g = self.groups[g]
        for x in self.group_names:
            if id(self.group_names[x]) == id(g):
    def get_group_idx(self, tg):
        se = id(tg)
        for i, tmp in enumerate(self.groups):
            for t in tmp:
                if id(t) == se:
                    return i
    def add_group(self, name=None, move=True):
        if name and name in self.group_names:
            raise Errors.WafError('add_group: name %s already present', name)
        g = []
        self.group_names[name] = g
        self.groups.append(g)
        if move:
            self.current_group = len(self.groups) - 1
    def set_group(self, idx):
        if isinstance(idx, str):
            g = self.group_names[idx]
                if id(g) == id(tmp):
                    self.current_group = i
            self.current_group = idx
        total = 0
        for group in self.groups:
            for tg in group:
                    total += len(tg.tasks)
                    total += 1
        return total
    def get_targets(self):
        to_post = []
        min_grp = 0
        for name in self.targets.split(','):
            tg = self.get_tgen_by_name(name)
            m = self.get_group_idx(tg)
            if m > min_grp:
                min_grp = m
                to_post = [tg]
            elif m == min_grp:
                to_post.append(tg)
        return (min_grp, to_post)
    def get_all_task_gen(self):
        lst = []
            lst.extend(g)
    def post_group(self):
        def tgpost(tg):
                f = tg.post
                f()
        if self.targets == '*':
            for tg in self.groups[self.current_group]:
                tgpost(tg)
        elif self.targets:
            if self.current_group < self._min_grp:
                for tg in self._exact_tg:
                    tg.post()
            ln = self.launch_node()
            if ln.is_child_of(self.bldnode):
                Logs.warn('Building from the build directory, forcing --targets=*')
                ln = self.srcnode
            elif not ln.is_child_of(self.srcnode):
                Logs.warn(
                    'CWD %s is not under %s, forcing --targets=* (run distclean?)', ln.abspath(), self.srcnode.abspath()
            def is_post(tg, ln):
                    p = tg.path
                    if p.is_child_of(ln):
            def is_post_group():
                for i, g in enumerate(self.groups):
                    if i > self.current_group:
                            if is_post(tg, ln):
            if self.post_mode == POST_LAZY and ln != self.srcnode:
                if is_post_group():
    def get_tasks_group(self, idx):
        tasks = []
        for tg in self.groups[idx]:
                tasks.extend(tg.tasks)
                tasks.append(tg)
        return tasks
    def get_build_iterator(self):
        if self.targets and self.targets != '*':
            (self._min_grp, self._exact_tg) = self.get_targets()
        if self.post_mode != POST_LAZY:
            for self.current_group, _ in enumerate(self.groups):
                self.post_group()
            if self.post_mode != POST_AT_ONCE:
            tasks = self.get_tasks_group(self.current_group)
            Task.set_file_constraints(tasks)
            Task.set_precedence_constraints(tasks)
            self.cur_tasks = tasks
            if tasks:
                yield tasks
        while 1:
            yield []
    def install_files(self, dest, files, **kw):
        assert (dest)
        tg = self(features='install_task', install_to=dest, install_from=files, **kw)
        tg.dest = tg.install_to
        tg.type = 'install_files'
        if not kw.get('postpone', True):
        return tg
    def install_as(self, dest, srcfile, **kw):
        tg = self(features='install_task', install_to=dest, install_from=srcfile, **kw)
        tg.type = 'install_as'
    def symlink_as(self, dest, src, **kw):
        tg = self(features='install_task', install_to=dest, install_from=src, **kw)
        tg.type = 'symlink_as'
        tg.link = src
@TaskGen.feature('install_task')
@TaskGen.before_method('process_rule', 'process_source')
def process_install_task(self):
    self.add_install_task(**self.__dict__)
@TaskGen.taskgen_method
def add_install_task(self, **kw):
    if not self.bld.is_install:
    if not kw['install_to']:
    if kw['type'] == 'symlink_as' and Utils.is_win32:
        if kw.get('win32_install'):
            kw['type'] = 'install_as'
    tsk = self.install_task = self.create_task('inst')
    tsk.chmod = kw.get('chmod', Utils.O644)
    tsk.link = kw.get('link', '') or kw.get('install_from', '')
    tsk.relative_trick = kw.get('relative_trick', False)
    tsk.type = kw['type']
    tsk.install_to = tsk.dest = kw['install_to']
    tsk.install_from = kw['install_from']
    tsk.relative_base = kw.get('cwd') or kw.get('relative_base', self.path)
    tsk.install_user = kw.get('install_user')
    tsk.install_group = kw.get('install_group')
    tsk.init_files()
        tsk.run_now()
    return tsk
def add_install_files(self, **kw):
    kw['type'] = 'install_files'
    return self.add_install_task(**kw)
def add_install_as(self, **kw):
def add_symlink_as(self, **kw):
    kw['type'] = 'symlink_as'
class inst(Task.Task):
    def uid(self):
        lst = self.inputs + self.outputs + [self.link, self.generator.path.abspath()]
        return Utils.h_list(lst)
    def init_files(self):
        if self.type == 'symlink_as':
            inputs = []
            inputs = self.generator.to_nodes(self.install_from)
            if self.type == 'install_as':
                assert len(inputs) == 1
        self.set_inputs(inputs)
        dest = self.get_install_path()
        outputs = []
            if self.relative_trick:
                self.link = os.path.relpath(self.link, os.path.dirname(dest))
            outputs.append(self.generator.bld.root.make_node(dest))
        elif self.type == 'install_as':
            for y in inputs:
                    destfile = os.path.join(dest, y.path_from(self.relative_base))
                    destfile = os.path.join(dest, y.name)
                outputs.append(self.generator.bld.root.make_node(destfile))
        self.set_outputs(outputs)
    def runnable_status(self):
        ret = super(inst, self).runnable_status()
        if ret == Task.SKIP_ME and self.generator.bld.is_install:
            return Task.RUN_ME
    def post_run(self):
    def get_install_path(self, destdir=True):
        if isinstance(self.install_to, Node.Node):
            dest = self.install_to.abspath()
            dest = os.path.normpath(Utils.subst_vars(self.install_to, self.env))
        if not os.path.isabs(dest):
            dest = os.path.join(self.env.PREFIX, dest)
        if destdir and Options.options.destdir:
            dest = os.path.join(Options.options.destdir, os.path.splitdrive(dest)[1].lstrip(os.sep))
        return dest
    def copy_fun(self, src, tgt):
        if Utils.is_win32 and len(tgt) > 259 and not tgt.startswith('\\\\?\\'):
            tgt = '\\\\?\\' + tgt
        shutil.copy2(src, tgt)
        self.fix_perms(tgt)
    def rm_empty_dirs(self, tgt):
        while tgt:
            tgt = os.path.dirname(tgt)
                os.rmdir(tgt)
        is_install = self.generator.bld.is_install
        if not is_install:
        for x in self.outputs:
            if is_install == INSTALL:
                x.parent.mkdir()
            fun = is_install == INSTALL and self.do_link or self.do_unlink
            fun(self.link, self.outputs[0].abspath())
            fun = is_install == INSTALL and self.do_install or self.do_uninstall
            launch_node = self.generator.bld.launch_node()
            for x, y in zip(self.inputs, self.outputs):
                fun(x.abspath(), y.abspath(), x.path_from(launch_node))
    def run_now(self):
        status = self.runnable_status()
        if status not in (Task.RUN_ME, Task.SKIP_ME):
            raise Errors.TaskNotReady('Could not process %r: status %r' % (self, status))
        self.run()
        self.hasrun = Task.SUCCESS
    def do_install(self, src, tgt, lbl, **kw):
        if not Options.options.force:
                st1 = os.stat(tgt)
                st2 = os.stat(src)
                if st1.st_mtime + 2 >= st2.st_mtime and st1.st_size == st2.st_size:
                    if not self.generator.bld.progress_bar:
                        c1 = Logs.colors.NORMAL
                        c2 = Logs.colors.BLUE
                        Logs.info('%s- install %s%s%s (from %s)', c1, c2, tgt, c1, lbl)
            Logs.info('%s+ install %s%s%s (from %s)', c1, c2, tgt, c1, lbl)
            os.chmod(tgt, Utils.O644 | stat.S_IMODE(os.stat(tgt).st_mode))
            os.remove(tgt)
            self.copy_fun(src, tgt)
            if not os.path.exists(src):
                Logs.error('File %r does not exist', src)
            elif not os.path.isfile(src):
                Logs.error('Input %r is not a file', src)
            raise Errors.WafError('Could not install the file %r' % tgt, e)
    def fix_perms(self, tgt):
            user = getattr(self, 'install_user', None) or getattr(self.generator, 'install_user', None)
            group = getattr(self, 'install_group', None) or getattr(self.generator, 'install_group', None)
            if user or group:
                Utils.lchown(tgt, user or -1, group or -1)
        if not os.path.islink(tgt):
            os.chmod(tgt, self.chmod)
    def do_link(self, src, tgt, **kw):
        if os.path.islink(tgt) and os.readlink(tgt) == src:
                Logs.info('%s- symlink %s%s%s (to %s)', c1, c2, tgt, c1, src)
                Logs.info('%s+ symlink %s%s%s (to %s)', c1, c2, tgt, c1, src)
            os.symlink(src, tgt)
    def do_uninstall(self, src, tgt, lbl, **kw):
            Logs.info('%s- remove %s%s%s', c1, c2, tgt, c1)
                if not getattr(self, 'uninstall_error', None):
                    self.uninstall_error = True
                    Logs.warn('build: some files could not be uninstalled (retry with -vv to list them)')
                if Logs.verbose > 1:
                    Logs.warn('Could not remove %s (error code %r)', e.filename, e.errno)
        self.rm_empty_dirs(tgt)
    def do_unlink(self, src, tgt, **kw):
class InstallContext(BuildContext):
    '''installs the targets on the system'''
    cmd = 'install'
        super(InstallContext, self).__init__(**kw)
        self.is_install = INSTALL
class UninstallContext(InstallContext):
    '''removes the targets installed'''
    cmd = 'uninstall'
        super(UninstallContext, self).__init__(**kw)
        self.is_install = UNINSTALL
class CleanContext(BuildContext):
    '''cleans the project'''
    cmd = 'clean'
            self.clean()
    def clean(self):
        Logs.debug('build: clean called')
        if hasattr(self, 'clean_files'):
            for n in self.clean_files:
                n.delete()
        elif self.bldnode != self.srcnode:
            for env in self.all_envs.values():
                lst.extend(self.root.find_or_declare(f) for f in env[CFG_FILES])
            excluded_dirs = '.lock* *conf_check_*/** config.log %s/*' % CACHE_DIR
            for n in self.bldnode.ant_glob('**/*', excl=excluded_dirs, quiet=True):
                if n in lst:
        self.root.children = {}
            if v == 'root':
class ListContext(BuildContext):
    '''lists the targets to execute'''
    cmd = 'list'
            self.get_tgen_by_name('')
        except Errors.WafError:
        targets = sorted(self.task_gen_cache_names)
        line_just = max(len(t) for t in targets) if targets else 0
        for target in targets:
            tgen = self.task_gen_cache_names[target]
            descript = getattr(tgen, 'description', '')
            if descript:
                target = target.ljust(line_just)
                descript = ': %s' % descript
            Logs.pprint('GREEN', target, label=descript)
class StepContext(BuildContext):
    '''executes tasks in a step-by-step fashion, for debugging'''
    cmd = 'step'
        super(StepContext, self).__init__(**kw)
        self.files = Options.options.files
        if not self.files:
            Logs.warn('Add a pattern for the debug build, for example "waf step --files=main.c,app"')
            BuildContext.compile(self)
        targets = []
            targets = self.targets.split(',')
                if targets and tg.name not in targets:
            for pat in self.files.split(','):
                matcher = self.get_matcher(pat)
                    if isinstance(tg, Task.Task):
                        lst = [tg]
                        lst = tg.tasks
                    for tsk in lst:
                        do_exec = False
                        for node in tsk.inputs:
                            if matcher(node, output=False):
                                do_exec = True
                        for node in tsk.outputs:
                            if matcher(node, output=True):
                        if do_exec:
                            ret = tsk.run()
                            Logs.info('%s -> exit %r', tsk, ret)
    def get_matcher(self, pat):
        inn = True
        out = True
        if pat.startswith('in:'):
            out = False
            pat = pat.replace('in:', '')
        elif pat.startswith('out:'):
            inn = False
            pat = pat.replace('out:', '')
        anode = self.root.find_node(pat)
        pattern = None
        if not anode:
            if not pat.startswith('^'):
                pat = '^.+?%s' % pat
            if not pat.endswith('$'):
                pat = '%s$' % pat
            pattern = re.compile(pat)
        def match(node, output):
            if output and not out:
            if not output and not inn:
            if anode:
                return anode == node
                return pattern.match(node.abspath())
class EnvContext(BuildContext):
    fun = cmd = None
"""distutils.command.build
Implements the Distutils 'build' command."""
import sys, os
from distutils.core import Command
from distutils.util import get_platform
def show_compilers():
    from distutils.ccompiler import show_compilers
    show_compilers()
class build(Command):
    description = "build everything needed to install"
        ('build-base=', 'b', "base directory for build library"),
        ('build-purelib=', None, "build directory for platform-neutral distributions"),
        ('build-platlib=', None, "build directory for platform-specific distributions"),
            'build-lib=',
            "build directory for all distribution (defaults to either "
            + "build-purelib or build-platlib",
        ('build-scripts=', None, "build directory for scripts"),
        ('build-temp=', 't', "temporary build directory"),
            'plat-name=',
            "platform name to build for, if supported "
            "(default: %s)" % get_platform(),
        ('compiler=', 'c', "specify the compiler type"),
        ('parallel=', 'j', "number of parallel build jobs"),
        ('debug', 'g', "compile extensions and libraries with debugging information"),
        ('force', 'f', "forcibly build everything (ignore file timestamps)"),
        ('executable=', 'e', "specify final destination interpreter path (build.py)"),
    boolean_options = ['debug', 'force']
    help_options = [
        ('help-compiler', None, "list available compilers", show_compilers),
        self.build_base = 'build'
        # these are decided only after 'build_base' has its final value
        # (unless overridden by the user or client)
        self.build_purelib = None
        self.build_platlib = None
        self.build_lib = None
        self.build_temp = None
        self.build_scripts = None
        self.compiler = None
        self.plat_name = None
        self.debug = None
        self.force = 0
        self.executable = None
        self.parallel = None
        if self.plat_name is None:
            self.plat_name = get_platform()
            # plat-name only supported for windows (other platforms are
            # supported via ./configure flags, if at all).  Avoid misleading
            # other platforms.
                    "--plat-name only supported on Windows (try "
                    "using './configure --help' on your platform)"
        plat_specifier = ".%s-%s" % (self.plat_name, sys.implementation.cache_tag)
        # Make it so Python 2.x and Python 2.x with --with-pydebug don't
        # share the same build directories. Doing so confuses the build
        # process for C modules
        if hasattr(sys, 'gettotalrefcount'):
            plat_specifier += '-pydebug'
        # 'build_purelib' and 'build_platlib' just default to 'lib' and
        # 'lib.<plat>' under the base build directory.  We only use one of
        # them for a given distribution, though --
        if self.build_purelib is None:
            self.build_purelib = os.path.join(self.build_base, 'lib')
        if self.build_platlib is None:
            self.build_platlib = os.path.join(self.build_base, 'lib' + plat_specifier)
        # 'build_lib' is the actual directory that we will use for this
        # particular module distribution -- if user didn't supply it, pick
        # one of 'build_purelib' or 'build_platlib'.
        if self.build_lib is None:
            if self.distribution.has_ext_modules():
                self.build_lib = self.build_platlib
                self.build_lib = self.build_purelib
        # 'build_temp' -- temporary directory for compiler turds,
        # "build/temp.<plat>"
        if self.build_temp is None:
            self.build_temp = os.path.join(self.build_base, 'temp' + plat_specifier)
        if self.build_scripts is None:
            self.build_scripts = os.path.join(
                self.build_base, 'scripts-%d.%d' % sys.version_info[:2]
        if self.executable is None and sys.executable:
            self.executable = os.path.normpath(sys.executable)
        if isinstance(self.parallel, str):
                self.parallel = int(self.parallel)
                raise DistutilsOptionError("parallel should be an integer")
        # Run all relevant sub-commands.  This will be some subset of:
        #  - build_py      - pure Python modules
        #  - build_clib    - standalone C libraries
        #  - build_ext     - Python extensions
        #  - build_scripts - (Python) scripts
        for cmd_name in self.get_sub_commands():
            self.run_command(cmd_name)
    # -- Predicates for the sub-command list ---------------------------
    def has_pure_modules(self):
        return self.distribution.has_pure_modules()
    def has_c_libraries(self):
        return self.distribution.has_c_libraries()
    def has_ext_modules(self):
        return self.distribution.has_ext_modules()
    def has_scripts(self):
        return self.distribution.has_scripts()
    sub_commands = [
        ('build_py', has_pure_modules),
        ('build_clib', has_c_libraries),
        ('build_ext', has_ext_modules),
        ('build_scripts', has_scripts),
from distutils.command.build import build as _build
from setuptools import SetuptoolsDeprecationWarning
_ORIGINAL_SUBCOMMANDS = {"build_py", "build_clib", "build_ext", "build_scripts"}
class build(_build):
    # copy to avoid sharing the object with parent class
    sub_commands = _build.sub_commands[:]
        subcommands = {cmd[0] for cmd in _build.sub_commands}
        if subcommands - _ORIGINAL_SUBCOMMANDS:
            It seems that you are using `distutils.command.build` to add
            new subcommands. Using `distutils` directly is considered deprecated,
            please use `setuptools.command.build`.
            warnings.warn(msg, SetuptoolsDeprecationWarning)
            self.sub_commands = _build.sub_commands
        super().run()
from distutils.command.build import build as old_build
from numpy.distutils.command.config_compiler import show_fortran_compilers
class build(old_build):
    sub_commands = [('config_cc',     lambda *args: True),
                    ('config_fc',     lambda *args: True),
                    ('build_src',     old_build.has_ext_modules),
                    ] + old_build.sub_commands
    user_options = old_build.user_options + [
        ('fcompiler=', None,
         "specify the Fortran compiler type"),
        ('warn-error', None,
         "turn all warnings into errors (-Werror)"),
        ('cpu-baseline=', None,
         "specify a list of enabled baseline CPU optimizations"),
        ('cpu-dispatch=', None,
         "specify a list of dispatched CPU optimizations"),
        ('disable-optimization', None,
         "disable CPU optimized code(dispatch,simd,fast...)"),
        ('simd-test=', None,
         "specify a list of CPU optimizations to be tested against NumPy SIMD interface"),
    help_options = old_build.help_options + [
        ('help-fcompiler', None, "list available Fortran compilers",
         show_fortran_compilers),
        old_build.initialize_options(self)
        self.fcompiler = None
        self.warn_error = False
        self.cpu_baseline = "min"
        self.cpu_dispatch = "max -xop -fma4" # drop AMD legacy features by default
        self.disable_optimization = False
        the '_simd' module is a very large. Adding more dispatched features
        will increase binary size and compile time. By default we minimize
        the targeted features to those most commonly used by the NumPy SIMD interface(NPYV),
        NOTE: any specified features will be ignored if they're:
            - part of the baseline(--cpu-baseline)
            - not part of dispatch-able features(--cpu-dispatch)
            - not supported by compiler or platform
        self.simd_test = "BASELINE SSE2 SSE42 XOP FMA4 (FMA3 AVX2) AVX512F " \
                         "AVX512_SKX VSX VSX2 VSX3 VSX4 NEON ASIMD VX VXE VXE2"
        build_scripts = self.build_scripts
        old_build.finalize_options(self)
        plat_specifier = ".{}-{}.{}".format(get_platform(), *sys.version_info[:2])
        if build_scripts is None:
            self.build_scripts = os.path.join(self.build_base,
                                              'scripts' + plat_specifier)
        old_build.run(self)
