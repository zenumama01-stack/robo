from ..error import OneDriveError
from ..model.upload_session import UploadSession
from ..options import HeaderOption
from ..request_builder_base import RequestBuilderBase
from ..request_base import RequestBase
from ..helpers.file_slice import FileSlice
import math
__PART_SIZE = 10 * 1024 * 1024  # recommended file size. Should be multiple of 320 * 1024
__MAX_SINGLE_FILE_UPLOAD = 100 * 1024 * 1024
class ItemUploadFragment(RequestBase):
    def __init__(self, request_url, client, options, file_handle):
        super(ItemUploadFragment, self).__init__(request_url, client, options)
        self.method = "PUT"
        self._file_handle = file_handle
    def post(self):
        """Sends the POST request
            :class:`UploadSession<onedrivesdk.model.upload_session.UploadSession>`:
                The resulting entity from the operation
        entity = UploadSession(json.loads(self.send(data=self._file_handle).content))
        return entity
    def post_async(self):
        """Sends the POST request using an asyncio coroutine
            :class:`UploadedSession<onedrivesdk.model.upload_session.UploadedSession>`:
                                                    self.post)
        entity = yield from future
class ItemUploadFragmentBuilder(RequestBuilderBase):
    def __init__(self, request_url, client, content_local_path):
        super(ItemUploadFragmentBuilder, self).__init__(request_url, client)
        self._method_options = {}
        self._file_handle = open(content_local_path, "rb")
        self._total_length = os.stat(content_local_path).st_size
        self._file_handle.close()
    def request(self, begin, length, options=None):
        """Builds the request for the ItemUploadFragment
            begin (int): First byte in range to be uploaded
            length (int): Number of bytes in range to be uploaded
                Default to None, list of options to include in the request
            :class:`ItemUploadFragment<onedrivesdk.request.item_upload_fragment.ItemUploadFragment>`:
                The request
        if not (options is None or len(options) == 0):
            opts = options.copy()
            opts = []
        self.content_type = "application/octet-stream"
        opts.append(HeaderOption("Content-Range", "bytes %d-%d/%d" % (begin, begin + length - 1, self._total_length)))
        opts.append(HeaderOption("Content-Length", str(length)))
        file_slice = FileSlice(self._file_handle, begin, length=length)
        req = ItemUploadFragment(self._request_url, self._client, opts, file_slice)
        return req
    def post(self, begin, length, options=None):
            :class:`UploadedFragment<onedrivesdk.model.uploaded_fragment.UploadedFragment>`:
            The resulting UploadSession from the operation
        return self.request(begin, length, options).post()
    def post_async(self, begin, length, options=None):
        entity = yield from self.request(begin, length, options).post_async()
def fragment_upload_async(self, local_path, conflict_behavior=None, upload_status=None):
    """Uploads file using PUT using multipart upload if needed.
        local_path (str): The path to the local file to upload.
        conflict_behavior (str): conflict behavior if the file is already
            uploaded. Use None value if file should be replaced or "rename", if
            the new file should get a new name
        upload_status (func): function(current_part, total_parts) to be called
            with upload status for each 10MB part
        Created entity.
    file_size = os.stat(local_path).st_size
    if file_size <= __MAX_SINGLE_FILE_UPLOAD:
        # fallback to single shot upload if file is small enough
        return self.content.request().upload(local_path)
        # multipart upload needed for larger files
        if conflict_behavior:
            item = Item({'@name.conflictBehavior': conflict_behavior})
            item = Item({})
        session = self.create_session(item).post()
        with ItemUploadFragmentBuilder(session.upload_url, self._client, local_path) as upload_builder:
            total_parts = math.ceil(file_size / __PART_SIZE)
            for i in range(total_parts):
                if upload_status:
                    upload_status(i, total_parts)
                length = min(__PART_SIZE, file_size - i * __PART_SIZE)
                tries = 0
                        tries += 1
                        resp = upload_builder.post(i * __PART_SIZE, length)
                    except OneDriveError as exc:
                        if exc.status_code in (408, 500, 502, 503, 504) and tries < 5:
                        elif exc.status_code == 416:
                            # Fragment already received
                        elif exc.status_code == 401:
                            self._client.auth_provider.refresh_token()
                        # Swallow value errors (usually JSON error) and try again.
                    break  # while True
            upload_status(total_parts, total_parts)  # job completed
        # return last response
# Overwrite the standard upload operation to use this one
ItemRequestBuilder.upload_async = fragment_upload_async
