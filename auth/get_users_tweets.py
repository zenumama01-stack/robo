# Get User's Tweets
# This endpoint/method returns Tweets composed by a single user, specified by
# the requested user ID
response = client.get_users_tweets(user_id)
response = client.get_users_tweets(user_id, max_results=100)
