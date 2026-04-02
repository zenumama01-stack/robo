__all__ = ["RealtimeCallIncomingWebhookEvent", "Data", "DataSipHeader"]
class DataSipHeader(BaseModel):
    """A header from the SIP Invite."""
    """Name of the SIP Header."""
    """Value of the SIP Header."""
    """The unique ID of this call."""
    sip_headers: List[DataSipHeader]
    """Headers from the SIP Invite."""
class RealtimeCallIncomingWebhookEvent(BaseModel):
    """Sent when Realtime API Receives a incoming SIP call."""
    """The Unix timestamp (in seconds) of when the model response was completed."""
    type: Literal["realtime.call.incoming"]
    """The type of the event. Always `realtime.call.incoming`."""
