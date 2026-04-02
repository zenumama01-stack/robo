from email.utils import parsedate_to_datetime
from tweepy.mixins import HashableID
class Model:
    def __init__(self, api=None):
        self._api = api
        pickle = self.__dict__.copy()
        pickle.pop('_api', None)
        return pickle
    @classmethod
    def parse(cls, api, json):
        """Parse a JSON object into a model instance."""
    def parse_list(cls, api, json_list):
            Parse a list of JSON objects into
            a result set of model instances.
        results = ResultSet()
        if isinstance(json_list, dict):
            # Handle map parameter for statuses/lookup
            if 'id' in json_list:
                for _id, obj in json_list['id'].items():
                        results.append(cls.parse(api, obj))
                        results.append(cls.parse(api, {'id': int(_id)}))
            # Handle premium search
            if 'results' in json_list:
                json_list = json_list['results']
        for obj in json_list:
        state = [f'{k}={v!r}' for (k, v) in vars(self).items()]
        return f'{self.__class__.__name__}({", ".join(state)})'
class ResultSet(list):
    """A list like object that holds results from a Twitter API query."""
    def __init__(self, max_id=None, since_id=None):
        self._max_id = max_id
        self._since_id = since_id
    @property
    def max_id(self):
        if self._max_id:
            return self._max_id
        ids = self.ids()
        # Max_id is always set to the *smallest* id, minus one, in the set
        return (min(ids) - 1) if ids else None
    def since_id(self):
        if self._since_id:
            return self._since_id
        # Since_id is always set to the *greatest* id in the set
        return max(ids) if ids else None
    def ids(self):
        return [item.id for item in self if hasattr(item, 'id')]
class BoundingBox(Model):
        result = cls(api)
        if json is not None:
            for k, v in json.items():
                setattr(result, k, v)
    def origin(self):
        Return longitude, latitude of southwest (bottom, left) corner of
        bounding box, as a tuple.
        This assumes that bounding box is always a rectangle, which
        appears to be the case at present.
        return tuple(self.coordinates[0][0])
    def corner(self):
        Return longitude, latitude of northeast (top, right) corner of
        return tuple(self.coordinates[0][2])
class DirectMessage(Model):
        dm = cls(api)
        if "event" in json:
            json = json["event"]
        setattr(dm, '_json', json)
            setattr(dm, k, v)
        return dm
        if isinstance(json_list, list):
            item_list = json_list
            item_list = json_list['events']
        for obj in item_list:
    def delete(self):
        return self._api.delete_direct_message(self.id)
class Friendship(Model):
        relationship = json['relationship']
        # parse source
        source = cls(api)
        setattr(source, '_json', relationship['source'])
        for k, v in relationship['source'].items():
            setattr(source, k, v)
        # parse target
        target = cls(api)
        setattr(target, '_json', relationship['target'])
        for k, v in relationship['target'].items():
            setattr(target, k, v)
        return source, target
class List(Model):
        lst = List(api)
        setattr(lst, '_json', json)
            if k == 'user':
                setattr(lst, k, User.parse(api, v))
            elif k == 'created_at':
                setattr(lst, k, parsedate_to_datetime(v))
                setattr(lst, k, v)
        return lst
    def parse_list(cls, api, json_list, result_set=None):
            json_list = json_list['lists']
    def update(self, **kwargs):
        return self._api.update_list(list_id=self.id, **kwargs)
    def destroy(self):
        return self._api.destroy_list(list_id=self.id)
    def timeline(self, **kwargs):
        return self._api.list_timeline(list_id=self.id, **kwargs)
    def add_member(self, id):
        return self._api.add_list_member(list_id=self.id, user_id=id)
    def remove_member(self, id):
        return self._api.remove_list_member(list_id=self.id, user_id=id)
    def members(self, **kwargs):
        return self._api.get_list_members(list_id=self.id, **kwargs)
    def subscribe(self):
        return self._api.subscribe_list(list_id=self.id)
    def unsubscribe(self):
        return self._api.unsubscribe_list(list_id=self.id)
    def subscribers(self, **kwargs):
        return self._api.get_list_subscribers(list_id=self.id, **kwargs)
class Media(Model):
        media = cls(api)
            setattr(media, k, v)
class Place(Model):
        place = cls(api)
            if k == 'bounding_box':
                # bounding_box value may be null (None.)
                # Example: "United States" (id=96683cc9126741d1)
                if v is not None:
                    t = BoundingBox.parse(api, v)
                    t = v
                setattr(place, k, t)
            elif k == 'contained_within':
                # contained_within is a list of Places.
                setattr(place, k, Place.parse_list(api, v))
                setattr(place, k, v)
        return place
            item_list = json_list['result']['places']
class Relationship(Model):
            if k == 'connections':
                setattr(result, 'is_following', 'following' in v)
                setattr(result, 'is_followed_by', 'followed_by' in v)
                setattr(result, 'is_muted', 'muting' in v)
                setattr(result, 'is_blocked', 'blocking' in v)
                setattr(result, 'is_following_requested', 'following_requested' in v)
                setattr(result, 'no_relationship', 'none' in v)
class SavedSearch(Model):
        ss = cls(api)
            if k == 'created_at':
                setattr(ss, k, parsedate_to_datetime(v))
                setattr(ss, k, v)
        return ss
        return self._api.destroy_saved_search(self.id)
class SearchResults(ResultSet):
        metadata = json['search_metadata']
        results = SearchResults()
        results.refresh_url = metadata.get('refresh_url')
        results.completed_in = metadata.get('completed_in')
        results.query = metadata.get('query')
        results.count = metadata.get('count')
        results.next_results = metadata.get('next_results')
            status_model = api.parser.model_factory.status
            status_model = Status
        for status in json['statuses']:
            results.append(status_model.parse(api, status))
class Status(Model, HashableID):
        status = cls(api)
        setattr(status, '_json', json)
                    user = api.parser.model_factory.user.parse(api, v)
                    user = User.parse(api, v)
                setattr(status, 'author', user)
                setattr(status, 'user', user)  # DEPRECIATED
                setattr(status, k, parsedate_to_datetime(v))
            elif k == 'source':
                if '<' in v:
                    # At this point, v should be of the format:
                    # <a href="{source_url}" rel="nofollow">{source}</a>
                    setattr(status, k, v[v.find('>') + 1:v.rfind('<')])
                    start = v.find('"') + 1
                    end = v.find('"', start)
                    setattr(status, 'source_url', v[start:end])
                    setattr(status, k, v)
                    setattr(status, 'source_url', None)
            elif k == 'retweeted_status':
                setattr(status, k, Status.parse(api, v))
            elif k == 'quoted_status':
            elif k == 'place':
                    setattr(status, k, Place.parse(api, v))
                    setattr(status, k, None)
        return status
        return self._api.destroy_status(self.id)
    def retweet(self):
        return self._api.retweet(self.id)
    def retweets(self):
        return self._api.get_retweets(self.id)
    def favorite(self):
        return self._api.create_favorite(self.id)
class User(Model, HashableID):
        user = cls(api)
        setattr(user, '_json', json)
                setattr(user, k, parsedate_to_datetime(v))
            elif k == 'status':
                setattr(user, k, Status.parse(api, v))
            elif k == 'following':
                # twitter sets this to null if it is false
                if v is True:
                    setattr(user, k, True)
                    setattr(user, k, False)
                setattr(user, k, v)
        return user
            item_list = json_list['users']
        return self._api.user_timeline(user_id=self.id, **kwargs)
    def friends(self, **kwargs):
        return self._api.get_friends(user_id=self.id, **kwargs)
    def followers(self, **kwargs):
        return self._api.get_followers(user_id=self.id, **kwargs)
    def follow(self):
        self._api.create_friendship(user_id=self.id)
        self.following = True
    def unfollow(self):
        self._api.destroy_friendship(user_id=self.id)
        self.following = False
    def list_memberships(self, *args, **kwargs):
        return self._api.get_list_memberships(user_id=self.id, *args, **kwargs)
    def list_ownerships(self, *args, **kwargs):
        return self._api.get_list_ownerships(user_id=self.id, *args, **kwargs)
    def list_subscriptions(self, *args, **kwargs):
        return self._api.get_list_subscriptions(
            user_id=self.id, *args, **kwargs
    def lists(self, *args, **kwargs):
        return self._api.get_lists(user_id=self.id, *args, **kwargs)
    def follower_ids(self, *args, **kwargs):
        return self._api.get_follower_ids(user_id=self.id, *args, **kwargs)
class IDModel(Model):
        if isinstance(json, list):
            return json['ids']
class JSONModel(Model):
class ModelFactory:
    Used by parsers for creating instances
    of models. You may subclass this factory
    to add your own extended models.
    bounding_box = BoundingBox
    direct_message = DirectMessage
    friendship = Friendship
    list = List
    media = Media
    place = Place
    relationship = Relationship
    saved_search = SavedSearch
    search_results = SearchResults
    status = Status
    user = User
    ids = IDModel
    json = JSONModel
    sub = subparser.add_parser("models.list")
    sub.set_defaults(func=CLIModels.list)
    sub = subparser.add_parser("models.retrieve")
    sub.add_argument("-i", "--id", required=True, help="The model ID")
    sub.set_defaults(func=CLIModels.get, args_model=CLIModelIDArgs)
    sub = subparser.add_parser("models.delete")
    sub.set_defaults(func=CLIModels.delete, args_model=CLIModelIDArgs)
class CLIModelIDArgs(BaseModel):
class CLIModels:
    def get(args: CLIModelIDArgs) -> None:
        model = get_client().models.retrieve(model=args.id)
    def delete(args: CLIModelIDArgs) -> None:
        model = get_client().models.delete(model=args.id)
        models = get_client().models.list()
        for model in models:
from .._types import Body, Query, Headers, NotGiven, not_given
from .._utils import path_template
from ..pagination import SyncPage, AsyncPage
from ..types.model import Model
    AsyncPaginator,
from ..types.model_deleted import ModelDeleted
__all__ = ["Models", "AsyncModels"]
class Models(SyncAPIResource):
    def with_raw_response(self) -> ModelsWithRawResponse:
        return ModelsWithRawResponse(self)
    def with_streaming_response(self) -> ModelsWithStreamingResponse:
        return ModelsWithStreamingResponse(self)
    ) -> Model:
        Retrieves a model instance, providing basic information about the model such as
        the owner and permissioning.
        if not model:
            raise ValueError(f"Expected a non-empty value for `model` but received {model!r}")
            path_template("/models/{model}", model=model),
            cast_to=Model,
    ) -> SyncPage[Model]:
        Lists the currently available models, and provides basic information about each
        one such as the owner and availability.
            "/models",
            page=SyncPage[Model],
            model=Model,
    ) -> ModelDeleted:
        """Delete a fine-tuned model.
        You must have the Owner role in your organization to
        delete a model.
            cast_to=ModelDeleted,
class AsyncModels(AsyncAPIResource):
    def with_raw_response(self) -> AsyncModelsWithRawResponse:
        return AsyncModelsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncModelsWithStreamingResponse:
        return AsyncModelsWithStreamingResponse(self)
    ) -> AsyncPaginator[Model, AsyncPage[Model]]:
            page=AsyncPage[Model],
class ModelsWithRawResponse:
    def __init__(self, models: Models) -> None:
        self._models = models
            models.retrieve,
            models.list,
            models.delete,
class AsyncModelsWithRawResponse:
    def __init__(self, models: AsyncModels) -> None:
class ModelsWithStreamingResponse:
class AsyncModelsWithStreamingResponse:
from dataclasses import dataclass, field
from functools import cached_property, partial
from typing import Any, Literal
from fastapi._compat import ModelField
from fastapi.security.base import SecurityBase
from fastapi.types import DependencyCacheKey
if sys.version_info >= (3, 13):  # pragma: no cover
    from inspect import iscoroutinefunction
else:  # pragma: no cover
    from asyncio import iscoroutinefunction
def _unwrapped_call(call: Callable[..., Any] | None) -> Any:
    if call is None:
        return call  # pragma: no cover
    unwrapped = inspect.unwrap(_impartial(call))
    return unwrapped
def _impartial(func: Callable[..., Any]) -> Callable[..., Any]:
    while isinstance(func, partial):
        func = func.func
    return func
class Dependant:
    path_params: list[ModelField] = field(default_factory=list)
    query_params: list[ModelField] = field(default_factory=list)
    header_params: list[ModelField] = field(default_factory=list)
    cookie_params: list[ModelField] = field(default_factory=list)
    body_params: list[ModelField] = field(default_factory=list)
    dependencies: list["Dependant"] = field(default_factory=list)
    name: str | None = None
    call: Callable[..., Any] | None = None
    request_param_name: str | None = None
    websocket_param_name: str | None = None
    http_connection_param_name: str | None = None
    response_param_name: str | None = None
    background_tasks_param_name: str | None = None
    security_scopes_param_name: str | None = None
    own_oauth_scopes: list[str] | None = None
    parent_oauth_scopes: list[str] | None = None
    use_cache: bool = True
    path: str | None = None
    scope: Literal["function", "request"] | None = None
    def oauth_scopes(self) -> list[str]:
        scopes = self.parent_oauth_scopes.copy() if self.parent_oauth_scopes else []
        # This doesn't use a set to preserve order, just in case
        for scope in self.own_oauth_scopes or []:
            if scope not in scopes:
                scopes.append(scope)
    def cache_key(self) -> DependencyCacheKey:
        scopes_for_cache = (
            tuple(sorted(set(self.oauth_scopes or []))) if self._uses_scopes else ()
            self.call,
            scopes_for_cache,
            self.computed_scope or "",
    def _uses_scopes(self) -> bool:
        if self.own_oauth_scopes:
        if self.security_scopes_param_name is not None:
        if self._is_security_scheme:
        for sub_dep in self.dependencies:
            if sub_dep._uses_scopes:
    def _is_security_scheme(self) -> bool:
        if self.call is None:
            return False  # pragma: no cover
        unwrapped = _unwrapped_call(self.call)
        return isinstance(unwrapped, SecurityBase)
    # Mainly to get the type of SecurityBase, but it's the same self.call
    def _security_scheme(self) -> SecurityBase:
        assert isinstance(unwrapped, SecurityBase)
    def _security_dependencies(self) -> list["Dependant"]:
        security_deps = [dep for dep in self.dependencies if dep._is_security_scheme]
        return security_deps
    def is_gen_callable(self) -> bool:
        if inspect.isgeneratorfunction(
            _impartial(self.call)
        ) or inspect.isgeneratorfunction(_unwrapped_call(self.call)):
        if inspect.isclass(_unwrapped_call(self.call)):
        dunder_call = getattr(_impartial(self.call), "__call__", None)  # noqa: B004
        if dunder_call is None:
            _impartial(dunder_call)
        ) or inspect.isgeneratorfunction(_unwrapped_call(dunder_call)):
        dunder_unwrapped_call = getattr(_unwrapped_call(self.call), "__call__", None)  # noqa: B004
        if dunder_unwrapped_call is None:
            _impartial(dunder_unwrapped_call)
        ) or inspect.isgeneratorfunction(_unwrapped_call(dunder_unwrapped_call)):
    def is_async_gen_callable(self) -> bool:
        if inspect.isasyncgenfunction(
        ) or inspect.isasyncgenfunction(_unwrapped_call(self.call)):
        ) or inspect.isasyncgenfunction(_unwrapped_call(dunder_call)):
        ) or inspect.isasyncgenfunction(_unwrapped_call(dunder_unwrapped_call)):
    def is_coroutine_callable(self) -> bool:
        if inspect.isroutine(_impartial(self.call)) and iscoroutinefunction(
        if inspect.isroutine(_unwrapped_call(self.call)) and iscoroutinefunction(
            _unwrapped_call(self.call)
        if iscoroutinefunction(_impartial(dunder_call)) or iscoroutinefunction(
            _unwrapped_call(dunder_call)
        if iscoroutinefunction(
        ) or iscoroutinefunction(_unwrapped_call(dunder_unwrapped_call)):
    def computed_scope(self) -> str | None:
        if self.scope:
            return self.scope
        if self.is_gen_callable or self.is_async_gen_callable:
            return "request"
from collections.abc import Callable, Iterable, Mapping
from typing import Annotated, Any, Literal, Optional, Union
from fastapi._compat import with_info_plain_validator_function
from pydantic import (
    AnyUrl,
    BaseModel,
    GetJsonSchemaHandler,
from typing_extensions import deprecated as typing_deprecated
    import email_validator
    assert email_validator  # make autoflake ignore the unused import
    from pydantic import EmailStr
    class EmailStr(str):  # type: ignore  # ty: ignore[unused-ignore-comment]
        def __get_validators__(cls) -> Iterable[Callable[..., Any]]:
            yield cls.validate
        def validate(cls, v: Any) -> str:
                "email-validator not installed, email fields will be treated as str.\n"
                "To install, run: pip install email-validator"
            return str(v)
        def _validate(cls, __input_value: Any, _: Any) -> str:
            return str(__input_value)
        def __get_pydantic_json_schema__(
            cls, core_schema: Mapping[str, Any], handler: GetJsonSchemaHandler
            return {"type": "string", "format": "email"}
        def __get_pydantic_core_schema__(
            cls, source: type[Any], handler: Callable[[Any], Mapping[str, Any]]
        ) -> Mapping[str, Any]:
            return with_info_plain_validator_function(cls._validate)
class BaseModelWithConfig(BaseModel):
    model_config = {"extra": "allow"}
class Contact(BaseModelWithConfig):
    url: AnyUrl | None = None
    email: EmailStr | None = None
class License(BaseModelWithConfig):
    identifier: str | None = None
class Info(BaseModelWithConfig):
    summary: str | None = None
    termsOfService: str | None = None
    contact: Contact | None = None
    license: License | None = None
class ServerVariable(BaseModelWithConfig):
    enum: Annotated[list[str] | None, Field(min_length=1)] = None
    default: str
class Server(BaseModelWithConfig):
    url: AnyUrl | str
    variables: dict[str, ServerVariable] | None = None
class Reference(BaseModel):
    ref: str = Field(alias="$ref")
class Discriminator(BaseModel):
    propertyName: str
    mapping: dict[str, str] | None = None
class XML(BaseModelWithConfig):
    namespace: str | None = None
    prefix: str | None = None
    attribute: bool | None = None
    wrapped: bool | None = None
class ExternalDocumentation(BaseModelWithConfig):
    url: AnyUrl
# Ref JSON Schema 2020-12: https://json-schema.org/draft/2020-12/json-schema-validation#name-type
SchemaType = Literal[
    "array", "boolean", "integer", "null", "number", "object", "string"
class Schema(BaseModelWithConfig):
    # Ref: JSON Schema 2020-12: https://json-schema.org/draft/2020-12/json-schema-core.html#name-the-json-schema-core-vocabu
    # Core Vocabulary
    schema_: str | None = Field(default=None, alias="$schema")
    vocabulary: str | None = Field(default=None, alias="$vocabulary")
    id: str | None = Field(default=None, alias="$id")
    anchor: str | None = Field(default=None, alias="$anchor")
    dynamicAnchor: str | None = Field(default=None, alias="$dynamicAnchor")
    ref: str | None = Field(default=None, alias="$ref")
    dynamicRef: str | None = Field(default=None, alias="$dynamicRef")
    defs: dict[str, "SchemaOrBool"] | None = Field(default=None, alias="$defs")
    comment: str | None = Field(default=None, alias="$comment")
    # Ref: JSON Schema 2020-12: https://json-schema.org/draft/2020-12/json-schema-core.html#name-a-vocabulary-for-applying-s
    # A Vocabulary for Applying Subschemas
    allOf: list["SchemaOrBool"] | None = None
    anyOf: list["SchemaOrBool"] | None = None
    oneOf: list["SchemaOrBool"] | None = None
    not_: Optional["SchemaOrBool"] = Field(default=None, alias="not")
    if_: Optional["SchemaOrBool"] = Field(default=None, alias="if")
    then: Optional["SchemaOrBool"] = None
    else_: Optional["SchemaOrBool"] = Field(default=None, alias="else")
    dependentSchemas: dict[str, "SchemaOrBool"] | None = None
    prefixItems: list["SchemaOrBool"] | None = None
    items: Optional["SchemaOrBool"] = None
    contains: Optional["SchemaOrBool"] = None
    properties: dict[str, "SchemaOrBool"] | None = None
    patternProperties: dict[str, "SchemaOrBool"] | None = None
    additionalProperties: Optional["SchemaOrBool"] = None
    propertyNames: Optional["SchemaOrBool"] = None
    unevaluatedItems: Optional["SchemaOrBool"] = None
    unevaluatedProperties: Optional["SchemaOrBool"] = None
    # Ref: JSON Schema Validation 2020-12: https://json-schema.org/draft/2020-12/json-schema-validation.html#name-a-vocabulary-for-structural
    # A Vocabulary for Structural Validation
    type: SchemaType | list[SchemaType] | None = None
    enum: list[Any] | None = None
    const: Any | None = None
    multipleOf: float | None = Field(default=None, gt=0)
    maximum: float | None = None
    exclusiveMaximum: float | None = None
    minimum: float | None = None
    exclusiveMinimum: float | None = None
    maxLength: int | None = Field(default=None, ge=0)
    minLength: int | None = Field(default=None, ge=0)
    pattern: str | None = None
    maxItems: int | None = Field(default=None, ge=0)
    minItems: int | None = Field(default=None, ge=0)
    uniqueItems: bool | None = None
    maxContains: int | None = Field(default=None, ge=0)
    minContains: int | None = Field(default=None, ge=0)
    maxProperties: int | None = Field(default=None, ge=0)
    minProperties: int | None = Field(default=None, ge=0)
    required: list[str] | None = None
    dependentRequired: dict[str, set[str]] | None = None
    # Ref: JSON Schema Validation 2020-12: https://json-schema.org/draft/2020-12/json-schema-validation.html#name-vocabularies-for-semantic-c
    # Vocabularies for Semantic Content With "format"
    format: str | None = None
    # Ref: JSON Schema Validation 2020-12: https://json-schema.org/draft/2020-12/json-schema-validation.html#name-a-vocabulary-for-the-conten
    # A Vocabulary for the Contents of String-Encoded Data
    contentEncoding: str | None = None
    contentMediaType: str | None = None
    contentSchema: Optional["SchemaOrBool"] = None
    # Ref: JSON Schema Validation 2020-12: https://json-schema.org/draft/2020-12/json-schema-validation.html#name-a-vocabulary-for-basic-meta
    # A Vocabulary for Basic Meta-Data Annotations
    title: str | None = None
    default: Any | None = None
    deprecated: bool | None = None
    readOnly: bool | None = None
    writeOnly: bool | None = None
    examples: list[Any] | None = None
    # Ref: OpenAPI 3.1.0: https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.1.0.md#schema-object
    # Schema Object
    discriminator: Discriminator | None = None
    xml: XML | None = None
    externalDocs: ExternalDocumentation | None = None
    example: Annotated[
        Any | None,
        typing_deprecated(
            "Deprecated in OpenAPI 3.1.0 that now uses JSON Schema 2020-12, "
            "although still supported. Use examples instead."
# Ref: https://json-schema.org/draft/2020-12/json-schema-core.html#name-json-schema-documents
# A JSON Schema MUST be an object or a boolean.
SchemaOrBool = Schema | bool
class Example(TypedDict, total=False):
    summary: str | None
    description: str | None
    value: Any | None
    externalValue: AnyUrl | None
    __pydantic_config__ = {"extra": "allow"}  # type: ignore[misc]
class ParameterInType(Enum):
    query = "query"
    header = "header"
    path = "path"
    cookie = "cookie"
class Encoding(BaseModelWithConfig):
    contentType: str | None = None
    headers: dict[str, Union["Header", Reference]] | None = None
    style: str | None = None
    explode: bool | None = None
    allowReserved: bool | None = None
class MediaType(BaseModelWithConfig):
    schema_: Schema | Reference | None = Field(default=None, alias="schema")
    example: Any | None = None
    examples: dict[str, Example | Reference] | None = None
    encoding: dict[str, Encoding] | None = None
class ParameterBase(BaseModelWithConfig):
    required: bool | None = None
    # Serialization rules for simple scenarios
    # Serialization rules for more complex scenarios
    content: dict[str, MediaType] | None = None
class Parameter(ParameterBase):
    in_: ParameterInType = Field(alias="in")
class Header(ParameterBase):
class RequestBody(BaseModelWithConfig):
    content: dict[str, MediaType]
class Link(BaseModelWithConfig):
    operationRef: str | None = None
    operationId: str | None = None
    parameters: dict[str, Any | str] | None = None
    requestBody: Any | str | None = None
    server: Server | None = None
class Response(BaseModelWithConfig):
    headers: dict[str, Header | Reference] | None = None
    links: dict[str, Link | Reference] | None = None
class Operation(BaseModelWithConfig):
    tags: list[str] | None = None
    parameters: list[Parameter | Reference] | None = None
    requestBody: RequestBody | Reference | None = None
    # Using Any for Specification Extensions
    responses: dict[str, Response | Any] | None = None
    callbacks: dict[str, dict[str, "PathItem"] | Reference] | None = None
    security: list[dict[str, list[str]]] | None = None
    servers: list[Server] | None = None
class PathItem(BaseModelWithConfig):
    get: Operation | None = None
    put: Operation | None = None
    post: Operation | None = None
    delete: Operation | None = None
    options: Operation | None = None
    head: Operation | None = None
    patch: Operation | None = None
    trace: Operation | None = None
class SecuritySchemeType(Enum):
    apiKey = "apiKey"
    http = "http"
    oauth2 = "oauth2"
    openIdConnect = "openIdConnect"
class SecurityBase(BaseModelWithConfig):
    type_: SecuritySchemeType = Field(alias="type")
class APIKeyIn(Enum):
class APIKey(SecurityBase):
    type_: SecuritySchemeType = Field(default=SecuritySchemeType.apiKey, alias="type")
    in_: APIKeyIn = Field(alias="in")
class HTTPBase(SecurityBase):
    type_: SecuritySchemeType = Field(default=SecuritySchemeType.http, alias="type")
    scheme: str
class HTTPBearer(HTTPBase):
    scheme: Literal["bearer"] = "bearer"
    bearerFormat: str | None = None
class OAuthFlow(BaseModelWithConfig):
    refreshUrl: str | None = None
    scopes: dict[str, str] = {}
class OAuthFlowImplicit(OAuthFlow):
    authorizationUrl: str
class OAuthFlowPassword(OAuthFlow):
    tokenUrl: str
class OAuthFlowClientCredentials(OAuthFlow):
class OAuthFlowAuthorizationCode(OAuthFlow):
class OAuthFlows(BaseModelWithConfig):
    implicit: OAuthFlowImplicit | None = None
    password: OAuthFlowPassword | None = None
    clientCredentials: OAuthFlowClientCredentials | None = None
    authorizationCode: OAuthFlowAuthorizationCode | None = None
class OAuth2(SecurityBase):
    type_: SecuritySchemeType = Field(default=SecuritySchemeType.oauth2, alias="type")
    flows: OAuthFlows
class OpenIdConnect(SecurityBase):
    type_: SecuritySchemeType = Field(
        default=SecuritySchemeType.openIdConnect, alias="type"
    openIdConnectUrl: str
SecurityScheme = APIKey | HTTPBase | OAuth2 | OpenIdConnect | HTTPBearer
class Components(BaseModelWithConfig):
    schemas: dict[str, Schema | Reference] | None = None
    responses: dict[str, Response | Reference] | None = None
    parameters: dict[str, Parameter | Reference] | None = None
    requestBodies: dict[str, RequestBody | Reference] | None = None
    securitySchemes: dict[str, SecurityScheme | Reference] | None = None
    callbacks: dict[str, dict[str, PathItem] | Reference | Any] | None = None
    pathItems: dict[str, PathItem | Reference] | None = None
class Tag(BaseModelWithConfig):
class OpenAPI(BaseModelWithConfig):
    openapi: str
    info: Info
    jsonSchemaDialect: str | None = None
    paths: dict[str, PathItem | Any] | None = None
    webhooks: dict[str, PathItem | Reference] | None = None
    components: Components | None = None
    tags: list[Tag] | None = None
Schema.model_rebuild()
Operation.model_rebuild()
Encoding.model_rebuild()
from django.contrib.admin.utils import quote
from django.contrib.contenttypes.models import ContentType
from django.db import models
from django.utils import timezone
from django.utils.text import get_text_list
ADDITION = 1
CHANGE = 2
DELETION = 3
ACTION_FLAG_CHOICES = [
    (ADDITION, _("Addition")),
    (CHANGE, _("Change")),
    (DELETION, _("Deletion")),
class LogEntryManager(models.Manager):
    use_in_migrations = True
    def log_actions(
        self, user_id, queryset, action_flag, change_message="", *, single_object=False
        if isinstance(change_message, list):
            change_message = json.dumps(change_message)
        log_entry_list = [
            self.model(
                content_type_id=ContentType.objects.get_for_model(
                    obj, for_concrete_model=False
                ).id,
                object_id=obj.pk,
                object_repr=str(obj)[:200],
                action_flag=action_flag,
                change_message=change_message,
            for obj in queryset
        if len(log_entry_list) == 1:
            instance = log_entry_list[0]
            instance.save()
            if single_object:
                return instance
            return [instance]
        return self.model.objects.bulk_create(log_entry_list)
class LogEntry(models.Model):
    action_time = models.DateTimeField(
        _("action time"),
        default=timezone.now,
        editable=False,
    user = models.ForeignKey(
        settings.AUTH_USER_MODEL,
        models.CASCADE,
        verbose_name=_("user"),
    content_type = models.ForeignKey(
        ContentType,
        models.SET_NULL,
        verbose_name=_("content type"),
        blank=True,
        null=True,
    object_id = models.TextField(_("object id"), blank=True, null=True)
    # Translators: 'repr' means representation
    # (https://docs.python.org/library/functions.html#repr)
    object_repr = models.CharField(_("object repr"), max_length=200)
    action_flag = models.PositiveSmallIntegerField(
        _("action flag"), choices=ACTION_FLAG_CHOICES
    # change_message is either a string or a JSON structure
    change_message = models.TextField(_("change message"), blank=True)
    objects = LogEntryManager()
    class Meta:
        verbose_name = _("log entry")
        verbose_name_plural = _("log entries")
        db_table = "django_admin_log"
        ordering = ["-action_time"]
        return str(self.action_time)
        if self.is_addition():
            return gettext("Added “%(object)s”.") % {"object": self.object_repr}
        elif self.is_change():
            return gettext("Changed “%(object)s” — %(changes)s") % {
                "object": self.object_repr,
                "changes": self.get_change_message(),
        elif self.is_deletion():
            return gettext("Deleted “%(object)s.”") % {"object": self.object_repr}
        return gettext("LogEntry Object")
    def is_addition(self):
        return self.action_flag == ADDITION
    def is_change(self):
        return self.action_flag == CHANGE
    def is_deletion(self):
        return self.action_flag == DELETION
    def get_change_message(self):
        If self.change_message is a JSON structure, interpret it as a change
        string, properly translated.
        if self.change_message and self.change_message[0] == "[":
                change_message = json.loads(self.change_message)
                return self.change_message
            messages = []
            for sub_message in change_message:
                if "added" in sub_message:
                    if sub_message["added"]:
                        sub_message["added"]["name"] = gettext(
                            sub_message["added"]["name"]
                        messages.append(
                            gettext("Added {name} “{object}”.").format(
                                **sub_message["added"]
                        messages.append(gettext("Added."))
                elif "changed" in sub_message:
                    sub_message["changed"]["fields"] = get_text_list(
                            gettext(field_name)
                            for field_name in sub_message["changed"]["fields"]
                        gettext("and"),
                    if "name" in sub_message["changed"]:
                        sub_message["changed"]["name"] = gettext(
                            sub_message["changed"]["name"]
                            gettext("Changed {fields} for {name} “{object}”.").format(
                                **sub_message["changed"]
                            gettext("Changed {fields}.").format(
                elif "deleted" in sub_message:
                    sub_message["deleted"]["name"] = gettext(
                        sub_message["deleted"]["name"]
                        gettext("Deleted {name} “{object}”.").format(
                            **sub_message["deleted"]
            change_message = " ".join(msg[0].upper() + msg[1:] for msg in messages)
            return change_message or gettext("No fields changed.")
    def get_edited_object(self):
        """Return the edited object represented by this log entry."""
        return self.content_type.get_object_for_this_type(pk=self.object_id)
    def get_admin_url(self):
        Return the admin URL to edit the object represented by this log entry.
        if self.content_type and self.object_id:
                self.content_type.app_label,
                self.content_type.model,
                return reverse(url_name, args=(quote(self.object_id),))
from collections.abc import Iterable
from django.contrib import auth
from django.contrib.auth.base_user import AbstractBaseUser, BaseUserManager
from django.contrib.auth.hashers import make_password
from django.core.exceptions import PermissionDenied
from django.core.mail import send_mail
from django.db.models.manager import EmptyManager
from .validators import UnicodeUsernameValidator
def update_last_login(sender, user, **kwargs):
    A signal receiver which updates the last_login date for
    the user logging in.
    user.last_login = timezone.now()
    user.save(update_fields=["last_login"])
class PermissionManager(models.Manager):
    def get_by_natural_key(self, codename, app_label, model):
        return self.get(
            codename=codename,
            content_type=ContentType.objects.db_manager(self.db).get_by_natural_key(
                app_label, model
class Permission(models.Model):
    The permissions system provides a way to assign permissions to specific
    users and groups of users.
    The permission system is used by the Django admin site, but may also be
    useful in your own code. The Django admin site uses permissions as follows:
        - The "add" permission limits the user's ability to view the "add" form
          and add an object.
        - The "change" permission limits a user's ability to view the change
          list, view the "change" form and change an object.
        - The "delete" permission limits the ability to delete an object.
        - The "view" permission limits the ability to view an object.
    Permissions are set globally per type of object, not per specific object
    instance. It is possible to say "Mary may change news stories," but it's
    not currently possible to say "Mary may change news stories, but only the
    ones she created herself" or "Mary may only change news stories that have a
    certain status or publication date."
    The permissions listed above are automatically created for each model.
    name = models.CharField(_("name"), max_length=255)
    codename = models.CharField(_("codename"), max_length=100)
    objects = PermissionManager()
        verbose_name = _("permission")
        verbose_name_plural = _("permissions")
        unique_together = [["content_type", "codename"]]
        ordering = ["content_type__app_label", "content_type__model", "codename"]
        return "%s | %s" % (self.content_type, self.name)
    def natural_key(self):
        return (self.codename, *self.content_type.natural_key())
    natural_key.dependencies = ["contenttypes.contenttype"]
class GroupManager(models.Manager):
    The manager for the auth's Group model.
    def get_by_natural_key(self, name):
        return self.get(name=name)
    async def aget_by_natural_key(self, name):
        return await self.aget(name=name)
class Group(models.Model):
    Groups are a generic way of categorizing users to apply permissions, or
    some other label, to those users. A user can belong to any number of
    A user in a group automatically has all the permissions granted to that
    group. For example, if the group 'Site editors' has the permission
    can_edit_home_page, any user in that group will have that permission.
    Beyond permissions, groups are a convenient way to categorize users to
    apply some label, or extended functionality, to them. For example, you
    could create a group 'Special users', and you could write code that would
    do special things to those users -- such as giving them access to a
    members-only portion of your site, or sending them members-only email
    name = models.CharField(_("name"), max_length=150, unique=True)
    permissions = models.ManyToManyField(
        Permission,
        verbose_name=_("permissions"),
    objects = GroupManager()
        verbose_name = _("group")
        verbose_name_plural = _("groups")
        return (self.name,)
class UserManager(BaseUserManager):
    def _create_user_object(self, username, email, password, **extra_fields):
            raise ValueError("The given username must be set")
        email = self.normalize_email(email)
        # Lookup the real model class from the global app registry so this
        # manager method can be used in migrations. This is fine because
        # managers are by definition working on the real model.
        GlobalUserModel = apps.get_model(
            self.model._meta.app_label, self.model._meta.object_name
        username = GlobalUserModel.normalize_username(username)
        user = self.model(username=username, email=email, **extra_fields)
        user.password = make_password(password)
    def _create_user(self, username, email, password, **extra_fields):
        Create and save a user with the given username, email, and password.
        user = self._create_user_object(username, email, password, **extra_fields)
        user.save(using=self._db)
    async def _acreate_user(self, username, email, password, **extra_fields):
        """See _create_user()"""
        await user.asave(using=self._db)
    def create_user(self, username, email=None, password=None, **extra_fields):
        extra_fields.setdefault("is_staff", False)
        extra_fields.setdefault("is_superuser", False)
        return self._create_user(username, email, password, **extra_fields)
    create_user.alters_data = True
    async def acreate_user(self, username, email=None, password=None, **extra_fields):
        return await self._acreate_user(username, email, password, **extra_fields)
    acreate_user.alters_data = True
    def create_superuser(self, username, email=None, password=None, **extra_fields):
        extra_fields.setdefault("is_staff", True)
        extra_fields.setdefault("is_superuser", True)
        if extra_fields.get("is_staff") is not True:
            raise ValueError("Superuser must have is_staff=True.")
        if extra_fields.get("is_superuser") is not True:
            raise ValueError("Superuser must have is_superuser=True.")
    create_superuser.alters_data = True
    async def acreate_superuser(
        self, username, email=None, password=None, **extra_fields
    acreate_superuser.alters_data = True
    def with_perm(
        self, perm, is_active=True, include_superusers=True, backend=None, obj=None
        if backend is None:
            backends = auth.get_backends()
                backend = backends[0]
                    "therefore must provide the `backend` argument."
        elif not isinstance(backend, str):
            backend = auth.load_backend(backend)
        if hasattr(backend, "with_perm"):
            return backend.with_perm(
                perm,
                is_active=is_active,
                include_superusers=include_superusers,
                obj=obj,
        return self.none()
# A few helper functions for common logic between User and AnonymousUser.
def _user_get_permissions(user, obj, from_name):
    permissions = set()
    name = "get_%s_permissions" % from_name
    for backend in auth.get_backends():
        if hasattr(backend, name):
            permissions.update(getattr(backend, name)(user, obj))
    return permissions
async def _auser_get_permissions(user, obj, from_name):
    name = "aget_%s_permissions" % from_name
            permissions.update(await getattr(backend, name)(user, obj))
def _user_has_perm(user, perm, obj):
    A backend can raise `PermissionDenied` to short-circuit permission checks.
        if not hasattr(backend, "has_perm"):
            if backend.has_perm(user, perm, obj):
async def _auser_has_perm(user, perm, obj):
    """See _user_has_perm()"""
        if not hasattr(backend, "ahas_perm"):
            if await backend.ahas_perm(user, perm, obj):
def _user_has_module_perms(user, app_label):
        if not hasattr(backend, "has_module_perms"):
            if backend.has_module_perms(user, app_label):
async def _auser_has_module_perms(user, app_label):
    """See _user_has_module_perms()"""
        if not hasattr(backend, "ahas_module_perms"):
            if await backend.ahas_module_perms(user, app_label):
class PermissionsMixin(models.Model):
    Add the fields and methods necessary to support the Group and Permission
    models using the ModelBackend.
    is_superuser = models.BooleanField(
        _("superuser status"),
        help_text=_(
            "Designates that this user has all permissions without "
            "explicitly assigning them."
    groups = models.ManyToManyField(
        Group,
        verbose_name=_("groups"),
            "The groups this user belongs to. A user will get all permissions "
            "granted to each of their groups."
        related_name="user_set",
        related_query_name="user",
    user_permissions = models.ManyToManyField(
        verbose_name=_("user permissions"),
        help_text=_("Specific permissions for this user."),
        abstract = True
    def get_user_permissions(self, obj=None):
        Return a list of permission strings that this user has directly.
        Query all available auth backends. If an object is passed in,
        return only permissions matching this object.
        return _user_get_permissions(self, obj, "user")
    async def aget_user_permissions(self, obj=None):
        """See get_user_permissions()"""
        return await _auser_get_permissions(self, obj, "user")
    def get_group_permissions(self, obj=None):
        Return a list of permission strings that this user has through their
        groups. Query all available auth backends. If an object is passed in,
        return _user_get_permissions(self, obj, "group")
    async def aget_group_permissions(self, obj=None):
        """See get_group_permissions()"""
        return await _auser_get_permissions(self, obj, "group")
    def get_all_permissions(self, obj=None):
        return _user_get_permissions(self, obj, "all")
    async def aget_all_permissions(self, obj=None):
        return await _auser_get_permissions(self, obj, "all")
    def has_perm(self, perm, obj=None):
        Return True if the user has the specified permission. Query all
        available auth backends, but return immediately if any backend returns
        True. Thus, a user who has permission from a single auth backend is
        assumed to have permission in general. If an object is provided, check
        permissions for that object.
        # Active superusers have all permissions.
        if self.is_active and self.is_superuser:
        # Otherwise we need to check the backends.
        return _user_has_perm(self, perm, obj)
    async def ahas_perm(self, perm, obj=None):
        """See has_perm()"""
        return await _auser_has_perm(self, perm, obj)
    def has_perms(self, perm_list, obj=None):
        Return True if the user has each of the specified permissions. If
        object is passed, check if the user has all required perms for it.
        if not isinstance(perm_list, Iterable) or isinstance(perm_list, str):
            raise ValueError("perm_list must be an iterable of permissions.")
        return all(self.has_perm(perm, obj) for perm in perm_list)
    async def ahas_perms(self, perm_list, obj=None):
        """See has_perms()"""
        for perm in perm_list:
            if not await self.ahas_perm(perm, obj):
    def has_module_perms(self, app_label):
        Return True if the user has any permissions in the given app label.
        Use similar logic as has_perm(), above.
        return _user_has_module_perms(self, app_label)
    async def ahas_module_perms(self, app_label):
        """See has_module_perms()"""
        return await _auser_has_module_perms(self, app_label)
class AbstractUser(AbstractBaseUser, PermissionsMixin):
    An abstract base class implementing a fully featured User model with
    admin-compliant permissions.
    Username and password are required. Other fields are optional.
    username_validator = UnicodeUsernameValidator()
    username = models.CharField(
        _("username"),
        max_length=150,
        unique=True,
            "Required. 150 characters or fewer. Letters, digits and @/./+/-/_ only."
        validators=[username_validator],
        error_messages={
            "unique": _("A user with that username already exists."),
    first_name = models.CharField(_("first name"), max_length=150, blank=True)
    last_name = models.CharField(_("last name"), max_length=150, blank=True)
    email = models.EmailField(_("email address"), blank=True)
    is_staff = models.BooleanField(
        _("staff status"),
        help_text=_("Designates whether the user can log into this admin site."),
    is_active = models.BooleanField(
        _("active"),
            "Designates whether this user should be treated as active. "
            "Unselect this instead of deleting accounts."
    date_joined = models.DateTimeField(_("date joined"), default=timezone.now)
    objects = UserManager()
    EMAIL_FIELD = "email"
    USERNAME_FIELD = "username"
    REQUIRED_FIELDS = ["email"]
        verbose_name = _("user")
        verbose_name_plural = _("users")
        super().clean()
        self.email = self.__class__.objects.normalize_email(self.email)
    def get_full_name(self):
        Return the first_name plus the last_name, with a space in between.
        full_name = "%s %s" % (self.first_name, self.last_name)
        return full_name.strip()
    def get_short_name(self):
        """Return the short name for the user."""
        return self.first_name
    def email_user(self, subject, message, from_email=None, **kwargs):
        """Send an email to this user."""
        send_mail(subject, message, from_email, [self.email], **kwargs)
class User(AbstractUser):
    Users within the Django authentication system are represented by this
    class Meta(AbstractUser.Meta):
        swappable = "AUTH_USER_MODEL"
class AnonymousUser:
    pk = None
    username = ""
    is_staff = False
    is_active = False
    is_superuser = False
    _groups = EmptyManager(Group)
    _user_permissions = EmptyManager(Permission)
        return "AnonymousUser"
        return isinstance(other, self.__class__)
        return 1  # instances always return the same hash value
    def __int__(self):
            "Cannot cast AnonymousUser to int. Are you trying to use it in place of "
            "User?"
    def save(self):
            "Django doesn't provide a DB representation for AnonymousUser."
    def set_password(self, raw_password):
    def check_password(self, raw_password):
    def groups(self):
        return self._groups
    def user_permissions(self):
        return self._user_permissions
        return set()
        return self.get_group_permissions(obj)
        return _user_has_perm(self, perm, obj=obj)
        return await _auser_has_perm(self, perm, obj=obj)
    def has_module_perms(self, module):
        return _user_has_module_perms(self, module)
    async def ahas_module_perms(self, module):
        return await _auser_has_module_perms(self, module)
    def is_authenticated(self):
    def get_username(self):
from django.db.models import Q
class ContentTypeManager(models.Manager):
        # Cache shared by all the get_for_* methods to speed up
        # ContentType retrieval.
    def get_by_natural_key(self, app_label, model):
            ct = self._cache[self.db][(app_label, model)]
            ct = self.get(app_label=app_label, model=model)
            self._add_to_cache(self.db, ct)
        return ct
    def _get_opts(self, model, for_concrete_model):
        if for_concrete_model:
            model = model._meta.concrete_model
        return model._meta
    def _get_from_cache(self, opts):
        key = (opts.app_label, opts.model_name)
        return self._cache[self.db][key]
    def get_for_model(self, model, for_concrete_model=True):
        Return the ContentType object for a given model, creating the
        ContentType if necessary. Lookups are cached so that subsequent lookups
        for the same model don't hit the database.
        opts = self._get_opts(model, for_concrete_model)
            return self._get_from_cache(opts)
        # The ContentType entry was not found in the cache, therefore we
        # proceed to load or create it.
            # Start with get() and not get_or_create() in order to use
            # the db_for_read (see #20401).
            ct = self.get(app_label=opts.app_label, model=opts.model_name)
        except self.model.DoesNotExist:
            # Not found in the database; we proceed to create it. This time
            # use get_or_create to take care of any race conditions.
            ct, created = self.get_or_create(
                app_label=opts.app_label,
                model=opts.model_name,
    def get_for_models(self, *models, for_concrete_models=True):
        Given *models, return a dictionary mapping {model: content_type}.
        results = {}
        # Models that aren't already in the cache grouped by app labels.
        needed_models = defaultdict(set)
        # Mapping of opts to the list of models requiring it.
        needed_opts = defaultdict(list)
            opts = self._get_opts(model, for_concrete_models)
                ct = self._get_from_cache(opts)
                needed_models[opts.app_label].add(opts.model_name)
                needed_opts[(opts.app_label, opts.model_name)].append(model)
                results[model] = ct
        if needed_opts:
            # Lookup required content types from the DB.
            condition = Q(
                *(
                    Q(("app_label", app_label), ("model__in", models))
                    for app_label, models in needed_models.items()
                _connector=Q.OR,
            cts = self.filter(condition)
            for ct in cts:
                opts_models = needed_opts.pop((ct.app_label, ct.model), [])
                for model in opts_models:
        # Create content types that weren't in the cache or DB.
        for (app_label, model_name), opts_models in needed_opts.items():
            ct = self.create(app_label=app_label, model=model_name)
    def get_for_id(self, id):
        Lookup a ContentType by ID. Use the same shared cache as get_for_model
        (though ContentTypes are not created on-the-fly by get_by_id).
            ct = self._cache[self.db][id]
            # This could raise a DoesNotExist; that's correct behavior and will
            # make sure that only correct ctypes get stored in the cache dict.
            ct = self.get(pk=id)
    def clear_cache(self):
        Clear out the content-type cache.
        self._cache.clear()
    def _add_to_cache(self, using, ct):
        """Insert a ContentType into the cache."""
        # Note it's possible for ContentType objects to be stale; model_class()
        # will return None. Hence, there is no reliance on
        # model._meta.app_label here, just using the model fields instead.
        key = (ct.app_label, ct.model)
        self._cache.setdefault(using, {})[key] = ct
        self._cache.setdefault(using, {})[ct.id] = ct
class ContentType(models.Model):
    app_label = models.CharField(max_length=100)
    model = models.CharField(_("python model class name"), max_length=100)
    objects = ContentTypeManager()
        verbose_name = _("content type")
        verbose_name_plural = _("content types")
        db_table = "django_content_type"
        unique_together = [["app_label", "model"]]
        return self.app_labeled_name
        model = self.model_class()
            return self.model
        return str(model._meta.verbose_name)
    def app_labeled_name(self):
            return "%s | %s" % (self.app_label, self.model)
        return "%s | %s" % (
            model._meta.app_config.verbose_name,
            model._meta.verbose_name,
    def model_class(self):
        """Return the model class for this type of content."""
            return apps.get_model(self.app_label, self.model)
    def get_object_for_this_type(self, using=None, **kwargs):
        Return an object of this type for the keyword arguments given.
        Basically, this is a proxy around this object_type's get_object() model
        method. The ObjectNotExist exception, if thrown, will not be caught,
        so code that calls this method should catch it.
        return self.model_class()._base_manager.using(using).get(**kwargs)
    def get_all_objects_for_this_type(self, **kwargs):
        Return all objects of this type for the keyword arguments given.
        return self.model_class()._base_manager.filter(**kwargs)
        return (self.app_label, self.model)
from django.contrib.sites.models import Site
from django.urls import NoReverseMatch, get_script_prefix, reverse
from django.utils.encoding import iri_to_uri
class FlatPage(models.Model):
    url = models.CharField(_("URL"), max_length=100, db_index=True)
    title = models.CharField(_("title"), max_length=200)
    content = models.TextField(_("content"), blank=True)
    enable_comments = models.BooleanField(_("enable comments"), default=False)
    template_name = models.CharField(
        _("template name"),
        max_length=70,
            "Example: “flatpages/contact_page.html”. If this isn’t provided, "
            "the system will use “flatpages/default.html”."
    registration_required = models.BooleanField(
        _("registration required"),
            "If this is checked, only logged-in users will be able to view the page."
    sites = models.ManyToManyField(Site, verbose_name=_("sites"))
        db_table = "django_flatpage"
        verbose_name = _("flat page")
        verbose_name_plural = _("flat pages")
        ordering = ["url"]
        return "%s -- %s" % (self.url, self.title)
    def get_absolute_url(self):
        from .views import flatpage
        for url in (self.url.lstrip("/"), self.url):
                return reverse(flatpage, kwargs={"url": url})
        # Handle script prefix manually because we bypass reverse()
        return iri_to_uri(get_script_prefix().rstrip("/") + self.url)
from django.contrib.gis import gdal
class SpatialRefSysMixin:
    The SpatialRefSysMixin is a class used by the database-dependent
    SpatialRefSys objects to reduce redundant code.
    def srs(self):
        Return a GDAL SpatialReference object.
            return gdal.SpatialReference(self.wkt)
            wkt_error = e
            return gdal.SpatialReference(self.proj4text)
            proj4_error = e
            "Could not get OSR SpatialReference.\n"
            f"Error for WKT '{self.wkt}': {wkt_error}\n"
            f"Error for PROJ.4 '{self.proj4text}': {proj4_error}"
    def ellipsoid(self):
        Return a tuple of the ellipsoid parameters:
        (semimajor axis, semiminor axis, and inverse flattening).
        return self.srs.ellipsoid
        "Return the projection name."
        return self.srs.name
    def spheroid(self):
        "Return the spheroid name for this spatial reference."
        return self.srs["spheroid"]
    def datum(self):
        "Return the datum for this spatial reference."
        return self.srs["datum"]
    def projected(self):
        "Is this Spatial Reference projected?"
        return self.srs.projected
    def local(self):
        "Is this Spatial Reference local?"
        return self.srs.local
    def geographic(self):
        "Is this Spatial Reference geographic?"
        return self.srs.geographic
    def linear_name(self):
        "Return the linear units name."
        return self.srs.linear_name
    def linear_units(self):
        "Return the linear units."
        return self.srs.linear_units
    def angular_name(self):
        "Return the name of the angular units."
        return self.srs.angular_name
    def angular_units(self):
        "Return the angular units."
        return self.srs.angular_units
    def units(self):
        "Return a tuple of the units and the name."
        if self.projected or self.local:
            return (self.linear_units, self.linear_name)
        elif self.geographic:
            return (self.angular_units, self.angular_name)
    def get_units(cls, wkt):
        Return a tuple of (unit_value, unit_name) for the given WKT without
        using any of the database fields.
        return gdal.SpatialReference(wkt).units
    def get_spheroid(cls, wkt, string=True):
        Class method used by GeometryField on initialization to
        retrieve the `SPHEROID[..]` parameters from the given WKT.
        srs = gdal.SpatialReference(wkt)
        sphere_params = srs.ellipsoid
        sphere_name = srs["spheroid"]
        if not string:
            return sphere_name, sphere_params
            # `string` parameter used to place in format acceptable by PostGIS
            if len(sphere_params) == 3:
                radius, flattening = sphere_params[0], sphere_params[2]
                radius, flattening = sphere_params
            return 'SPHEROID["%s",%s,%s]' % (sphere_name, radius, flattening)
        Return the string representation, a 'pretty' OGC WKT.
        return str(self.srs)
The GeometryColumns and SpatialRefSys models for the Oracle spatial
backend.
It should be noted that Oracle Spatial does not have database tables
named according to the OGC standard, so the closest analogs are used.
For example, the `USER_SDO_GEOM_METADATA` is used for the GeometryColumns
model and the `SDO_COORD_REF_SYS` is used for the SpatialRefSys model.
from django.contrib.gis.db.backends.base.models import SpatialRefSysMixin
class OracleGeometryColumns(models.Model):
    "Maps to the Oracle USER_SDO_GEOM_METADATA table."
    table_name = models.CharField(max_length=32)
    column_name = models.CharField(max_length=1024)
    srid = models.IntegerField(primary_key=True)
    # TODO: Add support for `diminfo` column (type MDSYS.SDO_DIM_ARRAY).
        app_label = "gis"
        db_table = "USER_SDO_GEOM_METADATA"
        managed = False
        return "%s - %s (SRID: %s)" % (self.table_name, self.column_name, self.srid)
    def table_name_col(cls):
        Return the name of the metadata column used to store the feature table
        return "table_name"
    def geom_col_name(cls):
        Return the name of the metadata column used to store the feature
        geometry column.
        return "column_name"
class OracleSpatialRefSys(models.Model, SpatialRefSysMixin):
    "Maps to the Oracle MDSYS.CS_SRS table."
    cs_name = models.CharField(max_length=68)
    auth_srid = models.IntegerField()
    auth_name = models.CharField(max_length=256)
    wktext = models.CharField(max_length=2046)
    # Optional geometry representing the bounds of this coordinate
    # system. By default, all are NULL in the table.
    cs_bounds = models.PolygonField(null=True)
        db_table = "CS_SRS"
    def wkt(self):
        return self.wktext
The GeometryColumns and SpatialRefSys models for the PostGIS backend.
class PostGISGeometryColumns(models.Model):
    The 'geometry_columns' view from PostGIS. See the PostGIS
    documentation at Ch. 4.3.2.
    f_table_catalog = models.CharField(max_length=256)
    f_table_schema = models.CharField(max_length=256)
    f_table_name = models.CharField(max_length=256)
    f_geometry_column = models.CharField(max_length=256)
    coord_dimension = models.IntegerField()
    type = models.CharField(max_length=30)
        db_table = "geometry_columns"
        return "%s.%s - %dD %s field (SRID: %d)" % (
            self.f_table_name,
            self.f_geometry_column,
            self.coord_dimension,
            self.type,
            self.srid,
        return "f_table_name"
        return "f_geometry_column"
class PostGISSpatialRefSys(models.Model, SpatialRefSysMixin):
    The 'spatial_ref_sys' table from PostGIS. See the PostGIS
    documentation at Ch. 4.2.1.
    srtext = models.CharField(max_length=2048)
    proj4text = models.CharField(max_length=2048)
        db_table = "spatial_ref_sys"
        return self.srtext
The GeometryColumns and SpatialRefSys models for the SpatiaLite backend.
class SpatialiteGeometryColumns(models.Model):
    The 'geometry_columns' table from SpatiaLite.
    spatial_index_enabled = models.IntegerField()
    type = models.IntegerField(db_column="geometry_type")
class SpatialiteSpatialRefSys(models.Model, SpatialRefSysMixin):
    The 'spatial_ref_sys' table from SpatiaLite.
    ref_sys_name = models.CharField(max_length=256)
class Redirect(models.Model):
    site = models.ForeignKey(Site, models.CASCADE, verbose_name=_("site"))
    old_path = models.CharField(
        _("redirect from"),
        max_length=200,
        db_index=True,
            "This should be an absolute path, excluding the domain name. Example: "
            "“/events/search/”."
    new_path = models.CharField(
        _("redirect to"),
            "This can be either an absolute path (as above) or a full URL "
            "starting with a scheme such as “https://”."
        verbose_name = _("redirect")
        verbose_name_plural = _("redirects")
        db_table = "django_redirect"
        unique_together = [["site", "old_path"]]
        ordering = ["old_path"]
        return "%s ---> %s" % (self.old_path, self.new_path)
from django.contrib.sessions.base_session import AbstractBaseSession, BaseSessionManager
class SessionManager(BaseSessionManager):
class Session(AbstractBaseSession):
    Django provides full support for anonymous sessions. The session
    framework lets you store and retrieve arbitrary data on a
    per-site-visitor basis. It stores data on the server side and
    abstracts the sending and receiving of cookies. Cookies contain a
    session ID -- not the data itself.
    The Django sessions framework is entirely cookie-based. It does
    not fall back to putting session IDs in URLs. This is an intentional
    design decision. Not only does that behavior make URLs ugly, it makes
    your site vulnerable to session-ID theft via the "Referer" header.
    For complete documentation on using Sessions in your code, consult
    the sessions documentation that is shipped with Django (also available
    on the Django web site).
    objects = SessionManager()
    def get_session_store_class(cls):
        from django.contrib.sessions.backends.db import SessionStore
        return SessionStore
    class Meta(AbstractBaseSession.Meta):
        db_table = "django_session"
from django.core.exceptions import ImproperlyConfigured, ValidationError
from django.db.models.signals import pre_delete, pre_save
from django.http.request import split_domain_port
SITE_CACHE = {}
def _simple_domain_name_validator(value):
    Validate that the given value contains no whitespaces to prevent common
    typos.
    checks = ((s in value) for s in string.whitespace)
    if any(checks):
            _("The domain name cannot contain any spaces or tabs."),
            code="invalid",
class SiteManager(models.Manager):
    def _get_site_by_id(self, site_id):
        if site_id not in SITE_CACHE:
            site = self.get(pk=site_id)
            SITE_CACHE[site_id] = site
        return SITE_CACHE[site_id]
    def _get_site_by_request(self, request):
        host = request.get_host()
            # First attempt to look up the site by host with or without port.
            if host not in SITE_CACHE:
                SITE_CACHE[host] = self.get(domain__iexact=host)
            return SITE_CACHE[host]
            # Fallback to looking up site after stripping port from the host.
            domain, port = split_domain_port(host)
            if domain not in SITE_CACHE:
                SITE_CACHE[domain] = self.get(domain__iexact=domain)
            return SITE_CACHE[domain]
    def get_current(self, request=None):
        Return the current Site based on the SITE_ID in the project's settings.
        If SITE_ID isn't defined, return the site with domain matching
        request.get_host(). The ``Site`` object is cached the first time it's
        retrieved from the database.
        if getattr(settings, "SITE_ID", ""):
            site_id = settings.SITE_ID
            return self._get_site_by_id(site_id)
        elif request:
            return self._get_site_by_request(request)
            'You\'re using the Django "sites framework" without having '
            "set the SITE_ID setting. Create a site in your database and "
            "set the SITE_ID setting or pass a request to "
            "Site.objects.get_current() to fix this error."
        """Clear the ``Site`` object cache."""
        global SITE_CACHE
    def get_by_natural_key(self, domain):
        return self.get(domain=domain)
class Site(models.Model):
    domain = models.CharField(
        _("domain name"),
        max_length=100,
        validators=[_simple_domain_name_validator],
    name = models.CharField(_("display name"), max_length=50)
    objects = SiteManager()
        db_table = "django_site"
        verbose_name = _("site")
        verbose_name_plural = _("sites")
        ordering = ["domain"]
        return self.domain
        return (self.domain,)
def clear_site_cache(sender, **kwargs):
    Clear the cache (if primed) each time a site is saved or deleted.
    instance = kwargs["instance"]
    using = kwargs["using"]
        del SITE_CACHE[instance.pk]
        del SITE_CACHE[Site.objects.using(using).get(pk=instance.pk).domain]
    except (KeyError, Site.DoesNotExist):
pre_save.connect(clear_site_cache, sender=Site)
pre_delete.connect(clear_site_cache, sender=Site)
from django.db.migrations.operations.base import Operation, OperationCategory
from django.db.migrations.state import ModelState
from django.db.migrations.utils import field_references, resolve_relation
from django.db.models.options import normalize_together
from django.utils.copy import replace
from .fields import AddField, AlterField, FieldOperation, RemoveField, RenameField
def _check_for_duplicates(arg_name, objs):
    used_vals = set()
    for val in objs:
        if val in used_vals:
                "Found duplicate value %s in CreateModel %s argument." % (val, arg_name)
        used_vals.add(val)
class ModelOperation(Operation):
    def __init__(self, name):
    def name_lower(self):
        return self.name.lower()
        return name.lower() == self.name_lower
        return super().reduce(operation, app_label) or self.can_reduce_through(
            operation, app_label
    def can_reduce_through(self, operation, app_label):
        return not operation.references_model(self.name, app_label)
class CreateModel(ModelOperation):
    """Create a model's table."""
    category = OperationCategory.ADDITION
    serialization_expand_args = ["fields", "options", "managers"]
    def __init__(self, name, fields, options=None, bases=None, managers=None):
        self.options = options or {}
        self.bases = bases or (models.Model,)
        self.managers = managers or []
        super().__init__(name)
        # Sanity-check that there are no duplicated field names, bases, or
        # manager names
        _check_for_duplicates("fields", (name for name, _ in self.fields))
        _check_for_duplicates(
            "bases",
                    base._meta.label_lower
                    if hasattr(base, "_meta")
                    else base.lower() if isinstance(base, str) else base
                for base in self.bases
        _check_for_duplicates("managers", (name for name, _ in self.managers))
            "name": self.name,
            "fields": self.fields,
        if self.options:
            kwargs["options"] = self.options
        if self.bases and self.bases != (models.Model,):
            kwargs["bases"] = self.bases
        if self.managers and self.managers != [("objects", models.Manager())]:
            kwargs["managers"] = self.managers
        return (self.__class__.__qualname__, [], kwargs)
        state.add_model(
            ModelState(
                app_label,
                list(self.fields),
                dict(self.options),
                tuple(self.bases),
                list(self.managers),
        model = to_state.apps.get_model(app_label, self.name)
        if self.allow_migrate_model(schema_editor.connection.alias, model):
            schema_editor.create_model(model)
            # While the `index_together` option has been deprecated some
            # historical migrations might still have references to them.
            # This can be moved to the schema editor once it's adapted to
            # from model states instead of rendered models (#29898).
            to_model_state = to_state.models[app_label, self.name_lower]
            if index_together := to_model_state.options.get("index_together"):
                schema_editor.alter_index_together(
                    set(),
                    index_together,
        model = from_state.apps.get_model(app_label, self.name)
            schema_editor.delete_model(model)
        return "Create %smodel %s" % (
            "proxy " if self.options.get("proxy", False) else "",
        return self.name_lower
        name_lower = name.lower()
        if name_lower == self.name_lower:
        # Check we didn't inherit from the model
        reference_model_tuple = (app_label, name_lower)
        for base in self.bases:
                base is not models.Model
                and isinstance(base, (models.base.ModelBase, str))
                and resolve_relation(base, app_label) == reference_model_tuple
        # Check we have no FKs/M2Ms with it
        for _name, field in self.fields:
            if field_references(
                (app_label, self.name_lower), field, reference_model_tuple
            isinstance(operation, DeleteModel)
            and self.name_lower == operation.name_lower
            and not self.options.get("proxy", False)
            isinstance(operation, RenameModel)
            and self.name_lower == operation.old_name_lower
            return [replace(self, name=operation.new_name)]
            isinstance(operation, AlterModelOptions)
            options = {**self.options, **operation.options}
            for key in operation.ALTER_OPTION_KEYS:
                if key not in operation.options:
                    options.pop(key, None)
            return [replace(self, options=options)]
            isinstance(operation, AlterModelManagers)
            return [replace(self, managers=operation.managers)]
            isinstance(operation, AlterModelTable)
                replace(
                    options={**self.options, "db_table": operation.table},
            isinstance(operation, AlterModelTableComment)
                    options={
                        **self.options,
                        "db_table_comment": operation.table_comment,
            isinstance(operation, AlterTogetherOptionOperation)
                        **{operation.option_name: operation.option_value},
            isinstance(operation, AlterOrderWithRespectTo)
                        "order_with_respect_to": operation.order_with_respect_to,
            isinstance(operation, FieldOperation)
            and self.name_lower == operation.model_name_lower
            if isinstance(operation, AddField):
                        fields=[*self.fields, (operation.name, operation.field)],
            elif isinstance(operation, AlterField):
                        fields=[
                            (n, operation.field if n == operation.name else v)
                            for n, v in self.fields
            elif isinstance(operation, RemoveField):
                options = self.options.copy()
                for option_name in ("unique_together", "index_together"):
                    option = options.pop(option_name, None)
                    if option:
                        option = set(
                            filter(
                                    tuple(
                                        f for f in fields if f != operation.name_lower
                                    for fields in option
                            options[option_name] = option
                order_with_respect_to = options.get("order_with_respect_to")
                if order_with_respect_to == operation.name_lower:
                    del options["order_with_respect_to"]
                            (n, v)
                            if n.lower() != operation.name_lower
            elif isinstance(operation, RenameField):
                    option = options.get(option_name)
                        options[option_name] = {
                                operation.new_name if f == operation.old_name else f
                                for f in fields
                if order_with_respect_to == operation.old_name:
                    options["order_with_respect_to"] = operation.new_name
                            (operation.new_name if n == operation.old_name else n, v)
            isinstance(operation, IndexOperation)
            if isinstance(operation, AddIndex):
                            "indexes": [
                                *self.options.get("indexes", []),
                                operation.index,
            elif isinstance(operation, RemoveIndex):
                options_indexes = [
                    for index in self.options.get("indexes", [])
                    if index.name != operation.name
                            "indexes": options_indexes,
            elif isinstance(operation, AddConstraint):
                            "constraints": [
                                *self.options.get("constraints", []),
                                operation.constraint,
            elif isinstance(operation, AlterConstraint):
                options_constraints = [
                    for constraint in self.options.get("constraints", [])
                    if constraint.name != operation.name
                ] + [operation.constraint]
                            "constraints": options_constraints,
            elif isinstance(operation, RemoveConstraint):
        return super().reduce(operation, app_label)
class DeleteModel(ModelOperation):
    """Drop a model's table."""
    category = OperationCategory.REMOVAL
        state.remove_model(app_label, self.name_lower)
        # The deleted model could be referencing the specified model through
        # related fields.
        return "Delete model %s" % self.name
        return "delete_%s" % self.name_lower
class RenameModel(ModelOperation):
    """Rename a model."""
    category = OperationCategory.ALTERATION
    def __init__(self, old_name, new_name):
        self.old_name = old_name
        self.new_name = new_name
        super().__init__(old_name)
    def old_name_lower(self):
        return self.old_name.lower()
    def new_name_lower(self):
        return self.new_name.lower()
            "old_name": self.old_name,
            "new_name": self.new_name,
        state.rename_model(app_label, self.old_name, self.new_name)
        new_model = to_state.apps.get_model(app_label, self.new_name)
        if self.allow_migrate_model(schema_editor.connection.alias, new_model):
            old_model = from_state.apps.get_model(app_label, self.old_name)
            # Move the main table
            schema_editor.alter_db_table(
                old_model._meta.db_table,
            # Alter the fields pointing to us
            for related_object in old_model._meta.related_objects:
                if related_object.related_model == old_model:
                    model = new_model
                    related_key = (app_label, self.new_name_lower)
                    related_key = (
                        related_object.related_model._meta.app_label,
                        related_object.related_model._meta.model_name,
                    model = to_state.apps.get_model(*related_key)
                to_field = to_state.apps.get_model(*related_key)._meta.get_field(
                    related_object.field.name
                schema_editor.alter_field(
                    related_object.field,
                    to_field,
            # Rename M2M fields whose name is based on this model's name.
            fields = zip(
                old_model._meta.local_many_to_many, new_model._meta.local_many_to_many
            for old_field, new_field in fields:
                # Skip self-referential fields as these are renamed above.
                    new_field.model == new_field.related_model
                    or not new_field.remote_field.through._meta.auto_created
                # Rename columns and the M2M table.
                schema_editor._alter_many_to_many(
        self.new_name_lower, self.old_name_lower = (
            self.old_name_lower,
            self.new_name_lower,
        self.new_name, self.old_name = self.old_name, self.new_name
        self.database_forwards(app_label, schema_editor, from_state, to_state)
            name.lower() == self.old_name_lower or name.lower() == self.new_name_lower
        return "Rename model %s to %s" % (self.old_name, self.new_name)
        return "rename_%s_%s" % (self.old_name_lower, self.new_name_lower)
            and self.new_name_lower == operation.old_name_lower
            return [replace(self, new_name=operation.new_name)]
        # Skip `ModelOperation.reduce` as we want to run `references_model`
        # against self.new_name.
        return super(ModelOperation, self).reduce(
        ) or not operation.references_model(self.new_name, app_label)
class ModelOptionOperation(ModelOperation):
            isinstance(operation, (self.__class__, DeleteModel))
class AlterModelTable(ModelOptionOperation):
    """Rename a model's table."""
    def __init__(self, name, table):
        self.table = table
            "table": self.table,
        state.alter_model_options(app_label, self.name_lower, {"db_table": self.table})
        new_model = to_state.apps.get_model(app_label, self.name)
            old_model = from_state.apps.get_model(app_label, self.name)
            # Rename M2M fields whose name is based on this model's db_table
            for old_field, new_field in zip(
                if new_field.remote_field.through._meta.auto_created:
        return self.database_forwards(app_label, schema_editor, from_state, to_state)
        return "Rename table for %s to %s" % (
            self.table if self.table is not None else "(default)",
        return "alter_%s_table" % self.name_lower
class AlterModelTableComment(ModelOptionOperation):
    def __init__(self, name, table_comment):
        self.table_comment = table_comment
            "table_comment": self.table_comment,
        state.alter_model_options(
            app_label, self.name_lower, {"db_table_comment": self.table_comment}
            schema_editor.alter_db_table_comment(
                old_model._meta.db_table_comment,
                new_model._meta.db_table_comment,
        return f"Alter {self.name} table comment"
        return f"alter_{self.name_lower}_table_comment"
class AlterTogetherOptionOperation(ModelOptionOperation):
    option_name = None
    def __init__(self, name, option_value):
        if option_value:
            option_value = set(normalize_together(option_value))
        setattr(self, self.option_name, option_value)
    def option_value(self):
        return getattr(self, self.option_name)
            self.option_name: self.option_value,
            self.name_lower,
            {self.option_name: self.option_value},
            from_model_state = from_state.models[app_label, self.name_lower]
            alter_together = getattr(schema_editor, "alter_%s" % self.option_name)
            alter_together(
                from_model_state.options.get(self.option_name) or set(),
                to_model_state.options.get(self.option_name) or set(),
        return self.references_model(model_name, app_label) and (
            not self.option_value
            or any((name in fields) for fields in self.option_value)
        return "Alter %s for %s (%s constraint(s))" % (
            self.option_name,
            len(self.option_value or ""),
        return "alter_%s_%s" % (self.name_lower, self.option_name)
        return super().can_reduce_through(operation, app_label) or (
            and type(operation) is not type(self)
class AlterUniqueTogether(AlterTogetherOptionOperation):
    Change the value of unique_together to the target one.
    Input value of unique_together must be a set of tuples.
    option_name = "unique_together"
    def __init__(self, name, unique_together):
        super().__init__(name, unique_together)
class AlterIndexTogether(AlterTogetherOptionOperation):
    Change the value of index_together to the target one.
    Input value of index_together must be a set of tuples.
    option_name = "index_together"
    def __init__(self, name, index_together):
        super().__init__(name, index_together)
class AlterOrderWithRespectTo(ModelOptionOperation):
    """Represent a change with the order_with_respect_to option."""
    option_name = "order_with_respect_to"
    def __init__(self, name, order_with_respect_to):
        self.order_with_respect_to = order_with_respect_to
            "order_with_respect_to": self.order_with_respect_to,
            {self.option_name: self.order_with_respect_to},
        to_model = to_state.apps.get_model(app_label, self.name)
        if self.allow_migrate_model(schema_editor.connection.alias, to_model):
            from_model = from_state.apps.get_model(app_label, self.name)
            # Remove a field if we need to
                from_model._meta.order_with_respect_to
                and not to_model._meta.order_with_respect_to
                schema_editor.remove_field(
                    from_model, from_model._meta.get_field("_order")
            # Add a field if we need to (altering the column is untouched as
            # it's likely a rename)
                to_model._meta.order_with_respect_to
                and not from_model._meta.order_with_respect_to
                field = to_model._meta.get_field("_order")
                if not field.has_default():
                    field.default = 0
                schema_editor.add_field(
                    from_model,
            self.order_with_respect_to is None or name == self.order_with_respect_to
        return "Set order_with_respect_to on %s to %s" % (
            self.order_with_respect_to,
        return "alter_%s_order_with_respect_to" % self.name_lower
class AlterModelOptions(ModelOptionOperation):
    Set new model options that don't directly affect the database schema
    (like verbose_name, permissions, ordering). Python code in migrations
    may still need them.
    # Model options we want to compare and preserve in an AlterModelOptions op
    ALTER_OPTION_KEYS = [
        "base_manager_name",
        "default_manager_name",
        "default_related_name",
        "get_latest_by",
        "managed",
        "ordering",
        "permissions",
        "default_permissions",
        "select_on_save",
        "verbose_name",
        "verbose_name_plural",
    def __init__(self, name, options):
            "options": self.options,
            self.options,
            self.ALTER_OPTION_KEYS,
        return "Change Meta options on %s" % self.name
        return "alter_%s_options" % self.name_lower
class AlterModelManagers(ModelOptionOperation):
    """Alter the model's managers."""
    serialization_expand_args = ["managers"]
    def __init__(self, name, managers):
        self.managers = managers
        return (self.__class__.__qualname__, [self.name, self.managers], {})
        state.alter_model_managers(app_label, self.name_lower, self.managers)
        return "Change managers on %s" % self.name
        return "alter_%s_managers" % self.name_lower
class IndexOperation(Operation):
    option_name = "indexes"
    def model_name_lower(self):
        return self.model_name.lower()
class AddIndex(IndexOperation):
    def __init__(self, model_name, index):
        self.model_name = model_name
        if not index.name:
                "Indexes passed to AddIndex operations require a name "
                "argument. %r doesn't have one." % index
        self.index = index
        state.add_index(app_label, self.model_name_lower, self.index)
        model = to_state.apps.get_model(app_label, self.model_name)
            schema_editor.add_index(model, self.index)
        model = from_state.apps.get_model(app_label, self.model_name)
            schema_editor.remove_index(model, self.index)
            "model_name": self.model_name,
            "index": self.index,
            kwargs,
        if self.index.expressions:
            return "Create index %s on %s on model %s" % (
                self.index.name,
                ", ".join([str(expression) for expression in self.index.expressions]),
                self.model_name,
        return "Create index %s on field(s) %s of model %s" % (
            ", ".join(self.index.fields),
        return "%s_%s" % (self.model_name_lower, self.index.name.lower())
        if isinstance(operation, RemoveIndex) and self.index.name == operation.name:
        if isinstance(operation, RenameIndex) and self.index.name == operation.old_name:
            index = copy(self.index)
            index.name = operation.new_name
            return [replace(self, index=index)]
class RemoveIndex(IndexOperation):
    def __init__(self, model_name, name):
        state.remove_index(app_label, self.model_name_lower, self.name)
            from_model_state = from_state.models[app_label, self.model_name_lower]
            index = from_model_state.get_index_by_name(self.name)
            schema_editor.remove_index(model, index)
            to_model_state = to_state.models[app_label, self.model_name_lower]
            index = to_model_state.get_index_by_name(self.name)
            schema_editor.add_index(model, index)
        return "Remove index %s from %s" % (self.name, self.model_name)
        return "remove_%s_%s" % (self.model_name_lower, self.name.lower())
class RenameIndex(IndexOperation):
    """Rename an index."""
    def __init__(self, model_name, new_name, old_name=None, old_fields=None):
        if not old_name and not old_fields:
                "RenameIndex requires one of old_name and old_fields arguments to be "
                "set."
        if old_name and old_fields:
                "RenameIndex.old_name and old_fields are mutually exclusive."
        self.old_fields = old_fields
        if self.old_name:
            kwargs["old_name"] = self.old_name
        if self.old_fields:
            kwargs["old_fields"] = self.old_fields
            state.add_index(
                self.model_name_lower,
                models.Index(fields=self.old_fields, name=self.new_name),
            state.remove_model_options(
                AlterIndexTogether.option_name,
                self.old_fields,
            state.rename_index(
                app_label, self.model_name_lower, self.old_name, self.new_name
        if not self.allow_migrate_model(schema_editor.connection.alias, model):
            from_model = from_state.apps.get_model(app_label, self.model_name)
            columns = [
                from_model._meta.get_field(field).column for field in self.old_fields
            matching_index_name = schema_editor._constraint_names(
                column_names=columns,
                unique=False,
            if len(matching_index_name) != 1:
                    "Found wrong number (%s) of indexes for %s(%s)."
                        len(matching_index_name),
                        from_model._meta.db_table,
            old_index = models.Index(
                fields=self.old_fields,
                name=matching_index_name[0],
            old_index = from_model_state.get_index_by_name(self.old_name)
        # Don't alter when the index name is not changed.
        if old_index.name == self.new_name:
        new_index = to_model_state.get_index_by_name(self.new_name)
        schema_editor.rename_index(model, old_index, new_index)
            # Backward operation with unnamed index is a no-op.
                f"Rename index {self.old_name} on {self.model_name} to {self.new_name}"
            f"Rename unnamed index for {self.old_fields} on {self.model_name} to "
            f"{self.new_name}"
        return "rename_%s_%s_%s" % (
            "_".join(self.old_fields),
            isinstance(operation, RenameIndex)
            and self.model_name_lower == operation.model_name_lower
            and operation.old_name
class AddConstraint(IndexOperation):
    option_name = "constraints"
    def __init__(self, model_name, constraint):
        self.constraint = constraint
        state.add_constraint(app_label, self.model_name_lower, self.constraint)
            schema_editor.add_constraint(model, self.constraint)
            schema_editor.remove_constraint(model, self.constraint)
                "constraint": self.constraint,
        return "Create constraint %s on model %s" % (
            self.constraint.name,
        return "%s_%s" % (self.model_name_lower, self.constraint.name.lower())
            isinstance(operation, RemoveConstraint)
            and self.constraint.name == operation.name
            isinstance(operation, AlterConstraint)
            return [replace(self, constraint=operation.constraint)]
class RemoveConstraint(IndexOperation):
        state.remove_constraint(app_label, self.model_name_lower, self.name)
            constraint = from_model_state.get_constraint_by_name(self.name)
            schema_editor.remove_constraint(model, constraint)
            constraint = to_model_state.get_constraint_by_name(self.name)
            schema_editor.add_constraint(model, constraint)
        return "Remove constraint %s from model %s" % (self.name, self.model_name)
class AlterConstraint(IndexOperation):
    def __init__(self, model_name, name, constraint):
        state.alter_constraint(
            app_label, self.model_name_lower, self.name, self.constraint
        return f"Alter constraint {self.name} on {self.model_name}"
        return "alter_%s_%s" % (self.model_name_lower, self.constraint.name.lower())
            isinstance(operation, (AlterConstraint, RemoveConstraint))
            and self.name == operation.name
Helper functions for creating Form classes from Django models
and database field objects.
from django.core.validators import ProhibitNullCharactersValidator
from django.forms.fields import ChoiceField, Field
from django.forms.forms import BaseForm, DeclarativeFieldsMetaclass
from django.forms.formsets import BaseFormSet, formset_factory
from django.forms.utils import ErrorList
from django.forms.widgets import (
    HiddenInput,
    MultipleHiddenInput,
    RadioSelect,
    SelectMultiple,
from django.utils.choices import BaseChoiceIterator
    "ModelForm",
    "BaseModelForm",
    "model_to_dict",
    "fields_for_model",
    "ModelChoiceField",
    "ModelMultipleChoiceField",
    "ALL_FIELDS",
    "BaseModelFormSet",
    "modelformset_factory",
    "BaseInlineFormSet",
    "inlineformset_factory",
    "modelform_factory",
ALL_FIELDS = "__all__"
def construct_instance(form, instance, fields=None, exclude=None):
    Construct and return a model instance from the bound ``form``'s
    ``cleaned_data``, but do not save the returned instance to the database.
    opts = instance._meta
    cleaned_data = form.cleaned_data
    file_field_list = []
    for f in opts.fields:
            not f.editable
            or isinstance(f, models.AutoField)
            or f.name not in cleaned_data
        if fields is not None and f.name not in fields:
        if exclude and f.name in exclude:
        # Leave defaults for fields that aren't in POST data, except for
        # checkbox inputs because they don't appear in POST data if not
        # checked.
            f.has_default()
            and form[f.name].field.widget.value_omitted_from_data(
                form.data, form.files, form.add_prefix(f.name)
            and cleaned_data.get(f.name) in form[f.name].field.empty_values
        # Defer saving file-type fields until after the other fields, so a
        # callable upload_to can use the values from other fields.
        if isinstance(f, models.FileField):
            file_field_list.append(f)
            f.save_form_data(instance, cleaned_data[f.name])
    for f in file_field_list:
# ModelForms #################################################################
def model_to_dict(instance, fields=None, exclude=None):
    Return a dict containing the data in ``instance`` suitable for passing as
    a Form's ``initial`` keyword argument.
    ``fields`` is an optional list of field names. If provided, return only the
    named.
    ``exclude`` is an optional list of field names. If provided, exclude the
    named from the returned dict, even if they are listed in the ``fields``
    argument.
    for f in chain(opts.concrete_fields, opts.private_fields, opts.many_to_many):
        if not getattr(f, "editable", False):
        data[f.name] = f.value_from_object(instance)
def apply_limit_choices_to_to_formfield(formfield):
    """Apply limit_choices_to to the formfield's queryset if needed."""
    from django.db.models import Exists, OuterRef, Q
    if hasattr(formfield, "queryset") and hasattr(formfield, "get_limit_choices_to"):
        limit_choices_to = formfield.get_limit_choices_to()
        if limit_choices_to:
            complex_filter = limit_choices_to
            if not isinstance(complex_filter, Q):
                complex_filter = Q(**limit_choices_to)
            complex_filter &= Q(pk=OuterRef("pk"))
            # Use Exists() to avoid potential duplicates.
            formfield.queryset = formfield.queryset.filter(
                Exists(formfield.queryset.model._base_manager.filter(complex_filter)),
def fields_for_model(
    widgets=None,
    formfield_callback=None,
    localized_fields=None,
    labels=None,
    help_texts=None,
    field_classes=None,
    apply_limit_choices_to=True,
    form_declared_fields=None,
    Return a dictionary containing form fields for the given model.
    named fields.
    named fields from the returned fields, even if they are listed in the
    ``fields`` argument.
    ``widgets`` is a dictionary of model field names mapped to a widget.
    ``formfield_callback`` is a callable that takes a model field and returns
    a form field.
    ``localized_fields`` is a list of names of fields which should be
    localized.
    ``labels`` is a dictionary of model field names mapped to a label.
    ``help_texts`` is a dictionary of model field names mapped to a help text.
    ``error_messages`` is a dictionary of model field names mapped to a
    dictionary of error messages.
    ``field_classes`` is a dictionary of model field names mapped to a form
    field class.
    ``apply_limit_choices_to`` is a boolean indicating if limit_choices_to
    should be applied to a field's queryset.
    ``form_declared_fields`` is a dictionary of form fields created directly on
    a form.
    form_declared_fields = form_declared_fields or {}
    ignored = []
    # Avoid circular import
    from django.db.models import Field as ModelField
    sortable_private_fields = [
        f for f in opts.private_fields if isinstance(f, ModelField)
    for f in sorted(
        chain(opts.concrete_fields, sortable_private_fields, opts.many_to_many)
                fields is not None
                and f.name in fields
                and (exclude is None or f.name not in exclude)
                    "'%s' cannot be specified for %s model form as it is a "
                    "non-editable field" % (f.name, model.__name__)
        if f.name in form_declared_fields:
            field_dict[f.name] = form_declared_fields[f.name]
        if widgets and f.name in widgets:
            kwargs["widget"] = widgets[f.name]
        if localized_fields == ALL_FIELDS or (
            localized_fields and f.name in localized_fields
            kwargs["localize"] = True
        if labels and f.name in labels:
            kwargs["label"] = labels[f.name]
        if help_texts and f.name in help_texts:
            kwargs["help_text"] = help_texts[f.name]
        if error_messages and f.name in error_messages:
            kwargs["error_messages"] = error_messages[f.name]
        if field_classes and f.name in field_classes:
            kwargs["form_class"] = field_classes[f.name]
        if formfield_callback is None:
            formfield = f.formfield(**kwargs)
        elif not callable(formfield_callback):
            raise TypeError("formfield_callback must be a function or callable")
            formfield = formfield_callback(f, **kwargs)
        if formfield:
            if apply_limit_choices_to:
                apply_limit_choices_to_to_formfield(formfield)
            field_dict[f.name] = formfield
            ignored.append(f.name)
        field_dict = {
            f: field_dict.get(f)
            if (not exclude or f not in exclude) and f not in ignored
    return field_dict
class ModelFormOptions:
    def __init__(self, options=None):
        self.model = getattr(options, "model", None)
        self.fields = getattr(options, "fields", None)
        self.exclude = getattr(options, "exclude", None)
        self.widgets = getattr(options, "widgets", None)
        self.localized_fields = getattr(options, "localized_fields", None)
        self.labels = getattr(options, "labels", None)
        self.help_texts = getattr(options, "help_texts", None)
        self.error_messages = getattr(options, "error_messages", None)
        self.field_classes = getattr(options, "field_classes", None)
        self.formfield_callback = getattr(options, "formfield_callback", None)
class ModelFormMetaclass(DeclarativeFieldsMetaclass):
    def __new__(mcs, name, bases, attrs):
        new_class = super().__new__(mcs, name, bases, attrs)
        if bases == (BaseModelForm,):
        opts = new_class._meta = ModelFormOptions(getattr(new_class, "Meta", None))
        # We check if a string was passed to `fields` or `exclude`,
        # which is likely to be a mistake where the user typed ('foo') instead
        # of ('foo',)
        for opt in ["fields", "exclude", "localized_fields"]:
            value = getattr(opts, opt)
            if isinstance(value, str) and value != ALL_FIELDS:
                    "%(model)s.Meta.%(opt)s cannot be a string. "
                    "Did you mean to type: ('%(value)s',)?"
                        "model": new_class.__name__,
                        "opt": opt,
                        "value": value,
        if opts.model:
            # If a model is defined, extract form fields from it.
            if opts.fields is None and opts.exclude is None:
                    "Creating a ModelForm without either the 'fields' attribute "
                    "or the 'exclude' attribute is prohibited; form %s "
                    "needs updating." % name
            if opts.fields == ALL_FIELDS:
                # Sentinel for fields_for_model to indicate "get the list of
                # fields from the model"
                opts.fields = None
            fields = fields_for_model(
                opts.model,
                opts.fields,
                opts.exclude,
                opts.widgets,
                opts.formfield_callback,
                opts.localized_fields,
                opts.labels,
                opts.help_texts,
                opts.error_messages,
                opts.field_classes,
                # limit_choices_to will be applied during ModelForm.__init__().
                apply_limit_choices_to=False,
                form_declared_fields=new_class.declared_fields,
            # make sure opts.fields doesn't specify an invalid field
            none_model_fields = {k for k, v in fields.items() if not v}
            missing_fields = none_model_fields.difference(new_class.declared_fields)
            if missing_fields:
                message = "Unknown field(s) (%s) specified for %s"
                message %= (", ".join(missing_fields), opts.model.__name__)
                raise FieldError(message)
            # Include all the other declared fields.
            fields.update(new_class.declared_fields)
            fields = new_class.declared_fields
        new_class.base_fields = fields
class BaseModelForm(BaseForm, AltersData):
        data=None,
        auto_id="id_%s",
        prefix=None,
        initial=None,
        error_class=ErrorList,
        label_suffix=None,
        empty_permitted=False,
        instance=None,
        use_required_attribute=None,
        renderer=None,
        if opts.model is None:
            raise ValueError("ModelForm has no model class specified.")
            # if we didn't get an instance, instantiate a new one
            self.instance = opts.model()
            object_data = {}
            object_data = model_to_dict(instance, opts.fields, opts.exclude)
        # if initial was provided, it should override the values from instance
        if initial is not None:
            object_data.update(initial)
        # self._validate_(unique|constraints) will be set to True by
        # BaseModelForm.clean(). It is False by default so overriding
        # self.clean() and failing to call super will stop
        # validate_(unique|constraints) from being called.
        self._validate_unique = False
        self._validate_constraints = False
            auto_id,
            object_data,
            error_class,
            label_suffix,
            empty_permitted,
            use_required_attribute=use_required_attribute,
            renderer=renderer,
        for formfield in self.fields.values():
    def _get_validation_exclusions(self):
        For backwards-compatibility, exclude several types of fields from model
        validation. See tickets #12507, #12521, #12553.
        # Build up a list of fields that should be excluded from model field
        # validation and unique checks.
        for f in self.instance._meta.fields:
            field = f.name
            # Exclude fields that aren't on the form. The developer may be
            # adding these values to the model after form validation.
            if field not in self.fields:
                exclude.add(f.name)
            # Don't perform model validation on fields that were defined
            # manually on the form and excluded via the ModelForm's Meta
            # class. See #12901.
            elif self._meta.fields and field not in self._meta.fields:
            elif self._meta.exclude and field in self._meta.exclude:
            # Exclude fields that failed form validation. There's no need for
            # the model fields to validate them as well.
            elif field in self._errors:
            # Exclude empty fields that are not required by the form, if the
            # underlying model field is required. This keeps the model field
            # from raising a required error. Note: don't exclude the field from
            # validation if the model field allows blanks. If it does, the
            # blank value may be included in a unique check, so cannot be
            # excluded from validation.
                form_field = self.fields[field]
                field_value = self.cleaned_data.get(field)
                    not f.blank
                    and not form_field.required
                    and field_value in form_field.empty_values
        return exclude
        self._validate_unique = True
        self._validate_constraints = True
        return self.cleaned_data
    def _update_errors(self, errors):
        # Override any validation error messages defined at the model level
        # with those defined at the form level.
        # Allow the model generated by construct_instance() to raise
        # ValidationError and have them handled in the same way as others.
        if hasattr(errors, "error_dict"):
            error_dict = errors.error_dict
            error_dict = {NON_FIELD_ERRORS: errors}
        for field, messages in error_dict.items():
                field == NON_FIELD_ERRORS
                and opts.error_messages
                and NON_FIELD_ERRORS in opts.error_messages
                error_messages = opts.error_messages[NON_FIELD_ERRORS]
            elif field in self.fields:
                error_messages = self.fields[field].error_messages
                    isinstance(message, ValidationError)
                    and message.code in error_messages
                    message.message = error_messages[message.code]
        self.add_error(None, errors)
    def _post_clean(self):
        exclude = self._get_validation_exclusions()
        # Foreign Keys being used to represent inline relationships
        # are excluded from basic field value validation. This is for two
        # reasons: firstly, the value may not be supplied (#12507; the
        # case of providing new values to the admin); secondly the
        # object being referred to may not yet fully exist (#12749).
        # However, these fields *must* be included in uniqueness checks,
        # so this can't be part of _get_validation_exclusions().
        for name, field in self.fields.items():
            if isinstance(field, InlineForeignKeyField):
            self.instance = construct_instance(
                self, self.instance, opts.fields, opts.exclude
            self._update_errors(e)
            self.instance.full_clean(
                exclude=exclude, validate_unique=False, validate_constraints=False
        # Validate uniqueness and constraints if needed.
        if self._validate_unique:
            self.validate_unique()
        if self._validate_constraints:
            self.validate_constraints()
    def validate_unique(self):
        Call the instance's validate_unique() method and update the form's
        validation errors if any were raised.
            self.instance.validate_unique(exclude=exclude)
    def validate_constraints(self):
        Call the instance's validate_constraints() method and update the form's
            self.instance.validate_constraints(exclude=exclude)
    def _save_m2m(self):
        Save the many-to-many fields and generic relations for this form.
        cleaned_data = self.cleaned_data
        exclude = self._meta.exclude
        fields = self._meta.fields
        opts = self.instance._meta
        # Note that for historical reasons we want to include also
        # private_fields here. (GenericRelation was previously a fake
        # m2m field).
        for f in chain(opts.many_to_many, opts.private_fields):
            if not hasattr(f, "save_form_data"):
            if fields and f.name not in fields:
            if f.name in cleaned_data:
                f.save_form_data(self.instance, cleaned_data[f.name])
    def save(self, commit=True):
        Save this form's self.instance object if commit=True. Otherwise, add
        a save_m2m() method to the form which can be called after the instance
        is saved manually at a later time. Return the model instance.
        if self.errors:
                "The %s could not be %s because the data didn't validate."
                    self.instance._meta.object_name,
                    "created" if self.instance._state.adding else "changed",
        if commit:
            # If committing, save the instance and the m2m data immediately.
            self._save_m2m()
            # If not committing, add a method to the form to allow deferred
            # saving of m2m data.
            self.save_m2m = self._save_m2m
        return self.instance
class ModelForm(BaseModelForm, metaclass=ModelFormMetaclass):
def modelform_factory(
    form=ModelForm,
    Return a ModelForm containing form fields for the given model. You can
    optionally pass a `form` argument to use as a starting point for
    constructing the ModelForm.
    ``fields`` is an optional list of field names. If provided, include only
    the named fields in the returned fields. If omitted or '__all__', use all
    # Create the inner Meta class. FIXME: ideally, we should be able to
    # construct a ModelForm without creating and passing in a temporary
    # inner class.
    # Build up a list of attributes that the Meta object will have.
    attrs = {"model": model}
        attrs["fields"] = fields
        attrs["exclude"] = exclude
    if widgets is not None:
        attrs["widgets"] = widgets
    if localized_fields is not None:
        attrs["localized_fields"] = localized_fields
    if labels is not None:
        attrs["labels"] = labels
    if help_texts is not None:
        attrs["help_texts"] = help_texts
    if error_messages is not None:
        attrs["error_messages"] = error_messages
    if field_classes is not None:
        attrs["field_classes"] = field_classes
    # If parent form class already has an inner Meta, the Meta we're
    # creating needs to inherit from the parent's inner meta.
    bases = (form.Meta,) if hasattr(form, "Meta") else ()
    Meta = type("Meta", bases, attrs)
    if formfield_callback:
        Meta.formfield_callback = staticmethod(formfield_callback)
    # Give this new form class a reasonable name.
    class_name = model.__name__ + "Form"
    # Class attributes for the new form class.
    form_class_attrs = {"Meta": Meta}
    if getattr(Meta, "fields", None) is None and getattr(Meta, "exclude", None) is None:
            "Calling modelform_factory without defining 'fields' or "
            "'exclude' explicitly is prohibited."
    # Instantiate type(form) in order to use the same metaclass as form.
    return type(form)(class_name, (form,), form_class_attrs)
# ModelFormSets ##############################################################
class BaseModelFormSet(BaseFormSet, AltersData):
    A ``FormSet`` for editing a queryset and/or adding new objects to it.
    edit_only = False
    # Set of fields that must be unique among forms of this set.
    unique_fields = set()
        queryset=None,
        self.initial_extra = initial
                "data": data,
                "auto_id": auto_id,
    def initial_form_count(self):
        """Return the number of forms that are required in this FormSet."""
        if not self.is_bound:
            return len(self.get_queryset())
        return super().initial_form_count()
    def _existing_object(self, pk):
        if not hasattr(self, "_object_dict"):
            self._object_dict = {o.pk: o for o in self.get_queryset()}
        return self._object_dict.get(pk)
    def _get_to_python(self, field):
        If the field is a related field, fetch the concrete field's (that
        is, the ultimate pointed-to field's) to_python.
        while field.remote_field is not None:
            field = field.remote_field.get_related_field()
        return field.to_python
    def _construct_form(self, i, **kwargs):
        pk_required = i < self.initial_form_count()
        if pk_required:
            if self.is_bound:
                pk_key = "%s-%s" % (self.add_prefix(i), self.model._meta.pk.name)
                    pk = self.data[pk_key]
                    # The primary key is missing. The user may have tampered
                    # with POST data.
                    to_python = self._get_to_python(self.model._meta.pk)
                        pk = to_python(pk)
                        # The primary key exists but is an invalid value. The
                        # user may have tampered with POST data.
                        kwargs["instance"] = self._existing_object(pk)
                kwargs["instance"] = self.get_queryset()[i]
        elif self.initial_extra:
            # Set initial values for extra forms
                kwargs["initial"] = self.initial_extra[i - self.initial_form_count()]
        form = super()._construct_form(i, **kwargs)
            form.fields[self.model._meta.pk.name].required = True
        return form
    def get_queryset(self):
        if not hasattr(self, "_queryset"):
                qs = self.queryset
            # If the queryset isn't already ordered we need to add an
            # artificial ordering here to make sure that all formsets
            # constructed from this queryset have the same form order.
            if not qs.ordered:
                qs = qs.order_by(self.model._meta.pk.name)
            # Removed queryset limiting here. As per discussion re: #13023
            # on django-dev, max_num should not prevent existing
            # related objects/inlines from being displayed.
            self._queryset = qs
        return self._queryset
    def save_new(self, form, commit=True):
        """Save and return a new model instance for the given form."""
        return form.save(commit=commit)
    def save_existing(self, form, obj, commit=True):
        """Save and return an existing model instance for the given form."""
    def delete_existing(self, obj, commit=True):
        """Deletes an existing model instance."""
        Save model instances for every form, adding and changing instances
        as necessary, and return the list of instances.
        if not commit:
            self.saved_forms = []
            def save_m2m():
                for form in self.saved_forms:
            self.save_m2m = save_m2m
        if self.edit_only:
            return self.save_existing_objects(commit)
            return self.save_existing_objects(commit) + self.save_new_objects(commit)
        # Collect unique_checks and date_checks to run from all the forms.
        all_unique_checks = set()
        all_date_checks = set()
        forms_to_delete = self.deleted_forms
        valid_forms = [
            form
            for form in self.forms
            if form.is_valid() and form not in forms_to_delete
        for form in valid_forms:
            exclude = form._get_validation_exclusions()
            unique_checks, date_checks = form.instance._get_unique_checks(
                include_meta_constraints=True,
            all_unique_checks.update(unique_checks)
            all_date_checks.update(date_checks)
        # Do each of the unique checks (unique and unique_together)
        for uclass, unique_check in all_unique_checks:
            seen_data = set()
                # Get the data for the set of fields that must be unique among
                # the forms.
                row_data = (
                    field if field in self.unique_fields else form.cleaned_data[field]
                    for field in unique_check
                    if field in form.cleaned_data
                # Reduce Model instances to their primary key values
                row_data = tuple(
                        d._get_pk_val()
                        if hasattr(d, "_get_pk_val")
                        # Prevent "unhashable type" errors later on.
                        else make_hashable(d)
                    for d in row_data
                if row_data and None not in row_data:
                    # if we've already seen it then we have a uniqueness
                    # failure
                    if row_data in seen_data:
                        # poke error messages into the right places and mark
                        # the form as invalid
                        errors.append(self.get_unique_error_message(unique_check))
                        form._errors[NON_FIELD_ERRORS] = self.error_class(
                            [self.get_form_error()],
                            renderer=self.renderer,
                        # Remove the data from the cleaned_data dict since it
                        # was invalid.
                        for field in unique_check:
                            if field in form.cleaned_data:
                                del form.cleaned_data[field]
                    # mark the data as seen
                    seen_data.add(row_data)
        # iterate over each of the date checks now
        for date_check in all_date_checks:
            uclass, lookup, field, unique_for = date_check
                # see if we have data for both fields
                    form.cleaned_data
                    and form.cleaned_data[field] is not None
                    and form.cleaned_data[unique_for] is not None
                    # if it's a date lookup we need to get the data for all the
                    # fields
                    if lookup == "date":
                        date = form.cleaned_data[unique_for]
                        date_data = (date.year, date.month, date.day)
                    # otherwise it's just the attribute on the date/datetime
                    # object
                        date_data = (getattr(form.cleaned_data[unique_for], lookup),)
                    data = (form.cleaned_data[field], *date_data)
                    if data in seen_data:
                        errors.append(self.get_date_error_message(date_check))
                    seen_data.add(data)
    def get_unique_error_message(self, unique_check):
            return gettext("Please correct the duplicate data for %(field)s.") % {
                "field": unique_check[0],
            return gettext(
                "Please correct the duplicate data for %(field)s, which must be unique."
                "field": get_text_list(unique_check, _("and")),
    def get_date_error_message(self, date_check):
            "Please correct the duplicate data for %(field_name)s "
            "which must be unique for the %(lookup)s in %(date_field)s."
            "field_name": date_check[2],
            "date_field": date_check[3],
            "lookup": str(date_check[1]),
    def get_form_error(self):
        return gettext("Please correct the duplicate values below.")
    def save_existing_objects(self, commit=True):
        self.changed_objects = []
        self.deleted_objects = []
        if not self.initial_forms:
        saved_instances = []
        for form in self.initial_forms:
            obj = form.instance
            # If the pk is None, it means either:
            # 1. The object is an unexpected empty model, created by invalid
            #    POST data such as an object outside the formset's queryset.
            # 2. The object was already deleted from the database.
            if form in forms_to_delete:
                self.deleted_objects.append(obj)
                self.delete_existing(obj, commit=commit)
            elif form.has_changed():
                self.changed_objects.append((obj, form.changed_data))
                saved_instances.append(self.save_existing(form, obj, commit=commit))
                    self.saved_forms.append(form)
        return saved_instances
    def save_new_objects(self, commit=True):
        self.new_objects = []
        for form in self.extra_forms:
            if not form.has_changed():
            # If someone has marked an add form for deletion, don't save the
            # object.
            if self.can_delete and self._should_delete_form(form):
            self.new_objects.append(self.save_new(form, commit=commit))
        return self.new_objects
    def add_fields(self, form, index):
        """Add a hidden field for the object's primary key."""
        from django.db.models import AutoField, ForeignKey, OneToOneField
        self._pk_field = pk = self.model._meta.pk
        # If a pk isn't editable, then it won't be on the form, so we need to
        # add it here so we can tell which object is which when we get the
        # data back. Generally, pk.editable should be false, but for some
        # reason, auto_created pk fields and AutoField's editable attribute is
        # True, so check for that as well.
        def pk_is_not_editable(pk):
                (not pk.editable)
                or (pk.auto_created or isinstance(pk, AutoField))
                    pk.remote_field
                    and pk.remote_field.parent_link
                    and pk_is_not_editable(pk.remote_field.model._meta.pk)
        if pk_is_not_editable(pk) or pk.name not in form.fields:
                # If we're adding the related instance, ignore its primary key
                # as it could be an auto-generated default which isn't actually
                # in the database.
                pk_value = None if form.instance._state.adding else form.instance.pk
                    if index is not None:
                        pk_value = self.get_queryset()[index].pk
                        pk_value = None
            if isinstance(pk, (ForeignKey, OneToOneField)):
                qs = pk.remote_field.model._default_manager.get_queryset()
            qs = qs.using(form.instance._state.db)
            if form._meta.widgets:
                widget = form._meta.widgets.get(self._pk_field.name, HiddenInput)
                widget = HiddenInput
            form.fields[self._pk_field.name] = ModelChoiceField(
                qs, initial=pk_value, required=False, widget=widget
        super().add_fields(form, index)
def modelformset_factory(
    formset=BaseModelFormSet,
    extra=1,
    can_delete=False,
    can_order=False,
    max_num=None,
    validate_max=False,
    min_num=None,
    validate_min=False,
    absolute_max=None,
    can_delete_extra=True,
    edit_only=False,
    """Return a FormSet class for the given Django model class."""
    meta = getattr(form, "Meta", None)
        getattr(meta, "fields", fields) is None
        and getattr(meta, "exclude", exclude) is None
            "Calling modelformset_factory without defining 'fields' or "
    form = modelform_factory(
        form=form,
        formfield_callback=formfield_callback,
        widgets=widgets,
        localized_fields=localized_fields,
        labels=labels,
        help_texts=help_texts,
        error_messages=error_messages,
        field_classes=field_classes,
    FormSet = formset_factory(
        extra=extra,
        min_num=min_num,
        max_num=max_num,
        can_order=can_order,
        can_delete=can_delete,
        validate_min=validate_min,
        validate_max=validate_max,
        absolute_max=absolute_max,
        can_delete_extra=can_delete_extra,
    FormSet.model = model
    FormSet.edit_only = edit_only
    return FormSet
# InlineFormSets #############################################################
class BaseInlineFormSet(BaseModelFormSet):
    """A formset for child objects related to a parent."""
        save_as_new=False,
            self.instance = self.fk.remote_field.model()
        self.save_as_new = save_as_new
        if queryset is None:
            queryset = self.model._default_manager
        if self.instance._is_pk_set():
            qs = queryset.filter(**{self.fk.name: self.instance})
            qs = queryset.none()
        self.unique_fields = {self.fk.name}
        super().__init__(data, files, prefix=prefix, queryset=qs, **kwargs)
        # Add the inline foreign key field to form._meta.fields if it's defined
        # to make sure validation isn't skipped on that field.
        if self.form._meta.fields and self.fk.name not in self.form._meta.fields:
            self.form._meta.fields = list(self.form._meta.fields)
            self.form._meta.fields.append(self.fk.name)
        if self.save_as_new:
            mutable = getattr(form.data, "_mutable", None)
            # Allow modifying an immutable QueryDict.
            if mutable is not None:
                form.data._mutable = True
            # Remove the primary key from the form's data, we are only
            # creating new instances
            form.data[form.add_prefix(self._pk_field.name)] = None
            # Remove the foreign key from the form's data
            form.data[form.add_prefix(self.fk.name)] = None
                form.data._mutable = mutable
        # Set the fk value here so that the form can do its validation.
        fk_value = self.instance.pk
        if self.fk.remote_field.field_name != self.fk.remote_field.model._meta.pk.name:
            fk_value = getattr(self.instance, self.fk.remote_field.field_name)
            fk_value = getattr(fk_value, "pk", fk_value)
        setattr(form.instance, self.fk.attname, fk_value)
    def get_default_prefix(cls):
        return cls.fk.remote_field.get_accessor_name(model=cls.model).replace("+", "")
        # Ensure the latest copy of the related instance is present on each
        # form (it may have been saved after the formset was originally
        # instantiated).
        setattr(form.instance, self.fk.name, self.instance)
        return super().save_new(form, commit=commit)
        if self._pk_field == self.fk:
            name = self._pk_field.name
            kwargs = {"pk_field": True}
            # The foreign key field might not be on the form, so we poke at the
            # Model field to get the label, since we need that for error
            # messages.
            name = self.fk.name
                "label": getattr(
                    form.fields.get(name), "label", capfirst(self.fk.verbose_name)
        # The InlineForeignKeyField assumes that the foreign key relation is
        # based on the parent model's pk. If this isn't the case, set to_field
        # to correctly resolve the initial form value.
            kwargs["to_field"] = self.fk.remote_field.field_name
        # If we're adding a new object, ignore a parent's auto-generated key
        # as it will be regenerated on the save request.
            if kwargs.get("to_field") is not None:
                to_field = self.instance._meta.get_field(kwargs["to_field"])
                to_field = self.instance._meta.pk
            if to_field.has_default() and (
                # Don't ignore a parent's auto-generated key if it's not the
                # parent model's pk and form data is provided.
                to_field.attname == self.fk.remote_field.model._meta.pk.name
                or not form.data
                setattr(self.instance, to_field.attname, None)
        form.fields[name] = InlineForeignKeyField(self.instance, **kwargs)
        unique_check = [field for field in unique_check if field != self.fk.name]
        return super().get_unique_error_message(unique_check)
def _get_foreign_key(parent_model, model, fk_name=None, can_fail=False):
    Find and return the ForeignKey from model to parent if there is one
    (return None if can_fail is True and no such field exists). If fk_name is
    provided, assume it is the name of the ForeignKey field. Unless can_fail is
    True, raise an exception if there isn't a ForeignKey from model to
    parent_model.
    # avoid circular import
    from django.db.models import ForeignKey
    if fk_name:
        fks_to_parent = [f for f in opts.fields if f.name == fk_name]
        if len(fks_to_parent) == 1:
            fk = fks_to_parent[0]
            all_parents = (*parent_model._meta.all_parents, parent_model)
                not isinstance(fk, ForeignKey)
                    # ForeignKey to proxy models.
                    fk.remote_field.model._meta.proxy
                    and fk.remote_field.model._meta.proxy_for_model not in all_parents
                    # ForeignKey to concrete models.
                    not fk.remote_field.model._meta.proxy
                    and fk.remote_field.model != parent_model
                    and fk.remote_field.model not in all_parents
                    "fk_name '%s' is not a ForeignKey to '%s'."
                    % (fk_name, parent_model._meta.label)
        elif not fks_to_parent:
                "'%s' has no field named '%s'." % (model._meta.label, fk_name)
        # Try to discover what the ForeignKey from model to parent_model is
        fks_to_parent = [
            for f in opts.fields
            if isinstance(f, ForeignKey)
                f.remote_field.model == parent_model
                or f.remote_field.model in all_parents
                    f.remote_field.model._meta.proxy
                    and f.remote_field.model._meta.proxy_for_model in all_parents
            if can_fail:
                "'%s' has no ForeignKey to '%s'."
                    model._meta.label,
                    parent_model._meta.label,
                "'%s' has more than one ForeignKey to '%s'. You must specify "
                "a 'fk_name' attribute."
    return fk
def inlineformset_factory(
    parent_model,
    formset=BaseInlineFormSet,
    fk_name=None,
    extra=3,
    can_delete=True,
    Return an ``InlineFormSet`` for the given kwargs.
    ``fk_name`` must be provided if ``model`` has more than one ``ForeignKey``
    to ``parent_model``.
    fk = _get_foreign_key(parent_model, model, fk_name=fk_name)
    # enforce a max_num=1 when the foreign key to the parent model is unique.
    if fk.unique:
        max_num = 1
        "formfield_callback": formfield_callback,
        "formset": formset,
        "extra": extra,
        "can_order": can_order,
        "min_num": min_num,
        "max_num": max_num,
        "widgets": widgets,
        "validate_min": validate_min,
        "validate_max": validate_max,
        "localized_fields": localized_fields,
        "labels": labels,
        "help_texts": help_texts,
        "error_messages": error_messages,
        "field_classes": field_classes,
        "absolute_max": absolute_max,
        "can_delete_extra": can_delete_extra,
        "renderer": renderer,
        "edit_only": edit_only,
    FormSet = modelformset_factory(model, **kwargs)
    FormSet.fk = fk
# Fields #####################################################################
class InlineForeignKeyField(Field):
    A basic integer field that deals with validating the given value to a
    given parent instance in an inline.
        "invalid_choice": _("The inline value did not match the parent instance."),
    def __init__(self, parent_instance, *args, pk_field=False, to_field=None, **kwargs):
        self.parent_instance = parent_instance
        self.pk_field = pk_field
        if self.parent_instance is not None:
            if self.to_field:
                kwargs["initial"] = getattr(self.parent_instance, self.to_field)
                kwargs["initial"] = self.parent_instance.pk
        kwargs["required"] = False
    def clean(self, value):
            if self.pk_field:
            # if there is no value act as we did before.
            return self.parent_instance
        # ensure the we compare the values as equal types.
            orig = getattr(self.parent_instance, self.to_field)
            orig = self.parent_instance.pk
        if str(value) != str(orig):
                self.error_messages["invalid_choice"], code="invalid_choice"
    def has_changed(self, initial, data):
class ModelChoiceIteratorValue:
    def __init__(self, value, instance):
        return str(self.value)
        return hash(self.value)
        if isinstance(other, ModelChoiceIteratorValue):
            other = other.value
        return self.value == other
class ModelChoiceIterator(BaseChoiceIterator):
        self.queryset = field.queryset
        if self.field.empty_label is not None:
            yield ("", self.field.empty_label)
        # Can't use iterator() when queryset uses prefetch_related()
        if not queryset._prefetch_related_lookups:
            queryset = queryset.iterator()
        for obj in queryset:
            yield self.choice(obj)
        # count() adds a query but uses less memory since the QuerySet results
        # won't be cached. In most cases, the choices will only be iterated on,
        # and __len__() won't be called.
        return self.queryset.count() + (1 if self.field.empty_label is not None else 0)
        return self.field.empty_label is not None or self.queryset.exists()
    def choice(self, obj):
            ModelChoiceIteratorValue(self.field.prepare_value(obj), obj),
            self.field.label_from_instance(obj),
class ModelChoiceField(ChoiceField):
    """A ChoiceField whose choices are a model QuerySet."""
    # This class is a subclass of ChoiceField for purity, but it doesn't
    # actually use any of ChoiceField's implementation.
        "invalid_choice": _(
            "Select a valid choice. That choice is not one of the available choices."
    iterator = ModelChoiceIterator
        empty_label="---------",
        widget=None,
        label=None,
        to_field_name=None,
        # Call Field instead of ChoiceField __init__() because we don't need
        # ChoiceField.__init__().
        Field.__init__(
            required=required,
            widget=widget,
            label=label,
            initial=initial,
            help_text=help_text,
        if (required and initial is not None) or (
            isinstance(self.widget, RadioSelect) and not blank
            self.empty_label = None
            self.empty_label = empty_label
        self.limit_choices_to = limit_choices_to  # limit the queryset later.
        self.to_field_name = to_field_name
    def validate_no_null_characters(self, value):
        non_null_character_validator = ProhibitNullCharactersValidator()
        return non_null_character_validator(value)
    def get_limit_choices_to(self):
        Return ``limit_choices_to`` for this form field.
        If it is a callable, invoke it and return the result.
        if callable(self.limit_choices_to):
            return self.limit_choices_to()
        return self.limit_choices_to
        result = super(ChoiceField, self).__deepcopy__(memo)
        # Need to force a new ModelChoiceIterator to be created, bug #11183
            result.queryset = self.queryset.all()
    def _get_queryset(self):
    def _set_queryset(self, queryset):
        self._queryset = None if queryset is None else queryset.all()
        self.widget.choices = self.choices
    queryset = property(_get_queryset, _set_queryset)
    # this method will be used to create object labels by the QuerySetIterator.
    # Override it to customize the label.
    def label_from_instance(self, obj):
        Convert objects into strings and generate the labels for the choices
        presented by this object. Subclasses can override this method to
        customize the display of the choices.
        return str(obj)
    def _get_choices(self):
        # If self._choices is set, then somebody must have manually set
        # the property self.choices. In this case, just return self._choices.
        if hasattr(self, "_choices"):
        # Otherwise, execute the QuerySet in self.queryset to determine the
        # choices dynamically. Return a fresh ModelChoiceIterator that has not
        # been consumed. Note that we're instantiating a new
        # ModelChoiceIterator *each* time _get_choices() is called (and, thus,
        # each time self.choices is accessed) so that we can ensure the
        # QuerySet has not been consumed. This construct might look complicated
        # but it allows for lazy evaluation of the queryset.
        return self.iterator(self)
    choices = property(_get_choices, ChoiceField.choices.fset)
    def prepare_value(self, value):
            if self.to_field_name:
                return value.serializable_value(self.to_field_name)
                return value.pk
        return super().prepare_value(value)
        self.validate_no_null_characters(value)
            key = self.to_field_name or "pk"
            if isinstance(value, self.queryset.model):
                value = getattr(value, key)
            value = self.queryset.get(**{key: value})
        except (
            self.queryset.model.DoesNotExist,
    def validate(self, value):
        return Field.validate(self, value)
        if self.disabled:
        initial_value = initial if initial is not None else ""
        data_value = data if data is not None else ""
        return str(self.prepare_value(initial_value)) != str(data_value)
class ModelMultipleChoiceField(ModelChoiceField):
    """A MultipleChoiceField whose choices are a model QuerySet."""
    widget = SelectMultiple
    hidden_widget = MultipleHiddenInput
        "invalid_list": _("Enter a list of values."),
            "Select a valid choice. %(value)s is not one of the available choices."
        "invalid_pk_value": _("“%(pk)s” is not a valid value."),
    def __init__(self, queryset, **kwargs):
        super().__init__(queryset, empty_label=None, **kwargs)
        return list(self._check_values(value))
        value = self.prepare_value(value)
        if self.required and not value:
            raise ValidationError(self.error_messages["required"], code="required")
        elif not self.required and not value:
            return self.queryset.none()
        if not isinstance(value, (list, tuple)):
                self.error_messages["invalid_list"],
                code="invalid_list",
        qs = self._check_values(value)
        # Since this overrides the inherited ModelChoiceField.clean
        # we run custom validators here
    def _check_values(self, value):
        Given a list of possible PK values, return a QuerySet of the
        corresponding objects. Raise a ValidationError if a given value is
        invalid (not a valid PK, not in the queryset, etc.)
        # deduplicate given values to avoid creating many querysets or
        # requiring the database backend deduplicate efficiently.
            value = frozenset(value)
            # list of lists isn't hashable, for example
        for pk in value:
            self.validate_no_null_characters(pk)
                self.queryset.filter(**{key: pk})
            except (ValueError, TypeError, ValidationError):
                    self.error_messages["invalid_pk_value"],
                    code="invalid_pk_value",
                    params={"pk": pk},
        qs = self.queryset.filter(**{"%s__in" % key: value})
        pks = {str(getattr(o, key)) for o in qs}
        for val in value:
            if str(val) not in pks:
                    params={"value": val},
            hasattr(value, "__iter__")
            and not isinstance(value, str)
            and not hasattr(value, "_meta")
            prepare_value = super().prepare_value
            return [prepare_value(v) for v in value]
        if initial is None:
            initial = []
            data = []
        if len(initial) != len(data):
        initial_set = {str(value) for value in self.prepare_value(initial)}
        data_set = {str(value) for value in data}
        return data_set != initial_set
def modelform_defines_fields(form_class):
    return hasattr(form_class, "_meta") and (
        form_class._meta.fields is not None or form_class._meta.exclude is not None
class Story(models.Model):
    title = models.CharField(max_length=10)
from django.contrib.auth.models import User
class Event(models.Model):
    # Oracle can have problems with a column named "date"
    date = models.DateField(db_column="event_date")
class Parent(models.Model):
    name = models.CharField(max_length=128)
class Child(models.Model):
    parent = models.ForeignKey(Parent, models.SET_NULL, editable=False, null=True)
    name = models.CharField(max_length=30, blank=True)
    age = models.IntegerField(null=True, blank=True)
    is_active = models.BooleanField(default=True)
class GrandChild(models.Model):
    parent = models.ForeignKey(Child, models.SET_NULL, editable=False, null=True)
    def __html__(self):
        return f'<h2 class="main">{self.name}</h2>'
class Genre(models.Model):
    name = models.CharField(max_length=20)
    file = models.FileField(upload_to="documents/", blank=True, null=True)
    url = models.URLField(blank=True, null=True)
class Band(models.Model):
    nr_of_members = models.PositiveIntegerField()
    genres = models.ManyToManyField(Genre)
class Musician(models.Model):
    name = models.CharField(max_length=30)
    members = models.ManyToManyField(Musician, through="Membership")
class Concert(models.Model):
    group = models.ForeignKey(Group, models.CASCADE)
class Membership(models.Model):
    music = models.ForeignKey(Musician, models.CASCADE)
    role = models.CharField(max_length=15)
class Quartet(Group):
class ChordsMusician(Musician):
class ChordsBand(models.Model):
    members = models.ManyToManyField(ChordsMusician, through="Invitation")
class Invitation(models.Model):
    player = models.ForeignKey(ChordsMusician, models.CASCADE)
    band = models.ForeignKey(ChordsBand, models.CASCADE)
    instrument = models.CharField(max_length=15)
class Swallow(models.Model):
    uuid = models.UUIDField(primary_key=True, default=uuid.uuid4)
    origin = models.CharField(max_length=255)
    load = models.FloatField()
    speed = models.FloatField()
        ordering = ("speed", "load")
class SwallowOneToOne(models.Model):
    swallow = models.OneToOneField(Swallow, models.CASCADE)
class UnorderedObject(models.Model):
    Model without any defined `Meta.ordering`.
    Refs #17198.
    bool = models.BooleanField(default=True)
class OrderedObjectManager(models.Manager):
        return super().get_queryset().order_by("number")
class OrderedObject(models.Model):
    Model with Manager that defines a default order.
    name = models.CharField(max_length=255)
    number = models.IntegerField(default=0, db_column="number_val")
    objects = OrderedObjectManager()
class CustomIdUser(models.Model):
    uuid = models.AutoField(primary_key=True)
class CharPK(models.Model):
    char_pk = models.CharField(max_length=100, primary_key=True)
class ProxyUser(User):
        proxy = True
class MixedFieldsModel(models.Model):
    """Model with multiple field types for testing search validation."""
    int_field = models.IntegerField(null=True, blank=True)
    json_field = models.JSONField(null=True, blank=True)
Tests of ModelAdmin system checks logic.
from django.contrib.contenttypes.fields import GenericForeignKey
class Album(models.Model):
    title = models.CharField(max_length=150)
class Song(models.Model):
    album = models.ForeignKey(Album, models.CASCADE)
    original_release = models.DateField(editable=False)
        ordering = ("title",)
        return self.title
    def readonly_method_on_model(self):
        # does nothing
class TwoAlbumFKAndAnE(models.Model):
    album1 = models.ForeignKey(Album, models.CASCADE, related_name="album1_set")
    album2 = models.ForeignKey(Album, models.CASCADE, related_name="album2_set")
    e = models.CharField(max_length=1)
class Author(models.Model):
    name = models.CharField(max_length=100)
class Book(models.Model):
    subtitle = models.CharField(max_length=100)
    price = models.FloatField()
    authors = models.ManyToManyField(Author, through="AuthorsBooks")
class AuthorsBooks(models.Model):
    author = models.ForeignKey(Author, models.CASCADE)
    book = models.ForeignKey(Book, models.CASCADE)
    featured = models.BooleanField()
class State(models.Model):
    name = models.CharField(max_length=15)
class City(models.Model):
    state = models.ForeignKey(State, models.CASCADE)
class Influence(models.Model):
    name = models.TextField()
    content_type = models.ForeignKey(ContentType, models.CASCADE)
    object_id = models.PositiveIntegerField()
    content_object = GenericForeignKey("content_type", "object_id")
from functools import update_wrapper
from django.contrib import admin
class Action(models.Model):
    name = models.CharField(max_length=50, primary_key=True)
    description = models.CharField(max_length=70)
class ActionAdmin(admin.ModelAdmin):
    A ModelAdmin for the Action model that changes the URL of the add_view
    to '<app name>/<model name>/!add/'
    The Action model has a CharField PK.
    list_display = ("name", "description")
    def remove_url(self, name):
        Remove all entries named 'name' from the ModelAdmin instance URL
        patterns list
        return [url for url in super().get_urls() if url.name != name]
        # Add the URL of our custom 'add_view' view to the front of the URLs
        # list. Remove the existing one(s) first
        from django.urls import re_path
        view_name = "%s_%s_add" % info
            re_path("^!add/$", wrap(self.add_view), name=view_name),
        ] + self.remove_url(view_name)
class Person(models.Model):
class PersonAdmin(admin.ModelAdmin):
        return HttpResponseRedirect(
            reverse("admin:admin_custom_urls_person_history", args=[obj.pk])
            reverse("admin:admin_custom_urls_person_delete", args=[obj.pk])
class Car(models.Model):
class CarAdmin(admin.ModelAdmin):
        return super().response_add(
            post_url_continue=reverse(
                "admin:admin_custom_urls_car_history", args=[obj.pk]
site = admin.AdminSite(name="admin_custom_urls")
site.register(Action, ActionAdmin)
site.register(Person, PersonAdmin)
site.register(Car, CarAdmin)
Models for testing various aspects of the django.contrib.admindocs app.
class Company(models.Model):
    name = models.CharField(max_length=200)
class Family(models.Model):
    Links with different link text.
    This is a line with tag :tag:`extends <built_in-extends>`
    This is a line with model :model:`Family <myapp.Family>`
    This is a line with view :view:`Index <myapp.views.Index>`
    This is a line with template :template:`index template <Index.html>`
    This is a line with filter :filter:`example filter <filtername>`
    last_name = models.CharField(max_length=200)
    Stores information about a person, related to :model:`myapp.Company`.
    **Notes**
    Use ``save_changes()`` when saving this object.
    ``company``
        Field storing :model:`myapp.Company` where the person works.
    (DESCRIPTION)
    .. raw:: html
        :file: admin_docs/evilfile.txt
    .. include:: admin_docs/evilfile.txt
    first_name = models.CharField(max_length=200, help_text="The person's first name")
    last_name = models.CharField(max_length=200, help_text="The person's last name")
    company = models.ForeignKey(Company, models.CASCADE, help_text="place of work")
    family = models.ForeignKey(Family, models.SET_NULL, related_name="+", null=True)
    groups = models.ManyToManyField(Group, help_text="has membership")
    def _get_full_name(self):
        return "%s %s" % (self.first_name, self.last_name)
    def rename_company(self, new_name):
        self.company.name = new_name
        self.company.save()
        return new_name
    def dummy_function(self, baz, rox, *some_args, **some_kwargs):
        return some_kwargs
    def dummy_function_keyword_only_arg(self, *, keyword_only_arg):
        return keyword_only_arg
    def all_kinds_arg_function(self, position_only_arg, /, arg, *, kwarg):
        return position_only_arg, arg, kwarg
    def a_property(self):
        return "a_property"
    def a_cached_property(self):
        return "a_cached_property"
    def suffix_company_name(self, suffix="ltd"):
        return self.company.name + suffix
    def add_image(self):
    def delete_image(self):
    def save_changes(self):
    def set_status(self):
        Get the full name of the person
        return self._get_full_name()
    def get_status_count(self):
    def get_groups_list(self):
from django.contrib.contenttypes.fields import GenericForeignKey, GenericRelation
    title = models.CharField(max_length=50)
    year = models.PositiveIntegerField(null=True, blank=True)
    author = models.ForeignKey(
        verbose_name="Verbose Author",
        related_name="books_authored",
    contributors = models.ManyToManyField(
        verbose_name="Verbose Contributors",
        related_name="books_contributed",
    employee = models.ForeignKey(
        "Employee",
        verbose_name="Employee",
    is_best_seller = models.BooleanField(default=0, null=True)
    date_registered = models.DateField(null=True)
    availability = models.BooleanField(
        choices=(
            (False, "Paid"),
            (True, "Free"),
            (None, "Obscure"),
    # This field name is intentionally 2 characters long (#16080).
    no = models.IntegerField(verbose_name="number", blank=True, null=True)
    CHOICES = [
        ("non-fiction", "Non-Fictional"),
        ("fiction", "Fictional"),
        (None, "Not categorized"),
        ("", "We don't know"),
    category = models.CharField(max_length=20, choices=CHOICES, blank=True, null=True)
class ImprovedBook(models.Model):
    book = models.OneToOneField(Book, models.CASCADE)
class Department(models.Model):
    code = models.CharField(max_length=4, unique=True)
    description = models.CharField(max_length=50, blank=True, null=True)
        return self.description
class Employee(models.Model):
    department = models.ForeignKey(Department, models.CASCADE, to_field="code")
class TaggedItem(models.Model):
    tag = models.SlugField()
        ContentType, models.CASCADE, related_name="tagged_items"
        return self.tag
class Bookmark(models.Model):
    url = models.URLField()
    tags = GenericRelation(TaggedItem)
        ("a", "A"),
        (None, "None"),
        ("", "-"),
    none_or_null = models.CharField(
        max_length=20, choices=CHOICES, blank=True, null=True
Testing of admin inline formsets.
    name = models.CharField(max_length=50)
class Teacher(models.Model):
    teacher = models.ForeignKey(Teacher, models.CASCADE)
    parent = GenericForeignKey()
        return "I am %s, a child of %s" % (self.name, self.parent)
    books = models.ManyToManyField(Book)
    person = models.OneToOneField("Person", models.CASCADE, null=True)
class NonAutoPKBook(models.Model):
    rand_pk = models.IntegerField(primary_key=True, editable=False)
    def save(self, *args, **kwargs):
        while not self.rand_pk:
            test_pk = random.randint(1, 99999)
            if not NonAutoPKBook.objects.filter(rand_pk=test_pk).exists():
                self.rand_pk = test_pk
        super().save(*args, **kwargs)
class NonAutoPKBookChild(NonAutoPKBook):
class EditablePKBook(models.Model):
    manual_pk = models.IntegerField(primary_key=True)
class Holder(models.Model):
    dummy = models.IntegerField()
class Inner(models.Model):
    holder = models.ForeignKey(Holder, models.CASCADE)
    readonly = models.CharField("Inner readonly label", max_length=1)
        return "/inner/"
class Holder2(models.Model):
class Inner2(models.Model):
    holder = models.ForeignKey(Holder2, models.CASCADE)
class Holder3(models.Model):
class Inner3(models.Model):
    holder = models.ForeignKey(Holder3, models.CASCADE)
# Models for ticket #8190
class Holder4(models.Model):
class Inner4Stacked(models.Model):
    dummy = models.IntegerField(help_text="Awesome stacked help text is awesome.")
    holder = models.ForeignKey(Holder4, models.CASCADE)
        constraints = [
            models.UniqueConstraint(
                fields=["dummy", "holder"], name="unique_stacked_dummy_per_holder"
class Inner4Tabular(models.Model):
    dummy = models.IntegerField(help_text="Awesome tabular help text is awesome.")
                fields=["dummy", "holder"], name="unique_tabular_dummy_per_holder"
# Models for ticket #31441
class Holder5(models.Model):
class Inner5Stacked(models.Model):
    name = models.CharField(max_length=10)
    select = models.CharField(choices=(("1", "One"), ("2", "Two")), max_length=10)
    text = models.TextField()
    holder = models.ForeignKey(Holder5, models.CASCADE)
class Inner5Tabular(models.Model):
# Models for #12749
    firstname = models.CharField(max_length=15)
class OutfitItem(models.Model):
class Fashionista(models.Model):
    person = models.OneToOneField(Person, models.CASCADE, primary_key=True)
    weaknesses = models.ManyToManyField(
        OutfitItem, through="ShoppingWeakness", blank=True
class ShoppingWeakness(models.Model):
    fashionista = models.ForeignKey(Fashionista, models.CASCADE)
    item = models.ForeignKey(OutfitItem, models.CASCADE)
# Models for #35189
class Photographer(Person):
    fullname = models.CharField(max_length=100)
    nationality = models.CharField(max_length=100)
    residency = models.CharField(max_length=100)
    siblings = models.IntegerField()
    children = models.IntegerField()
class Photo(models.Model):
    photographer = models.ForeignKey(Photographer, on_delete=models.CASCADE)
    image = models.CharField(max_length=100)
    title = models.CharField(max_length=100)
    description = models.TextField()
    creation_date = models.DateField()
    update_date = models.DateField()
    updated_by = models.CharField(max_length=100)
# Models for #13510
class TitleCollection(models.Model):
class Title(models.Model):
    collection = models.ForeignKey(
        TitleCollection, models.SET_NULL, blank=True, null=True
    title1 = models.CharField(max_length=100)
    title2 = models.CharField(max_length=100)
# Models for #15424
class Poll(models.Model):
    name = models.CharField(max_length=40)
class Question(models.Model):
    text = models.CharField(max_length=40)
    poll = models.ForeignKey(Poll, models.CASCADE)
        raise ValidationError("Always invalid model.")
class Novel(models.Model):
class NovelReadonlyChapter(Novel):
class Chapter(models.Model):
    novel = models.ForeignKey(Novel, models.CASCADE)
class FootNote(models.Model):
    Model added for ticket 19838
    chapter = models.ForeignKey(Chapter, models.PROTECT)
    note = models.CharField(max_length=40)
# Models for #16838
class CapoFamiglia(models.Model):
class Consigliere(models.Model):
    name = models.CharField(max_length=100, help_text="Help text for Consigliere")
    capo_famiglia = models.ForeignKey(CapoFamiglia, models.CASCADE, related_name="+")
class SottoCapo(models.Model):
class ReadOnlyInline(models.Model):
    name = models.CharField(max_length=100, help_text="Help text for ReadOnlyInline")
    capo_famiglia = models.ForeignKey(CapoFamiglia, models.CASCADE)
# Models for #18433
class ParentModelWithCustomPk(models.Model):
    my_own_pk = models.CharField(max_length=100, primary_key=True)
class ChildModel1(models.Model):
    parent = models.ForeignKey(ParentModelWithCustomPk, models.CASCADE)
        return "/child_model1/"
class ChildModel2(models.Model):
        return "/child_model2/"
# Models for #19425
class BinaryTree(models.Model):
    parent = models.ForeignKey("self", models.SET_NULL, null=True, blank=True)
# Models for #19524
class LifeForm(models.Model):
class ExtraTerrestrial(LifeForm):
class Sighting(models.Model):
    et = models.ForeignKey(ExtraTerrestrial, models.CASCADE)
    place = models.CharField(max_length=100)
        return self.place
# Models for #18263
class SomeParentModel(models.Model):
    name = models.CharField(max_length=1)
class SomeChildModel(models.Model):
    position = models.PositiveIntegerField(help_text="Position help_text.")
    parent = models.ForeignKey(SomeParentModel, models.CASCADE)
    readonly_field = models.CharField(max_length=1)
# Models for #30231
class Course(models.Model):
class Class(models.Model):
    person = models.ManyToManyField(Person, verbose_name="attendant")
    course = models.ForeignKey(Course, on_delete=models.CASCADE)
class CourseProxy(Course):
class CourseProxy1(Course):
class CourseProxy2(Course):
# Other models
class ShowInlineParent(models.Model):
    show_inlines = models.BooleanField(default=False)
class ShowInlineChild(models.Model):
    parent = models.ForeignKey(ShowInlineParent, on_delete=models.CASCADE)
class ProfileCollection(models.Model):
class Profile(models.Model):
        ProfileCollection, models.SET_NULL, blank=True, null=True
    first_name = models.CharField(max_length=100)
    last_name = models.CharField(max_length=100)
class VerboseNameProfile(Profile):
        verbose_name = "Model with verbose name only"
class VerboseNamePluralProfile(Profile):
        verbose_name_plural = "Model with verbose name plural only"
class BothVerboseNameProfile(Profile):
        verbose_name = "Model with both - name"
        verbose_name_plural = "Model with both - plural name"
class UUIDParent(models.Model):
class UUIDChild(models.Model):
    id = models.UUIDField(default=uuid.uuid4, primary_key=True)
    title = models.CharField(max_length=128)
    parent = models.ForeignKey(UUIDParent, on_delete=models.CASCADE)
    bio = models.TextField()
    rank = models.IntegerField()
        ordering = ("name",)
    band = models.ForeignKey(Band, models.CASCADE)
    duration = models.IntegerField()
    other_interpreters = models.ManyToManyField(Band, related_name="covers")
class SongInlineDefaultOrdering(admin.StackedInline):
    model = Song
class SongInlineNewOrdering(admin.StackedInline):
    ordering = ("duration",)
class DynOrderingBandAdmin(admin.ModelAdmin):
        if request.user.is_superuser:
            return ["rank"]
            return ["name"]
Tests for various ways of registering models with the admin site.
class Traveler(Person):
class Location(models.Model):
class Place(Location):
class Guest(models.Model):
    pk = models.CompositePrimaryKey("traveler", "place")
    traveler = models.ForeignKey(Traveler, on_delete=models.CASCADE)
    place = models.ForeignKey(Place, on_delete=models.CASCADE)
class Foo(models.Model):
        app_label = "another_app_waiting_migration"
class ModelRaisingMessages(models.Model):
            checks.Warning("First warning", hint="Hint", obj="obj"),
            checks.Warning("Second warning", obj="a"),
            checks.Error("An error", hint="Error hint"),
        return [checks.Warning("A warning")]
class Bar(models.Model):
        app_label = "app_waiting_migration"
# Regression for #13368. This is an example of a model
# that imports a class that has an abstract base class.
class UserProfile(models.Model):
    user = models.OneToOneField(User, models.CASCADE, primary_key=True)
from django.db import modelz  # NOQA
from ..complex_app.models.bar import Bar
__all__ = ["Bar"]
    domain = models.CharField(max_length=100)
class Article(models.Model):
    A simple Article model for testing
    site = models.ForeignKey(Site, models.CASCADE, related_name="admin_articles")
    hist = models.CharField(
        verbose_name=_("History"),
        help_text=_("History help text"),
    created = models.DateTimeField(null=True)
    def test_from_model(self):
        return "nothing"
    @admin.display(description="not What you Expect")
    def test_from_model_with_override(self):
class ArticleProxy(Article):
class Cascade(models.Model):
    num = models.PositiveSmallIntegerField()
    parent = models.ForeignKey("self", models.CASCADE, null=True)
        return str(self.num)
class DBCascade(models.Model):
    parent = models.ForeignKey("self", models.DB_CASCADE, null=True)
        required_db_features = {"supports_on_delete_db_cascade"}
    date = models.DateTimeField(auto_now_add=True)
    event = models.OneToOneField(Event, models.CASCADE, verbose_name="awesome event")
    event = models.OneToOneField(Event, models.CASCADE)
        verbose_name = "awesome guest"
class EventGuide(models.Model):
    event = models.ForeignKey(Event, models.DO_NOTHING)
class Vehicle(models.Model):
class VehicleMixin(Vehicle):
    vehicle = models.OneToOneField(
        Vehicle,
        related_name="vehicle_%(app_label)s_%(class)s",
class Car(VehicleMixin):
from django.core.files.storage import FileSystemStorage
class Section(models.Model):
    A simple section that links to articles, to test linking to related items
    in admin views.
    def name_property(self):
        A property that simply returns the name. Used to test #24461
    A simple article to test admin views. Test backwards compatibility.
    content = models.TextField()
    date = models.DateTimeField()
    section = models.ForeignKey(Section, models.CASCADE, null=True, blank=True)
    another_section = models.ForeignKey(
        Section, models.CASCADE, null=True, blank=True, related_name="+"
    sub_section = models.ForeignKey(
        Section, models.SET_NULL, null=True, blank=True, related_name="+"
    @admin.display(ordering="date", description="")
    def model_year(self):
        return self.date.year
    @admin.display(ordering="-date", description="")
    def model_year_reversed(self):
    @admin.display(ordering="date")
    def model_property_year(self):
    def model_month(self):
        return self.date.month
    @admin.display(description="Is from past?", boolean=True)
    def model_property_is_from_past(self):
        return self.date < timezone.now()
    A simple book that has chapters.
    name = models.CharField(max_length=100, verbose_name="¿Name?")
        return f"/books/{self.id}/"
class Promo(models.Model):
    author = models.ForeignKey(User, models.SET_NULL, blank=True, null=True)
    title = models.CharField(max_length=100, verbose_name="¿Title?")
        # Use a utf-8 bytestring to ensure it works (see #11710)
        verbose_name = "¿Chapter?"
class ChapterXtra1(models.Model):
    chap = models.OneToOneField(Chapter, models.CASCADE, verbose_name="¿Chap?")
    xtra = models.CharField(max_length=100, verbose_name="¿Xtra?")
    guest_author = models.ForeignKey(User, models.SET_NULL, blank=True, null=True)
        return "¿Xtra1: %s" % self.xtra
class ChapterXtra2(models.Model):
        return "¿Xtra2: %s" % self.xtra
class RowLevelChangePermissionModel(models.Model):
    name = models.CharField(max_length=100, blank=True)
class CustomArticle(models.Model):
class ModelWithStringPrimaryKey(models.Model):
    string_pk = models.CharField(max_length=255, primary_key=True)
        return self.string_pk
        return "/dummy/%s/" % self.string_pk
class Color(models.Model):
    value = models.CharField(max_length=10)
    warm = models.BooleanField(default=False)
        return self.value
# we replicate Color to register with another ModelAdmin
class Color2(Color):
class Thing(models.Model):
    title = models.CharField(max_length=20)
    color = models.ForeignKey(Color, models.CASCADE, limit_choices_to={"warm": True})
    pub_date = models.DateField(blank=True, null=True)
class Actor(models.Model):
    age = models.IntegerField()
    title = models.CharField(max_length=50, null=True, blank=True)
class Inquisition(models.Model):
    expected = models.BooleanField(default=False)
    leader = models.ForeignKey(Actor, models.CASCADE)
    country = models.CharField(max_length=20)
        return "by %s from %s" % (self.leader, self.country)
class Sketch(models.Model):
    inquisition = models.ForeignKey(
        Inquisition,
        limit_choices_to={
            "leader__name": "Palin",
            "leader__age": 27,
            "expected": False,
    defendant0 = models.ForeignKey(
        Actor,
        limit_choices_to={"title__isnull": False},
        related_name="as_defendant0",
    defendant1 = models.ForeignKey(
        limit_choices_to={"title__isnull": True},
        related_name="as_defendant1",
def today_callable_dict():
    return {"last_action__gte": datetime.datetime.today()}
def today_callable_q():
    return models.Q(last_action__gte=datetime.datetime.today())
class Character(models.Model):
    username = models.CharField(max_length=100)
    last_action = models.DateTimeField()
class StumpJoke(models.Model):
    variation = models.CharField(max_length=100)
    most_recently_fooled = models.ForeignKey(
        Character,
        limit_choices_to=today_callable_dict,
        related_name="+",
    has_fooled_today = models.ManyToManyField(
        Character, limit_choices_to=today_callable_q, related_name="+"
        return self.variation
class Fabric(models.Model):
    NG_CHOICES = (
            "Textured",
                ("x", "Horizontal"),
                ("y", "Vertical"),
        ("plain", "Smooth"),
    surface = models.CharField(max_length=20, choices=NG_CHOICES)
    GENDER_CHOICES = (
        (1, "Male"),
        (2, "Female"),
    gender = models.IntegerField(
        choices=GENDER_CHOICES,
        verbose_name=(
            "very very very very very very very very very "
            "loooooooooooooooooooooooooooooooooooooooooong name"
    age = models.IntegerField(default=21)
    alive = models.BooleanField(default=True)
class Persona(models.Model):
    A simple persona associated with accounts, to test inlining of related
    accounts which inherit from a common accounts class.
    name = models.CharField(blank=False, max_length=80)
class Account(models.Model):
    A simple, generic account encapsulating the information shared by all
    types of accounts.
    username = models.CharField(blank=False, max_length=80)
    persona = models.ForeignKey(Persona, models.CASCADE, related_name="accounts")
    servicename = "generic service"
        return "%s: %s" % (self.servicename, self.username)
class FooAccount(Account):
    """A service-specific account of type Foo."""
    servicename = "foo"
class BarAccount(Account):
    """A service-specific account of type Bar."""
    servicename = "bar"
class Subscriber(models.Model):
    email = models.EmailField(blank=False, max_length=175)
        return "%s (%s)" % (self.name, self.email)
class ExternalSubscriber(Subscriber):
class OldSubscriber(Subscriber):
class Media(models.Model):
    name = models.CharField(max_length=60)
class Podcast(Media):
    release_date = models.DateField()
        ordering = ("release_date",)  # overridden in PodcastAdmin
class Vodcast(Media):
    media = models.OneToOneField(
        Media, models.CASCADE, primary_key=True, parent_link=True
    released = models.BooleanField(default=False)
        if self.name == "_invalid":
            raise ValidationError("invalid")
    parent = models.ForeignKey(Parent, models.CASCADE, editable=False)
class PKChild(models.Model):
    Used to check autocomplete to_field resolution when ForeignKey is PK.
    parent = models.ForeignKey(Parent, models.CASCADE, primary_key=True)
        ordering = ["parent"]
class Toy(models.Model):
    child = models.ForeignKey(PKChild, models.CASCADE)
class EmptyModel(models.Model):
        return "Primary key = %s" % self.id
temp_storage = FileSystemStorage(tempfile.mkdtemp())
class Gallery(models.Model):
class Picture(models.Model):
    image = models.FileField(storage=temp_storage, upload_to="test_upload")
    gallery = models.ForeignKey(Gallery, models.CASCADE, related_name="pictures")
class Language(models.Model):
    iso = models.CharField(max_length=5, primary_key=True, help_text="iso helptext")
    name = models.CharField(max_length=50, help_text="name helptext")
    english_name = models.CharField(max_length=50)
    shortlist = models.BooleanField(default=False)
        return self.iso
        ordering = ("iso",)
# a base class for Recommender and Recommendation
class TitleTranslation(models.Model):
    title = models.ForeignKey(Title, models.CASCADE)
    text = models.CharField(max_length=100)
class Recommender(Title):
class Recommendation(Title):
    the_recommender = models.ForeignKey(Recommender, models.CASCADE)
class Collector(models.Model):
class Widget(models.Model):
    owner = models.ForeignKey(Collector, models.CASCADE)
class DooHickey(models.Model):
    code = models.CharField(max_length=10, primary_key=True)
class Grommet(models.Model):
    code = models.AutoField(primary_key=True)
class Whatsit(models.Model):
    index = models.IntegerField(primary_key=True)
class Doodad(models.Model):
class FancyDoodad(Doodad):
    expensive = models.BooleanField(default=True)
class Category(models.Model):
    collector = models.ForeignKey(Collector, models.CASCADE)
    order = models.PositiveIntegerField()
        ordering = ("order",)
        return "%s:o%s" % (self.id, self.order)
def link_posted_default():
    return datetime.date.today() - datetime.timedelta(days=7)
class Link(models.Model):
    posted = models.DateField(default=link_posted_default)
    post = models.ForeignKey("Post", models.CASCADE)
    readonly_link_content = models.TextField()
class PrePopulatedPost(models.Model):
    published = models.BooleanField(default=False)
    slug = models.SlugField()
class PrePopulatedSubPost(models.Model):
    post = models.ForeignKey(PrePopulatedPost, models.CASCADE)
    subslug = models.SlugField()
class Post(models.Model):
    title = models.CharField(
        max_length=100, help_text="Some help text for the title (with Unicode ŠĐĆŽćžšđ)"
    content = models.TextField(
        help_text="Some help text for the content (with Unicode ŠĐĆŽćžšđ)"
    readonly_content = models.TextField()
    posted = models.DateField(
        default=datetime.date.today,
        help_text="Some help text for the date (with Unicode ŠĐĆŽćžšđ)",
    public = models.BooleanField(null=True, blank=True)
    def awesomeness_level(self):
        return "Very awesome."
# Proxy model to test overridden fields attrs on Post model so as not to
# interfere with other tests.
class FieldOverridePost(Post):
class Gadget(models.Model):
class Villain(models.Model):
class SuperVillain(Villain):
class FunkyTag(models.Model):
    "Because we all know there's only one real use case for GFKs."
    name = models.CharField(max_length=25)
class Plot(models.Model):
    team_leader = models.ForeignKey(Villain, models.CASCADE, related_name="lead_plots")
    contact = models.ForeignKey(Villain, models.CASCADE, related_name="contact_plots")
    tags = GenericRelation(FunkyTag)
class PlotDetails(models.Model):
    details = models.CharField(max_length=100)
    plot = models.OneToOneField(Plot, models.CASCADE, null=True, blank=True)
        return self.details
class PlotProxy(Plot):
class SecretHideout(models.Model):
    """Secret! Not registered with the admin!"""
    location = models.CharField(max_length=100)
    villain = models.ForeignKey(Villain, models.CASCADE)
        return self.location
class SuperSecretHideout(models.Model):
    supervillain = models.ForeignKey(SuperVillain, models.CASCADE)
    tag = GenericRelation(FunkyTag, related_query_name="bookmark")
class CyclicOne(models.Model):
    two = models.ForeignKey("CyclicTwo", models.CASCADE)
class CyclicTwo(models.Model):
    one = models.ForeignKey(CyclicOne, models.CASCADE)
    DIFFICULTY_CHOICES = [
        ("beginner", "Beginner Class"),
        ("intermediate", "Intermediate Class"),
        ("advanced", "Advanced Class"),
    materials = models.FileField(upload_to="test_upload")
    difficulty = models.CharField(
        max_length=20, choices=DIFFICULTY_CHOICES, null=True, blank=True
    categories = models.ManyToManyField(Category, blank=True)
    start_datetime = models.DateTimeField(null=True, blank=True)
class Topping(models.Model):
class Pizza(models.Model):
    toppings = models.ManyToManyField("Topping", related_name="pizzas")
# Pizza's ModelAdmin has readonly_fields = ['toppings'].
# toppings is editable for this model's admin.
class ReadablePizza(Pizza):
# No default permissions are created for this model and both name and toppings
# are readonly for this model's admin.
class ReadOnlyPizza(Pizza):
        default_permissions = ()
    owner = models.ForeignKey(User, models.SET_NULL, null=True, blank=True)
    title = models.CharField(max_length=30)
    album = models.ForeignKey(Album, on_delete=models.RESTRICT)
class Employee(Person):
    code = models.CharField(max_length=20)
        ordering = ["name"]
class WorkHour(models.Model):
    datum = models.DateField()
    employee = models.ForeignKey(Employee, models.CASCADE)
class Manager(Employee):
    A multi-layer MTI child.
class Bonus(models.Model):
    recipient = models.ForeignKey(Manager, on_delete=models.CASCADE)
    big_id = models.BigAutoField(primary_key=True)
    question = models.CharField(max_length=20)
    posted = models.DateField(default=datetime.date.today)
    expires = models.DateTimeField(null=True, blank=True)
    related_questions = models.ManyToManyField("self")
    uuid = models.UUIDField(default=uuid.uuid4, unique=True)
        return self.question
class Answer(models.Model):
    question = models.ForeignKey(Question, models.PROTECT)
    question_with_to_field = models.ForeignKey(
        Question,
        to_field="uuid",
        related_name="uuid_answers",
        limit_choices_to=~models.Q(question__istartswith="not"),
    related_answers = models.ManyToManyField("self")
    answer = models.CharField(max_length=20)
        return self.answer
class Answer2(Answer):
class Reservation(models.Model):
    start_date = models.DateTimeField()
    price = models.IntegerField()
class FoodDelivery(models.Model):
    DRIVER_CHOICES = (
        ("bill", "Bill G"),
        ("steve", "Steve J"),
    RESTAURANT_CHOICES = (
        ("indian", "A Taste of India"),
        ("thai", "Thai Pography"),
        ("pizza", "Pizza Mama"),
    reference = models.CharField(max_length=100)
    driver = models.CharField(max_length=100, choices=DRIVER_CHOICES, blank=True)
    restaurant = models.CharField(
        max_length=100, choices=RESTAURANT_CHOICES, blank=True
        unique_together = (("driver", "restaurant"),)
class CoverLetter(models.Model):
    author = models.CharField(max_length=30)
    date_written = models.DateField(null=True, blank=True)
        return self.author
class Paper(models.Model):
    author = models.CharField(max_length=30, blank=True, null=True)
class ShortMessage(models.Model):
    content = models.CharField(max_length=140)
    timestamp = models.DateTimeField(null=True, blank=True)
class Telegram(models.Model):
    date_sent = models.DateField(null=True, blank=True)
class OtherStory(models.Model):
class ComplexSortedPerson(models.Model):
    age = models.PositiveIntegerField()
    is_employee = models.BooleanField(null=True)
class PluggableSearchPerson(models.Model):
class PrePopulatedPostLargeSlug(models.Model):
    Regression test for #15938: a large max_length for the slugfield must not
    be localized in prepopulated_fields_js.html or it might end up breaking
    the JavaScript (ie, using THOUSAND_SEPARATOR ends up with maxLength=1,000)
    # `db_index=False` because MySQL cannot index large CharField (#21196).
    slug = models.SlugField(max_length=1000, db_index=False)
class AdminOrderedField(models.Model):
    order = models.IntegerField()
    stuff = models.CharField(max_length=200)
class AdminOrderedModelMethod(models.Model):
    @admin.display(ordering="order")
    def some_order(self):
        return self.order
class AdminOrderedAdminMethod(models.Model):
class AdminOrderedCallable(models.Model):
class Report(models.Model):
class MainPrepopulated(models.Model):
    pubdate = models.DateField()
    status = models.CharField(
        max_length=20,
        choices=(("option one", "Option One"), ("option two", "Option Two")),
    slug1 = models.SlugField(blank=True)
    slug2 = models.SlugField(blank=True)
    slug3 = models.SlugField(blank=True, allow_unicode=True)
class RelatedPrepopulated(models.Model):
    parent = models.ForeignKey(MainPrepopulated, models.CASCADE)
    name = models.CharField(max_length=75)
    fk = models.ForeignKey("self", models.CASCADE, blank=True, null=True)
    m2m = models.ManyToManyField("self", blank=True)
    slug1 = models.SlugField(max_length=50)
    slug2 = models.SlugField(max_length=60)
    Refs #16819.
class UndeletableObject(models.Model):
    Model whose show_delete in admin change_view has been disabled
    Refs #10057.
class UnchangeableObject(models.Model):
    Model whose change_view is disabled in admin
    Refs #20640.
class UserMessenger(models.Model):
    Dummy class for testing message_user functions on ModelAdmin
class Simple(models.Model):
    Simple model with nothing on it for use in testing
class Choice(models.Model):
    choice = models.IntegerField(
        choices=((1, "Yes"), (0, "No"), (None, "No opinion")),
class ParentWithDependentChildren(models.Model):
    Issue #20522
    Model where the validation of child foreign-key relationships depends
    on validation of the parent
    some_required_info = models.PositiveIntegerField()
    family_name = models.CharField(max_length=255, blank=False)
class DependentChild(models.Model):
    Model that depends on validation of the parent class for one of its
    fields to validate during clean
    parent = models.ForeignKey(ParentWithDependentChildren, models.CASCADE)
    family_name = models.CharField(max_length=255)
class _Manager(models.Manager):
        return super().get_queryset().filter(pk__gt=1)
class FilteredManager(models.Model):
        return "PK=%s" % self.pk
    pk_gt_1 = _Manager()
    objects = models.Manager()
class EmptyModelVisible(models.Model):
    """See ticket #11277."""
class EmptyModelHidden(models.Model):
class EmptyModelMixin(models.Model):
    name = models.CharField(max_length=100, verbose_name="State verbose_name")
    name = models.CharField(max_length=100, verbose_name="City verbose_name")
        return "/dummy/%s/" % self.pk
class Restaurant(models.Model):
    city = models.ForeignKey(City, models.CASCADE)
        verbose_name = (
class Worker(models.Model):
    work_at = models.ForeignKey(Restaurant, models.CASCADE)
    surname = models.CharField(max_length=50)
# Models for #23329
class ReferencedByParent(models.Model):
    name = models.CharField(max_length=20, unique=True)
class ParentWithFK(models.Model):
    fk = models.ForeignKey(
        ReferencedByParent,
        to_field="name",
        related_name="hidden+",
class ChildOfReferer(ParentWithFK):
# Models for #23431
class InlineReferer(models.Model):
class ReferencedByInline(models.Model):
class InlineReference(models.Model):
    referer = models.ForeignKey(InlineReferer, models.CASCADE)
        ReferencedByInline,
class Recipe(models.Model):
    rname = models.CharField(max_length=20, unique=True)
class Ingredient(models.Model):
    iname = models.CharField(max_length=20, unique=True)
    recipes = models.ManyToManyField(Recipe, through="RecipeIngredient")
class RecipeIngredient(models.Model):
    ingredient = models.ForeignKey(Ingredient, models.CASCADE, to_field="iname")
    recipe = models.ForeignKey(Recipe, models.CASCADE, to_field="rname")
# Model for #23839
class NotReferenced(models.Model):
    # Don't point any FK at this model.
# Models for #23934
class ExplicitlyProvidedPK(models.Model):
    name = models.IntegerField(primary_key=True)
class ImplicitlyGeneratedPK(models.Model):
    name = models.IntegerField(unique=True)
# Models for #25622
class ReferencedByGenRel(models.Model):
    content_type = models.ForeignKey(ContentType, on_delete=models.CASCADE)
class GenRelReference(models.Model):
    references = GenericRelation(ReferencedByGenRel)
class ParentWithUUIDPK(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
        return str(self.id)
class RelatedWithUUIDPKModel(models.Model):
    parent = models.ForeignKey(
        ParentWithUUIDPK, on_delete=models.SET_NULL, null=True, blank=True
class Authorship(models.Model):
class UserProxy(User):
    """Proxy a model with a different app_label."""
class ReadOnlyRelatedField(models.Model):
    chapter = models.ForeignKey(Chapter, models.CASCADE)
    language = models.ForeignKey(Language, models.CASCADE)
    user = models.ForeignKey(User, models.CASCADE)
class Héllo(models.Model):
class Box(models.Model):
    next_box = models.ForeignKey(
        "self", null=True, on_delete=models.SET_NULL, blank=True
class Country(models.Model):
    NORTH_AMERICA = "North America"
    SOUTH_AMERICA = "South America"
    EUROPE = "Europe"
    ASIA = "Asia"
    OCEANIA = "Oceania"
    ANTARCTICA = "Antarctica"
    CONTINENT_CHOICES = [
        (NORTH_AMERICA, NORTH_AMERICA),
        (SOUTH_AMERICA, SOUTH_AMERICA),
        (EUROPE, EUROPE),
        (ASIA, ASIA),
        (OCEANIA, OCEANIA),
        (ANTARCTICA, ANTARCTICA),
    name = models.CharField(max_length=80)
    continent = models.CharField(max_length=13, choices=CONTINENT_CHOICES)
class Traveler(models.Model):
    born_country = models.ForeignKey(Country, models.CASCADE)
    living_country = models.ForeignKey(
        Country, models.CASCADE, related_name="living_country_set"
    favorite_country_to_vacation = models.ForeignKey(
        Country,
        related_name="favorite_country_to_vacation_set",
        limit_choices_to={"continent": Country.ASIA},
class Square(models.Model):
    side = models.IntegerField()
    area = models.GeneratedField(
        db_persist=True,
        expression=models.F("side") * models.F("side"),
        output_field=models.BigIntegerField(),
        required_db_features = {"supports_stored_generated_columns"}
class CamelCaseModel(models.Model):
    interesting_name = models.CharField(max_length=100)
        return self.interesting_name
class CamelCaseRelatedModel(models.Model):
    m2m = models.ManyToManyField(CamelCaseModel, related_name="m2m")
    fk = models.ForeignKey(CamelCaseModel, on_delete=models.CASCADE, related_name="fk")
    # Add another relation that will not participate in filter_horizontal.
    fk2 = models.ForeignKey(
        CamelCaseModel, on_delete=models.CASCADE, related_name="fk2"
    Image = None
    temp_storage_dir = tempfile.mkdtemp()
    temp_storage = FileSystemStorage(temp_storage_dir)
class MyFileField(models.FileField):
class Member(models.Model):
    birthdate = models.DateTimeField(blank=True, null=True)
    gender = models.CharField(
        max_length=1, blank=True, choices=[("M", "Male"), ("F", "Female")]
    email = models.EmailField(blank=True)
class Artist(models.Model):
class Band(Artist):
    uuid = models.UUIDField(unique=True, default=uuid.uuid4)
    style = models.CharField(max_length=20)
    members = models.ManyToManyField(Member)
class UnsafeLimitChoicesTo(models.Model):
    band = models.ForeignKey(
        Band,
        limit_choices_to={"name": '"&><escapeme'},
    band = models.ForeignKey(Band, models.CASCADE, to_field="uuid")
    featuring = models.ManyToManyField(Band, related_name="featured")
    cover_art = models.FileField(upload_to="albums")
    backside_art = MyFileField(upload_to="albums_back", null=True)
class ReleaseEvent(models.Model):
    Used to check that autocomplete widget correctly resolves attname for FK as
    PK example.
    album = models.ForeignKey(Album, models.CASCADE, primary_key=True)
class VideoStream(models.Model):
    release_event = models.ForeignKey(ReleaseEvent, models.CASCADE)
class HiddenInventoryManager(models.Manager):
        return super().get_queryset().filter(hidden=False)
class Inventory(models.Model):
    barcode = models.PositiveIntegerField(unique=True)
        "self", models.SET_NULL, to_field="barcode", blank=True, null=True
    name = models.CharField(blank=False, max_length=20)
    hidden = models.BooleanField(default=False)
    # see #9258
    default_manager = models.Manager()
    objects = HiddenInventoryManager()
    main_band = models.ForeignKey(
        limit_choices_to=models.Q(pk__gt=0),
        related_name="events_main_band_at",
    supporting_bands = models.ManyToManyField(
        related_name="events_supporting_band_at",
        help_text="Supporting Bands.",
    start_date = models.DateField(blank=True, null=True)
    start_time = models.TimeField(blank=True, null=True)
    description = models.TextField(blank=True)
    link = models.URLField(blank=True)
    min_age = models.IntegerField(blank=True, null=True)
    owner = models.ForeignKey(User, models.CASCADE)
    make = models.CharField(max_length=30)
    model = models.CharField(max_length=30)
        return "%s %s" % (self.make, self.model)
class CarTire(models.Model):
    A single car tire. This to test that a user can only select their own cars.
    car = models.ForeignKey(Car, models.CASCADE)
class Honeycomb(models.Model):
    location = models.CharField(max_length=20)
class Bee(models.Model):
    A model with a FK to a model that won't be registered with the admin
    (Honeycomb) so the corresponding raw ID widget won't have a magnifying
    glass link to select related honeycomb instances.
    honeycomb = models.ForeignKey(Honeycomb, models.CASCADE)
class Individual(models.Model):
    A model with a FK to itself. It won't be registered with the admin, so the
    corresponding raw ID widget won't have a magnifying glass link to select
    related instances (rendering will be called programmatically in this case).
    parent = models.ForeignKey("self", models.SET_NULL, null=True)
    soulmate = models.ForeignKey(
        "self", models.CASCADE, null=True, related_name="soulmates"
class Advisor(models.Model):
    A model with a m2m to a model that won't be registered with the admin
    (Company) so the corresponding raw ID widget won't have a magnifying
    glass link to select related company instances.
    companies = models.ManyToManyField(Company)
class Student(models.Model):
    if Image:
        photo = models.ImageField(
            storage=temp_storage, upload_to="photos", blank=True, null=True
class School(models.Model):
    students = models.ManyToManyField(Student, related_name="current_schools")
    alumni = models.ManyToManyField(Student, related_name="previous_schools")
    user = models.ForeignKey("auth.User", models.CASCADE, to_field="username")
        return self.user.username
    friends = models.ManyToManyField("self", blank=True)
    rating = models.FloatField(null=True)
class Publisher(models.Model):
    num_awards = models.IntegerField()
    duration = models.DurationField(blank=True, null=True)
    isbn = models.CharField(max_length=9)
    pages = models.IntegerField()
    rating = models.FloatField()
    price = models.DecimalField(decimal_places=2, max_digits=6)
    authors = models.ManyToManyField(Author)
    contact = models.ForeignKey(Author, models.CASCADE, related_name="book_contact_set")
    publisher = models.ForeignKey(Publisher, models.CASCADE)
class Store(models.Model):
    original_opening = models.DateTimeField()
    friday_night_closing = models.TimeField()
    work_day_preferences = models.JSONField()
        required_db_features = {"supports_json_field"}
class ItemTag(models.Model):
    tag = models.CharField(max_length=100)
    tags = GenericRelation(ItemTag)
class Entries(models.Model):
    EntryID = models.AutoField(primary_key=True, db_column="Entry ID")
    Entry = models.CharField(unique=True, max_length=50)
    Exclude = models.BooleanField(default=False)
class Clues(models.Model):
    ID = models.AutoField(primary_key=True)
    EntryID = models.ForeignKey(
        Entries, models.CASCADE, verbose_name="Entry", db_column="Entry ID"
    Clue = models.CharField(max_length=150)
class WithManualPK(models.Model):
    # The generic relations regression test needs two different model
    # classes with the same PK value, and there are some (external)
    # DB backends that don't work nicely when assigning integer to AutoField
    # column (MSSQL at least).
    id = models.IntegerField(primary_key=True)
class HardbackBook(Book):
    weight = models.FloatField()
# Models for ticket #21150
class Alfa(models.Model):
    name = models.CharField(max_length=10, null=True)
class Bravo(models.Model):
class Charlie(models.Model):
    alfa = models.ForeignKey(Alfa, models.SET_NULL, null=True)
    bravo = models.ForeignKey(Bravo, models.SET_NULL, null=True)
class SelfRefFK(models.Model):
        "self", models.SET_NULL, null=True, blank=True, related_name="children"
class AuthorProxy(Author):
    author = models.ForeignKey(AuthorProxy, models.CASCADE)
    tasters = models.ManyToManyField(AuthorProxy, related_name="recipes")
class RecipeProxy(Recipe):
class AuthorUnmanaged(models.Model):
        db_table = Author._meta.db_table
class RecipeTasterUnmanaged(models.Model):
    recipe = models.ForeignKey("RecipeUnmanaged", models.CASCADE)
        AuthorUnmanaged, models.CASCADE, db_column="authorproxy_id"
        db_table = Recipe.tasters.through._meta.db_table
class RecipeUnmanaged(models.Model):
    author = models.ForeignKey(AuthorUnmanaged, models.CASCADE)
    tasters = models.ManyToManyField(
        AuthorUnmanaged, through=RecipeTasterUnmanaged, related_name="+"
        db_table = Recipe._meta.db_table
    area = models.IntegerField(null=True, db_column="surface")
class DepartmentStore(Store):
    chain = models.CharField(max_length=255)
    # The order of these fields matter, do not change. Certain backends
    # rely on field ordering to perform database conversions, and this
    # model helps to test that.
    first_name = models.CharField(max_length=20)
    manager = models.BooleanField(default=False)
    last_name = models.CharField(max_length=20)
    store = models.ForeignKey(Store, models.CASCADE)
    salary = models.DecimalField(max_digits=8, decimal_places=2)
    motto = models.CharField(max_length=200, null=True, blank=True)
    ticker_name = models.CharField(max_length=10, null=True, blank=True)
    description = models.CharField(max_length=200, null=True, blank=True)
class Ticket(models.Model):
    active_at = models.DateTimeField()
    duration = models.DurationField()
class JsonModel(models.Model):
    data = models.JSONField(default=dict, blank=True)
class NotInstalledModel(models.Model):
        app_label = "not_installed"
class RelatedModel(models.Model):
    not_installed = models.ForeignKey(NotInstalledModel, models.CASCADE)
class M2MRelatedModel(models.Model):
    not_installed = models.ManyToManyField(NotInstalledModel)
# We're testing app registry presence on load, so this is handy.
new_apps = Apps(["apps"])
class TotallyNormal(models.Model):
class SoAlternative(models.Model):
        apps = new_apps
    simple = models.ForeignKey("SimpleModel", models.CASCADE, null=True)
class SimpleModel(models.Model):
    field = models.IntegerField()
    created = models.DateTimeField(default=timezone.now)
class ManyToManyModel(models.Model):
    simples = models.ManyToManyField("SimpleModel")
    root = models.IntegerField()
    square = models.PositiveIntegerField(db_default=9)
        return "%s ** 2 == %s" % (self.root, self.square)
class SchoolClassManager(models.Manager):
        return super().get_queryset().exclude(year=1000)
class SchoolClass(models.Model):
    year = models.PositiveIntegerField()
    day = models.CharField(max_length=9, blank=True)
    last_updated = models.DateTimeField()
    objects = SchoolClassManager()
class SchoolBusManager(models.Manager):
        return super().get_queryset().prefetch_related("schoolclasses")
class SchoolBus(models.Model):
    number = models.IntegerField()
    schoolclasses = models.ManyToManyField("SchoolClass")
    objects = SchoolBusManager()
        base_manager_name = "objects"
class VeryLongModelNameZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ(models.Model):
    primary_key_is_quite_long_zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = models.AutoField(
        primary_key=True
    charfield_is_quite_long_zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = models.CharField(
        max_length=100
    m2m_also_quite_long_zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz = (
        models.ManyToManyField(Person, blank=True)
class Tag(models.Model):
        ContentType, models.CASCADE, related_name="backend_tags"
    tags = GenericRelation("Tag")
        db_table = "CaseSensitive_Post"
class Reporter(models.Model):
    first_name = models.CharField(max_length=30)
    last_name = models.CharField(max_length=30)
class ReporterProxy(Reporter):
    headline = models.CharField(max_length=100)
    pub_date = models.DateField()
    reporter = models.ForeignKey(Reporter, models.CASCADE)
    reporter_proxy = models.ForeignKey(
        ReporterProxy,
        related_name="reporter_proxy",
        return self.headline
class Item(models.Model):
    date = models.DateField()
    time = models.TimeField()
    last_modified = models.DateTimeField()
class Object(models.Model):
    related_objects = models.ManyToManyField(
        "self", db_constraint=False, symmetrical=False
    obj_ref = models.ForeignKey("ObjectReference", models.CASCADE, null=True)
class ObjectReference(models.Model):
    obj = models.ForeignKey(Object, models.CASCADE, db_constraint=False)
        return str(self.obj_id)
class ObjectSelfReference(models.Model):
    key = models.CharField(max_length=3, unique=True)
    obj = models.ForeignKey("ObjectSelfReference", models.SET_NULL, null=True)
class CircularA(models.Model):
    obj = models.ForeignKey("CircularB", models.SET_NULL, null=True)
        return (self.key,)
class CircularB(models.Model):
    obj = models.ForeignKey("CircularA", models.SET_NULL, null=True)
class RawData(models.Model):
    raw_data = models.BinaryField()
    name = models.CharField(max_length=255, unique=True)
    author = models.ForeignKey(Author, models.CASCADE, to_field="name")
class SQLKeywordsModel(models.Model):
    id = models.AutoField(primary_key=True, db_column="select")
    reporter = models.ForeignKey(Reporter, models.CASCADE, db_column="where")
        db_table = "order"
        app_label = "app_unmigrated"
# The models definitions below used to crash. Generating models dynamically
# at runtime is a bad idea because it pollutes the app registry. This doesn't
# integrate well with the test suite but at least it prevents regressions.
class CustomBaseModel(models.base.ModelBase):
class MyModel(models.Model, metaclass=CustomBaseModel):
    """Model subclass with a custom base using metaclass."""
    headline = models.CharField(max_length=100, default="Default headline")
    pub_date = models.DateTimeField()
        ordering = ("pub_date", "headline")
class FeaturedArticle(models.Model):
    article = models.OneToOneField(Article, models.CASCADE, related_name="featured")
class ArticleSelectOnSave(Article):
        select_on_save = True
class SelfRef(models.Model):
    selfref = models.ForeignKey(
        "self",
    article = models.ForeignKey(Article, models.SET_NULL, null=True, blank=True)
    article_cited = models.ForeignKey(
        Article, models.SET_NULL, null=True, blank=True, related_name="cited"
        # This method intentionally doesn't work for all cases - part
        # of the test for ticket #20278
        return SelfRef.objects.get(selfref=self).pk
class PrimaryKeyWithDefault(models.Model):
class PrimaryKeyWithDbDefault(models.Model):
    uuid = models.IntegerField(primary_key=True, db_default=1)
class PrimaryKeyWithFalseyDefault(models.Model):
    uuid = models.IntegerField(primary_key=True, default=0)
class PrimaryKeyWithFalseyDbDefault(models.Model):
    uuid = models.IntegerField(primary_key=True, db_default=0)
class ChildPrimaryKeyWithDefault(PrimaryKeyWithDefault):
from django.db.models.functions import Now
    iso_two_letter = models.CharField(max_length=2)
                fields=["iso_two_letter", "name"],
                name="country_name_iso_unique",
class ProxyCountry(Country):
class ProxyProxyCountry(ProxyCountry):
class ProxyMultiCountry(ProxyCountry):
class ProxyMultiProxyCountry(ProxyMultiCountry):
class Place(models.Model):
class Restaurant(Place):
class Pizzeria(Restaurant):
    two_letter_code = models.CharField(max_length=2, primary_key=True)
class TwoFields(models.Model):
    f1 = models.IntegerField(unique=True)
    f2 = models.IntegerField(unique=True)
    name = models.CharField(max_length=15, null=True)
class FieldsWithDbColumns(models.Model):
    rank = models.IntegerField(unique=True, db_column="rAnK")
    name = models.CharField(max_length=15, null=True, db_column="oTheRNaMe")
class UpsertConflict(models.Model):
    number = models.IntegerField(unique=True)
class NoFields(models.Model):
class SmallAutoFieldModel(models.Model):
    id = models.SmallAutoField(primary_key=True)
class BigAutoFieldModel(models.Model):
    id = models.BigAutoField(primary_key=True)
class NullableFields(models.Model):
    # Fields in db.backends.oracle.BulkInsertMapper
    big_int_filed = models.BigIntegerField(null=True, default=1)
    binary_field = models.BinaryField(null=True, default=b"data")
    date_field = models.DateField(null=True, default=timezone.now)
    datetime_field = models.DateTimeField(null=True, default=timezone.now)
    decimal_field = models.DecimalField(
        null=True, max_digits=2, decimal_places=1, default=Decimal("1.1")
    duration_field = models.DurationField(null=True, default=datetime.timedelta(1))
    float_field = models.FloatField(null=True, default=3.2)
    integer_field = models.IntegerField(null=True, default=2)
    null_boolean_field = models.BooleanField(null=True, default=False)
    positive_big_integer_field = models.PositiveBigIntegerField(
        null=True, default=2**63 - 1
    positive_integer_field = models.PositiveIntegerField(null=True, default=3)
    positive_small_integer_field = models.PositiveSmallIntegerField(
        null=True, default=4
    small_integer_field = models.SmallIntegerField(null=True, default=5)
    time_field = models.TimeField(null=True, default=timezone.now)
    auto_field = models.ForeignKey(NoFields, on_delete=models.CASCADE, null=True)
    small_auto_field = models.ForeignKey(
        SmallAutoFieldModel, on_delete=models.CASCADE, null=True
    big_auto_field = models.ForeignKey(
        BigAutoFieldModel, on_delete=models.CASCADE, null=True
    # Fields not required in BulkInsertMapper
    char_field = models.CharField(null=True, max_length=4, default="char")
    email_field = models.EmailField(null=True, default="user@example.com")
    file_field = models.FileField(null=True, default="file.txt")
    file_path_field = models.FilePathField(path="/tmp", null=True, default="file.txt")
    generic_ip_address_field = models.GenericIPAddressField(
        null=True, default="127.0.0.1"
        image_field = models.ImageField(null=True, default="image.jpg")
    slug_field = models.SlugField(null=True, default="slug")
    text_field = models.TextField(null=True, default="text")
    url_field = models.URLField(null=True, default="/")
    uuid_field = models.UUIDField(null=True, default=uuid.uuid4)
    country = models.OneToOneField(Country, models.CASCADE, primary_key=True)
    big_auto_fields = models.ManyToManyField(BigAutoFieldModel)
class DbDefaultModel(models.Model):
    created_at = models.DateTimeField(db_default=Now())
        required_db_features = {"supports_expression_defaults"}
class DbDefaultPrimaryKey(models.Model):
    id = models.DateTimeField(primary_key=True, db_default=Now())
def expensive_calculation():
    expensive_calculation.num_runs += 1
    return timezone.now()
    question = models.CharField(max_length=200)
    answer = models.CharField(max_length=200)
    pub_date = models.DateTimeField("date published", default=expensive_calculation)
from django.core.checks import register
    manager = models.manager.Manager()
@register("tests")
def my_check(app_configs, **kwargs):
    my_check.did_run = True
my_check.did_run = False
from django.db.models.functions import Coalesce, Lower
class Product(models.Model):
    price = models.IntegerField(null=True)
    discounted_price = models.IntegerField(null=True)
    unit = models.CharField(max_length=15, null=True)
        required_db_features = {
            "supports_table_check_constraints",
            models.CheckConstraint(
                condition=models.Q(price__gt=models.F("discounted_price")),
                name="price_gt_discounted_price",
                condition=models.Q(price__gt=0),
                name="%(app_label)s_%(class)s_price_gt_0",
                condition=models.Q(
                    models.Q(unit__isnull=True) | models.Q(unit__in=["μg/mL", "ng/mL"])
                name="unicode_unit_list",
class GeneratedFieldStoredProduct(models.Model):
    name = models.CharField(max_length=255, null=True)
    rebate = models.GeneratedField(
        expression=Coalesce("price", 0)
        - Coalesce("discounted_price", Coalesce("price", 0)),
        output_field=models.IntegerField(),
    lower_name = models.GeneratedField(
        expression=Lower(models.F("name")),
        output_field=models.CharField(max_length=255, null=True),
class GeneratedFieldVirtualProduct(models.Model):
        db_persist=False,
        required_db_features = {"supports_virtual_generated_columns"}
class UniqueConstraintProduct(models.Model):
    color = models.CharField(max_length=32, null=True)
    age = models.IntegerField(null=True)
    updated = models.DateTimeField(null=True)
                fields=["name", "color"],
                name="name_color_uniq",
class ChildUniqueConstraintProduct(UniqueConstraintProduct):
class UniqueConstraintConditionProduct(models.Model):
        required_db_features = {"supports_partial_indexes"}
                fields=["name"],
                name="name_without_color_uniq",
                condition=models.Q(color__isnull=True),
class UniqueConstraintDeferrable(models.Model):
    shelf = models.CharField(max_length=31)
            "supports_deferrable_unique_constraints",
                name="name_init_deferred_uniq",
                deferrable=models.Deferrable.DEFERRED,
                fields=["shelf"],
                name="sheld_init_immediate_uniq",
                deferrable=models.Deferrable.IMMEDIATE,
class UniqueConstraintInclude(models.Model):
            "supports_covering_indexes",
                name="name_include_color_uniq",
                include=["color"],
class AbstractModel(models.Model):
                condition=models.Q(age__gte=18),
                name="%(app_label)s_%(class)s_adult",
class ChildModel(AbstractModel):
class JSONFieldModel(models.Model):
    data = models.JSONField(null=True)
class ModelWithDatabaseDefault(models.Model):
    field = models.CharField(max_length=255)
    field_with_db_default = models.CharField(
        max_length=255, db_default=models.Value("field_with_db_default")
from django.contrib.sites.models import SiteManager
        return "/authors/%s/" % self.id
    date_created = models.DateTimeField()
class SchemeIncludedURL(models.Model):
    url = models.URLField(max_length=100)
class ConcreteModel(models.Model):
class ProxyModel(ConcreteModel):
class FooWithoutUrl(models.Model):
    Fake model not defining ``get_absolute_url`` for
    ContentTypesTests.test_shortcut_view_without_get_absolute_url()
    name = models.CharField(max_length=30, unique=True)
class FooWithUrl(FooWithoutUrl):
    Fake model defining ``get_absolute_url`` for
    ContentTypesTests.test_shortcut_view().
        return "/users/%s/" % quote(self.name)
class FooWithBrokenAbsoluteUrl(FooWithoutUrl):
    Fake model defining a ``get_absolute_url`` method containing an error
        return "/users/%s/" % self.unknown_field
    text = models.CharField(max_length=200)
    answer_set = GenericRelation("Answer")
    question = GenericForeignKey()
        order_with_respect_to = "question"
    """An ordered tag on an item."""
    title = models.CharField(max_length=200)
    content_type = models.ForeignKey(ContentType, models.CASCADE, null=True)
    object_id = models.PositiveIntegerField(null=True)
    children = GenericRelation("Post")
        order_with_respect_to = "parent"
class ModelWithNullFKToSite(models.Model):
    site = models.ForeignKey(Site, null=True, on_delete=models.CASCADE)
    post = models.ForeignKey(Post, null=True, on_delete=models.CASCADE)
        return "/title/%s/" % quote(self.title)
class ModelWithM2MToSite(models.Model):
    sites = models.ManyToManyField(Site)
class UUIDModel(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4)
        return "/uuid/%s/" % self.pk
class DebugObject(models.Model):
Custom column/table names
If your database column name is different than your model attribute, use the
``db_column`` parameter. Note that you'll use the field's name, not its column
name, in API usage.
If your database table name is different than your model name, use the
``db_table`` Meta attribute. This has no effect on the API used to
query the database.
If you need to use a table name for a many-to-many relationship that differs
from the default generated name, use the ``db_table`` parameter on the
``ManyToManyField``. This has no effect on the API for querying the database.
    Author_ID = models.AutoField(primary_key=True, db_column="Author ID")
    first_name = models.CharField(max_length=30, db_column="firstname")
    last_name = models.CharField(max_length=30, db_column="last")
        db_table = "my_author_table"
        ordering = ("last_name", "first_name")
    Article_ID = models.AutoField(primary_key=True, db_column="Article ID")
    authors = models.ManyToManyField(Author, db_table="my_m2m_table")
    primary_author = models.ForeignKey(
        Author,
        db_column="Author ID",
        related_name="primary_set",
        ordering = ("headline",)
    alias = models.CharField(max_length=20)
    birthdate = models.DateField(null=True)
    average_rating = models.FloatField(null=True)
    author = models.ForeignKey(Author, on_delete=models.CASCADE)
class MySQLUnixTimestamp(models.Model):
    timestamp = models.PositiveIntegerField()
Giving models a custom manager
You can use a custom ``Manager`` in a particular model by extending the base
``Manager`` class and instantiating your custom ``Manager`` in your model.
There are two reasons you might want to customize a ``Manager``: to add extra
``Manager`` methods, and/or to modify the initial ``QuerySet`` the ``Manager``
class PersonManager(models.Manager):
    def get_fun_people(self):
        return self.filter(fun=True)
class PublishedBookManager(models.Manager):
        return super().get_queryset().filter(is_published=True)
class AnnotatedBookManager(models.Manager):
            .get_queryset()
            .annotate(favorite_avg=models.Avg("favorite_books__favorite_thing_id"))
class CustomQuerySet(models.QuerySet):
        queryset = super().filter(fun=True)
        queryset._filter_CustomQuerySet = True
    def public_method(self, *args, **kwargs):
        return self.all()
    def _private_method(self, *args, **kwargs):
    def optout_public_method(self, *args, **kwargs):
    optout_public_method.queryset_only = True
    def _optin_private_method(self, *args, **kwargs):
    _optin_private_method.queryset_only = False
class BaseCustomManager(models.Manager):
    def __init__(self, arg):
        self.init_arg = arg
        queryset._filter_CustomManager = True
    def manager_only(self):
CustomManager = BaseCustomManager.from_queryset(CustomQuerySet)
class CustomInitQuerySet(models.QuerySet):
    # QuerySet with an __init__() method that takes an additional argument.
        self, custom_optional_arg=None, model=None, query=None, using=None, hints=None
        super().__init__(model=model, query=query, using=using, hints=hints)
class DeconstructibleCustomManager(BaseCustomManager.from_queryset(CustomQuerySet)):
    def __init__(self, a, b, c=1, d=2):
        super().__init__(a)
class FunPeopleManager(models.Manager):
        return super().get_queryset().filter(fun=True)
class BoringPeopleManager(models.Manager):
        return super().get_queryset().filter(fun=False)
    fun = models.BooleanField(default=False)
    favorite_book = models.ForeignKey(
        "Book", models.SET_NULL, null=True, related_name="favorite_books"
    favorite_thing_type = models.ForeignKey(
        "contenttypes.ContentType", models.SET_NULL, null=True
    favorite_thing_id = models.IntegerField(null=True)
    favorite_thing = GenericForeignKey("favorite_thing_type", "favorite_thing_id")
    objects = PersonManager()
    fun_people = FunPeopleManager()
    boring_people = BoringPeopleManager()
    custom_queryset_default_manager = CustomQuerySet.as_manager()
    custom_queryset_custom_manager = CustomManager("hello")
    custom_init_queryset_manager = CustomInitQuerySet.as_manager()
class FunPerson(models.Model):
    fun = models.BooleanField(default=True)
        "Book",
        related_name="fun_people_favorite_books",
    objects = FunPeopleManager()
    is_published = models.BooleanField(default=False)
    authors = models.ManyToManyField(Person, related_name="books")
    fun_authors = models.ManyToManyField(FunPerson, related_name="books")
    favorite_things = GenericRelation(
        Person,
        content_type_field="favorite_thing_type",
        object_id_field="favorite_thing_id",
    fun_people_favorite_things = GenericRelation(
        FunPerson,
    published_objects = PublishedBookManager()
    annotated_objects = AnnotatedBookManager()
        base_manager_name = "annotated_objects"
class FastCarManager(models.Manager):
        return super().get_queryset().filter(top_speed__gt=150)
    mileage = models.IntegerField()
    top_speed = models.IntegerField(help_text="In miles per hour.")
    cars = models.Manager()
    fast_cars = FastCarManager()
class FastCarAsBase(Car):
        base_manager_name = "fast_cars"
class FastCarAsDefault(Car):
        default_manager_name = "fast_cars"
class RestrictedManager(models.Manager):
        return super().get_queryset().filter(is_public=True)
class RestrictedModel(models.Model):
    is_public = models.BooleanField(default=False)
    related = models.ForeignKey(RelatedModel, models.CASCADE)
    objects = RestrictedManager()
    plain_manager = models.Manager()
class OneToOneRestrictedModel(models.Model):
    related = models.OneToOneField(RelatedModel, models.CASCADE)
class AbstractPerson(models.Model):
    abstract_persons = models.Manager()
    objects = models.CharField(max_length=30)
class PersonFromAbstract(AbstractPerson):
Giving models custom methods
Any method you add to a model will be available to instances.
    def was_published_today(self):
        return self.pub_date == datetime.date.today()
    def articles_from_same_day_1(self):
        return Article.objects.filter(pub_date=self.pub_date).exclude(id=self.id)
    def articles_from_same_day_2(self):
        Verbose version of get_articles_from_same_day_1, which does a custom
        database query for the sake of demonstration.
                SELECT id, headline, pub_date
                FROM custom_methods_article
                WHERE pub_date = %s
                    AND id != %s""",
                [connection.ops.adapt_datefield_value(self.pub_date), self.id],
            return [self.__class__(*row) for row in cursor.fetchall()]
Using a custom primary key
By default, Django adds an ``"id"`` field to each model. But you can override
this behavior by explicitly adding ``primary_key=True`` to a field.
from .fields import MyAutoField, MyWrapperField
    employee_code = models.IntegerField(primary_key=True, db_column="code")
class Business(models.Model):
    name = models.CharField(max_length=20, primary_key=True)
    employees = models.ManyToManyField(Employee)
        verbose_name_plural = "businesses"
    id = MyWrapperField(primary_key=True, db_index=True)
    bar = models.ForeignKey(Bar, models.CASCADE)
class CustomAutoFieldModel(models.Model):
    id = MyAutoField(primary_key=True)
This is a basic model to test saving and loading boolean and date-related
types, which in the past were problematic for some database backends.
class Donut(models.Model):
    is_frosted = models.BooleanField(default=False)
    has_sprinkles = models.BooleanField(null=True)
    baked_date = models.DateField(null=True)
    baked_time = models.TimeField(null=True)
    consumed_at = models.DateTimeField(null=True)
    review = models.TextField()
        ordering = ("consumed_at",)
class RumBaba(models.Model):
    baked_date = models.DateField(auto_now_add=True)
    baked_timestamp = models.DateTimeField(auto_now_add=True)
    pub_datetime = models.DateTimeField(default=timezone.now)
    categories = models.ManyToManyField("Category", related_name="articles")
class Comment(models.Model):
    article = models.ForeignKey(Article, models.CASCADE, related_name="comments")
    approval_date = models.DateField(null=True)
    published_on = models.DateField(null=True)
    approval_date = models.DateTimeField(null=True)
        return "Comment to %s (%s)" % (self.article.title, self.pub_date)
Tests for built in Function expressions.
    alias = models.CharField(max_length=50, null=True, blank=True)
    goes_by = models.CharField(max_length=50, null=True, blank=True)
    age = models.PositiveSmallIntegerField(default=30)
    authors = models.ManyToManyField(Author, related_name="articles")
    summary = models.CharField(max_length=200, null=True, blank=True)
    written = models.DateTimeField()
    published = models.DateTimeField(null=True, blank=True)
    updated = models.DateTimeField(null=True, blank=True)
    views = models.PositiveIntegerField(default=0)
class Fan(models.Model):
    author = models.ForeignKey(Author, models.CASCADE, related_name="fans")
    fan_since = models.DateTimeField(null=True, blank=True)
class DTModel(models.Model):
    name = models.CharField(max_length=32)
    end_datetime = models.DateTimeField(null=True, blank=True)
    start_date = models.DateField(null=True, blank=True)
    end_date = models.DateField(null=True, blank=True)
    start_time = models.TimeField(null=True, blank=True)
    end_time = models.TimeField(null=True, blank=True)
    duration = models.DurationField(null=True, blank=True)
class DecimalModel(models.Model):
    n1 = models.DecimalField(decimal_places=2, max_digits=6)
    n2 = models.DecimalField(decimal_places=7, max_digits=9, null=True, blank=True)
class IntegerModel(models.Model):
    big = models.BigIntegerField(null=True, blank=True)
    normal = models.IntegerField(null=True, blank=True)
    small = models.SmallIntegerField(null=True, blank=True)
class FloatModel(models.Model):
    f1 = models.FloatField(null=True, blank=True)
    f2 = models.FloatField(null=True, blank=True)
    uuid = models.UUIDField(null=True)
    shift = models.DurationField(null=True)
Tests for defer() and only().
class Secondary(models.Model):
    first = models.CharField(max_length=50)
    second = models.CharField(max_length=50)
class Primary(models.Model):
    value = models.CharField(max_length=50)
    related = models.ForeignKey(Secondary, models.CASCADE)
class PrimaryOneToOne(models.Model):
    related = models.OneToOneField(
        Secondary, models.CASCADE, related_name="primary_o2o"
class Child(Primary):
class BigChild(Primary):
    other = models.CharField(max_length=50)
class ChildProxy(Child):
class RefreshPrimaryProxy(Primary):
    def refresh_from_db(self, using=None, fields=None, **kwargs):
        # Reloads all deferred fields if any of the fields is deferred.
            if fields.intersection(deferred_fields):
                fields = fields.union(deferred_fields)
        super().refresh_from_db(using, fields, **kwargs)
class ShadowParent(models.Model):
    ShadowParent declares a scalar, rather than a field. When this is
    overridden, the field value, rather than the scalar value must still be
    used when the field is deferred.
    name = "aphrodite"
class ShadowChild(ShadowParent):
    name = models.CharField(default="adonis", max_length=6)
Regression tests for defer() / only() behavior.
    text = models.TextField(default="xyzzy")
    value = models.IntegerField()
    other_value = models.IntegerField(default=0)
    source = models.OneToOneField(
        related_name="destination",
        on_delete=models.CASCADE,
class RelatedItem(models.Model):
    item = models.ForeignKey(Item, models.CASCADE)
class ProxyRelated(RelatedItem):
class Leaf(models.Model):
    child = models.ForeignKey(Child, models.CASCADE)
    second_child = models.ForeignKey(
        Child, models.SET_NULL, related_name="other", null=True
    value = models.IntegerField(default=42)
class ResolveThis(models.Model):
    num = models.FloatField()
    name = models.CharField(max_length=16)
class Proxy(Item):
class SimpleItem(models.Model):
class Feature(models.Model):
    item = models.ForeignKey(SimpleItem, models.CASCADE)
class SpecialFeature(models.Model):
    feature = models.ForeignKey(Feature, models.CASCADE)
class OneToOneItem(models.Model):
    item = models.OneToOneField(Item, models.CASCADE, related_name="one_to_one_item")
class ItemAndSimpleItem(models.Model):
    simple = models.ForeignKey(SimpleItem, models.CASCADE)
    profile1 = models.CharField(max_length=255, default="profile1")
    location1 = models.CharField(max_length=255, default="location1")
class Request(models.Model):
    profile = models.ForeignKey(Profile, models.SET_NULL, null=True, blank=True)
    location = models.ForeignKey(Location, models.CASCADE)
    items = models.ManyToManyField(Item)
    request1 = models.CharField(default="request1", max_length=255)
    request2 = models.CharField(default="request2", max_length=255)
    request3 = models.CharField(default="request3", max_length=255)
    request4 = models.CharField(default="request4", max_length=255)
class Base(models.Model):
class Derived(Base):
    other_text = models.TextField()
class P(models.Model):
class R(models.Model):
    is_default = models.BooleanField(default=False)
    p = models.ForeignKey(P, models.CASCADE, null=True)
def get_default_r():
    return R.objects.get_or_create(is_default=True)[0].pk
class S(models.Model):
    r = models.ForeignKey(R, models.CASCADE)
class T(models.Model):
    s = models.ForeignKey(S, models.CASCADE)
class U(models.Model):
    t = models.ForeignKey(T, models.CASCADE)
class RChild(R):
class RProxy(R):
class RChildChild(RChild):
class RelatedDbOptionGrandParent(models.Model):
class RelatedDbOptionParent(models.Model):
    p = models.ForeignKey(RelatedDbOptionGrandParent, models.DB_CASCADE, null=True)
class CascadeDbModel(models.Model):
    db_cascade = models.ForeignKey(
        RelatedDbOptionParent, models.DB_CASCADE, related_name="db_cascade_set"
class SetNullDbModel(models.Model):
    db_setnull = models.ForeignKey(
        RelatedDbOptionParent,
        models.DB_SET_NULL,
        related_name="db_setnull_set",
        required_db_features = {"supports_on_delete_db_null"}
class SetDefaultDbModel(models.Model):
    db_setdefault = models.ForeignKey(
        models.DB_SET_DEFAULT,
        db_default=models.Value(1),
        related_name="db_setdefault_set",
    db_setdefault_none = models.ForeignKey(
        db_default=None,
        related_name="db_setnull_nullable_set",
        required_db_features = {"supports_on_delete_db_default"}
class A(models.Model):
    auto = models.ForeignKey(R, models.CASCADE, related_name="auto_set")
    auto_nullable = models.ForeignKey(
        R, models.CASCADE, null=True, related_name="auto_nullable_set"
    setvalue = models.ForeignKey(R, models.SET(get_default_r), related_name="setvalue")
    setnull = models.ForeignKey(
        R, models.SET_NULL, null=True, related_name="setnull_set"
    setdefault = models.ForeignKey(
        R, models.SET_DEFAULT, default=get_default_r, related_name="setdefault_set"
    setdefault_none = models.ForeignKey(
        models.SET_DEFAULT,
        related_name="setnull_nullable_set",
    cascade = models.ForeignKey(R, models.CASCADE, related_name="cascade_set")
    cascade_nullable = models.ForeignKey(
        R, models.CASCADE, null=True, related_name="cascade_nullable_set"
    protect = models.ForeignKey(
        R, models.PROTECT, null=True, related_name="protect_set"
    restrict = models.ForeignKey(
        R, models.RESTRICT, null=True, related_name="restrict_set"
    donothing = models.ForeignKey(
        R, models.DO_NOTHING, null=True, related_name="donothing_set"
    child = models.ForeignKey(RChild, models.CASCADE, related_name="child")
    child_setnull = models.ForeignKey(
        RChild, models.SET_NULL, null=True, related_name="child_setnull"
    cascade_p = models.ForeignKey(
        P, models.CASCADE, related_name="cascade_p_set", null=True
    # A OneToOneField is just a ForeignKey unique=True, so we don't duplicate
    # all the tests; just one smoke test to ensure on_delete works for it as
    # well.
    o2o_setnull = models.ForeignKey(
        R, models.SET_NULL, null=True, related_name="o2o_nullable_set"
class B(models.Model):
    protect = models.ForeignKey(R, models.PROTECT)
def create_a(name):
    a = A(name=name)
    for name in (
        "auto",
        "auto_nullable",
        "setvalue",
        "setnull",
        "setdefault",
        "setdefault_none",
        "cascade",
        "cascade_nullable",
        "protect",
        "restrict",
        "donothing",
        "o2o_setnull",
        r = R.objects.create()
        setattr(a, name, r)
    a.child = RChild.objects.create()
    a.child_setnull = RChild.objects.create()
    a.save()
class M(models.Model):
    m2m = models.ManyToManyField(R, related_name="m_set")
    m2m_through = models.ManyToManyField(R, through="MR", related_name="m_through_set")
    m2m_through_null = models.ManyToManyField(
        R, through="MRNull", related_name="m_through_null_set"
class MR(models.Model):
    m = models.ForeignKey(M, models.CASCADE)
class MRNull(models.Model):
    r = models.ForeignKey(R, models.SET_NULL, null=True)
class Avatar(models.Model):
    desc = models.TextField(null=True)
# This model is used to test a duplicate query regression (#25685)
class AvatarProxy(Avatar):
class User(models.Model):
    avatar = models.ForeignKey(Avatar, models.CASCADE, null=True)
class HiddenUser(models.Model):
    r = models.ForeignKey(R, models.CASCADE, related_name="+")
class HiddenUserProfile(models.Model):
    user = models.ForeignKey(HiddenUser, models.CASCADE)
class M2MTo(models.Model):
class M2MFrom(models.Model):
    m2m = models.ManyToManyField(M2MTo)
class Child(Parent):
class RelToBase(models.Model):
    base = models.ForeignKey(Base, models.DO_NOTHING, related_name="rels")
class Origin(models.Model):
    r_proxy = models.ForeignKey("RProxy", models.CASCADE, null=True)
class Referrer(models.Model):
    origin = models.ForeignKey(Origin, models.CASCADE)
    unique_field = models.IntegerField(unique=True)
    large_field = models.TextField()
class SecondReferrer(models.Model):
    referrer = models.ForeignKey(Referrer, models.CASCADE)
    other_referrer = models.ForeignKey(
        Referrer, models.CASCADE, to_field="unique_field", related_name="+"
class DeleteTop(models.Model):
    b1 = GenericRelation("GenericB1")
    b2 = GenericRelation("GenericB2")
class B1(models.Model):
    delete_top = models.ForeignKey(DeleteTop, models.CASCADE)
class B2(models.Model):
class B3(models.Model):
    restrict = models.ForeignKey(R, models.RESTRICT)
class DeleteBottom(models.Model):
    b1 = models.ForeignKey(B1, models.RESTRICT)
    b2 = models.ForeignKey(B2, models.CASCADE)
class GenericB1(models.Model):
    generic_delete_top = GenericForeignKey("content_type", "object_id")
class GenericB2(models.Model):
    generic_delete_bottom = GenericRelation("GenericDeleteBottom")
class GenericDeleteBottom(models.Model):
    generic_b1 = models.ForeignKey(GenericB1, models.RESTRICT)
    generic_b2 = GenericForeignKey()
class GenericDeleteBottomParent(models.Model):
    generic_delete_bottom = models.ForeignKey(
        GenericDeleteBottom, on_delete=models.CASCADE
class Award(models.Model):
    content_object = GenericForeignKey()
class AwardNote(models.Model):
    award = models.ForeignKey(Award, models.CASCADE)
    note = models.CharField(max_length=100)
    awards = GenericRelation(Award)
    pagecount = models.IntegerField()
    owner = models.ForeignKey("Child", models.CASCADE, null=True)
    toys = models.ManyToManyField(Toy, through="PlayedWith")
class PlayedWith(models.Model):
    toy = models.ForeignKey(Toy, models.CASCADE)
    date = models.DateField(db_column="date_col")
class PlayedWithNote(models.Model):
    played = models.ForeignKey(PlayedWith, models.CASCADE)
    note = models.TextField()
class Contact(models.Model):
    label = models.CharField(max_length=100)
class Email(Contact):
    email_address = models.EmailField(max_length=100)
class Researcher(models.Model):
    contacts = models.ManyToManyField(Contact, related_name="research_contacts")
    primary_contact = models.ForeignKey(
        Contact, models.SET_NULL, null=True, related_name="primary_contacts"
    secondary_contact = models.ForeignKey(
        Contact, models.SET_NULL, null=True, related_name="secondary_contacts"
class Food(models.Model):
class Eaten(models.Model):
    food = models.ForeignKey(Food, models.CASCADE, to_field="name")
    meal = models.CharField(max_length=20)
# Models for #15776
class Policy(models.Model):
    policy_number = models.CharField(max_length=10)
class Version(models.Model):
    policy = models.ForeignKey(Policy, models.CASCADE)
    version = models.ForeignKey(Version, models.SET_NULL, blank=True, null=True)
    version = models.ForeignKey(Version, models.CASCADE)
    location = models.ForeignKey(Location, models.SET_NULL, blank=True, null=True)
    location_value = models.ForeignKey(
        Location, models.SET(42), default=1, db_constraint=False, related_name="+"
# Models for #16128
class File(models.Model):
class Image(File):
class Photo(Image):
class FooImage(models.Model):
    my_image = models.ForeignKey(Image, models.CASCADE)
class FooFile(models.Model):
    my_file = models.ForeignKey(File, models.CASCADE)
class FooPhoto(models.Model):
    my_photo = models.ForeignKey(Photo, models.CASCADE)
class FooFileProxy(FooFile):
class OrgUnit(models.Model):
    name = models.CharField(max_length=64, unique=True)
class Login(models.Model):
    description = models.CharField(max_length=32)
    orgunit = models.ForeignKey(OrgUnit, models.CASCADE)
class House(models.Model):
    address = models.CharField(max_length=32)
class OrderedPerson(models.Model):
    lives_in = models.ForeignKey(House, models.CASCADE)
def get_best_toy():
    toy, _ = Toy.objects.get_or_create(name="best")
    return toy
def get_worst_toy():
    toy, _ = Toy.objects.get_or_create(name="worst")
    best_toy = models.ForeignKey(
        Toy, default=get_best_toy, on_delete=models.SET_DEFAULT, related_name="toys"
    worst_toy = models.ForeignKey(
        Toy, models.SET(get_worst_toy), related_name="bad_toys"
        related_name="children",
class Celebrity(models.Model):
    name = models.CharField("Name", max_length=20)
    greatest_fan = models.ForeignKey(
        "Fan",
    fan_of = models.ForeignKey(Celebrity, models.CASCADE)
class Staff(models.Model):
    organisation = models.CharField(max_length=100)
    tags = models.ManyToManyField(Tag, through="StaffTag")
    coworkers = models.ManyToManyField("self")
class StaffTag(models.Model):
    staff = models.ForeignKey(Staff, models.CASCADE)
    tag = models.ForeignKey(Tag, models.CASCADE)
        return "%s -> %s" % (self.tag, self.staff)
Empty model tests
These test that things behave sensibly for the rare corner-case of a model with
no fields.
class Empty(models.Model):
Tests for F() query expression syntax.
class Manager(models.Model):
    secretary = models.ForeignKey(
        "Employee", models.CASCADE, null=True, related_name="managers"
    firstname = models.CharField(max_length=50)
    lastname = models.CharField(max_length=50)
    salary = models.IntegerField(blank=True, null=True)
    manager = models.ForeignKey(Manager, models.CASCADE, null=True)
    based_in_eu = models.BooleanField(default=False)
        return "%s %s" % (self.firstname, self.lastname)
class RemoteEmployee(Employee):
    adjusted_salary = models.IntegerField()
    num_employees = models.PositiveIntegerField()
    num_chairs = models.PositiveIntegerField()
    ceo = models.ForeignKey(
        Employee,
        related_name="company_ceo_set",
    point_of_contact = models.ForeignKey(
        related_name="company_point_of_contact_set",
class Number(models.Model):
    integer = models.BigIntegerField(db_column="the_integer")
    float = models.FloatField(null=True, db_column="the_float")
    decimal_value = models.DecimalField(max_digits=20, decimal_places=17, null=True)
        return "%i, %s, %s" % (
            self.integer,
            "%.3f" % self.float if self.float is not None else None,
            "%.17f" % self.decimal_value if self.decimal_value is not None else None,
class Experiment(models.Model):
    name = models.CharField(max_length=24)
    assigned = models.DateField()
    completed = models.DateField()
    estimated_time = models.DurationField()
    start = models.DateTimeField()
    end = models.DateTimeField()
    scalar = models.IntegerField(null=True)
        db_table = "expressions_ExPeRiMeNt"
        return self.end - self.start
class Result(models.Model):
    experiment = models.ForeignKey(Experiment, models.CASCADE)
    result_time = models.DateTimeField()
        return "Result at %s" % self.result_time
class Time(models.Model):
    time = models.TimeField(null=True)
        return str(self.time)
class SimulationRun(models.Model):
    start = models.ForeignKey(Time, models.CASCADE, null=True, related_name="+")
    end = models.ForeignKey(Time, models.CASCADE, null=True, related_name="+")
    midpoint = models.TimeField()
        return "%s (%s to %s)" % (self.midpoint, self.start, self.end)
class UUIDPK(models.Model):
class UUID(models.Model):
    uuid_fk = models.ForeignKey(UUIDPK, models.CASCADE, null=True)
class Text(models.Model):
class CaseTestModel(models.Model):
    integer = models.IntegerField()
    integer2 = models.IntegerField(null=True)
    string = models.CharField(max_length=100, default="")
    big_integer = models.BigIntegerField(null=True)
    binary = models.BinaryField(default=b"")
    boolean = models.BooleanField(default=False)
    date = models.DateField(null=True, db_column="date_field")
    date_time = models.DateTimeField(null=True)
    decimal = models.DecimalField(
        max_digits=2, decimal_places=1, null=True, db_column="decimal_field"
    duration = models.DurationField(null=True)
    email = models.EmailField(default="")
    file = models.FileField(null=True, db_column="file_field")
    file_path = models.FilePathField(null=True)
    float = models.FloatField(null=True, db_column="float_field")
        image = models.ImageField(null=True)
    generic_ip_address = models.GenericIPAddressField(null=True)
    null_boolean = models.BooleanField(null=True)
    positive_integer = models.PositiveIntegerField(null=True)
    positive_small_integer = models.PositiveSmallIntegerField(null=True)
    positive_big_integer = models.PositiveSmallIntegerField(null=True)
    slug = models.SlugField(default="")
    small_integer = models.SmallIntegerField(null=True)
    text = models.TextField(default="")
    time = models.TimeField(null=True, db_column="time_field")
    url = models.URLField(default="")
    fk = models.ForeignKey("self", models.CASCADE, null=True)
class O2OCaseTestModel(models.Model):
    o2o = models.OneToOneField(CaseTestModel, models.CASCADE, related_name="o2o_rel")
class FKCaseTestModel(models.Model):
    fk = models.ForeignKey(CaseTestModel, models.CASCADE, related_name="fk_rel")
class Client(models.Model):
    REGULAR = "R"
    GOLD = "G"
    PLATINUM = "P"
    ACCOUNT_TYPE_CHOICES = (
        (REGULAR, "Regular"),
        (GOLD, "Gold"),
        (PLATINUM, "Platinum"),
    registered_on = models.DateField()
    account_type = models.CharField(
        max_length=1,
        choices=ACCOUNT_TYPE_CHOICES,
        default=REGULAR,
class Classification(models.Model):
    code = models.CharField(max_length=10)
    name = models.CharField(max_length=40, blank=False, null=False)
    salary = models.PositiveIntegerField()
    department = models.CharField(max_length=40, blank=False, null=False)
    hire_date = models.DateField(blank=False, null=False)
    age = models.IntegerField(blank=False, null=False)
    classification = models.ForeignKey(
        "Classification", on_delete=models.CASCADE, null=True
    bonus = models.DecimalField(decimal_places=2, max_digits=15, null=True)
class PastEmployeeDepartment(models.Model):
        Employee, related_name="past_departments", on_delete=models.CASCADE
class Detail(models.Model):
    value = models.JSONField()
class RevisionableModel(models.Model):
    base = models.ForeignKey("self", models.SET_NULL, null=True)
    title = models.CharField(blank=True, max_length=255)
    when = models.DateTimeField(default=datetime.datetime.now)
    def save(self, *args, force_insert=False, force_update=False, **kwargs):
        super().save(
            *args, force_insert=force_insert, force_update=force_update, **kwargs
        if not self.base:
            self.base = self
    def new_revision(self):
        new_revision = copy.copy(self)
        new_revision.pk = None
        return new_revision
class Order(models.Model):
    created_by = models.ForeignKey(User, models.CASCADE)
class TestObject(models.Model):
    first = models.CharField(max_length=20)
    second = models.CharField(max_length=20)
    third = models.CharField(max_length=20)
        return "TestObject: %s,%s,%s" % (self.first, self.second, self.third)
Callable defaults
You can pass callable objects as the ``default`` parameter to a field. When
the object is created without an explicit value passed in, Django will call
the method to determine the default value.
This example uses ``datetime.datetime.now`` as the default for the ``pub_date``
from django.db.models.functions import Coalesce, ExtractYear, Now, Pi
from django.db.models.lookups import GreaterThan
    pub_date = models.DateTimeField(default=datetime.now)
class DBArticle(models.Model):
    Values or expressions can be passed as the db_default parameter to a field.
    When the object is created without an explicit value passed in, the
    database will insert the default value automatically.
    headline = models.CharField(max_length=100, db_default="Default headline")
    pub_date = models.DateTimeField(db_default=Now())
    cost = models.DecimalField(
        max_digits=3, decimal_places=2, db_default=Decimal("3.33")
class DBDefaults(models.Model):
    both = models.IntegerField(default=1, db_default=2)
    null = models.FloatField(null=True, db_default=1.1)
class DBDefaultsFunction(models.Model):
    number = models.FloatField(db_default=Pi())
    year = models.IntegerField(db_default=ExtractYear(Now()))
    added = models.FloatField(db_default=Pi() + 4.5)
    multiple_subfunctions = models.FloatField(db_default=Coalesce(4.5, Pi()))
    case_when = models.IntegerField(
        db_default=models.Case(models.When(GreaterThan(2, 1), then=3), default=4)
class DBDefaultsPK(models.Model):
    language_code = models.CharField(primary_key=True, max_length=2, db_default="en")
class DBDefaultsFK(models.Model):
    language_code = models.ForeignKey(
        DBDefaultsPK, db_default="fr", on_delete=models.CASCADE
Storing files according to a custom storage system
``FileField`` and its variations can take a ``storage`` argument to specify how
and where files should be stored.
from django.core.files.storage import FileSystemStorage, default_storage
class CustomValidNameStorage(FileSystemStorage):
        # mark the name to show that this was called
        return name + "_valid"
temp_storage_location = tempfile.mkdtemp()
temp_storage = FileSystemStorage(location=temp_storage_location)
def callable_storage():
    return temp_storage
def callable_default_storage():
    return default_storage
class CallableStorage(FileSystemStorage):
        # no-op implementation.
class LazyTempStorage(LazyObject):
        self._wrapped = temp_storage
class Storage(models.Model):
    def custom_upload_to(self, filename):
        return "foo"
    def random_upload_to(self, filename):
        # This returns a different result each time,
        # to make sure it only gets called once.
        return "%s/%s" % (random.randint(100, 999), filename)
    def pathlib_upload_to(self, filename):
        return Path("bar") / filename
    normal = models.FileField(storage=temp_storage, upload_to="tests")
    custom = models.FileField(storage=temp_storage, upload_to=custom_upload_to)
    pathlib_callable = models.FileField(
        storage=temp_storage, upload_to=pathlib_upload_to
    pathlib_direct = models.FileField(storage=temp_storage, upload_to=Path("bar"))
    random = models.FileField(storage=temp_storage, upload_to=random_upload_to)
    custom_valid_name = models.FileField(
        storage=CustomValidNameStorage(location=temp_storage_location),
        upload_to=random_upload_to,
    storage_callable = models.FileField(
        storage=callable_storage, upload_to="storage_callable"
    storage_callable_class = models.FileField(
        storage=CallableStorage, upload_to="storage_callable_class"
    storage_callable_default = models.FileField(
        storage=callable_default_storage, upload_to="storage_callable_default"
    default = models.FileField(
        storage=temp_storage, upload_to="tests", default="tests/default.txt"
    db_default = models.FileField(
        storage=temp_storage, upload_to="tests", db_default="tests/db_default.txt"
    empty = models.FileField(storage=temp_storage)
    limited_length = models.FileField(
        storage=temp_storage, upload_to="tests", max_length=20
    extended_length = models.FileField(
        storage=temp_storage, upload_to="tests", max_length=1024
    lazy_storage = models.FileField(storage=LazyTempStorage(), upload_to="tests")
class FileModel(models.Model):
    testfile = models.FileField(upload_to="test_upload")
    name = models.CharField(max_length=50, unique=True)
    favorite_books = models.ManyToManyField(
        related_name="preferred_by_authors",
        related_query_name="preferred_by_authors",
class Editor(models.Model):
    AVAILABLE = "available"
    RESERVED = "reserved"
    RENTED = "rented"
    STATES = (
        (AVAILABLE, "Available"),
        (RESERVED, "reserved"),
        (RENTED, "Rented"),
    title = models.CharField(max_length=255)
        related_name="books",
        related_query_name="book",
    editor = models.ForeignKey(Editor, models.CASCADE)
    number_editor = models.IntegerField(default=-1)
    editor_number = models.IntegerField(default=-2)
    generic_author = GenericRelation(Author)
    state = models.CharField(max_length=9, choices=STATES, default=AVAILABLE)
class Borrower(models.Model):
    NEW = "new"
    STOPPED = "stopped"
        (NEW, "New"),
        (STOPPED, "Stopped"),
    borrower = models.ForeignKey(
        Borrower,
        related_name="reservations",
        related_query_name="reservation",
    book = models.ForeignKey(
        Book,
    state = models.CharField(max_length=7, choices=STATES, default=NEW)
class RentalSession(models.Model):
        related_name="rental_sessions",
        related_query_name="rental_session",
class Seller(models.Model):
class Currency(models.Model):
    currency = models.CharField(max_length=3)
class ExchangeRate(models.Model):
    rate_date = models.DateField()
    from_currency = models.ForeignKey(
        Currency,
        related_name="rates_from",
    to_currency = models.ForeignKey(
        related_name="rates_to",
    rate = models.DecimalField(max_digits=6, decimal_places=4)
class BookDailySales(models.Model):
    book = models.ForeignKey(Book, models.CASCADE, related_name="daily_sales")
    sale_date = models.DateField()
    currency = models.ForeignKey(Currency, models.CASCADE)
    seller = models.ForeignKey(Seller, models.CASCADE)
    sales = models.DecimalField(max_digits=10, decimal_places=2)
Fixtures.
Fixtures are a way of loading data into the database in bulk. Fixure data
can be stored in any serializable format (including JSON and XML). Fixtures
are identified by name, and are stored in either a directory named 'fixtures'
in the application directory, or in one of the directories named in the
``FIXTURE_DIRS`` setting.
from django.contrib.auth.models import Permission
        ordering = ("-pub_date", "headline")
class Blog(models.Model):
    featured = models.ForeignKey(
        Article, models.CASCADE, related_name="fixtures_featured_set"
    articles = models.ManyToManyField(
        Article, blank=True, related_name="fixtures_articles_set"
    tagged_type = models.ForeignKey(
        ContentType, models.CASCADE, related_name="fixtures_tag_set"
    tagged_id = models.PositiveIntegerField(default=0)
    tagged = GenericForeignKey(ct_field="tagged_type", fk_field="tagged_id")
        return '<%s: %s> tagged "%s"' % (
            self.tagged.__class__.__name__,
            self.tagged,
    name = models.CharField(max_length=100, unique=True)
class SpyManager(PersonManager):
        return super().get_queryset().filter(cover_blown=False)
class Spy(Person):
    objects = SpyManager()
    cover_blown = models.BooleanField(default=False)
class ProxySpy(Spy):
class VisaManager(models.Manager):
        return super().get_queryset().prefetch_related("permissions")
class Visa(models.Model):
    person = models.ForeignKey(Person, models.CASCADE)
    permissions = models.ManyToManyField(Permission, blank=True)
    objects = VisaManager()
        return "%s %s" % (
            self.person.name,
            ", ".join(p.name for p in self.permissions.all()),
    authors = models.ManyToManyField(Person)
        authors = " and ".join(a.name for a in self.authors.all())
        return "%s by %s" % (self.name, authors) if authors else self.name
class PrimaryKeyUUIDModel(models.Model):
class NaturalKeyManager(models.Manager):
    def get_by_natural_key(self, key):
        return self.get(key=key)
class NaturalKeyThing(models.Model):
    key = models.CharField(max_length=100, unique=True)
    other_thing = models.ForeignKey(
        "NaturalKeyThing", on_delete=models.CASCADE, null=True
    other_things = models.ManyToManyField(
        "NaturalKeyThing", related_name="thing_m2m_set"
    objects = NaturalKeyManager()
        return self.key
class Animal(models.Model):
    name = models.CharField(max_length=150)
    latin_name = models.CharField(max_length=150)
    count = models.IntegerField()
    # use a non-default name for the default manager
    specimens = models.Manager()
class Plant(models.Model):
        # For testing when upper case letter in app name; regression for #4057
        db_table = "Fixtures_regress_plant"
class Stuff(models.Model):
    name = models.CharField(max_length=20, null=True)
    owner = models.ForeignKey(User, models.SET_NULL, null=True)
        return self.name + " is owned by " + str(self.owner)
class Absolute(models.Model):
        ordering = ("id",)
    data = models.CharField(max_length=10)
# Models to regression test #7572, #20820
class Channel(models.Model):
    channels = models.ManyToManyField(Channel)
# Subclass of a model with a ManyToManyField for test_ticket_20820
class SpecialArticle(Article):
# Models to regression test #22421
class CommonFeature(Article):
class Feature(CommonFeature):
# Models to regression test #11428
class WidgetProxy(Widget):
# Check for forward references in FKs and M2Ms with natural keys
class TestManager(models.Manager):
        return self.get(name=key)
    main = models.ForeignKey("self", models.SET_NULL, null=True)
    objects = TestManager()
    # Person doesn't actually have a dependency on store, but we need to define
    # one to test the behavior of the dependency resolution algorithm.
    natural_key.dependencies = ["fixtures_regress.store"]
    author = models.ForeignKey(Person, models.CASCADE)
    stores = models.ManyToManyField(Store)
        return "%s by %s (available at %s)" % (
            self.author.name,
            ", ".join(s.name for s in self.stores.all()),
class NaturalKeyWithFKDependencyManager(models.Manager):
    def get_by_natural_key(self, name, author):
        return self.get(name=name, author__name=author)
class NaturalKeyWithFKDependency(models.Model):
    objects = NaturalKeyWithFKDependencyManager()
        unique_together = ["name", "author"]
        return (self.name,) + self.author.natural_key()
    natural_key.dependencies = ["fixtures_regress.Person"]
class NKManager(models.Manager):
    def get_by_natural_key(self, data):
        return self.get(data=data)
class NKChild(Parent):
    data = models.CharField(max_length=10, unique=True)
    objects = NKManager()
        return (self.data,)
        return "NKChild %s:%s" % (self.name, self.data)
class RefToNKChild(models.Model):
    text = models.CharField(max_length=10)
    nk_fk = models.ForeignKey(NKChild, models.CASCADE, related_name="ref_fks")
    nk_m2m = models.ManyToManyField(NKChild, related_name="ref_m2ms")
        return "%s: Reference to %s [%s]" % (
            self.text,
            self.nk_fk,
            ", ".join(str(o) for o in self.nk_m2m.all()),
# ome models with pathological circular dependencies
class Circle1(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle2"]
class Circle2(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle1"]
class Circle3(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle3"]
class Circle4(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle5"]
class Circle5(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle6"]
class Circle6(models.Model):
    natural_key.dependencies = ["fixtures_regress.circle4"]
class ExternalDependency(models.Model):
    natural_key.dependencies = ["fixtures_regress.book"]
# Model for regression test of #11101
class Thingy(models.Model):
class M2MToSelf(models.Model):
    parent = models.ManyToManyField("self", blank=True)
class BaseNKModel(models.Model):
    Base model with a natural_key and a manager with `get_by_natural_key`
    data = models.CharField(max_length=20, unique=True)
class M2MSimpleA(BaseNKModel):
    b_set = models.ManyToManyField("M2MSimpleB")
class M2MSimpleB(BaseNKModel):
class M2MSimpleCircularA(BaseNKModel):
    b_set = models.ManyToManyField("M2MSimpleCircularB")
class M2MSimpleCircularB(BaseNKModel):
    a_set = models.ManyToManyField("M2MSimpleCircularA")
class M2MComplexA(BaseNKModel):
    b_set = models.ManyToManyField("M2MComplexB", through="M2MThroughAB")
class M2MComplexB(BaseNKModel):
class M2MThroughAB(BaseNKModel):
    a = models.ForeignKey(M2MComplexA, models.CASCADE)
    b = models.ForeignKey(M2MComplexB, models.CASCADE)
class M2MComplexCircular1A(BaseNKModel):
    b_set = models.ManyToManyField(
        "M2MComplexCircular1B", through="M2MCircular1ThroughAB"
class M2MComplexCircular1B(BaseNKModel):
    c_set = models.ManyToManyField(
        "M2MComplexCircular1C", through="M2MCircular1ThroughBC"
class M2MComplexCircular1C(BaseNKModel):
    a_set = models.ManyToManyField(
        "M2MComplexCircular1A", through="M2MCircular1ThroughCA"
class M2MCircular1ThroughAB(BaseNKModel):
    a = models.ForeignKey(M2MComplexCircular1A, models.CASCADE)
    b = models.ForeignKey(M2MComplexCircular1B, models.CASCADE)
class M2MCircular1ThroughBC(BaseNKModel):
    c = models.ForeignKey(M2MComplexCircular1C, models.CASCADE)
class M2MCircular1ThroughCA(BaseNKModel):
class M2MComplexCircular2A(BaseNKModel):
        "M2MComplexCircular2B", through="M2MCircular2ThroughAB"
class M2MComplexCircular2B(BaseNKModel):
    # Fake the dependency for a circularity
    natural_key.dependencies = ["fixtures_regress.M2MComplexCircular2A"]
class M2MCircular2ThroughAB(BaseNKModel):
    a = models.ForeignKey(M2MComplexCircular2A, models.CASCADE)
    b = models.ForeignKey(M2MComplexCircular2B, models.CASCADE)
Tests for forcing insert and update queries (instead of Django's normal
automatic behavior).
class Counter(models.Model):
class InheritedCounter(Counter):
    tag = models.CharField(max_length=10)
class ProxyCounter(Counter):
class SubCounter(Counter):
class SubSubCounter(SubCounter):
class WithCustomPK(models.Model):
class OtherSubCounter(Counter):
    other_counter_ptr = models.OneToOneField(
        Counter, primary_key=True, parent_link=True, on_delete=models.CASCADE
class DiamondSubSubCounter(SubCounter, OtherSubCounter):
callable_default_counter = itertools.count()
def callable_default():
    return next(callable_default_counter)
temp_storage = FileSystemStorage(location=tempfile.mkdtemp())
class BoundaryModel(models.Model):
    positive_integer = models.PositiveIntegerField(null=True, blank=True)
class Defaults(models.Model):
    name = models.CharField(max_length=255, default="class default value")
    def_date = models.DateField(default=datetime.date(1980, 1, 1))
    callable_default = models.IntegerField(default=callable_default)
class ChoiceModel(models.Model):
    """For ModelChoiceField and ModelMultipleChoiceField tests."""
        ("", "No Preference"),
        ("f", "Foo"),
        ("b", "Bar"),
    INTEGER_CHOICES = [
        (None, "No Preference"),
        (1, "Foo"),
        (2, "Bar"),
    STRING_CHOICES_WITH_NONE = [
    choice = models.CharField(max_length=2, blank=True, choices=CHOICES)
    choice_string_w_none = models.CharField(
        max_length=2, blank=True, null=True, choices=STRING_CHOICES_WITH_NONE
    choice_integer = models.IntegerField(choices=INTEGER_CHOICES, blank=True, null=True)
class ChoiceOptionModel(models.Model):
    Destination for ChoiceFieldModel's ForeignKey.
    Can't reuse ChoiceModel because error_message tests require that it have no
        return "ChoiceOption %s" % self.pk
def choice_default():
    return ChoiceOptionModel.objects.get_or_create(name="default")[0].pk
def choice_default_list():
    return [choice_default()]
def int_default():
def int_list_default():
    return [1]
class ChoiceFieldModel(models.Model):
    """Model with ForeignKey to another model, for testing ModelForm
    generation with ModelChoiceField."""
    choice = models.ForeignKey(
        ChoiceOptionModel,
        default=choice_default,
    choice_int = models.ForeignKey(
        related_name="choice_int",
        default=int_default,
    multi_choice = models.ManyToManyField(
        related_name="multi_choice",
        default=choice_default_list,
    multi_choice_int = models.ManyToManyField(
        related_name="multi_choice_int",
        default=int_list_default,
class OptionalMultiChoiceModel(models.Model):
        related_name="not_relevant",
    multi_choice_optional = models.ManyToManyField(
        related_name="not_relevant2",
    file = models.FileField(storage=temp_storage, upload_to="tests")
class Cash(decimal.Decimal):
    currency = "USD"
class CashField(models.DecimalField):
        kwargs["max_digits"] = 20
        kwargs["decimal_places"] = 2
    def from_db_value(self, value, expression, connection):
        cash = Cash(value)
        cash.vendor = connection.vendor
        return cash
class CashModel(models.Model):
    cash = CashField()
class Episode(models.Model):
    length = models.CharField(max_length=100, blank=True)
    author = models.CharField(max_length=100, blank=True)
    Media that can associated to any object.
    description = models.CharField(max_length=100, blank=True)
    keywords = models.CharField(max_length=100, blank=True)
# Generic inline with unique_together
class PhoneNumber(models.Model):
    phone_number = models.CharField(max_length=30)
    category = models.ForeignKey(Category, models.SET_NULL, null=True, blank=True)
        unique_together = (
                "content_type",
                "object_id",
                "phone_number",
    phone_numbers = GenericRelation(PhoneNumber, related_query_name="phone_numbers")
# Generic inline with can_delete=False
class EpisodePermanent(Episode):
Generic relations
Generic relations let an object have a foreign key to any object through a
content-type/object-id field. A ``GenericForeignKey`` field can point to any
object, be it animal, vegetable, or mineral.
The canonical example is tags (although this example implementation is *far*
from complete).
    """A tag on an item."""
        ordering = ["tag", "content_type__model"]
class ValuableTaggedItem(TaggedItem):
    value = models.PositiveIntegerField()
class AbstractComparison(models.Model):
    comparative = models.CharField(max_length=50)
    content_type1 = models.ForeignKey(
        ContentType, models.CASCADE, related_name="comparative1_set"
    object_id1 = models.PositiveIntegerField()
    first_obj = GenericForeignKey(ct_field="content_type1", fk_field="object_id1")
class Comparison(AbstractComparison):
    A model that tests having multiple GenericForeignKeys. One is defined
    through an inherited abstract model and one defined directly on this class.
    content_type2 = models.ForeignKey(
        ContentType, models.CASCADE, related_name="comparative2_set"
    object_id2 = models.PositiveIntegerField()
    other_obj = GenericForeignKey(ct_field="content_type2", fk_field="object_id2")
        return "%s is %s than %s" % (self.first_obj, self.comparative, self.other_obj)
    common_name = models.CharField(max_length=150)
    tags = GenericRelation(TaggedItem, related_query_name="animal")
    comparisons = GenericRelation(
        Comparison, object_id_field="object_id1", content_type_field="content_type1"
        return self.common_name
class Vegetable(models.Model):
    is_yucky = models.BooleanField(default=True)
class Carrot(Vegetable):
class Mineral(models.Model):
    hardness = models.PositiveSmallIntegerField()
    # note the lack of an explicit GenericRelation here...
class GeckoManager(models.Manager):
        return super().get_queryset().filter(has_tail=True)
class Gecko(models.Model):
    has_tail = models.BooleanField(default=False)
    objects = GeckoManager()
# To test fix for #11263
class Rock(Mineral):
class ValuableRock(Mineral):
    tags = GenericRelation(ValuableTaggedItem)
class ManualPK(models.Model):
    tags = GenericRelation(TaggedItem, related_query_name="manualpk")
class ForProxyModelModel(models.Model):
    obj = GenericForeignKey(for_concrete_model=False)
    title = models.CharField(max_length=255, null=True)
class ForConcreteModelModel(models.Model):
    obj = GenericForeignKey()
class ConcreteRelatedModel(models.Model):
    bases = GenericRelation(ForProxyModelModel, for_concrete_model=False)
class ProxyRelatedModel(ConcreteRelatedModel):
# To test fix for #7551
class AllowsNullGFK(models.Model):
    content_type = models.ForeignKey(ContentType, models.SET_NULL, null=True)
    "Link",
    "Place",
    "Restaurant",
    "CharLink",
    "TextLink",
    "OddRelation1",
    "OddRelation2",
    "Note",
    "Company",
class LinkProxy(Link):
    links = GenericRelation(Link, related_query_name="places")
    link_proxy = GenericRelation(LinkProxy)
class Cafe(Restaurant):
class Address(models.Model):
    street = models.CharField(max_length=80)
    city = models.CharField(max_length=50)
    state = models.CharField(max_length=2)
    zipcode = models.CharField(max_length=5)
    account = models.IntegerField(primary_key=True)
    addresses = GenericRelation(Address)
class CharLink(models.Model):
    object_id = models.CharField(max_length=100)
    value = models.CharField(max_length=250)
class TextLink(models.Model):
    object_id = models.TextField()
class OddRelation1(models.Model):
    clinks = GenericRelation(CharLink)
class OddRelation2(models.Model):
    tlinks = GenericRelation(TextLink)
# models for test_q_object_or:
class Note(models.Model):
    notes = GenericRelation(Note)
class Organization(models.Model):
    contacts = models.ManyToManyField(Contact, related_name="organizations")
    links = GenericRelation(Link)
class Team(models.Model):
class Guild(models.Model):
        ContentType, models.CASCADE, related_name="g_r_r_tags"
    object_id = models.CharField(max_length=15)
    label = models.CharField(max_length=15)
class Board(models.Model):
    name = models.CharField(primary_key=True, max_length=25)
class SpecialGenericRelation(GenericRelation):
        self.editable = True
        self.save_form_data_calls = 0
    def save_form_data(self, *args, **kwargs):
        self.save_form_data_calls += 1
class HasLinks(models.Model):
    links = SpecialGenericRelation(Link, related_query_name="targets")
class HasLinkThing(HasLinks):
    flag = models.BooleanField(null=True)
    a = GenericRelation(A)
class C(models.Model):
    b = models.ForeignKey(B, models.CASCADE)
class D(models.Model):
    b = models.ForeignKey(B, models.SET_NULL, null=True)
# Ticket #22998
class Node(models.Model):
    content = GenericForeignKey("content_type", "object_id")
class Content(models.Model):
    nodes = GenericRelation(Node)
    related_obj = models.ForeignKey("Related", models.CASCADE)
class Related(models.Model):
def prevent_deletes(sender, instance, **kwargs):
    raise models.ProtectedError("Not allowed to delete.", [instance])
models.signals.pre_delete.connect(prevent_deletes, sender=Node)
from django.db.models.manager import BaseManager
        verbose_name = "professional artist"
        verbose_name_plural = "professional artists"
        return reverse("artist_detail", kwargs={"pk": self.id})
class DoesNotExistQuerySet(QuerySet):
        raise Author.DoesNotExist
DoesNotExistBookManager = BaseManager.from_queryset(DoesNotExistQuerySet)
    does_not_exist = DoesNotExistBookManager()
        ordering = ["-pubdate"]
class Page(models.Model):
    template = models.CharField(max_length=255)
class BookSigning(models.Model):
    event_date = models.DateTimeField()
    expire_date = models.DateField()
        get_latest_by = "pub_date"
    birthday = models.DateField()
    # Note that this model doesn't have "get_latest_by" set.
    article = models.ForeignKey(Article, on_delete=models.CASCADE)
    likes_count = models.PositiveIntegerField()
class OrderedArticle(models.Model):
        ordering = ["headline"]
# Ticket #23555 - model with an intentionally broken QuerySet.__iter__ method.
class IndexErrorQuerySet(models.QuerySet):
    Emulates the case when some internal code raises an unexpected
    IndexError.
        raise IndexError
class IndexErrorArticle(Article):
    objects = IndexErrorQuerySet.as_manager()
DB-API Shortcuts
``get_object_or_404()`` is a shortcut function to be used in view functions for
performing a ``get()`` lookup and raising a ``Http404`` exception if a
``DoesNotExist`` exception was raised during the ``get()`` call.
``get_list_or_404()`` is a shortcut function to be used in view functions for
performing a ``filter()`` lookup and raising a ``Http404`` exception if a
``DoesNotExist`` exception was raised during the ``filter()`` call.
class ArticleManager(models.Manager):
        return super().get_queryset().filter(authors__name__icontains="sir")
class AttributeErrorManager(models.Manager):
        raise AttributeError("AttributeErrorManager")
    by_a_sir = ArticleManager()
    attribute_error_objects = AttributeErrorManager()
    first_name = models.CharField(max_length=100, unique=True)
    defaults = models.TextField()
    create_defaults = models.TextField()
class DefaultPerson(models.Model):
    first_name = models.CharField(max_length=100, default="Anonymous")
class ManualPrimaryKeyTest(models.Model):
    data = models.CharField(max_length=100)
    person = models.ForeignKey(Person, models.CASCADE, primary_key=True)
    text = models.CharField(max_length=255, unique=True)
    tags = models.ManyToManyField(Tag)
    def capitalized_name_property(self):
    @capitalized_name_property.setter
    def capitalized_name_property(self, val):
        self.name = val.capitalize()
    def name_in_all_caps(self):
        return self.name.upper()
class Journalist(Author):
    specialty = models.CharField(max_length=100)
    authors = models.ManyToManyField(Author, related_name="books")
    publisher = models.ForeignKey(
        Publisher,
        db_column="publisher_id_column",
    updated = models.DateTimeField(auto_now=True)
from ..utils import gisfield_may_be_null
class NamedModel(models.Model):
class SouthTexasCity(NamedModel):
    "City model on projected coordinate system for South Texas."
    point = models.PointField(srid=32140)
    radius = models.IntegerField(default=10000)
class SouthTexasCityFt(NamedModel):
    "Same City model as above, but U.S. survey feet are the units."
    point = models.PointField(srid=2278)
class AustraliaCity(NamedModel):
    "City model for Australia, using WGS84."
    point = models.PointField()
    allowed_distance = models.FloatField(default=0.5)
    ref_point = models.PointField(null=True)
class CensusZipcode(NamedModel):
    "Model for a few South Texas ZIP codes (in original Census NAD83)."
    poly = models.PolygonField(srid=4269)
class SouthTexasZipcode(NamedModel):
    "Model for a few South Texas ZIP codes."
    poly = models.PolygonField(srid=32140, null=gisfield_may_be_null)
class Interstate(NamedModel):
    "Geodetic model for U.S. Interstates."
    path = models.LineStringField()
class SouthTexasInterstate(NamedModel):
    "Projected model for South Texas Interstates."
    path = models.LineStringField(srid=32140)
class City3D(NamedModel):
    point = models.PointField(dim=3)
    pointg = models.PointField(dim=3, geography=True)
        required_db_features = {"supports_3d_storage"}
class Interstate2D(NamedModel):
    line = models.LineStringField(srid=4269)
class Interstate3D(NamedModel):
    line = models.LineStringField(dim=3, srid=4269)
class InterstateProj2D(NamedModel):
    line = models.LineStringField(srid=32140)
class InterstateProj3D(NamedModel):
    line = models.LineStringField(dim=3, srid=32140)
class Polygon2D(NamedModel):
    poly = models.PolygonField(srid=32140)
class Polygon3D(NamedModel):
    poly = models.PolygonField(dim=3, srid=32140)
class Point2D(SimpleModel):
    point = models.PointField(null=True)
class Point3D(SimpleModel):
class MultiPoint3D(SimpleModel):
    mpoint = models.MultiPointField(dim=3)
from ..admin import admin
        app_label = "geoadmin"
class CityAdminCustomWidgetKwargs(admin.GISModelAdmin):
    gis_widget_kwargs = {
        "attrs": {
            "default_lat": 55,
            "default_lon": 37,
site = admin.AdminSite(name="gis_admin_modeladmin")
site.register(City, admin.ModelAdmin)
site_gis = admin.AdminSite(name="gis_admin_gismodeladmin")
site_gis.register(City, admin.GISModelAdmin)
site_gis_custom = admin.AdminSite(name="gis_admin_gismodeladmin")
site_gis_custom.register(City, CityAdminCustomWidgetKwargs)
class Country(NamedModel):
    mpoly = models.MultiPolygonField()  # SRID, by default, is 4326
class CountryWebMercator(NamedModel):
    mpoly = models.MultiPolygonField(srid=3857)
class City(NamedModel):
        app_label = "geoapp"
# This is an inherited model from City
class PennsylvaniaCity(City):
    county = models.CharField(max_length=30)
    founded = models.DateTimeField(null=True)
class State(NamedModel):
    poly = models.PolygonField(
        null=gisfield_may_be_null
    )  # Allowing NULL geometries here.
class Track(NamedModel):
    line = models.LineStringField()
class MultiFields(NamedModel):
    poly = models.PolygonField()
class UniqueTogetherModel(models.Model):
    city = models.CharField(max_length=30)
        unique_together = ("city", "point")
        required_db_features = ["supports_geometry_field_unique_index"]
class Truth(models.Model):
    val = models.BooleanField(default=False)
class Feature(NamedModel):
    geom = models.GeometryField()
class ThreeDimensionalFeature(NamedModel):
    geom = models.GeometryField(dim=3)
class MinusOneSRID(models.Model):
    geom = models.PointField(srid=-1)  # Minus one SRID.
class NonConcreteField(models.IntegerField):
        attname, column = super().get_attname_column()
        return attname, None
class NonConcreteModel(NamedModel):
    non_concrete = NonConcreteField()
    point = models.PointField(geography=True)
class ManyPointModel(NamedModel):
    point1 = models.PointField()
    point2 = models.PointField()
    point3 = models.PointField(srid=3857)
class Points(models.Model):
    geom = models.MultiPointField()
class Lines(models.Model):
    geom = models.MultiLineStringField()
class GeometryCollectionModel(models.Model):
    geom = models.GeometryCollectionField()
        app_label = "geogapp"
class CityUnique(NamedModel):
    point = models.PointField(geography=True, unique=True)
            "supports_geography",
            "supports_geometry_field_unique_index",
class Zipcode(NamedModel):
    poly = models.PolygonField(geography=True)
class County(NamedModel):
    state = models.CharField(max_length=20)
    mpoly = models.MultiPolygonField(geography=True)
        return " County, ".join([self.name, self.state])
class AllOGRFields(models.Model):
    f_decimal = models.FloatField()
    f_float = models.FloatField()
    f_int = models.IntegerField()
    f_char = models.CharField(max_length=10)
    f_date = models.DateField()
    f_datetime = models.DateTimeField()
    f_time = models.TimeField()
    geom = models.PolygonField()
class Fields3D(models.Model):
    line = models.LineStringField(dim=3)
    poly = models.PolygonField(dim=3)
    mpoly = models.MultiPolygonField(srid=4269, null=True)  # Multipolygon in NAD83
class CountyFeat(NamedModel):
    name_txt = models.TextField(default="")
    name_short = models.CharField(max_length=5)
    population = models.IntegerField()
    density = models.DecimalField(max_digits=7, decimal_places=1)
    dt = models.DateField()
        app_label = "layermap"
    length = models.DecimalField(max_digits=6, decimal_places=2)
# Same as `City` above, but for testing model inheritance.
class CityBase(NamedModel):
class ICity1(CityBase):
    class Meta(CityBase.Meta):
class ICity2(ICity1):
    dt_time = models.DateTimeField(auto_now=True)
    class Meta(ICity1.Meta):
class Invalid(models.Model):
class HasNulls(models.Model):
    uuid = models.UUIDField(primary_key=True, editable=False)
    geom = models.PolygonField(srid=4326, blank=True, null=True)
    datetime = models.DateTimeField(blank=True, null=True)
    integer = models.IntegerField(blank=True, null=True)
    num = models.FloatField(blank=True, null=True)
    boolean = models.BooleanField(blank=True, null=True)
    name = models.CharField(blank=True, null=True, max_length=20)
class DoesNotAllowNulls(models.Model):
    geom = models.PolygonField(srid=4326)
    datetime = models.DateTimeField()
    boolean = models.BooleanField()
# Mapping dictionaries for the models above.
co_mapping = {
    # ForeignKey's use another mapping dictionary for the _related_ Model
    # (State in this case).
    "state": {"name": "State"},
    "mpoly": "MULTIPOLYGON",  # Will convert POLYGON features into MULTIPOLYGONS.
cofeat_mapping = {
    "poly": "POLYGON",
city_mapping = {
    "population": "Population",
    "density": "Density",
    "dt": "Created",
    "point": "POINT",
inter_mapping = {
    "length": "Length",
    "path": "LINESTRING",
has_nulls_mapping = {
    "geom": "POLYGON",
    "uuid": "uuid",
    "datetime": "datetime",
    "integer": "integer",
    "num": "num",
    "boolean": "boolean",
class RasterModel(models.Model):
    rast = models.RasterField(
        "A Verbose Raster Name", null=True, srid=4326, spatial_index=True, blank=True
    rastprojected = models.RasterField("A Projected Raster Table", srid=3086, null=True)
    geom = models.PointField(null=True)
        required_db_features = ["supports_raster"]
class RasterRelatedModel(models.Model):
    rastermodel = models.ForeignKey(RasterModel, models.CASCADE)
class Location(SimpleModel):
        return self.point.wkt
class City(SimpleModel):
class AugmentedLocation(Location):
    extra_text = models.TextField(blank=True)
class DirectoryEntry(SimpleModel):
    listing_text = models.CharField(max_length=50)
    location = models.ForeignKey(AugmentedLocation, models.CASCADE)
class Parcel(SimpleModel):
    center1 = models.PointField()
    # Throwing a curveball w/`db_column` here.
    center2 = models.PointField(srid=2276, db_column="mycenter")
    border1 = models.PolygonField()
    border2 = models.PolygonField(srid=2276)
class Author(SimpleModel):
    dob = models.DateField()
class Article(SimpleModel):
    author = models.ForeignKey(Author, models.CASCADE, unique=True)
class Book(SimpleModel):
    author = models.ForeignKey(Author, models.SET_NULL, related_name="books", null=True)
class Event(SimpleModel):
    when = models.DateTimeField()
class TestModel(models.Model):
    text = models.CharField(max_length=10, default=_("Anything"))
    date_added = models.DateTimeField(default=datetime(1799, 1, 31, 23, 59, 59, 0))
    cents_paid = models.DecimalField(max_digits=4, decimal_places=2)
    products_delivered = models.IntegerField()
        verbose_name = _("Company")
string = _("This app has no locale directory")
string = _("This app has a locale directory")
class CurrentTranslation(models.ForeignObject):
    Creates virtual relation to the translation with model cache enabled.
    # Avoid validation
    requires_unique_target = False
    def __init__(self, to, on_delete, from_fields, to_fields, **kwargs):
        # Disable reverse relation
        kwargs["related_name"] = "+"
        # Set unique to enable model cache.
        kwargs["unique"] = True
        super().__init__(to, on_delete, from_fields, to_fields, **kwargs)
class ArticleTranslation(models.Model):
    article = models.ForeignKey("indexes.Article", models.CASCADE)
    article_no_constraint = models.ForeignKey(
        "indexes.Article", models.CASCADE, db_constraint=False, related_name="+"
    language = models.CharField(max_length=10, unique=True)
    # Add virtual relation to the ArticleTranslation model.
    translation = CurrentTranslation(
        ArticleTranslation, models.CASCADE, ["id"], ["article"]
        indexes = [models.Index(fields=["headline", "pub_date"])]
class IndexedArticle(models.Model):
    headline = models.CharField(max_length=100, db_index=True)
    body = models.TextField(db_index=True)
    slug = models.CharField(max_length=40, unique=True)
        required_db_features = {"supports_index_on_text_field"}
class IndexedArticle2(models.Model):
    body = models.TextField()
    mother = models.ForeignKey(Parent, models.CASCADE, related_name="mothers_children")
    father = models.ForeignKey(Parent, models.CASCADE, related_name="fathers_children")
    school = models.ForeignKey(School, models.CASCADE)
            models.UniqueConstraint("mother", "father", name="unique_parents"),
class Poet(models.Model):
class Poem(models.Model):
    poet = models.ForeignKey(Poet, models.CASCADE)
        unique_together = ("poet", "name")
from django.db import connection, models
from django.db.models.functions import Lower
class People(models.Model):
    parent = models.ForeignKey("self", models.CASCADE)
class Message(models.Model):
    from_field = models.ForeignKey(People, models.CASCADE, db_column="from_id")
    author = models.ForeignKey(People, models.CASCADE, related_name="message_authors")
class PeopleData(models.Model):
    people_pk = models.ForeignKey(People, models.CASCADE, primary_key=True)
    ssn = models.CharField(max_length=11)
class PeopleMoreData(models.Model):
    people_unique = models.ForeignKey(People, models.CASCADE, unique=True)
    message = models.ForeignKey(Message, models.CASCADE, blank=True, null=True)
    license = models.CharField(max_length=255)
class ForeignKeyToField(models.Model):
    to_field_fk = models.ForeignKey(
        PeopleMoreData,
        to_field="people_unique",
class DigitsInColumnName(models.Model):
    all_digits = models.CharField(max_length=11, db_column="123")
    leading_digit = models.CharField(max_length=11, db_column="4extra")
    leading_digits = models.CharField(max_length=11, db_column="45extra")
class SpecialName(models.Model):
    field = models.IntegerField(db_column="field")
    # Underscores
    field_field_0 = models.IntegerField(db_column="Field_")
    field_field_1 = models.IntegerField(db_column="Field__")
    field_field_2 = models.IntegerField(db_column="__field")
    # Other chars
    prc_x = models.IntegerField(db_column="prc(%) x")
    non_ascii = models.IntegerField(db_column="tamaño")
        db_table = "inspectdb_special.table name"
class PascalCaseName(models.Model):
        db_table = "inspectdb_pascal.PascalCase"
class ColumnTypes(models.Model):
    id = models.AutoField(primary_key=True)
    big_int_field = models.BigIntegerField()
    bool_field = models.BooleanField(default=False)
    null_bool_field = models.BooleanField(null=True)
    char_field = models.CharField(max_length=10)
    null_char_field = models.CharField(max_length=10, blank=True, null=True)
    date_field = models.DateField()
    date_time_field = models.DateTimeField()
    decimal_field = models.DecimalField(max_digits=6, decimal_places=1)
    email_field = models.EmailField()
    file_field = models.FileField(upload_to="unused")
    file_path_field = models.FilePathField()
    float_field = models.FloatField()
    int_field = models.IntegerField()
    gen_ip_address_field = models.GenericIPAddressField(protocol="ipv4")
    pos_big_int_field = models.PositiveBigIntegerField()
    pos_int_field = models.PositiveIntegerField()
    pos_small_int_field = models.PositiveSmallIntegerField()
    slug_field = models.SlugField()
    small_int_field = models.SmallIntegerField()
    text_field = models.TextField()
    time_field = models.TimeField()
    url_field = models.URLField()
    uuid_field = models.UUIDField()
class JSONFieldColumnType(models.Model):
    json_field = models.JSONField()
    null_json_field = models.JSONField(blank=True, null=True)
            "can_introspect_json_field",
            "supports_json_field",
test_collation = SimpleLazyObject(
    lambda: connection.features.test_collations.get("non_default")
class CharFieldDbCollation(models.Model):
    char_field = models.CharField(max_length=10, db_collation=test_collation)
        required_db_features = {"supports_collation_on_charfield"}
class TextFieldDbCollation(models.Model):
    text_field = models.TextField(db_collation=test_collation)
        required_db_features = {"supports_collation_on_textfield"}
class CharFieldUnlimited(models.Model):
    char_field = models.CharField(max_length=None)
        required_db_features = {"supports_unlimited_charfield"}
class DecimalFieldNoPrec(models.Model):
    decimal_field_no_precision = models.DecimalField(
        max_digits=None, decimal_places=None
        required_db_features = {"supports_no_precision_decimalfield"}
class UniqueTogether(models.Model):
    field1 = models.IntegerField()
    field2 = models.CharField(max_length=10)
    from_field = models.IntegerField(db_column="from")
    non_unique = models.IntegerField(db_column="non__unique_column")
    non_unique_0 = models.IntegerField(db_column="non_unique__column")
            ("field1", "field2"),
            ("from_field", "field1"),
            ("non_unique", "non_unique_0"),
class FuncUniqueConstraint(models.Model):
                Lower("name"), models.F("rank"), name="index_lower_name"
        required_db_features = {"supports_expression_indexes"}
class DbComment(models.Model):
    rank = models.IntegerField(db_comment="'Rank' column comment")
        db_table_comment = "Custom table comment"
        required_db_features = {"supports_comments"}
class CompositePKModel(models.Model):
    pk = models.CompositePrimaryKey("column_1", "column_2")
    column_1 = models.IntegerField()
    column_2 = models.IntegerField()
class DbOnDeleteModel(models.Model):
    fk_do_nothing = models.ForeignKey(UniqueTogether, on_delete=models.DO_NOTHING)
    fk_db_cascade = models.ForeignKey(ColumnTypes, on_delete=models.DB_CASCADE)
    fk_set_null = models.ForeignKey(
        DigitsInColumnName, on_delete=models.DB_SET_NULL, null=True
            "supports_on_delete_db_cascade",
            "supports_on_delete_db_null",
class District(models.Model):
    city = models.ForeignKey(City, models.CASCADE, primary_key=True)
    email = models.EmailField()
    facebook_user_id = models.BigIntegerField(null=True)
    raw_data = models.BinaryField(null=True)
    small_int = models.SmallIntegerField()
    interval = models.DurationField()
        unique_together = ("first_name", "last_name")
    body = models.TextField(default="")
    response_to = models.ForeignKey("self", models.SET_NULL, null=True)
    unmanaged_reporters = models.ManyToManyField(
        Reporter, through="ArticleReporter", related_name="+"
            models.Index(fields=["headline", "pub_date"]),
            models.Index(fields=["headline", "response_to", "pub_date", "reporter"]),
class ArticleReporter(models.Model):
    article = models.ForeignKey(Article, models.CASCADE)
    ref = models.UUIDField(unique=True)
    article = models.ForeignKey(Article, models.CASCADE, db_index=True)
                fields=["article", "email", "pub_date"],
                name="article_email_pub_date_uniq",
            models.Index(fields=["email", "pub_date"], name="email_pub_date_idx"),
class CheckConstraintModel(models.Model):
    up_votes = models.PositiveIntegerField()
    voting_number = models.PositiveIntegerField(unique=True)
                name="up_votes_gte_0_check", condition=models.Q(up_votes__gte=0)
class UniqueConstraintConditionModel(models.Model):
                name="cond_name_without_color_uniq",
class DbCommentModel(models.Model):
    name = models.CharField(max_length=15, db_comment="'Name' column comment")
class DbOnDeleteCascadeModel(models.Model):
    fk_do_nothing = models.ForeignKey(Country, on_delete=models.DO_NOTHING)
    fk_db_cascade = models.ForeignKey(City, on_delete=models.DB_CASCADE)
class DbOnDeleteSetNullModel(models.Model):
    fk_set_null = models.ForeignKey(Reporter, on_delete=models.DB_SET_NULL, null=True)
class DbOnDeleteSetDefaultModel(models.Model):
    fk_db_set_default = models.ForeignKey(
        Country, on_delete=models.DB_SET_DEFAULT, db_default=models.Value(1)
Existing related object instance caching.
Queries are not redone when going back through known relations.
class Tournament(models.Model):
class Organiser(models.Model):
class Pool(models.Model):
    tournament = models.ForeignKey(Tournament, models.CASCADE)
    organiser = models.ForeignKey(Organiser, models.CASCADE)
class PoolStyle(models.Model):
    pool = models.OneToOneField(Pool, models.CASCADE)
    another_pool = models.OneToOneField(
        Pool, models.CASCADE, null=True, related_name="another_style"
The lookup API
This demonstrates features of the database API.
from django.db.models.lookups import IsNull
class Alarm(models.Model):
    desc = models.CharField(max_length=100)
        return "%s (%s)" % (self.time, self.desc)
    bio = models.TextField(null=True)
    author = models.ForeignKey(Author, models.SET_NULL, blank=True, null=True)
    slug = models.SlugField(unique=True, blank=True, null=True)
    articles = models.ManyToManyField(Article)
class NulledTextField(models.TextField):
        return None if value == "" else value
@NulledTextField.register_lookup
class NulledTransform(models.Transform):
    lookup_name = "nulled"
    template = "NULL"
class IsNullWithNoneAsRHS(IsNull):
    lookup_name = "isnull_none_rhs"
    can_use_none_as_rhs = True
class Season(models.Model):
    year = models.PositiveSmallIntegerField()
    gt = models.IntegerField(null=True, blank=True)
    nulled_text_field = NulledTextField(null=True)
            models.UniqueConstraint(fields=["year"], name="season_year_unique"),
        return str(self.year)
class Game(models.Model):
    season = models.ForeignKey(Season, models.CASCADE, related_name="games")
    home = models.CharField(max_length=100)
    away = models.CharField(max_length=100)
class Player(models.Model):
    games = models.ManyToManyField(Game, related_name="players")
    qty_target = models.DecimalField(max_digits=6, decimal_places=2)
class Stock(models.Model):
    product = models.ForeignKey(Product, models.CASCADE)
    short = models.BooleanField(default=False)
    qty_available = models.DecimalField(max_digits=6, decimal_places=2)
class Freebie(models.Model):
    gift_product = models.ForeignKey(Product, models.CASCADE)
    stock_id = models.IntegerField(blank=True, null=True)
    stock = models.ForeignObject(
        Stock,
        from_fields=["stock_id", "gift_product"],
        to_fields=["id", "product"],
Many-to-many and many-to-one relationships to the same table
Make sure to set ``related_name`` if you use relationships to the same table.
    username = models.CharField(max_length=20)
class Issue(models.Model):
    num = models.IntegerField()
    cc = models.ManyToManyField(User, blank=True, related_name="test_issue_cc")
    client = models.ForeignKey(User, models.CASCADE, related_name="test_issue_client")
        ordering = ("num",)
class StringReferenceModel(models.Model):
    others = models.ManyToManyField("StringReferenceModel")
Many-to-many relationships via an intermediary table
For many-to-many relationships that need extra fields on the intermediary
table, use an intermediary model.
In this example, an ``Article`` can have multiple ``Reporter`` objects, and
each ``Article``-``Reporter`` combination (a ``Writer``) has a ``position``
field, which specifies the ``Reporter``'s position for the given article
(e.g. "Staff writer").
class Writer(models.Model):
    position = models.CharField(max_length=100)
Multiple many-to-many relationships between the same two tables
In this example, an ``Article`` can have many "primary" ``Category`` objects
and many "secondary" ``Category`` objects.
Set ``related_name`` to designate what the reverse relationship is called.
    headline = models.CharField(max_length=50)
    primary_categories = models.ManyToManyField(
        Category, related_name="primary_article_set"
    secondary_categories = models.ManyToManyField(
        Category, related_name="secondary_article_set"
        ordering = ("pub_date",)
Many-to-many relationships between the same two tables
In this example, a ``Person`` can have many friends, who are also ``Person``
objects. Friendship is a symmetrical relationship - if I am your friend, you
are my friend. Here, ``friends`` is an example of a symmetrical
``ManyToManyField``.
A ``Person`` can also have many idols - but while I may idolize you, you may
not think the same of me. Here, ``idols`` is an example of a non-symmetrical
``ManyToManyField``. Only recursive ``ManyToManyField`` fields may be
non-symmetrical, and they are symmetrical by default.
This test validates that the many-to-many table is created using a mangled name
if there is a name clash, and tests that symmetry is preserved where
appropriate.
    friends = models.ManyToManyField("self")
    colleagues = models.ManyToManyField("self", symmetrical=True, through="Colleague")
    idols = models.ManyToManyField("self", symmetrical=False, related_name="stalkers")
class Colleague(models.Model):
    first = models.ForeignKey(Person, models.CASCADE)
    second = models.ForeignKey(Person, models.CASCADE, related_name="+")
    first_meet = models.DateField()
from django.contrib.auth import models as auth
# No related name is needed here, since symmetrical relations are not
# explicitly reversible.
class SelfRefer(models.Model):
    references = models.ManyToManyField("self")
    related = models.ManyToManyField("self")
# Regression for #11956 -- a many to many to the base class
class TagCollection(Tag):
    tags = models.ManyToManyField(Tag, related_name="tag_collections")
# A related_name is required on one of the ManyToManyField entries here because
# they are both addressable as reverse relations from Tag.
class Entry(models.Model):
    topics = models.ManyToManyField(Tag)
    related = models.ManyToManyField(Tag, related_name="similar")
# Two models both inheriting from a base model with a self-referential m2m
# field
class SelfReferChild(SelfRefer):
class SelfReferChildSibling(SelfRefer):
# Many-to-Many relation between models, where one of the PK's isn't an
# Autofield
class Line(models.Model):
class Worksheet(models.Model):
    id = models.CharField(primary_key=True, max_length=100)
    lines = models.ManyToManyField(Line, blank=True)
# Regression for #11226 -- A model with the same name that another one to
# which it has a m2m relation. This shouldn't cause a name clash between
# the automatically created m2m intermediary table FK field names when
# running migrate
    friends = models.ManyToManyField(auth.User)
class BadModelWithSplit(models.Model):
    def split(self):
        raise RuntimeError("split should not be called")
class RegressionModelSplit(BadModelWithSplit):
    Model with a split method should not cause an error in add_lazy_relation
    others = models.ManyToManyField("self")
# Regression for #24505 -- Two ManyToManyFields with the same "to" model
# and related_name set to '+'.
    primary_lines = models.ManyToManyField(Line, related_name="+")
    secondary_lines = models.ManyToManyField(Line, related_name="+")
class Part(models.Model):
    default_parts = models.ManyToManyField(Part)
    optional_parts = models.ManyToManyField(Part, related_name="cars_optional")
class SportsCar(Car):
    fans = models.ManyToManyField("self", related_name="idols", symmetrical=False)
# M2M described on one of the models
class PersonChild(Person):
    members = models.ManyToManyField(Person, through="Membership")
    custom_members = models.ManyToManyField(
        Person, through="CustomMembership", related_name="custom"
    nodefaultsnonulls = models.ManyToManyField(
        through="TestNoDefaultsOrNulls",
        related_name="testnodefaultsnonulls",
    date_joined = models.DateTimeField(default=datetime.now)
    invite_reason = models.CharField(max_length=64, null=True)
        ordering = ("date_joined", "invite_reason", "group")
        return "%s is a member of %s" % (self.person.name, self.group.name)
class CustomMembership(models.Model):
    person = models.ForeignKey(
        db_column="custom_person_column",
        related_name="custom_person_related_name",
    weird_fk = models.ForeignKey(Membership, models.SET_NULL, null=True)
        db_table = "test_table"
        ordering = ["date_joined"]
class TestNoDefaultsOrNulls(models.Model):
    nodefaultnonull = models.IntegerField()
class PersonSelfRefM2M(models.Model):
    name = models.CharField(max_length=5)
    friends = models.ManyToManyField("self", through="Friendship", symmetrical=False)
    sym_friends = models.ManyToManyField(
        "self", through="SymmetricalFriendship", symmetrical=True
class Friendship(models.Model):
    first = models.ForeignKey(
        PersonSelfRefM2M, models.CASCADE, related_name="rel_from_set"
    second = models.ForeignKey(
        PersonSelfRefM2M, models.CASCADE, related_name="rel_to_set"
    date_friended = models.DateTimeField()
class SymmetricalFriendship(models.Model):
    first = models.ForeignKey(PersonSelfRefM2M, models.CASCADE)
    second = models.ForeignKey(PersonSelfRefM2M, models.CASCADE, related_name="+")
    date_friended = models.DateField()
# Custom through link fields
    invitees = models.ManyToManyField(
        to=Person,
        through="Invitation",
        through_fields=["event", "invitee"],
        related_name="events_invited",
    event = models.ForeignKey(Event, models.CASCADE, related_name="invitations")
    # field order is deliberately inverted. the target field is "invitee".
    inviter = models.ForeignKey(Person, models.CASCADE, related_name="invitations_sent")
    invitee = models.ForeignKey(Person, models.CASCADE, related_name="invitations")
    subordinates = models.ManyToManyField(
        through="Relationship",
        through_fields=("source", "target"),
        symmetrical=False,
        ordering = ("pk",)
class Relationship(models.Model):
    # field order is deliberately inverted.
    another = models.ForeignKey(
        Employee, models.SET_NULL, related_name="rel_another_set", null=True
    target = models.ForeignKey(Employee, models.CASCADE, related_name="rel_target_set")
    source = models.ForeignKey(Employee, models.CASCADE, related_name="rel_source_set")
        ordering = ("iname",)
    ingredients = models.ManyToManyField(
        Ingredient,
        through="RecipeIngredient",
        related_name="recipes",
        ordering = ("rname",)
# Forward declared intermediate model
    person = models.ForeignKey("Person", models.CASCADE)
    group = models.ForeignKey("Group", models.CASCADE)
    price = models.IntegerField(default=100)
# using custom id column to test ticket #11107
class UserMembership(models.Model):
    id = models.AutoField(db_column="usermembership_id", primary_key=True)
    # Membership object defined as a class
    members = models.ManyToManyField(Person, through=Membership)
    user_members = models.ManyToManyField(User, through="UserMembership")
# Using to_field on the through model
    make = models.CharField(max_length=20, unique=True, null=True)
    drivers = models.ManyToManyField("Driver", through="CarDriver")
        return str(self.make)
class Driver(models.Model):
    name = models.CharField(max_length=20, unique=True, null=True)
        return str(self.name)
class CarDriver(models.Model):
    car = models.ForeignKey("Car", models.CASCADE, to_field="make")
    driver = models.ForeignKey("Driver", models.CASCADE, to_field="name")
        return "pk=%s car=%s driver=%s" % (str(self.pk), self.car, self.driver)
# Through models using multi-table inheritance
    people = models.ManyToManyField("Person", through="IndividualCompetitor")
    special_people = models.ManyToManyField(
        through="ProxiedIndividualCompetitor",
        related_name="special_event_set",
    teams = models.ManyToManyField("Group", through="CompetingTeam")
class Competitor(models.Model):
    event = models.ForeignKey(Event, models.CASCADE)
class IndividualCompetitor(Competitor):
class CompetingTeam(Competitor):
    team = models.ForeignKey(Group, models.CASCADE)
class ProxiedIndividualCompetitor(IndividualCompetitor):
Relating an object to itself, many-to-one
To define a many-to-one relationship between a model and itself, use
``ForeignKey('self', ...)``.
In this example, a ``Category`` is related to itself. That is, each
``Category`` has a parent ``Category``.
        "self", models.SET_NULL, blank=True, null=True, related_name="child_set"
    full_name = models.CharField(max_length=20)
    mother = models.ForeignKey(
        "self", models.SET_NULL, null=True, related_name="mothers_child_set"
    father = models.ForeignKey(
        "self", models.SET_NULL, null=True, related_name="fathers_child_set"
Various edge-cases for model managers.
class OnlyFred(models.Manager):
        return super().get_queryset().filter(name="fred")
class OnlyBarney(models.Manager):
        return super().get_queryset().filter(name="barney")
class Value42(models.Manager):
        return super().get_queryset().filter(value=42)
class AbstractBase1(models.Model):
    # Custom managers
    manager1 = OnlyFred()
    manager2 = OnlyBarney()
class AbstractBase2(models.Model):
    # Custom manager
    restricted = Value42()
# No custom manager on this class to make sure the default case doesn't break.
class AbstractBase3(models.Model):
    comment = models.CharField(max_length=50)
    manager = OnlyFred()
# Managers from base classes are inherited and, if no manager is specified
# *and* the parent has a manager specified, the first one (in the MRO) will
# become the default.
class Child1(AbstractBase1):
    data = models.CharField(max_length=25)
class Child2(AbstractBase1, AbstractBase2):
class Child3(AbstractBase1, AbstractBase3):
class Child4(AbstractBase1):
    # Should be the default manager, although the parent managers are
    # inherited.
    default = models.Manager()
class Child5(AbstractBase3):
    default = OnlyFred()
class Child6(Child4):
class Child7(Parent):
# RelatedManagers
    test_gfk = GenericRelation(
        "RelationModel", content_type_field="gfk_ctype", object_id_field="gfk_id"
    exact = models.BooleanField(null=True)
        return str(self.pk)
    fk = models.ForeignKey(RelatedModel, models.CASCADE, related_name="test_fk")
    m2m = models.ManyToManyField(RelatedModel, related_name="test_m2m")
    gfk_ctype = models.ForeignKey(ContentType, models.SET_NULL, null=True)
    gfk_id = models.IntegerField(null=True)
    gfk = GenericForeignKey(ct_field="gfk_ctype", fk_field="gfk_id")
Many-to-many relationships
To define a many-to-many relationship, use ``ManyToManyField()``.
In this example, an ``Article`` can be published in multiple ``Publication``
objects, and a ``Publication`` has multiple ``Article`` objects.
class Publication(models.Model):
class NoDeletedArticleManager(models.Manager):
        return super().get_queryset().exclude(headline="deleted")
    # Assign a string as name to make sure the intermediary model is
    # correctly created. Refs #20207
    publications = models.ManyToManyField(Publication, name="publications")
    tags = models.ManyToManyField(Tag, related_name="tags")
    authors = models.ManyToManyField("User", through="UserArticle")
    objects = NoDeletedArticleManager()
    username = models.CharField(max_length=20, unique=True)
class UserArticle(models.Model):
    user = models.ForeignKey(User, models.CASCADE, to_field="username")
# Models to test correct related_name inheritance
class AbstractArticle(models.Model):
    publications = models.ManyToManyField(
        Publication, name="publications", related_name="+"
class InheritedArticleA(AbstractArticle):
class InheritedArticleB(AbstractArticle):
class NullableTargetArticle(models.Model):
        Publication, through="NullablePublicationThrough"
class NullablePublicationThrough(models.Model):
    article = models.ForeignKey(NullableTargetArticle, models.CASCADE)
    publication = models.ForeignKey(Publication, models.CASCADE, null=True)
Many-to-one relationships
To define a many-to-one relationship, use ``ForeignKey()``.
    country = models.ForeignKey(
        Country, models.CASCADE, related_name="cities", null=True
    city = models.ForeignKey(City, models.CASCADE, related_name="districts", null=True)
# If ticket #1578 ever slips back in, these models will not be able to be
# created (the field names being lowercased versions of their opposite classes
# is important here).
class First(models.Model):
    second = models.IntegerField()
class Second(models.Model):
    first = models.ForeignKey(First, models.CASCADE, related_name="the_first")
# Protect against repetition of #1839, #2415 and #2536.
class Third(models.Model):
    third = models.ForeignKey(
        "self", models.SET_NULL, null=True, related_name="child_set"
    bestchild = models.ForeignKey(
        "Child", models.SET_NULL, null=True, related_name="favored_by"
class ParentStringPrimaryKey(models.Model):
    name = models.CharField(primary_key=True, max_length=15)
class ChildNullableParent(models.Model):
    parent = models.ForeignKey(Parent, models.CASCADE, null=True)
class ChildStringPrimaryKeyParent(models.Model):
    parent = models.ForeignKey(ParentStringPrimaryKey, on_delete=models.CASCADE)
class ToFieldChild(models.Model):
        Parent, models.CASCADE, to_field="name", related_name="to_field_children"
# Multiple paths to the same model (#7110, #7125)
class Record(models.Model):
    category = models.ForeignKey(Category, models.CASCADE)
class Relation(models.Model):
    left = models.ForeignKey(Record, models.CASCADE, related_name="left_set")
    right = models.ForeignKey(Record, models.CASCADE, related_name="right_set")
        return "%s - %s" % (self.left.category.name, self.right.category.name)
# Test related objects visibility.
class SchoolManager(models.Manager):
    objects = SchoolManager()
Many-to-one relationships that can be null
To define a many-to-one relationship that can have a null foreign key, use
``ForeignKey()`` with ``null=True`` .
    reporter = models.ForeignKey(Reporter, models.SET_NULL, null=True)
    make = models.CharField(max_length=100, null=True, unique=True)
    car = models.ForeignKey(
        Car, models.SET_NULL, to_field="make", null=True, related_name="drivers"
class PersonWithDefaultMaxLengths(models.Model):
    vcard = models.FileField()
    homepage = models.URLField()
    avatar = models.FilePathField()
class PersonWithCustomMaxLengths(models.Model):
    email = models.EmailField(max_length=250)
    vcard = models.FileField(max_length=250)
    homepage = models.URLField(max_length=250)
    avatar = models.FilePathField(max_length=250)
class SomeObject(models.Model):
# This module has to exist, otherwise pre/post_migrate aren't sent for the
# migrate_signals application.
class Unmanaged(models.Model):
class CustomModelBase(models.base.ModelBase):
class ModelWithCustomBase(models.Model, metaclass=CustomModelBase):
class UnicodeModel(models.Model):
    title = models.CharField("ÚÑÍ¢ÓÐÉ", max_length=20, default="“Ðjáñgó”")
        # Disable auto loading of this model as we load it on our own
        verbose_name = "úñí©óðé µóðéø"
        verbose_name_plural = "úñí©óðé µóðéøß"
class Unserializable:
    An object that migration doesn't know how to serialize.
class UnserializableModel(models.Model):
    title = models.CharField(max_length=20, default=Unserializable())
class UnmigratedModel(models.Model):
    A model that is in a migration-less app (which this app is
    if its migrations directory has not been repointed)
class EmptyManager(models.Manager):
class FoodQuerySet(models.query.QuerySet):
class BaseFoodManager(models.Manager):
        self.args = (a, b, c, d)
class FoodManager(BaseFoodManager.from_queryset(FoodQuerySet)):
class NoMigrationFoodManager(BaseFoodManager.from_queryset(FoodQuerySet)):
class A3(models.Model):
    b2 = models.ForeignKey("lookuperror_b.B2", models.CASCADE)
    c2 = models.ForeignKey("lookuperror_c.C2", models.CASCADE)
class A4(models.Model):
    a1 = models.ForeignKey("lookuperror_a.A1", models.CASCADE)
class C3(models.Model):
class OtherAuthor(models.Model):
    slug = models.SlugField(null=True)
    age = models.IntegerField(default=0)
    silly_field = models.BooleanField(default=False)
        app_label = "migrated_unapplied_app"
class SillyModel(models.Model):
    silly_tribble = models.ForeignKey("migrations.Tribble", models.CASCADE)
    is_trouble = models.BooleanField(default=True)
class Classroom(models.Model):
class Lesson(models.Model):
    classroom = models.ForeignKey(Classroom, on_delete=models.CASCADE)
class VeryLongNameModel(models.Model):
        db_table = "long_db_table_that_should_be_truncated_before_checking"
T = typing.TypeVar("T")
class GenericModel(typing.Generic[T], models.Model):
    """A model inheriting from typing.Generic."""
class GenericModelPEP695[T](models.Model):
    """A model inheriting from typing.Generic via the PEP 695 syntax."""
# Example from Python docs:
# https://typing.python.org/en/latest/spec/generics.html#arbitrary-generic-types-as-base-classes
T1 = typing.TypeVar("T1")
T2 = typing.TypeVar("T2")
T3 = typing.TypeVar("T3")
class Parent1(typing.Generic[T1, T2]):
class Parent2(typing.Generic[T1, T2]):
class Child(Parent1[T1, T3], Parent2[T2, T3]):
class CustomGenericModel(Child[T1, T3, T2], models.Model):
    """A model inheriting from a custom subclass of typing.Generic."""
# Required for migration detection (#22645)
from django.db.models import F, Value
from django.db.models.fields.files import ImageFieldFile
from django.db.models.functions import Cast, Lower
from .storage import NoReadFileSystemStorage
# Set up a temp directory for file storage.
    lambda: connection.features.test_collations["virtual"]
    a = models.CharField(max_length=10)
    d = models.DecimalField(max_digits=5, decimal_places=3)
def get_foo():
    return Foo.objects.get(id=1).pk
    b = models.CharField(max_length=10)
    a = models.ForeignKey(Foo, models.CASCADE, default=get_foo, related_name="bars")
class Whiz(models.Model):
    CHOICES = {
        "Group 1": {
            1: "First",
            2: "Second",
        "Group 2": (
            (3, "Third"),
            (4, "Fourth"),
        0: "Other",
        5: _("translated"),
    c = models.IntegerField(choices=CHOICES, null=True)
class WhizDelayed(models.Model):
    c = models.IntegerField(choices=(), null=True)
# Contrived way of adding choices later.
WhizDelayed._meta.get_field("c").choices = Whiz.CHOICES
class WhizIter(models.Model):
    c = models.IntegerField(choices=iter(Whiz.CHOICES.items()), null=True)
class WhizIterEmpty(models.Model):
    c = models.CharField(choices=iter(()), blank=True, max_length=1)
class Choiceful(models.Model):
    class Suit(models.IntegerChoices):
        DIAMOND = 1, "Diamond"
        SPADE = 2, "Spade"
        HEART = 3, "Heart"
        CLUB = 4, "Club"
    def get_choices():
        return [(i, str(i)) for i in range(3)]
    no_choices = models.IntegerField(null=True)
    empty_choices = models.IntegerField(choices=(), null=True)
    with_choices = models.IntegerField(choices=[(1, "A")], null=True)
    with_choices_dict = models.IntegerField(choices={1: "A"}, null=True)
    with_choices_nested_dict = models.IntegerField(
        choices={"Thing": {1: "A"}}, null=True
    empty_choices_bool = models.BooleanField(choices=())
    empty_choices_text = models.TextField(choices=())
    choices_from_enum = models.IntegerField(choices=Suit)
    choices_from_iterator = models.IntegerField(choices=((i, str(i)) for i in range(3)))
    choices_from_callable = models.IntegerField(choices=get_choices)
class BigD(models.Model):
    d = models.DecimalField(max_digits=32, decimal_places=30)
    large_int = models.DecimalField(max_digits=16, decimal_places=0, null=True)
    size = models.FloatField()
class BigS(models.Model):
    s = models.SlugField(max_length=255)
class UnicodeSlugField(models.Model):
    s = models.SlugField(max_length=255, allow_unicode=True)
class AutoModel(models.Model):
    value = models.AutoField(primary_key=True)
class BigAutoModel(models.Model):
    value = models.BigAutoField(primary_key=True)
class SmallAutoModel(models.Model):
    value = models.SmallAutoField(primary_key=True)
class SmallIntegerModel(models.Model):
    value = models.SmallIntegerField()
class BigIntegerModel(models.Model):
    value = models.BigIntegerField()
    null_value = models.BigIntegerField(null=True, blank=True)
class PositiveBigIntegerModel(models.Model):
    value = models.PositiveBigIntegerField()
class PositiveSmallIntegerModel(models.Model):
    value = models.PositiveSmallIntegerField()
class PositiveIntegerModel(models.Model):
class NullBooleanModel(models.Model):
    nbfield = models.BooleanField(null=True, blank=True)
class BooleanModel(models.Model):
    bfield = models.BooleanField()
class DateTimeModel(models.Model):
    d = models.DateField()
    dt = models.DateTimeField()
    t = models.TimeField()
class DurationModel(models.Model):
    field = models.DurationField()
class NullDurationModel(models.Model):
    field = models.DurationField(null=True)
class PrimaryKeyCharModel(models.Model):
    string = models.CharField(max_length=10, primary_key=True)
class FksToBooleans(models.Model):
    """Model with FKs to models with {Null,}BooleanField's, #15040"""
    bf = models.ForeignKey(BooleanModel, models.CASCADE)
    nbf = models.ForeignKey(NullBooleanModel, models.CASCADE)
class FkToChar(models.Model):
    """Model with FK to a model with a CharField primary key, #19299"""
    out = models.ForeignKey(PrimaryKeyCharModel, models.CASCADE)
class RenamedField(models.Model):
    modelname = models.IntegerField(name="fieldname", choices=((1, "One"),))
class VerboseNameField(models.Model):
    id = models.AutoField("verbose pk", primary_key=True)
    field1 = models.BigIntegerField("verbose field1")
    field2 = models.BooleanField("verbose field2", default=False)
    field3 = models.CharField("verbose field3", max_length=10)
    field4 = models.DateField("verbose field4")
    field5 = models.DateTimeField("verbose field5")
    field6 = models.DecimalField("verbose field6", max_digits=6, decimal_places=1)
    field7 = models.EmailField("verbose field7")
    field8 = models.FileField(
        "verbose field8", storage=temp_storage, upload_to="unused"
    field9 = models.FilePathField("verbose field9")
    field10 = models.FloatField("verbose field10")
    # Don't want to depend on Pillow in this test
    # field_image = models.ImageField("verbose field")
    field11 = models.IntegerField("verbose field11")
    field12 = models.GenericIPAddressField("verbose field12", protocol="ipv4")
    field13 = models.PositiveIntegerField("verbose field13")
    field14 = models.PositiveSmallIntegerField("verbose field14")
    field15 = models.SlugField("verbose field15")
    field16 = models.SmallIntegerField("verbose field16")
    field17 = models.TextField("verbose field17")
    field18 = models.TimeField("verbose field18")
    field19 = models.URLField("verbose field19")
    field20 = models.UUIDField("verbose field20")
    field21 = models.DurationField("verbose field21")
class GenericIPAddress(models.Model):
    ip = models.GenericIPAddressField(null=True, protocol="ipv4")
###############################################################################
# These models aren't used in any test, just here to ensure they validate
# successfully.
# See ticket #16570.
class DecimalLessThanOne(models.Model):
    d = models.DecimalField(max_digits=3, decimal_places=3)
# See ticket #18389.
class FieldClassAttributeModel(models.Model):
    field_class = models.CharField
class DataModel(models.Model):
    short_data = models.BinaryField(max_length=10, default=b"\x08")
    data = models.BinaryField()
# FileField
def upload_to_with_date(instance, filename):
    return f"{instance.created_at.year}/{filename}"
class Document(models.Model):
    myfile = models.FileField(storage=temp_storage, upload_to="unused", unique=True)
# See ticket #36847.
class DocumentWithTimestamp(models.Model):
    created_at = models.DateTimeField(auto_now_add=True)
    myfile = models.FileField(storage=temp_storage, upload_to=upload_to_with_date)
# ImageField
# If Pillow available, do these tests.
    class TestImageFieldFile(ImageFieldFile):
        Custom Field File class that records whether or not the underlying file
        was opened.
            self.was_opened = False
            self.was_opened = True
            super().open()
    class TestImageField(models.ImageField):
        attr_class = TestImageFieldFile
        Model that defines an ImageField with no dimension fields.
        mugshot = TestImageField(storage=temp_storage, upload_to="tests")
    class AbstractPersonWithHeight(models.Model):
        Abstract model that defines an ImageField with only one dimension field
        to make sure the dimension update is correctly run on concrete subclass
        instance post-initialization.
        mugshot = TestImageField(
            storage=temp_storage, upload_to="tests", height_field="mugshot_height"
        mugshot_height = models.PositiveSmallIntegerField()
    class PersonWithHeight(AbstractPersonWithHeight):
        Concrete model that subclass an abstract one with only on dimension
    class PersonWithHeightAndWidth(models.Model):
        Model that defines height and width fields after the ImageField.
            storage=temp_storage,
            upload_to="tests",
            height_field="mugshot_height",
            width_field="mugshot_width",
        mugshot_width = models.PositiveSmallIntegerField()
    class PersonDimensionsFirst(models.Model):
        Model that defines height and width fields before the ImageField.
    class PersonTwoImages(models.Model):
        Model that:
        * Defines two ImageFields
        * Defines the height/width fields before the ImageFields
        * Has a nullable ImageField
        headshot_height = models.PositiveSmallIntegerField(blank=True, null=True)
        headshot_width = models.PositiveSmallIntegerField(blank=True, null=True)
        headshot = TestImageField(
            height_field="headshot_height",
            width_field="headshot_width",
    class PersonNoReadImage(models.Model):
        Model that defines an ImageField with a storage backend that does not
        support reading.
        mugshot = models.ImageField(
            storage=NoReadFileSystemStorage(temp_storage_dir),
        mugshot_width = models.IntegerField()
        mugshot_height = models.IntegerField()
class CustomJSONDecoder(json.JSONDecoder):
    def __init__(self, object_hook=None, *args, **kwargs):
        return super().__init__(object_hook=self.as_uuid, *args, **kwargs)
    def as_uuid(self, dct):
        if "uuid" in dct:
            dct["uuid"] = uuid.UUID(dct["uuid"])
        return dct
class JSONNullCustomEncoder(json.JSONEncoder):
    def default(self, o):
        if isinstance(o, models.JSONNull):
class JSONModel(models.Model):
class NullableJSONModel(models.Model):
    value = models.JSONField(blank=True, null=True)
    value_custom = models.JSONField(
        decoder=CustomJSONDecoder,
class JSONNullDefaultModel(models.Model):
    value = models.JSONField(
        db_default=models.JSONNull(), encoder=JSONNullCustomEncoder
class RelatedJSONModel(models.Model):
    json_model = models.ForeignKey(NullableJSONModel, models.CASCADE)
    summary = models.CharField(max_length=100, null=True, blank=True)
class CustomSerializationJSONModel(models.Model):
    class StringifiedJSONField(models.JSONField):
            return json.dumps(value, cls=self.encoder)
    json_field = StringifiedJSONField()
            "supports_primitives_in_json_field",
class AllFieldsModel(models.Model):
    big_integer = models.BigIntegerField()
    binary = models.BinaryField()
    char = models.CharField(max_length=10)
    decimal = models.DecimalField(decimal_places=2, max_digits=2)
    file_path = models.FilePathField()
    floatf = models.FloatField()
    generic_ip = models.GenericIPAddressField()
    positive_integer = models.PositiveIntegerField()
    positive_small_integer = models.PositiveSmallIntegerField()
    small_integer = models.SmallIntegerField()
    uuid = models.UUIDField()
    fo = models.ForeignObject(
        from_fields=["positive_integer"],
        to_fields=["id"],
        related_name="reverse",
    fk = models.ForeignKey("self", models.CASCADE, related_name="reverse2")
    oto = models.OneToOneField("self", models.CASCADE)
    gfk = GenericForeignKey()
    gr = GenericRelation(DataModel)
class ManyToMany(models.Model):
    field = models.UUIDField()
class NullableUUIDModel(models.Model):
    field = models.UUIDField(blank=True, null=True)
class RelatedToUUIDModel(models.Model):
    uuid_fk = models.ForeignKey("PrimaryKeyUUIDModel", models.CASCADE)
class UUIDChild(PrimaryKeyUUIDModel):
class UUIDGrandchild(UUIDChild):
class GeneratedModelFieldWithConverters(models.Model):
    field_copy = models.GeneratedField(
        expression=Cast("field", models.UUIDField()),
        output_field=models.UUIDField(),
class GeneratedModel(models.Model):
    a = models.IntegerField()
    b = models.IntegerField()
    field = models.GeneratedField(
        expression=F("a") + F("b"),
    fk = models.ForeignKey(Foo, on_delete=models.CASCADE, null=True, blank=True)
class GeneratedModelNonAutoPk(models.Model):
    b = models.GeneratedField(
        expression=F("a") + 1,
class GeneratedModelVirtual(models.Model):
class GeneratedModelParams(models.Model):
        expression=Value("Constant", output_field=models.CharField(max_length=10)),
        output_field=models.CharField(max_length=10),
class GeneratedModelParamsVirtual(models.Model):
class GeneratedModelOutputFieldDbCollation(models.Model):
        expression=Lower("name"),
        output_field=models.CharField(db_collation=test_collation, max_length=11),
class GeneratedModelOutputFieldDbCollationVirtual(models.Model):
class GeneratedModelNull(models.Model):
class GeneratedModelNullVirtual(models.Model):
class GeneratedModelBase(models.Model):
    a_squared = models.GeneratedField(
        expression=F("a") * F("a"),
class GeneratedModelVirtualBase(models.Model):
class GeneratedModelCheckConstraint(GeneratedModelBase):
            "supports_stored_generated_columns",
                condition=models.Q(a__gt=0),
                name="Generated model check constraint a > 0",
class GeneratedModelCheckConstraintVirtual(GeneratedModelVirtualBase):
            "supports_virtual_generated_columns",
                name="Generated model check constraint virtual a > 0",
class GeneratedModelUniqueConstraint(GeneratedModelBase):
            "supports_expression_indexes",
            models.UniqueConstraint(F("a"), name="Generated model unique constraint a"),
class GeneratedModelUniqueConstraintVirtual(GeneratedModelVirtualBase):
                F("a"), name="Generated model unique constraint virtual a"
from django.core import validators
    slug = models.SlugField(max_length=20)
    url = models.CharField("The URL", max_length=40)
        return self.__str__()
class WriterManager(models.Manager):
        qs = super().get_queryset()
        return qs.filter(archived=False)
    name = models.CharField(max_length=50, help_text="Use both first and last names.")
    archived = models.BooleanField(default=False, editable=False)
    objects = WriterManager()
    ARTICLE_STATUS = (
        (1, "Draft"),
        (2, "Pending"),
        (3, "Live"),
    created = models.DateField(editable=False)
    writer = models.ForeignKey(Writer, models.CASCADE)
    article = models.TextField()
    status = models.PositiveIntegerField(choices=ARTICLE_STATUS, blank=True, null=True)
        if not self.id:
            self.created = datetime.date.today()
        return super().save(*args, **kwargs)
class ImprovedArticle(models.Model):
    article = models.OneToOneField(Article, models.CASCADE)
class ImprovedArticleWithParentLink(models.Model):
    article = models.OneToOneField(Article, models.CASCADE, parent_link=True)
class BetterWriter(Writer):
    score = models.IntegerField()
    date_published = models.DateField()
def default_mode():
    return "di"
def default_category():
    return 3
class PublicationDefaults(models.Model):
    MODE_CHOICES = (("di", "direct"), ("de", "delayed"))
    CATEGORY_CHOICES = ((1, "Games"), (2, "Comics"), (3, "Novel"))
    date_published = models.DateField(default=datetime.date.today)
    datetime_published = models.DateTimeField(default=datetime.datetime(2000, 1, 1))
    mode = models.CharField(max_length=2, choices=MODE_CHOICES, default=default_mode)
    category = models.IntegerField(choices=CATEGORY_CHOICES, default=default_category)
    active = models.BooleanField(default=True)
    file = models.FileField(default="default.txt")
    publication = models.OneToOneField(
        Publication, models.SET_NULL, null=True, blank=True
    full_name = models.CharField(max_length=255)
class Author1(models.Model):
    publication = models.OneToOneField(Publication, models.CASCADE, null=False)
class WriterProfile(models.Model):
    writer = models.OneToOneField(Writer, models.CASCADE, primary_key=True)
        return "%s is %s" % (self.writer, self.age)
    myfile = models.FileField(storage=temp_storage, upload_to="unused", blank=True)
class TextFile(models.Model):
    description = models.CharField(max_length=20)
    file = models.FileField(storage=temp_storage, upload_to="tests", max_length=15)
class CustomFileField(models.FileField):
        been_here = getattr(self, "been_saved", False)
        assert not been_here, "save_form_data called more than once"
        setattr(self, "been_saved", True)
class CustomFF(models.Model):
    f = CustomFileField(upload_to="unused", blank=True)
class FilePathModel(models.Model):
    path = models.FilePathField(
        path=os.path.dirname(__file__), match="models.py", blank=True
    from PIL import Image  # NOQA: detect if Pillow is installed
    test_images = True
    class ImageFile(models.Model):
        def custom_upload_path(self, filename):
            path = self.path or "tests"
            return "%s/%s" % (path, filename)
        # Deliberately put the image field *after* the width/height fields to
        # trigger the bug in #10404 with width/height not getting assigned.
        width = models.IntegerField(editable=False)
        height = models.IntegerField(editable=False)
        image = models.ImageField(
            upload_to=custom_upload_path,
            width_field="width",
            height_field="height",
        path = models.CharField(max_length=16, blank=True, default="")
    class OptionalImageFile(models.Model):
        width = models.IntegerField(editable=False, null=True)
        height = models.IntegerField(editable=False, null=True)
    class NoExtensionImageFile(models.Model):
        def upload_to(self, filename):
            return "tests/no_extension"
        image = models.ImageField(storage=temp_storage, upload_to=upload_to)
    test_images = False
class Homepage(models.Model):
    slug = models.SlugField(unique=True)
        return self.slug
class Price(models.Model):
    price = models.DecimalField(max_digits=10, decimal_places=2)
    quantity = models.PositiveIntegerField()
        unique_together = (("price", "quantity"),)
        return "%s for %s" % (self.quantity, self.price)
class Triple(models.Model):
    left = models.IntegerField()
    middle = models.IntegerField()
    right = models.IntegerField()
        unique_together = (("left", "middle"), ("middle", "right"))
class ArticleStatus(models.Model):
    ARTICLE_STATUS_CHAR = (
        ("d", "Draft"),
        ("p", "Pending"),
        ("l", "Live"),
        max_length=2, choices=ARTICLE_STATUS_CHAR, blank=True, null=True
    title = models.CharField(max_length=40)
    author = models.ForeignKey(Writer, models.SET_NULL, blank=True, null=True)
    special_id = models.IntegerField(blank=True, null=True, unique=True)
        unique_together = ("title", "author")
class BookXtra(models.Model):
    isbn = models.CharField(max_length=16, unique=True)
    suffix1 = models.IntegerField(blank=True, default=0)
    suffix2 = models.IntegerField(blank=True, default=0)
        unique_together = ("suffix1", "suffix2")
class DerivedBook(Book, BookXtra):
class ExplicitPK(models.Model):
    key = models.CharField(max_length=20, primary_key=True)
    desc = models.CharField(max_length=20, blank=True, unique=True)
        unique_together = ("key", "desc")
    title = models.CharField(max_length=50, unique_for_date="posted", blank=True)
    slug = models.CharField(max_length=50, unique_for_year="posted", blank=True)
    subtitle = models.CharField(max_length=50, unique_for_month="posted", blank=True)
    posted = models.DateField()
class DateTimePost(models.Model):
    posted = models.DateTimeField(editable=False)
class DerivedPost(Post):
class BigInt(models.Model):
    biggie = models.BigIntegerField()
        return str(self.biggie)
class MarkupField(models.CharField):
        kwargs["max_length"] = 20
        # don't allow this field to be used in form (real use-case might be
        # that you know the markup will always be X, but it is among an app
        # that allows the user to say it could be something else)
        # regressed at r10062
class CustomFieldForExclusionModel(models.Model):
    markup = MarkupField()
class FlexibleDatePost(models.Model):
    posted = models.DateField(blank=True, null=True)
        yield from range(5)
class ColorfulItem(models.Model):
    colors = models.ManyToManyField(Color)
class CustomErrorMessage(models.Model):
    name1 = models.CharField(
        max_length=50,
        validators=[validators.validate_slug],
        error_messages={"invalid": "Model custom error message."},
    name2 = models.CharField(
        if self.name1 == "FORBIDDEN_VALUE":
                {"name1": [ValidationError("Model.clean() error messages.")]}
        elif self.name1 == "FORBIDDEN_VALUE2":
                {"name1": "Model.clean() error messages (simpler syntax)."}
        elif self.name1 == "GLOBAL_ERROR":
            raise ValidationError("Global error message.")
        related_name="jokes",
        limit_choices_to=today_callable_q,
        related_name="jokes_today",
    funny = models.BooleanField(default=False)
# Model for #13776
    character = models.ForeignKey(Character, models.CASCADE)
    study = models.CharField(max_length=30)
# Model for #639
    image = models.FileField(storage=temp_storage, upload_to="tests")
    # Support code for the tests; this keeps track of how many times save()
    # gets called on each instance.
        self._savecount = 0
    def save(self, force_insert=False, force_update=False):
        super().save(force_insert=force_insert, force_update=force_update)
        self._savecount += 1
    uuid = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
# Models for #24706
class StrictAssignmentFieldSpecific(models.Model):
    _should_error = False
        if self._should_error is True:
            raise ValidationError(message={key: "Cannot set attribute"}, code="invalid")
        super().__setattr__(key, value)
class StrictAssignmentAll(models.Model):
            raise ValidationError(message="Cannot set attribute", code="invalid")
# A model with ForeignKey(blank=False, null=True)
    character = models.ForeignKey(Character, models.SET_NULL, blank=False, null=True)
class NullableUniqueCharFieldModel(models.Model):
    codename = models.CharField(max_length=50, blank=True, null=True, unique=True)
    email = models.EmailField(blank=True, null=True)
    slug = models.SlugField(blank=True, null=True)
class NumbersToDice(models.Model):
    number = models.ForeignKey("Number", on_delete=models.CASCADE)
    die = models.ForeignKey("Dice", on_delete=models.CASCADE)
class Dice(models.Model):
    numbers = models.ManyToManyField(
        through=NumbersToDice,
        limit_choices_to=models.Q(value__gte=1),
class ConstraintsModel(models.Model):
    category = models.CharField(max_length=50, default="uncategorized")
    price = models.DecimalField(max_digits=10, decimal_places=2, default=0)
                "category",
                name="unique_name_category",
                violation_error_message="This product already exists.",
                name="price_gte_zero",
                violation_error_message="Price must be greater than zero.",
class AttnameConstraintsModel(models.Model):
    left = models.ForeignKey(
        "self", related_name="+", null=True, on_delete=models.SET_NULL
    right = models.ForeignKey(
                name="%(app_label)s_%(class)s_left_not_right",
                # right_id here is the ForeignKey's attname, not name.
                condition=~models.Q(left=models.F("right_id")),
class BetterAuthor(Author):
    write_speed = models.IntegerField()
        unique_together = (("author", "title"),)
        ordering = ["id"]
        # Ensure author is always accessible in clean method
        assert self.author.name is not None
class BookWithCustomPK(models.Model):
    my_pk = models.DecimalField(max_digits=5, decimal_places=0, primary_key=True)
        return "%s: %s" % (self.my_pk, self.title)
class BookWithOptionalAltEditor(models.Model):
    # Optional secondary author
    alt_editor = models.ForeignKey(Editor, models.SET_NULL, blank=True, null=True)
        unique_together = (("author", "title", "alt_editor"),)
class AlternateBook(Book):
    notes = models.CharField(max_length=100)
        return "%s - %s" % (self.title, self.notes)
class AuthorMeeting(models.Model):
class CustomPrimaryKey(models.Model):
    my_pk = models.CharField(max_length=10, primary_key=True)
    some_field = models.CharField(max_length=100)
# models for inheritance tests.
class Owner(models.Model):
    auto_id = models.AutoField(primary_key=True)
    place = models.ForeignKey(Place, models.CASCADE)
        return "%s at %s" % (self.name, self.place)
    place = models.ForeignKey(Place, models.CASCADE, unique=True)
    # this is purely for testing the data doesn't matter here :)
    lat = models.CharField(max_length=100)
    lon = models.CharField(max_length=100)
class OwnerProfile(models.Model):
    owner = models.OneToOneField(Owner, models.CASCADE, primary_key=True)
        return "%s is %d" % (self.owner.name, self.age)
    serves_pizza = models.BooleanField(default=False)
class MexicanRestaurant(Restaurant):
    serves_tacos = models.BooleanField(default=False)
class ClassyMexicanRestaurant(MexicanRestaurant):
    the_restaurant = models.OneToOneField(
        MexicanRestaurant, models.CASCADE, parent_link=True, primary_key=True
    tacos_are_yummy = models.BooleanField(default=False)
# models for testing unique_together validation when a fk is involved and
# using inlineformset_factory.
class Repository(models.Model):
class Revision(models.Model):
    repository = models.ForeignKey(Repository, models.CASCADE)
    revision = models.CharField(max_length=40)
        unique_together = (("repository", "revision"),)
        return "%s (%s)" % (self.revision, str(self.repository))
# models for testing callable defaults (see bug #7975). If you define a model
# with a callable default value, you cannot rely on the initial value in a
# form.
    date_joined = models.DateTimeField(default=datetime.datetime.now)
    karma = models.IntegerField()
# models for testing a null=True fk to a parent
    team = models.ForeignKey(Team, models.SET_NULL, null=True)
# Models for testing custom ModelForm save methods in formsets and inline
# formsets
# Models for testing UUID primary keys
class UUIDPKParent(models.Model):
class UUIDPKChild(models.Model):
    parent = models.ForeignKey(UUIDPKParent, models.CASCADE)
class ChildWithEditablePK(models.Model):
    name = models.CharField(max_length=255, primary_key=True)
class AutoPKChildOfUUIDPKParent(models.Model):
class AutoPKParent(models.Model):
class UUIDPKChildOfAutoPKParent(models.Model):
    parent = models.ForeignKey(AutoPKParent, models.CASCADE)
class ParentWithUUIDAlternateKey(models.Model):
    uuid = models.UUIDField(unique=True, default=uuid.uuid4, editable=False)
class ChildRelatedViaAK(models.Model):
        ParentWithUUIDAlternateKey, models.CASCADE, to_field="uuid"
    username = models.CharField(max_length=12, unique=True)
    serial = models.IntegerField()
class UserSite(models.Model):
    data = models.IntegerField()
    user = models.ForeignKey(User, models.CASCADE, unique=True, to_field="username")
    about = models.TextField()
class UserPreferences(models.Model):
    user = models.OneToOneField(
        to_field="username",
        primary_key=True,
    favorite_number = models.IntegerField()
class ProfileNetwork(models.Model):
    profile = models.ForeignKey(UserProfile, models.CASCADE, to_field="user")
    network = models.IntegerField()
    identifier = models.IntegerField()
    restaurant = models.ForeignKey(Restaurant, models.CASCADE)
class Network(models.Model):
class Host(models.Model):
    network = models.ForeignKey(Network, models.CASCADE)
    hostname = models.CharField(max_length=25)
        return self.hostname
    author = models.CharField(max_length=50)
    pages = models.IntegerField(db_column="page_count")
    shortcut = models.CharField(max_length=50, db_tablespace="idx_tbls")
    isbn = models.CharField(max_length=50, db_tablespace="idx_tbls")
    barcode = models.CharField(max_length=31)
            models.Index(fields=["title"]),
            models.Index(fields=["isbn", "id"]),
                fields=["barcode"], name="%(app_label)s_%(class)s_barcode_idx"
    shortcut = models.CharField(max_length=3)
            models.Index(fields=["name"]),
            models.Index(fields=["shortcut"], name="%(app_label)s_%(class)s_idx"),
class ChildModel1(AbstractModel):
class ChildModel2(AbstractModel):
XX. Model inheritance
Model inheritance exists in two varieties:
    - abstract base classes which are a way of specifying common
      information inherited by the subclasses. They don't exist as a separate
    - non-abstract base classes (the default), which are models in their own
      right with their own database tables and everything. Their subclasses
      have references back to them, created automatically.
Both styles are demonstrated here.
# Abstract base classes
class CommonInfo(models.Model):
        return "%s %s" % (self.__class__.__name__, self.name)
class Worker(CommonInfo):
    job = models.CharField(max_length=50)
class Student(CommonInfo):
    school_class = models.CharField(max_length=10)
# Abstract base classes with related models
class Attachment(models.Model):
    post = models.ForeignKey(
        related_name="attached_%(class)s_set",
        related_query_name="attached_%(app_label)s_%(class)ss",
class Comment(Attachment):
    is_spam = models.BooleanField(default=False)
class Link(Attachment):
# Multi-table inheritance
class Chef(models.Model):
    address = models.CharField(max_length=80)
class Rating(models.Model):
    rating = models.IntegerField(null=True, blank=True)
        ordering = ["-rating"]
class Restaurant(Place, Rating):
    serves_hot_dogs = models.BooleanField(default=False)
    chef = models.ForeignKey(Chef, models.SET_NULL, null=True, blank=True)
    class Meta(Rating.Meta):
        db_table = "my_restaurant"
class ItalianRestaurant(Restaurant):
    serves_gnocchi = models.BooleanField(default=False)
class ItalianRestaurantCommonParent(ItalianRestaurant, Place):
    place_ptr_two = models.OneToOneField(
        Place, on_delete=models.CASCADE, parent_link=True
class Supplier(Place):
    customers = models.ManyToManyField(Restaurant, related_name="provider")
class CustomSupplier(Supplier):
    # An explicit link to the parent (we can control the attribute name).
    parent = models.OneToOneField(
        Place, models.CASCADE, primary_key=True, parent_link=True
    main_site = models.ForeignKey(Place, models.CASCADE, related_name="lot")
# Abstract base classes with related models where the sub-class has the
# same name in a different app and inherits from the same abstract base
# class.
# NOTE: The actual API tests for the following classes are in
#       model_inheritance_same_model_name/models.py - They are defined
#       here in order to have the name conflict between apps
class NamedURL(models.Model):
    title = models.ForeignKey(
        Title, models.CASCADE, related_name="attached_%(app_label)s_%(class)s_set"
class Mixin:
        self.other_attr = 1
class MixinModel(models.Model, Mixin):
    titles = models.ManyToManyField(Title)
class SubBase(Base):
    sub_id = models.IntegerField(primary_key=True)
    first_name = models.CharField(max_length=80)
    last_name = models.CharField(max_length=80)
    email = models.EmailField(unique=True)
    place = models.ForeignKey(Place, models.CASCADE, null=True, related_name="+")
        # Ordering used by test_inherited_ordering_pk_desc.
        ordering = ["-pk"]
class CommonAncestor(models.Model):
    id = models.IntegerField(primary_key=True, default=1)
class FirstParent(CommonAncestor):
    first_ancestor = models.OneToOneField(
        CommonAncestor, models.CASCADE, primary_key=True, parent_link=True
class SecondParent(CommonAncestor):
    second_ancestor = models.OneToOneField(
class CommonChild(FirstParent, SecondParent):
    capacity = models.IntegerField()
class ParkingLot3(Place):
    # The parent_link connector need not be the pk on the model.
    primary_key = models.AutoField(primary_key=True)
    parent = models.OneToOneField(Place, models.CASCADE, parent_link=True)
class ParkingLot4(models.Model):
    # Test parent_link connector can be discovered in abstract classes.
class ParkingLot4A(ParkingLot4, Place):
class ParkingLot4B(Place, ParkingLot4):
class Supplier(models.Model):
class Wholesaler(Supplier):
    retailer = models.ForeignKey(
        Supplier, models.CASCADE, related_name="wholesale_supplier"
    created = models.DateTimeField(default=datetime.datetime.now)
class SelfRefParent(models.Model):
    parent_data = models.IntegerField()
    self_data = models.ForeignKey("self", models.SET_NULL, null=True)
class SelfRefChild(SelfRefParent):
    child_data = models.IntegerField()
class ArticleWithAuthor(Article):
    author = models.CharField(max_length=100)
class M2MBase(models.Model):
class M2MChild(M2MBase):
class Evaluation(Article):
    quality = models.IntegerField()
class QualityControl(Evaluation):
    assignee = models.CharField(max_length=50)
class BaseM(models.Model):
    base_name = models.CharField(max_length=100)
class DerivedM(BaseM):
    customPK = models.IntegerField(primary_key=True)
    derived_name = models.CharField(max_length=100)
class AuditBase(models.Model):
    planned_date = models.DateField()
        verbose_name_plural = "Audits"
class CertificationAudit(AuditBase):
    class Meta(AuditBase.Meta):
class InternalCertificationAudit(CertificationAudit):
    auditing_dept = models.CharField(max_length=20)
# Abstract classes don't get m2m tables autocreated.
class AbstractEvent(models.Model):
    attendees = models.ManyToManyField(Person, related_name="%(class)s_set")
class BirthdayParty(AbstractEvent):
class BachelorParty(AbstractEvent):
class MessyBachelorParty(BachelorParty):
# Check concrete -> abstract -> concrete inheritance
class SearchableLocation(models.Model):
    keywords = models.CharField(max_length=255)
class Station(SearchableLocation):
class BusStation(Station):
    inbound = models.BooleanField(default=False)
class TrainStation(Station):
    zone = models.IntegerField()
    username = models.CharField(max_length=30, unique=True)
class Profile(User):
    profile_id = models.AutoField(primary_key=True)
    extra = models.CharField(max_length=30, blank=True)
# Check concrete + concrete -> concrete -> concrete
class Politician(models.Model):
    politician_id = models.AutoField(primary_key=True)
class Congressman(Person, Politician):
class Senator(Congressman):
class InstanceOnlyDescriptor:
            raise AttributeError("Instance only")
    # DATA fields
    data_abstract = models.CharField(max_length=10)
    fk_abstract = models.ForeignKey(
        Relation, models.CASCADE, related_name="fk_abstract_rel"
    # M2M fields
    m2m_abstract = models.ManyToManyField(Relation, related_name="m2m_abstract_rel")
    friends_abstract = models.ManyToManyField("self", symmetrical=True)
    following_abstract = models.ManyToManyField(
        "self", related_name="followers_abstract", symmetrical=False
    # VIRTUAL fields
    data_not_concrete_abstract = models.ForeignObject(
        Relation,
        from_fields=["abstract_non_concrete_id"],
        related_name="fo_abstract_rel",
    # GFK fields
    content_type_abstract = models.ForeignKey(
        ContentType, models.CASCADE, related_name="+"
    object_id_abstract = models.PositiveIntegerField()
    content_object_abstract = GenericForeignKey(
        "content_type_abstract", "object_id_abstract"
    # GR fields
    generic_relation_abstract = GenericRelation(Relation)
    def test_property(self):
    test_instance_only_descriptor = InstanceOnlyDescriptor()
class BasePerson(AbstractPerson):
    data_base = models.CharField(max_length=10)
    fk_base = models.ForeignKey(Relation, models.CASCADE, related_name="fk_base_rel")
    m2m_base = models.ManyToManyField(Relation, related_name="m2m_base_rel")
    friends_base = models.ManyToManyField("self", symmetrical=True)
    following_base = models.ManyToManyField(
        "self", related_name="followers_base", symmetrical=False
    data_not_concrete_base = models.ForeignObject(
        from_fields=["base_non_concrete_id"],
        related_name="fo_base_rel",
    content_type_base = models.ForeignKey(ContentType, models.CASCADE, related_name="+")
    object_id_base = models.PositiveIntegerField()
    content_object_base = GenericForeignKey("content_type_base", "object_id_base")
    generic_relation_base = GenericRelation(Relation)
class Person(BasePerson):
    data_inherited = models.CharField(max_length=10)
    fk_inherited = models.ForeignKey(
        Relation, models.CASCADE, related_name="fk_concrete_rel"
    # M2M Fields
    m2m_inherited = models.ManyToManyField(Relation, related_name="m2m_concrete_rel")
    friends_inherited = models.ManyToManyField("self", symmetrical=True)
    following_inherited = models.ManyToManyField(
        "self", related_name="followers_concrete", symmetrical=False
    data_not_concrete_inherited = models.ForeignObject(
        from_fields=["model_non_concrete_id"],
        related_name="fo_concrete_rel",
    content_type_concrete = models.ForeignKey(
    object_id_concrete = models.PositiveIntegerField()
    content_object_concrete = GenericForeignKey(
        "content_type_concrete", "object_id_concrete"
    generic_relation_concrete = GenericRelation(Relation)
        verbose_name = _("Person")
class ProxyPerson(Person):
class PersonThroughProxySubclass(ProxyPerson):
class Relating(models.Model):
    # ForeignKey to BasePerson
    baseperson = models.ForeignKey(
        BasePerson, models.CASCADE, related_name="relating_baseperson"
    baseperson_hidden = models.ForeignKey(BasePerson, models.CASCADE, related_name="+")
    # ForeignKey to Person
    person = models.ForeignKey(Person, models.CASCADE, related_name="relating_person")
    person_hidden = models.ForeignKey(Person, models.CASCADE, related_name="+")
    # ForeignKey to ProxyPerson
    proxyperson = models.ForeignKey(
        ProxyPerson, models.CASCADE, related_name="relating_proxyperson"
    proxyperson_hidden = models.ForeignKey(
        ProxyPerson, models.CASCADE, related_name="relating_proxyperson_hidden+"
    # ManyToManyField to BasePerson
    basepeople = models.ManyToManyField(BasePerson, related_name="relating_basepeople")
    basepeople_hidden = models.ManyToManyField(BasePerson, related_name="+")
    # ManyToManyField to Person
    people = models.ManyToManyField(Person, related_name="relating_people")
    people_hidden = models.ManyToManyField(Person, related_name="+")
class Swappable(models.Model):
        swappable = "MODEL_META_TESTS_SWAPPED"
# ParentListTests models
class Child(FirstParent, SecondParent):
    CHOICES = (
        (1, "first"),
        (2, "second"),
    status = models.IntegerField(blank=True, null=True, choices=CHOICES)
    misc_data = models.CharField(max_length=100, blank=True)
    article_text = models.TextField()
        # A utf-8 verbose name (Ångström's Articles) to test they are valid.
        verbose_name = "\xc3\x85ngstr\xc3\xb6m's Articles"
class Movie(models.Model):
    # Test models with non-default primary keys / AutoFields #5218
    movie_id = models.AutoField(primary_key=True)
class Party(models.Model):
    when = models.DateField(null=True)
    id = models.PositiveIntegerField(primary_key=True)
    department = models.ForeignKey(Department, models.CASCADE)
class WorkerProfile(models.Model):
    worker = models.OneToOneField(Worker, on_delete=models.CASCADE)
class NonAutoPK(models.Model):
    name = models.CharField(max_length=10, primary_key=True)
# Chained foreign keys with to_field produce incorrect query #18432
class Model1(models.Model):
    pkey = models.IntegerField(unique=True, db_index=True)
class Model2(models.Model):
    model1 = models.ForeignKey(Model1, models.CASCADE, unique=True, to_field="pkey")
class Model3(models.Model):
    model2 = models.ForeignKey(Model2, models.CASCADE, unique=True, to_field="model1")
    sign_date = models.DateField()
    main_band = models.ForeignKey(Band, models.CASCADE, related_name="main_concerts")
    opening_band = models.ForeignKey(
        Band, models.CASCADE, related_name="opening_concerts", blank=True
    day = models.CharField(max_length=3, choices=((1, "Fri"), (2, "Sat")))
    transport = models.CharField(
        max_length=100, choices=((1, "Plane"), (2, "Train"), (3, "Bus")), blank=True
class ValidationTestModel(models.Model):
    users = models.ManyToManyField(User)
    state = models.CharField(
        max_length=2, choices=(("CO", "Colorado"), ("WA", "Washington"))
    is_active = models.BooleanField(default=False)
    best_friend = models.OneToOneField(User, models.CASCADE, related_name="best_friend")
    # This field is intentionally 2 characters long (#16080).
    no = models.IntegerField(verbose_name="Number", blank=True, null=True)
    def decade_published_in(self):
        return self.pub_date.strftime("%Y")[:3] + "0's"
class ValidationTestInlineModel(models.Model):
    parent = models.ForeignKey(ValidationTestModel, models.CASCADE)
class Review(models.Model):
    source = models.CharField(max_length=100)
        ordering = ("source",)
        return self.source
# This book manager doesn't do anything interesting; it just
# exists to strip out the 'extra_arg' argument to certain
# calls. This argument is used to establish that the BookManager
# is actually getting used when it should be.
class BookManager(models.Manager):
    def create(self, *args, extra_arg=None, **kwargs):
        return super().create(*args, **kwargs)
    def get_or_create(self, *args, extra_arg=None, **kwargs):
        return super().get_or_create(*args, **kwargs)
    published = models.DateField()
    editor = models.ForeignKey(
        Person, models.SET_NULL, null=True, related_name="edited"
    reviews = GenericRelation(Review)
    pages = models.IntegerField(default=100)
    objects = BookManager()
class Pet(models.Model):
    owner = models.ForeignKey(Person, models.CASCADE)
    user = models.OneToOneField(User, models.SET_NULL, null=True)
    flavor = models.CharField(max_length=100)
        ordering = ("flavor",)
Mutually referential many-to-one relationships
Strings can be used instead of model literals to set up "lazy" relations.
    # Use a simple string for forward declarations.
    # You can also explicitly specify the related app.
    parent = models.ForeignKey("mutually_referential.Parent", models.CASCADE)
    director = models.ForeignKey(Person, models.CASCADE)
class Screening(Event):
    movie = models.ForeignKey(Movie, models.CASCADE)
class ScreeningNullFK(Event):
    movie = models.ForeignKey(Movie, models.SET_NULL, null=True)
class Package(models.Model):
    screening = models.ForeignKey(Screening, models.SET_NULL, null=True)
class PackageNullFK(models.Model):
    screening = models.ForeignKey(ScreeningNullFK, models.SET_NULL, null=True)
Regression tests for proper working of ForeignKey(null=True).
class SystemDetails(models.Model):
    details = models.TextField()
class SystemInfo(models.Model):
    system_details = models.ForeignKey(SystemDetails, models.CASCADE)
    system_name = models.CharField(max_length=32)
class Forum(models.Model):
    system_info = models.ForeignKey(SystemInfo, models.CASCADE)
    forum_name = models.CharField(max_length=32)
    forum = models.ForeignKey(Forum, models.SET_NULL, null=True)
    title = models.CharField(max_length=32)
    post = models.ForeignKey(Post, models.SET_NULL, null=True)
    comment_text = models.CharField(max_length=250)
        ordering = ("comment_text",)
# Ticket 15823
class PropertyValue(models.Model):
class Property(models.Model):
    item = models.ForeignKey(Item, models.CASCADE, related_name="props")
    key = models.CharField(max_length=100)
    value = models.ForeignKey(PropertyValue, models.SET_NULL, null=True)
Regression tests for proper working of ForeignKey(null=True). Tests these bugs:
    * #7512: including a nullable foreign key reference in Meta ordering has
unexpected results
# The first two models represent a very simple null FK ordering case.
    author = models.ForeignKey(Author, models.SET_NULL, null=True)
        ordering = ["author__name"]
# These following 4 models represent a far more complex ordering case.
        ordering = ["post__forum__system_info__system_name", "comment_text"]
    choice = models.CharField(max_length=200, null=True)
# A set of models with an inner one pointing to two outer ones.
class OuterA(models.Model):
class OuterB(models.Model):
    first = models.ForeignKey(OuterA, models.CASCADE)
    # second would clash with the __second lookup.
    third = models.ForeignKey(OuterB, models.SET_NULL, null=True)
One-to-one relationships
To define a one-to-one relationship, use ``OneToOneField()``.
In this example, a ``Place`` optionally can be a ``Restaurant``.
        return "%s the place" % self.name
    place = models.OneToOneField(Place, models.CASCADE, primary_key=True)
        return "%s the restaurant" % self.place.name
    place = models.OneToOneField(Place, models.CASCADE)
    serves_cocktails = models.BooleanField(default=True)
class UndergroundBar(models.Model):
    place = models.OneToOneField(Place, models.SET_NULL, null=True)
class Waiter(models.Model):
        return "%s the waiter at %s" % (self.name, self.restaurant)
class Favorites(models.Model):
    restaurants = models.ManyToManyField(Restaurant)
class ManualPrimaryKey(models.Model):
    primary_key = models.CharField(max_length=10, primary_key=True)
    link = models.OneToOneField(ManualPrimaryKey, models.CASCADE)
class MultiModel(models.Model):
    link1 = models.OneToOneField(Place, models.CASCADE)
    link2 = models.OneToOneField(ManualPrimaryKey, models.CASCADE)
        return "Multimodel %s" % self.name
class Pointer(models.Model):
    other = models.OneToOneField(Target, models.CASCADE, primary_key=True)
class Pointer2(models.Model):
    other = models.OneToOneField(Target, models.CASCADE, related_name="second_pointer")
class HiddenPointer(models.Model):
    target = models.OneToOneField(Target, models.CASCADE, related_name="hidden+")
class ToFieldPointer(models.Model):
    target = models.OneToOneField(
        Target, models.CASCADE, to_field="name", primary_key=True
class DirectorManager(models.Manager):
        return super().get_queryset().filter(is_temp=False)
class Director(models.Model):
    is_temp = models.BooleanField(default=False)
    school = models.OneToOneField(School, models.CASCADE)
    objects = DirectorManager()
OR lookups
To perform an OR lookup, or a lookup that combines ANDs and ORs, combine
``QuerySet`` objects using ``&`` and ``|`` operators.
Alternatively, use positional arguments, and pass one or more expressions of
clauses using the variable ``django.db.models.Q``.
Tests for the order_with_respect_to Meta attribute.
        "self", models.SET_NULL, related_name="children", null=True
# order_with_respect_to points to a model with a OneToOneField primary key.
class Entity(models.Model):
class Dimension(models.Model):
    entity = models.OneToOneField("Entity", primary_key=True, on_delete=models.CASCADE)
class Component(models.Model):
    dimension = models.ForeignKey("Dimension", on_delete=models.CASCADE)
        order_with_respect_to = "dimension"
Specifying ordering
Specify default ordering for a model using the ``ordering`` attribute, which
should be a list or tuple of field names. This tells Django how to order
``QuerySet`` results.
If a field name in ``ordering`` starts with a hyphen, that field will be
ordered in descending order. Otherwise, it'll be ordered in ascending order.
The special-case field name ``"?"`` specifies random order.
The ordering attribute is not required. If you leave it off, ordering will be
undefined -- not random, just undefined.
    name = models.CharField(max_length=63, null=True, blank=True)
    editor = models.ForeignKey("self", models.CASCADE, null=True)
        ordering = ("-pk",)
    second_author = models.ForeignKey(
        Author, models.SET_NULL, null=True, related_name="+"
        ordering = (
            "-pub_date",
            models.F("headline"),
            models.F("author__name").asc(),
            models.OrderBy(models.F("second_author__name")),
class OrderedByAuthorArticle(Article):
        ordering = ("author", "second_author")
class OrderedByFArticle(Article):
        ordering = (models.F("author").asc(nulls_first=True), "id")
class ChildArticle(Article):
    article = models.ForeignKey(OrderedByAuthorArticle, models.CASCADE)
    proof = models.OneToOneField(Article, models.CASCADE, related_name="+")
        ordering = ("article",)
class OrderedByExpression(models.Model):
        ordering = [models.functions.Lower("name")]
class OrderedByExpressionChild(models.Model):
    parent = models.ForeignKey(OrderedByExpression, models.CASCADE)
class OrderedByExpressionGrandChild(models.Model):
    parent = models.ForeignKey(OrderedByExpressionChild, models.CASCADE)
class BarcodedArticle(models.Model):
    rank = models.IntegerField(unique=True, null=True)
    slug = models.CharField(max_length=100, default="slug")
    pub_date = models.DateField(null=True)
    barcode = models.CharField(max_length=30, default="bar")
        unique_together = (("headline", "slug"),)
                fields=["pub_date", "rank"],
                name="unique_pub_date_rank",
                fields=["rank"],
                condition=models.Q(rank__gt=0),
                name="unique_rank_conditional",
                fields=["barcode"],
                condition=models.Q(),
                name="unique_barcode_empty_condition",
from .fields import (
    ArrayField,
    BigIntegerRangeField,
    DateRangeField,
    DateTimeRangeField,
    DecimalRangeField,
    EnumField,
    HStoreField,
    IntegerRangeField,
    OffByOneField,
    SearchVectorField,
class Tag:
    def __init__(self, tag_id):
        self.tag_id = tag_id
        return isinstance(other, Tag) and self.tag_id == other.tag_id
class TagField(models.SmallIntegerField):
        return Tag(int(value))
        if isinstance(value, Tag):
        return value.tag_id
class PostgreSQLModel(models.Model):
        required_db_vendor = "postgresql"
class IntegerArrayModel(PostgreSQLModel):
    field = ArrayField(models.BigIntegerField(), default=list, blank=True)
class NullableIntegerArrayModel(PostgreSQLModel):
    field = ArrayField(models.BigIntegerField(), blank=True, null=True)
    field_nested = ArrayField(ArrayField(models.BigIntegerField(null=True)), null=True)
    order = models.IntegerField(null=True)
class CharArrayModel(PostgreSQLModel):
    field = ArrayField(models.CharField(max_length=10))
class DateTimeArrayModel(PostgreSQLModel):
    datetimes = ArrayField(models.DateTimeField())
    dates = ArrayField(models.DateField())
    times = ArrayField(models.TimeField())
class WithSizeArrayModel(PostgreSQLModel):
    field = ArrayField(models.FloatField(), size=3)
class NestedIntegerArrayModel(PostgreSQLModel):
    field = ArrayField(ArrayField(models.IntegerField()))
class OtherTypesArrayModel(PostgreSQLModel):
    ips = ArrayField(models.GenericIPAddressField(), default=list)
    uuids = ArrayField(models.UUIDField(), default=list)
    decimals = ArrayField(
        models.DecimalField(max_digits=5, decimal_places=2), default=list
    tags = ArrayField(TagField(), blank=True, null=True)
    json = ArrayField(models.JSONField(default=dict), default=list, null=True)
    int_ranges = ArrayField(IntegerRangeField(), blank=True, null=True)
    bigint_ranges = ArrayField(BigIntegerRangeField(), blank=True, null=True)
class HStoreModel(PostgreSQLModel):
    field = HStoreField(blank=True, null=True)
    array_field = ArrayField(HStoreField(), null=True)
class ArrayEnumModel(PostgreSQLModel):
    array_of_enums = ArrayField(EnumField(max_length=20))
class CharFieldModel(models.Model):
    field = models.CharField(max_length=64)
class TextFieldModel(models.Model):
    field = models.TextField()
# Scene/Character/Line models are used to test full text search. They're
# populated with content from Monty Python and the Holy Grail.
class Scene(models.Model):
    scene = models.TextField()
    setting = models.CharField(max_length=255)
class Line(PostgreSQLModel):
    scene = models.ForeignKey("Scene", models.CASCADE)
    character = models.ForeignKey("Character", models.CASCADE)
    dialogue = models.TextField(blank=True, null=True)
    dialogue_search_vector = SearchVectorField(blank=True, null=True)
    dialogue_config = models.CharField(max_length=100, blank=True, null=True)
class LineSavedSearch(PostgreSQLModel):
    line = models.ForeignKey("Line", models.CASCADE)
    query = models.CharField(max_length=100)
class RangesModel(PostgreSQLModel):
    ints = IntegerRangeField(blank=True, null=True, db_default=(5, 10))
    bigints = BigIntegerRangeField(blank=True, null=True)
    decimals = DecimalRangeField(blank=True, null=True)
    timestamps = DateTimeRangeField(blank=True, null=True)
    timestamps_inner = DateTimeRangeField(blank=True, null=True)
    timestamps_closed_bounds = DateTimeRangeField(
        default_bounds="[]",
    dates = DateRangeField(blank=True, null=True)
    dates_inner = DateRangeField(blank=True, null=True)
class RangeLookupsModel(PostgreSQLModel):
    parent = models.ForeignKey(RangesModel, models.SET_NULL, blank=True, null=True)
    big_integer = models.BigIntegerField(blank=True, null=True)
    float = models.FloatField(blank=True, null=True)
    timestamp = models.DateTimeField(blank=True, null=True)
    date = models.DateField(blank=True, null=True)
    small_integer = models.SmallIntegerField(blank=True, null=True)
        max_digits=5, decimal_places=2, blank=True, null=True
class ArrayFieldSubclass(ArrayField):
        super().__init__(models.IntegerField())
class AggregateTestModel(PostgreSQLModel):
    To test postgres-specific general aggregation functions
    char_field = models.CharField(max_length=30, blank=True)
    text_field = models.TextField(blank=True)
    integer_field = models.IntegerField(null=True)
    boolean_field = models.BooleanField(null=True)
    json_field = models.JSONField(null=True)
class StatTestModel(PostgreSQLModel):
    To test postgres-specific aggregation functions for statistics
    int1 = models.IntegerField()
    int2 = models.IntegerField()
    related_field = models.ForeignKey(AggregateTestModel, models.SET_NULL, null=True)
class NowTestModel(models.Model):
    when = models.DateTimeField(null=True, default=None)
class UUIDTestModel(models.Model):
    uuid = models.UUIDField(default=None, null=True)
class Room(models.Model):
class HotelReservation(PostgreSQLModel):
    room = models.ForeignKey("Room", on_delete=models.CASCADE)
    datespan = DateRangeField()
    cancelled = models.BooleanField(default=False)
    requirements = models.JSONField(blank=True, null=True)
class OffByOneModel(PostgreSQLModel):
    one_off = OffByOneField()
from django.db.models.query import ModelIterable
    first_book = models.ForeignKey(
        "Book", models.CASCADE, related_name="first_time_authors"
    favorite_authors = models.ManyToManyField(
        "self", through="FavoriteAuthors", symmetrical=False, related_name="favors_me"
class AuthorWithAge(Author):
    author = models.OneToOneField(Author, models.CASCADE, parent_link=True)
class AuthorWithAgeChild(AuthorWithAge):
class FavoriteAuthors(models.Model):
        Author, models.CASCADE, to_field="name", related_name="i_like"
    likes_author = models.ForeignKey(
        Author, models.CASCADE, to_field="name", related_name="likes_me"
class AuthorAddress(models.Model):
        Author, models.CASCADE, to_field="name", related_name="addresses"
    address = models.TextField()
class BookWithYear(Book):
    book = models.OneToOneField(Book, models.CASCADE, parent_link=True)
    published_year = models.IntegerField()
    aged_authors = models.ManyToManyField(AuthorWithAge, related_name="books_with_year")
class Bio(models.Model):
    author = models.OneToOneField(
    books = models.ManyToManyField(Book, blank=True)
class Reader(models.Model):
    books_read = models.ManyToManyField(Book, related_name="read_by")
class BookReview(models.Model):
    # Intentionally does not have a related name.
    book = models.ForeignKey(BookWithYear, models.CASCADE, null=True)
    notes = models.TextField(null=True, blank=True)
# Models for default manager tests
class Qualification(models.Model):
class ModelIterableSubclass(ModelIterable):
class TeacherQuerySet(models.QuerySet):
        self._iterable_class = ModelIterableSubclass
class TeacherManager(models.Manager):
        return super().get_queryset().prefetch_related("qualifications")
    qualifications = models.ManyToManyField(Qualification)
    objects = TeacherManager()
    objects_custom = TeacherQuerySet.as_manager()
        return "%s (%s)" % (
            ", ".join(q.name for q in self.qualifications.all()),
    teachers = models.ManyToManyField(Teacher)
# GenericRelation/GenericForeignKey tests
        related_name="taggeditem_set2",
    created_by_ct = models.ForeignKey(
        related_name="taggeditem_set3",
    created_by_fkey = models.PositiveIntegerField(null=True)
    created_by = GenericForeignKey(
        "created_by_ct",
        "created_by_fkey",
    favorite_ct = models.ForeignKey(
        related_name="taggeditem_set4",
    favorite_fkey = models.CharField(max_length=64, null=True)
    favorite = GenericForeignKey("favorite_ct", "favorite_fkey")
    tags = GenericRelation(TaggedItem, related_query_name="bookmarks")
    favorite_tags = GenericRelation(
        TaggedItem,
        content_type_field="favorite_ct",
        object_id_field="favorite_fkey",
        related_query_name="favorite_bookmarks",
    comment = models.TextField()
    # Content-object field
    object_pk = models.TextField()
    content_object = GenericForeignKey(ct_field="content_type", fk_field="object_pk")
    content_type_uuid = models.ForeignKey(
        ContentType, models.CASCADE, related_name="comments", null=True
    object_pk_uuid = models.TextField()
    content_object_uuid = GenericForeignKey(
        ct_field="content_type_uuid", fk_field="object_pk_uuid"
class ArticleCustomUUID(models.Model):
    class CustomUUIDField(models.UUIDField):
            # Use prepared=False to ensure str -> UUID conversion is performed.
            return super().get_db_prep_value(value, connection, prepared=False)
    id = CustomUUIDField(primary_key=True, default=uuid.uuid4)
# Models for lookup ordering tests
    address = models.CharField(max_length=255)
    owner = models.ForeignKey("Person", models.SET_NULL, null=True)
    main_room = models.OneToOneField(
        "Room", models.SET_NULL, related_name="main_room_of", null=True
    house = models.ForeignKey(House, models.CASCADE, related_name="rooms")
    houses = models.ManyToManyField(House, related_name="occupants")
    def primary_house(self):
        # Assume business logic forces every person to have at least one house.
        return sorted(self.houses.all(), key=lambda house: -house.rooms.count())[0]
    def all_houses(self):
        return list(self.houses.all())
    def cached_all_houses(self):
        return self.all_houses
# Models for nullable FK tests
    boss = models.ForeignKey("self", models.SET_NULL, null=True, related_name="serfs")
class SelfDirectedEmployee(Employee):
# Ticket #19607
class LessonEntry(models.Model):
    name1 = models.CharField(max_length=200)
    name2 = models.CharField(max_length=200)
class WordEntry(models.Model):
    lesson_entry = models.ForeignKey(LessonEntry, models.CASCADE)
# Ticket #21410: Regression when related_name="+"
class Author2(models.Model):
        "Book", models.CASCADE, related_name="first_time_authors+"
    favorite_books = models.ManyToManyField("Book", related_name="+")
# Models for many-to-many with UUID pk test:
    people = models.ManyToManyField(Person, related_name="pets")
class Flea(models.Model):
    current_room = models.ForeignKey(
        Room, models.SET_NULL, related_name="fleas", null=True
    pets_visited = models.ManyToManyField(Pet, related_name="fleas_hosted")
    people_visited = models.ManyToManyField(Person, related_name="fleas_hosted")
Using properties on models
Use properties on models just like on any other Python object.
    def _set_full_name(self, combined_name):
        self.first_name, self.last_name = combined_name.split(" ", 1)
    full_name = property(_get_full_name)
    full_name_2 = property(_get_full_name, _set_full_name)
class ConcreteModelSubclass(ProxyModel):
class ConcreteModelSubclassProxy(ConcreteModelSubclass):
from app2.models import NiceModel
class ProxyModel(NiceModel):
class NiceModel(models.Model):
By specifying the 'proxy' Meta attribute, model subclasses can specify that
they will take data directly from the table of their base class table rather
than using a new table of their own. This allows them to act as simple proxies,
providing a modified interface to the data from the base class.
# A couple of managers for testing managing overriding in proxy model cases.
        return super().get_queryset().exclude(name="fred")
class SubManager(models.Manager):
        return super().get_queryset().exclude(name="wilma")
    A simple concrete base class.
class Abstract(models.Model):
    A simple abstract base class, to be used for error checking.
class MyPerson(Person):
    A proxy subclass, this should not get a new table. Overrides the default
    manager.
        permissions = (("display_users", "May display users information"),)
    objects = SubManager()
    other = PersonManager()
    def has_special_name(self):
        return self.name.lower() == "special"
class ManagerMixin(models.Model):
    excluder = SubManager()
class OtherPerson(Person, ManagerMixin):
    A class with the default manager from Person, plus a secondary manager.
class StatusPerson(MyPerson):
    A non-proxy subclass of a proxy, it should get a new table.
    status = models.CharField(max_length=80)
# We can even have proxies of proxies (and subclass of those).
class MyPersonProxy(MyPerson):
class LowerStatusPerson(MyPersonProxy):
class AnotherUserProxy(User):
class UserProxyProxy(UserProxy):
class MultiUserProxy(UserProxy, AnotherUserProxy):
# We can still use `select_related()` to include related models in our
# querysets.
    country = models.ForeignKey(Country, models.CASCADE)
class StateProxy(State):
# Proxy models still works with filters (on related fields)
# and select_related, even when mixed with model inheritance
class BaseUser(models.Model):
        return ":".join(
class TrackerUser(BaseUser):
    status = models.CharField(max_length=50)
class ProxyTrackerUser(TrackerUser):
    summary = models.CharField(max_length=255)
    assignee = models.ForeignKey(
        ProxyTrackerUser, models.CASCADE, related_name="issues"
                self.summary,
class Bug(Issue):
    version = models.CharField(max_length=50)
    reporter = models.ForeignKey(BaseUser, models.CASCADE)
class ProxyBug(Bug):
    Proxy of an inherited class
class ProxyProxyBug(ProxyBug):
    A proxy of proxy model with related field
class Improvement(Issue):
    A model that has relation to a proxy model
    or to a proxy of proxy model
    reporter = models.ForeignKey(ProxyTrackerUser, models.CASCADE)
    associated_bug = models.ForeignKey(ProxyProxyBug, models.CASCADE)
class ProxyImprovement(Improvement):
Various complex queries that have been problematic in the past.
class DumbCategory(models.Model):
class ProxyCategory(DumbCategory):
class NamedCategory(DumbCategory):
    category = models.ForeignKey(
        NamedCategory, models.SET_NULL, null=True, default=None
    misc = models.CharField(max_length=25)
    tag = models.ForeignKey(Tag, models.SET_NULL, blank=True, null=True)
    negate = models.BooleanField(default=True)
        ordering = ["note"]
        return self.note
class Annotation(models.Model):
    notes = models.ManyToManyField(Note)
class DateTimePK(models.Model):
    date = models.DateTimeField(primary_key=True, default=datetime.datetime.now)
        ordering = ["date"]
class ExtraInfo(models.Model):
    info = models.CharField(max_length=100)
    note = models.ForeignKey(Note, models.CASCADE, null=True)
    value = models.IntegerField(null=True)
    date = models.ForeignKey(DateTimePK, models.SET_NULL, null=True)
    filterable = models.BooleanField(default=True)
        ordering = ["info"]
        return self.info
    num = models.IntegerField(unique=True)
    extra = models.ForeignKey(ExtraInfo, models.CASCADE)
    created = models.DateTimeField()
    modified = models.DateTimeField(blank=True, null=True)
    tags = models.ManyToManyField(Tag, blank=True)
    creator = models.ForeignKey(Author, models.CASCADE)
    note = models.ForeignKey(Note, models.CASCADE)
        ordering = ["-note", "name"]
    creator = models.ForeignKey(Author, models.SET_NULL, to_field="num", null=True)
class ReportComment(models.Model):
    report = models.ForeignKey(Report, models.CASCADE)
class Ranking(models.Model):
        # A complex ordering specification. Should stress the system a bit.
        ordering = ("author__extra__note", "author__name", "rank")
        return "%d: %s" % (self.rank, self.author.name)
class Cover(models.Model):
        ordering = ["item"]
    other_num = models.IntegerField(null=True)
    another_num = models.IntegerField(null=True)
# Symmetrical m2m field with a normal field using the reverse accessor name
# ("valid").
class Valid(models.Model):
    valid = models.CharField(max_length=10)
    parent = models.ManyToManyField("self")
        ordering = ["valid"]
# Some funky cross-linked models for testing a couple of infinite recursion
class X(models.Model):
    y = models.ForeignKey("Y", models.CASCADE)
class Y(models.Model):
    x1 = models.ForeignKey(X, models.CASCADE, related_name="y1")
# Some models with a cycle in the default ordering. This would be bad if we
# didn't catch the infinite loop.
class LoopX(models.Model):
    y = models.ForeignKey("LoopY", models.CASCADE)
        ordering = ["y"]
class LoopY(models.Model):
    x = models.ForeignKey(LoopX, models.CASCADE)
        ordering = ["x"]
class LoopZ(models.Model):
    z = models.ForeignKey("self", models.CASCADE)
        ordering = ["z"]
# A model and custom default manager combination.
class CustomManager(models.Manager):
        return qs.filter(public=True, tag__name="t1")
class ManagedModel(models.Model):
    public = models.BooleanField(default=True)
    objects = CustomManager()
    normal_manager = models.Manager()
# An inter-related setup with multiple paths from Child to Detail.
class MemberManager(models.Manager):
        return super().get_queryset().select_related("details")
    details = models.OneToOneField(Detail, models.CASCADE, primary_key=True)
    objects = MemberManager()
    person = models.OneToOneField(Member, models.CASCADE, primary_key=True)
    parent = models.ForeignKey(Member, models.CASCADE, related_name="children")
# Custom primary keys interfered with ordering in the past.
class CustomPk(models.Model):
    extra = models.CharField(max_length=10)
        ordering = ["name", "extra"]
    custom = models.ForeignKey(CustomPk, models.CASCADE, null=True)
class CustomPkTag(models.Model):
    id = models.CharField(max_length=20, primary_key=True)
    custom_pk = models.ManyToManyField(CustomPk)
    tag = models.CharField(max_length=20)
# An inter-related setup with a model subclass that has a nullable
# path to another model, and a return path from that model.
    greatest_fan = models.ForeignKey("Fan", models.SET_NULL, null=True, unique=True)
class TvChef(Celebrity):
# Multiple foreign keys
class LeafA(models.Model):
class LeafB(models.Model):
class Join(models.Model):
    a = models.ForeignKey(LeafA, models.CASCADE)
    b = models.ForeignKey(LeafB, models.CASCADE)
class ReservedName(models.Model):
# A simpler shared-foreign-key setup that can expose some problems.
class SharedConnection(models.Model):
class PointerA(models.Model):
    connection = models.ForeignKey(SharedConnection, models.CASCADE)
class PointerB(models.Model):
# Multi-layer ordering
class SingleObject(models.Model):
class RelatedObject(models.Model):
    single = models.ForeignKey(SingleObject, models.SET_NULL, null=True)
    f = models.IntegerField(null=True)
        ordering = ["single"]
class Plaything(models.Model):
    others = models.ForeignKey(RelatedObject, models.SET_NULL, null=True)
        ordering = ["others"]
    food = models.ForeignKey(Food, models.SET_NULL, to_field="name", null=True)
        return "%s at %s" % (self.food, self.meal)
    parent = models.ForeignKey("self", models.SET_NULL, to_field="num", null=True)
# Bug #12252
class ObjectA(models.Model):
        # Ticket #23721
        assert False, "type checking should happen without calling model __iter__"
class ProxyObjectA(ObjectA):
class ChildObjectA(ObjectA):
class ObjectB(models.Model):
    objecta = models.ForeignKey(ObjectA, models.CASCADE)
    num = models.PositiveIntegerField()
class ProxyObjectB(ObjectB):
class ObjectC(models.Model):
    objecta = models.ForeignKey(ObjectA, models.SET_NULL, null=True)
    objectb = models.ForeignKey(ObjectB, models.SET_NULL, null=True)
    childobjecta = models.ForeignKey(
        ChildObjectA, models.SET_NULL, null=True, related_name="ca_pk"
class SimpleCategory(models.Model):
class SpecialCategory(SimpleCategory):
    special_name = models.CharField(max_length=35)
        return self.name + " " + self.special_name
class CategoryItem(models.Model):
    category = models.ForeignKey(SimpleCategory, models.CASCADE)
        return "category item: " + str(self.category)
class MixedCaseFieldCategoryItem(models.Model):
    CaTeGoRy = models.ForeignKey(SimpleCategory, models.CASCADE)
class MixedCaseDbColumnCategoryItem(models.Model):
        SimpleCategory, models.CASCADE, db_column="CaTeGoRy_Id"
class OneToOneCategory(models.Model):
    new_name = models.CharField(max_length=15)
    category = models.OneToOneField(SimpleCategory, models.CASCADE)
        return "one2one " + self.new_name
class CategoryRelationship(models.Model):
    first = models.ForeignKey(SimpleCategory, models.CASCADE, related_name="first_rel")
        SimpleCategory, models.CASCADE, related_name="second_rel"
class CommonMixedCaseForeignKeys(models.Model):
    category = models.ForeignKey(CategoryItem, models.CASCADE)
    mixed_case_field_category = models.ForeignKey(
        MixedCaseFieldCategoryItem, models.CASCADE
    mixed_case_db_column_category = models.ForeignKey(
        MixedCaseDbColumnCategoryItem, models.CASCADE
class NullableName(models.Model):
class ModelD(models.Model):
class ModelC(models.Model):
class ModelB(models.Model):
    c = models.ForeignKey(ModelC, models.CASCADE)
class ModelA(models.Model):
    b = models.ForeignKey(ModelB, models.SET_NULL, null=True)
    d = models.ForeignKey(ModelD, models.CASCADE)
class Job(models.Model):
class JobResponsibilities(models.Model):
    job = models.ForeignKey(Job, models.CASCADE, to_field="name")
    responsibility = models.ForeignKey(
        "Responsibility", models.CASCADE, to_field="description"
class Responsibility(models.Model):
    description = models.CharField(max_length=20, unique=True)
    jobs = models.ManyToManyField(
        Job, through=JobResponsibilities, related_name="responsibilities"
# Models for disjunction join promotion low level testing.
class FK1(models.Model):
    f1 = models.TextField()
    f2 = models.TextField()
class FK2(models.Model):
class FK3(models.Model):
class BaseA(models.Model):
    a = models.ForeignKey(FK1, models.SET_NULL, null=True)
    b = models.ForeignKey(FK2, models.SET_NULL, null=True)
    c = models.ForeignKey(FK3, models.SET_NULL, null=True)
class Identifier(models.Model):
class Program(models.Model):
    identifier = models.OneToOneField(Identifier, models.CASCADE)
    programs = models.ManyToManyField(Program)
    title = models.TextField()
    chapter = models.ForeignKey("Chapter", models.CASCADE)
    paragraph = models.ForeignKey("Paragraph", models.CASCADE)
class Paragraph(models.Model):
    page = models.ManyToManyField("Page")
class MyObject(models.Model):
# Models for #17600 regressions
    name = models.CharField(max_length=12, null=True, default="")
class OrderItem(models.Model):
    order = models.ForeignKey(Order, models.CASCADE, related_name="items")
    status = models.IntegerField()
    annotation = models.ForeignKey(Annotation, models.CASCADE, null=True, blank=True)
class Task(models.Model):
    owner = models.ForeignKey(BaseUser, models.CASCADE, related_name="owner")
    creator = models.ForeignKey(BaseUser, models.CASCADE, related_name="creator")
    note = models.ForeignKey(Note, on_delete=models.CASCADE, null=True, blank=True)
class StaffUser(BaseUser):
    staff = models.OneToOneField(Staff, models.CASCADE, related_name="user")
        return str(self.staff)
class Ticket21203Parent(models.Model):
    parentid = models.AutoField(primary_key=True)
    parent_bool = models.BooleanField(default=True)
    created = models.DateTimeField(auto_now=True)
class Ticket21203Child(models.Model):
    childid = models.AutoField(primary_key=True)
    parent = models.ForeignKey(Ticket21203Parent, models.CASCADE)
    employees = models.ManyToManyField(
        Person, related_name="employers", through="Employment"
class Employment(models.Model):
    employer = models.ForeignKey(Company, models.CASCADE)
    employee = models.ForeignKey(Person, models.CASCADE)
    has_blackboard = models.BooleanField(null=True)
    students = models.ManyToManyField(Student, related_name="classroom")
    schools = models.ManyToManyField(School)
class Ticket23605AParent(models.Model):
class Ticket23605A(Ticket23605AParent):
class Ticket23605B(models.Model):
    modela_fk = models.ForeignKey(Ticket23605A, models.CASCADE)
    modelc_fk = models.ForeignKey("Ticket23605C", models.CASCADE)
    field_b0 = models.IntegerField(null=True)
    field_b1 = models.BooleanField(default=False)
class Ticket23605C(models.Model):
    field_c0 = models.FloatField()
# db_table names have capital letters to ensure they are quoted in queries.
    alive = models.BooleanField()
        db_table = "Individual"
class RelatedIndividual(models.Model):
    related = models.ForeignKey(
        Individual, models.CASCADE, related_name="related_individual"
        db_table = "RelatedIndividual"
class CustomDbColumn(models.Model):
    custom_column = models.IntegerField(db_column="custom_name", null=True)
    ip_address = models.GenericIPAddressField(null=True)
class CreatedField(models.DateTimeField):
        kwargs.setdefault("default", Now)
class ReturningModel(models.Model):
    created = CreatedField(editable=False)
class NonIntegerPKReturningModel(models.Model):
    created = CreatedField(editable=False, primary_key=True)
class JSONFieldNullable(models.Model):
    json_field = models.JSONField(blank=True, null=True)
from django.db import DJANGO_VERSION_PICKLE_KEY, models
def standalone_number():
class Numbers:
    def get_static_number():
class PreviousDjangoVersionQuerySet(models.QuerySet):
        state = super().__getstate__()
        state[DJANGO_VERSION_PICKLE_KEY] = "1.0"
class MissingDjangoVersionQuerySet(models.QuerySet):
        del state[DJANGO_VERSION_PICKLE_KEY]
    name = models.CharField(_("name"), max_length=100)
    previous_django_version_objects = PreviousDjangoVersionQuerySet.as_manager()
    missing_django_version_objects = MissingDjangoVersionQuerySet.as_manager()
    group = models.ForeignKey(Group, models.CASCADE, limit_choices_to=models.Q())
class Happening(models.Model):
    when = models.DateTimeField(blank=True, default=datetime.datetime.now)
    name = models.CharField(blank=True, max_length=100, default="test")
    number1 = models.IntegerField(blank=True, default=standalone_number)
    number2 = models.IntegerField(blank=True, default=Numbers.get_static_number)
    event = models.OneToOneField(Event, models.CASCADE, null=True)
class BinaryFieldModel(models.Model):
    data = models.BinaryField(null=True)
class Container:
    # To test pickling we need a class that isn't defined on module, but
    # is still available from app-cache. So, the Container class moves
    # SomeModel outside of module level
    class SomeModel(models.Model):
        somefield = models.IntegerField()
class M2MModel(models.Model):
    added = models.DateField(default=datetime.date.today)
    groups = models.ManyToManyField(Group)
class AbstractEvent(Event):
        ordering = ["title"]
class MyEvent(AbstractEvent):
class Edition(models.Model):
    event = models.ForeignKey("MyEvent", on_delete=models.CASCADE)
    first_name = models.CharField(max_length=255)
    last_name = models.CharField(max_length=255)
        # Protect against annotations being passed to __init__ --
        # this'll make the test suite get angry if annotations aren't
        # treated differently than fields.
        for k in kwargs:
            assert k in [f.attname for f in self._meta.fields], (
                "Author.__init__ got an unexpected parameter: %s" % k
    paperback = models.BooleanField(default=False)
    opening_line = models.TextField()
class BookFkAsPk(models.Model):
        Book, models.CASCADE, primary_key=True, db_column="not_the_default"
class Coffee(models.Model):
    brand = models.CharField(max_length=255, db_column="name")
class MixedCaseIDColumn(models.Model):
    id = models.AutoField(primary_key=True, db_column="MiXeD_CaSe_Id")
class Reviewer(models.Model):
    reviewed = models.ManyToManyField(Book)
class FriendlyAuthor(Author):
Using SQL reserved names
Need to use a reserved SQL name as a column name or table name? Need to include
a hyphen in a column or table name? No problem. Django quotes names
appropriately behind the scenes, so your database won't complain about
reserved-name usage.
    when = models.CharField(max_length=1, primary_key=True)
    join = models.CharField(max_length=1)
    like = models.CharField(max_length=1)
    drop = models.CharField(max_length=1)
    alter = models.CharField(max_length=1)
    having = models.CharField(max_length=1)
    where = models.DateField(max_length=1)
    has_hyphen = models.CharField(max_length=1, db_column="has-hyphen")
        db_table = "select"
        return self.when
Regression tests for the resolve_url function.
class UnimportantThing(models.Model):
    importance = models.IntegerField()
        return "/importance/%d/" % self.importance
Reverse lookups
This demonstrates the reverse lookup features of the database API.
    creator = models.ForeignKey(User, models.CASCADE)
    poll = models.ForeignKey(Poll, models.CASCADE, related_name="poll_choice")
    related_poll = models.ForeignKey(
        Poll, models.CASCADE, related_name="related_choice"
Adding hooks before/after saving and deleting
To execute arbitrary code around ``save()`` and ``delete()``, just subclass
the methods.
        self.data = []
        self.data.append("Before save")
        # Call the "real" save() method
        self.data.append("After save")
        self.data.append("Before deletion")
        # Call the "real" delete() method
        super().delete()
        self.data.append("After deletion")
# Because we want to test creation and deletion of these as separate things,
# these models are all inserted into a separate Apps so the main test
# runner doesn't migrate them.
new_apps = Apps()
    height = models.PositiveIntegerField(null=True, blank=True)
    weight = models.IntegerField(null=True, blank=True)
class AuthorCharFieldWithIndex(models.Model):
    char_field = models.CharField(max_length=31, db_index=True)
class AuthorTextFieldWithIndex(models.Model):
    text_field = models.TextField(db_index=True)
class AuthorWithDefaultHeight(models.Model):
    height = models.PositiveIntegerField(null=True, blank=True, default=42)
class AuthorWithEvenLongerName(models.Model):
class AuthorWithIndexedName(models.Model):
    name = models.CharField(max_length=255, db_index=True)
class AuthorWithUniqueName(models.Model):
class AuthorWithUniqueNameAndBirthday(models.Model):
        unique_together = [["name", "birthday"]]
    title = models.CharField(max_length=100, db_index=True)
    # tags = models.ManyToManyField("Tag", related_name="books")
class BookWeak(models.Model):
    author = models.ForeignKey(Author, models.CASCADE, db_constraint=False)
class BookWithLongName(models.Model):
    author_foreign_key_with_really_long_field_name = models.ForeignKey(
        AuthorWithEvenLongerName,
class BookWithO2O(models.Model):
    author = models.OneToOneField(Author, models.CASCADE)
        db_table = "schema_book"
class BookWithSlug(models.Model):
    slug = models.CharField(max_length=20, unique=True)
class BookWithoutAuthor(models.Model):
class BookForeignObj(models.Model):
    author_id = models.IntegerField()
class IntegerPK(models.Model):
    i = models.IntegerField(primary_key=True)
    j = models.IntegerField(unique=True)
        db_table = "INTEGERPK"  # uppercase to ensure proper quoting
    info = models.TextField()
    address = models.TextField(null=True)
class NoteRename(models.Model):
    detail_info = models.TextField()
        db_table = "schema_note"
class TagM2MTest(models.Model):
class TagUniqueRename(models.Model):
    slug2 = models.SlugField(unique=True)
        db_table = "schema_tag"
# Based on tests/reserved_names/models.py
        db_table = "drop"
class UniqueTest(models.Model):
    year = models.IntegerField()
    slug = models.SlugField(unique=False)
        unique_together = ["year", "slug"]
    node_id = models.AutoField(primary_key=True)
    parent = models.ForeignKey("self", models.CASCADE, null=True, blank=True)
class Country(Entity):
class EUCountry(Country):
    join_date = models.DateField()
class EUCity(models.Model):
    country = models.ForeignKey(EUCountry, models.CASCADE)
class CountryProxy(Country):
class CountryProxyProxy(CountryProxy):
class CityCountryProxy(models.Model):
    country = models.ForeignKey(CountryProxyProxy, models.CASCADE)
    born = models.ForeignKey(City, models.CASCADE, related_name="+")
    died = models.ForeignKey(City, models.CASCADE, related_name="+")
class PersonProfile(models.Model):
    person = models.OneToOneField(Person, models.CASCADE, related_name="profile")
Tests for select_related()
``select_related()`` follows all relationships and pre-caches any foreign key
values so that complex trees can be fetched in a single query. However, this
isn't always a good idea, so the ``depth`` argument control how many "levels"
the select-related behavior will traverse.
# Who remembers high school biology?
class Domain(models.Model):
class Kingdom(models.Model):
    domain = models.ForeignKey(Domain, models.CASCADE)
class Phylum(models.Model):
    kingdom = models.ForeignKey(Kingdom, models.CASCADE)
class Klass(models.Model):
    phylum = models.ForeignKey(Phylum, models.CASCADE)
    klass = models.ForeignKey(Klass, models.CASCADE)
    order = models.ForeignKey(Order, models.CASCADE)
class Genus(models.Model):
    family = models.ForeignKey(Family, models.CASCADE)
class Species(models.Model):
    genus = models.ForeignKey(Genus, models.CASCADE)
# and we'll invent a new thing so we have a model with two foreign keys
class HybridSpecies(models.Model):
    parent_1 = models.ForeignKey(Species, models.CASCADE, related_name="child_1")
    parent_2 = models.ForeignKey(Species, models.CASCADE, related_name="child_2")
    toppings = models.ManyToManyField(Topping)
    tag = models.CharField(max_length=30)
        ContentType, models.CASCADE, related_name="select_related_tagged_items"
    user = models.OneToOneField(User, models.CASCADE)
    city = models.CharField(max_length=100)
class UserStatResult(models.Model):
    results = models.CharField(max_length=50)
class UserStat(models.Model):
    posts = models.IntegerField()
    results = models.ForeignKey(UserStatResult, models.CASCADE)
class StatDetails(models.Model):
    base_stats = models.OneToOneField(UserStat, models.CASCADE)
    comments = models.IntegerField()
class AdvancedUserStat(UserStat):
class Image(models.Model):
    image = models.OneToOneField(Image, models.SET_NULL, null=True)
    name1 = models.CharField(max_length=50)
    # Avoid having two "id" fields in the Child1 subclass
    id2 = models.AutoField(primary_key=True)
    name2 = models.CharField(max_length=50)
class Child1(Parent1, Parent2):
class Child2(Parent1):
    parent2 = models.OneToOneField(Parent2, models.CASCADE)
class Child3(Child2):
    value3 = models.IntegerField()
class Child4(Child1):
    value4 = models.IntegerField()
class LinkedList(models.Model):
    previous_item = models.OneToOneField(
        related_name="next_item",
class Building(models.Model):
class Device(models.Model):
    building = models.ForeignKey("Building", models.CASCADE)
class Port(models.Model):
    device = models.ForeignKey("Device", models.CASCADE)
    port_number = models.CharField(max_length=10)
        return "%s/%s" % (self.device.name, self.port_number)
class Connection(models.Model):
    start = models.ForeignKey(
        Port,
        related_name="connection_start",
    end = models.ForeignKey(
        related_name="connection_end",
# Another non-tree hierarchy that exercises code paths similar to the above
# example, but in a slightly different configuration.
class TUser(models.Model):
    user = models.ForeignKey(TUser, models.CASCADE, unique=True)
class Organizer(models.Model):
    org = models.ForeignKey(Organizer, models.CASCADE)
class Enrollment(models.Model):
    std = models.ForeignKey(Student, models.CASCADE)
    cls = models.ForeignKey(Class, models.CASCADE)
# Models for testing bug #8036.
class ClientStatus(models.Model):
    state = models.ForeignKey(State, models.SET_NULL, null=True)
    status = models.ForeignKey(ClientStatus, models.CASCADE)
class SpecialClient(Client):
# Some model inheritance exercises
    child = models.ForeignKey(Child, models.SET_NULL, null=True)
# Models for testing bug #19870.
class Fowl(models.Model):
class Hen(Fowl):
class Chick(Fowl):
    mother = models.ForeignKey(Hen, models.CASCADE)
    lots_of_text = models.TextField()
class A(Base):
    a_field = models.CharField(max_length=10)
class B(Base):
    b_field = models.CharField(max_length=10)
class C(Base):
    c_a = models.ForeignKey(A, models.CASCADE)
    c_b = models.ForeignKey(B, models.CASCADE)
This custom Session model adds an extra column to store an account ID. In
real-world applications, it gives you the option of querying the database for
all active sessions for a particular account.
from django.contrib.sessions.backends.db import SessionStore as DBStore
from django.contrib.sessions.base_session import AbstractBaseSession
class CustomSession(AbstractBaseSession):
    A session model with a column for an account ID.
    account_id = models.IntegerField(null=True, db_index=True)
class SessionStore(DBStore):
    A database session store, that handles updating the account ID column
    inside the custom session model.
    def get_model_class(cls):
        return CustomSession
    def create_model_instance(self, data):
        obj = super().create_model_instance(data)
            account_id = int(data.get("_auth_user_id"))
            account_id = None
        obj.account_id = account_id
        return 60 * 60 * 24  # One day.
class Marker(models.Model):
class Phone(models.Model):
Testing signals before/after saving and deleting.
    make = models.CharField(max_length=20)
    model = models.CharField(max_length=20)
    book = models.ForeignKey(Book, on_delete=models.CASCADE)
    lastmod = models.DateTimeField(null=True)
        return "/testmodel/%s/" % self.id
class I18nTestModel(models.Model):
        return reverse("i18n_testmodel", args=[self.id])
from django.contrib.sites.managers import CurrentSiteManager
    on_site = CurrentSiteManager()
class SyndicatedArticle(AbstractArticle):
class ExclusiveArticle(AbstractArticle):
    site = models.ForeignKey(Site, models.CASCADE)
class CustomArticle(AbstractArticle):
    places_this_article_should_appear = models.ForeignKey(Site, models.CASCADE)
    on_site = CurrentSiteManager("places_this_article_should_appear")
Adding __str__() to models
Although it's not a strict requirement, each model should have a ``_str__()``
method to return a "human-readable" representation of the object. Do this not
only for your own sanity when dealing with the interactive prompt, but also
because objects' representations are used throughout Django's
automatically-generated admin.
class InternationalArticle(models.Model):
    friend = models.CharField(max_length=50, blank=True)
    normal = models.ForeignKey(Foo, models.CASCADE, related_name="normal_foo")
    fwd = models.ForeignKey("Whiz", models.CASCADE)
    back = models.ForeignKey("Foo", models.CASCADE)
    parent = models.OneToOneField("Base", models.CASCADE)
    submitted_from = models.GenericIPAddressField(blank=True, null=True)
    publication_date = models.DateField()
        swappable = "TEST_ARTICLE_MODEL"
class AlternateArticle(models.Model):
    byline = models.CharField(max_length=100)
    updated = models.DateTimeField()
    published = models.DateTimeField()
        ordering = ("updated",)
        return "/blog/%s/" % self.pk
    entry = models.ForeignKey(Entry, models.CASCADE)
        ordering = ["updated"]
from django.contrib.auth.models import AbstractBaseUser, BaseUserManager
class CustomUser(AbstractBaseUser):
    email = models.EmailField(verbose_name="email address", max_length=255, unique=True)
    custom_objects = BaseUserManager()
    USERNAME_FIELD = "email"
        app_label = "test_client_regress"
    system_check_run_count = 0
    def check(cls, *args, **kwargs):
        cls.system_check_run_count += 1
        return super().check(**kwargs)
# A set of models that use a non-abstract inherited 'through' model.
class ThroughBase(models.Model):
    b = models.ForeignKey("B", models.CASCADE)
class Through(ThroughBase):
    extra = models.CharField(max_length=20)
    people = models.ManyToManyField(Person, through=Through)
    cars = models.ManyToManyField(Car, through="PossessedCar")
class PossessedCar(models.Model):
    belongs_to = models.ForeignKey(
        Person, models.CASCADE, related_name="possessed_cars"
class MaybeEvent(models.Model):
    dt = models.DateTimeField(blank=True, null=True)
class Session(models.Model):
class SessionEvent(models.Model):
    session = models.ForeignKey(Session, models.CASCADE, related_name="events")
class Timestamp(models.Model):
    created = models.DateTimeField(auto_now_add=True)
class AllDayEvent(models.Model):
    day = models.DateField()
class DailyEvent(models.Model):
Transactions
Django handles transactions in three different ways. The default is to commit
each transaction upon a write, but you can decorate a function to get
commit-on-success behavior. Alternatively, you can manage the transaction
        ordering = ("first_name", "last_name")
        return ("%s %s" % (self.first_name, self.last_name)).strip()
Models can have a ``managed`` attribute, which specifies whether the SQL code
is generated for the table on various manage.py operations.
# All of these models are created in the database by Django.
class A01(models.Model):
    f_a = models.CharField(max_length=10, db_index=True)
    f_b = models.IntegerField()
        db_table = "a01"
class B01(models.Model):
    fk_a = models.ForeignKey(A01, models.CASCADE)
        db_table = "b01"
        # 'managed' is True by default. This tests we can set it explicitly.
        managed = True
class C01(models.Model):
    mm_a = models.ManyToManyField(A01, db_table="d01")
        db_table = "c01"
# All of these models use the same tables as the previous set (they are shadows
# of possibly a subset of the columns). There should be no creation errors,
# since we have told Django they aren't managed by Django.
class A02(models.Model):
class B02(models.Model):
    fk_a = models.ForeignKey(A02, models.CASCADE)
# To re-use the many-to-many intermediate table, we need to manually set up
# things up.
class C02(models.Model):
    mm_a = models.ManyToManyField(A02, through="Intermediate")
    a02 = models.ForeignKey(A02, models.CASCADE, db_column="a01_id")
    c02 = models.ForeignKey(C02, models.CASCADE, db_column="c01_id")
        db_table = "d01"
# These next models test the creation (or not) of many to many join tables
# between managed and unmanaged models. A join table between two unmanaged
# models shouldn't be automatically created (see #10647).
# Firstly, we need some models that will create the tables, purely so that the
# tables are created. This is a test setup, not a requirement for unmanaged
class Proxy1(models.Model):
        db_table = "unmanaged_models_proxy1"
class Proxy2(models.Model):
        db_table = "unmanaged_models_proxy2"
class Unmanaged1(models.Model):
# Unmanaged with an m2m to unmanaged: the intermediary table won't be created.
class Unmanaged2(models.Model):
    mm = models.ManyToManyField(Unmanaged1)
# Here's an unmanaged model with an m2m to a managed one; the intermediary
# table *will* be created (unless given a custom `through` as for C02 above).
class Managed1(models.Model):
Tests for the update() queryset method that allows in-place, multi-object
updates.
class DataPoint(models.Model):
    value = models.CharField(max_length=20)
    another_value = models.CharField(max_length=20, blank=True)
class RelatedPoint(models.Model):
    data = models.ForeignKey(DataPoint, models.CASCADE)
    x = models.IntegerField(default=10)
    a = models.ForeignKey(A, models.CASCADE)
    y = models.IntegerField(default=10)
class D(C):
    target = models.CharField(max_length=10, unique=True)
    foo = models.ForeignKey(Foo, models.CASCADE, to_field="target")
    o2o_foo = models.OneToOneField(
        Foo, models.CASCADE, related_name="o2o_bar", null=True
    m2m_foo = models.ManyToManyField(Foo, related_name="m2m_foo")
    x = models.IntegerField(default=0)
class UniqueNumber(models.Model):
class UniqueNumberChild(UniqueNumber):
        ("M", "Male"),
        ("F", "Female"),
    gender = models.CharField(max_length=1, choices=GENDER_CHOICES)
    pid = models.IntegerField(null=True, default=None)
    employee_num = models.IntegerField(default=0)
    profile = models.ForeignKey(
        "Profile", models.SET_NULL, related_name="profiles", null=True
    accounts = models.ManyToManyField("Account", related_name="employees", blank=True)
        attname, _ = super().get_attname_column()
    salary = models.FloatField(default=1000.0)
class ProxyEmployee(Employee):
User-registered management commands
The ``manage.py`` utility provides a number of useful commands for managing a
Django project. If you want to add a utility command of your own, you can.
The user-defined command ``dance`` is defined in the management/commands
subdirectory of this test application. It is a simple command that responds
with a printed message when invoked.
For more details on how to define your own ``manage.py`` commands, look at the
``django.core.management.commands`` directory. This directory contains the
definitions for the base Django ``manage.py`` commands.
class CategoryInfo(models.Model):
    category = models.OneToOneField(Category, models.CASCADE)
def validate_answer_to_universe(value):
    if value != 42:
            "This is not the answer to life, universe and everything!", code="not42"
class ModelToValidate(models.Model):
    created = models.DateTimeField(default=datetime.now)
    number = models.IntegerField(db_column="number_val")
        limit_choices_to={"number": 10},
    ufm = models.ForeignKey(
        "UniqueFieldsModel",
        to_field="unique_charfield",
    url = models.URLField(blank=True)
    f_with_custom_validator = models.IntegerField(
        blank=True, null=True, validators=[validate_answer_to_universe]
    f_with_iterable_of_validators = models.IntegerField(
        blank=True, null=True, validators=(validate_answer_to_universe,)
    slug = models.SlugField(blank=True)
        if self.number == 11:
            raise ValidationError("Invalid number supplied!")
class UniqueFieldsModel(models.Model):
    unique_charfield = models.CharField(max_length=100, unique=True)
    unique_integerfield = models.IntegerField(unique=True, db_default=42)
    non_unique_field = models.IntegerField()
class CustomPKModel(models.Model):
    my_pk_field = models.CharField(max_length=100, primary_key=True)
    cfield = models.CharField(max_length=100)
    ifield = models.IntegerField()
    efield = models.EmailField()
                "ifield",
                "cfield",
            ["ifield", "efield"],
class UniqueForDateModel(models.Model):
    start_date = models.DateField()
    end_date = models.DateTimeField()
    count = models.IntegerField(
        unique_for_date="start_date", unique_for_year="end_date"
    order = models.IntegerField(unique_for_month="end_date")
class CustomMessagesModel(models.Model):
    other = models.IntegerField(blank=True, null=True)
    number = models.IntegerField(
        db_column="number_val",
        error_messages={"null": "NULL", "not42": "AAARGH", "not_equal": "%s != me"},
        validators=[validate_answer_to_universe],
class AuthorManager(models.Manager):
    archived = models.BooleanField(default=False)
    objects = AuthorManager()
    pub_date = models.DateTimeField(blank=True)
        if self.pub_date is None:
            self.pub_date = datetime.now()
class UniqueErrorsModel(models.Model):
    name = models.CharField(
        error_messages={"unique": "Custom unique name message."},
    no = models.IntegerField(
        unique=True, error_messages={"unique": "Custom unique number message."}
class GenericIPAddressTestModel(models.Model):
    generic_ip = models.GenericIPAddressField(blank=True, null=True, unique=True)
    v4_ip = models.GenericIPAddressField(blank=True, null=True, protocol="ipv4")
    v6_ip = models.GenericIPAddressField(blank=True, null=True, protocol="ipv6")
    ip_verbose_name = models.GenericIPAddressField(
        "IP Address Verbose", blank=True, null=True
class GenericIPAddrUnpackUniqueTest(models.Model):
    generic_v4unpack_ip = models.GenericIPAddressField(
        null=True, blank=True, unique=True, unpack_ipv4=True
class UniqueFuncConstraintModel(models.Model):
            models.UniqueConstraint(Lower("field"), name="func_lower_field_uq"),
                name="price_gt_discounted_price_validation",
class ChildProduct(Product):
    color = models.CharField(max_length=32)
                fields=["name", "color"], name="name_color_uniq_validation"
            models.UniqueConstraint(fields=["rank"], name="rank_uniq_validation"),
    color = models.CharField(max_length=31, null=True, blank=True)
                name="name_without_color_uniq_validation",
Regression tests for Django built-in views.
class BaseArticle(models.Model):
    An abstract article Model so that we can create article models with and
    without a get_absolute_url method (for create_update generic views tests).
class Article(BaseArticle):
class UrlArticle(BaseArticle):
    An Article class with a get_absolute_url defined.
        return "/urlarticles/%s/" % self.slug
    get_absolute_url.purge = True
class DateArticle(BaseArticle):
    An article Model with a DateField instead of DateTimeField,
    for testing #7602
    date_created = models.DateField()
"""Models for the Constitutional AI chain."""
class ConstitutionalPrinciple(BaseModel):
    """Class for a constitutional principle."""
    critique_request: str
    revision_request: str
    name: str = "Constitutional Principle"
    from langchain_community.tools.eleven_labs.models import ElevenLabsModel
DEPRECATED_LOOKUP = {"ElevenLabsModel": "langchain_community.tools.eleven_labs.models"}
    "ElevenLabsModel",
requests.models
This module contains the primary objects that power Requests.
# Import encoding now, to avoid implicit import later.
# Implicit import within threads may cause LookupError when standard library is in a ZIP,
# such as in Embedded Python. See https://github.com/psf/requests/issues/3578.
import encodings.idna  # noqa: F401
from io import UnsupportedOperation
from pip._vendor.urllib3.exceptions import (
    DecodeError,
    LocationParseError,
    ProtocolError,
    ReadTimeoutError,
    SSLError,
from pip._vendor.urllib3.fields import RequestField
from pip._vendor.urllib3.filepost import encode_multipart_formdata
from pip._vendor.urllib3.util import parse_url
from ._internal_utils import to_native_string, unicode_is_ascii
from .auth import HTTPBasicAuth
from .compat import (
    basestring,
    builtin_str,
    chardet,
    cookielib,
from .compat import json as complexjson
from .compat import urlencode, urlsplit, urlunparse
from .cookies import _copy_cookie_jar, cookiejar_from_dict, get_cookie_header
    ChunkedEncodingError,
    ContentDecodingError,
    InvalidJSONError,
    InvalidURL,
from .exceptions import JSONDecodeError as RequestsJSONDecodeError
from .exceptions import MissingSchema
from .exceptions import SSLError as RequestsSSLError
from .exceptions import StreamConsumedError
from .hooks import default_hooks
from .structures import CaseInsensitiveDict
    check_header_validity,
    get_auth_from_url,
    guess_filename,
    guess_json_utf,
    iter_slices,
    parse_header_links,
    requote_uri,
    stream_decode_response_unicode,
    super_len,
    to_key_val_list,
#: The set of HTTP status codes that indicate an automatically
#: processable redirect.
REDIRECT_STATI = (
    codes.moved,  # 301
    codes.found,  # 302
    codes.other,  # 303
    codes.temporary_redirect,  # 307
    codes.permanent_redirect,  # 308
DEFAULT_REDIRECT_LIMIT = 30
CONTENT_CHUNK_SIZE = 10 * 1024
ITER_CHUNK_SIZE = 512
class RequestEncodingMixin:
    def path_url(self):
        """Build the path URL to use."""
        url = []
        p = urlsplit(self.url)
        path = p.path
        url.append(path)
        query = p.query
            url.append("?")
            url.append(query)
        return "".join(url)
    def _encode_params(data):
        """Encode parameters in a piece of data.
        Will successfully encode parameters when passed as a dict or a list of
        2-tuples. Order is retained if data is a list of 2-tuples but arbitrary
        if parameters are supplied as a dict.
        if isinstance(data, (str, bytes)):
        elif hasattr(data, "read"):
        elif hasattr(data, "__iter__"):
            for k, vs in to_key_val_list(data):
                if isinstance(vs, basestring) or not hasattr(vs, "__iter__"):
                    vs = [vs]
                for v in vs:
                        result.append(
                                k.encode("utf-8") if isinstance(k, str) else k,
                                v.encode("utf-8") if isinstance(v, str) else v,
            return urlencode(result, doseq=True)
    def _encode_files(files, data):
        """Build the body for a multipart/form-data request.
        Will successfully encode files when passed as a dict or a list of
        tuples. Order is retained if data is a list of tuples but arbitrary
        The tuples may be 2-tuples (filename, fileobj), 3-tuples (filename, fileobj, contentype)
        or 4-tuples (filename, fileobj, contentype, custom_headers).
            raise ValueError("Files must be provided.")
        elif isinstance(data, basestring):
            raise ValueError("Data must not be a string.")
        new_fields = []
        fields = to_key_val_list(data or {})
        files = to_key_val_list(files or {})
        for field, val in fields:
            if isinstance(val, basestring) or not hasattr(val, "__iter__"):
                val = [val]
            for v in val:
                    # Don't call str() on bytestrings: in Py3 it all goes wrong.
                    if not isinstance(v, bytes):
                        v = str(v)
                    new_fields.append(
                            field.decode("utf-8")
                            if isinstance(field, bytes)
                            else field,
        for k, v in files:
            # support for explicit filename
            ft = None
            fh = None
            if isinstance(v, (tuple, list)):
                if len(v) == 2:
                    fn, fp = v
                elif len(v) == 3:
                    fn, fp, ft = v
                    fn, fp, ft, fh = v
                fn = guess_filename(v) or k
                fp = v
            if isinstance(fp, (str, bytes, bytearray)):
                fdata = fp
            elif hasattr(fp, "read"):
                fdata = fp.read()
            elif fp is None:
            rf = RequestField(name=k, data=fdata, filename=fn, headers=fh)
            rf.make_multipart(content_type=ft)
            new_fields.append(rf)
        body, content_type = encode_multipart_formdata(new_fields)
        return body, content_type
class RequestHooksMixin:
    def register_hook(self, event, hook):
        """Properly register a hook."""
        if event not in self.hooks:
            raise ValueError(f'Unsupported event specified, with event name "{event}"')
        if isinstance(hook, Callable):
            self.hooks[event].append(hook)
        elif hasattr(hook, "__iter__"):
            self.hooks[event].extend(h for h in hook if isinstance(h, Callable))
    def deregister_hook(self, event, hook):
        """Deregister a previously registered hook.
        Returns True if the hook existed, False if not.
            self.hooks[event].remove(hook)
class Request(RequestHooksMixin):
    """A user-created :class:`Request <Request>` object.
    Used to prepare a :class:`PreparedRequest <PreparedRequest>`, which is sent to the server.
    :param method: HTTP method to use.
    :param url: URL to send.
    :param headers: dictionary of headers to send.
    :param files: dictionary of {filename: fileobject} files to multipart upload.
    :param data: the body to attach to the request. If a dictionary or
        list of tuples ``[(key, value)]`` is provided, form-encoding will
        take place.
    :param json: json for the body to attach to the request (if files or data is not specified).
    :param params: URL parameters to append to the URL. If a dictionary or
    :param auth: Auth handler or (user, pass) tuple.
    :param cookies: dictionary or CookieJar of cookies to attach to this request.
    :param hooks: dictionary of callback hooks, for internal usage.
      >>> req = requests.Request('GET', 'https://httpbin.org/get')
      >>> req.prepare()
      <PreparedRequest [GET]>
        method=None,
        auth=None,
        cookies=None,
        hooks=None,
        json=None,
        # Default empty dicts for dict params.
        data = [] if data is None else data
        files = [] if files is None else files
        headers = {} if headers is None else headers
        params = {} if params is None else params
        hooks = {} if hooks is None else hooks
        self.hooks = default_hooks()
        for k, v in list(hooks.items()):
            self.register_hook(event=k, hook=v)
        self.cookies = cookies
        return f"<Request [{self.method}]>"
    def prepare(self):
        """Constructs a :class:`PreparedRequest <PreparedRequest>` for transmission and returns it."""
        p = PreparedRequest()
        p.prepare(
            files=self.files,
            data=self.data,
            json=self.json,
            cookies=self.cookies,
            hooks=self.hooks,
class PreparedRequest(RequestEncodingMixin, RequestHooksMixin):
    """The fully mutable :class:`PreparedRequest <PreparedRequest>` object,
    containing the exact bytes that will be sent to the server.
    Instances are generated from a :class:`Request <Request>` object, and
    should not be instantiated manually; doing so may produce undesirable
    effects.
      >>> r = req.prepare()
      >>> r
      >>> s = requests.Session()
      >>> s.send(r)
        #: HTTP verb to send to the server.
        #: HTTP URL to send the request to.
        self.url = None
        #: dictionary of HTTP headers.
        # The `CookieJar` used to create the Cookie header will be stored here
        # after prepare_cookies is called
        self._cookies = None
        #: request body to send to the server.
        #: dictionary of callback hooks, for internal usage.
        #: integer denoting starting position of a readable file-like body.
        self._body_position = None
    def prepare(
        """Prepares the entire request with the given parameters."""
        self.prepare_method(method)
        self.prepare_url(url, params)
        self.prepare_headers(headers)
        self.prepare_cookies(cookies)
        self.prepare_body(data, files, json)
        self.prepare_auth(auth, url)
        # Note that prepare_auth must be last to enable authentication schemes
        # such as OAuth to work on a fully prepared request.
        # This MUST go after prepare_auth. Authenticators could add a hook
        self.prepare_hooks(hooks)
        return f"<PreparedRequest [{self.method}]>"
        p.method = self.method
        p.url = self.url
        p.headers = self.headers.copy() if self.headers is not None else None
        p._cookies = _copy_cookie_jar(self._cookies)
        p.body = self.body
        p.hooks = self.hooks
        p._body_position = self._body_position
    def prepare_method(self, method):
        """Prepares the given HTTP method."""
        if self.method is not None:
            self.method = to_native_string(self.method.upper())
    def _get_idna_encoded_host(host):
        from pip._vendor import idna
            host = idna.encode(host, uts46=True).decode("utf-8")
        except idna.IDNAError:
            raise UnicodeError
        return host
    def prepare_url(self, url, params):
        """Prepares the given HTTP URL."""
        #: Accept objects that have string representations.
        #: We're unable to blindly call unicode/str functions
        #: as this will include the bytestring indicator (b'')
        #: on python 3.x.
        #: https://github.com/psf/requests/pull/2238
        if isinstance(url, bytes):
            url = url.decode("utf8")
            url = str(url)
        # Remove leading whitespaces from url
        # Don't do any URL preparation for non-HTTP schemes like `mailto`,
        # `data` etc to work around exceptions from `url_parse`, which
        # handles RFC 3986 only.
        if ":" in url and not url.lower().startswith("http"):
        # Support for unicode domain names and paths.
            scheme, auth, host, port, path, query, fragment = parse_url(url)
        except LocationParseError as e:
            raise InvalidURL(*e.args)
        if not scheme:
            raise MissingSchema(
                f"Invalid URL {url!r}: No scheme supplied. "
                f"Perhaps you meant https://{url}?"
        if not host:
            raise InvalidURL(f"Invalid URL {url!r}: No host supplied")
        # In general, we want to try IDNA encoding the hostname if the string contains
        # non-ASCII characters. This allows users to automatically get the correct IDNA
        # behaviour. For strings containing only ASCII characters, we need to also verify
        # it doesn't start with a wildcard (*), before allowing the unencoded hostname.
        if not unicode_is_ascii(host):
                host = self._get_idna_encoded_host(host)
            except UnicodeError:
                raise InvalidURL("URL has an invalid label.")
        elif host.startswith(("*", ".")):
        # Carefully reconstruct the network location
        netloc = auth or ""
        if netloc:
            netloc += "@"
        netloc += host
            netloc += f":{port}"
        # Bare domains aren't valid URLs.
        if isinstance(params, (str, bytes)):
            params = to_native_string(params)
        enc_params = self._encode_params(params)
        if enc_params:
                query = f"{query}&{enc_params}"
                query = enc_params
        url = requote_uri(urlunparse([scheme, netloc, path, None, query, fragment]))
    def prepare_headers(self, headers):
        """Prepares the given HTTP headers."""
        self.headers = CaseInsensitiveDict()
            for header in headers.items():
                # Raise exception on invalid header value.
                check_header_validity(header)
                name, value = header
                self.headers[to_native_string(name)] = value
    def prepare_body(self, data, files, json=None):
        """Prepares the given HTTP body data."""
        # Check if file, fo, generator, iterator.
        # If not, run through normal process.
        # Nottin' on you.
        if not data and json is not None:
            # urllib3 requires a bytes-like body. Python 2's json.dumps
            # provides this natively, but Python 3 gives a Unicode string.
                body = complexjson.dumps(json, allow_nan=False)
                raise InvalidJSONError(ve, request=self)
            if not isinstance(body, bytes):
                body = body.encode("utf-8")
        is_stream = all(
                hasattr(data, "__iter__"),
                not isinstance(data, (basestring, list, tuple, Mapping)),
        if is_stream:
                length = super_len(data)
            except (TypeError, AttributeError, UnsupportedOperation):
                length = None
            body = data
            if getattr(body, "tell", None) is not None:
                # Record the current file position before reading.
                # This will allow us to rewind a file in the event
                # of a redirect.
                    self._body_position = body.tell()
                    # This differentiates from None, allowing us to catch
                    # a failed `tell()` later when trying to rewind the body
                    self._body_position = object()
                    "Streamed bodies and files are mutually exclusive."
            if length:
                self.headers["Content-Length"] = builtin_str(length)
                self.headers["Transfer-Encoding"] = "chunked"
            # Multi-part file uploads.
                (body, content_type) = self._encode_files(files, data)
                    body = self._encode_params(data)
                    if isinstance(data, basestring) or hasattr(data, "read"):
                        content_type = "application/x-www-form-urlencoded"
            self.prepare_content_length(body)
            # Add content-type if it wasn't explicitly provided.
            if content_type and ("content-type" not in self.headers):
    def prepare_content_length(self, body):
        """Prepare Content-Length header based on request method and body"""
        if body is not None:
            length = super_len(body)
                # If length exists, set it. Otherwise, we fallback
                # to Transfer-Encoding: chunked.
            self.method not in ("GET", "HEAD")
            and self.headers.get("Content-Length") is None
            # Set Content-Length to 0 for methods that can have a body
            # but don't provide one. (i.e. not GET or HEAD)
            self.headers["Content-Length"] = "0"
    def prepare_auth(self, auth, url=""):
        """Prepares the given HTTP auth data."""
        # If no Auth is explicitly provided, extract it from the URL first.
        if auth is None:
            url_auth = get_auth_from_url(self.url)
            auth = url_auth if any(url_auth) else None
        if auth:
            if isinstance(auth, tuple) and len(auth) == 2:
                # special-case basic HTTP auth
                auth = HTTPBasicAuth(*auth)
            # Allow auth to make its changes.
            r = auth(self)
            # Update self to reflect the auth changes.
            self.__dict__.update(r.__dict__)
            # Recompute Content-Length
            self.prepare_content_length(self.body)
    def prepare_cookies(self, cookies):
        """Prepares the given HTTP cookie data.
        This function eventually generates a ``Cookie`` header from the
        given cookies using cookielib. Due to cookielib's design, the header
        will not be regenerated if it already exists, meaning this function
        can only be called once for the life of the
        :class:`PreparedRequest <PreparedRequest>` object. Any subsequent calls
        to ``prepare_cookies`` will have no actual effect, unless the "Cookie"
        header is removed beforehand.
        if isinstance(cookies, cookielib.CookieJar):
            self._cookies = cookies
            self._cookies = cookiejar_from_dict(cookies)
        cookie_header = get_cookie_header(self._cookies, self)
        if cookie_header is not None:
            self.headers["Cookie"] = cookie_header
    def prepare_hooks(self, hooks):
        """Prepares the given hooks."""
        # hooks can be passed as None to the prepare method and to this
        # method. To prevent iterating over None, simply use an empty list
        # if hooks is False-y
        hooks = hooks or []
        for event in hooks:
            self.register_hook(event, hooks[event])
class Response:
    """The :class:`Response <Response>` object, which contains a
    server's response to an HTTP request.
    __attrs__ = [
        "_content",
        "status_code",
        "history",
        "encoding",
        "reason",
        "cookies",
        "elapsed",
        self._content = False
        self._content_consumed = False
        self._next = None
        #: Integer Code of responded HTTP Status, e.g. 404 or 200.
        self.status_code = None
        #: Case-insensitive Dictionary of Response Headers.
        #: For example, ``headers['content-encoding']`` will return the
        #: value of a ``'Content-Encoding'`` response header.
        #: File-like object representation of response (for advanced usage).
        #: Use of ``raw`` requires that ``stream=True`` be set on the request.
        #: This requirement does not apply for use internally to Requests.
        self.raw = None
        #: Final URL location of Response.
        #: Encoding to decode with when accessing r.text.
        self.encoding = None
        #: A list of :class:`Response <Response>` objects from
        #: the history of the Request. Any redirect responses will end
        #: up here. The list is sorted from the oldest to the most recent request.
        self.history = []
        #: Textual reason of responded HTTP Status, e.g. "Not Found" or "OK".
        #: A CookieJar of Cookies the server sent back.
        self.cookies = cookiejar_from_dict({})
        #: The amount of time elapsed between sending the request
        #: and the arrival of the response (as a timedelta).
        #: This property specifically measures the time taken between sending
        #: the first byte of the request and finishing parsing the headers. It
        #: is therefore unaffected by consuming the response content or the
        #: value of the ``stream`` keyword argument.
        self.elapsed = datetime.timedelta(0)
        #: The :class:`PreparedRequest <PreparedRequest>` object to which this
        #: is a response.
        self.request = None
        # Consume everything; accessing the content attribute makes
        # sure the content has been fully read.
        if not self._content_consumed:
            self.content
        return {attr: getattr(self, attr, None) for attr in self.__attrs__}
        for name, value in state.items():
        # pickled objects do not have .raw
        setattr(self, "_content_consumed", True)
        setattr(self, "raw", None)
        return f"<Response [{self.status_code}]>"
        """Returns True if :attr:`status_code` is less than 400.
        This attribute checks if the status code of the response is between
        400 and 600 to see if there was a client error or a server error. If
        the status code, is between 200 and 400, this will return True. This
        is **not** a check to see if the response code is ``200 OK``.
        return self.ok
    def __nonzero__(self):
        """Allows you to use a response as an iterator."""
        return self.iter_content(128)
    def ok(self):
        """Returns True if :attr:`status_code` is less than 400, False if not.
        the status code is between 200 and 400, this will return True. This
            self.raise_for_status()
        except HTTPError:
    def is_redirect(self):
        """True if this Response is a well-formed HTTP redirect that could have
        been processed automatically (by :meth:`Session.resolve_redirects`).
        return "location" in self.headers and self.status_code in REDIRECT_STATI
    def is_permanent_redirect(self):
        """True if this Response one of the permanent versions of redirect."""
        return "location" in self.headers and self.status_code in (
            codes.moved_permanently,
            codes.permanent_redirect,
        """Returns a PreparedRequest for the next request in a redirect chain, if there is one."""
        return self._next
    def apparent_encoding(self):
        """The apparent encoding, provided by the charset_normalizer or chardet libraries."""
        if chardet is not None:
            return chardet.detect(self.content)["encoding"]
            # If no character detection library is available, we'll fall back
            # to a standard Python utf-8 str.
            return "utf-8"
    def iter_content(self, chunk_size=1, decode_unicode=False):
        """Iterates over the response data.  When stream=True is set on the
        request, this avoids reading the content at once into memory for
        large responses.  The chunk size is the number of bytes it should
        read into memory.  This is not necessarily the length of each item
        returned as decoding can take place.
        chunk_size must be of type int or None. A value of None will
        function differently depending on the value of `stream`.
        stream=True will read data as it arrives in whatever size the
        chunks are received. If stream=False, data is returned as
        a single chunk.
        If decode_unicode is True, content will be decoded using the best
        available encoding based on the response.
        def generate():
            if hasattr(self.raw, "stream"):
                    yield from self.raw.stream(chunk_size, decode_content=True)
                except ProtocolError as e:
                    raise ChunkedEncodingError(e)
                except DecodeError as e:
                    raise ContentDecodingError(e)
                except ReadTimeoutError as e:
                    raise ConnectionError(e)
                except SSLError as e:
                    raise RequestsSSLError(e)
                    chunk = self.raw.read(chunk_size)
            self._content_consumed = True
        if self._content_consumed and isinstance(self._content, bool):
            raise StreamConsumedError()
        elif chunk_size is not None and not isinstance(chunk_size, int):
                f"chunk_size must be an int, it is instead a {type(chunk_size)}."
        # simulate reading small chunks of the content
        reused_chunks = iter_slices(self._content, chunk_size)
        stream_chunks = generate()
        chunks = reused_chunks if self._content_consumed else stream_chunks
        if decode_unicode:
            chunks = stream_decode_response_unicode(chunks, self)
    def iter_lines(
        self, chunk_size=ITER_CHUNK_SIZE, decode_unicode=False, delimiter=None
        """Iterates over the response data, one line at a time.  When
        stream=True is set on the request, this avoids reading the
        content at once into memory for large responses.
        .. note:: This method is not reentrant safe.
        for chunk in self.iter_content(
            chunk_size=chunk_size, decode_unicode=decode_unicode
            if pending is not None:
                chunk = pending + chunk
            if delimiter:
                lines = chunk.split(delimiter)
                lines = chunk.splitlines()
            if lines and lines[-1] and chunk and lines[-1][-1] == chunk[-1]:
                pending = lines.pop()
            yield from lines
            yield pending
        """Content of the response, in bytes."""
        if self._content is False:
            # Read the contents.
            if self._content_consumed:
                raise RuntimeError("The content for this response was already consumed")
            if self.status_code == 0 or self.raw is None:
                self._content = None
                self._content = b"".join(self.iter_content(CONTENT_CHUNK_SIZE)) or b""
        # don't need to release the connection; that's been handled by urllib3
        # since we exhausted the data.
        """Content of the response, in unicode.
        If Response.encoding is None, encoding will be guessed using
        ``charset_normalizer`` or ``chardet``.
        The encoding of the response content is determined based solely on HTTP
        headers, following RFC 2616 to the letter. If you can take advantage of
        non-HTTP knowledge to make a better guess at the encoding, you should
        set ``r.encoding`` appropriately before accessing this property.
        # Try charset from content-type
        encoding = self.encoding
        if not self.content:
        # Fallback to auto-detected encoding.
        if self.encoding is None:
            encoding = self.apparent_encoding
        # Decode unicode from given encoding.
            content = str(self.content, encoding, errors="replace")
        except (LookupError, TypeError):
            # A LookupError is raised if the encoding was not found which could
            # indicate a misspelling or similar mistake.
            # A TypeError can be raised if encoding is None
            # So we try blindly encoding.
            content = str(self.content, errors="replace")
    def json(self, **kwargs):
        r"""Decodes the JSON response body (if any) as a Python object.
        This may return a dictionary, list, etc. depending on what is in the response.
        :param \*\*kwargs: Optional arguments that ``json.loads`` takes.
        :raises requests.exceptions.JSONDecodeError: If the response body does not
            contain valid json.
        if not self.encoding and self.content and len(self.content) > 3:
            # No encoding set. JSON RFC 4627 section 3 states we should expect
            # UTF-8, -16 or -32. Detect which one to use; If the detection or
            # decoding fails, fall back to `self.text` (using charset_normalizer to make
            # a best guess).
            encoding = guess_json_utf(self.content)
            if encoding is not None:
                    return complexjson.loads(self.content.decode(encoding), **kwargs)
                    # Wrong UTF codec detected; usually because it's not UTF-8
                    # but some other 8-bit codec.  This is an RFC violation,
                    # and the server didn't bother to tell us what codec *was*
                    raise RequestsJSONDecodeError(e.msg, e.doc, e.pos)
            return complexjson.loads(self.text, **kwargs)
            # Catch JSON-related errors and raise as requests.JSONDecodeError
            # This aliases json.JSONDecodeError and simplejson.JSONDecodeError
    def links(self):
        """Returns the parsed header links of the response, if any."""
        header = self.headers.get("link")
        resolved_links = {}
        if header:
            links = parse_header_links(header)
            for link in links:
                key = link.get("rel") or link.get("url")
                resolved_links[key] = link
        return resolved_links
    def raise_for_status(self):
        """Raises :class:`HTTPError`, if one occurred."""
        if isinstance(self.reason, bytes):
            # encodings. (See PR #3538)
                reason = self.reason.decode("utf-8")
                reason = self.reason.decode("iso-8859-1")
            reason = self.reason
        if 400 <= self.status_code < 500:
                f"{self.status_code} Client Error: {reason} for url: {self.url}"
        elif 500 <= self.status_code < 600:
                f"{self.status_code} Server Error: {reason} for url: {self.url}"
            raise HTTPError(http_error_msg, response=self)
        """Releases the connection back to the pool. Once this method has been
        called the underlying ``raw`` object must not be accessed again.
        *Note: Should not normally need to be called explicitly.*
            self.raw.close()
        release_conn = getattr(self.raw, "release_conn", None)
        if release_conn is not None:
            release_conn()
from encodings.aliases import aliases
from hashlib import sha256
from json import dumps
from re import sub
from typing import Any, Iterator, List, Tuple
from .constant import RE_POSSIBLE_ENCODING_INDICATION, TOO_BIG_SEQUENCE
from .utils import iana_name, is_multi_byte_encoding, unicode_range
class CharsetMatch:
        payload: bytes,
        guessed_encoding: str,
        mean_mess_ratio: float,
        has_sig_or_bom: bool,
        languages: CoherenceMatches,
        decoded_payload: str | None = None,
        preemptive_declaration: str | None = None,
        self._payload: bytes = payload
        self._encoding: str = guessed_encoding
        self._mean_mess_ratio: float = mean_mess_ratio
        self._languages: CoherenceMatches = languages
        self._has_sig_or_bom: bool = has_sig_or_bom
        self._unicode_ranges: list[str] | None = None
        self._leaves: list[CharsetMatch] = []
        self._mean_coherence_ratio: float = 0.0
        self._output_payload: bytes | None = None
        self._output_encoding: str | None = None
        self._string: str | None = decoded_payload
        self._preemptive_declaration: str | None = preemptive_declaration
        if not isinstance(other, CharsetMatch):
                return iana_name(other) == self.encoding
        return self.encoding == other.encoding and self.fingerprint == other.fingerprint
        Implemented to make sorted available upon CharsetMatches items.
        chaos_difference: float = abs(self.chaos - other.chaos)
        coherence_difference: float = abs(self.coherence - other.coherence)
        # Below 1% difference --> Use Coherence
        if chaos_difference < 0.01 and coherence_difference > 0.02:
            return self.coherence > other.coherence
        elif chaos_difference < 0.01 and coherence_difference <= 0.02:
            # When having a difficult decision, use the result that decoded as many multi-byte as possible.
            # preserve RAM usage!
            if len(self._payload) >= TOO_BIG_SEQUENCE:
                return self.chaos < other.chaos
            return self.multi_byte_usage > other.multi_byte_usage
    def multi_byte_usage(self) -> float:
        return 1.0 - (len(str(self)) / len(self.raw))
        # Lazy Str Loading
        if self._string is None:
            self._string = str(self._payload, self._encoding, "strict")
        return self._string
        return f"<CharsetMatch '{self.encoding}' bytes({self.fingerprint})>"
    def add_submatch(self, other: CharsetMatch) -> None:
        if not isinstance(other, CharsetMatch) or other == self:
                "Unable to add instance <{}> as a submatch of a CharsetMatch".format(
                    other.__class__
        other._string = None  # Unload RAM usage; dirty trick.
        self._leaves.append(other)
    def encoding(self) -> str:
        return self._encoding
    def encoding_aliases(self) -> list[str]:
        Encoding name are known by many name, using this could help when searching for IBM855 when it's listed as CP855.
        also_known_as: list[str] = []
        for u, p in aliases.items():
            if self.encoding == u:
                also_known_as.append(p)
            elif self.encoding == p:
                also_known_as.append(u)
        return also_known_as
    def bom(self) -> bool:
        return self._has_sig_or_bom
    def byte_order_mark(self) -> bool:
    def languages(self) -> list[str]:
        Return the complete list of possible languages found in decoded sequence.
        Usually not really useful. Returned list may be empty even if 'language' property return something != 'Unknown'.
        return [e[0] for e in self._languages]
    def language(self) -> str:
        Most probable language found in decoded sequence. If none were detected or inferred, the property will return
        "Unknown".
        if not self._languages:
            # Trying to infer the language based on the given encoding
            # Its either English or we should not pronounce ourselves in certain cases.
            if "ascii" in self.could_be_from_charset:
                return "English"
            # doing it there to avoid circular import
            from charset_normalizer.cd import encoding_languages, mb_encoding_languages
            languages = (
                mb_encoding_languages(self.encoding)
                if is_multi_byte_encoding(self.encoding)
                else encoding_languages(self.encoding)
            if len(languages) == 0 or "Latin Based" in languages:
            return languages[0]
        return self._languages[0][0]
    def chaos(self) -> float:
        return self._mean_mess_ratio
    def coherence(self) -> float:
        return self._languages[0][1]
    def percent_chaos(self) -> float:
        return round(self.chaos * 100, ndigits=3)
    def percent_coherence(self) -> float:
        return round(self.coherence * 100, ndigits=3)
    def raw(self) -> bytes:
        Original untouched bytes.
        return self._payload
    def submatch(self) -> list[CharsetMatch]:
        return self._leaves
    def has_submatch(self) -> bool:
        return len(self._leaves) > 0
    def alphabets(self) -> list[str]:
        if self._unicode_ranges is not None:
            return self._unicode_ranges
        # list detected ranges
        detected_ranges: list[str | None] = [unicode_range(char) for char in str(self)]
        # filter and sort
        self._unicode_ranges = sorted(list({r for r in detected_ranges if r}))
    def could_be_from_charset(self) -> list[str]:
        The complete list of encoding that output the exact SAME str result and therefore could be the originating
        encoding.
        This list does include the encoding available in property 'encoding'.
        return [self._encoding] + [m.encoding for m in self._leaves]
    def output(self, encoding: str = "utf_8") -> bytes:
        Method to get re-encoded bytes payload using given target encoding. Default to UTF-8.
        Any errors will be simply ignored by the encoder NOT replaced.
        if self._output_encoding is None or self._output_encoding != encoding:
            self._output_encoding = encoding
            decoded_string = str(self)
                self._preemptive_declaration is not None
                and self._preemptive_declaration.lower()
                not in ["utf-8", "utf8", "utf_8"]
                patched_header = sub(
                    RE_POSSIBLE_ENCODING_INDICATION,
                    lambda m: m.string[m.span()[0] : m.span()[1]].replace(
                        m.groups()[0],
                        iana_name(self._output_encoding).replace("_", "-"),  # type: ignore[arg-type]
                    decoded_string[:8192],
                    count=1,
                decoded_string = patched_header + decoded_string[8192:]
            self._output_payload = decoded_string.encode(encoding, "replace")
        return self._output_payload  # type: ignore
    def fingerprint(self) -> str:
        Retrieve the unique SHA256 computed using the transformed (re-encoded) payload. Not the original one.
        return sha256(self.output()).hexdigest()
class CharsetMatches:
    Container with every CharsetMatch items ordered by default from most probable to the less one.
    Act like a list(iterable) but does not implements all related methods.
    def __init__(self, results: list[CharsetMatch] | None = None):
        self._results: list[CharsetMatch] = sorted(results) if results else []
    def __iter__(self) -> Iterator[CharsetMatch]:
        yield from self._results
    def __getitem__(self, item: int | str) -> CharsetMatch:
        Retrieve a single item either by its position or encoding name (alias may be used here).
        Raise KeyError upon invalid index or encoding not present in results.
        if isinstance(item, int):
            return self._results[item]
            item = iana_name(item, False)
            for result in self._results:
                if item in result.could_be_from_charset:
        return len(self._results)
        return len(self._results) > 0
    def append(self, item: CharsetMatch) -> None:
        Insert a single match. Will be inserted accordingly to preserve sort.
        Can be inserted as a submatch.
        if not isinstance(item, CharsetMatch):
                "Cannot append instance '{}' to CharsetMatches".format(
                    str(item.__class__)
        # We should disable the submatch factoring when the input file is too heavy (conserve RAM usage)
        if len(item.raw) < TOO_BIG_SEQUENCE:
            for match in self._results:
                if match.fingerprint == item.fingerprint and match.chaos == item.chaos:
                    match.add_submatch(item)
        self._results.append(item)
        self._results = sorted(self._results)
    def best(self) -> CharsetMatch | None:
        Simply return the first match. Strict equivalent to matches[0].
        if not self._results:
        return self._results[0]
    def first(self) -> CharsetMatch | None:
        Redundant method, call the method best(). Kept for BC reasons.
        return self.best()
CoherenceMatch = Tuple[str, float]
CoherenceMatches = List[CoherenceMatch]
class CliDetectionResult:
        encoding: str | None,
        encoding_aliases: list[str],
        alternative_encodings: list[str],
        language: str,
        alphabets: list[str],
        chaos: float,
        coherence: float,
        unicode_path: str | None,
        is_preferred: bool,
        self.unicode_path: str | None = unicode_path
        self.encoding: str | None = encoding
        self.encoding_aliases: list[str] = encoding_aliases
        self.alternative_encodings: list[str] = alternative_encodings
        self.language: str = language
        self.alphabets: list[str] = alphabets
        self.has_sig_or_bom: bool = has_sig_or_bom
        self.chaos: float = chaos
        self.coherence: float = coherence
        self.is_preferred: bool = is_preferred
    def __dict__(self) -> dict[str, Any]:  # type: ignore
            "path": self.path,
            "encoding": self.encoding,
            "encoding_aliases": self.encoding_aliases,
            "alternative_encodings": self.alternative_encodings,
            "language": self.language,
            "alphabets": self.alphabets,
            "has_sig_or_bom": self.has_sig_or_bom,
            "chaos": self.chaos,
            "coherence": self.coherence,
            "unicode_path": self.unicode_path,
            "is_preferred": self.is_preferred,
    def to_json(self) -> str:
        return dumps(self.__dict__, ensure_ascii=True, indent=4)
# Use the `scipy.odr` namespace for importing the functions
    'Model', 'exponential', 'multilinear', 'unilinear',
    'quadratic', 'polynomial'
    return _sub_module_deprecation(sub_package="odr", module="models",
                                   private_modules=["_models"], all=__all__,
from scipy.linalg import eigh
from .settings import Options
from .utils import MaxEvalError, TargetSuccess, FeasibleSuccess
class Interpolation:
    Interpolation set.
    This class stores a base point around which the models are expanded and the
    interpolation points. The coordinates of the interpolation points are
    relative to the base point.
    def __init__(self, pb, options):
        Initialize the interpolation set.
        pb : `cobyqa.problem.Problem`
            Problem to be solved.
        options : dict
            Options of the solver.
        # Reduce the initial trust-region radius if necessary.
        self._debug = options[Options.DEBUG]
        max_radius = 0.5 * np.min(pb.bounds.xu - pb.bounds.xl)
        if options[Options.RHOBEG] > max_radius:
            options[Options.RHOBEG.value] = max_radius
                    max_radius,
        # Set the initial point around which the models are expanded.
        self._x_base = np.copy(pb.x0)
        very_close_xl_idx = (
            self.x_base <= pb.bounds.xl + 0.5 * options[Options.RHOBEG]
        self.x_base[very_close_xl_idx] = pb.bounds.xl[very_close_xl_idx]
        close_xl_idx = (
            pb.bounds.xl + 0.5 * options[Options.RHOBEG] < self.x_base
        ) & (self.x_base <= pb.bounds.xl + options[Options.RHOBEG])
        self.x_base[close_xl_idx] = np.minimum(
            pb.bounds.xl[close_xl_idx] + options[Options.RHOBEG],
            pb.bounds.xu[close_xl_idx],
        very_close_xu_idx = (
            self.x_base >= pb.bounds.xu - 0.5 * options[Options.RHOBEG]
        self.x_base[very_close_xu_idx] = pb.bounds.xu[very_close_xu_idx]
        close_xu_idx = (
            self.x_base < pb.bounds.xu - 0.5 * options[Options.RHOBEG]
        ) & (pb.bounds.xu - options[Options.RHOBEG] <= self.x_base)
        self.x_base[close_xu_idx] = np.maximum(
            pb.bounds.xu[close_xu_idx] - options[Options.RHOBEG],
            pb.bounds.xl[close_xu_idx],
        # Set the initial interpolation set.
        self._xpt = np.zeros((pb.n, options[Options.NPT]))
        for k in range(1, options[Options.NPT]):
            if k <= pb.n:
                if very_close_xu_idx[k - 1]:
                    self.xpt[k - 1, k] = -options[Options.RHOBEG]
                    self.xpt[k - 1, k] = options[Options.RHOBEG]
            elif k <= 2 * pb.n:
                if very_close_xl_idx[k - pb.n - 1]:
                    self.xpt[k - pb.n - 1, k] = 2.0 * options[Options.RHOBEG]
                elif very_close_xu_idx[k - pb.n - 1]:
                    self.xpt[k - pb.n - 1, k] = -2.0 * options[Options.RHOBEG]
                    self.xpt[k - pb.n - 1, k] = -options[Options.RHOBEG]
                spread = (k - pb.n - 1) // pb.n
                k1 = k - (1 + spread) * pb.n - 1
                k2 = (k1 + spread) % pb.n
                self.xpt[k1, k] = self.xpt[k1, k1 + 1]
                self.xpt[k2, k] = self.xpt[k2, k2 + 1]
    def n(self):
        return self.xpt.shape[0]
    def npt(self):
        Number of interpolation points.
        return self.xpt.shape[1]
    def xpt(self):
        Interpolation points.
        `numpy.ndarray`, shape (n, npt)
        return self._xpt
    @xpt.setter
    def xpt(self, xpt):
        Set the interpolation points.
        xpt : `numpy.ndarray`, shape (n, npt)
            New interpolation points.
        if self._debug:
            assert xpt.shape == (
                self.n,
                self.npt,
            ), "The shape of `xpt` is not valid."
        self._xpt = xpt
    def x_base(self):
        Base point around which the models are expanded.
        `numpy.ndarray`, shape (n,)
        return self._x_base
    @x_base.setter
    def x_base(self, x_base):
        Set the base point around which the models are expanded.
        x_base : `numpy.ndarray`, shape (n,)
            New base point around which the models are expanded.
            assert x_base.shape == (
            ), "The shape of `x_base` is not valid."
        self._x_base = x_base
    def point(self, k):
        Get the `k`-th interpolation point.
        The return point is relative to the origin.
        k : int
            Index of the interpolation point.
            `k`-th interpolation point.
            assert 0 <= k < self.npt, "The index `k` is not valid."
        return self.x_base + self.xpt[:, k]
_cache = {"xpt": None, "a": None, "right_scaling": None, "eigh": None}
def build_system(interpolation):
    Build the left-hand side matrix of the interpolation system. The
    matrix below stores W * diag(right_scaling),
    where W is the theoretical matrix of the interpolation system. The
    right scaling matrices is chosen to keep the elements in
    the matrix well-balanced.
    interpolation : `cobyqa.models.Interpolation`
    # Compute the scaled directions from the base point to the
    # interpolation points. We scale the directions to avoid numerical
    # difficulties.
    if _cache["xpt"] is not None and np.array_equal(
        interpolation.xpt, _cache["xpt"]
        return _cache["a"], _cache["right_scaling"], _cache["eigh"]
    scale = np.max(np.linalg.norm(interpolation.xpt, axis=0), initial=EPS)
    xpt_scale = interpolation.xpt / scale
    n, npt = xpt_scale.shape
    a = np.zeros((npt + n + 1, npt + n + 1))
    a[:npt, :npt] = 0.5 * (xpt_scale.T @ xpt_scale) ** 2.0
    a[:npt, npt] = 1.0
    a[:npt, npt + 1:] = xpt_scale.T
    a[npt, :npt] = 1.0
    a[npt + 1:, :npt] = xpt_scale
    # Build the left and right scaling diagonal matrices.
    right_scaling = np.empty(npt + n + 1)
    right_scaling[:npt] = 1.0 / scale**2.0
    right_scaling[npt] = scale**2.0
    right_scaling[npt + 1:] = scale
    eig_values, eig_vectors = eigh(a, check_finite=False)
    _cache["xpt"] = np.copy(interpolation.xpt)
    _cache["a"] = np.copy(a)
    _cache["right_scaling"] = np.copy(right_scaling)
    _cache["eigh"] = (eig_values, eig_vectors)
    return a, right_scaling, (eig_values, eig_vectors)
class Quadratic:
    Quadratic model.
    This class stores the Hessian matrix of the quadratic model using the
    implicit/explicit representation designed by Powell for NEWUOA [1]_.
    .. [1] M. J. D. Powell. The NEWUOA software for unconstrained optimization
       without derivatives. In G. Di Pillo and M. Roma, editors, *Large-Scale
       Nonlinear Optimization*, volume 83 of Nonconvex Optim. Appl., pages
       255--297. Springer, Boston, MA, USA, 2006. `doi:10.1007/0-387-30065-1_16
       <https://doi.org/10.1007/0-387-30065-1_16>`_.
    def __init__(self, interpolation, values, debug):
        Initialize the quadratic model.
        values : `numpy.ndarray`, shape (npt,)
            Values of the interpolated function at the interpolation points.
        debug : bool
            Whether to make debugging tests during the execution.
        `numpy.linalg.LinAlgError`
            If the interpolation system is ill-defined.
        self._debug = debug
            assert values.shape == (
                interpolation.npt,
            ), "The shape of `values` is not valid."
        if interpolation.npt < interpolation.n + 1:
                f"The number of interpolation points must be at least "
                f"{interpolation.n + 1}."
        self._const, self._grad, self._i_hess, _ = self._get_model(
            interpolation,
        self._e_hess = np.zeros((self.n, self.n))
    def __call__(self, x, interpolation):
        Evaluate the quadratic model at a given point.
            Point at which the quadratic model is evaluated.
        float
            Value of the quadratic model at `x`.
            assert x.shape == (self.n,), "The shape of `x` is not valid."
        x_diff = x - interpolation.x_base
            self._const
            + self._grad @ x_diff
            + 0.5
                self._i_hess @ (interpolation.xpt.T @ x_diff) ** 2.0
                + x_diff @ self._e_hess @ x_diff
        return self._grad.size
        Number of interpolation points used to define the quadratic model.
        return self._i_hess.size
    def grad(self, x, interpolation):
        Evaluate the gradient of the quadratic model at a given point.
            Point at which the gradient of the quadratic model is evaluated.
            Gradient of the quadratic model at `x`.
        return self._grad + self.hess_prod(x_diff, interpolation)
    def hess(self, interpolation):
        Evaluate the Hessian matrix of the quadratic model.
        `numpy.ndarray`, shape (n, n)
            Hessian matrix of the quadratic model.
        return self._e_hess + interpolation.xpt @ (
            self._i_hess[:, np.newaxis] * interpolation.xpt.T
    def hess_prod(self, v, interpolation):
        Evaluate the right product of the Hessian matrix of the quadratic model
        with a given vector.
        v : `numpy.ndarray`, shape (n,)
            Vector with which the Hessian matrix of the quadratic model is
            multiplied from the right.
            Right product of the Hessian matrix of the quadratic model with
            `v`.
            assert v.shape == (self.n,), "The shape of `v` is not valid."
        return self._e_hess @ v + interpolation.xpt @ (
            self._i_hess * (interpolation.xpt.T @ v)
    def curv(self, v, interpolation):
        Evaluate the curvature of the quadratic model along a given direction.
            Direction along which the curvature of the quadratic model is
            Curvature of the quadratic model along `v`.
            v @ self._e_hess @ v
            + self._i_hess @ (interpolation.xpt.T @ v) ** 2.0
    def update(self, interpolation, k_new, dir_old, values_diff):
        Update the quadratic model.
        This method applies the derivative-free symmetric Broyden update to the
        quadratic model. The `knew`-th interpolation point must be updated
        before calling this method.
            Updated interpolation set.
        k_new : int
            Index of the updated interpolation point.
        dir_old : `numpy.ndarray`, shape (n,)
            Value of ``interpolation.xpt[:, k_new]`` before the update.
        values_diff : `numpy.ndarray`, shape (npt,)
            Differences between the values of the interpolated nonlinear
            function and the previous quadratic model at the updated
            interpolation points.
            assert 0 <= k_new < self.npt, "The index `k_new` is not valid."
            assert dir_old.shape == (
            ), "The shape of `dir_old` is not valid."
            assert values_diff.shape == (
            ), "The shape of `values_diff` is not valid."
        # Forward the k_new-th element of the implicit Hessian matrix to the
        # explicit Hessian matrix. This must be done because the implicit
        # Hessian matrix is related to the interpolation points, and the
        # k_new-th interpolation point is modified.
        self._e_hess += self._i_hess[k_new] * np.outer(dir_old, dir_old)
        self._i_hess[k_new] = 0.0
        # Update the quadratic model.
        const, grad, i_hess, ill_conditioned = self._get_model(
            values_diff,
        self._const += const
        self._grad += grad
        self._i_hess += i_hess
        return ill_conditioned
    def shift_x_base(self, interpolation, new_x_base):
        Shift the point around which the quadratic model is defined.
            Previous interpolation set.
        new_x_base : `numpy.ndarray`, shape (n,)
            Point that will replace ``interpolation.x_base``.
            assert new_x_base.shape == (
            ), "The shape of `new_x_base` is not valid."
        self._const = self(new_x_base, interpolation)
        self._grad = self.grad(new_x_base, interpolation)
        shift = new_x_base - interpolation.x_base
        update = np.outer(
            shift,
            (interpolation.xpt - 0.5 * shift[:, np.newaxis]) @ self._i_hess,
        self._e_hess += update + update.T
    def solve_systems(interpolation, rhs):
        Solve the interpolation systems.
        rhs : `numpy.ndarray`, shape (npt + n + 1, m)
            Right-hand side vectors of the ``m`` interpolation systems.
        `numpy.ndarray`, shape (npt + n + 1, m)
            Solutions of the interpolation systems.
        `numpy.ndarray`, shape (m, )
            Whether the interpolation systems are ill-conditioned.
            If the interpolation systems are ill-defined.
        n, npt = interpolation.xpt.shape
            rhs.ndim == 2 and rhs.shape[0] == npt + n + 1
        ), "The shape of `rhs` is not valid."
        # Build the left-hand side matrix of the interpolation system. The
        # matrix below stores diag(left_scaling) * W * diag(right_scaling),
        # where W is the theoretical matrix of the interpolation system. The
        # left and right scaling matrices are chosen to keep the elements in
        # the matrix well-balanced.
        a, right_scaling, eig = build_system(interpolation)
        # Build the solution. After a discussion with Mike Saunders and Alexis
        # Montoison during their visit to the Hong Kong Polytechnic University
        # in 2024, we decided to use the eigendecomposition of the symmetric
        # matrix a. This is more stable than the previously employed LBL
        # decomposition, and allows us to directly detect ill-conditioning of
        # the system and to build the least-squares solution if necessary.
        # Numerical experiments have shown that this strategy improves the
        # performance of the solver.
        rhs_scaled = rhs * right_scaling[:, np.newaxis]
        if not (np.all(np.isfinite(a)) and np.all(np.isfinite(rhs_scaled))):
            raise np.linalg.LinAlgError(
                "The interpolation system is ill-defined."
        # calculated in build_system
        eig_values, eig_vectors = eig
        large_eig_values = np.abs(eig_values) > EPS
        eig_vectors = eig_vectors[:, large_eig_values]
        inv_eig_values = 1.0 / eig_values[large_eig_values]
        ill_conditioned = ~np.all(large_eig_values, 0)
        left_scaled_solutions = eig_vectors @ (
            (eig_vectors.T @ rhs_scaled) * inv_eig_values[:, np.newaxis]
            left_scaled_solutions * right_scaling[:, np.newaxis],
            ill_conditioned,
    def _get_model(interpolation, values):
        Solve the interpolation system.
            Constant term of the quadratic model.
            Gradient of the quadratic model at ``interpolation.x_base``.
        `numpy.ndarray`, shape (npt,)
            Implicit Hessian matrix of the quadratic model.
        x, ill_conditioned = Quadratic.solve_systems(
            np.block(
                        np.zeros(n + 1),
            ).T,
        return x[npt, 0], x[npt + 1:, 0], x[:npt, 0], ill_conditioned
class Models:
    Models for a nonlinear optimization problem.
    def __init__(self, pb, options, penalty):
        Initialize the models.
        penalty : float
            Penalty parameter used to select the point in the filter to forward
            to the callback function.
        `cobyqa.utils.MaxEvalError`
            If the maximum number of evaluations is reached.
        `cobyqa.utils.TargetSuccess`
            If a nearly feasible point has been found with an objective
            function value below the target.
        `cobyqa.utils.FeasibleSuccess`
            If a feasible point has been found for a feasibility problem.
        self._interpolation = Interpolation(pb, options)
        # Evaluate the nonlinear functions at the initial interpolation points.
        x_eval = self.interpolation.point(0)
        fun_init, cub_init, ceq_init = pb(x_eval, penalty)
        self._fun_val = np.full(options[Options.NPT], np.nan)
        self._cub_val = np.full((options[Options.NPT], cub_init.size), np.nan)
        self._ceq_val = np.full((options[Options.NPT], ceq_init.size), np.nan)
        for k in range(options[Options.NPT]):
            if k >= options[Options.MAX_EVAL]:
            if k == 0:
                self.fun_val[k] = fun_init
                self.cub_val[k, :] = cub_init
                self.ceq_val[k, :] = ceq_init
                x_eval = self.interpolation.point(k)
                self.fun_val[k], self.cub_val[k, :], self.ceq_val[k, :] = pb(
                    x_eval,
                    penalty,
            # Stop the iterations if the problem is a feasibility problem and
            # the current interpolation point is feasible.
                pb.is_feasibility
                and pb.maxcv(
                    self.interpolation.point(k),
                    self.cub_val[k, :],
                    self.ceq_val[k, :],
                <= options[Options.FEASIBILITY_TOL]
            # Stop the iterations if the current interpolation point is nearly
            # feasible and has an objective function value below the target.
                self._fun_val[k] <= options[Options.TARGET]
        # Build the initial quadratic models.
        self._fun = Quadratic(
            self.interpolation,
            self._fun_val,
            options[Options.DEBUG],
        self._cub = np.empty(self.m_nonlinear_ub, dtype=Quadratic)
        self._ceq = np.empty(self.m_nonlinear_eq, dtype=Quadratic)
        for i in range(self.m_nonlinear_ub):
            self._cub[i] = Quadratic(
                self.cub_val[:, i],
        for i in range(self.m_nonlinear_eq):
            self._ceq[i] = Quadratic(
                self.ceq_val[:, i],
            self._check_interpolation_conditions()
        Dimension of the problem.
        return self.interpolation.n
        return self.interpolation.npt
    def m_nonlinear_ub(self):
        Number of nonlinear inequality constraints.
        return self.cub_val.shape[1]
    def m_nonlinear_eq(self):
        Number of nonlinear equality constraints.
        return self.ceq_val.shape[1]
    def interpolation(self):
        `cobyqa.models.Interpolation`
        return self._interpolation
    def fun_val(self):
        Values of the objective function at the interpolation points.
        return self._fun_val
    def cub_val(self):
        Values of the nonlinear inequality constraint functions at the
        `numpy.ndarray`, shape (npt, m_nonlinear_ub)
        return self._cub_val
    def ceq_val(self):
        Values of the nonlinear equality constraint functions at the
        `numpy.ndarray`, shape (npt, m_nonlinear_eq)
        return self._ceq_val
    def fun(self, x):
        Evaluate the quadratic model of the objective function at a given
        point.
            Point at which to evaluate the quadratic model of the objective
            Value of the quadratic model of the objective function at `x`.
        return self._fun(x, self.interpolation)
    def fun_grad(self, x):
        Evaluate the gradient of the quadratic model of the objective function
        at a given point.
            Point at which to evaluate the gradient of the quadratic model of
            the objective function.
            Gradient of the quadratic model of the objective function at `x`.
        return self._fun.grad(x, self.interpolation)
    def fun_hess(self):
        Evaluate the Hessian matrix of the quadratic model of the objective
            Hessian matrix of the quadratic model of the objective function.
        return self._fun.hess(self.interpolation)
    def fun_hess_prod(self, v):
        of the objective function with a given vector.
            Vector with which the Hessian matrix of the quadratic model of the
            objective function is multiplied from the right.
            Right product of the Hessian matrix of the quadratic model of the
            objective function with `v`.
        return self._fun.hess_prod(v, self.interpolation)
    def fun_curv(self, v):
        Evaluate the curvature of the quadratic model of the objective function
        along a given direction.
            Direction along which the curvature of the quadratic model of the
            objective function is evaluated.
            Curvature of the quadratic model of the objective function along
        return self._fun.curv(v, self.interpolation)
    def fun_alt_grad(self, x):
        Evaluate the gradient of the alternative quadratic model of the
        objective function at a given point.
            Point at which to evaluate the gradient of the alternative
            quadratic model of the objective function.
            Gradient of the alternative quadratic model of the objective
            function at `x`.
        model = Quadratic(self.interpolation, self.fun_val, self._debug)
        return model.grad(x, self.interpolation)
    def cub(self, x, mask=None):
        Evaluate the quadratic models of the nonlinear inequality functions at
        a given point.
            Point at which to evaluate the quadratic models of the nonlinear
            inequality functions.
        mask : `numpy.ndarray`, shape (m_nonlinear_ub,), optional
            Mask of the quadratic models to consider.
        `numpy.ndarray`
            Values of the quadratic model of the nonlinear inequality
            assert mask is None or mask.shape == (
                self.m_nonlinear_ub,
            ), "The shape of `mask` is not valid."
        return np.array(
            [model(x, self.interpolation) for model in self._get_cub(mask)]
    def cub_grad(self, x, mask=None):
        Evaluate the gradients of the quadratic models of the nonlinear
        inequality functions at a given point.
            Point at which to evaluate the gradients of the quadratic models of
            the nonlinear inequality functions.
        mask : `numpy.ndarray`, shape (m_nonlinear_eq,), optional
            Gradients of the quadratic model of the nonlinear inequality
        return np.reshape(
            [model.grad(x, self.interpolation)
             for model in self._get_cub(mask)],
            (-1, self.n),
    def cub_hess(self, mask=None):
        Evaluate the Hessian matrices of the quadratic models of the nonlinear
            Hessian matrices of the quadratic models of the nonlinear
            [model.hess(self.interpolation) for model in self._get_cub(mask)],
            (-1, self.n, self.n),
    def cub_hess_prod(self, v, mask=None):
        Evaluate the right product of the Hessian matrices of the quadratic
        models of the nonlinear inequality functions with a given vector.
            Vector with which the Hessian matrices of the quadratic models of
            the nonlinear inequality functions are multiplied from the right.
            Right products of the Hessian matrices of the quadratic models of
            the nonlinear inequality functions with `v`.
                model.hess_prod(v, self.interpolation)
                for model in self._get_cub(mask)
    def cub_curv(self, v, mask=None):
        Evaluate the curvature of the quadratic models of the nonlinear
        inequality functions along a given direction.
            Direction along which the curvature of the quadratic models of the
            nonlinear inequality functions is evaluated.
            Curvature of the quadratic models of the nonlinear inequality
            functions along `v`.
            [model.curv(v, self.interpolation)
             for model in self._get_cub(mask)]
    def ceq(self, x, mask=None):
        Evaluate the quadratic models of the nonlinear equality functions at a
        given point.
            equality functions.
            Values of the quadratic model of the nonlinear equality functions.
                self.m_nonlinear_eq,
            [model(x, self.interpolation) for model in self._get_ceq(mask)]
    def ceq_grad(self, x, mask=None):
        equality functions at a given point.
            the nonlinear equality functions.
            Gradients of the quadratic model of the nonlinear equality
             for model in self._get_ceq(mask)],
    def ceq_hess(self, mask=None):
            Hessian matrices of the quadratic models of the nonlinear equality
            [model.hess(self.interpolation) for model in self._get_ceq(mask)],
    def ceq_hess_prod(self, v, mask=None):
        models of the nonlinear equality functions with a given vector.
            the nonlinear equality functions are multiplied from the right.
            the nonlinear equality functions with `v`.
                for model in self._get_ceq(mask)
    def ceq_curv(self, v, mask=None):
        equality functions along a given direction.
            nonlinear equality functions is evaluated.
            Curvature of the quadratic models of the nonlinear equality
             for model in self._get_ceq(mask)]
    def reset_models(self):
        Set the quadratic models of the objective function, nonlinear
        inequality constraints, and nonlinear equality constraints to the
        alternative quadratic models.
        self._fun = Quadratic(self.interpolation, self.fun_val, self._debug)
                self._debug,
    def update_interpolation(self, k_new, x_new, fun_val, cub_val, ceq_val):
        Update the interpolation set.
        This method updates the interpolation set by replacing the `knew`-th
        interpolation point with `xnew`. It also updates the function values
        and the quadratic models.
        x_new : `numpy.ndarray`, shape (n,)
            New interpolation point. Its value is interpreted as relative to
            the origin, not the base point.
        fun_val : float
            Value of the objective function at `x_new`.
            Objective function value at `x_new`.
        cub_val : `numpy.ndarray`, shape (m_nonlinear_ub,)
            Values of the nonlinear inequality constraints at `x_new`.
        ceq_val : `numpy.ndarray`, shape (m_nonlinear_eq,)
            Values of the nonlinear equality constraints at `x_new`.
            assert x_new.shape == (self.n,), \
                "The shape of `x_new` is not valid."
            assert isinstance(fun_val, float), \
                "The function value is not valid."
            assert cub_val.shape == (
            ), "The shape of `cub_val` is not valid."
            assert ceq_val.shape == (
            ), "The shape of `ceq_val` is not valid."
        # Compute the updates in the interpolation conditions.
        fun_diff = np.zeros(self.npt)
        cub_diff = np.zeros(self.cub_val.shape)
        ceq_diff = np.zeros(self.ceq_val.shape)
        fun_diff[k_new] = fun_val - self.fun(x_new)
        cub_diff[k_new, :] = cub_val - self.cub(x_new)
        ceq_diff[k_new, :] = ceq_val - self.ceq(x_new)
        # Update the function values.
        self.fun_val[k_new] = fun_val
        self.cub_val[k_new, :] = cub_val
        self.ceq_val[k_new, :] = ceq_val
        dir_old = np.copy(self.interpolation.xpt[:, k_new])
        self.interpolation.xpt[:, k_new] = x_new - self.interpolation.x_base
        # Update the quadratic models.
        ill_conditioned = self._fun.update(
            dir_old,
            fun_diff,
            ill_conditioned = ill_conditioned or self._cub[i].update(
                cub_diff[:, i],
            ill_conditioned = ill_conditioned or self._ceq[i].update(
                ceq_diff[:, i],
    def determinants(self, x_new, k_new=None):
        Compute the normalized determinants of the new interpolation systems.
        k_new : int, optional
            Index of the updated interpolation point. If `k_new` is not
            specified, all the possible determinants are computed.
        {float, `numpy.ndarray`, shape (npt,)}
            Determinant(s) of the new interpolation system.
        The determinants are normalized by the determinant of the current
        interpolation system. For stability reasons, the calculations are done
        using the formula (2.12) in [1]_.
        .. [1] M. J. D. Powell. On updating the inverse of a KKT matrix.
           Technical Report DAMTP 2004/NA01, Department of Applied Mathematics
           and Theoretical Physics, University of Cambridge, Cambridge, UK,
           2004.
                k_new is None or 0 <= k_new < self.npt
            ), "The index `k_new` is not valid."
        # Compute the values independent of k_new.
        shift = x_new - self.interpolation.x_base
        new_col = np.empty((self.npt + self.n + 1, 1))
        new_col[: self.npt, 0] = (
                0.5 * (self.interpolation.xpt.T @ shift) ** 2.0)
        new_col[self.npt, 0] = 1.0
        new_col[self.npt + 1:, 0] = shift
        inv_new_col = Quadratic.solve_systems(self.interpolation, new_col)[0]
        beta = 0.5 * (shift @ shift) ** 2.0 - new_col[:, 0] @ inv_new_col[:, 0]
        # Compute the values that depend on k.
        if k_new is None:
            coord_vec = np.eye(self.npt + self.n + 1, self.npt)
            alpha = np.diag(
                Quadratic.solve_systems(
                    coord_vec,
            tau = inv_new_col[: self.npt, 0]
            coord_vec = np.eye(self.npt + self.n + 1, 1, -k_new)
            alpha = Quadratic.solve_systems(
            )[
            ][k_new, 0]
            tau = inv_new_col[k_new, 0]
        return alpha * beta + tau**2.0
    def shift_x_base(self, new_x_base, options):
        Shift the base point without changing the interpolation set.
            New base point.
        # Update the models.
        self._fun.shift_x_base(self.interpolation, new_x_base)
        for model in self._cub:
            model.shift_x_base(self.interpolation, new_x_base)
        for model in self._ceq:
        # Update the base point and the interpolation points.
        shift = new_x_base - self.interpolation.x_base
        self.interpolation.x_base += shift
        self.interpolation.xpt -= shift[:, np.newaxis]
        if options[Options.DEBUG]:
    def _get_cub(self, mask=None):
        Get the quadratic models of the nonlinear inequality constraints.
            Mask of the quadratic models to return.
            Quadratic models of the nonlinear inequality constraints.
        return self._cub if mask is None else self._cub[mask]
    def _get_ceq(self, mask=None):
        Get the quadratic models of the nonlinear equality constraints.
            Quadratic models of the nonlinear equality constraints.
        return self._ceq if mask is None else self._ceq[mask]
    def _check_interpolation_conditions(self):
        Check the interpolation conditions of all quadratic models.
        error_fun = 0.0
        error_cub = 0.0
        error_ceq = 0.0
        for k in range(self.npt):
            error_fun = np.max(
                    error_fun,
                    np.abs(
                        self.fun(self.interpolation.point(k)) - self.fun_val[k]
            error_cub = np.max(
                    self.cub(self.interpolation.point(k)) - self.cub_val[k, :]
                initial=error_cub,
            error_ceq = np.max(
                    self.ceq(self.interpolation.point(k)) - self.ceq_val[k, :]
                initial=error_ceq,
        tol = 10.0 * np.sqrt(EPS) * max(self.n, self.npt)
        if error_fun > tol * np.max(np.abs(self.fun_val), initial=1.0):
                "The interpolation conditions for the objective function are "
                "not satisfied.",
        if error_cub > tol * np.max(np.abs(self.cub_val), initial=1.0):
                "The interpolation conditions for the inequality constraint "
                "function are not satisfied.",
        if error_ceq > tol * np.max(np.abs(self.ceq_val), initial=1.0):
                "The interpolation conditions for the equality constraint "
from urllib3.exceptions import (
from urllib3.fields import RequestField
from urllib3.filepost import encode_multipart_formdata
from urllib3.util import parse_url
        import idna
"""Models for WebSocket protocol versions 13 and 8."""
from typing import Any, Callable, Final, NamedTuple, Optional, cast
WS_DEFLATE_TRAILING: Final[bytes] = bytes([0x00, 0x00, 0xFF, 0xFF])
class WSCloseCode(IntEnum):
    OK = 1000
    GOING_AWAY = 1001
    PROTOCOL_ERROR = 1002
    UNSUPPORTED_DATA = 1003
    ABNORMAL_CLOSURE = 1006
    INVALID_TEXT = 1007
    POLICY_VIOLATION = 1008
    MESSAGE_TOO_BIG = 1009
    MANDATORY_EXTENSION = 1010
    INTERNAL_ERROR = 1011
    SERVICE_RESTART = 1012
    TRY_AGAIN_LATER = 1013
    BAD_GATEWAY = 1014
class WSMsgType(IntEnum):
    # websocket spec types
    CONTINUATION = 0x0
    TEXT = 0x1
    BINARY = 0x2
    PING = 0x9
    PONG = 0xA
    CLOSE = 0x8
    # aiohttp specific types
    CLOSING = 0x100
    CLOSED = 0x101
    ERROR = 0x102
    text = TEXT
    binary = BINARY
    ping = PING
    pong = PONG
    close = CLOSE
    closing = CLOSING
    closed = CLOSED
    error = ERROR
class WSMessage(NamedTuple):
    type: WSMsgType
    # To type correctly, this would need some kind of tagged union for each type.
    extra: Optional[str]
    def json(self, *, loads: Callable[[Any], Any] = json.loads) -> Any:
        """Return parsed JSON data.
        .. versionadded:: 0.22
        return loads(self.data)
# Constructing the tuple directly to avoid the overhead of
# the lambda and arg processing since NamedTuples are constructed
# with a run time built lambda
# https://github.com/python/cpython/blob/d83fcf8371f2f33c7797bc8f5423a8bca8c46e5c/Lib/collections/__init__.py#L441
WS_CLOSED_MESSAGE = tuple.__new__(WSMessage, (WSMsgType.CLOSED, None, None))
WS_CLOSING_MESSAGE = tuple.__new__(WSMessage, (WSMsgType.CLOSING, None, None))
class WebSocketError(Exception):
    """WebSocket protocol parser error."""
    def __init__(self, code: int, message: str) -> None:
        super().__init__(code, message)
        return cast(str, self.args[1])
class WSHandshakeError(Exception):
    """WebSocket protocol handshake error."""
    from .core import TyperCommand, TyperGroup
    from .main import Typer
AnyType = type[Any]
Required = ...
class Context(click.Context):
    The [`Context`](https://click.palletsprojects.com/en/stable/api/#click.Context) has some additional data about the current execution of your program.
    When declaring it in a [callback](https://typer.tiangolo.com/tutorial/options/callback-and-context/) function,
    you can access this additional information.
class FileText(io.TextIOWrapper):
    Gives you a file-like object for reading text, and you will get a `str` data from it.
    The default mode of this class is `mode="r"`.
    def main(config: Annotated[typer.FileText, typer.Option()]):
        for line in config:
            print(f"Config line: {line}")
class FileTextWrite(FileText):
    You can use this class for writing text. Alternatively, you can use `FileText` with `mode="w"`.
    The default mode of this class is `mode="w"`.
    def main(config: Annotated[typer.FileTextWrite, typer.Option()]):
        config.write("Some config written by the app")
        print("Config written")
class FileBinaryRead(io.BufferedReader):
    You can use this class to read binary data, receiving `bytes`.
    The default mode of this class is `mode="rb"`.
    It is useful for reading binary files like images:
    def main(file: Annotated[typer.FileBinaryRead, typer.Option()]):
        processed_total = 0
        for bytes_chunk in file:
            # Process the bytes in bytes_chunk
            processed_total += len(bytes_chunk)
            print(f"Processed bytes total: {processed_total}")
class FileBinaryWrite(io.BufferedWriter):
    You can use this class to write binary data: you pass `bytes` to it instead of strings.
    The default mode of this class is `mode="wb"`.
    It is useful for writing binary files like images:
    def main(file: Annotated[typer.FileBinaryWrite, typer.Option()]):
        first_line_str = "some settings\\n"
        # You cannot write str directly to a binary file; encode it first
        first_line_bytes = first_line_str.encode("utf-8")
        # Then you can write the bytes
        file.write(first_line_bytes)
        # This is already bytes, it starts with b"
        second_line = b"la cig\xc3\xbce\xc3\xb1a trae al ni\xc3\xb1o"
        file.write(second_line)
        print("Binary file written")
class CallbackParam(click.Parameter):
    In a callback function, you can declare a function parameter with type `CallbackParam`
    to access the specific Click [`Parameter`](https://click.palletsprojects.com/en/stable/api/#click.Parameter) object.
class DefaultPlaceholder:
    You shouldn't use this class directly.
    It's used internally to recognize when a default value has been overwritten, even
    if the new value is `None`.
    def __init__(self, value: Any):
        return bool(self.value)
DefaultType = TypeVar("DefaultType")
CommandFunctionType = TypeVar("CommandFunctionType", bound=Callable[..., Any])
def Default(value: DefaultType) -> DefaultType:
    You shouldn't use this function directly.
    return DefaultPlaceholder(value)  # type: ignore
class CommandInfo:
        name: Optional[str] = None,
        cls: Optional[type["TyperCommand"]] = None,
        context_settings: Optional[dict[Any, Any]] = None,
        help: Optional[str] = None,
        epilog: Optional[str] = None,
        short_help: Optional[str] = None,
        options_metavar: str = "[OPTIONS]",
        add_help_option: bool = True,
        no_args_is_help: bool = False,
        hidden: bool = False,
        deprecated: bool = False,
        rich_help_panel: Union[str, None] = None,
        self.context_settings = context_settings
        self.help = help
        self.epilog = epilog
        self.short_help = short_help
        self.options_metavar = options_metavar
        self.add_help_option = add_help_option
        self.no_args_is_help = no_args_is_help
        self.hidden = hidden
        self.deprecated = deprecated
class TyperInfo:
        typer_instance: Optional["Typer"] = Default(None),
        name: Optional[str] = Default(None),
        cls: Optional[type["TyperGroup"]] = Default(None),
        invoke_without_command: bool = Default(False),
        no_args_is_help: bool = Default(False),
        subcommand_metavar: Optional[str] = Default(None),
        chain: bool = Default(False),
        result_callback: Optional[Callable[..., Any]] = Default(None),
        context_settings: Optional[dict[Any, Any]] = Default(None),
        callback: Optional[Callable[..., Any]] = Default(None),
        help: Optional[str] = Default(None),
        epilog: Optional[str] = Default(None),
        short_help: Optional[str] = Default(None),
        options_metavar: str = Default("[OPTIONS]"),
        add_help_option: bool = Default(True),
        hidden: bool = Default(False),
        deprecated: bool = Default(False),
        rich_help_panel: Union[str, None] = Default(None),
        self.typer_instance = typer_instance
        self.invoke_without_command = invoke_without_command
        self.subcommand_metavar = subcommand_metavar
        self.result_callback = result_callback
class ParameterInfo:
        default: Optional[Any] = None,
        param_decls: Optional[Sequence[str]] = None,
        metavar: Optional[str] = None,
        expose_value: bool = True,
        is_eager: bool = False,
        envvar: Optional[Union[str, list[str]]] = None,
        # Note that shell_complete is not fully supported and will be removed in future versions
        # TODO: Remove shell_complete in a future version (after 0.16.0)
        shell_complete: Optional[
            Callable[
                [click.Context, click.Parameter, str],
                Union[list["click.shell_completion.CompletionItem"], list[str]],
        autocompletion: Optional[Callable[..., Any]] = None,
        default_factory: Optional[Callable[[], Any]] = None,
        # Custom type
        parser: Optional[Callable[[str], Any]] = None,
        click_type: Optional[click.ParamType] = None,
        show_default: Union[bool, str] = True,
        show_choices: bool = True,
        show_envvar: bool = True,
        # Choice
        min: Optional[Union[int, float]] = None,
        max: Optional[Union[int, float]] = None,
        clamp: bool = False,
        # DateTime
        formats: Optional[list[str]] = None,
        # File
        mode: Optional[str] = None,
        errors: Optional[str] = "strict",
        lazy: Optional[bool] = None,
        # Path
        file_okay: bool = True,
        dir_okay: bool = True,
        writable: bool = False,
        readable: bool = True,
        resolve_path: bool = False,
        allow_dash: bool = False,
        path_type: Union[None, type[str], type[bytes]] = None,
        # Check if user has provided multiple custom parsers
        if parser and click_type:
                "Multiple custom type parsers provided. "
                "`parser` and `click_type` may not both be provided."
        self.param_decls = param_decls
        self.metavar = metavar
        self.expose_value = expose_value
        self.is_eager = is_eager
        self.envvar = envvar
        self.shell_complete = shell_complete
        self.autocompletion = autocompletion
        self.default_factory = default_factory
        self.click_type = click_type
        self.show_default = show_default
        self.show_choices = show_choices
        self.show_envvar = show_envvar
        self.case_sensitive = case_sensitive
        self.min = min
        self.max = max
        self.clamp = clamp
        self.formats = formats
        self.lazy = lazy
        self.exists = exists
        self.file_okay = file_okay
        self.dir_okay = dir_okay
        self.writable = writable
        self.readable = readable
        self.resolve_path = resolve_path
        self.allow_dash = allow_dash
        self.path_type = path_type
class OptionInfo(ParameterInfo):
        # ParameterInfo
        prompt: Union[bool, str] = False,
        confirmation_prompt: bool = False,
        prompt_required: bool = True,
        hide_input: bool = False,
        # TODO: remove is_flag and flag_value in a future release
        is_flag: Optional[bool] = None,
        flag_value: Optional[Any] = None,
        count: bool = False,
        allow_from_autoenv: bool = True,
            metavar=metavar,
            expose_value=expose_value,
            is_eager=is_eager,
            envvar=envvar,
            shell_complete=shell_complete,
            autocompletion=autocompletion,
            default_factory=default_factory,
            parser=parser,
            click_type=click_type,
            show_default=show_default,
            show_choices=show_choices,
            show_envvar=show_envvar,
            case_sensitive=case_sensitive,
            min=min,
            max=max,
            clamp=clamp,
            formats=formats,
            lazy=lazy,
            atomic=atomic,
            exists=exists,
            file_okay=file_okay,
            dir_okay=dir_okay,
            writable=writable,
            readable=readable,
            resolve_path=resolve_path,
            allow_dash=allow_dash,
            path_type=path_type,
        if is_flag is not None or flag_value is not None:
                "The 'is_flag' and 'flag_value' parameters are not supported by Typer "
                "and will be removed entirely in a future release.",
        self.prompt = prompt
        self.confirmation_prompt = confirmation_prompt
        self.prompt_required = prompt_required
        self.hide_input = hide_input
        self.allow_from_autoenv = allow_from_autoenv
class ArgumentInfo(ParameterInfo):
class ParamMeta:
    empty = inspect.Parameter.empty
        default: Any = inspect.Parameter.empty,
        annotation: Any = inspect.Parameter.empty,
        self.annotation = annotation
class DeveloperExceptionConfig:
        pretty_exceptions_enable: bool = True,
        pretty_exceptions_show_locals: bool = True,
        pretty_exceptions_short: bool = True,
class TyperPath(click.Path):
    # Overwrite Click's behaviour to be compatible with Typer's autocompletion system
        self, ctx: click.Context, param: click.Parameter, incomplete: str
    ) -> list[click.shell_completion.CompletionItem]:
        """Return an empty list so that the autocompletion functionality
        will work properly from the commandline.
            f"/models/{model}",
from typing import Any, Callable, Optional, Union
def _unwrapped_call(call: Optional[Callable[..., Any]]) -> Any:
    call: Optional[Callable[..., Any]] = None
    request_param_name: Optional[str] = None
    websocket_param_name: Optional[str] = None
    http_connection_param_name: Optional[str] = None
    response_param_name: Optional[str] = None
    background_tasks_param_name: Optional[str] = None
    security_scopes_param_name: Optional[str] = None
    own_oauth_scopes: Optional[list[str]] = None
    parent_oauth_scopes: Optional[list[str]] = None
    path: Optional[str] = None
    scope: Union[Literal["function", "request"], None] = None
    def computed_scope(self) -> Union[str, None]:
    class EmailStr(str):  # type: ignore
    url: Optional[AnyUrl] = None
    email: Optional[EmailStr] = None
    identifier: Optional[str] = None
    termsOfService: Optional[str] = None
    contact: Optional[Contact] = None
    license: Optional[License] = None
    enum: Annotated[Optional[list[str]], Field(min_length=1)] = None
    url: Union[AnyUrl, str]
    variables: Optional[dict[str, ServerVariable]] = None
    mapping: Optional[dict[str, str]] = None
    prefix: Optional[str] = None
    attribute: Optional[bool] = None
    wrapped: Optional[bool] = None
    schema_: Optional[str] = Field(default=None, alias="$schema")
    vocabulary: Optional[str] = Field(default=None, alias="$vocabulary")
    id: Optional[str] = Field(default=None, alias="$id")
    anchor: Optional[str] = Field(default=None, alias="$anchor")
    dynamicAnchor: Optional[str] = Field(default=None, alias="$dynamicAnchor")
    ref: Optional[str] = Field(default=None, alias="$ref")
    dynamicRef: Optional[str] = Field(default=None, alias="$dynamicRef")
    defs: Optional[dict[str, "SchemaOrBool"]] = Field(default=None, alias="$defs")
    comment: Optional[str] = Field(default=None, alias="$comment")
    allOf: Optional[list["SchemaOrBool"]] = None
    anyOf: Optional[list["SchemaOrBool"]] = None
    oneOf: Optional[list["SchemaOrBool"]] = None
    dependentSchemas: Optional[dict[str, "SchemaOrBool"]] = None
    prefixItems: Optional[list["SchemaOrBool"]] = None
    properties: Optional[dict[str, "SchemaOrBool"]] = None
    patternProperties: Optional[dict[str, "SchemaOrBool"]] = None
    type: Optional[Union[SchemaType, list[SchemaType]]] = None
    enum: Optional[list[Any]] = None
    const: Optional[Any] = None
    multipleOf: Optional[float] = Field(default=None, gt=0)
    maximum: Optional[float] = None
    exclusiveMaximum: Optional[float] = None
    minimum: Optional[float] = None
    exclusiveMinimum: Optional[float] = None
    maxLength: Optional[int] = Field(default=None, ge=0)
    minLength: Optional[int] = Field(default=None, ge=0)
    pattern: Optional[str] = None
    maxItems: Optional[int] = Field(default=None, ge=0)
    minItems: Optional[int] = Field(default=None, ge=0)
    uniqueItems: Optional[bool] = None
    maxContains: Optional[int] = Field(default=None, ge=0)
    minContains: Optional[int] = Field(default=None, ge=0)
    maxProperties: Optional[int] = Field(default=None, ge=0)
    minProperties: Optional[int] = Field(default=None, ge=0)
    required: Optional[list[str]] = None
    dependentRequired: Optional[dict[str, set[str]]] = None
    format: Optional[str] = None
    contentEncoding: Optional[str] = None
    contentMediaType: Optional[str] = None
    default: Optional[Any] = None
    deprecated: Optional[bool] = None
    readOnly: Optional[bool] = None
    writeOnly: Optional[bool] = None
    examples: Optional[list[Any]] = None
    discriminator: Optional[Discriminator] = None
    xml: Optional[XML] = None
    externalDocs: Optional[ExternalDocumentation] = None
        Optional[Any],
SchemaOrBool = Union[Schema, bool]
    summary: Optional[str]
    value: Optional[Any]
    externalValue: Optional[AnyUrl]
    contentType: Optional[str] = None
    headers: Optional[dict[str, Union["Header", Reference]]] = None
    style: Optional[str] = None
    explode: Optional[bool] = None
    allowReserved: Optional[bool] = None
    schema_: Optional[Union[Schema, Reference]] = Field(default=None, alias="schema")
    example: Optional[Any] = None
    examples: Optional[dict[str, Union[Example, Reference]]] = None
    encoding: Optional[dict[str, Encoding]] = None
    required: Optional[bool] = None
    content: Optional[dict[str, MediaType]] = None
    operationRef: Optional[str] = None
    operationId: Optional[str] = None
    parameters: Optional[dict[str, Union[Any, str]]] = None
    requestBody: Optional[Union[Any, str]] = None
    server: Optional[Server] = None
    headers: Optional[dict[str, Union[Header, Reference]]] = None
    links: Optional[dict[str, Union[Link, Reference]]] = None
    tags: Optional[list[str]] = None
    parameters: Optional[list[Union[Parameter, Reference]]] = None
    requestBody: Optional[Union[RequestBody, Reference]] = None
    responses: Optional[dict[str, Union[Response, Any]]] = None
    callbacks: Optional[dict[str, Union[dict[str, "PathItem"], Reference]]] = None
    security: Optional[list[dict[str, list[str]]]] = None
    servers: Optional[list[Server]] = None
    get: Optional[Operation] = None
    put: Optional[Operation] = None
    post: Optional[Operation] = None
    delete: Optional[Operation] = None
    options: Optional[Operation] = None
    head: Optional[Operation] = None
    patch: Optional[Operation] = None
    trace: Optional[Operation] = None
    bearerFormat: Optional[str] = None
    refreshUrl: Optional[str] = None
    implicit: Optional[OAuthFlowImplicit] = None
    password: Optional[OAuthFlowPassword] = None
    clientCredentials: Optional[OAuthFlowClientCredentials] = None
    authorizationCode: Optional[OAuthFlowAuthorizationCode] = None
SecurityScheme = Union[APIKey, HTTPBase, OAuth2, OpenIdConnect, HTTPBearer]
    schemas: Optional[dict[str, Union[Schema, Reference]]] = None
    responses: Optional[dict[str, Union[Response, Reference]]] = None
    parameters: Optional[dict[str, Union[Parameter, Reference]]] = None
    requestBodies: Optional[dict[str, Union[RequestBody, Reference]]] = None
    securitySchemes: Optional[dict[str, Union[SecurityScheme, Reference]]] = None
    callbacks: Optional[dict[str, Union[dict[str, PathItem], Reference, Any]]] = None
    pathItems: Optional[dict[str, Union[PathItem, Reference]]] = None
    jsonSchemaDialect: Optional[str] = None
    paths: Optional[dict[str, Union[PathItem, Any]]] = None
    webhooks: Optional[dict[str, Union[PathItem, Reference]]] = None
    components: Optional[Components] = None
    tags: Optional[list[Tag]] = None
"""Contains commands to interact with models on the Hugging Face Hub.
    # list models on the Hub
    hf models ls
    # list models with a search query
    hf models ls --search "llama"
    # get info about a model
    hf models info Lightricks/LTX-2
from typing import Annotated, Optional, get_args
from huggingface_hub.errors import CLIError, RepositoryNotFoundError, RevisionNotFoundError
from huggingface_hub.hf_api import ExpandModelProperty_T, ModelSort_T
    AuthorOpt,
    FilterOpt,
    FormatOpt,
    LimitOpt,
    QuietOpt,
    SearchOpt,
    api_object_to_dict,
    make_expand_properties_parser,
    print_list_output,
_EXPAND_PROPERTIES = sorted(get_args(ExpandModelProperty_T))
_SORT_OPTIONS = get_args(ModelSort_T)
ModelSortEnum = enum.Enum("ModelSortEnum", {s: s for s in _SORT_OPTIONS}, type=str)  # type: ignore[misc]
ExpandOpt = Annotated[
        help=f"Comma-separated properties to expand. Example: '--expand=downloads,likes,tags'. Valid: {', '.join(_EXPAND_PROPERTIES)}.",
        callback=make_expand_properties_parser(_EXPAND_PROPERTIES),
models_cli = typer_factory(help="Interact with models on the Hub.")
@models_cli.command(
    "ls",
        "hf models ls --sort downloads --limit 10",
        'hf models ls --search "llama" --author meta-llama',
def models_ls(
    search: SearchOpt = None,
    author: AuthorOpt = None,
    filter: FilterOpt = None,
        Optional[ModelSortEnum],
        typer.Option(help="Sort results."),
    limit: LimitOpt = 10,
    expand: ExpandOpt = None,
    format: FormatOpt = OutputFormat.table,
    quiet: QuietOpt = False,
    """List models on the Hub."""
    sort_key = sort.value if sort else None
    results = [
        api_object_to_dict(model_info)
        for model_info in api.list_models(
            filter=filter, author=author, search=search, sort=sort_key, limit=limit, expand=expand
    print_list_output(results, format=format, quiet=quiet)
        "hf models info meta-llama/Llama-3.2-1B-Instruct",
        "hf models info gpt2 --expand downloads,likes,tags",
def models_info(
    model_id: Annotated[str, typer.Argument(help="The model ID (e.g. `username/repo-name`).")],
    """Get info about a model on the Hub."""
        info = api.model_info(repo_id=model_id, revision=revision, expand=expand)  # type: ignore[arg-type]
    except RepositoryNotFoundError as e:
        raise CLIError(f"Model '{model_id}' not found.") from e
    except RevisionNotFoundError as e:
        raise CLIError(f"Revision '{revision}' not found on '{model_id}'.") from e
    print(json.dumps(api_object_to_dict(info), indent=2))
from typing import Union, Literal
from pydantic import BaseModel, Field, field_validator
def validate_different_content(v: Union[str, dict, list]) -> str:
    if v in ((), {}, []):
    elif isinstance(v, dict) and "text" in v:
        return v['text']
    elif isinstance(v, list):
        new_v = []
        for item in v:
            if isinstance(item, dict) and "text" in item:
                if item['text']:
                    new_v.append(item['text'])
            elif isinstance(item, str):
                new_v.append(item)
        return '\n'.join(new_v)
    elif isinstance(v, str):
    raise ValueError("Content must be a string")
    type_: Literal["text"] = Field(default="text", alias="type")
class ImageURLContent(BaseModel):
    detail: str = "auto"
class ImageContent(BaseModel):
    type_: Literal["image_url"] = Field(default="image_url", alias="type")
    image_url: ImageURLContent
class FunctionObj(BaseModel):
    description: str = ""
    parameters: dict = {}
    strict: bool = False
class ChatCompletionTool(BaseModel):
    type_: Literal["function"] = Field(default="function", alias="type")
    function: FunctionTool
class MessageToolCall(BaseModel):
    function: FunctionObj
class SAPMessage(BaseModel):
    Model for SystemChatMessage and DeveloperChatMessage
    role: Literal["system", "developer"] = "system"
    _content_validator = field_validator("content", mode="before")(validate_different_content)
class SAPUserMessage(BaseModel):
    role: Literal["user"] = "user"
    content: Union[
        str, TextContent, ImageContent, list[Union[TextContent, ImageContent]]
class SAPAssistantMessage(BaseModel):
    role: Literal["assistant"] = "assistant"
    content: str = ""
    refusal: str = ""
    tool_calls: list[MessageToolCall] = []
class SAPToolChatMessage(BaseModel):
    role: Literal["tool"] = "tool"
class ResponseFormat(BaseModel):
    type_: Literal["text", "json_object"] = Field(default="text", alias="type")
class JSONResponseSchema(BaseModel):
    schema_: dict = Field(default_factory=dict, alias="schema")
    type_: Literal["json_schema"] = Field(default="json_schema", alias="type")
    json_schema: JSONResponseSchema
from typing import List, Dict, Any, Optional, Union
from .exceptions import UnauthorizedError, NotFoundError
class ModelsManagementClient:
        Initialize the ModelsManagementClient.
    def list(self, return_request: bool = False) -> Union[List[Dict[str, Any]], requests.Request]:
        Get the list of models supported by the server.
            return_request (bool): If True, returns the prepared request object instead of executing it.
                                 Useful for inspection or modification before sending.
            Union[List[Dict[str, Any]], requests.Request]: Either a list of model information dictionaries
            or a prepared request object if return_request is True.
        url = f"{self._base_url}/models"
        request = requests.Request("GET", url, headers=self._get_headers())
            return response.json()["data"]
        model_params: Dict[str, Any],
        model_info: Optional[Dict[str, Any]] = None,
        Add a new model to the proxy.
            model_name (str): Name of the model to add
            model_params (Dict[str, Any]): Parameters for the model (e.g., model type, api_base, api_key)
            model_info (Optional[Dict[str, Any]]): Additional information about the model
            Union[Dict[str, Any], requests.Request]: Either the response from the server or
        url = f"{self._base_url}/model/new"
            "litellm_params": model_params,
        if model_info:
            data["model_info"] = model_info
    def delete(self, model_id: str, return_request: bool = False) -> Union[Dict[str, Any], requests.Request]:
        Delete a model from the proxy.
            model_id (str): ID of the model to delete (e.g., "2f23364f-4579-4d79-a43a-2d48dd551c2e")
            NotFoundError: If the request fails with a 404 status code or indicates the model was not found
        url = f"{self._base_url}/model/delete"
        data = {"id": model_id}
            if e.response.status_code == 404 or "not found" in e.response.text.lower():
                raise NotFoundError(e)
        self, model_id: Optional[str] = None, model_name: Optional[str] = None, return_request: bool = False
        Get information about a specific model by its ID or name.
            model_id (Optional[str]): ID of the model to retrieve
            model_name (Optional[str]): Name of the model to retrieve
            Union[Dict[str, Any], requests.Request]: Either the model information from the server or
            ValueError: If neither model_id nor model_name is provided, or if both are provided
            NotFoundError: If the model is not found
        if (model_id is None and model_name is None) or (model_id is not None and model_name is not None):
            raise ValueError("Exactly one of model_id or model_name must be provided")
        # If return_request is True, delegate to info
            result = self.info(return_request=True)
            assert isinstance(result, requests.Request)
        # Get all models and filter
        models = self.info()
        assert isinstance(models, List)
        # Find the matching model
            if (model_id and model.get("model_info", {}).get("id") == model_id) or (
                model_name and model.get("model_name") == model_name
        # If we get here, no model was found
        if model_id:
            msg = f"Model with id={model_id} not found"
        elif model_name:
            msg = f"Model with model_name={model_name} not found"
            msg = "Unknown error trying to find model"
        raise NotFoundError(
            requests.exceptions.HTTPError(
                response=requests.Response(),  # Empty response since we didn't make a direct request
    def info(self, return_request: bool = False) -> Union[List[Dict[str, Any]], requests.Request]:
        Get detailed information about all models from the server.
            or a prepared request object if return_request is True
        url = f"{self._base_url}/v1/model/info"
        Update an existing model's configuration.
            model_id (str): ID of the model to update
            model_params (Dict[str, Any]): New parameters for the model (e.g., model type, api_base, api_key)
        url = f"{self._base_url}/model/update"
from typing import Optional, Literal, Any
class ModelYamlInfo:
    model_params: dict[str, Any]
    model_info: dict[str, Any]
    access_groups: list[str]
    provider: str
    def access_groups_str(self) -> str:
        return ", ".join(self.access_groups) if self.access_groups else ""
def _get_model_info_obj_from_yaml(model: dict[str, Any]) -> ModelYamlInfo:
    """Extract model info from a model dict and return as ModelYamlInfo dataclass."""
    model_name: str = model["model_name"]
    model_params: dict[str, Any] = model["litellm_params"]
    model_info: dict[str, Any] = model.get("model_info", {})
    model_id: str = model_params["model"]
    access_groups = model_info.get("access_groups", [])
    provider = model_id.split("/", 1)[0] if "/" in model_id else model_id
    return ModelYamlInfo(
        model_name=model_name,
        model_id=model_id,
        access_groups=access_groups,
def format_iso_datetime_str(iso_datetime_str: Optional[str]) -> str:
    """Format an ISO format datetime string to human-readable date with minute resolution."""
    if not iso_datetime_str:
        # Parse ISO format datetime string
        dt = datetime.fromisoformat(iso_datetime_str.replace("Z", "+00:00"))
        return dt.strftime("%Y-%m-%d %H:%M")
        return str(iso_datetime_str)
def format_timestamp(timestamp: Optional[int]) -> str:
    """Format a Unix timestamp (integer) to human-readable date with minute resolution."""
        dt = datetime.fromtimestamp(timestamp)
        return str(timestamp)
def format_cost_per_1k_tokens(cost: Optional[float]) -> str:
    """Format a per-token cost to cost per 1000 tokens."""
    if cost is None:
        # Convert string to float if needed
        cost_float = float(cost)
        # Multiply by 1000 and format to 4 decimal places
        return f"${cost_float * 1000:.4f}"
        return str(cost)
def create_client(ctx: click.Context) -> Client:
    """Helper function to create a client from context."""
    return Client(base_url=ctx.obj["base_url"], api_key=ctx.obj["api_key"])
def models() -> None:
    """Manage models on your LiteLLM proxy server"""
@models.command("list")
    "output_format",
    type=click.Choice(["table", "json"]),
    default="table",
    help="Output format (table or json)",
def list_models(ctx: click.Context, output_format: Literal["table", "json"]) -> None:
    """List all available models"""
    client = create_client(ctx)
    assert isinstance(models_list, list)
    if output_format == "json":
        rich.print_json(data=models_list)
    else:  # table format
        table = rich.table.Table(title="Available Models")
        # Add columns based on the data structure
        table.add_column("ID", style="cyan")
        table.add_column("Object", style="green")
        table.add_column("Created", style="magenta")
        # Add rows
        for model in models_list:
            created = model.get("created")
            # Convert string timestamp to integer if needed
            if isinstance(created, str) and created.isdigit():
                created = int(created)
                str(model.get("object", "model")),
                format_timestamp(created) if isinstance(created, int) else format_iso_datetime_str(created),
                str(model.get("owned_by", "")),
        rich.print(table)
@models.command("add")
@click.argument("model-name")
    "--param",
    help="Model parameters in key=value format (can be specified multiple times)",
    "--info",
    help="Model info in key=value format (can be specified multiple times)",
def add_model(ctx: click.Context, model_name: str, param: tuple[str, ...], info: tuple[str, ...]) -> None:
    """Add a new model to the proxy"""
    # Convert parameters from key=value format to dict
    model_params = dict(p.split("=", 1) for p in param)
    model_info = dict(i.split("=", 1) for i in info) if info else None
    result = client.models.new(
    rich.print_json(data=result)
@models.command("delete")
@click.argument("model-id")
def delete_model(ctx: click.Context, model_id: str) -> None:
    """Delete a model from the proxy"""
    result = client.models.delete(model_id=model_id)
@models.command("get")
@click.option("--id", "model_id", help="ID of the model to retrieve")
@click.option("--name", "model_name", help="Name of the model to retrieve")
def get_model(ctx: click.Context, model_id: Optional[str], model_name: Optional[str]) -> None:
    """Get information about a specific model"""
    if not model_id and not model_name:
        raise click.UsageError("Either --id or --name must be provided")
    result = client.models.get(model_id=model_id, model_name=model_name)
@models.command("info")
    "--columns",
    "columns",
    default="public_model,upstream_model,updated_at",
    help="Comma-separated list of columns to display. Valid columns: public_model, upstream_model, credential_name, created_at, updated_at, id, input_cost, output_cost. Default: public_model,upstream_model,updated_at",
def get_models_info(ctx: click.Context, output_format: Literal["table", "json"], columns: str) -> None:
    """Get detailed information about all models"""
    models_info = client.models.info()
    assert isinstance(models_info, list)
        rich.print_json(data=models_info)
        table = rich.table.Table(title="Models Information")
        # Define all possible columns with their configurations
        column_configs: dict[str, dict[str, Any]] = {
            "public_model": {
                "header": "Public Model",
                "style": "cyan",
                "get_value": lambda m: str(m.get("model_name", "")),
            "upstream_model": {
                "header": "Upstream Model",
                "style": "green",
                "get_value": lambda m: str(m.get("litellm_params", {}).get("model", "")),
            "credential_name": {
                "header": "Credential Name",
                "style": "yellow",
                "get_value": lambda m: str(m.get("litellm_params", {}).get("litellm_credential_name", "")),
            "created_at": {
                "header": "Created At",
                "style": "magenta",
                "get_value": lambda m: format_iso_datetime_str(m.get("model_info", {}).get("created_at")),
            "updated_at": {
                "header": "Updated At",
                "get_value": lambda m: format_iso_datetime_str(m.get("model_info", {}).get("updated_at")),
                "header": "ID",
                "style": "blue",
                "get_value": lambda m: str(m.get("model_info", {}).get("id", "")),
            "input_cost": {
                "header": "Input Cost",
                "justify": "right",
                "get_value": lambda m: format_cost_per_1k_tokens(m.get("model_info", {}).get("input_cost_per_token")),
            "output_cost": {
                "header": "Output Cost",
                "get_value": lambda m: format_cost_per_1k_tokens(m.get("model_info", {}).get("output_cost_per_token")),
        # Add requested columns
        requested_columns = [col.strip() for col in columns.split(",")]
        for col_name in requested_columns:
            if col_name in column_configs:
                config = column_configs[col_name]
                table.add_column(config["header"], style=config["style"], justify=config.get("justify", "left"))
                click.echo(f"Warning: Unknown column '{col_name}'", err=True)
        # Add rows with only the requested columns
        for model in models_info:
            row_values = []
                    row_values.append(column_configs[col_name]["get_value"](model))
            if row_values:
                table.add_row(*row_values)
@models.command("update")
def update_model(ctx: click.Context, model_id: str, param: tuple[str, ...], info: tuple[str, ...]) -> None:
    """Update an existing model's configuration"""
    result = client.models.update(
def _filter_model(model, model_regex, access_group_regex):
    model_name = model.get("model_name")
    model_params = model.get("litellm_params")
    model_info = model.get("model_info", {})
    if not model_name or not model_params:
    model_id = model_params.get("model")
    if not model_id or not isinstance(model_id, str):
    if model_regex and not model_regex.search(model_id):
    if access_group_regex:
        if not isinstance(access_groups, list):
        if not any(isinstance(group, str) and access_group_regex.search(group) for group in access_groups):
def _print_models_table(added_models: list[ModelYamlInfo], table_title: str):
    if not added_models:
    table = rich.table.Table(title=table_title)
    table.add_column("Model Name", style="cyan")
    table.add_column("Upstream Model", style="green")
    table.add_column("Access Groups", style="magenta")
    for m in added_models:
        table.add_row(m.model_name, m.model_id, m.access_groups_str)
def _print_summary_table(provider_counts):
    summary_table = rich.table.Table(title="Model Import Summary")
    summary_table.add_column("Provider", style="cyan")
    summary_table.add_column("Count", style="green")
    for provider, count in provider_counts.items():
        summary_table.add_row(str(provider), str(count))
    total = sum(provider_counts.values())
    summary_table.add_row("[bold]Total[/bold]", f"[bold]{total}[/bold]")
    rich.print(summary_table)
def get_model_list_from_yaml_file(yaml_file: str) -> list[dict[str, Any]]:
    """Load and validate the model list from a YAML file."""
    with open(yaml_file, "r") as f:
    if not data or "model_list" not in data:
        raise click.ClickException("YAML file must contain a 'model_list' key with a list of models.")
    model_list = data["model_list"]
    if not isinstance(model_list, list):
        raise click.ClickException("'model_list' must be a list of model definitions.")
def _get_filtered_model_list(model_list, only_models_matching_regex, only_access_groups_matching_regex):
    """Return a list of models that pass the filter criteria."""
    model_regex = re.compile(only_models_matching_regex) if only_models_matching_regex else None
    access_group_regex = re.compile(only_access_groups_matching_regex) if only_access_groups_matching_regex else None
    return [model for model in model_list if _filter_model(model, model_regex, access_group_regex)]
def _import_models_get_table_title(dry_run: bool) -> str:
        return "Models that would be imported if [yellow]--dry-run[/yellow] was not provided"
        return "Models Imported"
@models.command("import")
@click.argument("yaml_file", type=click.Path(exists=True, dir_okay=False, readable=True))
@click.option("--dry-run", is_flag=True, help="Show what would be imported without making any changes.")
    "--only-models-matching-regex",
    help="Only import models where litellm_params.model matches the given regex.",
    "--only-access-groups-matching-regex",
    help="Only import models where at least one item in model_info.access_groups matches the given regex.",
def import_models(
    yaml_file: str,
    dry_run: bool,
    only_models_matching_regex: Optional[str],
    only_access_groups_matching_regex: Optional[str],
    """Import models from a YAML file and add them to the proxy."""
    provider_counts: dict[str, int] = defaultdict(int)
    added_models: list[ModelYamlInfo] = []
    model_list = get_model_list_from_yaml_file(yaml_file)
    filtered_model_list = _get_filtered_model_list(
        model_list, only_models_matching_regex, only_access_groups_matching_regex
    for model in filtered_model_list:
        model_info_obj = _get_model_info_obj_from_yaml(model)
                client.models.new(
                    model_name=model_info_obj.model_name,
                    model_params=model_info_obj.model_params,
                    model_info=model_info_obj.model_info,
                pass  # For summary, ignore errors
        added_models.append(model_info_obj)
        provider_counts[model_info_obj.provider] += 1
    table_title = _import_models_get_table_title(dry_run)
    _print_models_table(added_models, table_title)
    _print_summary_table(provider_counts)
from dataclasses import dataclass, fields
from aider import __version__
from aider.llm import litellm
from aider.openrouter import OpenRouterModelManager
from aider.sendchat import ensure_alternating_roles, sanity_check_messages
from aider.utils import check_pip_install_extra
RETRY_TIMEOUT = 60
request_timeout = 600
DEFAULT_MODEL_NAME = "gpt-4o"
ANTHROPIC_BETA_HEADER = "prompt-caching-2024-07-31,pdfs-2024-09-25"
OPENAI_MODELS = """
o1
o1-preview
o1-mini
o3-mini
gpt-4
gpt-4o
gpt-4o-2024-05-13
gpt-4-turbo-preview
gpt-4-0314
gpt-4-0613
gpt-4-32k
gpt-4-32k-0314
gpt-4-32k-0613
gpt-4-turbo
gpt-4-turbo-2024-04-09
gpt-4-1106-preview
gpt-4-0125-preview
gpt-4-vision-preview
gpt-4-1106-vision-preview
gpt-4o-mini
gpt-4o-mini-2024-07-18
gpt-3.5-turbo
gpt-3.5-turbo-0301
gpt-3.5-turbo-0613
gpt-3.5-turbo-1106
gpt-3.5-turbo-0125
gpt-3.5-turbo-16k
gpt-3.5-turbo-16k-0613
OPENAI_MODELS = [ln.strip() for ln in OPENAI_MODELS.splitlines() if ln.strip()]
ANTHROPIC_MODELS = """
claude-2
claude-2.1
claude-3-haiku-20240307
claude-3-5-haiku-20241022
claude-3-opus-20240229
claude-3-sonnet-20240229
claude-3-5-sonnet-20240620
claude-3-5-sonnet-20241022
claude-sonnet-4-20250514
claude-opus-4-20250514
claude-opus-4-6
claude-sonnet-4-5
claude-sonnet-4-5-20250929
claude-haiku-4-5
claude-haiku-4-5-20251001
ANTHROPIC_MODELS = [ln.strip() for ln in ANTHROPIC_MODELS.splitlines() if ln.strip()]
# Mapping of model aliases to their canonical names
MODEL_ALIASES = {
    # Claude models
    "sonnet": "claude-sonnet-4-5",
    "haiku": "claude-haiku-4-5",
    "opus": "claude-opus-4-6",
    # GPT models
    "4": "gpt-4-0613",
    "4o": "gpt-4o",
    "4-turbo": "gpt-4-1106-preview",
    "35turbo": "gpt-3.5-turbo",
    "35-turbo": "gpt-3.5-turbo",
    "3": "gpt-3.5-turbo",
    "deepseek": "deepseek/deepseek-chat",
    "flash": "gemini/gemini-flash-latest",
    "flash-lite": "gemini/gemini-2.5-flash-lite",
    "quasar": "openrouter/openrouter/quasar-alpha",
    "r1": "deepseek/deepseek-reasoner",
    "gemini-2.5-pro": "gemini/gemini-2.5-pro",
    "gemini-3-pro-preview": "gemini/gemini-3-pro-preview",
    "gemini": "gemini/gemini-3-pro-preview",
    "gemini-exp": "gemini/gemini-2.5-pro-exp-03-25",
    "grok3": "xai/grok-3-beta",
    "optimus": "openrouter/openrouter/optimus-alpha",
# Model metadata loaded from resources and user's files.
class ModelSettings:
    # Model class needs to have each of these as well
    edit_format: str = "whole"
    weak_model_name: Optional[str] = None
    use_repo_map: bool = False
    send_undo_reply: bool = False
    lazy: bool = False
    overeager: bool = False
    reminder: str = "user"
    examples_as_sys_msg: bool = False
    extra_params: Optional[dict] = None
    cache_control: bool = False
    caches_by_default: bool = False
    use_system_prompt: bool = True
    use_temperature: Union[bool, float] = True
    streaming: bool = True
    editor_model_name: Optional[str] = None
    editor_edit_format: Optional[str] = None
    reasoning_tag: Optional[str] = None
    remove_reasoning: Optional[str] = None  # Deprecated alias for reasoning_tag
    system_prompt_prefix: Optional[str] = None
    accepts_settings: Optional[list] = None
# Load model settings from package resource
MODEL_SETTINGS = []
with importlib.resources.open_text("aider.resources", "model-settings.yml") as f:
    model_settings_list = yaml.safe_load(f)
    for model_settings_dict in model_settings_list:
        MODEL_SETTINGS.append(ModelSettings(**model_settings_dict))
class ModelInfoManager:
    MODEL_INFO_URL = (
        "https://raw.githubusercontent.com/BerriAI/litellm/main/"
        "model_prices_and_context_window.json"
    CACHE_TTL = 60 * 60 * 24  # 24 hours
        self.cache_dir = Path.home() / ".aider" / "caches"
        self.cache_file = self.cache_dir / "model_prices_and_context_window.json"
        self.content = None
        self.local_model_metadata = {}
        self.verify_ssl = True
        self._cache_loaded = False
        # Manager for the cached OpenRouter model database
        self.openrouter_manager = OpenRouterModelManager()
    def set_verify_ssl(self, verify_ssl):
        if hasattr(self, "openrouter_manager"):
            self.openrouter_manager.set_verify_ssl(verify_ssl)
    def _load_cache(self):
        if self._cache_loaded:
            self.cache_dir.mkdir(parents=True, exist_ok=True)
            if self.cache_file.exists():
                cache_age = time.time() - self.cache_file.stat().st_mtime
                if cache_age < self.CACHE_TTL:
                        self.content = json.loads(self.cache_file.read_text())
                        # If the cache file is corrupted, treat it as missing
        self._cache_loaded = True
    def _update_cache(self):
            # Respect the --no-verify-ssl switch
            response = requests.get(self.MODEL_INFO_URL, timeout=5, verify=self.verify_ssl)
                self.content = response.json()
                    self.cache_file.write_text(json.dumps(self.content, indent=4))
            print(str(ex))
                # Save empty dict to cache file on failure
                self.cache_file.write_text("{}")
    def get_model_from_cached_json_db(self, model):
        data = self.local_model_metadata.get(model)
        # Ensure cache is loaded before checking content
        self._load_cache()
            self._update_cache()
            return dict()
        info = self.content.get(model, dict())
        pieces = model.split("/")
        if len(pieces) == 2:
            info = self.content.get(pieces[1])
            if info and info.get("litellm_provider") == pieces[0]:
    def get_model_info(self, model):
        cached_info = self.get_model_from_cached_json_db(model)
        litellm_info = None
        if litellm._lazy_module or not cached_info:
                litellm_info = litellm.get_model_info(model)
                if "model_prices_and_context_window.json" not in str(ex):
        if litellm_info:
            return litellm_info
        if not cached_info and model.startswith("openrouter/"):
            # First try using the locally cached OpenRouter model database
            openrouter_info = self.openrouter_manager.get_model_info(model)
            if openrouter_info:
                return openrouter_info
            # Fallback to legacy web-scraping if the API cache does not contain the model
            openrouter_info = self.fetch_openrouter_model_info(model)
        return cached_info
    def fetch_openrouter_model_info(self, model):
        Fetch model info by scraping the openrouter model page.
        Expected URL: https://openrouter.ai/<model_route>
        Example: openrouter/qwen/qwen-2.5-72b-instruct:free
        Returns a dict with keys: max_tokens, max_input_tokens, max_output_tokens,
        input_cost_per_token, output_cost_per_token.
        url_part = model[len("openrouter/") :]
        url = "https://openrouter.ai/" + url_part
            response = requests.get(url, timeout=5, verify=self.verify_ssl)
            html = response.text
            if re.search(
                rf"The model\s*.*{re.escape(url_part)}.* is not available", html, re.IGNORECASE
                print(f"\033[91mError: Model '{url_part}' is not available\033[0m")
            text = re.sub(r"<[^>]+>", " ", html)
            context_match = re.search(r"([\d,]+)\s*context", text)
            if context_match:
                context_str = context_match.group(1).replace(",", "")
                context_size = int(context_str)
                context_size = None
            input_cost_match = re.search(r"\$\s*([\d.]+)\s*/M input tokens", text, re.IGNORECASE)
            output_cost_match = re.search(r"\$\s*([\d.]+)\s*/M output tokens", text, re.IGNORECASE)
            input_cost = float(input_cost_match.group(1)) / 1000000 if input_cost_match else None
            output_cost = float(output_cost_match.group(1)) / 1000000 if output_cost_match else None
            if context_size is None or input_cost is None or output_cost is None:
                "max_input_tokens": context_size,
                "max_tokens": context_size,
                "max_output_tokens": context_size,
                "input_cost_per_token": input_cost,
                "output_cost_per_token": output_cost,
            print("Error fetching openrouter info:", str(e))
model_info_manager = ModelInfoManager()
class Model(ModelSettings):
        self, model, weak_model=None, editor_model=None, editor_edit_format=None, verbose=False
        # Map any alias to its canonical name
        model = MODEL_ALIASES.get(model, model)
        self.name = model
        self.max_chat_history_tokens = 1024
        self.weak_model = None
        self.editor_model = None
        # Find the extra settings
        self.extra_model_settings = next(
            (ms for ms in MODEL_SETTINGS if ms.name == "aider/extra_params"), None
        self.info = self.get_model_info(model)
        # Are all needed keys/params available?
        res = self.validate_environment()
        self.missing_keys = res.get("missing_keys")
        self.keys_in_environment = res.get("keys_in_environment")
        max_input_tokens = self.info.get("max_input_tokens") or 0
        # Calculate max_chat_history_tokens as 1/16th of max_input_tokens,
        # with minimum 1k and maximum 8k
        self.max_chat_history_tokens = min(max(max_input_tokens / 16, 1024), 8192)
        self.configure_model_settings(model)
        if weak_model is False:
            self.weak_model_name = None
            self.get_weak_model(weak_model)
        if editor_model is False:
            self.editor_model_name = None
            self.get_editor_model(editor_model, editor_edit_format)
        return model_info_manager.get_model_info(model)
    def _copy_fields(self, source):
        """Helper to copy fields from a ModelSettings instance to self"""
        for field in fields(ModelSettings):
            val = getattr(source, field.name)
            setattr(self, field.name, val)
        # Handle backward compatibility: if remove_reasoning is set but reasoning_tag isn't,
        # use remove_reasoning's value for reasoning_tag
        if self.reasoning_tag is None and self.remove_reasoning is not None:
            self.reasoning_tag = self.remove_reasoning
    def configure_model_settings(self, model):
        # Look for exact model match
        exact_match = False
        for ms in MODEL_SETTINGS:
            # direct match, or match "provider/<model>"
            if model == ms.name:
                self._copy_fields(ms)
                exact_match = True
                break  # Continue to apply overrides
        # Initialize accepts_settings if it's None
        if self.accepts_settings is None:
            self.accepts_settings = []
        # If no exact match, try generic settings
        if not exact_match:
            self.apply_generic_model_settings(model)
        # Apply override settings last if they exist
            self.extra_model_settings
            and self.extra_model_settings.extra_params
            and self.extra_model_settings.name == "aider/extra_params"
            # Initialize extra_params if it doesn't exist
            if not self.extra_params:
                self.extra_params = {}
            # Deep merge the extra_params dicts
            for key, value in self.extra_model_settings.extra_params.items():
                if isinstance(value, dict) and isinstance(self.extra_params.get(key), dict):
                    # For nested dicts, merge recursively
                    self.extra_params[key] = {**self.extra_params[key], **value}
                    # For non-dict values, simply update
                    self.extra_params[key] = value
        # Ensure OpenRouter models accept thinking_tokens and reasoning_effort
        if self.name.startswith("openrouter/"):
            if "thinking_tokens" not in self.accepts_settings:
                self.accepts_settings.append("thinking_tokens")
            if "reasoning_effort" not in self.accepts_settings:
                self.accepts_settings.append("reasoning_effort")
    def apply_generic_model_settings(self, model):
        if "/o3-mini" in model:
            self.edit_format = "diff"
            self.use_repo_map = True
            self.use_temperature = False
            self.system_prompt_prefix = "Formatting re-enabled. "
            return  # <--
        if "gpt-4.1-mini" in model:
            self.reminder = "sys"
            self.examples_as_sys_msg = False
        if "gpt-4.1" in model:
        last_segment = model.split("/")[-1]
        if last_segment in ("gpt-5", "gpt-5-2025-08-07"):
        if "/o1-mini" in model:
            self.use_system_prompt = False
        if "/o1-preview" in model:
        if "/o1" in model:
            self.streaming = False
        if "deepseek" in model and "v3" in model:
            self.examples_as_sys_msg = True
        if "deepseek" in model and ("r1" in model or "reasoning" in model):
            self.reasoning_tag = "think"
        if ("llama3" in model or "llama-3" in model) and "70b" in model:
            self.send_undo_reply = True
        if "gpt-4-turbo" in model or ("gpt-4-" in model and "-preview" in model):
            self.edit_format = "udiff"
        if "gpt-4" in model or "claude-3-opus" in model:
        if "gpt-3.5" in model or "gpt-4" in model:
            "sonnet-4-5" in model
            or "opus-4-6" in model
            or "haiku-4-5" in model
            or "claude-sonnet-4-5" in model
            or "claude-opus-4-6" in model
            or "claude-haiku-4-5" in model
        if "3-7-sonnet" in model:
            self.reminder = "user"
        if "3.5-sonnet" in model or "3-5-sonnet" in model:
        if model.startswith("o1-") or "/o1-" in model:
            "qwen" in model
            and "coder" in model
            and ("2.5" in model or "2-5" in model)
            and "32b" in model
            self.editor_edit_format = "editor-diff"
        if "qwq" in model and "32b" in model and "preview" not in model:
            self.use_temperature = 0.6
            self.extra_params = dict(top_p=0.95)
        if "qwen3" in model and "235b" in model:
            self.system_prompt_prefix = "/no_think"
            self.use_temperature = 0.7
            self.extra_params = {"top_p": 0.8, "top_k": 20, "min_p": 0.0}
        # use the defaults
        if self.edit_format == "diff":
    def get_weak_model(self, provided_weak_model_name):
        # If weak_model_name is provided, override the model settings
        if provided_weak_model_name:
            self.weak_model_name = provided_weak_model_name
        if not self.weak_model_name:
            self.weak_model = self
        if self.weak_model_name == self.name:
        self.weak_model = Model(
            self.weak_model_name,
            weak_model=False,
        return self.weak_model
    def commit_message_models(self):
        return [self.weak_model, self]
    def get_editor_model(self, provided_editor_model_name, editor_edit_format):
        # If editor_model_name is provided, override the model settings
        if provided_editor_model_name:
            self.editor_model_name = provided_editor_model_name
        if editor_edit_format:
            self.editor_edit_format = editor_edit_format
        if not self.editor_model_name or self.editor_model_name == self.name:
            self.editor_model = self
            self.editor_model = Model(
                self.editor_model_name,
                editor_model=False,
        if not self.editor_edit_format:
            self.editor_edit_format = self.editor_model.edit_format
            if self.editor_edit_format in ("diff", "whole", "diff-fenced"):
                self.editor_edit_format = "editor-" + self.editor_edit_format
        return self.editor_model
    def tokenizer(self, text):
        return litellm.encode(model=self.name, text=text)
    def token_count(self, messages):
        if type(messages) is list:
                return litellm.token_counter(model=self.name, messages=messages)
                print(f"Unable to count tokens: {err}")
        if not self.tokenizer:
        if type(messages) is str:
            msgs = messages
            msgs = json.dumps(messages)
            return len(self.tokenizer(msgs))
    def token_count_for_image(self, fname):
        Calculate the token cost for an image assuming high detail.
        The token cost is determined by the size of the image.
        :param fname: The filename of the image.
        :return: The token cost for the image.
        width, height = self.get_image_size(fname)
        # If the image is larger than 2048 in any dimension, scale it down to fit within 2048x2048
        max_dimension = max(width, height)
        if max_dimension > 2048:
            scale_factor = 2048 / max_dimension
            width = int(width * scale_factor)
            height = int(height * scale_factor)
        # Scale the image such that the shortest side is 768 pixels long
        min_dimension = min(width, height)
        scale_factor = 768 / min_dimension
        # Calculate the number of 512x512 tiles needed to cover the image
        tiles_width = math.ceil(width / 512)
        tiles_height = math.ceil(height / 512)
        num_tiles = tiles_width * tiles_height
        # Each tile costs 170 tokens, and there's an additional fixed cost of 85 tokens
        token_cost = num_tiles * 170 + 85
        return token_cost
    def get_image_size(self, fname):
        Retrieve the size of an image.
        :return: A tuple (width, height) representing the image size in pixels.
        with Image.open(fname) as img:
            return img.size
    def fast_validate_environment(self):
        """Fast path for common models. Avoids forcing litellm import."""
        model = self.name
        if len(pieces) > 1:
            provider = pieces[0]
            provider = None
        keymap = dict(
            openrouter="OPENROUTER_API_KEY",
            openai="OPENAI_API_KEY",
            deepseek="DEEPSEEK_API_KEY",
            gemini="GEMINI_API_KEY",
            anthropic="ANTHROPIC_API_KEY",
            groq="GROQ_API_KEY",
            fireworks_ai="FIREWORKS_API_KEY",
        var = None
        if model in OPENAI_MODELS:
            var = "OPENAI_API_KEY"
        elif model in ANTHROPIC_MODELS:
            var = "ANTHROPIC_API_KEY"
            var = keymap.get(provider)
        if var and os.environ.get(var):
            return dict(keys_in_environment=[var], missing_keys=[])
    def validate_environment(self):
        res = self.fast_validate_environment()
        # https://github.com/BerriAI/litellm/issues/3190
        res = litellm.validate_environment(model)
        # If missing AWS credential keys but AWS_PROFILE is set, consider AWS credentials valid
        if res["missing_keys"] and any(
            key in ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"] for key in res["missing_keys"]
            if model.startswith("bedrock/") or model.startswith("us.anthropic."):
                if os.environ.get("AWS_PROFILE"):
                    res["missing_keys"] = [
                        for k in res["missing_keys"]
                        if k not in ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"]
                    if not res["missing_keys"]:
                        res["keys_in_environment"] = True
        if res["keys_in_environment"]:
        if res["missing_keys"]:
        provider = self.info.get("litellm_provider", "").lower()
        if provider == "cohere_chat":
            return validate_variables(["COHERE_API_KEY"])
        if provider == "gemini":
            return validate_variables(["GEMINI_API_KEY"])
        if provider == "groq":
            return validate_variables(["GROQ_API_KEY"])
    def get_repo_map_tokens(self):
        map_tokens = 1024
        max_inp_tokens = self.info.get("max_input_tokens")
        if max_inp_tokens:
            map_tokens = max_inp_tokens / 8
            map_tokens = min(map_tokens, 4096)
            map_tokens = max(map_tokens, 1024)
        return map_tokens
    def set_reasoning_effort(self, effort):
        """Set the reasoning effort parameter for models that support it"""
        if effort is not None:
                if "extra_body" not in self.extra_params:
                    self.extra_params["extra_body"] = {}
                self.extra_params["extra_body"]["reasoning"] = {"effort": effort}
                self.extra_params["extra_body"]["reasoning_effort"] = effort
    def parse_token_value(self, value):
        Parse a token value string into an integer.
        Accepts formats: 8096, "8k", "10.5k", "0.5M", "10K", etc.
            value: String or int token value
            Integer token value
            return int(value)  # Try to convert to int
        value = value.strip().upper()
        if value.endswith("K"):
            multiplier = 1024
            value = value[:-1]
        elif value.endswith("M"):
            multiplier = 1024 * 1024
            multiplier = 1
        # Convert to float first to handle decimal values like "10.5k"
        return int(float(value) * multiplier)
    def set_thinking_tokens(self, value):
        Set the thinking token budget for models that support it.
        Pass "0" to disable thinking tokens.
            num_tokens = self.parse_token_value(value)
            # OpenRouter models use 'reasoning' instead of 'thinking'
                if num_tokens > 0:
                    self.extra_params["extra_body"]["reasoning"] = {"max_tokens": num_tokens}
                    if "reasoning" in self.extra_params["extra_body"]:
                        del self.extra_params["extra_body"]["reasoning"]
                    self.extra_params["thinking"] = {"type": "enabled", "budget_tokens": num_tokens}
                    if "thinking" in self.extra_params:
                        del self.extra_params["thinking"]
    def get_raw_thinking_tokens(self):
        """Get formatted thinking token budget if available"""
        budget = None
        if self.extra_params:
            # Check for OpenRouter reasoning format
                    "extra_body" in self.extra_params
                    and "reasoning" in self.extra_params["extra_body"]
                    and "max_tokens" in self.extra_params["extra_body"]["reasoning"]
                    budget = self.extra_params["extra_body"]["reasoning"]["max_tokens"]
            # Check for standard thinking format
                "thinking" in self.extra_params and "budget_tokens" in self.extra_params["thinking"]
                budget = self.extra_params["thinking"]["budget_tokens"]
        return budget
    def get_thinking_tokens(self):
        budget = self.get_raw_thinking_tokens()
        if budget is not None:
            # Format as xx.yK for thousands, xx.yM for millions
            if budget >= 1024 * 1024:
                value = budget / (1024 * 1024)
                if value == int(value):
                    return f"{int(value)}M"
                    return f"{value:.1f}M"
                value = budget / 1024
                    return f"{int(value)}k"
                    return f"{value:.1f}k"
    def get_reasoning_effort(self):
        """Get reasoning effort value if available"""
                    and "effort" in self.extra_params["extra_body"]["reasoning"]
                    return self.extra_params["extra_body"]["reasoning"]["effort"]
            # Check for standard reasoning_effort format (e.g. in extra_body)
                and "reasoning_effort" in self.extra_params["extra_body"]
                return self.extra_params["extra_body"]["reasoning_effort"]
    def is_deepseek_r1(self):
        name = self.name.lower()
        if "deepseek" not in name:
        return "r1" in name or "reasoner" in name
    def is_ollama(self):
        return self.name.startswith("ollama/") or self.name.startswith("ollama_chat/")
    def github_copilot_token_to_open_ai_key(self, extra_headers):
        # check to see if there's an openai api key
        # If so, check to see if it's expire
        openai_api_key = "OPENAI_API_KEY"
        if openai_api_key not in os.environ or (
            int(dict(x.split("=") for x in os.environ[openai_api_key].split(";"))["exp"])
            < int(datetime.now().timestamp())
            class GitHubCopilotTokenError(Exception):
                """Custom exception for GitHub Copilot token-related errors."""
            # Validate GitHub Copilot token exists
            if "GITHUB_COPILOT_TOKEN" not in os.environ:
                raise KeyError("GITHUB_COPILOT_TOKEN environment variable not found")
            github_token = os.environ["GITHUB_COPILOT_TOKEN"]
            if not github_token.strip():
                raise KeyError("GITHUB_COPILOT_TOKEN environment variable is empty")
                "Authorization": f"Bearer {os.environ['GITHUB_COPILOT_TOKEN']}",
                "Editor-Version": extra_headers["Editor-Version"],
                "Copilot-Integration-Id": extra_headers["Copilot-Integration-Id"],
            url = "https://api.github.com/copilot_internal/v2/token"
            res = requests.get(url, headers=headers)
            if res.status_code != 200:
                safe_headers = {k: v for k, v in headers.items() if k != "Authorization"}
                token_preview = github_token[:5] + "..." if len(github_token) >= 5 else github_token
                safe_headers["Authorization"] = f"Bearer {token_preview}"
                raise GitHubCopilotTokenError(
                    f"GitHub Copilot API request failed (Status: {res.status_code})\n"
                    f"URL: {url}\n"
                    f"Headers: {json.dumps(safe_headers, indent=2)}\n"
                    f"JSON: {res.text}"
            response_data = res.json()
            token = response_data.get("token")
                raise GitHubCopilotTokenError("Response missing 'token' field")
            os.environ[openai_api_key] = token
    def send_completion(self, messages, functions, stream, temperature=None):
        if os.environ.get("AIDER_SANITY_CHECK_TURNS"):
            sanity_check_messages(messages)
        if self.is_deepseek_r1():
            messages = ensure_alternating_roles(messages)
        kwargs = dict(
            model=self.name,
        if self.use_temperature is not False:
            if temperature is None:
                if isinstance(self.use_temperature, bool):
                    temperature = 0
                    temperature = float(self.use_temperature)
            kwargs["temperature"] = temperature
        if functions is not None:
            function = functions[0]
            kwargs["tools"] = [dict(type="function", function=function)]
            kwargs["tool_choice"] = {"type": "function", "function": {"name": function["name"]}}
            kwargs.update(self.extra_params)
        if self.is_ollama() and "num_ctx" not in kwargs:
            num_ctx = int(self.token_count(messages) * 1.25) + 8192
            kwargs["num_ctx"] = num_ctx
        key = json.dumps(kwargs, sort_keys=True).encode()
        # dump(kwargs)
        hash_object = hashlib.sha1(key)
        if "timeout" not in kwargs:
            kwargs["timeout"] = request_timeout
            dump(kwargs)
        kwargs["messages"] = messages
        # Are we using github copilot?
        if "GITHUB_COPILOT_TOKEN" in os.environ:
            if "extra_headers" not in kwargs:
                kwargs["extra_headers"] = {
                    "Editor-Version": f"aider/{__version__}",
                    "Copilot-Integration-Id": "vscode-chat",
            self.github_copilot_token_to_open_ai_key(kwargs["extra_headers"])
        res = litellm.completion(**kwargs)
        return hash_object, res
    def simple_send_with_retries(self, messages):
        from aider.exceptions import LiteLLMExceptions
        litellm_ex = LiteLLMExceptions()
        if "deepseek-reasoner" in self.name:
        retry_delay = 0.125
            dump(messages)
                    "functions": None,
                _hash, response = self.send_completion(**kwargs)
                if not response or not hasattr(response, "choices") or not response.choices:
                res = response.choices[0].message.content
                from aider.reasoning_tags import remove_reasoning_content
                return remove_reasoning_content(res, self.reasoning_tag)
            except litellm_ex.exceptions_tuple() as err:
                ex_info = litellm_ex.get_ex_info(err)
                print(str(err))
                if ex_info.description:
                    print(ex_info.description)
                should_retry = ex_info.retry
                if should_retry:
                    retry_delay *= 2
                    if retry_delay > RETRY_TIMEOUT:
                        should_retry = False
                if not should_retry:
                print(f"Retrying in {retry_delay:.1f} seconds...")
def register_models(model_settings_fnames):
    files_loaded = []
    for model_settings_fname in model_settings_fnames:
        if not os.path.exists(model_settings_fname):
        if not Path(model_settings_fname).read_text().strip():
            with open(model_settings_fname, "r") as model_settings_file:
                model_settings_list = yaml.safe_load(model_settings_file)
                model_settings = ModelSettings(**model_settings_dict)
                # Remove all existing settings for this model name
                MODEL_SETTINGS[:] = [ms for ms in MODEL_SETTINGS if ms.name != model_settings.name]
                # Add the new settings
                MODEL_SETTINGS.append(model_settings)
            raise Exception(f"Error loading model settings from {model_settings_fname}: {e}")
        files_loaded.append(model_settings_fname)
    return files_loaded
def register_litellm_models(model_fnames):
    for model_fname in model_fnames:
        if not os.path.exists(model_fname):
            data = Path(model_fname).read_text()
            if not data.strip():
            model_def = json5.loads(data)
            if not model_def:
            # Defer registration with litellm to faster path.
            model_info_manager.local_model_metadata.update(model_def)
            raise Exception(f"Error loading model definition from {model_fname}: {e}")
        files_loaded.append(model_fname)
def validate_variables(vars):
    for var in vars:
        if var not in os.environ:
            missing.append(var)
        return dict(keys_in_environment=False, missing_keys=missing)
    return dict(keys_in_environment=True, missing_keys=missing)
def sanity_check_models(io, main_model):
    problem_main = sanity_check_model(io, main_model)
    problem_weak = None
    if main_model.weak_model and main_model.weak_model is not main_model:
        problem_weak = sanity_check_model(io, main_model.weak_model)
    problem_editor = None
        main_model.editor_model
        and main_model.editor_model is not main_model
        and main_model.editor_model is not main_model.weak_model
        problem_editor = sanity_check_model(io, main_model.editor_model)
    return problem_main or problem_weak or problem_editor
def sanity_check_model(io, model):
    show = False
    if model.missing_keys:
        show = True
        io.tool_warning(f"Warning: {model} expects these environment variables")
        for key in model.missing_keys:
            value = os.environ.get(key, "")
            status = "Set" if value else "Not set"
            io.tool_output(f"- {key}: {status}")
                "Note: You may need to restart your terminal or command prompt for `setx` to take"
                " effect."
    elif not model.keys_in_environment:
        io.tool_warning(f"Warning for {model}: Unknown which environment variables are required.")
    # Check for model-specific dependencies
    check_for_dependencies(io, model.name)
    if not model.info:
            f"Warning for {model}: Unknown context window size and costs, using sane defaults."
        possible_matches = fuzzy_match_models(model.name)
            io.tool_output("Did you mean one of these?")
            for match in possible_matches:
                io.tool_output(f"- {match}")
    return show
def check_for_dependencies(io, model_name):
    Check for model-specific dependencies and install them if needed.
        io: The IO object for user interaction
        model_name: The name of the model to check dependencies for
    # Check if this is a Bedrock model and ensure boto3 is installed
    if model_name.startswith("bedrock/"):
        check_pip_install_extra(
            io, "boto3", "AWS Bedrock models require the boto3 package.", ["boto3"]
    # Check if this is a Vertex AI model and ensure google-cloud-aiplatform is installed
    elif model_name.startswith("vertex_ai/"):
            "google.cloud.aiplatform",
            "Google Vertex AI models require the google-cloud-aiplatform package.",
            ["google-cloud-aiplatform"],
def fuzzy_match_models(name):
    chat_models = set()
    model_metadata = list(litellm.model_cost.items())
    model_metadata += list(model_info_manager.local_model_metadata.items())
    for orig_model, attrs in model_metadata:
        model = orig_model.lower()
        if attrs.get("mode") != "chat":
        provider = attrs.get("litellm_provider", "").lower()
        provider += "/"
        if model.startswith(provider):
            fq_model = orig_model
            fq_model = provider + orig_model
        chat_models.add(fq_model)
        chat_models.add(orig_model)
    chat_models = sorted(chat_models)
    # exactly matching model
    # matching_models = [
    #    (fq,m) for fq,m in chat_models
    #    if name == fq or name == m
    # if matching_models:
    #    return matching_models
    # Check for model names containing the name
    matching_models = [m for m in chat_models if name in m]
    if matching_models:
        return sorted(set(matching_models))
    # Check for slight misspellings
    models = set(chat_models)
    matching_models = difflib.get_close_matches(name, models, n=3, cutoff=0.8)
def print_matching_models(io, search):
    matches = fuzzy_match_models(search)
        io.tool_output(f'Models which match "{search}":')
        for model in matches:
            io.tool_output(f"- {model}")
        io.tool_output(f'No models match "{search}".')
def get_model_settings_as_yaml():
    model_settings_list = []
    # Add default settings first with all field values
    defaults = {}
        defaults[field.name] = field.default
    defaults["name"] = "(default values)"
    model_settings_list.append(defaults)
    # Sort model settings by name
    for ms in sorted(MODEL_SETTINGS, key=lambda x: x.name):
        # Create dict with explicit field order
        model_settings_dict = {}
            value = getattr(ms, field.name)
            if value != field.default:
                model_settings_dict[field.name] = value
        model_settings_list.append(model_settings_dict)
        # Add blank line between entries
        model_settings_list.append(None)
    # Filter out None values before dumping
    yaml_str = yaml.dump(
        [ms for ms in model_settings_list if ms is not None],
        default_flow_style=False,
        sort_keys=False,  # Preserve field order from dataclass
    # Add actual blank lines between entries
    return yaml_str.replace("\n- ", "\n\n- ")
    if len(sys.argv) < 2:
        print("Usage: python models.py <model_name> or python models.py --yaml")
    if sys.argv[1] == "--yaml":
        yaml_string = get_model_settings_as_yaml()
        print(yaml_string)
        model_name = sys.argv[1]
        matching_models = fuzzy_match_models(model_name)
            print(f"Matching models for '{model_name}':")
            for model in matching_models:
            print(f"No matching models found for '{model_name}'.")
