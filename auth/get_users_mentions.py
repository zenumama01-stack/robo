# Get User's Mentions
# This endpoint/method returns Tweets mentioning a single user specified by the
# requested user ID
response = client.get_users_mentions(user_id)
    print(tweet.text)
# By default, the 10 most recent Tweets will be returned
# You can retrieve up to 100 Tweets by specifying max_results
response = client.get_users_mentions(user_id, max_results=100)
