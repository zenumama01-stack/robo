__all__ = ["Model"]
class Model(BaseModel):
    """Describes an OpenAI model offering that can be used with the API."""
    """The model identifier, which can be referenced in the API endpoints."""
    """The Unix timestamp (in seconds) when the model was created."""
    object: Literal["model"]
    """The object type, which is always "model"."""
    owned_by: str
    """The organization that owns the model."""
"""Model objects for requests and responses.
Each API may support one or more serializations, such
as JSON, Atom, etc. The model classes are responsible
for converting between the wire format and the Python
object representation.
from googleapiclient import version as googleapiclient_version
from googleapiclient.errors import HttpError
    from google.api_core.version_header import API_VERSION_METADATA_KEY
    HAS_API_VERSION = True
    HAS_API_VERSION = False
_LIBRARY_VERSION = googleapiclient_version.__version__
_PY_VERSION = platform.python_version()
dump_request_response = False
def _abstract():
    raise NotImplementedError("You need to override this function")
class Model(object):
    """Model base class.
    All Model classes should implement this interface.
    The Model serializes and de-serializes between a wire
    format such as JSON and a Python object representation.
    def request(self, headers, path_params, query_params, body_value):
        """Updates outgoing requests with a serialized body.
        _abstract()
    def response(self, resp, content):
        """Convert the response wire format into a Python object.
class BaseModel(Model):
    """Base model class.
    Subclasses should provide implementations for the "serialize" and
    "deserialize" methods, as well as values for the following class attributes.
      no_content_response: The value to return when deserializing a 204 "No
          Content" response.
      alt_param: The value to supply as the "alt" query parameter for requests.
    accept = None
    content_type = None
    no_content_response = None
    alt_param = None
    def _log_request(self, headers, path_params, query, body):
        """Logs debugging information about the request if requested."""
        if dump_request_response:
            LOGGER.info("--request-start--")
            LOGGER.info("-headers-start-")
            for h, v in headers.items():
                LOGGER.info("%s: %s", h, v)
            LOGGER.info("-headers-end-")
            LOGGER.info("-path-parameters-start-")
            for h, v in path_params.items():
            LOGGER.info("-path-parameters-end-")
            LOGGER.info("body: %s", body)
            LOGGER.info("query: %s", query)
            LOGGER.info("--request-end--")
    def request(self, headers, path_params, query_params, body_value, api_version=None):
          api_version: str, The precise API version represented by this request,
              which will result in an API Version header being sent along with the
              HTTP request.
        query = self._build_query(query_params)
        headers["accept"] = self.accept
        headers["accept-encoding"] = "gzip, deflate"
            headers["user-agent"] += " "
            headers["user-agent"] = ""
        headers["user-agent"] += "(gzip)"
        if "x-goog-api-client" in headers:
            headers["x-goog-api-client"] += " "
            headers["x-goog-api-client"] = ""
        headers["x-goog-api-client"] += "gdcl/%s gl-python/%s" % (
            _LIBRARY_VERSION,
            _PY_VERSION,
        if api_version and HAS_API_VERSION:
            headers[API_VERSION_METADATA_KEY] = api_version
        elif api_version:
                "The `api_version` argument is ignored as a newer version of "
                "`google-api-core` is required to use this feature."
                "Please upgrade `google-api-core` to 2.19.0 or newer."
        if body_value is not None:
            headers["content-type"] = self.content_type
            body_value = self.serialize(body_value)
        self._log_request(headers, path_params, query, body_value)
        return (headers, path_params, query, body_value)
    def _build_query(self, params):
        """Builds a query string.
        if self.alt_param is not None:
            params.update({"alt": self.alt_param})
        astuples = []
        for key, value in params.items():
            if type(value) == type([]):
                for x in value:
                    x = x.encode("utf-8")
                    astuples.append((key, x))
                if isinstance(value, str) and callable(value.encode):
                    value = value.encode("utf-8")
                astuples.append((key, value))
        return "?" + urllib.parse.urlencode(astuples)
    def _log_response(self, resp, content):
        """Logs debugging information about the response if requested."""
            LOGGER.info("--response-start--")
            for h, v in resp.items():
                LOGGER.info(content)
            LOGGER.info("--response-end--")
        self._log_response(resp, content)
        # Error handling is TBD, for example, do we retry
        # for some operation/error combinations?
        if resp.status < 300:
            if resp.status == 204:
                # A 204: No Content response should be treated differently
                # to all the other success states
                return self.no_content_response
            return self.deserialize(content)
            LOGGER.debug("Content from bad request was: %r" % content)
            raise HttpError(resp, content)
    def serialize(self, body_value):
        """Perform the actual Python object serialization.
    def deserialize(self, content):
        """Perform the actual deserialization from response string to Python
class JsonModel(BaseModel):
    """Model class for JSON.
    Serializes and de-serializes between JSON and the Python
    object representation of HTTP request and response bodies.
    accept = "application/json"
    content_type = "application/json"
    alt_param = "json"
    def __init__(self, data_wrapper=False):
        """Construct a JsonModel.
        self._data_wrapper = data_wrapper
            isinstance(body_value, dict)
            and "data" not in body_value
            and self._data_wrapper
            body_value = {"data": body_value}
        return json.dumps(body_value)
            body = json.loads(content)
        except json.decoder.JSONDecodeError:
            body = content
            if self._data_wrapper and "data" in body:
                body = body["data"]
    def no_content_response(self):
class RawModel(JsonModel):
    """Model class for requests that don't return JSON.
    object representation of HTTP request, and returns the raw bytes
    of the response body.
    accept = "*/*"
class MediaModel(JsonModel):
    """Model class for requests that return Media.
    alt_param = "media"
class ProtocolBufferModel(BaseModel):
    """Model class for protocol buffers.
    Serializes and de-serializes the binary protocol buffer sent in the HTTP
    request and response bodies.
    accept = "application/x-protobuf"
    content_type = "application/x-protobuf"
    alt_param = "proto"
    def __init__(self, protocol_buffer):
        """Constructs a ProtocolBufferModel.
        self._protocol_buffer = protocol_buffer
        return body_value.SerializeToString()
        return self._protocol_buffer.FromString(content)
        return self._protocol_buffer()
def makepatch(original, modified):
    """Create a patch object.
    patch = {}
    for key, original_value in original.items():
        modified_value = modified.get(key, None)
        if modified_value is None:
            # Use None to signal that the element is deleted
            patch[key] = None
        elif original_value != modified_value:
            if type(original_value) == type({}):
                # Recursively descend objects
                patch[key] = makepatch(original_value, modified_value)
                # In the case of simple types or arrays we just replace
                patch[key] = modified_value
            # Don't add anything to patch if there's no change
    for key in modified:
        if key not in original:
            patch[key] = modified[key]
    return patch
from dataclasses import asdict, is_dataclass
from langchain_core.callbacks import CallbackManagerForLLMRun
from langchain_core.outputs import ChatGeneration, ChatResult
class FakeToolCallingModel(BaseChatModel):
    tool_calls: list[list[ToolCall]] | list[list[dict[str, Any]]] | None = None
    structured_response: Any | None = None
    index: int = 0
    tool_style: Literal["openai", "anthropic"] = "openai"
    def _generate(
        run_manager: CallbackManagerForLLMRun | None = None,
    ) -> ChatResult:
        """Top Level call."""
        is_native = kwargs.get("response_format")
        if self.tool_calls:
            if is_native:
                tool_calls = (
                    self.tool_calls[self.index] if self.index < len(self.tool_calls) else []
                tool_calls = self.tool_calls[self.index % len(self.tool_calls)]
        if is_native and not tool_calls:
            if isinstance(self.structured_response, BaseModel):
                content_obj = self.structured_response.model_dump()
            elif is_dataclass(self.structured_response) and not isinstance(
                self.structured_response, type
                content_obj = asdict(self.structured_response)
            elif isinstance(self.structured_response, dict):
                content_obj = self.structured_response
            message = AIMessage(content=json.dumps(content_obj), id=str(self.index))
            messages_string = "-".join([m.text for m in messages])
            message = AIMessage(
                content=messages_string,
                id=str(self.index),
                tool_calls=tool_calls.copy(),
        return ChatResult(generations=[ChatGeneration(message=message)])
    def _llm_type(self) -> str:
        return "fake-tool-call-model"
        tools: Sequence[dict[str, Any] | type | Callable[..., Any] | BaseTool],
        tool_choice: str | None = None,
            msg = "Must provide at least one tool"
        tool_dicts = []
            if isinstance(tool, dict):
                tool_dicts.append(tool)
            if not isinstance(tool, BaseTool):
                msg = "Only BaseTool and dict is supported by FakeToolCallingModel.bind_tools"
            # NOTE: this is a simplified tool spec for testing purposes only
            if self.tool_style == "openai":
                tool_dicts.append(
                            "name": tool.name,
            elif self.tool_style == "anthropic":
        return self.bind(tools=tool_dicts, **kwargs)
import torch
from coqpit import Coqpit
from trainer import TrainerModel
class BaseTrainerModel(TrainerModel):
    """BaseTrainerModel model expanding TrainerModel with required functions by 🐸TTS.
    Every new 🐸TTS model must inherit it.
    def init_from_config(config: Coqpit):
        """Init the model and all its attributes from the given config.
        Override this depending on your model.
    def inference(self, input: torch.Tensor, aux_input={}) -> Dict:
        """Forward pass for inference.
        It must return a dictionary with the main model output and all the auxiliary outputs. The key ```model_outputs```
        is considered to be the main output and you can add any other auxiliary outputs as you want.
        We don't use `*kwargs` since it is problematic with the TorchScript API.
            input (torch.Tensor): [description]
            aux_input (Dict): Auxiliary inputs like speaker embeddings, durations etc.
            Dict: [description]
        outputs_dict = {"model_outputs": None}
        return outputs_dict
    def load_checkpoint(
        self, config: Coqpit, checkpoint_path: str, eval: bool = False, strict: bool = True, cache=False
        """Load a model checkpoint gile and get ready for training or inference.
            config (Coqpit): Model configuration.
            checkpoint_path (str): Path to the model checkpoint file.
            eval (bool, optional): If true, init model for inference else for training. Defaults to False.
            strict (bool, optional): Match all checkpoint keys to model's keys. Defaults to True.
            cache (bool, optional): If True, cache the file locally for subsequent calls. It is cached under `get_user_data_dir()/tts_cache`. Defaults to False.
Much of this code is adapted from Andrej Karpathy's NanoGPT
(https://github.com/karpathy/nanoGPT)
from torch.nn import functional as F
class LayerNorm(nn.Module):
    """LayerNorm but with an optional bias. PyTorch doesn't support simply bias=False"""
    def __init__(self, ndim, bias):
        self.weight = nn.Parameter(torch.ones(ndim))
        self.bias = nn.Parameter(torch.zeros(ndim)) if bias else None
    def forward(self, x):
        return F.layer_norm(x, self.weight.shape, self.weight, self.bias, 1e-5)
class CausalSelfAttention(nn.Module):
    def __init__(self, config):
        assert config.n_embd % config.n_head == 0
        # key, query, value projections for all heads, but in a batch
        self.c_attn = nn.Linear(config.n_embd, 3 * config.n_embd, bias=config.bias)
        # output projection
        self.c_proj = nn.Linear(config.n_embd, config.n_embd, bias=config.bias)
        # regularization
        self.attn_dropout = nn.Dropout(config.dropout)
        self.resid_dropout = nn.Dropout(config.dropout)
        self.n_head = config.n_head
        self.n_embd = config.n_embd
        self.dropout = config.dropout
        # flash attention make GPU go brrrrr but support is only in PyTorch nightly and still a bit scary
        self.flash = hasattr(torch.nn.functional, "scaled_dot_product_attention")
        if not self.flash:
            # print("WARNING: using slow attention. Flash Attention atm needs PyTorch nightly and dropout=0.0")
            # causal mask to ensure that attention is only applied to the left in the input sequence
            self.register_buffer(
                "bias",
                torch.tril(torch.ones(config.block_size, config.block_size)).view(
                    1, 1, config.block_size, config.block_size
    def forward(self, x, past_kv=None, use_cache=False):
        B, T, C = x.size()  # batch size, sequence length, embedding dimensionality (n_embd)
        # calculate query, key, values for all heads in batch and move head forward to be the batch dim
        q, k, v = self.c_attn(x).split(self.n_embd, dim=2)
        k = k.view(B, T, self.n_head, C // self.n_head).transpose(1, 2)  # (B, nh, T, hs)
        q = q.view(B, T, self.n_head, C // self.n_head).transpose(1, 2)  # (B, nh, T, hs)
        v = v.view(B, T, self.n_head, C // self.n_head).transpose(1, 2)  # (B, nh, T, hs)
        if past_kv is not None:
            past_key = past_kv[0]
            past_value = past_kv[1]
            k = torch.cat((past_key, k), dim=-2)
            v = torch.cat((past_value, v), dim=-2)
        FULL_T = k.shape[-2]
        if use_cache is True:
            present = (k, v)
            present = None
        # causal self-attention; Self-attend: (B, nh, T, hs) x (B, nh, hs, T) -> (B, nh, T, T)
        if self.flash:
            # efficient attention using Flash Attention CUDA kernels
                # When `past_kv` is provided, we're doing incremental decoding and `q.shape[2] == 1`: q only contains
                # the query for the last token. scaled_dot_product_attention interprets this as the first token in the
                # sequence, so if is_causal=True it will mask out all attention from it. This is not what we want, so
                # to work around this we set is_causal=False.
                is_causal = False
                is_causal = True
            y = torch.nn.functional.scaled_dot_product_attention(q, k, v, dropout_p=self.dropout, is_causal=is_causal)
            # manual implementation of attention
            att = (q @ k.transpose(-2, -1)) * (1.0 / math.sqrt(k.size(-1)))
            att = att.masked_fill(self.bias[:, :, FULL_T - T : FULL_T, :FULL_T] == 0, float("-inf"))
            att = F.softmax(att, dim=-1)
            att = self.attn_dropout(att)
            y = att @ v  # (B, nh, T, T) x (B, nh, T, hs) -> (B, nh, T, hs)
        y = y.transpose(1, 2).contiguous().view(B, T, C)  # re-assemble all head outputs side by side
        y = self.resid_dropout(self.c_proj(y))
        return (y, present)
class MLP(nn.Module):
        self.c_fc = nn.Linear(config.n_embd, 4 * config.n_embd, bias=config.bias)
        self.c_proj = nn.Linear(4 * config.n_embd, config.n_embd, bias=config.bias)
        self.dropout = nn.Dropout(config.dropout)
        self.gelu = nn.GELU()
        x = self.c_fc(x)
        x = self.gelu(x)
        x = self.c_proj(x)
        x = self.dropout(x)
class Block(nn.Module):
    def __init__(self, config, layer_idx):
        self.ln_1 = LayerNorm(config.n_embd, bias=config.bias)
        self.attn = CausalSelfAttention(config)
        self.ln_2 = LayerNorm(config.n_embd, bias=config.bias)
        self.mlp = MLP(config)
        self.layer_idx = layer_idx
        attn_output, prev_kvs = self.attn(self.ln_1(x), past_kv=past_kv, use_cache=use_cache)
        x = x + attn_output
        x = x + self.mlp(self.ln_2(x))
        return (x, prev_kvs)
class GPTConfig(Coqpit):
    block_size: int = 1024
    input_vocab_size: int = 10_048
    output_vocab_size: int = 10_048
    n_layer: int = 12
    n_head: int = 12
    n_embd: int = 768
    dropout: float = 0.0
    bias: bool = True  # True: bias in Linears and LayerNorms, like GPT-2. False: a bit better and faster
class GPT(nn.Module):
        assert config.input_vocab_size is not None
        assert config.output_vocab_size is not None
        assert config.block_size is not None
        self.config = config
        self.transformer = nn.ModuleDict(
                wte=nn.Embedding(config.input_vocab_size, config.n_embd),
                wpe=nn.Embedding(config.block_size, config.n_embd),
                drop=nn.Dropout(config.dropout),
                h=nn.ModuleList([Block(config, idx) for idx in range(config.n_layer)]),
                ln_f=LayerNorm(config.n_embd, bias=config.bias),
        self.lm_head = nn.Linear(config.n_embd, config.output_vocab_size, bias=False)
    def get_num_params(self, non_embedding=True):
        Return the number of parameters in the model.
        For non-embedding count (default), the position embeddings get subtracted.
        The token embeddings would too, except due to the parameter sharing these
        params are actually used as weights in the final layer, so we include them.
        n_params = sum(p.numel() for p in self.parameters())
        if non_embedding:
            n_params -= self.transformer.wte.weight.numel()
            n_params -= self.transformer.wpe.weight.numel()
        return n_params
    def forward(self, idx, merge_context=False, past_kv=None, position_ids=None, use_cache=False):
        device = idx.device
        _, t = idx.size()
            assert t == 1
            tok_emb = self.transformer.wte(idx)  # token embeddings of shape (b, t, n_embd)
            if merge_context:
                assert idx.shape[1] >= 256 + 256 + 1
                t = idx.shape[1] - 256
                    t <= self.config.block_size
                ), f"Cannot forward sequence of length {t}, block size is only {self.config.block_size}"
            # forward the GPT model itself
                tok_emb = torch.cat(
                        self.transformer.wte(idx[:, :256]) + self.transformer.wte(idx[:, 256 : 256 + 256]),
                        self.transformer.wte(idx[:, 256 + 256 :]),
                    dim=1,
        if past_kv is None:
            past_length = 0
            past_kv = tuple([None] * len(self.transformer.h))
            past_length = past_kv[0][0].size(-2)
        if position_ids is None:
            position_ids = torch.arange(past_length, t + past_length, dtype=torch.long, device=device)
            position_ids = position_ids.unsqueeze(0)  # shape (1, t)
            assert position_ids.shape == (1, t)
        pos_emb = self.transformer.wpe(position_ids)  # position embeddings of shape (1, t, n_embd)
        x = self.transformer.drop(tok_emb + pos_emb)
        new_kv = () if use_cache else None
        for _, (block, past_layer_kv) in enumerate(zip(self.transformer.h, past_kv)):
            x, kv = block(x, past_kv=past_layer_kv, use_cache=use_cache)
            if use_cache:
                new_kv = new_kv + (kv,)
        x = self.transformer.ln_f(x)
        # inference-time mini-optimization: only forward the lm_head on the very last position
        logits = self.lm_head(x[:, [-1], :])  # note: using list [-1] to preserve the time dim
        return (logits, new_kv)
from .error import CDefError, VerificationError, VerificationMissing
# type qualifiers
Q_CONST    = 0x01
Q_RESTRICT = 0x02
Q_VOLATILE = 0x04
def qualify(quals, replace_with):
    if quals & Q_CONST:
        replace_with = ' const ' + replace_with.lstrip()
    if quals & Q_VOLATILE:
        replace_with = ' volatile ' + replace_with.lstrip()
    if quals & Q_RESTRICT:
        # It seems that __restrict is supported by gcc and msvc.
        # If you hit some different compiler, add a #define in
        # _cffi_include.h for it (and in its copies, documented there)
        replace_with = ' __restrict ' + replace_with.lstrip()
    return replace_with
class BaseTypeByIdentity(object):
    is_array_type = False
    is_raw_function = False
    def get_c_name(self, replace_with='', context='a C file', quals=0):
        result = self.c_name_with_marker
        assert result.count('&') == 1
        # some logic duplication with ffi.getctype()... :-(
        if replace_with:
            if replace_with.startswith('*') and '&[' in result:
            elif not replace_with[0] in '[(':
        replace_with = qualify(quals, replace_with)
        result = result.replace('&', replace_with)
        if '$' in result:
            raise VerificationError(
                "cannot generate '%s' in %s: unknown type name"
                % (self._get_c_name(), context))
    def _get_c_name(self):
        return self.c_name_with_marker.replace('&', '')
    def has_c_name(self):
        return '$' not in self._get_c_name()
    def is_integer_type(self):
    def get_cached_btype(self, ffi, finishlist, can_delay=False):
            BType = ffi._cached_btypes[self]
            BType = self.build_backend_type(ffi, finishlist)
            BType2 = ffi._cached_btypes.setdefault(self, BType)
            assert BType2 is BType
        return '<%s>' % (self._get_c_name(),)
    def _get_items(self):
        return [(name, getattr(self, name)) for name in self._attrs_]
class BaseType(BaseTypeByIdentity):
        return (self.__class__ == other.__class__ and
                self._get_items() == other._get_items())
        return hash((self.__class__, tuple(self._get_items())))
class VoidType(BaseType):
    _attrs_ = ()
        self.c_name_with_marker = 'void&'
    def build_backend_type(self, ffi, finishlist):
        return global_cache(self, ffi, 'new_void_type')
void_type = VoidType()
class BasePrimitiveType(BaseType):
    def is_complex_type(self):
class PrimitiveType(BasePrimitiveType):
    _attrs_ = ('name',)
    ALL_PRIMITIVE_TYPES = {
        'char':               'c',
        'short':              'i',
        'int':                'i',
        'long':               'i',
        'long long':          'i',
        'signed char':        'i',
        'unsigned char':      'i',
        'unsigned short':     'i',
        'unsigned int':       'i',
        'unsigned long':      'i',
        'unsigned long long': 'i',
        'float':              'f',
        'double':             'f',
        'long double':        'f',
        '_cffi_float_complex_t': 'j',
        '_cffi_double_complex_t': 'j',
        '_Bool':              'i',
        # the following types are not primitive in the C sense
        'wchar_t':            'c',
        'char16_t':           'c',
        'char32_t':           'c',
        'int8_t':             'i',
        'uint8_t':            'i',
        'int16_t':            'i',
        'uint16_t':           'i',
        'int32_t':            'i',
        'uint32_t':           'i',
        'int64_t':            'i',
        'uint64_t':           'i',
        'int_least8_t':       'i',
        'uint_least8_t':      'i',
        'int_least16_t':      'i',
        'uint_least16_t':     'i',
        'int_least32_t':      'i',
        'uint_least32_t':     'i',
        'int_least64_t':      'i',
        'uint_least64_t':     'i',
        'int_fast8_t':        'i',
        'uint_fast8_t':       'i',
        'int_fast16_t':       'i',
        'uint_fast16_t':      'i',
        'int_fast32_t':       'i',
        'uint_fast32_t':      'i',
        'int_fast64_t':       'i',
        'uint_fast64_t':      'i',
        'intptr_t':           'i',
        'uintptr_t':          'i',
        'intmax_t':           'i',
        'uintmax_t':          'i',
        'ptrdiff_t':          'i',
        'size_t':             'i',
        'ssize_t':            'i',
        assert name in self.ALL_PRIMITIVE_TYPES
        self.c_name_with_marker = name + '&'
    def is_char_type(self):
        return self.ALL_PRIMITIVE_TYPES[self.name] == 'c'
        return self.ALL_PRIMITIVE_TYPES[self.name] == 'i'
    def is_float_type(self):
        return self.ALL_PRIMITIVE_TYPES[self.name] == 'f'
        return self.ALL_PRIMITIVE_TYPES[self.name] == 'j'
        return global_cache(self, ffi, 'new_primitive_type', self.name)
class UnknownIntegerType(BasePrimitiveType):
        raise NotImplementedError("integer type '%s' can only be used after "
                                  "compilation" % self.name)
class UnknownFloatType(BasePrimitiveType):
    _attrs_ = ('name', )
        raise NotImplementedError("float type '%s' can only be used after "
class BaseFunctionType(BaseType):
    _attrs_ = ('args', 'result', 'ellipsis', 'abi')
    def __init__(self, args, result, ellipsis, abi=None):
        self.ellipsis = ellipsis
        self.abi = abi
        reprargs = [arg._get_c_name() for arg in self.args]
        if self.ellipsis:
            reprargs.append('...')
        reprargs = reprargs or ['void']
        replace_with = self._base_pattern % (', '.join(reprargs),)
        if abi is not None:
            replace_with = replace_with[:1] + abi + ' ' + replace_with[1:]
        self.c_name_with_marker = (
            self.result.c_name_with_marker.replace('&', replace_with))
class RawFunctionType(BaseFunctionType):
    # Corresponds to a C type like 'int(int)', which is the C type of
    # a function, but not a pointer-to-function.  The backend has no
    # notion of such a type; it's used temporarily by parsing.
    _base_pattern = '(&)(%s)'
    is_raw_function = True
        raise CDefError("cannot render the type %r: it is a function "
                        "type, not a pointer-to-function type" % (self,))
    def as_function_pointer(self):
        return FunctionPtrType(self.args, self.result, self.ellipsis, self.abi)
class FunctionPtrType(BaseFunctionType):
    _base_pattern = '(*&)(%s)'
        result = self.result.get_cached_btype(ffi, finishlist)
        for tp in self.args:
            args.append(tp.get_cached_btype(ffi, finishlist))
        abi_args = ()
        if self.abi == "__stdcall":
            if not self.ellipsis:    # __stdcall ignored for variadic funcs
                    abi_args = (ffi._backend.FFI_STDCALL,)
        return global_cache(self, ffi, 'new_function_type',
                            tuple(args), result, self.ellipsis, *abi_args)
    def as_raw_function(self):
        return RawFunctionType(self.args, self.result, self.ellipsis, self.abi)
class PointerType(BaseType):
    _attrs_ = ('totype', 'quals')
    def __init__(self, totype, quals=0):
        self.totype = totype
        self.quals = quals
        extra = " *&"
        if totype.is_array_type:
            extra = "(%s)" % (extra.lstrip(),)
        extra = qualify(quals, extra)
        self.c_name_with_marker = totype.c_name_with_marker.replace('&', extra)
        BItem = self.totype.get_cached_btype(ffi, finishlist, can_delay=True)
        return global_cache(self, ffi, 'new_pointer_type', BItem)
voidp_type = PointerType(void_type)
def ConstPointerType(totype):
    return PointerType(totype, Q_CONST)
const_voidp_type = ConstPointerType(void_type)
class NamedPointerType(PointerType):
    _attrs_ = ('totype', 'name')
    def __init__(self, totype, name, quals=0):
        PointerType.__init__(self, totype, quals)
class ArrayType(BaseType):
    _attrs_ = ('item', 'length')
    is_array_type = True
    def __init__(self, item, length):
        self.item = item
            brackets = '&[]'
        elif length == '...':
            brackets = '&[/*...*/]'
            brackets = '&[%s]' % length
            self.item.c_name_with_marker.replace('&', brackets))
    def length_is_unknown(self):
        return isinstance(self.length, str)
    def resolve_length(self, newlength):
        return ArrayType(self.item, newlength)
        if self.length_is_unknown():
            raise CDefError("cannot render the type %r: unknown length" %
                            (self,))
        self.item.get_cached_btype(ffi, finishlist)   # force the item BType
        BPtrItem = PointerType(self.item).get_cached_btype(ffi, finishlist)
        return global_cache(self, ffi, 'new_array_type', BPtrItem, self.length)
char_array_type = ArrayType(PrimitiveType('char'), None)
class StructOrUnionOrEnum(BaseTypeByIdentity):
    forcename = None
    def build_c_name_with_marker(self):
        name = self.forcename or '%s %s' % (self.kind, self.name)
    def force_the_name(self, forcename):
        self.forcename = forcename
        self.build_c_name_with_marker()
    def get_official_name(self):
        assert self.c_name_with_marker.endswith('&')
        return self.c_name_with_marker[:-1]
class StructOrUnion(StructOrUnionOrEnum):
    fixedlayout = None
    completed = 0
    partial = False
    packed = 0
    def __init__(self, name, fldnames, fldtypes, fldbitsize, fldquals=None):
        self.fldnames = fldnames
        self.fldtypes = fldtypes
        self.fldbitsize = fldbitsize
        self.fldquals = fldquals
    def anonymous_struct_fields(self):
        if self.fldtypes is not None:
            for name, type in zip(self.fldnames, self.fldtypes):
                if name == '' and isinstance(type, StructOrUnion):
                    yield type
    def enumfields(self, expand_anonymous_struct_union=True):
        fldquals = self.fldquals
        if fldquals is None:
            fldquals = (0,) * len(self.fldnames)
        for name, type, bitsize, quals in zip(self.fldnames, self.fldtypes,
                                              self.fldbitsize, fldquals):
            if (name == '' and isinstance(type, StructOrUnion)
                    and expand_anonymous_struct_union):
                # nested anonymous struct/union
                for result in type.enumfields():
                yield (name, type, bitsize, quals)
    def force_flatten(self):
        # force the struct or union to have a declaration that lists
        # directly all fields returned by enumfields(), flattening
        # nested anonymous structs/unions.
        types = []
        bitsizes = []
        fldquals = []
        for name, type, bitsize, quals in self.enumfields():
            types.append(type)
            bitsizes.append(bitsize)
            fldquals.append(quals)
        self.fldnames = tuple(names)
        self.fldtypes = tuple(types)
        self.fldbitsize = tuple(bitsizes)
        self.fldquals = tuple(fldquals)
        BType = StructOrUnionOrEnum.get_cached_btype(self, ffi, finishlist,
                                                     can_delay)
        if not can_delay:
            self.finish_backend_type(ffi, finishlist)
    def finish_backend_type(self, ffi, finishlist):
        if self.completed:
            if self.completed != 2:
                raise NotImplementedError("recursive structure declaration "
                                          "for '%s'" % (self.name,))
        self.completed = 1
        if self.fldtypes is None:
            pass    # not completing it: it's an opaque struct
        elif self.fixedlayout is None:
            fldtypes = [tp.get_cached_btype(ffi, finishlist)
                        for tp in self.fldtypes]
            lst = list(zip(self.fldnames, fldtypes, self.fldbitsize))
            extra_flags = ()
            if self.packed:
                if self.packed == 1:
                    extra_flags = (8,)    # SF_PACKED
                    extra_flags = (0, self.packed)
            ffi._backend.complete_struct_or_union(BType, lst, self,
                                                  -1, -1, *extra_flags)
            fldtypes = []
            fieldofs, fieldsize, totalsize, totalalignment = self.fixedlayout
            for i in range(len(self.fldnames)):
                fsize = fieldsize[i]
                ftype = self.fldtypes[i]
                if isinstance(ftype, ArrayType) and ftype.length_is_unknown():
                    # fix the length to match the total size
                    BItemType = ftype.item.get_cached_btype(ffi, finishlist)
                    nlen, nrest = divmod(fsize, ffi.sizeof(BItemType))
                    if nrest != 0:
                        self._verification_error(
                            "field '%s.%s' has a bogus size?" % (
                            self.name, self.fldnames[i] or '{}'))
                    ftype = ftype.resolve_length(nlen)
                    self.fldtypes = (self.fldtypes[:i] + (ftype,) +
                                     self.fldtypes[i+1:])
                BFieldType = ftype.get_cached_btype(ffi, finishlist)
                if isinstance(ftype, ArrayType) and ftype.length is None:
                    assert fsize == 0
                    bitemsize = ffi.sizeof(BFieldType)
                    if bitemsize != fsize:
                            "field '%s.%s' is declared as %d bytes, but is "
                            "really %d bytes" % (self.name,
                                                 self.fldnames[i] or '{}',
                                                 bitemsize, fsize))
                fldtypes.append(BFieldType)
            lst = list(zip(self.fldnames, fldtypes, self.fldbitsize, fieldofs))
                                                  totalsize, totalalignment)
        self.completed = 2
    def _verification_error(self, msg):
        raise VerificationError(msg)
    def check_not_partial(self):
        if self.partial and self.fixedlayout is None:
            raise VerificationMissing(self._get_c_name())
        self.check_not_partial()
        finishlist.append(self)
        return global_cache(self, ffi, 'new_%s_type' % self.kind,
                            self.get_official_name(), key=self)
class StructType(StructOrUnion):
    kind = 'struct'
class UnionType(StructOrUnion):
    kind = 'union'
class EnumType(StructOrUnionOrEnum):
    kind = 'enum'
    partial_resolved = False
    def __init__(self, name, enumerators, enumvalues, baseinttype=None):
        self.enumerators = enumerators
        self.enumvalues = enumvalues
        self.baseinttype = baseinttype
        StructOrUnionOrEnum.force_the_name(self, forcename)
        if self.forcename is None:
            name = self.get_official_name()
            self.forcename = '$' + name.replace(' ', '_')
        if self.partial and not self.partial_resolved:
        base_btype = self.build_baseinttype(ffi, finishlist)
        return global_cache(self, ffi, 'new_enum_type',
                            self.get_official_name(),
                            self.enumerators, self.enumvalues,
                            base_btype, key=self)
    def build_baseinttype(self, ffi, finishlist):
        if self.baseinttype is not None:
            return self.baseinttype.get_cached_btype(ffi, finishlist)
        if self.enumvalues:
            smallest_value = min(self.enumvalues)
            largest_value = max(self.enumvalues)
                # XXX!  The goal is to ensure that the warnings.warn()
                # will not suppress the warning.  We want to get it
                # several times if we reach this point several times.
                __warningregistry__.clear()
            warnings.warn("%r has no values explicitly defined; "
                          "guessing that it is equivalent to 'unsigned int'"
                          % self._get_c_name())
            smallest_value = largest_value = 0
        if smallest_value < 0:   # needs a signed type
            sign = 1
            candidate1 = PrimitiveType("int")
            candidate2 = PrimitiveType("long")
            sign = 0
            candidate1 = PrimitiveType("unsigned int")
            candidate2 = PrimitiveType("unsigned long")
        btype1 = candidate1.get_cached_btype(ffi, finishlist)
        btype2 = candidate2.get_cached_btype(ffi, finishlist)
        size1 = ffi.sizeof(btype1)
        size2 = ffi.sizeof(btype2)
        if (smallest_value >= ((-1) << (8*size1-1)) and
            largest_value < (1 << (8*size1-sign))):
            return btype1
        if (smallest_value >= ((-1) << (8*size2-1)) and
            largest_value < (1 << (8*size2-sign))):
            return btype2
        raise CDefError("%s values don't all fit into either 'long' "
                        "or 'unsigned long'" % self._get_c_name())
def unknown_type(name, structname=None):
    if structname is None:
        structname = '$%s' % name
    tp = StructType(structname, None, None, None)
    tp.force_the_name(name)
    tp.origin = "unknown_type"
    return tp
def unknown_ptr_type(name, structname=None):
        structname = '$$%s' % name
    return NamedPointerType(tp, name)
global_lock = allocate_lock()
_typecache_cffi_backend = weakref.WeakValueDictionary()
def get_typecache(backend):
    # returns _typecache_cffi_backend if backend is the _cffi_backend
    # module, or type(backend).__typecache if backend is an instance of
    # CTypesBackend (or some FakeBackend class during tests)
        return _typecache_cffi_backend
    with global_lock:
        if not hasattr(type(backend), '__typecache'):
            type(backend).__typecache = weakref.WeakValueDictionary()
        return type(backend).__typecache
def global_cache(srctype, ffi, funcname, *args, **kwds):
    key = kwds.pop('key', (funcname, args))
    assert not kwds
        return ffi._typecache[key]
        res = getattr(ffi._backend, funcname)(*args)
    except NotImplementedError as e:
        raise NotImplementedError("%s: %r: %s" % (funcname, srctype, e))
    # note that setdefault() on WeakValueDictionary is not atomic
    # and contains a rare bug (http://bugs.python.org/issue19542);
    # we have to use a lock and do it ourselves
    cache = ffi._typecache
        res1 = cache.get(key)
        if res1 is None:
            cache[key] = res
            return res1
def pointer_cache(ffi, BType):
    return global_cache('?', ffi, 'new_pointer_type', BType)
def attach_exception_info(e, name):
    if e.args and type(e.args[0]) is str:
        e.args = ('%s: %s' % (name, e.args[0]),) + e.args[1:]
from .core import Encoding
from .registry import get_encoding
# TODO: these will likely be replaced by an API endpoint
MODEL_PREFIX_TO_ENCODING: dict[str, str] = {
    "o1-": "o200k_base",
    "o3-": "o200k_base",
    "o4-mini-": "o200k_base",
    # chat
    "gpt-5-": "o200k_base",
    "gpt-4.5-": "o200k_base",
    "gpt-4.1-": "o200k_base",
    "chatgpt-4o-": "o200k_base",
    "gpt-4o-": "o200k_base",  # e.g., gpt-4o-2024-05-13
    "gpt-4-": "cl100k_base",  # e.g., gpt-4-0314, etc., plus gpt-4-32k
    "gpt-3.5-turbo-": "cl100k_base",  # e.g, gpt-3.5-turbo-0301, -0401, etc.
    "gpt-35-turbo-": "cl100k_base",  # Azure deployment name
    "gpt-oss-": "o200k_harmony",
    # fine-tuned
    "ft:gpt-4o": "o200k_base",
    "ft:gpt-4": "cl100k_base",
    "ft:gpt-3.5-turbo": "cl100k_base",
    "ft:davinci-002": "cl100k_base",
    "ft:babbage-002": "cl100k_base",
MODEL_TO_ENCODING: dict[str, str] = {
    # reasoning
    "o1": "o200k_base",
    "o3": "o200k_base",
    "o4-mini": "o200k_base",
    "gpt-5": "o200k_base",
    "gpt-4.1": "o200k_base",
    "gpt-4o": "o200k_base",
    "gpt-4": "cl100k_base",
    "gpt-3.5-turbo": "cl100k_base",
    "gpt-3.5": "cl100k_base",  # Common shorthand
    "gpt-35-turbo": "cl100k_base",  # Azure deployment name
    # base
    "davinci-002": "cl100k_base",
    "babbage-002": "cl100k_base",
    # embeddings
    "text-embedding-ada-002": "cl100k_base",
    "text-embedding-3-small": "cl100k_base",
    "text-embedding-3-large": "cl100k_base",
    # DEPRECATED MODELS
    # text (DEPRECATED)
    "text-davinci-003": "p50k_base",
    "text-davinci-002": "p50k_base",
    "text-davinci-001": "r50k_base",
    "text-curie-001": "r50k_base",
    "text-babbage-001": "r50k_base",
    "text-ada-001": "r50k_base",
    "davinci": "r50k_base",
    "curie": "r50k_base",
    "babbage": "r50k_base",
    "ada": "r50k_base",
    # code (DEPRECATED)
    "code-davinci-002": "p50k_base",
    "code-davinci-001": "p50k_base",
    "code-cushman-002": "p50k_base",
    "code-cushman-001": "p50k_base",
    "davinci-codex": "p50k_base",
    "cushman-codex": "p50k_base",
    # edit (DEPRECATED)
    "text-davinci-edit-001": "p50k_edit",
    "code-davinci-edit-001": "p50k_edit",
    # old embeddings (DEPRECATED)
    "text-similarity-davinci-001": "r50k_base",
    "text-similarity-curie-001": "r50k_base",
    "text-similarity-babbage-001": "r50k_base",
    "text-similarity-ada-001": "r50k_base",
    "text-search-davinci-doc-001": "r50k_base",
    "text-search-curie-doc-001": "r50k_base",
    "text-search-babbage-doc-001": "r50k_base",
    "text-search-ada-doc-001": "r50k_base",
    "code-search-babbage-code-001": "r50k_base",
    "code-search-ada-code-001": "r50k_base",
    # open source
    "gpt2": "gpt2",
    "gpt-2": "gpt2",  # Maintains consistency with gpt-4
def encoding_name_for_model(model_name: str) -> str:
    """Returns the name of the encoding used by a model.
    Raises a KeyError if the model name is not recognised.
    encoding_name = None
    if model_name in MODEL_TO_ENCODING:
        encoding_name = MODEL_TO_ENCODING[model_name]
        # Check if the model matches a known prefix
        # Prefix matching avoids needing library updates for every model version release
        # Note that this can match on non-existent models (e.g., gpt-3.5-turbo-FAKE)
        for model_prefix, model_encoding_name in MODEL_PREFIX_TO_ENCODING.items():
            if model_name.startswith(model_prefix):
                return model_encoding_name
    if encoding_name is None:
            f"Could not automatically map {model_name} to a tokeniser. "
            "Please use `tiktoken.get_encoding` to explicitly get the tokeniser you expect."
    return encoding_name
def encoding_for_model(model_name: str) -> Encoding:
    """Returns the encoding used by a model.
    return get_encoding(encoding_name_for_model(model_name))
