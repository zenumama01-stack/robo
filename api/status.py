class Status(OneDriveObjectBase):
    def lockdown_date_time(self):
        """Gets and sets the lockdownDateTime
                The lockdownDateTime
        if "lockdownDateTime" in self._prop_dict:
            if '.' in self._prop_dict["lockdownDateTime"]:
                return datetime.strptime(self._prop_dict["lockdownDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["lockdownDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @lockdown_date_time.setter
    def lockdown_date_time(self, val):
        self._prop_dict["lockdownDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def lockdown_reasons(self):
        """Gets and sets the lockdownReasons
                The lockdownReasons
        if "lockdownReasons" in self._prop_dict:
            return self._prop_dict["lockdownReasons"]
    @lockdown_reasons.setter
    def lockdown_reasons(self, val):
        self._prop_dict["lockdownReasons"] = val
    def drive_deletion_date_time(self):
        """Gets and sets the driveDeletionDateTime
                The driveDeletionDateTime
        if "driveDeletionDateTime" in self._prop_dict:
            if '.' in self._prop_dict["driveDeletionDateTime"]:
                return datetime.strptime(self._prop_dict["driveDeletionDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["driveDeletionDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @drive_deletion_date_time.setter
    def drive_deletion_date_time(self, val):
        self._prop_dict["driveDeletionDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def last_unlock_date_time(self):
        """Gets and sets the lastUnlockDateTime
                The lastUnlockDateTime
        if "lastUnlockDateTime" in self._prop_dict:
            if '.' in self._prop_dict["lastUnlockDateTime"]:
                return datetime.strptime(self._prop_dict["lastUnlockDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["lastUnlockDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @last_unlock_date_time.setter
    def last_unlock_date_time(self, val):
        self._prop_dict["lastUnlockDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def user_unlocks(self):
        """Gets and sets the userUnlocks
                The userUnlocks
        if "userUnlocks" in self._prop_dict:
            return self._prop_dict["userUnlocks"]
    @user_unlocks.setter
    def user_unlocks(self, val):
        self._prop_dict["userUnlocks"] = val
    def user_unlocks_remaining(self):
        """Gets and sets the userUnlocksRemaining
                The userUnlocksRemaining
        if "userUnlocksRemaining" in self._prop_dict:
            return self._prop_dict["userUnlocksRemaining"]
    @user_unlocks_remaining.setter
    def user_unlocks_remaining(self, val):
        self._prop_dict["userUnlocksRemaining"] = val
    def support_agent_unlocks(self):
        """Gets and sets the supportAgentUnlocks
                The supportAgentUnlocks
        if "supportAgentUnlocks" in self._prop_dict:
            return self._prop_dict["supportAgentUnlocks"]
    @support_agent_unlocks.setter
    def support_agent_unlocks(self, val):
        self._prop_dict["supportAgentUnlocks"] = val
from typing import Optional, Type
from .console import Console, RenderableType
from .jupyter import JupyterMixin
from .live import Live
from .spinner import Spinner
from .style import StyleType
class Status(JupyterMixin):
    """Displays a status indicator with a 'spinner' animation.
        status (RenderableType): A status renderable (str or Text typically).
        console (Console, optional): Console instance to use, or None for global console. Defaults to None.
        spinner (str, optional): Name of spinner animation (see python -m rich.spinner). Defaults to "dots".
        spinner_style (StyleType, optional): Style of spinner. Defaults to "status.spinner".
        speed (float, optional): Speed factor for spinner animation. Defaults to 1.0.
        refresh_per_second (float, optional): Number of refreshes per second. Defaults to 12.5.
        status: RenderableType,
        console: Optional[Console] = None,
        spinner: str = "dots",
        spinner_style: StyleType = "status.spinner",
        refresh_per_second: float = 12.5,
        self.spinner_style = spinner_style
        self.speed = speed
        self._spinner = Spinner(spinner, text=status, style=spinner_style, speed=speed)
        self._live = Live(
            self.renderable,
            console=console,
            refresh_per_second=refresh_per_second,
            transient=True,
    def renderable(self) -> Spinner:
        return self._spinner
    def console(self) -> "Console":
        """Get the Console used by the Status objects."""
        return self._live.console
        status: Optional[RenderableType] = None,
        spinner: Optional[str] = None,
        spinner_style: Optional[StyleType] = None,
        speed: Optional[float] = None,
        """Update status.
            status (Optional[RenderableType], optional): New status renderable or None for no change. Defaults to None.
            spinner (Optional[str], optional): New spinner or None for no change. Defaults to None.
            spinner_style (Optional[StyleType], optional): New spinner style or None for no change. Defaults to None.
            speed (Optional[float], optional): Speed factor for spinner animation or None for no change. Defaults to None.
        if spinner_style is not None:
        if speed is not None:
        if spinner is not None:
            self._spinner = Spinner(
                spinner, text=self.status, style=self.spinner_style, speed=self.speed
            self._live.update(self.renderable, refresh=True)
            self._spinner.update(
                text=self.status, style=self.spinner_style, speed=self.speed
    def start(self) -> None:
        """Start the status animation."""
        self._live.start()
    def stop(self) -> None:
        """Stop the spinner animation."""
        self._live.stop()
    def __rich__(self) -> RenderableType:
        return self.renderable
    def __enter__(self) -> "Status":
        exc_type: Optional[Type[BaseException]],
        exc_val: Optional[BaseException],
        exc_tb: Optional[TracebackType],
        self.stop()
    with console.status("[magenta]Covid detector booting up") as status:
        sleep(3)
        console.log("Importing advanced AI")
        console.log("Advanced Covid AI Ready")
        status.update(status="[bold blue] Scanning for Covid", spinner="earth")
        console.log("Found 10,000,000,000 copies of Covid32.exe")
        status.update(
            status="[bold red]Moving Covid32.exe to Trash",
            spinner="bouncingBall",
            spinner_style="yellow",
        sleep(5)
    console.print("[bold green]Covid deleted successfully")
HTTP codes
See HTTP Status Code Registry:
https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
And RFC 9110 - https://www.rfc-editor.org/rfc/rfc9110
    "HTTP_100_CONTINUE",
    "HTTP_101_SWITCHING_PROTOCOLS",
    "HTTP_102_PROCESSING",
    "HTTP_103_EARLY_HINTS",
    "HTTP_200_OK",
    "HTTP_201_CREATED",
    "HTTP_202_ACCEPTED",
    "HTTP_203_NON_AUTHORITATIVE_INFORMATION",
    "HTTP_204_NO_CONTENT",
    "HTTP_205_RESET_CONTENT",
    "HTTP_206_PARTIAL_CONTENT",
    "HTTP_207_MULTI_STATUS",
    "HTTP_208_ALREADY_REPORTED",
    "HTTP_226_IM_USED",
    "HTTP_300_MULTIPLE_CHOICES",
    "HTTP_301_MOVED_PERMANENTLY",
    "HTTP_302_FOUND",
    "HTTP_303_SEE_OTHER",
    "HTTP_304_NOT_MODIFIED",
    "HTTP_305_USE_PROXY",
    "HTTP_306_RESERVED",
    "HTTP_307_TEMPORARY_REDIRECT",
    "HTTP_308_PERMANENT_REDIRECT",
    "HTTP_400_BAD_REQUEST",
    "HTTP_401_UNAUTHORIZED",
    "HTTP_402_PAYMENT_REQUIRED",
    "HTTP_403_FORBIDDEN",
    "HTTP_404_NOT_FOUND",
    "HTTP_405_METHOD_NOT_ALLOWED",
    "HTTP_406_NOT_ACCEPTABLE",
    "HTTP_407_PROXY_AUTHENTICATION_REQUIRED",
    "HTTP_408_REQUEST_TIMEOUT",
    "HTTP_409_CONFLICT",
    "HTTP_410_GONE",
    "HTTP_411_LENGTH_REQUIRED",
    "HTTP_412_PRECONDITION_FAILED",
    "HTTP_413_CONTENT_TOO_LARGE",
    "HTTP_414_URI_TOO_LONG",
    "HTTP_415_UNSUPPORTED_MEDIA_TYPE",
    "HTTP_416_RANGE_NOT_SATISFIABLE",
    "HTTP_417_EXPECTATION_FAILED",
    "HTTP_418_IM_A_TEAPOT",
    "HTTP_421_MISDIRECTED_REQUEST",
    "HTTP_422_UNPROCESSABLE_CONTENT",
    "HTTP_423_LOCKED",
    "HTTP_424_FAILED_DEPENDENCY",
    "HTTP_425_TOO_EARLY",
    "HTTP_426_UPGRADE_REQUIRED",
    "HTTP_428_PRECONDITION_REQUIRED",
    "HTTP_429_TOO_MANY_REQUESTS",
    "HTTP_431_REQUEST_HEADER_FIELDS_TOO_LARGE",
    "HTTP_451_UNAVAILABLE_FOR_LEGAL_REASONS",
    "HTTP_500_INTERNAL_SERVER_ERROR",
    "HTTP_501_NOT_IMPLEMENTED",
    "HTTP_502_BAD_GATEWAY",
    "HTTP_503_SERVICE_UNAVAILABLE",
    "HTTP_504_GATEWAY_TIMEOUT",
    "HTTP_505_HTTP_VERSION_NOT_SUPPORTED",
    "HTTP_506_VARIANT_ALSO_NEGOTIATES",
    "HTTP_507_INSUFFICIENT_STORAGE",
    "HTTP_508_LOOP_DETECTED",
    "HTTP_510_NOT_EXTENDED",
    "HTTP_511_NETWORK_AUTHENTICATION_REQUIRED",
    "WS_1000_NORMAL_CLOSURE",
    "WS_1001_GOING_AWAY",
    "WS_1002_PROTOCOL_ERROR",
    "WS_1003_UNSUPPORTED_DATA",
    "WS_1005_NO_STATUS_RCVD",
    "WS_1006_ABNORMAL_CLOSURE",
    "WS_1007_INVALID_FRAME_PAYLOAD_DATA",
    "WS_1008_POLICY_VIOLATION",
    "WS_1009_MESSAGE_TOO_BIG",
    "WS_1010_MANDATORY_EXT",
    "WS_1011_INTERNAL_ERROR",
    "WS_1012_SERVICE_RESTART",
    "WS_1013_TRY_AGAIN_LATER",
    "WS_1014_BAD_GATEWAY",
    "WS_1015_TLS_HANDSHAKE",
HTTP_100_CONTINUE = 100
HTTP_101_SWITCHING_PROTOCOLS = 101
HTTP_102_PROCESSING = 102
HTTP_103_EARLY_HINTS = 103
HTTP_200_OK = 200
HTTP_201_CREATED = 201
HTTP_202_ACCEPTED = 202
HTTP_203_NON_AUTHORITATIVE_INFORMATION = 203
HTTP_204_NO_CONTENT = 204
HTTP_205_RESET_CONTENT = 205
HTTP_206_PARTIAL_CONTENT = 206
HTTP_207_MULTI_STATUS = 207
HTTP_208_ALREADY_REPORTED = 208
HTTP_226_IM_USED = 226
HTTP_300_MULTIPLE_CHOICES = 300
HTTP_301_MOVED_PERMANENTLY = 301
HTTP_302_FOUND = 302
HTTP_303_SEE_OTHER = 303
HTTP_304_NOT_MODIFIED = 304
HTTP_305_USE_PROXY = 305
HTTP_306_RESERVED = 306
HTTP_307_TEMPORARY_REDIRECT = 307
HTTP_308_PERMANENT_REDIRECT = 308
HTTP_400_BAD_REQUEST = 400
HTTP_401_UNAUTHORIZED = 401
HTTP_402_PAYMENT_REQUIRED = 402
HTTP_403_FORBIDDEN = 403
HTTP_404_NOT_FOUND = 404
HTTP_405_METHOD_NOT_ALLOWED = 405
HTTP_406_NOT_ACCEPTABLE = 406
HTTP_407_PROXY_AUTHENTICATION_REQUIRED = 407
HTTP_408_REQUEST_TIMEOUT = 408
HTTP_409_CONFLICT = 409
HTTP_410_GONE = 410
HTTP_411_LENGTH_REQUIRED = 411
HTTP_412_PRECONDITION_FAILED = 412
HTTP_413_CONTENT_TOO_LARGE = 413
HTTP_414_URI_TOO_LONG = 414
HTTP_415_UNSUPPORTED_MEDIA_TYPE = 415
HTTP_416_RANGE_NOT_SATISFIABLE = 416
HTTP_417_EXPECTATION_FAILED = 417
HTTP_418_IM_A_TEAPOT = 418
HTTP_421_MISDIRECTED_REQUEST = 421
HTTP_422_UNPROCESSABLE_CONTENT = 422
HTTP_423_LOCKED = 423
HTTP_424_FAILED_DEPENDENCY = 424
HTTP_425_TOO_EARLY = 425
HTTP_426_UPGRADE_REQUIRED = 426
HTTP_428_PRECONDITION_REQUIRED = 428
HTTP_429_TOO_MANY_REQUESTS = 429
HTTP_431_REQUEST_HEADER_FIELDS_TOO_LARGE = 431
HTTP_451_UNAVAILABLE_FOR_LEGAL_REASONS = 451
HTTP_500_INTERNAL_SERVER_ERROR = 500
HTTP_501_NOT_IMPLEMENTED = 501
HTTP_502_BAD_GATEWAY = 502
HTTP_503_SERVICE_UNAVAILABLE = 503
HTTP_504_GATEWAY_TIMEOUT = 504
HTTP_505_HTTP_VERSION_NOT_SUPPORTED = 505
HTTP_506_VARIANT_ALSO_NEGOTIATES = 506
HTTP_507_INSUFFICIENT_STORAGE = 507
HTTP_508_LOOP_DETECTED = 508
HTTP_510_NOT_EXTENDED = 510
HTTP_511_NETWORK_AUTHENTICATION_REQUIRED = 511
WebSocket codes
https://www.iana.org/assignments/websocket/websocket.xml#close-code-number
https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent
WS_1000_NORMAL_CLOSURE = 1000
WS_1001_GOING_AWAY = 1001
WS_1002_PROTOCOL_ERROR = 1002
WS_1003_UNSUPPORTED_DATA = 1003
WS_1005_NO_STATUS_RCVD = 1005
WS_1006_ABNORMAL_CLOSURE = 1006
WS_1007_INVALID_FRAME_PAYLOAD_DATA = 1007
WS_1008_POLICY_VIOLATION = 1008
WS_1009_MESSAGE_TOO_BIG = 1009
WS_1010_MANDATORY_EXT = 1010
WS_1011_INTERNAL_ERROR = 1011
WS_1012_SERVICE_RESTART = 1012
WS_1013_TRY_AGAIN_LATER = 1013
WS_1014_BAD_GATEWAY = 1014
WS_1015_TLS_HANDSHAKE = 1015
__deprecated__ = {
    "HTTP_413_REQUEST_ENTITY_TOO_LARGE": 413,
    "HTTP_414_REQUEST_URI_TOO_LONG": 414,
    "HTTP_416_REQUESTED_RANGE_NOT_SATISFIABLE": 416,
    "HTTP_422_UNPROCESSABLE_ENTITY": 422,
def __getattr__(name: str) -> int:
    deprecation_changes = {
        "HTTP_413_REQUEST_ENTITY_TOO_LARGE": "HTTP_413_CONTENT_TOO_LARGE",
        "HTTP_414_REQUEST_URI_TOO_LONG": "HTTP_414_URI_TOO_LONG",
        "HTTP_416_REQUESTED_RANGE_NOT_SATISFIABLE": "HTTP_416_RANGE_NOT_SATISFIABLE",
        "HTTP_422_UNPROCESSABLE_ENTITY": "HTTP_422_UNPROCESSABLE_CONTENT",
    deprecated = __deprecated__.get(name)
    if deprecated:
            f"'{name}' is deprecated. Use '{deprecation_changes[name]}' instead.",
        return deprecated
    raise AttributeError(f"module 'starlette.status' has no attribute '{name}'")
    return sorted(list(__all__) + list(__deprecated__.keys()))  # pragma: no cover
