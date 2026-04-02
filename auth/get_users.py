# Get Users
# This endpoint/method returns a variety of information about one or more users
# specified by the requested IDs or usernames
user_ids = [2244994945, 6253282]
response = client.get_users(ids=user_ids, user_fields=["profile_image_url"])
