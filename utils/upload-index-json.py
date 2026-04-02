from urllib.request import Request, urlopen
sys.path.append(
  os.path.abspath(os.path.dirname(os.path.abspath(__file__)) + "/../.."))
from lib.util import store_artifact, scoped_cwd, safe_mkdir, get_out_dir, \
  ELECTRON_DIR
OUT_DIR     = get_out_dir()
BASE_URL = 'https://electron-metadumper.herokuapp.com/?version='
AUTH_TOKEN = os.getenv('META_DUMPER_AUTH_HEADER')
def is_json(myjson):
    json.loads(myjson)
  except ValueError:
def get_content(version, retry_count=5):
  for attempt in range(retry_count):
      request = Request(
        BASE_URL + version,
        headers={"Authorization": AUTH_TOKEN}
      with urlopen(request) as resp:
        proposed_content = resp.read()
      if is_json(proposed_content):
        return proposed_content
      print("Received content is not valid JSON.")
      if attempt == retry_count - 1:
      print(f"Attempt {attempt + 1} failed: {e}")
    if attempt < retry_count - 1:
      print("Retrying...")
  if not AUTH_TOKEN or AUTH_TOKEN == "":
    raise Exception("Please set META_DUMPER_AUTH_HEADER")
  if len(sys.argv) < 2 or not sys.argv[1]:
    raise Exception("Version is required")
  version = sys.argv[1]
  with scoped_cwd(ELECTRON_DIR):
    safe_mkdir(OUT_DIR)
    index_json = os.path.relpath(os.path.join(OUT_DIR, 'index.json'))
    new_content = get_content(version)
    if new_content is None:
      raise Exception("Failed to fetch valid JSON after maximum retries.")
    with open(index_json, "wb") as f:
      f.write(new_content)
    store_artifact(OUT_DIR, 'headers/dist', [index_json])
