#: All the potential fields for :class:`Poll` objects
POLL_FIELDS = [
    "duration_minutes",
    "end_datetime",
    "options",
    "voting_status",
class Poll(HashableID, DataMapping):
    """A poll included in a Tweet is not a primary object on any endpoint, but
    can be found and expanded in the Tweet object. 
    ``?expansions=attachments.poll_ids`` to get the condensed object with only
    default fields. Use the expansion with the field parameter: ``poll.fields``
    when requesting additional fields to complete the object.
        The JSON data representing the poll.
        Unique identifier of the expanded poll.
    options : list
        Contains objects describing each choice in the referenced poll.
    duration_minutes : int | None
        Specifies the total duration of this poll.
    end_datetime : datetime.datetime | None
        Specifies the end date and time for this poll.
    voting_status : str | None
        Indicates if this poll is still active and can receive votes, or if the
        voting is now closed.
    https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/poll
        "data", "id", "options", "duration_minutes", "end_datetime",
        "voting_status"
        self.options = data["options"]
        self.duration_minutes = data.get("duration_minutes")
        self.end_datetime = data.get("end_datetime")
        if self.end_datetime is not None:
            self.end_datetime = parse_datetime(self.end_datetime)
        self.voting_status = data.get("voting_status")
        return iter(self.options)
        return len(self.options)
        return f"<Poll id={self.id} options={self.options}>"
