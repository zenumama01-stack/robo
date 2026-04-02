class OneDriveError(Exception):
    def __init__(self, prop_dict, status_code):
        """Initialize a OneDriveError given the JSON
        error response dictionary, and the HTTP status code
            prop_dict (dict): A dictionary containing the response
                from OneDrive
            status_code (int): The HTTP status code (ex. 200, 201, etc.)
        if "code" not in prop_dict or "message" not in prop_dict:
            prop_dict["code"] = ErrorCode.Malformed
            prop_dict["message"] = "The received response was malformed"
            super(OneDriveError, self).__init__(prop_dict["code"]+" - "+prop_dict["message"])
        self._status_code = status_code
        """The HTTP status code
            int: The HTTP status code
        return self._status_code
    def code(self):
        """The OneDrive error code sent back in
        the response. Possible codes can be found
        in the :class:`ErrorCode` enum.
            str: The error code
        return self._prop_dict["code"]
    def inner_error(self):
        """Creates a OneDriveError object from the specified inner 
        error within the response.
            :class:`OneDriveError`: Error from within the inner
        return OneDriveError(self._prop_dict["innererror"], self.status_code) if "innererror" in self._prop_dict else None
    def matches(self, code):
        """Recursively searches the :class:`OneDriveError` to find
        if the specified code was found
            code (str): The error code to search for
            bool: True if the error code was found, false otherwise
        if self.code == code:
        return False if self.inner_error is None else self.inner_error.matches(code)
class ErrorCode(object):
    #: Access was denied to the resource
    AccessDenied = "accessDenied"
    #: The activity limit has been reached
    ActivityLimitReached = "activityLimitReached"
    #: A general exception occured
    GeneralException = "generalException"
    #: An invalid range was provided
    InvalidRange = "invalidRange"
    #: An invalid request was provided
    InvalidRequest = "invalidRequest"
    #: The requested resource was not found
    ItemNotFound = "itemNotFound"
    #: Malware was detected in the resource
    MalwareDetected = "malwareDetected"
    #: The name already exists
    NameAlreadyExists = "nameAlreadyExists"
    #: The action was not allowed
    NotAllowed = "notAllowed"
    #: The action was not supported
    NotSupported = "notSupported"
    #: The resource was modified
    ResourceModified = "resourceModified"
    #: A resync is required
    ResyncRequired = "resyncRequired"
    #: The OneDrive service is not available
    ServiceNotAvailable = "serviceNotAvailable"
    #: The quota for this OneDrive has been reached
    QuotaLimitReached = "quotaLimitReached"
    #: The user is unauthenticated
    Unauthenticated = "unauthenticated"
    #: The response was malformed
    Malformed = "malformed"
This module houses the GDAL & SRS Exception objects, and the
check_err() routine which checks the status code returned by
GDAL/OGR methods.
# #### GDAL & SRS Exceptions ####
class GDALException(Exception):
class SRSException(Exception):
# #### GDAL/OGR error checking codes and routine ####
# OGR Error Codes
OGRERR_DICT = {
    1: (GDALException, "Not enough data."),
    2: (GDALException, "Not enough memory."),
    3: (GDALException, "Unsupported geometry type."),
    4: (GDALException, "Unsupported operation."),
    5: (GDALException, "Corrupt data."),
    6: (GDALException, "OGR failure."),
    7: (SRSException, "Unsupported SRS."),
    8: (GDALException, "Invalid handle."),
# CPL Error Codes
# https://gdal.org/api/cpl.html#cpl-error-h
CPLERR_DICT = {
    1: (GDALException, "AppDefined"),
    2: (GDALException, "OutOfMemory"),
    3: (GDALException, "FileIO"),
    4: (GDALException, "OpenFailed"),
    5: (GDALException, "IllegalArg"),
    6: (GDALException, "NotSupported"),
    7: (GDALException, "AssertionFailed"),
    8: (GDALException, "NoWriteAccess"),
    9: (GDALException, "UserInterrupt"),
    10: (GDALException, "ObjectNull"),
ERR_NONE = 0
def check_err(code, cpl=False):
    Check the given CPL/OGRERR and raise an exception where appropriate.
    err_dict = CPLERR_DICT if cpl else OGRERR_DICT
    if code == ERR_NONE:
    elif code in err_dict:
        e, msg = err_dict[code]
        raise e(msg)
        raise GDALException('Unknown error code: "%s"' % code)
class GEOSException(Exception):
    "The base GEOS exception, indicates a GEOS-related error."
__all__ = ['Mark', 'YAMLError', 'MarkedYAMLError']
class Mark:
    def __init__(self, name, index, line, column, buffer, pointer):
        self.line = line
        self.column = column
        self.buffer = buffer
        self.pointer = pointer
    def get_snippet(self, indent=4, max_length=75):
        if self.buffer is None:
        head = ''
        start = self.pointer
        while start > 0 and self.buffer[start-1] not in '\0\r\n\x85\u2028\u2029':
            start -= 1
            if self.pointer-start > max_length/2-1:
                head = ' ... '
                start += 5
        tail = ''
        end = self.pointer
        while end < len(self.buffer) and self.buffer[end] not in '\0\r\n\x85\u2028\u2029':
            end += 1
            if end-self.pointer > max_length/2-1:
                tail = ' ... '
                end -= 5
        snippet = self.buffer[start:end]
        return ' '*indent + head + snippet + tail + '\n'  \
                + ' '*(indent+self.pointer-start+len(head)) + '^'
        snippet = self.get_snippet()
        where = "  in \"%s\", line %d, column %d"   \
                % (self.name, self.line+1, self.column+1)
        if snippet is not None:
            where += ":\n"+snippet
        return where
class YAMLError(Exception):
class MarkedYAMLError(YAMLError):
    def __init__(self, context=None, context_mark=None,
            problem=None, problem_mark=None, note=None):
        self.context_mark = context_mark
        self.problem = problem
        self.problem_mark = problem_mark
        self.note = note
            lines.append(self.context)
        if self.context_mark is not None  \
            and (self.problem is None or self.problem_mark is None
                    or self.context_mark.name != self.problem_mark.name
                    or self.context_mark.line != self.problem_mark.line
                    or self.context_mark.column != self.problem_mark.column):
            lines.append(str(self.context_mark))
        if self.problem is not None:
            lines.append(self.problem)
        if self.problem_mark is not None:
            lines.append(str(self.problem_mark))
        if self.note is not None:
            lines.append(self.note)
class FFIError(Exception):
    __module__ = 'cffi'
class CDefError(Exception):
            current_decl = self.args[1]
            filename = current_decl.coord.file
            linenum = current_decl.coord.line
            prefix = '%s:%d: ' % (filename, linenum)
        except (AttributeError, TypeError, IndexError):
            prefix = ''
        return '%s%s' % (prefix, self.args[0])
class VerificationError(Exception):
    """ An error raised when verification fails
class VerificationMissing(Exception):
    """ An error raised when incomplete structures are passed into
    cdef, but no verification has been done
class PkgConfigError(Exception):
    """ An error raised for missing modules in pkg-config
