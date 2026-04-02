This example walks through a basic oauth flow using the existing long-lived token type
Populate your app key and app secret in order to run this locally
auth_flow = DropboxOAuth2FlowNoRedirect(APP_KEY, APP_SECRET)
with dropbox.Dropbox(oauth2_access_token=oauth_result.access_token) as dbx:
