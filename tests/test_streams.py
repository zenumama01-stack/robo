class TestStreams(unittest.TestCase):
    def test_put(self, MockHttpProvider, MockAuthProvider):
        response = HttpResponse(200, None, json.dumps({"name":"test1", "folder":{}, "id":"test!id"}))
        response_item = client.drives["me"].items["root"].children["newFile.txt"].content.request().upload("./myPath/myFile.txt")
        assert client.http_provider.send.call_args[1]["path"] == "./myPath/myFile.txt"
        assert client.http_provider.send.call_args[0][2] == "onedriveurl/drives/me/items/root/children/newFile.txt/content"
        assert all(item in response_item._prop_dict.items() for item in json.loads(response.content).items())
    def test_download(self, MockHttpProvider, MockAuthProvider):
        path = "./myPath/myFile.txt"
        response = HttpResponse(200, None, None)
        instance.download.return_value = response
        client.drives["me"].items["root"].children["newFile.txt"].content.request().download(path)
        assert client.http_provider.download.call_args[0][2] == path
        assert client.http_provider.download.call_args[0][1] == "onedriveurl/drives/me/items/root/children/newFile.txt/content"
""" Testing
from numpy.testing import assert_, assert_equal
from pytest import raises as assert_raises
from scipy.io.matlab._streams import (make_stream,
    GenericStream, ZlibInputStream,
    _read_into, _read_string, BLOCK_SIZE)
def setup_test_file():
    val = b'a\x00string'
    fd, fname = mkstemp()
    with os.fdopen(fd, 'wb') as fs:
        fs.write(val)
    with open(fname, 'rb') as fs:
        gs = BytesIO(val)
        cs = BytesIO(val)
        yield fs, gs, cs
    os.unlink(fname)
def test_make_stream():
    with setup_test_file() as (fs, gs, cs):
        # test stream initialization
        assert_(isinstance(make_stream(gs), GenericStream))
def test_tell_seek():
        for s in (fs, gs, cs):
            st = make_stream(s)
            res = st.seek(0)
            assert_equal(res, 0)
            assert_equal(st.tell(), 0)
            res = st.seek(5)
            assert_equal(st.tell(), 5)
            res = st.seek(2, 1)
            assert_equal(st.tell(), 7)
            res = st.seek(-2, 2)
            assert_equal(st.tell(), 6)
def test_read():
            st.seek(0)
            res = st.read(-1)
            assert_equal(res, b'a\x00string')
            res = st.read(4)
            assert_equal(res, b'a\x00st')
            # read into
            res = _read_into(st, 4)
            assert_equal(res, b'ring')
            assert_raises(OSError, _read_into, st, 2)
            # read alloc
            res = _read_string(st, 4)
            assert_raises(OSError, _read_string, st, 2)
class TestZlibInputStream:
    def _get_data(self, size):
        data = np.random.randint(0, 256, size).astype(np.uint8).tobytes()
        compressed_data = zlib.compress(data)
        stream = BytesIO(compressed_data)
        return stream, len(compressed_data), data
        SIZES = [0, 1, 10, BLOCK_SIZE//2, BLOCK_SIZE-1,
                 BLOCK_SIZE, BLOCK_SIZE+1, 2*BLOCK_SIZE-1]
        READ_SIZES = [BLOCK_SIZE//2, BLOCK_SIZE-1,
                      BLOCK_SIZE, BLOCK_SIZE+1]
        def check(size, read_size):
            compressed_stream, compressed_data_len, data = self._get_data(size)
            stream = ZlibInputStream(compressed_stream, compressed_data_len)
            data2 = b''
            so_far = 0
                block = stream.read(min(read_size,
                                        size - so_far))
                so_far += len(block)
                data2 += block
            assert_equal(data, data2)
        for size in SIZES:
            for read_size in READ_SIZES:
                check(size, read_size)
    def test_read_max_length(self):
        size = 1234
        compressed_stream = BytesIO(compressed_data + b"abbacaca")
        stream = ZlibInputStream(compressed_stream, len(compressed_data))
        stream.read(len(data))
        assert_equal(compressed_stream.tell(), len(compressed_data))
        assert_raises(OSError, stream.read, 1)
    def test_read_bad_checksum(self):
        data = np.random.randint(0, 256, 10).astype(np.uint8).tobytes()
        # break checksum
        compressed_data = (compressed_data[:-1]
                           + bytes([(compressed_data[-1] + 1) & 255]))
        compressed_stream = BytesIO(compressed_data)
        assert_raises(zlib.error, stream.read, len(data))
    def test_seek(self):
        compressed_stream, compressed_data_len, data = self._get_data(1024)
        stream.seek(123)
        p = 123
        assert_equal(stream.tell(), p)
        d1 = stream.read(11)
        assert_equal(d1, data[p:p+11])
        stream.seek(321, 1)
        p = 123+11+321
        d2 = stream.read(21)
        assert_equal(d2, data[p:p+21])
        stream.seek(641, 0)
        p = 641
        d3 = stream.read(11)
        assert_equal(d3, data[p:p+11])
        assert_raises(OSError, stream.seek, 10, 2)
        assert_raises(OSError, stream.seek, -1, 1)
        assert_raises(ValueError, stream.seek, 1, 123)
        stream.seek(10000, 1)
        assert_raises(OSError, stream.read, 12)
    def test_seek_bad_checksum(self):
        assert_raises(zlib.error, stream.seek, len(data))
    def test_all_data_read(self):
        assert_(not stream.all_data_read())
        stream.seek(512)
        stream.seek(1024)
        assert_(stream.all_data_read())
    def test_all_data_read_overlap(self):
        COMPRESSION_LEVEL = 6
        data = np.arange(33707000).astype(np.uint8).tobytes()
        compressed_data = zlib.compress(data, COMPRESSION_LEVEL)
        compressed_data_len = len(compressed_data)
        # check that part of the checksum overlaps
        assert_(compressed_data_len == BLOCK_SIZE + 2)
        stream.seek(len(data))
    def test_all_data_read_bad_checksum(self):
        assert_raises(zlib.error, stream.all_data_read)
