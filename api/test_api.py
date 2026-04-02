from ast import literal_eval
from config import tape, TweepyTestCase, username
from tweepy import API, FileCache, MemoryCache
from tweepy.models import Friendship
from tweepy.parsers import Parser
test_tweet_id = '266367358078169089'
tweet_text = 'testing 1000'
"""Unit tests"""
class TweepyAPITests(TweepyTestCase):
    #@tape.use_cassette('testfailure.json')
    #def testapierror(self):
    #    from tweepy.errors import TweepyException
    #    with self.assertRaises(TweepyException) as cm:
    #        self.api.direct_messages()
    #    reason, = literal_eval(cm.exception.reason)
    #    self.assertEqual(reason['message'], 'Bad Authentication data.')
    #    self.assertEqual(reason['code'], 215)
    #    self.assertEqual(cm.exception.api_code, 215)
    # TODO: Actually have some sort of better assertion
    @tape.use_cassette('testgetoembed.json', serializer='json')
    def testgetoembed(self):
        data = self.api.get_oembed("https://twitter.com/Twitter/status/" + test_tweet_id)
        self.assertEqual(data['author_name'], "Twitter")
    @tape.use_cassette('testparserargumenthastobeaparserinstance.json',
                       serializer='json')
    def testparserargumenthastobeaparserinstance(self):
        """ Testing the issue https://github.com/tweepy/tweepy/issues/421"""
        self.assertRaises(TypeError, API, self.auth, parser=Parser)
    @tape.use_cassette('testhometimeline.json', serializer='json')
    def testhometimeline(self):
        self.api.home_timeline()
    @tape.use_cassette('testusertimeline.json', serializer='json')
    def testusertimeline(self):
        self.api.user_timeline()
        self.api.user_timeline(screen_name='Twitter')
    @tape.use_cassette('testmentionstimeline.json', serializer='json')
    def testmentionstimeline(self):
        self.api.mentions_timeline()
    @tape.use_cassette('testgetretweetsofme.json', serializer='json')
    def testgetretweetsofme(self):
        self.api.get_retweets_of_me()
    @tape.use_cassette('testretweetandunretweet.json', serializer='json')
    def testretweetandunretweet(self):
        self.api.retweet(test_tweet_id)
        self.api.unretweet(test_tweet_id)
    @tape.use_cassette('testgetretweets.json', serializer='json')
    def testgetretweets(self):
        self.api.get_retweets(test_tweet_id)
    @tape.use_cassette('testgetretweeterids.json', serializer='json')
    def testgetretweeterids(self):
        self.api.get_retweeter_ids(test_tweet_id)
    @tape.use_cassette('testgetstatus.json', serializer='json')
    def testgetstatus(self):
        self.api.get_status(id=test_tweet_id)
    @tape.use_cassette('testupdateanddestroystatus.json', serializer='json')
    def testupdateanddestroystatus(self):
        # test update
        update = self.api.update_status(status=tweet_text)
        self.assertEqual(update.text, tweet_text)
        # test destroy
        deleted = self.api.destroy_status(id=update.id)
        self.assertEqual(deleted.id, update.id)
    def testupdateanddestroystatuswithoutkwarg(self):
        # test update, passing text as a positional argument (#554)
        update = self.api.update_status(tweet_text)
    @tape.use_cassette('testupdatestatuswithmedia.yaml')
    def testupdatestatuswithmedia(self):
        update = self.api.update_status_with_media(tweet_text, 'assets/banner.png')
        self.assertIn(tweet_text + ' https://t.co', update.text)
    @tape.use_cassette('testmediauploadpng.yaml')
    def testmediauploadpng(self):
        self.api.media_upload('assets/banner.png')
    @tape.use_cassette('testmediauploadgif.yaml')
    def testmediauploadgif(self):
        self.api.media_upload('assets/animated.gif')
    @tape.use_cassette('testmediauploadmp4.yaml')
    def testmediauploadmp4(self):
        self.api.media_upload('assets/video.mp4')
    @tape.use_cassette('testgetuser.yaml')
    def testgetuser(self):
        u = self.api.get_user(screen_name='Twitter')
        self.assertEqual(u.screen_name, 'Twitter')
        u = self.api.get_user(user_id=783214)
    @tape.use_cassette('testlookupusers.json', serializer='json')
    def testlookupusers(self):
        def check(users):
            self.assertEqual(len(users), 2)
        check(self.api.lookup_users(user_id=[6844292, 6253282]))
        check(self.api.lookup_users(screen_name=['twitterapi', 'twitter']))
    @tape.use_cassette('testsearchusers.json', serializer='json')
    def testsearchusers(self):
        self.api.search_users('twitter')
    @tape.use_cassette('testgetdirectmessages.json', serializer='json')
    def testgetdirectmessages(self):
        self.api.get_direct_messages()
    @tape.use_cassette('testsendanddeletedirectmessage.json',
    def testsendanddeletedirectmessage(self):
        me = self.api.verify_credentials()
        # send
        sent_dm = self.api.send_direct_message(me.id, text='test message')
        self.assertEqual(sent_dm.message_create['message_data']['text'], 'test message')
        self.assertEqual(int(sent_dm.message_create['sender_id']), me.id)
        self.assertEqual(int(sent_dm.message_create['target']['recipient_id']), me.id)
        # destroy
        self.api.delete_direct_message(sent_dm.id)
    @tape.use_cassette('test_api_indicate_direct_message_typing.yaml')
    def test_indicate_direct_message_typing(self):
        self.api.indicate_direct_message_typing(me.id)
    # TODO: Test API.mark_direct_message_read
    @tape.use_cassette('testcreatedestroyfriendship.yaml')
    def testcreatedestroyfriendship(self):
        enemy = self.api.destroy_friendship(screen_name='Twitter')
        self.assertEqual(enemy.screen_name, 'Twitter')
        friend = self.api.create_friendship(screen_name='Twitter')
        self.assertEqual(friend.screen_name, 'Twitter')
    @tape.use_cassette('testgetfriendship.json', serializer='json')
    def testgetfriendship(self):
        source, target = self.api.get_friendship(target_screen_name='twitter')
        self.assertTrue(isinstance(source, Friendship))
        self.assertTrue(isinstance(target, Friendship))
    @tape.use_cassette('testgetfriendids.yaml')
    def testgetfriendids(self):
        self.api.get_friend_ids(screen_name=username)
    @tape.use_cassette('testgetfollowerids.yaml')
    def testgetfollowerids(self):
        self.api.get_follower_ids(screen_name=username)
    @tape.use_cassette('testgetfriends.yaml')
    def testgetfriends(self):
        self.api.get_friends(screen_name=username)
    @tape.use_cassette('testgetfollowers.yaml')
    def testgetfollowers(self):
        self.api.get_followers(screen_name=username)
    @tape.use_cassette('testverifycredentials.json', serializer='json')
    def testverifycredentials(self):
        self.api.verify_credentials()
        # make sure that `me.status.entities` is not an empty dict
        me = self.api.verify_credentials(include_entities=True)
        self.assertTrue(me.status.entities)
        # `status` shouldn't be included
        me = self.api.verify_credentials(skip_status=True)
        self.assertFalse(hasattr(me, 'status'))
    @tape.use_cassette('testratelimitstatus.json', serializer='json')
    def testratelimitstatus(self):
        self.api.rate_limit_status()
    @tape.use_cassette('testupdateprofilecolors.json', serializer='json')
    def testupdateprofilecolors(self):
        original = self.api.verify_credentials()
        updated = self.api.update_profile(profile_link_color='D0F900')
        # restore colors
        self.api.update_profile(
            profile_link_color=original.profile_link_color,
        self.assertEqual(updated.profile_background_color, '000000')
        self.assertEqual(updated.profile_text_color, '000000')
        self.assertEqual(updated.profile_link_color, 'D0F900')
        self.assertEqual(updated.profile_sidebar_fill_color, '000000')
        self.assertEqual(updated.profile_sidebar_border_color, '000000')
    def testupateprofileimage(self):
        self.api.update_profile_image('examples/profile.png')
    # TODO: Use logo
    @tape.use_cassette('testupdateprofilebannerimage.yaml')
    def testupdateprofilebannerimage(self):
        self.api.update_profile_banner('assets/banner.png')
    @tape.use_cassette('testupdateprofile.json', serializer='json')
    def testupdateprofile(self):
        profile = {
            'name': 'Tweepy test 123',
            'location': 'pytopia',
            'description': 'just testing things out'
        updated = self.api.update_profile(**profile)
            name = original.name, url = original.url,
            location = original.location, description = original.description
        for k,v in profile.items():
            if k == 'email': continue
            self.assertEqual(getattr(updated, k), v)
    @tape.use_cassette('testgetfavorites.json', serializer='json')
    def testgetfavorites(self):
        self.api.get_favorites()
    @tape.use_cassette('testcreatedestroyfavorite.json', serializer='json')
    def testcreatedestroyfavorite(self):
        self.api.create_favorite(145344012)
        self.api.destroy_favorite(145344012)
    @tape.use_cassette('testcreatedestroyblock.yaml')
    def testcreatedestroyblock(self):
        self.api.create_block(screen_name='twitter')
        self.api.destroy_block(screen_name='twitter')
        self.api.create_friendship(screen_name='twitter')  # restore
    @tape.use_cassette('testgetblocks.json', serializer='json')
    def testgetblocks(self):
        self.api.get_blocks()
    @tape.use_cassette('testgetblockedids.json', serializer='json')
    def testgetblockedids(self):
        self.api.get_blocked_ids()
    @tape.use_cassette('testcreateupdatedestroylist.yaml')
    def testcreateupdatedestroylist(self):
        l = self.api.create_list(name="tweeps")
        l = self.api.update_list(list_id=l.id, description='updated!')
        self.assertEqual(l.description, 'updated!')
        self.api.destroy_list(list_id=l.id)
    @tape.use_cassette('testgetlists.json', serializer='json')
    def testgetlists(self):
        self.api.get_lists()
    @tape.use_cassette('testgetlistmemberships.json', serializer='json')
    def testgetlistmemberships(self):
        self.api.get_list_memberships()
    @tape.use_cassette('testgetlistownerships.json', serializer='json')
    def testgetlistownerships(self):
        self.api.get_list_ownerships()
    @tape.use_cassette('testgetlistsubscriptions.json', serializer='json')
    def testgetlistsubscriptions(self):
        self.api.get_list_subscriptions()
    @tape.use_cassette('testlisttimeline.json', serializer='json')
    def testlisttimeline(self):
        self.api.list_timeline(owner_screen_name='Twitter', slug='Official-Twitter-Accounts')
    @tape.use_cassette('testgetlist.json', serializer='json')
    def testgetlist(self):
        self.api.get_list(owner_screen_name='Twitter', slug='Official-Twitter-Accounts')
    @tape.use_cassette('testaddremovelistmember.json', serializer='json')
    def testaddremovelistmember(self):
        params = {
            'slug': 'test',
            'owner_screen_name': username,
            'screen_name': 'twitter'
        def assert_list(l):
            self.assertEqual(l.name, params['slug'])
        assert_list(self.api.add_list_member(**params))
        assert_list(self.api.remove_list_member(**params))
    @tape.use_cassette('testaddremovelistmembers.json', serializer='json')
    def testaddremovelistmembers(self):
            'screen_name': ['Twitter', 'TwitterAPI']
        assert_list(self.api.add_list_members(**params))
        assert_list(self.api.remove_list_members(**params))
    @tape.use_cassette('testgetlistmembers.json', serializer='json')
    def testgetlistmembers(self):
        self.api.get_list_members(owner_screen_name='Twitter', slug='Official-Twitter-Accounts')
    @tape.use_cassette('testgetlistmember.json', serializer='json')
    def testgetlistmember(self):
        self.assertTrue(self.api.get_list_member(owner_screen_name='Twitter', slug='Official-Twitter-Accounts', screen_name='TwitterAPI'))
    @tape.use_cassette('testsubscribeunsubscribelist.json', serializer='json')
    def testsubscribeunsubscribelist(self):
            'owner_screen_name': 'Twitter',
            'slug': 'Official-Twitter-Accounts'
        self.api.subscribe_list(**params)
        self.api.unsubscribe_list(**params)
    @tape.use_cassette('testgetlistsubscribers.json', serializer='json')
    def testgetlistsubscribers(self):
        self.api.get_list_subscribers(owner_screen_name='Twitter', slug='Official-Twitter-Accounts')
    @tape.use_cassette('testgetlistsubscriber.json', serializer='json')
    def testgetlistsubscriber(self):
        self.assertTrue(self.api.get_list_subscriber(owner_screen_name='Twitter', slug='Official-Twitter-Accounts', screen_name='TwitterMktg'))
    @tape.use_cassette('testsavedsearches.json', serializer='json')
    def testsavedsearches(self):
        s = self.api.create_saved_search('test')
        self.api.get_saved_searches()
        self.assertEqual(self.api.get_saved_search(s.id).query, 'test')
        self.api.destroy_saved_search(s.id)
    @tape.use_cassette('testsearchtweets.json', serializer='json')
    def testsearchtweets(self):
        self.api.search_tweets('tweepy')
    @tape.use_cassette('testgeoapis.json', serializer='json')
    def testgeoapis(self):
        def place_name_in_list(place_name, place_list):
            """Return True if a given place_name is in place_list."""
            return any(x.full_name.lower() == place_name.lower() for x in place_list)
        # Test various API functions using Austin, TX, USA
        self.assertEqual(self.api.geo_id(place_id='1ffd3558f2e98349').full_name, 'Dogpatch, San Francisco')
        self.assertTrue(place_name_in_list('Austin, TX',
            self.api.reverse_geocode(lat=30.2673701685, long= -97.7426147461)))  # Austin, TX, USA
    @tape.use_cassette('testsupportedlanguages.json', serializer='json')
    def testsupportedlanguages(self):
        languages = self.api.supported_languages()
        expected_dict = {
            "code": "en",
            "debug": False,
            "local_name": "English",
            "name": "English",
            "status": "production"
        self.assertTrue(expected_dict in languages)
    @tape.use_cassette('testcachedresult.yaml')
    def testcachedresult(self):
        self.api.cache = MemoryCache()
        self.assertFalse(self.api.cached_result)
        self.assertTrue(self.api.cached_result)
    def testcachedifferentqueryparameters(self):
        user1 = self.api.get_user(screen_name='TweepyDev')
        self.assertEqual('TweepyDev', user1.screen_name)
        user2 = self.api.get_user(screen_name='Twitter')
        self.assertEqual('Twitter', user2.screen_name)
class TweepyCacheTests(unittest.TestCase):
    timeout = 0.5
    memcache_servers = ['127.0.0.1:11211']  # must be running for test to pass
    def _run_tests(self, do_cleanup=True):
        # test store and get
        self.cache.store('testkey', 'testvalue')
        self.assertEqual(self.cache.get('testkey'), 'testvalue',
            'Stored value does not match retrieved value')
        # test timeout
        time.sleep(self.timeout)
        self.assertEqual(self.cache.get('testkey'), None,
            'Cache entry should have expired')
        # test cleanup
        if do_cleanup:
            self.cache.cleanup()
            self.assertEqual(self.cache.count(), 0, 'Cache cleanup failed')
        # test count
        for i in range(20):
            self.cache.store(f'testkey{i}', 'testvalue')
        self.assertEqual(self.cache.count(), 20, 'Count is wrong')
        # test flush
        self.cache.flush()
        self.assertEqual(self.cache.count(), 0, 'Cache failed to flush')
    def testmemorycache(self):
        self.cache = MemoryCache(timeout=self.timeout)
        self._run_tests()
    def testfilecache(self):
        os.mkdir('cache_test_dir')
            self.cache = FileCache('cache_test_dir', self.timeout)
            if os.path.exists('cache_test_dir'):
                shutil.rmtree('cache_test_dir')
from django.test import RequestFactory, SimpleTestCase
from .utils import DummyStorage
class ApiTests(SimpleTestCase):
        self.request = self.rf.request()
        self.storage = DummyStorage()
    def test_ok(self):
        msg = "some message"
        self.request._messages = self.storage
        messages.add_message(self.request, messages.DEBUG, msg)
        [message] = self.storage.store
        self.assertEqual(msg, message.message)
    def test_request_is_none(self):
        msg = "add_message() argument must be an HttpRequest object, not 'NoneType'."
            messages.add_message(None, messages.DEBUG, "some message")
        self.assertEqual(self.storage.store, [])
    def test_middleware_missing(self):
        with self.assertRaisesMessage(messages.MessageFailure, msg):
            messages.add_message(self.request, messages.DEBUG, "some message")
    def test_middleware_missing_silently(self):
            self.request, messages.DEBUG, "some message", fail_silently=True
class CustomRequest:
    def __init__(self, request):
    def __getattribute__(self, attr):
            return super().__getattribute__(attr)
            return getattr(self._request, attr)
class CustomRequestApiTests(ApiTests):
    add_message() should use ducktyping to allow request wrappers such as the
    one in Django REST framework.
        self.request = CustomRequest(self.request)
from langchain_classic.indexes import __all__
def test_all() -> None:
    """Use to catch obvious breaking changes."""
    assert sorted(__all__) == sorted(expected)
Tests for L{pyflakes.scripts.pyflakes}.
from pyflakes.checker import PYPY
from pyflakes.messages import UnusedImport
from pyflakes.reporter import Reporter
from pyflakes.api import (
    main,
    check,
    checkPath,
    checkRecursive,
    iterSourceCode,
from pyflakes.test.harness import TestCase, skipIf
def withStderrTo(stderr, f, *args, **kwargs):
    Call C{f} with C{sys.stderr} redirected to C{stderr}.
    (outer, sys.stderr) = (sys.stderr, stderr)
        return f(*args, **kwargs)
        sys.stderr = outer
    Mock an AST node.
    def __init__(self, lineno, col_offset=0):
        self.col_offset = col_offset
class SysStreamCapturing:
    """Context manager capturing sys.stdin, sys.stdout and sys.stderr.
    The file handles are replaced with a StringIO object.
    def __init__(self, stdin):
        self._stdin = io.StringIO(stdin or '', newline=os.linesep)
        self._orig_stdin = sys.stdin
        self._orig_stdout = sys.stdout
        self._orig_stderr = sys.stderr
        sys.stdin = self._stdin
        sys.stdout = self._stdout_stringio = io.StringIO(newline=os.linesep)
        sys.stderr = self._stderr_stringio = io.StringIO(newline=os.linesep)
        self.output = self._stdout_stringio.getvalue()
        self.error = self._stderr_stringio.getvalue()
        sys.stdin = self._orig_stdin
        sys.stdout = self._orig_stdout
        sys.stderr = self._orig_stderr
class LoggingReporter:
    Implementation of Reporter that just appends any error to a list.
    def __init__(self, log):
        Construct a C{LoggingReporter}.
        @param log: A list to append log messages to.
        self.log = log
    def flake(self, message):
        self.log.append(('flake', str(message)))
    def unexpectedError(self, filename, message):
        self.log.append(('unexpectedError', filename, message))
    def syntaxError(self, filename, msg, lineno, offset, line):
        self.log.append(('syntaxError', filename, msg, lineno, offset, line))
class TestIterSourceCode(TestCase):
    Tests for L{iterSourceCode}.
        self.tempdir = tempfile.mkdtemp()
        shutil.rmtree(self.tempdir)
    def makeEmptyFile(self, *parts):
        assert parts
        fpath = os.path.join(self.tempdir, *parts)
        open(fpath, 'a').close()
        return fpath
    def test_emptyDirectory(self):
        There are no Python files in an empty directory.
        self.assertEqual(list(iterSourceCode([self.tempdir])), [])
    def test_singleFile(self):
        If the directory contains one Python file, C{iterSourceCode} will find
        childpath = self.makeEmptyFile('foo.py')
        self.assertEqual(list(iterSourceCode([self.tempdir])), [childpath])
    def test_onlyPythonSource(self):
        Files that are not Python source files are not included.
        self.makeEmptyFile('foo.pyc')
    def test_recurses(self):
        If the Python files are hidden deep down in child directories, we will
        find them.
        os.mkdir(os.path.join(self.tempdir, 'foo'))
        apath = self.makeEmptyFile('foo', 'a.py')
        self.makeEmptyFile('foo', 'a.py~')
        os.mkdir(os.path.join(self.tempdir, 'bar'))
        bpath = self.makeEmptyFile('bar', 'b.py')
        cpath = self.makeEmptyFile('c.py')
            sorted(iterSourceCode([self.tempdir])),
            sorted([apath, bpath, cpath]))
    def test_shebang(self):
        Find Python files that don't end with `.py`, but contain a Python
        shebang.
        python = os.path.join(self.tempdir, 'a')
        with open(python, 'w') as fd:
            fd.write('#!/usr/bin/env python\n')
        self.makeEmptyFile('b')
        with open(os.path.join(self.tempdir, 'c'), 'w') as fd:
            fd.write('hello\nworld\n')
        python3 = os.path.join(self.tempdir, 'e')
        with open(python3, 'w') as fd:
            fd.write('#!/usr/bin/env python3\n')
        pythonw = os.path.join(self.tempdir, 'f')
        with open(pythonw, 'w') as fd:
            fd.write('#!/usr/bin/env pythonw\n')
        python3args = os.path.join(self.tempdir, 'g')
        with open(python3args, 'w') as fd:
            fd.write('#!/usr/bin/python3 -u\n')
        python3d = os.path.join(self.tempdir, 'i')
        with open(python3d, 'w') as fd:
            fd.write('#!/usr/local/bin/python3d\n')
        python38m = os.path.join(self.tempdir, 'j')
        with open(python38m, 'w') as fd:
            fd.write('#! /usr/bin/env python3.8m\n')
        # Should NOT be treated as Python source
        notfirst = os.path.join(self.tempdir, 'l')
        with open(notfirst, 'w') as fd:
            fd.write('#!/bin/sh\n#!/usr/bin/python\n')
            sorted([
                python, python3, pythonw, python3args, python3d,
                python38m,
    def test_multipleDirectories(self):
        L{iterSourceCode} can be given multiple directories.  It will recurse
        into each of them.
        foopath = os.path.join(self.tempdir, 'foo')
        barpath = os.path.join(self.tempdir, 'bar')
        os.mkdir(foopath)
        os.mkdir(barpath)
            sorted(iterSourceCode([foopath, barpath])),
            sorted([apath, bpath]))
    def test_explicitFiles(self):
        If one of the paths given to L{iterSourceCode} is not a directory but
        a file, it will include that in its output.
        epath = self.makeEmptyFile('e.py')
        self.assertEqual(list(iterSourceCode([epath])),
                         [epath])
class TestReporter(TestCase):
    Tests for L{Reporter}.
    def test_syntaxError(self):
        C{syntaxError} reports that there was a syntax error in the source
        file.  It reports to the error stream and includes the filename, line
        number, error message, actual line of source and a caret pointing to
        where the error is.
        err = io.StringIO()
        reporter = Reporter(None, err)
        reporter.syntaxError('foo.py', 'a problem', 3, 8, 'bad line of source')
            ("foo.py:3:8: a problem\n"
             "bad line of source\n"
             "       ^\n"),
            err.getvalue())
    def test_syntaxErrorNoOffset(self):
        C{syntaxError} doesn't include a caret pointing to the error if
        C{offset} is passed as C{None}.
        reporter.syntaxError('foo.py', 'a problem', 3, None,
                             'bad line of source')
            ("foo.py:3: a problem\n"
             "bad line of source\n"),
    def test_syntaxErrorNoText(self):
        C{syntaxError} doesn't include text or nonsensical offsets if C{text} is C{None}.
        This typically happens when reporting syntax errors from stdin.
        reporter.syntaxError('<stdin>', 'a problem', 0, 0, None)
        self.assertEqual(("<stdin>:1:1: a problem\n"), err.getvalue())
    def test_multiLineSyntaxError(self):
        If there's a multi-line syntax error, then we only report the last
        line.  The offset is adjusted so that it is relative to the start of
        the last line.
        lines = [
            'bad line of source',
            'more bad lines of source',
        reporter.syntaxError('foo.py', 'a problem', 3, len(lines[0]) + 7,
                             '\n'.join(lines))
            ("foo.py:3:25: a problem\n" +
             lines[-1] + "\n" +
             " " * 24 + "^\n"),
    def test_unexpectedError(self):
        C{unexpectedError} reports an error processing a source file.
        reporter.unexpectedError('source.py', 'error message')
        self.assertEqual('source.py: error message\n', err.getvalue())
    def test_flake(self):
        C{flake} reports a code warning from Pyflakes.  It is exactly the
        str() of a L{pyflakes.messages.Message}.
        out = io.StringIO()
        reporter = Reporter(out, None)
        message = UnusedImport('foo.py', Node(42), 'bar')
        reporter.flake(message)
        self.assertEqual(out.getvalue(), f"{message}\n")
class CheckTests(TestCase):
    Tests for L{check} and L{checkPath} which check a file for flakes.
    def makeTempFile(self, content):
        Make a temporary file containing C{content} and return a path to it.
        fd, name = tempfile.mkstemp()
            with os.fdopen(fd, 'wb') as f:
                if not hasattr(content, 'decode'):
                    content = content.encode('ascii')
    def assertHasErrors(self, path, errorList):
        Assert that C{path} causes errors.
        @param path: A path to a file to check.
        @param errorList: A list of errors expected to be printed to stderr.
        count = withStderrTo(err, checkPath, path)
            (count, err.getvalue()), (len(errorList), ''.join(errorList)))
    def getErrors(self, path):
        Get any warnings or errors reported by pyflakes for the file at C{path}.
        @param path: The path to a Python file on disk that pyflakes will check.
        @return: C{(count, log)}, where C{count} is the number of warnings or
            errors generated, and log is a list of those warnings, presented
            as structured data.  See L{LoggingReporter} for more details.
        log = []
        reporter = LoggingReporter(log)
        count = checkPath(path, reporter)
        return count, log
    def test_legacyScript(self):
        from pyflakes.scripts import pyflakes as script_pyflakes
        self.assertIs(script_pyflakes.checkPath, checkPath)
    def test_missingTrailingNewline(self):
        Source which doesn't end with a newline shouldn't cause any
        exception to be raised nor an error indicator to be returned by
        L{check}.
        with self.makeTempFile("def foo():\n\tpass\n\t") as fName:
            self.assertHasErrors(fName, [])
    def test_checkPathNonExisting(self):
        L{checkPath} handles non-existing files.
        count, errors = self.getErrors('extremo')
        self.assertEqual(count, 1)
            [('unexpectedError', 'extremo', 'No such file or directory')])
    def test_multilineSyntaxError(self):
        Source which includes a syntax error which results in the raised
        L{SyntaxError.text} containing multiple lines of source are reported
        with only the last line of that source.
        source = """\
def foo():
def bar():
def baz():
    '''quux'''
        # Sanity check - SyntaxError.text should be multiple lines, if it
        # isn't, something this test was unprepared for has happened.
        def evaluate(source):
            exec(source)
            evaluate(source)
            if not PYPY and sys.version_info < (3, 10):
                self.assertTrue(e.text.count('\n') > 1)
        with self.makeTempFile(source) as sourcePath:
                message = 'end of file (EOF) while scanning triple-quoted string literal'
            elif sys.version_info >= (3, 10):
                message = 'unterminated triple-quoted string literal (detected at line 8)'  # noqa: E501
                message = 'invalid syntax'
            if PYPY or sys.version_info >= (3, 10):
                column = 12
                column = 8
            self.assertHasErrors(
                ["""\
%s:8:%d: %s
%s^
""" % (sourcePath, column, message, ' ' * (column - 1))])
    def test_eofSyntaxError(self):
        The error reported for source files which end prematurely causing a
        syntax error reflects the cause for the syntax error.
        with self.makeTempFile("def foo(") as sourcePath:
                msg = 'parenthesis is never closed'
                msg = "'(' was never closed"
                msg = 'unexpected EOF while parsing'
                column = 9
            spaces = ' ' * (column - 1)
            expected = '{}:1:{}: {}\ndef foo(\n{}^\n'.format(
                sourcePath, column, msg, spaces
            self.assertHasErrors(sourcePath, [expected])
    def test_eofSyntaxErrorWithTab(self):
        with self.makeTempFile("if True:\n\tfoo =") as sourcePath:
                [f"""\
{sourcePath}:2:7: invalid syntax
\tfoo =
\t     ^
"""])
    def test_nonDefaultFollowsDefaultSyntaxError(self):
        Source which has a non-default argument following a default argument
        should include the line number of the syntax error.  However these
        exceptions do not include an offset.
def foo(bar=baz, bax):
                msg = 'parameter without a default follows parameter with a default'  # noqa: E501
                msg = 'non-default argument follows default argument'
                column = 18
                column = 21
            last_line = ' ' * (column - 1) + '^\n'
{sourcePath}:1:{column}: {msg}
{last_line}"""]
    def test_nonKeywordAfterKeywordSyntaxError(self):
        Source which has a non-keyword argument after a keyword argument should
        include the line number of the syntax error.  However these exceptions
        do not include an offset.
foo(bar=baz, bax)
            last_line = ' ' * 16 + '^\n'
{sourcePath}:1:17: positional argument follows keyword argument
{last_line}"""])
    def test_invalidEscape(self):
        The invalid escape syntax raises ValueError in Python 2
        # ValueError: invalid \x escape
        with self.makeTempFile(r"foo = '\xyz'") as sourcePath:
            position_end = 1
                column = 7
            elif sys.version_info < (3, 12):
                column = 13
            last_line = '%s^\n' % (' ' * (column - 1))
            decoding_error = """\
%s:1:%d: (unicode error) 'unicodeescape' codec can't decode bytes \
in position 0-%d: truncated \\xXX escape
foo = '\\xyz'
%s""" % (sourcePath, column, position_end, last_line)
                sourcePath, [decoding_error])
    @skipIf(sys.platform == 'win32', 'unsupported on Windows')
    def test_permissionDenied(self):
        If the source file is not readable, this is reported on standard
        if os.getuid() == 0:
            self.skipTest('root user can access all files regardless of '
                          'permissions')
        with self.makeTempFile('') as sourcePath:
            os.chmod(sourcePath, 0)
            count, errors = self.getErrors(sourcePath)
                [('unexpectedError', sourcePath, "Permission denied")])
    def test_pyflakesWarning(self):
        If the source file has a pyflakes warning, this is reported as a
        'flake'.
        with self.makeTempFile("import foo") as sourcePath:
                errors, [('flake', str(UnusedImport(sourcePath, Node(1), 'foo')))])
    def test_encodedFileUTF8(self):
        If source file declares the correct encoding, no error is reported.
        SNOWMAN = chr(0x2603)
        source = ("""\
# coding: utf-8
x = "%s"
""" % SNOWMAN).encode('utf-8')
            self.assertHasErrors(sourcePath, [])
    def test_CRLFLineEndings(self):
        Source files with Windows CR LF line endings are parsed successfully.
        with self.makeTempFile("x = 42\r\n") as sourcePath:
    def test_misencodedFileUTF8(self):
        If a source file contains bytes which cannot be decoded, this is
        reported on stderr.
# coding: ascii
                [f"{sourcePath}:1:1: 'ascii' codec can't decode byte 0xe2 in position 21: ordinal not in range(128)\n"])  # noqa: E501
    def test_misencodedFileUTF16(self):
""" % SNOWMAN).encode('utf-16')
            if sys.version_info < (3, 11, 4):
                expected = f"{sourcePath}: problem decoding source\n"
                expected = f"{sourcePath}:1: source code string cannot contain null bytes\n"  # noqa: E501
    def test_checkRecursive(self):
        L{checkRecursive} descends into each directory, finding Python files
        and reporting problems.
        tempdir = tempfile.mkdtemp()
            os.mkdir(os.path.join(tempdir, 'foo'))
            file1 = os.path.join(tempdir, 'foo', 'bar.py')
            with open(file1, 'wb') as fd:
                fd.write(b"import baz\n")
            file2 = os.path.join(tempdir, 'baz.py')
            with open(file2, 'wb') as fd:
                fd.write(b"import contraband")
            warnings = checkRecursive([tempdir], reporter)
            self.assertEqual(warnings, 2)
                sorted(log),
                sorted([('flake', str(UnusedImport(file1, Node(1), 'baz'))),
                        ('flake',
                         str(UnusedImport(file2, Node(1), 'contraband')))]))
            shutil.rmtree(tempdir)
    def test_stdinReportsErrors(self):
        L{check} reports syntax errors from stdin
        source = "max(1 for i in range(10), key=lambda x: x+1)\n"
        count = withStderrTo(err, check, source, "<stdin>")
        errlines = err.getvalue().split("\n")[:-1]
        expected_error = [
            "<stdin>:1:5: Generator expression must be parenthesized",
            "max(1 for i in range(10), key=lambda x: x+1)",
            "    ^",
        self.assertEqual(errlines, expected_error)
class IntegrationTests(TestCase):
    Tests of the pyflakes script that actually spawn the script.
        self.tempfilepath = os.path.join(self.tempdir, 'temp')
    def getPyflakesBinary(self):
        Return the path to the pyflakes binary.
        import pyflakes
        package_dir = os.path.dirname(pyflakes.__file__)
        return os.path.join(package_dir, '..', 'bin', 'pyflakes')
    def runPyflakes(self, paths, stdin=None):
        Launch a subprocess running C{pyflakes}.
        @param paths: Command-line arguments to pass to pyflakes.
        @param stdin: Text to use as stdin.
        @return: C{(returncode, stdout, stderr)} of the completed pyflakes
            process.
        env['PYTHONPATH'] = os.pathsep.join(sys.path)
        command = [sys.executable, self.getPyflakesBinary()]
        command.extend(paths)
        if stdin:
            p = subprocess.Popen(command, env=env, stdin=subprocess.PIPE,
                                 stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            (stdout, stderr) = p.communicate(stdin.encode('ascii'))
            p = subprocess.Popen(command, env=env,
            (stdout, stderr) = p.communicate()
        rv = p.wait()
        stdout = stdout.decode('utf-8')
        stderr = stderr.decode('utf-8')
        return (stdout, stderr, rv)
    def test_goodFile(self):
        When a Python source file is all good, the return code is zero and no
        messages are printed to either stdout or stderr.
        open(self.tempfilepath, 'a').close()
        d = self.runPyflakes([self.tempfilepath])
        self.assertEqual(d, ('', '', 0))
    def test_fileWithFlakes(self):
        When a Python source file has warnings, the return code is non-zero
        and the warnings are printed to stdout.
        with open(self.tempfilepath, 'wb') as fd:
            fd.write(b"import contraband\n")
        expected = UnusedImport(self.tempfilepath, Node(1), 'contraband')
        self.assertEqual(d, (f"{expected}{os.linesep}", '', 1))
    def test_errors_io(self):
        When pyflakes finds errors with the files it's given, (if they don't
        exist, say), then the return code is non-zero and the errors are
        printed to stderr.
        error_msg = '{}: No such file or directory{}'.format(self.tempfilepath,
                                                             os.linesep)
        self.assertEqual(d, ('', error_msg, 1))
    def test_errors_syntax(self):
            fd.write(b"import")
        if sys.version_info >= (3, 13):
            message = "Expected one or more names after 'import'"
        error_msg = '{0}:1:7: {1}{2}import{2}      ^{2}'.format(
            self.tempfilepath, message, os.linesep)
    def test_readFromStdin(self):
        If no arguments are passed to C{pyflakes} then it reads from stdin.
        d = self.runPyflakes([], stdin='import contraband')
        expected = UnusedImport('<stdin>', Node(1), 'contraband')
class TestMain(IntegrationTests):
    Tests of the pyflakes main function.
            with SysStreamCapturing(stdin) as capture:
                main(args=paths)
        except SystemExit as e:
            self.assertIsInstance(e.code, bool)
            rv = int(e.code)
            return (capture.output, capture.error, rv)
            raise RuntimeError('SystemExit not raised')
from numpy.core._rational_tests import rational
from numpy.testing import (
     assert_, assert_equal, assert_array_equal, assert_raises, assert_warns,
     HAS_REFCOUNT
def test_array_array():
    tobj = type(object)
    ones11 = np.ones((1, 1), np.float64)
    tndarray = type(ones11)
    # Test is_ndarray
    assert_equal(np.array(ones11, dtype=np.float64), ones11)
    if HAS_REFCOUNT:
        old_refcount = sys.getrefcount(tndarray)
        np.array(ones11)
        assert_equal(old_refcount, sys.getrefcount(tndarray))
    # test None
    assert_equal(np.array(None, dtype=np.float64),
                 np.array(np.nan, dtype=np.float64))
        old_refcount = sys.getrefcount(tobj)
        np.array(None, dtype=np.float64)
        assert_equal(old_refcount, sys.getrefcount(tobj))
    # test scalar
    assert_equal(np.array(1.0, dtype=np.float64),
                 np.ones((), dtype=np.float64))
        old_refcount = sys.getrefcount(np.float64)
        np.array(np.array(1.0, dtype=np.float64), dtype=np.float64)
        assert_equal(old_refcount, sys.getrefcount(np.float64))
    # test string
    S2 = np.dtype((bytes, 2))
    S3 = np.dtype((bytes, 3))
    S5 = np.dtype((bytes, 5))
    assert_equal(np.array(b"1.0", dtype=np.float64),
    assert_equal(np.array(b"1.0").dtype, S3)
    assert_equal(np.array(b"1.0", dtype=bytes).dtype, S3)
    assert_equal(np.array(b"1.0", dtype=S2), np.array(b"1."))
    assert_equal(np.array(b"1", dtype=S5), np.ones((), dtype=S5))
    U2 = np.dtype((str, 2))
    U3 = np.dtype((str, 3))
    U5 = np.dtype((str, 5))
    assert_equal(np.array("1.0", dtype=np.float64),
    assert_equal(np.array("1.0").dtype, U3)
    assert_equal(np.array("1.0", dtype=str).dtype, U3)
    assert_equal(np.array("1.0", dtype=U2), np.array(str("1.")))
    assert_equal(np.array("1", dtype=U5), np.ones((), dtype=U5))
    builtins = getattr(__builtins__, '__dict__', __builtins__)
    assert_(hasattr(builtins, 'get'))
    # test memoryview
    dat = np.array(memoryview(b'1.0'), dtype=np.float64)
    assert_equal(dat, [49.0, 46.0, 48.0])
    assert_(dat.dtype.type is np.float64)
    dat = np.array(memoryview(b'1.0'))
    assert_equal(dat, [49, 46, 48])
    assert_(dat.dtype.type is np.uint8)
    # test array interface
    a = np.array(100.0, dtype=np.float64)
    o = type("o", (object,),
             dict(__array_interface__=a.__array_interface__))
    assert_equal(np.array(o, dtype=np.float64), a)
    # test array_struct interface
    a = np.array([(1, 4.0, 'Hello'), (2, 6.0, 'World')],
                 dtype=[('f0', int), ('f1', float), ('f2', str)])
             dict(__array_struct__=a.__array_struct__))
    ## wasn't what I expected... is np.array(o) supposed to equal a ?
    ## instead we get a array([...], dtype=">V18")
    assert_equal(bytes(np.array(o).data), bytes(a.data))
    # test array
             dict(__array__=lambda *x: np.array(100.0, dtype=np.float64)))()
    assert_equal(np.array(o, dtype=np.float64), np.array(100.0, np.float64))
    # test recursion
    nested = 1.5
    for i in range(np.MAXDIMS):
        nested = [nested]
    # no error
    np.array(nested)
    # Exceeds recursion limit
    assert_raises(ValueError, np.array, [nested], dtype=np.float64)
    # Try with lists...
    # float32
    assert_equal(np.array([None] * 10, dtype=np.float32),
                 np.full((10,), np.nan, dtype=np.float32))
    assert_equal(np.array([[None]] * 10, dtype=np.float32),
                 np.full((10, 1), np.nan, dtype=np.float32))
    assert_equal(np.array([[None] * 10], dtype=np.float32),
                 np.full((1, 10), np.nan, dtype=np.float32))
    assert_equal(np.array([[None] * 10] * 10, dtype=np.float32),
                 np.full((10, 10), np.nan, dtype=np.float32))
    # float64
    assert_equal(np.array([None] * 10, dtype=np.float64),
                 np.full((10,), np.nan, dtype=np.float64))
    assert_equal(np.array([[None]] * 10, dtype=np.float64),
                 np.full((10, 1), np.nan, dtype=np.float64))
    assert_equal(np.array([[None] * 10], dtype=np.float64),
                 np.full((1, 10), np.nan, dtype=np.float64))
    assert_equal(np.array([[None] * 10] * 10, dtype=np.float64),
                 np.full((10, 10), np.nan, dtype=np.float64))
    assert_equal(np.array([1.0] * 10, dtype=np.float64),
                 np.ones((10,), dtype=np.float64))
    assert_equal(np.array([[1.0]] * 10, dtype=np.float64),
                 np.ones((10, 1), dtype=np.float64))
    assert_equal(np.array([[1.0] * 10], dtype=np.float64),
                 np.ones((1, 10), dtype=np.float64))
    assert_equal(np.array([[1.0] * 10] * 10, dtype=np.float64),
                 np.ones((10, 10), dtype=np.float64))
    # Try with tuples
    assert_equal(np.array((None,) * 10, dtype=np.float64),
    assert_equal(np.array([(None,)] * 10, dtype=np.float64),
    assert_equal(np.array([(None,) * 10], dtype=np.float64),
    assert_equal(np.array([(None,) * 10] * 10, dtype=np.float64),
    assert_equal(np.array((1.0,) * 10, dtype=np.float64),
    assert_equal(np.array([(1.0,)] * 10, dtype=np.float64),
    assert_equal(np.array([(1.0,) * 10], dtype=np.float64),
    assert_equal(np.array([(1.0,) * 10] * 10, dtype=np.float64),
@pytest.mark.parametrize("array", [True, False])
def test_array_impossible_casts(array):
    # All builtin types can be forcibly cast, at least theoretically,
    # but user dtypes cannot necessarily.
    rt = rational(1, 2)
    if array:
        rt = np.array(rt)
    with assert_raises(TypeError):
        np.array(rt, dtype="M8")
# TODO: remove when fastCopyAndTranspose deprecation expires
@pytest.mark.parametrize("a",
        np.array(2),  # 0D array
        np.array([3, 2, 7, 0]),  # 1D array
        np.arange(6).reshape(2, 3)  # 2D array
def test_fastCopyAndTranspose(a):
    with pytest.deprecated_call():
        b = np.fastCopyAndTranspose(a)
        assert_equal(b, a.T)
        assert b.flags.owndata
def test_array_astype():
    a = np.arange(6, dtype='f4').reshape(2, 3)
    # Default behavior: allows unsafe casts, keeps memory layout,
    #                   always copies.
    b = a.astype('i4')
    assert_equal(a, b)
    assert_equal(b.dtype, np.dtype('i4'))
    assert_equal(a.strides, b.strides)
    b = a.T.astype('i4')
    assert_equal(a.T, b)
    assert_equal(a.T.strides, b.strides)
    b = a.astype('f4')
    assert_(not (a is b))
    # copy=False parameter can sometimes skip a copy
    b = a.astype('f4', copy=False)
    assert_(a is b)
    # order parameter allows overriding of the memory layout,
    # forcing a copy if the layout is wrong
    b = a.astype('f4', order='F', copy=False)
    assert_(b.flags.f_contiguous)
    b = a.astype('f4', order='C', copy=False)
    assert_(b.flags.c_contiguous)
    # casting parameter allows catching bad casts
    b = a.astype('c8', casting='safe')
    assert_equal(b.dtype, np.dtype('c8'))
    assert_raises(TypeError, a.astype, 'i4', casting='safe')
    # subok=False passes through a non-subclassed array
    b = a.astype('f4', subok=0, copy=False)
    class MyNDArray(np.ndarray):
    a = np.array([[0, 1, 2], [3, 4, 5]], dtype='f4').view(MyNDArray)
    # subok=True passes through a subclass
    b = a.astype('f4', subok=True, copy=False)
    # subok=True is default, and creates a subtype on a cast
    b = a.astype('i4', copy=False)
    assert_equal(type(b), MyNDArray)
    # subok=False never returns a subclass
    b = a.astype('f4', subok=False, copy=False)
    assert_(type(b) is not MyNDArray)
    # Make sure converting from string object to fixed length string
    # does not truncate.
    a = np.array([b'a'*100], dtype='O')
    b = a.astype('S')
    assert_equal(b.dtype, np.dtype('S100'))
    a = np.array(['a'*100], dtype='O')
    b = a.astype('U')
    assert_equal(b.dtype, np.dtype('U100'))
    # Same test as above but for strings shorter than 64 characters
    a = np.array([b'a'*10], dtype='O')
    assert_equal(b.dtype, np.dtype('S10'))
    a = np.array(['a'*10], dtype='O')
    assert_equal(b.dtype, np.dtype('U10'))
    a = np.array(123456789012345678901234567890, dtype='O').astype('S')
    assert_array_equal(a, np.array(b'1234567890' * 3, dtype='S30'))
    a = np.array(123456789012345678901234567890, dtype='O').astype('U')
    assert_array_equal(a, np.array('1234567890' * 3, dtype='U30'))
    a = np.array([123456789012345678901234567890], dtype='O').astype('S')
    a = np.array([123456789012345678901234567890], dtype='O').astype('U')
    a = np.array(123456789012345678901234567890, dtype='S')
    a = np.array(123456789012345678901234567890, dtype='U')
    a = np.array('a\u0140', dtype='U')
    b = np.ndarray(buffer=a, dtype='uint32', shape=2)
    assert_(b.size == 2)
    a = np.array([1000], dtype='i4')
    assert_raises(TypeError, a.astype, 'S1', casting='safe')
    a = np.array(1000, dtype='i4')
    assert_raises(TypeError, a.astype, 'U1', casting='safe')
    # gh-24023
    assert_raises(TypeError, a.astype)
@pytest.mark.parametrize("dt", ["S", "U"])
def test_array_astype_to_string_discovery_empty(dt):
    # See also gh-19085
    arr = np.array([""], dtype=object)
    # Note, the itemsize is the `0 -> 1` logic, which should change.
    # The important part the test is rather that it does not error.
    assert arr.astype(dt).dtype.itemsize == np.dtype(f"{dt}1").itemsize
    # check the same thing for `np.can_cast` (since it accepts arrays)
    assert np.can_cast(arr, dt, casting="unsafe")
    assert not np.can_cast(arr, dt, casting="same_kind")
    # as well as for the object as a descriptor:
    assert np.can_cast("O", dt, casting="unsafe")
@pytest.mark.parametrize("dt", ["d", "f", "S13", "U32"])
def test_array_astype_to_void(dt):
    dt = np.dtype(dt)
    arr = np.array([], dtype=dt)
    assert arr.astype("V").dtype.itemsize == dt.itemsize
def test_object_array_astype_to_void():
    # This is different to `test_array_astype_to_void` as object arrays
    # are inspected.  The default void is "V8" (8 is the length of double)
    arr = np.array([], dtype="O").astype("V")
    assert arr.dtype == "V8"
@pytest.mark.parametrize("t",
    np.sctypes['uint'] + np.sctypes['int'] + np.sctypes['float']
def test_array_astype_warning(t):
    # test ComplexWarning when casting from complex to float or int
    a = np.array(10, dtype=np.complex_)
    assert_warns(np.ComplexWarning, a.astype, t)
@pytest.mark.parametrize(["dtype", "out_dtype"],
        [(np.bytes_, np.bool_),
         (np.str_, np.bool_),
         (np.dtype("S10,S9"), np.dtype("?,?"))])
def test_string_to_boolean_cast(dtype, out_dtype):
    Currently, for `astype` strings are cast to booleans effectively by
    calling `bool(int(string)`. This is not consistent (see gh-9875) and
    will eventually be deprecated.
    arr = np.array(["10", "10\0\0\0", "0\0\0", "0"], dtype=dtype)
    expected = np.array([True, True, False, False], dtype=out_dtype)
    assert_array_equal(arr.astype(out_dtype), expected)
def test_string_to_boolean_cast_errors(dtype, out_dtype):
    These currently error out, since cast to integers fails, but should not
    error out in the future.
    for invalid in ["False", "True", "", "\0", "non-empty"]:
        arr = np.array([invalid], dtype=dtype)
        with assert_raises(ValueError):
            arr.astype(out_dtype)
@pytest.mark.parametrize("str_type", [str, bytes, np.str_, np.unicode_])
@pytest.mark.parametrize("scalar_type",
        [np.complex64, np.complex128, np.clongdouble])
def test_string_to_complex_cast(str_type, scalar_type):
    value = scalar_type(b"1+3j")
    assert scalar_type(value) == 1+3j
    assert np.array([value], dtype=object).astype(scalar_type)[()] == 1+3j
    assert np.array(value).astype(scalar_type)[()] == 1+3j
    arr = np.zeros(1, dtype=scalar_type)
    arr[0] = value
    assert arr[0] == 1+3j
@pytest.mark.parametrize("dtype", np.typecodes["AllFloat"])
def test_none_to_nan_cast(dtype):
    # Note that at the time of writing this test, the scalar constructors
    # reject None
    arr = np.zeros(1, dtype=dtype)
    arr[0] = None
    assert np.isnan(arr)[0]
    assert np.isnan(np.array(None, dtype=dtype))[()]
    assert np.isnan(np.array([None], dtype=dtype))[0]
    assert np.isnan(np.array(None).astype(dtype))[()]
def test_copyto_fromscalar():
    # Simple copy
    np.copyto(a, 1.5)
    assert_equal(a, 1.5)
    np.copyto(a.T, 2.5)
    assert_equal(a, 2.5)
    # Where-masked copy
    mask = np.array([[0, 1, 0], [0, 0, 1]], dtype='?')
    np.copyto(a, 3.5, where=mask)
    assert_equal(a, [[2.5, 3.5, 2.5], [2.5, 2.5, 3.5]])
    mask = np.array([[0, 1], [1, 1], [1, 0]], dtype='?')
    np.copyto(a.T, 4.5, where=mask)
    assert_equal(a, [[2.5, 4.5, 4.5], [4.5, 4.5, 3.5]])
def test_copyto():
    a = np.arange(6, dtype='i4').reshape(2, 3)
    np.copyto(a, [[3, 1, 5], [6, 2, 1]])
    assert_equal(a, [[3, 1, 5], [6, 2, 1]])
    # Overlapping copy should work
    np.copyto(a[:, :2], a[::-1, 1::-1])
    assert_equal(a, [[2, 6, 5], [1, 3, 1]])
    # Defaults to 'same_kind' casting
    assert_raises(TypeError, np.copyto, a, 1.5)
    # Force a copy with 'unsafe' casting, truncating 1.5 to 1
    np.copyto(a, 1.5, casting='unsafe')
    assert_equal(a, 1)
    # Copying with a mask
    np.copyto(a, 3, where=[True, False, True])
    assert_equal(a, [[3, 1, 3], [3, 1, 3]])
    # Casting rule still applies with a mask
    assert_raises(TypeError, np.copyto, a, 3.5, where=[True, False, True])
    # Lists of integer 0's and 1's is ok too
    np.copyto(a, 4.0, casting='unsafe', where=[[0, 1, 1], [1, 0, 0]])
    assert_equal(a, [[3, 4, 4], [4, 1, 3]])
    # Overlapping copy with mask should work
    np.copyto(a[:, :2], a[::-1, 1::-1], where=[[0, 1], [1, 1]])
    assert_equal(a, [[3, 4, 4], [4, 3, 3]])
    # 'dst' must be an array
    assert_raises(TypeError, np.copyto, [1, 2, 3], [2, 3, 4])
def test_copyto_permut():
    # test explicit overflow case
    pad = 500
    l = [True] * pad + [True, True, True, True]
    r = np.zeros(len(l)-pad)
    d = np.ones(len(l)-pad)
    mask = np.array(l)[pad:]
    np.copyto(r, d, where=mask[::-1])
    # test all permutation of possible masks, 9 should be sufficient for
    # current 4 byte unrolled code
    power = 9
    d = np.ones(power)
    for i in range(2**power):
        r = np.zeros(power)
        l = [(i & x) != 0 for x in range(power)]
        mask = np.array(l)
        np.copyto(r, d, where=mask)
        assert_array_equal(r == 1, l)
        assert_equal(r.sum(), sum(l))
        assert_array_equal(r == 1, l[::-1])
        np.copyto(r[::2], d[::2], where=mask[::2])
        assert_array_equal(r[::2] == 1, l[::2])
        assert_equal(r[::2].sum(), sum(l[::2]))
        np.copyto(r[::2], d[::2], where=mask[::-2])
        assert_array_equal(r[::2] == 1, l[::-2])
        assert_equal(r[::2].sum(), sum(l[::-2]))
        for c in [0xFF, 0x7F, 0x02, 0x10]:
            imask = np.array(l).view(np.uint8)
            imask[mask != 0] = c
    np.copyto(r, d, where=True)
    assert_equal(r.sum(), r.size)
    r = np.ones(power)
    d = np.zeros(power)
    np.copyto(r, d, where=False)
def test_copy_order():
    a = np.arange(24).reshape(2, 1, 3, 4)
    b = a.copy(order='F')
    c = np.arange(24).reshape(2, 1, 4, 3).swapaxes(2, 3)
    def check_copy_result(x, y, ccontig, fcontig, strides=False):
        assert_(not (x is y))
        assert_equal(x, y)
        assert_equal(res.flags.c_contiguous, ccontig)
        assert_equal(res.flags.f_contiguous, fcontig)
    # Validate the initial state of a, b, and c
    assert_(a.flags.c_contiguous)
    assert_(not a.flags.f_contiguous)
    assert_(not b.flags.c_contiguous)
    assert_(not c.flags.c_contiguous)
    assert_(not c.flags.f_contiguous)
    # Copy with order='C'
    res = a.copy(order='C')
    check_copy_result(res, a, ccontig=True, fcontig=False, strides=True)
    res = b.copy(order='C')
    check_copy_result(res, b, ccontig=True, fcontig=False, strides=False)
    res = c.copy(order='C')
    check_copy_result(res, c, ccontig=True, fcontig=False, strides=False)
    res = np.copy(a, order='C')
    res = np.copy(b, order='C')
    res = np.copy(c, order='C')
    # Copy with order='F'
    res = a.copy(order='F')
    check_copy_result(res, a, ccontig=False, fcontig=True, strides=False)
    res = b.copy(order='F')
    check_copy_result(res, b, ccontig=False, fcontig=True, strides=True)
    res = c.copy(order='F')
    check_copy_result(res, c, ccontig=False, fcontig=True, strides=False)
    res = np.copy(a, order='F')
    res = np.copy(b, order='F')
    res = np.copy(c, order='F')
    # Copy with order='K'
    res = a.copy(order='K')
    res = b.copy(order='K')
    res = c.copy(order='K')
    check_copy_result(res, c, ccontig=False, fcontig=False, strides=True)
    res = np.copy(a, order='K')
    res = np.copy(b, order='K')
    res = np.copy(c, order='K')
def test_contiguous_flags():
    a = np.ones((4, 4, 1))[::2,:,:]
    a.strides = a.strides[:2] + (-123,)
    b = np.ones((2, 2, 1, 2, 2)).swapaxes(3, 4)
    def check_contig(a, ccontig, fcontig):
        assert_(a.flags.c_contiguous == ccontig)
        assert_(a.flags.f_contiguous == fcontig)
    # Check if new arrays are correct:
    check_contig(a, False, False)
    check_contig(b, False, False)
    check_contig(np.empty((2, 2, 0, 2, 2)), True, True)
    check_contig(np.array([[[1], [2]]], order='F'), True, True)
    check_contig(np.empty((2, 2)), True, False)
    check_contig(np.empty((2, 2), order='F'), False, True)
    # Check that np.array creates correct contiguous flags:
    check_contig(np.array(a, copy=False), False, False)
    check_contig(np.array(a, copy=False, order='C'), True, False)
    check_contig(np.array(a, ndmin=4, copy=False, order='F'), False, True)
    # Check slicing update of flags and :
    check_contig(a[0], True, True)
    check_contig(a[None, ::4, ..., None], True, True)
    check_contig(b[0, 0, ...], False, True)
    check_contig(b[:, :, 0:0, :, :], True, True)
    # Test ravel and squeeze.
    check_contig(a.ravel(), True, True)
    check_contig(np.ones((1, 3, 1)).squeeze(), True, True)
def test_broadcast_arrays():
    # Test user defined dtypes
    a = np.array([(1, 2, 3)], dtype='u4,u4,u4')
    b = np.array([(1, 2, 3), (4, 5, 6), (7, 8, 9)], dtype='u4,u4,u4')
    result = np.broadcast_arrays(a, b)
    assert_equal(result[0], np.array([(1, 2, 3), (1, 2, 3), (1, 2, 3)], dtype='u4,u4,u4'))
    assert_equal(result[1], np.array([(1, 2, 3), (4, 5, 6), (7, 8, 9)], dtype='u4,u4,u4'))
@pytest.mark.parametrize(["shape", "fill_value", "expected_output"],
        [((2, 2), [5.0,  6.0], np.array([[5.0, 6.0], [5.0, 6.0]])),
         ((3, 2), [1.0,  2.0], np.array([[1.0, 2.0], [1.0, 2.0], [1.0,  2.0]]))])
def test_full_from_list(shape, fill_value, expected_output):
    output = np.full(shape, fill_value)
    assert_equal(output, expected_output)
def test_astype_copyflag():
    # test the various copyflag options
    arr = np.arange(10, dtype=np.intp)
    res_true = arr.astype(np.intp, copy=True)
    assert not np.may_share_memory(arr, res_true)
    res_always = arr.astype(np.intp, copy=np._CopyMode.ALWAYS)
    assert not np.may_share_memory(arr, res_always)
    res_false = arr.astype(np.intp, copy=False)
    # `res_false is arr` currently, but check `may_share_memory`.
    assert np.may_share_memory(arr, res_false)
    res_if_needed = arr.astype(np.intp, copy=np._CopyMode.IF_NEEDED)
    # `res_if_needed is arr` currently, but check `may_share_memory`.
    assert np.may_share_memory(arr, res_if_needed)
    res_never = arr.astype(np.intp, copy=np._CopyMode.NEVER)
    assert np.may_share_memory(arr, res_never)
    # Simple tests for when a copy is necessary:
    res_false = arr.astype(np.float64, copy=False)
    assert_array_equal(res_false, arr)
    res_if_needed = arr.astype(np.float64, 
                               copy=np._CopyMode.IF_NEEDED)
    assert_array_equal(res_if_needed, arr)
    assert_raises(ValueError, arr.astype, np.float64,
                  copy=np._CopyMode.NEVER)
