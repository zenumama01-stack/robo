# Get Tweet's Retweeters
# This endpoint/method allows you to get information about who has Retweeted a
# Tweet
response = client.get_retweeters(tweet_id, user_fields=["profile_image_url"])
