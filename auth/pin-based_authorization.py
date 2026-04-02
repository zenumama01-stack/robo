# PIN-based OAuth
# https://developer.twitter.com/en/docs/authentication/oauth-1-0a/pin-based-oauth
auth = tweepy.OAuth1UserHandler(consumer_key, consumer_secret)
# This prints a URL that can be used to authorize your app
# After granting access to the app, a PIN to complete the authorization process
# will be displayed
print(auth.get_authorization_url())
# Enter that PIN to continue
verifier = input("PIN: ")
auth.get_access_token(verifier)
