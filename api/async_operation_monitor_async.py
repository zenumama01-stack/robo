from ..async_operation_monitor import AsyncOperationMonitor
@asyncio.coroutine
def poll_until_done_async(self):
    """Yielding this method blocks until done polling for
    the result of the async operation is returned
        :class:`Item<onedrivesdk.models.item.Item>`: item from the async operation
    future = self._client._loop.run_in_executor(None,
                                                self.poll_until_done)
    item = yield from future
AsyncOperationMonitor.poll_until_done_async = poll_until_done_async
