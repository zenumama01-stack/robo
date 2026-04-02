from urllib.request import urlopen
import urllib.parse
    chrome_path = '/usr/bin/google-chrome'
    chrome_path = 'open -a /Applications/Google\ Chrome.app'
    chrome_path = 'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe'
webbrowser.register('chrome', None, webbrowser.BackgroundBrowser(chrome_path))
def youtube(textToSearch):
    query = urllib.parse.quote(textToSearch)
    url = "https://www.youtube.com/results?search_query=" + query
    youtube('any text')
Youtube module for downloading and searching songs.
from typing import Any, Dict, List, Optional
from pytube import Search
from pytube import YouTube as PyTube
from pytube import innertube
from spotdl.providers.audio.base import AudioProvider
__all__ = ["YouTube"]
class YouTube(AudioProvider):
    YouTube audio provider class
    SUPPORTS_ISRC = False
    GET_RESULTS_OPTS: List[Dict[str, Any]] = [{}]
    def __init__(self, *args, **kwargs) -> None:
        Initialize the YouTube audio provider
        # Set the client version to a specific version to avoid issues with pytube
        # See #2323 or https://github.com/pytube/pytube/issues/296
        innertube._default_clients["WEB"]["context"]["client"][
            "clientVersion"
        ] = "2.20230427.04.00"
    def get_results(
        self, search_term: str, *_args, **_kwargs
    ) -> List[Result]:  # pylint: disable=W0221
        Get results from YouTube
        - search_term: The search term to search for.
        - args: Unused.
        - kwargs: Unused.
        - A list of YouTube results if found, None otherwise.
        search_results: Optional[List[PyTube]] = Search(search_term).results
        if not search_results:
        for result in search_results:
            if result.watch_url:
                    duration = result.length
                    duration = 0
                    views = result.views
                    views = 0
                results.append(
                    Result(
                        source=self.name,
                        url=result.watch_url,
                        verified=False,
                        name=result.title,
                        duration=duration,
                        author=result.author,
                        search_query=search_term,
                        views=views,
                        result_id=result.video_id,
