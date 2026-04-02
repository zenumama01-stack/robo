from collections import namedtuple
    from functools import cache
except ImportError:  # Remove when support for Python 3.8 is dropped
    from functools import lru_cache
    cache = lru_cache(maxsize=None)
from tweepy.direct_message_event import DirectMessageEvent
    TwitterServerError, Unauthorized
from tweepy.list import List
from tweepy.media import Media
from tweepy.place import Place
from tweepy.poll import Poll
from tweepy.space import Space
from tweepy.tweet import Tweet
from tweepy.user import User
Response = namedtuple("Response", ("data", "includes", "errors", "meta"))
class BaseClient:
        self, bearer_token=None, consumer_key=None, consumer_secret=None,
        access_token=None, access_token_secret=None, *, return_type=Response,
        self.return_type = return_type
        self.user_agent = (
    def request(self, method, route, params=None, json=None, user_auth=False):
        host = "https://api.twitter.com"
        headers = {"User-Agent": self.user_agent}
        if user_auth:
                self.consumer_key, self.consumer_secret,
                self.access_token, self.access_token_secret
            auth = auth.apply_auth()
            headers["Authorization"] = f"Bearer {self.bearer_token}"
        log.debug(
            f"Making API request: {method} {host + route}\n"
            f"Parameters: {params}\n"
            f"Headers: {headers}\n"
            f"Body: {json}"
        with self.session.request(
            method, host + route, params=params, json=json, headers=headers,
            auth=auth
        ) as response:
                "Received API response: "
                f"{response.status_code} {response.reason}\n"
                f"Headers: {response.headers}\n"
                f"Content: {response.content}"
            if response.status_code == 400:
                raise BadRequest(response)
            if response.status_code == 401:
                raise Unauthorized(response)
            if response.status_code == 403:
                raise Forbidden(response)
            if response.status_code == 404:
                raise NotFound(response)
            if response.status_code == 429:
                if "x-rate-limit-reset" in response.headers:
                    reset_time = int(response.headers["x-rate-limit-reset"])
                if self.wait_on_rate_limit:
                        sleep_time = reset_time - int(time.time()) + 1
                            log.warning(
                                "Rate limit exceeded. "
                                f"Sleeping for {sleep_time} seconds."
                            time.sleep(sleep_time)
                    return self.request(method, route, params, json, user_auth)
                    raise TooManyRequests(response, reset_time=reset_time)
            if response.status_code >= 500:
                raise TwitterServerError(response)
            if not 200 <= response.status_code < 300:
                raise HTTPException(response)
            return response
    def _make_request(
        self, method, route, params={}, endpoint_parameters=(), json=None,
        data_type=None, user_auth=False
        request_params = self._process_params(params, endpoint_parameters)
        response = self.request(method, route, params=request_params,
                                json=json, user_auth=user_auth)
        if self.return_type is requests.Response:
        response = response.json()
        if self.return_type is dict:
        return self._construct_response(response, data_type=data_type)
    def _construct_response(self, response, data_type=None):
        data = response.get("data")
        data = self._process_data(data, data_type=data_type)
        includes = response.get("includes", {})
        includes = self._process_includes(includes)
        errors = response.get("errors", [])
        meta = response.get("meta", {})
        return Response(data, includes, errors, meta)
    def _process_data(self, data, data_type=None):
        if data_type is not None:
            if isinstance(data, list):
                data = [data_type(result) for result in data]
            elif data is not None:
                data = data_type(data)
        return data
    def _process_includes(self, includes):
        if "media" in includes:
            includes["media"] = [Media(media) for media in includes["media"]]
        if "places" in includes:
            includes["places"] = [Place(place) for place in includes["places"]]
        if "polls" in includes:
            includes["polls"] = [Poll(poll) for poll in includes["polls"]]
        if "tweets" in includes:
            includes["tweets"] = [Tweet(tweet) for tweet in includes["tweets"]]
        if "users" in includes:
            includes["users"] = [User(user) for user in includes["users"]]
        return includes
    def _process_params(self, params, endpoint_parameters):
        endpoint_parameters = {
            endpoint_parameter.replace('.', '_'): endpoint_parameter
            for endpoint_parameter in endpoint_parameters
        request_params = {}
        for param_name, param_value in params.items():
                param_name = endpoint_parameters[param_name]
                log.warn(f"Unexpected parameter: {param_name}")
            if isinstance(param_value, list):
                request_params[param_name] = ','.join(map(str, param_value))
            elif isinstance(param_value, datetime.datetime):
                if param_value.tzinfo is not None:
                    param_value = param_value.astimezone(datetime.timezone.utc)
                request_params[param_name] = param_value.strftime(
                    "%Y-%m-%dT%H:%M:%SZ"
                # TODO: Constant datetime format string?
            elif param_value is not None:
                request_params[param_name] = param_value
        return request_params
class Client(BaseClient):
    """Client( \
        bearer_token=None, consumer_key=None, consumer_secret=None, \
        access_token=None, access_token_secret=None, *, return_type=Response, \
        wait_on_rate_limit=False \
    Twitter API v2 Client
    .. versionadded:: 4.0
    .. versionchanged:: 4.15
        Removed ``block`` and ``unblock`` methods, as the endpoints they use
        have been removed
    bearer_token : str | None
        Twitter API OAuth 2.0 Bearer Token / Access Token
    consumer_key : str | None
        Twitter API OAuth 1.0a Consumer Key
    consumer_secret : str | None
        Twitter API OAuth 1.0a Consumer Secret
    access_token : str | None
        Twitter API OAuth 1.0a Access Token
    access_token_secret : str | None
        Twitter API OAuth 1.0a Access Token Secret
    return_type : type[dict | requests.Response | Response]
        Type to return from requests to the API
    wait_on_rate_limit : bool
        Whether to wait when rate limit is reached
    session : requests.Session
        Requests Session used to make requests to the API
    user_agent : str
        User agent used when making requests to the API
    def _get_authenticating_user_id(self, *, oauth_1=False):
        if oauth_1:
            if self.access_token is None:
                    "Access Token must be provided for OAuth 1.0a User Context"
                return self._get_oauth_1_authenticating_user_id(
                    self.access_token
            if self.bearer_token is None:
                    "Access Token must be provided for "
                    "OAuth 2.0 Authorization Code Flow with PKCE"
                return self._get_oauth_2_authenticating_user_id(
                    self.bearer_token
    @cache
    def _get_oauth_1_authenticating_user_id(self, access_token):
        return access_token.partition('-')[0]
    def _get_oauth_2_authenticating_user_id(self, access_token):
        original_access_token = self.bearer_token
        original_return_type = self.return_type
        self.bearer_token = access_token
        self.return_type = dict
        user_id = self.get_me(user_auth=False)["data"]["id"]
        self.bearer_token = original_access_token
        self.return_type = original_return_type
        return user_id
    # Bookmarks
    def remove_bookmark(self, tweet_id):
        """Allows a user or authenticated user ID to remove a Bookmark of a
            A request is made beforehand to Twitter's API to determine the
            authenticating user's ID. This is cached and only done once per
            :class:`Client` instance for each access token used.
        .. versionadded:: 4.8
        tweet_id : int | str
            The ID of the Tweet that you would like the ``id`` to remove a
            Bookmark of.
            If the access token isn't set
        dict | requests.Response | Response
        https://developer.twitter.com/en/docs/twitter-api/tweets/bookmarks/api-reference/delete-users-id-bookmarks-tweet_id
        id = self._get_authenticating_user_id()
        route = f"/2/users/{id}/bookmarks/{tweet_id}"
        return self._make_request(
            "DELETE", route
    def get_bookmarks(self, **params):
        """get_bookmarks( \
            *, expansions=None, max_results=None, media_fields=None, \
            pagination_token=None, place_fields=None, poll_fields=None, \
            tweet_fields=None, user_fields=None \
        Allows you to get an authenticated user's 800 most recent bookmarked
        Tweets.
        expansions : list[str] | str | None
            :ref:`expansions_parameter`
        max_results : int | None
            The maximum number of results to be returned per page. This can be
            a number between 1 and 100. By default, each page will return 100
            results.
        media_fields : list[str] | str | None
            :ref:`media_fields_parameter`
        pagination_token : str | None
            Used to request the next page of results if all results weren't
            returned with the latest request, or to go back to the previous
            page of results. To return the next page, pass the ``next_token``
            returned in your previous response. To go back one page, pass the
            ``previous_token`` returned in your previous response.
        place_fields : list[str] | str | None
            :ref:`place_fields_parameter`
        poll_fields : list[str] | str | None
            :ref:`poll_fields_parameter`
        tweet_fields : list[str] | str | None
            :ref:`tweet_fields_parameter`
        user_fields : list[str] | str | None
            :ref:`user_fields_parameter`
        https://developer.twitter.com/en/docs/twitter-api/tweets/bookmarks/api-reference/get-users-id-bookmarks
        route = f"/2/users/{id}/bookmarks"
            "GET", route, params=params,
            endpoint_parameters=(
                "expansions", "max_results", "media.fields",
                "pagination_token", "place.fields", "poll.fields",
                "tweet.fields", "user.fields"
            ), data_type=Tweet
    def bookmark(self, tweet_id):
        """Causes the authenticating user to Bookmark the target Tweet provided
        in the request body.
            The ID of the Tweet that you would like the user ``id`` to
            Bookmark.
        https://developer.twitter.com/en/docs/twitter-api/tweets/bookmarks/api-reference/post-users-id-bookmarks
            "POST", route, json={"tweet_id": str(tweet_id)}
    # Hide replies
    def hide_reply(self, id, *, user_auth=True):
        """Hides a reply to a Tweet.
            Added ``user_auth`` parameter
        id : int | str
            Unique identifier of the Tweet to hide. The Tweet must belong to a
            conversation initiated by the authenticating user.
        user_auth : bool
            Whether or not to use OAuth 1.0a User Context to authenticate
        https://developer.twitter.com/en/docs/twitter-api/tweets/hide-replies/api-reference/put-tweets-id-hidden
            "PUT", f"/2/tweets/{id}/hidden", json={"hidden": True},
            user_auth=user_auth
    def unhide_reply(self, id, *, user_auth=True):
        """Unhides a reply to a Tweet.
            Unique identifier of the Tweet to unhide. The Tweet must belong to
            a conversation initiated by the authenticating user.
            "PUT", f"/2/tweets/{id}/hidden", json={"hidden": False},
    # Likes
    def unlike(self, tweet_id, *, user_auth=True):
        """Unlike a Tweet.
        The request succeeds with no action when the user sends a request to a
        user they're not liking the Tweet or have already unliked the Tweet.
            When using OAuth 2.0 Authorization Code Flow with PKCE with
            ``user_auth=False``, a request is made beforehand to Twitter's API
            to determine the authenticating user's ID. This is cached and only
            done once per :class:`Client` instance for each access token used.
        .. versionchanged:: 4.8
            Added support for using OAuth 2.0 Authorization Code Flow with PKCE
            Changed to raise :class:`TypeError` when the access token isn't set
            The ID of the Tweet that you would like to unlike.
        https://developer.twitter.com/en/docs/twitter-api/tweets/likes/api-reference/delete-users-id-likes-tweet_id
        id = self._get_authenticating_user_id(oauth_1=user_auth)
        route = f"/2/users/{id}/likes/{tweet_id}"
            "DELETE", route, user_auth=user_auth
    def get_liking_users(self, id, *, user_auth=False, **params):
        """get_liking_users( \
            id, *, expansions=None, max_results=None, media_fields=None, \
            tweet_fields=None, user_fields=None, user_auth=False \
        Allows you to get information about a Tweet's liking users.
        .. versionchanged:: 4.6
            Added ``max_results`` and ``pagination_token`` parameters
            Tweet ID of the Tweet to request liking users of.
        https://developer.twitter.com/en/docs/twitter-api/tweets/likes/api-reference/get-tweets-id-liking_users
            "GET", f"/2/tweets/{id}/liking_users", params=params,
            ), data_type=User, user_auth=user_auth
    def get_liked_tweets(self, id, *, user_auth=False, **params):
        """get_liked_tweets( \
        Allows you to get information about a user's liked Tweets.
        The Tweets returned by this endpoint count towards the Project-level
        `Tweet cap`_.
            User ID of the user to request liked Tweets for.
            a number between 5 and 100. By default, each page will return 100
        https://developer.twitter.com/en/docs/twitter-api/tweets/likes/api-reference/get-users-id-liked_tweets
        .. _Tweet cap: https://developer.twitter.com/en/docs/projects/overview#tweet-cap
            "GET", f"/2/users/{id}/liked_tweets", params=params,
            ), data_type=Tweet, user_auth=user_auth
    def like(self, tweet_id, *, user_auth=True):
        """Like a Tweet.
            The ID of the Tweet that you would like to Like.
        https://developer.twitter.com/en/docs/twitter-api/tweets/likes/api-reference/post-users-id-likes
        route = f"/2/users/{id}/likes"
            "POST", route, json={"tweet_id": str(tweet_id)},
    # Manage Tweets
    def delete_tweet(self, id, *, user_auth=True):
        """Allows an authenticated user ID to delete a Tweet.
        .. versionadded:: 4.3
            The Tweet ID you are deleting.
        https://developer.twitter.com/en/docs/twitter-api/tweets/manage-tweets/api-reference/delete-tweets-id
            "DELETE", f"/2/tweets/{id}", user_auth=user_auth
    def create_tweet(
        self, *, direct_message_deep_link=None, for_super_followers_only=None,
        place_id=None, media_ids=None, media_tagged_user_ids=None,
        poll_duration_minutes=None, poll_options=None, quote_tweet_id=None,
        exclude_reply_user_ids=None, in_reply_to_tweet_id=None,
        reply_settings=None, text=None, user_auth=True, community_id=None
        """Creates a Tweet on behalf of an authenticated user.
        direct_message_deep_link : str | None
            `Tweets a link directly to a Direct Message conversation`_ with an
            account.
        for_super_followers_only : bool | None
            Allows you to Tweet exclusively for `Super Followers`_.
        place_id : str | None
            Place ID being attached to the Tweet for geo location.
        media_ids : list[int | str] | None
            A list of Media IDs being attached to the Tweet. This is only
            required if the request includes the ``tagged_user_ids``.
        media_tagged_user_ids : list[int | str] | None
            A list of User IDs being tagged in the Tweet with Media. If the
            user you're tagging doesn't have photo-tagging enabled, their names
            won't show up in the list of tagged users even though the Tweet is
            successfully created.
        poll_duration_minutes : int | None
            Duration of the poll in minutes for a Tweet with a poll. This is
            only required if the request includes ``poll.options``.
        poll_options : list[str] | None
            A list of poll options for a Tweet with a poll.
        quote_tweet_id : int | str | None
            Link to the Tweet being quoted.
        exclude_reply_user_ids : list[int | str] | None
            A list of User IDs to be excluded from the reply Tweet thus
            removing a user from a thread.
        in_reply_to_tweet_id : int | str | None
            Tweet ID of the Tweet being replied to. Please note that
            ``in_reply_to_tweet_id`` needs to be in the request if
            ``exclude_reply_user_ids`` is present.
        reply_settings : str | None
            `Settings`_ to indicate who can reply to the Tweet. Limited to
            "mentionedUsers" and "following". If the field isn’t specified, it
            will default to everyone.
        text : str | None
            Text of the Tweet being created. This field is required if
            ``media.media_ids`` is not present.
        community_id : int | str | None
            The ID of the Community to tweet in. The authenticated user must be a
            member of the Community.
        https://developer.twitter.com/en/docs/twitter-api/tweets/manage-tweets/api-reference/post-tweets
        .. _Tweets a link directly to a Direct Message conversation: https://business.twitter.com/en/help/campaign-editing-and-optimization/public-to-private-conversation.html
        .. _Super Followers: https://help.twitter.com/en/using-twitter/super-follows
        .. _Settings: https://blog.twitter.com/en_us/topics/product/2020/new-conversation-settings-coming-to-a-tweet-near-you
        json = {}
        if direct_message_deep_link is not None:
            json["direct_message_deep_link"] = direct_message_deep_link
        if community_id is not None:
            json["community_id"] = community_id
        if for_super_followers_only is not None:
            json["for_super_followers_only"] = for_super_followers_only
        if place_id is not None:
            json["geo"] = {"place_id": place_id}
        if media_ids is not None:
            json["media"] = {
                "media_ids": [str(media_id) for media_id in media_ids]
            if media_tagged_user_ids is not None:
                json["media"]["tagged_user_ids"] = [
                    str(media_tagged_user_id)
                    for media_tagged_user_id in media_tagged_user_ids
        if poll_options is not None:
            json["poll"] = {"options": poll_options}
            if poll_duration_minutes is not None:
                json["poll"]["duration_minutes"] = poll_duration_minutes
        if quote_tweet_id is not None:
            json["quote_tweet_id"] = str(quote_tweet_id)
        if in_reply_to_tweet_id is not None:
            json["reply"] = {"in_reply_to_tweet_id": str(in_reply_to_tweet_id)}
            if exclude_reply_user_ids is not None:
                json["reply"]["exclude_reply_user_ids"] = [
                    str(exclude_reply_user_id)
                    for exclude_reply_user_id in exclude_reply_user_ids
        if reply_settings is not None:
            json["reply_settings"] = reply_settings
        if text is not None:
            json["text"] = text
            "POST", f"/2/tweets", json=json, user_auth=user_auth
    # Quote Tweets
    def get_quote_tweets(self, id, *, user_auth=False, **params):
        """get_quote_tweets( \
            id, *, exclude=None, expansions=None, max_results=None, \
            media_fields=None, pagination_token=None, place_fields=None, \
            poll_fields=None, tweet_fields=None, user_fields=None, \
            user_auth=False \
        Returns Quote Tweets for a Tweet specified by the requested Tweet ID.
        .. versionadded:: 4.7
            Added ``exclude`` parameter
            Unique identifier of the Tweet to request.
        exclude : list[str] | str | None
            Comma-separated list of the types of Tweets to exclude from the
            response.
            Specifies the number of Tweets to try and retrieve, up to a maximum
            of 100 per distinct request. By default, 10 results are returned if
            this parameter is not supplied. The minimum permitted value is 10.
            It is possible to receive less than the ``max_results`` per request
            throughout the pagination process.
            This parameter is used to move forwards through 'pages' of results,
            based on the value of the ``next_token``. The value used with the
            parameter is pulled directly from the response provided by the API,
            and should not be modified.
        https://developer.twitter.com/en/docs/twitter-api/tweets/quote-tweets/api-reference/get-tweets-id-quote_tweets
            "GET", f"/2/tweets/{id}/quote_tweets", params=params,
                "exclude", "expansions", "max_results", "media.fields",
    # Retweets
    def unretweet(self, source_tweet_id, *, user_auth=True):
        """Allows an authenticated user ID to remove the Retweet of a Tweet.
        user they're not Retweeting the Tweet or have already removed the
        Retweet of.
        source_tweet_id : int | str
            The ID of the Tweet that you would like to remove the Retweet of.
        https://developer.twitter.com/en/docs/twitter-api/tweets/retweets/api-reference/delete-users-id-retweets-tweet_id
        route = f"/2/users/{id}/retweets/{source_tweet_id}"
    def get_retweeters(self, id, *, user_auth=False, **params):
        """get_retweeters( \
        Allows you to get information about who has Retweeted a Tweet.
            Tweet ID of the Tweet to request Retweeting users of.
        https://developer.twitter.com/en/docs/twitter-api/tweets/retweets/api-reference/get-tweets-id-retweeted_by
            "GET", f"/2/tweets/{id}/retweeted_by", params=params,
    def retweet(self, tweet_id, *, user_auth=True):
        """Causes the user ID to Retweet the target Tweet.
            The ID of the Tweet that you would like to Retweet.
        https://developer.twitter.com/en/docs/twitter-api/tweets/retweets/api-reference/post-users-id-retweets
        route = f"/2/users/{id}/retweets"
    def search_all_tweets(self, query, **params):
        """search_all_tweets( \
            query, *, end_time=None, expansions=None, max_results=None, \
            media_fields=None, next_token=None, place_fields=None, \
            poll_fields=None, since_id=None, sort_order=None, \
            start_time=None, tweet_fields=None, until_id=None, \
            user_fields=None \
        This endpoint is only available to those users who have been approved
        for the `Academic Research product track`_.
        The full-archive search endpoint returns the complete history of public
        Tweets matching a search query; since the first Tweet was created March
        26, 2006.
            By default, a request will return Tweets from up to 30 days ago if
            the ``start_time`` parameter is not provided.
            Added ``sort_order`` parameter
        query : str
            One query for matching Tweets. Up to 1024 characters.
        end_time : datetime.datetime | str | None
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). Used with ``start_time``.
            The newest, most recent UTC timestamp to which the Tweets will be
            provided. Timestamp is in second granularity and is exclusive (for
            example, 12:00:01 excludes the first second of the minute). If used
            without ``start_time``, Tweets from 30 days before ``end_time``
            will be returned by default. If not specified, ``end_time`` will
            default to [now - 30 seconds].
            The maximum number of search results to be returned by a request. A
            number between 10 and the system limit (currently 500). By default,
            a request response will return 10 results.
        next_token : str | None
            This parameter is used to get the next 'page' of results. The value
            used with the parameter is pulled directly from the response
            provided by the API, and should not be modified. You can learn more
            by visiting our page on `pagination`_.
        since_id : int | str | None
            Returns results with a Tweet ID greater than (for example, more
            recent than) the specified ID. The ID specified is exclusive and
            responses will not include it. If included with the same request as
            a ``start_time`` parameter, only ``since_id`` will be used.
        sort_order : str | None
            This parameter is used to specify the order in which you want the
            Tweets returned. By default, a request will return the most recent
            Tweets first (sorted by recency).
        start_time : datetime.datetime | str | None
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). The oldest UTC timestamp
            from which the Tweets will be provided. Timestamp is in second
            granularity and is inclusive (for example, 12:00:01 includes the
            first second of the minute). By default, a request will return
            Tweets from up to 30 days ago if you do not include this parameter.
        until_id : int | str | None
            Returns results with a Tweet ID less than (that is, older than) the
            specified ID. Used with ``since_id``. The ID specified is exclusive
            and responses will not include it.
        https://developer.twitter.com/en/docs/twitter-api/tweets/search/api-reference/get-tweets-search-all
        .. _Academic Research product track: https://developer.twitter.com/en/docs/projects/overview#product-track
        .. _pagination: https://developer.twitter.com/en/docs/twitter-api/tweets/search/integrate/paginate
        params["query"] = query
            "GET", "/2/tweets/search/all", params=params,
                "end_time", "expansions", "max_results", "media.fields",
                "next_token", "place.fields", "poll.fields", "query",
                "since_id", "sort_order", "start_time", "tweet.fields",
                "until_id", "user.fields"
    def search_recent_tweets(self, query, *, user_auth=False, **params):
        """search_recent_tweets( \
            user_fields=None, user_auth=False \
        The recent search endpoint returns Tweets from the last seven days that
        match a search query.
            One rule for matching Tweets. If you are using a
            `Standard Project`_ at the Basic `access level`_, you can use the
            basic set of `operators`_ and can make queries up to 512 characters
            long. If you are using an `Academic Research Project`_ at the Basic
            access level, you can use all available operators and can make
            queries up to 1,024 characters long.
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). The newest, most recent
            UTC timestamp to which the Tweets will be provided. Timestamp is in
            second granularity and is exclusive (for example, 12:00:01 excludes
            the first second of the minute). By default, a request will return
            Tweets from as recent as 30 seconds ago if you do not include this
            parameter.
            number between 10 and 100. By default, a request response will
            return 10 results.
            provided by the API, and should not be modified.
            Returns results with a Tweet ID greater than (that is, more recent
            than) the specified ID. The ID specified is exclusive and responses
            will not include it. If included with the same request as a
            ``start_time`` parameter, only ``since_id`` will be used.
            (from most recent seven days) from which the Tweets will be
            provided. Timestamp is in second granularity and is inclusive (for
            example, 12:00:01 includes the first second of the minute). If
            included with the same request as a ``since_id`` parameter, only
            ``since_id`` will be used. By default, a request will return Tweets
            from up to seven days ago if you do not include this parameter.
            specified ID. The ID specified is exclusive and responses will not
            include it.
        https://developer.twitter.com/en/docs/twitter-api/tweets/search/api-reference/get-tweets-search-recent
        .. _Standard Project: https://developer.twitter.com/en/docs/projects
        .. _access level: https://developer.twitter.com/en/products/twitter-api/early-access/guide.html#na_1
        .. _operators: https://developer.twitter.com/en/docs/twitter-api/tweets/search/integrate/build-a-query
        .. _Academic Research Project: https://developer.twitter.com/en/docs/projects
            "GET", "/2/tweets/search/recent", params=params,
    # Timelines
    def get_users_mentions(self, id, *, user_auth=False, **params):
        """get_users_mentions( \
            id, *, end_time=None, expansions=None, max_results=None, \
            poll_fields=None, since_id=None, start_time=None, \
            tweet_fields=None, until_id=None, user_fields=None, \
        Returns Tweets mentioning a single user specified by the requested user
        ID. By default, the most recent ten Tweets are returned per request.
        Using pagination, up to the most recent 800 Tweets can be retrieved.
            Unique identifier of the user for whom to return Tweets mentioning
            the user. User ID can be referenced using the `user/lookup`_
            endpoint. More information on Twitter IDs is `here`_.
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). The new UTC timestamp
            first second of the minute).
            Please note that this parameter does not support a millisecond
            this parameter is not supplied. The minimum permitted value is 5.
            This parameter is used to move forwards or backwards through
            'pages' of results, based on the value of the ``next_token`` or
            ``previous_token`` in the response. The value used with the
            than) the specified 'since' Tweet ID. There are limits to the
            number of Tweets that can be accessed through the API. If the limit
            of Tweets has occurred since the ``since_id``, the ``since_id``
            will be forced to the oldest ID available. More information on
            Twitter IDs is `here`_.
            Returns results with a Tweet ID less less than (that is, older
            than) the specified 'until' Tweet ID. There are limits to the
            of Tweets has occurred since the ``until_id``, the ``until_id``
            will be forced to the most recent ID available. More information on
        https://developer.twitter.com/en/docs/twitter-api/tweets/timelines/api-reference/get-users-id-mentions
        .. _user/lookup: https://developer.twitter.com/en/docs/twitter-api/users/lookup/introduction
        .. _here: https://developer.twitter.com/en/docs/twitter-ids
            "GET", f"/2/users/{id}/mentions", params=params,
                "pagination_token", "place.fields", "poll.fields", "since_id",
                "start_time", "tweet.fields", "until_id", "user.fields"
    def get_home_timeline(self, *, user_auth=True, **params):
        """get_home_timeline( \
            *, end_time=None, exclude=None, expansions=None, \
            max_results=None, media_fields=None, pagination_token=None, \
            place_fields=None, poll_fields=None, since_id=None, \
            user_fields=None, user_auth=True \
        Allows you to retrieve a collection of the most recent Tweets and
        Retweets posted by you and users you follow. This endpoint returns up
        to the last 3200 Tweets.
            of 100 per distinct request. By default, 100 results are returned
            if this parameter is not supplied. The minimum permitted value is
            1. It is possible to receive less than the ``max_results`` per
            request throughout the pagination process.
            number of Tweets that can be accessed through the API. If the
            limit of Tweets has occurred since the ``since_id``, the
            ``since_id`` will be forced to the oldest ID available. More
            information on Twitter IDs is `here`_.
            specified 'until' Tweet ID. There are limits to the number of
            Tweets that can be accessed through the API. If the limit of Tweets
            has occurred since the ``until_id``, the ``until_id`` will be
            forced to the most recent ID available. More information on Twitter
            IDs is `here`_.
        https://developer.twitter.com/en/docs/twitter-api/tweets/timelines/api-reference/get-users-id-reverse-chronological
        route = f"/2/users/{id}/timelines/reverse_chronological"
                "end_time", "exclude", "expansions", "max_results",
                "media.fields", "pagination_token", "place.fields",
                "poll.fields", "since_id", "start_time", "tweet.fields",
    def get_users_tweets(self, id, *, user_auth=False, **params):
        """get_users_tweets( \
            id, *, end_time=None, exclude=None, expansions=None, \
        Returns Tweets composed by a single user, specified by the requested
        user ID. By default, the most recent ten Tweets are returned per
        request. Using pagination, the most recent 3,200 Tweets can be
        retrieved.
            Unique identifier of the Twitter account (user ID) for whom to
            return results. User ID can be referenced using the `user/lookup`_
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). The newest or most recent
            UTC timestamp from which the Tweets will be provided. Only the 3200
            most recent Tweets are available. Timestamp is in second
            first second of the minute). Minimum allowable time is
            2010-11-06T00:00:01Z
            response. When ``exclude=retweets`` is used, the maximum historical
            Tweets returned is still 3200. When the ``exclude=replies``
            parameter is used for any value, only the most recent 800 Tweets
            are available.
            than) the specified 'since' Tweet ID. Only the 3200 most recent
            Tweets are available. The result will exclude the ``since_id``. If
            the limit of Tweets has occurred since the ``since_id``, the
            ``since_id`` will be forced to the oldest ID available.
            YYYY-MM-DDTHH:mm:ssZ (ISO 8601/RFC 3339). The oldest or earliest
            2010-11-06T00:00:00Z
            than) the specified 'until' Tweet ID. Only the 3200 most recent
            Tweets are available. The result will exclude the ``until_id``. If
            the limit of Tweets has occurred since the ``until_id``, the
            ``until_id`` will be forced to the most recent ID available.
        https://developer.twitter.com/en/docs/twitter-api/tweets/timelines/api-reference/get-users-id-tweets
            "GET", f"/2/users/{id}/tweets", params=params,
    # Tweet counts
    def get_all_tweets_count(self, query, **params):
        """get_all_tweets_count( \
            query, *, end_time=None, granularity=None, next_token=None, \
            since_id=None, start_time=None, until_id=None \
        granularity : str | None
            This is the granularity that you want the timeseries count data to
            be grouped by. You can request ``minute``, ``hour``, or ``day``
            granularity. The default granularity, if not specified is ``hour``.
        https://developer.twitter.com/en/docs/twitter-api/tweets/counts/api-reference/get-tweets-counts-all
            "GET", "/2/tweets/counts/all", params=params,
                "end_time", "granularity", "next_token", "query", "since_id",
                "start_time", "until_id"
    def get_recent_tweets_count(self, query, **params):
        """get_recent_tweets_count( \
            query, *, end_time=None, granularity=None, since_id=None, \
            start_time=None, until_id=None \
        The recent Tweet counts endpoint returns count of Tweets from the last
        seven days that match a search query.
        https://developer.twitter.com/en/docs/twitter-api/tweets/counts/api-reference/get-tweets-counts-recent
            "GET", "/2/tweets/counts/recent", params=params,
                "end_time", "granularity", "query", "since_id", "start_time",
                "until_id"
    # Tweet lookup
    def get_tweet(self, id, *, user_auth=False, **params):
        """get_tweet( \
            id, *, expansions=None, media_fields=None, place_fields=None, \
        Returns a variety of information about a single Tweet specified by
        the requested ID.
            Unique identifier of the Tweet to request
        https://developer.twitter.com/en/docs/twitter-api/tweets/lookup/api-reference/get-tweets-id
            "GET", f"/2/tweets/{id}", params=params,
                "expansions", "media.fields", "place.fields", "poll.fields",
    def get_tweets(self, ids, *, user_auth=False, **params):
        """get_tweets( \
            ids, *, expansions=None, media_fields=None, place_fields=None, \
        Returns a variety of information about the Tweet specified by the
        requested ID or list of IDs.
        ids : list[int | str] | str
            A comma separated list of Tweet IDs. Up to 100 are allowed in a
            single request. Make sure to not include a space between commas and
            fields.
        https://developer.twitter.com/en/docs/twitter-api/tweets/lookup/api-reference/get-tweets
        params["ids"] = ids
            "GET", "/2/tweets", params=params,
                "ids", "expansions", "media.fields", "place.fields",
                "poll.fields", "tweet.fields", "user.fields"
    # Blocks
    def get_blocked(self, *, user_auth=True, **params):
        """get_blocked( \
            *, expansions=None, max_results=None, pagination_token=None, \
            tweet_fields=None, user_fields=None, user_auth=True \
        Returns a list of users who are blocked by the authenticating user.
            a number between 1 and 1000. By default, each page will return 100
            page of results.
        https://developer.twitter.com/en/docs/twitter-api/users/blocks/api-reference/get-users-blocking
        route = f"/2/users/{id}/blocking"
                "expansions", "max_results", "pagination_token",
    # Follows
    def unfollow_user(self, target_user_id, *, user_auth=True):
        """Allows a user ID to unfollow another user.
        The request succeeds with no action when the authenticated user sends a
        request to a user they're not following or have already unfollowed.
        .. versionchanged:: 4.2
            Renamed from :meth:`Client.unfollow`
        target_user_id : int | str
            The user ID of the user that you would like to unfollow.
        https://developer.twitter.com/en/docs/twitter-api/users/follows/api-reference/delete-users-source_id-following
        source_user_id = self._get_authenticating_user_id(oauth_1=user_auth)
        route = f"/2/users/{source_user_id}/following/{target_user_id}"
    def unfollow(self, target_user_id, *, user_auth=True):
        """Alias for :meth:`Client.unfollow_user`
        .. deprecated:: 4.2
            Use :meth:`Client.unfollow_user` instead.
            "Client.unfollow is deprecated; use Client.unfollow_user instead.",
        return self.unfollow_user(target_user_id, user_auth=user_auth)
    def get_users_followers(self, id, *, user_auth=False, **params):
        """get_users_followers( \
            id, *, expansions=None, max_results=None, pagination_token=None, \
        Returns a list of users who are followers of the specified user ID.
            The Twitter API endpoint that this method uses has been removed
            from the Basic and Pro tiers [#changelog]_.
            The user ID whose followers you would like to retrieve.
            a number between 1 and the 1000. By default, each page will return
            100 results.
        https://developer.twitter.com/en/docs/twitter-api/users/follows/api-reference/get-users-id-followers
            "GET", f"/2/users/{id}/followers", params=params,
            data_type=User, user_auth=user_auth
    def get_users_following(self, id, *, user_auth=False, **params):
        """get_users_following( \
        Returns a list of users the specified user ID is following.
            The user ID whose following you would like to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/users/follows/api-reference/get-users-id-following
            "GET", f"/2/users/{id}/following", params=params,
    def follow_user(self, target_user_id, *, user_auth=True):
        """Allows a user ID to follow another user.
        If the target user does not have public Tweets, this endpoint will send
        a follow request.
        request to a user they're already following, or if they're sending a
        follower request to a user that does not have public Tweets.
            Renamed from :meth:`Client.follow`
            The user ID of the user that you would like to follow.
        https://developer.twitter.com/en/docs/twitter-api/users/follows/api-reference/post-users-source_user_id-following
        route = f"/2/users/{source_user_id}/following"
            "POST", route, json={"target_user_id": str(target_user_id)},
    def follow(self, target_user_id, *, user_auth=True):
        """Alias for :meth:`Client.follow_user`
            Use :meth:`Client.follow_user` instead.
            "Client.follow is deprecated; use Client.follow_user instead.",
        return self.follow_user(target_user_id, user_auth=user_auth)
    # Mutes
    def unmute(self, target_user_id, *, user_auth=True):
        """Allows an authenticated user ID to unmute the target user.
        user they're not muting or have already unmuted.
            The user ID of the user that you would like to unmute.
        https://developer.twitter.com/en/docs/twitter-api/users/mutes/api-reference/delete-users-user_id-muting
        route = f"/2/users/{source_user_id}/muting/{target_user_id}"
    def get_muted(self, *, user_auth=True, **params):
        """get_muted( \
        Returns a list of users who are muted by the authenticating user.
        .. versionadded:: 4.1
        https://developer.twitter.com/en/docs/twitter-api/users/mutes/api-reference/get-users-muting
        route = f"/2/users/{id}/muting"
    def mute(self, target_user_id, *, user_auth=True):
        """Allows an authenticated user ID to mute the target user.
            The user ID of the user that you would like to mute.
        https://developer.twitter.com/en/docs/twitter-api/users/mutes/api-reference/post-users-user_id-muting
    # User lookup
    def get_user(self, *, id=None, username=None, user_auth=False, **params):
        """get_user(*, id=None, username=None, expansions=None, \
                    tweet_fields=None, user_fields=None, user_auth=False)
        Returns a variety of information about a single user specified by the
        requested ID or username.
        id : int | str | None
            The ID of the user to lookup.
        username : str | None
            The Twitter username (handle) of the user.
            If ID and username are not passed or both are passed
        https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-id
        https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-by-username-username
        if id is not None and username is not None:
            raise TypeError("Expected ID or username, not both")
        route = "/2/users"
        if id is not None:
            route += f"/{id}"
        elif username is not None:
            route += f"/by/username/{username}"
            raise TypeError("ID or username is required")
            endpoint_parameters=("expansions", "tweet.fields", "user.fields"),
    def get_users(self, *, ids=None, usernames=None, user_auth=False,
                  **params):
        """get_users(*, ids=None, usernames=None, expansions=None, \
        Returns a variety of information about one or more users specified by
        the requested IDs or usernames.
        ids : list[int | str] | str | None
            A comma separated list of user IDs. Up to 100 are allowed in a
        usernames : list[str] | str | None
            A comma separated list of Twitter usernames (handles). Up to 100
            are allowed in a single request. Make sure to not include a space
            between commas and fields.
            If IDs and usernames are not passed or both are passed
        https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users
        https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-by
        if ids is not None and usernames is not None:
            raise TypeError("Expected IDs or usernames, not both")
        if ids is not None:
        elif usernames is not None:
            route += "/by"
            params["usernames"] = usernames
            raise TypeError("IDs or usernames are required")
                "ids", "usernames", "expansions", "tweet.fields", "user.fields"
    def get_me(self, *, user_auth=True, **params):
        """get_me(*, expansions=None, tweet_fields=None, user_fields=None, \
                  user_auth=True)
        Returns information about an authorized user.
        https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-me
            "GET", f"/2/users/me", params=params,
    # Search Spaces
    def search_spaces(self, query, **params):
        """search_spaces(query, *, expansions=None, max_results=None, \
                         space_fields=None, state=None, user_fields=None)
        Return live or scheduled Spaces matching your specified search terms
            ``state`` is now an optional parameter.
            Your search term. This can be any text (including mentions and
            Hashtags) present in the title of the Space.
            The maximum number of results to return in this request. Specify a
            value between 1 and 100.
        space_fields : list[str] | str | None
            :ref:`space_fields_parameter`
        state : str | None
            Determines the type of results to return. This endpoint returns all
            Spaces by default. Use ``live`` to only return live Spaces or
            ``scheduled`` to only return upcoming Spaces.
        https://developer.twitter.com/en/docs/twitter-api/spaces/search/api-reference/get-spaces-search
            "GET", "/2/spaces/search", params=params,
                "query", "expansions", "max_results", "space.fields", "state",
                "user.fields"
            ), data_type=Space
    # Spaces lookup
    def get_spaces(self, *, ids=None, user_ids=None, **params):
        """get_spaces(*, ids=None, user_ids=None, expansions=None, \
                      space_fields=None, user_fields=None)
        Returns details about multiple live or scheduled Spaces (created by the
        specified user IDs if specified). Up to 100 comma-separated Space or
        user IDs can be looked up using this endpoint.
        ids : list[str] | str | None
            A comma separated list of Spaces (up to 100).
        user_ids : list[int | str] | str | None
            A comma separated list of user IDs (up to 100).
            If IDs and user IDs are not passed or both are passed
        https://developer.twitter.com/en/docs/twitter-api/spaces/lookup/api-reference/get-spaces
        https://developer.twitter.com/en/docs/twitter-api/spaces/lookup/api-reference/get-spaces-by-creator-ids
        if ids is not None and user_ids is not None:
            raise TypeError("Expected IDs or user IDs, not both")
        route = "/2/spaces"
        elif user_ids is not None:
            route += "/by/creator_ids"
            params["user_ids"] = user_ids
            raise TypeError("IDs or user IDs are required")
                "ids", "user_ids", "expansions", "space.fields", "user.fields"
    def get_space(self, id, **params):
        """get_space(id, *, expansions=None, space_fields=None, \
                     user_fields=None)
        Returns a variety of information about a single Space specified by the
        requested ID.
        id : list[str] | str
            Unique identifier of the Space to request.
        https://developer.twitter.com/en/docs/twitter-api/spaces/lookup/api-reference/get-spaces-id
            "GET", f"/2/spaces/{id}", params=params,
                "expansions", "space.fields", "user.fields"
    def get_space_buyers(self, id, **params):
        """get_space_buyers( \
            poll_fields=None, tweet_fields=None, user_fields=None \
        Returns a list of user who purchased a ticket to the requested Space.
        You must authenticate the request using the Access Token of the creator
        of the requested Space.
        .. versionadded:: 4.4
        id : str
            Unique identifier of the Space for which you want to request
        https://developer.twitter.com/en/docs/twitter-api/spaces/lookup/api-reference/get-spaces-id-buyers
            "GET", f"/2/spaces/{id}/buyers", params=params,
            ), data_type=User
    def get_space_tweets(self, id, **params):
        """get_space_tweets( \
        Returns Tweets shared in the requested Spaces.
        .. versionadded:: 4.6
            Unique identifier of the Space containing the Tweets you'd like to
            access.
        https://developer.twitter.com/en/docs/twitter-api/spaces/lookup/api-reference/get-spaces-id-tweets
            "GET", f"/2/spaces/{id}/tweets", params=params,
    # Direct Messages lookup
    def get_direct_message_events(
        self, *, dm_conversation_id=None, participant_id=None, user_auth=True,
        **params
        """get_direct_message_events( \
            *, dm_conversation_id=None, participant_id=None, \
            dm_event_fields=None, event_types=None, expansions=None, \
        If ``dm_conversation_id`` is passed, returns a list of Direct Messages
        within the conversation specified. Messages are returned in reverse
        chronological order.
        If ``participant_id`` is passed, returns a list of Direct Messages (DM)
        events within a 1-1 conversation with the user specified. Messages are
        returned in reverse chronological order.
        If neither is passed, returns a list of Direct Messages for the
        authenticated user, both sent and received. Direct Message events are
        returned in reverse chronological order. Supports retrieving events
        from the previous 30 days.
            There is an alias for this method named ``get_dm_events``.
        .. versionadded:: 4.12
        dm_conversation_id : str | None
            The ``id`` of the Direct Message conversation for which events are
            being retrieved.
        participant_id : int | str | None
            The ``participant_id`` of the user that the authenticating user is
            having a 1-1 conversation with.
        dm_event_fields : list[str] | str | None
            Extra fields to include in the event payload. ``id`` and
            ``event_type`` are returned by default. The ``text`` value isn't
            included for ``ParticipantsJoin`` and ``ParticipantsLeave`` events.
        event_types : str
            The type of Direct Message event to return. If not included, all
            types are returned.
            The maximum number of results to be returned in a page. Must be
            between 1 and 100. The default is 100.
            Contains either the ``next_token`` or ``previous_token`` value.
            If both ``dm_conversation_id`` and ``participant_id`` are passed
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/lookup/api-reference/get-dm_events
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/lookup/api-reference/get-dm_conversations-with-participant_id-dm_events
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/lookup/api-reference/get-dm_conversations-dm_conversation_id-dm_events
        if dm_conversation_id is not None and participant_id is not None:
                "Expected DM conversation ID or participant ID, not both"
        elif dm_conversation_id is not None:
            path = f"/2/dm_conversations/{dm_conversation_id}/dm_events"
        elif participant_id is not None:
            path = f"/2/dm_conversations/with/{participant_id}/dm_events"
            path = "/2/dm_events"
            "GET", path, params=params,
                "dm_event.fields", "event_types", "expansions", "max_results",
                "media.fields", "pagination_token", "tweet.fields",
            ), data_type=DirectMessageEvent, user_auth=user_auth
    get_dm_events = get_direct_message_events
    # Manage Direct Messages
    def create_direct_message(
        self, *, dm_conversation_id=None, participant_id=None, media_id=None,
        text=None, user_auth=True
        """If ``dm_conversation_id`` is passed, creates a Direct Message on
        behalf of the authenticated user, and adds it to the specified
        conversation.
        If ``participant_id`` is passed, creates a one-to-one Direct Message
        and adds it to the one-to-one conversation. This method either creates
        a new one-to-one conversation or retrieves the current conversation and
        adds the Direct Message to it.
            There is an alias for this method named ``create_dm``.
            The ``dm_conversation_id`` of the conversation to add the Direct
            Message to. Supports both 1-1 and group conversations.
            The User ID of the account this one-to-one Direct Message is to be
            sent to.
        media_id : int | str | None
            A single Media ID being attached to the Direct Message. This field
            is required if ``text`` is not present. For this launch, only 1
            attachment is supported.
            Text of the Direct Message being created. This field is required if
            ``media_id`` is not present. Text messages support up to 10,000
            characters.
            If ``dm_conversation_id`` and ``participant_id`` are not passed or
            both are passed
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/manage/api-reference/post-dm_conversations-dm_conversation_id-messages
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/manage/api-reference/post-dm_conversations-with-participant_id-messages
            path = f"/2/dm_conversations/{dm_conversation_id}/messages"
            path = f"/2/dm_conversations/with/{participant_id}/messages"
            raise TypeError("DM conversation ID or participant ID is required")
        if media_id is not None:
            json["attachments"] = [{"media_id": str(media_id)}]
        return self._make_request("POST", path, json=json, user_auth=user_auth)
    create_dm = create_direct_message
    def create_direct_message_conversation(
        self, *, media_id=None, text=None, participant_ids, user_auth=True
        """Creates a new group conversation and adds a Direct Message to it on
        behalf of the authenticated user.
            There is an alias for this method named ``create_dm_conversation``.
        participant_ids : list[int | str]
            An array of User IDs that the conversation is created with.
            Conversations can have up to 50 participants.
        https://developer.twitter.com/en/docs/twitter-api/direct-messages/manage/api-reference/post-dm_conversations
        json = {
            "conversation_type": "Group",
            "message": {},
            "participant_ids": list(map(str, participant_ids))
            json["message"]["attachments"] = [{"media_id": str(media_id)}]
            json["message"]["text"] = text
            "POST", "/2/dm_conversations", json=json, user_auth=user_auth
    create_dm_conversation = create_direct_message_conversation
    # List Tweets lookup
    def get_list_tweets(self, id, *, user_auth=False, **params):
        """get_list_tweets( \
        Returns a list of Tweets from the specified List.
        .. versionchanged:: 4.10.1
            Added ``media_fields``, ``place_fields``, and ``poll_fields``
            parameters
            The ID of the List whose Tweets you would like to retrieve.
            page of results. To return the next page, pass the next_token
            previous_token returned in your previous response.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-tweets/api-reference/get-lists-id-tweets
            "GET", f"/2/lists/{id}/tweets", params=params,
    # List follows
    def unfollow_list(self, list_id, *, user_auth=True):
        """Enables the authenticated user to unfollow a List.
        .. versionadded:: 4.2
        list_id : int | str
            The ID of the List that you would like the user to unfollow.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-follows/api-reference/delete-users-id-followed-lists-list_id
        route = f"/2/users/{id}/followed_lists/{list_id}"
    def get_list_followers(self, id, *, user_auth=False, **params):
        """get_list_followers( \
        Returns a list of users who are followers of the specified List.
            The ID of the List whose followers you would like to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-follows/api-reference/get-lists-id-followers
            "GET", f"/2/lists/{id}/followers", params=params,
    def get_followed_lists(self, id, *, user_auth=False, **params):
        """get_followed_lists( \
            id, *, expansions=None, list_fields=None, max_results=None, \
            pagination_token=None, user_fields=None, user_auth=False \
        Returns all Lists a specified user follows.
            The user ID whose followed Lists you would like to retrieve.
        list_fields : list[str] | str | None
            :ref:`list_fields_parameter`
        https://developer.twitter.com/en/docs/twitter-api/lists/list-follows/api-reference/get-users-id-followed_lists
            "GET", f"/2/users/{id}/followed_lists", params=params,
                "expansions", "list.fields", "max_results", "pagination_token",
            ), data_type=List, user_auth=user_auth
    def follow_list(self, list_id, *, user_auth=True):
        """Enables the authenticated user to follow a List.
            The ID of the List that you would like the user to follow.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-follows/api-reference/post-users-id-followed-lists
        route = f"/2/users/{id}/followed_lists"
            "POST", route, json={"list_id": str(list_id)}, user_auth=user_auth
    # List lookup
    def get_list(self, id, *, user_auth=False, **params):
        """get_list(id, *, expansions=None, list_fields=None, \
                    user_fields=None, user_auth=False)
        Returns the details of a specified List.
            The ID of the List to lookup.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-lookup/api-reference/get-lists-id
            "GET", f"/2/lists/{id}", params=params,
                "expansions", "list.fields", "user.fields"
    def get_owned_lists(self, id, *, user_auth=False, **params):
        """get_owned_lists( \
        Returns all Lists owned by the specified user.
            The user ID whose owned Lists you would like to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-lookup/api-reference/get-users-id-owned_lists
            "GET", f"/2/users/{id}/owned_lists", params=params,
    # List members
    def remove_list_member(self, id, user_id, *, user_auth=True):
        """Enables the authenticated user to remove a member from a List they
        own.
            The ID of the List you are removing a member from.
        user_id : int | str
            The ID of the user you wish to remove as a member of the List.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-members/api-reference/delete-lists-id-members-user_id
            "DELETE", f"/2/lists/{id}/members/{user_id}", user_auth=user_auth
    def get_list_members(self, id, *, user_auth=False, **params):
        """get_list_members( \
        Returns a list of users who are members of the specified List.
            The ID of the List whose members you would like to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-members/api-reference/get-lists-id-members
            "GET", f"/2/lists/{id}/members", params=params,
    def get_list_memberships(self, id, *, user_auth=False, **params):
        """get_list_memberships( \
        Returns all Lists a specified user is a member of.
            The user ID whose List memberships you would like to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-members/api-reference/get-users-id-list_memberships
            "GET", f"/2/users/{id}/list_memberships", params=params,
    def add_list_member(self, id, user_id, *, user_auth=True):
        """Enables the authenticated user to add a member to a List they own.
            The ID of the List you are adding a member to.
            The ID of the user you wish to add as a member of the List.
        https://developer.twitter.com/en/docs/twitter-api/lists/list-members/api-reference/post-lists-id-members
            "POST", f"/2/lists/{id}/members", json={"user_id": str(user_id)},
    # Manage Lists
    def delete_list(self, id, *, user_auth=True):
        """Enables the authenticated user to delete a List that they own.
            The ID of the List to be deleted.
        https://developer.twitter.com/en/docs/twitter-api/lists/manage-lists/api-reference/delete-lists-id
            "DELETE", f"/2/lists/{id}", user_auth=user_auth
    def update_list(self, id, *, description=None, name=None, private=None,
                    user_auth=True):
        """Enables the authenticated user to update the meta data of a
        specified List that they own.
            The ID of the List to be updated.
        description : str | None
            Updates the description of the List.
        name : str | None
            Updates the name of the List.
        private : bool | None
            Determines whether the List should be private.
        https://developer.twitter.com/en/docs/twitter-api/lists/manage-lists/api-reference/put-lists-id
        if description is not None:
            json["description"] = description
        if name is not None:
            json["name"] = name
        if private is not None:
            json["private"] = private
            "PUT", f"/2/lists/{id}", json=json, user_auth=user_auth
    def create_list(self, name, *, description=None, private=None,
        """Enables the authenticated user to create a List.
        name : str
            The name of the List you wish to create.
            Description of the List.
            Determine whether the List should be private.
        https://developer.twitter.com/en/docs/twitter-api/lists/manage-lists/api-reference/post-lists
        json = {"name": name}
            "POST", f"/2/lists", json=json, user_auth=user_auth
    # Pinned Lists
    def unpin_list(self, list_id, *, user_auth=True):
        """Enables the authenticated user to unpin a List.
            The ID of the List that you would like the user to unpin.
        https://developer.twitter.com/en/docs/twitter-api/lists/pinned-lists/api-reference/delete-users-id-pinned-lists-list_id
        route = f"/2/users/{id}/pinned_lists/{list_id}"
    def get_pinned_lists(self, *, user_auth=True, **params):
        """get_pinned_lists(*, expansions=None, list_fields=None, \
                            user_fields=None, user_auth=True)
        Returns the Lists pinned by a specified user.
        https://developer.twitter.com/en/docs/twitter-api/lists/pinned-lists/api-reference/get-users-id-pinned_lists
        route = f"/2/users/{id}/pinned_lists"
    def pin_list(self, list_id, *, user_auth=True):
        """Enables the authenticated user to pin a List.
            The ID of the List that you would like the user to pin.
        https://developer.twitter.com/en/docs/twitter-api/lists/pinned-lists/api-reference/post-users-id-pinned-lists
    # Batch Compliance
    def get_compliance_jobs(self, type, **params):
        """get_compliance_jobs(type, *, status=None)
        Returns a list of recent compliance jobs.
        type : str
            Allows to filter by job type - either by tweets or user ID. Only
            one filter (tweets or users) can be specified per request.
        status : str | None
            Allows to filter by job status. Only one filter can be specified
            per request.
            Default: ``all``
        https://developer.twitter.com/en/docs/twitter-api/compliance/batch-compliance/api-reference/get-compliance-jobs
        params["type"] = type
            "GET", "/2/compliance/jobs", params=params,
            endpoint_parameters=("type", "status")
    def get_compliance_job(self, id):
        """Get a single compliance job with the specified ID.
            The unique identifier for the compliance job you want to retrieve.
        https://developer.twitter.com/en/docs/twitter-api/compliance/batch-compliance/api-reference/get-compliance-jobs-id
            "GET", f"/2/compliance/jobs/{id}"
    def create_compliance_job(self, type, *, name=None, resumable=None):
        """Creates a new compliance job for Tweet IDs or user IDs.
        A compliance job will contain an ID and a destination URL. The
        destination URL represents the location that contains the list of IDs
        consumed by your app.
        You can run one batch job at a time.
            Specify whether you will be uploading tweet or user IDs. You can
            either specify tweets or users.
            A name for this job, useful to identify multiple jobs using a label
            you define.
        resumable : bool | None
            Specifies whether to enable the upload URL with support for
            resumable uploads. If true, this endpoint will return a pre-signed
            URL with resumable uploads enabled.
        https://developer.twitter.com/en/docs/twitter-api/compliance/batch-compliance/api-reference/post-compliance-jobs
        json = {"type": type}
        if resumable is not None:
            json["resumable"] = resumable
            "POST", "/2/compliance/jobs", json=json
from async_lru import alru_cache
from oauthlib.oauth1 import Client as OAuthClient
from yarl import URL
async_cache = alru_cache(maxsize=None)
class AsyncBaseClient(BaseClient):
        self.session = None
            f"aiohttp/{aiohttp.__version__} "
    async def request(
        self, method, route, params=None, json=None, user_auth=False
        session = self.session or aiohttp.ClientSession()
        url = "https://api.twitter.com" + route
            headers["Content-Type"] = "application/json"
            oauth_client = OAuthClient(
            url = str(URL(url).with_query(sorted(params.items())))
            url, headers, body = oauth_client.sign(
                url, method, headers=headers
            # oauthlib.oauth1.Client (OAuthClient) expects colons in query
            # values (e.g. in timestamps) to be percent-encoded, while
            # aiohttp.ClientSession does not automatically encode them
            before_query, question_mark, query = url.partition('?')
            url = URL(
                f"{before_query}?{query.replace(':', '%3A')}",
                encoded = True
            params = None
            f"Making API request: {method} {url}\n"
            f"JSON: {json}"
        async with session.request(
            method, url, params=params, json=json, headers=headers
            await response.read()
            f"Received API response: {response.status} {response.reason}\n"
            f"Headers: {response.headers}"
        if self.session is None:
            await session.close()
        if not 200 <= response.status < 300:
            response_json = await response.json()
        if response.status == 400:
            raise BadRequest(response, response_json=response_json)
        if response.status == 401:
            raise Unauthorized(response, response_json=response_json)
        if response.status == 403:
            raise Forbidden(response, response_json=response_json)
        if response.status == 404:
            raise NotFound(response, response_json=response_json)
        if response.status == 429:
                        await asyncio.sleep(sleep_time)
                return await self.request(method, route, params, json, user_auth)
                raise TooManyRequests(response, response_json=response_json, reset_time=reset_time)
        if response.status >= 500:
            raise TwitterServerError(response, response_json=response_json)
            raise HTTPException(response, response_json=response_json)
    async def _make_request(
        response = await self.request(method, route, params=request_params,
        if self.return_type is aiohttp.ClientResponse:
        response = await response.json()
class AsyncClient(AsyncBaseClient):
    """AsyncClient( \
    Asynchronous Twitter API v2 Client
    .. versionadded:: 4.10
    return_type : type[dict | aiohttp.ClientResponse | Response]
    session : aiohttp.ClientSession
        Aiohttp client session used to make requests to the API
    async def _get_authenticating_user_id(self, *, oauth_1=False):
                return await self._get_oauth_2_authenticating_user_id(
    @async_cache
    async def _get_oauth_2_authenticating_user_id(self, access_token):
        user_id = (await self.get_me(user_auth=False))["data"]["id"]
    async def remove_bookmark(self, tweet_id):
            :class:`AsyncClient` instance for each access token used.
        dict | aiohttp.ClientResponse | Response
        id = await self._get_authenticating_user_id()
        return await self._make_request(
    async def get_bookmarks(self, **params):
    async def bookmark(self, tweet_id):
    async def hide_reply(self, id, *, user_auth=True):
    async def unhide_reply(self, id, *, user_auth=True):
    async def unlike(self, tweet_id, *, user_auth=True):
            done once per :class:`AsyncClient` instance for each access token
            used.
        id = await self._get_authenticating_user_id(oauth_1=user_auth)
    async def get_liking_users(self, id, *, user_auth=False, **params):
        Allows you to get information about a Tweet’s liking users.
    async def get_liked_tweets(self, id, *, user_auth=False, **params):
        Allows you to get information about a user’s liked Tweets.
    async def like(self, tweet_id, *, user_auth=True):
    async def delete_tweet(self, id, *, user_auth=True):
    async def create_tweet(
        reply_settings=None, text=None, user_auth=True
    async def get_quote_tweets(self, id, *, user_auth=False, **params):
    async def unretweet(self, source_tweet_id, *, user_auth=True):
    async def get_retweeters(self, id, *, user_auth=False, **params):
    async def retweet(self, tweet_id, *, user_auth=True):
    async def search_all_tweets(self, query, **params):
    async def search_recent_tweets(self, query, *, user_auth=False, **params):
    async def get_users_mentions(self, id, *, user_auth=False, **params):
    async def get_home_timeline(self, *, user_auth=True, **params):
    async def get_users_tweets(self, id, *, user_auth=False, **params):
    async def get_all_tweets_count(self, query, **params):
    async def get_recent_tweets_count(self, query, **params):
    async def get_tweet(self, id, *, user_auth=False, **params):
    async def get_tweets(self, ids, *, user_auth=False, **params):
    async def get_blocked(self, *, user_auth=True, **params):
    async def unfollow_user(self, target_user_id, *, user_auth=True):
        source_user_id = await self._get_authenticating_user_id(
            oauth_1=user_auth
    async def get_users_followers(self, id, *, user_auth=False, **params):
    async def get_users_following(self, id, *, user_auth=False, **params):
    async def follow_user(self, target_user_id, *, user_auth=True):
    async def unmute(self, target_user_id, *, user_auth=True):
    async def get_muted(self, *, user_auth=True, **params):
    async def mute(self, target_user_id, *, user_auth=True):
    async def get_user(
        self, *, id=None, username=None, user_auth=False, **params
    async def get_users(self, *, ids=None, usernames=None, user_auth=False,
    async def get_me(self, *, user_auth=True, **params):
    async def search_spaces(self, query, **params):
    async def get_spaces(self, *, ids=None, user_ids=None, **params):
    async def get_space(self, id, **params):
    async def get_space_buyers(self, id, **params):
    async def get_space_tweets(self, id, **params):
    async def get_direct_message_events(
    async def create_direct_message(
            "POST", path, json=json, user_auth=user_auth
    async def create_direct_message_conversation(
    async def get_list_tweets(self, id, *, user_auth=False, **params):
    async def unfollow_list(self, list_id, *, user_auth=True):
    async def get_list_followers(self, id, *, user_auth=False, **params):
    async def get_followed_lists(self, id, *, user_auth=False, **params):
    async def follow_list(self, list_id, *, user_auth=True):
    async def get_list(self, id, *, user_auth=False, **params):
    async def get_owned_lists(self, id, *, user_auth=False, **params):
    async def remove_list_member(self, id, user_id, *, user_auth=True):
    async def get_list_members(self, id, *, user_auth=False, **params):
    async def get_list_memberships(self, id, *, user_auth=False, **params):
    async def add_list_member(self, id, user_id, *, user_auth=True):
    async def delete_list(self, id, *, user_auth=True):
    async def update_list(self, id, *, description=None, name=None,
                          private=None, user_auth=True):
    async def create_list(self, name, *, description=None, private=None,
    async def unpin_list(self, list_id, *, user_auth=True):
    async def get_pinned_lists(self, *, user_auth=True, **params):
    async def pin_list(self, list_id, *, user_auth=True):
    async def get_compliance_jobs(self, type, **params):
    async def get_compliance_job(self, id):
    async def create_compliance_job(self, type, *, name=None, resumable=None):
from django.db.backends.sqlite3.client import DatabaseClient
class SpatiaLiteClient(DatabaseClient):
    executable_name = "spatialite"
class BaseDatabaseClient:
    """Encapsulate backend-specific methods for opening a client shell."""
    # This should be a string representing the name of the executable
    # (e.g., "psql"). Subclasses must override this.
    executable_name = None
    def __init__(self, connection):
        # connection is an instance of BaseDatabaseWrapper.
        del self.connection
    def settings_to_cmd_args_env(cls, settings_dict, parameters):
            "subclasses of BaseDatabaseClient must provide a "
            "settings_to_cmd_args_env() method or override a runshell()."
    def runshell(self, parameters):
        args, env = self.settings_to_cmd_args_env(
            self.connection.settings_dict, parameters
        env = {**os.environ, **env} if env else None
        subprocess.run(args, env=env, check=True)
    executable_name = "mysql"
        args = [cls.executable_name]
        env = None
        database = settings_dict["OPTIONS"].get(
            "database",
            settings_dict["OPTIONS"].get("db", settings_dict["NAME"]),
        user = settings_dict["OPTIONS"].get("user", settings_dict["USER"])
        password = settings_dict["OPTIONS"].get(
            "password",
            settings_dict["OPTIONS"].get("passwd", settings_dict["PASSWORD"]),
        host = settings_dict["OPTIONS"].get("host", settings_dict["HOST"])
        port = settings_dict["OPTIONS"].get("port", settings_dict["PORT"])
        server_ca = settings_dict["OPTIONS"].get("ssl", {}).get("ca")
        client_cert = settings_dict["OPTIONS"].get("ssl", {}).get("cert")
        client_key = settings_dict["OPTIONS"].get("ssl", {}).get("key")
        defaults_file = settings_dict["OPTIONS"].get("read_default_file")
        charset = settings_dict["OPTIONS"].get("charset")
        # Seems to be no good way to set sql_mode with CLI.
        if defaults_file:
            args += ["--defaults-file=%s" % defaults_file]
        if user:
            args += ["--user=%s" % user]
        if password:
            # The MYSQL_PWD environment variable usage is discouraged per
            # MySQL's documentation due to the possibility of exposure through
            # `ps` on old Unix flavors but --password suffers from the same
            # flaw on even more systems. Usage of an environment variable also
            # prevents password exposure if the subprocess.run(check=True) call
            # raises a CalledProcessError since the string representation of
            # the latter includes all of the provided `args`.
            env = {"MYSQL_PWD": password}
        if host:
            if "/" in host:
                args += ["--socket=%s" % host]
                args += ["--host=%s" % host]
        if port:
            args += ["--port=%s" % port]
        if server_ca:
            args += ["--ssl-ca=%s" % server_ca]
        if client_cert:
            args += ["--ssl-cert=%s" % client_cert]
        if client_key:
            args += ["--ssl-key=%s" % client_key]
        if charset:
            args += ["--default-character-set=%s" % charset]
        if database:
            args += [database]
        args.extend(parameters)
        return args, env
        sigint_handler = signal.getsignal(signal.SIGINT)
            # Allow SIGINT to pass to mysql to abort queries.
            signal.signal(signal.SIGINT, signal.SIG_IGN)
            super().runshell(parameters)
            # Restore the original SIGINT handler.
            signal.signal(signal.SIGINT, sigint_handler)
    executable_name = "sqlplus"
    wrapper_name = "rlwrap"
    def connect_string(settings_dict):
        from django.db.backends.oracle.utils import dsn
        return '%s/"%s"@%s' % (
            settings_dict["USER"],
            settings_dict["PASSWORD"],
            dsn(settings_dict),
        args = [cls.executable_name, "-L", cls.connect_string(settings_dict)]
        wrapper_path = shutil.which(cls.wrapper_name)
        if wrapper_path:
            args = [wrapper_path, *args]
        return args, None
    executable_name = "psql"
        options = settings_dict["OPTIONS"]
        host = settings_dict.get("HOST")
        port = settings_dict.get("PORT")
        dbname = settings_dict.get("NAME")
        user = settings_dict.get("USER")
        passwd = settings_dict.get("PASSWORD")
        passfile = options.get("passfile")
        service = options.get("service")
        sslmode = options.get("sslmode")
        sslrootcert = options.get("sslrootcert")
        sslcert = options.get("sslcert")
        sslkey = options.get("sslkey")
        if not dbname and not service:
            dbname = "postgres"
            args += ["-U", user]
            args += ["-h", host]
            args += ["-p", str(port)]
        if dbname:
            args += [dbname]
        if passwd:
            env["PGPASSWORD"] = str(passwd)
        if service:
            env["PGSERVICE"] = str(service)
        if sslmode:
            env["PGSSLMODE"] = str(sslmode)
        if sslrootcert:
            env["PGSSLROOTCERT"] = str(sslrootcert)
        if sslcert:
            env["PGSSLCERT"] = str(sslcert)
        if sslkey:
            env["PGSSLKEY"] = str(sslkey)
        if passfile:
            env["PGPASSFILE"] = str(passfile)
        return args, (env or None)
            # Allow SIGINT to pass to psql to abort queries.
    executable_name = "sqlite3"
        args = [cls.executable_name, settings_dict["NAME"], *parameters]
from io import BytesIO, IOBase
from urllib.parse import unquote_to_bytes, urljoin, urlsplit
from django.core.handlers.asgi import ASGIRequest
from django.core.handlers.base import BaseHandler
from django.core.handlers.wsgi import LimitedStream, WSGIRequest
from django.core.signals import got_request_exception, request_finished, request_started
from django.db import close_old_connections
from django.http import HttpHeaders, HttpRequest, QueryDict, SimpleCookie
from django.test import signals
from django.test.utils import ContextList
from django.urls import resolve
from django.utils.encoding import force_bytes
    "RedirectCycleError",
    "encode_file",
    "encode_multipart",
BOUNDARY = "BoUnDaRyStRiNg"
MULTIPART_CONTENT = "multipart/form-data; boundary=%s" % BOUNDARY
CONTENT_TYPE_RE = _lazy_re_compile(r".*; charset=([\w-]+);?")
# Structured suffix spec: https://tools.ietf.org/html/rfc6838#section-4.2.8
JSON_CONTENT_TYPE_RE = _lazy_re_compile(r"^application\/(.+\+)?json")
REDIRECT_STATUS_CODES = frozenset(
        HTTPStatus.MOVED_PERMANENTLY,
        HTTPStatus.FOUND,
        HTTPStatus.SEE_OTHER,
        HTTPStatus.TEMPORARY_REDIRECT,
        HTTPStatus.PERMANENT_REDIRECT,
class RedirectCycleError(Exception):
    """The test client has been asked to follow a redirect loop."""
    def __init__(self, message, last_response):
        self.last_response = last_response
        self.redirect_chain = last_response.redirect_chain
class FakePayload(IOBase):
    A wrapper around BytesIO that restricts what can be read since data from
    the network can't be sought and cannot be read outside of its content
    length. This makes sure that views can't do anything under the test client
    that wouldn't work in real life.
    def __init__(self, initial_bytes=None):
        self.__content = BytesIO()
        self.__len = 0
        self.read_started = False
        if initial_bytes is not None:
            self.write(initial_bytes)
        return self.__len
    def read(self, size=-1, /):
        if not self.read_started:
            self.__content.seek(0)
            self.read_started = True
        if size == -1 or size is None:
            size = self.__len
            self.__len >= size
        ), "Cannot read more than the available bytes from the HTTP incoming data."
        content = self.__content.read(size)
        self.__len -= len(content)
    def readline(self, size=-1, /):
        content = self.__content.readline(size)
    def write(self, b, /):
        if self.read_started:
            raise ValueError("Unable to write a payload after it's been read")
        content = force_bytes(b)
        self.__content.write(content)
        self.__len += len(content)
def closing_iterator_wrapper(iterable, close):
        request_finished.disconnect(close_old_connections)
        close()  # will fire request_finished
        request_finished.connect(close_old_connections)
async def aclosing_iterator_wrapper(iterable, close):
        async for chunk in iterable:
def conditional_content_removal(request, response):
    Simulate the behavior of most web servers by removing the content of
    responses for HEAD requests, 1xx, 204, and 304 responses. Ensure
    compliance with RFC 9112 Section 6.3.
    if 100 <= response.status_code < 200 or response.status_code in (204, 304):
        if response.streaming:
            response.streaming_content = []
            response.content = b""
    if request.method == "HEAD":
class ClientHandler(BaseHandler):
    An HTTP Handler that can be used for testing purposes. Use the WSGI
    interface to compose requests, but return the raw HttpResponse object with
    the originating WSGIRequest attached to its ``wsgi_request`` attribute.
    def __init__(self, enforce_csrf_checks=True, *args, **kwargs):
        self.enforce_csrf_checks = enforce_csrf_checks
    def __call__(self, environ):
        # Set up middleware if needed. We couldn't do this earlier, because
        # settings weren't available.
        if self._middleware_chain is None:
            self.load_middleware()
        request_started.disconnect(close_old_connections)
        request_started.send(sender=self.__class__, environ=environ)
        request_started.connect(close_old_connections)
        request = WSGIRequest(environ)
        # sneaky little hack so that we can easily get round
        # CsrfViewMiddleware. This makes life easier, and is probably
        # required for backwards compatibility with external tests against
        # admin views.
        request._dont_enforce_csrf_checks = not self.enforce_csrf_checks
        # Request goes through middleware.
        response = self.get_response(request)
        # Simulate behaviors of most web servers.
        conditional_content_removal(request, response)
        # Attach the originating request to the response so that it could be
        # later retrieved.
        response.wsgi_request = request
        # Emulate a WSGI server by calling the close method on completion.
            if response.is_async:
                response.streaming_content = aclosing_iterator_wrapper(
                    response.streaming_content, response.close
                response.streaming_content = closing_iterator_wrapper(
            response.close()  # will fire request_finished
class AsyncClientHandler(BaseHandler):
    """An async version of ClientHandler."""
    async def __call__(self, scope):
            self.load_middleware(is_async=True)
        # Extract body file from the scope, if provided.
        if "_body_file" in scope:
            body_file = scope.pop("_body_file")
            body_file = FakePayload("")
        await request_started.asend(sender=self.__class__, scope=scope)
        # Wrap FakePayload body_file to allow large read() in test environment.
        request = ASGIRequest(scope, LimitedStream(body_file, len(body_file)))
        # Sneaky little hack so that we can easily get round
        # CsrfViewMiddleware. This makes life easier, and is probably required
        # for backwards compatibility with external tests against admin views.
        response = await self.get_response_async(request)
        # Attach the originating ASGI request to the response so that it could
        # be later retrieved.
        response.asgi_request = request
        # Emulate a server by calling the close method on completion.
            # Will fire request_finished.
            await sync_to_async(response.close, thread_sensitive=False)()
def store_rendered_templates(store, signal, sender, template, context, **kwargs):
    Store templates and contexts that are rendered.
    The context is copied so that it is an accurate representation at the time
    of rendering.
    store.setdefault("templates", []).append(template)
    if "context" not in store:
        store["context"] = ContextList()
    store["context"].append(copy(context))
def encode_multipart(boundary, data):
    Encode multipart POST data from a dictionary of form values.
    The key will be used as the form data name; the value will be transmitted
    as content. If the value is a file, the contents of the file will be sent
    as an application/octet-stream; otherwise, str(value) will be sent.
    def to_bytes(s):
        return force_bytes(s, settings.DEFAULT_CHARSET)
    # Not by any means perfect, but good enough for our purposes.
    def is_file(thing):
        return hasattr(thing, "read") and callable(thing.read)
    # Each bit of the multipart form data could be either a form value or a
    # file, or a *list* of form values and/or files. Remember that HTTP field
    # names can be duplicated!
                "Cannot encode None for key '%s' as POST data. Did you mean "
                "to pass an empty string or omit the value?" % key
        elif is_file(value):
            lines.extend(encode_file(boundary, key, value))
        elif not isinstance(value, str) and isinstance(value, Iterable):
                if is_file(item):
                    lines.extend(encode_file(boundary, key, item))
                    lines.extend(
                        to_bytes(val)
                        for val in [
                            "--%s" % boundary,
                            'Content-Disposition: form-data; name="%s"' % key,
            to_bytes("--%s--" % boundary),
    return b"\r\n".join(lines)
def encode_file(boundary, key, file):
    # file.name might not be a string. For example, it's an int for
    # tempfile.TemporaryFile().
    file_has_string_name = hasattr(file, "name") and isinstance(file.name, str)
    filename = os.path.basename(file.name) if file_has_string_name else ""
    if hasattr(file, "content_type"):
        content_type = file.content_type
    elif filename:
        content_type = mimetypes.guess_type(filename)[0]
        content_type = "application/octet-stream"
    filename = filename or key
        to_bytes("--%s" % boundary),
        to_bytes(
            'Content-Disposition: form-data; name="%s"; filename="%s"' % (key, filename)
        to_bytes("Content-Type: %s" % content_type),
        to_bytes(file.read()),
class RequestFactory:
    Class that lets you create mock Request objects for use in testing.
    rf = RequestFactory()
    get_request = rf.get('/hello/')
    post_request = rf.post('/submit/', {'foo': 'bar'})
    Once you have a request object you can pass it to any view function,
    just as if that view had been hooked up using a URLconf.
        json_encoder=DjangoJSONEncoder,
        query_params=None,
        self.json_encoder = json_encoder
        self.defaults = defaults
        self.errors = BytesIO()
            self.defaults.update(HttpHeaders.to_wsgi_names(headers))
        if query_params:
            self.defaults["QUERY_STRING"] = urlencode(query_params, doseq=True)
    def _base_environ(self, **request):
        The base environment for a request.
        # This is a minimal valid WSGI environ dictionary, plus:
        # - HTTP_COOKIE: for cookie support,
        # - REMOTE_ADDR: often useful, see #8551.
        # See https://www.python.org/dev/peps/pep-3333/#environ-variables
            "HTTP_COOKIE": "; ".join(
                sorted(
                    "%s=%s" % (morsel.key, morsel.coded_value)
                    for morsel in self.cookies.values()
            "PATH_INFO": "/",
            "REMOTE_ADDR": "127.0.0.1",
            "REQUEST_METHOD": "GET",
            "SCRIPT_NAME": "",
            "SERVER_NAME": "testserver",
            "SERVER_PORT": "80",
            "SERVER_PROTOCOL": "HTTP/1.1",
            "wsgi.version": (1, 0),
            "wsgi.url_scheme": "http",
            "wsgi.input": FakePayload(b""),
            "wsgi.errors": self.errors,
            "wsgi.multiprocess": True,
            "wsgi.multithread": False,
            "wsgi.run_once": False,
            **self.defaults,
            **request,
    def request(self, **request):
        "Construct a generic request object."
        return WSGIRequest(self._base_environ(**request))
    def _encode_data(self, data, content_type):
        if content_type is MULTIPART_CONTENT:
            return encode_multipart(BOUNDARY, data)
            # Encode the content so that the byte representation is correct.
            match = CONTENT_TYPE_RE.match(content_type)
                charset = match[1]
                charset = settings.DEFAULT_CHARSET
            return force_bytes(data, encoding=charset)
    def _encode_json(self, data, content_type):
        Return encoded JSON if data is a dict, list, or tuple and content_type
        is application/json.
        should_encode = JSON_CONTENT_TYPE_RE.match(content_type) and isinstance(
            data, (dict, list, tuple)
        return json.dumps(data, cls=self.json_encoder) if should_encode else data
    def _get_path(self, parsed):
        path = unquote_to_bytes(parsed.path)
        # Replace the behavior where non-ASCII values in the WSGI environ are
        # arbitrarily decoded with ISO-8859-1.
        # Refs comment in `get_bytes_from_wsgi()`.
        return path.decode("iso-8859-1")
        self, path, data=None, secure=False, *, headers=None, query_params=None, **extra
        """Construct a GET request."""
        if query_params and data:
            raise ValueError("query_params and data arguments are mutually exclusive.")
        query_params = data or query_params
        query_params = {} if query_params is None else query_params
        return self.generic(
            query_params=query_params,
            **extra,
        content_type=MULTIPART_CONTENT,
        """Construct a POST request."""
        data = self._encode_json({} if data is None else data, content_type)
        post_data = self._encode_data(data, content_type)
            "POST",
            post_data,
            content_type,
    def head(
        """Construct a HEAD request."""
            "HEAD",
    def trace(self, path, secure=False, *, headers=None, query_params=None, **extra):
        """Construct a TRACE request."""
            "TRACE",
    def options(
        data="",
        content_type="application/octet-stream",
        "Construct an OPTIONS request."
            "OPTIONS",
        """Construct a PUT request."""
        data = self._encode_json(data, content_type)
            "PUT",
        """Construct a PATCH request."""
            "PATCH",
        """Construct a DELETE request."""
            "DELETE",
    def generic(
        """Construct an arbitrary HTTP request."""
        parsed = urlsplit(str(path))  # path can be lazy
        data = force_bytes(data, settings.DEFAULT_CHARSET)
        r = {
            "PATH_INFO": self._get_path(parsed),
            "REQUEST_METHOD": method,
            "SERVER_PORT": "443" if secure else "80",
            "wsgi.url_scheme": "https" if secure else "http",
            r.update(
                    "CONTENT_LENGTH": str(len(data)),
                    "CONTENT_TYPE": content_type,
                    "wsgi.input": FakePayload(data),
            extra.update(HttpHeaders.to_wsgi_names(headers))
            extra["QUERY_STRING"] = urlencode(query_params, doseq=True)
        r.update(extra)
        # If QUERY_STRING is absent or empty, extract it from the URL.
        if not r.get("QUERY_STRING"):
            # WSGI requires latin-1 encoded strings. See get_path_info().
            r["QUERY_STRING"] = parsed.query.encode().decode("iso-8859-1")
        return self.request(**r)
class AsyncRequestFactory(RequestFactory):
    Class that lets you create mock ASGI-like Request objects for use in
    testing. Usage:
    rf = AsyncRequestFactory()
    get_request = rf.get("/hello/")
    post_request = rf.post("/submit/", {"foo": "bar"})
    including synchronous ones. The reason we have a separate class here is:
    a) this makes ASGIRequest subclasses, and
    b) AsyncClient can subclass it.
    def _base_scope(self, **request):
        """The base scope for a request."""
        # This is a minimal valid ASGI scope, plus:
        # - headers['cookie'] for cookie support,
        # - 'client' often useful, see #8551.
        scope = {
            "asgi": {"version": "3.0"},
            "type": "http",
            "http_version": "1.1",
            "client": ["127.0.0.1", 0],
            "server": ("testserver", "80"),
            "scheme": "http",
            "method": "GET",
            "headers": [],
        scope["headers"].append(
                b"cookie",
                b"; ".join(
                        ("%s=%s" % (morsel.key, morsel.coded_value)).encode("ascii")
        return scope
        """Construct a generic request object."""
        # This is synchronous, which means all methods on this class are.
        # AsyncClient, however, has an async request function, which makes all
        # its methods async.
        if "_body_file" in request:
            body_file = request.pop("_body_file")
        return ASGIRequest(
            self._base_scope(**request), LimitedStream(body_file, len(body_file))
        parsed = urlsplit(str(path))  # path can be lazy.
        s = {
            "path": self._get_path(parsed),
            "server": ("127.0.0.1", "443" if secure else "80"),
            "scheme": "https" if secure else "http",
            "headers": [(b"host", b"testserver")],
        if self.defaults:
            extra = {**self.defaults, **extra}
            s["headers"].extend(
                    (b"content-length", str(len(data)).encode("ascii")),
                    (b"content-type", content_type.encode("ascii")),
            s["_body_file"] = FakePayload(data)
            s["query_string"] = urlencode(query_params, doseq=True)
        elif query_string := extra.pop("QUERY_STRING", None):
            s["query_string"] = query_string
            # If QUERY_STRING is absent or empty, we want to extract it from
            # the URL.
            s["query_string"] = parsed.query
            extra.update(HttpHeaders.to_asgi_names(headers))
        s["headers"] += [
            (key.lower().encode("ascii"), value.encode("latin1"))
            for key, value in extra.items()
        return self.request(**s)
class ClientMixin:
    Mixin with common methods between Client and AsyncClient.
    def store_exc_info(self, **kwargs):
        """Store exceptions when they are generated by a view."""
        self.exc_info = sys.exc_info()
    def check_exception(self, response):
        Look for a signaled exception, clear the current context exception
        data, re-raise the signaled exception, and clear the signaled exception
        from the local cache.
        response.exc_info = self.exc_info
        if self.exc_info:
            _, exc_value, _ = self.exc_info
            self.exc_info = None
            if self.raise_request_exception:
                raise exc_value
    def session(self):
        """Return the current session variables."""
        engine = import_module(settings.SESSION_ENGINE)
        cookie = self.cookies.get(settings.SESSION_COOKIE_NAME)
        if cookie:
            return engine.SessionStore(cookie.value)
        session = engine.SessionStore()
        session.save()
        self.cookies[settings.SESSION_COOKIE_NAME] = session.session_key
    async def asession(self):
        await session.asave()
    def login(self, **credentials):
        Set the Factory to appear as if it has successfully logged into a site.
        Return True if login is possible or False if the provided credentials
        are incorrect.
        from django.contrib.auth import authenticate
        user = authenticate(**credentials)
            self._login(user)
    async def alogin(self, **credentials):
        from django.contrib.auth import aauthenticate
        user = await aauthenticate(**credentials)
            await self._alogin(user)
    def force_login(self, user, backend=None):
            backend = self._get_backend()
        user.backend = backend
        self._login(user, backend)
    async def aforce_login(self, user, backend=None):
        await self._alogin(user, backend)
    def _get_backend(self):
        from django.contrib.auth import load_backend
            if hasattr(backend, "get_user"):
                return backend_path
    def _login(self, user, backend=None):
        from django.contrib.auth import login
        # Create a fake request to store login details.
        request = HttpRequest()
        if self.session:
            request.session = self.session
            request.session = engine.SessionStore()
        login(request, user, backend)
        # Save the session values.
        request.session.save()
        self._set_login_cookies(request)
    async def _alogin(self, user, backend=None):
        from django.contrib.auth import alogin
        session = await self.asession()
            request.session = session
        await alogin(request, user, backend)
        await request.session.asave()
    def _set_login_cookies(self, request):
        # Set the cookie to represent the session.
        session_cookie = settings.SESSION_COOKIE_NAME
        self.cookies[session_cookie] = request.session.session_key
        cookie_data = {
            "max-age": None,
            "path": "/",
            "domain": settings.SESSION_COOKIE_DOMAIN,
            "secure": settings.SESSION_COOKIE_SECURE or None,
            "expires": None,
        self.cookies[session_cookie].update(cookie_data)
    def logout(self):
        """Log out the user by removing the cookies and session object."""
        from django.contrib.auth import get_user, logout
            request.user = get_user(request)
        logout(request)
    async def alogout(self):
        from django.contrib.auth import aget_user, alogout
            request.user = await aget_user(request)
        await alogout(request)
    def _parse_json(self, response, **extra):
        if not hasattr(response, "_json"):
            if not JSON_CONTENT_TYPE_RE.match(response.get("Content-Type")):
                    'Content-Type header is "%s", not "application/json"'
                    % response.get("Content-Type")
            response._json = json.loads(response.text, **extra)
        return response._json
    def _follow_redirect(
        content_type="",
        """Follow a single redirect contained in response using GET."""
        response_url = response.url
        redirect_chain = response.redirect_chain
        redirect_chain.append((response_url, response.status_code))
        url = urlsplit(response_url)
        if url.scheme:
            extra["wsgi.url_scheme"] = url.scheme
        if url.hostname:
            extra["SERVER_NAME"] = url.hostname
            extra["HTTP_HOST"] = url.hostname
        if url.port:
            extra["SERVER_PORT"] = str(url.port)
        path = url.path
        # RFC 3986 Section 6.2.3: Empty path should be normalized to "/".
        if not path and url.netloc:
            path = "/"
        # Prepend the request path to handle relative path redirects
        if not path.startswith("/"):
            path = urljoin(response.request["PATH_INFO"], path)
        if response.status_code in (
            # Preserve request method and query string (if needed)
            # post-redirect for 307/308 responses.
            request_method = response.request["REQUEST_METHOD"].lower()
            if request_method not in ("get", "head"):
                extra["QUERY_STRING"] = url.query
            request_method = getattr(self, request_method)
            request_method = self.get
            data = QueryDict(url.query)
            query_params = None
        return request_method(
            content_type=content_type,
            follow=False,
    def _ensure_redirects_not_cyclic(self, response):
        Raise a RedirectCycleError if response contains too many redirects.
        if redirect_chain[-1] in redirect_chain[:-1]:
            # Check that we're not redirecting to somewhere we've already been
            # to, to prevent loops.
            raise RedirectCycleError("Redirect loop detected.", last_response=response)
        if len(redirect_chain) > 20:
            # Such a lengthy chain likely also means a loop, but one with a
            # growing path, changing view, or changing query argument. 20 is
            # the value of "network.http.redirection-limit" from Firefox.
            raise RedirectCycleError("Too many redirects.", last_response=response)
class Client(ClientMixin, RequestFactory):
    A class that can act as a client for testing purposes.
    It allows the user to compose GET and POST requests, and
    obtain the response that the server gave to those requests.
    The server Response objects are annotated with the details
    of the contexts and templates that were rendered during the
    process of serving the request.
    Client objects are stateful - they will retain cookie (and
    thus session) details for the lifetime of the Client instance.
    This is not intended as a replacement for Twill/Selenium or
    the like - it is here to allow testing against the
    contexts and templates produced by a view, rather than the
    HTML rendered to the end-user.
        enforce_csrf_checks=False,
        raise_request_exception=True,
        super().__init__(headers=headers, query_params=query_params, **defaults)
        self.handler = ClientHandler(enforce_csrf_checks)
        self.raise_request_exception = raise_request_exception
        self.extra = None
        Make a generic request. Compose the environment dictionary and pass
        to the handler, return the result of the handler. Assume defaults for
        the query environment, which can be overridden using the arguments to
        environ = self._base_environ(**request)
        # Curry a data dictionary into an instance of the template renderer
        # callback function.
        on_template_render = partial(store_rendered_templates, data)
        signal_uid = "template-render-%s" % id(request)
        signals.template_rendered.connect(on_template_render, dispatch_uid=signal_uid)
        # Capture exceptions created by the handler.
        exception_uid = "request-exception-%s" % id(request)
        got_request_exception.connect(self.store_exc_info, dispatch_uid=exception_uid)
            response = self.handler(environ)
            signals.template_rendered.disconnect(dispatch_uid=signal_uid)
            got_request_exception.disconnect(dispatch_uid=exception_uid)
        # Check for signaled exceptions.
        self.check_exception(response)
        # Save the client and request that stimulated the response.
        response.client = self
        response.request = request
        # Add any rendered template detail to the response.
        response.templates = data.get("templates", [])
        response.context = data.get("context")
        response.json = partial(self._parse_json, response)
        # Attach the ResolverMatch instance to the response.
        urlconf = getattr(response.wsgi_request, "urlconf", None)
        response.resolver_match = SimpleLazyObject(
            lambda: resolve(request["PATH_INFO"], urlconf=urlconf),
        # Flatten a single context. Not really necessary anymore thanks to the
        # __getattr__ flattening in ContextList, but has some edge case
        # backwards compatibility implications.
        if response.context and len(response.context) == 1:
            response.context = response.context[0]
        # Update persistent cookie data.
        if response.cookies:
            self.cookies.update(response.cookies)
        """Request a response from the server using GET."""
        self.extra = extra
        response = super().get(
        if follow:
            response = self._handle_redirects(
                response, data=data, headers=headers, query_params=query_params, **extra
        """Request a response from the server using POST."""
        response = super().post(
        """Request a response from the server using HEAD."""
        response = super().head(
        """Request a response from the server using OPTIONS."""
        response = super().options(
        """Send a resource to the server using PUT."""
        response = super().put(
        """Send a resource to the server using PATCH."""
        response = super().patch(
        """Send a DELETE request to the server."""
        response = super().delete(
    def trace(
        """Send a TRACE request to the server."""
        response = super().trace(
    def _handle_redirects(
        Follow any redirects by requesting responses from the server using GET.
        response.redirect_chain = []
        while response.status_code in REDIRECT_STATUS_CODES:
            response = self._follow_redirect(
            response.redirect_chain = redirect_chain
            self._ensure_redirects_not_cyclic(response)
class AsyncClient(ClientMixin, AsyncRequestFactory):
    An async version of Client that creates ASGIRequests and calls through an
    async request path.
    Does not currently support "follow" on its methods.
        self.handler = AsyncClientHandler(enforce_csrf_checks)
    async def request(self, **request):
        Make a generic request. Compose the scope dictionary and pass to the
        handler, return the result of the handler. Assume defaults for the
        query environment, which can be overridden using the arguments to the
        scope = self._base_scope(**request)
            response = await self.handler(scope)
        urlconf = getattr(response.asgi_request, "urlconf", None)
            lambda: resolve(request["path"], urlconf=urlconf),
        response = await super().get(
            response = await self._ahandle_redirects(
        response = await super().post(
    async def head(
        response = await super().head(
    async def options(
        response = await super().options(
        response = await super().put(
        response = await super().patch(
        response = await super().delete(
    async def trace(
        response = await super().trace(
    async def _ahandle_redirects(
            response = await self._follow_redirect(
from django.contrib.auth.views import (
    INTERNAL_RESET_SESSION_TOKEN,
    PasswordResetConfirmView,
from django.test import Client
def extract_token_from_url(url):
    token_search = re.search(r"/reset/.*/(.+?)/", url)
    if token_search:
        return token_search[1]
class PasswordResetConfirmClient(Client):
    This client eases testing the password reset flow by emulating the
    PasswordResetConfirmView's redirect and saving of the reset token in the
    user's session. This request puts 'my-token' in the session and redirects
    to '/reset/bla/set-password/':
    >>> client = PasswordResetConfirmClient()
    >>> client.get('/reset/bla/my-token/')
    reset_url_token = PasswordResetConfirmView.reset_url_token
    def _get_password_reset_confirm_redirect_url(self, url):
        token = extract_token_from_url(url)
        if not token:
        # Add the token to the session
        session = self.session
        session[INTERNAL_RESET_SESSION_TOKEN] = token
        return url.replace(token, self.reset_url_token)
    def get(self, path, *args, **kwargs):
        redirect_url = self._get_password_reset_confirm_redirect_url(path)
        return super().get(redirect_url, *args, **kwargs)
    def post(self, path, *args, **kwargs):
        return super().post(redirect_url, *args, **kwargs)
from typing import Any, Dict, Optional, Union
from dateutil.tz import tzutc
from six import string_types
from posthog.args import ID_TYPES, ExceptionArg, OptionalCaptureArgs, OptionalSetArgs
from posthog.consumer import Consumer
    _get_current_context,
    get_capture_exception_code_variables_context,
    get_code_variables_ignore_patterns_context,
    get_code_variables_mask_patterns_context,
    get_context_device_id,
    get_context_distinct_id,
    get_context_session_id,
    new_context,
from posthog.exception_capture import ExceptionCapture
    exc_info_from_error,
    exception_is_already_captured,
    exceptions_from_error_tuple,
    handle_in_app,
    mark_exception_as_captured,
    try_attach_code_variables_to_frames,
    InconclusiveMatchError,
    RequiresServerEvaluation,
    match_feature_flag_properties,
    FlagDefinitionCacheData,
    FlagDefinitionCacheProvider,
from posthog.poller import Poller
    DEFAULT_HOST,
    QuotaLimitError,
    RequestsConnectionError,
    RequestsTimeout,
    batch_post,
    determine_server_host,
    get,
    remote_config,
    FeatureFlagError,
    FeatureFlagResult,
    FlagMetadata,
    FlagsResponse,
    FlagValue,
    SendFeatureFlagsOptions,
    normalize_flags_response,
    to_flags_and_payloads,
    to_payloads,
    to_values,
from posthog.utils import (
    FlagCache,
    RedisFlagCache,
    SizeLimitedDict,
    clean,
    guess_timezone,
    system_context,
    import Queue as queue
MAX_DICT_SIZE = 50_000
def get_identity_state(passed) -> tuple[str, bool]:
    """Returns the distinct id to use, and whether this is a personless event or not"""
    stringified = stringify_id(passed)
    if stringified and len(stringified):
        return (stringified, False)
    context_id = get_context_distinct_id()
    if context_id:
        return (context_id, False)
    return (str(uuid4()), True)
def add_context_tags(properties):
    properties = properties or {}
    current_context = _get_current_context()
    if current_context:
        context_tags = current_context.collect_tags()
        properties["$context_tags"] = set(context_tags.keys())
        # We want explicitly passed properties to override context tags
        context_tags.update(properties)
        properties = context_tags
    if "$session_id" not in properties and get_context_session_id():
        properties["$session_id"] = get_context_session_id()
    return properties
def no_throw(default_return=None):
    Decorator to prevent raising exceptions from public API methods.
    Note that this doesn't prevent errors from propagating via `on_error`.
    Exceptions will still be raised if the debug flag is enabled.
        default_return: Value to return on exception (default: None)
                self.log.exception(f"Error in {func.__name__}: {e}")
                return default_return
class Client(object):
    This is the SDK reference for the PostHog Python SDK.
    You can learn more about example usage in the [Python SDK documentation](/docs/libraries/python).
    You can also follow [Flask](/docs/libraries/flask) and [Django](/docs/libraries/django)
    guides to integrate PostHog into your project.
        from posthog import Posthog
        posthog = Posthog('<ph_project_api_key>', host='<ph_client_api_host>')
        posthog.debug = True
        if settings.TEST:
            posthog.disabled = True
    log = logging.getLogger("posthog")
        project_api_key: str,
        host=None,
        max_queue_size=10000,
        send=True,
        on_error=None,
        flush_at=100,
        flush_interval=0.5,
        gzip=False,
        sync_mode=False,
        timeout=15,
        thread=1,
        poll_interval=30,
        personal_api_key=None,
        disabled=False,
        disable_geoip=True,
        historical_migration=False,
        feature_flags_request_timeout_seconds=3,
        super_properties=None,
        enable_exception_autocapture=False,
        log_captured_exceptions=False,
        project_root=None,
        privacy_mode=False,
        before_send=None,
        flag_fallback_cache_url=None,
        enable_local_evaluation=True,
        flag_definition_cache_provider: Optional[FlagDefinitionCacheProvider] = None,
        capture_exception_code_variables=False,
        code_variables_mask_patterns=None,
        code_variables_ignore_patterns=None,
        in_app_modules: list[str] | None = None,
        Initialize a new PostHog client instance.
            project_api_key: The project API key.
            host: The host to use for the client.
            debug: Whether to enable debug mode.
            posthog = Posthog('<ph_project_api_key>', host='<ph_app_host>')
            Initialization
        self.queue = queue.Queue(max_queue_size)
        # api_key: This should be the Team API Key (token), public
        self.api_key = project_api_key
        self.on_error = on_error
        self.send = send
        self.sync_mode = sync_mode
        # Used for session replay URL generation - we don't want the server host here.
        self.raw_host = host or DEFAULT_HOST
        self.host = determine_server_host(host)
        self.gzip = gzip
        self._feature_flags = None  # private variable to store flags
        self.feature_flags_by_key = None
        self.group_type_mapping: Optional[dict[str, str]] = None
        self.cohorts: Optional[dict[str, Any]] = None
        self.poll_interval = poll_interval
        self.feature_flags_request_timeout_seconds = (
            feature_flags_request_timeout_seconds
        self.poller = None
        self.distinct_ids_feature_flags_reported = SizeLimitedDict(MAX_DICT_SIZE, set)
        self.flag_cache = self._initialize_flag_cache(flag_fallback_cache_url)
        self.flag_definition_version = 0
        self._flags_etag: Optional[str] = None
        self._flag_definition_cache_provider = flag_definition_cache_provider
        self.disable_geoip = disable_geoip
        self.historical_migration = historical_migration
        self.super_properties = super_properties
        self.enable_exception_autocapture = enable_exception_autocapture
        self.log_captured_exceptions = log_captured_exceptions
        self.exception_capture = None
        self.privacy_mode = privacy_mode
        self.enable_local_evaluation = enable_local_evaluation
        self.capture_exception_code_variables = capture_exception_code_variables
        self.code_variables_mask_patterns = (
            code_variables_mask_patterns
            if code_variables_mask_patterns is not None
            else DEFAULT_CODE_VARIABLES_MASK_PATTERNS
        self.code_variables_ignore_patterns = (
            code_variables_ignore_patterns
            if code_variables_ignore_patterns is not None
            else DEFAULT_CODE_VARIABLES_IGNORE_PATTERNS
        self.in_app_modules = in_app_modules
        if project_root is None:
                project_root = os.getcwd()
                project_root = None
        self.project_root = project_root
        # personal_api_key: This should be a generated Personal API Key, private
        self.personal_api_key = personal_api_key
            # Ensures that debug level messages are logged when debug mode is on.
            # Otherwise, defaults to WARNING level. See https://docs.python.org/3/howto/logging.html#what-happens-if-no-configuration-is-provided
            self.log.setLevel(logging.DEBUG)
            self.log.setLevel(logging.WARNING)
        if before_send is not None:
            if callable(before_send):
                self.before_send = before_send
                self.log.warning("before_send is not callable, it will be ignored")
                self.before_send = None
        if self.enable_exception_autocapture:
            self.exception_capture = ExceptionCapture(self)
        if sync_mode:
            self.consumers = None
            # On program exit, allow the consumer thread to exit cleanly.
            # This prevents exceptions and a messy shutdown when the
            # interpreter is destroyed before the daemon thread finishes
            # execution. However, it is *not* the same as flushing the queue!
            # To guarantee all messages have been delivered, you'll still need
            # to call flush().
            if send:
                atexit.register(self.join)
            self.consumers = []
            for _ in range(thread):
                consumer = Consumer(
                    self.queue,
                    self.api_key,
                    host=self.host,
                    flush_at=flush_at,
                    flush_interval=flush_interval,
                    gzip=gzip,
                    retries=max_retries,
                    historical_migration=historical_migration,
                self.consumers.append(consumer)
                # if we've disabled sending, just don't start the consumer
                    consumer.start()
    def new_context(self, fresh=False, capture_exceptions=True):
        Create a new context for managing shared state. Learn more about [contexts](/docs/libraries/python#contexts).
            fresh: Whether to create a fresh context that doesn't inherit from parent.
            capture_exceptions: Whether to automatically capture exceptions in this context.
            with posthog.new_context():
                identify_context('<distinct_id>')
                posthog.capture('event_name')
        return new_context(
            fresh=fresh, capture_exceptions=capture_exceptions, client=self
    def feature_flags(self):
        Get the local evaluation feature flags.
        return self._feature_flags
    @feature_flags.setter
    def feature_flags(self, flags):
        Set the local evaluation feature flags.
        self._feature_flags = flags or []
        self.feature_flags_by_key = {
            flag["key"]: flag
            for flag in self._feature_flags
            if flag.get("key") is not None
        assert self.feature_flags_by_key is not None, (
            "feature_flags_by_key should be initialized when feature_flags is set"
    def get_feature_variants(
        person_properties=None,
        group_properties=None,
        disable_geoip=None,
        flag_keys_to_evaluate: Optional[list[str]] = None,
        device_id: Optional[str] = None,
    ) -> dict[str, Union[bool, str]]:
        Get feature flag variants for a user by calling decide.
            distinct_id: The distinct ID of the user.
            groups: A dictionary of group information.
            person_properties: A dictionary of person properties.
            group_properties: A dictionary of group properties.
            disable_geoip: Whether to disable GeoIP for this request.
            flag_keys_to_evaluate: A list of specific flag keys to evaluate. If provided,
                only these flags will be evaluated, improving performance.
            device_id: The device ID for this request.
        resp_data = self.get_flags_decision(
            person_properties,
            group_properties,
            disable_geoip,
            flag_keys_to_evaluate,
        return to_values(resp_data) or {}
    def get_feature_payloads(
        Get feature flag payloads for a user by calling decide.
            payloads = posthog.get_feature_payloads('<distinct_id>')
        return to_payloads(resp_data) or {}
    def get_feature_flags_and_payloads(
        Get feature flags and payloads for a user by calling decide.
            result = posthog.get_feature_flags_and_payloads('<distinct_id>')
        resp = self.get_flags_decision(
        return to_flags_and_payloads(resp)
    def get_flags_decision(
        distinct_id: Optional[ID_TYPES] = None,
        groups: Optional[dict] = None,
    ) -> FlagsResponse:
        Get feature flags decision.
            decision = posthog.get_flags_decision('user123')
        groups = groups or {}
        person_properties = person_properties or {}
        group_properties = group_properties or {}
        if distinct_id is None:
            distinct_id = get_context_distinct_id()
        if device_id is None:
            device_id = get_context_device_id()
        if disable_geoip is None:
            disable_geoip = self.disable_geoip
        if not groups:
        request_data = {
            "distinct_id": distinct_id,
            "groups": groups,
            "person_properties": person_properties,
            "group_properties": group_properties,
            "geoip_disable": disable_geoip,
            "device_id": device_id,
        if flag_keys_to_evaluate:
            request_data["flag_keys_to_evaluate"] = flag_keys_to_evaluate
        resp_data = flags(
            self.host,
            timeout=self.feature_flags_request_timeout_seconds,
            **request_data,
        return normalize_flags_response(resp_data)
    @no_throw()
    def capture(
        self, event: str, **kwargs: Unpack[OptionalCaptureArgs]
        Captures an event manually. [Learn about capture best practices](https://posthog.com/docs/product-analytics/capture-events)
            event: The event name to capture.
            properties: A dictionary of properties to include with the event.
            timestamp: The timestamp of the event.
            uuid: A unique identifier for the event.
            send_feature_flags: Whether to send feature flags with the event.
            disable_geoip: Whether to disable GeoIP for this event.
            # Anonymous event
            posthog.capture('some-anon-event')
            # Context usage
            from posthog import identify_context, new_context
                identify_context('distinct_id_of_the_user')
                posthog.capture('user_signed_up')
                posthog.capture('user_logged_in')
                posthog.capture('some-custom-action', distinct_id='distinct_id_of_the_user')
            posthog.capture(
            # Page view event
            posthog.capture('$pageview', distinct_id="distinct_id_of_the_user", properties={'$current_url': 'https://example.com'})
            Capture
        distinct_id = kwargs.get("distinct_id", None)
        properties = kwargs.get("properties", None)
        timestamp = kwargs.get("timestamp", None)
        uuid = kwargs.get("uuid", None)
        groups = kwargs.get("groups", None)
        send_feature_flags = kwargs.get("send_feature_flags", False)
        disable_geoip = kwargs.get("disable_geoip", None)
        properties = {**(properties or {}), **system_context()}
        properties = add_context_tags(properties)
        assert properties is not None  # Type hint for mypy
        (distinct_id, personless) = get_identity_state(distinct_id)
        if personless and "$process_person_profile" not in properties:
            properties["$process_person_profile"] = False
        msg = {
            "properties": properties,
            "timestamp": timestamp,
            "event": event,
            "uuid": uuid,
        if groups:
            properties["$groups"] = groups
        extra_properties: dict[str, Any] = {}
        feature_variants: Optional[dict[str, Union[bool, str]]] = {}
        # Parse and normalize send_feature_flags parameter
        flag_options = self._parse_send_feature_flags(send_feature_flags)
        if flag_options["should_send"]:
                if flag_options["only_evaluate_locally"] is True:
                    # Local evaluation explicitly requested
                    feature_variants = self.get_all_flags(
                        groups=(groups or {}),
                        person_properties=flag_options["person_properties"],
                        group_properties=flag_options["group_properties"],
                        only_evaluate_locally=True,
                        flag_keys_to_evaluate=flag_options["flag_keys_filter"],
                elif flag_options["only_evaluate_locally"] is False:
                    # Remote evaluation explicitly requested
                    feature_variants = self.get_feature_variants(
                elif self.feature_flags:
                    # Local flags available, prefer local evaluation
                    # Fall back to remote evaluation
                self.log.exception(
                    f"[FEATURE FLAGS] Unable to get feature variants: {e}"
        for feature, variant in (feature_variants or {}).items():
            extra_properties[f"$feature/{feature}"] = variant
        active_feature_flags = [
            for (key, value) in (feature_variants or {}).items()
            if value is not False
        if active_feature_flags:
            extra_properties["$active_feature_flags"] = active_feature_flags
        if extra_properties:
            properties = {**extra_properties, **properties}
            msg["properties"] = properties
        return self._enqueue(msg, disable_geoip)
    def _parse_send_feature_flags(self, send_feature_flags) -> SendFeatureFlagsOptions:
        Parse and normalize send_feature_flags parameter into a standard format.
            send_feature_flags: Either bool or SendFeatureFlagsOptions dict
            SendFeatureFlagsOptions: Normalized options with keys: should_send, only_evaluate_locally,
                  person_properties, group_properties, flag_keys_filter
            TypeError: If send_feature_flags is not bool or dict
        if isinstance(send_feature_flags, dict):
                "should_send": True,
                "only_evaluate_locally": send_feature_flags.get(
                    "only_evaluate_locally"
                "person_properties": send_feature_flags.get("person_properties"),
                "group_properties": send_feature_flags.get("group_properties"),
                "flag_keys_filter": send_feature_flags.get("flag_keys_filter"),
        elif isinstance(send_feature_flags, bool):
                "should_send": send_feature_flags,
                "only_evaluate_locally": None,
                "person_properties": None,
                "group_properties": None,
                "flag_keys_filter": None,
                f"Invalid type for send_feature_flags: {type(send_feature_flags)}. "
                f"Expected bool or dict."
    def set(self, **kwargs: Unpack[OptionalSetArgs]) -> Optional[str]:
        Set properties on a person profile.
            properties: A dictionary of properties to set.
            # Set with distinct id
            posthog.set(distinct_id='user123', properties={'name': 'Max Hedgehog'})
        Note: This method will not raise exceptions. Errors are logged.
        if personless or not properties:
            return None  # Personless set() does nothing
            "$set": properties,
            "event": "$set",
    def set_once(self, **kwargs: Unpack[OptionalSetArgs]) -> Optional[str]:
        Set properties on a person profile only if they haven't been set before.
            properties: A dictionary of properties to set once.
            posthog.set_once(distinct_id='user123', properties={'initial_signup_date': '2024-01-01'})
            return None  # Personless set_once() does nothing
            "$set_once": properties,
            "event": "$set_once",
        group_type: str,
        group_key: str,
        properties: Optional[Dict[str, Any]] = None,
        timestamp: Optional[Union[datetime, str]] = None,
        uuid: Optional[str] = None,
        disable_geoip: Optional[bool] = None,
        Identify a group and set its properties.
            group_type: The type of group (e.g., 'company', 'team').
            group_key: The unique identifier for the group.
            properties: A dictionary of properties to set on the group.
            distinct_id: The distinct ID of the user performing the action.
            posthog.group_identify('company', 'company_id_in_your_db', {
        # group_identify is purposefully always personful
        distinct_id = get_identity_state(distinct_id)[0]
        msg: Dict[str, Any] = {
            "event": "$groupidentify",
                "$group_type": group_type,
                "$group_key": group_key,
                "$group_set": properties,
        # NOTE - group_identify doesn't generally use context properties - should it?
        if get_context_session_id():
            msg["properties"]["$session_id"] = str(get_context_session_id())
        previous_id: str,
        distinct_id: Optional[str],
        uuid=None,
        Create an alias between two distinct IDs.
            previous_id: The previous distinct ID.
            distinct_id: The new distinct ID to alias to.
            posthog.alias(previous_id='distinct_id', distinct_id='alias_id')
        if personless:
            return None  # Personless alias() does nothing - should this throw?
                "distinct_id": previous_id,
                "alias": distinct_id,
            "event": "$create_alias",
        exception: Optional[ExceptionArg],
        Capture an exception for error tracking.
            exception: The exception to capture.
            properties: A dictionary of additional properties.
            send_feature_flags: Whether to send feature flags with the exception.
                # Some code that might fail
                posthog.capture_exception(e, 'user_distinct_id', properties=additional_properties)
            Error Tracking
        # this function shouldn't ever throw an error, so it logs exceptions instead of raising them.
        # this is important to ensure we don't unexpectedly re-raise exceptions in the user's code.
            # Check if this exception has already been captured
            if exception is not None and exception_is_already_captured(exception):
                self.log.debug("Exception already captured, skipping")
                exc_info = exc_info_from_error(exception)
                exc_info = sys.exc_info()
            if exc_info is None or exc_info == (None, None, None):
                self.log.warning("No exception information available")
            # Format stack trace for cymbal
            all_exceptions_with_trace = exceptions_from_error_tuple(exc_info)
            # Add in-app property to frames in the exceptions
            event = handle_in_app(
                    "exception": {
                        "values": all_exceptions_with_trace,
                in_app_include=self.in_app_modules,
                project_root=self.project_root,
            all_exceptions_with_trace_and_in_app = event["exception"]["values"]
                "$exception_list": all_exceptions_with_trace_and_in_app,
                **properties,
            context_enabled = get_capture_exception_code_variables_context()
            context_mask = get_code_variables_mask_patterns_context()
            context_ignore = get_code_variables_ignore_patterns_context()
            enabled = (
                context_enabled
                if context_enabled is not None
                else self.capture_exception_code_variables
            mask_patterns = (
                context_mask
                if context_mask is not None
                else self.code_variables_mask_patterns
            ignore_patterns = (
                context_ignore
                if context_ignore is not None
                else self.code_variables_ignore_patterns
            if enabled:
                try_attach_code_variables_to_frames(
                    all_exceptions_with_trace_and_in_app,
                    exc_info,
                    mask_patterns=mask_patterns,
                    ignore_patterns=ignore_patterns,
            if self.log_captured_exceptions:
                self.log.exception(exception, extra=kwargs)
            res = self.capture(
                "$exception",
                groups=groups,
                send_feature_flags=send_feature_flags,
            # Mark the exception as captured to prevent duplicate captures
            if exception is not None and res is not None:
                mark_exception_as_captured(exception, res)
            self.log.exception(f"Failed to capture exception: {e}")
    def _enqueue(self, msg, disable_geoip):
        """Push a new `msg` onto the queue, return `(success, msg)`"""
        timestamp = msg["timestamp"]
        if timestamp is None:
            timestamp = datetime.now(tz=tzutc())
        # add common
        timestamp = guess_timezone(timestamp)
        msg["timestamp"] = timestamp.isoformat()
        if "uuid" in msg:
            uuid = msg.pop("uuid")
            if uuid:
                msg["uuid"] = stringify_id(uuid)
        if "uuid" not in msg:
            # Always send a uuid, so we can always return one
            msg["uuid"] = stringify_id(uuid4())
        sent_uuid = msg["uuid"]
        if not msg.get("properties"):
            msg["properties"] = {}
        msg["properties"]["$lib"] = "posthog-python"
        msg["properties"]["$lib_version"] = VERSION
        if disable_geoip:
            msg["properties"]["$geoip_disable"] = True
        if self.super_properties:
            msg["properties"] = {**msg["properties"], **self.super_properties}
        msg["distinct_id"] = stringify_id(msg.get("distinct_id", None))
        msg = clean(msg)
        if self.before_send:
                modified_msg = self.before_send(msg)
                if modified_msg is None:
                    self.log.debug("Event dropped by before_send callback")
                msg = modified_msg
                self.log.exception(f"Error in before_send callback: {e}")
                # Continue with the original message if callback fails
        self.log.debug("queueing: %s", msg)
        # if send is False, return msg as if it was successfully queued
        if not self.send:
            return sent_uuid
        if self.sync_mode:
            self.log.debug("enqueued with blocking %s.", msg["event"])
            batch_post(
                gzip=self.gzip,
                batch=[msg],
                historical_migration=self.historical_migration,
            self.queue.put(msg, block=False)
            self.log.debug("enqueued %s.", msg["event"])
        except queue.Full:
            self.log.warning("analytics-python queue is full")
        Force a flush from the internal queue to the server. Do not use directly, call `shutdown()` instead.
            posthog.flush()  # Ensures the event is sent immediately
        queue = self.queue
        size = queue.qsize()
        queue.join()
        # Note that this message may not be precise, because of threading.
        self.log.debug("successfully flushed about %s items.", size)
    def join(self):
        End the consumer thread once the queue is empty. Do not use directly, call `shutdown()` instead.
            posthog.join()
        if self.consumers:
            for consumer in self.consumers:
                consumer.pause()
                    consumer.join()
                    # consumer thread has not started
        if self.poller:
            self.poller.stop()
        # Shutdown the cache provider (release locks, cleanup)
        if self._flag_definition_cache_provider:
                self._flag_definition_cache_provider.shutdown()
                self.log.error(f"[FEATURE FLAGS] Cache provider shutdown error: {e}")
    def shutdown(self):
        Flush all messages and cleanly shutdown the client. Call this before the process ends in serverless environments to avoid data loss.
            posthog.shutdown()
        self.join()
        if self.exception_capture:
            self.exception_capture.close()
    def _update_flag_state(
        self, data: FlagDefinitionCacheData, old_flags_by_key: Optional[dict] = None
        """Update internal flag state from cache data and invalidate evaluation cache if changed."""
        self.feature_flags = data["flags"]
        self.group_type_mapping = data["group_type_mapping"]
        self.cohorts = data["cohorts"]
        # Invalidate evaluation cache if flag definitions changed
            self.flag_cache
            and old_flags_by_key is not None
            and old_flags_by_key != (self.feature_flags_by_key or {})
            old_version = self.flag_definition_version
            self.flag_definition_version += 1
            self.flag_cache.invalidate_version(old_version)
    def _load_feature_flags(self):
        should_fetch = True
                should_fetch = (
                    self._flag_definition_cache_provider.should_fetch_flag_definitions()
                self.log.error(
                    f"[FEATURE FLAGS] Cache provider should_fetch error: {e}"
                # Fail-safe: fetch from API if cache provider errors
        # If not fetching, try to get from cache
        if not should_fetch and self._flag_definition_cache_provider:
                cached_data = (
                    self._flag_definition_cache_provider.get_flag_definitions()
                if cached_data:
                    self.log.debug(
                        "[FEATURE FLAGS] Using cached flag definitions from external cache"
                    self._update_flag_state(
                        cached_data, old_flags_by_key=self.feature_flags_by_key or {}
                    self._last_feature_flag_poll = datetime.now(tz=tzutc())
                    # Emergency fallback: if cache is empty and we have no flags, fetch anyway.
                    # There's really no other way of recovering in this case.
                    if not self.feature_flags:
                            "[FEATURE FLAGS] Cache empty and no flags loaded, falling back to API fetch"
                self.log.error(f"[FEATURE FLAGS] Cache provider get error: {e}")
        if should_fetch:
            self._fetch_feature_flags_from_api()
    def _fetch_feature_flags_from_api(self):
        """Fetch feature flags from the PostHog API."""
            # Store old flags to detect changes
            old_flags_by_key: dict[str, dict] = self.feature_flags_by_key or {}
            response = get(
                self.personal_api_key,
                f"/api/feature_flag/local_evaluation/?token={self.api_key}&send_cohorts",
                etag=self._flags_etag,
            # Update stored ETag (clear if server stops sending one)
            self._flags_etag = response.etag
            # If 304 Not Modified, flags haven't changed - skip processing
            if response.not_modified:
                    "[FEATURE FLAGS] Flags not modified (304), using cached data"
            if response.data is None:
                    "[FEATURE FLAGS] Unexpected empty response data in non-304 response"
            self._update_flag_state(response.data, old_flags_by_key=old_flags_by_key)
            # Store in external cache if provider is configured
                    self._flag_definition_cache_provider.on_flag_definitions_received(
                            "flags": self.feature_flags or [],
                            "group_type_mapping": self.group_type_mapping or {},
                            "cohorts": self.cohorts or {},
                    self.log.error(f"[FEATURE FLAGS] Cache provider store error: {e}")
                    # Flags are already in memory, so continue normally
        except APIError as e:
            if e.status == 401:
                    "[FEATURE FLAGS] Error loading feature flags: To use feature flags, please set a valid personal_api_key. More information: https://posthog.com/docs/api/overview"
                self.feature_flags = []
                self.group_type_mapping = {}
                self.cohorts = {}
                if self.flag_cache:
                    self.flag_cache.clear()
                        status=401,
                        message="You are using a write-only key with feature flags. "
                        "To use feature flags, please set a personal_api_key "
                        "More information: https://posthog.com/docs/api/overview",
            elif e.status == 402:
                self.log.warning(
                    "[FEATURE FLAGS] PostHog feature flags quota limited, resetting feature flag data.  Learn more about billing limits at https://posthog.com/docs/billing/limits-alerts"
                # Reset all feature flag data when quota limited
                # Clear flag cache when quota limited
                        status=402,
                        message="PostHog feature flags quota limited",
                self.log.error(f"[FEATURE FLAGS] Error loading feature flags: {e}")
                "[FEATURE FLAGS] Fetching feature flags failed with following error. We will retry in %s seconds."
                % self.poll_interval
            self.log.warning(e)
    def load_feature_flags(self):
        Load feature flags for local evaluation.
            posthog.load_feature_flags()
        if not self.personal_api_key:
                "[FEATURE FLAGS] You have to specify a personal_api_key to use feature flags."
        self._load_feature_flags()
        # Only start the poller if local evaluation is enabled
        if self.enable_local_evaluation and not (
            self.poller and self.poller.is_alive()
            self.poller = Poller(
                interval=timedelta(seconds=self.poll_interval),
                execute=self._load_feature_flags,
            self.poller.start()
    def _compute_flag_locally(
        feature_flag,
        warn_on_unknown_groups=True,
    ) -> FlagValue:
        # Create evaluation cache for flag dependencies
        evaluation_cache: dict[str, Optional[FlagValue]] = {}
        if feature_flag.get("ensure_experience_continuity", False):
            raise InconclusiveMatchError("Flag has experience continuity enabled")
        if not feature_flag.get("active"):
        flag_filters = feature_flag.get("filters") or {}
        aggregation_group_type_index = flag_filters.get("aggregation_group_type_index")
        if aggregation_group_type_index is not None:
            group_type_mapping = self.group_type_mapping or {}
            group_name = group_type_mapping.get(str(aggregation_group_type_index))
            if not group_name:
                    f"[FEATURE FLAGS] Unknown group type index {aggregation_group_type_index} for feature flag {feature_flag['key']}"
                # failover to `/flags`
                raise InconclusiveMatchError("Flag has unknown group type index")
            if group_name not in groups:
                # Group flags are never enabled in `groups` aren't passed in
                # don't failover to `/flags`, since response will be the same
                if warn_on_unknown_groups:
                        f"[FEATURE FLAGS] Can't compute group feature flag: {feature_flag['key']} without group names passed in"
            focused_group_properties = group_properties[group_name]
            return match_feature_flag_properties(
                groups[group_name],
                focused_group_properties,
                self.feature_flags_by_key,
                evaluation_cache,
                self.cohorts,
        Check if a feature flag is enabled for a user.
            key: The feature flag key.
            only_evaluate_locally: Whether to only evaluate locally.
            send_feature_flag_events: Whether to send feature flag events.
            is_my_flag_enabled = posthog.feature_enabled('flag-key', 'distinct_id_of_your_user')
                # Do something differently for this user
                # Optional: fetch the payload
                matched_flag_payload = posthog.get_feature_flag_payload('flag-key', 'distinct_id_of_your_user')
        response = self.get_feature_flag(
            person_properties=person_properties,
            group_properties=group_properties,
        return bool(response)
    def _get_stale_flag_fallback(
        self, distinct_id: ID_TYPES, key: str
    ) -> Optional[FeatureFlagResult]:
        """Returns a stale cached flag value if available, otherwise None."""
            stale_result = self.flag_cache.get_stale_cached_flag(distinct_id, key)
            if stale_result:
                self.log.info(
                    f"[FEATURE FLAGS] Using stale cached value for flag {key}"
                return stale_result
    def _get_feature_flag_result(
        distinct_id: ID_TYPES,
        override_match_value: Optional[FlagValue] = None,
        groups: Optional[Dict[str, str]] = None,
        person_properties, group_properties = (
            self._add_local_person_and_group_properties(
                groups or {},
                person_properties or {},
                group_properties or {},
        # Ensure non-None values for type checking
        flag_result = None
        flag_details = None
        request_id = None
        evaluated_at = None
        feature_flag_error: Optional[str] = None
        flag_value = self._locally_evaluate_flag(
            key, distinct_id, groups, person_properties, group_properties
        flag_was_locally_evaluated = flag_value is not None
        if flag_was_locally_evaluated:
            lookup_match_value = override_match_value or flag_value
            payload = (
                self._compute_payload_locally(key, lookup_match_value)
                if lookup_match_value is not None
            flag_result = FeatureFlagResult.from_value_and_payload(
                key, lookup_match_value, payload
            # Cache successful local evaluation
            if self.flag_cache and flag_result:
                self.flag_cache.set_cached_flag(
                    distinct_id, key, flag_result, self.flag_definition_version
        elif not only_evaluate_locally:
                flag_details, request_id, evaluated_at, errors_while_computing = (
                    self._get_feature_flag_details_from_server(
                if errors_while_computing:
                    errors.append(FeatureFlagError.ERRORS_WHILE_COMPUTING)
                if flag_details is None:
                    errors.append(FeatureFlagError.FLAG_MISSING)
                    feature_flag_error = ",".join(errors)
                flag_result = FeatureFlagResult.from_flag_details(
                    flag_details, override_match_value
                # Cache successful remote evaluation
                    f"Successfully computed flag remotely: #{key} -> #{flag_result}"
            except QuotaLimitError as e:
                self.log.warning(f"[FEATURE FLAGS] Quota limit exceeded: {e}")
                feature_flag_error = FeatureFlagError.QUOTA_LIMITED
                flag_result = self._get_stale_flag_fallback(distinct_id, key)
            except RequestsTimeout as e:
                self.log.warning(f"[FEATURE FLAGS] Request timed out: {e}")
                feature_flag_error = FeatureFlagError.TIMEOUT
            except RequestsConnectionError as e:
                self.log.warning(f"[FEATURE FLAGS] Connection error: {e}")
                feature_flag_error = FeatureFlagError.CONNECTION_ERROR
                self.log.warning(f"[FEATURE FLAGS] API error: {e}")
                feature_flag_error = FeatureFlagError.api_error(e.status)
                self.log.exception(f"[FEATURE FLAGS] Unable to get flag remotely: {e}")
                feature_flag_error = FeatureFlagError.UNKNOWN_ERROR
        if send_feature_flag_events:
            self._capture_feature_flag_called(
                flag_result.get_value() if flag_result else None,
                flag_result.payload if flag_result else None,
                flag_was_locally_evaluated,
                request_id,
                evaluated_at,
                flag_details,
                feature_flag_error,
        return flag_result
        Get a FeatureFlagResult object which contains the flag result and payload for a key by evaluating locally or remotely
        depending on whether local evaluation is enabled and the flag can be locally evaluated.
        This also captures the `$feature_flag_called` event unless `send_feature_flag_events` is `False`.
            flag_result = posthog.get_feature_flag_result('flag-key', 'distinct_id_of_your_user')
            if flag_result and flag_result.get_value() == 'variant-key':
                matched_flag_payload = flag_result.payload
            Optional[FeatureFlagResult]: The feature flag result or None if disabled/not found.
        return self._get_feature_flag_result(
    ) -> Optional[FlagValue]:
        Get multivariate feature flag value for a user.
            enabled_variant = posthog.get_feature_flag('flag-key', 'distinct_id_of_your_user')
            if enabled_variant == 'variant-key': # replace 'variant-key' with the key of your variant
        feature_flag_result = self.get_feature_flag_result(
        return feature_flag_result.get_value() if feature_flag_result else None
    def _locally_evaluate_flag(
        groups: dict[str, str],
        person_properties: dict[str, str],
        group_properties: dict[str, str],
        if self.feature_flags is None and self.personal_api_key:
            self.load_feature_flags()
        if self.feature_flags:
            # Local evaluation
            flag = self.feature_flags_by_key.get(key)
            if flag:
                    response = self._compute_flag_locally(
                        f"Successfully computed flag locally: {key} -> {response}"
                except (RequiresServerEvaluation, InconclusiveMatchError) as e:
                    self.log.debug(f"Failed to compute flag {key} locally: {e}")
                        f"[FEATURE FLAGS] Error while computing variant locally: {e}"
        match_value: Optional[FlagValue] = None,
        send_feature_flag_events=False,
        Get the payload for a feature flag.
            match_value: The specific flag value to get payload for.
            send_feature_flag_events: Deprecated. Use get_feature_flag() instead if you need events.
                "send_feature_flag_events is deprecated in get_feature_flag_payload() and will be removed "
                "in a future version. Use get_feature_flag() if you want to send $feature_flag_called events.",
        feature_flag_result = self._get_feature_flag_result(
            override_match_value=match_value,
        return feature_flag_result.payload if feature_flag_result else None
    def _get_feature_flag_details_from_server(
        disable_geoip: Optional[bool],
    ) -> tuple[Optional[FeatureFlag], Optional[str], Optional[int], bool]:
        Calls /flags and returns the flag details, request id, evaluated at timestamp,
        and whether there were errors while computing flags.
            flag_keys_to_evaluate=[key],
        request_id = resp_data.get("requestId")
        evaluated_at = resp_data.get("evaluatedAt")
        errors_while_computing = resp_data.get("errorsWhileComputingFlags", False)
        flags = resp_data.get("flags")
        flag_details = flags.get(key) if flags else None
        return flag_details, request_id, evaluated_at, errors_while_computing
    def _capture_feature_flag_called(
        response: Optional[FlagValue],
        payload: Optional[str],
        flag_was_locally_evaluated: bool,
        groups: Dict[str, str],
        request_id: Optional[str],
        evaluated_at: Optional[int],
        flag_details: Optional[FeatureFlag],
        feature_flag_error: Optional[str] = None,
        feature_flag_reported_key = (
            f"{key}_{'::null::' if response is None else str(response)}"
            feature_flag_reported_key
            not in self.distinct_ids_feature_flags_reported[distinct_id]
            properties: dict[str, Any] = {
                "$feature_flag": key,
                "$feature_flag_response": response,
                "locally_evaluated": flag_was_locally_evaluated,
                f"$feature/{key}": response,
            if payload is not None:
                # if payload is not a string, json serialize it to a string
                properties["$feature_flag_payload"] = payload
            if request_id:
                properties["$feature_flag_request_id"] = request_id
            if evaluated_at:
                properties["$feature_flag_evaluated_at"] = evaluated_at
            if isinstance(flag_details, FeatureFlag):
                if flag_details.reason and flag_details.reason.description:
                    properties["$feature_flag_reason"] = flag_details.reason.description
                if isinstance(flag_details.metadata, FlagMetadata):
                    if flag_details.metadata.version:
                        properties["$feature_flag_version"] = (
                            flag_details.metadata.version
                    if flag_details.metadata.id:
                        properties["$feature_flag_id"] = flag_details.metadata.id
            if feature_flag_error:
                properties["$feature_flag_error"] = feature_flag_error
            self.capture(
                "$feature_flag_called",
            self.distinct_ids_feature_flags_reported[distinct_id].add(
    def get_remote_config_payload(self, key: str):
        if self.personal_api_key is None:
                "[FEATURE FLAGS] You have to specify a personal_api_key to fetch decrypted feature flag payloads."
            return remote_config(
                f"[FEATURE FLAGS] Unable to get decrypted feature flag payload: {e}"
    def _compute_payload_locally(
        self, key: str, match_value: FlagValue
        payload = None
        if self.feature_flags_by_key is None:
        flag_definition = self.feature_flags_by_key.get(key)
        if flag_definition:
            flag_filters = flag_definition.get("filters") or {}
            flag_payloads = flag_filters.get("payloads") or {}
            # For boolean flags, convert True to "true"
            # For multivariate flags, use the variant string as-is
            lookup_value = (
                "true"
                if isinstance(match_value, bool) and match_value
                else str(match_value)
            payload = flag_payloads.get(lookup_value, None)
    ) -> Optional[dict[str, Union[bool, str]]]:
        Get all feature flags for a user.
            posthog.get_all_flags('distinct_id_of_your_user')
        response = self.get_all_flags_and_payloads(
            flag_keys_to_evaluate=flag_keys_to_evaluate,
        return response["featureFlags"]
        Get all feature flags and their payloads for a user.
            posthog.get_all_flags_and_payloads('distinct_id_of_your_user')
            return {"featureFlags": None, "featureFlagPayloads": None}
                distinct_id, groups, person_properties, group_properties
        response, fallback_to_flags = self._get_all_flags_and_payloads_locally(
        if fallback_to_flags and not only_evaluate_locally:
                decide_response = self.get_flags_decision(
                return to_flags_and_payloads(decide_response)
                    f"[FEATURE FLAGS] Unable to get feature flags and payloads: {e}"
    def _get_all_flags_and_payloads_locally(
        groups: Dict[str, Union[str, int]],
        warn_on_unknown_groups=False,
    ) -> tuple[FlagsAndPayloads, bool]:
        flags: dict[str, FlagValue] = {}
        payloads: dict[str, str] = {}
        fallback_to_flags = False
        # If loading in previous line failed
            # Filter flags based on flag_keys_to_evaluate if provided
            flags_to_process = self.feature_flags
                flag_keys_set = set(flag_keys_to_evaluate)
                flags_to_process = [
                    flag for flag in self.feature_flags if flag["key"] in flag_keys_set
            for flag in flags_to_process:
                    flags[flag["key"]] = self._compute_flag_locally(
                        warn_on_unknown_groups=warn_on_unknown_groups,
                    matched_payload = self._compute_payload_locally(
                        flag["key"], flags[flag["key"]]
                    if matched_payload is not None:
                        payloads[flag["key"]] = matched_payload
                except InconclusiveMatchError:
                    # No need to log this, since it's just telling us to fall back to `/flags`
                    fallback_to_flags = True
                        f"[FEATURE FLAGS] Error while computing variant and payload: {e}"
            "featureFlags": flags,
            "featureFlagPayloads": payloads,
        }, fallback_to_flags
    def _initialize_flag_cache(self, cache_url):
        """Initialize feature flag cache for graceful degradation during service outages.
        When enabled, the cache stores flag evaluation results and serves them as fallback
        when the PostHog API is unavailable. This ensures your application continues to
        receive flag values even during outages.
            cache_url: Cache configuration URL. Examples:
                - None: Disable caching
                - "memory://local/?ttl=300&size=10000": Memory cache with TTL and size
                - "redis://localhost:6379/0/?ttl=300": Redis cache with TTL
                - "redis://username:password@host:port/?ttl=300": Redis with auth
            # Memory cache
            client = Client(
                "your-api-key",
                flag_fallback_cache_url="memory://local/?ttl=300&size=10000"
            # Redis cache
                flag_fallback_cache_url="redis://localhost:6379/0/?ttl=300"
            # Normal evaluation - cache is populated
            flag_value = client.get_feature_flag("my-flag", "user123")
            # During API outage - returns cached value instead of None
            flag_value = client.get_feature_flag("my-flag", "user123")  # Uses cache
        if not cache_url:
            from urllib.parse import parse_qs, urlparse
            from urlparse import parse_qs, urlparse
            parsed = urlparse(cache_url)
            scheme = parsed.scheme.lower()
            query_params = parse_qs(parsed.query)
            ttl = int(query_params.get("ttl", [300])[0])
            if scheme == "memory":
                size = int(query_params.get("size", [10000])[0])
                return FlagCache(size, ttl)
            elif scheme == "redis":
                    # Not worth importing redis if we're not using it
                    import redis
                    redis_url = f"{parsed.scheme}://"
                    if parsed.username or parsed.password:
                        redis_url += f"{parsed.username or ''}:{parsed.password or ''}@"
                    redis_url += (
                        f"{parsed.hostname or 'localhost'}:{parsed.port or 6379}"
                        redis_url += parsed.path
                    client = redis.from_url(redis_url)
                    # Test connection before using it
                    client.ping()
                    return RedisFlagCache(client, default_ttl=ttl)
                        "[FEATURE FLAGS] Redis not available, flag caching disabled"
                        f"[FEATURE FLAGS] Redis connection failed: {e}, flag caching disabled"
                    f"Unknown cache URL scheme: {scheme}. Supported schemes: memory, redis"
                f"[FEATURE FLAGS] Failed to parse cache URL '{cache_url}': {e}"
    def feature_flag_definitions(self):
        return self.feature_flags
    def _add_local_person_and_group_properties(
        self, distinct_id, groups, person_properties, group_properties
        all_person_properties = {
            **(person_properties or {}),
        all_group_properties = {}
            for group_name in groups:
                all_group_properties[group_name] = {
                    "$group_key": groups[group_name],
                    **((group_properties or {}).get(group_name) or {}),
        return all_person_properties, all_group_properties
def stringify_id(val):
    if isinstance(val, string_types):
    return str(val)
"""HTTP Client for asyncio."""
    Final,
import attr
from multidict import CIMultiDict, MultiDict, MultiDictProxy, istr
from . import hdrs, http, payload
from ._websocket.reader import WebSocketDataQueue
from .abc import AbstractCookieJar
from .client_exceptions import (
from .client_middlewares import ClientMiddlewareType, build_client_middlewares
from .client_reqrep import (
    ClientRequest as ClientRequest,
    ClientResponse as ClientResponse,
    Fingerprint as Fingerprint,
    RequestInfo as RequestInfo,
    _merge_ssl_params,
from .client_ws import (
    DEFAULT_WS_CLIENT_TIMEOUT,
    ClientWebSocketResponse as ClientWebSocketResponse,
    ClientWSTimeout as ClientWSTimeout,
    HTTP_AND_EMPTY_SCHEMA_SET,
    BaseConnector as BaseConnector,
    NamedPipeConnector as NamedPipeConnector,
    TCPConnector as TCPConnector,
    UnixConnector as UnixConnector,
from .cookiejar import CookieJar
from .helpers import (
    _SENTINEL,
    EMPTY_BODY_METHODS,
    BasicAuth,
    TimeoutHandle,
    basicauth_from_netrc,
    get_env_proxy_for_url,
    netrc_from_env,
    sentinel,
    strip_auth_from_url,
from .http import WS_KEY, HttpVersion, WebSocketReader, WebSocketWriter
from .http_websocket import WSHandshakeError, ws_ext_gen, ws_ext_parse
from .tracing import Trace, TraceConfig
from .typedefs import JSONEncoder, LooseCookies, LooseHeaders, Query, StrOrURL
    # client_exceptions
    # client_reqrep
    # connector
    # client_ws
    SSLContext = None
if sys.version_info >= (3, 11) and TYPE_CHECKING:
class _RequestOptions(TypedDict, total=False):
    json: Any
    cookies: Union[LooseCookies, None]
    headers: Union[LooseHeaders, None]
    skip_auto_headers: Union[Iterable[str], None]
    auth: Union[BasicAuth, None]
    allow_redirects: bool
    max_redirects: int
    compress: Union[str, bool, None]
    chunked: Union[bool, None]
    expect100: bool
    raise_for_status: Union[None, bool, Callable[[ClientResponse], Awaitable[None]]]
    read_until_eof: bool
    proxy: Union[StrOrURL, None]
    proxy_auth: Union[BasicAuth, None]
    timeout: "Union[ClientTimeout, _SENTINEL, None]"
    ssl: Union[SSLContext, bool, Fingerprint]
    server_hostname: Union[str, None]
    proxy_headers: Union[LooseHeaders, None]
    trace_request_ctx: Union[Mapping[str, Any], None]
    read_bufsize: Union[int, None]
    auto_decompress: Union[bool, None]
    max_line_size: Union[int, None]
    max_field_size: Union[int, None]
    middlewares: Optional[Sequence[ClientMiddlewareType]]
@attr.s(auto_attribs=True, frozen=True, slots=True)
class ClientTimeout:
    total: Optional[float] = None
    connect: Optional[float] = None
    sock_read: Optional[float] = None
    sock_connect: Optional[float] = None
    ceil_threshold: float = 5
    # pool_queue_timeout: Optional[float] = None
    # dns_resolution_timeout: Optional[float] = None
    # socket_connect_timeout: Optional[float] = None
    # connection_acquiring_timeout: Optional[float] = None
    # new_connection_timeout: Optional[float] = None
    # http_header_timeout: Optional[float] = None
    # response_body_timeout: Optional[float] = None
    # to create a timeout specific for a single request, either
    # - create a completely new one to overwrite the default
    # - or use http://www.attrs.org/en/stable/api.html#attr.evolve
    # to overwrite the defaults
# 5 Minute default read timeout
DEFAULT_TIMEOUT: Final[ClientTimeout] = ClientTimeout(total=5 * 60, sock_connect=30)
# https://www.rfc-editor.org/rfc/rfc9110#section-9.2.2
IDEMPOTENT_METHODS = frozenset({"GET", "HEAD", "OPTIONS", "TRACE", "PUT", "DELETE"})
_RetType = TypeVar("_RetType", ClientResponse, ClientWebSocketResponse)
_CharsetResolver = Callable[[ClientResponse, bytes], str]
class ClientSession:
    """First-class interface for making HTTP requests."""
    ATTRS = frozenset(
            "_base_url",
            "_base_url_origin",
            "_source_traceback",
            "_connector",
            "_loop",
            "_cookie_jar",
            "_connector_owner",
            "_default_auth",
            "_version",
            "_json_serialize",
            "_requote_redirect_url",
            "_timeout",
            "_raise_for_status",
            "_auto_decompress",
            "_trust_env",
            "_default_headers",
            "_skip_auto_headers",
            "_request_class",
            "_response_class",
            "_ws_response_class",
            "_trace_configs",
            "_read_bufsize",
            "_max_line_size",
            "_max_field_size",
            "_resolve_charset",
            "_default_proxy",
            "_default_proxy_auth",
            "_retry_connection",
            "_middlewares",
            "requote_redirect_url",
    _source_traceback: Optional[traceback.StackSummary] = None
    _connector: Optional[BaseConnector] = None
        base_url: Optional[StrOrURL] = None,
        connector: Optional[BaseConnector] = None,
        cookies: Optional[LooseCookies] = None,
        headers: Optional[LooseHeaders] = None,
        proxy: Optional[StrOrURL] = None,
        proxy_auth: Optional[BasicAuth] = None,
        skip_auto_headers: Optional[Iterable[str]] = None,
        auth: Optional[BasicAuth] = None,
        json_serialize: JSONEncoder = json.dumps,
        request_class: Type[ClientRequest] = ClientRequest,
        response_class: Type[ClientResponse] = ClientResponse,
        ws_response_class: Type[ClientWebSocketResponse] = ClientWebSocketResponse,
        version: HttpVersion = http.HttpVersion11,
        cookie_jar: Optional[AbstractCookieJar] = None,
        connector_owner: bool = True,
        raise_for_status: Union[
            bool, Callable[[ClientResponse], Awaitable[None]]
        ] = False,
        read_timeout: Union[float, _SENTINEL] = sentinel,
        conn_timeout: Optional[float] = None,
        timeout: Union[object, ClientTimeout] = sentinel,
        auto_decompress: bool = True,
        trust_env: bool = False,
        requote_redirect_url: bool = True,
        trace_configs: Optional[List[TraceConfig]] = None,
        read_bufsize: int = 2**16,
        max_line_size: int = 8190,
        max_field_size: int = 8190,
        fallback_charset_resolver: _CharsetResolver = lambda r, b: "utf-8",
        middlewares: Sequence[ClientMiddlewareType] = (),
        ssl_shutdown_timeout: Union[_SENTINEL, None, float] = sentinel,
        # We initialise _connector to None immediately, as it's referenced in __del__()
        # and could cause issues if an exception occurs during initialisation.
        self._connector: Optional[BaseConnector] = None
        if loop is None:
                loop = connector._loop
        loop = loop or asyncio.get_running_loop()
        if base_url is None or isinstance(base_url, URL):
            self._base_url: Optional[URL] = base_url
            self._base_url_origin = None if base_url is None else base_url.origin()
            self._base_url = URL(base_url)
            self._base_url_origin = self._base_url.origin()
            assert self._base_url.absolute, "Only absolute URLs are supported"
        if self._base_url is not None and not self._base_url.path.endswith("/"):
            raise ValueError("base_url must have a trailing '/'")
        if timeout is sentinel or timeout is None:
            self._timeout = DEFAULT_TIMEOUT
            if read_timeout is not sentinel:
                    "read_timeout is deprecated, use timeout argument instead",
                self._timeout = attr.evolve(self._timeout, total=read_timeout)
            if conn_timeout is not None:
                self._timeout = attr.evolve(self._timeout, connect=conn_timeout)
                    "conn_timeout is deprecated, use timeout argument instead",
            if not isinstance(timeout, ClientTimeout):
                    f"timeout parameter cannot be of {type(timeout)} type, "
                    "please use 'timeout=ClientTimeout(...)'",
                    "read_timeout and timeout parameters "
                    "conflict, please setup "
                    "timeout.read"
                    "conn_timeout and timeout parameters "
                    "timeout.connect"
        if ssl_shutdown_timeout is not sentinel:
                "The ssl_shutdown_timeout parameter is deprecated and will be removed in aiohttp 4.0",
        if connector is None:
            connector = TCPConnector(
                loop=loop, ssl_shutdown_timeout=ssl_shutdown_timeout
        if connector._loop is not loop:
            raise RuntimeError("Session and connector has to use same event loop")
        self._loop = loop
        if loop.get_debug():
            self._source_traceback = traceback.extract_stack(sys._getframe(1))
        if cookie_jar is None:
            cookie_jar = CookieJar(loop=loop)
        self._cookie_jar = cookie_jar
            self._cookie_jar.update_cookies(cookies)
        self._connector = connector
        self._connector_owner = connector_owner
        self._default_auth = auth
        self._json_serialize = json_serialize
        self._raise_for_status = raise_for_status
        self._auto_decompress = auto_decompress
        self._requote_redirect_url = requote_redirect_url
        self._read_bufsize = read_bufsize
        self._max_line_size = max_line_size
        self._max_field_size = max_field_size
        # Convert to list of tuples
            real_headers: CIMultiDict[str] = CIMultiDict(headers)
            real_headers = CIMultiDict()
        self._default_headers: CIMultiDict[str] = real_headers
        if skip_auto_headers is not None:
            self._skip_auto_headers = frozenset(istr(i) for i in skip_auto_headers)
            self._skip_auto_headers = frozenset()
        self._request_class = request_class
        self._response_class = response_class
        self._ws_response_class = ws_response_class
        self._trace_configs = trace_configs or []
        for trace_config in self._trace_configs:
            trace_config.freeze()
        self._resolve_charset = fallback_charset_resolver
        self._default_proxy = proxy
        self._default_proxy_auth = proxy_auth
        self._retry_connection: bool = True
        self._middlewares = middlewares
    def __init_subclass__(cls: Type["ClientSession"]) -> None:
            "Inheritance class {} from ClientSession "
            "is discouraged".format(cls.__name__),
    if DEBUG:
        def __setattr__(self, name: str, val: Any) -> None:
            if name not in self.ATTRS:
                    "Setting custom ClientSession.{} attribute "
                    "is discouraged".format(name),
            super().__setattr__(name, val)
    def __del__(self, _warnings: Any = warnings) -> None:
            kwargs = {"source": self}
            _warnings.warn(
                f"Unclosed client session {self!r}", ResourceWarning, **kwargs
            context = {"client_session": self, "message": "Unclosed client session"}
            if self._source_traceback is not None:
                context["source_traceback"] = self._source_traceback
            self._loop.call_exception_handler(context)
            url: StrOrURL,
            **kwargs: Unpack[_RequestOptions],
        ) -> "_RequestContextManager": ...
            self, method: str, url: StrOrURL, **kwargs: Any
        ) -> "_RequestContextManager":
            """Perform HTTP request."""
            return _RequestContextManager(self._request(method, url, **kwargs))
    def _build_url(self, str_or_url: StrOrURL) -> URL:
        url = URL(str_or_url)
        if self._base_url and not url.absolute:
            return self._base_url.join(url)
    async def _request(
        str_or_url: StrOrURL,
        params: Query = None,
        json: Any = None,
        allow_redirects: bool = True,
        max_redirects: int = 10,
        compress: Union[str, bool, None] = None,
        chunked: Optional[bool] = None,
        expect100: bool = False,
            None, bool, Callable[[ClientResponse], Awaitable[None]]
        read_until_eof: bool = True,
        timeout: Union[ClientTimeout, _SENTINEL] = sentinel,
        verify_ssl: Optional[bool] = None,
        fingerprint: Optional[bytes] = None,
        ssl_context: Optional[SSLContext] = None,
        ssl: Union[SSLContext, bool, Fingerprint] = True,
        server_hostname: Optional[str] = None,
        proxy_headers: Optional[LooseHeaders] = None,
        trace_request_ctx: Optional[Mapping[str, Any]] = None,
        read_bufsize: Optional[int] = None,
        auto_decompress: Optional[bool] = None,
        max_line_size: Optional[int] = None,
        max_field_size: Optional[int] = None,
        middlewares: Optional[Sequence[ClientMiddlewareType]] = None,
    ) -> ClientResponse:
        # NOTE: timeout clamps existing connect and read timeouts.  We cannot
        # set the default to None because we need to detect if the user wants
        # to use the existing timeouts by setting timeout to None.
            raise RuntimeError("Session is closed")
        ssl = _merge_ssl_params(ssl, verify_ssl, ssl_context, fingerprint)
        if data is not None and json is not None:
                "data and json parameters can not be used at the same time"
        elif json is not None:
            data = payload.JsonPayload(json, dumps=self._json_serialize)
        if not isinstance(chunked, bool) and chunked is not None:
            warnings.warn("Chunk size is deprecated #1615", DeprecationWarning)
        redirects = 0
        history: List[ClientResponse] = []
        version = self._version
        # Merge with default headers and transform to CIMultiDict
        headers = self._prepare_headers(headers)
            url = self._build_url(str_or_url)
            raise InvalidUrlClientError(str_or_url) from e
        assert self._connector is not None
        if url.scheme not in self._connector.allowed_protocol_schema_set:
            raise NonHttpUrlClientError(url)
        skip_headers: Optional[Iterable[istr]]
            skip_headers = {
                istr(i) for i in skip_auto_headers
            } | self._skip_auto_headers
        elif self._skip_auto_headers:
            skip_headers = self._skip_auto_headers
            skip_headers = None
            proxy = self._default_proxy
        if proxy_auth is None:
            proxy_auth = self._default_proxy_auth
            proxy_headers = None
            proxy_headers = self._prepare_headers(proxy_headers)
                proxy = URL(proxy)
                raise InvalidURL(proxy) from e
        if timeout is sentinel:
            real_timeout: ClientTimeout = self._timeout
                real_timeout = ClientTimeout(total=timeout)
                real_timeout = timeout
        # timeout is cumulative for all request operations
        # (request, redirects, responses, data consuming)
        tm = TimeoutHandle(
            self._loop, real_timeout.total, ceil_threshold=real_timeout.ceil_threshold
        handle = tm.start()
        if read_bufsize is None:
            read_bufsize = self._read_bufsize
        if auto_decompress is None:
            auto_decompress = self._auto_decompress
        if max_line_size is None:
            max_line_size = self._max_line_size
        if max_field_size is None:
            max_field_size = self._max_field_size
        traces = [
            Trace(
                trace_config,
                trace_config.trace_config_ctx(trace_request_ctx=trace_request_ctx),
            for trace_config in self._trace_configs
        for trace in traces:
            await trace.send_request_start(method, url.update_query(params), headers)
        timer = tm.timer()
            with timer:
                # https://www.rfc-editor.org/rfc/rfc9112.html#name-retrying-requests
                retry_persistent_connection = (
                    self._retry_connection and method in IDEMPOTENT_METHODS
                    url, auth_from_url = strip_auth_from_url(url)
                    if not url.raw_host:
                        # NOTE: Bail early, otherwise, causes `InvalidURL` through
                        # NOTE: `self._request_class()` below.
                        err_exc_cls = (
                            InvalidUrlRedirectClientError
                            if redirects
                            else InvalidUrlClientError
                        raise err_exc_cls(url)
                    # If `auth` was passed for an already authenticated URL,
                    # disallow only if this is the initial URL; this is to avoid issues
                    # with sketchy redirects that are not the caller's responsibility
                    if not history and (auth and auth_from_url):
                            "Cannot combine AUTH argument with "
                            "credentials encoded in URL"
                    # Override the auth with the one from the URL only if we
                    # have no auth, or if we got an auth from a redirect URL
                    if auth is None or (history and auth_from_url is not None):
                        auth = auth_from_url
                        auth is None
                        and self._default_auth
                            not self._base_url or self._base_url_origin == url.origin()
                        auth = self._default_auth
                    # Try netrc if auth is still None and trust_env is enabled.
                    if auth is None and self._trust_env and url.host is not None:
                        auth = await self._loop.run_in_executor(
                            None, self._get_netrc_auth, url.host
                    # It would be confusing if we support explicit
                    # Authorization header with auth argument
                        headers is not None
                        and auth is not None
                        and hdrs.AUTHORIZATION in headers
                            "Cannot combine AUTHORIZATION header "
                            "with AUTH argument or credentials "
                            "encoded in URL"
                    all_cookies = self._cookie_jar.filter_cookies(url)
                        tmp_cookie_jar = CookieJar(
                            quote_cookie=self._cookie_jar.quote_cookie
                        tmp_cookie_jar.update_cookies(cookies)
                        req_cookies = tmp_cookie_jar.filter_cookies(url)
                        if req_cookies:
                            all_cookies.load(req_cookies)
                    proxy_: Optional[URL] = None
                        proxy_ = URL(proxy)
                    elif self._trust_env:
                        with suppress(LookupError):
                            proxy_, proxy_auth = await asyncio.to_thread(
                                get_env_proxy_for_url, url
                    req = self._request_class(
                        skip_auto_headers=skip_headers,
                        cookies=all_cookies,
                        compress=compress,
                        chunked=chunked,
                        expect100=expect100,
                        loop=self._loop,
                        response_class=self._response_class,
                        proxy=proxy_,
                        proxy_auth=proxy_auth,
                        timer=timer,
                        session=self,
                        ssl=ssl if ssl is not None else True,
                        server_hostname=server_hostname,
                        proxy_headers=proxy_headers,
                        traces=traces,
                        trust_env=self.trust_env,
                    async def _connect_and_send_request(
                        req: ClientRequest,
                        # connection timeout
                            conn = await self._connector.connect(
                                req, traces=traces, timeout=real_timeout
                        except asyncio.TimeoutError as exc:
                            raise ConnectionTimeoutError(
                                f"Connection timeout to host {req.url}"
                        assert conn.protocol is not None
                        conn.protocol.set_response_params(
                            skip_payload=req.method in EMPTY_BODY_METHODS,
                            read_until_eof=read_until_eof,
                            auto_decompress=auto_decompress,
                            read_timeout=real_timeout.sock_read,
                            read_bufsize=read_bufsize,
                            timeout_ceil_threshold=self._connector._timeout_ceil_threshold,
                            max_line_size=max_line_size,
                            max_field_size=max_field_size,
                            resp = await req.send(conn)
                                await resp.start(conn)
                    # Apply middleware (if any) - per-request middleware overrides session middleware
                    effective_middlewares = (
                        self._middlewares if middlewares is None else middlewares
                    if effective_middlewares:
                        handler = build_client_middlewares(
                            _connect_and_send_request, effective_middlewares
                        handler = _connect_and_send_request
                        resp = await handler(req)
                    # Client connector errors should not be retried
                    except (ClientOSError, ServerDisconnectedError):
                        if retry_persistent_connection:
                            retry_persistent_connection = False
                    except ClientError:
                        if exc.errno is None and isinstance(exc, asyncio.TimeoutError):
                        raise ClientOSError(*exc.args) from exc
                    # Update cookies from raw headers to preserve duplicates
                    if resp._raw_cookie_headers:
                        self._cookie_jar.update_cookies_from_headers(
                            resp._raw_cookie_headers, resp.url
                    # redirects
                    if resp.status in (301, 302, 303, 307, 308) and allow_redirects:
                            await trace.send_request_redirect(
                                method, url.update_query(params), headers, resp
                        redirects += 1
                        history.append(resp)
                        if max_redirects and redirects >= max_redirects:
                            if req._body is not None:
                                await req._body.close()
                                history[0].request_info, tuple(history)
                        # For 301 and 302, mimic IE, now changed in RFC
                        # https://github.com/kennethreitz/requests/pull/269
                        if (resp.status == 303 and resp.method != hdrs.METH_HEAD) or (
                            resp.status in (301, 302) and resp.method == hdrs.METH_POST
                            method = hdrs.METH_GET
                            data = None
                            if headers.get(hdrs.CONTENT_LENGTH):
                                headers.pop(hdrs.CONTENT_LENGTH)
                            # For 307/308, always preserve the request body
                            # For 301/302 with non-POST methods, preserve the request body
                            # https://www.rfc-editor.org/rfc/rfc9110#section-15.4.3-3.1
                            # Use the existing payload to avoid recreating it from a potentially consumed file
                            data = req._body
                        r_url = resp.headers.get(hdrs.LOCATION) or resp.headers.get(
                            hdrs.URI
                        if r_url is None:
                            # see github.com/aio-libs/aiohttp/issues/2022
                            # reading from correct redirection
                            # response is forbidden
                            resp.release()
                            parsed_redirect_url = URL(
                                r_url, encoded=not self._requote_redirect_url
                            raise InvalidUrlRedirectClientError(
                                r_url,
                                "Server attempted redirecting to a location that does not look like a URL",
                        scheme = parsed_redirect_url.scheme
                        if scheme not in HTTP_AND_EMPTY_SCHEMA_SET:
                            raise NonHttpUrlRedirectClientError(r_url)
                        elif not scheme:
                            parsed_redirect_url = url.join(parsed_redirect_url)
                            redirect_origin = parsed_redirect_url.origin()
                        except ValueError as origin_val_err:
                                parsed_redirect_url,
                                "Invalid redirect URL origin",
                            ) from origin_val_err
                        if url.origin() != redirect_origin:
                            headers.pop(hdrs.AUTHORIZATION, None)
                        url = parsed_redirect_url
            # check response status
            if raise_for_status is None:
                raise_for_status = self._raise_for_status
            elif callable(raise_for_status):
                await raise_for_status(resp)
            elif raise_for_status:
            # register connection
            if handle is not None:
                if resp.connection is not None:
                    resp.connection.add_callback(handle.cancel)
                    handle.cancel()
            resp._history = tuple(history)
                await trace.send_request_end(
            # cleanup timer
            tm.close()
            if handle:
                handle = None
                await trace.send_request_exception(
                    method, url.update_query(params), headers, e
    def ws_connect(
        method: str = hdrs.METH_GET,
        protocols: Iterable[str] = (),
        timeout: Union[ClientWSTimeout, _SENTINEL] = sentinel,
        receive_timeout: Optional[float] = None,
        autoclose: bool = True,
        autoping: bool = True,
        heartbeat: Optional[float] = None,
        origin: Optional[str] = None,
        compress: int = 0,
        max_msg_size: int = 4 * 1024 * 1024,
    ) -> "_WSRequestContextManager":
        """Initiate websocket connection."""
        return _WSRequestContextManager(
            self._ws_connect(
                protocols=protocols,
                receive_timeout=receive_timeout,
                autoclose=autoclose,
                autoping=autoping,
                heartbeat=heartbeat,
                origin=origin,
                ssl=ssl,
                verify_ssl=verify_ssl,
                fingerprint=fingerprint,
                max_msg_size=max_msg_size,
    async def _ws_connect(
    ) -> ClientWebSocketResponse:
        if timeout is not sentinel:
            if isinstance(timeout, ClientWSTimeout):
                ws_timeout = timeout
                    "parameter 'timeout' of type 'float' "
                    "is deprecated, please use "
                    "'timeout=ClientWSTimeout(ws_close=...)'",
                ws_timeout = ClientWSTimeout(ws_close=timeout)
            ws_timeout = DEFAULT_WS_CLIENT_TIMEOUT
        if receive_timeout is not None:
                "float parameter 'receive_timeout' "
                "is deprecated, please use parameter "
                "'timeout=ClientWSTimeout(ws_receive=...)'",
            ws_timeout = attr.evolve(ws_timeout, ws_receive=receive_timeout)
            real_headers: CIMultiDict[str] = CIMultiDict()
            real_headers = CIMultiDict(headers)
        default_headers = {
            hdrs.UPGRADE: "websocket",
            hdrs.CONNECTION: "Upgrade",
            hdrs.SEC_WEBSOCKET_VERSION: "13",
            real_headers.setdefault(key, value)
        sec_key = base64.b64encode(os.urandom(16))
        real_headers[hdrs.SEC_WEBSOCKET_KEY] = sec_key.decode()
        if protocols:
            real_headers[hdrs.SEC_WEBSOCKET_PROTOCOL] = ",".join(protocols)
            real_headers[hdrs.ORIGIN] = origin
            extstr = ws_ext_gen(compress=compress)
            real_headers[hdrs.SEC_WEBSOCKET_EXTENSIONS] = extstr
        # For the sake of backward compatibility, if user passes in None, convert it to True
        if ssl is None:
                "ssl=None is deprecated, please use ssl=True",
            ssl = True
        # send request
        resp = await self.request(
            headers=real_headers,
            read_until_eof=False,
            # check handshake
            if resp.status != 101:
                raise WSServerHandshakeError(
                    resp.request_info,
                    resp.history,
                    message="Invalid response status",
                    status=resp.status,
                    headers=resp.headers,
            if resp.headers.get(hdrs.UPGRADE, "").lower() != "websocket":
                    message="Invalid upgrade header",
            if resp.headers.get(hdrs.CONNECTION, "").lower() != "upgrade":
                    message="Invalid connection header",
            # key calculation
            r_key = resp.headers.get(hdrs.SEC_WEBSOCKET_ACCEPT, "")
            match = base64.b64encode(hashlib.sha1(sec_key + WS_KEY).digest()).decode()
            if r_key != match:
                    message="Invalid challenge response",
            # websocket protocol
            if protocols and hdrs.SEC_WEBSOCKET_PROTOCOL in resp.headers:
                resp_protocols = [
                    proto.strip()
                    for proto in resp.headers[hdrs.SEC_WEBSOCKET_PROTOCOL].split(",")
                for proto in resp_protocols:
                    if proto in protocols:
                        protocol = proto
            # websocket compress
            notakeover = False
                compress_hdrs = resp.headers.get(hdrs.SEC_WEBSOCKET_EXTENSIONS)
                if compress_hdrs:
                        compress, notakeover = ws_ext_parse(compress_hdrs)
                    except WSHandshakeError as exc:
                            message=exc.args[0],
                    compress = 0
            conn = resp.connection
            assert conn is not None
            conn_proto = conn.protocol
            assert conn_proto is not None
            # For WS connection the read_timeout must be either receive_timeout or greater
            # None == no timeout, i.e. infinite timeout, so None is the max timeout possible
            if ws_timeout.ws_receive is None:
                # Reset regardless
                conn_proto.read_timeout = None
            elif conn_proto.read_timeout is not None:
                conn_proto.read_timeout = max(
                    ws_timeout.ws_receive, conn_proto.read_timeout
            transport = conn.transport
            assert transport is not None
            reader = WebSocketDataQueue(conn_proto, 2**16, loop=self._loop)
            conn_proto.set_parser(WebSocketReader(reader, max_msg_size), reader)
            writer = WebSocketWriter(
                conn_proto,
                use_mask=True,
                notakeover=notakeover,
            return self._ws_response_class(
                writer,
                ws_timeout,
                autoclose,
                autoping,
                self._loop,
                client_notakeover=notakeover,
    def _prepare_headers(self, headers: Optional[LooseHeaders]) -> "CIMultiDict[str]":
        """Add default headers and transform it to CIMultiDict"""
        # Convert headers to MultiDict
        result = CIMultiDict(self._default_headers)
            if not isinstance(headers, (MultiDictProxy, MultiDict)):
                headers = CIMultiDict(headers)
            added_names: Set[str] = set()
                if key in added_names:
                    result.add(key, value)
                    added_names.add(key)
    def _get_netrc_auth(self, host: str) -> Optional[BasicAuth]:
        Get auth from netrc for the given host.
        This method is designed to be called in an executor to avoid
        blocking I/O in the event loop.
        netrc_obj = netrc_from_env()
            return basicauth_from_netrc(netrc_obj, host)
            self, url: StrOrURL, *, allow_redirects: bool = True, **kwargs: Any
            """Perform HTTP GET request."""
            return _RequestContextManager(
                self._request(
                    hdrs.METH_GET, url, allow_redirects=allow_redirects, **kwargs
            """Perform HTTP OPTIONS request."""
                    hdrs.METH_OPTIONS, url, allow_redirects=allow_redirects, **kwargs
            self, url: StrOrURL, *, allow_redirects: bool = False, **kwargs: Any
            """Perform HTTP HEAD request."""
                    hdrs.METH_HEAD, url, allow_redirects=allow_redirects, **kwargs
            self, url: StrOrURL, *, data: Any = None, **kwargs: Any
            """Perform HTTP POST request."""
                self._request(hdrs.METH_POST, url, data=data, **kwargs)
            """Perform HTTP PUT request."""
                self._request(hdrs.METH_PUT, url, data=data, **kwargs)
            """Perform HTTP PATCH request."""
                self._request(hdrs.METH_PATCH, url, data=data, **kwargs)
        def delete(self, url: StrOrURL, **kwargs: Any) -> "_RequestContextManager":
            """Perform HTTP DELETE request."""
                self._request(hdrs.METH_DELETE, url, **kwargs)
        """Close underlying connector.
        Release all acquired resources.
            if self._connector is not None and self._connector_owner:
                await self._connector.close()
            self._connector = None
        """Is client session closed.
        A readonly property.
        return self._connector is None or self._connector.closed
    def connector(self) -> Optional[BaseConnector]:
        """Connector instance used for the session."""
        return self._connector
    def cookie_jar(self) -> AbstractCookieJar:
        """The session cookies."""
        return self._cookie_jar
    def version(self) -> Tuple[int, int]:
        """The session HTTP protocol version."""
    def requote_redirect_url(self) -> bool:
        """Do URL requoting on redirection handling."""
        return self._requote_redirect_url
    @requote_redirect_url.setter
    def requote_redirect_url(self, val: bool) -> None:
            "session.requote_redirect_url modification is deprecated #2778",
        self._requote_redirect_url = val
    def loop(self) -> asyncio.AbstractEventLoop:
        """Session's loop."""
            "client.loop property is deprecated", DeprecationWarning, stacklevel=2
        return self._loop
    def timeout(self) -> ClientTimeout:
        """Timeout for the session."""
    def headers(self) -> "CIMultiDict[str]":
        """The default headers of the client session."""
        return self._default_headers
    def skip_auto_headers(self) -> FrozenSet[istr]:
        """Headers for which autogeneration should be skipped"""
        return self._skip_auto_headers
    def auth(self) -> Optional[BasicAuth]:
        """An object that represents HTTP Basic Authorization"""
        return self._default_auth
    def json_serialize(self) -> JSONEncoder:
        """Json serializer callable"""
        return self._json_serialize
    def connector_owner(self) -> bool:
        """Should connector be closed on session closing"""
        return self._connector_owner
    def raise_for_status(
    ) -> Union[bool, Callable[[ClientResponse], Awaitable[None]]]:
        """Should `ClientResponse.raise_for_status()` be called for each response."""
        return self._raise_for_status
    def auto_decompress(self) -> bool:
        """Should the body response be automatically decompressed."""
        return self._auto_decompress
        Should proxies information from environment or netrc be trusted.
        Information is from HTTP_PROXY / HTTPS_PROXY environment variables
        or ~/.netrc file if present.
    def trace_configs(self) -> List[TraceConfig]:
        """A list of TraceConfig instances used for client tracing"""
        return self._trace_configs
    def detach(self) -> None:
        """Detach connector from session without closing the former.
        Session is switched to closed state anyway.
        raise TypeError("Use async with instead")
        # __exit__ should exist in pair with __enter__ but never executed
        pass  # pragma: no cover
    async def __aenter__(self) -> "ClientSession":
class _BaseRequestContextManager(Coroutine[Any, Any, _RetType], Generic[_RetType]):
    __slots__ = ("_coro", "_resp")
    def __init__(self, coro: Coroutine["asyncio.Future[Any]", None, _RetType]) -> None:
        self._coro: Coroutine["asyncio.Future[Any]", None, _RetType] = coro
    def send(self, arg: None) -> "asyncio.Future[Any]":
        return self._coro.send(arg)
    def throw(self, *args: Any, **kwargs: Any) -> "asyncio.Future[Any]":
        return self._coro.throw(*args, **kwargs)
        return self._coro.close()
    def __await__(self) -> Generator[Any, None, _RetType]:
        ret = self._coro.__await__()
    def __iter__(self) -> Generator[Any, None, _RetType]:
        return self.__await__()
    async def __aenter__(self) -> _RetType:
        self._resp: _RetType = await self._coro
        return await self._resp.__aenter__()
        exc: Optional[BaseException],
        tb: Optional[TracebackType],
        await self._resp.__aexit__(exc_type, exc, tb)
_RequestContextManager = _BaseRequestContextManager[ClientResponse]
_WSRequestContextManager = _BaseRequestContextManager[ClientWebSocketResponse]
class _SessionRequestContextManager:
    __slots__ = ("_coro", "_resp", "_session")
        coro: Coroutine["asyncio.Future[Any]", None, ClientResponse],
        session: ClientSession,
        self._coro = coro
        self._resp: Optional[ClientResponse] = None
    async def __aenter__(self) -> ClientResponse:
            self._resp = await self._coro
            await self._session.close()
            return self._resp
        assert self._resp is not None
        self._resp.close()
    ) -> _SessionRequestContextManager: ...
    ) -> _SessionRequestContextManager:
        """Constructs and sends a request.
        Returns response object.
        method - HTTP method
        url - request url
        params - (optional) Dictionary or bytes to be sent in the query
        string of the new request
        data - (optional) Dictionary, bytes, or file-like object to
        send in the body of the request
        json - (optional) Any json compatible python object
        headers - (optional) Dictionary of HTTP Headers to send with
        the request
        cookies - (optional) Dict object to send with the request
        auth - (optional) BasicAuth named tuple represent HTTP Basic Auth
        auth - aiohttp.helpers.BasicAuth
        allow_redirects - (optional) If set to False, do not follow
        redirects
        version - Request HTTP version.
        compress - Set to True if request has to be compressed
        with deflate encoding.
        chunked - Set to chunk size for chunked transfer encoding.
        expect100 - Expect 100-continue response from server.
        connector - BaseConnector sub-class instance to support
        connection pooling.
        read_until_eof - Read response until eof if response
        does not have Content-Length header.
        loop - Optional event loop.
        timeout - Optional ClientTimeout settings structure, 5min
        total timeout by default.
        >>> import aiohttp
        >>> async with aiohttp.request('GET', 'http://python.org/') as resp:
        ...    print(resp)
        ...    data = await resp.read()
        <ClientResponse(https://www.python.org/) [200 OK]>
        connector_owner = False
            connector_owner = True
            connector = TCPConnector(loop=loop, force_close=True)
        session = ClientSession(
            cookies=kwargs.pop("cookies", None),
            timeout=kwargs.pop("timeout", sentinel),
            connector=connector,
            connector_owner=connector_owner,
        return _SessionRequestContextManager(
            session._request(method, url, **kwargs),
LiteLLM A2A Client class.
Provides a class-based interface for A2A agent invocation.
from typing import TYPE_CHECKING, AsyncIterator, Dict, Optional
    from a2a.client import A2AClient as A2AClientType
    from a2a.types import (
        AgentCard,
        SendMessageRequest,
        SendStreamingMessageRequest,
        SendStreamingMessageResponse,
class A2AClient:
    LiteLLM wrapper for A2A agent invocation.
    Creates the underlying A2A client once on first use and reuses it.
        timeout: float = 60.0,
        extra_headers: Optional[Dict[str, str]] = None,
        Initialize the A2A client wrapper.
            base_url: The base URL of the A2A agent (e.g., "http://localhost:10001")
            timeout: Request timeout in seconds (default: 60.0)
            extra_headers: Optional additional headers to include in requests
        self.extra_headers = extra_headers
        self._a2a_client: Optional["A2AClientType"] = None
    async def _get_client(self) -> "A2AClientType":
        """Get or create the underlying A2A client."""
        if self._a2a_client is None:
            from litellm.a2a_protocol.main import create_a2a_client
            self._a2a_client = await create_a2a_client(
                base_url=self.base_url,
                extra_headers=self.extra_headers,
        return self._a2a_client
    async def get_agent_card(self) -> "AgentCard":
        """Fetch the agent card from the server."""
        from litellm.a2a_protocol.main import aget_agent_card
        return await aget_agent_card(
    async def send_message(
        self, request: "SendMessageRequest"
    ) -> LiteLLMSendMessageResponse:
        """Send a message to the A2A agent."""
        from litellm.a2a_protocol.main import asend_message
        a2a_client = await self._get_client()
        return await asend_message(a2a_client=a2a_client, request=request)
    async def send_message_streaming(
        self, request: "SendStreamingMessageRequest"
    ) -> AsyncIterator["SendStreamingMessageResponse"]:
        """Send a streaming message to the A2A agent."""
        from litellm.a2a_protocol.main import asend_message_streaming
        async for chunk in asend_message_streaming(a2a_client=a2a_client, request=request):
LiteLLM Proxy uses this MCP Client to connnect to other MCP servers.
from typing import Any, Awaitable, Callable, Dict, List, Optional, Tuple, TypeVar, Union
from mcp import ClientSession, ReadResourceResult, Resource, StdioServerParameters
from mcp.client.sse import sse_client
from mcp.client.stdio import stdio_client
streamable_http_client: Optional[Any] = None
    import mcp.client.streamable_http as streamable_http_module  # type: ignore
    streamable_http_client = getattr(streamable_http_module, "streamable_http_client", None)
from mcp.types import CallToolRequestParams as MCPCallToolRequestParams
from mcp.types import CallToolResult as MCPCallToolResult
from mcp.types import (
    GetPromptRequestParams,
    GetPromptResult,
    Prompt,
    ResourceTemplate,
from mcp.types import Tool as MCPTool
from pydantic import AnyUrl
from litellm.llms.custom_httpx.http_handler import get_ssl_configuration
from litellm.types.llms.custom_http import VerifyTypes
from litellm.types.mcp import (
    MCPAuth,
    MCPStdioConfig,
    MCPTransport,
def to_basic_auth(auth_value: str) -> str:
    """Convert auth value to Basic Auth format."""
    return base64.b64encode(auth_value.encode("utf-8")).decode()
TSessionResult = TypeVar("TSessionResult")
class MCPClient:
    MCP Client supporting:
      SSE and HTTP transports
      Authentication via Bearer token, Basic Auth, or API Key
      Tool calling with error handling and result parsing
        server_url: str = "",
        transport_type: MCPTransportType = MCPTransport.http,
        auth_type: MCPAuthType = None,
        auth_value: Optional[Union[str, Dict[str, str]]] = None,
        stdio_config: Optional[MCPStdioConfig] = None,
        ssl_verify: Optional[VerifyTypes] = None,
        self.server_url: str = server_url
        self.transport_type: MCPTransport = transport_type
        self.auth_type: MCPAuthType = auth_type
        self.timeout: float = timeout
        self._mcp_auth_value: Optional[Union[str, Dict[str, str]]] = None
        self.stdio_config: Optional[MCPStdioConfig] = stdio_config
        self.extra_headers: Optional[Dict[str, str]] = extra_headers
        self.ssl_verify: Optional[VerifyTypes] = ssl_verify
        # handle the basic auth value if provided
        if auth_value:
            self.update_auth_value(auth_value)
    def _create_transport_context(
    ) -> Tuple[Any, Optional[httpx.AsyncClient]]:
        Create the appropriate transport context based on transport type.
            Tuple of (transport_context, http_client).
            http_client is only set for HTTP transport and needs cleanup.
        http_client: Optional[httpx.AsyncClient] = None
        if self.transport_type == MCPTransport.stdio:
            if not self.stdio_config:
                raise ValueError("stdio_config is required for stdio transport")
            server_params = StdioServerParameters(
                command=self.stdio_config.get("command", ""),
                args=self.stdio_config.get("args", []),
                env=self.stdio_config.get("env", {}),
            return stdio_client(server_params), None
        if self.transport_type == MCPTransport.sse:
            headers = self._get_auth_headers()
            httpx_client_factory = self._create_httpx_client_factory()
            return sse_client(
                url=self.server_url,
                httpx_client_factory=httpx_client_factory,
            ), None
        # HTTP transport (default)
        if streamable_http_client is None:
                "streamable_http_client is not available. "
                "Please install mcp with HTTP support."
            "litellm headers for streamable_http_client: %s", headers
        http_client = httpx_client_factory(
            timeout=httpx.Timeout(self.timeout),
        transport_ctx = streamable_http_client(
        return transport_ctx, http_client
    async def _execute_session_operation(
        transport_ctx: Any,
        operation: Callable[[ClientSession], Awaitable[TSessionResult]],
    ) -> TSessionResult:
        Execute an operation within a transport and session context.
        Handles entering/exiting contexts and running the operation.
        transport = await transport_ctx.__aenter__()
            read_stream, write_stream = transport[0], transport[1]
            session_ctx = ClientSession(read_stream, write_stream)
            session = await session_ctx.__aenter__()
                await session.initialize()
                return await operation(session)
                    await session_ctx.__aexit__(None, None, None)
                    verbose_logger.debug(f"Error during session context exit: {e}")
                await transport_ctx.__aexit__(None, None, None)
                verbose_logger.debug(f"Error during transport context exit: {e}")
    async def run_with_session(
        self, operation: Callable[[ClientSession], Awaitable[TSessionResult]]
        """Open a session, run the provided coroutine, and clean up."""
            transport_ctx, http_client = self._create_transport_context()
            return await self._execute_session_operation(transport_ctx, operation)
                "MCP client run_with_session failed for %s", self.server_url or "stdio"
            if http_client is not None:
                    await http_client.aclose()
                    verbose_logger.debug(f"Error during http_client cleanup: {e}")
    def update_auth_value(self, mcp_auth_value: Union[str, Dict[str, str]]):
        Set the authentication header for the MCP client.
        if isinstance(mcp_auth_value, dict):
            self._mcp_auth_value = mcp_auth_value
            if self.auth_type == MCPAuth.basic:
                # Assuming mcp_auth_value is in format "username:password", convert it when updating
                mcp_auth_value = to_basic_auth(mcp_auth_value)
    def _get_auth_headers(self) -> dict:
        """Generate authentication headers based on auth type."""
        if self._mcp_auth_value:
            if isinstance(self._mcp_auth_value, str):
                if self.auth_type == MCPAuth.bearer_token:
                    headers["Authorization"] = f"Bearer {self._mcp_auth_value}"
                elif self.auth_type == MCPAuth.basic:
                    headers["Authorization"] = f"Basic {self._mcp_auth_value}"
                elif self.auth_type == MCPAuth.api_key:
                    headers["X-API-Key"] = self._mcp_auth_value
                elif self.auth_type == MCPAuth.authorization:
                    headers["Authorization"] = self._mcp_auth_value
                elif self.auth_type == MCPAuth.oauth2:
            elif isinstance(self._mcp_auth_value, dict):
                headers.update(self._mcp_auth_value)
        # update the headers with the extra headers
        if self.extra_headers:
            headers.update(self.extra_headers)
    def _create_httpx_client_factory(self) -> Callable[..., httpx.AsyncClient]:
        Create a custom httpx client factory that uses LiteLLM's SSL configuration.
        This factory follows the same CA bundle path logic as http_handler.py:
        1. Check ssl_verify parameter (can be SSLContext, bool, or path to CA bundle)
        2. Check SSL_VERIFY environment variable
        3. Check SSL_CERT_FILE environment variable
        4. Fall back to certifi CA bundle
        def factory(
            headers: Optional[Dict[str, str]] = None,
            timeout: Optional[httpx.Timeout] = None,
            auth: Optional[httpx.Auth] = None,
        ) -> httpx.AsyncClient:
            """Create an httpx.AsyncClient with LiteLLM's SSL configuration."""
            # Get unified SSL configuration using the same logic as http_handler.py
            ssl_config = get_ssl_configuration(self.ssl_verify)
                f"MCP client using SSL configuration: {type(ssl_config).__name__}"
            return httpx.AsyncClient(
                verify=ssl_config,
                follow_redirects=True,
        return factory
    async def list_tools(self) -> List[MCPTool]:
        """List available tools from the server."""
            f"MCP client listing tools from {self.server_url or 'stdio'}"
        async def _list_tools_operation(session: ClientSession):
            return await session.list_tools()
            result = await self.run_with_session(_list_tools_operation)
            tool_count = len(result.tools)
            tool_names = [tool.name for tool in result.tools]
                f"MCP client listed {tool_count} tools from {self.server_url or 'stdio'}: {tool_names}"
            return result.tools
            verbose_logger.warning("MCP client list_tools was cancelled")
            error_type = type(e).__name__
                f"MCP client list_tools failed - "
                f"Error Type: {error_type}, "
                f"Error: {str(e)}, "
                f"Server: {self.server_url or 'stdio'}, "
                f"Transport: {self.transport_type}"
            # Check if it's a stream/connection error
            if "BrokenResourceError" in error_type or "Broken" in error_type:
                    "MCP client detected broken connection/stream during list_tools - "
                    "the MCP server may have crashed, disconnected, or timed out"
            # Return empty list instead of raising to allow graceful degradation
    async def call_tool(
        call_tool_request_params: MCPCallToolRequestParams,
        host_progress_callback: Optional[Callable] = None
    ) -> MCPCallToolResult:
        Call an MCP Tool.
            f"MCP client calling tool '{call_tool_request_params.name}' with arguments: {call_tool_request_params.arguments}"
        async def on_progress(progress: float, total: float | None, message: str | None):
            percentage = (progress / total * 100) if total else 0
                f"MCP Tool '{call_tool_request_params.name}' progress: "
                f"{progress}/{total} ({percentage:.0f}%) - {message or ''}"
            # Forward to Host if callback provided
            if host_progress_callback:
                    await host_progress_callback(progress, total)
                    verbose_logger.warning(f"Failed to forward to Host: {e}")
        async def _call_tool_operation(session: ClientSession):
            verbose_logger.debug("MCP client sending tool call to session")
            return await session.call_tool(
                name=call_tool_request_params.name,
                arguments=call_tool_request_params.arguments,
                progress_callback=on_progress,
            tool_result = await self.run_with_session(_call_tool_operation)
                f"MCP client tool call '{call_tool_request_params.name}' completed successfully"
            return tool_result
            verbose_logger.warning("MCP client tool call was cancelled")
            error_trace = traceback.format_exc()
            verbose_logger.debug(f"MCP client tool call traceback:\n{error_trace}")
            # Log detailed error information
                f"MCP client call_tool failed - "
                f"Tool: {call_tool_request_params.name}, "
                    "MCP client detected broken connection/stream - "
                    "the MCP server may have crashed, disconnected, or timed out."
            # Return a default error result instead of raising
            return MCPCallToolResult(
                    TextContent(type="text", text=f"{error_type}: {str(e)}")
                ],  # Empty content for error case
                isError=True,
    async def list_prompts(self) -> List[Prompt]:
        """List available prompts from the server."""
        async def _list_prompts_operation(session: ClientSession):
            return await session.list_prompts()
            result = await self.run_with_session(_list_prompts_operation)
            prompt_count = len(result.prompts)
            prompt_names = [prompt.name for prompt in result.prompts]
                f"MCP client listed {prompt_count} tools from {self.server_url or 'stdio'}: {prompt_names}"
            return result.prompts
            verbose_logger.warning("MCP client list_prompts was cancelled")
                f"MCP client list_prompts failed - "
    async def get_prompt(
        self, get_prompt_request_params: GetPromptRequestParams
    ) -> GetPromptResult:
        """Fetch a prompt definition from the MCP server."""
            f"MCP client fetching prompt '{get_prompt_request_params.name}' with arguments: {get_prompt_request_params.arguments}"
        async def _get_prompt_operation(session: ClientSession):
            verbose_logger.debug("MCP client sending get_prompt request to session")
            return await session.get_prompt(
                name=get_prompt_request_params.name,
                arguments=get_prompt_request_params.arguments,
            get_prompt_result = await self.run_with_session(_get_prompt_operation)
                f"MCP client get_prompt '{get_prompt_request_params.name}' completed successfully"
            return get_prompt_result
            verbose_logger.warning("MCP client get_prompt was cancelled")
            verbose_logger.debug(f"MCP client get_prompt traceback:\n{error_trace}")
                f"MCP client get_prompt failed - "
                f"Prompt: {get_prompt_request_params.name}, "
                    "MCP client detected broken connection/stream during get_prompt - "
    async def list_resources(self) -> list[Resource]:
        """List available resources from the server."""
            f"MCP client listing resources from {self.server_url or 'stdio'}"
        async def _list_resources_operation(session: ClientSession):
            return await session.list_resources()
            result = await self.run_with_session(_list_resources_operation)
            resource_count = len(result.resources)
            resource_names = [resource.name for resource in result.resources]
                f"MCP client listed {resource_count} resources from {self.server_url or 'stdio'}: {resource_names}"
            return result.resources
            verbose_logger.warning("MCP client list_resources was cancelled")
                f"MCP client list_resources failed - "
                    "MCP client detected broken connection/stream during list_resources - "
    async def list_resource_templates(self) -> list[ResourceTemplate]:
        """List available resource templates from the server."""
            f"MCP client listing resource templates from {self.server_url or 'stdio'}"
        async def _list_resource_templates_operation(session: ClientSession):
            return await session.list_resource_templates()
            result = await self.run_with_session(_list_resource_templates_operation)
            resource_template_count = len(result.resourceTemplates)
            resource_template_names = [
                resourceTemplate.name for resourceTemplate in result.resourceTemplates
                f"MCP client listed {resource_template_count} resource templates from {self.server_url or 'stdio'}: {resource_template_names}"
            return result.resourceTemplates
            verbose_logger.warning("MCP client list_resource_templates was cancelled")
                f"MCP client list_resource_templates failed - "
                    "MCP client detected broken connection/stream during list_resource_templates - "
    async def read_resource(self, url: AnyUrl) -> ReadResourceResult:
        """Fetch resource contents from the MCP server."""
        verbose_logger.info(f"MCP client fetching resource '{url}'")
        async def _read_resource_operation(session: ClientSession):
            verbose_logger.debug("MCP client sending read_resource request to session")
            return await session.read_resource(url)
            read_resource_result = await self.run_with_session(_read_resource_operation)
                f"MCP client read_resource '{url}' completed successfully"
            return read_resource_result
            verbose_logger.warning("MCP client read_resource was cancelled")
            verbose_logger.debug(f"MCP client read_resource traceback:\n{error_trace}")
                f"MCP client read_resource failed - "
                f"Url: {url}, "
                    "MCP client detected broken connection/stream during read_resource - "
from .credentials import CredentialsManagementClient
from .http_client import HTTPClient
from .keys import KeysManagementClient
from .teams import TeamsManagementClient
class Client:
    """Main client for interacting with the LiteLLM proxy API."""
        timeout: int = 30,
        Initialize the LiteLLM proxy client.
            base_url (str): The base URL of the LiteLLM proxy server (e.g., "http://localhost:4000")
            timeout: Request timeout in seconds (default: 30)
        self._api_key = get_litellm_gateway_api_key() or api_key
        # Initialize resource clients
        self.http = HTTPClient(base_url=base_url, api_key=api_key, timeout=timeout)
        self.models = ModelsManagementClient(base_url=self._base_url, api_key=self._api_key)
        self.model_groups = ModelGroupsManagementClient(base_url=self._base_url, api_key=self._api_key)
        self.chat = ChatClient(base_url=self._base_url, api_key=self._api_key)
        self.keys = KeysManagementClient(base_url=self._base_url, api_key=self._api_key)
        self.credentials = CredentialsManagementClient(base_url=self._base_url, api_key=self._api_key)
        self.teams = TeamsManagementClient(base_url=self._base_url, api_key=self._api_key)
