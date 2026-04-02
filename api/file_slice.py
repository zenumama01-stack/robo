class FileSlice(io.RawIOBase):
    This class represents a window over a file handle. It will allow only access
    and read of bytes above start and no further than end/length bytes
    def __init__(self, handle, start, end=None, length=None):
        Creates new instance of FileSlice on file-like object handle. It behaves
        like normal file object, but only allow reading of bytes above start and
        no further than end/lenght byte
            handle (file-like object): file handle object to create a view over.
                File should be open in binary mode
            start (int): start byte number, makring the first byte that can be
                read from FileSlice
            end (int): Optional. Last byte of the file that can be read.
            length (int): Optional. Number of the bytes, starting from the
                start, that can be read from FileSlice
            One of end or length must be provided
        assert end or length, "You need to provide one of end or length parameter"
        assert not (end and length), "You need to proivde only one parameter: end or length, not both"
            raise ValueError("Start of the file smaller than 0")
        if end and end < start:
            raise ValueError("End of the tile smaller than start")
        if length and length < 0:
            raise ValueError("Length smaller than 0")
        self._handle = handle
        self._start = start
        if end:
            self._end = end
            self._end = start + length
        self._end = min(self._end, os.fstat(handle.fileno()).st_size)
        self.seek(0)
    def _bytes_left(self):
        current_pos = self._handle.tell()
        return self._end - current_pos
        # do nothing, someone else might want to process this file
    def closed(self):
        return self._handle.closed
    def fileno(self):
        return self._handle.fileno()
        return self._handle.flush()
    def len(self):
        # this is provided for requests, so it will properly recognize the size of the file
        return self._bytes_left
        return self.len()
    def isatty(self):
        return self._handle.isatty()
    def readable(self):
        return self._handle.readable()
    def read(self, size=-1):
        if size == -1:
            read_size = self._bytes_left
            read_size = min(size, self._bytes_left)
        return self._handle.read(read_size)
    def readall(self):
        return self._handle.read(self._bytes_left)
    def readinto(self, b):
        if len(b) > self._bytes_left:
            r = self._handle.read(self._bytes_left)
            b[:len(r)] = r
            return len(r)
        return self._handle.readinto(b)
    def readline(self, size=-1):
        return self._handle.readline(max(size, self._bytes_left))
    def readlines(self, hint=-1):
        return self._handle.readlines(max(hint, self._bytes_left))
    def seek(self, offset, whence=io.SEEK_SET):
        if whence == io.SEEK_SET:
            desired_pos = self._start + offset
        if whence == io.SEEK_CUR:
            desired_pos = self._handle.tell() + offset
        if whence == io.SEEK_END:
            desired_pos = self._end + offset
        if desired_pos < self._start:
            raise ValueError("Seeking before the file slice")
        if desired_pos > self._end:
            raise ValueError("Seekeing past the end of file slice")
        ret = self._handle.seek(desired_pos, io.SEEK_SET)
        if ret:
            return ret - self._start
    def seekable(self):
        return self._handle.seekable()
    def tell(self):
        return self._handle.tell() - self._start
    def truncate(self, size=None):
        raise IOError("Operation not supported")
    def writable(self):
    def write(self, b):
    def writelines(self, lines):
