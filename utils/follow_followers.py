# Follow every follower of the authenticated user
for follower in tweepy.Cursor(api.get_followers).items():
    follower.follow()
