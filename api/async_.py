class LaunchResultBase(bb.Union):
    Result returned by methods that launch an asynchronous job. A method who may
    either launch an asynchronous job, or complete the request synchronously,
    can use this union by extending it, and adding a 'complete' field with the
    type of the synchronous response. See :class:`LaunchEmptyResult` for an
    example.
    :ivar str async.LaunchResultBase.async_job_id: This response indicates that
        the processing is asynchronous. The string is an id that can be used to
        obtain the status of the asynchronous job.
    _catch_all = None
    def async_job_id(cls, val):
        Create an instance of this class set to the ``async_job_id`` tag with
        :rtype: LaunchResultBase
        return cls('async_job_id', val)
    def is_async_job_id(self):
        Check if the union tag is ``async_job_id``.
        return self._tag == 'async_job_id'
    def get_async_job_id(self):
        This response indicates that the processing is asynchronous. The string
        is an id that can be used to obtain the status of the asynchronous job.
        Only call this if :meth:`is_async_job_id` is true.
        if not self.is_async_job_id():
            raise AttributeError("tag 'async_job_id' not set")
        super(LaunchResultBase, self)._process_custom_annotations(annotation_type, field_path, processor)
LaunchResultBase_validator = bv.Union(LaunchResultBase)
class LaunchEmptyResult(LaunchResultBase):
    Result returned by methods that may either launch an asynchronous job or
    complete synchronously. Upon synchronous completion of the job, no
    additional information is returned.
    :ivar async.LaunchEmptyResult.complete: The job finished synchronously and
        successfully.
    complete = None
    def is_complete(self):
        Check if the union tag is ``complete``.
        return self._tag == 'complete'
        super(LaunchEmptyResult, self)._process_custom_annotations(annotation_type, field_path, processor)
LaunchEmptyResult_validator = bv.Union(LaunchEmptyResult)
class PollArg(bb.Struct):
    Arguments for methods that poll the status of an asynchronous job.
    :ivar async.PollArg.async_job_id: Id of the asynchronous job. This is the
        value of a response returned from the method that launched the job.
        '_async_job_id_value',
                 async_job_id=None):
        self._async_job_id_value = bb.NOT_SET
        if async_job_id is not None:
            self.async_job_id = async_job_id
    async_job_id = bb.Attribute("async_job_id")
        super(PollArg, self)._process_custom_annotations(annotation_type, field_path, processor)
PollArg_validator = bv.Struct(PollArg)
class PollResultBase(bb.Union):
    Result returned by methods that poll for the status of an asynchronous job.
    Unions that extend this union should add a 'complete' field with a type of
    the information returned upon job completion. See :class:`PollEmptyResult`
    for an example.
    :ivar async.PollResultBase.in_progress: The asynchronous job is still in
        progress.
    in_progress = None
    def is_in_progress(self):
        Check if the union tag is ``in_progress``.
        return self._tag == 'in_progress'
        super(PollResultBase, self)._process_custom_annotations(annotation_type, field_path, processor)
PollResultBase_validator = bv.Union(PollResultBase)
class PollEmptyResult(PollResultBase):
    Upon completion of the job, no additional information is returned.
    :ivar async.PollEmptyResult.complete: The asynchronous job has completed
        super(PollEmptyResult, self)._process_custom_annotations(annotation_type, field_path, processor)
PollEmptyResult_validator = bv.Union(PollEmptyResult)
class PollError(bb.Union):
    Error returned by methods for polling the status of asynchronous job.
    :ivar async.PollError.invalid_async_job_id: The job ID is invalid.
    :ivar async.PollError.internal_error: Something went wrong with the job on
        Dropbox's end. You'll need to verify that the action you were taking
        succeeded, and if not, try again. This should happen very rarely.
    invalid_async_job_id = None
    internal_error = None
    def is_invalid_async_job_id(self):
        Check if the union tag is ``invalid_async_job_id``.
        return self._tag == 'invalid_async_job_id'
    def is_internal_error(self):
        Check if the union tag is ``internal_error``.
        return self._tag == 'internal_error'
        super(PollError, self)._process_custom_annotations(annotation_type, field_path, processor)
PollError_validator = bv.Union(PollError)
AsyncJobId_validator = bv.String(min_length=1)
LaunchResultBase._async_job_id_validator = AsyncJobId_validator
LaunchResultBase._tagmap = {
    'async_job_id': LaunchResultBase._async_job_id_validator,
LaunchEmptyResult._complete_validator = bv.Void()
LaunchEmptyResult._tagmap = {
    'complete': LaunchEmptyResult._complete_validator,
LaunchEmptyResult._tagmap.update(LaunchResultBase._tagmap)
LaunchEmptyResult.complete = LaunchEmptyResult('complete')
PollArg.async_job_id.validator = AsyncJobId_validator
PollArg._all_field_names_ = set(['async_job_id'])
PollArg._all_fields_ = [('async_job_id', PollArg.async_job_id.validator)]
PollResultBase._in_progress_validator = bv.Void()
PollResultBase._tagmap = {
    'in_progress': PollResultBase._in_progress_validator,
PollResultBase.in_progress = PollResultBase('in_progress')
PollEmptyResult._complete_validator = bv.Void()
PollEmptyResult._tagmap = {
    'complete': PollEmptyResult._complete_validator,
PollEmptyResult._tagmap.update(PollResultBase._tagmap)
PollEmptyResult.complete = PollEmptyResult('complete')
PollError._invalid_async_job_id_validator = bv.Void()
PollError._internal_error_validator = bv.Void()
PollError._other_validator = bv.Void()
PollError._tagmap = {
    'invalid_async_job_id': PollError._invalid_async_job_id_validator,
    'internal_error': PollError._internal_error_validator,
    'other': PollError._other_validator,
PollError.invalid_async_job_id = PollError('invalid_async_job_id')
PollError.internal_error = PollError('internal_error')
PollError.other = PollError('other')
