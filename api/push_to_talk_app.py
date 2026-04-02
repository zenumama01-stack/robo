#!/usr/bin/env uv run
####################################################################
# Sample TUI app with a push to talk interface to the Realtime API #
# If you have `uv` installed and the `OPENAI_API_KEY`              #
# environment variable set, you can run this example with just     #
#                                                                  #
# `./examples/realtime/push_to_talk_app.py`                        #
# On Mac, you'll also need `brew install portaudio ffmpeg`           #
# /// script
# requires-python = ">=3.9"
# dependencies = [
#     "textual",
#     "numpy",
#     "pyaudio",
#     "pydub",
#     "sounddevice",
#     "openai[realtime]",
# ]
# [tool.uv.sources]
# openai = { path = "../../", editable = true }
# ///
from typing import Any, cast
from typing_extensions import override
from textual import events
from audio_util import CHANNELS, SAMPLE_RATE, AudioPlayerAsync
from textual.app import App, ComposeResult
from textual.widgets import Button, Static, RichLog
from textual.reactive import reactive
from textual.containers import Container
from openai.types.realtime.session import Session
class SessionDisplay(Static):
    """A widget that shows the current session ID."""
    session_id = reactive("")
    @override
    def render(self) -> str:
        return f"Session ID: {self.session_id}" if self.session_id else "Connecting..."
class AudioStatusIndicator(Static):
    """A widget that shows the current audio recording status."""
    is_recording = reactive(False)
        status = (
            "🔴 Recording... (Press K to stop)" if self.is_recording else "⚪ Press K to start recording (Q to quit)"
class RealtimeApp(App[None]):
    CSS = """
        Screen {
            background: #1a1b26;  /* Dark blue-grey background */
        Container {
            border: double rgb(91, 164, 91);
        Horizontal {
        #input-container {
            height: 5;  /* Explicit height for input container */
            margin: 1 1;
            padding: 1 2;
        Input {
            height: 3;  /* Explicit height for input */
        Button {
            height: 3;  /* Explicit height for button */
        #bottom-pane {
            height: 82%;  /* Reduced to make room for session display */
            border: round rgb(205, 133, 63);
            content-align: center middle;
        #status-indicator {
            height: 3;
            background: #2a2b36;
            border: solid rgb(91, 164, 91);
        #session-display {
        Static {
    client: AsyncOpenAI
    should_send_audio: asyncio.Event
    audio_player: AudioPlayerAsync
    last_audio_item_id: str | None
    connection: AsyncRealtimeConnection | None
    session: Session | None
    connected: asyncio.Event
        self.connection = None
        self.client = AsyncOpenAI()
        self.audio_player = AudioPlayerAsync()
        self.last_audio_item_id = None
        self.should_send_audio = asyncio.Event()
        self.connected = asyncio.Event()
    def compose(self) -> ComposeResult:
        """Create child widgets for the app."""
        with Container():
            yield SessionDisplay(id="session-display")
            yield AudioStatusIndicator(id="status-indicator")
            yield RichLog(id="bottom-pane", wrap=True, highlight=True, markup=True)
    async def on_mount(self) -> None:
        self.run_worker(self.handle_realtime_connection())
        self.run_worker(self.send_mic_audio())
    async def handle_realtime_connection(self) -> None:
        async with self.client.realtime.connect(model="gpt-realtime") as conn:
            self.connection = conn
            self.connected.set()
            # note: this is the default and can be omitted
            # if you want to manually handle VAD yourself, then set `'turn_detection': None`
            await conn.session.update(
                    "audio": {
                        "input": {"turn_detection": {"type": "server_vad"}},
                    "model": "gpt-realtime",
            acc_items: dict[str, Any] = {}
            async for event in conn:
                if event.type == "session.created":
                    self.session = event.session
                    session_display = self.query_one(SessionDisplay)
                    assert event.session.id is not None
                    session_display.session_id = event.session.id
                if event.type == "session.updated":
                if event.type == "response.output_audio.delta":
                    if event.item_id != self.last_audio_item_id:
                        self.audio_player.reset_frame_count()
                        self.last_audio_item_id = event.item_id
                    bytes_data = base64.b64decode(event.delta)
                    self.audio_player.add_data(bytes_data)
                if event.type == "response.output_audio_transcript.delta":
                        text = acc_items[event.item_id]
                        acc_items[event.item_id] = event.delta
                        acc_items[event.item_id] = text + event.delta
                    # Clear and update the entire content because RichLog otherwise treats each delta as a new line
                    bottom_pane = self.query_one("#bottom-pane", RichLog)
                    bottom_pane.clear()
                    bottom_pane.write(acc_items[event.item_id])
    async def _get_connection(self) -> AsyncRealtimeConnection:
        await self.connected.wait()
        assert self.connection is not None
        return self.connection
    async def send_mic_audio(self) -> None:
        import sounddevice as sd  # type: ignore
        status_indicator = self.query_one(AudioStatusIndicator)
                await self.should_send_audio.wait()
                status_indicator.is_recording = True
                connection = await self._get_connection()
                if not sent_audio:
                    asyncio.create_task(connection.send({"type": "response.cancel"}))
                await connection.input_audio_buffer.append(audio=base64.b64encode(cast(Any, data)).decode("utf-8"))
    async def on_key(self, event: events.Key) -> None:
        """Handle key press events."""
        if event.key == "enter":
            self.query_one(Button).press()
        if event.key == "q":
            self.exit()
        if event.key == "k":
            if status_indicator.is_recording:
                self.should_send_audio.clear()
                status_indicator.is_recording = False
                if self.session and self.session.turn_detection is None:
                    # The default in the API is that the model will automatically detect when the user has
                    # stopped talking and then start responding itself.
                    # However if we're in manual `turn_detection` mode then we need to
                    # manually tell the model to commit the audio buffer and start responding.
                    conn = await self._get_connection()
                    await conn.input_audio_buffer.commit()
                    await conn.response.create()
                self.should_send_audio.set()
    app = RealtimeApp()
