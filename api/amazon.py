import requests
from bs4 import BeautifulSoup
import pyttsx3
import smtplib
engine = pyttsx3.init()
voices = engine.getProperty('voices')
engine.setProperty('voice', voices[0].id)
def speak(audio):
    engine.say(audio)
    engine.runAndWait()
def send_email():
    server = smtplib.SMTP('smtp.gmail.com', 587)
    server.ehlo()
    server.starttls()
    server.login('sendersemail', 'password')
    subject = 'Price fell down!'
    body = 'https://www.amazon.in/WOW-Brightening-Vitamin-Face-Wash/dp/B07SZ243VZ/ref=sr_1_6?dchild=1&keywords=wow+face+wash&qid=1594306550&smid=A27LPMZIGZ21IK&sr=8-6'
    content = f'Subject: {subject}\n\n{body}'
    server.sendmail('email', 'receiver email', content)
    server.close()
URL = 'https://www.amazon.in/WOW-Brightening-Vitamin-Face-Wash/dp/B07SZ243VZ/ref=sr_1_6?dchild=1&keywords=wow+face+wash&qid=1594306550&smid=A27LPMZIGZ21IK&sr=8-6'
headers = {
    "User-Agent": 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36'}
page = requests.get(URL, headers=headers)
soup = BeautifulSoup(page.content, 'html.parser')
title = soup.find(id='productTitle')
price = soup.find(id='priceblock_dealprice').get_text().strip()
speak(price)
price = price[1:5]
price = float(price)
send_email()
from .common import InfoExtractor
    traverse_obj,
class AmazonStoreIE(InfoExtractor):
    _VALID_URL = r'https?://(?:www\.)?amazon\.(?:[a-z]{2,3})(?:\.[a-z]{2})?/(?:[^/]+/)?(?:dp|gp/product)/(?P<id>[^/&#$?]+)'
    _TESTS = [{
        'url': 'https://www.amazon.co.uk/dp/B098XNCHLD/',
        'info_dict': {
            'id': 'B098XNCHLD',
            'title': str,
        'playlist_mincount': 1,
        'playlist': [{
                'id': 'A1F83G8C2ARO7P',
                'ext': 'mp4',
                'title': 'mcdodo usb c cable 100W 5a',
                'thumbnail': r're:^https?://.*\.jpg$',
                'duration': 34,
        'expected_warnings': ['Unable to extract data'],
        'url': 'https://www.amazon.in/Sony-WH-1000XM4-Cancelling-Headphones-Bluetooth/dp/B0863TXGM3',
            'id': 'B0863TXGM3',
        'playlist_mincount': 4,
        'url': 'https://www.amazon.com/dp/B0845NXCXF/',
            'id': 'B0845NXCXF',
        'playlist-mincount': 1,
        'url': 'https://www.amazon.es/Samsung-Smartphone-s-AMOLED-Quad-c%C3%A1mara-espa%C3%B1ola/dp/B08WX337PQ',
            'id': 'B08WX337PQ',
    def _real_extract(self, url):
        playlist_id = self._match_id(url)
        for retry in self.RetryManager():
            webpage = self._download_webpage(url, playlist_id)
                data_json = self._search_json(
                    r'var\s?obj\s?=\s?jQuery\.parseJSON\(\'', webpage, 'data', playlist_id,
                    transform_source=js_to_json)
            except ExtractorError as e:
                retry.error = e
        entries = [{
            'id': video['marketPlaceID'],
            'url': video['url'],
            'title': video.get('title'),
            'thumbnail': video.get('thumbUrl') or video.get('thumb'),
            'duration': video.get('durationSeconds'),
            'height': int_or_none(video.get('videoHeight')),
            'width': int_or_none(video.get('videoWidth')),
        } for video in (data_json.get('videos') or []) if video.get('isVideo') and video.get('url')]
        return self.playlist_result(entries, playlist_id=playlist_id, playlist_title=data_json.get('title'))
class AmazonReviewsIE(InfoExtractor):
    _VALID_URL = r'https?://(?:www\.)?amazon\.(?:[a-z]{2,3})(?:\.[a-z]{2})?/gp/customer-reviews/(?P<id>[^/&#$?]+)'
        'url': 'https://www.amazon.com/gp/customer-reviews/R10VE9VUSY19L3/ref=cm_cr_arp_d_rvw_ttl',
            'id': 'R10VE9VUSY19L3',
            'title': 'Get squad #Suspicious',
            'description': 'md5:7012695052f440a1e064e402d87e0afb',
            'uploader': 'Kimberly Cronkright',
            'average_rating': 1.0,
        'expected_warnings': ['Review body was not found in webpage'],
        'url': 'https://www.amazon.com/gp/customer-reviews/R10VE9VUSY19L3/ref=cm_cr_arp_d_rvw_ttl?language=es_US',
        'url': 'https://www.amazon.in/gp/customer-reviews/RV1CO8JN5VGXV/',
            'id': 'RV1CO8JN5VGXV',
            'title': 'Not sure about its durability',
            'description': 'md5:1a252c106357f0a3109ebf37d2e87494',
            'uploader': 'Shoaib Gulzar',
            'average_rating': 2.0,
        video_id = self._match_id(url)
            webpage = self._download_webpage(url, video_id)
            review_body = get_element_by_attribute('data-hook', 'review-body', webpage)
            if not review_body:
                retry.error = ExtractorError('Review body was not found in webpage', expected=True)
        formats, subtitles = [], {}
        manifest_url = self._search_regex(
            r'data-video-url="([^"]+)"', review_body, 'm3u8 url', default=None)
        if url_or_none(manifest_url):
            fmts, subtitles = self._extract_m3u8_formats_and_subtitles(
                manifest_url, video_id, 'mp4', fatal=False)
            formats.extend(fmts)
        video_url = self._search_regex(
            r'<input[^>]+\bvalue="([^"]+)"[^>]+\bclass="video-url"', review_body, 'mp4 url', default=None)
        if url_or_none(video_url):
            formats.append({
                'url': video_url,
                'format_id': 'http-mp4',
        if not formats:
            self.raise_no_formats('No video found for this customer review', expected=True)
            'id': video_id,
            'title': (clean_html(get_element_by_attribute('data-hook', 'review-title', webpage))
                      or self._html_extract_title(webpage)),
            'description': clean_html(traverse_obj(re.findall(
                r'<span(?:\s+class="cr-original-review-content")?>(.+?)</span>', review_body), -1)),
            'uploader': clean_html(get_element_by_class('a-profile-name', webpage)),
            'average_rating': float_or_none(clean_html(get_element_by_attribute(
                'data-hook', 'review-star-rating', webpage) or '').partition(' ')[0]),
            'thumbnail': self._search_regex(
                r'data-thumbnail-url="([^"]+)"', review_body, 'thumbnail', default=None),
            'formats': formats,
            'subtitles': subtitles,
