import weakref
    IO,
    Tuple,
    AsyncIterable,
from typing_extensions import (
    Unpack,
    Literal,
    ClassVar,
    Protocol,
    Required,
    ParamSpec,
    TypedDict,
    TypeGuard,
    final,
    runtime_checkable,
    IncEx,
    ModelT,
    PropertyInfo,
    is_list,
    json_safe,
    lru_cache,
    parse_date,
    coerce_boolean,
    parse_datetime,
    strip_not_given,
    extract_type_arg,
    is_annotated_type,
    is_type_alias_type,
    strip_annotated_type,
from ._compat import (
    PYDANTIC_V1,
    ConfigDict,
    GenericModel as BaseGenericModel,
    get_args,
    is_union,
    parse_obj,
    get_origin,
    is_literal_type,
    get_model_config,
    get_model_fields,
    field_get_default,
    from pydantic_core.core_schema import ModelField, ModelSchema, LiteralSchema, ModelFieldsSchema
__all__ = ["BaseModel", "GenericModel"]
_BaseModelT = TypeVar("_BaseModelT", bound="BaseModel")
ReprArgs = Sequence[Tuple[Optional[str], Any]]
@runtime_checkable
class _ConfigProtocol(Protocol):
    allow_population_by_field_name: bool
class BaseModel(pydantic.BaseModel):
        def model_fields_set(self) -> set[str]:
            # a forwards-compat shim for pydantic v2
            return self.__fields_set__  # type: ignore
        class Config(pydantic.BaseConfig):  # pyright: ignore[reportDeprecated]
            extra: Any = pydantic.Extra.allow  # type: ignore
        def __repr_args__(self) -> ReprArgs:
            # we don't want these attributes to be included when something like `rich.print` is used
            return [arg for arg in super().__repr_args__() if arg[0] not in {"_request_id", "__exclude_fields__"}]
        model_config: ClassVar[ConfigDict] = ConfigDict(
            extra="allow", defer_build=coerce_boolean(os.environ.get("DEFER_PYDANTIC_BUILD", "true"))
        _request_id: Optional[str] = None
        """The ID of the request, returned via the X-Request-ID header. Useful for debugging requests and reporting issues to OpenAI.
        This will **only** be set for the top-level response object, it will not be defined for nested objects. For example:
        completion = await client.chat.completions.create(...)
        completion._request_id  # req_id_xxx
        completion.usage._request_id  # raises `AttributeError`
        Note: unlike other properties that use an `_` prefix, this property
        *is* public. Unless documented otherwise, all other `_` prefix properties,
        methods and modules are *private*.
    def to_dict(
        use_api_names: bool = True,
        exclude_unset: bool = True,
        exclude_none: bool = False,
    ) -> dict[str, object]:
        """Recursively generate a dictionary representation of the model, optionally specifying which fields to include or exclude.
        By default, fields that were not set by the API will not be included,
        and keys will match the API response, *not* the property names from the model.
        For example, if the API responds with `"fooBar": true` but we've defined a `foo_bar: bool` property,
        the output will use the `"fooBar"` key (unless `use_api_names=False` is passed).
            mode:
                If mode is 'json', the dictionary will only contain JSON serializable types. e.g. `datetime` will be turned into a string, `"2024-3-22T18:11:19.117000Z"`.
                If mode is 'python', the dictionary may contain any Python objects. e.g. `datetime(2024, 3, 22)`
            use_api_names: Whether to use the key that the API responded with or the property name. Defaults to `True`.
            exclude_unset: Whether to exclude fields that have not been explicitly set.
            exclude_defaults: Whether to exclude fields that are set to their default value from the output.
            exclude_none: Whether to exclude fields that have a value of `None` from the output.
            warnings: Whether to log warnings when invalid fields are encountered. This is only supported in Pydantic v2.
        return self.model_dump(
            by_alias=use_api_names,
            exclude_none=exclude_none,
            warnings=warnings,
    def to_json(
        indent: int | None = 2,
    ) -> str:
        """Generates a JSON string representing this model as it would be received from or sent to the API (but with indentation).
            indent: Indentation to use in the JSON output. If `None` is passed, the output will be compact. Defaults to `2`
            exclude_defaults: Whether to exclude fields that have the default value.
            exclude_none: Whether to exclude fields that have a value of `None`.
            warnings: Whether to show any warnings that occurred during serialization. This is only supported in Pydantic v2.
        return self.model_dump_json(
            indent=indent,
        # mypy complains about an invalid self arg
        return f"{self.__repr_name__()}({self.__repr_str__(', ')})"  # type: ignore[misc]
    # Override the 'construct' method in a way that supports recursive parsing without validation.
    # Based on https://github.com/samuelcolvin/pydantic/issues/1168#issuecomment-817742836.
    def construct(  # pyright: ignore[reportIncompatibleMethodOverride]
        __cls: Type[ModelT],
        _fields_set: set[str] | None = None,
        **values: object,
    ) -> ModelT:
        m = __cls.__new__(__cls)
        fields_values: dict[str, object] = {}
        config = get_model_config(__cls)
        populate_by_name = (
            config.allow_population_by_field_name
            if isinstance(config, _ConfigProtocol)
            else config.get("populate_by_name")
        if _fields_set is None:
            _fields_set = set()
        model_fields = get_model_fields(__cls)
        for name, field in model_fields.items():
            key = field.alias
            if key is None or (key not in values and populate_by_name):
                key = name
            if key in values:
                fields_values[name] = _construct_field(value=values[key], field=field, key=key)
                _fields_set.add(name)
                fields_values[name] = field_get_default(field)
        extra_field_type = _get_extra_fields_type(__cls)
        _extra = {}
        for key, value in values.items():
            if key not in model_fields:
                parsed = construct_type(value=value, type_=extra_field_type) if extra_field_type is not None else value
                    _fields_set.add(key)
                    fields_values[key] = parsed
                    _extra[key] = parsed
        object.__setattr__(m, "__dict__", fields_values)
            # init_private_attributes() does not exist in v2
            m._init_private_attributes()  # type: ignore
            # copied from Pydantic v1's `construct()` method
            object.__setattr__(m, "__fields_set__", _fields_set)
            # these properties are copied from Pydantic's `model_construct()` method
            object.__setattr__(m, "__pydantic_private__", None)
            object.__setattr__(m, "__pydantic_extra__", _extra)
            object.__setattr__(m, "__pydantic_fields_set__", _fields_set)
        return m
    if not TYPE_CHECKING:
        # type checkers incorrectly complain about this assignment
        # because the type signatures are technically different
        # although not in practice
        model_construct = construct
        # we define aliases for some of the new pydantic v2 methods so
        # that we can just document these methods without having to specify
        # a specific pydantic version as some users may not know which
        # pydantic version they are currently using
            mode: Literal["json", "python"] | str = "python",
            include: IncEx | None = None,
            context: Any | None = None,
            exclude_computed_fields: bool = False,
            round_trip: bool = False,
            warnings: bool | Literal["none", "warn", "error"] = True,
            fallback: Callable[[Any], Any] | None = None,
            serialize_as_any: bool = False,
            """Usage docs: https://docs.pydantic.dev/2.4/concepts/serialization/#modelmodel_dump
            Generate a dictionary representation of the model, optionally specifying which fields to include or exclude.
                mode: The mode in which `to_python` should run.
                    If mode is 'json', the output will only contain JSON serializable types.
                    If mode is 'python', the output may contain non-JSON-serializable Python objects.
                include: A set of fields to include in the output.
                exclude: A set of fields to exclude from the output.
                context: Additional context to pass to the serializer.
                by_alias: Whether to use the field's alias in the dictionary key if defined.
                exclude_defaults: Whether to exclude fields that are set to their default value.
                exclude_computed_fields: Whether to exclude computed fields.
                    While this can be useful for round-tripping, it is usually recommended to use the dedicated
                    `round_trip` parameter instead.
                round_trip: If True, dumped values should be valid as input for non-idempotent types such as Json[T].
                warnings: How to handle serialization errors. False/"none" ignores them, True/"warn" logs errors,
                    "error" raises a [`PydanticSerializationError`][pydantic_core.PydanticSerializationError].
                fallback: A function to call when an unknown value is encountered. If not provided,
                    a [`PydanticSerializationError`][pydantic_core.PydanticSerializationError] error is raised.
                serialize_as_any: Whether to serialize fields with duck-typing serialization behavior.
                A dictionary representation of the model.
            if mode not in {"json", "python"}:
                raise ValueError("mode must be either 'json' or 'python'")
            if round_trip != False:
                raise ValueError("round_trip is only supported in Pydantic v2")
            if warnings != True:
                raise ValueError("warnings is only supported in Pydantic v2")
            if context is not None:
                raise ValueError("context is only supported in Pydantic v2")
            if serialize_as_any != False:
                raise ValueError("serialize_as_any is only supported in Pydantic v2")
            if fallback is not None:
                raise ValueError("fallback is only supported in Pydantic v2")
            if exclude_computed_fields != False:
                raise ValueError("exclude_computed_fields is only supported in Pydantic v2")
            dumped = super().dict(  # pyright: ignore[reportDeprecated]
                include=include,
                by_alias=by_alias if by_alias is not None else False,
            return cast("dict[str, Any]", json_safe(dumped)) if mode == "json" else dumped
        def model_dump_json(
            indent: int | None = None,
            ensure_ascii: bool = False,
            """Usage docs: https://docs.pydantic.dev/2.4/concepts/serialization/#modelmodel_dump_json
            Generates a JSON representation of the model using Pydantic's `to_json` method.
                indent: Indentation to use in the JSON output. If None is passed, the output will be compact.
                include: Field(s) to include in the JSON output. Can take either a string or set of strings.
                exclude: Field(s) to exclude from the JSON output. Can take either a string or set of strings.
                by_alias: Whether to serialize using field aliases.
                round_trip: Whether to use serialization/deserialization between JSON and class instance.
                warnings: Whether to show any warnings that occurred during serialization.
                A JSON string representation of the model.
            if ensure_ascii != False:
                raise ValueError("ensure_ascii is only supported in Pydantic v2")
            return super().json(  # type: ignore[reportDeprecated]
def _construct_field(value: object, field: FieldInfo, key: str) -> object:
        return field_get_default(field)
        type_ = cast(type, field.outer_type_)  # type: ignore
        type_ = field.annotation  # type: ignore
    if type_ is None:
        raise RuntimeError(f"Unexpected field type is None for {key}")
    return construct_type(value=value, type_=type_, metadata=getattr(field, "metadata", None))
def _get_extra_fields_type(cls: type[pydantic.BaseModel]) -> type | None:
        # TODO
    schema = cls.__pydantic_core_schema__
    if schema["type"] == "model":
        fields = schema["schema"]
        if fields["type"] == "model-fields":
            extras = fields.get("extras_schema")
            if extras and "cls" in extras:
                # mypy can't narrow the type
                return extras["cls"]  # type: ignore[no-any-return]
def is_basemodel(type_: type) -> bool:
    """Returns whether or not the given type is either a `BaseModel` or a union of `BaseModel`"""
    if is_union(type_):
        for variant in get_args(type_):
            if is_basemodel(variant):
    return is_basemodel_type(type_)
def is_basemodel_type(type_: type) -> TypeGuard[type[BaseModel] | type[GenericModel]]:
    origin = get_origin(type_) or type_
    if not inspect.isclass(origin):
    return issubclass(origin, BaseModel) or issubclass(origin, GenericModel)
def build(
    base_model_cls: Callable[P, _BaseModelT],
    *args: P.args,
    **kwargs: P.kwargs,
) -> _BaseModelT:
    """Construct a BaseModel class without validation.
    This is useful for cases where you need to instantiate a `BaseModel`
    from an API response as this provides type-safe params which isn't supported
    by helpers like `construct_type()`.
    build(MyModel, my_field_a="foo", my_field_b=123)
    if args:
            "Received positional arguments which are not supported; Keyword arguments must be used instead",
    return cast(_BaseModelT, construct_type(type_=base_model_cls, value=kwargs))
def construct_type_unchecked(*, value: object, type_: type[_T]) -> _T:
    """Loose coercion to the expected type with construction of nested values.
    Note: the returned value from this function is not guaranteed to match the
    given type.
    return cast(_T, construct_type(value=value, type_=type_))
def construct_type(*, value: object, type_: object, metadata: Optional[List[Any]] = None) -> object:
    If the given value does not match the expected type then it is returned as-is.
    # store a reference to the original type we were given before we extract any inner
    # types so that we can properly resolve forward references in `TypeAliasType` annotations
    original_type = None
    # we allow `object` as the input type because otherwise, passing things like
    # `Literal['value']` will be reported as a type error by type checkers
    type_ = cast("type[object]", type_)
    if is_type_alias_type(type_):
        original_type = type_  # type: ignore[unreachable]
        type_ = type_.__value__  # type: ignore[unreachable]
    if metadata is not None and len(metadata) > 0:
        meta: tuple[Any, ...] = tuple(metadata)
    elif is_annotated_type(type_):
        meta = get_args(type_)[1:]
        type_ = extract_type_arg(type_, 0)
        meta = tuple()
    # we need to use the origin class for any types that are subscripted generics
    # e.g. Dict[str, object]
    args = get_args(type_)
    if is_union(origin):
            return validate_type(type_=cast("type[object]", original_type or type_), value=value)
        # if the type is a discriminated union then we want to construct the right variant
        # in the union, even if the data doesn't match exactly, otherwise we'd break code
        # that relies on the constructed class types, e.g.
        # class FooType:
        #   kind: Literal['foo']
        #   value: str
        # class BarType:
        #   kind: Literal['bar']
        #   value: int
        # without this block, if the data we get is something like `{'kind': 'bar', 'value': 'foo'}` then
        # we'd end up constructing `FooType` when it should be `BarType`.
        discriminator = _build_discriminated_union_meta(union=type_, meta_annotations=meta)
        if discriminator and is_mapping(value):
            variant_value = value.get(discriminator.field_alias_from or discriminator.field_name)
            if variant_value and isinstance(variant_value, str):
                variant_type = discriminator.mapping.get(variant_value)
                if variant_type:
                    return construct_type(type_=variant_type, value=value)
        # if the data is not valid, use the first variant that doesn't fail while deserializing
        for variant in args:
                return construct_type(value=value, type_=variant)
        raise RuntimeError(f"Could not convert data into a valid instance of {type_}")
    if origin == dict:
        if not is_mapping(value):
        _, items_type = get_args(type_)  # Dict[_, items_type]
        return {key: construct_type(value=item, type_=items_type) for key, item in value.items()}
        not is_literal_type(type_)
        and inspect.isclass(origin)
        and (issubclass(origin, BaseModel) or issubclass(origin, GenericModel))
        if is_list(value):
            return [cast(Any, type_).construct(**entry) if is_mapping(entry) else entry for entry in value]
        if is_mapping(value):
            if issubclass(type_, BaseModel):
                return type_.construct(**value)  # type: ignore[arg-type]
            return cast(Any, type_).construct(**value)
    if origin == list:
        if not is_list(value):
        inner_type = args[0]  # List[inner_type]
        return [construct_type(value=entry, type_=inner_type) for entry in value]
    if origin == float:
        if isinstance(value, int):
            coerced = float(value)
            if coerced != value:
            return coerced
    if type_ == datetime:
            return parse_datetime(value)  # type: ignore
    if type_ == date:
            return parse_date(value)  # type: ignore
class CachedDiscriminatorType(Protocol):
    __discriminator__: DiscriminatorDetails
DISCRIMINATOR_CACHE: weakref.WeakKeyDictionary[type, DiscriminatorDetails] = weakref.WeakKeyDictionary()
class DiscriminatorDetails:
    field_name: str
    """The name of the discriminator field in the variant class, e.g.
    class Foo(BaseModel):
        type: Literal['foo']
    Will result in field_name='type'
    field_alias_from: str | None
    """The name of the discriminator field in the API response, e.g.
        type: Literal['foo'] = Field(alias='type_from_api')
    Will result in field_alias_from='type_from_api'
    mapping: dict[str, type]
    """Mapping of discriminator value to variant type, e.g.
    {'foo': FooVariant, 'bar': BarVariant}
        mapping: dict[str, type],
        discriminator_field: str,
        discriminator_alias: str | None,
        self.mapping = mapping
        self.field_name = discriminator_field
        self.field_alias_from = discriminator_alias
def _build_discriminated_union_meta(*, union: type, meta_annotations: tuple[Any, ...]) -> DiscriminatorDetails | None:
    cached = DISCRIMINATOR_CACHE.get(union)
        return cached
    discriminator_field_name: str | None = None
    for annotation in meta_annotations:
        if isinstance(annotation, PropertyInfo) and annotation.discriminator is not None:
            discriminator_field_name = annotation.discriminator
    if not discriminator_field_name:
    mapping: dict[str, type] = {}
    discriminator_alias: str | None = None
    for variant in get_args(union):
        variant = strip_annotated_type(variant)
        if is_basemodel_type(variant):
                field_info = cast("dict[str, FieldInfo]", variant.__fields__).get(discriminator_field_name)  # pyright: ignore[reportDeprecated, reportUnnecessaryCast]
                if not field_info:
                # Note: if one variant defines an alias then they all should
                discriminator_alias = field_info.alias
                if (annotation := getattr(field_info, "annotation", None)) and is_literal_type(annotation):
                    for entry in get_args(annotation):
                        if isinstance(entry, str):
                            mapping[entry] = variant
                field = _extract_field_schema_pv2(variant, discriminator_field_name)
                if not field:
                discriminator_alias = field.get("serialization_alias")
                field_schema = field["schema"]
                if field_schema["type"] == "literal":
                    for entry in cast("LiteralSchema", field_schema)["expected"]:
    if not mapping:
    details = DiscriminatorDetails(
        mapping=mapping,
        discriminator_field=discriminator_field_name,
        discriminator_alias=discriminator_alias,
    DISCRIMINATOR_CACHE.setdefault(union, details)
    return details
def _extract_field_schema_pv2(model: type[BaseModel], field_name: str) -> ModelField | None:
    schema = model.__pydantic_core_schema__
    if schema["type"] == "definitions":
        schema = schema["schema"]
    if schema["type"] != "model":
    schema = cast("ModelSchema", schema)
    fields_schema = schema["schema"]
    if fields_schema["type"] != "model-fields":
    fields_schema = cast("ModelFieldsSchema", fields_schema)
    field = fields_schema["fields"].get(field_name)
    return cast("ModelField", field)  # pyright: ignore[reportUnnecessaryCast]
def validate_type(*, type_: type[_T], value: object) -> _T:
    """Strict validation that the given value matches the expected type"""
    if inspect.isclass(type_) and issubclass(type_, pydantic.BaseModel):
        return cast(_T, parse_obj(type_, value))
    return cast(_T, _validate_non_model_type(type_=type_, value=value))
def set_pydantic_config(typ: Any, config: pydantic.ConfigDict) -> None:
    """Add a pydantic config for the given type.
    Note: this is a no-op on Pydantic v1.
    setattr(typ, "__pydantic_config__", config)  # noqa: B010
def add_request_id(obj: BaseModel, request_id: str | None) -> None:
    obj._request_id = request_id
    # in Pydantic v1, using setattr like we do above causes the attribute
    # to be included when serializing the model which we don't want in this
    # case so we need to explicitly exclude it
            exclude_fields = obj.__exclude_fields__  # type: ignore
            cast(Any, obj).__exclude_fields__ = {"_request_id", "__exclude_fields__"}
            cast(Any, obj).__exclude_fields__ = {*(exclude_fields or {}), "_request_id", "__exclude_fields__"}
# our use of subclassing here causes weirdness for type checkers,
# so we just pretend that we don't subclass
    GenericModel = BaseModel
    class GenericModel(BaseGenericModel, BaseModel):
if not PYDANTIC_V1:
    from pydantic import TypeAdapter as _TypeAdapter
    _CachedTypeAdapter = cast("TypeAdapter[object]", lru_cache(maxsize=None)(_TypeAdapter))
        from pydantic import TypeAdapter
        TypeAdapter = _CachedTypeAdapter
    def _validate_non_model_type(*, type_: type[_T], value: object) -> _T:
        return TypeAdapter(type_).validate_python(value)
elif not TYPE_CHECKING:  # TODO: condition is weird
    class RootModel(GenericModel, Generic[_T]):
        """Used as a placeholder to easily convert runtime types to a Pydantic format
        to provide validation.
        For example:
        validated = RootModel[int](__root__="5").__root__
        # validated: 5
        __root__: _T
        model = _create_pydantic_model(type_).validate(value)
        return cast(_T, model.__root__)
    def _create_pydantic_model(type_: _T) -> Type[RootModel[_T]]:
        return RootModel[type_]  # type: ignore
class FinalRequestOptionsInput(TypedDict, total=False):
    method: Required[str]
    url: Required[str]
    params: Query
    headers: Headers
    timeout: float | Timeout | None
    files: HttpxRequestFiles | None
    idempotency_key: str
    content: Union[bytes, bytearray, IO[bytes], Iterable[bytes], AsyncIterable[bytes], None]
    json_data: Body
    extra_json: AnyMapping
    follow_redirects: bool
    synthesize_event_and_data: bool
@final
class FinalRequestOptions(pydantic.BaseModel):
    method: str
    url: str
    params: Query = {}
    headers: Union[Headers, NotGiven] = NotGiven()
    max_retries: Union[int, NotGiven] = NotGiven()
    timeout: Union[float, Timeout, None, NotGiven] = NotGiven()
    files: Union[HttpxRequestFiles, None] = None
    idempotency_key: Union[str, None] = None
    post_parser: Union[Callable[[Any], Any], NotGiven] = NotGiven()
    follow_redirects: Union[bool, None] = None
    synthesize_event_and_data: Optional[bool] = None
    content: Union[bytes, bytearray, IO[bytes], Iterable[bytes], AsyncIterable[bytes], None] = None
    # It should be noted that we cannot use `json` here as that would override
    # a BaseModel method in an incompatible fashion.
    json_data: Union[Body, None] = None
    extra_json: Union[AnyMapping, None] = None
            arbitrary_types_allowed: bool = True
        model_config: ClassVar[ConfigDict] = ConfigDict(arbitrary_types_allowed=True)
    def get_max_retries(self, max_retries: int) -> int:
        if isinstance(self.max_retries, NotGiven):
        return self.max_retries
    def _strip_raw_response_header(self) -> None:
        if not is_given(self.headers):
        if self.headers.get(RAW_RESPONSE_HEADER):
            self.headers = {**self.headers}
            self.headers.pop(RAW_RESPONSE_HEADER)
    # override the `construct` method so that we can run custom transformations.
    # this is necessary as we don't want to do any actual runtime type checking
    # (which means we can't use validators) but we do want to ensure that `NotGiven`
    # values are not present
    # type ignore required because we're adding explicit types to `**values`
    def construct(  # type: ignore
        **values: Unpack[FinalRequestOptionsInput],
        kwargs: dict[str, Any] = {
            # we unconditionally call `strip_not_given` on any value
            # as it will just ignore any non-mapping types
            key: strip_not_given(value)
            for key, value in values.items()
            return cast(FinalRequestOptions, super().construct(_fields_set, **kwargs))  # pyright: ignore[reportDeprecated]
        return super().model_construct(_fields_set, **kwargs)
from .. import _models
from .._compat import PYDANTIC_V1, ConfigDict
class BaseModel(_models.BaseModel):
        model_config: ClassVar[ConfigDict] = ConfigDict(extra="ignore", arbitrary_types_allowed=True)
""" Collection of Model instances for use with the odrpack fitting package.
from scipy.odr._odrpack import Model
__all__ = ['Model', 'exponential', 'multilinear', 'unilinear', 'quadratic',
           'polynomial']
def _lin_fcn(B, x):
    a, b = B[0], B[1:]
    b.shape = (b.shape[0], 1)
    return a + (x*b).sum(axis=0)
def _lin_fjb(B, x):
    a = np.ones(x.shape[-1], float)
    res = np.concatenate((a, x.ravel()))
    res.shape = (B.shape[-1], x.shape[-1])
def _lin_fjd(B, x):
    b = B[1:]
    b = np.repeat(b, (x.shape[-1],)*b.shape[-1], axis=0)
    b.shape = x.shape
def _lin_est(data):
    # Eh. The answer is analytical, so just return all ones.
    # Don't return zeros since that will interfere with
    # ODRPACK's auto-scaling procedures.
    if len(data.x.shape) == 2:
        m = data.x.shape[0]
        m = 1
    return np.ones((m + 1,), float)
def _poly_fcn(B, x, powers):
    return a + np.sum(b * np.power(x, powers), axis=0)
def _poly_fjacb(B, x, powers):
    res = np.concatenate((np.ones(x.shape[-1], float),
                          np.power(x, powers).flat))
def _poly_fjacd(B, x, powers):
    b = b * powers
    return np.sum(b * np.power(x, powers-1), axis=0)
def _exp_fcn(B, x):
    return B[0] + np.exp(B[1] * x)
def _exp_fjd(B, x):
    return B[1] * np.exp(B[1] * x)
def _exp_fjb(B, x):
    res = np.concatenate((np.ones(x.shape[-1], float), x * np.exp(B[1] * x)))
    res.shape = (2, x.shape[-1])
def _exp_est(data):
    # Eh.
    return np.array([1., 1.])
class _MultilinearModel(Model):
    Arbitrary-dimensional linear model
    This model is defined by :math:`y=\beta_0 + \sum_{i=1}^m \beta_i x_i`
    We can calculate orthogonal distance regression with an arbitrary
    dimensional linear model:
    >>> from scipy import odr
    >>> x = np.linspace(0.0, 5.0)
    >>> y = 10.0 + 5.0 * x
    >>> data = odr.Data(x, y)
    >>> odr_obj = odr.ODR(data, odr.multilinear)
    >>> output = odr_obj.run()
    >>> print(output.beta)
    [10.  5.]
            _lin_fcn, fjacb=_lin_fjb, fjacd=_lin_fjd, estimate=_lin_est,
            meta={'name': 'Arbitrary-dimensional Linear',
                  'equ': 'y = B_0 + Sum[i=1..m, B_i * x_i]',
                  'TeXequ': r'$y=\beta_0 + \sum_{i=1}^m \beta_i x_i$'})
multilinear = _MultilinearModel()
def polynomial(order):
    Factory function for a general polynomial model.
    order : int or sequence
        If an integer, it becomes the order of the polynomial to fit. If
        a sequence of numbers, then these are the explicit powers in the
        polynomial.
        A constant term (power 0) is always included, so don't include 0.
        Thus, polynomial(n) is equivalent to polynomial(range(1, n+1)).
    polynomial : Model instance
        Model instance.
    We can fit an input data using orthogonal distance regression (ODR) with
    a polynomial model:
    >>> y = np.sin(x)
    >>> poly_model = odr.polynomial(3)  # using third order polynomial model
    >>> odr_obj = odr.ODR(data, poly_model)
    >>> output = odr_obj.run()  # running ODR fitting
    >>> poly = np.poly1d(output.beta[::-1])
    >>> poly_y = poly(x)
    >>> plt.plot(x, y, label="input data")
    >>> plt.plot(x, poly_y, label="polynomial ODR")
    >>> plt.legend()
    powers = np.asarray(order)
    if powers.shape == ():
        # Scalar.
        powers = np.arange(1, powers + 1)
    powers.shape = (len(powers), 1)
    len_beta = len(powers) + 1
    def _poly_est(data, len_beta=len_beta):
        # Eh. Ignore data and return all ones.
        return np.ones((len_beta,), float)
    return Model(_poly_fcn, fjacd=_poly_fjacd, fjacb=_poly_fjacb,
                 estimate=_poly_est, extra_args=(powers,),
                 meta={'name': 'Sorta-general Polynomial',
                 'equ': 'y = B_0 + Sum[i=1..%s, B_i * (x**i)]' % (len_beta-1),
                 'TeXequ': r'$y=\beta_0 + \sum_{i=1}^{%s} \beta_i x^i$' %
                        (len_beta-1)})
class _ExponentialModel(Model):
    Exponential model
    This model is defined by :math:`y=\beta_0 + e^{\beta_1 x}`
    We can calculate orthogonal distance regression with an exponential model:
    >>> y = -10.0 + np.exp(0.5*x)
    >>> odr_obj = odr.ODR(data, odr.exponential)
    [-10.    0.5]
        super().__init__(_exp_fcn, fjacd=_exp_fjd, fjacb=_exp_fjb,
                         estimate=_exp_est,
                         meta={'name': 'Exponential',
                               'equ': 'y= B_0 + exp(B_1 * x)',
                               'TeXequ': r'$y=\beta_0 + e^{\beta_1 x}$'})
exponential = _ExponentialModel()
def _unilin(B, x):
    return x*B[0] + B[1]
def _unilin_fjd(B, x):
    return np.ones(x.shape, float) * B[0]
def _unilin_fjb(B, x):
    _ret = np.concatenate((x, np.ones(x.shape, float)))
    _ret.shape = (2,) + x.shape
    return _ret
def _unilin_est(data):
    return (1., 1.)
def _quadratic(B, x):
    return x*(x*B[0] + B[1]) + B[2]
def _quad_fjd(B, x):
    return 2*x*B[0] + B[1]
def _quad_fjb(B, x):
    _ret = np.concatenate((x*x, x, np.ones(x.shape, float)))
    _ret.shape = (3,) + x.shape
def _quad_est(data):
    return (1.,1.,1.)
class _UnilinearModel(Model):
    Univariate linear model
    This model is defined by :math:`y = \beta_0 x + \beta_1`
    We can calculate orthogonal distance regression with an unilinear model:
    >>> y = 1.0 * x + 2.0
    >>> odr_obj = odr.ODR(data, odr.unilinear)
    [1. 2.]
        super().__init__(_unilin, fjacd=_unilin_fjd, fjacb=_unilin_fjb,
                         estimate=_unilin_est,
                         meta={'name': 'Univariate Linear',
                               'equ': 'y = B_0 * x + B_1',
                               'TeXequ': '$y = \\beta_0 x + \\beta_1$'})
unilinear = _UnilinearModel()
class _QuadraticModel(Model):
    Quadratic model
    This model is defined by :math:`y = \beta_0 x^2 + \beta_1 x + \beta_2`
    We can calculate orthogonal distance regression with a quadratic model:
    >>> y = 1.0 * x ** 2 + 2.0 * x + 3.0
    >>> odr_obj = odr.ODR(data, odr.quadratic)
    [1. 2. 3.]
            _quadratic, fjacd=_quad_fjd, fjacb=_quad_fjb, estimate=_quad_est,
            meta={'name': 'Quadratic',
                  'equ': 'y = B_0*x**2 + B_1*x + B_2',
                  'TeXequ': '$y = \\beta_0 x^2 + \\beta_1 x + \\beta_2'})
quadratic = _QuadraticModel()
# Functions for typechecking...
ByteOrStr = typing.Union[bytes, str]
HeadersAsSequence = typing.Sequence[typing.Tuple[ByteOrStr, ByteOrStr]]
HeadersAsMapping = typing.Mapping[ByteOrStr, ByteOrStr]
HeaderTypes = typing.Union[HeadersAsSequence, HeadersAsMapping, None]
Extensions = typing.MutableMapping[str, typing.Any]
def enforce_bytes(value: bytes | str, *, name: str) -> bytes:
    Any arguments that are ultimately represented as bytes can be specified
    either as bytes or as strings.
    However we enforce that any string arguments must only contain characters in
    the plain ASCII range. chr(0)...chr(127). If you need to use characters
    outside that range then be precise, and use a byte-wise argument.
            return value.encode("ascii")
            raise TypeError(f"{name} strings may not include unicode characters.")
    seen_type = type(value).__name__
    raise TypeError(f"{name} must be bytes or str, but got {seen_type}.")
def enforce_url(value: URL | bytes | str, *, name: str) -> URL:
    Type check for URL parameters.
    if isinstance(value, (bytes, str)):
        return URL(value)
    elif isinstance(value, URL):
    raise TypeError(f"{name} must be a URL, bytes, or str, but got {seen_type}.")
def enforce_headers(
    value: HeadersAsMapping | HeadersAsSequence | None = None, *, name: str
) -> list[tuple[bytes, bytes]]:
    Convienence function that ensure all items in request or response headers
    are either bytes or strings in the plain ASCII range.
    elif isinstance(value, typing.Mapping):
                enforce_bytes(k, name="header name"),
                enforce_bytes(v, name="header value"),
            for k, v in value.items()
    elif isinstance(value, typing.Sequence):
            for k, v in value
        f"{name} must be a mapping or sequence of two-tuples, but got {seen_type}."
def enforce_stream(
    value: bytes | typing.Iterable[bytes] | typing.AsyncIterable[bytes] | None,
) -> typing.Iterable[bytes] | typing.AsyncIterable[bytes]:
        return ByteStream(b"")
        return ByteStream(value)
# * https://tools.ietf.org/html/rfc3986#section-3.2.3
# * https://url.spec.whatwg.org/#url-miscellaneous
# * https://url.spec.whatwg.org/#scheme-state
DEFAULT_PORTS = {
    b"ftp": 21,
    b"http": 80,
    b"https": 443,
    b"ws": 80,
    b"wss": 443,
def include_request_headers(
    headers: list[tuple[bytes, bytes]],
    url: "URL",
    content: None | bytes | typing.Iterable[bytes] | typing.AsyncIterable[bytes],
    headers_set = set(k.lower() for k, v in headers)
    if b"host" not in headers_set:
        default_port = DEFAULT_PORTS.get(url.scheme)
        if url.port is None or url.port == default_port:
            header_value = url.host
            header_value = b"%b:%d" % (url.host, url.port)
        headers = [(b"Host", header_value)] + headers
        content is not None
        and b"content-length" not in headers_set
        and b"transfer-encoding" not in headers_set
            content_length = str(len(content)).encode("ascii")
            headers += [(b"Content-Length", content_length)]
            headers += [(b"Transfer-Encoding", b"chunked")]  # pragma: nocover
# Interfaces for byte streams...
class ByteStream:
    A container for non-streaming content, and that supports both sync and async
    stream iteration.
    def __init__(self, content: bytes) -> None:
        yield self._content
    async def __aiter__(self) -> typing.AsyncIterator[bytes]:
        return f"<{self.__class__.__name__} [{len(self._content)} bytes]>"
    def __init__(self, scheme: bytes, host: bytes, port: int) -> None:
        self.port = port
    def __eq__(self, other: typing.Any) -> bool:
            and self.scheme == other.scheme
            and self.host == other.host
            and self.port == other.port
        scheme = self.scheme.decode("ascii")
        host = self.host.decode("ascii")
        port = str(self.port)
        return f"{scheme}://{host}:{port}"
class URL:
    Represents the URL against which an HTTP request may be made.
    The URL may either be specified as a plain string, for convienence:
    url = httpcore.URL("https://www.example.com/")
    Or be constructed with explicitily pre-parsed components:
    url = httpcore.URL(scheme=b'https', host=b'www.example.com', port=None, target=b'/')
    Using this second more explicit style allows integrations that are using
    `httpcore` to pass through URLs that have already been parsed in order to use
    libraries such as `rfc-3986` rather than relying on the stdlib. It also ensures
    that URL parsing is treated identically at both the networking level and at any
    higher layers of abstraction.
    The four components are important here, as they allow the URL to be precisely
    specified in a pre-parsed format. They also allow certain types of request to
    be created that could not otherwise be expressed.
    For example, an HTTP request to `http://www.example.com/` forwarded via a proxy
    at `http://localhost:8080`...
    # Constructs an HTTP request with a complete URL as the target:
    # GET https://www.example.com/ HTTP/1.1
    url = httpcore.URL(
        scheme=b'http',
        host=b'localhost',
        port=8080,
        target=b'https://www.example.com/'
    request = httpcore.Request(
        url=url
    Another example is constructing an `OPTIONS *` request...
    # Constructs an 'OPTIONS *' HTTP request:
    # OPTIONS * HTTP/1.1
    url = httpcore.URL(scheme=b'https', host=b'www.example.com', target=b'*')
    request = httpcore.Request(method="OPTIONS", url=url)
    This kind of request is not possible to formulate with a URL string,
    because the `/` delimiter is always used to demark the target from the
    host/port portion of the URL.
    For convenience, string-like arguments may be specified either as strings or
    as bytes. However, once a request is being issue over-the-wire, the URL
    components are always ultimately required to be a bytewise representation.
    In order to avoid any ambiguity over character encodings, when strings are used
    as arguments, they must be strictly limited to the ASCII range `chr(0)`-`chr(127)`.
    If you require a bytewise representation that is outside this range you must
    handle the character encoding directly, and pass a bytes instance.
        url: bytes | str = "",
        scheme: bytes | str = b"",
        host: bytes | str = b"",
        port: int | None = None,
        target: bytes | str = b"",
            url: The complete URL as a string or bytes.
            scheme: The URL scheme as a string or bytes.
                Typically either `"http"` or `"https"`.
            host: The URL host as a string or bytes. Such as `"www.example.com"`.
            port: The port to connect to. Either an integer or `None`.
            target: The target of the HTTP request. Such as `"/items?search=red"`.
            parsed = urllib.parse.urlparse(enforce_bytes(url, name="url"))
            self.scheme = parsed.scheme
            self.host = parsed.hostname or b""
            self.port = parsed.port
            self.target = (parsed.path or b"/") + (
                b"?" + parsed.query if parsed.query else b""
            self.scheme = enforce_bytes(scheme, name="scheme")
            self.host = enforce_bytes(host, name="host")
            self.target = enforce_bytes(target, name="target")
    def origin(self) -> Origin:
        default_port = {
            b"socks5": 1080,
            b"socks5h": 1080,
        }[self.scheme]
        return Origin(
            scheme=self.scheme, host=self.host, port=self.port or default_port
            isinstance(other, URL)
            and other.scheme == self.scheme
            and other.host == self.host
            and other.port == self.port
            and other.target == self.target
    def __bytes__(self) -> bytes:
        if self.port is None:
            return b"%b://%b%b" % (self.scheme, self.host, self.target)
        return b"%b://%b:%d%b" % (self.scheme, self.host, self.port, self.target)
            f"{self.__class__.__name__}(scheme={self.scheme!r}, "
            f"host={self.host!r}, port={self.port!r}, target={self.target!r})"
    An HTTP request.
        method: bytes | str,
        url: URL | bytes | str,
        headers: HeaderTypes = None,
        content: bytes
        | typing.Iterable[bytes]
        | typing.AsyncIterable[bytes]
        extensions: Extensions | None = None,
            method: The HTTP request method, either as a string or bytes.
                For example: `GET`.
            url: The request URL, either as a `URL` instance, or as a string or bytes.
                For example: `"https://www.example.com".`
            headers: The HTTP request headers.
            content: The content of the request body.
            extensions: A dictionary of optional extra information included on
                the request. Possible keys include `"timeout"`, and `"trace"`.
        self.method: bytes = enforce_bytes(method, name="method")
        self.url: URL = enforce_url(url, name="url")
        self.headers: list[tuple[bytes, bytes]] = enforce_headers(
            headers, name="headers"
        self.stream: typing.Iterable[bytes] | typing.AsyncIterable[bytes] = (
            enforce_stream(content, name="content")
        self.extensions = {} if extensions is None else extensions
        if "target" in self.extensions:
            self.url = URL(
                scheme=self.url.scheme,
                host=self.url.host,
                port=self.url.port,
                target=self.extensions["target"],
        return f"<{self.__class__.__name__} [{self.method!r}]>"
    An HTTP response.
            status: The HTTP status code of the response. For example `200`.
            headers: The HTTP response headers.
            content: The content of the response body.
                the responseself.Possible keys include `"http_version"`,
                `"reason_phrase"`, and `"network_stream"`.
        self.status: int = status
        self._stream_consumed = False
        if not hasattr(self, "_content"):
            if isinstance(self.stream, typing.Iterable):
                    "Attempted to access 'response.content' on a streaming response. "
                    "Call 'response.read()' first."
                    "Call 'await response.aread()' first."
        return f"<{self.__class__.__name__} [{self.status}]>"
    # Sync interface...
        if not isinstance(self.stream, typing.Iterable):  # pragma: nocover
                "Attempted to read an asynchronous response using 'response.read()'. "
                "You should use 'await response.aread()' instead."
            self._content = b"".join([part for part in self.iter_stream()])
    def iter_stream(self) -> typing.Iterator[bytes]:
                "Attempted to stream an asynchronous response using 'for ... in "
                "response.iter_stream()'. "
                "You should use 'async for ... in response.aiter_stream()' instead."
        if self._stream_consumed:
                "Attempted to call 'for ... in response.iter_stream()' more than once."
        self._stream_consumed = True
        for chunk in self.stream:
                "Attempted to close an asynchronous response using 'response.close()'. "
                "You should use 'await response.aclose()' instead."
        if hasattr(self.stream, "close"):
    # Async interface...
        if not isinstance(self.stream, typing.AsyncIterable):  # pragma: nocover
                "Attempted to read an synchronous response using "
                "'await response.aread()'. "
                "You should use 'response.read()' instead."
            self._content = b"".join([part async for part in self.aiter_stream()])
    async def aiter_stream(self) -> typing.AsyncIterator[bytes]:
                "Attempted to stream an synchronous response using 'async for ... in "
                "response.aiter_stream()'. "
                "You should use 'for ... in response.iter_stream()' instead."
                "Attempted to call 'async for ... in response.aiter_stream()' "
                "more than once."
        async for chunk in self.stream:
                "Attempted to close a synchronous response using "
                "'await response.aclose()'. "
                "You should use 'response.close()' instead."
        if hasattr(self.stream, "aclose"):
            await self.stream.aclose()
class Proxy:
        auth: tuple[bytes | str, bytes | str] | None = None,
        headers: HeadersAsMapping | HeadersAsSequence | None = None,
        ssl_context: ssl.SSLContext | None = None,
        self.url = enforce_url(url, name="url")
        self.headers = enforce_headers(headers, name="headers")
        self.ssl_context = ssl_context
        if auth is not None:
            username = enforce_bytes(auth[0], name="auth")
            password = enforce_bytes(auth[1], name="auth")
            userpass = username + b":" + password
            authorization = b"Basic " + base64.b64encode(userpass)
            self.auth: tuple[bytes, bytes] | None = (username, password)
            self.headers = [(b"Proxy-Authorization", authorization)] + self.headers
import json as jsonlib
from http.cookiejar import Cookie, CookieJar
from ._content import ByteStream, UnattachedStream, encode_request, encode_response
from ._decoders import (
    SUPPORTED_DECODERS,
    ByteChunker,
    ContentDecoder,
    IdentityDecoder,
    LineDecoder,
    MultiDecoder,
    TextChunker,
    TextDecoder,
    CookieConflict,
    HTTPStatusError,
    RequestNotRead,
    ResponseNotRead,
    StreamClosed,
    StreamConsumed,
from ._multipart import get_multipart_boundary_from_content_type
    ResponseContent,
    ResponseExtensions,
from ._urls import URL
from ._utils import to_bytes_or_str, to_str
__all__ = ["Cookies", "Headers", "Request", "Response"]
SENSITIVE_HEADERS = {"authorization", "proxy-authorization"}
def _is_known_encoding(encoding: str) -> bool:
    Return `True` if `encoding` is a known codec.
        codecs.lookup(encoding)
def _normalize_header_key(key: str | bytes, encoding: str | None = None) -> bytes:
    Coerce str/bytes into a strictly byte-wise HTTP header key.
    return key if isinstance(key, bytes) else key.encode(encoding or "ascii")
def _normalize_header_value(value: str | bytes, encoding: str | None = None) -> bytes:
    Coerce str/bytes into a strictly byte-wise HTTP header value.
        raise TypeError(f"Header value must be str or bytes, not {type(value)}")
    return value.encode(encoding or "ascii")
def _parse_content_type_charset(content_type: str) -> str | None:
    # We used to use `cgi.parse_header()` here, but `cgi` became a dead battery.
    # See: https://peps.python.org/pep-0594/#cgi
    msg = email.message.Message()
    msg["content-type"] = content_type
    return msg.get_content_charset(failobj=None)
def _parse_header_links(value: str) -> list[dict[str, str]]:
    Returns a list of parsed link headers, for more info see:
    https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Link
    The generic syntax of those is:
    Link: < uri-reference >; param1=value1; param2="value2"
    So for instance:
    Link; '<http:/.../front.jpeg>; type="image/jpeg",<http://.../back.jpeg>;'
    would return
            {"url": "http:/.../front.jpeg", "type": "image/jpeg"},
            {"url": "http://.../back.jpeg"},
    :param value: HTTP Link entity-header field
    :return: list of parsed link headers
    links: list[dict[str, str]] = []
def _obfuscate_sensitive_headers(
    items: typing.Iterable[tuple[typing.AnyStr, typing.AnyStr]],
) -> typing.Iterator[tuple[typing.AnyStr, typing.AnyStr]]:
    for k, v in items:
        if to_str(k.lower()) in SENSITIVE_HEADERS:
            v = to_bytes_or_str("[secure]", match_type_of=v)
class Headers(typing.MutableMapping[str, str]):
    HTTP headers, as a case-insensitive multi-dict.
        self._list = []  # type: typing.List[typing.Tuple[bytes, bytes, bytes]]
            self._list = list(headers._list)
        elif isinstance(headers, Mapping):
                bytes_key = _normalize_header_key(k, encoding)
                bytes_value = _normalize_header_value(v, encoding)
                self._list.append((bytes_key, bytes_key.lower(), bytes_value))
        elif headers is not None:
            for k, v in headers:
        Header encoding is mandated as ascii, but we allow fallbacks to utf-8
        or iso-8859-1.
        if self._encoding is None:
            for encoding in ["ascii", "utf-8"]:
                for key, value in self.raw:
                        key.decode(encoding)
                        value.decode(encoding)
                    # The else block runs if 'break' did not occur, meaning
                    # all values fitted the encoding.
                # The ISO-8859-1 encoding covers all 256 code points in a byte,
                # so will never raise decode errors.
                self._encoding = "iso-8859-1"
    @encoding.setter
    def encoding(self, value: str) -> None:
        self._encoding = value
    def raw(self) -> list[tuple[bytes, bytes]]:
        Returns a list of the raw header items, as byte pairs.
        return [(raw_key, value) for raw_key, _, value in self._list]
    def keys(self) -> typing.KeysView[str]:
        return {key.decode(self.encoding): None for _, key, value in self._list}.keys()
    def values(self) -> typing.ValuesView[str]:
        values_dict: dict[str, str] = {}
        for _, key, value in self._list:
            str_key = key.decode(self.encoding)
            str_value = value.decode(self.encoding)
            if str_key in values_dict:
                values_dict[str_key] += f", {str_value}"
                values_dict[str_key] = str_value
        return values_dict.values()
    def items(self) -> typing.ItemsView[str, str]:
        Return `(key, value)` items of headers. Concatenate headers
        into a single comma separated value when a key occurs multiple times.
        return values_dict.items()
    def multi_items(self) -> list[tuple[str, str]]:
        Return a list of `(key, value)` pairs of headers. Allow multiple
        occurrences of the same key without concatenating into a single
        comma separated value.
            (key.decode(self.encoding), value.decode(self.encoding))
            for _, key, value in self._list
    def get(self, key: str, default: typing.Any = None) -> typing.Any:
        Return a header value. If multiple occurrences of the header occur
        then concatenate them together with commas.
    def get_list(self, key: str, split_commas: bool = False) -> list[str]:
        Return a list of all header values for a given key.
        If `split_commas=True` is passed, then any comma separated header
        values are split into multiple return strings.
        get_header_key = key.lower().encode(self.encoding)
            item_value.decode(self.encoding)
            for _, item_key, item_value in self._list
            if item_key.lower() == get_header_key
        if not split_commas:
        split_values = []
            split_values.extend([item.strip() for item in value.split(",")])
        return split_values
    def update(self, headers: HeaderTypes | None = None) -> None:  # type: ignore
        headers = Headers(headers)
        for key in headers.keys():
            if key in self:
        self._list.extend(headers._list)
    def copy(self) -> Headers:
        return Headers(self, encoding=self.encoding)
        Return a single header value.
        If there are multiple headers with the same key, then we concatenate
        them with commas. See: https://tools.ietf.org/html/rfc7230#section-3.2.2
        normalized_key = key.lower().encode(self.encoding)
            header_value.decode(self.encoding)
            for _, header_key, header_value in self._list
            if header_key == normalized_key
        if items:
            return ", ".join(items)
        Set the header `key` to `value`, removing any duplicate entries.
        Retains insertion order.
        set_key = key.encode(self._encoding or "utf-8")
        set_value = value.encode(self._encoding or "utf-8")
        lookup_key = set_key.lower()
        found_indexes = [
            for idx, (_, item_key, _) in enumerate(self._list)
            if item_key == lookup_key
        for idx in reversed(found_indexes[1:]):
            del self._list[idx]
        if found_indexes:
            idx = found_indexes[0]
            self._list[idx] = (set_key, lookup_key, set_value)
            self._list.append((set_key, lookup_key, set_value))
        Remove the header `key`.
        del_key = key.lower().encode(self.encoding)
        pop_indexes = [
            if item_key.lower() == del_key
        if not pop_indexes:
        for idx in reversed(pop_indexes):
    def __contains__(self, key: typing.Any) -> bool:
        header_key = key.lower().encode(self.encoding)
        return header_key in [key for _, key, _ in self._list]
    def __iter__(self) -> typing.Iterator[typing.Any]:
        return iter(self.keys())
        return len(self._list)
            other_headers = Headers(other)
        self_list = [(key, value) for _, key, value in self._list]
        other_list = [(key, value) for _, key, value in other_headers._list]
        return sorted(self_list) == sorted(other_list)
        encoding_str = ""
        if self.encoding != "ascii":
            encoding_str = f", encoding={self.encoding!r}"
        as_list = list(_obfuscate_sensitive_headers(self.multi_items()))
        as_dict = dict(as_list)
        no_duplicate_keys = len(as_dict) == len(as_list)
        if no_duplicate_keys:
            return f"{class_name}({as_dict!r}{encoding_str})"
        return f"{class_name}({as_list!r}{encoding_str})"
        stream: SyncByteStream | AsyncByteStream | None = None,
        self.method = method.upper()
        self.url = URL(url) if params is None else URL(url, params=params)
        self.extensions = {} if extensions is None else dict(extensions)
            Cookies(cookies).set_cookie_header(self)
            content_type: str | None = self.headers.get("content-type")
            headers, stream = encode_request(
                boundary=get_multipart_boundary_from_content_type(
                    content_type=content_type.encode(self.headers.encoding)
                    if content_type
            self._prepare(headers)
            # Load the request body, except for streaming content.
            if isinstance(stream, ByteStream):
            # There's an important distinction between `Request(content=...)`,
            # and `Request(stream=...)`.
            # Using `content=...` implies automatically populated `Host` and content
            # headers, of either `Content-Length: ...` or `Transfer-Encoding: chunked`.
            # Using `stream=...` will not automatically include *any*
            # auto-populated headers.
            # As an end-user you don't really need `stream=...`. It's only
            # useful when:
            # * Preserving the request stream when copying requests, eg for redirects.
            # * Creating request instances on the *server-side* of the transport API.
    def _prepare(self, default_headers: dict[str, str]) -> None:
        for key, value in default_headers.items():
            # Ignore Transfer-Encoding if the Content-Length has been set explicitly.
            if key.lower() == "transfer-encoding" and "Content-Length" in self.headers:
        auto_headers: list[tuple[bytes, bytes]] = []
        has_host = "Host" in self.headers
        has_content_length = (
            "Content-Length" in self.headers or "Transfer-Encoding" in self.headers
        if not has_host and self.url.host:
            auto_headers.append((b"Host", self.url.netloc))
        if not has_content_length and self.method in ("POST", "PUT", "PATCH"):
            auto_headers.append((b"Content-Length", b"0"))
        self.headers = Headers(auto_headers + self.headers.raw)
            raise RequestNotRead()
        Read and return the request content.
            assert isinstance(self.stream, typing.Iterable)
            self._content = b"".join(self.stream)
            if not isinstance(self.stream, ByteStream):
                # If a streaming request has been read entirely into memory, then
                # we can replace the stream with a raw bytes implementation,
                # to ensure that any non-replayable streams can still be used.
                self.stream = ByteStream(self._content)
            assert isinstance(self.stream, typing.AsyncIterable)
            self._content = b"".join([part async for part in self.stream])
        url = str(self.url)
        return f"<{class_name}({self.method!r}, {url!r})>"
    def __getstate__(self) -> dict[str, typing.Any]:
            for name, value in self.__dict__.items()
            if name not in ["extensions", "stream"]
    def __setstate__(self, state: dict[str, typing.Any]) -> None:
        self.extensions = {}
        self.stream = UnattachedStream()
        content: ResponseContent | None = None,
        html: str | None = None,
        json: typing.Any = None,
        extensions: ResponseExtensions | None = None,
        history: list[Response] | None = None,
        self._request: Request | None = request
        # When follow_redirects=False and a redirect is received,
        # the client will set `response.next_request`.
        self.next_request: Request | None = None
        self.history = [] if history is None else list(history)
        self.is_closed = False
        self.is_stream_consumed = False
        self.default_encoding = default_encoding
            headers, stream = encode_response(content, text, html, json)
                # Load the response body, except for streaming content.
            # There's an important distinction between `Response(content=...)`,
            # and `Response(stream=...)`.
            # Using `content=...` implies automatically populated content headers,
            # of either `Content-Length: ...` or `Transfer-Encoding: chunked`.
            # Using `stream=...` will not automatically include any content headers.
            # useful when creating response instances having received a stream
            # from the transport API.
        self._num_bytes_downloaded = 0
            if key.lower() == "transfer-encoding" and "content-length" in self.headers:
        Returns the time taken for the complete request/response
        cycle to complete.
        if not hasattr(self, "_elapsed"):
                "'.elapsed' may only be accessed after the response "
                "has been read or closed."
        return self._elapsed
    @elapsed.setter
    def elapsed(self, elapsed: datetime.timedelta) -> None:
        self._elapsed = elapsed
        Returns the request instance associated to the current response.
                "The request instance has not been set on this response."
    def request(self, value: Request) -> None:
        self._request = value
            http_version: bytes = self.extensions["http_version"]
            return "HTTP/1.1"
            return http_version.decode("ascii", errors="ignore")
    def reason_phrase(self) -> str:
            reason_phrase: bytes = self.extensions["reason_phrase"]
            return codes.get_reason_phrase(self.status_code)
            return reason_phrase.decode("ascii", errors="ignore")
    def url(self) -> URL:
        Returns the URL for which the request was made.
        return self.request.url
            raise ResponseNotRead()
        if not hasattr(self, "_text"):
            content = self.content
                self._text = ""
                decoder = TextDecoder(encoding=self.encoding or "utf-8")
                self._text = "".join([decoder.decode(self.content), decoder.flush()])
        return self._text
        Return an encoding to use for decoding the byte content into text.
        The priority for determining this is given by...
        * `.encoding = <>` has been set explicitly.
        * The encoding as specified by the charset parameter in the Content-Type header.
        * The encoding as determined by `default_encoding`, which may either be
          a string like "utf-8" indicating the encoding to use, or may be a callable
          which enables charset autodetection.
        if not hasattr(self, "_encoding"):
            encoding = self.charset_encoding
            if encoding is None or not _is_known_encoding(encoding):
                if isinstance(self.default_encoding, str):
                    encoding = self.default_encoding
                elif hasattr(self, "_content"):
                    encoding = self.default_encoding(self._content)
            self._encoding = encoding or "utf-8"
        Set the encoding to use for decoding the byte content into text.
        If the `text` attribute has been accessed, attempting to set the
        encoding will throw a ValueError.
        if hasattr(self, "_text"):
                "Setting encoding after `text` has been accessed is not allowed."
        Return the encoding, as specified by the Content-Type header.
        content_type = self.headers.get("Content-Type")
        return _parse_content_type_charset(content_type)
    def _get_content_decoder(self) -> ContentDecoder:
        Returns a decoder instance which can be used to decode the raw byte
        content, depending on the Content-Encoding used in the response.
        if not hasattr(self, "_decoder"):
            decoders: list[ContentDecoder] = []
            values = self.headers.get_list("content-encoding", split_commas=True)
                value = value.strip().lower()
                    decoder_cls = SUPPORTED_DECODERS[value]
                    decoders.append(decoder_cls())
            if len(decoders) == 1:
                self._decoder = decoders[0]
            elif len(decoders) > 1:
                self._decoder = MultiDecoder(children=decoders)
                self._decoder = IdentityDecoder()
        return self._decoder
    def is_informational(self) -> bool:
        A property which is `True` for 1xx status codes, `False` otherwise.
        return codes.is_informational(self.status_code)
    def is_success(self) -> bool:
        A property which is `True` for 2xx status codes, `False` otherwise.
        return codes.is_success(self.status_code)
    def is_redirect(self) -> bool:
        A property which is `True` for 3xx status codes, `False` otherwise.
        Note that not all responses with a 3xx status code indicate a URL redirect.
        Use `response.has_redirect_location` to determine responses with a properly
        formed URL redirection.
        return codes.is_redirect(self.status_code)
    def is_client_error(self) -> bool:
        A property which is `True` for 4xx status codes, `False` otherwise.
        return codes.is_client_error(self.status_code)
    def is_server_error(self) -> bool:
        A property which is `True` for 5xx status codes, `False` otherwise.
        return codes.is_server_error(self.status_code)
    def is_error(self) -> bool:
        A property which is `True` for 4xx and 5xx status codes, `False` otherwise.
        return codes.is_error(self.status_code)
    def has_redirect_location(self) -> bool:
        Returns True for 3xx responses with a properly formed URL redirection,
        `False` otherwise.
                # 301 (Cacheable redirect. Method may change to GET.)
                codes.MOVED_PERMANENTLY,
                # 302 (Uncacheable redirect. Method may change to GET.)
                codes.FOUND,
                # 303 (Client should make a GET or HEAD request.)
                codes.SEE_OTHER,
                # 307 (Equiv. 302, but retain method)
                codes.TEMPORARY_REDIRECT,
                # 308 (Equiv. 301, but retain method)
                codes.PERMANENT_REDIRECT,
            and "Location" in self.headers
    def raise_for_status(self) -> Response:
        Raise the `HTTPStatusError` if one occurred.
        request = self._request
        if request is None:
                "Cannot call `raise_for_status` as the request "
                "instance has not been set on this response."
        if self.is_success:
        if self.has_redirect_location:
                "{error_type} '{0.status_code} {0.reason_phrase}' for url '{0.url}'\n"
                "Redirect location: '{0.headers[location]}'\n"
                "For more information check: https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{0.status_code}"
        status_class = self.status_code // 100
        error_types = {
            1: "Informational response",
            3: "Redirect response",
            4: "Client error",
            5: "Server error",
        error_type = error_types.get(status_class, "Invalid status code")
        message = message.format(self, error_type=error_type)
        raise HTTPStatusError(message, request=request, response=self)
    def json(self, **kwargs: typing.Any) -> typing.Any:
        return jsonlib.loads(self.content, **kwargs)
        if not hasattr(self, "_cookies"):
            self._cookies = Cookies()
            self._cookies.extract_cookies(self)
    def links(self) -> dict[str | None, dict[str, str]]:
        Returns the parsed header links of the response, if any
        if header is None:
            (link.get("rel") or link.get("url")): link
            for link in _parse_header_links(header)
    def num_bytes_downloaded(self) -> int:
        return self._num_bytes_downloaded
        return f"<Response [{self.status_code} {self.reason_phrase}]>"
            if name not in ["extensions", "stream", "is_closed", "_decoder"]
        self.is_closed = True
        Read and return the response content.
            self._content = b"".join(self.iter_bytes())
    def iter_bytes(self, chunk_size: int | None = None) -> typing.Iterator[bytes]:
        This allows us to handle gzip, deflate, brotli, and zstd encoded responses.
        if hasattr(self, "_content"):
            chunk_size = len(self._content) if chunk_size is None else chunk_size
            for i in range(0, len(self._content), max(chunk_size, 1)):
                yield self._content[i : i + chunk_size]
            decoder = self._get_content_decoder()
            chunker = ByteChunker(chunk_size=chunk_size)
            with request_context(request=self._request):
                for raw_bytes in self.iter_raw():
                    decoded = decoder.decode(raw_bytes)
                    for chunk in chunker.decode(decoded):
                decoded = decoder.flush()
                    yield chunk  # pragma: no cover
                for chunk in chunker.flush():
    def iter_text(self, chunk_size: int | None = None) -> typing.Iterator[str]:
        A str-iterator over the decoded response content
        chunker = TextChunker(chunk_size=chunk_size)
            for byte_content in self.iter_bytes():
                text_content = decoder.decode(byte_content)
                for chunk in chunker.decode(text_content):
            text_content = decoder.flush()
    def iter_lines(self) -> typing.Iterator[str]:
        decoder = LineDecoder()
            for text in self.iter_text():
                for line in decoder.decode(text):
            for line in decoder.flush():
    def iter_raw(self, chunk_size: int | None = None) -> typing.Iterator[bytes]:
        A byte-iterator over the raw response content.
        if self.is_stream_consumed:
            raise StreamConsumed()
            raise StreamClosed()
        if not isinstance(self.stream, SyncByteStream):
            raise RuntimeError("Attempted to call a sync iterator on an async stream.")
        self.is_stream_consumed = True
            for raw_stream_bytes in self.stream:
                self._num_bytes_downloaded += len(raw_stream_bytes)
                for chunk in chunker.decode(raw_stream_bytes):
            raise RuntimeError("Attempted to call an sync close on an async stream.")
        if not self.is_closed:
            self._content = b"".join([part async for part in self.aiter_bytes()])
    async def aiter_bytes(
        self, chunk_size: int | None = None
    ) -> typing.AsyncIterator[bytes]:
                async for raw_bytes in self.aiter_raw():
    async def aiter_text(
    ) -> typing.AsyncIterator[str]:
            async for byte_content in self.aiter_bytes():
    async def aiter_lines(self) -> typing.AsyncIterator[str]:
            async for text in self.aiter_text():
    async def aiter_raw(
        if not isinstance(self.stream, AsyncByteStream):
            raise RuntimeError("Attempted to call an async iterator on an sync stream.")
            async for raw_stream_bytes in self.stream:
        await self.aclose()
            raise RuntimeError("Attempted to call an async close on an sync stream.")
class Cookies(typing.MutableMapping[str, str]):
    HTTP Cookies, as a mutable mapping.
    def __init__(self, cookies: CookieTypes | None = None) -> None:
        if cookies is None or isinstance(cookies, dict):
            self.jar = CookieJar()
            if isinstance(cookies, dict):
                for key, value in cookies.items():
                    self.set(key, value)
        elif isinstance(cookies, list):
            for key, value in cookies:
        elif isinstance(cookies, Cookies):
            for cookie in cookies.jar:
                self.jar.set_cookie(cookie)
            self.jar = cookies
    def extract_cookies(self, response: Response) -> None:
        Loads any cookies based on the response `Set-Cookie` headers.
        urllib_response = self._CookieCompatResponse(response)
        urllib_request = self._CookieCompatRequest(response.request)
        self.jar.extract_cookies(urllib_response, urllib_request)  # type: ignore
    def set_cookie_header(self, request: Request) -> None:
        Sets an appropriate 'Cookie:' HTTP header on the `Request`.
        urllib_request = self._CookieCompatRequest(request)
        self.jar.add_cookie_header(urllib_request)
    def set(self, name: str, value: str, domain: str = "", path: str = "/") -> None:
        Set a cookie value by name. May optionally include domain and path.
            "version": 0,
            "port": None,
            "port_specified": False,
            "domain": domain,
            "domain_specified": bool(domain),
            "domain_initial_dot": domain.startswith("."),
            "path": path,
            "path_specified": bool(path),
            "secure": False,
            "discard": True,
            "comment": None,
            "comment_url": None,
            "rest": {"HttpOnly": None},
            "rfc2109": False,
        cookie = Cookie(**kwargs)  # type: ignore
    def get(  # type: ignore
        default: str | None = None,
    ) -> str | None:
        Get a cookie by name. May optionally include domain and path
        in order to specify exactly which cookie to retrieve.
        for cookie in self.jar:
            if cookie.name == name:
                if domain is None or cookie.domain == domain:
                    if path is None or cookie.path == path:
                            message = f"Multiple cookies exist with name={name}"
                            raise CookieConflict(message)
                        value = cookie.value
        Delete a cookie by name. May optionally include domain and path
        in order to specify exactly which cookie to delete.
        if domain is not None and path is not None:
            return self.jar.clear(domain, path, name)
        remove = [
            cookie
            for cookie in self.jar
            if cookie.name == name
            and (domain is None or cookie.domain == domain)
            and (path is None or cookie.path == path)
        for cookie in remove:
            self.jar.clear(cookie.domain, cookie.path, cookie.name)
    def clear(self, domain: str | None = None, path: str | None = None) -> None:
        Delete all cookies. Optionally include a domain and path in
        order to only delete a subset of all the cookies.
            args.append(domain)
            assert domain is not None
            args.append(path)
        self.jar.clear(*args)
    def update(self, cookies: CookieTypes | None = None) -> None:  # type: ignore
        cookies = Cookies(cookies)
    def __setitem__(self, name: str, value: str) -> None:
        return self.set(name, value)
    def __getitem__(self, name: str) -> str:
        value = self.get(name)
    def __delitem__(self, name: str) -> None:
        return self.delete(name)
        return len(self.jar)
    def __iter__(self) -> typing.Iterator[str]:
        return (cookie.name for cookie in self.jar)
        for _ in self.jar:
        cookies_repr = ", ".join(
                f"<Cookie {cookie.name}={cookie.value} for {cookie.domain} />"
        return f"<Cookies[{cookies_repr}]>"
    class _CookieCompatRequest(urllib.request.Request):
        Wraps a `Request` instance up in a compatibility interface suitable
        for use with `CookieJar` operations.
        def __init__(self, request: Request) -> None:
                url=str(request.url),
                headers=dict(request.headers),
                method=request.method,
        def add_unredirected_header(self, key: str, value: str) -> None:
            super().add_unredirected_header(key, value)
            self.request.headers[key] = value
    class _CookieCompatResponse:
        def __init__(self, response: Response) -> None:
        def info(self) -> email.message.Message:
            info = email.message.Message()
            for key, value in self.response.headers.multi_items():
                # Note that setting `info[key]` here is an "append" operation,
                # not a "replace" operation.
                # https://docs.python.org/3/library/email.compat32-message.html#email.message.Message.__setitem__
