"""Sample for the Group Settings API demonstrates get and update method.
  $ python groupsettings.py
  $ python groupsettings.py --help
__author__ = "Shraddha Gupta <shraddhag@google.com>"
from optparse import OptionParser
from oauth2client.client import flow_from_clientsecrets
from oauth2client.file import Storage
from oauth2client.tools import run_flow
CLIENT_SECRETS = "client_secrets.json"
MISSING_CLIENT_SECRETS_MESSAGE = """
WARNING: Please configure OAuth 2.0
   %s
with information from the APIs Console <https://code.google.com/apis/console>.
""" % os.path.join(
    os.path.dirname(__file__), CLIENT_SECRETS
def access_settings(service, groupId, settings):
    """Retrieves a group's settings and updates the access permissions to it.
      service: object service for the Group Settings API.
      groupId: string identifier of the group@domain.
      settings: dictionary key-value pairs of properties of group.
    # Get the resource 'group' from the set of resources of the API.
    # The Group Settings API has only one resource 'group'.
    group = service.groups()
    # Retrieve the group properties
    g = group.get(groupUniqueId=groupId).execute()
    print("\nGroup properties for group %s\n" % g["name"])
    pprint.pprint(g)
    # If dictionary is empty, return without updating the properties.
    if not settings.keys():
        print("\nGive access parameters to update group access permissions\n")
    body = {}
    # Settings might contain null value for some keys(properties).
    # Extract the properties with values and add to dictionary body.
    for key in settings.iterkeys():
        if settings[key] is not None:
            body[key] = settings[key]
    # Update the properties of group
    g1 = group.update(groupUniqueId=groupId, body=body).execute()
    print("\nUpdated Access Permissions to the group\n")
    pprint.pprint(g1)
    """Demos the setting of the access properties by the Groups Settings API."""
    usage = "usage: %prog [options]"
    parser = OptionParser(usage=usage)
    parser.add_option("--groupId", help="Group email address")
    parser.add_option(
        "--whoCanInvite",
        help="Possible values: ALL_MANAGERS_CAN_INVITE, " "ALL_MEMBERS_CAN_INVITE",
        "--whoCanJoin",
        help="Possible values: ALL_IN_DOMAIN_CAN_JOIN, "
        "ANYONE_CAN_JOIN, CAN_REQUEST_TO_JOIN, "
        "CAN_REQUEST_TO_JOIN",
        "--whoCanPostMessage",
        help="Possible values: ALL_IN_DOMAIN_CAN_POST, "
        "ALL_MANAGERS_CAN_POST, ALL_MEMBERS_CAN_POST, "
        "ANYONE_CAN_POST, NONE_CAN_POST",
        "--whoCanViewGroup",
        help="Possible values: ALL_IN_DOMAIN_CAN_VIEW, "
        "ALL_MANAGERS_CAN_VIEW, ALL_MEMBERS_CAN_VIEW, "
        "ANYONE_CAN_VIEW",
        "--whoCanViewMembership",
    (options, args) = parser.parse_args()
    if options.groupId is None:
        print("Give the groupId for the group")
    settings = {}
        options.whoCanInvite
        or options.whoCanJoin
        or options.whoCanPostMessage
        or options.whoCanViewMembership
    ) is None:
        print("No access parameters given in input to update access permissions")
        settings = {
            "whoCanInvite": options.whoCanInvite,
            "whoCanJoin": options.whoCanJoin,
            "whoCanPostMessage": options.whoCanPostMessage,
            "whoCanViewGroup": options.whoCanViewGroup,
            "whoCanViewMembership": options.whoCanViewMembership,
    FLOW = flow_from_clientsecrets(
        scope="https://www.googleapis.com/auth/apps.groups.settings",
    storage = Storage("groupsettings.dat")
        print("invalid credentials")
        # Save the credentials in storage to be used in subsequent runs.
        credentials = run_flow(FLOW, storage)
    # Create an httplib2.Http object to handle our HTTP requests and authorize it
    # with our good Credentials.
    http = httplib2.Http()
    http = credentials.authorize(http)
    service = build("groupssettings", "v1", http=http)
    access_settings(service=service, groupId=options.groupId, settings=settings)
