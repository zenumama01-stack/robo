from typing_extensions import Iterator, AsyncIterator
def consume_sync_iterator(iterator: Iterator[Any]) -> None:
    for _ in iterator:
async def consume_async_iterator(iterator: AsyncIterator[Any]) -> None:
    async for _ in iterator:
from ..streams.memory import (
    MemoryObjectReceiveStream,
    MemoryObjectSendStream,
    _MemoryObjectStreamState,
T_Item = TypeVar("T_Item")
class create_memory_object_stream(
    tuple[MemoryObjectSendStream[T_Item], MemoryObjectReceiveStream[T_Item]],
    Create a memory object stream.
    The stream's item type can be annotated like
    :func:`create_memory_object_stream[T_Item]`.
    :param max_buffer_size: number of items held in the buffer until ``send()`` starts
        blocking
    :param item_type: old way of marking the streams with the right generic type for
        static typing (does nothing on AnyIO 4)
        .. deprecated:: 4.0
          Use ``create_memory_object_stream[YourItemType](...)`` instead.
    :return: a tuple of (send stream, receive stream)
    def __new__(  # type: ignore[misc]
        cls, max_buffer_size: float = 0, item_type: object = None
    ) -> tuple[MemoryObjectSendStream[T_Item], MemoryObjectReceiveStream[T_Item]]:
        if max_buffer_size != math.inf and not isinstance(max_buffer_size, int):
            raise ValueError("max_buffer_size must be either an integer or math.inf")
        if max_buffer_size < 0:
            raise ValueError("max_buffer_size cannot be negative")
        if item_type is not None:
            warn(
                "The item_type argument has been deprecated in AnyIO 4.0. "
                "Use create_memory_object_stream[YourItemType](...) instead.",
        state = _MemoryObjectStreamState[T_Item](max_buffer_size)
        return (MemoryObjectSendStream(state), MemoryObjectReceiveStream(state))
from typing import Any, Generic, TypeVar, Union
from .._core._exceptions import EndOfStream
from .._core._typedattr import TypedAttributeProvider
from ._resources import AsyncResource
from ._tasks import TaskGroup
T_co = TypeVar("T_co", covariant=True)
T_contra = TypeVar("T_contra", contravariant=True)
class UnreliableObjectReceiveStream(
    Generic[T_co], AsyncResource, TypedAttributeProvider
    An interface for receiving objects.
    This interface makes no guarantees that the received messages arrive in the order in
    which they were sent, or that no messages are missed.
    Asynchronously iterating over objects of this type will yield objects matching the
    given type parameter.
    def __aiter__(self) -> UnreliableObjectReceiveStream[T_co]:
    async def __anext__(self) -> T_co:
            return await self.receive()
        except EndOfStream:
            raise StopAsyncIteration from None
    async def receive(self) -> T_co:
        Receive the next item.
        :raises ~anyio.ClosedResourceError: if the receive stream has been explicitly
            closed
        :raises ~anyio.EndOfStream: if this stream has been closed from the other end
        :raises ~anyio.BrokenResourceError: if this stream has been rendered unusable
            due to external causes
class UnreliableObjectSendStream(
    Generic[T_contra], AsyncResource, TypedAttributeProvider
    An interface for sending objects.
    This interface makes no guarantees that the messages sent will reach the
    recipient(s) in the same order in which they were sent, or at all.
    async def send(self, item: T_contra) -> None:
        Send an item to the peer(s).
        :param item: the item to send
        :raises ~anyio.ClosedResourceError: if the send stream has been explicitly
class UnreliableObjectStream(
    UnreliableObjectReceiveStream[T_Item], UnreliableObjectSendStream[T_Item]
    A bidirectional message stream which does not guarantee the order or reliability of
    message delivery.
class ObjectReceiveStream(UnreliableObjectReceiveStream[T_co]):
    A receive message stream which guarantees that messages are received in the same
    order in which they were sent, and that no messages are missed.
class ObjectSendStream(UnreliableObjectSendStream[T_contra]):
    A send message stream which guarantees that messages are delivered in the same order
    in which they were sent, without missing any messages in the middle.
class ObjectStream(
    ObjectReceiveStream[T_Item],
    ObjectSendStream[T_Item],
    UnreliableObjectStream[T_Item],
    A bidirectional message stream which guarantees the order and reliability of message
    delivery.
    async def send_eof(self) -> None:
        Send an end-of-file indication to the peer.
        You should not try to send any further data to this stream after calling this
        method. This method is idempotent (does nothing on successive calls).
class ByteReceiveStream(AsyncResource, TypedAttributeProvider):
    An interface for receiving bytes from a single peer.
    Iterating this byte stream will yield a byte string of arbitrary length, but no more
    than 65536 bytes.
    def __aiter__(self) -> ByteReceiveStream:
    async def __anext__(self) -> bytes:
    async def receive(self, max_bytes: int = 65536) -> bytes:
        Receive at most ``max_bytes`` bytes from the peer.
        .. note:: Implementers of this interface should not return an empty
            :class:`bytes` object, and users should ignore them.
        :param max_bytes: maximum number of bytes to receive
        :return: the received bytes
class ByteSendStream(AsyncResource, TypedAttributeProvider):
    """An interface for sending bytes to a single peer."""
    async def send(self, item: bytes) -> None:
        Send the given bytes to the peer.
        :param item: the bytes to send
class ByteStream(ByteReceiveStream, ByteSendStream):
    """A bidirectional byte stream."""
#: Type alias for all unreliable bytes-oriented receive streams.
AnyUnreliableByteReceiveStream: TypeAlias = Union[
    UnreliableObjectReceiveStream[bytes], ByteReceiveStream
#: Type alias for all unreliable bytes-oriented send streams.
AnyUnreliableByteSendStream: TypeAlias = Union[
    UnreliableObjectSendStream[bytes], ByteSendStream
#: Type alias for all unreliable bytes-oriented streams.
AnyUnreliableByteStream: TypeAlias = Union[UnreliableObjectStream[bytes], ByteStream]
#: Type alias for all bytes-oriented receive streams.
AnyByteReceiveStream: TypeAlias = Union[ObjectReceiveStream[bytes], ByteReceiveStream]
#: Type alias for all bytes-oriented send streams.
AnyByteSendStream: TypeAlias = Union[ObjectSendStream[bytes], ByteSendStream]
#: Type alias for all bytes-oriented streams.
AnyByteStream: TypeAlias = Union[ObjectStream[bytes], ByteStream]
class Listener(Generic[T_co], AsyncResource, TypedAttributeProvider):
    """An interface for objects that let you accept incoming connections."""
    async def serve(
        self, handler: Callable[[T_co], Any], task_group: TaskGroup | None = None
        Accept incoming connections as they come in and start tasks to handle them.
        :param handler: a callable that will be used to handle each accepted connection
        :param task_group: the task group that will be used to start tasks for handling
            each accepted connection (if omitted, an ad-hoc task group will be created)
class ObjectStreamConnectable(Generic[T_co], metaclass=ABCMeta):
    async def connect(self) -> ObjectStream[T_co]:
        Connect to the remote endpoint.
        :return: an object stream connected to the remote end
        :raises ConnectionFailed: if the connection fails
class ByteStreamConnectable(metaclass=ABCMeta):
    async def connect(self) -> ByteStream:
        :return: a bytestream connected to the remote end
#: Type alias for all connectables returning bytestreams or bytes-oriented object streams
AnyByteStreamConnectable: TypeAlias = Union[
    ObjectStreamConnectable[bytes], ByteStreamConnectable
