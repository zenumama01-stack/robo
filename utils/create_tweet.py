# Create Tweet
# Example 1: Create a regular Tweet
response = client.create_tweet(
    text="This Tweet was Tweeted using Tweepy and Twitter API v2!"
print(f"https://twitter.com/user/status/{response.data['id']}")
# Example 2: Create a Tweet in a Community
# Note: The authenticated user must be a member of the Community
# response = client.create_tweet(
#     text="This Tweet was posted in a Community using Tweepy and Twitter API v2!",
#     community_id="INSERT_COMMUNITY_ID_HERE"
# )
# print(f"https://twitter.com/user/status/{response.data['id']}")
