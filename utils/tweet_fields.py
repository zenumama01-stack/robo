# You can specify additional Tweet fields to retrieve using tweet_fields
    "Tweepy", tweet_fields=["created_at", "lang"]
# You can then access those fields as attributes of the Tweet objects
    print(tweet.id, tweet.lang)
# Alternatively, you can also access fields as keys, like a dictionary
    print(tweet["id"], tweet["lang"])
# There’s also a data attribute/key that provides the entire data dictionary
    print(tweet.data)
