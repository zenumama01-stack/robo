# mypy: ignore-errors
from typing import Any, Union, Callable, AsyncGenerator, cast
from typing_extensions import TYPE_CHECKING
from .. import _legacy_response
from .._extras import numpy as np, sounddevice as sd
from .._response import StreamedBinaryAPIResponse, AsyncStreamedBinaryAPIResponse
    import numpy.typing as npt
class LocalAudioPlayer:
        should_stop: Union[Callable[[], bool], None] = None,
        self.channels = 1
        self.dtype = np.float32
        self.should_stop = should_stop
    async def _tts_response_to_buffer(
        response: Union[
            _legacy_response.HttpxBinaryResponseContent,
            AsyncStreamedBinaryAPIResponse,
            StreamedBinaryAPIResponse,
    ) -> npt.NDArray[np.float32]:
        chunks: list[bytes] = []
        if isinstance(response, _legacy_response.HttpxBinaryResponseContent) or isinstance(
            response, StreamedBinaryAPIResponse
            for chunk in response.iter_bytes(chunk_size=1024):
                if chunk:
                    chunks.append(chunk)
            async for chunk in response.iter_bytes(chunk_size=1024):
        audio_bytes = b"".join(chunks)
        audio_np = np.frombuffer(audio_bytes, dtype=np.int16).astype(np.float32) / 32767.0
        audio_np = audio_np.reshape(-1, 1)
        return audio_np
    async def play(
        input: Union[
            npt.NDArray[np.int16],
            npt.NDArray[np.float32],
        audio_content: npt.NDArray[np.float32]
        if isinstance(input, np.ndarray):
            if input.dtype == np.int16 and self.dtype == np.float32:
                audio_content = (input.astype(np.float32) / 32767.0).reshape(-1, self.channels)
            elif input.dtype == np.float32:
                audio_content = cast("npt.NDArray[np.float32]", input)
                raise ValueError(f"Unsupported dtype: {input.dtype}")
            audio_content = await self._tts_response_to_buffer(input)
        loop = asyncio.get_event_loop()
        event = asyncio.Event()
        idx = 0
        def callback(
            outdata: npt.NDArray[np.float32],
            frame_count: int,
            _time_info: Any,
            _status: Any,
            nonlocal idx
            remainder = len(audio_content) - idx
            if remainder == 0 or (callable(self.should_stop) and self.should_stop()):
                loop.call_soon_threadsafe(event.set)
                raise sd.CallbackStop
            valid_frames = frame_count if remainder >= frame_count else remainder
            outdata[:valid_frames] = audio_content[idx : idx + valid_frames]
            outdata[valid_frames:] = 0
            idx += valid_frames
        stream = sd.OutputStream(
            callback=callback,
            dtype=audio_content.dtype,
            channels=audio_content.shape[1],
        with stream:
            await event.wait()
    async def play_stream(
        buffer_stream: AsyncGenerator[Union[npt.NDArray[np.float32], npt.NDArray[np.int16], None], None],
        buffer_queue: queue.Queue[Union[npt.NDArray[np.float32], npt.NDArray[np.int16], None]] = queue.Queue(maxsize=50)
        async def buffer_producer():
            async for buffer in buffer_stream:
                if buffer is None:
                await loop.run_in_executor(None, buffer_queue.put, buffer)
            await loop.run_in_executor(None, buffer_queue.put, None)  # Signal completion
            nonlocal current_buffer, buffer_pos
            frames_written = 0
            while frames_written < frame_count:
                if current_buffer is None or buffer_pos >= len(current_buffer):
                        current_buffer = buffer_queue.get(timeout=0.1)
                        if current_buffer is None:
                        buffer_pos = 0
                        if current_buffer.dtype == np.int16 and self.dtype == np.float32:
                            current_buffer = (current_buffer.astype(np.float32) / 32767.0).reshape(-1, self.channels)
                    except queue.Empty:
                        outdata[frames_written:] = 0
                remaining_frames = len(current_buffer) - buffer_pos
                frames_to_write = min(frame_count - frames_written, remaining_frames)
                outdata[frames_written : frames_written + frames_to_write] = current_buffer[
                    buffer_pos : buffer_pos + frames_to_write
                buffer_pos += frames_to_write
                frames_written += frames_to_write
        current_buffer = None
        producer_task = asyncio.create_task(buffer_producer())
        with sd.OutputStream(
            channels=self.channels,
            dtype=self.dtype,
        await producer_task
