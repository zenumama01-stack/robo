"""Simple command-line sample for Blogger.
Command-line application that retrieves the users blogs and posts.
  $ python blogger.py
  $ python blogger.py --help
  $ python blogger.py --logging_level=DEBUG
        "blogger",
        scope="https://www.googleapis.com/auth/blogger",
        users = service.users()
        # Retrieve this user's profile information
        thisuser = users.get(userId="self").execute()
        print("This user's display name is: %s" % thisuser["displayName"])
        blogs = service.blogs()
        # Retrieve the list of Blogs this user has write privileges on
        thisusersblogs = blogs.listByUser(userId="self").execute()
        for blog in thisusersblogs["items"]:
            print("The blog named '%s' is at: %s" % (blog["name"], blog["url"]))
        posts = service.posts()
        # List the posts for each blog this user has
            print("The posts for %s:" % blog["name"])
            request = posts.list(blogId=blog["id"])
            while request != None:
                posts_doc = request.execute()
                if "items" in posts_doc and not (posts_doc["items"] is None):
                    for post in posts_doc["items"]:
                        print("  %s (%s)" % (post["title"], post["url"]))
                request = posts.list_next(request, posts_doc)
    str_or_none,
class BloggerIE(InfoExtractor):
    IE_NAME = 'blogger.com'
    _VALID_URL = r'https?://(?:www\.)?blogger\.com/video\.g\?token=(?P<id>.+)'
    _EMBED_REGEX = [r'''<iframe[^>]+src=["'](?P<url>(?:https?:)?//(?:www\.)?blogger\.com/video\.g\?token=[^"']+)["']''']
        'url': 'https://www.blogger.com/video.g?token=AD6v5dzEe9hfcARr5Hlq1WTkYy6t-fXH3BBahVhGvVHe5szdEUBEloSEDSTA8-b111089KbfWuBvTN7fnbxMtymsHhXAXwVvyzHH4Qch2cfLQdGxKQrrEuFpC1amSl_9GuLWODjPgw',
        'md5': 'f1bc19b6ea1b0fd1d81e84ca9ec467ac',
            'id': 'BLOGGER-video-3c740e3a49197e16-796',
            'title': 'BLOGGER-video-3c740e3a49197e16-796',
            'duration': 76.068,
            'thumbnail': r're:https?://i9\.ytimg\.com/vi_blogger/.+',
    _WEBPAGE_TESTS = [{
        'url': 'https://blog.tomeuvizoso.net/2019/01/a-panfrost-milestone.html',
            'id': 'BLOGGER-video-3c740e3a49197e16-12203',
            'title': 'BLOGGER-video-3c740e3a49197e16-12203',
        token_id = self._match_id(url)
        webpage = self._download_webpage(url, token_id)
        data_json = self._search_regex(r'var\s+VIDEO_CONFIG\s*=\s*(\{.*)', webpage, 'JSON data')
        data = self._parse_json(data_json.encode().decode('unicode_escape'), token_id)
        streams = data['streams']
        formats = [{
            'ext': mimetype2ext(traverse_obj(parse_qs(stream['play_url']), ('mime', 0))),
            'url': stream['play_url'],
            'format_id': str_or_none(stream.get('format_id')),
        } for stream in streams]
            'id': data.get('iframe_id', token_id),
            'title': data.get('iframe_id', token_id),
            'thumbnail': data.get('thumbnail'),
            'duration': parse_duration(traverse_obj(parse_qs(streams[0]['play_url']), ('dur', 0))),
