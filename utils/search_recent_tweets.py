# Search Recent Tweets
# This endpoint/method returns Tweets from the last seven days
response = client.search_recent_tweets("Tweepy")
# The method returns a Response object, a named tuple with data, includes,
# errors, and meta fields
print(response.meta)
# In this case, the data field of the Response returned is a list of Tweet
# objects
# Each Tweet object has default ID and text fields
# By default, this endpoint/method returns 10 results
response = client.search_recent_tweets("Tweepy", max_results=100)
