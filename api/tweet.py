#: All the potential publically-available fields for :class:`Tweet` objects
PUBLIC_TWEET_FIELDS = [
    "author_id",
    "context_annotations",
    "conversation_id",
    "edit_controls",
    "edit_history_tweet_ids",
    "entities",
    "in_reply_to_user_id",
    "possibly_sensitive",
    "reply_settings",
    "source",
    "withheld",
#: All the potential fields for :class:`Tweet` objects
TWEET_FIELDS = PUBLIC_TWEET_FIELDS + [
class Tweet(HashableID, DataMapping):
    """Tweets are the basic building block of all things Twitter. The Tweet
    object has a long list of ‘root-level’ fields, such as ``id``, ``text``,
    and ``created_at``. Tweet objects are also the ‘parent’ object to several
    child objects including ``user``, ``media``, ``poll``, and ``place``. Use
    the field parameter ``tweet.fields`` when requesting these root-level
    fields on the Tweet object.
    The Tweet object that can be found and expanded in the user resource.
    Additional Tweets related to the requested Tweet can also be found and
    expanded in the Tweet resource. The object is available for expansion with
    ``?expansions=pinned_tweet_id`` in the user resource or
    ``?expansions=referenced_tweets.id`` in the Tweet resource to get the
    object with only default fields. Use the expansion with the field
    parameter: ``tweet.fields`` when requesting additional fields to complete
    the object.
        Added ``edit_history_tweet_ids`` and ``edit_controls`` fields
        The JSON data representing the Tweet.
        The unique identifier of the requested Tweet.
    text : str
        The actual UTF-8 text of the Tweet. See `twitter-text`_ for details on
        what characters are currently considered valid.
    edit_history_tweet_ids : list[int]
        Unique identifiers indicating all versions of a Tweet. For Tweets with
        no edits, there will be one ID. For Tweets with an edit history, there
        will be multiple IDs, arranged in ascending order reflecting the order
        of edits. The most recent version is the last position of the array.
        Specifies the type of attachments (if any) present in this Tweet.
    author_id : int | None
        The unique identifier of the User who posted this Tweet.
    context_annotations : list
        Contains context annotations for the Tweet.
    conversation_id : int | None
        The Tweet ID of the original Tweet of the conversation (which includes
        direct replies, replies of replies).
        Creation time of the Tweet.
    edit_controls : dict | None
        When present, this indicates how much longer the Tweet can be edited
        and the number of remaining edits. Tweets are only editable for the
        first 30 minutes after creation and can be edited up to five times.
    entities : dict | None
        Entities which have been parsed out of the text of the Tweet.
        Additionally see entities in Twitter Objects.
        Contains details about the location tagged by the user in this Tweet,
        if they specified one.
    in_reply_to_user_id : int | None
        If the represented Tweet is a reply, this field will contain the
        original Tweet’s author ID. This will not necessarily always be the
        user directly mentioned in the Tweet.
        Language of the Tweet, if detected by Twitter. Returned as a BCP47
        Non-public engagement metrics for the Tweet at the time of the request. 
    organic_metrics : dict | None
        Engagement metrics, tracked in an organic context, for the Tweet at the
        time of the request.
    possibly_sensitive : bool | None
        This field only surfaces when a Tweet contains a link. The meaning of
        the field doesn’t pertain to the Tweet content itself, but instead it
        is an indicator that the URL contained in the Tweet may contain content
        or media identified as sensitive content. 
        Engagement metrics, tracked in a promoted context, for the Tweet at the
        Public engagement metrics for the Tweet at the time of the request.
        A list of Tweets this Tweet refers to. For example, if the parent Tweet
        is a Retweet, a Retweet with comment (also known as Quoted Tweet) or a
        Reply, it will include the related Tweet referenced to by its parent.
        Shows you who can reply to a given Tweet. Fields returned are
        "everyone", "mentioned_users", and "followers".
    source : str | None
        The name of the app the user Tweeted from.
            As of December 20, 2022, this field has been removed from the Tweet
            payload. [#]_
    withheld : dict | None
        When present, contains withholding details for `withheld content`_.
    https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/tweet
    .. _twitter-text: https://github.com/twitter/twitter-text/
    .. _withheld content: https://help.twitter.com/en/rules-and-policies/tweet-withheld-by-country
        "data", "id", "text", "edit_history_tweet_ids", "attachments",
        "author_id", "context_annotations", "conversation_id", "created_at",
        "edit_controls", "entities", "geo", "in_reply_to_user_id", "lang",
        "non_public_metrics", "organic_metrics", "possibly_sensitive",
        "promoted_metrics", "public_metrics", "referenced_tweets",
        "reply_settings", "source", "withheld"
        self.text = data["text"]
            self.edit_history_tweet_ids = list(
                map(int, data["edit_history_tweet_ids"])
                "Tweet data missing default edit_history_tweet_ids field",
                RuntimeWarning,
                stacklevel=2
        self.author_id = data.get("author_id")
        if self.author_id is not None:
            self.author_id = int(self.author_id)
        self.context_annotations = data.get("context_annotations", [])
        self.conversation_id = data.get("conversation_id")
        if self.conversation_id is not None:
            self.conversation_id = int(self.conversation_id)
        self.edit_controls = data.get("edit_controls")
        if self.edit_controls is not None:
            self.edit_controls["edits_remaining"] = int(
                self.edit_controls["edits_remaining"]
            self.edit_controls["editable_until"] = parse_datetime(
                self.edit_controls["editable_until"]
        self.entities = data.get("entities")
        self.in_reply_to_user_id = data.get("in_reply_to_user_id")
        if self.in_reply_to_user_id is not None:
            self.in_reply_to_user_id = int(self.in_reply_to_user_id)
        self.possibly_sensitive = data.get("possibly_sensitive")
        self.reply_settings = data.get("reply_settings")
        self.source = data.get("source")
        self.withheld = data.get("withheld")
        return len(self.text)
        return f"<Tweet id={self.id} text={repr(self.text)}>"
        return self.text
class ReferencedTweet(HashableID, DataMapping):
    """.. versionadded:: 4.0
        Changed ``type`` to be optional
        The JSON data representing the referenced Tweet.
        The unique identifier of the referenced Tweet.
    type : str | None
    __slots__ = ("data", "id", "type")
        self.type = data.get("type")
        representation = f"<ReferencedTweet id={self.id}"
        if self.type is not None:
            representation += f" type={self.type}"
