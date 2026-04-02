#!/usr/bin/env rye run python
from openai import OpenAI
# gets OPENAI_API_KEY from your environment variables
openai = OpenAI()
speech_file_path = Path(__file__).parent / "speech.mp3"
def main() -> None:
    # Create text-to-speech audio file
    with openai.audio.speech.with_streaming_response.create(
        model="tts-1",
        voice="alloy",
        input="the quick brown fox jumped over the lazy dogs",
        response.stream_to_file(speech_file_path)
    # Create transcription from audio file
    transcription = openai.audio.transcriptions.create(
        model="whisper-1",
        file=speech_file_path,
    print(transcription.text)
    # Create translation from audio file
    translation = openai.audio.translations.create(
    print(translation.text)
from .._utils import get_client, print_model
from ..._types import omit
from .._progress import BufferReader
from ...types.audio import Transcription
    from argparse import _SubParsersAction
def register(subparser: _SubParsersAction[ArgumentParser]) -> None:
    # transcriptions
    sub = subparser.add_parser("audio.transcriptions.create")
    # Required
    sub.add_argument("-m", "--model", type=str, default="whisper-1")
    sub.add_argument("-f", "--file", type=str, required=True)
    # Optional
    sub.add_argument("--response-format", type=str)
    sub.add_argument("--language", type=str)
    sub.add_argument("-t", "--temperature", type=float)
    sub.add_argument("--prompt", type=str)
    sub.set_defaults(func=CLIAudio.transcribe, args_model=CLITranscribeArgs)
    # translations
    sub = subparser.add_parser("audio.translations.create")
    # TODO: doesn't seem to be supported by the API
    # sub.add_argument("--language", type=str)
    sub.set_defaults(func=CLIAudio.translate, args_model=CLITranslationArgs)
class CLITranscribeArgs(BaseModel):
    model: str
    file: str
    response_format: Optional[str] = None
    language: Optional[str] = None
    temperature: Optional[float] = None
    prompt: Optional[str] = None
class CLITranslationArgs(BaseModel):
class CLIAudio:
    def transcribe(args: CLITranscribeArgs) -> None:
        with open(args.file, "rb") as file_reader:
            buffer_reader = BufferReader(file_reader.read(), desc="Upload progress")
        model = cast(
            "Transcription | str",
            get_client().audio.transcriptions.create(
                file=(args.file, buffer_reader),
                language=args.language or omit,
                temperature=args.temperature or omit,
                prompt=args.prompt or omit,
                # casts required because the API is typed for enums
                # but we don't want to validate that here for forwards-compat
                response_format=cast(Any, args.response_format),
        if isinstance(model, str):
            sys.stdout.write(model + "\n")
            print_model(model)
    def translate(args: CLITranslationArgs) -> None:
            get_client().audio.translations.create(
from ..._compat import cached_property
from ..._resource import SyncAPIResource, AsyncAPIResource
__all__ = ["Audio", "AsyncAudio"]
class Audio(SyncAPIResource):
    def transcriptions(self) -> Transcriptions:
        """Turn audio into text or text into audio."""
        return Transcriptions(self._client)
    def translations(self) -> Translations:
        return Translations(self._client)
    def speech(self) -> Speech:
        return Speech(self._client)
    def with_raw_response(self) -> AudioWithRawResponse:
        return AudioWithRawResponse(self)
    def with_streaming_response(self) -> AudioWithStreamingResponse:
        return AudioWithStreamingResponse(self)
class AsyncAudio(AsyncAPIResource):
    def transcriptions(self) -> AsyncTranscriptions:
        return AsyncTranscriptions(self._client)
    def translations(self) -> AsyncTranslations:
        return AsyncTranslations(self._client)
    def speech(self) -> AsyncSpeech:
        return AsyncSpeech(self._client)
    def with_raw_response(self) -> AsyncAudioWithRawResponse:
        return AsyncAudioWithRawResponse(self)
    def with_streaming_response(self) -> AsyncAudioWithStreamingResponse:
        return AsyncAudioWithStreamingResponse(self)
class AudioWithRawResponse:
    def __init__(self, audio: Audio) -> None:
        self._audio = audio
    def transcriptions(self) -> TranscriptionsWithRawResponse:
        return TranscriptionsWithRawResponse(self._audio.transcriptions)
    def translations(self) -> TranslationsWithRawResponse:
        return TranslationsWithRawResponse(self._audio.translations)
    def speech(self) -> SpeechWithRawResponse:
        return SpeechWithRawResponse(self._audio.speech)
class AsyncAudioWithRawResponse:
    def __init__(self, audio: AsyncAudio) -> None:
    def transcriptions(self) -> AsyncTranscriptionsWithRawResponse:
        return AsyncTranscriptionsWithRawResponse(self._audio.transcriptions)
    def translations(self) -> AsyncTranslationsWithRawResponse:
        return AsyncTranslationsWithRawResponse(self._audio.translations)
    def speech(self) -> AsyncSpeechWithRawResponse:
        return AsyncSpeechWithRawResponse(self._audio.speech)
class AudioWithStreamingResponse:
    def transcriptions(self) -> TranscriptionsWithStreamingResponse:
        return TranscriptionsWithStreamingResponse(self._audio.transcriptions)
    def translations(self) -> TranslationsWithStreamingResponse:
        return TranslationsWithStreamingResponse(self._audio.translations)
    def speech(self) -> SpeechWithStreamingResponse:
        return SpeechWithStreamingResponse(self._audio.speech)
class AsyncAudioWithStreamingResponse:
    def transcriptions(self) -> AsyncTranscriptionsWithStreamingResponse:
        return AsyncTranscriptionsWithStreamingResponse(self._audio.transcriptions)
    def translations(self) -> AsyncTranslationsWithStreamingResponse:
        return AsyncTranslationsWithStreamingResponse(self._audio.translations)
    def speech(self) -> AsyncSpeechWithStreamingResponse:
        return AsyncSpeechWithStreamingResponse(self._audio.speech)
class Audio(OneDriveObjectBase):
    def album(self):
        """Gets and sets the album
                The album
        if "album" in self._prop_dict:
            return self._prop_dict["album"]
    @album.setter
    def album(self, val):
        self._prop_dict["album"] = val
    def album_artist(self):
        """Gets and sets the albumArtist
                The albumArtist
        if "albumArtist" in self._prop_dict:
            return self._prop_dict["albumArtist"]
    @album_artist.setter
    def album_artist(self, val):
        self._prop_dict["albumArtist"] = val
    def artist(self):
        """Gets and sets the artist
                The artist
        if "artist" in self._prop_dict:
            return self._prop_dict["artist"]
    @artist.setter
    def artist(self, val):
        self._prop_dict["artist"] = val
    def bitrate(self):
        """Gets and sets the bitrate
            int:
                The bitrate
        if "bitrate" in self._prop_dict:
            return self._prop_dict["bitrate"]
    @bitrate.setter
    def bitrate(self, val):
        self._prop_dict["bitrate"] = val
    def composers(self):
        """Gets and sets the composers
                The composers
        if "composers" in self._prop_dict:
            return self._prop_dict["composers"]
    @composers.setter
    def composers(self, val):
        self._prop_dict["composers"] = val
    def copyright(self):
        """Gets and sets the copyright
                The copyright
        if "copyright" in self._prop_dict:
            return self._prop_dict["copyright"]
    @copyright.setter
    def copyright(self, val):
        self._prop_dict["copyright"] = val
    def disc(self):
        """Gets and sets the disc
                The disc
        if "disc" in self._prop_dict:
            return self._prop_dict["disc"]
    @disc.setter
    def disc(self, val):
        self._prop_dict["disc"] = val
    def disc_count(self):
        """Gets and sets the discCount
                The discCount
        if "discCount" in self._prop_dict:
            return self._prop_dict["discCount"]
    @disc_count.setter
    def disc_count(self, val):
        self._prop_dict["discCount"] = val
    def duration(self):
        """Gets and sets the duration
                The duration
        if "duration" in self._prop_dict:
            return self._prop_dict["duration"]
    @duration.setter
    def duration(self, val):
        self._prop_dict["duration"] = val
    def genre(self):
        """Gets and sets the genre
                The genre
        if "genre" in self._prop_dict:
            return self._prop_dict["genre"]
    @genre.setter
    def genre(self, val):
        self._prop_dict["genre"] = val
    def has_drm(self):
        """Gets and sets the hasDrm
            bool:
                The hasDrm
        if "hasDrm" in self._prop_dict:
            return self._prop_dict["hasDrm"]
    @has_drm.setter
    def has_drm(self, val):
        self._prop_dict["hasDrm"] = val
    def is_variable_bitrate(self):
        """Gets and sets the isVariableBitrate
                The isVariableBitrate
        if "isVariableBitrate" in self._prop_dict:
            return self._prop_dict["isVariableBitrate"]
    @is_variable_bitrate.setter
    def is_variable_bitrate(self, val):
        self._prop_dict["isVariableBitrate"] = val
    def title(self):
        """Gets and sets the title
                The title
        if "title" in self._prop_dict:
            return self._prop_dict["title"]
    @title.setter
    def title(self, val):
        self._prop_dict["title"] = val
    def track(self):
        """Gets and sets the track
                The track
        if "track" in self._prop_dict:
            return self._prop_dict["track"]
    @track.setter
    def track(self, val):
        self._prop_dict["track"] = val
    def track_count(self):
        """Gets and sets the trackCount
                The trackCount
        if "trackCount" in self._prop_dict:
            return self._prop_dict["trackCount"]
    @track_count.setter
    def track_count(self, val):
        self._prop_dict["trackCount"] = val
    def year(self):
        """Gets and sets the year
                The year
        if "year" in self._prop_dict:
            return self._prop_dict["year"]
    @year.setter
    def year(self, val):
        self._prop_dict["year"] = val
    from langchain_community.document_loaders.parsers.audio import (
        OpenAIWhisperParser,
        OpenAIWhisperParserLocal,
        YandexSTTParser,
    "OpenAIWhisperParserLocal": "langchain_community.document_loaders.parsers.audio",
    "YandexSTTParser": "langchain_community.document_loaders.parsers.audio",
    "OpenAIWhisperParserLocal",
    "YandexSTTParser",
from typing import Optional, Union
# import webrtcvad
import librosa
from scipy.ndimage.morphology import binary_dilation
from TTS.vc.modules.freevc.speaker_encoder.hparams import *
int16_max = (2**15) - 1
def preprocess_wav(fpath_or_wav: Union[str, Path, np.ndarray], source_sr: Optional[int] = None):
    Applies the preprocessing operations used in training the Speaker Encoder to a waveform
    either on disk or in memory. The waveform will be resampled to match the data hyperparameters.
    :param fpath_or_wav: either a filepath to an audio file (many extensions are supported, not
    just .wav), either the waveform as a numpy array of floats.
    :param source_sr: if passing an audio waveform, the sampling rate of the waveform before
    preprocessing. After preprocessing, the waveform's sampling rate will match the data
    hyperparameters. If passing a filepath, the sampling rate will be automatically detected and
    this argument will be ignored.
    # Load the wav from disk if needed
    if isinstance(fpath_or_wav, str) or isinstance(fpath_or_wav, Path):
        wav, source_sr = librosa.load(fpath_or_wav, sr=None)
        wav = fpath_or_wav
    # Resample the wav if needed
    if source_sr is not None and source_sr != sampling_rate:
        wav = librosa.resample(wav, source_sr, sampling_rate)
    # Apply the preprocessing: normalize volume and shorten long silences
    wav = normalize_volume(wav, audio_norm_target_dBFS, increase_only=True)
    wav = trim_long_silences(wav)
def wav_to_mel_spectrogram(wav):
    Derives a mel spectrogram ready to be used by the encoder from a preprocessed audio waveform.
    Note: this not a log-mel spectrogram.
    frames = librosa.feature.melspectrogram(
        y=wav,
        sr=sampling_rate,
        n_fft=int(sampling_rate * mel_window_length / 1000),
        hop_length=int(sampling_rate * mel_window_step / 1000),
        n_mels=mel_n_channels,
    return frames.astype(np.float32).T
def normalize_volume(wav, target_dBFS, increase_only=False, decrease_only=False):
    if increase_only and decrease_only:
        raise ValueError("Both increase only and decrease only are set")
    dBFS_change = target_dBFS - 10 * np.log10(np.mean(wav**2))
    if (dBFS_change < 0 and increase_only) or (dBFS_change > 0 and decrease_only):
    return wav * (10 ** (dBFS_change / 20))
