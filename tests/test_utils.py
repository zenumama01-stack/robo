from tweepy.utils import *
class TweepyUtilsTests(unittest.TestCase):
    def testlist_to_csv(self):
        self.assertEqual("1,2,3", list_to_csv([1,2,3]))
        self.assertEqual("bird,tweet,nest,egg",
                         list_to_csv(["bird", "tweet", "nest", "egg"]))
# Allow direct execution
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import ntpath
import unittest.mock
import xml.etree.ElementTree
from yt_dlp.compat import (
    compat_etree_fromstring,
    compat_HTMLParseError,
from yt_dlp.utils import (
    Config,
    DateRange,
    ExtractorError,
    InAdvancePagedList,
    LazyList,
    NO_DEFAULT,
    OnDemandPagedList,
    Popen,
    age_restricted,
    args_to_str,
    caesar,
    clean_html,
    clean_podcast_url,
    cli_bool_option,
    cli_option,
    cli_valueless_option,
    date_from_str,
    datetime_from_str,
    detect_exe_version,
    determine_ext,
    determine_file_encoding,
    dfxp2srt,
    encode_base_n,
    encode_compat_str,
    expand_path,
    extract_attributes,
    extract_basic_auth,
    find_xpath_attr,
    fix_xml_ampersands,
    float_or_none,
    format_bytes,
    get_compatible_ext,
    get_element_by_attribute,
    get_element_by_class,
    get_element_html_by_attribute,
    get_element_html_by_class,
    get_element_text_and_html_by_tag,
    get_elements_by_attribute,
    get_elements_by_class,
    get_elements_html_by_attribute,
    get_elements_html_by_class,
    get_elements_text_and_html_by_attribute,
    int_or_none,
    iri_to_uri,
    is_html,
    js_to_json,
    jwt_decode_hs256,
    jwt_encode,
    limit_length,
    locked_file,
    lowercase_escape,
    match_str,
    merge_dicts,
    mimetype2ext,
    month_by_name,
    multipart_encode,
    ohdave_rsa_encrypt,
    orderedSet,
    parse_age_limit,
    parse_bitrate,
    parse_codecs,
    parse_count,
    parse_dfxp_time_expr,
    parse_duration,
    parse_filesize,
    parse_iso8601,
    parse_qs,
    parse_resolution,
    pkcs1pad,
    prepend_extension,
    read_batch_urls,
    remove_end,
    remove_quotes,
    remove_start,
    render_table,
    replace_extension,
    datetime_round,
    rot47,
    sanitize_filename,
    sanitize_path,
    sanitize_url,
    shell_quote,
    strftime_or_none,
    smuggle_url,
    str_to_int,
    strip_jsonp,
    strip_or_none,
    subtitles_filename,
    timeconvert,
    try_call,
    unescapeHTML,
    unified_strdate,
    unified_timestamp,
    unsmuggle_url,
    update_url_query,
    uppercase_escape,
    url_basename,
    url_or_none,
    urlencode_postdata,
    urljoin,
    urshift,
    variadic,
    version_tuple,
    xpath_attr,
    xpath_element,
    xpath_text,
    xpath_with_ns,
from yt_dlp.utils._utils import _UnsafeExtensionError
from yt_dlp.utils.networking import (
    HTTPHeaderDict,
    escape_rfc3986,
    normalize_url,
    remove_dot_segments,
class TestUtil(unittest.TestCase):
    def test_timeconvert(self):
        self.assertTrue(timeconvert('') is None)
        self.assertTrue(timeconvert('bougrg') is None)
    def test_sanitize_filename(self):
        self.assertEqual(sanitize_filename(''), '')
        self.assertEqual(sanitize_filename('abc'), 'abc')
        self.assertEqual(sanitize_filename('abc_d-e'), 'abc_d-e')
        self.assertEqual(sanitize_filename('123'), '123')
        self.assertEqual('abc⧸de', sanitize_filename('abc/de'))
        self.assertFalse('/' in sanitize_filename('abc/de///'))
        self.assertEqual('abc_de', sanitize_filename('abc/<>\\*|de', is_id=False))
        self.assertEqual('xxx', sanitize_filename('xxx/<>\\*|', is_id=False))
        self.assertEqual('yes no', sanitize_filename('yes? no', is_id=False))
        self.assertEqual('this - that', sanitize_filename('this: that', is_id=False))
        self.assertEqual(sanitize_filename('AT&T'), 'AT&T')
        aumlaut = 'ä'
        self.assertEqual(sanitize_filename(aumlaut), aumlaut)
        tests = '\u043a\u0438\u0440\u0438\u043b\u043b\u0438\u0446\u0430'
        self.assertEqual(sanitize_filename(tests), tests)
            sanitize_filename('New World record at 0:12:34'),
            'New World record at 0_12_34')
        self.assertEqual(sanitize_filename('--gasdgf'), '--gasdgf')
        self.assertEqual(sanitize_filename('--gasdgf', is_id=True), '--gasdgf')
        self.assertEqual(sanitize_filename('--gasdgf', is_id=False), '_-gasdgf')
        self.assertEqual(sanitize_filename('.gasdgf'), '.gasdgf')
        self.assertEqual(sanitize_filename('.gasdgf', is_id=True), '.gasdgf')
        self.assertEqual(sanitize_filename('.gasdgf', is_id=False), 'gasdgf')
        forbidden = '"\0\\/'
        for fc in forbidden:
            for fbc in forbidden:
                self.assertTrue(fbc not in sanitize_filename(fc))
    def test_sanitize_filename_restricted(self):
        self.assertEqual(sanitize_filename('abc', restricted=True), 'abc')
        self.assertEqual(sanitize_filename('abc_d-e', restricted=True), 'abc_d-e')
        self.assertEqual(sanitize_filename('123', restricted=True), '123')
        self.assertEqual('abc_de', sanitize_filename('abc/de', restricted=True))
        self.assertFalse('/' in sanitize_filename('abc/de///', restricted=True))
        self.assertEqual('abc_de', sanitize_filename('abc/<>\\*|de', restricted=True))
        self.assertEqual('xxx', sanitize_filename('xxx/<>\\*|', restricted=True))
        self.assertEqual('yes_no', sanitize_filename('yes? no', restricted=True))
        self.assertEqual('this_-_that', sanitize_filename('this: that', restricted=True))
        tests = 'aäb\u4e2d\u56fd\u7684c'
        self.assertEqual(sanitize_filename(tests, restricted=True), 'aab_c')
        self.assertTrue(sanitize_filename('\xf6', restricted=True) != '')  # No empty filename
        forbidden = '"\0\\/&!: \'\t\n()[]{}$;`^,#'
                self.assertTrue(fbc not in sanitize_filename(fc, restricted=True))
        # Handle a common case more neatly
        self.assertEqual(sanitize_filename('\u5927\u58f0\u5e26 - Song', restricted=True), 'Song')
        self.assertEqual(sanitize_filename('\u603b\u7edf: Speech', restricted=True), 'Speech')
        # .. but make sure the file name is never empty
        self.assertTrue(sanitize_filename('-', restricted=True) != '')
        self.assertTrue(sanitize_filename(':', restricted=True) != '')
        self.assertEqual(sanitize_filename(
            'ÂÃÄÀÁÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖŐØŒÙÚÛÜŰÝÞßàáâãäåæçèéêëìíîïðñòóôõöőøœùúûüűýþÿ', restricted=True),
            'AAAAAAAECEEEEIIIIDNOOOOOOOOEUUUUUYTHssaaaaaaaeceeeeiiiionooooooooeuuuuuythy')
    def test_sanitize_ids(self):
        self.assertEqual(sanitize_filename('_n_cd26wFpw', is_id=True), '_n_cd26wFpw')
        self.assertEqual(sanitize_filename('_BD_eEpuzXw', is_id=True), '_BD_eEpuzXw')
        self.assertEqual(sanitize_filename('N0Y__7-UOdI', is_id=True), 'N0Y__7-UOdI')
    @unittest.mock.patch('sys.platform', 'win32')
    def test_sanitize_path(self):
        self.assertEqual(sanitize_path('abc'), 'abc')
        self.assertEqual(sanitize_path('abc/def'), 'abc\\def')
        self.assertEqual(sanitize_path('abc\\def'), 'abc\\def')
        self.assertEqual(sanitize_path('abc|def'), 'abc#def')
        self.assertEqual(sanitize_path('<>:"|?*'), '#######')
        self.assertEqual(sanitize_path('C:/abc/def'), 'C:\\abc\\def')
        self.assertEqual(sanitize_path('C?:/abc/def'), 'C##\\abc\\def')
        self.assertEqual(sanitize_path('\\\\?\\UNC\\ComputerName\\abc'), '\\\\?\\UNC\\ComputerName\\abc')
        self.assertEqual(sanitize_path('\\\\?\\UNC/ComputerName/abc'), '\\\\?\\UNC\\ComputerName\\abc')
        self.assertEqual(sanitize_path('\\\\?\\C:\\abc'), '\\\\?\\C:\\abc')
        self.assertEqual(sanitize_path('\\\\?\\C:/abc'), '\\\\?\\C:\\abc')
        self.assertEqual(sanitize_path('\\\\?\\C:\\ab?c\\de:f'), '\\\\?\\C:\\ab#c\\de#f')
            sanitize_path('youtube/%(uploader)s/%(autonumber)s-%(title)s-%(upload_date)s.%(ext)s'),
            'youtube\\%(uploader)s\\%(autonumber)s-%(title)s-%(upload_date)s.%(ext)s')
            sanitize_path('youtube/TheWreckingYard ./00001-Not bad, Especially for Free! (1987 Yamaha 700)-20141116.mp4.part'),
            'youtube\\TheWreckingYard #\\00001-Not bad, Especially for Free! (1987 Yamaha 700)-20141116.mp4.part')
        self.assertEqual(sanitize_path('abc/def...'), 'abc\\def..#')
        self.assertEqual(sanitize_path('abc.../def'), 'abc..#\\def')
        self.assertEqual(sanitize_path('abc.../def...'), 'abc..#\\def..#')
        self.assertEqual(sanitize_path('C:\\abc:%(title)s.%(ext)s'), 'C:\\abc#%(title)s.%(ext)s')
        for test, expected in [
            ('C:\\', 'C:\\'),
            ('../abc', '..\\abc'),
            ('../../abc', '..\\..\\abc'),
            ('./abc', 'abc'),
            ('./../abc', '..\\abc'),
            ('\\abc', '\\abc'),
            ('C:abc', 'C:abc'),
            ('C:abc\\..\\', 'C:'),
            ('C:abc\\..\\def\\..\\..\\', 'C:..'),
            ('C:\\abc\\xyz///..\\def\\', 'C:\\abc\\def'),
            ('abc/../', '.'),
            ('./abc/../', '.'),
            result = sanitize_path(test)
            assert result == expected, f'{test} was incorrectly resolved'
            assert result == sanitize_path(result), f'{test} changed after sanitizing again'
            assert result == ntpath.normpath(test), f'{test} does not match ntpath.normpath'
    def test_sanitize_url(self):
        self.assertEqual(sanitize_url('//foo.bar'), 'http://foo.bar')
        self.assertEqual(sanitize_url('httpss://foo.bar'), 'https://foo.bar')
        self.assertEqual(sanitize_url('rmtps://foo.bar'), 'rtmps://foo.bar')
        self.assertEqual(sanitize_url('https://foo.bar'), 'https://foo.bar')
        self.assertEqual(sanitize_url('foo bar'), 'foo bar')
    def test_expand_path(self):
        def env(var):
            return f'%{var}%' if sys.platform == 'win32' else f'${var}'
        os.environ['yt_dlp_EXPATH_PATH'] = 'expanded'
        self.assertEqual(expand_path(env('yt_dlp_EXPATH_PATH')), 'expanded')
        old_home = os.environ.get('HOME')
        test_str = R'C:\Documents and Settings\тест\Application Data'
            os.environ['HOME'] = test_str
            self.assertEqual(expand_path(env('HOME')), os.getenv('HOME'))
            self.assertEqual(expand_path('~'), os.getenv('HOME'))
                expand_path('~/{}'.format(env('yt_dlp_EXPATH_PATH'))),
                '{}/expanded'.format(os.getenv('HOME')))
            os.environ['HOME'] = old_home or ''
    _uncommon_extensions = [
        ('exe', 'abc.exe.ext'),
        ('de', 'abc.de.ext'),
        ('../.mp4', None),
        ('..\\.mp4', None),
    def test_prepend_extension(self):
        self.assertEqual(prepend_extension('abc.ext', 'temp'), 'abc.temp.ext')
        self.assertEqual(prepend_extension('abc.ext', 'temp', 'ext'), 'abc.temp.ext')
        self.assertEqual(prepend_extension('abc.unexpected_ext', 'temp', 'ext'), 'abc.unexpected_ext.temp')
        self.assertEqual(prepend_extension('abc', 'temp'), 'abc.temp')
        self.assertEqual(prepend_extension('.abc', 'temp'), '.abc.temp')
        self.assertEqual(prepend_extension('.abc.ext', 'temp'), '.abc.temp.ext')
        # Test uncommon extensions
        self.assertEqual(prepend_extension('abc.ext', 'bin'), 'abc.bin.ext')
        for ext, result in self._uncommon_extensions:
            with self.assertRaises(_UnsafeExtensionError):
                prepend_extension('abc', ext)
                self.assertEqual(prepend_extension('abc.ext', ext, 'ext'), result)
                    prepend_extension('abc.ext', ext, 'ext')
                prepend_extension('abc.unexpected_ext', ext, 'ext')
    def test_replace_extension(self):
        self.assertEqual(replace_extension('abc.ext', 'temp'), 'abc.temp')
        self.assertEqual(replace_extension('abc.ext', 'temp', 'ext'), 'abc.temp')
        self.assertEqual(replace_extension('abc.unexpected_ext', 'temp', 'ext'), 'abc.unexpected_ext.temp')
        self.assertEqual(replace_extension('abc', 'temp'), 'abc.temp')
        self.assertEqual(replace_extension('.abc', 'temp'), '.abc.temp')
        self.assertEqual(replace_extension('.abc.ext', 'temp'), '.abc.temp')
        self.assertEqual(replace_extension('abc.ext', 'bin'), 'abc.unknown_video')
        for ext, _ in self._uncommon_extensions:
                replace_extension('abc', ext)
                replace_extension('abc.ext', ext, 'ext')
                replace_extension('abc.unexpected_ext', ext, 'ext')
    def test_subtitles_filename(self):
        self.assertEqual(subtitles_filename('abc.ext', 'en', 'vtt'), 'abc.en.vtt')
        self.assertEqual(subtitles_filename('abc.ext', 'en', 'vtt', 'ext'), 'abc.en.vtt')
        self.assertEqual(subtitles_filename('abc.unexpected_ext', 'en', 'vtt', 'ext'), 'abc.unexpected_ext.en.vtt')
    def test_remove_start(self):
        self.assertEqual(remove_start(None, 'A - '), None)
        self.assertEqual(remove_start('A - B', 'A - '), 'B')
        self.assertEqual(remove_start('B - A', 'A - '), 'B - A')
        self.assertEqual(remove_start('non-empty', ''), 'non-empty')
    def test_remove_end(self):
        self.assertEqual(remove_end(None, ' - B'), None)
        self.assertEqual(remove_end('A - B', ' - B'), 'A')
        self.assertEqual(remove_end('B - A', ' - B'), 'B - A')
        self.assertEqual(remove_end('non-empty', ''), 'non-empty')
    def test_remove_quotes(self):
        self.assertEqual(remove_quotes(None), None)
        self.assertEqual(remove_quotes('"'), '"')
        self.assertEqual(remove_quotes("'"), "'")
        self.assertEqual(remove_quotes(';'), ';')
        self.assertEqual(remove_quotes('";'), '";')
        self.assertEqual(remove_quotes('""'), '')
        self.assertEqual(remove_quotes('";"'), ';')
    def test_ordered_set(self):
        self.assertEqual(orderedSet([1, 1, 2, 3, 4, 4, 5, 6, 7, 3, 5]), [1, 2, 3, 4, 5, 6, 7])
        self.assertEqual(orderedSet([]), [])
        self.assertEqual(orderedSet([1]), [1])
        # keep the list ordered
        self.assertEqual(orderedSet([135, 1, 1, 1]), [135, 1])
    def test_unescape_html(self):
        self.assertEqual(unescapeHTML('%20;'), '%20;')
        self.assertEqual(unescapeHTML('&#x2F;'), '/')
        self.assertEqual(unescapeHTML('&#47;'), '/')
        self.assertEqual(unescapeHTML('&eacute;'), 'é')
        self.assertEqual(unescapeHTML('&#2013266066;'), '&#2013266066;')
        self.assertEqual(unescapeHTML('&a&quot;'), '&a"')
        # HTML5 entities
        self.assertEqual(unescapeHTML('&period;&apos;'), '.\'')
    def test_date_from_str(self):
        self.assertEqual(date_from_str('yesterday'), date_from_str('now-1day'))
        self.assertEqual(date_from_str('now+7day'), date_from_str('now+1week'))
        self.assertEqual(date_from_str('now+14day'), date_from_str('now+2week'))
        self.assertEqual(date_from_str('20200229+365day'), date_from_str('20200229+1year'))
        self.assertEqual(date_from_str('20210131+28day'), date_from_str('20210131+1month'))
    def test_datetime_from_str(self):
        self.assertEqual(datetime_from_str('yesterday', precision='day'), datetime_from_str('now-1day', precision='auto'))
        self.assertEqual(datetime_from_str('now+7day', precision='day'), datetime_from_str('now+1week', precision='auto'))
        self.assertEqual(datetime_from_str('now+14day', precision='day'), datetime_from_str('now+2week', precision='auto'))
        self.assertEqual(datetime_from_str('20200229+365day', precision='day'), datetime_from_str('20200229+1year', precision='auto'))
        self.assertEqual(datetime_from_str('20210131+28day', precision='day'), datetime_from_str('20210131+1month', precision='auto'))
        self.assertEqual(datetime_from_str('20210131+59day', precision='day'), datetime_from_str('20210131+2month', precision='auto'))
        self.assertEqual(datetime_from_str('now+1day', precision='hour'), datetime_from_str('now+24hours', precision='auto'))
        self.assertEqual(datetime_from_str('now+23hours', precision='hour'), datetime_from_str('now+23hours', precision='auto'))
    def test_datetime_round(self):
        self.assertEqual(datetime_round(dt.datetime.strptime('1820-05-12T01:23:45Z', '%Y-%m-%dT%H:%M:%SZ')),
                         dt.datetime(1820, 5, 12, tzinfo=dt.timezone.utc))
        self.assertEqual(datetime_round(dt.datetime.strptime('1969-12-31T23:34:45Z', '%Y-%m-%dT%H:%M:%SZ'), 'hour'),
                         dt.datetime(1970, 1, 1, 0, tzinfo=dt.timezone.utc))
        self.assertEqual(datetime_round(dt.datetime.strptime('2024-12-25T01:23:45Z', '%Y-%m-%dT%H:%M:%SZ'), 'minute'),
                         dt.datetime(2024, 12, 25, 1, 24, tzinfo=dt.timezone.utc))
        self.assertEqual(datetime_round(dt.datetime.strptime('2024-12-25T01:23:45.123Z', '%Y-%m-%dT%H:%M:%S.%fZ'), 'second'),
                         dt.datetime(2024, 12, 25, 1, 23, 45, tzinfo=dt.timezone.utc))
        self.assertEqual(datetime_round(dt.datetime.strptime('2024-12-25T01:23:45.678Z', '%Y-%m-%dT%H:%M:%S.%fZ'), 'second'),
                         dt.datetime(2024, 12, 25, 1, 23, 46, tzinfo=dt.timezone.utc))
    def test_strftime_or_none(self):
        self.assertEqual(strftime_or_none(-4722192000), '18200512')
        self.assertEqual(strftime_or_none(0), '19700101')
        self.assertEqual(strftime_or_none(1735084800), '20241225')
        # Throws OverflowError
        self.assertEqual(strftime_or_none(1735084800000), None)
    def test_daterange(self):
        _20century = DateRange('19000101', '20000101')
        self.assertFalse('17890714' in _20century)
        _ac = DateRange('00010101')
        self.assertTrue('19690721' in _ac)
        _firstmilenium = DateRange(end='10000101')
        self.assertTrue('07110427' in _firstmilenium)
    def test_unified_dates(self):
        self.assertEqual(unified_strdate('December 21, 2010'), '20101221')
        self.assertEqual(unified_strdate('8/7/2009'), '20090708')
        self.assertEqual(unified_strdate('Dec 14, 2012'), '20121214')
        self.assertEqual(unified_strdate('2012/10/11 01:56:38 +0000'), '20121011')
        self.assertEqual(unified_strdate('1968 12 10'), '19681210')
        self.assertEqual(unified_strdate('1968-12-10'), '19681210')
        self.assertEqual(unified_strdate('31-07-2022 20:00'), '20220731')
        self.assertEqual(unified_strdate('28/01/2014 21:00:00 +0100'), '20140128')
            unified_strdate('11/26/2014 11:30:00 AM PST', day_first=False),
            '20141126')
            unified_strdate('2/2/2015 6:47:40 PM', day_first=False),
            '20150202')
        self.assertEqual(unified_strdate('Feb 14th 2016 5:45PM'), '20160214')
        self.assertEqual(unified_strdate('25-09-2014'), '20140925')
        self.assertEqual(unified_strdate('27.02.2016 17:30'), '20160227')
        self.assertEqual(unified_strdate('UNKNOWN DATE FORMAT'), None)
        self.assertEqual(unified_strdate('Feb 7, 2016 at 6:35 pm'), '20160207')
        self.assertEqual(unified_strdate('July 15th, 2013'), '20130715')
        self.assertEqual(unified_strdate('September 1st, 2013'), '20130901')
        self.assertEqual(unified_strdate('Sep 2nd, 2013'), '20130902')
        self.assertEqual(unified_strdate('November 3rd, 2019'), '20191103')
        self.assertEqual(unified_strdate('October 23rd, 2005'), '20051023')
    def test_unified_timestamps(self):
        self.assertEqual(unified_timestamp('December 21, 2010'), 1292889600)
        self.assertEqual(unified_timestamp('8/7/2009'), 1247011200)
        self.assertEqual(unified_timestamp('Dec 14, 2012'), 1355443200)
        self.assertEqual(unified_timestamp('2012/10/11 01:56:38 +0000'), 1349920598)
        self.assertEqual(unified_timestamp('1968 12 10'), -33436800)
        self.assertEqual(unified_timestamp('1968-12-10'), -33436800)
        self.assertEqual(unified_timestamp('28/01/2014 21:00:00 +0100'), 1390939200)
            unified_timestamp('11/26/2014 11:30:00 AM PST', day_first=False),
            1417001400)
            unified_timestamp('2/2/2015 6:47:40 PM', day_first=False),
            1422902860)
        self.assertEqual(unified_timestamp('Feb 14th 2016 5:45PM'), 1455471900)
        self.assertEqual(unified_timestamp('25-09-2014'), 1411603200)
        self.assertEqual(unified_timestamp('27.02.2016 17:30'), 1456594200)
        self.assertEqual(unified_timestamp('UNKNOWN DATE FORMAT'), None)
        self.assertEqual(unified_timestamp('May 16, 2016 11:15 PM'), 1463440500)
        self.assertEqual(unified_timestamp('Feb 7, 2016 at 6:35 pm'), 1454870100)
        self.assertEqual(unified_timestamp('2017-03-30T17:52:41Q'), 1490896361)
        self.assertEqual(unified_timestamp('Sep 11, 2013 | 5:49 AM'), 1378878540)
        self.assertEqual(unified_timestamp('December 15, 2017 at 7:49 am'), 1513324140)
        self.assertEqual(unified_timestamp('2018-03-14T08:32:43.1493874+00:00'), 1521016363)
        self.assertEqual(unified_timestamp('Sunday, 26 Nov 2006, 19:00'), 1164567600)
        self.assertEqual(unified_timestamp('wed, aug 16, 2008, 12:00pm'), 1218931200)
        self.assertEqual(unified_timestamp('December 31 1969 20:00:01 EDT'), 1)
        self.assertEqual(unified_timestamp('Wednesday 31 December 1969 18:01:26 MDT'), 86)
        self.assertEqual(unified_timestamp('12/31/1969 20:01:18 EDT', False), 78)
        self.assertEqual(unified_timestamp('2026-01-01 00:00:00', tz_offset=0), 1767225600)
        self.assertEqual(unified_timestamp('2026-01-01 00:00:00', tz_offset=8), 1767196800)
        self.assertEqual(unified_timestamp('2026-01-01 00:00:00 +0800', tz_offset=-5), 1767196800)
    def test_determine_ext(self):
        self.assertEqual(determine_ext('http://example.com/foo/bar.mp4/?download'), 'mp4')
        self.assertEqual(determine_ext('http://example.com/foo/bar/?download', None), None)
        self.assertEqual(determine_ext('http://example.com/foo/bar.nonext/?download', None), None)
        self.assertEqual(determine_ext('http://example.com/foo/bar/mp4?download', None), None)
        self.assertEqual(determine_ext('http://example.com/foo/bar.m3u8//?download'), 'm3u8')
        self.assertEqual(determine_ext('foobar', None), None)
    def test_find_xpath_attr(self):
        testxml = '''<root>
            <node/>
            <node x="a"/>
            <node x="a" y="c" />
            <node x="b" y="d" />
            <node x="" />
        </root>'''
        doc = compat_etree_fromstring(testxml)
        self.assertEqual(find_xpath_attr(doc, './/fourohfour', 'n'), None)
        self.assertEqual(find_xpath_attr(doc, './/fourohfour', 'n', 'v'), None)
        self.assertEqual(find_xpath_attr(doc, './/node', 'n'), None)
        self.assertEqual(find_xpath_attr(doc, './/node', 'n', 'v'), None)
        self.assertEqual(find_xpath_attr(doc, './/node', 'x'), doc[1])
        self.assertEqual(find_xpath_attr(doc, './/node', 'x', 'a'), doc[1])
        self.assertEqual(find_xpath_attr(doc, './/node', 'x', 'b'), doc[3])
        self.assertEqual(find_xpath_attr(doc, './/node', 'y'), doc[2])
        self.assertEqual(find_xpath_attr(doc, './/node', 'y', 'c'), doc[2])
        self.assertEqual(find_xpath_attr(doc, './/node', 'y', 'd'), doc[3])
        self.assertEqual(find_xpath_attr(doc, './/node', 'x', ''), doc[4])
    def test_xpath_with_ns(self):
        testxml = '''<root xmlns:media="http://example.com/">
            <media:song>
                <media:author>The Author</media:author>
                <url>http://server.com/download.mp3</url>
            </media:song>
        find = lambda p: doc.find(xpath_with_ns(p, {'media': 'http://example.com/'}))
        self.assertTrue(find('media:song') is not None)
        self.assertEqual(find('media:song/media:author').text, 'The Author')
        self.assertEqual(find('media:song/url').text, 'http://server.com/download.mp3')
    def test_xpath_element(self):
        doc = xml.etree.ElementTree.Element('root')
        div = xml.etree.ElementTree.SubElement(doc, 'div')
        p = xml.etree.ElementTree.SubElement(div, 'p')
        p.text = 'Foo'
        self.assertEqual(xpath_element(doc, 'div/p'), p)
        self.assertEqual(xpath_element(doc, ['div/p']), p)
        self.assertEqual(xpath_element(doc, ['div/bar', 'div/p']), p)
        self.assertEqual(xpath_element(doc, 'div/bar', default='default'), 'default')
        self.assertEqual(xpath_element(doc, ['div/bar'], default='default'), 'default')
        self.assertTrue(xpath_element(doc, 'div/bar') is None)
        self.assertTrue(xpath_element(doc, ['div/bar']) is None)
        self.assertTrue(xpath_element(doc, ['div/bar'], 'div/baz') is None)
        self.assertRaises(ExtractorError, xpath_element, doc, 'div/bar', fatal=True)
        self.assertRaises(ExtractorError, xpath_element, doc, ['div/bar'], fatal=True)
        self.assertRaises(ExtractorError, xpath_element, doc, ['div/bar', 'div/baz'], fatal=True)
    def test_xpath_text(self):
                <p>Foo</p>
        self.assertEqual(xpath_text(doc, 'div/p'), 'Foo')
        self.assertEqual(xpath_text(doc, 'div/bar', default='default'), 'default')
        self.assertTrue(xpath_text(doc, 'div/bar') is None)
        self.assertRaises(ExtractorError, xpath_text, doc, 'div/bar', fatal=True)
    def test_xpath_attr(self):
                <p x="a">Foo</p>
        self.assertEqual(xpath_attr(doc, 'div/p', 'x'), 'a')
        self.assertEqual(xpath_attr(doc, 'div/bar', 'x'), None)
        self.assertEqual(xpath_attr(doc, 'div/p', 'y'), None)
        self.assertEqual(xpath_attr(doc, 'div/bar', 'x', default='default'), 'default')
        self.assertEqual(xpath_attr(doc, 'div/p', 'y', default='default'), 'default')
        self.assertRaises(ExtractorError, xpath_attr, doc, 'div/bar', 'x', fatal=True)
        self.assertRaises(ExtractorError, xpath_attr, doc, 'div/p', 'y', fatal=True)
    def test_smuggle_url(self):
        data = {'ö': 'ö', 'abc': [3]}
        url = 'https://foo.bar/baz?x=y#a'
        smug_url = smuggle_url(url, data)
        unsmug_url, unsmug_data = unsmuggle_url(smug_url)
        self.assertEqual(url, unsmug_url)
        self.assertEqual(data, unsmug_data)
        res_url, res_data = unsmuggle_url(url)
        self.assertEqual(res_url, url)
        self.assertEqual(res_data, None)
        smug_url = smuggle_url(url, {'a': 'b'})
        smug_smug_url = smuggle_url(smug_url, {'c': 'd'})
        res_url, res_data = unsmuggle_url(smug_smug_url)
        self.assertEqual(res_data, {'a': 'b', 'c': 'd'})
    def test_shell_quote(self):
        args = ['ffmpeg', '-i', 'ñ€ß\'.mp4']
            shell_quote(args),
            """ffmpeg -i 'ñ€ß'"'"'.mp4'""" if os.name != 'nt' else '''ffmpeg -i "ñ€ß'.mp4"''')
    def test_float_or_none(self):
        self.assertEqual(float_or_none('42.42'), 42.42)
        self.assertEqual(float_or_none('42'), 42.0)
        self.assertEqual(float_or_none(''), None)
        self.assertEqual(float_or_none(None), None)
        self.assertEqual(float_or_none([]), None)
        self.assertEqual(float_or_none(set()), None)
    def test_int_or_none(self):
        self.assertEqual(int_or_none('42'), 42)
        self.assertEqual(int_or_none(''), None)
        self.assertEqual(int_or_none(None), None)
        self.assertEqual(int_or_none([]), None)
        self.assertEqual(int_or_none(set()), None)
    def test_str_to_int(self):
        self.assertEqual(str_to_int('123,456'), 123456)
        self.assertEqual(str_to_int('123.456'), 123456)
        self.assertEqual(str_to_int(523), 523)
        self.assertEqual(str_to_int('noninteger'), None)
        self.assertEqual(str_to_int([]), None)
    def test_url_basename(self):
        self.assertEqual(url_basename('http://foo.de/'), '')
        self.assertEqual(url_basename('http://foo.de/bar/baz'), 'baz')
        self.assertEqual(url_basename('http://foo.de/bar/baz?x=y'), 'baz')
        self.assertEqual(url_basename('http://foo.de/bar/baz#x=y'), 'baz')
        self.assertEqual(url_basename('http://foo.de/bar/baz/'), 'baz')
            url_basename('http://media.w3.org/2010/05/sintel/trailer.mp4'),
            'trailer.mp4')
    def test_base_url(self):
        self.assertEqual(base_url('http://foo.de/'), 'http://foo.de/')
        self.assertEqual(base_url('http://foo.de/bar'), 'http://foo.de/')
        self.assertEqual(base_url('http://foo.de/bar/'), 'http://foo.de/bar/')
        self.assertEqual(base_url('http://foo.de/bar/baz'), 'http://foo.de/bar/')
        self.assertEqual(base_url('http://foo.de/bar/baz?x=z/x/c'), 'http://foo.de/bar/')
        self.assertEqual(base_url('http://foo.de/bar/baz&x=z&w=y/x/c'), 'http://foo.de/bar/baz&x=z&w=y/x/')
        self.assertEqual(urljoin('http://foo.de/', '/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin(b'http://foo.de/', '/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de/', b'/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin(b'http://foo.de/', b'/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('//foo.de/', '/a/b/c.txt'), '//foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de/', 'a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de', '/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de', 'a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de/', 'http://foo.de/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de/', '//foo.de/a/b/c.txt'), '//foo.de/a/b/c.txt')
        self.assertEqual(urljoin(None, 'http://foo.de/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin(None, '//foo.de/a/b/c.txt'), '//foo.de/a/b/c.txt')
        self.assertEqual(urljoin('', 'http://foo.de/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin(['foobar'], 'http://foo.de/a/b/c.txt'), 'http://foo.de/a/b/c.txt')
        self.assertEqual(urljoin('http://foo.de/', None), None)
        self.assertEqual(urljoin('http://foo.de/', ''), None)
        self.assertEqual(urljoin('http://foo.de/', ['foobar']), None)
        self.assertEqual(urljoin('http://foo.de/a/b/c.txt', '.././../d.txt'), 'http://foo.de/d.txt')
        self.assertEqual(urljoin('http://foo.de/a/b/c.txt', 'rtmp://foo.de'), 'rtmp://foo.de')
        self.assertEqual(urljoin(None, 'rtmp://foo.de'), 'rtmp://foo.de')
    def test_url_or_none(self):
        self.assertEqual(url_or_none(None), None)
        self.assertEqual(url_or_none(''), None)
        self.assertEqual(url_or_none('foo'), None)
        self.assertEqual(url_or_none('http://foo.de'), 'http://foo.de')
        self.assertEqual(url_or_none('https://foo.de'), 'https://foo.de')
        self.assertEqual(url_or_none('http$://foo.de'), None)
        self.assertEqual(url_or_none('//foo.de'), '//foo.de')
        self.assertEqual(url_or_none('s3://foo.de'), None)
        self.assertEqual(url_or_none('rtmpte://foo.de'), 'rtmpte://foo.de')
        self.assertEqual(url_or_none('mms://foo.de'), 'mms://foo.de')
        self.assertEqual(url_or_none('rtspu://foo.de'), 'rtspu://foo.de')
        self.assertEqual(url_or_none('ftps://foo.de'), 'ftps://foo.de')
        self.assertEqual(url_or_none('ws://foo.de'), 'ws://foo.de')
        self.assertEqual(url_or_none('wss://foo.de'), 'wss://foo.de')
    def test_parse_age_limit(self):
        self.assertEqual(parse_age_limit(None), None)
        self.assertEqual(parse_age_limit(False), None)
        self.assertEqual(parse_age_limit('invalid'), None)
        self.assertEqual(parse_age_limit(0), 0)
        self.assertEqual(parse_age_limit(18), 18)
        self.assertEqual(parse_age_limit(21), 21)
        self.assertEqual(parse_age_limit(22), None)
        self.assertEqual(parse_age_limit('18'), 18)
        self.assertEqual(parse_age_limit('18+'), 18)
        self.assertEqual(parse_age_limit('PG-13'), 13)
        self.assertEqual(parse_age_limit('TV-14'), 14)
        self.assertEqual(parse_age_limit('TV-MA'), 17)
        self.assertEqual(parse_age_limit('TV14'), 14)
        self.assertEqual(parse_age_limit('TV_G'), 0)
    def test_parse_duration(self):
        self.assertEqual(parse_duration(None), None)
        self.assertEqual(parse_duration(False), None)
        self.assertEqual(parse_duration('invalid'), None)
        self.assertEqual(parse_duration('1'), 1)
        self.assertEqual(parse_duration('1337:12'), 80232)
        self.assertEqual(parse_duration('9:12:43'), 33163)
        self.assertEqual(parse_duration('12:00'), 720)
        self.assertEqual(parse_duration('00:01:01'), 61)
        self.assertEqual(parse_duration('x:y'), None)
        self.assertEqual(parse_duration('3h11m53s'), 11513)
        self.assertEqual(parse_duration('3h 11m 53s'), 11513)
        self.assertEqual(parse_duration('3 hours 11 minutes 53 seconds'), 11513)
        self.assertEqual(parse_duration('3 hours 11 mins 53 secs'), 11513)
        self.assertEqual(parse_duration('3 hours, 11 minutes, 53 seconds'), 11513)
        self.assertEqual(parse_duration('3 hours, 11 mins, 53 secs'), 11513)
        self.assertEqual(parse_duration('62m45s'), 3765)
        self.assertEqual(parse_duration('6m59s'), 419)
        self.assertEqual(parse_duration('49s'), 49)
        self.assertEqual(parse_duration('0h0m0s'), 0)
        self.assertEqual(parse_duration('0m0s'), 0)
        self.assertEqual(parse_duration('0s'), 0)
        self.assertEqual(parse_duration('01:02:03.05'), 3723.05)
        self.assertEqual(parse_duration('T30M38S'), 1838)
        self.assertEqual(parse_duration('5 s'), 5)
        self.assertEqual(parse_duration('3 min'), 180)
        self.assertEqual(parse_duration('2.5 hours'), 9000)
        self.assertEqual(parse_duration('02:03:04'), 7384)
        self.assertEqual(parse_duration('01:02:03:04'), 93784)
        self.assertEqual(parse_duration('1 hour 3 minutes'), 3780)
        self.assertEqual(parse_duration('87 Min.'), 5220)
        self.assertEqual(parse_duration('PT1H0.040S'), 3600.04)
        self.assertEqual(parse_duration('PT00H03M30SZ'), 210)
        self.assertEqual(parse_duration('P0Y0M0DT0H4M20.880S'), 260.88)
        self.assertEqual(parse_duration('01:02:03:050'), 3723.05)
        self.assertEqual(parse_duration('103:050'), 103.05)
        self.assertEqual(parse_duration('1HR 3MIN'), 3780)
        self.assertEqual(parse_duration('2hrs 3mins'), 7380)
    def test_fix_xml_ampersands(self):
            fix_xml_ampersands('"&x=y&z=a'), '"&amp;x=y&amp;z=a')
            fix_xml_ampersands('"&amp;x=y&wrong;&z=a'),
            '"&amp;x=y&amp;wrong;&amp;z=a')
            fix_xml_ampersands('&amp;&apos;&gt;&lt;&quot;'),
            '&amp;&apos;&gt;&lt;&quot;')
            fix_xml_ampersands('&#1234;&#x1abC;'), '&#1234;&#x1abC;')
        self.assertEqual(fix_xml_ampersands('&#&#'), '&amp;#&amp;#')
    def test_paged_list(self):
        def testPL(size, pagesize, sliceargs, expected):
            def get_page(pagenum):
                firstid = pagenum * pagesize
                upto = min(size, pagenum * pagesize + pagesize)
                yield from range(firstid, upto)
            pl = OnDemandPagedList(get_page, pagesize)
            got = pl.getslice(*sliceargs)
            self.assertEqual(got, expected)
            iapl = InAdvancePagedList(get_page, size // pagesize + 1, pagesize)
            got = iapl.getslice(*sliceargs)
        testPL(5, 2, (), [0, 1, 2, 3, 4])
        testPL(5, 2, (1,), [1, 2, 3, 4])
        testPL(5, 2, (2,), [2, 3, 4])
        testPL(5, 2, (4,), [4])
        testPL(5, 2, (0, 3), [0, 1, 2])
        testPL(5, 2, (1, 4), [1, 2, 3])
        testPL(5, 2, (2, 99), [2, 3, 4])
        testPL(5, 2, (20, 99), [])
    def test_read_batch_urls(self):
        f = io.StringIO('''\xef\xbb\xbf foo
            bar\r
            baz
            # More after this line\r
            ; or after this
            bam''')
        self.assertEqual(read_batch_urls(f), ['foo', 'bar', 'baz', 'bam'])
    def test_urlencode_postdata(self):
        data = urlencode_postdata({'username': 'foo@bar.com', 'password': '1234'})
        self.assertTrue(isinstance(data, bytes))
    def test_update_url_query(self):
        self.assertEqual(parse_qs(update_url_query(
            'http://example.com/path', {'quality': ['HD'], 'format': ['mp4']})),
            parse_qs('http://example.com/path?quality=HD&format=mp4'))
            'http://example.com/path', {'system': ['LINUX', 'WINDOWS']})),
            parse_qs('http://example.com/path?system=LINUX&system=WINDOWS'))
            'http://example.com/path', {'fields': 'id,formats,subtitles'})),
            parse_qs('http://example.com/path?fields=id,formats,subtitles'))
            'http://example.com/path', {'fields': ('id,formats,subtitles', 'thumbnails')})),
            parse_qs('http://example.com/path?fields=id,formats,subtitles&fields=thumbnails'))
            'http://example.com/path?manifest=f4m', {'manifest': []})),
            parse_qs('http://example.com/path'))
            'http://example.com/path?system=LINUX&system=WINDOWS', {'system': 'LINUX'})),
            parse_qs('http://example.com/path?system=LINUX'))
            'http://example.com/path', {'fields': b'id,formats,subtitles'})),
            'http://example.com/path', {'width': 1080, 'height': 720})),
            parse_qs('http://example.com/path?width=1080&height=720'))
            'http://example.com/path', {'bitrate': 5020.43})),
            parse_qs('http://example.com/path?bitrate=5020.43'))
            'http://example.com/path', {'test': '第二行тест'})),
            parse_qs('http://example.com/path?test=%E7%AC%AC%E4%BA%8C%E8%A1%8C%D1%82%D0%B5%D1%81%D1%82'))
    def test_multipart_encode(self):
            multipart_encode({b'field': b'value'}, boundary='AAAAAA')[0],
            b'--AAAAAA\r\nContent-Disposition: form-data; name="field"\r\n\r\nvalue\r\n--AAAAAA--\r\n')
            multipart_encode({'欄位'.encode(): '值'.encode()}, boundary='AAAAAA')[0],
            b'--AAAAAA\r\nContent-Disposition: form-data; name="\xe6\xac\x84\xe4\xbd\x8d"\r\n\r\n\xe5\x80\xbc\r\n--AAAAAA--\r\n')
            ValueError, multipart_encode, {b'field': b'value'}, boundary='value')
    def test_merge_dicts(self):
        self.assertEqual(merge_dicts({'a': 1}, {'b': 2}), {'a': 1, 'b': 2})
        self.assertEqual(merge_dicts({'a': 1}, {'a': 2}), {'a': 1})
        self.assertEqual(merge_dicts({'a': 1}, {'a': None}), {'a': 1})
        self.assertEqual(merge_dicts({'a': 1}, {'a': ''}), {'a': 1})
        self.assertEqual(merge_dicts({'a': 1}, {}), {'a': 1})
        self.assertEqual(merge_dicts({'a': None}, {'a': 1}), {'a': 1})
        self.assertEqual(merge_dicts({'a': ''}, {'a': 1}), {'a': ''})
        self.assertEqual(merge_dicts({'a': ''}, {'a': 'abc'}), {'a': 'abc'})
        self.assertEqual(merge_dicts({'a': None}, {'a': ''}, {'a': 'abc'}), {'a': 'abc'})
    def test_encode_compat_str(self):
        self.assertEqual(encode_compat_str(b'\xd1\x82\xd0\xb5\xd1\x81\xd1\x82', 'utf-8'), 'тест')
        self.assertEqual(encode_compat_str('тест', 'utf-8'), 'тест')
    def test_parse_iso8601(self):
        self.assertEqual(parse_iso8601('2014-03-23T23:04:26+0100'), 1395612266)
        self.assertEqual(parse_iso8601('2014-03-23T23:04:26-07:00'), 1395641066)
        self.assertEqual(parse_iso8601('2014-03-23T23:04:26', timezone=dt.timedelta(hours=-7)), 1395641066)
        self.assertEqual(parse_iso8601('2014-03-23T23:04:26', timezone=NO_DEFAULT), None)
        # default does not override timezone in date_str
        self.assertEqual(parse_iso8601('2014-03-23T23:04:26-07:00', timezone=dt.timedelta(hours=-10)), 1395641066)
        self.assertEqual(parse_iso8601('2014-03-23T22:04:26+0000'), 1395612266)
        self.assertEqual(parse_iso8601('2014-03-23T22:04:26Z'), 1395612266)
        self.assertEqual(parse_iso8601('2014-03-23T22:04:26.1234Z'), 1395612266)
        self.assertEqual(parse_iso8601('2015-09-29T08:27:31.727'), 1443515251)
        self.assertEqual(parse_iso8601('2015-09-29T08-27-31.727'), None)
    def test_strip_jsonp(self):
        stripped = strip_jsonp('cb ([ {"id":"532cb",\n\n\n"x":\n3}\n]\n);')
        d = json.loads(stripped)
        self.assertEqual(d, [{'id': '532cb', 'x': 3}])
        stripped = strip_jsonp('parseMetadata({"STATUS":"OK"})\n\n\n//epc')
        self.assertEqual(d, {'STATUS': 'OK'})
        stripped = strip_jsonp('ps.embedHandler({"status": "success"});')
        self.assertEqual(d, {'status': 'success'})
        stripped = strip_jsonp('window.cb && window.cb({"status": "success"});')
        stripped = strip_jsonp('window.cb && cb({"status": "success"});')
        stripped = strip_jsonp('({"status": "success"});')
    def test_strip_or_none(self):
        self.assertEqual(strip_or_none(' abc'), 'abc')
        self.assertEqual(strip_or_none('abc '), 'abc')
        self.assertEqual(strip_or_none(' abc '), 'abc')
        self.assertEqual(strip_or_none('\tabc\t'), 'abc')
        self.assertEqual(strip_or_none('\n\tabc\n\t'), 'abc')
        self.assertEqual(strip_or_none('abc'), 'abc')
        self.assertEqual(strip_or_none(''), '')
        self.assertEqual(strip_or_none(None), None)
        self.assertEqual(strip_or_none(42), None)
        self.assertEqual(strip_or_none([]), None)
    def test_uppercase_escape(self):
        self.assertEqual(uppercase_escape('aä'), 'aä')
        self.assertEqual(uppercase_escape('\\U0001d550'), '𝕐')
    def test_lowercase_escape(self):
        self.assertEqual(lowercase_escape('aä'), 'aä')
        self.assertEqual(lowercase_escape('\\u0026'), '&')
    def test_limit_length(self):
        self.assertEqual(limit_length(None, 12), None)
        self.assertEqual(limit_length('foo', 12), 'foo')
            limit_length('foo bar baz asd', 12).startswith('foo bar'))
        self.assertTrue('...' in limit_length('foo bar baz asd', 12))
    def test_mimetype2ext(self):
        self.assertEqual(mimetype2ext(None), None)
        self.assertEqual(mimetype2ext('video/x-flv'), 'flv')
        self.assertEqual(mimetype2ext('application/x-mpegURL'), 'm3u8')
        self.assertEqual(mimetype2ext('text/vtt'), 'vtt')
        self.assertEqual(mimetype2ext('text/vtt;charset=utf-8'), 'vtt')
        self.assertEqual(mimetype2ext('text/html; charset=utf-8'), 'html')
        self.assertEqual(mimetype2ext('audio/x-wav'), 'wav')
        self.assertEqual(mimetype2ext('audio/x-wav;codec=pcm'), 'wav')
    def test_month_by_name(self):
        self.assertEqual(month_by_name(None), None)
        self.assertEqual(month_by_name('December', 'en'), 12)
        self.assertEqual(month_by_name('décembre', 'fr'), 12)
        self.assertEqual(month_by_name('desember', 'is'), 12)
        self.assertEqual(month_by_name('December'), 12)
        self.assertEqual(month_by_name('décembre'), None)
        self.assertEqual(month_by_name('Unknown', 'unknown'), None)
    def test_parse_codecs(self):
        self.assertEqual(parse_codecs(''), {})
        self.assertEqual(parse_codecs('avc1.77.30, mp4a.40.2'), {
            'vcodec': 'avc1.77.30',
            'acodec': 'mp4a.40.2',
            'dynamic_range': None,
        self.assertEqual(parse_codecs('mp4a.40.2'), {
            'vcodec': 'none',
        self.assertEqual(parse_codecs('mp4a.40.5,avc1.42001e'), {
            'vcodec': 'avc1.42001e',
            'acodec': 'mp4a.40.5',
        self.assertEqual(parse_codecs('avc3.640028'), {
            'vcodec': 'avc3.640028',
            'acodec': 'none',
        self.assertEqual(parse_codecs(', h264,,newcodec,aac'), {
            'vcodec': 'h264',
            'acodec': 'aac',
        self.assertEqual(parse_codecs('av01.0.05M.08'), {
            'vcodec': 'av01.0.05M.08',
        self.assertEqual(parse_codecs('vp9.2'), {
            'vcodec': 'vp9.2',
            'dynamic_range': 'HDR10',
        self.assertEqual(parse_codecs('vp09.02.50.10.01.09.18.09.00'), {
            'vcodec': 'vp09.02.50.10.01.09.18.09.00',
        self.assertEqual(parse_codecs('av01.0.12M.10.0.110.09.16.09.0'), {
            'vcodec': 'av01.0.12M.10.0.110.09.16.09.0',
        self.assertEqual(parse_codecs('dvhe'), {
            'vcodec': 'dvhe',
            'dynamic_range': 'DV',
        self.assertEqual(parse_codecs('fLaC'), {
            'acodec': 'flac',
        self.assertEqual(parse_codecs('theora, vorbis'), {
            'vcodec': 'theora',
            'acodec': 'vorbis',
        self.assertEqual(parse_codecs('unknownvcodec, unknownacodec'), {
            'vcodec': 'unknownvcodec',
            'acodec': 'unknownacodec',
        self.assertEqual(parse_codecs('unknown'), {})
    def test_escape_rfc3986(self):
        reserved = "!*'();:@&=+$,/?#[]"
        unreserved = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~'
        self.assertEqual(escape_rfc3986(reserved), reserved)
        self.assertEqual(escape_rfc3986(unreserved), unreserved)
        self.assertEqual(escape_rfc3986('тест'), '%D1%82%D0%B5%D1%81%D1%82')
        self.assertEqual(escape_rfc3986('%D1%82%D0%B5%D1%81%D1%82'), '%D1%82%D0%B5%D1%81%D1%82')
        self.assertEqual(escape_rfc3986('foo bar'), 'foo%20bar')
        self.assertEqual(escape_rfc3986('foo%20bar'), 'foo%20bar')
    def test_normalize_url(self):
            normalize_url('http://wowza.imust.org/srv/vod/telemb/new/UPLOAD/UPLOAD/20224_IncendieHavré_FD.mp4'),
            'http://wowza.imust.org/srv/vod/telemb/new/UPLOAD/UPLOAD/20224_IncendieHavre%CC%81_FD.mp4',
            normalize_url('http://www.ardmediathek.de/tv/Sturm-der-Liebe/Folge-2036-Zu-Mann-und-Frau-erklärt/Das-Erste/Video?documentId=22673108&bcastId=5290'),
            'http://www.ardmediathek.de/tv/Sturm-der-Liebe/Folge-2036-Zu-Mann-und-Frau-erkl%C3%A4rt/Das-Erste/Video?documentId=22673108&bcastId=5290',
            normalize_url('http://тест.рф/фрагмент'),
            'http://xn--e1aybc.xn--p1ai/%D1%84%D1%80%D0%B0%D0%B3%D0%BC%D0%B5%D0%BD%D1%82',
            normalize_url('http://тест.рф/абв?абв=абв#абв'),
            'http://xn--e1aybc.xn--p1ai/%D0%B0%D0%B1%D0%B2?%D0%B0%D0%B1%D0%B2=%D0%B0%D0%B1%D0%B2#%D0%B0%D0%B1%D0%B2',
        self.assertEqual(normalize_url('http://vimeo.com/56015672#at=0'), 'http://vimeo.com/56015672#at=0')
        self.assertEqual(normalize_url('http://www.example.com/../a/b/../c/./d.html'), 'http://www.example.com/a/c/d.html')
    def test_remove_dot_segments(self):
        self.assertEqual(remove_dot_segments('/a/b/c/./../../g'), '/a/g')
        self.assertEqual(remove_dot_segments('mid/content=5/../6'), 'mid/6')
        self.assertEqual(remove_dot_segments('/ad/../cd'), '/cd')
        self.assertEqual(remove_dot_segments('/ad/../cd/'), '/cd/')
        self.assertEqual(remove_dot_segments('/..'), '/')
        self.assertEqual(remove_dot_segments('/./'), '/')
        self.assertEqual(remove_dot_segments('/./a'), '/a')
        self.assertEqual(remove_dot_segments('/abc/./.././d/././e/.././f/./../../ghi'), '/ghi')
        self.assertEqual(remove_dot_segments('/'), '/')
        self.assertEqual(remove_dot_segments('/t'), '/t')
        self.assertEqual(remove_dot_segments('t'), 't')
        self.assertEqual(remove_dot_segments(''), '')
        self.assertEqual(remove_dot_segments('/../a/b/c'), '/a/b/c')
        self.assertEqual(remove_dot_segments('../a'), 'a')
        self.assertEqual(remove_dot_segments('./a'), 'a')
        self.assertEqual(remove_dot_segments('.'), '')
        self.assertEqual(remove_dot_segments('////'), '////')
    def test_js_to_json_vars_strings(self):
        self.assertDictEqual(
            json.loads(js_to_json(
                '''{
                    'null': a,
                    'nullStr': b,
                    'true': c,
                    'trueStr': d,
                    'false': e,
                    'falseStr': f,
                    'unresolvedVar': g,
                }''',
                    'a': 'null',
                    'b': '"null"',
                    'c': 'true',
                    'd': '"true"',
                    'e': 'false',
                    'f': '"false"',
                    'g': 'var',
            )),
                'null': None,
                'nullStr': 'null',
                'true': True,
                'trueStr': 'true',
                'false': False,
                'falseStr': 'false',
                'unresolvedVar': 'var',
                    'int': a,
                    'intStr': b,
                    'float': c,
                    'floatStr': d,
                    'a': '123',
                    'b': '"123"',
                    'c': '1.23',
                    'd': '"1.23"',
                'int': 123,
                'intStr': '123',
                'float': 1.23,
                'floatStr': '1.23',
                    'object': a,
                    'objectStr': b,
                    'array': c,
                    'arrayStr': d,
                    'a': '{}',
                    'b': '"{}"',
                    'c': '[]',
                    'd': '"[]"',
                'object': {},
                'objectStr': '{}',
                'array': [],
                'arrayStr': '[]',
    def test_js_to_json_realworld(self):
        inp = '''{
            'clip':{'provider':'pseudo'}
        }'''
        self.assertEqual(js_to_json(inp), '''{
            "clip":{"provider":"pseudo"}
        }''')
        json.loads(js_to_json(inp))
            'playlist':[{'controls':{'all':null}}]
            "playlist":[{"controls":{"all":null}}]
        inp = '''"The CW\\'s \\'Crazy Ex-Girlfriend\\'"'''
        self.assertEqual(js_to_json(inp), '''"The CW's 'Crazy Ex-Girlfriend'"''')
        inp = '"SAND Number: SAND 2013-7800P\\nPresenter: Tom Russo\\nHabanero Software Training - Xyce Software\\nXyce, Sandia\\u0027s"'
        json_code = js_to_json(inp)
        self.assertEqual(json.loads(json_code), json.loads(inp))
            0:{src:'skipped', type: 'application/dash+xml'},
            1:{src:'skipped', type: 'application/vnd.apple.mpegURL'},
            "0":{"src":"skipped", "type": "application/dash+xml"},
            "1":{"src":"skipped", "type": "application/vnd.apple.mpegURL"}
        inp = '''{"foo":101}'''
        self.assertEqual(js_to_json(inp), '''{"foo":101}''')
        inp = '''{"duration": "00:01:07"}'''
        self.assertEqual(js_to_json(inp), '''{"duration": "00:01:07"}''')
        inp = '''{segments: [{"offset":-3.885780586188048e-16,"duration":39.75000000000001}]}'''
        self.assertEqual(js_to_json(inp), '''{"segments": [{"offset":-3.885780586188048e-16,"duration":39.75000000000001}]}''')
    def test_js_to_json_edgecases(self):
        on = js_to_json("{abc_def:'1\\'\\\\2\\\\\\'3\"4'}")
        self.assertEqual(json.loads(on), {'abc_def': "1'\\2\\'3\"4"})
        on = js_to_json('{"abc": true}')
        self.assertEqual(json.loads(on), {'abc': True})
        # Ignore JavaScript code as well
        on = js_to_json('''{
            "x": 1,
            y: "a",
            z: some.code
        d = json.loads(on)
        self.assertEqual(d['x'], 1)
        self.assertEqual(d['y'], 'a')
        # Just drop ! prefix for now though this results in a wrong value
            a: !0,
            b: !1,
            c: !!0,
            d: !!42.42,
            e: !!![],
            f: !"abc",
            g: !"",
            !42: 42
        self.assertEqual(json.loads(on), {
            'a': 0,
            'b': 1,
            'c': 0,
            'd': 42.42,
            'e': [],
            'f': 'abc',
            'g': '',
            '42': 42,
        on = js_to_json('["abc", "def",]')
        self.assertEqual(json.loads(on), ['abc', 'def'])
        on = js_to_json('[/*comment\n*/"abc"/*comment\n*/,/*comment\n*/"def",/*comment\n*/]')
        on = js_to_json('[//comment\n"abc" //comment\n,//comment\n"def",//comment\n]')
        on = js_to_json('{"abc": "def",}')
        self.assertEqual(json.loads(on), {'abc': 'def'})
        on = js_to_json('{/*comment\n*/"abc"/*comment\n*/:/*comment\n*/"def"/*comment\n*/,/*comment\n*/}')
        on = js_to_json('{ 0: /* " \n */ ",]" , }')
        self.assertEqual(json.loads(on), {'0': ',]'})
        on = js_to_json('{ /*comment\n*/0/*comment\n*/: /* " \n */ ",]" , }')
        on = js_to_json('{ 0: // comment\n1 }')
        self.assertEqual(json.loads(on), {'0': 1})
        on = js_to_json(r'["<p>x<\/p>"]')
        self.assertEqual(json.loads(on), ['<p>x</p>'])
        on = js_to_json(r'["\xaa"]')
        self.assertEqual(json.loads(on), ['\u00aa'])
        on = js_to_json("['a\\\nb']")
        self.assertEqual(json.loads(on), ['ab'])
        on = js_to_json("/*comment\n*/[/*comment\n*/'a\\\nb'/*comment\n*/]/*comment\n*/")
        on = js_to_json('{0xff:0xff}')
        self.assertEqual(json.loads(on), {'255': 255})
        on = js_to_json('{/*comment\n*/0xff/*comment\n*/:/*comment\n*/0xff/*comment\n*/}')
        on = js_to_json('{077:077}')
        self.assertEqual(json.loads(on), {'63': 63})
        on = js_to_json('{/*comment\n*/077/*comment\n*/:/*comment\n*/077/*comment\n*/}')
        on = js_to_json('{42:42}')
        self.assertEqual(json.loads(on), {'42': 42})
        on = js_to_json('{/*comment\n*/42/*comment\n*/:/*comment\n*/42/*comment\n*/}')
        on = js_to_json('{42:4.2e1}')
        self.assertEqual(json.loads(on), {'42': 42.0})
        on = js_to_json('{ "0x40": "0x40" }')
        self.assertEqual(json.loads(on), {'0x40': '0x40'})
        on = js_to_json('{ "040": "040" }')
        self.assertEqual(json.loads(on), {'040': '040'})
        on = js_to_json('[1,//{},\n2]')
        self.assertEqual(json.loads(on), [1, 2])
        on = js_to_json(R'"\^\$\#"')
        self.assertEqual(json.loads(on), R'^$#', msg='Unnecessary escapes should be stripped')
        on = js_to_json('\'"\\""\'')
        self.assertEqual(json.loads(on), '"""', msg='Unnecessary quote escape should be escaped')
        on = js_to_json('[new Date("spam"), \'("eggs")\']')
        self.assertEqual(json.loads(on), ['spam', '("eggs")'], msg='Date regex should match a single string')
        on = js_to_json('[0.077, 7.06, 29.064, 169.0072]')
        self.assertEqual(json.loads(on), [0.077, 7.06, 29.064, 169.0072])
    def test_js_to_json_malformed(self):
        self.assertEqual(js_to_json('42a1'), '42"a1"')
        self.assertEqual(js_to_json('42a-1'), '42"a"-1')
        self.assertEqual(js_to_json('{a: `${e("")}`}'), '{"a": "\\"e\\"(\\"\\")"}')
    def test_js_to_json_template_literal(self):
        self.assertEqual(js_to_json('`Hello ${name}`', {'name': '"world"'}), '"Hello world"')
        self.assertEqual(js_to_json('`${name}${name}`', {'name': '"X"'}), '"XX"')
        self.assertEqual(js_to_json('`${name}${name}`', {'name': '5'}), '"55"')
        self.assertEqual(js_to_json('`${name}"${name}"`', {'name': '5'}), '"5\\"5\\""')
        self.assertEqual(js_to_json('`${name}`', {}), '"name"')
    def test_js_to_json_common_constructors(self):
        self.assertEqual(json.loads(js_to_json('new Map([["a", 5]])')), {'a': 5})
        self.assertEqual(json.loads(js_to_json('Array(5, 10)')), [5, 10])
        self.assertEqual(json.loads(js_to_json('new Array(15,5)')), [15, 5])
        self.assertEqual(json.loads(js_to_json('new Map([Array(5, 10),new Array(15,5)])')), {'5': 10, '15': 5})
        self.assertEqual(json.loads(js_to_json('new Date("123")')), '123')
        self.assertEqual(json.loads(js_to_json('new Date(\'2023-10-19\')')), '2023-10-19')
    def test_extract_attributes(self):
        self.assertEqual(extract_attributes('<e x="y">'), {'x': 'y'})
        self.assertEqual(extract_attributes("<e x='y'>"), {'x': 'y'})
        self.assertEqual(extract_attributes('<e x=y>'), {'x': 'y'})
        self.assertEqual(extract_attributes('<e x="a \'b\' c">'), {'x': "a 'b' c"})
        self.assertEqual(extract_attributes('<e x=\'a "b" c\'>'), {'x': 'a "b" c'})
        self.assertEqual(extract_attributes('<e x="&#121;">'), {'x': 'y'})
        self.assertEqual(extract_attributes('<e x="&#x79;">'), {'x': 'y'})
        self.assertEqual(extract_attributes('<e x="&amp;">'), {'x': '&'})  # XML
        self.assertEqual(extract_attributes('<e x="&quot;">'), {'x': '"'})
        self.assertEqual(extract_attributes('<e x="&pound;">'), {'x': '£'})  # HTML 3.2
        self.assertEqual(extract_attributes('<e x="&lambda;">'), {'x': 'λ'})  # HTML 4.0
        self.assertEqual(extract_attributes('<e x="&foo">'), {'x': '&foo'})
        self.assertEqual(extract_attributes('<e x="\'">'), {'x': "'"})
        self.assertEqual(extract_attributes('<e x=\'"\'>'), {'x': '"'})
        self.assertEqual(extract_attributes('<e x >'), {'x': None})
        self.assertEqual(extract_attributes('<e x=y a>'), {'x': 'y', 'a': None})
        self.assertEqual(extract_attributes('<e x= y>'), {'x': 'y'})
        self.assertEqual(extract_attributes('<e x=1 y=2 x=3>'), {'y': '2', 'x': '3'})
        self.assertEqual(extract_attributes('<e \nx=\ny\n>'), {'x': 'y'})
        self.assertEqual(extract_attributes('<e \nx=\n"y"\n>'), {'x': 'y'})
        self.assertEqual(extract_attributes("<e \nx=\n'y'\n>"), {'x': 'y'})
        self.assertEqual(extract_attributes('<e \nx="\ny\n">'), {'x': '\ny\n'})
        self.assertEqual(extract_attributes('<e CAPS=x>'), {'caps': 'x'})  # Names lowercased
        self.assertEqual(extract_attributes('<e x=1 X=2>'), {'x': '2'})
        self.assertEqual(extract_attributes('<e X=1 x=2>'), {'x': '2'})
        self.assertEqual(extract_attributes('<e _:funny-name1=1>'), {'_:funny-name1': '1'})
        self.assertEqual(extract_attributes('<e x="Fáilte 世界 \U0001f600">'), {'x': 'Fáilte 世界 \U0001f600'})
        self.assertEqual(extract_attributes('<e x="décompose&#769;">'), {'x': 'décompose\u0301'})
        # "Narrow" Python builds don't support unicode code points outside BMP.
            chr(0x10000)
            supports_outside_bmp = True
            supports_outside_bmp = False
        if supports_outside_bmp:
            self.assertEqual(extract_attributes('<e x="Smile &#128512;!">'), {'x': 'Smile \U0001f600!'})
        # Malformed HTML should not break attributes extraction on older Python
        self.assertEqual(extract_attributes('<mal"formed/>'), {})
    def test_clean_html(self):
        self.assertEqual(clean_html('a:\nb'), 'a: b')
        self.assertEqual(clean_html('a:\n   "b"'), 'a: "b"')
        self.assertEqual(clean_html('a<br>\xa0b'), 'a\nb')
    def test_args_to_str(self):
            args_to_str(['foo', 'ba/r', '-baz', '2 be', '']),
            'foo ba/r -baz \'2 be\' \'\'' if os.name != 'nt' else 'foo ba/r -baz "2 be" ""',
    def test_parse_filesize(self):
        self.assertEqual(parse_filesize(None), None)
        self.assertEqual(parse_filesize(''), None)
        self.assertEqual(parse_filesize('91 B'), 91)
        self.assertEqual(parse_filesize('foobar'), None)
        self.assertEqual(parse_filesize('2 MiB'), 2097152)
        self.assertEqual(parse_filesize('5 GB'), 5000000000)
        self.assertEqual(parse_filesize('1.2Tb'), 1200000000000)
        self.assertEqual(parse_filesize('1.2tb'), 1200000000000)
        self.assertEqual(parse_filesize('1,24 KB'), 1240)
        self.assertEqual(parse_filesize('1,24 kb'), 1240)
        self.assertEqual(parse_filesize('8.5 megabytes'), 8500000)
    def test_parse_count(self):
        self.assertEqual(parse_count(None), None)
        self.assertEqual(parse_count(''), None)
        self.assertEqual(parse_count('0'), 0)
        self.assertEqual(parse_count('1000'), 1000)
        self.assertEqual(parse_count('1.000'), 1000)
        self.assertEqual(parse_count('1.1k'), 1100)
        self.assertEqual(parse_count('1.1 k'), 1100)
        self.assertEqual(parse_count('1,1 k'), 1100)
        self.assertEqual(parse_count('1.1kk'), 1100000)
        self.assertEqual(parse_count('1.1kk '), 1100000)
        self.assertEqual(parse_count('1,1kk'), 1100000)
        self.assertEqual(parse_count('100 views'), 100)
        self.assertEqual(parse_count('1,100 views'), 1100)
        self.assertEqual(parse_count('1.1kk views'), 1100000)
        self.assertEqual(parse_count('10M views'), 10000000)
        self.assertEqual(parse_count('has 10M views'), 10000000)
    def test_parse_resolution(self):
        self.assertEqual(parse_resolution(None), {})
        self.assertEqual(parse_resolution(''), {})
        self.assertEqual(parse_resolution(' 1920x1080'), {'width': 1920, 'height': 1080})
        self.assertEqual(parse_resolution('1920×1080 '), {'width': 1920, 'height': 1080})
        self.assertEqual(parse_resolution('1920 x 1080'), {'width': 1920, 'height': 1080})
        self.assertEqual(parse_resolution('720p'), {'height': 720})
        self.assertEqual(parse_resolution('4k'), {'height': 2160})
        self.assertEqual(parse_resolution('8K'), {'height': 4320})
        self.assertEqual(parse_resolution('pre_1920x1080_post'), {'width': 1920, 'height': 1080})
        self.assertEqual(parse_resolution('ep1x2'), {})
        self.assertEqual(parse_resolution('1920, 1080'), {'width': 1920, 'height': 1080})
        self.assertEqual(parse_resolution('1920w', lenient=True), {'width': 1920})
    def test_parse_bitrate(self):
        self.assertEqual(parse_bitrate(None), None)
        self.assertEqual(parse_bitrate(''), None)
        self.assertEqual(parse_bitrate('300kbps'), 300)
        self.assertEqual(parse_bitrate('1500kbps'), 1500)
        self.assertEqual(parse_bitrate('300 kbps'), 300)
    def test_version_tuple(self):
        self.assertEqual(version_tuple('1'), (1,))
        self.assertEqual(version_tuple('10.23.344'), (10, 23, 344))
        self.assertEqual(version_tuple('10.1-6'), (10, 1, 6))  # avconv style
        self.assertEqual(version_tuple('invalid', lenient=True), (-1,))
        self.assertEqual(version_tuple('1.2.3', lenient=True), (1, 2, 3))
        self.assertEqual(version_tuple('12.34-something', lenient=True), (12, 34, -1))
    def test_detect_exe_version(self):
        self.assertEqual(detect_exe_version('''ffmpeg version 1.2.1
built on May 27 2013 08:37:26 with gcc 4.7 (Debian 4.7.3-4)
configuration: --prefix=/usr --extra-'''), '1.2.1')
        self.assertEqual(detect_exe_version('''ffmpeg version N-63176-g1fb4685
built on May 15 2014 22:09:06 with gcc 4.8.2 (GCC)'''), 'N-63176-g1fb4685')
        self.assertEqual(detect_exe_version('''X server found. dri2 connection failed!
Trying to open render node...
Success at /dev/dri/renderD128.
ffmpeg version 2.4.4 Copyright (c) 2000-2014 the FFmpeg ...'''), '2.4.4')
    def test_age_restricted(self):
        self.assertFalse(age_restricted(None, 10))  # unrestricted content
        self.assertFalse(age_restricted(1, None))  # unrestricted policy
        self.assertFalse(age_restricted(8, 10))
        self.assertTrue(age_restricted(18, 14))
        self.assertFalse(age_restricted(18, 18))
    def test_is_html(self):
        self.assertFalse(is_html(b'\x49\x44\x43<html'))
        self.assertTrue(is_html(b'<!DOCTYPE foo>\xaaa'))
        self.assertTrue(is_html(  # UTF-8 with BOM
            b'\xef\xbb\xbf<!DOCTYPE foo>\xaaa'))
        self.assertTrue(is_html(  # UTF-16-LE
            b'\xff\xfe<\x00h\x00t\x00m\x00l\x00>\x00\xe4\x00',
        self.assertTrue(is_html(  # UTF-16-BE
            b'\xfe\xff\x00<\x00h\x00t\x00m\x00l\x00>\x00\xe4',
        self.assertTrue(is_html(  # UTF-32-BE
            b'\x00\x00\xFE\xFF\x00\x00\x00<\x00\x00\x00h\x00\x00\x00t\x00\x00\x00m\x00\x00\x00l\x00\x00\x00>\x00\x00\x00\xe4'))
        self.assertTrue(is_html(  # UTF-32-LE
            b'\xFF\xFE\x00\x00<\x00\x00\x00h\x00\x00\x00t\x00\x00\x00m\x00\x00\x00l\x00\x00\x00>\x00\x00\x00\xe4\x00\x00\x00'))
    def test_render_table(self):
            render_table(
                ['a', 'empty', 'bcd'],
                [[123, '', 4], [9999, '', 51]]),
            'a    empty bcd\n'
            '123        4\n'
            '9999       51')
                [[123, '', 4], [9999, '', 51]],
                hide_empty=True),
            'a    bcd\n'
            '123  4\n'
            '9999 51')
                ['\ta', 'bcd'],
                [['1\t23', 4], ['\t9999', 51]]),
            '   a bcd\n'
            '1 23 4\n'
                ['a', 'bcd'],
                [[123, 4], [9999, 51]],
                delim='-'),
            '--------\n'
                delim='-', extra_gap=2),
            'a      bcd\n'
            '----------\n'
            '123    4\n'
            '9999   51')
    def test_match_str(self):
        # Unary
        self.assertFalse(match_str('xy', {'x': 1200}))
        self.assertTrue(match_str('!xy', {'x': 1200}))
        self.assertTrue(match_str('x', {'x': 1200}))
        self.assertFalse(match_str('!x', {'x': 1200}))
        self.assertTrue(match_str('x', {'x': 0}))
        self.assertTrue(match_str('is_live', {'is_live': True}))
        self.assertFalse(match_str('is_live', {'is_live': False}))
        self.assertFalse(match_str('is_live', {'is_live': None}))
        self.assertFalse(match_str('is_live', {}))
        self.assertFalse(match_str('!is_live', {'is_live': True}))
        self.assertTrue(match_str('!is_live', {'is_live': False}))
        self.assertTrue(match_str('!is_live', {'is_live': None}))
        self.assertTrue(match_str('!is_live', {}))
        self.assertTrue(match_str('title', {'title': 'abc'}))
        self.assertTrue(match_str('title', {'title': ''}))
        self.assertFalse(match_str('!title', {'title': 'abc'}))
        self.assertFalse(match_str('!title', {'title': ''}))
        # Numeric
        self.assertFalse(match_str('x>0', {'x': 0}))
        self.assertFalse(match_str('x>0', {}))
        self.assertTrue(match_str('x>?0', {}))
        self.assertTrue(match_str('x>1K', {'x': 1200}))
        self.assertFalse(match_str('x>2K', {'x': 1200}))
        self.assertTrue(match_str('x>=1200 & x < 1300', {'x': 1200}))
        self.assertFalse(match_str('x>=1100 & x < 1200', {'x': 1200}))
        self.assertTrue(match_str('x > 1:0:0', {'x': 3700}))
        # String
        self.assertFalse(match_str('y=a212', {'y': 'foobar42'}))
        self.assertTrue(match_str('y=foobar42', {'y': 'foobar42'}))
        self.assertFalse(match_str('y!=foobar42', {'y': 'foobar42'}))
        self.assertTrue(match_str('y!=foobar2', {'y': 'foobar42'}))
        self.assertTrue(match_str('y^=foo', {'y': 'foobar42'}))
        self.assertFalse(match_str('y!^=foo', {'y': 'foobar42'}))
        self.assertFalse(match_str('y^=bar', {'y': 'foobar42'}))
        self.assertTrue(match_str('y!^=bar', {'y': 'foobar42'}))
        self.assertRaises(ValueError, match_str, 'x^=42', {'x': 42})
        self.assertTrue(match_str('y*=bar', {'y': 'foobar42'}))
        self.assertFalse(match_str('y!*=bar', {'y': 'foobar42'}))
        self.assertFalse(match_str('y*=baz', {'y': 'foobar42'}))
        self.assertTrue(match_str('y!*=baz', {'y': 'foobar42'}))
        self.assertTrue(match_str('y$=42', {'y': 'foobar42'}))
        self.assertFalse(match_str('y$=43', {'y': 'foobar42'}))
        # And
        self.assertFalse(match_str(
            'like_count > 100 & dislike_count <? 50 & description',
            {'like_count': 90, 'description': 'foo'}))
        self.assertTrue(match_str(
            {'like_count': 190, 'description': 'foo'}))
            {'like_count': 190, 'dislike_count': 60, 'description': 'foo'}))
            {'like_count': 190, 'dislike_count': 10}))
        # Regex
        self.assertTrue(match_str(r'x~=\bbar', {'x': 'foo bar'}))
        self.assertFalse(match_str(r'x~=\bbar.+', {'x': 'foo bar'}))
        self.assertFalse(match_str(r'x~=^FOO', {'x': 'foo bar'}))
        self.assertTrue(match_str(r'x~=(?i)^FOO', {'x': 'foo bar'}))
        # Quotes
        self.assertTrue(match_str(r'x^="foo"', {'x': 'foo "bar"'}))
        self.assertFalse(match_str(r'x^="foo  "', {'x': 'foo "bar"'}))
        self.assertFalse(match_str(r'x$="bar"', {'x': 'foo "bar"'}))
        self.assertTrue(match_str(r'x$=" \"bar\""', {'x': 'foo "bar"'}))
        # Escaping &
        self.assertFalse(match_str(r'x=foo & bar', {'x': 'foo & bar'}))
        self.assertTrue(match_str(r'x=foo \& bar', {'x': 'foo & bar'}))
        self.assertTrue(match_str(r'x=foo \& bar & x^=foo', {'x': 'foo & bar'}))
        self.assertTrue(match_str(r'x="foo \& bar" & x^=foo', {'x': 'foo & bar'}))
        # Example from docs
            r"!is_live & like_count>?100 & description~='(?i)\bcats \& dogs\b'",
            {'description': 'Raining Cats & Dogs'}))
        # Incomplete
        self.assertFalse(match_str('id!=foo', {'id': 'foo'}, True))
        self.assertTrue(match_str('x', {'id': 'foo'}, True))
        self.assertTrue(match_str('!x', {'id': 'foo'}, True))
        self.assertFalse(match_str('x', {'id': 'foo'}, False))
    def test_parse_dfxp_time_expr(self):
        self.assertEqual(parse_dfxp_time_expr(None), None)
        self.assertEqual(parse_dfxp_time_expr(''), None)
        self.assertEqual(parse_dfxp_time_expr('0.1'), 0.1)
        self.assertEqual(parse_dfxp_time_expr('0.1s'), 0.1)
        self.assertEqual(parse_dfxp_time_expr('00:00:01'), 1.0)
        self.assertEqual(parse_dfxp_time_expr('00:00:01.100'), 1.1)
        self.assertEqual(parse_dfxp_time_expr('00:00:01:100'), 1.1)
    def test_dfxp2srt(self):
        dfxp_data = '''<?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xml:lang="en" xmlns:tts="http://www.w3.org/ns/ttml#parameter">
                <div xml:lang="en">
                    <p begin="0" end="1">The following line contains Chinese characters and special symbols</p>
                    <p begin="1" end="2">第二行<br/>♪♪</p>
                    <p begin="2" dur="1"><span>Third<br/>Line</span></p>
                    <p begin="3" end="-1">Lines with invalid timestamps are ignored</p>
                    <p begin="-1" end="-1">Ignore, two</p>
                    <p begin="3" dur="-1">Ignored, three</p>
            </tt>'''.encode()
        srt_data = '''1
00:00:00,000 --> 00:00:01,000
The following line contains Chinese characters and special symbols
00:00:01,000 --> 00:00:02,000
第二行
♪♪
00:00:02,000 --> 00:00:03,000
Third
Line
        self.assertEqual(dfxp2srt(dfxp_data), srt_data)
        dfxp_data_no_default_namespace = b'''<?xml version="1.0" encoding="UTF-8"?>
            <tt xml:lang="en" xmlns:tts="http://www.w3.org/ns/ttml#parameter">
                    <p begin="0" end="1">The first line</p>
            </tt>'''
The first line
        self.assertEqual(dfxp2srt(dfxp_data_no_default_namespace), srt_data)
        dfxp_data_with_style = b'''<?xml version="1.0" encoding="utf-8"?>
<tt xmlns="http://www.w3.org/2006/10/ttaf1" xmlns:ttp="http://www.w3.org/2006/10/ttaf1#parameter" ttp:timeBase="media" xmlns:tts="http://www.w3.org/2006/10/ttaf1#style" xml:lang="en" xmlns:ttm="http://www.w3.org/2006/10/ttaf1#metadata">
    <styling>
      <style id="s2" style="s0" tts:color="cyan" tts:fontWeight="bold" />
      <style id="s1" style="s0" tts:color="yellow" tts:fontStyle="italic" />
      <style id="s3" style="s0" tts:color="lime" tts:textDecoration="underline" />
      <style id="s0" tts:backgroundColor="black" tts:fontStyle="normal" tts:fontSize="16" tts:fontFamily="sansSerif" tts:color="white" />
    </styling>
  <body tts:textAlign="center" style="s0">
      <p begin="00:00:02.08" id="p0" end="00:00:05.84">default style<span tts:color="red">custom style</span></p>
      <p style="s2" begin="00:00:02.08" id="p0" end="00:00:05.84"><span tts:color="lime">part 1<br /></span><span tts:color="cyan">part 2</span></p>
      <p style="s3" begin="00:00:05.84" id="p1" end="00:00:09.56">line 3<br />part 3</p>
      <p style="s1" tts:textDecoration="underline" begin="00:00:09.56" id="p2" end="00:00:12.36"><span style="s2" tts:color="lime">inner<br /> </span>style</p>
00:00:02,080 --> 00:00:05,840
<font color="white" face="sansSerif" size="16">default style<font color="red">custom style</font></font>
<b><font color="cyan" face="sansSerif" size="16"><font color="lime">part 1
</font>part 2</font></b>
00:00:05,840 --> 00:00:09,560
<u><font color="lime">line 3
part 3</font></u>
4
00:00:09,560 --> 00:00:12,360
<i><u><font color="yellow"><font color="lime">inner
 </font>style</font></u></i>
        self.assertEqual(dfxp2srt(dfxp_data_with_style), srt_data)
        dfxp_data_non_utf8 = '''<?xml version="1.0" encoding="UTF-16"?>
                    <p begin="0" end="1">Line 1</p>
                    <p begin="1" end="2">第二行</p>
            </tt>'''.encode('utf-16')
Line 1
        self.assertEqual(dfxp2srt(dfxp_data_non_utf8), srt_data)
    def test_cli_option(self):
        self.assertEqual(cli_option({'proxy': '127.0.0.1:3128'}, '--proxy', 'proxy'), ['--proxy', '127.0.0.1:3128'])
        self.assertEqual(cli_option({'proxy': None}, '--proxy', 'proxy'), [])
        self.assertEqual(cli_option({}, '--proxy', 'proxy'), [])
        self.assertEqual(cli_option({'retries': 10}, '--retries', 'retries'), ['--retries', '10'])
    def test_cli_valueless_option(self):
        self.assertEqual(cli_valueless_option(
            {'downloader': 'external'}, '--external-downloader', 'downloader', 'external'), ['--external-downloader'])
            {'downloader': 'internal'}, '--external-downloader', 'downloader', 'external'), [])
            {'nocheckcertificate': True}, '--no-check-certificate', 'nocheckcertificate'), ['--no-check-certificate'])
            {'nocheckcertificate': False}, '--no-check-certificate', 'nocheckcertificate'), [])
            {'checkcertificate': True}, '--no-check-certificate', 'checkcertificate', False), [])
            {'checkcertificate': False}, '--no-check-certificate', 'checkcertificate', False), ['--no-check-certificate'])
    def test_cli_bool_option(self):
            cli_bool_option(
                {'nocheckcertificate': True}, '--no-check-certificate', 'nocheckcertificate'),
            ['--no-check-certificate', 'true'])
                {'nocheckcertificate': True}, '--no-check-certificate', 'nocheckcertificate', separator='='),
            ['--no-check-certificate=true'])
                {'nocheckcertificate': True}, '--check-certificate', 'nocheckcertificate', 'false', 'true'),
            ['--check-certificate', 'false'])
                {'nocheckcertificate': True}, '--check-certificate', 'nocheckcertificate', 'false', 'true', '='),
            ['--check-certificate=false'])
                {'nocheckcertificate': False}, '--check-certificate', 'nocheckcertificate', 'false', 'true'),
            ['--check-certificate', 'true'])
                {'nocheckcertificate': False}, '--check-certificate', 'nocheckcertificate', 'false', 'true', '='),
            ['--check-certificate=true'])
                {}, '--check-certificate', 'nocheckcertificate', 'false', 'true', '='),
            [])
    def test_ohdave_rsa_encrypt(self):
        N = 0xab86b6371b5318aaa1d3c9e612a9f1264f372323c8c0f19875b5fc3b3fd3afcc1e5bec527aa94bfa85bffc157e4245aebda05389a5357b75115ac94f074aefcd
        e = 65537
            ohdave_rsa_encrypt(b'aa111222', e, N),
            '726664bd9a23fd0c70f9f1b84aab5e3905ce1e45a584e9cbcf9bcc7510338fc1986d6c599ff990d923aa43c51c0d9013cd572e13bc58f4ae48f2ed8c0b0ba881')
    def test_pkcs1pad(self):
        padded_data = pkcs1pad(data, 32)
        self.assertEqual(padded_data[:2], [0, 2])
        self.assertEqual(padded_data[28:], [0, 1, 2, 3])
        self.assertRaises(ValueError, pkcs1pad, data, 8)
    def test_encode_base_n(self):
        self.assertEqual(encode_base_n(0, 30), '0')
        self.assertEqual(encode_base_n(80, 30), '2k')
        custom_table = '9876543210ZYXWVUTSRQPONMLKJIHGFEDCBA'
        self.assertEqual(encode_base_n(0, 30, custom_table), '9')
        self.assertEqual(encode_base_n(80, 30, custom_table), '7P')
        self.assertRaises(ValueError, encode_base_n, 0, 70)
        self.assertRaises(ValueError, encode_base_n, 0, 60, custom_table)
    def test_caesar(self):
        self.assertEqual(caesar('ace', 'abcdef', 2), 'cea')
        self.assertEqual(caesar('cea', 'abcdef', -2), 'ace')
        self.assertEqual(caesar('ace', 'abcdef', -2), 'eac')
        self.assertEqual(caesar('eac', 'abcdef', 2), 'ace')
        self.assertEqual(caesar('ace', 'abcdef', 0), 'ace')
        self.assertEqual(caesar('xyz', 'abcdef', 2), 'xyz')
        self.assertEqual(caesar('abc', 'acegik', 2), 'ebg')
        self.assertEqual(caesar('ebg', 'acegik', -2), 'abc')
    def test_rot47(self):
        self.assertEqual(rot47('yt-dlp'), r'JE\5=A')
        self.assertEqual(rot47('YT-DLP'), r'*%\s{!')
    def test_urshift(self):
        self.assertEqual(urshift(3, 1), 1)
        self.assertEqual(urshift(-3, 1), 2147483646)
    GET_ELEMENT_BY_CLASS_TEST_STRING = '''
        <span class="foo bar">nice</span>
    def test_get_element_by_class(self):
        html = self.GET_ELEMENT_BY_CLASS_TEST_STRING
        self.assertEqual(get_element_by_class('foo', html), 'nice')
        self.assertEqual(get_element_by_class('no-such-class', html), None)
    def test_get_element_html_by_class(self):
        self.assertEqual(get_element_html_by_class('foo', html), html.strip())
    GET_ELEMENT_BY_ATTRIBUTE_TEST_STRING = '''
        <div itemprop="author" itemscope>foo</div>
    def test_get_element_by_attribute(self):
        self.assertEqual(get_element_by_attribute('class', 'foo bar', html), 'nice')
        self.assertEqual(get_element_by_attribute('class', 'foo', html), None)
        self.assertEqual(get_element_by_attribute('class', 'no-such-foo', html), None)
        html = self.GET_ELEMENT_BY_ATTRIBUTE_TEST_STRING
        self.assertEqual(get_element_by_attribute('itemprop', 'author', html), 'foo')
    def test_get_element_html_by_attribute(self):
        self.assertEqual(get_element_html_by_attribute('class', 'foo bar', html), html.strip())
        self.assertEqual(get_element_html_by_attribute('class', 'foo', html), None)
        self.assertEqual(get_element_html_by_attribute('class', 'no-such-foo', html), None)
        self.assertEqual(get_element_html_by_attribute('itemprop', 'author', html), html.strip())
    GET_ELEMENTS_BY_CLASS_TEST_STRING = '''
        <span class="foo bar">nice</span><span class="foo bar">also nice</span>
    GET_ELEMENTS_BY_CLASS_RES = ['<span class="foo bar">nice</span>', '<span class="foo bar">also nice</span>']
    def test_get_elements_by_class(self):
        html = self.GET_ELEMENTS_BY_CLASS_TEST_STRING
        self.assertEqual(get_elements_by_class('foo', html), ['nice', 'also nice'])
        self.assertEqual(get_elements_by_class('no-such-class', html), [])
    def test_get_elements_html_by_class(self):
        self.assertEqual(get_elements_html_by_class('foo', html), self.GET_ELEMENTS_BY_CLASS_RES)
        self.assertEqual(get_elements_html_by_class('no-such-class', html), [])
    def test_get_elements_by_attribute(self):
        self.assertEqual(get_elements_by_attribute('class', 'foo bar', html), ['nice', 'also nice'])
        self.assertEqual(get_elements_by_attribute('class', 'foo', html), [])
        self.assertEqual(get_elements_by_attribute('class', 'no-such-foo', html), [])
    def test_get_elements_html_by_attribute(self):
        self.assertEqual(get_elements_html_by_attribute('class', 'foo bar', html), self.GET_ELEMENTS_BY_CLASS_RES)
        self.assertEqual(get_elements_html_by_attribute('class', 'foo', html), [])
        self.assertEqual(get_elements_html_by_attribute('class', 'no-such-foo', html), [])
    def test_get_elements_text_and_html_by_attribute(self):
            list(get_elements_text_and_html_by_attribute('class', 'foo bar', html)),
            list(zip(['nice', 'also nice'], self.GET_ELEMENTS_BY_CLASS_RES, strict=True)))
        self.assertEqual(list(get_elements_text_and_html_by_attribute('class', 'foo', html)), [])
        self.assertEqual(list(get_elements_text_and_html_by_attribute('class', 'no-such-foo', html)), [])
        self.assertEqual(list(get_elements_text_and_html_by_attribute(
            'class', 'foo', '<a class="foo">nice</a><span class="foo">nice</span>', tag='a')), [('nice', '<a class="foo">nice</a>')])
    GET_ELEMENT_BY_TAG_TEST_STRING = '''
    random text lorem ipsum</p>
        this should be returned
        <span>this should also be returned</span>
            this should also be returned
        closing tag above should not trick, so this should also be returned
    but this text should not be returned
    GET_ELEMENT_BY_TAG_RES_OUTERDIV_HTML = GET_ELEMENT_BY_TAG_TEST_STRING.strip()[32:276]
    GET_ELEMENT_BY_TAG_RES_OUTERDIV_TEXT = GET_ELEMENT_BY_TAG_RES_OUTERDIV_HTML[5:-6]
    GET_ELEMENT_BY_TAG_RES_INNERSPAN_HTML = GET_ELEMENT_BY_TAG_TEST_STRING.strip()[78:119]
    GET_ELEMENT_BY_TAG_RES_INNERSPAN_TEXT = GET_ELEMENT_BY_TAG_RES_INNERSPAN_HTML[6:-7]
    def test_get_element_text_and_html_by_tag(self):
        html = self.GET_ELEMENT_BY_TAG_TEST_STRING
            get_element_text_and_html_by_tag('div', html),
            (self.GET_ELEMENT_BY_TAG_RES_OUTERDIV_TEXT, self.GET_ELEMENT_BY_TAG_RES_OUTERDIV_HTML))
            get_element_text_and_html_by_tag('span', html),
            (self.GET_ELEMENT_BY_TAG_RES_INNERSPAN_TEXT, self.GET_ELEMENT_BY_TAG_RES_INNERSPAN_HTML))
        self.assertRaises(compat_HTMLParseError, get_element_text_and_html_by_tag, 'article', html)
    def test_iri_to_uri(self):
            iri_to_uri('https://www.google.com/search?q=foo&ie=utf-8&oe=utf-8&client=firefox-b'),
            'https://www.google.com/search?q=foo&ie=utf-8&oe=utf-8&client=firefox-b')  # Same
            iri_to_uri('https://www.google.com/search?q=Käsesoßenrührlöffel'),  # German for cheese sauce stirring spoon
            'https://www.google.com/search?q=K%C3%A4seso%C3%9Fenr%C3%BChrl%C3%B6ffel')
            iri_to_uri('https://www.google.com/search?q=lt<+gt>+eq%3D+amp%26+percent%25+hash%23+colon%3A+tilde~#trash=?&garbage=#'),
            'https://www.google.com/search?q=lt%3C+gt%3E+eq%3D+amp%26+percent%25+hash%23+colon%3A+tilde~#trash=?&garbage=#')
            iri_to_uri('http://правозащита38.рф/category/news/'),
            'http://xn--38-6kcaak9aj5chl4a3g.xn--p1ai/category/news/')
            iri_to_uri('http://www.правозащита38.рф/category/news/'),
            'http://www.xn--38-6kcaak9aj5chl4a3g.xn--p1ai/category/news/')
            iri_to_uri('https://i❤.ws/emojidomain/👍👏🤝💪'),
            'https://xn--i-7iq.ws/emojidomain/%F0%9F%91%8D%F0%9F%91%8F%F0%9F%A4%9D%F0%9F%92%AA')
            iri_to_uri('http://日本語.jp/'),
            'http://xn--wgv71a119e.jp/')
            iri_to_uri('http://导航.中国/'),
            'http://xn--fet810g.xn--fiqs8s/')
    def test_clean_podcast_url(self):
        self.assertEqual(clean_podcast_url('https://www.podtrac.com/pts/redirect.mp3/chtbl.com/track/5899E/traffic.megaphone.fm/HSW7835899191.mp3'), 'https://traffic.megaphone.fm/HSW7835899191.mp3')
        self.assertEqual(clean_podcast_url('https://play.podtrac.com/npr-344098539/edge1.pod.npr.org/anon.npr-podcasts/podcast/npr/waitwait/2020/10/20201003_waitwait_wwdtmpodcast201003-015621a5-f035-4eca-a9a1-7c118d90bc3c.mp3'), 'https://edge1.pod.npr.org/anon.npr-podcasts/podcast/npr/waitwait/2020/10/20201003_waitwait_wwdtmpodcast201003-015621a5-f035-4eca-a9a1-7c118d90bc3c.mp3')
        self.assertEqual(clean_podcast_url('https://pdst.fm/e/2.gum.fm/chtbl.com/track/chrt.fm/track/34D33/pscrb.fm/rss/p/traffic.megaphone.fm/ITLLC7765286967.mp3?updated=1687282661'), 'https://traffic.megaphone.fm/ITLLC7765286967.mp3?updated=1687282661')
        self.assertEqual(clean_podcast_url('https://pdst.fm/e/https://mgln.ai/e/441/www.buzzsprout.com/1121972/13019085-ep-252-the-deep-life-stack.mp3'), 'https://www.buzzsprout.com/1121972/13019085-ep-252-the-deep-life-stack.mp3')
    def test_LazyList(self):
        it = list(range(10))
        self.assertEqual(list(LazyList(it)), it)
        self.assertEqual(LazyList(it).exhaust(), it)
        self.assertEqual(LazyList(it)[5], it[5])
        self.assertEqual(LazyList(it)[5:], it[5:])
        self.assertEqual(LazyList(it)[:5], it[:5])
        self.assertEqual(LazyList(it)[::2], it[::2])
        self.assertEqual(LazyList(it)[1::2], it[1::2])
        self.assertEqual(LazyList(it)[5::-1], it[5::-1])
        self.assertEqual(LazyList(it)[6:2:-2], it[6:2:-2])
        self.assertEqual(LazyList(it)[::-1], it[::-1])
        self.assertTrue(LazyList(it))
        self.assertFalse(LazyList(range(0)))
        self.assertEqual(len(LazyList(it)), len(it))
        self.assertEqual(repr(LazyList(it)), repr(it))
        self.assertEqual(str(LazyList(it)), str(it))
        self.assertEqual(list(LazyList(it, reverse=True)), it[::-1])
        self.assertEqual(list(reversed(LazyList(it))[::-1]), it)
        self.assertEqual(list(reversed(LazyList(it))[1:3:7]), it[::-1][1:3:7])
    def test_LazyList_laziness(self):
        def test(ll, idx, val, cache):
            self.assertEqual(ll[idx], val)
            self.assertEqual(ll._cache, list(cache))
        ll = LazyList(range(10))
        test(ll, 0, 0, range(1))
        test(ll, 5, 5, range(6))
        test(ll, -3, 7, range(10))
        ll = LazyList(range(10), reverse=True)
        test(ll, -1, 0, range(1))
        test(ll, 3, 6, range(10))
        ll = LazyList(itertools.count())
        test(ll, 10, 10, range(11))
        ll = reversed(ll)
        test(ll, -15, 14, range(15))
    def test_format_bytes(self):
        self.assertEqual(format_bytes(0), '0.00B')
        self.assertEqual(format_bytes(1000), '1000.00B')
        self.assertEqual(format_bytes(1024), '1.00KiB')
        self.assertEqual(format_bytes(1024**2), '1.00MiB')
        self.assertEqual(format_bytes(1024**3), '1.00GiB')
        self.assertEqual(format_bytes(1024**4), '1.00TiB')
        self.assertEqual(format_bytes(1024**5), '1.00PiB')
        self.assertEqual(format_bytes(1024**6), '1.00EiB')
        self.assertEqual(format_bytes(1024**7), '1.00ZiB')
        self.assertEqual(format_bytes(1024**8), '1.00YiB')
        self.assertEqual(format_bytes(1024**9), '1024.00YiB')
    def test_hide_login_info(self):
        self.assertEqual(Config.hide_login_info(['-u', 'foo', '-p', 'bar']),
                         ['-u', 'PRIVATE', '-p', 'PRIVATE'])
        self.assertEqual(Config.hide_login_info(['-u']), ['-u'])
        self.assertEqual(Config.hide_login_info(['-u', 'foo', '-u', 'bar']),
                         ['-u', 'PRIVATE', '-u', 'PRIVATE'])
        self.assertEqual(Config.hide_login_info(['--username=foo']),
                         ['--username=PRIVATE'])
    def test_locked_file(self):
        TEXT = 'test_locked_file\n'
        FILE = 'test_locked_file.ytdl'
        MODES = 'war'  # Order is important
            for lock_mode in MODES:
                with locked_file(FILE, lock_mode, False) as f:
                    if lock_mode == 'r':
                        self.assertEqual(f.read(), TEXT * 2, 'Wrong file content')
                        f.write(TEXT)
                    for test_mode in MODES:
                        testing_write = test_mode != 'r'
                            with locked_file(FILE, test_mode, False):
                        except (BlockingIOError, PermissionError):
                            if not testing_write:  # FIXME: blocked read access
                                print(f'Known issue: Exclusive lock ({lock_mode}) blocks read access ({test_mode})')
                            self.assertTrue(testing_write, f'{test_mode} is blocked by {lock_mode}')
                            self.assertFalse(testing_write, f'{test_mode} is not blocked by {lock_mode}')
            with contextlib.suppress(OSError):
                os.remove(FILE)
    def test_determine_file_encoding(self):
        self.assertEqual(determine_file_encoding(b''), (None, 0))
        self.assertEqual(determine_file_encoding(b'--verbose -x --audio-format mkv\n'), (None, 0))
        self.assertEqual(determine_file_encoding(b'\xef\xbb\xbf'), ('utf-8', 3))
        self.assertEqual(determine_file_encoding(b'\x00\x00\xfe\xff'), ('utf-32-be', 4))
        self.assertEqual(determine_file_encoding(b'\xff\xfe'), ('utf-16-le', 2))
        self.assertEqual(determine_file_encoding(b'\xff\xfe# coding: utf-8\n--verbose'), ('utf-16-le', 2))
        self.assertEqual(determine_file_encoding(b'# coding: utf-8\n--verbose'), ('utf-8', 0))
        self.assertEqual(determine_file_encoding(b'# coding: someencodinghere-12345\n--verbose'), ('someencodinghere-12345', 0))
        self.assertEqual(determine_file_encoding(b'#coding:utf-8\n--verbose'), ('utf-8', 0))
        self.assertEqual(determine_file_encoding(b'#  coding:   utf-8   \r\n--verbose'), ('utf-8', 0))
        self.assertEqual(determine_file_encoding('# coding: utf-32-be'.encode('utf-32-be')), ('utf-32-be', 0))
        self.assertEqual(determine_file_encoding('# coding: utf-16-le'.encode('utf-16-le')), ('utf-16-le', 0))
    def test_get_compatible_ext(self):
        self.assertEqual(get_compatible_ext(
            vcodecs=[None], acodecs=[None, None], vexts=['mp4'], aexts=['m4a', 'm4a']), 'mkv')
            vcodecs=[None], acodecs=[None], vexts=['flv'], aexts=['flv']), 'flv')
            vcodecs=[None], acodecs=[None], vexts=['mp4'], aexts=['m4a']), 'mp4')
            vcodecs=[None], acodecs=[None], vexts=['mp4'], aexts=['webm']), 'mkv')
            vcodecs=[None], acodecs=[None], vexts=['webm'], aexts=['m4a']), 'mkv')
            vcodecs=[None], acodecs=[None], vexts=['webm'], aexts=['webm']), 'webm')
            vcodecs=[None], acodecs=[None], vexts=['webm'], aexts=['weba']), 'webm')
            vcodecs=['h264'], acodecs=['mp4a'], vexts=['mov'], aexts=['m4a']), 'mp4')
            vcodecs=['av01.0.12M.08'], acodecs=['opus'], vexts=['mp4'], aexts=['webm']), 'webm')
            vcodecs=['vp9'], acodecs=['opus'], vexts=['webm'], aexts=['webm'], preferences=['flv', 'mp4']), 'mp4')
            vcodecs=['av1'], acodecs=['mp4a'], vexts=['webm'], aexts=['m4a'], preferences=('webm', 'mkv')), 'mkv')
    def test_try_call(self):
        def total(*x, **kwargs):
            return sum(x) + sum(kwargs.values())
        self.assertEqual(try_call(None), None,
                         msg='not a fn should give None')
        self.assertEqual(try_call(lambda: 1), 1,
                         msg='int fn with no expected_type should give int')
        self.assertEqual(try_call(lambda: 1, expected_type=int), 1,
                         msg='int fn with expected_type int should give int')
        self.assertEqual(try_call(lambda: 1, expected_type=dict), None,
                         msg='int fn with wrong expected_type should give None')
        self.assertEqual(try_call(total, args=(0, 1, 0), expected_type=int), 1,
                         msg='fn should accept arglist')
        self.assertEqual(try_call(total, kwargs={'a': 0, 'b': 1, 'c': 0}, expected_type=int), 1,
                         msg='fn should accept kwargs')
                         msg='int fn with no expected_type should give None')
        self.assertEqual(try_call(lambda x: {}, total, args=(42, ), expected_type=int), 42,
                         msg='expect first int result with expected_type int')
    def test_variadic(self):
        self.assertEqual(variadic(None), (None, ))
        self.assertEqual(variadic('spam'), ('spam', ))
        self.assertEqual(variadic('spam', allowed_types=dict), 'spam')
        with warnings.catch_warnings():
            warnings.simplefilter('ignore')
            self.assertEqual(variadic('spam', allowed_types=[dict]), 'spam')
    def test_http_header_dict(self):
        headers = HTTPHeaderDict()
        headers['ytdl-test'] = b'0'
        self.assertEqual(list(headers.items()), [('Ytdl-Test', '0')])
        self.assertEqual(list(headers.sensitive().items()), [('ytdl-test', '0')])
        headers['ytdl-test'] = 1
        self.assertEqual(list(headers.items()), [('Ytdl-Test', '1')])
        self.assertEqual(list(headers.sensitive().items()), [('ytdl-test', '1')])
        headers['Ytdl-test'] = '2'
        self.assertEqual(list(headers.items()), [('Ytdl-Test', '2')])
        self.assertEqual(list(headers.sensitive().items()), [('Ytdl-test', '2')])
        self.assertTrue('ytDl-Test' in headers)
        self.assertEqual(str(headers), str(dict(headers)))
        self.assertEqual(repr(headers), str(dict(headers)))
        headers.update({'X-dlp': 'data'})
        self.assertEqual(set(headers.items()), {('Ytdl-Test', '2'), ('X-Dlp', 'data')})
        self.assertEqual(set(headers.sensitive().items()), {('Ytdl-test', '2'), ('X-dlp', 'data')})
        self.assertEqual(dict(headers), {'Ytdl-Test': '2', 'X-Dlp': 'data'})
        self.assertEqual(len(headers), 2)
        self.assertEqual(headers.copy(), headers)
        headers2 = HTTPHeaderDict({'X-dlp': 'data3'}, headers, **{'X-dlP': 'data2'})
        self.assertEqual(set(headers2.items()), {('Ytdl-Test', '2'), ('X-Dlp', 'data2')})
        self.assertEqual(set(headers2.sensitive().items()), {('Ytdl-test', '2'), ('X-dlP', 'data2')})
        self.assertEqual(len(headers2), 2)
        headers2.clear()
        self.assertEqual(len(headers2), 0)
        # ensure we prefer latter headers
        headers3 = HTTPHeaderDict({'Ytdl-TeSt': 1}, {'Ytdl-test': 2})
        self.assertEqual(set(headers3.items()), {('Ytdl-Test', '2')})
        self.assertEqual(set(headers3.sensitive().items()), {('Ytdl-test', '2')})
        del headers3['ytdl-tesT']
        self.assertEqual(dict(headers3), {})
        headers4 = HTTPHeaderDict({'ytdl-test': 'data;'})
        self.assertEqual(set(headers4.items()), {('Ytdl-Test', 'data;')})
        self.assertEqual(set(headers4.sensitive().items()), {('ytdl-test', 'data;')})
        # common mistake: strip whitespace from values
        # https://github.com/yt-dlp/yt-dlp/issues/8729
        headers5 = HTTPHeaderDict({'ytdl-test': ' data; '})
        self.assertEqual(set(headers5.items()), {('Ytdl-Test', 'data;')})
        self.assertEqual(set(headers5.sensitive().items()), {('ytdl-test', 'data;')})
        # test if picklable
        headers6 = HTTPHeaderDict(a=1, b=2)
        self.assertEqual(pickle.loads(pickle.dumps(headers6)), headers6)
    def test_extract_basic_auth(self):
        assert extract_basic_auth('http://:foo.bar') == ('http://:foo.bar', None)
        assert extract_basic_auth('http://foo.bar') == ('http://foo.bar', None)
        assert extract_basic_auth('http://@foo.bar') == ('http://foo.bar', 'Basic Og==')
        assert extract_basic_auth('http://:pass@foo.bar') == ('http://foo.bar', 'Basic OnBhc3M=')
        assert extract_basic_auth('http://user:@foo.bar') == ('http://foo.bar', 'Basic dXNlcjo=')
        assert extract_basic_auth('http://user:pass@foo.bar') == ('http://foo.bar', 'Basic dXNlcjpwYXNz')
    @unittest.skipUnless(os.name == 'nt', 'Only relevant on Windows')
    def test_windows_escaping(self):
        tests = [
            'test"&',
            '%CMDCMDLINE:~-1%&',
            'a\nb',
            '\\',
            '!',
            '^!',
            'a \\ b',
            'a \\" b',
            'a \\ b\\',
            # We replace \r with \n
            ('a\r\ra', 'a\n\na'),
        def run_shell(args):
            stdout, stderr, error = Popen.run(
                args, text=True, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            assert not stderr
            assert not error
        for argument in tests:
            if isinstance(argument, str):
                expected = argument
                argument, expected = argument
            args = [sys.executable, '-c', 'import sys; print(end=sys.argv[1])', argument, 'end']
            assert run_shell(args) == expected
            assert run_shell(shell_quote(args, shell=True)) == expected
    def test_partial_application(self):
        assert callable(int_or_none(scale=10)), 'missing positional parameter should apply partially'
        assert int_or_none(10, scale=0.1) == 100, 'positionally passed argument should call function'
        assert int_or_none(v=10) == 10, 'keyword passed positional should call function'
        assert int_or_none(scale=0.1)(10) == 100, 'call after partial application should call the function'
    _JWT_KEY = '12345678'
    _JWT_HEADERS_1 = {'a': 'b'}
    _JWT_HEADERS_2 = {'typ': 'JWT', 'alg': 'HS256'}
    _JWT_HEADERS_3 = {'typ': 'JWT', 'alg': 'RS256'}
    _JWT_HEADERS_4 = {'c': 'd', 'alg': 'ES256'}
    _JWT_DECODED = {
        'foo': 'bar',
        'qux': 'baz',
    _JWT_SIMPLE = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmb28iOiJiYXIiLCJxdXgiOiJiYXoifQ.fKojvTWqnjNTbsdoDTmYNc4tgYAG3h_SWRzM77iLH0U'
    _JWT_WITH_EXTRA_HEADERS = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCIsImEiOiJiIn0.eyJmb28iOiJiYXIiLCJxdXgiOiJiYXoifQ.Ia91-B77yasfYM7jsB6iVKLew-3rO6ITjNmjWUVXCvQ'
    _JWT_WITH_REORDERED_HEADERS = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJiYXIiLCJxdXgiOiJiYXoifQ.slg-7COta5VOfB36p3tqV4MGPV6TTA_ouGnD48UEVq4'
    _JWT_WITH_REORDERED_HEADERS_AND_RS256_ALG = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJmb28iOiJiYXIiLCJxdXgiOiJiYXoifQ.XWp496oVgQnoits0OOocutdjxoaQwn4GUWWxUsKENPM'
    _JWT_WITH_EXTRA_HEADERS_AND_ES256_ALG = 'eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImMiOiJkIn0.eyJmb28iOiJiYXIiLCJxdXgiOiJiYXoifQ.oM_tc7IkfrwkoRh43rFFE1wOi3J3mQGwx7_lMyKQqDg'
    def test_jwt_encode(self):
        def test(expected, headers={}):
            self.assertEqual(jwt_encode(self._JWT_DECODED, self._JWT_KEY, headers=headers), expected)
        test(self._JWT_SIMPLE)
        test(self._JWT_WITH_EXTRA_HEADERS, headers=self._JWT_HEADERS_1)
        test(self._JWT_WITH_REORDERED_HEADERS, headers=self._JWT_HEADERS_2)
        test(self._JWT_WITH_REORDERED_HEADERS_AND_RS256_ALG, headers=self._JWT_HEADERS_3)
        test(self._JWT_WITH_EXTRA_HEADERS_AND_ES256_ALG, headers=self._JWT_HEADERS_4)
    def test_jwt_decode_hs256(self):
        def test(inp):
            self.assertEqual(jwt_decode_hs256(inp), self._JWT_DECODED)
        test(self._JWT_WITH_EXTRA_HEADERS)
        test(self._JWT_WITH_REORDERED_HEADERS)
        test(self._JWT_WITH_REORDERED_HEADERS_AND_RS256_ALG)
        test(self._JWT_WITH_EXTRA_HEADERS_AND_ES256_ALG)
from django.contrib.admindocs.utils import (
    docutils_is_available,
    parse_docstring,
    parse_rst,
from django.test.utils import captured_stderr
from .tests import AdminDocsSimpleTestCase
@unittest.skipUnless(docutils_is_available, "no docutils installed.")
class TestUtils(AdminDocsSimpleTestCase):
    This __doc__ output is required for testing. I copied this example from
    `admindocs` documentation. (TITLE)
    Display an individual :model:`myapp.MyModel`.
    **Context**
    ``RequestContext``
    ``mymodel``
        An instance of :model:`myapp.MyModel`.
    **Template:**
    :template:`myapp/my_template.html` (DESCRIPTION)
    some_metadata: some data
        self.docstring = self.__doc__
    def test_parse_docstring(self):
        title, description, metadata = parse_docstring(self.docstring)
        docstring_title = (
            "This __doc__ output is required for testing. I copied this example from\n"
            "`admindocs` documentation. (TITLE)"
        docstring_description = (
            "Display an individual :model:`myapp.MyModel`.\n\n"
            "**Context**\n\n``RequestContext``\n\n``mymodel``\n"
            "    An instance of :model:`myapp.MyModel`.\n\n"
            "**Template:**\n\n:template:`myapp/my_template.html` "
            "(DESCRIPTION)"
        self.assertEqual(title, docstring_title)
        self.assertEqual(description, docstring_description)
        self.assertEqual(metadata, {"some_metadata": "some data"})
    def test_title_output(self):
        title_output = parse_rst(title, "model", "model:admindocs")
        self.assertIn("TITLE", title_output)
        title_rendered = (
            "<p>This __doc__ output is required for testing. I copied this "
            'example from\n<a class="reference external" '
            'href="/admindocs/models/admindocs/">admindocs</a> documentation. '
            "(TITLE)</p>\n"
        self.assertHTMLEqual(title_output, title_rendered)
    def test_description_output(self):
        description_output = parse_rst(description, "model", "model:admindocs")
        description_rendered = (
            '<p>Display an individual <a class="reference external" '
            'href="/admindocs/models/myapp.mymodel/">myapp.MyModel</a>.</p>\n'
            '<p><strong>Context</strong></p>\n<p><tt class="docutils literal">'
            'RequestContext</tt></p>\n<dl class="docutils">\n<dt><tt class="'
            'docutils literal">mymodel</tt></dt>\n<dd>An instance of <a class="'
            'reference external" href="/admindocs/models/myapp.mymodel/">'
            "myapp.MyModel</a>.</dd>\n</dl>\n<p><strong>Template:</strong></p>"
            '\n<p><a class="reference external" href="/admindocs/templates/'
            'myapp/my_template.html/">myapp/my_template.html</a> (DESCRIPTION)'
            "</p>\n"
        self.assertHTMLEqual(description_output, description_rendered)
    def test_initial_header_level(self):
        header = "should be h3...\n\nHeader\n------\n"
        output = parse_rst(header, "header")
        self.assertIn("<h3>Header</h3>", output)
    def test_parse_rst(self):
        parse_rst() should use `cmsreference` as the default role.
        markup = '<p><a class="reference external" href="/admindocs/%s">title</a></p>\n'
        self.assertEqual(parse_rst("`title`", "model"), markup % "models/title/")
        self.assertEqual(parse_rst("`title`", "view"), markup % "views/title/")
        self.assertEqual(parse_rst("`title`", "template"), markup % "templates/title/")
        self.assertEqual(parse_rst("`title`", "filter"), markup % "filters/#title")
        self.assertEqual(parse_rst("`title`", "tag"), markup % "tags/#title")
    def test_parse_rst_with_docstring_no_leading_line_feed(self):
        title, body, _ = parse_docstring("firstline\n\n    second line")
            self.assertEqual(parse_rst(title, ""), "<p>firstline</p>\n")
            self.assertEqual(parse_rst(body, ""), "<p>second line</p>\n")
        self.assertEqual(stderr.getvalue(), "")
    def test_parse_rst_view_case_sensitive(self):
        source = ":view:`myapp.views.Index`"
        rendered = (
            '<p><a class="reference external" '
            'href="/admindocs/views/myapp.views.Index/">myapp.views.Index</a></p>'
        self.assertHTMLEqual(parse_rst(source, "view"), rendered)
    def test_parse_rst_template_case_sensitive(self):
        source = ":template:`Index.html`"
            '<p><a class="reference external" href="/admindocs/templates/Index.html/">'
            "Index.html</a></p>"
        self.assertHTMLEqual(parse_rst(source, "template"), rendered)
    def test_publish_parts(self):
        Django shouldn't break the default role for interpreted text
        when ``publish_parts`` is used directly, by setting it to
        ``cmsreference`` (#6681).
        import docutils
        self.assertNotEqual(
            docutils.parsers.rst.roles.DEFAULT_INTERPRETED_ROLE, "cmsreference"
        source = "reST, `interpreted text`, default role."
        markup = "<p>reST, <cite>interpreted text</cite>, default role.</p>\n"
        parts = docutils.core.publish_parts(source=source, writer="html4css1")
        self.assertEqual(parts["fragment"], markup)
"""Tests for django.db.backends.utils"""
from decimal import Decimal, Rounded
from django.db import NotSupportedError, connection
from django.db.backends.utils import (
    format_number,
    split_identifier,
    split_tzname_delta,
    truncate_name,
from django.test import (
class TestUtils(SimpleTestCase):
    def test_truncate_name(self):
        self.assertEqual(truncate_name("some_table", 10), "some_table")
        self.assertEqual(truncate_name("some_long_table", 10), "some_la38a")
        self.assertEqual(truncate_name("some_long_table", 10, 3), "some_loa38")
        self.assertEqual(truncate_name("some_long_table"), "some_long_table")
        # "user"."table" syntax
            truncate_name('username"."some_table', 10), 'username"."some_table'
            truncate_name('username"."some_long_table', 10), 'username"."some_la38a'
            truncate_name('username"."some_long_table', 10, 3), 'username"."some_loa38'
    def test_split_identifier(self):
        self.assertEqual(split_identifier("some_table"), ("", "some_table"))
        self.assertEqual(split_identifier('"some_table"'), ("", "some_table"))
            split_identifier('namespace"."some_table'), ("namespace", "some_table")
            split_identifier('"namespace"."some_table"'), ("namespace", "some_table")
    def test_format_number(self):
        def equal(value, max_d, places, result):
            self.assertEqual(format_number(Decimal(value), max_d, places), result)
        equal("0", 12, 3, "0.000")
        equal("0", 12, 8, "0.00000000")
        equal("1", 12, 9, "1.000000000")
        equal("0.00000000", 12, 8, "0.00000000")
        equal("0.000000004", 12, 8, "0.00000000")
        equal("0.000000008", 12, 8, "0.00000001")
        equal("0.000000000000000000999", 10, 8, "0.00000000")
        equal("0.1234567890", 12, 10, "0.1234567890")
        equal("0.1234567890", 12, 9, "0.123456789")
        equal("0.1234567890", 12, 8, "0.12345679")
        equal("0.1234567890", 12, 5, "0.12346")
        equal("0.1234567890", 12, 3, "0.123")
        equal("0.1234567890", 12, 1, "0.1")
        equal("0.1234567890", 12, 0, "0")
        equal("0.1234567890", None, 0, "0")
        equal("1234567890.1234567890", None, 0, "1234567890")
        equal("1234567890.1234567890", None, 2, "1234567890.12")
        equal("0.1234", 5, None, "0.1234")
        equal("123.12", 5, None, "123.12")
        with self.assertRaises(Rounded):
            equal("0.1234567890", 5, None, "0.12346")
            equal("1234567890.1234", 5, None, "1234600000")
    def test_split_tzname_delta(self):
            ("Asia/Ust+Nera", ("Asia/Ust+Nera", None, None)),
            ("Asia/Ust-Nera", ("Asia/Ust-Nera", None, None)),
            ("Asia/Ust+Nera-02:00", ("Asia/Ust+Nera", "-", "02:00")),
            ("Asia/Ust-Nera+05:00", ("Asia/Ust-Nera", "+", "05:00")),
            ("America/Coral_Harbour-01:00", ("America/Coral_Harbour", "-", "01:00")),
            ("America/Coral_Harbour+02:30", ("America/Coral_Harbour", "+", "02:30")),
            ("UTC+15:00", ("UTC", "+", "15:00")),
            ("UTC-04:43", ("UTC", "-", "04:43")),
            ("UTC", ("UTC", None, None)),
            ("UTC+1", ("UTC+1", None, None)),
        for tzname, expected in tests:
            with self.subTest(tzname=tzname):
                self.assertEqual(split_tzname_delta(tzname), expected)
class CursorWrapperTests(TransactionTestCase):
    available_apps = []
    def _test_procedure(self, procedure_sql, params, param_types, kparams=None):
            cursor.execute(procedure_sql)
        # Use a new cursor because in MySQL a procedure can't be used in the
        # same cursor in which it was created.
            cursor.callproc("test_procedure", params, kparams)
            editor.remove_procedure("test_procedure", param_types)
    @skipUnlessDBFeature("create_test_procedure_without_params_sql")
    def test_callproc_without_params(self):
        self._test_procedure(
            connection.features.create_test_procedure_without_params_sql, [], []
    @skipUnlessDBFeature("create_test_procedure_with_int_param_sql")
    def test_callproc_with_int_params(self):
            connection.features.create_test_procedure_with_int_param_sql,
            [1],
            ["INTEGER"],
    @skipUnlessDBFeature(
        "create_test_procedure_with_int_param_sql", "supports_callproc_kwargs"
    def test_callproc_kparams(self):
            {"P_I": 1},
    @skipIfDBFeature("supports_callproc_kwargs")
    def test_unsupported_callproc_kparams_raises_error(self):
            "Keyword parameters for callproc are not supported on this database "
            "backend."
        with self.assertRaisesMessage(NotSupportedError, msg):
                cursor.callproc("test_procedure", [], {"P_I": 1})
from django.forms.renderers import DjangoTemplates
from django.forms.utils import (
    ErrorDict,
    ErrorList,
    RenderableFieldMixin,
    RenderableMixin,
    flatatt,
    pretty_name,
from django.utils.translation import gettext_lazy
class FormsUtilsTestCase(SimpleTestCase):
    # Tests for forms/utils.py module.
    def test_flatatt(self):
        ###########
        # flatatt #
        self.assertEqual(flatatt({"id": "header"}), ' id="header"')
            flatatt({"class": "news", "title": "Read this"}),
            ' class="news" title="Read this"',
            flatatt({"class": "news", "title": "Read this", "required": "required"}),
            ' class="news" required="required" title="Read this"',
            flatatt({"class": "news", "title": "Read this", "required": True}),
            ' class="news" title="Read this" required',
            flatatt({"class": "news", "title": "Read this", "required": False}),
        self.assertEqual(flatatt({"class": None}), "")
        self.assertEqual(flatatt({}), "")
    def test_flatatt_no_side_effects(self):
        flatatt() does not modify the dict passed in.
        attrs = {"foo": "bar", "true": True, "false": False}
        attrs_copy = copy.copy(attrs)
        self.assertEqual(attrs, attrs_copy)
        first_run = flatatt(attrs)
        self.assertEqual(first_run, ' foo="bar" true')
        second_run = flatatt(attrs)
        self.assertEqual(first_run, second_run)
    def test_validation_error(self):
        # ValidationError #
        # Can take a string.
        self.assertHTMLEqual(
            str(ErrorList(ValidationError("There was an error.").messages)),
            '<ul class="errorlist"><li>There was an error.</li></ul>',
        # Can take a Unicode string.
            str(ErrorList(ValidationError("Not \u03c0.").messages)),
            '<ul class="errorlist"><li>Not π.</li></ul>',
        # Can take a lazy string.
            str(ErrorList(ValidationError(gettext_lazy("Error.")).messages)),
            '<ul class="errorlist"><li>Error.</li></ul>',
        # Can take a list.
            str(ErrorList(ValidationError(["Error one.", "Error two."]).messages)),
            '<ul class="errorlist"><li>Error one.</li><li>Error two.</li></ul>',
        # Can take a dict.
            str(
                ErrorList(
                        ValidationError(
                            {"error_1": "1. Error one.", "error_2": "2. Error two."}
                        ).messages
            '<ul class="errorlist"><li>1. Error one.</li><li>2. Error two.</li></ul>',
        # Can take a mixture in a list.
                                "1. First error.",
                                "2. Not \u03c0.",
                                gettext_lazy("3. Error."),
                                    "error_1": "4. First dict error.",
                                    "error_2": "5. Second dict error.",
            '<ul class="errorlist">'
            "<li>1. First error.</li>"
            "<li>2. Not π.</li>"
            "<li>3. Error.</li>"
            "<li>4. First dict error.</li>"
            "<li>5. Second dict error.</li>"
            "</ul>",
        class VeryBadError:
                return "A very bad error."
        # Can take a non-string.
            str(ErrorList(ValidationError(VeryBadError()).messages)),
            '<ul class="errorlist"><li>A very bad error.</li></ul>',
        # Escapes non-safe input but not input marked safe.
        example = 'Example of link: <a href="http://www.example.com/">example</a>'
            str(ErrorList([example])),
            '<ul class="errorlist"><li>Example of link: '
            "&lt;a href=&quot;http://www.example.com/&quot;&gt;example&lt;/a&gt;"
            "</li></ul>",
            str(ErrorList([mark_safe(example)])),
            '<a href="http://www.example.com/">example</a></li></ul>',
            str(ErrorDict({"name": example})),
            '<ul class="errorlist"><li>nameExample of link: '
            str(ErrorDict({"name": mark_safe(example)})),
    def test_error_list_copy(self):
        e = ErrorList(
                    message="message %(i)s",
                    params={"i": 1},
                    params={"i": 2},
        e_copy = copy.copy(e)
        self.assertEqual(e, e_copy)
        self.assertEqual(e.as_data(), e_copy.as_data())
    def test_error_list_copy_attributes(self):
        class CustomRenderer(DjangoTemplates):
        renderer = CustomRenderer()
        e = ErrorList(error_class="woopsies", renderer=renderer)
        e_copy = e.copy()
        self.assertEqual(e.error_class, e_copy.error_class)
        self.assertEqual(e.renderer, e_copy.renderer)
    def test_error_dict_copy(self):
        e = ErrorDict()
        e["__all__"] = ErrorList(
        e_deepcopy = copy.deepcopy(e)
        self.assertEqual(e, e_deepcopy)
    def test_error_dict_copy_attributes(self):
        e = ErrorDict(renderer=renderer)
    def test_error_dict_html_safe(self):
        e["username"] = "Invalid username."
        self.assertTrue(hasattr(ErrorDict, "__html__"))
        self.assertEqual(str(e), e.__html__())
    def test_error_list_html_safe(self):
        e = ErrorList(["Invalid username."])
        self.assertTrue(hasattr(ErrorList, "__html__"))
    def test_error_dict_is_dict(self):
        self.assertIsInstance(ErrorDict(), dict)
    def test_error_dict_is_json_serializable(self):
        init_errors = ErrorDict(
                    "__all__",
                        [ValidationError("Sorry this form only works on leap days.")]
                ("name", ErrorList([ValidationError("This field is required.")])),
        min_value_error_list = ErrorList(
            [ValidationError("Ensure this value is greater than or equal to 0.")]
        e = ErrorDict(
            init_errors,
            date=ErrorList(
                    ErrorDict(
                            "day": min_value_error_list,
                            "month": min_value_error_list,
                            "year": min_value_error_list,
        e["renderer"] = ErrorList(
                    "Select a valid choice. That choice is not one of the "
                    "available choices."
        self.assertJSONEqual(
            json.dumps(e),
                "__all__": ["Sorry this form only works on leap days."],
                "name": ["This field is required."],
                "date": [
                        "day": ["Ensure this value is greater than or equal to 0."],
                        "month": ["Ensure this value is greater than or equal to 0."],
                        "year": ["Ensure this value is greater than or equal to 0."],
                "renderer": [
    def test_get_context_must_be_implemented(self):
        mixin = RenderableMixin()
        msg = "Subclasses of RenderableMixin must provide a get_context() method."
            mixin.get_context()
    def test_field_mixin_as_hidden_must_be_implemented(self):
        mixin = RenderableFieldMixin()
        msg = "Subclasses of RenderableFieldMixin must provide an as_hidden() method."
            mixin.as_hidden()
    def test_field_mixin_as_widget_must_be_implemented(self):
        msg = "Subclasses of RenderableFieldMixin must provide an as_widget() method."
            mixin.as_widget()
    def test_pretty_name(self):
        self.assertEqual(pretty_name("john_doe"), "John doe")
        self.assertEqual(pretty_name(None), "")
        self.assertEqual(pretty_name(""), "")
from django.contrib.staticfiles.utils import check_settings
class CheckSettingsTests(SimpleTestCase):
    @override_settings(DEBUG=True, MEDIA_URL="static/media/", STATIC_URL="static/")
    def test_media_url_in_static_url(self):
        msg = "runserver can't serve media if MEDIA_URL is within STATIC_URL."
        with self.assertRaisesMessage(ImproperlyConfigured, msg):
            check_settings()
        with self.settings(DEBUG=False):  # Check disabled if DEBUG=False.
from django.template import engines
class TemplateUtilsTests(SimpleTestCase):
    @override_settings(TEMPLATES=[{"BACKEND": "raise.import.error"}])
    def test_backend_import_error(self):
        Failing to import a backend keeps raising the original import error
        (#24265).
        with self.assertRaisesMessage(ImportError, "No module named 'raise"):
            engines.all()
                # Incorrect: APP_DIRS and loaders are mutually incompatible.
                "OPTIONS": {"loaders": []},
    def test_backend_improperly_configured(self):
        Failing to initialize a backend keeps raising the original exception
        msg = "app_dirs must not be set when loaders is defined."
    def test_backend_names_must_be_unique(self):
            "Template engine aliases aren't unique, duplicates: django. Set "
            "a unique NAME for each engine in settings.TEMPLATES."
from collections.abc import Callable, Sequence
from typing import Any, TypedDict
from typing_extensions import NotRequired, override
from langchain_core.language_models.fake_chat_models import FakeChatModel
    count_tokens_approximately,
from langchain_core.tools import BaseTool, tool
@pytest.mark.parametrize("msg_cls", [HumanMessage, AIMessage, SystemMessage])
def test_merge_message_runs_str(msg_cls: type[BaseMessage]) -> None:
    messages = [msg_cls("foo"), msg_cls("bar"), msg_cls("baz")]
    messages_model_copy = [m.model_copy(deep=True) for m in messages]
    expected = [msg_cls("foo\nbar\nbaz")]
    actual = merge_message_runs(messages)
    assert messages == messages_model_copy
def test_merge_message_runs_str_with_specified_separator(
    msg_cls: type[BaseMessage],
    expected = [msg_cls("foo<sep>bar<sep>baz")]
    actual = merge_message_runs(messages, chunk_separator="<sep>")
def test_merge_message_runs_str_without_separator(
    expected = [msg_cls("foobarbaz")]
    actual = merge_message_runs(messages, chunk_separator="")
def test_merge_message_runs_response_metadata() -> None:
        AIMessage("foo", id="1", response_metadata={"input_tokens": 1}),
        AIMessage("bar", id="2", response_metadata={"input_tokens": 2}),
            "foo\nbar",
            response_metadata={"input_tokens": 1},
    # Check it's not mutated
    assert messages[1].response_metadata == {"input_tokens": 2}
def test_merge_message_runs_content() -> None:
        AIMessage("foo", id="1"),
                {"text": "bar", "type": "text"},
                {"image_url": "...", "type": "image_url"},
                ToolCall(name="foo_tool", args={"x": 1}, id="tool1", type="tool_call")
            id="2",
            "baz",
                ToolCall(name="foo_tool", args={"x": 5}, id="tool2", type="tool_call")
            id="3",
                "foo",
                ToolCall(name="foo_tool", args={"x": 1}, id="tool1", type="tool_call"),
                ToolCall(name="foo_tool", args={"x": 5}, id="tool2", type="tool_call"),
    invoked = merge_message_runs().invoke(messages)
    assert actual == invoked
def test_merge_messages_tool_messages() -> None:
        ToolMessage("foo", tool_call_id="1"),
        ToolMessage("bar", tool_call_id="2"),
    assert actual == messages
class FilterFields(TypedDict):
    include_names: NotRequired[Sequence[str]]
    exclude_names: NotRequired[Sequence[str]]
    include_types: NotRequired[Sequence[str | type[BaseMessage]]]
    exclude_types: NotRequired[Sequence[str | type[BaseMessage]]]
    include_ids: NotRequired[Sequence[str]]
    exclude_ids: NotRequired[Sequence[str]]
    exclude_tool_calls: NotRequired[Sequence[str] | bool]
    "filters",
        {"include_names": ["blur"]},
        {"exclude_names": ["blah"]},
        {"include_ids": ["2"]},
        {"exclude_ids": ["1"]},
        {"include_types": "human"},
        {"include_types": ["human"]},
        {"include_types": HumanMessage},
        {"include_types": [HumanMessage]},
        {"exclude_types": "system"},
        {"exclude_types": ["system"]},
        {"exclude_types": SystemMessage},
        {"exclude_types": [SystemMessage]},
        {"include_names": ["blah", "blur"], "exclude_types": [SystemMessage]},
def test_filter_message(filters: FilterFields) -> None:
        SystemMessage("foo", name="blah", id="1"),
        HumanMessage("bar", name="blur", id="2"),
    expected = messages[1:2]
    actual = filter_messages(messages, **filters)
    invoked = filter_messages(**filters).invoke(messages)
    assert invoked == actual
def test_filter_message_exclude_tool_calls() -> None:
        {"name": "foo", "id": "1", "args": {}, "type": "tool_call"},
        {"name": "bar", "id": "2", "args": {}, "type": "tool_call"},
        HumanMessage("foo", name="blah", id="1"),
        AIMessage("foo-response", name="blah", id="2"),
        HumanMessage("bar", name="blur", id="3"),
            "bar-response",
            tool_calls=tool_calls,
            id="4",
        ToolMessage("baz", tool_call_id="1", id="5"),
        ToolMessage("qux", tool_call_id="2", id="6"),
    expected = messages[:3]
    # test excluding all tool calls
    actual = filter_messages(messages, exclude_tool_calls=True)
    # test explicitly excluding all tool calls
    actual = filter_messages(messages, exclude_tool_calls=["1", "2"])
    # test excluding a specific tool call
    expected = messages[:5]
    expected[3] = expected[3].model_copy(update={"tool_calls": [tool_calls[0]]})
    actual = filter_messages(messages, exclude_tool_calls=["2"])
    # assert that we didn't mutate the original messages
def test_filter_message_exclude_tool_calls_content_blocks() -> None:
                {"text": "bar-response", "type": "text"},
                {"name": "foo", "type": "tool_use", "id": "1"},
                {"name": "bar", "type": "tool_use", "id": "2"},
    expected = messages[:4] + messages[-1:]
    expected[3] = expected[3].model_copy(
            "tool_calls": [tool_calls[1]],
    actual = filter_messages(messages, exclude_tool_calls=["1"])
_MESSAGES_TO_TRIM = [
    SystemMessage("This is a 4 token text."),
    HumanMessage("This is a 4 token text.", id="first"),
    HumanMessage("This is a 4 token text.", id="third"),
    AIMessage("This is a 4 token text.", id="fourth"),
_MESSAGES_TO_TRIM_COPY = [m.model_copy(deep=True) for m in _MESSAGES_TO_TRIM]
def test_trim_messages_first_30() -> None:
    actual = trim_messages(
        _MESSAGES_TO_TRIM,
    assert _MESSAGES_TO_TRIM == _MESSAGES_TO_TRIM_COPY
def test_trim_messages_first_30_allow_partial() -> None:
            [{"type": "text", "text": "This is the FIRST 4 token block."}], id="second"
def test_trim_messages_first_30_allow_partial_end_on_human() -> None:
        end_on="human",
def test_trim_messages_last_30_include_system() -> None:
def test_trim_messages_last_40_include_system_allow_partial() -> None:
        max_tokens=40,
def test_trim_messages_last_30_include_system_allow_partial_end_on_human() -> None:
def test_trim_messages_last_40_include_system_allow_partial_start_on_human() -> None:
def test_trim_messages_allow_partial_one_message() -> None:
        HumanMessage("Th", id="third"),
        [HumanMessage("This is a funky text.", id="third")],
        max_tokens=2,
        token_counter=lambda messages: sum(len(m.content) for m in messages),
        text_splitter=list,
def test_trim_messages_last_allow_partial_one_message() -> None:
        HumanMessage("t.", id="third"),
def test_trim_messages_allow_partial_text_splitter() -> None:
        HumanMessage("a 4 token text.", id="third"),
    def count_words(msgs: list[BaseMessage]) -> int:
        for msg in msgs:
                count += len(msg.content.split(" "))
                count += len(
                    " ".join(block["text"] for block in msg.content).split(" ")  # type: ignore[index]
    def _split_on_space(text: str) -> list[str]:
        splits = text.split(" ")
        return [s + " " for s in splits[:-1]] + splits[-1:]
        max_tokens=10,
        token_counter=count_words,
        text_splitter=_split_on_space,
def test_trim_messages_include_system_strategy_last_empty_messages() -> None:
    expected: list[BaseMessage] = []
    ).invoke([])
def test_trim_messages_invoke() -> None:
    actual = trim_messages(max_tokens=10, token_counter=dummy_token_counter).invoke(
        _MESSAGES_TO_TRIM
    expected = trim_messages(
        _MESSAGES_TO_TRIM, max_tokens=10, token_counter=dummy_token_counter
def test_trim_messages_bound_model_token_counter() -> None:
    trimmer = trim_messages(
        token_counter=FakeTokenCountingModel().bind(foo="bar"),  # type: ignore[call-overload]
    trimmer.invoke([HumanMessage("foobar")])
def test_trim_messages_bad_token_counter() -> None:
    trimmer = trim_messages(max_tokens=10, token_counter={})  # type: ignore[call-overload]
        match=re.escape(
            "'token_counter' expected to be a model that implements "
            "'get_num_tokens_from_messages()' or a function. "
            "Received object of type <class 'dict'>."
                default_msg_prefix_len + default_content_len + default_msg_suffix_len
def test_trim_messages_partial_text_splitting() -> None:
    messages = [HumanMessage(content="This is a long message that needs trimming")]
    messages_copy = [m.model_copy(deep=True) for m in messages]
    def count_characters(msgs: list[BaseMessage]) -> int:
        return sum(len(m.content) if isinstance(m.content, str) else 0 for m in msgs)
    # Return individual characters to test text splitting
    def char_splitter(text: str) -> list[str]:
        return list(text)
    result = trim_messages(
        max_tokens=10,  # Only allow 10 characters
        token_counter=count_characters,
        text_splitter=char_splitter,
    assert len(result) == 1
    assert result[0].content == "This is a "  # First 10 characters
    assert messages == messages_copy
def test_trim_messages_mixed_content_with_partial() -> None:
                {"type": "text", "text": "First part of text."},
                {"type": "text", "text": "Second part that should be trimmed."},
    # Count total length of all text parts
    def count_text_length(msgs: list[BaseMessage]) -> int:
                for block in msg.content:
                        total += len(block["text"])
            elif isinstance(msg.content, str):
                total += len(msg.content)
        max_tokens=20,  # Only allow first text block
        token_counter=count_text_length,
    assert len(result[0].content) == 1
    content = result[0].content[0]
    assert isinstance(content, dict)
    assert content["text"] == "First part of text."
def test_trim_messages_exact_token_boundary() -> None:
        SystemMessage(content="10 tokens exactly."),
        HumanMessage(content="Another 10 tokens."),
    # First message only
    result1 = trim_messages(
        max_tokens=10,  # Exactly the size of first message
    assert len(result1) == 1
    assert result1[0].content == "10 tokens exactly."
    # Both messages exactly fit
    result2 = trim_messages(
        max_tokens=20,  # Exactly the size of both messages
    assert len(result2) == 2
    assert result2 == messages
def test_trim_messages_start_on_with_allow_partial() -> None:
        HumanMessage(content="First human message"),
        AIMessage(content="AI response"),
        HumanMessage(content="Second human message"),
        max_tokens=20,
    assert result[0].content == "Second human message"
def test_trim_messages_token_counter_shortcut_approximate() -> None:
    """Test that `'approximate'` shortcut works for `token_counter`."""
        SystemMessage("This is a test message"),
        HumanMessage("Another test message", id="first"),
        AIMessage("AI response here", id="second"),
    # Test using the "approximate" shortcut
    result_shortcut = trim_messages(
        max_tokens=50,
    # Test using count_tokens_approximately directly
    result_direct = trim_messages(
    # Both should produce the same result
    assert result_shortcut == result_direct
def test_trim_messages_token_counter_shortcut_invalid() -> None:
    """Test that invalid `token_counter` shortcut raises `ValueError`."""
        HumanMessage("Another test message"),
    # Test with invalid shortcut - intentionally passing invalid string to verify
    # runtime error handling for dynamically-constructed inputs
    with pytest.raises(ValueError, match="Invalid token_counter shortcut 'invalid'"):
        trim_messages(  # type: ignore[call-overload]
            token_counter="invalid",
def test_trim_messages_token_counter_shortcut_with_options() -> None:
    """Test that `'approximate'` shortcut works with different trim options."""
        SystemMessage("System instructions"),
        HumanMessage("First human message", id="first"),
        AIMessage("First AI response", id="ai1"),
        HumanMessage("Second human message", id="second"),
        AIMessage("Second AI response", id="ai2"),
    # Test with various options
        max_tokens=100,
    # Should include system message and start on human
    assert len(result) >= 2
    assert isinstance(result[0], SystemMessage)
    assert any(isinstance(msg, HumanMessage) for msg in result[1:])
class FakeTokenCountingModel(FakeChatModel):
        tools: Sequence[dict[str, Any] | type | Callable | BaseTool] | None = None,
        return dummy_token_counter(messages)
    message_like: list = [
        # BaseMessage
        SystemMessage("1"),
        SystemMessage("1.1", additional_kwargs={"__openai_role__": "developer"}),
        HumanMessage([{"type": "image_url", "image_url": {"url": "2.1"}}], name="2.2"),
                {"type": "text", "text": "3.1"},
                    "id": "3.2",
                    "name": "3.3",
                    "input": {"3.4": "3.5"},
                {"type": "text", "text": "4.1"},
                    "id": "4.2",
                    "name": "4.3",
                    "input": {"4.4": "4.5"},
                    "args": {"4.4": "4.5"},
        ToolMessage("5.1", tool_call_id="5.2", name="5.3"),
        # OpenAI dict
        {"role": "system", "content": "6"},
        {"role": "developer", "content": "6.1"},
            "content": [{"type": "image_url", "image_url": {"url": "7.1"}}],
            "name": "7.2",
            "content": [{"type": "text", "text": "8.1"}],
                        "arguments": json.dumps({"8.2": "8.3"}),
                        "name": "8.4",
                    "id": "8.5",
            "name": "8.6",
        {"role": "tool", "content": "10.1", "tool_call_id": "10.2"},
        # Tuple/List
        ("system", "11.1"),
        ("developer", "11.2"),
        ("human", [{"type": "image_url", "image_url": {"url": "12.1"}}]),
            "ai",
                {"type": "text", "text": "13.1"},
                    "id": "13.2",
                    "name": "13.3",
                    "input": {"13.4": "13.5"},
        "14.1",
        # LangChain dict
            "role": "ai",
            "content": [{"type": "text", "text": "15.1"}],
            "tool_calls": [{"args": {"15.2": "15.3"}, "name": "15.4", "id": "15.5"}],
            "name": "15.6",
        SystemMessage(content="1"),
            content="1.1", additional_kwargs={"__openai_role__": "developer"}
            content=[{"type": "image_url", "image_url": {"url": "2.1"}}], name="2.2"
        ToolMessage(content="5.1", name="5.3", tool_call_id="5.2"),
        SystemMessage(content="6"),
            content="6.1", additional_kwargs={"__openai_role__": "developer"}
            content=[{"type": "image_url", "image_url": {"url": "7.1"}}], name="7.2"
            content=[{"type": "text", "text": "8.1"}],
            name="8.6",
                    "args": {"8.2": "8.3"},
        ToolMessage(content="10.1", tool_call_id="10.2"),
        SystemMessage(content="11.1"),
            content="11.2", additional_kwargs={"__openai_role__": "developer"}
        HumanMessage(content=[{"type": "image_url", "image_url": {"url": "12.1"}}]),
        HumanMessage(content="14.1"),
            content=[{"type": "text", "text": "15.1"}],
            name="15.6",
                    "name": "15.4",
                    "args": {"15.2": "15.3"},
                    "id": "15.5",
    actual = convert_to_messages(message_like)
def test_convert_to_messages_openai_refusal() -> None:
        [{"role": "assistant", "content": "", "refusal": "9.1"}]
    expected = [AIMessage("", additional_kwargs={"refusal": "9.1"})]
    # Raises error if content is missing.
        ValueError, match="Message dict must contain 'role' and 'content' keys"
        convert_to_messages([{"role": "assistant", "refusal": "9.1"}])
def create_image_data() -> str:
    return "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3+iiigD//2Q=="  # noqa: E501
def create_base64_image(image_format: str = "jpeg") -> str:
    data = create_image_data()
    return f"data:image/{image_format};base64,{data}"
def test_convert_to_openai_messages_string() -> None:
    message = "Hello"
    result = convert_to_openai_messages(message)
    assert result == {"role": "user", "content": "Hello"}
def test_convert_to_openai_messages_single_message() -> None:
    message: BaseMessage = HumanMessage(content="Hello")
    # Test IDs
    result = convert_to_openai_messages(message, include_id=True)
    assert result == {"role": "user", "content": "Hello"}  # no ID
    message = AIMessage(content="Hello", id="resp_123")
    assert result == {"role": "assistant", "content": "Hello"}
    assert result == {"role": "assistant", "content": "Hello", "id": "resp_123"}
def test_convert_to_openai_messages_multiple_messages() -> None:
        SystemMessage(content="System message"),
        HumanMessage(content="Human message"),
        AIMessage(content="AI message"),
    result = convert_to_openai_messages(messages)
        {"role": "system", "content": "System message"},
        {"role": "user", "content": "Human message"},
        {"role": "assistant", "content": "AI message"},
def test_convert_to_openai_messages_openai_string() -> None:
                {"type": "text", "text": "Hello"},
                {"type": "text", "text": "World"},
            content=[{"type": "text", "text": "Hi"}, {"type": "text", "text": "there"}]
        {"role": "user", "content": "Hello\nWorld"},
        {"role": "assistant", "content": "Hi\nthere"},
def test_convert_to_openai_messages_openai_block() -> None:
    messages = [HumanMessage(content="Hello"), AIMessage(content="Hi there")]
    result = convert_to_openai_messages(messages, text_format="block")
        {"role": "user", "content": [{"type": "text", "text": "Hello"}]},
        {"role": "assistant", "content": [{"type": "text", "text": "Hi there"}]},
def test_convert_to_openai_messages_invalid_format() -> None:
    with pytest.raises(ValueError, match="Unrecognized text_format="):
        convert_to_openai_messages(  # type: ignore[call-overload]
            [HumanMessage(content="Hello")],
            text_format="invalid",
def test_convert_to_openai_messages_openai_image() -> None:
    base64_image = create_base64_image()
                {"type": "text", "text": "Here's an image:"},
                {"type": "image_url", "image_url": {"url": base64_image}},
def test_convert_to_openai_messages_anthropic() -> None:
    image_data = create_image_data()
                    "text": "Here's an image:",
                        "data": image_data,
                {"type": "tool_use", "name": "foo", "input": {"bar": "baz"}, "id": "1"}
                    "type": "tool_result",
                    "tool_use_id": "1",
                    "is_error": False,
                {"type": "image_url", "image_url": {"url": create_base64_image()}},
                        "name": "foo",
                        "arguments": json.dumps({"bar": "baz"}),
                {"type": "image_url", "image_url": {"url": create_base64_image()}}
    # Test thinking blocks (pass through)
    thinking_block = {
        "signature": "abc123",
        "thinking": "Thinking text.",
        "type": "thinking",
    text_block = {"text": "Response text.", "type": "text"}
    messages = [AIMessage([thinking_block, text_block])]
    expected = [{"role": "assistant", "content": [thinking_block, text_block]}]
def test_convert_to_openai_messages_bedrock_converse_image() -> None:
                    "image": {
                        "format": "jpeg",
                        "source": {"bytes": base64.b64decode(image_data)},
    assert result[0]["content"][1]["type"] == "image_url"
    assert result[0]["content"][1]["image_url"]["url"] == create_base64_image()
def test_convert_to_openai_messages_vertexai_image() -> None:
                    "type": "media",
                    "data": base64.b64decode(image_data),
def test_convert_to_openai_messages_tool_message() -> None:
    tool_message = ToolMessage(content="Tool result", tool_call_id="123")
    result = convert_to_openai_messages([tool_message], text_format="block")
    assert result[0]["content"] == [{"type": "text", "text": "Tool result"}]
    assert result[0]["tool_call_id"] == "123"
def test_convert_to_openai_messages_tool_use() -> None:
                    "name": "calculator",
                    "input": {"a": "b"},
    assert result[0]["tool_calls"][0]["type"] == "function"
    assert result[0]["tool_calls"][0]["id"] == "123"
    assert result[0]["tool_calls"][0]["function"]["name"] == "calculator"
    assert result[0]["tool_calls"][0]["function"]["arguments"] == json.dumps({"a": "b"})
def test_convert_to_openai_messages_tool_use_unicode() -> None:
    """Test that Unicode characters in tool call args are preserved correctly."""
                    "name": "create_customer",
                    "input": {"customer_name": "你好啊集团"},
    assert result[0]["tool_calls"][0]["function"]["name"] == "create_customer"
    # Ensure Unicode characters are preserved, not escaped as \\uXXXX
    arguments_str = result[0]["tool_calls"][0]["function"]["arguments"]
    parsed_args = json.loads(arguments_str)
    assert parsed_args["customer_name"] == "你好啊集团"
    # Also ensure the raw JSON string contains Unicode, not escaped sequences
    assert "你好啊集团" in arguments_str
    assert "\\u4f60" not in arguments_str  # Should not contain escaped Unicode
def test_convert_to_openai_messages_json() -> None:
    json_data = {"key": "value"}
    messages = [HumanMessage(content=[{"type": "json", "json": json_data}])]
    assert result[0]["content"][0]["type"] == "text"
    assert json.loads(result[0]["content"][0]["text"]) == json_data
def test_convert_to_openai_messages_guard_content() -> None:
                    "type": "guard_content",
                    "guard_content": {"text": "Protected content"},
    assert result[0]["content"][0]["text"] == "Protected content"
def test_convert_to_openai_messages_invalid_block() -> None:
    messages = [HumanMessage(content=[{"type": "invalid", "foo": "bar"}])]
    with pytest.raises(ValueError, match="Unrecognized content block"):
            text_format="block",
            pass_through_unknown_blocks=False,
    # Accept by default
    assert result == [{"role": "user", "content": [{"type": "invalid", "foo": "bar"}]}]
def test_handle_openai_responses_blocks() -> None:
    blocks: str | list[str | dict] = [
        {"type": "reasoning", "id": "1"},
            "type": "function_call",
            "name": "multiply",
            "arguments": '{"x":5,"y":4}',
            "call_id": "call_abc123",
            "id": "fc_abc123",
            "status": "completed",
    message = AIMessage(content=blocks)
    expected_tool_call = {
        "id": "call_abc123",
    assert isinstance(result, dict)
    assert result["content"] == blocks
    assert result["tool_calls"] == [expected_tool_call]
    result = convert_to_openai_messages(message, pass_through_unknown_blocks=False)
    assert result["content"] == [{"type": "reasoning", "id": "1"}]
def test_convert_to_openai_messages_empty_message() -> None:
    result = convert_to_openai_messages(HumanMessage(content=""))
    assert result == {"role": "user", "content": ""}
def test_convert_to_openai_messages_empty_list() -> None:
    result = convert_to_openai_messages([])
    assert result == []
def test_convert_to_openai_messages_mixed_content_types() -> None:
                "Text message",
                {"type": "text", "text": "Structured text"},
    assert len(result[0]["content"]) == 3
    assert isinstance(result[0]["content"][0], dict)
    assert isinstance(result[0]["content"][1], dict)
    assert isinstance(result[0]["content"][2], dict)
def test_convert_to_openai_messages_developer() -> None:
    messages: list[MessageLikeRepresentation] = [
        SystemMessage("a", additional_kwargs={"__openai_role__": "developer"}),
        {"role": "developer", "content": "a"},
    assert result == [{"role": "developer", "content": "a"}] * 2
def test_convert_to_openai_messages_multimodal() -> None:
    """v0 and v1 content to OpenAI messages conversion."""
                # Prior v0 blocks
                {"type": "text", "text": "Text message"},
                    "url": "https://example.com/test.png",
                    "data": "<base64 string>",
                    "mime_type": "image/png",
                    "filename": "test.pdf",
                    # OpenAI Chat Completions file format
                    "file": {
                        "filename": "draconomicon.pdf",
                        "file_data": "data:application/pdf;base64,<base64 string>",
                    "source_type": "id",
                    "id": "file-abc123",
                    "mime_type": "audio/wav",
                    "type": "input_audio",
                # v1 Additions
                    "source_type": "url",  # backward compatibility v0 block field
                    "base64": "<base64 string>",
                    "filename": "test.pdf",  # backward compatibility v0 block field
                    "file_id": "file-abc123",
    message = result[0]
    assert len(message["content"]) == 13
    # Test auto-adding filename
    with pytest.warns(match="filename"):
    assert len(message["content"]) == 1
    block = message["content"][0]
    assert block == {
            "filename": "LC_AUTOGENERATED",
def test_count_tokens_approximately_empty_messages() -> None:
    # Test with empty message list
    assert count_tokens_approximately([]) == 0
    # Test with empty content
    messages = [HumanMessage(content="")]
    # 4 role chars -> 1 + 3 = 4 tokens
    assert count_tokens_approximately(messages) == 4
def test_count_tokens_approximately_with_names() -> None:
        # 5 chars + 4 role chars -> 3 + 3 = 6 tokens
        # (with name: extra 4 name chars, so total = 4 + 3 = 7 tokens)
        HumanMessage(content="Hello", name="user"),
        # 8 chars + 9 role chars -> 5 + 3 = 8 tokens
        # (with name: extra 9 name chars, so total = 7 + 3 = 10 tokens)
        AIMessage(content="Hi there", name="assistant"),
    # With names included (default)
    assert count_tokens_approximately(messages) == 17
    # Without names
    without_names = count_tokens_approximately(messages, count_name=False)
    assert without_names == 14
def test_count_tokens_approximately_openai_format() -> None:
    # same as test_count_tokens_approximately_with_names, but in OpenAI format
        {"role": "user", "content": "Hello", "name": "user"},
        {"role": "assistant", "content": "Hi there", "name": "assistant"},
def test_count_tokens_approximately_string_content() -> None:
        HumanMessage(content="Hello"),
        AIMessage(content="Hi there"),
        # 12 chars + 4 role chars -> 4 + 3 = 7 tokens
        HumanMessage(content="How are you?"),
    assert count_tokens_approximately(messages) == 21
def test_count_tokens_approximately_list_content() -> None:
        # '[{"foo": "bar"}]' -> 16 chars + 4 role chars -> 5 + 3 = 8 tokens
        HumanMessage(content=[{"foo": "bar"}]),
        # '[{"test": 123}]' -> 15 chars + 9 role chars -> 6 + 3 = 9 tokens
        AIMessage(content=[{"test": 123}]),
def test_count_tokens_approximately_tool_calls() -> None:
    tool_calls = [{"name": "test_tool", "args": {"foo": "bar"}, "id": "1"}]
        # tool calls json -> 79 chars + 9 role chars -> 22 + 3 = 25 tokens
        AIMessage(content="", tool_calls=tool_calls),
        # 15 chars + 4 role chars -> 5 + 3 = 8 tokens
        HumanMessage(content="Regular message"),
    assert count_tokens_approximately(messages) == 33
    # AI message w/ both content and tool calls
    # 94 chars + 9 role chars -> 26 + 3 = 29 tokens
        AIMessage(content="Regular message", tool_calls=tool_calls),
    assert count_tokens_approximately(messages) == 29
def test_count_tokens_approximately_custom_token_length() -> None:
        # 11 chars + 4 role chars -> (4 tokens of length 4 / 8 tokens of length 2) + 3
        HumanMessage(content="Hello world"),
        # 7 chars + 9 role chars -> (4 tokens of length 4 / 8 tokens of length 2) + 3
        AIMessage(content="Testing"),
    assert count_tokens_approximately(messages, chars_per_token=4) == 14
    assert count_tokens_approximately(messages, chars_per_token=2) == 22
def test_count_tokens_approximately_large_message_content() -> None:
    # Test with large content to ensure no issues
    large_text = "x" * 10000
    messages = [HumanMessage(content=large_text)]
    # 10,000 chars + 4 role chars -> 2501 + 3 = 2504 tokens
    assert count_tokens_approximately(messages) == 2504
def test_count_tokens_approximately_large_number_of_messages() -> None:
    messages = [HumanMessage(content="x")] * 1_000
    # 1 chars + 4 role chars -> 2 + 3 = 5 tokens
    assert count_tokens_approximately(messages) == 5_000
def test_count_tokens_approximately_mixed_content_types() -> None:
    # Test with a variety of content types in the same message list
        # 13 chars + 6 role chars -> 5 + 3 = 8 tokens
        SystemMessage(content="System prompt"),
        # 13 chars + 4 role chars + 9 name chars + 1 tool call ID char ->
        # 7 + 3 = 10 tokens
        ToolMessage(content="Tool response", name="test_tool", tool_call_id="1"),
    token_count = count_tokens_approximately(messages)
    assert token_count == 51
    # Ensure that count is consistent if we do one message at a time
    assert sum(count_tokens_approximately([m]) for m in messages) == token_count
def test_count_tokens_approximately_usage_metadata_scaling() -> None:
        HumanMessage("text"),
            response_metadata={"model_provider": "openai"},
            usage_metadata={"input_tokens": 0, "output_tokens": 0, "total_tokens": 100},
            usage_metadata={"input_tokens": 0, "output_tokens": 0, "total_tokens": 200},
    unscaled = count_tokens_approximately(messages)
    scaled = count_tokens_approximately(messages, use_usage_metadata_scaling=True)
    ratio = scaled / unscaled
    assert 1 <= round(ratio, 1) <= 1.2  # we ceil scale token counts, so can be > 1.2
    messages.extend([ToolMessage("text", tool_call_id="abc123")] * 3)
    unscaled_extended = count_tokens_approximately(messages)
    scaled_extended = count_tokens_approximately(
        messages, use_usage_metadata_scaling=True
    # scaling should still be based on the most recent AIMessage with total_tokens=200
    assert unscaled_extended > unscaled
    assert scaled_extended > scaled
    # And the scaled total should be the unscaled total multiplied by the same ratio.
    # ratio = 200 / unscaled (as of last AI message)
    expected_scaled_extended = math.ceil(unscaled_extended * ratio)
    assert scaled_extended <= expected_scaled_extended <= scaled_extended + 1
def test_count_tokens_approximately_usage_metadata_scaling_model_provider() -> None:
        HumanMessage("Hello"),
            "Hi",
        HumanMessage("More text"),
            "More response",
            response_metadata={"model_provider": "anthropic"},
    assert scaled == unscaled
def test_count_tokens_approximately_usage_metadata_scaling_total_tokens() -> None:
            # no usage metadata -> skip
    unscaled = count_tokens_approximately(messages, chars_per_token=5)
    scaled = count_tokens_approximately(
        messages, chars_per_token=5, use_usage_metadata_scaling=True
def test_count_tokens_approximately_usage_metadata_scaling_floor_at_one() -> None:
            # Set total_tokens lower than the approximate count up through this message.
            usage_metadata={"input_tokens": 0, "output_tokens": 0, "total_tokens": 1},
    # scale factor would be < 1, but we floor it at 1.0 to avoid decreasing counts
def test_get_buffer_string_with_structured_content() -> None:
    """Test get_buffer_string with structured content in messages."""
        HumanMessage(content=[{"type": "text", "text": "Hello, world!"}]),
        AIMessage(content=[{"type": "text", "text": "Hi there!"}]),
        SystemMessage(content=[{"type": "text", "text": "System message"}]),
    expected = "Human: Hello, world!\nAI: Hi there!\nSystem: System message"
    actual = get_buffer_string(messages)
def test_get_buffer_string_with_mixed_content() -> None:
    """Test get_buffer_string with mixed content types in messages."""
        HumanMessage(content="Simple text"),
        AIMessage(content=[{"type": "text", "text": "Structured text"}]),
        SystemMessage(content=[{"type": "text", "text": "Another structured text"}]),
        "Human: Simple text\nAI: Structured text\nSystem: Another structured text"
def test_get_buffer_string_with_function_call() -> None:
    """Test get_buffer_string with function call in additional_kwargs."""
            content="Hi",
                    "name": "test_function",
                    "arguments": '{"arg": "value"}',
    # TODO: consider changing this
        "Human: Hello\n"
        "AI: Hi{'name': 'test_function', 'arguments': '{\"arg\": \"value\"}'}"
def test_get_buffer_string_with_empty_content() -> None:
    """Test get_buffer_string with empty content in messages."""
        HumanMessage(content=[]),
        AIMessage(content=""),
        SystemMessage(content=[]),
    expected = "Human: \nAI: \nSystem: "
def test_get_buffer_string_with_tool_calls() -> None:
    """Test `get_buffer_string` with `tool_calls` field."""
        HumanMessage(content="What's the weather?"),
            content="Let me check the weather",
                    "args": {"city": "NYC"},
                    "id": "call_1",
    result = get_buffer_string(messages)
    assert "Human: What's the weather?" in result
    assert "AI: Let me check the weather" in result
    assert "get_weather" in result
    assert "NYC" in result
def test_get_buffer_string_with_tool_calls_empty_content() -> None:
    """Test `get_buffer_string` with `tool_calls` and empty `content`."""
                    "name": "search",
                    "args": {"query": "test"},
                    "id": "call_2",
    assert "AI: " in result
    assert "search" in result
def test_get_buffer_string_tool_calls_preferred_over_function_call() -> None:
    """Test that `tool_calls` takes precedence over legacy `function_call`."""
            content="Calling tools",
                    "name": "modern_tool",
                    "args": {"key": "value"},
                    "id": "call_3",
                "function_call": {"name": "legacy_function", "arguments": "{}"}
    assert "modern_tool" in result
    assert "legacy_function" not in result
def test_convert_to_openai_messages_reasoning_content() -> None:
    """Test convert_to_openai_messages with reasoning content blocks."""
    # Test reasoning block with empty summary
    msg = AIMessage(content=[{"type": "reasoning", "summary": []}])
    result = convert_to_openai_messages(msg, text_format="block")
    expected = {"role": "assistant", "content": [{"type": "reasoning", "summary": []}]}
    # Test reasoning block with summary content
    msg_with_summary = AIMessage(
                "type": "reasoning",
                "summary": [
                    {"type": "text", "text": "First thought"},
                    {"type": "text", "text": "Second thought"},
    result_with_summary = convert_to_openai_messages(
        msg_with_summary, text_format="block"
    expected_with_summary = {
    assert result_with_summary == expected_with_summary
    # Test mixed content with reasoning and text
            {"type": "text", "text": "Regular response"},
                "summary": [{"type": "text", "text": "My reasoning process"}],
    mixed_result = convert_to_openai_messages(mixed_msg, text_format="block")
    expected_mixed = {
    assert mixed_result == expected_mixed
# Tests for get_buffer_string XML format
def test_get_buffer_string_xml_empty_messages_list() -> None:
    """Test XML format with empty messages list."""
    messages: list[BaseMessage] = []
    result = get_buffer_string(messages, format="xml")
    expected = ""
def test_get_buffer_string_xml_basic() -> None:
    """Test XML format output with all message types."""
        FunctionMessage(content="Function result", name="test_fn"),
        ToolMessage(content="Tool result", tool_call_id="123"),
        '<message type="system">System message</message>\n'
        '<message type="human">Human message</message>\n'
        '<message type="ai">AI message</message>\n'
        '<message type="function">Function result</message>\n'
        '<message type="tool">Tool result</message>'
def test_get_buffer_string_xml_custom_prefixes() -> None:
    """Test XML format with custom human and ai prefixes."""
    result = get_buffer_string(
        messages, human_prefix="User", ai_prefix="Assistant", format="xml"
        '<message type="user">Hello</message>\n'
        '<message type="assistant">Hi there</message>'
def test_get_buffer_string_xml_custom_separator() -> None:
    """Test XML format with custom message separator."""
    result = get_buffer_string(messages, format="xml", message_separator="\n\n")
        '<message type="human">Hello</message>\n\n<message type="ai">Hi there</message>'
def test_get_buffer_string_prefix_custom_separator() -> None:
    """Test prefix format with custom message separator."""
    result = get_buffer_string(messages, format="prefix", message_separator=" | ")
    expected = "Human: Hello | AI: Hi there"
def test_get_buffer_string_xml_escaping() -> None:
    """Test XML format properly escapes special characters in content."""
        AIMessage(content='Yes, and here\'s a "quote"'),
    # xml.sax.saxutils.escape escapes <, >, & (not quotes in content)
        '<message type="human">Is 5 &lt; 10 &amp; 10 &gt; 5?</message>\n'
        '<message type="ai">Yes, and here\'s a "quote"</message>'
def test_get_buffer_string_xml_unicode_content() -> None:
    """Test XML format with Unicode content."""
        HumanMessage(content="你好世界"),  # Chinese: Hello World
        AIMessage(content="こんにちは"),  # Japanese: Hello
        '<message type="human">你好世界</message>\n'
        '<message type="ai">こんにちは</message>'
def test_get_buffer_string_xml_chat_message_valid_role() -> None:
    """Test XML format with `ChatMessage` having valid XML tag name role."""
        ChatMessage(content="Hello", role="Assistant"),
    # Role is used directly as the type attribute value
    expected = '<message type="Assistant">Hello</message>'
    # Spaces in role
        ChatMessage(content="Hello", role="my custom role"),
    # Custom roles with spaces use quoteattr for proper escaping
    expected = '<message type="my custom role">Hello</message>'
    # Special characters in role
        ChatMessage(content="Hello", role='role"with<special>'),
    # quoteattr handles escaping of special characters in attribute values
    # Note: quoteattr uses single quotes when the string contains double quotes
    expected = """<message type='role"with&lt;special&gt;'>Hello</message>"""
def test_get_buffer_string_xml_empty_content() -> None:
    """Test XML format with empty content."""
        HumanMessage(content=""),
    expected = '<message type="human"></message>\n<message type="ai"></message>'
def test_get_buffer_string_xml_tool_calls_with_content() -> None:
    """Test XML format with `AIMessage` having both `content` and `tool_calls`."""
            content="Let me check that",
    # Nested structure with content and tool_call elements
        '<message type="ai">\n'
        "  <content>Let me check that</content>\n"
        '  <tool_call id="call_1" name="get_weather">{"city": "NYC"}</tool_call>\n'
        "</message>"
def test_get_buffer_string_xml_tool_calls_empty_content() -> None:
    """Test XML format with `AIMessage` having empty `content` and `tool_calls`."""
    # No content element when content is empty
        '  <tool_call id="call_2" name="search">{"query": "test"}</tool_call>\n'
def test_get_buffer_string_xml_tool_calls_escaping() -> None:
    """Test XML format escapes special characters in tool calls."""
                    "name": "calculate",
                    "args": {"expression": "5 < 10 & 10 > 5"},
    # Special characters in tool_calls args should be escaped
    assert "&lt;" in result
    assert "&gt;" in result
    assert "&amp;" in result
    # Verify overall structure
    assert result.startswith('<message type="ai">')
    assert result.endswith("</message>")
def test_get_buffer_string_xml_function_call_legacy() -> None:
    """Test XML format with legacy `function_call` in `additional_kwargs`."""
            content="Calling function",
                "function_call": {"name": "test_fn", "arguments": '{"x": 1}'}
    # Nested structure with function_call element
    # Note: arguments is a string, so quotes inside are escaped
        "  <content>Calling function</content>\n"
        '  <function_call name="test_fn">{"x": 1}</function_call>\n'
def test_get_buffer_string_xml_structured_content() -> None:
    """Test XML format with structured content (list content blocks)."""
    # message.text property should extract text from structured content
        '<message type="human">Hello, world!</message>\n'
        '<message type="ai">Hi there!</message>'
def test_get_buffer_string_xml_multiline_content() -> None:
    """Test XML format with multiline content."""
        HumanMessage(content="Line 1\nLine 2\nLine 3"),
    expected = '<message type="human">Line 1\nLine 2\nLine 3</message>'
def test_get_buffer_string_xml_tool_calls_preferred_over_function_call() -> None:
    """Test that `tool_calls` takes precedence over legacy `function_call` in XML."""
    # Should use tool_call element, not function_call
    assert "<tool_call" in result
    assert "<function_call" not in result
def test_get_buffer_string_xml_multiple_tool_calls() -> None:
    """Test XML format with `AIMessage` having multiple `tool_calls`."""
            content="I'll help with that",
                    "name": "get_time",
                    "args": {"timezone": "EST"},
    # Should have nested structure with multiple tool_call elements
        "  <content>I'll help with that</content>\n"
        '  <tool_call id="call_2" name="get_time">{"timezone": "EST"}</tool_call>\n'
def test_get_buffer_string_xml_tool_call_special_chars_in_attrs() -> None:
    """Test that tool call attributes with quotes are properly escaped."""
    messages: list[BaseMessage] = [
                    "name": 'search"with"quotes',
                    "id": 'call"id',
    # quoteattr uses single quotes when value contains double quotes
    assert "name='search\"with\"quotes'" in result
    assert "id='call\"id'" in result
def test_get_buffer_string_xml_tool_call_none_id() -> None:
    """Test that tool calls with `None` id are handled correctly."""
                    "args": {},
    # Should handle None by converting to empty string
    assert 'id=""' in result
def test_get_buffer_string_xml_function_call_special_chars_in_name() -> None:
    """Test that `function_call` name with quotes is properly escaped."""
                    "name": 'func"name',
                    "arguments": "{}",
    assert "name='func\"name'" in result
def test_get_buffer_string_invalid_format() -> None:
    """Test that invalid format values raise `ValueError`."""
    messages: list[BaseMessage] = [HumanMessage(content="Hello")]
    with pytest.raises(ValueError, match="Unrecognized format"):
        get_buffer_string(messages, format="xm")  # type: ignore[arg-type]
        get_buffer_string(messages, format="invalid")  # type: ignore[arg-type]
        get_buffer_string(messages, format="")  # type: ignore[arg-type]
def test_get_buffer_string_xml_image_url_block() -> None:
    """Test XML format with image content block containing URL."""
                {"type": "text", "text": "What is in this image?"},
                {"type": "image", "url": "https://example.com/image.png"},
    assert '<message type="human">' in result
    assert "What is in this image?" in result
    assert '<image url="https://example.com/image.png" />' in result
def test_get_buffer_string_xml_image_file_id_block() -> None:
    """Test XML format with image content block containing `file_id`."""
                {"type": "text", "text": "Describe this:"},
                {"type": "image", "file_id": "file-abc123"},
    assert '<image file_id="file-abc123" />' in result
def test_get_buffer_string_xml_image_base64_skipped() -> None:
    """Test XML format skips image blocks with base64 data."""
                {"type": "text", "text": "What is this?"},
                {"type": "image", "base64": "iVBORw0KGgo...", "mime_type": "image/png"},
    assert "What is this?" in result
    assert "base64" not in result
    assert "iVBORw0KGgo" not in result
def test_get_buffer_string_xml_image_data_url_skipped() -> None:
    """Test XML format skips image blocks with data: URLs."""
                {"type": "text", "text": "Check this:"},
                {"type": "image", "url": "data:image/png;base64,iVBORw0KGgo..."},
    assert "Check this:" in result
    assert "data:image" not in result
def test_get_buffer_string_xml_openai_image_url_block() -> None:
    """Test XML format with OpenAI-style `image_url` block."""
                {"type": "text", "text": "Analyze this:"},
                    "image_url": {"url": "https://example.com/photo.jpg"},
    assert "Analyze this:" in result
    assert '<image url="https://example.com/photo.jpg" />' in result
def test_get_buffer_string_xml_openai_image_url_data_skipped() -> None:
    """Test XML format skips OpenAI-style `image_url` blocks with data: URLs."""
                {"type": "text", "text": "See this:"},
                    "image_url": {"url": "data:image/jpeg;base64,/9j/4AAQ..."},
    assert "See this:" in result
    assert "/9j/4AAQ" not in result
def test_get_buffer_string_xml_audio_url_block() -> None:
    """Test XML format with audio content block containing URL."""
                {"type": "text", "text": "Transcribe this:"},
                {"type": "audio", "url": "https://example.com/audio.mp3"},
    assert "Transcribe this:" in result
    assert '<audio url="https://example.com/audio.mp3" />' in result
def test_get_buffer_string_xml_audio_base64_skipped() -> None:
    """Test XML format skips audio blocks with base64 data."""
                {"type": "text", "text": "Listen:"},
                {"type": "audio", "base64": "UklGRi...", "mime_type": "audio/wav"},
    assert "Listen:" in result
    assert "UklGRi" not in result
def test_get_buffer_string_xml_video_url_block() -> None:
    """Test XML format with video content block containing URL."""
                {"type": "text", "text": "Describe this video:"},
                {"type": "video", "url": "https://example.com/video.mp4"},
    assert "Describe this video:" in result
    assert '<video url="https://example.com/video.mp4" />' in result
def test_get_buffer_string_xml_video_base64_skipped() -> None:
    """Test XML format skips video blocks with base64 data."""
                {"type": "text", "text": "Watch:"},
                {"type": "video", "base64": "AAAAFGZ0eXA...", "mime_type": "video/mp4"},
    assert "Watch:" in result
    assert "AAAAFGZ0eXA" not in result
def test_get_buffer_string_xml_reasoning_block() -> None:
    """Test XML format with reasoning content block."""
                {"type": "reasoning", "reasoning": "Let me think about this..."},
                {"type": "text", "text": "The answer is 42."},
    assert "<reasoning>Let me think about this...</reasoning>" in result
    assert "The answer is 42." in result
def test_get_buffer_string_xml_text_plain_block() -> None:
    """Test XML format with text-plain content block."""
                {"type": "text", "text": "Here is a document:"},
                    "type": "text-plain",
                    "text": "Document content here.",
                    "mime_type": "text/plain",
    assert "Here is a document:" in result
    assert "Document content here." in result
def test_get_buffer_string_xml_server_tool_call_block() -> None:
    """Test XML format with server_tool_call content block."""
                {"type": "text", "text": "Let me search for that."},
                    "type": "server_tool_call",
                    "id": "call_123",
                    "args": {"query": "weather today"},
    assert "Let me search for that." in result
    assert '<server_tool_call id="call_123" name="web_search">' in result
    assert '{"query": "weather today"}' in result
    assert "</server_tool_call>" in result
def test_get_buffer_string_xml_server_tool_result_block() -> None:
    """Test XML format with server_tool_result content block."""
                    "type": "server_tool_result",
                    "tool_call_id": "call_123",
                    "output": {"temperature": 72, "conditions": "sunny"},
    assert '<server_tool_result tool_call_id="call_123" status="success">' in result
    assert '"temperature": 72' in result
    assert "</server_tool_result>" in result
def test_get_buffer_string_xml_unknown_block_type_skipped() -> None:
    """Test XML format silently skips unknown block types."""
                {"type": "unknown_type", "data": "some data"},
    assert "Hello" in result
    assert "unknown_type" not in result
    assert "some data" not in result
def test_get_buffer_string_xml_mixed_content_blocks() -> None:
    """Test XML format with multiple different content block types."""
                {"type": "text", "text": "Look at this image and document:"},
                {"type": "image", "url": "https://example.com/img.png"},
                    "text": "Doc content",
                # This should be skipped (base64)
                {"type": "image", "base64": "abc123", "mime_type": "image/png"},
    assert "Look at this image and document:" in result
    assert '<image url="https://example.com/img.png" />' in result
    assert "Doc content" in result
    assert "abc123" not in result
def test_get_buffer_string_xml_escaping_in_content_blocks() -> None:
    """Test that special XML characters are escaped in content blocks."""
                {"type": "text", "text": "Is 5 < 10 & 10 > 5?"},
                {"type": "reasoning", "reasoning": "Let's check: <value> & </value>"},
    assert "Is 5 &lt; 10 &amp; 10 &gt; 5?" in result
    assert "&lt;value&gt; &amp; &lt;/value&gt;" in result
def test_get_buffer_string_xml_url_with_special_chars() -> None:
    """Test that URLs with special characters are properly quoted."""
                {"type": "image", "url": "https://example.com/img?a=1&b=2"},
    # quoteattr should handle the & in the URL
    assert "https://example.com/img?a=1&amp;b=2" in result
def test_get_buffer_string_xml_text_plain_truncation() -> None:
    """Test that text-plain content is truncated to 500 chars."""
    long_text = "x" * 600
                {"type": "text-plain", "text": long_text, "mime_type": "text/plain"},
    # Should be truncated to 500 chars + "..."
    assert "x" * 500 + "..." in result
    assert "x" * 501 not in result
def test_get_buffer_string_xml_server_tool_call_args_truncation() -> None:
    """Test that server_tool_call args are truncated to 500 chars."""
    long_value = "y" * 600
                    "name": "test_tool",
                    "args": {"data": long_value},
    assert "..." in result
    # The full 600-char value should not appear
    assert long_value not in result
def test_get_buffer_string_xml_server_tool_result_output_truncation() -> None:
    """Test that server_tool_result output is truncated to 500 chars."""
    long_output = "z" * 600
                    "tool_call_id": "call_1",
                    "output": {"result": long_output},
    assert long_output not in result
def test_get_buffer_string_xml_no_truncation_under_limit() -> None:
    """Test that content under 500 chars is not truncated."""
    short_text = "a" * 400
                {"type": "text-plain", "text": short_text, "mime_type": "text/plain"},
    assert short_text in result
    assert "..." not in result
def test_get_buffer_string_custom_system_prefix() -> None:
    """Test `get_buffer_string` with custom `system_prefix`."""
    result = get_buffer_string(messages, system_prefix="Instructions")
    assert result == "Instructions: You are a helpful assistant.\nHuman: Hello"
def test_get_buffer_string_custom_function_prefix() -> None:
    """Test `get_buffer_string` with custom `function_prefix`."""
        HumanMessage(content="Call a function"),
        FunctionMessage(name="test_func", content="Function result"),
    result = get_buffer_string(messages, function_prefix="Func")
    assert result == "Human: Call a function\nFunc: Function result"
def test_get_buffer_string_custom_tool_prefix() -> None:
    """Test `get_buffer_string` with custom `tool_prefix`."""
        HumanMessage(content="Use a tool"),
        ToolMessage(tool_call_id="call_123", content="Tool result"),
    result = get_buffer_string(messages, tool_prefix="ToolResult")
    assert result == "Human: Use a tool\nToolResult: Tool result"
def test_get_buffer_string_all_custom_prefixes() -> None:
    """Test `get_buffer_string` with all custom prefixes."""
        SystemMessage(content="System says hello"),
        HumanMessage(content="Human says hello"),
        AIMessage(content="AI says hello"),
        FunctionMessage(name="func", content="Function says hello"),
        ToolMessage(tool_call_id="call_1", content="Tool says hello"),
        human_prefix="User",
        ai_prefix="Assistant",
        system_prefix="Sys",
        function_prefix="Fn",
        tool_prefix="T",
        "Sys: System says hello\n"
        "User: Human says hello\n"
        "Assistant: AI says hello\n"
        "Fn: Function says hello\n"
        "T: Tool says hello"
def test_get_buffer_string_xml_custom_system_prefix() -> None:
    """Test `get_buffer_string` XML format with custom `system_prefix`."""
    result = get_buffer_string(messages, system_prefix="Instructions", format="xml")
        result == '<message type="instructions">You are a helpful assistant.</message>'
def test_get_buffer_string_xml_custom_function_prefix() -> None:
    """Test `get_buffer_string` XML format with custom `function_prefix`."""
    result = get_buffer_string(messages, function_prefix="Fn", format="xml")
    assert result == '<message type="fn">Function result</message>'
def test_get_buffer_string_xml_custom_tool_prefix() -> None:
    """Test `get_buffer_string` XML format with custom `tool_prefix`."""
    result = get_buffer_string(messages, tool_prefix="ToolOutput", format="xml")
    assert result == '<message type="tooloutput">Tool result</message>'
def test_get_buffer_string_xml_all_custom_prefixes() -> None:
    """Test `get_buffer_string` XML format with all custom prefixes."""
        FunctionMessage(name="func", content="Function message"),
        ToolMessage(tool_call_id="call_1", content="Tool message"),
        format="xml",
    # The messages are processed in order, not by type
    assert '<message type="sys">System message</message>' in result
    assert '<message type="user">Human message</message>' in result
    assert '<message type="assistant">AI message</message>' in result
    assert '<message type="fn">Function message</message>' in result
    assert '<message type="t">Tool message</message>' in result
def test_count_tokens_approximately_with_image_content() -> None:
    """Test approximate token counting with image content blocks."""
    message_with_image = HumanMessage(
            {"type": "text", "text": "What's in this image?"},
                "image_url": {"url": "data:image/jpeg;base64," + "A" * 100000},
    token_count = count_tokens_approximately([message_with_image])
    # Should be ~85 (image) + ~5 (text) + 3 (extra) = ~93 tokens, NOT 25,000+
    assert token_count < 200, f"Expected <200 tokens, got {token_count}"
    assert token_count > 80, f"Expected >80 tokens, got {token_count}"
def test_count_tokens_approximately_with_multiple_images() -> None:
    """Test token counting with multiple images."""
    message = HumanMessage(
            {"type": "text", "text": "Compare these images"},
            {"type": "image_url", "image_url": {"url": "data:image/jpeg;base64,AAA"}},
            {"type": "image_url", "image_url": {"url": "data:image/jpeg;base64,BBB"}},
    token_count = count_tokens_approximately([message])
    # Should be ~85 * 2 (images) + ~6 (text) + 3 (extra) = ~179 tokens
    assert 170 < token_count < 190
def test_count_tokens_approximately_text_only_backward_compatible() -> None:
    """Test that text-only messages still work correctly."""
        AIMessage(content="Hi there!"),
    # Should be ~15 tokens
    # (11 chars + 9 chars + roles + 2*3 extra)
    assert 13 <= token_count <= 17
def test_count_tokens_approximately_with_custom_image_penalty() -> None:
    """Test custom tokens_per_image parameter."""
            {"type": "text", "text": "test"},
            {"type": "image_url", "image_url": {"url": "data:image/jpeg;base64,XYZ"}},
    # Using custom image penalty (e.g., for Anthropic models)
    token_count = count_tokens_approximately([message], tokens_per_image=1600)
    # Should be ~1600 (image) + ~1 (text) + 3 (extra) = ~1604 tokens
    assert 1600 < token_count < 1610
def test_count_tokens_approximately_with_image_only_message() -> None:
    """Test token counting for a message that only contains an image."""
                "image_url": {"url": "data:image/jpeg;base64,AAA"},
    # Should be roughly tokens_per_image + role + extra per message.
    # Default tokens_per_image is 85 and extra_tokens_per_message is 3,
    # so we expect something in the ~90-110 range.
    assert 80 < token_count < 120
def test_count_tokens_approximately_with_unknown_block_type() -> None:
    """Test that unknown multimodal block types still contribute to token count."""
    text_only = count_tokens_approximately([HumanMessage(content="hello")])
    message_with_unknown_block = HumanMessage(
            {"type": "text", "text": "hello"},
            {"type": "foo", "bar": "baz"},  # unknown type, falls back to repr(block)
    mixed = count_tokens_approximately([message_with_unknown_block])
    # The message with an extra unknown block should be counted as more expensive
    # than the text-only version.
    assert mixed > text_only
def test_count_tokens_approximately_ai_tool_calls_skipped_for_list_content() -> None:
    """Test that tool_calls aren't double-counted for list (Anthropic-style) content."""
            "args": {"x": 1},
    # Case 1: content is a string -> tool_calls should be added to the char count.
    ai_with_text_content = AIMessage(
        content="do something",
    count_text = count_tokens_approximately([ai_with_text_content])
    # Case 2: content is a list (e.g. Anthropic-style blocks) -> tool_calls are
    # already represented in the content and should NOT be counted again.
    ai_with_list_content = AIMessage(
            {"type": "text", "text": "do something"},
                "input": {"x": 1},
    count_list = count_tokens_approximately([ai_with_list_content])
    assert count_text - 1 <= count_list <= count_text + 1
def test_count_tokens_approximately_respects_count_name_flag() -> None:
    """Test that the count_name flag controls whether names are included."""
    message = HumanMessage(content="hello", name="user-name")
    with_name = count_tokens_approximately([message], count_name=True)
    without_name = count_tokens_approximately([message], count_name=False)
    # When count_name is True, the name should contribute to the token count.
    assert with_name > without_name
def test_count_tokens_approximately_with_tools() -> None:
    """Test that tools parameter adds to token count."""
    messages = [HumanMessage(content="Hello")]
    base_count = count_tokens_approximately(messages)
    # Test with a BaseTool instance
    def get_weather(location: str) -> str:
        """Get the weather for a location."""
        return f"Weather in {location}"
    count_with_tool = count_tokens_approximately(messages, tools=[get_weather])
    assert count_with_tool > base_count
    # Test with a dict tool schema
    tool_schema = {
            "description": "Get the weather for a location.",
                "properties": {"location": {"type": "string"}},
                "required": ["location"],
    count_with_dict_tool = count_tokens_approximately(messages, tools=[tool_schema])
    assert count_with_dict_tool > base_count
    # Test with multiple tools
    def get_time(timezone: str) -> str:
        """Get the current time in a timezone."""
        return f"Time in {timezone}"
    count_with_multiple = count_tokens_approximately(
        messages, tools=[get_weather, get_time]
    assert count_with_multiple > count_with_tool
    # Test with no tools (None) should equal base count
    count_no_tools = count_tokens_approximately(messages, tools=None)
    assert count_no_tools == base_count
    # Test with empty tools list should equal base count
    count_empty_tools = count_tokens_approximately(messages, tools=[])
    assert count_empty_tools == base_count
"""Test functionality related to prompt utils."""
from langchain_core.example_selectors import sorted_values
def test_sorted_vals() -> None:
    """Test sorted values from dictionary."""
    test_dict = {"key2": "val2", "key1": "val1"}
    expected_response = ["val1", "val2"]
    assert sorted_values(test_dict) == expected_response
from langchain_core.runnables.base import RunnableLambda
    ("func", "expected_source"),
        (lambda x: x * 2, "lambda x: x * 2"),
        (lambda a, b: a + b, "lambda a, b: a + b"),
        (lambda x: x if x > 0 else 0, "lambda x: x if x > 0 else 0"),  # noqa: FURB136
def test_get_lambda_source(func: Callable[..., Any], expected_source: str) -> None:
    """Test get_lambda_source function."""
    source = get_lambda_source(func)
    assert source == expected_source
    ("text", "prefix", "expected_output"),
        ("line 1\nline 2\nline 3", "1", "line 1\n line 2\n line 3"),
        ("line 1\nline 2\nline 3", "ax", "line 1\n  line 2\n  line 3"),
def test_indent_lines_after_first(text: str, prefix: str, expected_output: str) -> None:
    """Test indent_lines_after_first function."""
    indented_text = indent_lines_after_first(text, prefix)
    assert indented_text == expected_output
global_agent = RunnableLambda[str, str](lambda x: x * 3)
def test_nonlocals() -> None:
    agent = RunnableLambda[str, str](lambda x: x * 2)
    def my_func(value: str, agent: dict[str, str]) -> str:
        return agent.get("agent_name", value)
    def my_func2(value: str) -> str:
        return str(agent.get("agent_name", value))  # type: ignore[attr-defined]
    def my_func3(value: str) -> str:
        return agent.invoke(value)
    def my_func4(value: str) -> str:
        return global_agent.invoke(value)
    def my_func5() -> tuple[Callable[[str], str], RunnableLambda]:
        def my_func6(value: str) -> str:
        return my_func6, global_agent
    assert get_function_nonlocals(my_func) == []
    assert get_function_nonlocals(my_func2) == []
    assert get_function_nonlocals(my_func3) == [agent.invoke]
    assert get_function_nonlocals(my_func4) == [global_agent.invoke]
    func, nl = my_func5()
    assert get_function_nonlocals(func) == [nl.invoke]
    assert RunnableLambda(my_func3).deps == [agent]
    assert RunnableLambda(my_func4).deps == [global_agent]
    assert RunnableLambda(func).deps == [nl]
from contextlib import AbstractContextManager, nullcontext
from copy import deepcopy
from pydantic import BaseModel, Field, SecretStr
from pydantic.v1 import BaseModel as PydanticV1BaseModel
from pydantic.v1 import Field as PydanticV1Field
from langchain_core import utils
from langchain_core.outputs import GenerationChunk
from langchain_core.utils import (
from langchain_core.utils._merge import merge_dicts, merge_lists, merge_obj
from langchain_core.utils.utils import secret_from_env
    ("package", "check_kwargs", "actual_version", "expected"),
        ("stub", {"gt_version": "0.1"}, "0.1.2", None),
        ("stub", {"gt_version": "0.1.2"}, "0.1.12", None),
        ("stub", {"gt_version": "0.1.2"}, "0.1.2", (ValueError, "> 0.1.2")),
        ("stub", {"gte_version": "0.1"}, "0.1.2", None),
        ("stub", {"gte_version": "0.1.2"}, "0.1.2", None),
def test_check_package_version(
    check_kwargs: dict[str, str | None],
    actual_version: str,
    expected: tuple[type[Exception], str] | None,
    with patch("langchain_core.utils.utils.version", return_value=actual_version):
        if expected is None:
            check_package_version(package, **check_kwargs)
            with pytest.raises(expected[0], match=expected[1]):
    ("left", "right", "expected"),
        # Merge `None` and `1`.
        ({"a": None}, {"a": 1}, {"a": 1}),
        # Merge `1` and `None`.
        ({"a": 1}, {"a": None}, {"a": 1}),
        # Merge `None` and a value.
        ({"a": None}, {"a": 0}, {"a": 0}),
        ({"a": None}, {"a": "txt"}, {"a": "txt"}),
        # Merge equal values.
        ({"a": 1}, {"a": 1}, {"a": 1}),
        ({"a": 1.5}, {"a": 1.5}, {"a": 1.5}),
        ({"a": True}, {"a": True}, {"a": True}),
        ({"a": False}, {"a": False}, {"a": False}),
        ({"a": "txt"}, {"a": "txt"}, {"a": "txttxt"}),
        ({"a": [1, 2]}, {"a": [1, 2]}, {"a": [1, 2, 1, 2]}),
        ({"a": {"b": "txt"}}, {"a": {"b": "txt"}}, {"a": {"b": "txttxt"}}),
        # Merge strings.
        ({"a": "one"}, {"a": "two"}, {"a": "onetwo"}),
        # Merge dicts.
        ({"a": {"b": 1}}, {"a": {"c": 2}}, {"a": {"b": 1, "c": 2}}),
            {"function_call": {"arguments": None}},
            {"function_call": {"arguments": "{\n"}},
        # Merge lists.
        ({"a": [1, 2]}, {"a": [3]}, {"a": [1, 2, 3]}),
        ({"a": 1, "b": 2}, {"a": 1}, {"a": 1, "b": 2}),
        ({"a": 1, "b": 2}, {"c": None}, {"a": 1, "b": 2, "c": None}),
        # Invalid inputs.
            {"a": 1},
            {"a": "1"},
            pytest.raises(
                    'additional_kwargs["a"] already exists in this message, '
                    "but with a different type."
            {"a": (1, 2)},
            {"a": (3,)},
                match=(
                    "Additional kwargs key a already exists in left dict and value "
                    r"has unsupported type .+tuple.+."
        # 'index' keyword has special handling
            {"a": [{"index": 0, "b": "{"}]},
            {"a": [{"index": 0, "b": "f"}]},
            {"a": [{"index": 0, "b": "{f"}]},
            {"a": [{"idx": 0, "b": "{"}]},
            {"a": [{"idx": 0, "b": "f"}]},
            {"a": [{"idx": 0, "b": "{"}, {"idx": 0, "b": "f"}]},
        # Integer 'index' should be preserved, not summed (tool call identification)
        ({"index": 1}, {"index": 1}, {"index": 1}),
        ({"index": 0}, {"index": 1}, {"index": 1}),
        # 'created' timestamp should be preserved, not summed
        ({"created": 1700000000}, {"created": 1700000000}, {"created": 1700000000}),
        ({"created": 1700000000}, {"created": 1700000001}, {"created": 1700000001}),
        # 'timestamp' should be preserved, not summed
        ({"timestamp": 100}, {"timestamp": 100}, {"timestamp": 100}),
        ({"timestamp": 100}, {"timestamp": 200}, {"timestamp": 200}),
        # Other integer fields should still be summed (e.g., token counts)
        ({"tokens": 10}, {"tokens": 5}, {"tokens": 15}),
        ({"count": 1}, {"count": 2}, {"count": 3}),
def test_merge_dicts(
    left: dict, right: dict, expected: dict | AbstractContextManager
    err = expected if isinstance(expected, AbstractContextManager) else nullcontext()
    left_copy = deepcopy(left)
    right_copy = deepcopy(right)
    with err:
        actual = merge_dicts(left, right)
        # no mutation
        assert left == left_copy
        assert right == right_copy
        # 'type' special key handling
        ({"type": "foo"}, {"type": "foo"}, {"type": "foo"}),
            {"type": "foo"},
            {"type": "bar"},
            pytest.raises(ValueError, match="Unable to merge"),
@pytest.mark.xfail(reason="Refactors to make in 0.3")
def test_merge_dicts_0_3(
    ("module_name", "pip_name", "package", "expected"),
        ("langchain_core.utils", None, None, utils),
        ("langchain_core.utils", "langchain-core", None, utils),
        ("langchain_core.utils", None, "langchain-core", utils),
        ("langchain_core.utils", "langchain-core", "langchain-core", utils),
def test_guard_import(
    module_name: str, pip_name: str | None, package: str | None, expected: Any
    if package is None and pip_name is None:
        ret = guard_import(module_name)
    elif package is None and pip_name is not None:
        ret = guard_import(module_name, pip_name=pip_name)
    elif package is not None and pip_name is None:
        ret = guard_import(module_name, package=package)
    elif package is not None and pip_name is not None:
        ret = guard_import(module_name, pip_name=pip_name, package=package)
        msg = "Invalid test case"
    assert ret == expected
    ("module_name", "pip_name", "package", "expected_pip_name"),
        ("langchain_core.utilsW", None, None, "langchain-core"),
        ("langchain_core.utilsW", "langchain-core-2", None, "langchain-core-2"),
        ("langchain_core.utilsW", None, "langchain-coreWX", "langchain-core"),
            "langchain_core.utilsW",
            "langchain-core-2",
            "langchain-coreWX",
        ("langchain_coreW", None, None, "langchain-coreW"),  # ModuleNotFoundError
def test_guard_import_failure(
    module_name: str,
    pip_name: str | None,
    package: str | None,
    expected_pip_name: str,
        ImportError,
        match=f"Could not import {module_name} python package. "
        f"Please install it with `pip install {expected_pip_name}`.",
        guard_import(module_name, pip_name=pip_name, package=package)
def test_get_pydantic_field_names_v1_in_2() -> None:
    class PydanticV1Model(PydanticV1BaseModel):
        field1: str
        field2: int
        alias_field: int = PydanticV1Field(alias="aliased_field")
    result = get_pydantic_field_names(PydanticV1Model)
    expected = {"field1", "field2", "aliased_field", "alias_field"}
def test_get_pydantic_field_names_v2_in_2() -> None:
    class PydanticModel(BaseModel):
        alias_field: int = Field(alias="aliased_field")
    result = get_pydantic_field_names(PydanticModel)
def test_from_env_with_env_variable() -> None:
    key = "TEST_KEY"
    value = "test_value"
    with patch.dict(os.environ, {key: value}):
        get_value = from_env(key)
        assert get_value() == value
def test_from_env_with_default_value() -> None:
    default_value = "default_value"
    with patch.dict(os.environ, {}, clear=True):
        get_value = from_env(key, default=default_value)
        assert get_value() == default_value
def test_from_env_with_error_message() -> None:
    error_message = "Custom error message"
        get_value = from_env(key, error_message=error_message)
        with pytest.raises(ValueError, match=error_message):
            get_value()
def test_from_env_with_default_error_message() -> None:
        with pytest.raises(ValueError, match=f"Did not find {key}"):
def test_secret_from_env_with_env_variable(monkeypatch: pytest.MonkeyPatch) -> None:
    # Set the environment variable
    monkeypatch.setenv("TEST_KEY", "secret_value")
    # Get the function
    get_secret: Callable[[], SecretStr | None] = secret_from_env("TEST_KEY")
    # Assert that it returns the correct value
    assert get_secret() == SecretStr("secret_value")
def test_secret_from_env_with_default_value(monkeypatch: pytest.MonkeyPatch) -> None:
    # Unset the environment variable
    monkeypatch.delenv("TEST_KEY", raising=False)
    # Get the function with a default value
    get_secret: Callable[[], SecretStr] = secret_from_env(
        "TEST_KEY", default="default_value"
    # Assert that it returns the default value
    assert get_secret() == SecretStr("default_value")
def test_secret_from_env_with_none_default(monkeypatch: pytest.MonkeyPatch) -> None:
    # Get the function with a default value of None
    get_secret: Callable[[], SecretStr | None] = secret_from_env(
        "TEST_KEY", default=None
    # Assert that it returns None
    assert get_secret() is None
def test_secret_from_env_without_default_raises_error(
    monkeypatch: pytest.MonkeyPatch,
    # Get the function without a default value
    get_secret: Callable[[], SecretStr] = secret_from_env("TEST_KEY")
    # Assert that it raises a ValueError with the correct message
    with pytest.raises(ValueError, match="Did not find TEST_KEY"):
        get_secret()
def test_secret_from_env_with_custom_error_message(
    # Get the function without a default value but with a custom error message
        "TEST_KEY", error_message="Custom error message"
    # Assert that it raises a ValueError with the custom message
    with pytest.raises(ValueError, match="Custom error message"):
def test_using_secret_from_env_as_default_factory(
        secret: SecretStr = Field(default_factory=secret_from_env("TEST_KEY"))
    # Pass the secret as a parameter
    foo = Foo(secret="super_secret")
    assert foo.secret.get_secret_value() == "super_secret"
    assert Foo().secret.get_secret_value() == "secret_value"
    class Bar(BaseModel):
        secret: SecretStr | None = Field(
            default_factory=secret_from_env("TEST_KEY_2", default=None)
    assert Bar().secret is None
    class Buzz(BaseModel):
            default_factory=secret_from_env("TEST_KEY_2", default="hello")
    # We know it will be SecretStr rather than SecretStr | None
    assert Buzz().secret.get_secret_value() == "hello"  # type: ignore[union-attr]
    class OhMy(BaseModel):
            default_factory=secret_from_env("FOOFOOFOOBAR")
    with pytest.raises(ValueError, match="Did not find FOOFOOFOOBAR"):
        OhMy()
def test_generation_chunk_addition_type_error() -> None:
    chunk1 = GenerationChunk(text="", generation_info={"len": 0})
    chunk2 = GenerationChunk(text="Non-empty text", generation_info={"len": 14})
    result = chunk1 + chunk2
    assert result == GenerationChunk(text="Non-empty text", generation_info={"len": 14})
        # Both None
        (None, None, None),
        # Left None
        (None, [1, 2], [1, 2]),
        # Right None
        ([1, 2], None, [1, 2]),
        # Simple merge
        ([1, 2], [3, 4], [1, 2, 3, 4]),
        # Empty lists
        ([], [], []),
        ([], [1], [1]),
        ([1], [], [1]),
        # Merge with index handling
            [{"index": 0, "text": "hello"}],
            [{"index": 0, "text": " world"}],
            [{"index": 0, "text": "hello world"}],
        # Multiple elements with different indexes
            [{"index": 0, "a": "x"}],
            [{"index": 1, "b": "y"}],
            [{"index": 0, "a": "x"}, {"index": 1, "b": "y"}],
        # Elements without index key
            [{"no_index": "a"}],
            [{"no_index": "b"}],
            [{"no_index": "a"}, {"no_index": "b"}],
def test_merge_lists(
    left: list | None, right: list | None, expected: list | None
    actual = merge_lists(left, right)
    # Verify no mutation
def test_merge_lists_multiple_others() -> None:
    """Test `merge_lists` with multiple lists."""
    result = merge_lists([1], [2], [3])
    assert result == [1, 2, 3]
def test_merge_lists_all_none() -> None:
    """Test `merge_lists` with all `None` arguments."""
    result = merge_lists(None, None, None)
    assert result is None
        (None, "hello", "hello"),
        ("hello", None, "hello"),
        # String merge
        ("hello", " world", "hello world"),
        # Dict merge
        ({"a": 1}, {"b": 2}, {"a": 1, "b": 2}),
        # List merge
        ([1, 2], [3], [1, 2, 3]),
        # Equal values
        (42, 42, 42),
        (3.14, 3.14, 3.14),
        (True, True, True),
def test_merge_obj(left: Any, right: Any, expected: Any) -> None:
    actual = merge_obj(left, right)
def test_merge_obj_type_mismatch() -> None:
    """Test `merge_obj` raises `TypeError` on type mismatch."""
    with pytest.raises(TypeError, match="left and right are of different types"):
        merge_obj("string", 123)
def test_merge_obj_unmergeable_values() -> None:
    """Test `merge_obj` raises `ValueError` on unmergeable values."""
    with pytest.raises(ValueError, match="Unable to merge"):
        merge_obj(1, 2)  # Different integers
def test_merge_obj_tuple_raises() -> None:
    """Test `merge_obj` raises `ValueError` for tuples."""
        merge_obj((1, 2), (3, 4))
"""Tests for langchain_core.vectorstores.utils module."""
pytest.importorskip("numpy")
from langchain_core.vectorstores.utils import _cosine_similarity
class TestCosineSimilarity:
    """Tests for _cosine_similarity function."""
    def test_basic_cosine_similarity(self) -> None:
        """Test basic cosine similarity calculation."""
        # Simple orthogonal vectors
        x: list[list[float]] = [[1, 0], [0, 1]]
        y: list[list[float]] = [[1, 0], [0, 1]]
        result = _cosine_similarity(x, y)
        expected = np.array([[1.0, 0.0], [0.0, 1.0]])
        np.testing.assert_array_almost_equal(result, expected)
    def test_identical_vectors(self) -> None:
        """Test cosine similarity of identical vectors."""
        x: list[list[float]] = [[1, 2, 3]]
        y: list[list[float]] = [[1, 2, 3]]
        expected = np.array([[1.0]])
    def test_opposite_vectors(self) -> None:
        """Test cosine similarity of opposite vectors."""
        y: list[list[float]] = [[-1, -2, -3]]
        expected = np.array([[-1.0]])
    def test_zero_vector(self) -> None:
        """Test cosine similarity with zero vector."""
        x: list[list[float]] = [[0, 0, 0]]
        with pytest.raises(ValueError, match="NaN values found"):
            _cosine_similarity(x, y)
    def test_multiple_vectors(self) -> None:
        """Test cosine similarity with multiple vectors."""
        x: list[list[float]] = [[1, 0], [0, 1], [1, 1]]
        expected = np.array(
                [1.0, 0.0],
                [0.0, 1.0],
                [1 / math.sqrt(2), 1 / math.sqrt(2)],
    def test_numpy_array_input(self) -> None:
        """Test with numpy array inputs."""
        x: np.ndarray = np.array([[1, 0], [0, 1]])
        y: np.ndarray = np.array([[1, 0], [0, 1]])
    def test_mixed_input_types(self) -> None:
        """Test with mixed input types (list and numpy array)."""
    def test_higher_dimensions(self) -> None:
        """Test with higher dimensional vectors."""
        x: list[list[float]] = [[1, 0, 0, 0], [0, 1, 0, 0]]
        y: list[list[float]] = [[1, 0, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]]
        expected = np.array([[1.0, 0.0, 0.0], [0.0, 0.0, 0.0]])
    def test_empty_matrices(self) -> None:
        """Test with empty matrices."""
        x: list[list[float]] = []
        y: list[list[float]] = []
        expected = np.array([[]])
        np.testing.assert_array_equal(result, expected)
    def test_single_empty_matrix(self) -> None:
        """Test with one empty matrix."""
    def test_dimension_mismatch_error(self) -> None:
        """Test error when matrices have different number of columns."""
        x: list[list[float]] = [[1, 2]]  # 2 columns
        y: list[list[float]] = [[1, 2, 3]]  # 3 columns
            ValueError, match="Number of columns in X and Y must be the same"
    def test_nan_and_inf_handling(self) -> None:
        """Test that NaN and inf values are handled properly."""
        # Create vectors that would result in NaN/inf in similarity calculation
        x: list[list[float]] = [[0, 0]]  # Zero vector
        y: list[list[float]] = [[0, 0]]  # Zero vector
    def test_large_values(self) -> None:
        """Test with large values to check numerical stability."""
        x: list[list[float]] = [[1e6, 1e6]]
        y: list[list[float]] = [[1e6, 1e6], [1e6, -1e6]]
        expected = np.array([[1.0, 0.0]])
    def test_small_values(self) -> None:
        """Test with very small values."""
        x: list[list[float]] = [[1e-10, 1e-10]]
        y: list[list[float]] = [[1e-10, 1e-10], [1e-10, -1e-10]]
    def test_single_vector_vs_multiple(self) -> None:
        """Test single vector against multiple vectors."""
        x: list[list[float]] = [[1, 1]]
        y: list[list[float]] = [[1, 0], [0, 1], [1, 1], [-1, -1]]
                    1 / math.sqrt(2),  # cos(45°)
                    1.0,  # cos(0°)
                    -1.0,  # cos(180°)
    def test_single_dimension_vectors(self) -> None:
        """Test with single-dimension vectors."""
        x: list[list[float]] = [[5], [-3]]
        y: list[list[float]] = [[2], [-1], [4]]
                [1.0, -1.0, 1.0],  # [5] vs [2], [-1], [4]
                [-1.0, 1.0, -1.0],  # [-3] vs [2], [-1], [4]
from langchain_core.utils import check_package_version
def test_check_package_version_pass() -> None:
    check_package_version("PyYAML", gte_version="5.4.1")
def test_check_package_version_fail() -> None:
        ValueError, match=re.escape("Expected PyYAML version to be < 5.4.1. Received ")
        check_package_version("PyYAML", lt_version="5.4.1")
from langchain_classic.schema.runnable.utils import __all__
from numpy.core import arange
from numpy.testing import assert_, assert_equal, assert_raises_regex
from numpy.lib import deprecate, deprecate_with_doc
import numpy.lib.utils as utils
@pytest.mark.skipif(sys.flags.optimize == 2, reason="Python running -OO")
    sys.version_info == (3, 10, 0, "candidate", 1),
    reason="Broken as of bpo-44524",
def test_lookfor():
    out = StringIO()
    utils.lookfor('eigenvalue', module='numpy', output=out,
                  import_modules=False)
    out = out.getvalue()
    assert_('numpy.linalg.eig' in out)
@deprecate
def old_func(self, x):
@deprecate(message="Rather use new_func2")
def old_func2(self, x):
def old_func3(self, x):
new_func3 = deprecate(old_func3, old_name="old_func3", new_name="new_func3")
def old_func4(self, x):
    """Summary.
    Further info.
new_func4 = deprecate(old_func4)
def old_func5(self, x):
        Bizarre indentation.
new_func5 = deprecate(old_func5, message="This function is\ndeprecated.")
def old_func6(self, x):
    Also in PEP-257.
new_func6 = deprecate(old_func6)
@deprecate_with_doc(msg="Rather use new_func7")
def old_func7(self,x):
def test_deprecate_decorator():
    assert_('deprecated' in old_func.__doc__)
def test_deprecate_decorator_message():
    assert_('Rather use new_func2' in old_func2.__doc__)
def test_deprecate_fn():
    assert_('old_func3' in new_func3.__doc__)
    assert_('new_func3' in new_func3.__doc__)
def test_deprecate_with_doc_decorator_message():
    assert_('Rather use new_func7' in old_func7.__doc__)
@pytest.mark.skipif(sys.flags.optimize == 2, reason="-OO discards docstrings")
@pytest.mark.parametrize('old_func, new_func', [
    (old_func4, new_func4),
    (old_func5, new_func5),
    (old_func6, new_func6),
def test_deprecate_help_indentation(old_func, new_func):
    _compare_docs(old_func, new_func)
    # Ensure we don't mess up the indentation
    for knd, func in (('old', old_func), ('new', new_func)):
        for li, line in enumerate(func.__doc__.split('\n')):
            if li == 0:
                assert line.startswith('    ') or not line.startswith(' '), knd
            elif line:
                assert line.startswith('    '), knd
def _compare_docs(old_func, new_func):
    old_doc = inspect.getdoc(old_func)
    new_doc = inspect.getdoc(new_func)
    index = new_doc.index('\n\n') + 2
    assert_equal(new_doc[index:], old_doc)
def test_deprecate_preserve_whitespace():
    assert_('\n        Bizarre' in new_func5.__doc__)
def test_deprecate_module():
    assert_(old_func.__module__ == __name__)
def test_safe_eval_nameconstant():
    # Test if safe_eval supports Python 3.4 _ast.NameConstant
    utils.safe_eval('None')
class TestByteBounds:
    def test_byte_bounds(self):
        # pointer difference matches size * itemsize
        # due to contiguity
        a = arange(12).reshape(3, 4)
        low, high = utils.byte_bounds(a)
        assert_equal(high - low, a.size * a.itemsize)
    def test_unusual_order_positive_stride(self):
        b = a.T
        low, high = utils.byte_bounds(b)
        assert_equal(high - low, b.size * b.itemsize)
    def test_unusual_order_negative_stride(self):
        b = a.T[::-1]
    def test_strided(self):
        a = arange(12)
        b = a[::2]
        # the largest pointer address is lost (even numbers only in the
        # stride), and compensate addresses for striding by 2
        assert_equal(high - low, b.size * 2 * b.itemsize - b.itemsize)
def test_assert_raises_regex_context_manager():
    with assert_raises_regex(ValueError, 'no deprecation warning'):
        raise ValueError('no deprecation warning')
def test_info_method_heading():
    # info(class) should only print "Methods:" heading if methods exist
    class NoPublicMethods:
    class WithPublicMethods:
        def first_method():
    def _has_method_heading(cls):
        utils.info(cls, output=out)
        return 'Methods:' in out.getvalue()
    assert _has_method_heading(WithPublicMethods)
    assert not _has_method_heading(NoPublicMethods)
def test_drop_metadata():
    def _compare_dtypes(dt1, dt2):
        return np.can_cast(dt1, dt2, casting='no')
    # structured dtype
    dt = np.dtype([('l1', [('l2', np.dtype('S8', metadata={'msg': 'toto'}))])],
                  metadata={'msg': 'titi'})
    dt_m = utils.drop_metadata(dt)
    assert _compare_dtypes(dt, dt_m) is True
    assert dt_m.metadata is None
    assert dt_m['l1'].metadata is None
    assert dt_m['l1']['l2'].metadata is None
    # alignement
    dt = np.dtype([('x', '<f8'), ('y', '<i4')],
                  align=True,
                  metadata={'msg': 'toto'})
    # subdtype
    dt = np.dtype('8f',
    # scalar
    dt = np.dtype('uint32',
@pytest.mark.parametrize("dtype",
        [np.dtype("i,i,i,i")[["f1", "f3"]],
        np.dtype("f8"),
        np.dtype("10i")])
def test_drop_metadata_identity_and_copy(dtype):
    # If there is no metadata, the identity is preserved:
    assert utils.drop_metadata(dtype) is dtype
    # If there is any, it is dropped (subforms are checked above)
    dtype = np.dtype(dtype, metadata={1: 2})
    assert utils.drop_metadata(dtype).metadata is None
    assert_equal, assert_array_equal, assert_almost_equal,
    assert_array_almost_equal, assert_array_less, build_err_msg,
    assert_raises, assert_warns, assert_no_warnings, assert_allclose,
    assert_approx_equal, assert_array_almost_equal_nulp, assert_array_max_ulp,
    clear_and_catch_warnings, suppress_warnings, assert_string_equal, assert_,
    tempdir, temppath, assert_no_gc_cycles, HAS_REFCOUNT
class _GenericTest:
    def _test_equal(self, a, b):
        self._assert_func(a, b)
    def _test_not_equal(self, a, b):
        with assert_raises(AssertionError):
    def test_array_rank1_eq(self):
        """Test two equal array of rank 1 are found equal."""
        a = np.array([1, 2])
        b = np.array([1, 2])
        self._test_equal(a, b)
    def test_array_rank1_noteq(self):
        """Test two different array of rank 1 are found not equal."""
        b = np.array([2, 2])
        self._test_not_equal(a, b)
    def test_array_rank2_eq(self):
        """Test two equal array of rank 2 are found equal."""
        a = np.array([[1, 2], [3, 4]])
        b = np.array([[1, 2], [3, 4]])
    def test_array_diffshape(self):
        """Test two arrays with different shapes are found not equal."""
        b = np.array([[1, 2], [1, 2]])
    def test_objarray(self):
        """Test object arrays."""
        a = np.array([1, 1], dtype=object)
        self._test_equal(a, 1)
    def test_array_likes(self):
        self._test_equal([1, 2, 3], (1, 2, 3))
class TestArrayEqual(_GenericTest):
        self._assert_func = assert_array_equal
    def test_generic_rank1(self):
        """Test rank 1 array for all dtypes."""
        def foo(t):
            a = np.empty(2, t)
            a.fill(1)
            b = a.copy()
            c = a.copy()
            c.fill(0)
            self._test_not_equal(c, b)
        # Test numeric types and object
        for t in '?bhilqpBHILQPfdgFDG':
            foo(t)
        # Test strings
        for t in ['S1', 'U1']:
    def test_0_ndim_array(self):
        x = np.array(473963742225900817127911193656584771)
        y = np.array(18535119325151578301457182298393896)
        assert_raises(AssertionError, self._assert_func, x, y)
        y = x
        self._assert_func(x, y)
        x = np.array(43)
        y = np.array(10)
    def test_generic_rank3(self):
        """Test rank 3 array for all dtypes."""
            a = np.empty((4, 2, 3), t)
    def test_nan_array(self):
        """Test arrays with nan values in them."""
        a = np.array([1, 2, np.nan])
        b = np.array([1, 2, np.nan])
        c = np.array([1, 2, 3])
    def test_string_arrays(self):
        a = np.array(['floupi', 'floupa'])
        b = np.array(['floupi', 'floupa'])
        c = np.array(['floupipi', 'floupa'])
    def test_recarrays(self):
        """Test record arrays."""
        a = np.empty(2, [('floupi', float), ('floupa', float)])
        a['floupi'] = [1, 2]
        a['floupa'] = [1, 2]
        c = np.empty(2, [('floupipi', float),
                         ('floupi', float), ('floupa', float)])
        c['floupipi'] = a['floupi'].copy()
        c['floupa'] = a['floupa'].copy()
    def test_masked_nan_inf(self):
        # Regression test for gh-11121
        a = np.ma.MaskedArray([3., 4., 6.5], mask=[False, True, False])
        b = np.array([3., np.nan, 6.5])
        self._test_equal(b, a)
        a = np.ma.MaskedArray([3., 4., 6.5], mask=[True, False, False])
        b = np.array([np.inf, 4., 6.5])
    def test_subclass_that_overrides_eq(self):
        # While we cannot guarantee testing functions will always work for
        # subclasses, the tests should ideally rely only on subclasses having
        # comparison operators, not on them being able to store booleans
        # (which, e.g., astropy Quantity cannot usefully do). See gh-8452.
        class MyArray(np.ndarray):
                return bool(np.equal(self, other).all())
        a = np.array([1., 2.]).view(MyArray)
        b = np.array([2., 3.]).view(MyArray)
        assert_(type(a == a), bool)
        assert_(a == a)
        assert_(a != b)
        self._test_equal(a, a)
        self._test_not_equal(b, a)
    def test_subclass_that_does_not_implement_npall(self):
            def __array_function__(self, *args, **kwargs):
            np.all(a)
    def test_suppress_overflow_warnings(self):
        # Based on issue #18992
            with np.errstate(all="raise"):
                np.testing.assert_array_equal(
                    np.array([1, 2, 3], np.float32),
                    np.array([1, 1e-40, 3], np.float32))
    def test_array_vs_scalar_is_equal(self):
        """Test comparing an array with a scalar when all values are equal."""
        a = np.array([1., 1., 1.])
        b = 1.
    def test_array_vs_scalar_not_equal(self):
        """Test comparing an array with a scalar when not all values equal."""
        a = np.array([1., 2., 3.])
    def test_array_vs_scalar_strict(self):
        """Test comparing an array with a scalar with strict option."""
            assert_array_equal(a, b, strict=True)
    def test_array_vs_array_strict(self):
        """Test comparing two arrays with strict option."""
        b = np.array([1., 1., 1.])
    def test_array_vs_float_array_strict(self):
        a = np.array([1, 1, 1])
class TestBuildErrorMessage:
    def test_build_err_msg_defaults(self):
        x = np.array([1.00001, 2.00002, 3.00003])
        y = np.array([1.00002, 2.00003, 3.00004])
        err_msg = 'There is a mismatch'
        a = build_err_msg([x, y], err_msg)
        b = ('\nItems are not equal: There is a mismatch\n ACTUAL: array(['
             '1.00001, 2.00002, 3.00003])\n DESIRED: array([1.00002, '
             '2.00003, 3.00004])')
    def test_build_err_msg_no_verbose(self):
        a = build_err_msg([x, y], err_msg, verbose=False)
        b = '\nItems are not equal: There is a mismatch'
    def test_build_err_msg_custom_names(self):
        a = build_err_msg([x, y], err_msg, names=('FOO', 'BAR'))
        b = ('\nItems are not equal: There is a mismatch\n FOO: array(['
             '1.00001, 2.00002, 3.00003])\n BAR: array([1.00002, 2.00003, '
             '3.00004])')
    def test_build_err_msg_custom_precision(self):
        x = np.array([1.000000001, 2.00002, 3.00003])
        y = np.array([1.000000002, 2.00003, 3.00004])
        a = build_err_msg([x, y], err_msg, precision=10)
             '1.000000001, 2.00002    , 3.00003    ])\n DESIRED: array(['
             '1.000000002, 2.00003    , 3.00004    ])')
class TestEqual(TestArrayEqual):
        self._assert_func = assert_equal
    def test_nan_items(self):
        self._assert_func(np.nan, np.nan)
        self._assert_func([np.nan], [np.nan])
        self._test_not_equal(np.nan, [np.nan])
        self._test_not_equal(np.nan, 1)
    def test_inf_items(self):
        self._assert_func(np.inf, np.inf)
        self._assert_func([np.inf], [np.inf])
        self._test_not_equal(np.inf, [np.inf])
    def test_datetime(self):
        self._test_equal(
            np.datetime64("2017-01-01", "s"),
            np.datetime64("2017-01-01", "s")
            np.datetime64("2017-01-01", "m")
        # gh-10081
        self._test_not_equal(
            np.datetime64("2017-01-02", "s")
            np.datetime64("2017-01-02", "m")
    def test_nat_items(self):
        # not a datetime
        nadt_no_unit = np.datetime64("NaT")
        nadt_s = np.datetime64("NaT", "s")
        nadt_d = np.datetime64("NaT", "ns")
        # not a timedelta
        natd_no_unit = np.timedelta64("NaT")
        natd_s = np.timedelta64("NaT", "s")
        natd_d = np.timedelta64("NaT", "ns")
        dts = [nadt_no_unit, nadt_s, nadt_d]
        tds = [natd_no_unit, natd_s, natd_d]
        for a, b in itertools.product(dts, dts):
            self._assert_func([a], [b])
            self._test_not_equal([a], b)
        for a, b in itertools.product(tds, tds):
        for a, b in itertools.product(tds, dts):
            self._test_not_equal(a, [b])
            self._test_not_equal([a], [b])
            self._test_not_equal([a], np.datetime64("2017-01-01", "s"))
            self._test_not_equal([b], np.datetime64("2017-01-01", "s"))
            self._test_not_equal([a], np.timedelta64(123, "s"))
            self._test_not_equal([b], np.timedelta64(123, "s"))
    def test_non_numeric(self):
        self._assert_func('ab', 'ab')
        self._test_not_equal('ab', 'abb')
    def test_complex_item(self):
        self._assert_func(complex(1, 2), complex(1, 2))
        self._assert_func(complex(1, np.nan), complex(1, np.nan))
        self._test_not_equal(complex(1, np.nan), complex(1, 2))
        self._test_not_equal(complex(np.nan, 1), complex(1, np.nan))
        self._test_not_equal(complex(np.nan, np.inf), complex(np.nan, 2))
    def test_negative_zero(self):
        self._test_not_equal(np.PZERO, np.NZERO)
    def test_complex(self):
        x = np.array([complex(1, 2), complex(1, np.nan)])
        y = np.array([complex(1, 2), complex(1, 2)])
        self._assert_func(x, x)
        self._test_not_equal(x, y)
    def test_object(self):
        #gh-12942
        a = np.array([datetime.datetime(2000, 1, 1),
                      datetime.datetime(2000, 1, 2)])
        self._test_not_equal(a, a[::-1])
class TestArrayAlmostEqual(_GenericTest):
        self._assert_func = assert_array_almost_equal
    def test_closeness(self):
        # Note that in the course of time we ended up with
        #     `abs(x - y) < 1.5 * 10**(-decimal)`
        # instead of the previously documented
        #     `abs(x - y) < 0.5 * 10**(-decimal)`
        # so this check serves to preserve the wrongness.
        # test scalars
        self._assert_func(1.499999, 0.0, decimal=0)
        assert_raises(AssertionError,
                          lambda: self._assert_func(1.5, 0.0, decimal=0))
        # test arrays
        self._assert_func([1.499999], [0.0], decimal=0)
                          lambda: self._assert_func([1.5], [0.0], decimal=0))
    def test_simple(self):
        x = np.array([1234.2222])
        y = np.array([1234.2223])
        self._assert_func(x, y, decimal=3)
        self._assert_func(x, y, decimal=4)
                lambda: self._assert_func(x, y, decimal=5))
    def test_nan(self):
        anan = np.array([np.nan])
        aone = np.array([1])
        ainf = np.array([np.inf])
        self._assert_func(anan, anan)
                lambda: self._assert_func(anan, aone))
                lambda: self._assert_func(anan, ainf))
                lambda: self._assert_func(ainf, anan))
    def test_inf(self):
        a = np.array([[1., 2.], [3., 4.]])
        a[0, 0] = np.inf
                lambda: self._assert_func(a, b))
        b[0, 0] = -np.inf
    def test_subclass(self):
        b = np.ma.masked_array([[1., 2.], [0., 4.]],
                               [[False, False], [True, False]])
        self._assert_func(b, a)
        self._assert_func(b, b)
        # Test fully masked as well (see gh-11123).
        a = np.ma.MaskedArray(3.5, mask=True)
        b = np.array([3., 4., 6.5])
        a = np.ma.masked
        a = np.ma.MaskedArray([3., 4., 6.5], mask=[True, True, True])
        b = np.array([1., 2., 3.])
        b = np.array(1.)
    def test_subclass_that_cannot_be_bool(self):
                return super().__eq__(other).view(np.ndarray)
                return super().__lt__(other).view(np.ndarray)
            def all(self, *args, **kwargs):
        self._assert_func(a, a)
class TestAlmostEqual(_GenericTest):
        self._assert_func = assert_almost_equal
    def test_nan_item(self):
                      lambda: self._assert_func(np.nan, 1))
                      lambda: self._assert_func(np.nan, np.inf))
                      lambda: self._assert_func(np.inf, np.nan))
    def test_inf_item(self):
        self._assert_func(-np.inf, -np.inf)
                      lambda: self._assert_func(np.inf, 1))
                      lambda: self._assert_func(-np.inf, np.inf))
    def test_simple_item(self):
        self._test_not_equal(1, 2)
        self._assert_func(complex(np.inf, np.nan), complex(np.inf, np.nan))
        z = np.array([complex(1, 2), complex(np.nan, 1)])
        self._test_not_equal(x, z)
    def test_error_message(self):
        """Check the message is formatted correctly for the decimal value.
           Also check the message when input includes inf or nan (gh12200)"""
        x = np.array([1.00000000001, 2.00000000002, 3.00003])
        y = np.array([1.00000000002, 2.00000000003, 3.00004])
        # Test with a different amount of decimal digits
        with pytest.raises(AssertionError) as exc_info:
            self._assert_func(x, y, decimal=12)
        msgs = str(exc_info.value).split('\n')
        assert_equal(msgs[3], 'Mismatched elements: 3 / 3 (100%)')
        assert_equal(msgs[4], 'Max absolute difference: 1.e-05')
        assert_equal(msgs[5], 'Max relative difference: 3.33328889e-06')
        assert_equal(
            msgs[6],
            ' x: array([1.00000000001, 2.00000000002, 3.00003      ])')
            msgs[7],
            ' y: array([1.00000000002, 2.00000000003, 3.00004      ])')
        # With the default value of decimal digits, only the 3rd element
        # differs. Note that we only check for the formatting of the arrays
        # themselves.
        assert_equal(msgs[3], 'Mismatched elements: 1 / 3 (33.3%)')
        assert_equal(msgs[6], ' x: array([1.     , 2.     , 3.00003])')
        assert_equal(msgs[7], ' y: array([1.     , 2.     , 3.00004])')
        # Check the error message when input includes inf
        x = np.array([np.inf, 0])
        y = np.array([np.inf, 1])
        assert_equal(msgs[3], 'Mismatched elements: 1 / 2 (50%)')
        assert_equal(msgs[4], 'Max absolute difference: 1.')
        assert_equal(msgs[5], 'Max relative difference: 1.')
        assert_equal(msgs[6], ' x: array([inf,  0.])')
        assert_equal(msgs[7], ' y: array([inf,  1.])')
        # Check the error message when dividing by zero
        x = np.array([1, 2])
        y = np.array([0, 0])
        assert_equal(msgs[3], 'Mismatched elements: 2 / 2 (100%)')
        assert_equal(msgs[4], 'Max absolute difference: 2')
        assert_equal(msgs[5], 'Max relative difference: inf')
    def test_error_message_2(self):
        """Check the message is formatted correctly when either x or y is a scalar."""
        x = 2
        y = np.ones(20)
        assert_equal(msgs[3], 'Mismatched elements: 20 / 20 (100%)')
        y = 2
        x = np.ones(20)
        assert_equal(msgs[5], 'Max relative difference: 0.5')
class TestApproxEqual:
        self._assert_func = assert_approx_equal
    def test_simple_0d_arrays(self):
        x = np.array(1234.22)
        y = np.array(1234.23)
        self._assert_func(x, y, significant=5)
        self._assert_func(x, y, significant=6)
                      lambda: self._assert_func(x, y, significant=7))
    def test_simple_items(self):
        x = 1234.22
        y = 1234.23
        self._assert_func(x, y, significant=4)
        anan = np.array(np.nan)
        aone = np.array(1)
        ainf = np.array(np.inf)
        assert_raises(AssertionError, lambda: self._assert_func(anan, aone))
        assert_raises(AssertionError, lambda: self._assert_func(anan, ainf))
        assert_raises(AssertionError, lambda: self._assert_func(ainf, anan))
class TestArrayAssertLess:
        self._assert_func = assert_array_less
    def test_simple_arrays(self):
        x = np.array([1.1, 2.2])
        y = np.array([1.2, 2.3])
        assert_raises(AssertionError, lambda: self._assert_func(y, x))
        y = np.array([1.0, 2.3])
        assert_raises(AssertionError, lambda: self._assert_func(x, y))
    def test_rank2(self):
        x = np.array([[1.1, 2.2], [3.3, 4.4]])
        y = np.array([[1.2, 2.3], [3.4, 4.5]])
        y = np.array([[1.0, 2.3], [3.4, 4.5]])
    def test_rank3(self):
        x = np.ones(shape=(2, 2, 2))
        y = np.ones(shape=(2, 2, 2))+1
        y[0, 0, 0] = 0
        x = 1.1
        y = 2.2
        y = np.array([2.2, 3.3])
        y = np.array([1.0, 3.3])
    def test_nan_noncompare(self):
        assert_raises(AssertionError, lambda: self._assert_func(aone, anan))
    def test_nan_noncompare_array(self):
        x = np.array([1.1, 2.2, 3.3])
        assert_raises(AssertionError, lambda: self._assert_func(x, anan))
        assert_raises(AssertionError, lambda: self._assert_func(anan, x))
        x = np.array([1.1, 2.2, np.nan])
        y = np.array([1.0, 2.0, np.nan])
        self._assert_func(y, x)
    def test_inf_compare(self):
        self._assert_func(aone, ainf)
        self._assert_func(-ainf, aone)
        self._assert_func(-ainf, ainf)
        assert_raises(AssertionError, lambda: self._assert_func(ainf, aone))
        assert_raises(AssertionError, lambda: self._assert_func(aone, -ainf))
        assert_raises(AssertionError, lambda: self._assert_func(ainf, ainf))
        assert_raises(AssertionError, lambda: self._assert_func(ainf, -ainf))
        assert_raises(AssertionError, lambda: self._assert_func(-ainf, -ainf))
    def test_inf_compare_array(self):
        x = np.array([1.1, 2.2, np.inf])
        assert_raises(AssertionError, lambda: self._assert_func(x, ainf))
        assert_raises(AssertionError, lambda: self._assert_func(ainf, x))
        assert_raises(AssertionError, lambda: self._assert_func(x, -ainf))
        assert_raises(AssertionError, lambda: self._assert_func(-x, -ainf))
        assert_raises(AssertionError, lambda: self._assert_func(-ainf, -x))
        self._assert_func(-ainf, x)
class TestWarns:
    def test_warn(self):
        def f():
            warnings.warn("yo")
        before_filters = sys.modules['warnings'].filters[:]
        assert_equal(assert_warns(UserWarning, f), 3)
        after_filters = sys.modules['warnings'].filters
        assert_raises(AssertionError, assert_no_warnings, f)
        assert_equal(assert_no_warnings(lambda x: x, 1), 1)
        # Check that the warnings state is unchanged
        assert_equal(before_filters, after_filters,
                     "assert_warns does not preserver warnings state")
    def test_context_manager(self):
        with assert_warns(UserWarning):
        def no_warnings():
            with assert_no_warnings():
        assert_raises(AssertionError, no_warnings)
    def test_warn_wrong_warning(self):
            warnings.warn("yo", DeprecationWarning)
        failed = False
            warnings.simplefilter("error", DeprecationWarning)
                # Should raise a DeprecationWarning
                assert_warns(UserWarning, f)
                failed = True
            except DeprecationWarning:
            raise AssertionError("wrong warning caught by assert_warn")
class TestAssertAllclose:
        x = 1e-3
        y = 1e-9
        assert_allclose(x, y, atol=1)
        assert_raises(AssertionError, assert_allclose, x, y)
        a = np.array([x, y, x, y])
        b = np.array([x, y, x, x])
        assert_allclose(a, b, atol=1)
        assert_raises(AssertionError, assert_allclose, a, b)
        b[-1] = y * (1 + 1e-8)
        assert_allclose(a, b)
        assert_raises(AssertionError, assert_allclose, a, b, rtol=1e-9)
        assert_allclose(6, 10, rtol=0.5)
        assert_raises(AssertionError, assert_allclose, 10, 6, rtol=0.5)
    def test_min_int(self):
        a = np.array([np.iinfo(np.int_).min], dtype=np.int_)
        # Should not raise:
        assert_allclose(a, a)
    def test_report_fail_percentage(self):
        a = np.array([1, 1, 1, 1])
        b = np.array([1, 1, 1, 2])
        msg = str(exc_info.value)
        assert_('Mismatched elements: 1 / 4 (25%)\n'
                'Max absolute difference: 1\n'
                'Max relative difference: 0.5' in msg)
    def test_equal_nan(self):
        a = np.array([np.nan])
        b = np.array([np.nan])
        assert_allclose(a, b, equal_nan=True)
    def test_not_equal_nan(self):
        assert_raises(AssertionError, assert_allclose, a, b, equal_nan=False)
    def test_equal_nan_default(self):
        # Make sure equal_nan default behavior remains unchanged. (All
        # of these functions use assert_array_compare under the hood.)
        # None of these should raise.
        assert_array_equal(a, b)
        assert_array_almost_equal(a, b)
        assert_array_less(a, b)
    def test_report_max_relative_error(self):
        a = np.array([0, 1])
        b = np.array([0, 2])
        assert_('Max relative difference: 0.5' in msg)
        # see gh-18286
        a = np.array([[1, 2, 3, "NaT"]], dtype="m8[ns]")
    def test_error_message_unsigned(self):
        """Check the the message is formatted correctly when overflow can occur
           (gh21768)"""
        # Ensure to test for potential overflow in the case of:
        #        x - y
        #        y - x
        x = np.asarray([0, 1, 8], dtype='uint8')
        y = np.asarray([4, 4, 4], dtype='uint8')
            assert_allclose(x, y, atol=3)
        assert_equal(msgs[4], 'Max absolute difference: 4')
class TestArrayAlmostEqualNulp:
    def test_float64_pass(self):
        # The number of units of least precision
        # In this case, use a few places above the lowest level (ie nulp=1)
        nulp = 5
        x = np.linspace(-20, 20, 50, dtype=np.float64)
        x = 10**x
        x = np.r_[-x, x]
        # Addition
        eps = np.finfo(x.dtype).eps
        y = x + x*eps*nulp/2.
        assert_array_almost_equal_nulp(x, y, nulp)
        # Subtraction
        epsneg = np.finfo(x.dtype).epsneg
        y = x - x*epsneg*nulp/2.
    def test_float64_fail(self):
        y = x + x*eps*nulp*2.
        assert_raises(AssertionError, assert_array_almost_equal_nulp,
                      x, y, nulp)
        y = x - x*epsneg*nulp*2.
    def test_float64_ignore_nan(self):
        # Ignore ULP differences between various NAN's
        # Note that MIPS may reverse quiet and signaling nans
        # so we use the builtin version as a base.
        offset = np.uint64(0xffffffff)
        nan1_i64 = np.array(np.nan, dtype=np.float64).view(np.uint64)
        nan2_i64 = nan1_i64 ^ offset  # nan payload on MIPS is all ones.
        nan1_f64 = nan1_i64.view(np.float64)
        nan2_f64 = nan2_i64.view(np.float64)
        assert_array_max_ulp(nan1_f64, nan2_f64, 0)
    def test_float32_pass(self):
        x = np.linspace(-20, 20, 50, dtype=np.float32)
    def test_float32_fail(self):
    def test_float32_ignore_nan(self):
        offset = np.uint32(0xffff)
        nan1_i32 = np.array(np.nan, dtype=np.float32).view(np.uint32)
        nan2_i32 = nan1_i32 ^ offset  # nan payload on MIPS is all ones.
        nan1_f32 = nan1_i32.view(np.float32)
        nan2_f32 = nan2_i32.view(np.float32)
        assert_array_max_ulp(nan1_f32, nan2_f32, 0)
    def test_float16_pass(self):
        x = np.linspace(-4, 4, 10, dtype=np.float16)
    def test_float16_fail(self):
    def test_float16_ignore_nan(self):
        offset = np.uint16(0xff)
        nan1_i16 = np.array(np.nan, dtype=np.float16).view(np.uint16)
        nan2_i16 = nan1_i16 ^ offset  # nan payload on MIPS is all ones.
        nan1_f16 = nan1_i16.view(np.float16)
        nan2_f16 = nan2_i16.view(np.float16)
        assert_array_max_ulp(nan1_f16, nan2_f16, 0)
    def test_complex128_pass(self):
        xi = x + x*1j
        assert_array_almost_equal_nulp(xi, x + y*1j, nulp)
        assert_array_almost_equal_nulp(xi, y + x*1j, nulp)
        # The test condition needs to be at least a factor of sqrt(2) smaller
        # because the real and imaginary parts both change
        y = x + x*eps*nulp/4.
        assert_array_almost_equal_nulp(xi, y + y*1j, nulp)
        y = x - x*epsneg*nulp/4.
    def test_complex128_fail(self):
                      xi, x + y*1j, nulp)
                      xi, y + x*1j, nulp)
        y = x + x*eps*nulp
                      xi, y + y*1j, nulp)
        y = x - x*epsneg*nulp
    def test_complex64_pass(self):
    def test_complex64_fail(self):
class TestULP:
    def test_equal(self):
        x = np.random.randn(10)
        assert_array_max_ulp(x, x, maxulp=0)
    def test_single(self):
        # Generate 1 + small deviation, check that adding eps gives a few UNL
        x = np.ones(10).astype(np.float32)
        x += 0.01 * np.random.randn(10).astype(np.float32)
        eps = np.finfo(np.float32).eps
        assert_array_max_ulp(x, x+eps, maxulp=20)
    def test_double(self):
        x = np.ones(10).astype(np.float64)
        x += 0.01 * np.random.randn(10).astype(np.float64)
        eps = np.finfo(np.float64).eps
        assert_array_max_ulp(x, x+eps, maxulp=200)
        for dt in [np.float32, np.float64]:
            inf = np.array([np.inf]).astype(dt)
            big = np.array([np.finfo(dt).max])
            assert_array_max_ulp(inf, big, maxulp=200)
        # Test that nan is 'far' from small, tiny, inf, max and min
            if dt == np.float32:
                maxulp = 1e6
                maxulp = 1e12
            nan = np.array([np.nan]).astype(dt)
            tiny = np.array([np.finfo(dt).tiny])
            zero = np.array([np.PZERO]).astype(dt)
            nzero = np.array([np.NZERO]).astype(dt)
                          lambda: assert_array_max_ulp(nan, inf,
                          maxulp=maxulp))
                          lambda: assert_array_max_ulp(nan, big,
                          lambda: assert_array_max_ulp(nan, tiny,
                          lambda: assert_array_max_ulp(nan, zero,
                          lambda: assert_array_max_ulp(nan, nzero,
class TestStringEqual:
        assert_string_equal("hello", "hello")
        assert_string_equal("hello\nmultiline", "hello\nmultiline")
            assert_string_equal("foo\nbar", "hello\nbar")
        assert_equal(msg, "Differences in strings:\n- foo\n+ hello")
                      lambda: assert_string_equal("foo", "hello"))
    def test_regex(self):
        assert_string_equal("a+*b", "a+*b")
                      lambda: assert_string_equal("aaa", "a+b"))
def assert_warn_len_equal(mod, n_in_context):
        mod_warns = mod.__warningregistry__
        # the lack of a __warningregistry__
        # attribute means that no warning has
        # occurred; this can be triggered in
        # a parallel test scenario, while in
        # a serial test scenario an initial
        # warning (and therefore the attribute)
        # are always created first
        mod_warns = {}
    num_warns = len(mod_warns)
    if 'version' in mod_warns:
        # Python 3 adds a 'version' entry to the registry,
        # do not count it.
        num_warns -= 1
    assert_equal(num_warns, n_in_context)
def test_warn_len_equal_call_scenarios():
    # assert_warn_len_equal is called under
    # varying circumstances depending on serial
    # vs. parallel test scenarios; this test
    # simply aims to probe both code paths and
    # check that no assertion is uncaught
    # parallel scenario -- no warning issued yet
    class mod:
    mod_inst = mod()
    assert_warn_len_equal(mod=mod_inst,
                          n_in_context=0)
    # serial test scenario -- the __warningregistry__
    # attribute should be present
            self.__warningregistry__ = {'warning1':1,
                                        'warning2':2}
                          n_in_context=2)
def _get_fresh_mod():
    # Get this module, with warning registry empty
    my_mod = sys.modules[__name__]
        my_mod.__warningregistry__.clear()
        # will not have a __warningregistry__ unless warning has been
        # raised in the module at some point
    return my_mod
def test_clear_and_catch_warnings():
    # Initial state of module, no warnings
    my_mod = _get_fresh_mod()
    assert_equal(getattr(my_mod, '__warningregistry__', {}), {})
    with clear_and_catch_warnings(modules=[my_mod]):
        warnings.warn('Some warning')
    assert_equal(my_mod.__warningregistry__, {})
    # Without specified modules, don't clear warnings during context.
    # catch_warnings doesn't make an entry for 'ignore'.
    with clear_and_catch_warnings():
    assert_warn_len_equal(my_mod, 0)
    # Manually adding two warnings to the registry:
    my_mod.__warningregistry__ = {'warning1': 1,
                                  'warning2': 2}
    # Confirm that specifying module keeps old warning, does not add new
        warnings.warn('Another warning')
    assert_warn_len_equal(my_mod, 2)
    # Another warning, no module spec it clears up registry
def test_suppress_warnings_module():
    def warn_other_module():
        # Apply along axis is implemented in python; stacklevel=2 means
        # we end up inside its module, not ours.
        def warn(arr):
            warnings.warn("Some warning 2", stacklevel=2)
            return arr
        np.apply_along_axis(warn, 0, [0])
    # Test module based warning suppression:
    with suppress_warnings() as sup:
        sup.record(UserWarning)
        # suppress warning from other module (may have .pyc ending),
        # if apply_along_axis is moved, had to be changed.
        sup.filter(module=np.lib.shape_base)
        warnings.warn("Some warning")
        warn_other_module()
    # Check that the suppression did test the file correctly (this module
    # got filtered)
    assert_equal(len(sup.log), 1)
    assert_equal(sup.log[0].message.args[0], "Some warning")
    sup = suppress_warnings()
    # Will have to be changed if apply_along_axis is moved:
    sup.filter(module=my_mod)
    with sup:
    # And test repeat works:
    # Without specified modules
    with suppress_warnings():
def test_suppress_warnings_type():
        sup.filter(UserWarning)
def test_suppress_warnings_decorate_no_record():
    @sup
    def warn(category):
        warnings.warn('Some warning', category)
        warnings.simplefilter("always")
        warn(UserWarning)  # should be supppressed
        warn(RuntimeWarning)
        assert_equal(len(w), 1)
def test_suppress_warnings_record():
    log1 = sup.record()
        log2 = sup.record(message='Some other warning 2')
        sup.filter(message='Some warning')
        warnings.warn('Some other warning')
        warnings.warn('Some other warning 2')
        assert_equal(len(sup.log), 2)
        assert_equal(len(log1), 1)
        assert_equal(len(log2),1)
        assert_equal(log2[0].message.args[0], 'Some other warning 2')
    # Do it again, with the same context to see if some warnings survived:
        assert_equal(len(log2), 1)
    # Test nested:
        sup.record()
        with suppress_warnings() as sup2:
            sup2.record(message='Some warning')
            assert_equal(len(sup2.log), 1)
def test_suppress_warnings_forwarding():
            warnings.warn("Some warning", stacklevel=2)
        with suppress_warnings("always"):
            for i in range(2):
        with suppress_warnings("location"):
        with suppress_warnings("module"):
        with suppress_warnings("once"):
                warnings.warn("Some other warning")
def test_tempdir():
    with tempdir() as tdir:
        fpath = os.path.join(tdir, 'tmp')
        with open(fpath, 'w'):
    assert_(not os.path.isdir(tdir))
    raised = False
        raised = True
    assert_(raised)
def test_temppath():
    with temppath() as fpath:
    assert_(not os.path.isfile(fpath))
class my_cacw(clear_and_catch_warnings):
    class_modules = (sys.modules[__name__],)
def test_clear_and_catch_warnings_inherit():
    # Test can subclass and add default modules
    with my_cacw():
@pytest.mark.skipif(not HAS_REFCOUNT, reason="Python lacks refcounts")
class TestAssertNoGcCycles:
    """ Test assert_no_gc_cycles """
    def test_passes(self):
        def no_cycle():
            b = []
            b.append([])
        with assert_no_gc_cycles():
            no_cycle()
        assert_no_gc_cycles(no_cycle)
    def test_asserts(self):
        def make_cycle():
            a = []
            a.append(a)
                make_cycle()
            assert_no_gc_cycles(make_cycle)
    @pytest.mark.slow
    def test_fails(self):
        Test that in cases where the garbage cannot be collected, we raise an
        error, instead of hanging forever trying to clear it.
        class ReferenceCycleInDel:
            An object that not only contains a reference cycle, but creates new
            cycles whenever it's garbage-collected and its __del__ runs
            make_cycle = True
                self.cycle = self
                # break the current cycle so that `self` can be freed
                self.cycle = None
                if ReferenceCycleInDel.make_cycle:
                    # but create a new one so that the garbage collector has more
                    # work to do.
                    ReferenceCycleInDel()
            w = weakref.ref(ReferenceCycleInDel())
                with assert_raises(RuntimeError):
                    # this will be unable to get a baseline empty garbage
                    assert_no_gc_cycles(lambda: None)
                # the above test is only necessary if the GC actually tried to free
                # our object anyway, which python 2.7 does not.
                if w() is not None:
                    pytest.skip("GC does not call __del__ on cyclic objects")
            # make sure that we stop creating reference cycles
            ReferenceCycleInDel.make_cycle = False
"""Unit tests for the :mod:`networkx.algorithms.community.utils` module."""
def test_is_partition():
    G = nx.empty_graph(3)
    assert nx.community.is_partition(G, [{0, 1}, {2}])
    assert nx.community.is_partition(G, ({0, 1}, {2}))
    assert nx.community.is_partition(G, ([0, 1], [2]))
    assert nx.community.is_partition(G, [[0, 1], [2]])
def test_not_covering():
    assert not nx.community.is_partition(G, [{0}, {1}])
def test_not_disjoint():
    assert not nx.community.is_partition(G, [{0, 1}, {1, 2}])
def test_not_node():
    assert not nx.community.is_partition(G, [{0, 1}, {3}])
import scipy.sparse.linalg._isolve.utils as utils
def test_make_system_bad_shape():
    assert_raises(ValueError,
                  utils.make_system, np.zeros((5,3)), None, np.zeros(4), np.zeros(4))
from datetime import date, datetime, timedelta
from posthog import utils
from posthog.types import FeatureFlagResult
TEST_API_KEY = "kOOlRy2QlMY9jHZQv0bKz0FZyazBUoY8Arj0lFVNjs4"
FAKE_TEST_API_KEY = "random_key"
class TestUtils(unittest.TestCase):
            ("naive datetime should be naive", True),
            ("timezone-aware datetime should not be naive", False),
    def test_is_naive(self, _name: str, expected_naive: bool):
        if expected_naive:
            dt = datetime.now()  # naive datetime
            dt = datetime.now(tz=tzutc())  # timezone-aware datetime
        assert utils.is_naive(dt) is expected_naive
    def test_timezone_utils(self):
        now = datetime.now()
        utcnow = datetime.now(tz=tzutc())
        fixed = utils.guess_timezone(now)
        assert utils.is_naive(fixed) is False
        shouldnt_be_edited = utils.guess_timezone(utcnow)
        assert utcnow == shouldnt_be_edited
    def test_clean(self):
        simple = {
            "decimal": Decimal("0.142857"),
            "unicode": six.u("woo"),
            "date": datetime.now(),
            "long": 200000000,
            "integer": 1,
            "float": 2.0,
            "bool": True,
            "str": "woo",
            "none": None,
        complicated = {
            "exception": Exception("This should show up"),
            "timedelta": timedelta(microseconds=20),
            "list": [1, 2, 3],
        combined = dict(simple.items())
        combined.update(complicated.items())
        pre_clean_keys = combined.keys()
        utils.clean(combined)
        assert combined.keys() == pre_clean_keys
        # test UUID separately, as the UUID object doesn't equal its string representation according to Python
            utils.clean(UUID("12345678123456781234567812345678"))
            == "12345678-1234-5678-1234-567812345678"
    def test_clean_with_dates(self):
        dict_with_dates = {
            "birthdate": date(1980, 1, 1),
            "registration": datetime.now(tz=tzutc()),
        assert dict_with_dates == utils.clean(dict_with_dates)
    def test_bytes(self):
        item = bytes(10)
        utils.clean(item)
        assert utils.clean(item) == "\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"
    def test_clean_fn(self):
        cleaned = utils.clean({"fn": lambda x: x, "number": 4})
        assert cleaned == {"fn": None, "number": 4}
            ("http://posthog.io/", "http://posthog.io"),
            ("http://posthog.io", "http://posthog.io"),
            ("https://example.com/path/", "https://example.com/path"),
            ("https://example.com/path", "https://example.com/path"),
    def test_remove_slash(self, input_url, expected_url):
        assert expected_url == utils.remove_trailing_slash(input_url)
    def test_clean_pydantic(self):
        class ModelV2(BaseModel):
            baz: Optional[str] = None
        class ModelV1(BaseModelV1):
            foo: ModelV2
        assert utils.clean(ModelV2(foo="1", bar=2)) == {
            "foo": "1",
            "bar": 2,
            "baz": None,
        # Pydantic V1 is not compatible with Python 3.14+
            assert utils.clean(ModelV1(foo=1, bar="2")) == {"foo": 1, "bar": "2"}
        assert utils.clean(NestedModel(foo=ModelV2(foo="1", bar=2, baz="3"))) == {
            "foo": {"foo": "1", "bar": 2, "baz": "3"}
    def test_clean_pydantic_like_class(self) -> None:
        class Dummy:
            def model_dump(self, required_param: str) -> dict:
        # previously python 2 code would cause an error while cleaning,
        # and this entire object would be None, and we would log an error
        # let's allow ourselves to clean `Dummy` as None,
        # without blatting the `test` key
        assert utils.clean({"test": Dummy()}) == {"test": None}
    def test_clean_dataclass(self):
        class InnerDataClass:
            inner_foo: str
            inner_bar: int
            inner_uuid: UUID
            inner_date: datetime
            inner_optional: Optional[str] = None
        class TestDataClass:
            nested: InnerDataClass
        assert utils.clean(
            TestDataClass(
                foo="1",
                bar=2,
                nested=InnerDataClass(
                    inner_foo="3",
                    inner_bar=4,
                    inner_uuid=UUID("12345678123456781234567812345678"),
                    inner_date=datetime(2025, 1, 1),
        ) == {
            "nested": {
                "inner_foo": "3",
                "inner_bar": 4,
                "inner_uuid": "12345678-1234-5678-1234-567812345678",
                "inner_date": datetime(2025, 1, 1),
                "inner_optional": None,
class TestFlagCache(unittest.TestCase):
        self.cache = utils.FlagCache(max_size=3, default_ttl=1)
        self.flag_result = FeatureFlagResult.from_value_and_payload(
            "test-flag", True, None
    def test_cache_basic_operations(self):
        distinct_id = "user123"
        flag_key = "test-flag"
        flag_version = 1
        # Test cache miss
        result = self.cache.get_cached_flag(distinct_id, flag_key, flag_version)
        # Test cache set and hit
        self.cache.set_cached_flag(
            distinct_id, flag_key, self.flag_result, flag_version
        assert result is not None
        assert result.get_value()
    def test_cache_ttl_expiration(self):
        # Set flag in cache
        # Should be available immediately
        # Wait for TTL to expire (1 second + buffer)
        time.sleep(1.1)
        # Should be expired
    def test_cache_version_invalidation(self):
        old_version = 1
        new_version = 2
        # Set flag with old version
        self.cache.set_cached_flag(distinct_id, flag_key, self.flag_result, old_version)
        # Should hit with old version
        result = self.cache.get_cached_flag(distinct_id, flag_key, old_version)
        # Should miss with new version
        result = self.cache.get_cached_flag(distinct_id, flag_key, new_version)
        # Invalidate old version
        self.cache.invalidate_version(old_version)
        # Should miss even with old version after invalidation
    def test_stale_cache_functionality(self):
        # Wait for TTL to expire
        # Should not get fresh cache
        # Should get stale cache (within 1 hour default)
        stale_result = self.cache.get_stale_cached_flag(distinct_id, flag_key)
        assert stale_result is not None
        assert stale_result.get_value()
    def test_lru_eviction(self):
        # Cache has max_size=3, so adding 4 users should evict the LRU one
        # Add 3 users
        for i in range(3):
            user_id = f"user{i}"
                user_id, "test-flag", self.flag_result, flag_version
        # Access user0 to make it recently used
        self.cache.get_cached_flag("user0", "test-flag", flag_version)
        # Add 4th user, should evict user1 (least recently used)
        self.cache.set_cached_flag("user3", "test-flag", self.flag_result, flag_version)
        # user0 should still be there (was recently accessed)
        result = self.cache.get_cached_flag("user0", "test-flag", flag_version)
        # user2 should still be there (was recently added)
        result = self.cache.get_cached_flag("user2", "test-flag", flag_version)
        # user3 should be there (just added)
        result = self.cache.get_cached_flag("user3", "test-flag", flag_version)
"""Utilities shared by tests."""
from unittest import IsolatedAsyncioTestCase, mock
from aiosignal import Signal
from multidict import CIMultiDict, CIMultiDictProxy
from aiohttp.client import (
    _RequestContextManager,
    _RequestOptions,
    _WSRequestContextManager,
from . import ClientSession, hdrs
from .client_reqrep import ClientResponse
from .client_ws import ClientWebSocketResponse
from .helpers import sentinel
from .http import HttpVersion, RawRequestMessage
from .streams import EMPTY_PAYLOAD, StreamReader
from .typedefs import StrOrURL
from .web import (
    Application,
    AppRunner,
    BaseRequest,
    BaseRunner,
    ServerRunner,
    SockSite,
    UrlMappingMatchInfo,
from .web_protocol import _RequestHandler
    from typing import Self
    Self = Any
_ApplicationNone = TypeVar("_ApplicationNone", Application, None)
_Request = TypeVar("_Request", bound=BaseRequest)
REUSE_ADDRESS = os.name == "posix" and sys.platform != "cygwin"
def get_unused_port_socket(
    host: str, family: socket.AddressFamily = socket.AF_INET
) -> socket.socket:
    return get_port_socket(host, 0, family)
def get_port_socket(
    host: str, port: int, family: socket.AddressFamily
    s = socket.socket(family, socket.SOCK_STREAM)
    if REUSE_ADDRESS:
        # Windows has different semantics for SO_REUSEADDR,
        # so don't set it. Ref:
        # https://docs.microsoft.com/en-us/windows/win32/winsock/using-so-reuseaddr-and-so-exclusiveaddruse
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((host, port))
def unused_port() -> int:
    """Return a port that is unused on the current host."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(("127.0.0.1", 0))
        return cast(int, s.getsockname()[1])
class BaseTestServer(ABC):
        scheme: str = "",
        host: str = "127.0.0.1",
        port: Optional[int] = None,
        skip_url_asserts: bool = False,
        socket_factory: Callable[
            [str, int, socket.AddressFamily], socket.socket
        ] = get_port_socket,
        self.runner: Optional[BaseRunner] = None
        self._root: Optional[URL] = None
        self.skip_url_asserts = skip_url_asserts
        self.socket_factory = socket_factory
    async def start_server(
        self, loop: Optional[asyncio.AbstractEventLoop] = None, **kwargs: Any
        if self.runner:
        self._ssl = kwargs.pop("ssl", None)
        self.runner = await self._make_runner(handler_cancellation=True, **kwargs)
        await self.runner.setup()
        if not self.port:
            self.port = 0
        absolute_host = self.host
            version = ipaddress.ip_address(self.host).version
            version = 4
        if version == 6:
            absolute_host = f"[{self.host}]"
        family = socket.AF_INET6 if version == 6 else socket.AF_INET
        _sock = self.socket_factory(self.host, self.port, family)
        self.host, self.port = _sock.getsockname()[:2]
        site = SockSite(self.runner, sock=_sock, ssl_context=self._ssl)
        await site.start()
        server = site._server
        assert server is not None
        sockets = server.sockets  # type: ignore[attr-defined]
        assert sockets is not None
        self.port = sockets[0].getsockname()[1]
        if not self.scheme:
            self.scheme = "https" if self._ssl else "http"
        self._root = URL(f"{self.scheme}://{absolute_host}:{self.port}")
    @abstractmethod  # pragma: no cover
    async def _make_runner(self, **kwargs: Any) -> BaseRunner:
    def make_url(self, path: StrOrURL) -> URL:
        assert self._root is not None
        url = URL(path)
        if not self.skip_url_asserts:
            assert not url.absolute
            return self._root.join(url)
            return URL(str(self._root) + str(path))
    def started(self) -> bool:
        return self.runner is not None
        return self._closed
    def handler(self) -> Server:
        # web.Server instance
        runner = self.runner
        assert runner is not None
        assert runner.server is not None
        return runner.server
        """Close all fixtures created by the test client.
        After that point, the TestClient is no longer usable.
        This is an idempotent function: running close multiple times
        will not have any additional effects.
        close is also run when the object is garbage collected, and on
        exit when used as a context manager.
        if self.started and not self.closed:
            assert self.runner is not None
            await self.runner.cleanup()
            self._root = None
            self.port = None
        exc_value: Optional[BaseException],
        traceback: Optional[TracebackType],
    async def __aenter__(self) -> "BaseTestServer":
        await self.start_server(loop=self._loop)
class TestServer(BaseTestServer):
        app: Application,
        super().__init__(scheme=scheme, host=host, port=port, **kwargs)
        return AppRunner(self.app, **kwargs)
class RawTestServer(BaseTestServer):
        handler: _RequestHandler,
        self._handler = handler
    async def _make_runner(self, debug: bool = True, **kwargs: Any) -> ServerRunner:
        srv = Server(self._handler, loop=self._loop, debug=debug, **kwargs)
        return ServerRunner(srv, debug=debug, **kwargs)
class TestClient(Generic[_Request, _ApplicationNone]):
    A test client implementation.
    To write functional tests for aiohttp based servers.
        self: "TestClient[Request, Application]",
        server: TestServer,
        self: "TestClient[_Request, None]",
        server: BaseTestServer,
        if not isinstance(server, BaseTestServer):
                "server must be TestServer instance, found type: %r" % type(server)
        self._server = server
            cookie_jar = aiohttp.CookieJar(unsafe=True, loop=loop)
        self._session = ClientSession(loop=loop, cookie_jar=cookie_jar, **kwargs)
        self._session._retry_connection = False
        self._responses: List[ClientResponse] = []
        self._websockets: List[ClientWebSocketResponse] = []
    async def start_server(self) -> None:
        await self._server.start_server(loop=self._loop)
    def host(self) -> str:
        return self._server.host
    def port(self) -> Optional[int]:
        return self._server.port
    def server(self) -> BaseTestServer:
        return self._server
    def app(self) -> _ApplicationNone:
        return getattr(self._server, "app", None)  # type: ignore[return-value]
    def session(self) -> ClientSession:
        """An internal aiohttp.ClientSession.
        Unlike the methods on the TestClient, client session requests
        do not automatically include the host in the url queried, and
        will require an absolute path to the resource.
        return self._server.make_url(path)
        self, method: str, path: StrOrURL, **kwargs: Any
        resp = await self._session.request(method, self.make_url(path), **kwargs)
        # save it to close later
        self._responses.append(resp)
            self, method: str, path: StrOrURL, **kwargs: Unpack[_RequestOptions]
        ) -> _RequestContextManager: ...
            path: StrOrURL,
        ) -> _RequestContextManager:
            """Routes a request to tested http server.
            The interface is identical to aiohttp.ClientSession.request,
            except the loop kwarg is overridden by the instance used by the
            test server.
            return _RequestContextManager(self._request(method, path, **kwargs))
        def get(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP GET request."""
            return _RequestContextManager(self._request(hdrs.METH_GET, path, **kwargs))
        def post(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP POST request."""
            return _RequestContextManager(self._request(hdrs.METH_POST, path, **kwargs))
        def options(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP OPTIONS request."""
                self._request(hdrs.METH_OPTIONS, path, **kwargs)
        def head(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP HEAD request."""
            return _RequestContextManager(self._request(hdrs.METH_HEAD, path, **kwargs))
        def put(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP PUT request."""
            return _RequestContextManager(self._request(hdrs.METH_PUT, path, **kwargs))
        def patch(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
            """Perform an HTTP PATCH request."""
                self._request(hdrs.METH_PATCH, path, **kwargs)
        def delete(self, path: StrOrURL, **kwargs: Any) -> _RequestContextManager:
                self._request(hdrs.METH_DELETE, path, **kwargs)
    def ws_connect(self, path: StrOrURL, **kwargs: Any) -> _WSRequestContextManager:
        """Initiate websocket connection.
        The api corresponds to aiohttp.ClientSession.ws_connect.
        return _WSRequestContextManager(self._ws_connect(path, **kwargs))
        self, path: StrOrURL, **kwargs: Any
        ws = await self._session.ws_connect(self.make_url(path), **kwargs)
        self._websockets.append(ws)
        close is also run on exit when used as a(n) (asynchronous)
        context manager.
            for resp in self._responses:
            for ws in self._websockets:
                await ws.close()
            await self._server.close()
        await self.start_server()
class AioHTTPTestCase(IsolatedAsyncioTestCase):
    """A base class to allow for unittest web applications using aiohttp.
    Provides the following:
    * self.client (aiohttp.test_utils.TestClient): an aiohttp test client.
    * self.loop (asyncio.BaseEventLoop): the event loop in which the
        application and server are running.
    * self.app (aiohttp.web.Application): the application returned by
        self.get_application()
    Note that the TestClient's methods are asynchronous: you have to
    execute function on the test client using asynchronous methods.
    async def get_application(self) -> Application:
        """Get application.
        This method should be overridden
        to return the aiohttp.web.Application
        object to test.
        return self.get_app()
    def get_app(self) -> Application:
        """Obsolete method used to constructing web application.
        Use .get_application() coroutine instead.
        raise RuntimeError("Did you forget to define get_application()?")
    async def asyncSetUp(self) -> None:
        self.loop = asyncio.get_running_loop()
        return await self.setUpAsync()
    async def setUpAsync(self) -> None:
        self.app = await self.get_application()
        self.server = await self.get_server(self.app)
        self.client = await self.get_client(self.server)
        await self.client.start_server()
    async def asyncTearDown(self) -> None:
        return await self.tearDownAsync()
    async def tearDownAsync(self) -> None:
        await self.client.close()
    async def get_server(self, app: Application) -> TestServer:
        """Return a TestServer instance."""
        return TestServer(app, loop=self.loop)
    async def get_client(self, server: TestServer) -> TestClient[Request, Application]:
        """Return a TestClient instance."""
        return TestClient(server, loop=self.loop)
def unittest_run_loop(func: Any, *args: Any, **kwargs: Any) -> Any:
    A decorator dedicated to use with asynchronous AioHTTPTestCase test methods.
    In 3.8+, this does nothing.
        "Decorator `@unittest_run_loop` is no longer needed in aiohttp 3.8+",
_LOOP_FACTORY = Callable[[], asyncio.AbstractEventLoop]
def loop_context(
    loop_factory: _LOOP_FACTORY = asyncio.new_event_loop, fast: bool = False
) -> Iterator[asyncio.AbstractEventLoop]:
    """A contextmanager that creates an event_loop, for test purposes.
    Handles the creation and cleanup of a test loop.
    loop = setup_test_loop(loop_factory)
    yield loop
    teardown_test_loop(loop, fast=fast)
def setup_test_loop(
    loop_factory: _LOOP_FACTORY = asyncio.new_event_loop,
) -> asyncio.AbstractEventLoop:
    """Create and return an asyncio.BaseEventLoop instance.
    The caller should also call teardown_test_loop,
    once they are done with the loop.
    loop = loop_factory()
    asyncio.set_event_loop(loop)
    return loop
def teardown_test_loop(loop: asyncio.AbstractEventLoop, fast: bool = False) -> None:
    """Teardown and cleanup an event_loop created by setup_test_loop."""
    closed = loop.is_closed()
    if not closed:
        loop.call_soon(loop.stop)
        loop.run_forever()
        loop.close()
    if not fast:
    asyncio.set_event_loop(None)
def _create_app_mock() -> mock.MagicMock:
    def get_dict(app: Any, key: str) -> Any:
        return app.__app_dict[key]
    def set_dict(app: Any, key: str, value: Any) -> None:
        app.__app_dict[key] = value
    app = mock.MagicMock(spec=Application)
    app.__app_dict = {}
    app.__getitem__ = get_dict
    app.__setitem__ = set_dict
    app._debug = False
    app.on_response_prepare = Signal(app)
    app.on_response_prepare.freeze()
def _create_transport(sslcontext: Optional[SSLContext] = None) -> mock.Mock:
    transport = mock.Mock()
    def get_extra_info(key: str) -> Optional[SSLContext]:
        if key == "sslcontext":
            return sslcontext
    transport.get_extra_info.side_effect = get_extra_info
def make_mocked_request(
    headers: Any = None,
    match_info: Any = sentinel,
    version: HttpVersion = HttpVersion(1, 1),
    closing: bool = False,
    app: Any = None,
    writer: Any = sentinel,
    protocol: Any = sentinel,
    transport: Any = sentinel,
    payload: StreamReader = EMPTY_PAYLOAD,
    sslcontext: Optional[SSLContext] = None,
    client_max_size: int = 1024**2,
    loop: Any = ...,
    """Creates mocked web.Request testing purposes.
    Useful in unit tests, when spinning full web server is overkill or
    specific conditions and errors are hard to trigger.
    task = mock.Mock()
    if loop is ...:
        # no loop passed, try to get the current one if
        # its is running as we need a real loop to create
        # executor jobs to be able to do testing
        # with a real executor
            loop = mock.Mock()
            loop.create_future.return_value = ()
    if version < HttpVersion(1, 1):
        closing = True
        headers = CIMultiDictProxy(CIMultiDict(headers))
        raw_hdrs = tuple(
            (k.encode("utf-8"), v.encode("utf-8")) for k, v in headers.items()
        headers = CIMultiDictProxy(CIMultiDict())
        raw_hdrs = ()
    chunked = "chunked" in headers.get(hdrs.TRANSFER_ENCODING, "").lower()
    message = RawRequestMessage(
        raw_hdrs,
        closing,
        chunked,
        URL(path),
        app = _create_app_mock()
    if transport is sentinel:
        transport = _create_transport(sslcontext)
    if protocol is sentinel:
        protocol = mock.Mock()
        protocol.transport = transport
        type(protocol).peername = mock.PropertyMock(
            return_value=transport.get_extra_info("peername")
        type(protocol).ssl_context = mock.PropertyMock(return_value=sslcontext)
    if writer is sentinel:
        writer = mock.Mock()
        writer.write_headers = make_mocked_coro(None)
        writer.write = make_mocked_coro(None)
        writer.write_eof = make_mocked_coro(None)
        writer.drain = make_mocked_coro(None)
        writer.transport = transport
    protocol.writer = writer
        message, payload, protocol, writer, task, loop, client_max_size=client_max_size
    match_info = UrlMappingMatchInfo(
        {} if match_info is sentinel else match_info, mock.Mock()
    match_info.add_app(app)
    req._match_info = match_info
def make_mocked_coro(
    return_value: Any = sentinel, raise_exception: Any = sentinel
    """Creates a coroutine mock."""
    async def mock_coro(*args: Any, **kwargs: Any) -> Any:
        if raise_exception is not sentinel:
            raise raise_exception
        if not inspect.isawaitable(return_value):
        await return_value
    return mock.Mock(wraps=mock_coro)
from .utils import generate_traceparent, normalized_hash
class TestUtils:
    def test_traceparent_format_is_correct(self):
        traceparent = generate_traceparent()
        # W3C traceparent format: 00-{32 hex chars}-{16 hex chars}-{2 hex chars}
        # https://www.w3.org/TR/trace-context/#traceparent-header
        pattern = r'^00-[0-9a-f]{32}-[0-9a-f]{16}-01$'
        assert re.match(pattern, traceparent), f"Traceparent '{traceparent}' does not match W3C format"
    @pytest.mark.parametrize("key,salt,expected_hash", [
        ("abc", "variant", 0.72),
        ("def", "variant", 0.21),
    def test_normalized_hash_for_known_inputs(self, key, salt, expected_hash):
        result = normalized_hash(key, salt)
        assert result == expected_hash, f"Expected hash of {expected_hash} for '{key}' with salt '{salt}', got {result}"from math import nan
from jsonschema._utils import equal
class TestEqual(TestCase):
        self.assertTrue(equal(None, None))
        self.assertTrue(equal(nan, nan))
class TestDictEqual(TestCase):
    def test_equal_dictionaries(self):
        dict_1 = {"a": "b", "c": "d"}
        dict_2 = {"c": "d", "a": "b"}
        self.assertTrue(equal(dict_1, dict_2))
    def test_equal_dictionaries_with_nan(self):
        dict_1 = {"a": nan, "c": "d"}
        dict_2 = {"c": "d", "a": nan}
    def test_missing_key(self):
        dict_2 = {"c": "d", "x": "b"}
        self.assertFalse(equal(dict_1, dict_2))
    def test_additional_key(self):
        dict_2 = {"c": "d", "a": "b", "x": "x"}
    def test_missing_value(self):
        dict_2 = {"c": "d", "a": "x"}
    def test_empty_dictionaries(self):
        dict_1 = {}
        dict_2 = {}
    def test_one_none(self):
        dict_1 = None
        dict_2 = {"a": "b", "c": "d"}
    def test_same_item(self):
        self.assertTrue(equal(dict_1, dict_1))
    def test_nested_equal(self):
        dict_1 = {"a": {"a": "b", "c": "d"}, "c": "d"}
        dict_2 = {"c": "d", "a": {"a": "b", "c": "d"}}
    def test_nested_dict_unequal(self):
        dict_2 = {"c": "d", "a": {"a": "b", "c": "x"}}
    def test_mixed_nested_equal(self):
        dict_1 = {"a": ["a", "b", "c", "d"], "c": "d"}
        dict_2 = {"c": "d", "a": ["a", "b", "c", "d"]}
    def test_nested_list_unequal(self):
        dict_2 = {"c": "d", "a": ["b", "c", "d", "a"]}
class TestListEqual(TestCase):
    def test_equal_lists(self):
        list_1 = ["a", "b", "c"]
        list_2 = ["a", "b", "c"]
        self.assertTrue(equal(list_1, list_2))
    def test_equal_lists_with_nan(self):
        list_1 = ["a", nan, "c"]
        list_2 = ["a", nan, "c"]
    def test_unsorted_lists(self):
        list_2 = ["b", "b", "a"]
        self.assertFalse(equal(list_1, list_2))
    def test_first_list_larger(self):
        list_2 = ["a", "b"]
    def test_second_list_larger(self):
        list_1 = ["a", "b"]
    def test_list_with_none_unequal(self):
        list_1 = ["a", "b", None]
        list_2 = [None, "b", "c"]
    def test_list_with_none_equal(self):
        list_1 = ["a", None, "c"]
        list_2 = ["a", None, "c"]
    def test_empty_list(self):
        list_1 = []
        list_2 = []
        list_1 = None
    def test_same_list(self):
        self.assertTrue(equal(list_1, list_1))
    def test_equal_nested_lists(self):
        list_1 = ["a", ["b", "c"], "d"]
        list_2 = ["a", ["b", "c"], "d"]
    def test_unequal_nested_lists(self):
        list_2 = ["a", [], "c"]
