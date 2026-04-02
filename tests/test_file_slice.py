    from unittest.mock import patch, mock_open
    from mock import patch, mock_open
from onedrivesdk.helpers.file_slice import FileSlice
class TestFileSlice(unittest.TestCase):
    def testSliceFileStartEnd(self):
        with tempfile.TemporaryFile() as f:
            f.write(b'123456789')
            part = FileSlice(f, 0, 5)
            self.assertEqual(len(part), 5)
            self.assertEqual(part.read(), b'12345')
            self.assertEqual(part.read(3), b'')
            part.seek(0, io.SEEK_SET)
            self.assertEqual(part.read(3), b'123')
            self.assertEqual(part.tell(), 3)
            part.seek(-3, io.SEEK_CUR)
            self.assertEqual(part.tell(), 0)
            part.seek(-2, io.SEEK_END)
            self.assertEqual(part.readall(), b'45')
            with self.assertRaises(IOError):
                part.write('abc')
                part.writelines(['foo', 'bar'])
    def testSliceFileStartLength(self):
            part = FileSlice(f, 0, length=5)
            part.seek(0)
            self.assertEqual(part.readall(), b'12345')
    def testSliceFileMiddleStartEnd(self):
            part = FileSlice(f, 1, 5)
            self.assertEqual(len(part), 4)
            self.assertEqual(part.read(3), b'234')
            self.assertEqual(part.readall(), b'5')
            self.assertEqual(part.read(), b'')
            self.assertEqual(part.tell(), 4)
    def testSliceFileMiddleStartLength(self):
            part = FileSlice(f, 1, length=5)
            self.assertEqual(part.readall(), b'56')
            self.assertEqual(part.tell(), 5)
    def testSliceFileMiddleStartEnd_afterEOF(self):
            part = FileSlice(f, 8, 15)
            self.assertEqual(len(part), 1)
            self.assertEqual(part.read(3), b'9')
            self.assertEqual(part.readall(), b'')
            self.assertEqual(part.tell(), 1)
            part.seek(-1, io.SEEK_END)
            self.assertEqual(part.readall(), b'9')
    def testSliceFileMiddleStartLength_afterEOF(self):
            part = FileSlice(f, 8, length=15)
    def testSeek(self):
            part = FileSlice(f, 2, 7)
            part.seek(3)
            part.seek(part.tell(), io.SEEK_SET)
    def testSanityChecks(self):
                part = FileSlice(f, -5, -2)
                part = FileSlice(f, 0, -2)
                part = FileSlice(f, -10, 2)
                part = FileSlice(f, 10, 2)
                part = FileSlice(f, 10, length=-2)
                part.seek(8)
                part.seek(8, io.SEEK_SET)
                part.seek(4, io.SEEK_CUR)
                part.seek(-5, io.SEEK_END)
