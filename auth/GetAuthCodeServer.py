'''
    from http.server import HTTPServer, BaseHTTPRequestHandler
    from SimpleHTTPServer import SimpleHTTPRequestHandler as BaseHTTPRequestHandler
    from SocketServer import TCPServer as HTTPServer
    from urllib.parse import urlparse, parse_qs, unquote
    from urlparse import urlparse, parse_qs
    from urllib import unquote
def get_auth_code(auth_url, redirect_uri):
    """Easy way to get the auth code. Wraps up all the threading
    and stuff. Does block main thread.
        auth_url (str): URL of auth server.
        redirect_uri (str): Redirect URI, as set for the app. Should be 
            something like "http://localhost:8080" for this to work.
        str: A string representing the auth code, sent back by the server
    url_netloc = urlparse(redirect_uri).netloc
    if ':' not in url_netloc:
        host_address = url_netloc
        port = 80 # default port
        host_address, port = url_netloc.split(':')
        port = int(port)
    # Set up HTTP server and thread
    code_acquired = threading.Event()
    s = GetAuthCodeServer((host_address, port), code_acquired, GetAuthCodeRequestHandler)    
    th = threading.Thread(target=s.serve_forever)
    th.start()
    webbrowser.open(auth_url)
    # At this point the browser will open and the code
    # will be extracted by the server
    code_acquired.wait()  # First wait for the response from the auth server
    code = s.auth_code
    s.shutdown()
    th.join()
class GetAuthCodeServer(HTTPServer, object):
    def __init__(self, server_address, stop_event, RequestHandlerClass):
        HTTPServer.__init__(self, server_address, RequestHandlerClass)
        self._stop_event = stop_event
        self.auth_code = None
    def auth_code(self):
        return self._auth_code
    @auth_code.setter
    def auth_code(self, value):
        self._auth_code = value
            self._stop_event.set()
class GetAuthCodeRequestHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        params = parse_qs(urlparse(self.path).query)
        if "code" in params:
            # Extract the code query param
            self.server.auth_code = params["code"][0]
        if "error" in params:
            error_msg, error_desc = (unquote(params["error"][0]),
                                     unquote(params["error_description"][0]))
            raise RuntimeError("The server returned an error: {} - {}"
                               .format(error_msg, error_desc))
        self.send_response(200)
        self.send_header("Content-type", "text/html")
        self.end_headers()
        self.wfile.write(bytes(
            '<script type="text/javascript">window.close()</script>'
            .encode("utf-8")))
