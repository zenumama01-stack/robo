import logging
import mimetypes
from platform import python_version
from urllib.parse import urlencode
from tweepy.models import Model
from tweepy.parsers import ModelParser, Parser
from tweepy.utils import list_to_csv
log = logging.getLogger(__name__)
def pagination(mode):
    def decorator(method):
        @functools.wraps(method)
            return method(*args, **kwargs)
        wrapper.pagination_mode = mode
    return decorator
def payload(payload_type, **payload_kwargs):
    payload_list = payload_kwargs.get('list', False)
            kwargs['payload_list'] = payload_list
            kwargs['payload_type'] = payload_type
        wrapper.payload_list = payload_list
        wrapper.payload_type = payload_type
class API:
    """Twitter API v1.1 Interface
    .. versionchanged:: 4.11
        Added support for ``include_ext_edit_control`` endpoint/method
    .. versionchanged:: 4.14
        Removed ``search_30_day`` and ``search_full_archive`` methods, as
        `the Premium v1.1 API has been deprecated`_
    ----------
    auth
        The authentication handler to be used
    cache
        The cache to query if a GET method is used
    host
        The general REST API host server URL
    parser
        The Parser instance to use for parsing the response from Twitter;
        defaults to an instance of ModelParser
    proxy
        The full url to an HTTPS proxy to use for connecting to Twitter
    retry_count
        Number of retries to attempt when an error occurs
    retry_delay
        Number of seconds to wait between retries
    retry_errors
        Which HTTP status codes to retry
        The maximum amount of time to wait for a response from Twitter
    upload_host
        The URL of the upload server
    wait_on_rate_limit
        Whether or not to automatically wait for rate limits to replenish
    Raises
    ------
    TypeError
        If the given parser is not a Parser instance
    References
    https://developer.twitter.com/en/docs/api-reference-index
    .. _the Premium v1.1 API has been deprecated: https://twittercommunity.com/t/deprecating-the-premium-v1-1-api/191092
    def __init__(
        self, auth=None, *, cache=None, host='api.twitter.com', parser=None,
        proxy=None, retry_count=0, retry_delay=0, retry_errors=None,
        timeout=60, upload_host='upload.twitter.com', user_agent=None,
        wait_on_rate_limit=False
        self.auth = auth
        self.cache = cache
        self.host = host
        if parser is None:
            parser = ModelParser()
        self.parser = parser
        self.proxy = {}
        if proxy is not None:
            self.proxy['https'] = proxy
        self.retry_count = retry_count
        self.retry_delay = retry_delay
        self.retry_errors = retry_errors
        self.timeout = timeout
        self.upload_host = upload_host
        if user_agent is None:
            user_agent = (
                f"Python/{python_version()} "
                f"Requests/{requests.__version__} "
                f"Tweepy/{tweepy.__version__}"
        self.user_agent = user_agent
        self.wait_on_rate_limit = wait_on_rate_limit
        # Attempt to explain more clearly the parser argument requirements
        # https://github.com/tweepy/tweepy/issues/421
        if not isinstance(self.parser, Parser):
            raise TypeError(
                "parser should be an instance of Parser, not " +
                str(type(self.parser))
        self.session = requests.Session()
    def request(
        self, method, endpoint, *, endpoint_parameters=(), params=None,
        headers=None, json_payload=None, parser=None, payload_list=False,
        payload_type=None, post_data=None, files=None, require_auth=True,
        return_cursors=False, upload_api=False, use_cache=True, **kwargs
        # If authentication is required and no credentials
        # are provided, throw an error.
        if require_auth and not self.auth:
            raise TweepyException('Authentication required!')
        self.cached_result = False
        if headers is None:
        headers["User-Agent"] = self.user_agent
        # Build the request URL
        path = f'/1.1/{endpoint}.json'
        if upload_api:
            url = 'https://' + self.upload_host + path
            url = 'https://' + self.host + path
        if params is None:
            params = {}
        for k, arg in kwargs.items():
            if arg is None:
            if k not in endpoint_parameters + (
                "include_ext_edit_control", "tweet_mode"
                log.warning(f'Unexpected parameter: {k}')
            params[k] = str(arg)
        log.debug("PARAMS: %r", params)
        # Query the cache if one is available
        # and this request uses a GET method.
        if use_cache and self.cache and method == 'GET':
            cache_result = self.cache.get(f'{path}?{urlencode(params)}')
            # if cache result found and not expired, return it
            if cache_result:
                # must restore api reference
                if isinstance(cache_result, list):
                    for result in cache_result:
                        if isinstance(result, Model):
                            result._api = self
                    if isinstance(cache_result, Model):
                        cache_result._api = self
                self.cached_result = True
                return cache_result
        # Monitoring rate limits
        remaining_calls = None
        reset_time = None
            parser = self.parser
            # Continue attempting request until successful
            # or maximum number of retries is reached.
            retries_performed = 0
            while retries_performed <= self.retry_count:
                if (self.wait_on_rate_limit and reset_time is not None
                    and remaining_calls is not None
                    and remaining_calls < 1):
                    # Handle running out of API calls
                    sleep_time = reset_time - int(time.time())
                    if sleep_time > 0:
                        log.warning(f"Rate limit reached. Sleeping for: {sleep_time}")
                        time.sleep(sleep_time + 1)  # Sleep for extra sec
                # Apply authentication
                auth = None
                if self.auth:
                    auth = self.auth.apply_auth()
                # Execute request
                    resp = self.session.request(
                        method, url, params=params, headers=headers,
                        data=post_data, files=files, json=json_payload,
                        timeout=self.timeout, auth=auth, proxies=self.proxy
                    raise TweepyException(f'Failed to send request: {e}').with_traceback(sys.exc_info()[2])
                if 200 <= resp.status_code < 300:
                rem_calls = resp.headers.get('x-rate-limit-remaining')
                if rem_calls is not None:
                    remaining_calls = int(rem_calls)
                elif remaining_calls is not None:
                    remaining_calls -= 1
                reset_time = resp.headers.get('x-rate-limit-reset')
                if reset_time is not None:
                    reset_time = int(reset_time)
                retry_delay = self.retry_delay
                if resp.status_code in (420, 429) and self.wait_on_rate_limit:
                    if remaining_calls == 0:
                        # If ran out of calls before waiting switching retry last call
                    if 'retry-after' in resp.headers:
                        retry_delay = float(resp.headers['retry-after'])
                elif self.retry_errors and resp.status_code not in self.retry_errors:
                    # Exit request loop if non-retry error code
                # Sleep before retrying request again
                time.sleep(retry_delay)
                retries_performed += 1
            # If an error was returned, throw an exception
            self.last_response = resp
            if resp.status_code == 400:
                raise BadRequest(resp)
            if resp.status_code == 401:
                raise Unauthorized(resp)
            if resp.status_code == 403:
                raise Forbidden(resp)
            if resp.status_code == 404:
                raise NotFound(resp)
            if resp.status_code == 429:
                raise TooManyRequests(resp, reset_time=reset_time)
            if resp.status_code >= 500:
                raise TwitterServerError(resp)
            if resp.status_code and not 200 <= resp.status_code < 300:
                raise HTTPException(resp)
            # Parse the response payload
            return_cursors = return_cursors or 'cursor' in params or 'next' in params
            result = parser.parse(
                resp.text, api=self, payload_list=payload_list,
                payload_type=payload_type, return_cursors=return_cursors
            # Store result into cache if one is available.
            if use_cache and self.cache and method == 'GET' and result:
                self.cache.store(f'{path}?{urlencode(params)}', result)
            self.session.close()
    # Get Tweet timelines
    @pagination(mode='id')
    @payload('status', list=True)
    def home_timeline(self, **kwargs):
        """home_timeline(*, count, since_id, max_id, trim_user, \
                         exclude_replies, include_entities)
        Returns the 20 most recent statuses, including retweets, posted by
        the authenticating user and that user's friends. This is the equivalent
        of /timeline/home on the Web.
            |count|
        since_id
            |since_id|
        max_id
            |max_id|
        trim_user
            |trim_user|
        exclude_replies
            |exclude_replies|
        include_entities
            |include_entities|
        Returns
        -------
        :py:class:`List`[:class:`~tweepy.models.Status`]
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/timelines/api-reference/get-statuses-home_timeline
        return self.request(
            'GET', 'statuses/home_timeline', endpoint_parameters=(
                'count', 'since_id', 'max_id', 'trim_user', 'exclude_replies',
                'include_entities'
            ), **kwargs
    def mentions_timeline(self, **kwargs):
        """mentions_timeline(*, count, since_id, max_id, trim_user, \
                             include_entities)
        Returns the 20 most recent mentions, including retweets.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/timelines/api-reference/get-statuses-mentions_timeline
            'GET', 'statuses/mentions_timeline', endpoint_parameters=(
                'count', 'since_id', 'max_id', 'trim_user', 'include_entities'
    def user_timeline(self, **kwargs):
        """user_timeline(*, user_id, screen_name, since_id, count, max_id, \
                         trim_user, exclude_replies, include_rts)
        Returns the 20 most recent statuses posted from the authenticating user
        or the user specified. It's also possible to request another user's
        timeline via the id parameter.
        user_id
            |user_id|
        screen_name
            |screen_name|
        include_rts
            When set to ``false``, the timeline will strip any native retweets
            (though they will still count toward both the maximal length of the
            timeline and the slice selected by the count parameter). Note: If
            you're using the trim_user parameter in conjunction with
            include_rts, the retweets will still contain a full user object.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/timelines/api-reference/get-statuses-user_timeline
            'GET', 'statuses/user_timeline', endpoint_parameters=(
                'user_id', 'screen_name', 'since_id', 'count', 'max_id',
                'trim_user', 'exclude_replies', 'include_rts'
    # Post, retrieve, and engage with Tweets
    def get_favorites(self, **kwargs):
        """get_favorites(*, user_id, screen_name, count, since_id, max_id, \
        Returns the favorite statuses for the authenticating user or user
        specified by the ID parameter.
        .. versionchanged:: 4.0
            Renamed from ``API.favorites``
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-favorites-list
            'GET', 'favorites/list', endpoint_parameters=(
                'user_id', 'screen_name', 'count', 'since_id', 'max_id',
    def lookup_statuses(self, id, **kwargs):
        """lookup_statuses(id, *, include_entities, trim_user, map, \
                           include_ext_alt_text, include_card_uri)
        Returns full Tweet objects for up to 100 Tweets per request, specified
        by the ``id`` parameter.
        .. deprecated:: 4.15.0
            `The Twitter API v1.1 statuses/lookup endpoint that this method
            uses has been deprecated and has a retirement date of November 20,
            2023.`_ The Twitter API v2 replacement is
            :meth:`Client.get_tweets`.
            Renamed from ``API.statuses_lookup``
            A list of Tweet IDs to lookup, up to 100
        map
            A boolean indicating whether or not to include Tweets that cannot
            be shown. Defaults to False.
        include_ext_alt_text
            |include_ext_alt_text|
        include_card_uri
            |include_card_uri|
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-lookup
        .. _The Twitter API v1.1 statuses/lookup endpoint that this method uses
            has been deprecated and has a retirement date of November 20,
            2023.: https://twittercommunity.com/t/x-api-v2-migration/203391
            'GET', 'statuses/lookup', endpoint_parameters=(
                'id', 'include_entities', 'trim_user', 'map',
                'include_ext_alt_text', 'include_card_uri'
            ), id=list_to_csv(id), **kwargs
    @payload('json')
    def get_oembed(self, url, **kwargs):
        """get_oembed( \
            url, *, maxwidth, hide_media, hide_thread, omit_script, align, \
            related, lang, theme, link_color, widget_type, dnt \
        Returns a single Tweet, specified by either a Tweet web URL or the
        Tweet ID, in an oEmbed-compatible format. The returned HTML snippet will
        be automatically recognized as an Embedded Tweet when Twitter's widget
        JavaScript is included on the page.
        The oEmbed endpoint allows customization of the final appearance of an
        Embedded Tweet by setting the corresponding properties in HTML markup
        to be interpreted by Twitter's JavaScript bundled with the HTML
        response by default. The format of the returned markup may change over
        time as Twitter adds new features or adjusts its Tweet representation.
        The Tweet fallback markup is meant to be cached on your servers for up
        to the suggested cache lifetime specified in the ``cache_age``.
            The URL of the Tweet to be embedded
        maxwidth
            The maximum width of a rendered Tweet in whole pixels. A supplied
            value under or over the allowed range will be returned as the
            minimum or maximum supported width respectively; the reset width
            value will be reflected in the returned ``width`` property. Note
            that Twitter does not support the oEmbed ``maxheight`` parameter.
            Tweets are fundamentally text, and are therefore of unpredictable
            height that cannot be scaled like an image or video. Relatedly, the
            oEmbed response will not provide a value for ``height``.
            Implementations that need consistent heights for Tweets should
            refer to the ``hide_thread`` and ``hide_media`` parameters below.
        hide_media
            When set to ``true``, ``"t"``, or ``1``, links in a Tweet are not
            expanded to photo, video, or link previews.
        hide_thread
            When set to ``true``, ``"t"``, or ``1``, a collapsed version of the
            previous Tweet in a conversation thread will not be displayed when
            the requested Tweet is in reply to another Tweet.
        omit_script
            When set to ``true``, ``"t"``, or ``1``, the ``<script>``
            responsible for loading ``widgets.js`` will not be returned. Your
            webpages should include their own reference to ``widgets.js`` for
            use across all Twitter widgets including Embedded Tweets.
        align
            Specifies whether the embedded Tweet should be floated left, right,
            or center in the page relative to the parent element.
        related
            A comma-separated list of Twitter usernames related to your
            content. This value will be forwarded to Tweet action intents if a
            viewer chooses to reply, like, or retweet the embedded Tweet.
        lang
            Request returned HTML and a rendered Tweet in the specified Twitter
            language supported by embedded Tweets.
        theme
            When set to ``dark``, the Tweet is displayed with light text over a
            dark background.
        link_color
            Adjust the color of Tweet text links with a hexadecimal color
            value.
        widget_type
            Set to ``video`` to return a Twitter Video embed for the given
            Tweet.
        dnt
            When set to ``true``, the Tweet and its embedded page on your site
            are not used for purposes that include personalized suggestions and
            personalized ads.
        :class:`dict`
            JSON
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-oembed
            'GET', 'statuses/oembed', endpoint_parameters=(
                'url', 'maxwidth', 'hide_media', 'hide_thread', 'omit_script',
                'align', 'related', 'lang', 'theme', 'link_color',
                'widget_type', 'dnt'
            ), url=url, require_auth=False, **kwargs
    @pagination(mode='cursor')
    @payload('ids')
    def get_retweeter_ids(self, id, **kwargs):
        """get_retweeter_ids(id, *, count, cursor, stringify_ids)
        Returns up to 100 user IDs belonging to users who have retweeted the
        Tweet specified by the ``id`` parameter.
            Renamed from ``API.retweeters``
            |sid|
        cursor
            |cursor|
        stringify_ids
            |stringify_ids|
        :py:class:`List`[:class:`int`]
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-retweeters-ids
            'GET', 'statuses/retweeters/ids', endpoint_parameters=(
                'id', 'count', 'cursor', 'stringify_ids'
            ), id=id, **kwargs
    def get_retweets(self, id, **kwargs):
        """get_retweets(id, *, count, trim_user)
        Returns up to 100 of the first Retweets of the given Tweet.
            Renamed from ``API.retweets``
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-retweets-id
            'GET', f'statuses/retweets/{id}', endpoint_parameters=(
                'count', 'trim_user'
    def get_retweets_of_me(self, **kwargs):
        """get_retweets_of_me(*, count, since_id, max_id, trim_user, \
                              include_entities, include_user_entities)
        Returns the 20 most recent Tweets of the authenticated user that have
        been retweeted by others.
            Renamed from ``API.retweets_of_me``
        include_user_entities
            |include_user_entities|
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-retweets_of_me
            'GET', 'statuses/retweets_of_me', endpoint_parameters=(
                'count', 'since_id', 'max_id', 'trim_user', 'include_entities',
                'include_user_entities'
    @payload('status')
    def get_status(self, id, **kwargs):
        """get_status(id, *, trim_user, include_my_retweet, include_entities, \
        Returns a single status specified by the ID parameter.
            `The Twitter API v1.1 statuses/show endpoint that this method uses
            2023.`_ The Twitter API v2 replacement is :meth:`Client.get_tweet`.
        id:
        include_my_retweet:
            A boolean indicating if any Tweets returned that have been
            retweeted by the authenticating user should include an additional
            current_user_retweet node, containing the ID of the source status
            for the retweet.
        :class:`~tweepy.models.Status`
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/get-statuses-show-id
        .. _The Twitter API v1.1 statuses/show endpoint that this method uses
            'GET', 'statuses/show', endpoint_parameters=(
                'id', 'trim_user', 'include_my_retweet', 'include_entities',
    def create_favorite(self, id, **kwargs):
        """create_favorite(id, *, include_entities)
        Favorites the status specified in the ``id`` parameter as the
        authenticating user.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-favorites-create
            'POST', 'favorites/create', endpoint_parameters=(
                'id', 'include_entities'
    def destroy_favorite(self, id, **kwargs):
        """destroy_favorite(id, *, include_entities)
        Un-favorites the status specified in the ``id`` parameter as the
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-favorites-destroy
            'POST', 'favorites/destroy', endpoint_parameters=(
    def destroy_status(self, id, **kwargs):
        """destroy_status(id, *, trim_user)
        Destroy the status specified by the ``id`` parameter. The authenticated
        user must be the author of the status to destroy.
            `The Twitter API v1.1 statuses/destroy endpoint that this method
            :meth:`Client.delete_tweet`.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-statuses-destroy-id
        .. _The Twitter API v1.1 statuses/destroy endpoint that this method
            'POST', f'statuses/destroy/{id}', endpoint_parameters=(
                'trim_user',
    def retweet(self, id, **kwargs):
        """retweet(id, *, trim_user)
        Retweets a Tweet. Requires the ID of the Tweet you are retweeting.
            `The Twitter API v1.1 statuses/retweet endpoint that this method
            2023.`_ The Twitter API v2 replacement is :meth:`Client.retweet`.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-statuses-retweet-id
        .. _The Twitter API v1.1 statuses/retweet endpoint that this method
            'POST', f'statuses/retweet/{id}', endpoint_parameters=(
    def unretweet(self, id, **kwargs):
        """unretweet(id, *, trim_user)
        Untweets a retweeted status. Requires the ID of the retweet to
        unretweet.
            `The Twitter API v1.1 statuses/unretweet endpoint that this method
            :meth:`Client.unretweet`.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-statuses-unretweet-id
        .. _The Twitter API v1.1 statuses/unretweet endpoint that this method
            'POST', f'statuses/unretweet/{id}', endpoint_parameters=(
    def update_status(self, status, **kwargs):
        """update_status( \
            status, *, in_reply_to_status_id, auto_populate_reply_metadata, \
            exclude_reply_user_ids, attachment_url, media_ids, \
            possibly_sensitive, lat, long, place_id, display_coordinates, \
            trim_user, card_uri \
        Updates the authenticating user's current status, also known as
        Tweeting.
        For each update attempt, the update text is compared with the
        authenticating user's recent Tweets. Any attempt that would result in
        duplication will be blocked, resulting in a 403 error. A user cannot
        submit the same status twice in a row.
        While not rate limited by the API, a user is limited in the number of
        Tweets they can create at a time. If the number of updates posted by
        the user reaches the current allowed limit this method will return an
        HTTP 403 error.
            `The Twitter API v1.1 statuses/update endpoint that this method
            :meth:`Client.create_tweet`.
            The text of your status update.
        in_reply_to_status_id
            The ID of an existing status that the update is in reply to. Note:
            This parameter will be ignored unless the author of the Tweet this
            parameter references is mentioned within the status text.
            Therefore, you must include @username, where username is the author
            of the referenced Tweet, within the update.
        auto_populate_reply_metadata
            If set to true and used with in_reply_to_status_id, leading
            @mentions will be looked up from the original Tweet, and added to
            the new Tweet from there. This will append @mentions into the
            metadata of an extended Tweet as a reply chain grows, until the
            limit on @mentions is reached. In cases where the original Tweet
            has been deleted, the reply will fail.
        exclude_reply_user_ids
            When used with auto_populate_reply_metadata, a comma-separated list
            of user ids which will be removed from the server-generated
            @mentions prefix on an extended Tweet. Note that the leading
            @mention cannot be removed as it would break the
            in-reply-to-status-id semantics. Attempting to remove it will be
            silently ignored.
        attachment_url
            In order for a URL to not be counted in the status body of an
            extended Tweet, provide a URL as a Tweet attachment. This URL must
            be a Tweet permalink, or Direct Message deep link. Arbitrary,
            non-Twitter URLs must remain in the status text. URLs passed to the
            attachment_url parameter not matching either a Tweet permalink or
            Direct Message deep link will fail at Tweet creation and cause an
            exception.
        media_ids
            A list of media_ids to associate with the Tweet. You may include up
            to 4 photos or 1 animated GIF or 1 video in a Tweet.
        possibly_sensitive
            If you upload Tweet media that might be considered sensitive
            content such as nudity, or medical procedures, you must set this
            value to true.
        lat
            The latitude of the location this Tweet refers to. This parameter
            will be ignored unless it is inside the range -90.0 to +90.0 (North
            is positive) inclusive. It will also be ignored if there is no
            corresponding long parameter.
            The longitude of the location this Tweet refers to. The valid
            ranges for longitude are -180.0 to +180.0 (East is positive)
            inclusive. This parameter will be ignored if outside that range, if
            it is not a number, if geo_enabled is disabled, or if there no
            corresponding lat parameter.
        place_id
            A place in the world.
        display_coordinates
            Whether or not to put a pin on the exact coordinates a Tweet has
            been sent from.
        card_uri
            Associate an ads card with the Tweet using the card_uri value from
            any ads card response.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-statuses-update
        .. _The Twitter API v1.1 statuses/update endpoint that this method
        if 'media_ids' in kwargs:
            kwargs['media_ids'] = list_to_csv(kwargs['media_ids'])
            'POST', 'statuses/update', endpoint_parameters=(
                'status', 'in_reply_to_status_id',
                'auto_populate_reply_metadata', 'exclude_reply_user_ids',
                'attachment_url', 'media_ids', 'possibly_sensitive', 'lat',
                'long', 'place_id', 'display_coordinates', 'trim_user',
                'card_uri'
            ), status=status, **kwargs
    def update_status_with_media(self, status, filename, *, file=None,
                                 **kwargs):
        """update_status_with_media( \
            status, filename, *, file, possibly_sensitive, \
            in_reply_to_status_id, lat, long, place_id, display_coordinates \
        Update the authenticated user's status. Statuses that are duplicates or
        too long will be silently ignored.
        .. deprecated:: 3.7.0
            `The Twitter API v1.1 statuses/update_with_media endpoint that this
            method uses has been deprecated and has a retirement date of
            November 20, 2023.`_ :meth:`API.media_upload` and
            :meth:`Client.create_tweet` can be used instead.
            Renamed from ``API.update_with_media``
        filename
            |filename|
        file
            |file|
            Set to true for content which may not be suitable for every
            audience.
            The ID of an existing status that the update is in reply to.
            The location's latitude that this tweet refers to.
            The location's longitude that this tweet refers to.
            Twitter ID of location which is listed in the Tweet if geolocation
            is enabled for the user.
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/post-and-engage/api-reference/post-statuses-update_with_media
        .. _The Twitter API v1.1 statuses/update_with_media endpoint that this
            November 20, 2023.:
            https://twittercommunity.com/t/x-api-v2-migration/203391
        with contextlib.ExitStack() as stack:
            if file is not None:
                files = {'media[]': (filename, file)}
                files = {'media[]': stack.enter_context(open(filename, 'rb'))}
                'POST', 'statuses/update_with_media', endpoint_parameters=(
                    'status', 'possibly_sensitive', 'in_reply_to_status_id',
                    'lat', 'long', 'place_id', 'display_coordinates'
                ), status=status, files=files, **kwargs
    # Search Tweets
    @payload('search_results')
    def search_tweets(self, q, **kwargs):
        """search_tweets(q, *, geocode, lang, locale, result_type, count, \
                         until, since_id, max_id, include_entities)
        Returns a collection of relevant Tweets matching a specified query.
        Please note that Twitter's search service and, by extension, the Search
        API is not meant to be an exhaustive source of Tweets. Not all Tweets
        will be indexed or made available via the search interface.
        .. note::
            Twitter's standard search API only "searches against a sampling of
            recent Tweets published in the past 7 days."
            If you're specifying an ID range beyond the past 7 days or there
            are no results from the past 7 days, then no results will be
            returned.
            See `Twitter's documentation on the standard search API`_ for more
            information.
            In API v1.1, the response format of the Search API has been
            improved to return Tweet objects more similar to the objects you’ll
            find across the REST API and platform. However, perspectival
            attributes (fields that pertain to the perspective of the
            authenticating user) are not currently supported on this endpoint.
            [#]_ [#]_
            `The Twitter API v1.1 search/tweets endpoint that this method uses
            has been deprecated and has a retirement date of September 20,
            :meth:`Client.search_recent_tweets`.
            Renamed from ``API.search``
        q
            The search query string of 500 characters maximum, including
            operators. Queries may additionally be limited by complexity.
        geocode
            Returns tweets by users located within a given radius of the given
            latitude/longitude.  The location is preferentially taking from the
            Geotagging API, but will fall back to their Twitter profile. The
            parameter value is specified by "latitude,longitude,radius", where
            radius units must be specified as either "mi" (miles) or "km"
            (kilometers). Note that you cannot use the near operator via the
            API to geocode arbitrary locations; however you can use this
            geocode parameter to search near geocodes directly. A maximum of
            1,000 distinct "sub-regions" will be considered when using the
            radius modifier.
            Restricts tweets to the given language, given by an ISO 639-1 code.
            Language detection is best-effort.
        locale
            Specify the language of the query you are sending (only ja is
            currently effective). This is intended for language-specific
            consumers and the default should work in the majority of cases.
        result_type
            Specifies what type of search results you would prefer to receive.
            The current default is "mixed." Valid values include:
            * mixed : include both popular and real time results in the \
            * recent : return only the most recent results in the response
            * popular : return only the most popular results in the response
        until
            Returns tweets created before the given date. Date should be
            formatted as YYYY-MM-DD. Keep in mind that the search index has a
            7-day limit. In other words, no tweets will be found for a date
            older than one week.
            |since_id| There are limits to the number of Tweets which can be
            accessed through the API. If the limit of Tweets has occurred since
            the since_id, the since_id will be forced to the oldest ID
            available.
        :class:`SearchResults`
        https://developer.twitter.com/en/docs/twitter-api/v1/tweets/search/api-reference/get-search-tweets
        .. _Twitter's documentation on the standard search API:
            https://developer.twitter.com/en/docs/twitter-api/v1/tweets/search/overview
        .. _The Twitter API v1.1 search/tweets endpoint that this method uses
            'GET', 'search/tweets', endpoint_parameters=(
                'q', 'geocode', 'lang', 'locale', 'result_type', 'count',
                'until', 'since_id', 'max_id', 'include_entities'
            ), q=q, **kwargs
    # Create and manage lists
    @payload('list', list=True)
    def get_lists(self, **kwargs):
        """get_lists(*, user_id, screen_name, reverse)
        Returns all lists the authenticating or specified user subscribes to,
        including their own. The user is specified using the ``user_id`` or
        ``screen_name`` parameters. If no user is given, the authenticating
        user is used.
        A maximum of 100 results will be returned by this call. Subscribed
        lists are returned first, followed by owned lists. This means that if a
        user subscribes to 90 lists and owns 20 lists, this method returns 90
        subscriptions and 10 owned lists. The ``reverse`` method returns owned
        lists first, so with ``reverse=true``, 20 owned lists and 80
        subscriptions would be returned.
            Renamed from ``API.lists_all``
        reverse
            A boolean indicating if you would like owned lists to be returned
            first. See description above for information on how this parameter
            works.
        :py:class:`List`[:class:`~tweepy.models.List`]
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-list
            'GET', 'lists/list', endpoint_parameters=(
                'user_id', 'screen_name', 'reverse'
    @payload('user', list=True)
    def get_list_members(self, **kwargs):
        """get_list_members(*, list_id, slug, owner_screen_name, owner_id, \
                            count, cursor, include_entities, skip_status)
        Returns the members of the specified list.
            Renamed from ``API.list_members``
        list_id
            |list_id|
        slug
            |slug|
        owner_screen_name
            |owner_screen_name|
        owner_id
            |owner_id|
        skip_status
            |skip_status|
        :py:class:`List`[:class:`~tweepy.models.User`]
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-members
            'GET', 'lists/members', endpoint_parameters=(
                'list_id', 'slug', 'owner_screen_name', 'owner_id', 'count',
                'cursor', 'include_entities', 'skip_status'
    @payload('user')
    def get_list_member(self, **kwargs):
        """get_list_member( \
            *, list_id, slug, user_id, screen_name, owner_screen_name, \
            owner_id, include_entities, skip_status \
        Check if the specified user is a member of the specified list.
            Renamed from ``API.show_list_member``
        :class:`~tweepy.errors.NotFound`
            The user is not a member of the list.
        :class:`~tweepy.models.User`
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-members-show
            'GET', 'lists/members/show', endpoint_parameters=(
                'list_id', 'slug', 'user_id', 'screen_name',
                'owner_screen_name', 'owner_id', 'include_entities',
                'skip_status'
    def get_list_memberships(self, **kwargs):
        """get_list_memberships(*, user_id, screen_name, count, cursor, \
                                filter_to_owned_lists)
        Returns the lists the specified user has been added to. If ``user_id``
        or ``screen_name`` are not provided, the memberships for the
        authenticating user are returned.
            Renamed from ``API.lists_memberships``
        filter_to_owned_lists
            A boolean indicating whether to return just lists the
            authenticating user owns, and the user represented by ``user_id``
            or ``screen_name`` is a member of.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-memberships
            'GET', 'lists/memberships', endpoint_parameters=(
                'user_id', 'screen_name', 'count', 'cursor',
                'filter_to_owned_lists'
    def get_list_ownerships(self, **kwargs):
        """get_list_ownerships(*, user_id, screen_name, count, cursor)
        Returns the lists owned by the specified user. Private lists will only
        be shown if the authenticated user is also the owner of the lists. If
        ``user_id`` and ``screen_name`` are not provided, the ownerships for
        the authenticating user are returned.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-ownerships
            'GET', 'lists/ownerships', endpoint_parameters=(
                'user_id', 'screen_name', 'count', 'cursor'
    @payload('list')
    def get_list(self, **kwargs):
        """get_list(*, list_id, slug, owner_screen_name, owner_id)
        Returns the specified list. Private lists will only be shown if the
        authenticated user owns the specified list.
        :class:`~tweepy.models.List`
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-show
            'GET', 'lists/show', endpoint_parameters=(
                'list_id', 'slug', 'owner_screen_name', 'owner_id'
    def list_timeline(self, **kwargs):
        """list_timeline( \
            *, list_id, slug, owner_screen_name, owner_id, since_id, max_id, \
            count, include_entities, include_rts \
        Returns a timeline of Tweets authored by members of the specified list.
        Retweets are included by default. Use the ``include_rts=false``
        parameter to omit retweets.
            A boolean indicating whether the list timeline will contain native
            retweets (if they exist) in addition to the standard stream of
            Tweets. The output format of retweeted Tweets is identical to the
            representation you see in home_timeline.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-statuses
            'GET', 'lists/statuses', endpoint_parameters=(
                'list_id', 'slug', 'owner_screen_name', 'owner_id', 'since_id',
                'max_id', 'count', 'include_entities', 'include_rts'
    def get_list_subscribers(self, **kwargs):
        """get_list_subscribers( \
            *, list_id, slug, owner_screen_name, owner_id, count, cursor, \
            include_entities, skip_status \
        Returns the subscribers of the specified list. Private list subscribers
        will only be shown if the authenticated user owns the specified list.
            Renamed from ``API.list_subscribers``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-subscribers
            'GET', 'lists/subscribers', endpoint_parameters=(
    def get_list_subscriber(self, **kwargs):
        """get_list_subscriber( \
            *, owner_screen_name, owner_id, list_id, slug, user_id, \
            screen_name, include_entities, skip_status \
        Check if the specified user is a subscriber of the specified list.
            Renamed from ``API.show_list_subscriber``
            The user is not a subscriber of the list.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-subscribers-show
            'GET', 'lists/subscribers/show', endpoint_parameters=(
                'owner_screen_name', 'owner_id', 'list_id', 'slug', 'user_id',
                'screen_name', 'include_entities', 'skip_status'
    def get_list_subscriptions(self, **kwargs):
        """get_list_subscriptions(*, user_id, screen_name, count, cursor)
        Obtain a collection of the lists the specified user is subscribed to,
        20 lists per page by default. Does not include the user's own lists.
            Renamed from ``API.lists_subscriptions``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/get-lists-subscriptions
            'GET', 'lists/subscriptions', endpoint_parameters=(
    def create_list(self, name, **kwargs):
        """create_list(name, *, mode, description)
        Creates a new list for the authenticated user.
        Note that you can create up to 1000 lists per account.
            The name of the new list.
        mode
            |list_mode|
            The description of the list you are creating.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-create
            'POST', 'lists/create', endpoint_parameters=(
                'name', 'mode', 'description'
            ), name=name, **kwargs
    def destroy_list(self, **kwargs):
        """destroy_list(*, owner_screen_name, owner_id, list_id, slug)
        Deletes the specified list.
        The authenticated user must own the list to be able to destroy it.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-destroy
            'POST', 'lists/destroy', endpoint_parameters=(
                'owner_screen_name', 'owner_id', 'list_id', 'slug'
    def add_list_member(self, **kwargs):
        """add_list_member(*, list_id, slug, user_id, screen_name, \
                           owner_screen_name, owner_id)
        Add a member to a list. The authenticated user must own the list to be
        able to add members to it. Lists are limited to 5,000 members.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-members-create
            'POST', 'lists/members/create', endpoint_parameters=(
                'owner_screen_name', 'owner_id'
    def add_list_members(self, **kwargs):
        """add_list_members(*, list_id, slug, user_id, screen_name, \
        Add up to 100 members to a list. The authenticated user must own the
        list to be able to add members to it. Lists are limited to 5,000
        members.
            A comma separated list of user IDs, up to 100 are allowed in a
            single request
            A comma separated list of screen names, up to 100 are allowed in a
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-members-create_all
        if 'user_id' in kwargs:
            kwargs['user_id'] = list_to_csv(kwargs['user_id'])
        if 'screen_name' in kwargs:
            kwargs['screen_name'] = list_to_csv(kwargs['screen_name'])
            'POST', 'lists/members/create_all', endpoint_parameters=(
    def remove_list_member(self, **kwargs):
        """remove_list_member(*, list_id, slug, user_id, screen_name, \
        Removes the specified member from the list. The authenticated user must
        be the list's owner to remove members from the list.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-members-destroy
            'POST', 'lists/members/destroy', endpoint_parameters=(
    def remove_list_members(self, **kwargs):
        """remove_list_members(*, list_id, slug, user_id, screen_name, \
        Remove up to 100 members from a list. The authenticated user must own
        the list to be able to remove members from it. Lists are limited to
        5,000 members.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-members-destroy_all
            'POST', 'lists/members/destroy_all', endpoint_parameters=(
    def subscribe_list(self, **kwargs):
        """subscribe_list(*, owner_screen_name, owner_id, list_id, slug)
        Subscribes the authenticated user to the specified list.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-subscribers-create
            'POST', 'lists/subscribers/create', endpoint_parameters=(
    def unsubscribe_list(self, **kwargs):
        """unsubscribe_list(*, list_id, slug, owner_screen_name, owner_id)
        Unsubscribes the authenticated user from the specified list.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-subscribers-destroy
            'POST', 'lists/subscribers/destroy', endpoint_parameters=(
    def update_list(self, **kwargs):
        """update_list(*, list_id, slug, name, mode, description, \
        Updates the specified list.
        The authenticated user must own the list to be able to update it.
            The name for the list.
            The description to give the list.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/create-manage-lists/api-reference/post-lists-update
            'POST', 'lists/update', endpoint_parameters=(
                'list_id', 'slug', 'name', 'mode', 'description',
    # Follow, search, and get users
    def get_follower_ids(self, **kwargs):
        """get_follower_ids(*, user_id, screen_name, cursor, stringify_ids, \
                            count)
        Returns an array containing the IDs of users following the specified
        user.
            Renamed from ``API.followers_ids``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-followers-ids
            'GET', 'followers/ids', endpoint_parameters=(
                'user_id', 'screen_name', 'cursor', 'stringify_ids', 'count'
    def get_followers(self, **kwargs):
        """get_followers(*, user_id, screen_name, cursor, count, skip_status, \
                         include_user_entities)
        Returns a user's followers ordered in which they were added. If no user
        is specified by id/screen name, it defaults to the authenticated user.
            Renamed from ``API.followers``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-followers-list
            'GET', 'followers/list', endpoint_parameters=(
                'user_id', 'screen_name', 'cursor', 'count', 'skip_status',
    def get_friend_ids(self, **kwargs):
        """get_friend_ids(*, user_id, screen_name, cursor, stringify_ids, \
        Returns an array containing the IDs of users being followed by the
        specified user.
            Renamed from ``API.friends_ids``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friends-ids
            'GET', 'friends/ids', endpoint_parameters=(
    def get_friends(self, **kwargs):
        """get_friends(*, user_id, screen_name, cursor, count, skip_status, \
        Returns a user's friends ordered in which they were added 100 at a
        time. If no user is specified it defaults to the authenticated user.
            Renamed from ``API.friends``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friends-list
            'GET', 'friends/list', endpoint_parameters=(
    def incoming_friendships(self, **kwargs):
        """incoming_friendships(*, cursor, stringify_ids)
        Returns a collection of numeric IDs for every user who has a pending
        request to follow the authenticating user.
            Renamed from ``API.friendships_incoming``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friendships-incoming
            'GET', 'friendships/incoming', endpoint_parameters=(
                'cursor', 'stringify_ids'
    @payload('relationship', list=True)
    def lookup_friendships(self, *, screen_name=None, user_id=None, **kwargs):
        """lookup_friendships(*, screen_name, user_id)
        Returns the relationships of the authenticated user to the list of up
        to 100 screen_name or user_id provided.
            A list of screen names, up to 100 are allowed in a single request.
            A list of user IDs, up to 100 are allowed in a single request.
        :py:class:`List`[:class:`~tweepy.models.Relationship`]
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friendships-lookup
            'GET', 'friendships/lookup', endpoint_parameters=(
                'screen_name', 'user_id'
            ), screen_name=list_to_csv(screen_name),
            user_id=list_to_csv(user_id), **kwargs
    def no_retweets_friendships(self, **kwargs):
        """no_retweets_friendships(*, stringify_ids)
        Returns a collection of user_ids that the currently authenticated user
        does not want to receive retweets from.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friendships-no_retweets-ids
            'GET', 'friendships/no_retweets/ids', endpoint_parameters=(
                'stringify_ids',
    def outgoing_friendships(self, **kwargs):
        """outgoing_friendships(*, cursor, stringify_ids)
        Returns a collection of numeric IDs for every protected user for whom
        the authenticating user has a pending follow request.
            Renamed from ``API.friendships_outgoing``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friendships-outgoing
            'GET', 'friendships/outgoing', endpoint_parameters=(
    @payload('friendship')
    def get_friendship(self, **kwargs):
        """get_friendship(*, source_id, source_screen_name, target_id, \
                          target_screen_name)
        Returns detailed information about the relationship between two users.
            Renamed from ``API.show_friendship``
        source_id
            The user_id of the subject user.
        source_screen_name
            The screen_name of the subject user.
        target_id
            The user_id of the target user.
        target_screen_name
            The screen_name of the target user.
        :class:`~tweepy.models.Friendship`
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-friendships-show
            'GET', 'friendships/show', endpoint_parameters=(
                'source_id', 'source_screen_name', 'target_id',
                'target_screen_name'
    def lookup_users(self, *, screen_name=None, user_id=None, **kwargs):
        """lookup_users(*, screen_name, user_id, include_entities, tweet_mode)
        Returns fully-hydrated user objects for up to 100 users per request.
        There are a few things to note when using this method.
        * You must be following a protected user to be able to see their most \
            recent status update. If you don't follow a protected user their \
            status will be removed.
        * The order of user IDs or screen names may not match the order of \
            users in the returned array.
        * If a requested user is unknown, suspended, or deleted, then that \
            user will not be returned in the results list.
        * If none of your lookup criteria can be satisfied by returning a \
            user object, a HTTP 404 will be thrown.
        tweet_mode
            Valid request values are compat and extended, which give
            compatibility mode and extended mode, respectively for Tweets that
            contain over 140 characters.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-users-lookup
            'POST', 'users/lookup', endpoint_parameters=(
                'screen_name', 'user_id', 'include_entities', 'tweet_mode'
    @pagination(mode='page')
    def search_users(self, q, **kwargs):
        """search_users(q, *, page, count, include_entities)
        Run a search for users similar to Find People button on Twitter.com;
        the same results returned by people search on Twitter.com will be
        returned by using this API (about being listed in the People Search).
        It is only possible to retrieve the first 1000 matches from this API.
            `The Twitter API v1.1 users/search endpoint that this method uses
            2023.`_
            The query to run against people search.
        page
            |page|
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-users-search
        .. _The Twitter API v1.1 users/search endpoint that this method uses
            'GET', 'users/search', endpoint_parameters=(
                'q', 'page', 'count', 'include_entities'
    def get_user(self, **kwargs):
        """get_user(*, user_id, screen_name, include_entities)
        Returns information about the specified user.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-users-show
            'GET', 'users/show', endpoint_parameters=(
                'user_id', 'screen_name', 'include_entities'
    def create_friendship(self, **kwargs):
        """create_friendship(*, screen_name, user_id, follow)
        Create a new friendship with the specified user (aka follow).
        follow
            Enable notifications for the target user in addition to becoming
            friends.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/post-friendships-create
            'POST', 'friendships/create', endpoint_parameters=(
                'screen_name', 'user_id', 'follow'
    def destroy_friendship(self, **kwargs):
        """destroy_friendship(*, screen_name, user_id)
        Destroy a friendship with the specified user (aka unfollow).
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/post-friendships-destroy
            'POST', 'friendships/destroy', endpoint_parameters=(
    def update_friendship(self, **kwargs):
        """update_friendship(*, screen_name, user_id, device, retweets)
        Turn on/off Retweets and device notifications from the specified user.
        device
            Turn on/off device notifications from the target user.
        retweets
            Turn on/off Retweets from the target user.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/post-friendships-update
            'POST', 'friendships/update', endpoint_parameters=(
                'screen_name', 'user_id', 'device', 'retweets'
    # Manage account settings and profile
    def get_settings(self, **kwargs):
        """get_settings()
        Returns settings (including current trend, geo and sleep time
        information) for the authenticating user.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-account-settings
            'GET', 'account/settings', use_cache=False, **kwargs
    def verify_credentials(self, **kwargs):
        """verify_credentials(*, include_entities, skip_status, include_email)
        Verify the supplied user credentials are valid.
        include_email
            When set to true email will be returned in the user objects as a
            string.
        :class:`~tweepy.errors.Unauthorized`
            Authentication unsuccessful
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-account-verify_credentials
        if 'include_email' in kwargs:
            kwargs['include_email'] = str(kwargs['include_email']).lower()
            'GET', 'account/verify_credentials', endpoint_parameters=(
                'include_entities', 'skip_status', 'include_email'
    @payload('saved_search', list=True)
    def get_saved_searches(self, **kwargs):
        """get_saved_searches()
        Returns the authenticated user's saved search queries.
            Renamed from ``API.saved_searches``
        :py:class:`List`[:class:`~tweepy.models.SavedSearch`]
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-saved_searches-list
        return self.request('GET', 'saved_searches/list', **kwargs)
    @payload('saved_search')
    def get_saved_search(self, id, **kwargs):
        """get_saved_search(id)
        Retrieve the data for a saved search owned by the authenticating user
        specified by the given ID.
            The ID of the saved search to be retrieved.
        :class:`~tweepy.models.SavedSearch`
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-saved_searches-show-id
        return self.request('GET', f'saved_searches/show/{id}', **kwargs)
    def get_profile_banner(self, **kwargs):
        """get_profile_banner(*, user_id, screen_name)
        Returns a map of the available size variations of the specified user's
        profile banner. If the user has not uploaded a profile banner, a HTTP
        404 will be served instead.
        The profile banner data available at each size variant's URL is in PNG
        format.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/get-users-profile_banner
            'GET', 'users/profile_banner', endpoint_parameters=(
                'user_id', 'screen_name'
    def remove_profile_banner(self, **kwargs):
        """remove_profile_banner()
        Removes the uploaded profile banner for the authenticating user.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-account-remove_profile_banner
        return self.request('POST', 'account/remove_profile_banner', **kwargs)
    def set_settings(self, **kwargs):
        """set_settings(*, sleep_time_enabled, start_sleep_time, \
                        end_sleep_time, time_zone, trend_location_woeid, lang)
        Updates the authenticating user's settings.
        sleep_time_enabled
            When set to ``true``, ``t`` or ``1`` , will enable sleep time for
            the user. Sleep time is the time when push or SMS notifications
            should not be sent to the user.
        start_sleep_time
            The hour that sleep time should begin if it is enabled. The value
            for this parameter should be provided in `ISO 8601`_ format (i.e.
            00-23). The time is considered to be in the same timezone as the
            user's ``time_zone`` setting.
        end_sleep_time
            The hour that sleep time should end if it is enabled. The value for
            this parameter should be provided in `ISO 8601`_ format (i.e.
        time_zone
            The timezone dates and times should be displayed in for the user.
            The timezone must be one of the `Rails TimeZone`_ names.
        trend_location_woeid
            The Yahoo! Where On Earth ID to use as the user's default trend
            location. Global information is available by using 1 as the WOEID.
            The language which Twitter should render in for this user. The
            language must be specified by the appropriate two letter ISO 639-1
            representation.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-account-settings
        .. _ISO 8601: https://en.wikipedia.org/wiki/ISO_8601
        .. _Rails TimeZone:
            https://api.rubyonrails.org/classes/ActiveSupport/TimeZone.html
            'POST', 'account/settings', endpoint_parameters=(
                'sleep_time_enabled', 'start_sleep_time', 'end_sleep_time',
                'time_zone', 'trend_location_woeid', 'lang'
            ), use_cache=False, **kwargs
    def update_profile(self, **kwargs):
        """update_profile(*, name, url, location, description, \
                          profile_link_color, include_entities, skip_status)
        Sets values that users are able to set under the "Account" tab of their
        settings page.
            Full name associated with the profile.
            URL associated with the profile. Will be prepended with ``http://``
            if not present
        location
            The city or country describing where the user of the account is
            located. The contents are not normalized or geocoded in any way.
            A description of the user owning the account.
        profile_link_color
            Sets a hex value that controls the color scheme of links used on
            the authenticating user's profile page on twitter.com. This must be
            a valid hexadecimal value, and may be either three or six
            characters (ex: F00 or FF0000).
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-account-update_profile
            'POST', 'account/update_profile', endpoint_parameters=(
                'name', 'url', 'location', 'description', 'profile_link_color',
                'include_entities', 'skip_status'
    def update_profile_banner(self, filename, *, file=None, **kwargs):
        """update_profile_banner(filename, *, file, width, height, \
                                 offset_left, offset_top)
        Uploads a profile banner on behalf of the authenticating user.
        file:
        width
            The width of the preferred section of the image being uploaded in
            pixels. Use with ``height``, ``offset_left``, and ``offset_top`` to
            select the desired region of the image to use.
            The height of the preferred section of the image being uploaded in
            pixels. Use with ``width``, ``offset_left``, and ``offset_top`` to
        offset_left
            The number of pixels by which to offset the uploaded image from the
            left. Use with ``height``, ``width``, and ``offset_top`` to select
            the desired region of the image to use.
        offset_top
            top. Use with ``height``, ``width``, and ``offset_left`` to select
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-account-update_profile_banner
                files = {'banner': (filename, file)}
                files = {'banner': stack.enter_context(open(filename, 'rb'))}
                'POST', 'account/update_profile_banner', endpoint_parameters=(
                    'width', 'height', 'offset_left', 'offset_top'
                ), files=files, **kwargs
    def update_profile_image(self, filename, *, file=None, **kwargs):
        """update_profile_image(filename, *, file, include_entities, \
                                skip_status)
        Update the authenticating user's profile image. Valid formats: GIF,
        JPG, or PNG
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-account-update_profile_image
            files = {'image': (filename, file)}
            files = {'image': open(filename, 'rb')}
            'POST', 'account/update_profile_image', endpoint_parameters=(
    def create_saved_search(self, query, **kwargs):
        """create_saved_search(query)
        Creates a saved search for the authenticated user.
            The query of the search the user would like to save.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-saved_searches-create
            'POST', 'saved_searches/create', endpoint_parameters=(
                'query',
            ), query=query, **kwargs
    def destroy_saved_search(self, id, **kwargs):
        """destroy_saved_search(id)
        Destroys a saved search for the authenticated user. The search
        specified by ID must be owned by the authenticating user.
            The ID of the saved search to be deleted.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/manage-account-settings/api-reference/post-saved_searches-destroy-id
        return self.request('POST', f'saved_searches/destroy/{id}', **kwargs)
    # Mute, block, and report users
    def get_blocked_ids(self, **kwargs):
        """get_blocked_ids(*, stringify_ids, cursor)
        Returns an array of numeric user IDs the authenticating user is
        blocking.
            Renamed from ``API.blocks_ids``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/get-blocks-ids
            'GET', 'blocks/ids', endpoint_parameters=(
                'stringify_ids', 'cursor',
    def get_blocks(self, **kwargs):
        """get_blocks(*, include_entities, skip_status, cursor)
        Returns an array of user objects that the authenticating user is
            Renamed from ``API.blocks``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/get-blocks-list
            'GET', 'blocks/list', endpoint_parameters=(
                'include_entities', 'skip_status', 'cursor'
    def get_muted_ids(self, **kwargs):
        """get_muted_ids(*, stringify_ids, cursor)
        Returns an array of numeric user IDs the authenticating user has muted.
            Renamed from ``API.mutes_ids``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/get-mutes-users-ids
            'GET', 'mutes/users/ids', endpoint_parameters=(
                'stringify_ids', 'cursor'
    def get_mutes(self, **kwargs):
        """get_mutes(*, cursor, include_entities, skip_status)
        Returns an array of user objects the authenticating user has muted.
            Renamed from ``API.mutes``
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/get-mutes-users-list
            'GET', 'mutes/users/list', endpoint_parameters=(
    def create_block(self, **kwargs):
        """create_block(*, screen_name, user_id, include_entities, skip_status)
        Blocks the specified user from following the authenticating user. In
        addition the blocked user will not show in the authenticating users
        mentions or timeline (unless retweeted by another user). If a follow or
        friend relationship exists it is destroyed.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/post-blocks-create
            'POST', 'blocks/create', endpoint_parameters=(
                'screen_name', 'user_id', 'include_entities', 'skip_status'
    def destroy_block(self, **kwargs):
        """destroy_block(*, screen_name, user_id, include_entities, \
        Un-blocks the user specified in the ID parameter for the authenticating
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/post-blocks-destroy
            'POST', 'blocks/destroy', endpoint_parameters=(
    def create_mute(self, **kwargs):
        """create_mute(*, screen_name, user_id)
        Mutes the user specified in the ID parameter for the authenticating
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/post-mutes-users-create
            'POST', 'mutes/users/create', endpoint_parameters=(
    def destroy_mute(self, **kwargs):
        """destroy_mute(*, screen_name, user_id)
        Un-mutes the user specified in the ID parameter for the authenticating
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/post-mutes-users-destroy
            'POST', 'mutes/users/destroy', endpoint_parameters=(
    def report_spam(self, **kwargs):
        """report_spam(*, screen_name, user_id, perform_block)
        Report the specified user as a spam account to Twitter.
        perform_block
            A boolean indicating if the reported account should be blocked.
            Defaults to True.
        https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/mute-block-report-users/api-reference/post-users-report_spam
            'POST', 'users/report_spam', endpoint_parameters=(
                'screen_name', 'user_id', 'perform_block'
    # Sending and receiving events
    def delete_direct_message(self, id, **kwargs):
        """delete_direct_message(id)
        Deletes the direct message specified in the required ID parameter. The
        authenticating user must be the recipient of the specified direct
        message. Direct Messages are only removed from the interface of the
        user context provided. Other members of the conversation can still
        access the Direct Messages.
            Renamed from ``API.destroy_direct_message``
            The ID of the Direct Message that should be deleted.
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/sending-and-receiving/api-reference/delete-message-event
            'DELETE', 'direct_messages/events/destroy', endpoint_parameters=(
    @pagination(mode='dm_cursor')
    @payload('direct_message', list=True)
    def get_direct_messages(self, **kwargs):
        """get_direct_messages(*, count, cursor)
        Returns all Direct Message events (both sent and received) within the
        last 30 days. Sorted in reverse-chronological order.
            Renamed from ``API.list_direct_messages``
        :py:class:`List`[:class:`~tweepy.models.DirectMessage`]
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/sending-and-receiving/api-reference/list-events
            'GET', 'direct_messages/events/list', endpoint_parameters=(
                'count', 'cursor'
    @payload('direct_message')
    def get_direct_message(self, id, **kwargs):
        """get_direct_message(id)
        Returns a specific direct message.
            The ID of the Direct Message event that should be returned.
        :class:`~tweepy.models.DirectMessage`
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/sending-and-receiving/api-reference/get-event
            'GET', 'direct_messages/events/show', endpoint_parameters=(
    def send_direct_message(
        self, recipient_id, text, *, quick_reply_options=None,
        attachment_type=None, attachment_media_id=None, ctas=None, **kwargs
        """send_direct_message(recipient_id, text, *, quick_reply_options, \
                               attachment_type, attachment_media_id, ctas)
        Sends a new direct message to the specified user from the
        recipient_id
            The ID of the user who should receive the direct message.
            The text of your Direct Message. Max length of 10,000 characters.
        quick_reply_options
            Array of Options objects (20 max).
        attachment_type
            The attachment type. Can be media or location.
        attachment_media_id
            A media id to associate with the message. A Direct Message may only
            reference a single media_id.
        ctas
            Array of 1-3 call-to-action (CTA) button objects
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/sending-and-receiving/api-reference/new-event
        json_payload = {
            'event': {'type': 'message_create',
                      'message_create': {
                          'target': {'recipient_id': recipient_id},
                          'message_data': {'text': text}
        message_data = json_payload['event']['message_create']['message_data']
        if quick_reply_options is not None:
            message_data['quick_reply'] = {
                'type': 'options',
                'options': quick_reply_options
        if attachment_type is not None and attachment_media_id is not None:
            message_data['attachment'] = {
                'type': attachment_type,
                'media': {'id': attachment_media_id}
        if ctas is not None:
            message_data['ctas'] = ctas
            'POST', 'direct_messages/events/new',
            json_payload=json_payload, **kwargs
    # Typing indicator and read receipts
    def indicate_direct_message_typing(self, recipient_id, **kwargs):
        """indicate_direct_message_typing(recipient_id)
        Displays a visual typing indicator in the recipient’s Direct Message
        conversation view with the sender. Each request triggers a typing
        indicator animation with a duration of ~3 seconds.
        .. versionadded:: 4.9
            The user ID of the user to receive the typing indicator.
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/typing-indicator-and-read-receipts/api-reference/new-typing-indicator
            'POST', 'direct_messages/indicate_typing', endpoint_parameters=(
                'recipient_id',
            ), recipient_id=recipient_id, **kwargs
    def mark_direct_message_read(self, last_read_event_id, recipient_id,
        """mark_direct_message_read(last_read_event_id, recipient_id)
        Marks a message as read in the recipient’s Direct Message conversation
        view with the sender.
        last_read_event_id
            The message ID of the most recent message to be marked read. All
            messages before it will be marked read as well.
            The user ID of the user the message is from.
        https://developer.twitter.com/en/docs/twitter-api/v1/direct-messages/typing-indicator-and-read-receipts/api-reference/new-read-receipt
            'POST', 'direct_messages/mark_read', endpoint_parameters=(
                'last_read_event_id', 'recipient_id'
            ), last_read_event_id=last_read_event_id,
            recipient_id=recipient_id, **kwargs
    # Upload media
    @payload('media')
    def get_media_upload_status(self, media_id, **kwargs):
        """get_media_upload_status(media_id)
        Check on the progress of a chunked media upload. If the upload has
        succeeded, it's safe to create a Tweet with this ``media_id``.
        media_id
            The ID of the media to check.
        :class:`~tweepy.models.Media`
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/get-media-upload-status
            'GET', 'media/upload', endpoint_parameters=(
                'command', 'media_id'
            ), command='STATUS', media_id=media_id, upload_api=True, **kwargs
    def create_media_metadata(self, media_id, alt_text, **kwargs):
        """create_media_metadata(media_id, alt_text)
        This endpoint can be used to provide additional information about the
        uploaded ``media_id``. This feature is currently only supported for
        images and GIFs. Call this endpoint to attach additional metadata such
        as image alt text.
            The ID of the media to add alt text to.
        alt_text
            The alt text to add to the image.
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/post-media-metadata-create
            'media_id': media_id,
            'alt_text': {'text': alt_text}
            'POST', 'media/metadata/create', json_payload=json_payload,
            upload_api=True, **kwargs
    def media_upload(self, filename, *, file=None, chunked=False,
                     media_category=None, additional_owners=None, **kwargs):
        """media_upload(filename, *, file, chunked, media_category, \
                        additional_owners)
        Use this to upload media to Twitter. This calls either
        :meth:`simple_upload` or :meth:`chunked_upload`. Chunked media upload
        is automatically used for videos. If ``chunked`` is set or the media is
        a video, ``wait_for_async_finalize`` can be specified as a keyword
        argument to be passed to :meth:`chunked_upload`.
        chunked
            Whether or not to use chunked media upload. Videos use chunked
            upload regardless of this parameter. Defaults to ``False``.
        media_category
            |media_category|
        additional_owners
            |additional_owners|
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/overview
        file_type = None
            import imghdr
        except ModuleNotFoundError:
            # imghdr was removed in Python 3.13
            h = None
                location = file.tell()
                h = file.read(32)
                file.seek(location)
            file_type = imghdr.what(filename, h=h)
            if file_type is not None:
                file_type = 'image/' + file_type
        if file_type is None:
            file_type = mimetypes.guess_type(filename)[0]
        if chunked or file_type.startswith('video/'):
            return self.chunked_upload(
                filename, file=file, file_type=file_type,
                media_category=media_category,
                additional_owners=additional_owners, **kwargs
            return self.simple_upload(
                filename, file=file, media_category=media_category,
    def simple_upload(self, filename, *, file=None, media_category=None,
                      additional_owners=None, **kwargs):
        """simple_upload(filename, *, file, media_category, additional_owners)
        Use this endpoint to upload media to Twitter. This does not use the
        chunked upload endpoints.
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/post-media-upload
                files = {'media': (filename, file)}
                files = {'media': stack.enter_context(open(filename, 'rb'))}
            post_data = {}
            if media_category is not None:
                post_data['media_category'] = media_category
            if additional_owners is not None:
                post_data['additional_owners'] = additional_owners
                'POST', 'media/upload', post_data=post_data, files=files,
    def chunked_upload(self, filename, *, file=None, file_type=None,
                       wait_for_async_finalize=True, media_category=None,
        """chunked_upload( \
            filename, *, file, file_type, wait_for_async_finalize, \
            media_category, additional_owners \
        Use this to upload media to Twitter. This uses the chunked upload
        endpoints and calls :meth:`chunked_upload_init`,
        :meth:`chunked_upload_append`, and :meth:`chunked_upload_finalize`. If
        ``wait_for_async_finalize`` is set, this calls
        :meth:`get_media_upload_status` as well.
        file_type
            The MIME type of the media being uploaded.
        wait_for_async_finalize
            Whether to wait for Twitter's API to finish processing the media.
            Defaults to ``True``.
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/uploading-media/chunked-media-upload
        fp = file or open(filename, 'rb')
        start = fp.tell()
        fp.seek(0, 2)  # Seek to end of file
        file_size = fp.tell() - start
        fp.seek(start)
        min_chunk_size, remainder = divmod(file_size, 1000)
        min_chunk_size += bool(remainder)
        # Use 1 MiB as default chunk size
        chunk_size = kwargs.pop('chunk_size', 1024 * 1024)
        # Max chunk size is 5 MiB
        chunk_size = max(min(chunk_size, 5 * 1024 * 1024), min_chunk_size)
        segments, remainder = divmod(file_size, chunk_size)
        segments += bool(remainder)
        media_id = self.chunked_upload_init(
            file_size, file_type, media_category=media_category,
        ).media_id
        for segment_index in range(segments):
            # The APPEND command returns an empty response body
            self.chunked_upload_append(
                media_id, (filename, fp.read(chunk_size)), segment_index,
                **kwargs
        fp.close()
        media =  self.chunked_upload_finalize(media_id, **kwargs)
        if wait_for_async_finalize and hasattr(media, 'processing_info'):
                media.processing_info['state'] in (
                    'pending', 'in_progress'
                ) and 'error' not in media.processing_info
                time.sleep(media.processing_info['check_after_secs'])
                media = self.get_media_upload_status(media.media_id, **kwargs)
        return media
    def chunked_upload_append(self, media_id, media, segment_index, **kwargs):
        """chunked_upload_append(media_id, media, segment_index)
        Use this endpoint to upload a chunk (consecutive byte range) of the
        media file.
            The ``media_id`` returned from the initialization.
        media
            The raw binary file content being uploaded. It must be <= 5 MB.
        segment_index
            An ordered index of file chunk. It must be between 0-999 inclusive.
            The first segment has index 0, second segment has index 1, and so
            on.
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/post-media-upload-append
        post_data = {
            'command': 'APPEND',
            'segment_index': segment_index
        files = {'media': media}
    def chunked_upload_finalize(self, media_id, **kwargs):
        """chunked_upload_finalize(media_id)
        Use this endpoint after the entire media file is uploaded via
        appending. If (and only if) the response contains a
        ``processing_info field``, it may also be necessary to check its status
        and wait for it to return success before proceeding to Tweet creation.
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/post-media-upload-finalize
        headers = {'Content-Type': 'application/x-www-form-urlencoded'}
            'command': 'FINALIZE',
            'media_id': media_id
            'POST', 'media/upload', headers=headers, post_data=post_data,
    def chunked_upload_init(self, total_bytes, media_type, *,
                            media_category=None, additional_owners=None,
        """chunked_upload_init(total_bytes, media_type, *, media_category, \
        Use this endpoint to initiate a chunked file upload session.
        total_bytes
            The size of the media being uploaded in bytes.
        media_type
        https://developer.twitter.com/en/docs/twitter-api/v1/media/upload-media/api-reference/post-media-upload-init
            'command': 'INIT',
            'total_bytes': total_bytes,
            'media_type': media_type,
            post_data['additional_owners'] = list_to_csv(additional_owners)
    # Get locations with trending topics
    def available_trends(self, **kwargs):
        """available_trends()
        Returns the locations that Twitter has trending topic information for.
        The response is an array of "locations" that encode the location's
        WOEID (a Yahoo! Where On Earth ID) and some other human-readable
        information such as a canonical name and country the location belongs
        in.
            Renamed from ``API.trends_available``
        https://developer.twitter.com/en/docs/twitter-api/v1/trends/locations-with-trending-topics/api-reference/get-trends-available
        return self.request('GET', 'trends/available', **kwargs)
    def closest_trends(self, lat, long, **kwargs):
        """closest_trends(lat, long)
        Returns the locations that Twitter has trending topic information for,
        closest to a specified location.
        The response is an array of “locations” that encode the location’s
        WOEID and some other human-readable information such as a canonical
        name and country the location belongs in.
        A WOEID is a Yahoo! Where On Earth ID.
            Renamed from ``API.trends_closest``
            If provided with a long parameter the available trend locations
            will be sorted by distance, nearest to furthest, to the co-ordinate
            pair. The valid ranges for longitude is -180.0 to +180.0 (West is
            negative, East is positive) inclusive.
            If provided with a lat parameter the available trend locations will
            be sorted by distance, nearest to furthest, to the co-ordinate
        https://developer.twitter.com/en/docs/twitter-api/v1/trends/locations-with-trending-topics/api-reference/get-trends-closest
            'GET', 'trends/closest', endpoint_parameters=(
                'lat', 'long'
            ), lat=lat, long=long, **kwargs
    # Get trends near a location
    def get_place_trends(self, id, **kwargs):
        """get_place_trends(id *, exclude)
        Returns the top 50 trending topics for a specific WOEID, if trending
        information is available for it.
        The response is an array of “trend” objects that encode the name of the
        trending topic, the query parameter that can be used to search for the
        topic on Twitter Search, and the Twitter Search URL.
        This information is cached for 5 minutes. Requesting more frequently
        than that will not return any more data, and will count against your
        rate limit usage.
        The tweet_volume for the last 24 hours is also returned for many trends
        if this is available.
            Renamed from ``API.trends_place``
            The Yahoo! Where On Earth ID of the location to return trending
            information for. Global information is available by using 1 as the
            WOEID.
        exclude
            Setting this equal to hashtags will remove all hashtags from the
            trends list.
        https://developer.twitter.com/en/docs/twitter-api/v1/trends/trends-for-location/api-reference/get-trends-place
            'GET', 'trends/place', endpoint_parameters=(
                'id', 'exclude'
    # Get information about a place
    @payload('place')
    def geo_id(self, place_id, **kwargs):
        """geo_id(place_id)
        Given ``place_id``, provide more details about that place.
            Valid Twitter ID of a location.
        :class:`~tweepy.models.Place`
        https://developer.twitter.com/en/docs/twitter-api/v1/geo/place-information/api-reference/get-geo-id-place_id
        return self.request('GET', f'geo/id/{place_id}', **kwargs)
    # Get places near a location
    @payload('place', list=True)
    def reverse_geocode(self, lat, long, **kwargs):
        """reverse_geocode(lat, long, *, accuracy, granularity, max_results)
        Given a latitude and a longitude, searches for up to 20 places that can
        be used as a ``place_id`` when updating a status.
        This request is an informative call and will deliver generalized
        results about geography.
            The location's latitude.
            The location's longitude.
        accuracy
            Specify the "region" in which to search, such as a number (then
            this is a radius in meters, but it can also take a string that is
            suffixed with ft to specify feet). If this is not passed in, then
            it is assumed to be 0m
        granularity
            Assumed to be ``neighborhood`` by default; can also be ``city``.
        max_results
            A hint as to the maximum number of results to return. This is only
            a guideline, which may not be adhered to.
        :py:class:`List`[:class:`~tweepy.models.Place`]
        https://developer.twitter.com/en/docs/twitter-api/v1/geo/places-near-location/api-reference/get-geo-reverse_geocode
            'GET', 'geo/reverse_geocode', endpoint_parameters=(
                'lat', 'long', 'accuracy', 'granularity', 'max_results'
    def search_geo(self, **kwargs):
        """search_geo(*, lat, long, query, ip, granularity, max_results)
        Search for places that can be attached to a Tweet via
        :meth:`update_status`. Given a latitude and a longitude pair, an IP
        address, or a name, this request will return a list of all the valid
        places that can be used as the ``place_id`` when updating a status.
        Conceptually, a query can be made from the user's location, retrieve a
        list of places, have the user validate the location they are at, and
        then send the ID of this location with a call to
        :meth:`update_status`.
        This is the recommended method to use find places that can be attached
        to :meth:`update_status`. Unlike :meth:`reverse_geocode` which provides
        raw data access, this endpoint can potentially re-order places with
        regards to the user who is authenticated. This approach is also
        preferred for interactive place matching with the user.
        Some parameters in this method are only required based on the existence
        of other parameters. For instance, ``lat`` is required if ``long`` is
        provided, and vice-versa.
            `The Twitter API v1.1 geo/search endpoint that this method uses has
            been deprecated and has a retirement date of September 20, 2023.`_
            Renamed from ``API.geo_search``
            The latitude to search around. This parameter will be ignored
            unless it is inside the range -90.0 to +90.0 (North is positive)
            inclusive. It will also be ignored if there isn't a corresponding
            ``long`` parameter.
            The longitude to search around. The valid ranges for longitude are
            -180.0 to +180.0 (East is positive) inclusive. This parameter will
            be ignored if outside that range, if it is not a number, if
            ``geo_enabled`` is turned off, or if there not a corresponding
            ``lat`` parameter.
            Free-form text to match against while executing a geo-based query,
            best suited for finding nearby locations by name.
        ip
            An IP address. Used when attempting to fix geolocation based off of
            the user's IP address.
            This is the minimal granularity of place types to return and must
            be one of: ``neighborhood``, ``city``, ``admin`` or ``country``.
            If no granularity is provided for the request ``neighborhood`` is
            assumed.
            Setting this to ``city``, for example, will find places which have
            a type of ``city``, ``admin`` or ``country``.
            A hint as to the number of results to return. This does not
            guarantee that the number of results returned will equal
            ``max_results``, but instead informs how many "nearby" results to
            return. Ideally, only pass in the number of places you intend to
            display to the user here.
        https://developer.twitter.com/en/docs/twitter-api/v1/geo/places-near-location/api-reference/get-geo-search
        .. _The Twitter API v1.1 geo/search endpoint that this method uses has
            been deprecated and has a retirement date of September 20, 2023.:
            'GET', 'geo/search', endpoint_parameters=(
                'lat', 'long', 'query', 'ip', 'granularity', 'max_results'
    # Get Twitter supported languages
    def supported_languages(self, **kwargs):
        """supported_languages()
        Returns the list of languages supported by Twitter along with the
        language code supported by Twitter.
        The language code may be formatted as ISO 639-1 alpha-2 (``en``), ISO
        639-3 alpha-3 (``msa``), or ISO 639-1 alpha-2 combined with an ISO
        3166-1 alpha-2 localization (``zh-tw``).
        https://developer.twitter.com/en/docs/twitter-api/v1/developer-utilities/supported-languages/api-reference/get-help-languages
        return self.request('GET', 'help/languages', **kwargs)
    # Get app rate limit status
    def rate_limit_status(self, **kwargs):
        """rate_limit_status(*, resources)
        Returns the current rate limits for methods belonging to the specified
        resource families. When using application-only auth, this method's
        response indicates the application-only auth rate limiting context.
        resources
            A comma-separated list of resource families you want to know the
            current rate limit disposition for.
        https://developer.twitter.com/en/docs/twitter-api/v1/developer-utilities/rate-limit-status/api-reference/get-application-rate_limit_status
            'GET', 'application/rate_limit_status', endpoint_parameters=(
                'resources',
This module contains classes that are available for the .spec files.
Spec file is generated by PyInstaller. The generated code from .spec file
is a way how PyInstaller does the dependency analysis and creates executable.
from operator import itemgetter
from PyInstaller import HOMEPATH, PLATFORM
from PyInstaller.archive.writers import CArchiveWriter, ZlibArchiveWriter
from PyInstaller.building.datastruct import Target, _check_guts_eq, normalize_pyz_toc, normalize_toc
from PyInstaller.building.utils import (
    _check_guts_toc, _make_clean_directory, _rmtree, process_collected_binary, get_code_object, compile_pymodule
from PyInstaller.building.splash import Splash  # argument type validation in EXE
from PyInstaller.compat import is_cygwin, is_darwin, is_linux, is_win, strict_collect_mode, is_nogil, is_aix
from PyInstaller.depend import bindepend
from PyInstaller.depend.analysis import get_bootstrap_modules
import PyInstaller.utils.misc as miscutils
if is_win:
    from PyInstaller.utils.win32 import (icon, versioninfo, winmanifest, winresource, winutils)
if is_darwin:
    import PyInstaller.utils.osx as osxutils
class PYZ(Target):
    Creates a zlib-based PYZ archive that contains byte-compiled pure Python modules.
    def __init__(self, *tocs, **kwargs):
        tocs
            One or more TOC (Table of Contents) lists, usually an `Analysis.pure`.
            Possible keyword arguments:
                A filename for the .pyz. Normally not needed, as the generated name will do fine.
        if kwargs.get("cipher"):
            from PyInstaller.exceptions import RemovedCipherFeatureError
            raise RemovedCipherFeatureError(
                "Please remove the 'cipher' arguments to PYZ() and Analysis() in your spec file."
        name = kwargs.get('name', None)
            self.name = os.path.splitext(self.tocfilename)[0] + '.pyz'
        # PyInstaller bootstrapping modules.
        bootstrap_dependencies = get_bootstrap_modules()
        # Compile the python modules that are part of bootstrap dependencies, so that they can be collected into the
        # CArchive/PKG and imported by the bootstrap script.
        self.dependencies = []
        workpath = os.path.join(CONF['workpath'], 'localpycs')
        for name, src_path, typecode in bootstrap_dependencies:
            if typecode == 'PYMODULE':
                # Compile pymodule and include the compiled .pyc file.
                pyc_path = compile_pymodule(
                    src_path,
                    workpath,
                    # Never optimize bootstrap dependencies!
                    optimize=0,
                    code_cache=None,
                self.dependencies.append((name, pyc_path, typecode))
                # Include as is (extensions).
                self.dependencies.append((name, src_path, typecode))
        # Merge input TOC(s) and their code object dictionaries (if available). Skip the bootstrap modules, which will
        # be passed on to CArchive/PKG.
        bootstrap_module_names = set(name for name, _, typecode in self.dependencies if typecode == 'PYMODULE')
        self.toc = []
        self.code_dict = {}
        for toc in tocs:
            # Check if code cache association exists for the given TOC list
            code_cache = CONF['code_cache'].get(id(toc))
            if code_cache is not None:
                self.code_dict.update(code_cache)
            for entry in toc:
                name, _, typecode = entry
                # PYZ expects only PYMODULE entries (python code objects).
                assert typecode in {'PYMODULE', 'PYMODULE-1', 'PYMODULE-2'}, f"Invalid entry passed to PYZ: {entry}!"
                # Module required during bootstrap; skip to avoid collecting a duplicate.
                if name in bootstrap_module_names:
                self.toc.append(entry)
        # Normalize TOC
        self.toc = normalize_pyz_toc(self.toc)
        # Alphabetically sort the TOC to enable reproducible builds.
        self.toc.sort()
        self.__postinit__()
    _GUTS = (
        # input parameters
        ('name', _check_guts_eq),
        ('toc', _check_guts_toc),
        # no calculated/analysed values
    def assemble(self):
        logger.info("Building PYZ (ZlibArchive) %s", self.name)
        # Ensure code objects are available for all modules we are about to collect.
        # NOTE: PEP-420 namespace packages (marked by src_path being set to '-') do not have code objects.
        # NOTE: `self.toc` is already sorted by names.
        archive_toc = []
        for entry in self.toc:
            name, src_path, typecode = entry
            if src_path not in {'-', None} and name not in self.code_dict:
                # The code object is not available from the ModuleGraph's cache; re-create it.
                optim_level = {'PYMODULE': 0, 'PYMODULE-1': 1, 'PYMODULE-2': 2}[typecode]
                    self.code_dict[name] = get_code_object(name, src_path, optimize=optim_level)
                except SyntaxError:
                    # The module was likely written for different Python version; exclude it
            archive_toc.append(entry)
        # Create the archive
        ZlibArchiveWriter(self.name, archive_toc, code_dict=self.code_dict)
        logger.info("Building PYZ (ZlibArchive) %s completed successfully.", self.name)
class PKG(Target):
    Creates a CArchive. CArchive is the data structure that is embedded into the executable. This data structure allows
    to include various read-only data in a single-file deployment.
    xformdict = {
        # PYMODULE entries are already byte-compiled, so we do not need to encode optimization level in the low-level
        # typecodes. PYSOURCE entries are byte-compiled by the underlying writer, so we need to pass the optimization
        # level via low-level typecodes.
        'PYMODULE': 'm',
        'PYMODULE-1': 'm',
        'PYMODULE-2': 'm',
        'PYSOURCE': 's',
        'PYSOURCE-1': 's1',
        'PYSOURCE-2': 's2',
        'EXTENSION': 'b',
        'PYZ': 'z',
        'PKG': 'a',
        'DATA': 'x',
        'BINARY': 'b',
        'ZIPFILE': 'Z',
        'EXECUTABLE': 'b',
        'DEPENDENCY': 'd',
        'SPLASH': 'l',
        'SYMLINK': 'n',
        toc,
        python_lib_name,
        cdict=None,
        exclude_binaries=False,
        strip_binaries=False,
        upx_binaries=False,
        upx_exclude=None,
        target_arch=None,
        codesign_identity=None,
        entitlements_file=None
        toc
            A TOC (Table of Contents) list.
        python_lib_name
            Name of the python shared library to store in PKG. Required by bootloader.
            An optional filename for the PKG.
        cdict
            Dictionary that specifies compression by typecode. For Example, PYZ is left uncompressed so that it
            can be accessed inside the PKG. The default uses sensible values. If zlib is not available, no
            compression is used.
        exclude_binaries
            If True, EXTENSIONs and BINARYs will be left out of the PKG, and forwarded to its container (usually
            a COLLECT).
        strip_binaries
            If True, use 'strip' command to reduce the size of binary files.
        upx_binaries
        self.toc = normalize_toc(toc)  # Ensure guts contain normalized TOC
        self.python_lib_name = python_lib_name
        self.cdict = cdict
            self.name = os.path.splitext(self.tocfilename)[0] + '.pkg'
        self.exclude_binaries = exclude_binaries
        self.strip_binaries = strip_binaries
        self.upx_binaries = upx_binaries
        self.upx_exclude = upx_exclude or []
        self.target_arch = target_arch
        self.codesign_identity = codesign_identity
        self.entitlements_file = entitlements_file
        # This dict tells PyInstaller what items embedded in the executable should be compressed.
        if self.cdict is None:
            self.cdict = {
                'EXTENSION': COMPRESSED,
                'DATA': COMPRESSED,
                'BINARY': COMPRESSED,
                'EXECUTABLE': COMPRESSED,
                'PYSOURCE': COMPRESSED,
                'PYMODULE': COMPRESSED,
                'SPLASH': COMPRESSED,
                # Do not compress PYZ as a whole, as it contains individually-compressed modules.
                'PYZ': UNCOMPRESSED,
                # Do not compress target names in symbolic links.
                'SYMLINK': UNCOMPRESSED,
    _GUTS = (  # input parameters
        ('cdict', _check_guts_eq),
        ('toc', _check_guts_toc),  # list unchanged and no newer files
        ('python_lib_name', _check_guts_eq),
        ('exclude_binaries', _check_guts_eq),
        ('strip_binaries', _check_guts_eq),
        ('upx_binaries', _check_guts_eq),
        ('upx_exclude', _check_guts_eq),
        ('target_arch', _check_guts_eq),
        ('codesign_identity', _check_guts_eq),
        ('entitlements_file', _check_guts_eq),
        logger.info("Building PKG (CArchive) %s", os.path.basename(self.name))
        pkg_file = pathlib.Path(self.name).resolve()  # Used to detect attempts at PKG feeding itself
        bootstrap_toc = []  # TOC containing bootstrap scripts and modules, which must not be sorted.
        archive_toc = []  # TOC containing all other elements. Sorted to enable reproducible builds.
        for dest_name, src_name, typecode in self.toc:
            # Ensure that the source file exists, if necessary. Skip the check for OPTION entries, where 'src_name' is
            # None. Also skip DEPENDENCY entries due to special contents of 'dest_name' and/or 'src_name'. Same for the
            # SYMLINK entries, where 'src_name' is relative target name for symbolic link.
            if typecode not in {'OPTION', 'DEPENDENCY', 'SYMLINK'}:
                if not os.path.exists(src_name):
                    if strict_collect_mode:
                        raise ValueError(f"Non-existent resource {src_name}, meant to be collected as {dest_name}!")
                            "Ignoring non-existent resource %s, meant to be collected as %s", src_name, dest_name
                # Detect attempt at collecting PKG into itself, as it results in and endless feeding loop and exhaustion
                # of all available storage space.
                if pathlib.Path(src_name).resolve() == pkg_file:
                    raise ValueError(f"Trying to collect PKG file {src_name} into itself!")
            if typecode in ('BINARY', 'EXTENSION'):
                if self.exclude_binaries:
                    # This is onedir-specific codepath - the EXE and consequently PKG should not be passed the Analysis'
                    # `datas` and `binaries` TOCs (unless the user messes up the .spec file). However, EXTENSION entries
                    # might still slip in via `PYZ.dependencies`, which are merged by EXE into its TOC and passed on to
                    # PKG here. Such entries need to be passed to the parent container (the COLLECT) via
                    # `PKG.dependencies`.
                    # This codepath formerly performed such pass-through only for EXTENSION entries, but in order to
                    # keep code simple, we now also do it for BINARY entries. In a sane world, we do not expect to
                    # encounter them here; but if they do happen to pass through here and we pass them on, the
                    # container's TOC de-duplication should take care of them (same as with EXTENSION ones, really).
                    self.dependencies.append((dest_name, src_name, typecode))
                    # This is onefile-specific codepath. The binaries (both EXTENSION and BINARY entries) need to be
                    # processed using `process_collected_binary` helper.
                    src_name = process_collected_binary(
                        src_name,
                        dest_name,
                        use_strip=self.strip_binaries,
                        use_upx=self.upx_binaries,
                        upx_exclude=self.upx_exclude,
                        target_arch=self.target_arch,
                        codesign_identity=self.codesign_identity,
                        entitlements_file=self.entitlements_file,
                        strict_arch_validation=(typecode == 'EXTENSION'),
                    archive_toc.append((dest_name, src_name, self.cdict.get(typecode, False), self.xformdict[typecode]))
            elif typecode in ('DATA', 'ZIPFILE'):
                # Same logic as above for BINARY and EXTENSION; if `exclude_binaries` is set, we are in onedir mode;
                # we should exclude DATA (and ZIPFILE) entries and instead pass them on via PKG's `dependencies`. This
                # prevents a onedir application from becoming a broken onefile one if user accidentally passes datas
                # and binaries TOCs to EXE instead of COLLECT.
                    if typecode == 'DATA' and os.access(src_name, os.X_OK):
                        # DATA with executable bit set (e.g., shell script); turn into binary so that executable bit is
                        # restored on the extracted file.
                        carchive_typecode = 'b'
                        carchive_typecode = self.xformdict[typecode]
                    archive_toc.append((dest_name, src_name, self.cdict.get(typecode, False), carchive_typecode))
            elif typecode == 'OPTION':
                archive_toc.append((dest_name, '', False, 'o'))
            elif typecode in {'PYSOURCE', 'PYSOURCE-1', 'PYSOURCE-2', 'PYMODULE', 'PYMODULE-1', 'PYMODULE-2'}:
                # Collect python script and modules in a TOC that will not be sorted.
                bootstrap_toc.append((dest_name, src_name, self.cdict.get(typecode, False), self.xformdict[typecode]))
            elif typecode == 'PYZ':
                # Override PYZ name in the PKG archive into PYZ.pyz, regardless of what the original name was. The
                # bootloader looks for PYZ via the typecode and implicitly expects a single entry, so the name does
                # not matter. However, having a fixed name matters if we want reproducibility in scenarios where
                # multiple builds are performed within the same process (for example, on our CI).
                archive_toc.append(('PYZ.pyz', src_name, self.cdict.get(typecode, False), self.xformdict[typecode]))
                # PKG, DEPENDENCY, SPLASH, SYMLINK
        # Sort content alphabetically by type and name to enable reproducible builds.
        archive_toc.sort(key=itemgetter(3, 0))
        # Do *not* sort modules and scripts, as their order is important.
        # TODO: Think about having all modules first and then all scripts.
        CArchiveWriter(self.name, bootstrap_toc + archive_toc, pylib_name=self.python_lib_name)
        logger.info("Building PKG (CArchive) %s completed successfully.", os.path.basename(self.name))
class EXE(Target):
    Creates the final executable of the frozen app. This bundles all necessary files together.
            One or more arguments that are either an instance of `Target` or an iterable representing TOC list.
            bootloader_ignore_signals
                Non-Windows only. If True, the bootloader process will ignore all ignorable signals. If False (default),
                it will forward all signals to the child process. Useful in situations where for example a supervisor
                process signals both the bootloader and the child (e.g., via a process group) to avoid signalling the
                child twice.
            console
                On Windows or macOS governs whether to use the console executable or the windowed executable. Always
                True on Linux/Unix (always console executable - it does not matter there).
            hide_console
                Windows only. In console-enabled executable, hide or minimize the console window if the program owns the
                console window (i.e., was not launched from existing console window). Depending on the setting, the
                console is hidden/mininized either early in the bootloader execution ('hide-early', 'minimize-early') or
                late in the bootloader execution ('hide-late', 'minimize-late'). The early option takes place as soon as
                the PKG archive is found. In onefile builds, the late option takes place after application has unpacked
                itself and before it launches the child process. In onedir builds, the late option takes place before
                starting the embedded python interpreter.
            disable_windowed_traceback
                Disable traceback dump of unhandled exception in windowed (noconsole) mode (Windows and macOS only),
                and instead display a message that this feature is disabled.
                Setting to True gives you progress messages from the executable (for console=False there will be
                annoying MessageBoxes on Windows).
                The filename for the executable. On Windows suffix '.exe' is appended.
                Forwarded to the PKG the EXE builds.
                Windows and macOS only. icon='myicon.ico' to use an icon file or icon='notepad.exe,0' to grab an icon
                resource. Defaults to use PyInstaller's console or windowed icon. Use icon=`NONE` to not add any icon.
                Windows only. version='myversion.txt'. Use grab_version.py to get a version resource from an executable
                and then edit the output to create your own. (The syntax of version resources is so arcane that I would
                not attempt to write one from scratch).
            uac_admin
                Windows only. Setting to True creates a Manifest with will request elevation upon application start.
            uac_uiaccess
                Windows only. Setting to True allows an elevated application to work with Remote Desktop.
            argv_emulation
                macOS only. Enables argv emulation in macOS .app bundles (i.e., windowed bootloader). If enabled, the
                initial open document/URL Apple Events are intercepted by bootloader and converted into sys.argv.
            target_arch
                macOS only. Used to explicitly specify the target architecture; either single-arch ('x86_64' or 'arm64')
                or 'universal2'. Used in checks that the collected binaries contain the requires arch slice(s) and/or
                to convert fat binaries into thin ones as necessary. If not specified (default), a single-arch build
                corresponding to running architecture is assumed.
            codesign_identity
                macOS only. Use the provided identity to sign collected binaries and the generated executable. If
                signing identity is not provided, ad-hoc signing is performed.
            entitlements_file
                macOS only. Optional path to entitlements file to use with code signing of collected binaries
                (--entitlements option to codesign utility).
            contents_directory
                Onedir mode only. Specifies the name of the directory where all files par the executable will be placed.
                Setting the name to '.' (or '' or None) re-enables old onedir layout without contents directory.
        # Available options for EXE in .spec files.
        self.exclude_binaries = kwargs.get('exclude_binaries', False)
        self.bootloader_ignore_signals = kwargs.get('bootloader_ignore_signals', False)
        self.console = kwargs.get('console', True)
        self.hide_console = kwargs.get('hide_console', None)
        self.disable_windowed_traceback = kwargs.get('disable_windowed_traceback', False)
        self.debug = kwargs.get('debug', False)
        self.name = kwargs.get('name', None)
        self.icon = kwargs.get('icon', None)
        self.versrsrc = kwargs.get('version', None)
        self.manifest = kwargs.get('manifest', None)
        self.resources = kwargs.get('resources', [])
        self.strip = kwargs.get('strip', False)
        self.upx_exclude = kwargs.get("upx_exclude", [])
        self.runtime_tmpdir = kwargs.get('runtime_tmpdir', None)
        self.contents_directory = kwargs.get("contents_directory", "_internal")
        # If ``append_pkg`` is false, the archive will not be appended to the exe, but copied beside it.
        self.append_pkg = kwargs.get('append_pkg', True)
        # On Windows allows the exe to request admin privileges.
        self.uac_admin = kwargs.get('uac_admin', False)
        self.uac_uiaccess = kwargs.get('uac_uiaccess', False)
        # macOS argv emulation
        self.argv_emulation = kwargs.get('argv_emulation', False)
        # Target architecture (macOS only)
        self.target_arch = kwargs.get('target_arch', None)
            if self.target_arch is None:
                self.target_arch = platform.machine()
                assert self.target_arch in {'x86_64', 'arm64', 'universal2'}, \
                    f"Unsupported target arch: {self.target_arch}"
            logger.info("EXE target arch: %s", self.target_arch)
            self.target_arch = None  # explicitly disable
        # Code signing identity (macOS only)
        self.codesign_identity = kwargs.get('codesign_identity', None)
            logger.info("Code signing identity: %s", self.codesign_identity)
            self.codesign_identity = None  # explicitly disable
        # Code signing entitlements
        self.entitlements_file = kwargs.get('entitlements_file', None)
        # UPX needs to be both available and enabled for the target.
        self.upx = CONF['upx_available'] and kwargs.get('upx', False)
        # Catch and clear options that are unsupported on specific platforms.
        if self.versrsrc and not is_win:
            logger.warning('Ignoring version information; supported only on Windows!')
            self.versrsrc = None
        if self.manifest and not is_win:
            logger.warning('Ignoring manifest; supported only on Windows!')
            self.manifest = None
        if self.resources and not is_win:
            logger.warning('Ignoring resources; supported only on Windows!')
            self.resources = []
        if self.icon and not (is_win or is_darwin):
            logger.warning('Ignoring icon; supported only on Windows and macOS!')
            self.icon = None
        if self.hide_console and not is_win:
            logger.warning('Ignoring hide_console; supported only on Windows!')
            self.hide_console = None
        if self.contents_directory in ("", "."):
            self.contents_directory = None  # Re-enable old onedir layout without contents directory.
        elif self.contents_directory == ".." or "/" in self.contents_directory or "\\" in self.contents_directory:
                f'ERROR: Invalid value "{self.contents_directory}" passed to `--contents-directory` or '
                '`contents_directory`. Exactly one directory level is required (or just "." to disable the '
                'contents directory).'
        if not kwargs.get('embed_manifest', True):
            from PyInstaller.exceptions import RemovedExternalManifestError
            raise RemovedExternalManifestError(
                "Please remove the 'embed_manifest' argument to EXE() in your spec file."
        # Old .spec format included in 'name' the path where to put created app. New format includes only exename.
        # Ignore fullpath in the 'name' and prepend DISTPATH or WORKPATH.
        # DISTPATH - onefile
        # WORKPATH - onedir
            # onedir mode - create executable in WORKPATH.
            self.name = os.path.join(CONF['workpath'], os.path.basename(self.name))
            # onefile mode - create executable in DISTPATH.
            self.name = os.path.join(CONF['distpath'], os.path.basename(self.name))
        # Old .spec format included on Windows in 'name' .exe suffix.
        if is_win or is_cygwin:
            # Append .exe suffix if it is not already there.
            if not self.name.endswith('.exe'):
                self.name += '.exe'
            base_name = os.path.splitext(os.path.basename(self.name))[0]
            base_name = os.path.basename(self.name)
        # Create the CArchive PKG in WORKPATH. When instancing PKG(), set name so that guts check can test whether the
        # file already exists.
        self.pkgname = os.path.join(CONF['workpath'], base_name + '.pkg')
        for arg in args:
            # Valid arguments: PYZ object, Splash object, and TOC-list iterables
            if isinstance(arg, (PYZ, Splash)):
                # Add object as an entry to the TOC, and merge its dependencies TOC
                if isinstance(arg, PYZ):
                    self.toc.append((os.path.basename(arg.name), arg.name, "PYZ"))
                    self.toc.append((os.path.basename(arg.name), arg.name, "SPLASH"))
                self.toc.extend(arg.dependencies)
            elif miscutils.is_iterable(arg):
                # TOC-like iterable
                self.toc.extend(arg)
                raise TypeError(f"Invalid argument type for EXE: {type(arg)!r}")
        if is_nogil:
            # Signal to bootloader that python was built with Py_GIL_DISABLED, in order to select correct `PyConfig`
            # structure layout at run-time.
            self.toc.append(("pyi-python-flag Py_GIL_DISABLED", "", "OPTION"))
        if self.runtime_tmpdir is not None:
            self.toc.append(("pyi-runtime-tmpdir " + self.runtime_tmpdir, "", "OPTION"))
        if self.bootloader_ignore_signals:
            # no value; presence means "true"
            self.toc.append(("pyi-bootloader-ignore-signals", "", "OPTION"))
        if self.disable_windowed_traceback:
            self.toc.append(("pyi-disable-windowed-traceback", "", "OPTION"))
        if self.argv_emulation:
            self.toc.append(("pyi-macos-argv-emulation", "", "OPTION"))
        if self.contents_directory:
            self.toc.append(("pyi-contents-directory " + self.contents_directory, "", "OPTION"))
        if self.hide_console:
            # Validate the value
            _HIDE_CONSOLE_VALUES = {'hide-early', 'minimize-early', 'hide-late', 'minimize-late'}
            self.hide_console = self.hide_console.lower()
            if self.hide_console not in _HIDE_CONSOLE_VALUES:
                    f"Invalid hide_console value: {self.hide_console}! Allowed values: {_HIDE_CONSOLE_VALUES}"
            self.toc.append((f"pyi-hide-console {self.hide_console}", "", "OPTION"))
        # If the icon path is relative, make it relative to the .spec file.
        if self.icon and self.icon != "NONE":
            if isinstance(self.icon, list):
                self.icon = [self._makeabs(ic) for ic in self.icon]
                self.icon = [self._makeabs(self.icon)]
            if not self.icon:
                # --icon not specified; use default from bootloader folder
                if self.console:
                    ico = 'icon-console.ico'
                    ico = 'icon-windowed.ico'
                self.icon = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'bootloader', 'images', ico)
            # Prepare manifest for the executable by creating minimal manifest or modifying the supplied one.
            if self.manifest:
                # Determine if we were given a filename or an XML string.
                if "<" in self.manifest:
                    self.manifest = self.manifest.encode("utf-8")
                    self.manifest = self._makeabs(self.manifest)
                    with open(self.manifest, "rb") as fp:
                        self.manifest = fp.read()
            self.manifest = winmanifest.create_application_manifest(self.manifest, self.uac_admin, self.uac_uiaccess)
            if self.versrsrc:
                if isinstance(self.versrsrc, versioninfo.VSVersionInfo):
                    # We were passed a valid versioninfo.VSVersionInfo structure
                elif isinstance(self.versrsrc, (str, bytes, os.PathLike)):
                    # File path; either absolute, or relative to the spec file
                    self.versrsrc = self._makeabs(self.versrsrc)
                    logger.debug("Loading version info from file: %r", self.versrsrc)
                    self.versrsrc = versioninfo.load_version_info_from_text_file(self.versrsrc)
                    raise TypeError(f"Unsupported type for version info argument: {type(self.versrsrc)!r}")
        # Identify python shared library. This is needed both for PKG (where we need to store the name so that
        # bootloader can look it up), and for macOS-specific processing of the generated executable (adjusting the SDK
        # version).
        # NOTE: we already performed an equivalent search (using the same `get_python_library_path` helper) during the
        # analysis stage to ensure that the python shared library is collected. Unfortunately, with the way data passing
        # works in onedir builds, we cannot look up the value in the TOC at this stage, and we need to search again.
        self.python_lib = bindepend.get_python_library_path()
        if self.python_lib is None:
            from PyInstaller.exceptions import PythonLibraryNotFoundError
            raise PythonLibraryNotFoundError()
        # On AIX, the python shared library might in fact be an ar archive with shared object inside it, and needs to
        # be `dlopen`'ed with full name (for example, `libpython3.9.a(libpython3.9.so)`. So if the library's suffix is
        # .a, adjust the name accordingly, assuming fixed format for the shared object name. NOTE: the information about
        # shared object name is in fact available from `ldd` but not propagated from our binary dependency analysis. If
        # we ever need to determine the shared object's name dynamically, we could write a simple ar parser, based on
        # information from `https://www.ibm.com/docs/en/aix/7.3?topic=formats-ar-file-format-big`.
        if is_aix:
            _, ext = os.path.splitext(self.python_lib)
            if ext == '.a':
                _py_major, _py_minor = sys.version_info[:2]
                self.python_lib += f"(libpython{_py_major}.{_py_minor}.so)"
        self.toc = normalize_toc(self.toc)
        self.pkg = PKG(
            toc=self.toc,
            python_lib_name=os.path.basename(self.python_lib),
            name=self.pkgname,
            cdict=kwargs.get('cdict', None),
            exclude_binaries=self.exclude_binaries,
            strip_binaries=self.strip,
            upx_binaries=self.upx,
            entitlements_file=self.entitlements_file
        self.dependencies = self.pkg.dependencies
        # Get the path of the bootloader and store it in a TOC, so it can be checked for being changed.
        exe = self._bootloader_file('run', '.exe' if is_win or is_cygwin else '')
        self.exefiles = [(os.path.basename(exe), exe, 'EXECUTABLE')]
        ('console', _check_guts_eq),
        ('debug', _check_guts_eq),
        ('icon', _check_guts_eq),
        ('versrsrc', _check_guts_eq),
        ('uac_admin', _check_guts_eq),
        ('uac_uiaccess', _check_guts_eq),
        ('manifest', _check_guts_eq),
        ('append_pkg', _check_guts_eq),
        ('argv_emulation', _check_guts_eq),
        # for the case the directory is shared between platforms:
        ('pkgname', _check_guts_eq),
        ('toc', _check_guts_eq),
        ('resources', _check_guts_eq),
        ('strip', _check_guts_eq),
        ('upx', _check_guts_eq),
        ('mtm', None),  # checked below
        # derived values
        ('exefiles', _check_guts_toc),
        ('python_lib', _check_guts_eq),
    def _check_guts(self, data, last_build):
        if not os.path.exists(self.name):
            logger.info("Rebuilding %s because %s missing", self.tocbasename, os.path.basename(self.name))
        if not self.append_pkg and not os.path.exists(self.pkgname):
            logger.info("Rebuilding because %s missing", os.path.basename(self.pkgname))
        if Target._check_guts(self, data, last_build):
        mtm = data['mtm']
        if mtm != miscutils.mtime(self.name):
            logger.info("Rebuilding %s because mtimes don't match", self.tocbasename)
        if mtm < miscutils.mtime(self.pkg.tocfilename):
            logger.info("Rebuilding %s because pkg is more recent", self.tocbasename)
    def _makeabs(path):
        Helper for anchoring relative paths to spec file location.
        if os.path.isabs(path):
            return os.path.join(CONF['specpath'], path)
    def _bootloader_file(self, exe, extension=None):
        Pick up the right bootloader file - debug, console, windowed.
        # Having console/windowed bootloader makes sense only on Windows and macOS.
        if is_win or is_darwin:
            if not self.console:
                exe = exe + 'w'
        # There are two types of bootloaders:
        # run     - release, no verbose messages in console.
        # run_d   - contains verbose messages in console.
        if self.debug:
            exe = exe + '_d'
        if extension:
            exe = exe + extension
        bootloader_file = os.path.join(HOMEPATH, 'PyInstaller', 'bootloader', PLATFORM, exe)
        logger.info('Bootloader %s' % bootloader_file)
        return bootloader_file
        # On Windows, we used to append .notanexecutable to the intermediate/temporary file name to (attempt to)
        # prevent interference from anti-virus programs with the build process (see #6467). This is now disabled
        # as we wrap all processing steps that modify the executable in the `_retry_operation` helper; however,
        # we keep around the `build_name` variable instead of directly using `self.name`, just in case we need
        # to re-enable it...
        build_name = self.name
        logger.info("Building EXE from %s", self.tocbasename)
        if os.path.exists(self.name):
            if os.path.isdir(self.name):
                _rmtree(self.name)  # will prompt for confirmation if --noconfirm is not given
                os.remove(self.name)
        if not os.path.exists(os.path.dirname(self.name)):
            os.makedirs(os.path.dirname(self.name))
        bootloader_exe = self.exefiles[0][1]  # pathname of bootloader
        if not os.path.exists(bootloader_exe):
            raise SystemExit(_MISSING_BOOTLOADER_ERRORMSG)
        # Step 1: copy the bootloader file, and perform any operations that need to be done prior to appending the PKG.
        logger.info("Copying bootloader EXE to %s", build_name)
        self._retry_operation(shutil.copyfile, bootloader_exe, build_name)
        self._retry_operation(os.chmod, build_name, 0o755)
            # First, remove all resources from the file. This ensures that no manifest is embedded, even if bootloader
            # was compiled with a toolchain that forcibly embeds a default manifest (e.g., mingw toolchain from msys2).
            self._retry_operation(winresource.remove_all_resources, build_name)
            # Embed icon.
            if self.icon != "NONE":
                logger.info("Copying icon to EXE")
                self._retry_operation(icon.CopyIcons, build_name, self.icon)
            # Embed version info.
                logger.info("Copying version information to EXE")
                self._retry_operation(versioninfo.write_version_info_to_executable, build_name, self.versrsrc)
            # Embed/copy other resources.
            logger.info("Copying %d resources to EXE", len(self.resources))
            for resource in self.resources:
                self._retry_operation(self._copy_windows_resource, build_name, resource)
            # Embed the manifest into the executable.
            logger.info("Embedding manifest in EXE")
            self._retry_operation(winmanifest.write_manifest_to_executable, build_name, self.manifest)
        elif is_darwin:
            # Convert bootloader to the target arch
            logger.info("Converting EXE to target arch (%s)", self.target_arch)
            osxutils.binary_to_target_arch(build_name, self.target_arch, display_name='Bootloader EXE')
        # Step 2: append the PKG, if necessary
        if self.append_pkg:
            append_file = self.pkg.name  # Append PKG
            append_type = 'PKG archive'  # For debug messages
            # In onefile mode, copy the stand-alone PKG next to the executable. In onedir, this will be done by the
            # COLLECT() target.
            if not self.exclude_binaries:
                pkg_dst = os.path.join(os.path.dirname(build_name), os.path.basename(self.pkgname))
                logger.info("Copying stand-alone PKG archive from %s to %s", self.pkg.name, pkg_dst)
                shutil.copyfile(self.pkg.name, pkg_dst)
                logger.info("Stand-alone PKG archive will be handled by COLLECT")
            # The bootloader requires package side-loading to be explicitly enabled, which is done by embedding custom
            # signature to the executable. This extra signature ensures that the sideload-enabled executable is at least
            # slightly different from the stock bootloader executables, which should prevent antivirus programs from
            # flagging our stock bootloaders due to sideload-enabled applications in the wild.
            # Write to temporary file
            pkgsig_file = self.pkg.name + '.sig'
            with open(pkgsig_file, "wb") as f:
                # 8-byte MAGIC; slightly changed PKG MAGIC pattern
                f.write(b'MEI\015\013\012\013\016')
            append_file = pkgsig_file  # Append PKG-SIG
            append_type = 'PKG sideload signature'  # For debug messages
        if is_linux:
            # Linux: append data into custom ELF section using objcopy.
            logger.info("Appending %s to custom ELF section in EXE", append_type)
            cmd = ['objcopy', '--add-section', f'pydata={append_file}', build_name]
            p = subprocess.run(cmd, stderr=subprocess.STDOUT, stdout=subprocess.PIPE, encoding='utf-8')
            if p.returncode:
                raise SystemError(f"objcopy Failure: {p.returncode} {p.stdout}")
            # macOS: remove signature, append data, and fix-up headers so that the appended data appears to be part of
            # the executable (which is required by strict validation during code-signing).
            # Strip signatures from all arch slices. Strictly speaking, we need to remove signature (if present) from
            # the last slice, because we will be appending data to it. When building universal2 bootloaders natively on
            # macOS, only arm64 slices have a (dummy) signature. However, when cross-compiling with osxcross, we seem to
            # get dummy signatures on both x86_64 and arm64 slices. While the former should not have any impact, it does
            # seem to cause issues with further binary signing using real identity. Therefore, we remove all signatures
            # and re-sign the binary using dummy signature once the data is appended.
            logger.info("Removing signature(s) from EXE")
            osxutils.remove_signature_from_binary(build_name)
            # Fix Mach-O image UUID(s) in executable to ensure uniqueness across different builds.
            # NOTE: even if PKG is side-loaded, use the hash of its contents to generate the new UUID.
            # NOTE: this step is performed *before* PKG is appended and sizes are fixed in the executable's headers;
            # this ensures that we are operating only on original header size instead of enlarged one (which could
            # be significantly larger in large onefile builds).
            logger.info("Modifying Mach-O image UUID(s) in EXE")
            osxutils.update_exe_identifier(build_name, self.pkg.name)
            # Append the data
            logger.info("Appending %s to EXE", append_type)
            self._append_data_to_exe(build_name, append_file)
            # Fix Mach-O headers
            logger.info("Fixing EXE headers for code signing")
            osxutils.fix_exe_for_code_signing(build_name)
            # Fall back to just appending data at the end of the file
            self._retry_operation(self._append_data_to_exe, build_name, append_file)
        # Step 3: post-processing
            # Set checksum to appease antiviral software. Also set build timestamp to current time to increase entropy
            # (but honor SOURCE_DATE_EPOCH environment variable for reproducible builds).
            logger.info("Fixing EXE headers")
            build_timestamp = int(os.environ.get('SOURCE_DATE_EPOCH', time.time()))
            self._retry_operation(winutils.set_exe_build_timestamp, build_name, build_timestamp)
            self._retry_operation(winutils.update_exe_pe_checksum, build_name)
            # If the version of macOS SDK used to build bootloader exceeds that of macOS SDK used to built Python
            # library (and, by extension, bundled Tcl/Tk libraries), force the version declared by the frozen executable
            # to match that of the Python library.
            # Having macOS attempt to enable new features (based on SDK version) for frozen application has no benefit
            # if the Python library does not support them as well.
            # On the other hand, there seem to be UI issues in tkinter due to failed or partial enablement of dark mode
            # (i.e., the bootloader executable being built against SDK 10.14 or later, which causes macOS to enable dark
            # mode, and Tk libraries being built against an earlier SDK version that does not support the dark mode).
            # With python.org Intel macOS installers, this manifests as black Tk windows and UI elements (see issue
            # #5827), while in Anaconda python, it may result in white text on bright background.
            pylib_version = osxutils.get_macos_sdk_version(self.python_lib)
            exe_version = osxutils.get_macos_sdk_version(build_name)
            if pylib_version < exe_version:
                    "Rewriting the executable's macOS SDK version (%d.%d.%d) to match the SDK version of the Python "
                    "library (%d.%d.%d) in order to avoid inconsistent behavior and potential UI issues in the "
                    "frozen application.", *exe_version, *pylib_version
                osxutils.set_macos_sdk_version(build_name, *pylib_version)
            # Re-sign the binary (either ad-hoc or using real identity, if provided).
            logger.info("Re-signing the EXE")
            osxutils.sign_binary(build_name, self.codesign_identity, self.entitlements_file)
        # Ensure executable flag is set
        # Get mtime for storing into the guts
        self.mtm = self._retry_operation(miscutils.mtime, build_name)
        if build_name != self.name:
            self._retry_operation(os.rename, build_name, self.name)
        logger.info("Building EXE from %s completed successfully.", self.tocbasename)
    def _copy_windows_resource(self, build_name, resource_spec):
        import pefile
        # Helper for optionally converting integer strings to values; resource types and IDs/names can be specified as
        # either numeric values or custom strings...
        def _to_int(value):
                return int(value)
        logger.debug("Processing resource: %r", resource_spec)
        resource = resource_spec.split(",")  # filename,[type],[name],[language]
        if len(resource) < 1 or len(resource) > 4:
                f"Invalid Windows resource specifier {resource_spec!r}! "
                f"Must be in format 'filename,[type],[name],[language]'!"
        # Anchor resource file to spec file location, if necessary.
        src_filename = self._makeabs(resource[0])
        # Ensure file exists.
        if not os.path.isfile(src_filename):
            raise ValueError(f"Resource file {src_filename!r} does not exist!")
        # Check if src_filename points to a PE file or an arbitrary (data) file.
            with pefile.PE(src_filename, fast_load=True):
                is_pe_file = True
            is_pe_file = False
        if is_pe_file:
            # If resource file is PE file, copy all resources from it, subject to specified type, name, and language.
            logger.debug("Resource file %r is a PE file...", src_filename)
            # Resource type, name, and language serve as filters. If not specified, use "*".
            resource_type = _to_int(resource[1]) if len(resource) >= 2 else "*"
            resource_name = _to_int(resource[2]) if len(resource) >= 3 else "*"
            resource_lang = _to_int(resource[3]) if len(resource) >= 4 else "*"
                winresource.copy_resources_from_pe_file(
                    build_name,
                    src_filename,
                    [resource_type],
                    [resource_name],
                    [resource_lang],
                raise IOError(f"Failed to copy resources from PE file {src_filename!r}") from e
            logger.debug("Resource file %r is an arbitrary data file...", src_filename)
            # For arbitrary data file, resource type and name need to be provided.
            if len(resource) < 3:
                    f"For arbitrary data file, the format is 'filename,type,name,[language]'!"
            resource_type = _to_int(resource[1])
            resource_name = _to_int(resource[2])
            resource_lang = _to_int(resource[3]) if len(resource) >= 4 else 0  # LANG_NEUTRAL
            # Prohibit wildcards for resource type and name.
            if resource_type == "*":
                    f"For arbitrary data file, resource type cannot be a wildcard (*)!"
            if resource_name == "*":
                    f"For arbitrary data file, resource ma,e cannot be a wildcard (*)!"
                with open(src_filename, 'rb') as fp:
                    data = fp.read()
                winresource.add_or_update_resource(
                    resource_type,
                raise IOError(f"Failed to embed data file {src_filename!r} as Windows resource") from e
    def _append_data_to_exe(self, build_name, append_file):
        with open(build_name, 'ab') as outf:
            with open(append_file, 'rb') as inf:
                shutil.copyfileobj(inf, outf, length=64 * 1024)
    def _retry_operation(func, *args, max_attempts=20):
        Attempt to execute the given function `max_attempts` number of times while catching exceptions that are usually
        associated with Windows anti-virus programs temporarily locking the access to the executable.
        def _is_allowed_exception(e):
            Helper to determine whether the given exception is eligible for retry or not.
            if isinstance(e, PermissionError):
                # Always retry on all instances of PermissionError
            elif is_win:
                from PyInstaller.compat import pywintypes
                # Windows-specific errno and winerror codes.
                # https://learn.microsoft.com/en-us/cpp/c-runtime-library/errno-constants
                _ALLOWED_ERRNO = {
                    13,  # EACCES (would typically be a PermissionError instead)
                    22,  # EINVAL (reported to be caused by Crowdstrike; see #7840)
                # https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
                _ALLOWED_WINERROR = {
                    5,  # ERROR_ACCESS_DENIED (reported in #7825)
                    32,  # ERROR_SHARING_VIOLATION (exclusive lock via `CreateFileW` flags, or via `_locked`).
                    110,  # ERROR_OPEN_FAILED (reported in #8138)
                if isinstance(e, OSError):
                    # For OSError exceptions other than PermissionError, validate errno.
                    if e.errno in _ALLOWED_ERRNO:
                    # OSError typically translates `winerror` into `errno` equivalent; but try to match the original
                    # values as a fall back, just in case. `OSError.winerror` attribute exists only on Windows.
                    if e.winerror in _ALLOWED_WINERROR:
                elif isinstance(e, pywintypes.error):
                    # pywintypes.error is raised by helper functions that use win32 C API bound via pywin32-ctypes.
        func_name = func.__name__
        for attempt in range(max_attempts):
                return func(*args)
                # Check if exception is eligible for retry; if not, also check its immediate cause (in case the
                # exception was thrown from an eligible exception).
                if not _is_allowed_exception(e) and not _is_allowed_exception(e.__context__):
                # Retry after sleep (unless this was our last attempt)
                if attempt < max_attempts - 1:
                    sleep_duration = 1 / (max_attempts - 1 - attempt)
                        f"Execution of {func_name!r} failed on attempt #{attempt + 1} / {max_attempts}: {e!r}. "
                        f"Retrying in {sleep_duration:.2f} second(s)..."
                    time.sleep(sleep_duration)
                        f"Execution of {func_name!r} failed on attempt #{attempt + 1} / {max_attempts}: {e!r}."
                    raise RuntimeError(f"Execution of {func_name!r} failed - no more attempts left!") from e
class COLLECT(Target):
    In one-dir mode creates the output folder with all necessary files.
                The name of the directory to be built.
        self.strip_binaries = kwargs.get('strip', False)
        self.console = True
        self.target_arch = None
        self.codesign_identity = None
        self.entitlements_file = None
        # UPX needs to be both available and enabled for the taget.
        self.upx_binaries = CONF['upx_available'] and kwargs.get('upx', False)
        # The `name` should be the output directory name, without the parent path (the directory is created in the
        # DISTPATH). Old .spec formats included parent path, so strip it away.
        self.name = os.path.join(CONF['distpath'], os.path.basename(kwargs.get('name')))
            if isinstance(arg, EXE):
                self.contents_directory = arg.contents_directory
            raise ValueError("No EXE() instance was passed to COLLECT()")
            # Valid arguments: EXE object and TOC-like iterables
                # Add EXE as an entry to the TOC, and merge its dependencies TOC
                self.toc.append((os.path.basename(arg.name), arg.name, 'EXECUTABLE'))
                # Inherit settings
                self.console = arg.console
                self.target_arch = arg.target_arch
                self.codesign_identity = arg.codesign_identity
                self.entitlements_file = arg.entitlements_file
                # Search for the executable's external manifest, and collect it if available
                for dest_name, src_name, typecode in arg.toc:
                    if dest_name == os.path.basename(arg.name) + ".manifest":
                        self.toc.append((dest_name, src_name, typecode))
                # If PKG is not appended to the executable, we need to collect it.
                if not arg.append_pkg:
                    self.toc.append((os.path.basename(arg.pkgname), arg.pkgname, 'PKG'))
                raise TypeError(f"Invalid argument type for COLLECT: {type(arg)!r}")
        # Alphabetically sort the TOC to ensure that order of processing is predictable and reproducible.
        # COLLECT always builds, we just want the TOC to be written out.
        ('toc', None),
        # COLLECT always needs to be executed, in order to clean the output directory.
        _make_clean_directory(self.name)
        logger.info("Building COLLECT %s", self.tocbasename)
            # Ensure that the source file exists, if necessary. Skip the check for DEPENDENCY entries due to special
            # contents of 'dest_name' and/or 'src_name'. Same for the SYMLINK entries, where 'src_name' is relative
            # target name for symbolic link.
            if typecode not in {'DEPENDENCY', 'SYMLINK'} and not os.path.exists(src_name):
                # If file is contained within python egg, it will be added with the egg.
            # Disallow collection outside of the dist directory.
            if os.pardir in os.path.normpath(dest_name).split(os.sep) or os.path.isabs(dest_name):
                    'ERROR: attempting to store file outside of the dist directory: %r. Aborting.' % dest_name
            # Create parent directory structure, if necessary
            if typecode in ("EXECUTABLE", "PKG"):
                dest_path = os.path.join(self.name, dest_name)
                dest_path = os.path.join(self.name, self.contents_directory or "", dest_name)
            dest_dir = os.path.dirname(dest_path)
                os.makedirs(dest_dir, exist_ok=True)
            except FileExistsError:
                    f"ERROR: Pyinstaller needs to create a directory at {dest_dir!r}, "
                    "but there already exists a file at that path!"
            if typecode in ('EXTENSION', 'BINARY'):
            if typecode == 'SYMLINK':
                # On Windows, ensure that symlink target path (stored in src_name) is using Windows-style back slash
                # separators.
                if is_win and os.path.sep == '/':
                    src_name = src_name.replace(os.path.sep, '\\')
                os.symlink(src_name, dest_path)  # Create link at dest_path, pointing at (relative) src_name
            elif typecode != 'DEPENDENCY':
                # At this point, `src_name` should be a valid file.
                if not os.path.isfile(src_name):
                    raise ValueError(f"Resource {src_name!r} is not a valid file!")
                # If strict collection mode is enabled, the destination should not exist yet.
                if strict_collect_mode and os.path.exists(dest_path):
                        f"Attempting to collect a duplicated file into COLLECT: {dest_name} (type: {typecode})"
                # Use `shutil.copyfile` to copy file with default permissions. We do not attempt to preserve original
                # permissions nor metadata, as they might be too restrictive and cause issues either during subsequent
                # re-build attempts or when trying to move the application bundle. For binaries (and data files with
                # executable bit set), we manually set the executable bits after copying the file.
                shutil.copyfile(src_name, dest_path)
                typecode in ('EXTENSION', 'BINARY', 'EXECUTABLE')
                or (typecode == 'DATA' and os.access(src_name, os.X_OK))
                os.chmod(dest_path, 0o755)
        logger.info("Building COLLECT %s completed successfully.", self.tocbasename)
class MERGE:
    Given Analysis objects for multiple executables, replace occurrences of data and binary files with references to the
    first executable in which they occur. The actual data and binary files are then collected only once, thereby
    reducing the disk space used by multiple executables. Every executable (even onedir ones!) obtained from a
    MERGE-processed Analysis gains onefile semantics, because it needs to extract its referenced dependencies from other
    executables into temporary directory before they can run.
            Dependencies as a list of (analysis, identifier, path_to_exe) tuples. `analysis` is an instance of
            `Analysis`, `identifier` is the basename of the entry-point script (without .py suffix), and `path_to_exe`
            is path to the corresponding executable, relative to the `dist` directory (without .exe suffix in the
            filename component). For onefile executables, `path_to_exe` is usually just executable's base name
            (e.g., `myexecutable`). For onedir executables, `path_to_exe` usually comprises both the application's
            directory name and executable name (e.g., `myapp/myexecutable`).
        self._dependencies = {}
        self._symlinks = set()
        # Process all given (analysis, identifier, path_to_exe) tuples
        for analysis, identifier, path_to_exe in args:
            # Process analysis.binaries and analysis.datas TOCs. self._process_toc() call returns two TOCs; the first
            # contains entries that remain within this analysis, while the second contains entries that reference
            # an entry in another executable.
            binaries, binaries_refs = self._process_toc(analysis.binaries, path_to_exe)
            datas, datas_refs = self._process_toc(analysis.datas, path_to_exe)
            # Update `analysis.binaries`, `analysis.datas`, and `analysis.dependencies`.
            # The entries that are found in preceding executable(s) are removed from `binaries` and `datas`, and their
            # DEPENDENCY entry counterparts are added to `dependencies`. We cannot simply update the entries in
            # `binaries` and `datas`, because at least in theory, we need to support both onefile and onedir mode. And
            # while in onefile, `a.datas`, `a.binaries`, and `a.dependencies` are passed to `EXE` (and its `PKG`), with
            # onedir, `a.datas` and `a.binaries` need to be passed to `COLLECT` (as they were before the MERGE), while
            # `a.dependencies` needs to be passed to `EXE`. This split requires DEPENDENCY entries to be in a separate
            # TOC.
            analysis.binaries = normalize_toc(binaries)
            analysis.datas = normalize_toc(datas)
            analysis.dependencies += binaries_refs + datas_refs
    def _process_toc(self, toc, path_to_exe):
        # NOTE: unfortunately, these need to keep two separate lists. See the comment in the calling code on why this
        # is so.
        toc_keep = []
        toc_refs = []
            dest_name, src_name, typecode = entry
            # Special handling and bookkeeping for symbolic links. We need to account both for dest_name and src_name,
            # because src_name might be the same in different contexts. For example, when collecting Qt .framework
            # bundles on macOS, there are multiple relative symbolic links `Current -> A` (one in each .framework).
                key = dest_name, src_name
                if key not in self._symlinks:
                    # First occurrence; keep the entry in "for-keep" TOC, same as we would for binaries and datas.
                    logger.debug("Keeping symbolic link %r entry in original TOC.", entry)
                    self._symlinks.add(key)
                    toc_keep.append(entry)
                    # Subsequent occurrence; keep the SYMLINK entry intact, but add it to the references TOC instead of
                    # "for-keep" TOC, so it ends up in `a.dependencies`.
                    logger.debug("Moving symbolic link %r entry to references TOC.", entry)
                    toc_refs.append(entry)
                del key  # Block-local variable
            # In fact, we need to accout for both dest_name and src_name with regular entries as well; previous
            # approach that considered only src_name ended tripped up when same file was collected in different
            # locations (i.e., same src_name but different dest_names).
            if key not in self._dependencies:
                logger.debug("Adding dependency %r located in %s", key, path_to_exe)
                self._dependencies[key] = path_to_exe
                # Add entry to list of kept TOC entries
                # Construct relative dependency path; i.e., the relative path from this executable (or rather, its
                # parent directory) to the executable that contains the dependency.
                dep_path = os.path.relpath(self._dependencies[key], os.path.dirname(path_to_exe))
                # Ignore references that point to the origin package. This can happen if the same resource is listed
                # multiple times in TOCs (e.g., once as binary and once as data).
                if dep_path.endswith(path_to_exe):
                        "Ignoring self-reference of %r for %s, located in %s - duplicated TOC entry?", key, path_to_exe,
                        dep_path
                    # The entry is a duplicate, and should be ignored (i.e., do not add it to either of output TOCs).
                logger.debug("Referencing %r to be a dependency for %s, located in %s", key, path_to_exe, dep_path)
                # Create new DEPENDENCY entry; under destination path (first element), we store the original destination
                # path, while source path contains the relative reference path.
                toc_refs.append((dest_name, dep_path, "DEPENDENCY"))
        return toc_keep, toc_refs
UNCOMPRESSED = False
COMPRESSED = True
_MISSING_BOOTLOADER_ERRORMSG = """Fatal error: PyInstaller does not include a pre-compiled bootloader for your
platform. For more details and instructions how to build the bootloader see
<https://pyinstaller.readthedocs.io/en/stable/bootloader-building.html>"""
""" pkg.api """
if sys.version_info[0] == 2:
    from .api2 import *
    from .api3 import *
from django.contrib.messages import constants
from django.contrib.messages.storage import default_storage
    "add_message",
    "get_messages",
    "get_level",
    "set_level",
    "debug",
    "info",
    "success",
    "warning",
    "error",
    "MessageFailure",
class MessageFailure(Exception):
def add_message(request, level, message, extra_tags="", fail_silently=False):
    Attempt to add a message to the request using the 'messages' app.
        messages = request._messages
        if not hasattr(request, "META"):
                "add_message() argument must be an HttpRequest object, not "
                "'%s'." % request.__class__.__name__
        if not fail_silently:
            raise MessageFailure(
                "You cannot add messages without installing "
                "django.contrib.messages.middleware.MessageMiddleware"
        return messages.add(level, message, extra_tags)
def get_messages(request):
    Return the message storage on the request if it exists, otherwise return
    an empty list.
    return getattr(request, "_messages", [])
def get_level(request):
    Return the minimum level of messages to be recorded.
    The default level is the ``MESSAGE_LEVEL`` setting. If this is not found,
    use the ``INFO`` level.
    storage = getattr(request, "_messages", default_storage(request))
    return storage.level
def set_level(request, level):
    Set the minimum level of messages to be recorded, and return ``True`` if
    the level was recorded successfully.
    If set to ``None``, use the default level (see the get_level() function).
    if not hasattr(request, "_messages"):
    request._messages.level = level
def debug(request, message, extra_tags="", fail_silently=False):
    """Add a message with the ``DEBUG`` level."""
    add_message(
        constants.DEBUG,
        extra_tags=extra_tags,
        fail_silently=fail_silently,
def info(request, message, extra_tags="", fail_silently=False):
    """Add a message with the ``INFO`` level."""
        constants.INFO,
def success(request, message, extra_tags="", fail_silently=False):
    """Add a message with the ``SUCCESS`` level."""
        constants.SUCCESS,
def warning(request, message, extra_tags="", fail_silently=False):
    """Add a message with the ``WARNING`` level."""
        constants.WARNING,
def error(request, message, extra_tags="", fail_silently=False):
    """Add a message with the ``ERROR`` level."""
        constants.ERROR,
# your API code
"""Module contains logic for indexing documents into vector stores."""
from langchain_core.document_loaders.base import BaseLoader
from langchain_core.exceptions import LangChainException
from langchain_core.indexing.base import DocumentIndex, RecordManager
from langchain_core.vectorstores import VectorStore
# Magic UUID to use as a namespace for hashing.
# Used to try and generate a unique UUID for each document
# from hashing the document content and metadata.
NAMESPACE_UUID = uuid.UUID(int=1984)
def _hash_string_to_uuid(input_string: str) -> str:
    """Hashes a string and returns the corresponding UUID."""
    hash_value = hashlib.sha1(
        input_string.encode("utf-8"), usedforsecurity=False
    ).hexdigest()
    return str(uuid.uuid5(NAMESPACE_UUID, hash_value))
_WARNED_ABOUT_SHA1: bool = False
def _warn_about_sha1() -> None:
    """Emit a one-time warning about SHA-1 collision weaknesses."""
    # Global variable OK in this case
    global _WARNED_ABOUT_SHA1  # noqa: PLW0603
    if not _WARNED_ABOUT_SHA1:
            "Using SHA-1 for document hashing. SHA-1 is *not* "
            "collision-resistant; a motivated attacker can construct distinct inputs "
            "that map to the same fingerprint. If this matters in your "
            "threat model, switch to a stronger algorithm such "
            "as 'blake2b', 'sha256', or 'sha512' by specifying "
            " `key_encoder` parameter in the `index` or `aindex` function. ",
            category=UserWarning,
        _WARNED_ABOUT_SHA1 = True
def _hash_string(
    input_string: str, *, algorithm: Literal["sha1", "sha256", "sha512", "blake2b"]
) -> uuid.UUID:
    """Hash *input_string* to a deterministic UUID using the configured algorithm."""
    if algorithm == "sha1":
        _warn_about_sha1()
    hash_value = _calculate_hash(input_string, algorithm)
    return uuid.uuid5(NAMESPACE_UUID, hash_value)
def _hash_nested_dict(
    data: dict[Any, Any], *, algorithm: Literal["sha1", "sha256", "sha512", "blake2b"]
    """Hash a nested dictionary to a UUID using the configured algorithm."""
    serialized_data = json.dumps(data, sort_keys=True)
    return _hash_string(serialized_data, algorithm=algorithm)
def _batch(size: int, iterable: Iterable[T]) -> Iterator[list[T]]:
    """Utility batching function."""
    it = iter(iterable)
        chunk = list(islice(it, size))
        if not chunk:
async def _abatch(size: int, iterable: AsyncIterable[T]) -> AsyncIterator[list[T]]:
    batch: list[T] = []
    async for element in iterable:
        if len(batch) < size:
            batch.append(element)
        if len(batch) >= size:
            yield batch
            batch = []
    if batch:
def _get_source_id_assigner(
    source_id_key: str | Callable[[Document], str] | None,
) -> Callable[[Document], str | None]:
    """Get the source id from the document."""
    if source_id_key is None:
        return lambda _doc: None
    if isinstance(source_id_key, str):
        return lambda doc: doc.metadata[source_id_key]
    if callable(source_id_key):
        return source_id_key
        f"source_id_key should be either None, a string or a callable. "
        f"Got {source_id_key} of type {type(source_id_key)}."
def _deduplicate_in_order(
    hashed_documents: Iterable[Document],
) -> Iterator[Document]:
    """Deduplicate a list of hashed documents while preserving order."""
    seen: set[str] = set()
    for hashed_doc in hashed_documents:
        if hashed_doc.id not in seen:
            # At this stage, the id is guaranteed to be a string.
            # Avoiding unnecessary run time checks.
            seen.add(cast("str", hashed_doc.id))
            yield hashed_doc
class IndexingException(LangChainException):
    """Raised when an indexing operation fails."""
def _calculate_hash(
    text: str, algorithm: Literal["sha1", "sha256", "sha512", "blake2b"]
    """Return a hexadecimal digest of *text* using *algorithm*."""
        # Calculate the SHA-1 hash and return it as a UUID.
        digest = hashlib.sha1(text.encode("utf-8"), usedforsecurity=False).hexdigest()
        return str(uuid.uuid5(NAMESPACE_UUID, digest))
    if algorithm == "blake2b":
        return hashlib.blake2b(text.encode("utf-8")).hexdigest()
    if algorithm == "sha256":
        return hashlib.sha256(text.encode("utf-8")).hexdigest()
    if algorithm == "sha512":
        return hashlib.sha512(text.encode("utf-8")).hexdigest()
    msg = f"Unsupported hashing algorithm: {algorithm}"
def _get_document_with_hash(
    document: Document,
    key_encoder: Callable[[Document], str]
    | Literal["sha1", "sha256", "sha512", "blake2b"],
) -> Document:
    """Calculate a hash of the document, and assign it to the uid.
    When using one of the predefined hashing algorithms, the hash is calculated
    by hashing the content and the metadata of the document.
        document: Document to hash.
        key_encoder: Hashing algorithm to use for hashing the document.
            If not provided, a default encoder using SHA-1 will be used.
            SHA-1 is not collision-resistant, and a motivated attacker
            could craft two different texts that hash to the
            same cache key.
            New applications should use one of the alternative encoders
            or provide a custom and strong key encoder function to avoid this risk.
            When changing the key encoder, you must change the
            index as well to avoid duplicated documents in the cache.
        ValueError: If the metadata cannot be serialized using json.
        Document with a unique identifier based on the hash of the content and metadata.
    metadata: dict[str, Any] = dict(document.metadata or {})
    if callable(key_encoder):
        # If key_encoder is a callable, we use it to generate the hash.
        hash_ = key_encoder(document)
        # The hashes are calculated separate for the content and the metadata.
        content_hash = _calculate_hash(document.page_content, algorithm=key_encoder)
            serialized_meta = json.dumps(metadata, sort_keys=True)
                f"Failed to hash metadata: {e}. "
                f"Please use a dict that can be serialized using json."
            raise ValueError(msg) from e
        metadata_hash = _calculate_hash(serialized_meta, algorithm=key_encoder)
        hash_ = _calculate_hash(content_hash + metadata_hash, algorithm=key_encoder)
    return Document(
        # Assign a unique identifier based on the hash.
        id=hash_,
        page_content=document.page_content,
        metadata=document.metadata,
# This internal abstraction was imported by the langchain package internally, so
# we keep it here for backwards compatibility.
class _HashedDocument:
    def __init__(self, *args: Any, **kwargs: Any) -> None:
        """Raise an error if this class is instantiated."""
            "_HashedDocument is an internal abstraction that was deprecated in "
            " langchain-core 0.3.63. This abstraction is marked as private and "
            " should not have been used directly. If you are seeing this error, please "
            " update your code appropriately."
def _delete(
    vector_store: VectorStore | DocumentIndex,
    ids: list[str],
    """Delete documents from a vector store or document index by their IDs.
        vector_store: The vector store or document index to delete from.
        ids: List of document IDs to delete.
        IndexingException: If the delete operation fails.
        TypeError: If the `vector_store` is neither a `VectorStore` nor a
            `DocumentIndex`.
    if isinstance(vector_store, VectorStore):
        delete_ok = vector_store.delete(ids)
        if delete_ok is not None and delete_ok is False:
            msg = "The delete operation to VectorStore failed."
            raise IndexingException(msg)
    elif isinstance(vector_store, DocumentIndex):
        delete_response = vector_store.delete(ids)
        if "num_failed" in delete_response and delete_response["num_failed"] > 0:
            msg = "The delete operation to DocumentIndex failed."
            f"Vectorstore should be either a VectorStore or a DocumentIndex. "
            f"Got {type(vector_store)}."
# PUBLIC API
class IndexingResult(TypedDict):
    """Return a detailed a breakdown of the result of the indexing operation."""
    num_added: int
    """Number of added documents."""
    num_updated: int
    """Number of updated documents because they were not up to date."""
    num_deleted: int
    """Number of deleted documents."""
    num_skipped: int
    """Number of skipped documents because they were already up to date."""
def index(
    docs_source: BaseLoader | Iterable[Document],
    record_manager: RecordManager,
    batch_size: int = 100,
    cleanup: Literal["incremental", "full", "scoped_full"] | None = None,
    source_id_key: str | Callable[[Document], str] | None = None,
    cleanup_batch_size: int = 1_000,
    force_update: bool = False,
    key_encoder: Literal["sha1", "sha256", "sha512", "blake2b"]
    | Callable[[Document], str] = "sha1",
    upsert_kwargs: dict[str, Any] | None = None,
) -> IndexingResult:
    """Index data from the loader into the vector store.
    Indexing functionality uses a manager to keep track of which documents
    are in the vector store.
    This allows us to keep track of which documents were updated, and which
    documents were deleted, which documents should be skipped.
    For the time being, documents are indexed using their hashes, and users
    are not able to specify the uid of the document.
    !!! warning "Behavior changed in `langchain-core` 0.3.25"
        Added `scoped_full` cleanup mode.
        * In full mode, the loader should be returning
            the entire dataset, and not just a subset of the dataset.
            Otherwise, the auto_cleanup will remove documents that it is not
            supposed to.
        * In incremental mode, if documents associated with a particular
            source id appear across different batches, the indexing API
            will do some redundant work. This will still result in the
            correct end state of the index, but will unfortunately not be
            100% efficient. For example, if a given document is split into 15
            chunks, and we index them using a batch size of 5, we'll have 3 batches
            all with the same source id. In general, to avoid doing too much
            redundant work select as big a batch size as possible.
        * The `scoped_full` mode is suitable if determining an appropriate batch size
            is challenging or if your data loader cannot return the entire dataset at
            once. This mode keeps track of source IDs in memory, which should be fine
            for most use cases. If your dataset is large (10M+ docs), you will likely
            need to parallelize the indexing process regardless.
        docs_source: Data loader or iterable of documents to index.
        record_manager: Timestamped set to keep track of which documents were
            updated.
        vector_store: `VectorStore` or DocumentIndex to index the documents into.
        batch_size: Batch size to use when indexing.
        cleanup: How to handle clean up of documents.
            - incremental: Cleans up all documents that haven't been updated AND
                that are associated with source IDs that were seen during indexing.
                Clean up is done continuously during indexing helping to minimize the
                probability of users seeing duplicated content.
            - full: Delete all documents that have not been returned by the loader
                during this run of indexing.
                Clean up runs after all documents have been indexed.
                This means that users may see duplicated content during indexing.
            - scoped_full: Similar to Full, but only deletes all documents
                that haven't been updated AND that are associated with
                source IDs that were seen during indexing.
            - None: Do not delete any documents.
        source_id_key: Optional key that helps identify the original source
            of the document.
        cleanup_batch_size: Batch size to use when cleaning up documents.
        force_update: Force update documents even if they are present in the
            record manager. Useful if you are re-indexing with updated embeddings.
        key_encoder: Hashing algorithm to use for hashing the document content and
            metadata. Options include "blake2b", "sha256", and "sha512".
            !!! version-added "Added in `langchain-core` 0.3.66"
        upsert_kwargs: Additional keyword arguments to pass to the add_documents
            method of the `VectorStore` or the upsert method of the DocumentIndex.
            For example, you can use this to specify a custom vector_field:
            upsert_kwargs={"vector_field": "embedding"}
            !!! version-added "Added in `langchain-core` 0.3.10"
        Indexing result which contains information about how many documents
        were added, updated, deleted, or skipped.
        ValueError: If cleanup mode is not one of 'incremental', 'full' or None
        ValueError: If cleanup mode is incremental and source_id_key is None.
        ValueError: If `VectorStore` does not have
            "delete" and "add_documents" required methods.
        ValueError: If source_id_key is not None, but is not a string or callable.
        TypeError: If `vectorstore` is not a `VectorStore` or a DocumentIndex.
        AssertionError: If `source_id` is None when cleanup mode is incremental.
            (should be unreachable code).
    # Behavior is deprecated, but we keep it for backwards compatibility.
    # # Warn only once per process.
    if key_encoder == "sha1":
    if cleanup not in {"incremental", "full", "scoped_full", None}:
            f"cleanup should be one of 'incremental', 'full', 'scoped_full' or None. "
            f"Got {cleanup}."
    if (cleanup in {"incremental", "scoped_full"}) and source_id_key is None:
            "Source id key is required when cleanup mode is incremental or scoped_full."
    destination = vector_store  # Renaming internally for clarity
    # If it's a vectorstore, let's check if it has the required methods.
    if isinstance(destination, VectorStore):
        # Check that the Vectorstore has required methods implemented
        methods = ["delete", "add_documents"]
        for method in methods:
            if not hasattr(destination, method):
                    f"Vectorstore {destination} does not have required method {method}"
        if type(destination).delete == VectorStore.delete:
            # Checking if the VectorStore has overridden the default delete method
            # implementation which just raises a NotImplementedError
            msg = "Vectorstore has not implemented the delete method"
    elif isinstance(destination, DocumentIndex):
            f"Got {type(destination)}."
    if isinstance(docs_source, BaseLoader):
            doc_iterator = docs_source.lazy_load()
            doc_iterator = iter(docs_source.load())
        doc_iterator = iter(docs_source)
    source_id_assigner = _get_source_id_assigner(source_id_key)
    # Mark when the update started.
    index_start_dt = record_manager.get_time()
    num_added = 0
    num_skipped = 0
    num_updated = 0
    num_deleted = 0
    scoped_full_cleanup_source_ids: set[str] = set()
    for doc_batch in _batch(batch_size, doc_iterator):
        # Track original batch size before deduplication
        original_batch_size = len(doc_batch)
        hashed_docs = list(
            _deduplicate_in_order(
                    _get_document_with_hash(doc, key_encoder=key_encoder)
                    for doc in doc_batch
        # Count documents removed by within-batch deduplication
        num_skipped += original_batch_size - len(hashed_docs)
        source_ids: Sequence[str | None] = [
            source_id_assigner(hashed_doc) for hashed_doc in hashed_docs
        if cleanup in {"incremental", "scoped_full"}:
            # Source IDs are required.
            for source_id, hashed_doc in zip(source_ids, hashed_docs, strict=False):
                if source_id is None:
                        f"Source IDs are required when cleanup mode is "
                        f"incremental or scoped_full. "
                        f"Document that starts with "
                        f"content: {hashed_doc.page_content[:100]} "
                        f"was not assigned as source id."
                if cleanup == "scoped_full":
                    scoped_full_cleanup_source_ids.add(source_id)
            # Source IDs cannot be None after for loop above.
            source_ids = cast("Sequence[str]", source_ids)
        exists_batch = record_manager.exists(
            cast("Sequence[str]", [doc.id for doc in hashed_docs])
        # Filter out documents that already exist in the record store.
        uids = []
        docs_to_index = []
        uids_to_refresh = []
        seen_docs: set[str] = set()
        for hashed_doc, doc_exists in zip(hashed_docs, exists_batch, strict=False):
            hashed_id = cast("str", hashed_doc.id)
            if doc_exists:
                if force_update:
                    seen_docs.add(hashed_id)
                    uids_to_refresh.append(hashed_id)
            uids.append(hashed_id)
            docs_to_index.append(hashed_doc)
        # Update refresh timestamp
        if uids_to_refresh:
            record_manager.update(uids_to_refresh, time_at_least=index_start_dt)
            num_skipped += len(uids_to_refresh)
        # Be pessimistic and assume that all vector store write will fail.
        # First write to vector store
        if docs_to_index:
                destination.add_documents(
                    docs_to_index,
                    ids=uids,
                    **(upsert_kwargs or {}),
                destination.upsert(
            num_added += len(docs_to_index) - len(seen_docs)
            num_updated += len(seen_docs)
        # And only then update the record store.
        # Update ALL records, even if they already exist since we want to refresh
        # their timestamp.
        record_manager.update(
            cast("Sequence[str]", [doc.id for doc in hashed_docs]),
            group_ids=source_ids,
            time_at_least=index_start_dt,
        # If source IDs are provided, we can do the deletion incrementally!
        if cleanup == "incremental":
            # Get the uids of the documents that were not returned by the loader.
            # mypy isn't good enough to determine that source IDs cannot be None
            # here due to a check that's happening above, so we check again.
            for source_id in source_ids:
                        "source_id cannot be None at this point. "
                        "Reached unreachable code."
                    raise AssertionError(msg)
            source_ids_ = cast("Sequence[str]", source_ids)
            while uids_to_delete := record_manager.list_keys(
                group_ids=source_ids_, before=index_start_dt, limit=cleanup_batch_size
                # Then delete from vector store.
                _delete(destination, uids_to_delete)
                # First delete from record store.
                record_manager.delete_keys(uids_to_delete)
                num_deleted += len(uids_to_delete)
    if cleanup == "full" or (
        cleanup == "scoped_full" and scoped_full_cleanup_source_ids
        delete_group_ids: Sequence[str] | None = None
            delete_group_ids = list(scoped_full_cleanup_source_ids)
            group_ids=delete_group_ids, before=index_start_dt, limit=cleanup_batch_size
            # Then delete from record manager.
        "num_added": num_added,
        "num_updated": num_updated,
        "num_skipped": num_skipped,
        "num_deleted": num_deleted,
# Define an asynchronous generator function
async def _to_async_iterator(iterator: Iterable[T]) -> AsyncIterator[T]:
    """Convert an iterable to an async iterator."""
    for item in iterator:
async def _adelete(
        delete_ok = await vector_store.adelete(ids)
        delete_response = await vector_store.adelete(ids)
async def aindex(
    docs_source: BaseLoader | Iterable[Document] | AsyncIterator[Document],
    """Async index data from the loader into the vector store.
            "adelete" and "aadd_documents" required methods.
        TypeError: If `vector_store` is not a `VectorStore` or DocumentIndex.
        AssertionError: If `source_id_key` is None when cleanup mode is
            incremental or `scoped_full` (should be unreachable).
        methods = ["adelete", "aadd_documents"]
            type(destination).adelete == VectorStore.adelete
            and type(destination).delete == VectorStore.delete
            # Checking if the VectorStore has overridden the default adelete or delete
            # methods implementation which just raises a NotImplementedError
            msg = "Vectorstore has not implemented the adelete or delete method"
    async_doc_iterator: AsyncIterator[Document]
            async_doc_iterator = docs_source.alazy_load()
            # Exception triggered when neither lazy_load nor alazy_load are implemented.
            # * The default implementation of alazy_load uses lazy_load.
            # * The default implementation of lazy_load raises NotImplementedError.
            # In such a case, we use the load method and convert it to an async
            # iterator.
            async_doc_iterator = _to_async_iterator(docs_source.load())
    elif hasattr(docs_source, "__aiter__"):
        async_doc_iterator = docs_source  # type: ignore[assignment]
        async_doc_iterator = _to_async_iterator(docs_source)
    index_start_dt = await record_manager.aget_time()
    async for doc_batch in _abatch(batch_size, async_doc_iterator):
            source_id_assigner(doc) for doc in hashed_docs
            # If the cleanup mode is incremental, source IDs are required.
        exists_batch = await record_manager.aexists(
        uids: list[str] = []
        docs_to_index: list[Document] = []
            # Must be updated to refresh timestamp.
            await record_manager.aupdate(uids_to_refresh, time_at_least=index_start_dt)
                await destination.aadd_documents(
                await destination.aupsert(
        await record_manager.aupdate(
            while uids_to_delete := await record_manager.alist_keys(
                await _adelete(destination, uids_to_delete)
                await record_manager.adelete_keys(uids_to_delete)
from torch import nn
from TTS.utils.audio.numpy_transforms import save_wav
from TTS.utils.manage import ModelManager
from TTS.utils.synthesizer import Synthesizer
from TTS.config import load_config
class TTS(nn.Module):
    """TODO: Add voice conversion and Capacitron support."""
        model_name: str = "",
        model_path: str = None,
        config_path: str = None,
        vocoder_path: str = None,
        vocoder_config_path: str = None,
        progress_bar: bool = True,
        gpu=False,
        """🐸TTS python interface that allows to load and use the released models.
        Example with a multi-speaker model:
            >>> from TTS.api import TTS
            >>> tts = TTS(TTS.list_models()[0])
            >>> wav = tts.tts("This is a test! This is also a test!!", speaker=tts.speakers[0], language=tts.languages[0])
            >>> tts.tts_to_file(text="Hello world!", speaker=tts.speakers[0], language=tts.languages[0], file_path="output.wav")
        Example with a single-speaker model:
            >>> tts = TTS(model_name="tts_models/de/thorsten/tacotron2-DDC", progress_bar=False, gpu=False)
            >>> tts.tts_to_file(text="Ich bin eine Testnachricht.", file_path="output.wav")
        Example loading a model from a path:
            >>> tts = TTS(model_path="/path/to/checkpoint_100000.pth", config_path="/path/to/config.json", progress_bar=False, gpu=False)
        Example voice cloning with YourTTS in English, French and Portuguese:
            >>> tts = TTS(model_name="tts_models/multilingual/multi-dataset/your_tts", progress_bar=False, gpu=True)
            >>> tts.tts_to_file("This is voice cloning.", speaker_wav="my/cloning/audio.wav", language="en", file_path="thisisit.wav")
            >>> tts.tts_to_file("C'est le clonage de la voix.", speaker_wav="my/cloning/audio.wav", language="fr", file_path="thisisit.wav")
            >>> tts.tts_to_file("Isso é clonagem de voz.", speaker_wav="my/cloning/audio.wav", language="pt", file_path="thisisit.wav")
        Example Fairseq TTS models (uses ISO language codes in https://dl.fbaipublicfiles.com/mms/tts/all-tts-languages.html):
            >>> tts = TTS(model_name="tts_models/eng/fairseq/vits", progress_bar=False, gpu=True)
            >>> tts.tts_to_file("This is a test.", file_path="output.wav")
            model_name (str, optional): Model name to load. You can list models by ```tts.models```. Defaults to None.
            model_path (str, optional): Path to the model checkpoint. Defaults to None.
            config_path (str, optional): Path to the model config. Defaults to None.
            vocoder_path (str, optional): Path to the vocoder checkpoint. Defaults to None.
            vocoder_config_path (str, optional): Path to the vocoder config. Defaults to None.
            progress_bar (bool, optional): Whether to pring a progress bar while downloading a model. Defaults to True.
            gpu (bool, optional): Enable/disable GPU. Some models might be too slow on CPU. Defaults to False.
        self.manager = ModelManager(models_file=self.get_models_file_path(), progress_bar=progress_bar, verbose=False)
        self.config = load_config(config_path) if config_path else None
        self.synthesizer = None
        self.voice_converter = None
        self.model_name = ""
        if gpu:
            warnings.warn("`gpu` will be deprecated. Please use `tts.to(device)` instead.")
        if model_name is not None and len(model_name) > 0:
            if "tts_models" in model_name:
                self.load_tts_model_by_name(model_name, gpu)
            elif "voice_conversion_models" in model_name:
                self.load_vc_model_by_name(model_name, gpu)
                self.load_model_by_name(model_name, gpu)
        if model_path:
            self.load_tts_model_by_path(
                model_path, config_path, vocoder_path=vocoder_path, vocoder_config=vocoder_config_path, gpu=gpu
    def models(self):
        return self.manager.list_tts_models()
    def is_multi_speaker(self):
        if hasattr(self.synthesizer.tts_model, "speaker_manager") and self.synthesizer.tts_model.speaker_manager:
            return self.synthesizer.tts_model.speaker_manager.num_speakers > 1
    def is_multi_lingual(self):
        # Not sure what sets this to None, but applied a fix to prevent crashing.
            isinstance(self.model_name, str)
            and "xtts" in self.model_name
            or self.config
            and ("xtts" in self.config.model or len(self.config.languages) > 1)
        if hasattr(self.synthesizer.tts_model, "language_manager") and self.synthesizer.tts_model.language_manager:
            return self.synthesizer.tts_model.language_manager.num_languages > 1
    def speakers(self):
        if not self.is_multi_speaker:
        return self.synthesizer.tts_model.speaker_manager.speaker_names
    def languages(self):
        if not self.is_multi_lingual:
        return self.synthesizer.tts_model.language_manager.language_names
    def get_models_file_path():
        return Path(__file__).parent / ".models.json"
    def list_models(self):
        return ModelManager(models_file=TTS.get_models_file_path(), progress_bar=False, verbose=False)
    def download_model_by_name(self, model_name: str):
        model_path, config_path, model_item = self.manager.download_model(model_name)
        if "fairseq" in model_name or (model_item is not None and isinstance(model_item["model_url"], list)):
            # return model directory if there are multiple files
            # we assume that the model knows how to load itself
            return None, None, None, None, model_path
        if model_item.get("default_vocoder") is None:
            return model_path, config_path, None, None, None
        vocoder_path, vocoder_config_path, _ = self.manager.download_model(model_item["default_vocoder"])
        return model_path, config_path, vocoder_path, vocoder_config_path, None
    def load_model_by_name(self, model_name: str, gpu: bool = False):
        """Load one of the 🐸TTS models by name.
            model_name (str): Model name to load. You can list models by ```tts.models```.
    def load_vc_model_by_name(self, model_name: str, gpu: bool = False):
        """Load one of the voice conversion models by name.
        model_path, config_path, _, _, _ = self.download_model_by_name(model_name)
        self.voice_converter = Synthesizer(vc_checkpoint=model_path, vc_config=config_path, use_cuda=gpu)
    def load_tts_model_by_name(self, model_name: str, gpu: bool = False):
        """Load one of 🐸TTS models by name.
        TODO: Add tests
        model_path, config_path, vocoder_path, vocoder_config_path, model_dir = self.download_model_by_name(
            model_name
        # init synthesizer
        # None values are fetch from the model
        self.synthesizer = Synthesizer(
            tts_checkpoint=model_path,
            tts_config_path=config_path,
            tts_speakers_file=None,
            tts_languages_file=None,
            vocoder_checkpoint=vocoder_path,
            vocoder_config=vocoder_config_path,
            encoder_checkpoint=None,
            encoder_config=None,
            model_dir=model_dir,
            use_cuda=gpu,
    def load_tts_model_by_path(
        self, model_path: str, config_path: str, vocoder_path: str = None, vocoder_config: str = None, gpu: bool = False
        """Load a model from a path.
            model_path (str): Path to the model checkpoint.
            config_path (str): Path to the model config.
            vocoder_config (str, optional): Path to the vocoder config. Defaults to None.
            vocoder_config=vocoder_config,
    def _check_arguments(
        speaker: str = None,
        language: str = None,
        speaker_wav: str = None,
        emotion: str = None,
        speed: float = None,
        """Check if the arguments are valid for the model."""
        # check for the coqui tts models
        if self.is_multi_speaker and (speaker is None and speaker_wav is None):
            raise ValueError("Model is multi-speaker but no `speaker` is provided.")
        if self.is_multi_lingual and language is None:
            raise ValueError("Model is multi-lingual but no `language` is provided.")
        if not self.is_multi_speaker and speaker is not None and "voice_dir" not in kwargs:
            raise ValueError("Model is not multi-speaker but `speaker` is provided.")
        if not self.is_multi_lingual and language is not None:
            raise ValueError("Model is not multi-lingual but `language` is provided.")
        if not emotion is None and not speed is None:
            raise ValueError("Emotion and speed can only be used with Coqui Studio models. Which is discontinued.")
    def tts(
        split_sentences: bool = True,
        """Convert text to speech.
            text (str):
                Input text to synthesize.
            speaker (str, optional):
                Speaker name for multi-speaker. You can check whether loaded model is multi-speaker by
                `tts.is_multi_speaker` and list speakers by `tts.speakers`. Defaults to None.
            language (str): Language of the text. If None, the default language of the speaker is used. Language is only
                supported by `XTTS` model.
            speaker_wav (str, optional):
                Path to a reference wav file to use for voice cloning with supporting models like YourTTS.
                Defaults to None.
            emotion (str, optional):
                Emotion to use for 🐸Coqui Studio models. If None, Studio models use "Neutral". Defaults to None.
            speed (float, optional):
                Speed factor to use for 🐸Coqui Studio models, between 0 and 2.0. If None, Studio models use 1.0.
            split_sentences (bool, optional):
                Split text into sentences, synthesize them separately and concatenate the file audio.
                Setting it False uses more VRAM and possibly hit model specific text length or VRAM limits. Only
                applicable to the 🐸TTS models. Defaults to True.
            kwargs (dict, optional):
                Additional arguments for the model.
        self._check_arguments(
            speaker=speaker, language=language, speaker_wav=speaker_wav, emotion=emotion, speed=speed, **kwargs
        wav = self.synthesizer.tts(
            speaker_name=speaker,
            language_name=language,
            speaker_wav=speaker_wav,
            reference_wav=None,
            style_wav=None,
            style_text=None,
            reference_speaker_name=None,
            split_sentences=split_sentences,
        return wav
    def tts_to_file(
        speed: float = 1.0,
        pipe_out=None,
        file_path: str = "output.wav",
            language (str, optional):
                Language code for multi-lingual models. You can check whether loaded model is multi-lingual
                `tts.is_multi_lingual` and list available languages by `tts.languages`. Defaults to None.
                Emotion to use for 🐸Coqui Studio models. Defaults to "Neutral".
                Speed factor to use for 🐸Coqui Studio models, between 0.0 and 2.0. Defaults to None.
            pipe_out (BytesIO, optional):
                Flag to stdout the generated TTS wav file for shell pipe.
            file_path (str, optional):
                Output file path. Defaults to "output.wav".
        self._check_arguments(speaker=speaker, language=language, speaker_wav=speaker_wav, **kwargs)
        wav = self.tts(
            speaker=speaker,
            language=language,
        self.synthesizer.save_wav(wav=wav, path=file_path, pipe_out=pipe_out)
        return file_path
    def voice_conversion(
        source_wav: str,
        target_wav: str,
        """Voice conversion with FreeVC. Convert source wav to target speaker.
        Args:``
            source_wav (str):
                Path to the source wav file.
            target_wav (str):`
                Path to the target wav file.
        wav = self.voice_converter.voice_conversion(source_wav=source_wav, target_wav=target_wav)
    def voice_conversion_to_file(
            target_wav (str):
        wav = self.voice_conversion(source_wav=source_wav, target_wav=target_wav)
        save_wav(wav=wav, path=file_path, sample_rate=self.voice_converter.vc_config.audio.output_sample_rate)
    def tts_with_vc(
        """Convert text to speech with voice conversion.
        It combines tts with voice conversion to fake voice cloning.
        - Convert text to speech with tts.
        - Convert the output wav to target speaker with voice conversion.
        with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as fp:
            # Lazy code... save it to a temp file to resample it while reading it for VC
            self.tts_to_file(
                text=text, speaker=speaker, language=language, file_path=fp.name, split_sentences=split_sentences
        if self.voice_converter is None:
            self.load_vc_model_by_name("voice_conversion_models/multilingual/vctk/freevc24")
        wav = self.voice_converter.voice_conversion(source_wav=fp.name, target_wav=speaker_wav)
    def tts_with_vc_to_file(
        """Convert text to speech with voice conversion and save to file.
        Check `tts_with_vc` for more details.
        wav = self.tts_with_vc(
            text=text, language=language, speaker_wav=speaker_wav, speaker=speaker, split_sentences=split_sentences
"""Base API."""
    from collections.abc import Iterator
class PlatformDirsABC(ABC):  # noqa: PLR0904
    """Abstract base class for platform directories."""
    def __init__(  # noqa: PLR0913, PLR0917
        Create a new platform directory.
        :param appname: See `appname`.
        :param appauthor: See `appauthor`.
        :param version: See `version`.
        :param roaming: See `roaming`.
        :param multipath: See `multipath`.
        :param opinion: See `opinion`.
        :param ensure_exists: See `ensure_exists`.
        self.appname = appname  #: The name of application.
        self.appauthor = appauthor
        The name of the app author or distributing body for this application.
        Typically, it is the owning company name. Defaults to `appname`. You may pass ``False`` to disable it.
        self.version = version
        An optional version path element to append to the path.
        You might want to use this if you want multiple versions of your app to be able to run independently. If used,
        this would typically be ``<major>.<minor>``.
        self.roaming = roaming
        Whether to use the roaming appdata directory on Windows.
        That means that for users on a Windows network setup for roaming profiles, this user data will be synced on
        login (see
        `here <https://technet.microsoft.com/en-us/library/cc766489(WS.10).aspx>`_).
        self.multipath = multipath
        An optional parameter which indicates that the entire list of data dirs should be returned.
        By default, the first item would only be returned.
        self.opinion = opinion  #: A flag to indicating to use opinionated values.
        self.ensure_exists = ensure_exists
        Optionally create the directory (and any missing parents) upon access if it does not exist.
        By default, no directories are created.
    def _append_app_name_and_version(self, *base: str) -> str:
        params = list(base[1:])
        if self.appname:
            params.append(self.appname)
            if self.version:
                params.append(self.version)
        path = os.path.join(base[0], *params)  # noqa: PTH118
        self._optionally_create_directory(path)
    def _optionally_create_directory(self, path: str) -> None:
        if self.ensure_exists:
            Path(path).mkdir(parents=True, exist_ok=True)
    def _first_item_as_path_if_multipath(self, directory: str) -> Path:
        if self.multipath:
            # If multipath is True, the first path is returned.
            directory = directory.partition(os.pathsep)[0]
        return Path(directory)
    def user_data_dir(self) -> str:
        """:return: data directory tied to the user"""
    def site_data_dir(self) -> str:
        """:return: data directory shared by users"""
    def user_config_dir(self) -> str:
        """:return: config directory tied to the user"""
    def site_config_dir(self) -> str:
        """:return: config directory shared by the users"""
    def user_cache_dir(self) -> str:
        """:return: cache directory tied to the user"""
    def site_cache_dir(self) -> str:
        """:return: cache directory shared by users"""
    def user_state_dir(self) -> str:
        """:return: state directory tied to the user"""
    def user_log_dir(self) -> str:
        """:return: log directory tied to the user"""
    def user_documents_dir(self) -> str:
        """:return: documents directory tied to the user"""
    def user_downloads_dir(self) -> str:
        """:return: downloads directory tied to the user"""
    def user_pictures_dir(self) -> str:
        """:return: pictures directory tied to the user"""
    def user_videos_dir(self) -> str:
        """:return: videos directory tied to the user"""
    def user_music_dir(self) -> str:
        """:return: music directory tied to the user"""
    def user_desktop_dir(self) -> str:
        """:return: desktop directory tied to the user"""
    def user_runtime_dir(self) -> str:
        """:return: runtime directory tied to the user"""
    def site_runtime_dir(self) -> str:
        """:return: runtime directory shared by users"""
    def user_data_path(self) -> Path:
        """:return: data path tied to the user"""
        return Path(self.user_data_dir)
    def site_data_path(self) -> Path:
        """:return: data path shared by users"""
        return Path(self.site_data_dir)
    def user_config_path(self) -> Path:
        """:return: config path tied to the user"""
        return Path(self.user_config_dir)
    def site_config_path(self) -> Path:
        """:return: config path shared by the users"""
        return Path(self.site_config_dir)
    def user_cache_path(self) -> Path:
        """:return: cache path tied to the user"""
        return Path(self.user_cache_dir)
    def site_cache_path(self) -> Path:
        """:return: cache path shared by users"""
        return Path(self.site_cache_dir)
    def user_state_path(self) -> Path:
        """:return: state path tied to the user"""
        return Path(self.user_state_dir)
    def user_log_path(self) -> Path:
        """:return: log path tied to the user"""
        return Path(self.user_log_dir)
    def user_documents_path(self) -> Path:
        """:return: documents a path tied to the user"""
        return Path(self.user_documents_dir)
    def user_downloads_path(self) -> Path:
        """:return: downloads path tied to the user"""
        return Path(self.user_downloads_dir)
    def user_pictures_path(self) -> Path:
        """:return: pictures path tied to the user"""
        return Path(self.user_pictures_dir)
    def user_videos_path(self) -> Path:
        """:return: videos path tied to the user"""
        return Path(self.user_videos_dir)
    def user_music_path(self) -> Path:
        """:return: music path tied to the user"""
        return Path(self.user_music_dir)
    def user_desktop_path(self) -> Path:
        """:return: desktop path tied to the user"""
        return Path(self.user_desktop_dir)
    def user_runtime_path(self) -> Path:
        """:return: runtime path tied to the user"""
        return Path(self.user_runtime_dir)
    def site_runtime_path(self) -> Path:
        """:return: runtime path shared by users"""
        return Path(self.site_runtime_dir)
    def iter_config_dirs(self) -> Iterator[str]:
        """:yield: all user and site configuration directories."""
        yield self.user_config_dir
        yield self.site_config_dir
    def iter_data_dirs(self) -> Iterator[str]:
        """:yield: all user and site data directories."""
        yield self.user_data_dir
        yield self.site_data_dir
    def iter_cache_dirs(self) -> Iterator[str]:
        """:yield: all user and site cache directories."""
        yield self.user_cache_dir
        yield self.site_cache_dir
    def iter_runtime_dirs(self) -> Iterator[str]:
        """:yield: all user and site runtime directories."""
        yield self.user_runtime_dir
        yield self.site_runtime_dir
    def iter_config_paths(self) -> Iterator[Path]:
        """:yield: all user and site configuration paths."""
        for path in self.iter_config_dirs():
            yield Path(path)
    def iter_data_paths(self) -> Iterator[Path]:
        """:yield: all user and site data paths."""
        for path in self.iter_data_dirs():
    def iter_cache_paths(self) -> Iterator[Path]:
        """:yield: all user and site cache paths."""
        for path in self.iter_cache_dirs():
    def iter_runtime_paths(self) -> Iterator[Path]:
        """:yield: all user and site runtime paths."""
        for path in self.iter_runtime_dirs():
requests.api
~~~~~~~~~~~~
This module implements the Requests API.
:copyright: (c) 2012 by Kenneth Reitz.
:license: Apache2, see LICENSE for more details.
from . import sessions
def request(method, url, **kwargs):
    """Constructs and sends a :class:`Request <Request>`.
    :param method: method for the new :class:`Request` object: ``GET``, ``OPTIONS``, ``HEAD``, ``POST``, ``PUT``, ``PATCH``, or ``DELETE``.
    :param url: URL for the new :class:`Request` object.
    :param params: (optional) Dictionary, list of tuples or bytes to send
        in the query string for the :class:`Request`.
    :param data: (optional) Dictionary, list of tuples, bytes, or file-like
        object to send in the body of the :class:`Request`.
    :param json: (optional) A JSON serializable Python object to send in the body of the :class:`Request`.
    :param headers: (optional) Dictionary of HTTP Headers to send with the :class:`Request`.
    :param cookies: (optional) Dict or CookieJar object to send with the :class:`Request`.
    :param files: (optional) Dictionary of ``'name': file-like-objects`` (or ``{'name': file-tuple}``) for multipart encoding upload.
        ``file-tuple`` can be a 2-tuple ``('filename', fileobj)``, 3-tuple ``('filename', fileobj, 'content_type')``
        or a 4-tuple ``('filename', fileobj, 'content_type', custom_headers)``, where ``'content_type'`` is a string
        defining the content type of the given file and ``custom_headers`` a dict-like object containing additional headers
        to add for the file.
    :param auth: (optional) Auth tuple to enable Basic/Digest/Custom HTTP Auth.
    :param timeout: (optional) How many seconds to wait for the server to send data
        before giving up, as a float, or a :ref:`(connect timeout, read
        timeout) <timeouts>` tuple.
    :type timeout: float or tuple
    :param allow_redirects: (optional) Boolean. Enable/disable GET/OPTIONS/POST/PUT/PATCH/DELETE/HEAD redirection. Defaults to ``True``.
    :type allow_redirects: bool
    :param proxies: (optional) Dictionary mapping protocol to the URL of the proxy.
    :param verify: (optional) Either a boolean, in which case it controls whether we verify
            the server's TLS certificate, or a string, in which case it must be a path
            to a CA bundle to use. Defaults to ``True``.
    :param stream: (optional) if ``False``, the response content will be immediately downloaded.
    :param cert: (optional) if String, path to ssl client cert file (.pem). If Tuple, ('cert', 'key') pair.
    :return: :class:`Response <Response>` object
    :rtype: requests.Response
      >>> req = requests.request('GET', 'https://httpbin.org/get')
      >>> req
      <Response [200]>
    # By using the 'with' statement we are sure the session is closed, thus we
    # avoid leaving sockets open which can trigger a ResourceWarning in some
    # cases, and look like a memory leak in others.
    with sessions.Session() as session:
        return session.request(method=method, url=url, **kwargs)
def get(url, params=None, **kwargs):
    r"""Sends a GET request.
    :param \*\*kwargs: Optional arguments that ``request`` takes.
    return request("get", url, params=params, **kwargs)
def options(url, **kwargs):
    r"""Sends an OPTIONS request.
    return request("options", url, **kwargs)
def head(url, **kwargs):
    r"""Sends a HEAD request.
    :param \*\*kwargs: Optional arguments that ``request`` takes. If
        `allow_redirects` is not provided, it will be set to `False` (as
        opposed to the default :meth:`request` behavior).
    kwargs.setdefault("allow_redirects", False)
    return request("head", url, **kwargs)
def post(url, data=None, json=None, **kwargs):
    r"""Sends a POST request.
    return request("post", url, data=data, json=json, **kwargs)
def put(url, data=None, **kwargs):
    r"""Sends a PUT request.
    return request("put", url, data=data, **kwargs)
def patch(url, data=None, **kwargs):
    r"""Sends a PATCH request.
    return request("patch", url, data=data, **kwargs)
def delete(url, **kwargs):
    r"""Sends a DELETE request.
    return request("delete", url, **kwargs)
API for the command-line I{pyflakes} tool.
from pyflakes import checker, __version__
from pyflakes import reporter as modReporter
__all__ = ['check', 'checkPath', 'checkRecursive', 'iterSourceCode', 'main']
PYTHON_SHEBANG_REGEX = re.compile(br'^#!.*\bpython(3(\.\d+)?|w)?[dmu]?\s')
def check(codeString, filename, reporter=None):
    Check the Python source given by C{codeString} for flakes.
    @param codeString: The Python source to check.
    @type codeString: C{str}
    @param filename: The name of the file the source came from, used to report
    @type filename: C{str}
    @param reporter: A L{Reporter} instance, where errors and warnings will be
        reported.
    @return: The number of warnings emitted.
    @rtype: C{int}
    if reporter is None:
        reporter = modReporter._makeDefaultReporter()
    # First, compile into an AST and handle syntax errors.
        tree = ast.parse(codeString, filename=filename)
        reporter.syntaxError(filename, e.args[0], e.lineno, e.offset, e.text)
        reporter.unexpectedError(filename, 'problem decoding source')
    # Okay, it's syntactically valid.  Now check it.
    w = checker.Checker(tree, filename=filename)
    w.messages.sort(key=lambda m: m.lineno)
    for warning in w.messages:
        reporter.flake(warning)
    return len(w.messages)
def checkPath(filename, reporter=None):
    Check the given path, printing out any warnings detected.
    @return: the number of warnings printed
            codestr = f.read()
        reporter.unexpectedError(filename, e.args[1])
    return check(codestr, filename, reporter)
def isPythonFile(filename):
    """Return True if filename points to a Python file."""
    if filename.endswith('.py'):
    # Avoid obvious Emacs backup files
    if filename.endswith("~"):
    max_bytes = 128
            text = f.read(max_bytes)
    return PYTHON_SHEBANG_REGEX.match(text)
def iterSourceCode(paths):
    Iterate over all Python source files in C{paths}.
    @param paths: A list of paths.  Directories will be recursed into and
        any .py files found will be yielded.  Any non-directories will be
        yielded as-is.
            for dirpath, dirnames, filenames in os.walk(path):
                    full_path = os.path.join(dirpath, filename)
                    if isPythonFile(full_path):
                        yield full_path
def checkRecursive(paths, reporter):
    Recursively check all source files in C{paths}.
    @param paths: A list of paths to Python source files and directories
        containing Python source files.
    @param reporter: A L{Reporter} where all of the warnings and errors
        will be reported to.
    @return: The number of warnings found.
    warnings = 0
    for sourcePath in iterSourceCode(paths):
        warnings += checkPath(sourcePath, reporter)
def _exitOnSignal(sigName, message):
    """Handles a signal with sys.exit.
    Some of these signals (SIGPIPE, for example) don't exist or are invalid on
    Windows. So, ignore errors that might arise.
        sigNumber = getattr(signal, sigName)
        # the signal constants defined in the signal module are defined by
        # whether the C library supports them or not. So, SIGPIPE might not
        # even be defined.
    def handler(sig, f):
        sys.exit(message)
        signal.signal(sigNumber, handler)
        # It's also possible the signal is defined, but then it's invalid. In
        # this case, signal.signal raises ValueError.
def _get_version():
    Retrieve and format package version along with python version & OS used
    return ('%s Python %s on %s' %
            (__version__, platform.python_version(), platform.system()))
def main(prog=None, args=None):
    """Entry point for the script "pyflakes"."""
    # Handle "Keyboard Interrupt" and "Broken pipe" gracefully
    _exitOnSignal('SIGINT', '... stopped')
    _exitOnSignal('SIGPIPE', 1)
    parser = argparse.ArgumentParser(prog=prog,
                                     description='Check Python source files for errors')
    parser.add_argument('-V', '--version', action='version', version=_get_version())
    parser.add_argument('path', nargs='*',
                        help='Path(s) of Python file(s) to check. STDIN if not given.')
    args = parser.parse_args(args=args).path
        warnings = checkRecursive(args, reporter)
        warnings = check(sys.stdin.read(), '<stdin>', reporter)
    raise SystemExit(warnings > 0)
"""Public API of the property caching library."""
from ._helpers import cached_property, under_cached_property
    "cached_property",
    "under_cached_property",
from typing import BinaryIO
from .cd import (
    coherence_ratio,
    encoding_languages,
    mb_encoding_languages,
    merge_coherence_ratios,
from .constant import IANA_SUPPORTED, TOO_BIG_SEQUENCE, TOO_SMALL_SEQUENCE, TRACE
from .md import mess_ratio
    any_specified_encoding,
    cut_sequence_chunks,
    iana_name,
    identify_sig_or_bom,
    is_cp_similar,
    is_multi_byte_encoding,
    should_strip_sig_or_bom,
logger = logging.getLogger("charset_normalizer")
explain_handler = logging.StreamHandler()
explain_handler.setFormatter(
    logging.Formatter("%(asctime)s | %(levelname)s | %(message)s")
def from_bytes(
    sequences: bytes | bytearray,
    steps: int = 5,
    chunk_size: int = 512,
    threshold: float = 0.2,
    cp_isolation: list[str] | None = None,
    cp_exclusion: list[str] | None = None,
    preemptive_behaviour: bool = True,
    explain: bool = False,
    language_threshold: float = 0.1,
    enable_fallback: bool = True,
) -> CharsetMatches:
    Given a raw bytes sequence, return the best possibles charset usable to render str objects.
    If there is no results, it is a strong indicator that the source is binary/not text.
    By default, the process will extract 5 blocks of 512o each to assess the mess and coherence of a given sequence.
    And will give up a particular code page after 20% of measured mess. Those criteria are customizable at will.
    The preemptive behavior DOES NOT replace the traditional detection workflow, it prioritize a particular code page
    but never take it for granted. Can improve the performance.
    You may want to focus your attention to some code page or/and not others, use cp_isolation and cp_exclusion for that
    This function will strip the SIG in the payload/sequence every time except on UTF-16, UTF-32.
    By default the library does not setup any handler other than the NullHandler, if you choose to set the 'explain'
    toggle to True it will alter the logger configuration to add a StreamHandler that is suitable for debugging.
    Custom logging format and handler can be set manually.
    if not isinstance(sequences, (bytearray, bytes)):
            "Expected object of type bytes or bytearray, got: {}".format(
                type(sequences)
    if explain:
        previous_logger_level: int = logger.level
        logger.addHandler(explain_handler)
        logger.setLevel(TRACE)
    length: int = len(sequences)
    if length == 0:
        logger.debug("Encoding detection on empty bytes, assuming utf_8 intention.")
        if explain:  # Defensive: ensure exit path clean handler
            logger.removeHandler(explain_handler)
            logger.setLevel(previous_logger_level or logging.WARNING)
        return CharsetMatches([CharsetMatch(sequences, "utf_8", 0.0, False, [], "")])
    if cp_isolation is not None:
            TRACE,
            "cp_isolation is set. use this flag for debugging purpose. "
            "limited list of encoding allowed : %s.",
            ", ".join(cp_isolation),
        cp_isolation = [iana_name(cp, False) for cp in cp_isolation]
        cp_isolation = []
    if cp_exclusion is not None:
            "cp_exclusion is set. use this flag for debugging purpose. "
            "limited list of encoding excluded : %s.",
            ", ".join(cp_exclusion),
        cp_exclusion = [iana_name(cp, False) for cp in cp_exclusion]
        cp_exclusion = []
    if length <= (chunk_size * steps):
            "override steps (%i) and chunk_size (%i) as content does not fit (%i byte(s) given) parameters.",
            steps,
            length,
        steps = 1
        chunk_size = length
    if steps > 1 and length / steps < chunk_size:
        chunk_size = int(length / steps)
    is_too_small_sequence: bool = len(sequences) < TOO_SMALL_SEQUENCE
    is_too_large_sequence: bool = len(sequences) >= TOO_BIG_SEQUENCE
    if is_too_small_sequence:
            "Trying to detect encoding from a tiny portion of ({}) byte(s).".format(
                length
    elif is_too_large_sequence:
            "Using lazy str decoding because the payload is quite large, ({}) byte(s).".format(
    prioritized_encodings: list[str] = []
    specified_encoding: str | None = (
        any_specified_encoding(sequences) if preemptive_behaviour else None
    if specified_encoding is not None:
        prioritized_encodings.append(specified_encoding)
            "Detected declarative mark in sequence. Priority +1 given for %s.",
            specified_encoding,
    tested: set[str] = set()
    tested_but_hard_failure: list[str] = []
    tested_but_soft_failure: list[str] = []
    fallback_ascii: CharsetMatch | None = None
    fallback_u8: CharsetMatch | None = None
    fallback_specified: CharsetMatch | None = None
    results: CharsetMatches = CharsetMatches()
    early_stop_results: CharsetMatches = CharsetMatches()
    sig_encoding, sig_payload = identify_sig_or_bom(sequences)
    if sig_encoding is not None:
        prioritized_encodings.append(sig_encoding)
            "Detected a SIG or BOM mark on first %i byte(s). Priority +1 given for %s.",
            len(sig_payload),
            sig_encoding,
    prioritized_encodings.append("ascii")
    if "utf_8" not in prioritized_encodings:
        prioritized_encodings.append("utf_8")
    for encoding_iana in prioritized_encodings + IANA_SUPPORTED:
        if cp_isolation and encoding_iana not in cp_isolation:
        if cp_exclusion and encoding_iana in cp_exclusion:
        if encoding_iana in tested:
        tested.add(encoding_iana)
        decoded_payload: str | None = None
        bom_or_sig_available: bool = sig_encoding == encoding_iana
        strip_sig_or_bom: bool = bom_or_sig_available and should_strip_sig_or_bom(
            encoding_iana
        if encoding_iana in {"utf_16", "utf_32"} and not bom_or_sig_available:
                "Encoding %s won't be tested as-is because it require a BOM. Will try some sub-encoder LE/BE.",
                encoding_iana,
        if encoding_iana in {"utf_7"} and not bom_or_sig_available:
                "Encoding %s won't be tested as-is because detection is unreliable without BOM/SIG.",
            is_multi_byte_decoder: bool = is_multi_byte_encoding(encoding_iana)
        except (ModuleNotFoundError, ImportError):
                "Encoding %s does not provide an IncrementalDecoder",
            if is_too_large_sequence and is_multi_byte_decoder is False:
                        sequences[: int(50e4)]
                        if strip_sig_or_bom is False
                        else sequences[len(sig_payload) : int(50e4)]
                    encoding=encoding_iana,
                decoded_payload = str(
                        sequences
                        else sequences[len(sig_payload) :]
        except (UnicodeDecodeError, LookupError) as e:
            if not isinstance(e, LookupError):
                    "Code page %s does not fit given bytes sequence at ALL. %s",
                    str(e),
            tested_but_hard_failure.append(encoding_iana)
        similar_soft_failure_test: bool = False
        for encoding_soft_failed in tested_but_soft_failure:
            if is_cp_similar(encoding_iana, encoding_soft_failed):
                similar_soft_failure_test = True
        if similar_soft_failure_test:
                "%s is deemed too similar to code page %s and was consider unsuited already. Continuing!",
                encoding_soft_failed,
        r_ = range(
            0 if not bom_or_sig_available else len(sig_payload),
            int(length / steps),
        multi_byte_bonus: bool = (
            is_multi_byte_decoder
            and decoded_payload is not None
            and len(decoded_payload) < length
        if multi_byte_bonus:
                "Code page %s is a multi byte encoding table and it appear that at least one character "
                "was encoded using n-bytes.",
        max_chunk_gave_up: int = int(len(r_) / 4)
        max_chunk_gave_up = max(max_chunk_gave_up, 2)
        early_stop_count: int = 0
        lazy_str_hard_failure = False
        md_chunks: list[str] = []
        md_ratios = []
            for chunk in cut_sequence_chunks(
                sequences,
                r_,
                bom_or_sig_available,
                strip_sig_or_bom,
                sig_payload,
                is_multi_byte_decoder,
                decoded_payload,
                md_chunks.append(chunk)
                md_ratios.append(
                    mess_ratio(
                        threshold,
                        explain is True and 1 <= len(cp_isolation) <= 2,
                if md_ratios[-1] >= threshold:
                    early_stop_count += 1
                if (early_stop_count >= max_chunk_gave_up) or (
                    bom_or_sig_available and strip_sig_or_bom is False
            UnicodeDecodeError
        ) as e:  # Lazy str loading may have missed something there
                "LazyStr Loading: After MD chunk decode, code page %s does not fit given bytes sequence at ALL. %s",
            early_stop_count = max_chunk_gave_up
            lazy_str_hard_failure = True
        # We might want to check the sequence again with the whole content
        # Only if initial MD tests passes
            not lazy_str_hard_failure
            and is_too_large_sequence
            and not is_multi_byte_decoder
                sequences[int(50e3) :].decode(encoding_iana, errors="strict")
            except UnicodeDecodeError as e:
                    "LazyStr Loading: After final lookup, code page %s does not fit given bytes sequence at ALL. %s",
        mean_mess_ratio: float = sum(md_ratios) / len(md_ratios) if md_ratios else 0.0
        if mean_mess_ratio >= threshold or early_stop_count >= max_chunk_gave_up:
            tested_but_soft_failure.append(encoding_iana)
                "%s was excluded because of initial chaos probing. Gave up %i time(s). "
                "Computed mean chaos is %f %%.",
                early_stop_count,
                round(mean_mess_ratio * 100, ndigits=3),
            # Preparing those fallbacks in case we got nothing.
                enable_fallback
                and encoding_iana
                in ["ascii", "utf_8", specified_encoding, "utf_16", "utf_32"]
                and not lazy_str_hard_failure
                fallback_entry = CharsetMatch(
                    preemptive_declaration=specified_encoding,
                if encoding_iana == specified_encoding:
                    fallback_specified = fallback_entry
                elif encoding_iana == "ascii":
                    fallback_ascii = fallback_entry
                    fallback_u8 = fallback_entry
            "%s passed initial chaos probing. Mean measured chaos is %f %%",
        if not is_multi_byte_decoder:
            target_languages: list[str] = encoding_languages(encoding_iana)
            target_languages = mb_encoding_languages(encoding_iana)
        if target_languages:
                "{} should target any language(s) of {}".format(
                    encoding_iana, str(target_languages)
        cd_ratios = []
        # We shall skip the CD when its about ASCII
        # Most of the time its not relevant to run "language-detection" on it.
        if encoding_iana != "ascii":
            for chunk in md_chunks:
                chunk_languages = coherence_ratio(
                    language_threshold,
                    ",".join(target_languages) if target_languages else None,
                cd_ratios.append(chunk_languages)
        cd_ratios_merged = merge_coherence_ratios(cd_ratios)
        if cd_ratios_merged:
                "We detected language {} using {}".format(
                    cd_ratios_merged, encoding_iana
        current_match = CharsetMatch(
            mean_mess_ratio,
            cd_ratios_merged,
                decoded_payload
                    is_too_large_sequence is False
                    or encoding_iana in [specified_encoding, "ascii", "utf_8"]
        results.append(current_match)
            encoding_iana in [specified_encoding, "ascii", "utf_8"]
            and mean_mess_ratio < 0.1
            # If md says nothing to worry about, then... stop immediately!
            if mean_mess_ratio == 0.0:
                    "Encoding detection: %s is most likely the one.",
                    current_match.encoding,
                    logger.setLevel(previous_logger_level)
                return CharsetMatches([current_match])
            early_stop_results.append(current_match)
            len(early_stop_results)
            and (specified_encoding is None or specified_encoding in tested)
            and "ascii" in tested
            and "utf_8" in tested
            probable_result: CharsetMatch = early_stop_results.best()  # type: ignore[assignment]
                probable_result.encoding,
            return CharsetMatches([probable_result])
        if encoding_iana == sig_encoding:
                "Encoding detection: %s is most likely the one as we detected a BOM or SIG within "
                "the beginning of the sequence.",
            return CharsetMatches([results[encoding_iana]])
    if len(results) == 0:
        if fallback_u8 or fallback_ascii or fallback_specified:
                "Nothing got out of the detection process. Using ASCII/UTF-8/Specified fallback.",
        if fallback_specified:
                "Encoding detection: %s will be used as a fallback match",
                fallback_specified.encoding,
            results.append(fallback_specified)
            (fallback_u8 and fallback_ascii is None)
                fallback_u8
                and fallback_ascii
                and fallback_u8.fingerprint != fallback_ascii.fingerprint
            or (fallback_u8 is not None)
            logger.debug("Encoding detection: utf_8 will be used as a fallback match")
            results.append(fallback_u8)
        elif fallback_ascii:
            logger.debug("Encoding detection: ascii will be used as a fallback match")
            results.append(fallback_ascii)
            "Encoding detection: Found %s as plausible (best-candidate) for content. With %i alternatives.",
            results.best().encoding,  # type: ignore
            len(results) - 1,
        logger.debug("Encoding detection: Unable to determine any suitable charset.")
def from_fp(
    fp: BinaryIO,
    threshold: float = 0.20,
    Same thing than the function from_bytes but using a file pointer that is already ready.
    Will not close the file pointer.
    return from_bytes(
        fp.read(),
        cp_isolation,
        cp_exclusion,
        preemptive_behaviour,
        explain,
        enable_fallback,
    path: str | bytes | PathLike,  # type: ignore[type-arg]
    Same thing than the function from_bytes but with one extra step. Opening and reading given file path in binary mode.
    Can raise IOError.
    with open(path, "rb") as fp:
        return from_fp(
def is_binary(
    fp_or_path_or_payload: PathLike | str | BinaryIO | bytes,  # type: ignore[type-arg]
    enable_fallback: bool = False,
    Detect if the given input (file, bytes, or path) points to a binary file. aka. not a string.
    Based on the same main heuristic algorithms and default kwargs at the sole exception that fallbacks match
    are disabled to be stricter around ASCII-compatible but unlikely to be a string.
    if isinstance(fp_or_path_or_payload, (str, PathLike)):
        guesses = from_path(
            fp_or_path_or_payload,
            steps=steps,
            chunk_size=chunk_size,
            threshold=threshold,
            cp_isolation=cp_isolation,
            cp_exclusion=cp_exclusion,
            preemptive_behaviour=preemptive_behaviour,
            explain=explain,
            language_threshold=language_threshold,
            enable_fallback=enable_fallback,
            bytearray,
        guesses = from_bytes(
        guesses = from_fp(
    return not guesses
import sys, types
from .lock import allocate_lock
from .error import CDefError
from . import model
    callable
    # Python 3.1
    from collections import Callable
    callable = lambda x: isinstance(x, Callable)
    basestring
    # Python 3.x
_unspecified = object()
class FFI(object):
    r'''
    The main top-level class that you instantiate once, or once per module.
        ffi = FFI()
        ffi.cdef("""
            int printf(const char *, ...);
        C = ffi.dlopen(None)   # standard library
        -or-
        C = ffi.verify()  # use a C compiler: verify the decl above is right
        C.printf("hello, %s!\n", ffi.new("char[]", "world"))
    def __init__(self, backend=None):
        """Create an FFI instance.  The 'backend' argument is used to
        select a non-default backend, mostly for tests.
            # You need PyPy (>= 2.0 beta), or a CPython (>= 2.6) with
            # _cffi_backend.so compiled.
            import _cffi_backend as backend
            if backend.__version__ != __version__:
                # bad version!  Try to be as explicit as possible.
                if hasattr(backend, '__file__'):
                    # CPython
                    raise Exception("Version mismatch: this is the 'cffi' package version %s, located in %r.  When we import the top-level '_cffi_backend' extension module, we get version %s, located in %r.  The two versions should be equal; check your installation." % (
                        __version__, __file__,
                        backend.__version__, backend.__file__))
                    # PyPy
                    raise Exception("Version mismatch: this is the 'cffi' package version %s, located in %r.  This interpreter comes with a built-in '_cffi_backend' module, which is version %s.  The two versions should be equal; check your installation." % (
                        __version__, __file__, backend.__version__))
            # (If you insist you can also try to pass the option
            # 'backend=backend_ctypes.CTypesBackend()', but don't
            # rely on it!  It's probably not going to work well.)
        from . import cparser
        self._backend = backend
        self._lock = allocate_lock()
        self._parser = cparser.Parser()
        self._cached_btypes = {}
        self._parsed_types = types.ModuleType('parsed_types').__dict__
        self._new_types = types.ModuleType('new_types').__dict__
        self._function_caches = []
        self._libraries = []
        self._cdefsources = []
        self._included_ffis = []
        self._windows_unicode = None
        self._init_once_cache = {}
        self._cdef_version = None
        self._embedding = None
        self._typecache = model.get_typecache(backend)
        if hasattr(backend, 'set_ffi'):
            backend.set_ffi(self)
        for name in list(backend.__dict__):
            if name.startswith('RTLD_'):
                setattr(self, name, getattr(backend, name))
            self.BVoidP = self._get_cached_btype(model.voidp_type)
            self.BCharA = self._get_cached_btype(model.char_array_type)
        if isinstance(backend, types.ModuleType):
            # _cffi_backend: attach these constants to the class
            if not hasattr(FFI, 'NULL'):
                FFI.NULL = self.cast(self.BVoidP, 0)
                FFI.CData, FFI.CType = backend._get_types()
            # ctypes backend: attach these constants to the instance
            self.NULL = self.cast(self.BVoidP, 0)
            self.CData, self.CType = backend._get_types()
        self.buffer = backend.buffer
    def cdef(self, csource, override=False, packed=False, pack=None):
        """Parse the given C source.  This registers all declared functions,
        types, and global variables.  The functions and global variables can
        then be accessed via either 'ffi.dlopen()' or 'ffi.verify()'.
        The types can be used in 'ffi.new()' and other functions.
        If 'packed' is specified as True, all structs declared inside this
        cdef are packed, i.e. laid out without any field alignment at all.
        Alternatively, 'pack' can be a small integer, and requests for
        alignment greater than that are ignored (pack=1 is equivalent to
        packed=True).
        self._cdef(csource, override=override, packed=packed, pack=pack)
    def embedding_api(self, csource, packed=False, pack=None):
        self._cdef(csource, packed=packed, pack=pack, dllexport=True)
        if self._embedding is None:
            self._embedding = ''
    def _cdef(self, csource, override=False, **options):
        if not isinstance(csource, str):    # unicode, on Python 2
            if not isinstance(csource, basestring):
                raise TypeError("cdef() argument must be a string")
            csource = csource.encode('ascii')
            self._cdef_version = object()
            self._parser.parse(csource, override=override, **options)
            self._cdefsources.append(csource)
                for cache in self._function_caches:
            finishlist = self._parser._recomplete
            if finishlist:
                self._parser._recomplete = []
                for tp in finishlist:
                    tp.finish_backend_type(self, finishlist)
    def dlopen(self, name, flags=0):
        """Load and return a dynamic library identified by 'name'.
        The standard C library can be loaded by passing None.
        Note that functions and types declared by 'ffi.cdef()' are not
        linked to a particular library, just like C headers; in the
        library we only look for the actual (untyped) symbols.
        if not (isinstance(name, basestring) or
                name is None or
                isinstance(name, self.CData)):
            raise TypeError("dlopen(name): name must be a file name, None, "
                            "or an already-opened 'void *' handle")
            lib, function_cache = _make_ffi_library(self, name, flags)
            self._function_caches.append(function_cache)
            self._libraries.append(lib)
        return lib
    def dlclose(self, lib):
        """Close a library obtained with ffi.dlopen().  After this call,
        access to functions or variables from the library will fail
        (possibly with a segmentation fault).
        type(lib).__cffi_close__(lib)
    def _typeof_locked(self, cdecl):
        # call me with the lock!
        key = cdecl
        if key in self._parsed_types:
            return self._parsed_types[key]
        if not isinstance(cdecl, str):    # unicode, on Python 2
            cdecl = cdecl.encode('ascii')
        type = self._parser.parse_type(cdecl)
        really_a_function_type = type.is_raw_function
        if really_a_function_type:
            type = type.as_function_pointer()
        btype = self._get_cached_btype(type)
        result = btype, really_a_function_type
        self._parsed_types[key] = result
    def _typeof(self, cdecl, consider_function_as_funcptr=False):
        # string -> ctype object
            result = self._parsed_types[cdecl]
                result = self._typeof_locked(cdecl)
        btype, really_a_function_type = result
        if really_a_function_type and not consider_function_as_funcptr:
            raise CDefError("the type %r is a function type, not a "
                            "pointer-to-function type" % (cdecl,))
        return btype
    def typeof(self, cdecl):
        """Parse the C type given as a string and return the
        corresponding <ctype> object.
        It can also be used on 'cdata' instance to get its C type.
        if isinstance(cdecl, basestring):
            return self._typeof(cdecl)
        if isinstance(cdecl, self.CData):
            return self._backend.typeof(cdecl)
        if isinstance(cdecl, types.BuiltinFunctionType):
            res = _builtin_function_type(cdecl)
        if (isinstance(cdecl, types.FunctionType)
                and hasattr(cdecl, '_cffi_base_type')):
                return self._get_cached_btype(cdecl._cffi_base_type)
        raise TypeError(type(cdecl))
    def sizeof(self, cdecl):
        """Return the size in bytes of the argument.  It can be a
        string naming a C type, or a 'cdata' instance.
            BType = self._typeof(cdecl)
            return self._backend.sizeof(BType)
            return self._backend.sizeof(cdecl)
    def alignof(self, cdecl):
        """Return the natural alignment size in bytes of the C type
        given as a string.
            cdecl = self._typeof(cdecl)
        return self._backend.alignof(cdecl)
    def offsetof(self, cdecl, *fields_or_indexes):
        """Return the offset of the named field inside the given
        structure or array, which must be given as a C type name.
        You can give several field names in case of nested structures.
        You can also give numeric values which correspond to array
        items, in case of an array type.
        return self._typeoffsetof(cdecl, *fields_or_indexes)[1]
    def new(self, cdecl, init=None):
        """Allocate an instance according to the specified C type and
        return a pointer to it.  The specified C type must be either a
        pointer or an array: ``new('X *')`` allocates an X and returns
        a pointer to it, whereas ``new('X[n]')`` allocates an array of
        n X'es and returns an array referencing it (which works
        mostly like a pointer, like in C).  You can also use
        ``new('X[]', n)`` to allocate an array of a non-constant
        length n.
        The memory is initialized following the rules of declaring a
        global variable in C: by default it is zero-initialized, but
        an explicit initializer can be given which can be used to
        fill all or part of the memory.
        When the returned <cdata> object goes out of scope, the memory
        is freed.  In other words the returned <cdata> object has
        ownership of the value of type 'cdecl' that it points to.  This
        means that the raw data can be used as long as this object is
        kept alive, but must not be used for a longer time.  Be careful
        about that when copying the pointer to the memory somewhere
        else, e.g. into another structure.
        return self._backend.newp(cdecl, init)
    def new_allocator(self, alloc=None, free=None,
                      should_clear_after_alloc=True):
        """Return a new allocator, i.e. a function that behaves like ffi.new()
        but uses the provided low-level 'alloc' and 'free' functions.
        'alloc' is called with the size as argument.  If it returns NULL, a
        MemoryError is raised.  'free' is called with the result of 'alloc'
        as argument.  Both can be either Python function or directly C
        functions.  If 'free' is None, then no free function is called.
        If both 'alloc' and 'free' are None, the default is used.
        If 'should_clear_after_alloc' is set to False, then the memory
        returned by 'alloc' is assumed to be already cleared (or you are
        fine with garbage); otherwise CFFI will clear it.
        compiled_ffi = self._backend.FFI()
        allocator = compiled_ffi.new_allocator(alloc, free,
                                               should_clear_after_alloc)
        def allocate(cdecl, init=None):
            return allocator(cdecl, init)
        return allocate
    def cast(self, cdecl, source):
        """Similar to a C cast: returns an instance of the named C
        type initialized with the given 'source'.  The source is
        casted between integers or pointers of any type.
        return self._backend.cast(cdecl, source)
    def string(self, cdata, maxlen=-1):
        """Return a Python string (or unicode string) from the 'cdata'.
        If 'cdata' is a pointer or array of characters or bytes, returns
        the null-terminated string.  The returned string extends until
        the first null character, or at most 'maxlen' characters.  If
        'cdata' is an array then 'maxlen' defaults to its length.
        If 'cdata' is a pointer or array of wchar_t, returns a unicode
        string following the same rules.
        If 'cdata' is a single character or byte or a wchar_t, returns
        it as a string or unicode string.
        If 'cdata' is an enum, returns the value of the enumerator as a
        string, or 'NUMBER' if the value is out of range.
        return self._backend.string(cdata, maxlen)
    def unpack(self, cdata, length):
        """Unpack an array of C data of the given length,
        returning a Python string/unicode/list.
        If 'cdata' is a pointer to 'char', returns a byte string.
        It does not stop at the first null.  This is equivalent to:
        ffi.buffer(cdata, length)[:]
        If 'cdata' is a pointer to 'wchar_t', returns a unicode string.
        'length' is measured in wchar_t's; it is not the size in bytes.
        If 'cdata' is a pointer to anything else, returns a list of
        'length' items.  This is a faster equivalent to:
        [cdata[i] for i in range(length)]
        return self._backend.unpack(cdata, length)
   #def buffer(self, cdata, size=-1):
   #    """Return a read-write buffer object that references the raw C data
   #    pointed to by the given 'cdata'.  The 'cdata' must be a pointer or
   #    an array.  Can be passed to functions expecting a buffer, or directly
   #    manipulated with:
   #        buf[:]          get a copy of it in a regular string, or
   #        buf[idx]        as a single character
   #        buf[:] = ...
   #        buf[idx] = ...  change the content
   #    """
   #    note that 'buffer' is a type, set on this instance by __init__
    def from_buffer(self, cdecl, python_buffer=_unspecified,
                    require_writable=False):
        """Return a cdata of the given type pointing to the data of the
        given Python object, which must support the buffer interface.
        Note that this is not meant to be used on the built-in types
        str or unicode (you can build 'char[]' arrays explicitly)
        but only on objects containing large quantities of raw data
        in some other format, like 'array.array' or numpy arrays.
        The first argument is optional and default to 'char[]'.
        if python_buffer is _unspecified:
            cdecl, python_buffer = self.BCharA, cdecl
        elif isinstance(cdecl, basestring):
        return self._backend.from_buffer(cdecl, python_buffer,
                                         require_writable)
    def memmove(self, dest, src, n):
        """ffi.memmove(dest, src, n) copies n bytes of memory from src to dest.
        Like the C function memmove(), the memory areas may overlap;
        apart from that it behaves like the C function memcpy().
        'src' can be any cdata ptr or array, or any Python buffer object.
        'dest' can be any cdata ptr or array, or a writable Python buffer
        object.  The size to copy, 'n', is always measured in bytes.
        Unlike other methods, this one supports all Python buffer including
        byte strings and bytearrays---but it still does not support
        non-contiguous buffers.
        return self._backend.memmove(dest, src, n)
    def callback(self, cdecl, python_callable=None, error=None, onerror=None):
        """Return a callback object or a decorator making such a
        callback object.  'cdecl' must name a C function pointer type.
        The callback invokes the specified 'python_callable' (which may
        be provided either directly or via a decorator).  Important: the
        callback object must be manually kept alive for as long as the
        callback may be invoked from the C level.
        def callback_decorator_wrap(python_callable):
            if not callable(python_callable):
                raise TypeError("the 'python_callable' argument "
                                "is not callable")
            return self._backend.callback(cdecl, python_callable,
                                          error, onerror)
            cdecl = self._typeof(cdecl, consider_function_as_funcptr=True)
        if python_callable is None:
            return callback_decorator_wrap                # decorator mode
            return callback_decorator_wrap(python_callable)  # direct mode
    def getctype(self, cdecl, replace_with=''):
        """Return a string giving the C type 'cdecl', which may be itself
        a string or a <ctype> object.  If 'replace_with' is given, it gives
        extra text to append (or insert for more complicated C types), like
        a variable name, or '*' to get actually the C type 'pointer-to-cdecl'.
        replace_with = replace_with.strip()
        if (replace_with.startswith('*')
                and '&[' in self._backend.getcname(cdecl, '&')):
            replace_with = '(%s)' % replace_with
        elif replace_with and not replace_with[0] in '[(':
            replace_with = ' ' + replace_with
        return self._backend.getcname(cdecl, replace_with)
    def gc(self, cdata, destructor, size=0):
        """Return a new cdata object that points to the same
        data.  Later, when this new cdata object is garbage-collected,
        'destructor(old_cdata_object)' will be called.
        The optional 'size' gives an estimate of the size, used to
        trigger the garbage collection more eagerly.  So far only used
        on PyPy.  It tells the GC that the returned object keeps alive
        roughly 'size' bytes of external memory.
        return self._backend.gcp(cdata, destructor, size)
    def _get_cached_btype(self, type):
        assert self._lock.acquire(False) is False
            BType = self._cached_btypes[type]
            finishlist = []
            BType = type.get_cached_btype(self, finishlist)
            for type in finishlist:
                type.finish_backend_type(self, finishlist)
        return BType
    def verify(self, source='', tmpdir=None, **kwargs):
        """Verify that the current ffi signatures compile on this
        machine, and return a dynamic library object.  The dynamic
        library can be used to call functions and access global
        variables declared in this 'ffi'.  The library is compiled
        by the C compiler: it gives you C-level API compatibility
        (including calling macros).  This is unlike 'ffi.dlopen()',
        which requires binary compatibility in the signatures.
        from .verifier import Verifier, _caller_dir_pycache
        # If set_unicode(True) was called, insert the UNICODE and
        # _UNICODE macro declarations
        if self._windows_unicode:
            self._apply_windows_unicode(kwargs)
        # Set the tmpdir here, and not in Verifier.__init__: it picks
        # up the caller's directory, which we want to be the caller of
        # ffi.verify(), as opposed to the caller of Veritier().
        tmpdir = tmpdir or _caller_dir_pycache()
        # Make a Verifier() and use it to load the library.
        self.verifier = Verifier(self, source, tmpdir, **kwargs)
        lib = self.verifier.load_library()
        # Save the loaded library for keep-alive purposes, even
        # if the caller doesn't keep it alive itself (it should).
    def _get_errno(self):
        return self._backend.get_errno()
    def _set_errno(self, errno):
        self._backend.set_errno(errno)
    errno = property(_get_errno, _set_errno, None,
                     "the value of 'errno' from/to the C calls")
    def getwinerror(self, code=-1):
        return self._backend.getwinerror(code)
    def _pointer_to(self, ctype):
            return model.pointer_cache(self, ctype)
    def addressof(self, cdata, *fields_or_indexes):
        """Return the address of a <cdata 'struct-or-union'>.
        If 'fields_or_indexes' are given, returns the address of that
        field or array item in the structure or array, recursively in
        case of nested structures.
            ctype = self._backend.typeof(cdata)
            if '__addressof__' in type(cdata).__dict__:
                return type(cdata).__addressof__(cdata, *fields_or_indexes)
        if fields_or_indexes:
            ctype, offset = self._typeoffsetof(ctype, *fields_or_indexes)
            if ctype.kind == "pointer":
                raise TypeError("addressof(pointer)")
        ctypeptr = self._pointer_to(ctype)
        return self._backend.rawaddressof(ctypeptr, cdata, offset)
    def _typeoffsetof(self, ctype, field_or_index, *fields_or_indexes):
        ctype, offset = self._backend.typeoffsetof(ctype, field_or_index)
        for field1 in fields_or_indexes:
            ctype, offset1 = self._backend.typeoffsetof(ctype, field1, 1)
            offset += offset1
        return ctype, offset
    def include(self, ffi_to_include):
        """Includes the typedefs, structs, unions and enums defined
        in another FFI instance.  Usage is similar to a #include in C,
        where a part of the program might include types defined in
        another part for its own usage.  Note that the include()
        method has no effect on functions, constants and global
        variables, which must anyway be accessed directly from the
        lib object returned by the original FFI instance.
        if not isinstance(ffi_to_include, FFI):
            raise TypeError("ffi.include() expects an argument that is also of"
                            " type cffi.FFI, not %r" % (
                                type(ffi_to_include).__name__,))
        if ffi_to_include is self:
            raise ValueError("self.include(self)")
        with ffi_to_include._lock:
                self._parser.include(ffi_to_include._parser)
                self._cdefsources.append('[')
                self._cdefsources.extend(ffi_to_include._cdefsources)
                self._cdefsources.append(']')
                self._included_ffis.append(ffi_to_include)
    def new_handle(self, x):
        return self._backend.newp_handle(self.BVoidP, x)
    def from_handle(self, x):
        return self._backend.from_handle(x)
    def release(self, x):
        self._backend.release(x)
    def set_unicode(self, enabled_flag):
        """Windows: if 'enabled_flag' is True, enable the UNICODE and
        _UNICODE defines in C, and declare the types like TCHAR and LPTCSTR
        to be (pointers to) wchar_t.  If 'enabled_flag' is False,
        declare these types to be (pointers to) plain 8-bit characters.
        This is mostly for backward compatibility; you usually want True.
        if self._windows_unicode is not None:
            raise ValueError("set_unicode() can only be called once")
        enabled_flag = bool(enabled_flag)
        if enabled_flag:
            self.cdef("typedef wchar_t TBYTE;"
                      "typedef wchar_t TCHAR;"
                      "typedef const wchar_t *LPCTSTR;"
                      "typedef const wchar_t *PCTSTR;"
                      "typedef wchar_t *LPTSTR;"
                      "typedef wchar_t *PTSTR;"
                      "typedef TBYTE *PTBYTE;"
                      "typedef TCHAR *PTCHAR;")
            self.cdef("typedef char TBYTE;"
                      "typedef char TCHAR;"
                      "typedef const char *LPCTSTR;"
                      "typedef const char *PCTSTR;"
                      "typedef char *LPTSTR;"
                      "typedef char *PTSTR;"
        self._windows_unicode = enabled_flag
    def _apply_windows_unicode(self, kwds):
        defmacros = kwds.get('define_macros', ())
        if not isinstance(defmacros, (list, tuple)):
            raise TypeError("'define_macros' must be a list or tuple")
        defmacros = list(defmacros) + [('UNICODE', '1'),
                                       ('_UNICODE', '1')]
        kwds['define_macros'] = defmacros
    def _apply_embedding_fix(self, kwds):
        # must include an argument like "-lpython2.7" for the compiler
        def ensure(key, value):
            lst = kwds.setdefault(key, [])
            if value not in lst:
                lst.append(value)
        if '__pypy__' in sys.builtin_module_names:
                # we need 'libpypy-c.lib'.  Current distributions of
                # pypy (>= 4.1) contain it as 'libs/python27.lib'.
                pythonlib = "python{0[0]}{0[1]}".format(sys.version_info)
                if hasattr(sys, 'prefix'):
                    ensure('library_dirs', os.path.join(sys.prefix, 'libs'))
                # we need 'libpypy-c.{so,dylib}', which should be by
                # default located in 'sys.prefix/bin' for installed
                # systems.
                if sys.version_info < (3,):
                    pythonlib = "pypy-c"
                    pythonlib = "pypy3-c"
                    ensure('library_dirs', os.path.join(sys.prefix, 'bin'))
            # On uninstalled pypy's, the libpypy-c is typically found in
            # .../pypy/goal/.
                ensure('library_dirs', os.path.join(sys.prefix, 'pypy', 'goal'))
                template = "python%d%d"
                    template += '_d'
                except ImportError:    # 2.6
                    from cffi._shimmed_dist_utils import sysconfig
                template = "python%d.%d"
                if sysconfig.get_config_var('DEBUG_EXT'):
                    template += sysconfig.get_config_var('DEBUG_EXT')
            pythonlib = (template %
                    (sys.hexversion >> 24, (sys.hexversion >> 16) & 0xff))
            if hasattr(sys, 'abiflags'):
                pythonlib += sys.abiflags
        ensure('libraries', pythonlib)
            ensure('extra_link_args', '/MANIFEST')
    def set_source(self, module_name, source, source_extension='.c', **kwds):
        if hasattr(self, '_assigned_source'):
            raise ValueError("set_source() cannot be called several times "
                             "per ffi object")
        if not isinstance(module_name, basestring):
            raise TypeError("'module_name' must be a string")
        if os.sep in module_name or (os.altsep and os.altsep in module_name):
            raise ValueError("'module_name' must not contain '/': use a dotted "
                             "name to make a 'package.module' location")
        self._assigned_source = (str(module_name), source,
                                 source_extension, kwds)
    def set_source_pkgconfig(self, module_name, pkgconfig_libs, source,
                             source_extension='.c', **kwds):
        from . import pkgconfig
        if not isinstance(pkgconfig_libs, list):
            raise TypeError("the pkgconfig_libs argument must be a list "
                            "of package names")
        kwds2 = pkgconfig.flags_from_pkgconfig(pkgconfig_libs)
        pkgconfig.merge_flags(kwds, kwds2)
        self.set_source(module_name, source, source_extension, **kwds)
    def distutils_extension(self, tmpdir='build', verbose=True):
        from cffi._shimmed_dist_utils import mkpath
        from .recompiler import recompile
        if not hasattr(self, '_assigned_source'):
            if hasattr(self, 'verifier'):     # fallback, 'tmpdir' ignored
                return self.verifier.get_extension()
            raise ValueError("set_source() must be called before"
                             " distutils_extension()")
        module_name, source, source_extension, kwds = self._assigned_source
        if source is None:
            raise TypeError("distutils_extension() is only for C extension "
                            "modules, not for dlopen()-style pure Python "
                            "modules")
        mkpath(tmpdir)
        ext, updated = recompile(self, module_name,
                                 source, tmpdir=tmpdir, extradir=tmpdir,
                                 source_extension=source_extension,
                                 call_c_compiler=False, **kwds)
            if updated:
                sys.stderr.write("regenerated: %r\n" % (ext.sources[0],))
                sys.stderr.write("not modified: %r\n" % (ext.sources[0],))
    def emit_c_code(self, filename):
            raise ValueError("set_source() must be called before emit_c_code()")
            raise TypeError("emit_c_code() is only for C extension modules, "
                            "not for dlopen()-style pure Python modules")
        recompile(self, module_name, source,
                  c_file=filename, call_c_compiler=False,
                  uses_ffiplatform=False, **kwds)
    def emit_python_code(self, filename):
            raise TypeError("emit_python_code() is only for dlopen()-style "
                            "pure Python modules, not for C extension modules")
    def compile(self, tmpdir='.', verbose=0, target=None, debug=None):
        """The 'target' argument gives the final file name of the
        compiled DLL.  Use '*' to force distutils' choice, suitable for
        regular CPython C API modules.  Use a file name ending in '.*'
        to ask for the system's default extension for dynamic libraries
        (.so/.dll/.dylib).
        The default is '*' when building a non-embedded C API extension,
        and (module_name + '.*') when building an embedded library.
            raise ValueError("set_source() must be called before compile()")
        return recompile(self, module_name, source, tmpdir=tmpdir,
                         target=target, source_extension=source_extension,
                         compiler_verbose=verbose, debug=debug, **kwds)
    def init_once(self, func, tag):
        # Read _init_once_cache[tag], which is either (False, lock) if
        # we're calling the function now in some thread, or (True, result).
        # Don't call setdefault() in most cases, to avoid allocating and
        # immediately freeing a lock; but still use setdefaut() to avoid
        # races.
            x = self._init_once_cache[tag]
            x = self._init_once_cache.setdefault(tag, (False, allocate_lock()))
        # Common case: we got (True, result), so we return the result.
        if x[0]:
            return x[1]
        # Else, it's a lock.  Acquire it to serialize the following tests.
        with x[1]:
            # Read again from _init_once_cache the current status.
            # Call the function and store the result back.
            result = func()
            self._init_once_cache[tag] = (True, result)
    def embedding_init_code(self, pysource):
        if self._embedding:
            raise ValueError("embedding_init_code() can only be called once")
        # fix 'pysource' before it gets dumped into the C file:
        # - remove empty lines at the beginning, so it starts at "line 1"
        # - dedent, if all non-empty lines are indented
        # - check for SyntaxErrors
        match = re.match(r'\s*\n', pysource)
            pysource = pysource[match.end():]
        lines = pysource.splitlines() or ['']
        prefix = re.match(r'\s*', lines[0]).group()
        for i in range(1, len(lines)):
            line = lines[i]
            if line.rstrip():
                while not line.startswith(prefix):
                    prefix = prefix[:-1]
        lines = [line[i:]+'\n' for line in lines]
        pysource = ''.join(lines)
        compile(pysource, "cffi_init", "exec")
        self._embedding = pysource
    def def_extern(self, *args, **kwds):
        raise ValueError("ffi.def_extern() is only available on API-mode FFI "
                         "objects")
    def list_types(self):
        """Returns the user type names known to this FFI instance.
        This returns a tuple containing three lists of names:
        (typedef_names, names_of_structs, names_of_unions)
        typedefs = []
        structs = []
        unions = []
        for key in self._parser._declarations:
            if key.startswith('typedef '):
                typedefs.append(key[8:])
            elif key.startswith('struct '):
                structs.append(key[7:])
            elif key.startswith('union '):
                unions.append(key[6:])
        typedefs.sort()
        structs.sort()
        unions.sort()
        return (typedefs, structs, unions)
def _load_backend_lib(backend, name, flags):
    if not isinstance(name, basestring):
        if sys.platform != "win32" or name is not None:
            return backend.load_library(name, flags)
        name = "c"    # Windows: load_library(None) fails, but this works
                      # on Python 2 (backward compatibility hack only)
    first_error = None
    if '.' in name or '/' in name or os.sep in name:
            first_error = e
    path = ctypes.util.find_library(name)
        if name == "c" and sys.platform == "win32" and sys.version_info >= (3,):
            raise OSError("dlopen(None) cannot work on Windows for Python 3 "
                          "(see http://bugs.python.org/issue23606)")
        msg = ("ctypes.util.find_library() did not manage "
               "to locate a library called %r" % (name,))
        if first_error is not None:
            msg = "%s.  Additionally, %s" % (first_error, msg)
        raise OSError(msg)
    return backend.load_library(path, flags)
def _make_ffi_library(ffi, libname, flags):
    backend = ffi._backend
    backendlib = _load_backend_lib(backend, libname, flags)
    def accessor_function(name):
        key = 'function ' + name
        tp, _ = ffi._parser._declarations[key]
        BType = ffi._get_cached_btype(tp)
        value = backendlib.load_function(BType, name)
        library.__dict__[name] = value
    def accessor_variable(name):
        key = 'variable ' + name
        read_variable = backendlib.read_variable
        write_variable = backendlib.write_variable
        setattr(FFILibrary, name, property(
            lambda self: read_variable(BType, name),
            lambda self, value: write_variable(BType, name, value)))
    def addressof_var(name):
            return addr_variables[name]
            with ffi._lock:
                if name not in addr_variables:
                    if BType.kind != 'array':
                        BType = model.pointer_cache(ffi, BType)
                    p = backendlib.load_function(BType, name)
                    addr_variables[name] = p
    def accessor_constant(name):
        raise NotImplementedError("non-integer constant '%s' cannot be "
                                  "accessed from a dlopen() library" % (name,))
    def accessor_int_constant(name):
        library.__dict__[name] = ffi._parser._int_constants[name]
    accessors = {}
    accessors_version = [False]
    addr_variables = {}
    def update_accessors():
        if accessors_version[0] is ffi._cdef_version:
        for key, (tp, _) in ffi._parser._declarations.items():
            if not isinstance(tp, model.EnumType):
                tag, name = key.split(' ', 1)
                if tag == 'function':
                    accessors[name] = accessor_function
                elif tag == 'variable':
                    accessors[name] = accessor_variable
                elif tag == 'constant':
                    accessors[name] = accessor_constant
                for i, enumname in enumerate(tp.enumerators):
                    def accessor_enum(name, tp=tp, i=i):
                        tp.check_not_partial()
                        library.__dict__[name] = tp.enumvalues[i]
                    accessors[enumname] = accessor_enum
        for name in ffi._parser._int_constants:
            accessors.setdefault(name, accessor_int_constant)
        accessors_version[0] = ffi._cdef_version
    def make_accessor(name):
            if name in library.__dict__ or name in FFILibrary.__dict__:
                return    # added by another thread while waiting for the lock
            if name not in accessors:
                update_accessors()
            accessors[name](name)
    class FFILibrary(object):
            make_accessor(name)
                property = getattr(self.__class__, name)
                property.__set__(self, value)
                return accessors.keys()
        def __addressof__(self, name):
            if name in library.__dict__:
                return library.__dict__[name]
            if name in FFILibrary.__dict__:
                return addressof_var(name)
            raise AttributeError("cffi library has no function or "
                                 "global variable named '%s'" % (name,))
        def __cffi_close__(self):
            backendlib.close_lib()
    if isinstance(libname, basestring):
            if not isinstance(libname, str):    # unicode, on Python 2
                libname = libname.encode('utf-8')
            FFILibrary.__name__ = 'FFILibrary_%s' % libname
    library = FFILibrary()
    return library, library.__dict__
def _builtin_function_type(func):
    # a hack to make at least ffi.typeof(builtin_function) work,
    # if the builtin function was obtained by 'vengine_cpy'.
        module = sys.modules[func.__module__]
        ffi = module._cffi_original_ffi
        types_of_builtin_funcs = module._cffi_types_of_builtin_funcs
        tp = types_of_builtin_funcs[func]
    except (KeyError, AttributeError, TypeError):
            return ffi._get_cached_btype(tp)
# duplicate -> https://github.com/confident-ai/deepeval/blob/main/deepeval/confident/api.py
DEEPEVAL_BASE_URL = "https://deepeval.confident-ai.com"
DEEPEVAL_BASE_URL_EU = "https://eu.deepeval.confident-ai.com"
API_BASE_URL = "https://api.confident-ai.com"
API_BASE_URL_EU = "https://eu.api.confident-ai.com"
retryable_exceptions = httpx.HTTPError
    HTTPHandler,
def log_retry_error(details):
    exception = details.get("exception")
    tries = details.get("tries")
        logging.error(f"Confident AI Error: {exception}. Retrying: {tries} time(s)...")
        logging.error(f"Retrying: {tries} time(s)...")
class HttpMethods(Enum):
    GET = "GET"
    POST = "POST"
    DELETE = "DELETE"
    PUT = "PUT"
class Endpoints(Enum):
    DATASET_ENDPOINT = "/v1/dataset"
    TEST_RUN_ENDPOINT = "/v1/test-run"
    TRACING_ENDPOINT = "/v1/tracing"
    EVENT_ENDPOINT = "/v1/event"
    FEEDBACK_ENDPOINT = "/v1/feedback"
    PROMPT_ENDPOINT = "/v1/prompt"
    RECOMMEND_ENDPOINT = "/v1/recommend-metrics"
    EVALUATE_ENDPOINT = "/evaluate"
    GUARD_ENDPOINT = "/guard"
    GUARDRAILS_ENDPOINT = "/guardrails"
    BASELINE_ATTACKS_ENDPOINT = "/generate-baseline-attacks"
class Api:
    def __init__(self, api_key: str, base_url=None):
        self._headers = {
            # "User-Agent": "Python/Requests",
            "CONFIDENT_API_KEY": api_key,
        # using the global non-eu variable for base url
        self.base_api_url = base_url or API_BASE_URL
        self.sync_http_handler = HTTPHandler()
        self.async_http_handler = get_async_httpx_client(
            llm_provider=httpxSpecialProvider.LoggingCallback
    def _http_request(
        self, method: str, url: str, headers=None, json=None, params=None
        if method != "POST":
            raise Exception("Only POST requests are supported")
            self.sync_http_handler.post(
            raise Exception(f"DeepEval logging error: {e.response.text}")
    def send_request(
        self, method: HttpMethods, endpoint: Endpoints, body=None, params=None
        url = f"{self.base_api_url}{endpoint.value}"
        res = self._http_request(
            method=method.value,
            headers=self._headers,
            json=body,
        if res.status_code == 200:
                return res.json()
                return res.text
            verbose_logger.debug(res.json())
            raise Exception(res.json().get("error", res.text))
    async def a_send_request(
        if method != HttpMethods.POST:
            await self.async_http_handler.post(
"""Public API for Opik payload building."""
from typing import Any, Dict, Optional, Tuple
from litellm.integrations.opik import utils
from . import extractors, payload_builders, types
def build_opik_payload(
    response_obj: Dict[str, Any],
    start_time: datetime,
    end_time: datetime,
) -> Tuple[Optional[types.TracePayload], types.SpanPayload]:
    Build Opik trace and span payloads from LiteLLM completion data.
    This is the main public API for creating Opik payloads. It:
    1. Extracts all necessary data from LiteLLM kwargs and response
    2. Decides whether to create a new trace or attach to existing
    3. Builds trace payload (if new trace)
    4. Builds span payload (always)
        kwargs: LiteLLM kwargs containing request metadata and logging data
        response_obj: LiteLLM response object containing model response
        start_time: Request start time
        end_time: Request end time
        project_name: Default Opik project name
        Tuple of (optional trace payload, span payload)
        - First element is TracePayload if creating a new trace, None if attaching to existing
        - Second element is always SpanPayload
    standard_logging_object = kwargs["standard_logging_object"]
    # Extract litellm params and metadata
    litellm_metadata = litellm_params.get("metadata", {}) or {}
    standard_logging_metadata = standard_logging_object.get("metadata", {}) or {}
    # Extract and merge Opik metadata
    opik_metadata = extractors.extract_opik_metadata(
        litellm_metadata, standard_logging_metadata
    # Extract project name
    current_project_name = opik_metadata.get("project_name", project_name)
    # Extract trace identifiers
    current_span_data = opik_metadata.get("current_span_data")
    trace_id, parent_span_id = extractors.extract_span_identifiers(current_span_data)
    # Extract tags and thread_id
    tags = extractors.extract_tags(opik_metadata, kwargs.get("custom_llm_provider"))
    thread_id = opik_metadata.get("thread_id")
    # Apply proxy header overrides
    proxy_request = litellm_params.get("proxy_server_request", {}) or {}
    proxy_headers = proxy_request.get("headers", {}) or {}
    current_project_name, tags, thread_id = extractors.apply_proxy_header_overrides(
        current_project_name, tags, thread_id, proxy_headers
    # Build shared metadata
    metadata = extractors.extract_and_build_metadata(
        opik_metadata=opik_metadata,
        standard_logging_metadata=standard_logging_metadata,
        standard_logging_object=standard_logging_object,
        litellm_kwargs=kwargs,
    # Get input/output data
    input_data = standard_logging_object.get("messages", {})
    output_data = standard_logging_object.get("response", {})
    # Decide whether to create a new trace or attach to existing
    trace_payload: Optional[types.TracePayload] = None
    if trace_id is None:
        trace_id = utils.create_uuid7()
        trace_payload = payload_builders.build_trace_payload(
            project_name=current_project_name,
            trace_id=trace_id,
            input_data=input_data,
            output_data=output_data,
    # Always create a span
    usage = utils.create_usage_object(response_obj["usage"])
    # Extract provider and cost
    provider = extractors.normalize_provider_name(kwargs.get("custom_llm_provider"))
    cost = kwargs.get("response_cost")
    span_payload = payload_builders.build_span_payload(
        parent_span_id=parent_span_id,
        cost=cost,
    return trace_payload, span_payload
