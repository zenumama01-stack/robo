import os.path
import yt_dlp.extractor
from yt_dlp import YoutubeDL
from yt_dlp.utils import preferredencoding, try_call, write_string, find_available_port
if 'pytest' in sys.modules:
    is_download_test = pytest.mark.download
    def is_download_test(test_class):
        return test_class
def get_params(override=None):
    PARAMETERS_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                   'parameters.json')
    LOCAL_PARAMETERS_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                         'local_parameters.json')
    with open(PARAMETERS_FILE, encoding='utf-8') as pf:
        parameters = json.load(pf)
    if os.path.exists(LOCAL_PARAMETERS_FILE):
        with open(LOCAL_PARAMETERS_FILE, encoding='utf-8') as pf:
            parameters.update(json.load(pf))
    if override:
        parameters.update(override)
def try_rm(filename):
    """ Remove a file if it exists """
        os.remove(filename)
    except OSError as ose:
        if ose.errno != errno.ENOENT:
def report_warning(message, *args, **kwargs):
    Print the message to stderr, it will be prefixed with 'WARNING:'
    If stderr is a tty file the 'WARNING:' will be colored
    if sys.stderr.isatty() and os.name != 'nt':
        _msg_header = '\033[0;33mWARNING:\033[0m'
        _msg_header = 'WARNING:'
    output = f'{_msg_header} {message}\n'
    if 'b' in getattr(sys.stderr, 'mode', ''):
        output = output.encode(preferredencoding())
    sys.stderr.write(output)
class FakeYDL(YoutubeDL):
    def __init__(self, override=None):
        # Different instances of the downloader can't share the same dictionary
        # some test set the "sublang" parameter, which would break the md5 checks.
        params = get_params(override=override)
        super().__init__(params, auto_init=False)
        self.result = []
    def to_screen(self, s, *args, **kwargs):
        print(s)
    def trouble(self, s, *args, **kwargs):
        raise Exception(s)
    def download(self, x):
        self.result.append(x)
    def expect_warning(self, regex):
        # Silence an expected warning matching a regex
        old_report_warning = self.report_warning
        def report_warning(self, message, *args, **kwargs):
            if re.match(regex, message):
            old_report_warning(message, *args, **kwargs)
        self.report_warning = types.MethodType(report_warning, self)
def gettestcases(include_onlymatching=False):
    for ie in yt_dlp.extractor.gen_extractors():
        yield from ie.get_testcases(include_onlymatching)
def getwebpagetestcases():
        for tc in ie.get_webpage_testcases():
            tc.setdefault('add_ie', []).append('Generic')
            yield tc
md5 = lambda s: hashlib.md5(s.encode()).hexdigest()
def _iter_differences(got, expected, field):
    if isinstance(expected, str):
        op, _, val = expected.partition(':')
        if op in ('mincount', 'maxcount', 'count'):
            if not isinstance(got, (list, dict)):
                yield field, f'expected either {list.__name__} or {dict.__name__}, got {type(got).__name__}'
            expected_num = int(val)
            got_num = len(got)
            if op == 'mincount':
                if got_num < expected_num:
                    yield field, f'expected at least {val} items, got {got_num}'
            if op == 'maxcount':
                if got_num > expected_num:
                    yield field, f'expected at most {val} items, got {got_num}'
            assert op == 'count'
            if got_num != expected_num:
                yield field, f'expected exactly {val} items, got {got_num}'
        if not isinstance(got, str):
            yield field, f'expected {str.__name__}, got {type(got).__name__}'
        if op == 're':
            if not re.match(val, got):
                yield field, f'should match {val!r}, got {got!r}'
        if op == 'startswith':
            if not got.startswith(val):
                yield field, f'should start with {val!r}, got {got!r}'
        if op == 'contains':
            if not val.startswith(got):
                yield field, f'should contain {val!r}, got {got!r}'
        if op == 'md5':
            hash_val = md5(got)
            if hash_val != val:
                yield field, f'expected hash {val}, got {hash_val}'
        if got != expected:
            yield field, f'expected {expected!r}, got {got!r}'
    if isinstance(expected, dict) and isinstance(got, dict):
        for key, expected_val in expected.items():
            if key not in got:
                yield field, f'missing key: {key!r}'
            field_name = key if field is None else f'{field}.{key}'
            yield from _iter_differences(got[key], expected_val, field_name)
    if isinstance(expected, type):
        if not isinstance(got, expected):
            yield field, f'expected {expected.__name__}, got {type(got).__name__}'
    if isinstance(expected, list) and isinstance(got, list):
        # TODO: clever diffing algorithm lmao
        if len(expected) != len(got):
            yield field, f'expected length of {len(expected)}, got {len(got)}'
        for index, (got_val, expected_val) in enumerate(zip(got, expected, strict=True)):
            field_name = str(index) if field is None else f'{field}.{index}'
            yield from _iter_differences(got_val, expected_val, field_name)
def _expect_value(message, got, expected, field):
    mismatches = list(_iter_differences(got, expected, field))
    if not mismatches:
    fields = [field for field, _ in mismatches if field is not None]
    return ''.join((
        message, f' ({", ".join(fields)})' if fields else '',
        *(f'\n\t{field}: {message}' for field, message in mismatches)))
def expect_value(self, got, expected, field):
    if message := _expect_value('values differ', got, expected, field):
        self.fail(message)
def expect_dict(self, got_dict, expected_dict):
    if message := _expect_value('dictionaries differ', got_dict, expected_dict, None):
def sanitize_got_info_dict(got_dict):
    IGNORED_FIELDS = (
        *YoutubeDL._format_fields,
        # Lists
        'formats', 'thumbnails', 'subtitles', 'automatic_captions', 'comments', 'entries',
        # Auto-generated
        'autonumber', 'playlist', 'format_index', 'video_ext', 'audio_ext', 'duration_string', 'epoch', 'n_entries',
        'fulltitle', 'extractor', 'extractor_key', 'filename', 'filepath', 'infojson_filename', 'original_url',
        # Only live_status needs to be checked
        'is_live', 'was_live',
    IGNORED_PREFIXES = ('', 'playlist', 'requested', 'webpage')
    def sanitize(key, value):
        if isinstance(value, str) and len(value) > 100 and key != 'thumbnail':
            return f'md5:{md5(value)}'
        elif isinstance(value, list) and len(value) > 10:
            return f'count:{len(value)}'
        elif key.endswith('_count') and isinstance(value, int):
            return int
    test_info_dict = {
        key: sanitize(key, value) for key, value in got_dict.items()
        if value is not None and key not in IGNORED_FIELDS and (
            not any(key.startswith(f'{prefix}_') for prefix in IGNORED_PREFIXES)
            or key == '_old_archive_ids')
    # display_id may be generated from id
    if test_info_dict.get('display_id') == test_info_dict.get('id'):
        test_info_dict.pop('display_id')
    # Remove deprecated fields
    for old in YoutubeDL._deprecated_multivalue_fields:
        test_info_dict.pop(old, None)
    # release_year may be generated from release_date
    if try_call(lambda: test_info_dict['release_year'] == int(test_info_dict['release_date'][:4])):
        test_info_dict.pop('release_year')
    # Check url for flat entries
    if got_dict.get('_type', 'video') != 'video' and got_dict.get('url'):
        test_info_dict['url'] = got_dict['url']
    return test_info_dict
def expect_info_dict(self, got_dict, expected_dict):
    ALLOWED_KEYS_SORT_ORDER = (
        # NB: Keep in sync with the docstring of extractor/common.py
        'ie_key', 'url', 'id', 'ext', 'direct', 'display_id', 'title', 'alt_title', 'description', 'media_type',
        'uploader', 'uploader_id', 'uploader_url', 'channel', 'channel_id', 'channel_url', 'channel_is_verified',
        'channel_follower_count', 'comment_count', 'view_count', 'concurrent_view_count', 'save_count',
        'like_count', 'dislike_count', 'repost_count', 'average_rating', 'age_limit', 'duration', 'thumbnail', 'heatmap',
        'chapters', 'chapter', 'chapter_number', 'chapter_id', 'start_time', 'end_time', 'section_start', 'section_end',
        'categories', 'tags', 'cast', 'composers', 'artists', 'album_artists', 'creators', 'genres',
        'track', 'track_number', 'track_id', 'album', 'album_type', 'disc_number',
        'series', 'series_id', 'season', 'season_number', 'season_id', 'episode', 'episode_number', 'episode_id',
        'timestamp', 'upload_date', 'release_timestamp', 'release_date', 'release_year', 'modified_timestamp', 'modified_date',
        'playable_in_embed', 'availability', 'live_status', 'location', 'license', '_old_archive_ids',
    expect_dict(self, got_dict, expected_dict)
    # Check for the presence of mandatory fields
    if got_dict.get('_type') not in ('playlist', 'multi_video'):
        mandatory_fields = ['id', 'title']
        if expected_dict.get('ext'):
            mandatory_fields.extend(('url', 'ext'))
        for key in mandatory_fields:
            self.assertTrue(got_dict.get(key), f'Missing mandatory field {key}')
    # Check for mandatory fields that are automatically set by YoutubeDL
    if got_dict.get('_type', 'video') == 'video':
        for key in ['webpage_url', 'extractor', 'extractor_key']:
            self.assertTrue(got_dict.get(key), f'Missing field: {key}')
    test_info_dict = sanitize_got_info_dict(got_dict)
    # Check for invalid/misspelled field names being returned by the extractor
    invalid_keys = sorted(test_info_dict.keys() - ALLOWED_KEYS_SORT_ORDER)
    self.assertFalse(invalid_keys, f'Invalid fields returned by the extractor: {", ".join(invalid_keys)}')
    missing_keys = sorted(
        test_info_dict.keys() - expected_dict.keys(),
        key=ALLOWED_KEYS_SORT_ORDER.index)
    if missing_keys:
        def _repr(v):
            if isinstance(v, str):
                return "'{}'".format(v.replace('\\', '\\\\').replace("'", "\\'").replace('\n', '\\n'))
            elif isinstance(v, type):
                return v.__name__
                return repr(v)
        info_dict_str = ''.join(
            f'    {_repr(k)}: {_repr(v)},\n'
            for k, v in test_info_dict.items() if k not in missing_keys)
        if info_dict_str:
            info_dict_str += '\n'
        info_dict_str += ''.join(
            f'    {_repr(k)}: {_repr(test_info_dict[k])},\n'
            for k in missing_keys)
        info_dict_str = '\n\'info_dict\': {\n' + info_dict_str + '},\n'
        write_string(info_dict_str.replace('\n', '\n        '), out=sys.stderr)
        self.assertFalse(
            missing_keys,
            'Missing keys in test definition: {}'.format(', '.join(sorted(missing_keys))))
def assertRegexpMatches(self, text, regexp, msg=None):
    if hasattr(self, 'assertRegexp'):
        return self.assertRegexp(text, regexp, msg)
        m = re.match(regexp, text)
        if not m:
            note = f'Regexp didn\'t match: {regexp!r} not found'
            if len(text) < 1000:
                note += f' in {text!r}'
            if msg is None:
                msg = note
                msg = note + ', ' + msg
            self.assertTrue(m, msg)
def assertGreaterEqual(self, got, expected, msg=None):
    if not (got >= expected):
            msg = f'{got!r} not greater than or equal to {expected!r}'
        self.assertTrue(got >= expected, msg)
def assertLessEqual(self, got, expected, msg=None):
    if not (got <= expected):
            msg = f'{got!r} not less than or equal to {expected!r}'
        self.assertTrue(got <= expected, msg)
def assertEqual(self, got, expected, msg=None):
            msg = f'{got!r} not equal to {expected!r}'
        self.assertTrue(got == expected, msg)
def expect_warnings(ydl, warnings_re):
    real_warning = ydl.report_warning
    def _report_warning(w, *args, **kwargs):
        if not any(re.search(w_re, w) for w_re in warnings_re):
            real_warning(w, *args, **kwargs)
    ydl.report_warning = _report_warning
def http_server_port(httpd):
    if os.name == 'java' and isinstance(httpd.socket, ssl.SSLSocket):
        # In Jython SSLSocket is not a subclass of socket.socket
        sock = httpd.socket.sock
        sock = httpd.socket
    return sock.getsockname()[1]
def verify_address_availability(address):
    if find_available_port(address) is None:
        pytest.skip(f'Unable to bind to source address {address} (address may not exist)')
def validate_and_send(rh, req):
    rh.validate(req)
    return rh.send(req)
from django.contrib.staticfiles.urls import staticfiles_urlpatterns
urlpatterns = staticfiles_urlpatterns()
Discrete Fourier Transforms - helper.py
from numpy.core import integer, empty, arange, asarray, roll
from numpy.core.overrides import array_function_dispatch, set_module
# Created by Pearu Peterson, September 2002
__all__ = ['fftshift', 'ifftshift', 'fftfreq', 'rfftfreq']
integer_types = (int, integer)
def _fftshift_dispatcher(x, axes=None):
    return (x,)
@array_function_dispatch(_fftshift_dispatcher, module='numpy.fft')
def fftshift(x, axes=None):
    Shift the zero-frequency component to the center of the spectrum.
    This function swaps half-spaces for all axes listed (defaults to all).
    Note that ``y[0]`` is the Nyquist component only if ``len(x)`` is even.
    x : array_like
        Input array.
    axes : int or shape tuple, optional
        Axes over which to shift.  Default is None, which shifts all axes.
    y : ndarray
        The shifted array.
    ifftshift : The inverse of `fftshift`.
    >>> freqs = np.fft.fftfreq(10, 0.1)
    >>> freqs
    array([ 0.,  1.,  2., ..., -3., -2., -1.])
    >>> np.fft.fftshift(freqs)
    array([-5., -4., -3., -2., -1.,  0.,  1.,  2.,  3.,  4.])
    Shift the zero-frequency component only along the second axis:
    >>> freqs = np.fft.fftfreq(9, d=1./9).reshape(3, 3)
    array([[ 0.,  1.,  2.],
           [ 3.,  4., -4.],
           [-3., -2., -1.]])
    >>> np.fft.fftshift(freqs, axes=(1,))
    array([[ 2.,  0.,  1.],
           [-4.,  3.,  4.],
           [-1., -3., -2.]])
    x = asarray(x)
    if axes is None:
        axes = tuple(range(x.ndim))
        shift = [dim // 2 for dim in x.shape]
    elif isinstance(axes, integer_types):
        shift = x.shape[axes] // 2
        shift = [x.shape[ax] // 2 for ax in axes]
    return roll(x, shift, axes)
def ifftshift(x, axes=None):
    The inverse of `fftshift`. Although identical for even-length `x`, the
    functions differ by one sample for odd-length `x`.
        Axes over which to calculate.  Defaults to None, which shifts all axes.
    fftshift : Shift zero-frequency component to the center of the spectrum.
    >>> np.fft.ifftshift(np.fft.fftshift(freqs))
        shift = [-(dim // 2) for dim in x.shape]
        shift = -(x.shape[axes] // 2)
        shift = [-(x.shape[ax] // 2) for ax in axes]
@set_module('numpy.fft')
def fftfreq(n, d=1.0):
    Return the Discrete Fourier Transform sample frequencies.
    The returned float array `f` contains the frequency bin centers in cycles
    per unit of the sample spacing (with zero at the start).  For instance, if
    the sample spacing is in seconds, then the frequency unit is cycles/second.
    Given a window length `n` and a sample spacing `d`::
      f = [0, 1, ...,   n/2-1,     -n/2, ..., -1] / (d*n)   if n is even
      f = [0, 1, ..., (n-1)/2, -(n-1)/2, ..., -1] / (d*n)   if n is odd
    n : int
        Window length.
    d : scalar, optional
        Sample spacing (inverse of the sampling rate). Defaults to 1.
    f : ndarray
        Array of length `n` containing the sample frequencies.
    >>> signal = np.array([-2, 8, 6, 4, 1, 0, 3, 5], dtype=float)
    >>> fourier = np.fft.fft(signal)
    >>> n = signal.size
    >>> timestep = 0.1
    >>> freq = np.fft.fftfreq(n, d=timestep)
    >>> freq
    array([ 0.  ,  1.25,  2.5 , ..., -3.75, -2.5 , -1.25])
    if not isinstance(n, integer_types):
        raise ValueError("n should be an integer")
    val = 1.0 / (n * d)
    results = empty(n, int)
    N = (n-1)//2 + 1
    p1 = arange(0, N, dtype=int)
    results[:N] = p1
    p2 = arange(-(n//2), 0, dtype=int)
    results[N:] = p2
    return results * val
def rfftfreq(n, d=1.0):
    Return the Discrete Fourier Transform sample frequencies
    (for usage with rfft, irfft).
      f = [0, 1, ...,     n/2-1,     n/2] / (d*n)   if n is even
      f = [0, 1, ..., (n-1)/2-1, (n-1)/2] / (d*n)   if n is odd
    Unlike `fftfreq` (but like `scipy.fftpack.rfftfreq`)
    the Nyquist frequency component is considered to be positive.
        Array of length ``n//2 + 1`` containing the sample frequencies.
    >>> signal = np.array([-2, 8, 6, 4, 1, 0, 3, 5, -3, 4], dtype=float)
    >>> fourier = np.fft.rfft(signal)
    >>> sample_rate = 100
    >>> freq = np.fft.fftfreq(n, d=1./sample_rate)
    array([  0.,  10.,  20., ..., -30., -20., -10.])
    >>> freq = np.fft.rfftfreq(n, d=1./sample_rate)
    array([  0.,  10.,  20.,  30.,  40.,  50.])
    val = 1.0/(n*d)
    N = n//2 + 1
    results = arange(0, N, dtype=int)
from numbers import Number
from scipy._lib._util import copy_if_needed
# good_size is exposed (and used) from this import
from .pypocketfft import good_size, prev_good_size
__all__ = ['good_size', 'prev_good_size', 'set_workers', 'get_workers']
_config = threading.local()
_cpu_count = os.cpu_count()
def _iterable_of_int(x, name=None):
    """Convert ``x`` to an iterable sequence of int
    x : value, or sequence of values, convertible to int
    name : str, optional
        Name of the argument being converted, only used in the error message
    y : ``List[int]``
    if isinstance(x, Number):
        x = (x,)
        x = [operator.index(a) for a in x]
        name = name or "value"
        raise ValueError(f"{name} must be a scalar or iterable of integers") from e
def _init_nd_shape_and_axes(x, shape, axes):
    """Handles shape and axes arguments for nd transforms"""
    noshape = shape is None
    noaxes = axes is None
    if not noaxes:
        axes = _iterable_of_int(axes, 'axes')
        axes = [a + x.ndim if a < 0 else a for a in axes]
        if any(a >= x.ndim or a < 0 for a in axes):
            raise ValueError("axes exceeds dimensionality of input")
        if len(set(axes)) != len(axes):
            raise ValueError("all axes must be unique")
    if not noshape:
        shape = _iterable_of_int(shape, 'shape')
        if axes and len(axes) != len(shape):
            raise ValueError("when given, axes and shape arguments"
                             " have to be of the same length")
        if noaxes:
            if len(shape) > x.ndim:
                raise ValueError("shape requires more axes than are present")
            axes = range(x.ndim - len(shape), x.ndim)
        shape = [x.shape[a] if s == -1 else s for s, a in zip(shape, axes)]
    elif noaxes:
        shape = list(x.shape)
        axes = range(x.ndim)
        shape = [x.shape[a] for a in axes]
    if any(s < 1 for s in shape):
            f"invalid number of data points ({shape}) specified")
    return tuple(shape), list(axes)
def _asfarray(x):
    Convert to array with floating or complex dtype.
    float16 values are also promoted to float32.
    if not hasattr(x, "dtype"):
        return np.asarray(x, np.float32)
    elif x.dtype.kind not in 'fc':
        return np.asarray(x, np.float64)
    # Require native byte order
    dtype = x.dtype.newbyteorder('=')
    # Always align input
    copy = True if not x.flags['ALIGNED'] else copy_if_needed
    return np.array(x, dtype=dtype, copy=copy)
def _datacopied(arr, original):
    Strict check for `arr` not sharing any data with `original`,
    under the assumption that arr = asarray(original)
    if arr is original:
    if not isinstance(original, np.ndarray) and hasattr(original, '__array__'):
    return arr.base is None
def _fix_shape(x, shape, axes):
    """Internal auxiliary function for _raw_fft, _raw_fftnd."""
    must_copy = False
    # Build an nd slice with the dimensions to be read from x
    index = [slice(None)]*x.ndim
    for n, ax in zip(shape, axes):
        if x.shape[ax] >= n:
            index[ax] = slice(0, n)
            index[ax] = slice(0, x.shape[ax])
            must_copy = True
    index = tuple(index)
    if not must_copy:
        return x[index], False
    s = list(x.shape)
    for n, axis in zip(shape, axes):
        s[axis] = n
    z = np.zeros(s, x.dtype)
    z[index] = x[index]
    return z, True
def _fix_shape_1d(x, n, axis):
            f"invalid number of data points ({n}) specified")
    return _fix_shape(x, (n,), (axis,))
_NORM_MAP = {None: 0, 'backward': 0, 'ortho': 1, 'forward': 2}
def _normalization(norm, forward):
    """Returns the pypocketfft normalization mode from the norm argument"""
        inorm = _NORM_MAP[norm]
        return inorm if forward else (2 - inorm)
            f'Invalid norm value {norm!r}, should '
            'be "backward", "ortho" or "forward"') from None
def _workers(workers):
    if workers is None:
        return getattr(_config, 'default_workers', 1)
    if workers < 0:
        if workers >= -_cpu_count:
            workers += 1 + _cpu_count
            raise ValueError(f"workers value out of range; got {workers}, must not be"
                             f" less than {-_cpu_count}")
    elif workers == 0:
        raise ValueError("workers must not be zero")
    return workers
def set_workers(workers):
    """Context manager for the default number of workers used in `scipy.fft`
    workers : int
        The default number of workers to use
    >>> from scipy import fft, signal
    >>> rng = np.random.default_rng()
    >>> x = rng.standard_normal((128, 64))
    >>> with fft.set_workers(4):
    ...     y = signal.fftconvolve(x, x)
    old_workers = get_workers()
    _config.default_workers = _workers(operator.index(workers))
        _config.default_workers = old_workers
def get_workers():
    """Returns the default number of workers within the current context
    >>> from scipy import fft
    >>> fft.get_workers()
    ...     fft.get_workers()
    'fftshift', 'ifftshift', 'fftfreq', 'rfftfreq', 'next_fast_len'
    return _sub_module_deprecation(sub_package="fftpack", module="helper",
                                   private_modules=["_helper"], all=__all__,
