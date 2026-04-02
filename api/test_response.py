from typing import Any, List, Union, cast
from openai import OpenAI, BaseModel, AsyncOpenAI
from openai._response import (
    BinaryAPIResponse,
    AsyncBinaryAPIResponse,
class ConcreteBaseAPIResponse(APIResponse[bytes]): ...
class ConcreteAPIResponse(APIResponse[List[str]]): ...
class ConcreteAsyncAPIResponse(APIResponse[httpx.Response]): ...
def test_extract_response_type_direct_classes() -> None:
    assert extract_response_type(BaseAPIResponse[str]) == str
    assert extract_response_type(APIResponse[str]) == str
    assert extract_response_type(AsyncAPIResponse[str]) == str
def test_extract_response_type_direct_class_missing_type_arg() -> None:
        RuntimeError,
        match="Expected type <class 'openai._response.AsyncAPIResponse'> to have a type argument at index 0 but it did not",
        extract_response_type(AsyncAPIResponse)
def test_extract_response_type_concrete_subclasses() -> None:
    assert extract_response_type(ConcreteBaseAPIResponse) == bytes
    assert extract_response_type(ConcreteAPIResponse) == List[str]
    assert extract_response_type(ConcreteAsyncAPIResponse) == httpx.Response
def test_extract_response_type_binary_response() -> None:
    assert extract_response_type(BinaryAPIResponse) == bytes
    assert extract_response_type(AsyncBinaryAPIResponse) == bytes
    response = APIResponse(
async def test_async_response_parse_mismatched_basemodel(async_client: AsyncOpenAI) -> None:
    response = AsyncAPIResponse(
        client=async_client,
        await response.parse(to=PydanticModel)
async def test_async_response_parse_custom_stream(async_client: AsyncOpenAI) -> None:
    stream = await response.parse(to=Stream[int])
async def test_async_response_parse_custom_model(async_client: AsyncOpenAI) -> None:
    obj = await response.parse(to=CustomModel)
async def test_async_response_basemodel_request_id(client: OpenAI) -> None:
async def test_async_response_parse_annotated_type(async_client: AsyncOpenAI) -> None:
    obj = await response.parse(
async def test_async_response_parse_bool(client: AsyncOpenAI, content: str, expected: bool) -> None:
    result = await response.parse(to=bool)
class OtherModel(BaseModel):
@pytest.mark.parametrize("async_client", [False], indirect=True)  # loose validation
async def test_async_response_parse_expect_model_union_non_json_content(async_client: AsyncOpenAI) -> None:
    obj = await response.parse(to=cast(Any, Union[CustomModel, OtherModel]))
from django.template.response import (
    ContentNotRenderedError,
    SimpleTemplateResponse,
    TemplateResponse,
    RequestFactory,
from django.test.utils import require_jinja2
from .utils import TEMPLATE_DIR
def test_processor(request):
    return {"processors": "yes"}
test_processor_name = "template_tests.test_response.test_processor"
# A test middleware that installs a temporary URLConf
def custom_urlconf_middleware(get_response):
    def middleware(request):
        request.urlconf = "template_tests.alternate_urls"
        return get_response(request)
    return middleware
class SimpleTemplateResponseTest(SimpleTestCase):
    def _response(self, template="foo", *args, **kwargs):
        template = engines["django"].from_string(template)
        return SimpleTemplateResponse(template, *args, **kwargs)
    def test_template_resolving(self):
        response = SimpleTemplateResponse("first/test.html")
        response.render()
        self.assertEqual(response.content, b"First template\n")
        templates = ["foo.html", "second/test.html", "first/test.html"]
        response = SimpleTemplateResponse(templates)
        self.assertEqual(response.content, b"Second template\n")
        response = self._response()
        self.assertEqual(response.content, b"foo")
    def test_explicit_baking(self):
        # explicit baking
        self.assertFalse(response.is_rendered)
        self.assertTrue(response.is_rendered)
    def test_render(self):
        # response is not re-rendered without the render call
        response = self._response().render()
        # rebaking doesn't change the rendered content
        template = engines["django"].from_string("bar{{ baz }}")
        response.template_name = template
        # but rendered content can be overridden by manually
        # setting content
        response.content = "bar"
        self.assertEqual(response.content, b"bar")
    def test_iteration_unrendered(self):
        # unrendered response raises an exception on iteration
        def iteration():
            list(response)
        msg = "The response content must be rendered before it can be iterated over."
        with self.assertRaisesMessage(ContentNotRenderedError, msg):
            iteration()
    def test_iteration_rendered(self):
        # iteration works for rendered responses
        self.assertEqual(list(response), [b"foo"])
    def test_content_access_unrendered(self):
        # unrendered response raises an exception when content is accessed
        with self.assertRaises(ContentNotRenderedError):
            response.content
    def test_content_access_rendered(self):
        # rendered response content can be accessed
    def test_set_content(self):
        # content can be overridden
        response.content = "spam"
        self.assertEqual(response.content, b"spam")
        response.content = "baz"
        self.assertEqual(response.content, b"baz")
    def test_dict_context(self):
        response = self._response("{{ foo }}{{ processors }}", {"foo": "bar"})
        self.assertEqual(response.context_data, {"foo": "bar"})
    def test_kwargs(self):
        response = self._response(
            content_type="application/json", status=504, charset="ascii"
        self.assertEqual(response.headers["content-type"], "application/json")
        self.assertEqual(response.status_code, 504)
        self.assertEqual(response.charset, "ascii")
    def test_args(self):
        response = SimpleTemplateResponse("", {}, "application/json", 504)
    @require_jinja2
    def test_using(self):
        response = SimpleTemplateResponse("template_tests/using.html").render()
        self.assertEqual(response.content, b"DTL\n")
        response = SimpleTemplateResponse(
            "template_tests/using.html", using="django"
        ).render()
            "template_tests/using.html", using="jinja2"
        self.assertEqual(response.content, b"Jinja2\n")
    def test_post_callbacks(self):
        "Rendering a template response triggers the post-render callbacks"
        post = []
        def post1(obj):
            post.append("post1")
        def post2(obj):
            post.append("post2")
        response = SimpleTemplateResponse("first/test.html", {})
        response.add_post_render_callback(post1)
        response.add_post_render_callback(post2)
        # When the content is rendered, all the callbacks are invoked, too.
        self.assertEqual(post, ["post1", "post2"])
    def test_pickling(self):
        # Create a template response. The context is
        # known to be unpicklable (e.g., a function).
            "first/test.html",
                "value": 123,
                "fn": datetime.now,
            pickle.dumps(response)
        # But if we render the response, we can pickle it.
        pickled_response = pickle.dumps(response)
        unpickled_response = pickle.loads(pickled_response)
        self.assertEqual(unpickled_response.content, response.content)
            unpickled_response.headers["content-type"], response.headers["content-type"]
        self.assertEqual(unpickled_response.status_code, response.status_code)
        # ...and the unpickled response doesn't have the
        # template-related attributes, so it can't be re-rendered
        template_attrs = ("template_name", "context_data", "_post_render_callbacks")
        for attr in template_attrs:
            self.assertFalse(hasattr(unpickled_response, attr))
        # ...and requesting any of those attributes raises an exception
            with self.assertRaises(AttributeError):
                getattr(unpickled_response, attr)
    def test_repickling(self):
        pickle.dumps(unpickled_response)
    def test_pickling_cookie(self):
        response.cookies["key"] = "value"
        pickled_response = pickle.dumps(response, pickle.HIGHEST_PROTOCOL)
        self.assertEqual(unpickled_response.cookies["key"].value, "value")
    def test_headers(self):
            {"value": 123, "fn": datetime.now},
            headers={"X-Foo": "foo"},
        self.assertEqual(response.headers["X-Foo"], "foo")
            "DIRS": [TEMPLATE_DIR],
                "context_processors": [test_processor_name],
class TemplateResponseTest(SimpleTestCase):
    factory = RequestFactory()
        self._request = self.factory.get("/")
        return TemplateResponse(self._request, template, *args, **kwargs)
        response = self._response("{{ foo }}{{ processors }}").render()
        self.assertEqual(response.content, b"yes")
    def test_render_with_requestcontext(self):
        response = self._response("{{ foo }}{{ processors }}", {"foo": "bar"}).render()
        self.assertEqual(response.content, b"baryes")
    def test_context_processor_priority(self):
        # context processors should be overridden by passed-in context
            "{{ foo }}{{ processors }}", {"processors": "no"}
        self.assertEqual(response.content, b"no")
        response = self._response(content_type="application/json", status=504)
        response = TemplateResponse(
            self.factory.get("/"), "", {}, "application/json", 504
        request = self.factory.get("/")
        response = TemplateResponse(request, "template_tests/using.html").render()
            request, "template_tests/using.html", using="django"
            request, "template_tests/using.html", using="jinja2"
            self.factory.get("/"),
        template_attrs = (
            "template_name",
            "context_data",
            "_post_render_callbacks",
            "_request",
    MIDDLEWARE={"append": ["template_tests.test_response.custom_urlconf_middleware"]}
@override_settings(ROOT_URLCONF="template_tests.urls")
class CustomURLConfTest(SimpleTestCase):
    def test_custom_urlconf(self):
        response = self.client.get("/template_response_view/")
        self.assertContains(response, "This is where you can find the snark: /snark/")
    MIDDLEWARE={
        "append": [
            "django.middleware.cache.FetchFromCacheMiddleware",
            "django.middleware.cache.UpdateCacheMiddleware",
    CACHE_MIDDLEWARE_SECONDS=2, ROOT_URLCONF="template_tests.alternate_urls"
class CacheMiddlewareTest(SimpleTestCase):
    def test_middleware_caching(self):
        time.sleep(1.0)
        response2 = self.client.get("/template_response_view/")
        self.assertEqual(response2.status_code, 200)
        self.assertEqual(response.content, response2.content)
        time.sleep(2.0)
        # Let the cache expire and test again
        self.assertNotEqual(response.content, response2.content)
