"""Simple command-line sample for Google Maps Engine.
This sample code demonstrates use of the Google Maps Engine API.  For more
information on the API, see developers.google.com/maps-engine/documentation/
These samples allow you to
1) List projects you have access to
2) List tables in a given project.
3) Upload a shapefile to create a Table asset.
  $ python maps_engine.py [-p project_id] [-s shapefile]
If you do not enter a shapefile, it will upload the included "polygons".
  $ python maps_engine.py --help
  $ python maps_engine.py -p 123456 --logging_level=DEBUG
__author__ = "jlivni@google.com (Josh Livni)"
from googleapiclient.http import MediaFileUpload
logging.basicConfig(level=logging.INFO)
argparser.add_argument("-p", "--project_id", help="optional GME Project ID")
argparser.add_argument("-s", "--shapefile", help="Shapefile (without the .shp)")
SUCCESSFUL_STATUS = ["processed", "complete", "ready"]
class MapsEngineSampleException(Exception):
    """Catch this for failures specific to this sample code."""
def ListProjects(service):
    """List the projects available to the authorized account.
    projects = service.projects().list().execute()
    logging.info(json.dumps(projects, indent=2))
def ListTables(service, project_id):
    """List the tables in a given project.
      project_id: string, id of the GME project.
    tables = service.tables().list(projectId=project_id).execute()
    logging.info(json.dumps(tables, indent=2))
def UploadShapefile(service, project_id, shapefile_prefix):
    """Upload a shapefile to a given project, and display status when complete.
      shapefile_prefix: string, the shapefile without the .shp suffix.
      String id of the table asset.
    # A shapefile is actually a bunch of files; GME requires these four suffixes.
    suffixes = ["shp", "dbf", "prj", "shx"]
    for suffix in suffixes:
        files.append({"filename": "%s.%s" % (shapefile_prefix, suffix)})
        "projectId": project_id,
        "name": shapefile_prefix,
        "description": "polygons that were uploaded by a script",
        # You need the string value of a valid shared and published ACL
        # Check the "Access Lists" section of the Maps Engine UI for a list.
        "draftAccessList": "Map Editors",
        "tags": [shapefile_prefix, "auto_upload", "kittens"],
    logging.info("Uploading metadata for %s", shapefile_prefix)
    response = service.tables().upload(body=metadata).execute()
    # We have now created an empty asset.
    table_id = response["id"]
    # And now upload each of the files individually, passing in the table id.
        shapefile = "%s.%s" % (shapefile_prefix, suffix)
        media_body = MediaFileUpload(shapefile, mimetype="application/octet-stream")
        logging.info("uploading %s", shapefile)
        response = (
            service.tables()
            .files()
            .insert(id=table_id, filename=shapefile, media_body=media_body)
    # With all files uploaded, check status of the asset to ensure it's processed.
    CheckAssetStatus(service, "tables", table_id)
    return table_id
def CheckAssetStatus(service, asset_type, asset_id):
    endpoint = getattr(service, asset_type)
    response = endpoint().get(id=asset_id).execute()
    status = response["processingStatus"]
    logging.info("Asset Status: %s", status)
    if status in SUCCESSFUL_STATUS:
        logging.info("asset successfully processed; the id is %s", asset_id)
        logging.info("Asset %s; will check again in 5 seconds", status)
        time.sleep(5)
        CheckAssetStatus(service, asset_type, asset_id)
        "mapsengine",
        scope="https://www.googleapis.com/auth/mapsengine",
    if flags.project_id:
        # ListTables(service, flags.project_id)
        # The example polygons shapefile should be in this directory.
        filename = flags.shapefile or "polygons"
        table_id = UploadShapefile(service, flags.project_id, filename)
        logging.info("Sucessfully created table: %s", table_id)
        ListProjects(service)
