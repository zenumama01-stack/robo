# Get User's Followers
# This endpoint/method returns a list of users who are followers of the
# specified user ID
response = client.get_users_followers(
    user_id, user_fields=["profile_image_url"]
# By default, this endpoint/method returns 100 results
# You can retrieve up to 1000 users by specifying max_results
response = client.get_users_followers(user_id, max_results=1000)
