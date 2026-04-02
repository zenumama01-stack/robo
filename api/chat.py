from .completions.completions import (
__all__ = ["Chat", "AsyncChat"]
class Chat(SyncAPIResource):
        Given a list of messages comprising a conversation, the model will return a response.
        return Completions(self._client)
    def with_raw_response(self) -> ChatWithRawResponse:
        return ChatWithRawResponse(self)
    def with_streaming_response(self) -> ChatWithStreamingResponse:
        return ChatWithStreamingResponse(self)
class AsyncChat(AsyncAPIResource):
        return AsyncCompletions(self._client)
    def with_raw_response(self) -> AsyncChatWithRawResponse:
        return AsyncChatWithRawResponse(self)
    def with_streaming_response(self) -> AsyncChatWithStreamingResponse:
        return AsyncChatWithStreamingResponse(self)
class ChatWithRawResponse:
    def __init__(self, chat: Chat) -> None:
        self._chat = chat
    def completions(self) -> CompletionsWithRawResponse:
        return CompletionsWithRawResponse(self._chat.completions)
class AsyncChatWithRawResponse:
    def __init__(self, chat: AsyncChat) -> None:
    def completions(self) -> AsyncCompletionsWithRawResponse:
        return AsyncCompletionsWithRawResponse(self._chat.completions)
class ChatWithStreamingResponse:
    def completions(self) -> CompletionsWithStreamingResponse:
        return CompletionsWithStreamingResponse(self._chat.completions)
class AsyncChatWithStreamingResponse:
    def completions(self) -> AsyncCompletionsWithStreamingResponse:
        return AsyncCompletionsWithStreamingResponse(self._chat.completions)
"""Chat Message."""
from langchain_core.utils._merge import merge_dicts
class ChatMessage(BaseMessage):
    """Message that can be assigned an arbitrary speaker (i.e. role)."""
    """The speaker / role of the Message."""
    type: Literal["chat"] = "chat"
    """The type of the message (used during serialization)."""
class ChatMessageChunk(ChatMessage, BaseMessageChunk):
    """Chat Message chunk."""
    # Ignoring mypy re-assignment here since we're overriding the value
    # to make sure that the chunk variant can be discriminated from the
    # non-chunk variant.
    type: Literal["ChatMessageChunk"] = "ChatMessageChunk"  # type: ignore[assignment]
        if isinstance(other, ChatMessageChunk):
            if self.role != other.role:
                msg = "Cannot concatenate ChatMessageChunks with different roles."
                role=self.role,
        return super().__add__(other)
"""Chat prompt template."""
    PositiveInt,
    SkipValidation,
    model_validator,
from langchain_core.messages.base import get_msg_title_repr
from langchain_core.prompt_values import ChatPromptValue, ImageURL
from langchain_core.prompts.base import BasePromptTemplate
from langchain_core.prompts.image import ImagePromptTemplate
from langchain_core.prompts.message import (
    BaseMessagePromptTemplate,
    PromptTemplateFormat,
from langchain_core.utils import get_colored_text
class MessagesPlaceholder(BaseMessagePromptTemplate):
    """Prompt template that assumes variable is already list of messages.
    A placeholder which can be used to pass in a list of messages.
    !!! example "Direct usage"
        from langchain_core.prompts import MessagesPlaceholder
        prompt = MessagesPlaceholder("history")
        prompt.format_messages()  # raises KeyError
        prompt = MessagesPlaceholder("history", optional=True)
        prompt.format_messages()  # returns empty list []
        prompt.format_messages(
            history=[
                ("system", "You are an AI assistant."),
                ("human", "Hello!"),
        #     SystemMessage(content="You are an AI assistant."),
        #     HumanMessage(content="Hello!"),
    !!! example "Building a prompt with chat history"
        from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
        prompt = ChatPromptTemplate.from_messages(
                ("system", "You are a helpful assistant."),
                MessagesPlaceholder("history"),
                ("human", "{question}"),
        prompt.invoke(
                "history": [("human", "what's 5 + 2"), ("ai", "5 + 2 is 7")],
                "question": "now multiply that by 4",
        # -> ChatPromptValue(messages=[
        #     SystemMessage(content="You are a helpful assistant."),
        #     HumanMessage(content="what's 5 + 2"),
        #     AIMessage(content="5 + 2 is 7"),
        #     HumanMessage(content="now multiply that by 4"),
        # ])
    !!! example "Limiting the number of messages"
        prompt = MessagesPlaceholder("history", n_messages=1)
    variable_name: str
    """Name of variable to use as messages."""
    optional: bool = False
    """Whether `format_messages` must be provided.
    If `True` `format_messages` can be called with no arguments and will return an empty
    If `False` then a named argument with name `variable_name` must be passed in, even
    if the value is an empty list.
    n_messages: PositiveInt | None = None
    """Maximum number of messages to include.
    If `None`, then will include all.
        self, variable_name: str, *, optional: bool = False, **kwargs: Any
        """Create a messages placeholder.
            variable_name: Name of variable to use as messages.
            optional: Whether `format_messages` must be provided.
                If `True` format_messages can be called with no arguments and will
                return an empty list.
                If `False` then a named argument with name `variable_name` must be
                passed in, even if the value is an empty list.
        # mypy can't detect the init which is defined in the parent class
        # b/c these are BaseModel classes.
        super().__init__(variable_name=variable_name, optional=optional, **kwargs)  # type: ignore[call-arg,unused-ignore]
    def format_messages(self, **kwargs: Any) -> list[BaseMessage]:
        """Format messages from kwargs.
            **kwargs: Keyword arguments to use for formatting.
            List of `BaseMessage` objects.
            ValueError: If variable is not a list of messages.
            kwargs.get(self.variable_name, [])
            if self.optional
            else kwargs[self.variable_name]
        if not isinstance(value, list):
                f"variable {self.variable_name} should be a list of base messages, "
                f"got {value} of type {type(value)}"
        value = convert_to_messages(value)
        if self.n_messages:
            value = value[-self.n_messages :]
    def input_variables(self) -> list[str]:
        """Input variables for this prompt template.
            List of input variable names.
        return [self.variable_name] if not self.optional else []
    def pretty_repr(self, html: bool = False) -> str:
        """Human-readable representation.
            html: Whether to format as HTML.
            Human-readable representation.
        var = "{" + self.variable_name + "}"
            title = get_msg_title_repr("Messages Placeholder", bold=True)
            var = get_colored_text(var, "yellow")
            title = get_msg_title_repr("Messages Placeholder")
        return f"{title}\n\n{var}"
MessagePromptTemplateT = TypeVar(
    "MessagePromptTemplateT", bound="BaseStringMessagePromptTemplate"
"""Type variable for message prompt templates."""
class BaseStringMessagePromptTemplate(BaseMessagePromptTemplate, ABC):
    """Base class for message prompt templates that use a string prompt template."""
    prompt: StringPromptTemplate
    """String prompt template."""
    """Additional keyword arguments to pass to the prompt template."""
    def from_template(
        template_format: PromptTemplateFormat = "f-string",
        partial_variables: dict[str, Any] | None = None,
        """Create a class from a string template.
            template: a template.
            template_format: format of the template.
            partial_variables: A dictionary of variables that can be used to partially
                fill in the template.
                For example, if the template is `"{variable1} {variable2}"`, and
                `partial_variables` is `{"variable1": "foo"}`, then the final prompt
                will be `"foo {variable2}"`.
            **kwargs: Keyword arguments to pass to the constructor.
            A new instance of this class.
        prompt = PromptTemplate.from_template(
            template_format=template_format,
            partial_variables=partial_variables,
        return cls(prompt=prompt, **kwargs)
    def from_template_file(
        template_file: str | Path,
        """Create a class from a template file.
            template_file: path to a template file.
        prompt = PromptTemplate.from_file(template_file)
    def format(self, **kwargs: Any) -> BaseMessage:
        """Format the prompt template.
            Formatted message.
    async def aformat(self, **kwargs: Any) -> BaseMessage:
        """Async format the prompt template.
        return [self.format(**kwargs)]
    async def aformat_messages(self, **kwargs: Any) -> list[BaseMessage]:
        """Async format messages from kwargs.
        return [await self.aformat(**kwargs)]
        return self.prompt.input_variables
        # TODO: Handle partials
        title = self.__class__.__name__.replace("MessagePromptTemplate", " Message")
        title = get_msg_title_repr(title, bold=html)
        return f"{title}\n\n{self.prompt.pretty_repr(html=html)}"
class ChatMessagePromptTemplate(BaseStringMessagePromptTemplate):
    """Chat message prompt template."""
    """Role of the message."""
        text = self.prompt.format(**kwargs)
        return ChatMessage(
            content=text, role=self.role, additional_kwargs=self.additional_kwargs
        text = await self.prompt.aformat(**kwargs)
class _TextTemplateParam(TypedDict, total=False):
    text: str | dict
class _ImageTemplateParam(TypedDict, total=False):
    image_url: str | dict
class _StringImageMessagePromptTemplate(BaseMessagePromptTemplate):
    """Human message prompt template. This is a message sent from the user."""
    prompt: (
        StringPromptTemplate
        | list[StringPromptTemplate | ImagePromptTemplate | DictPromptTemplate]
    """Prompt template."""
    _msg_class: type[BaseMessage]
        cls: type[Self],
        template: str
        | list[str | _TextTemplateParam | _ImageTemplateParam | dict[str, Any]],
                Options are: `'f-string'`, `'mustache'`, `'jinja2'`.
            partial_variables: A dictionary of variables that can be used too partially.
            ValueError: If the template is not a string or list of strings.
        if isinstance(template, str):
            prompt: StringPromptTemplate | list = PromptTemplate.from_template(
        if isinstance(template, list):
            if (partial_variables is not None) and len(partial_variables) > 0:
                msg = "Partial variables are not supported for list of templates."
            prompt = []
            for tmpl in template:
                if isinstance(tmpl, str) or (
                    isinstance(tmpl, dict)
                    and "text" in tmpl
                    and set(tmpl.keys()) <= {"type", "text"}
                    if isinstance(tmpl, str):
                        text: str = tmpl
                        text = cast("_TextTemplateParam", tmpl)["text"]  # type: ignore[assignment]
                    prompt.append(
                        PromptTemplate.from_template(
                            text, template_format=template_format
                    and "image_url" in tmpl
                    and set(tmpl.keys())
                    <= {
                        "image_url",
                    img_template = cast("_ImageTemplateParam", tmpl)["image_url"]
                    input_variables = []
                    if isinstance(img_template, str):
                        variables = get_template_variables(
                            img_template, template_format
                        if variables:
                            if len(variables) > 1:
                                    "Only one format variable allowed per image"
                                    f" template.\nGot: {variables}"
                                    f"\nFrom: {tmpl}"
                            input_variables = [variables[0]]
                        img_template = {"url": img_template}
                        img_template_obj = ImagePromptTemplate(
                            input_variables=input_variables,
                            template=img_template,
                    elif isinstance(img_template, dict):
                        img_template = dict(img_template)
                        for key in ["url", "path", "detail"]:
                            if key in img_template:
                                input_variables.extend(
                                    get_template_variables(
                                        img_template[key], template_format
                        msg = f"Invalid image template: {tmpl}"
                    prompt.append(img_template_obj)
                elif isinstance(tmpl, dict):
                    if template_format == "jinja2":
                            "jinja2 is unsafe and is not supported for templates "
                            "expressed as dicts. Please use 'f-string' or 'mustache' "
                            "format."
                    data_template_obj = DictPromptTemplate(
                        template=cast("dict[str, Any]", tmpl),
                    prompt.append(data_template_obj)
                    msg = f"Invalid template: {tmpl}"
        msg = f"Invalid template: {template}"
        input_variables: list[str],
            input_variables: list of input variables.
        template = Path(template_file).read_text(encoding="utf-8")
        return cls.from_template(template, input_variables=input_variables, **kwargs)
        prompts = self.prompt if isinstance(self.prompt, list) else [self.prompt]
        return [iv for prompt in prompts for iv in prompt.input_variables]
        if isinstance(self.prompt, StringPromptTemplate):
            return self._msg_class(
                content=text, additional_kwargs=self.additional_kwargs
        content: list = []
        for prompt in self.prompt:
            inputs = {var: kwargs[var] for var in prompt.input_variables}
            if isinstance(prompt, StringPromptTemplate):
                formatted_text: str = prompt.format(**inputs)
                if formatted_text != "":
                    content.append({"type": "text", "text": formatted_text})
            elif isinstance(prompt, ImagePromptTemplate):
                formatted_image: ImageURL = prompt.format(**inputs)
                content.append({"type": "image_url", "image_url": formatted_image})
            elif isinstance(prompt, DictPromptTemplate):
                formatted_dict: dict[str, Any] = prompt.format(**inputs)
                content.append(formatted_dict)
            content=content, additional_kwargs=self.additional_kwargs
                formatted_text: str = await prompt.aformat(**inputs)
                formatted_image: ImageURL = await prompt.aformat(**inputs)
        prompt_reprs = "\n\n".join(prompt.pretty_repr(html=html) for prompt in prompts)
        return f"{title}\n\n{prompt_reprs}"
class HumanMessagePromptTemplate(_StringImageMessagePromptTemplate):
    """Human message prompt template.
    This is a message sent from the user.
    _msg_class: type[BaseMessage] = HumanMessage
class AIMessagePromptTemplate(_StringImageMessagePromptTemplate):
    """AI message prompt template.
    This is a message sent from the AI.
    _msg_class: type[BaseMessage] = AIMessage
class SystemMessagePromptTemplate(_StringImageMessagePromptTemplate):
    """System message prompt template.
    This is a message that is not sent to the user.
    _msg_class: type[BaseMessage] = SystemMessage
class BaseChatPromptTemplate(BasePromptTemplate, ABC):
    """Base class for chat prompt templates."""
    def lc_attributes(self) -> dict:
        return {"input_variables": self.input_variables}
    def format(self, **kwargs: Any) -> str:
        """Format the chat template into a string.
            **kwargs: Keyword arguments to use for filling in template variables in all
                the template messages in this chat template.
            Formatted string.
        return self.format_prompt(**kwargs).to_string()
    async def aformat(self, **kwargs: Any) -> str:
        """Async format the chat template into a string.
        return (await self.aformat_prompt(**kwargs)).to_string()
    def format_prompt(self, **kwargs: Any) -> ChatPromptValue:
        """Format prompt.
        Should return a `ChatPromptValue`.
        messages = self.format_messages(**kwargs)
        return ChatPromptValue(messages=messages)
    async def aformat_prompt(self, **kwargs: Any) -> ChatPromptValue:
        """Async format prompt.
        messages = await self.aformat_messages(**kwargs)
        """Format kwargs into a list of messages.
        """Async format kwargs into a list of messages.
        return self.format_messages(**kwargs)
        """Print a human-readable representation."""
MessageLike = BaseMessagePromptTemplate | BaseMessage | BaseChatPromptTemplate
    MessageLike
    | tuple[str | type, str | Sequence[dict] | Sequence[object]]
    | str
    | dict[str, Any]
class ChatPromptTemplate(BaseChatPromptTemplate):
    """Prompt template for chat models.
    Use to create flexible templated prompts for chat models.
    !!! example
        from langchain_core.prompts import ChatPromptTemplate
        template = ChatPromptTemplate(
                ("system", "You are a helpful AI bot. Your name is {name}."),
                ("human", "Hello, how are you doing?"),
                ("ai", "I'm doing well, thanks!"),
                ("human", "{user_input}"),
        prompt_value = template.invoke(
                "user_input": "What is your name?",
        # Output:
        # ChatPromptValue(
        #    messages=[
        #        SystemMessage(content='You are a helpful AI bot. Your name is Bob.'),
        #        HumanMessage(content='Hello, how are you doing?'),
        #        AIMessage(content="I'm doing well, thanks!"),
        #        HumanMessage(content='What is your name?')
        #    ]
    !!! note "Messages Placeholder"
        # In addition to Human/AI/Tool/Function messages,
        # you can initialize the template with a MessagesPlaceholder
        # either using the class directly or with the shorthand tuple syntax:
                ("system", "You are a helpful AI bot."),
                # Means the template will receive an optional list of messages under
                # the "conversation" key
                ("placeholder", "{conversation}"),
                # Equivalently:
                # MessagesPlaceholder(variable_name="conversation", optional=True)
                "conversation": [
                    ("human", "Hi!"),
                    ("ai", "How can I assist you today?"),
                    ("human", "Can you make me an ice cream sundae?"),
                    ("ai", "No."),
        #        SystemMessage(content='You are a helpful AI bot.'),
        #        HumanMessage(content='Hi!'),
        #        AIMessage(content='How can I assist you today?'),
        #        HumanMessage(content='Can you make me an ice cream sundae?'),
        #        AIMessage(content='No.'),
    !!! note "Single-variable template"
        If your prompt has only a single input variable (i.e., one instance of
        `'{variable_nams}'`), and you invoke the template with a non-dict object, the
        prompt template will inject the provided argument into that variable location.
                ("system", "You are a helpful AI bot. Your name is Carl."),
        prompt_value = template.invoke("Hello, there!")
        # Equivalent to
        # prompt_value = template.invoke({"user_input": "Hello, there!"})
        #  ChatPromptValue(
        #     messages=[
        #         SystemMessage(content='You are a helpful AI bot. Your name is Carl.'),
        #         HumanMessage(content='Hello, there!'),
        #     ]
    messages: Annotated[list[MessageLike], SkipValidation()]
    """List of messages consisting of either message prompt templates or messages."""
    validate_template: bool = False
    """Whether or not to try validating the template."""
        messages: Sequence[MessageLikeRepresentation],
        """Create a chat prompt template from a variety of message formats.
            messages: Sequence of message representations.
                A message can be represented using the following formats:
                1. `BaseMessagePromptTemplate`
                2. `BaseMessage`
                3. 2-tuple of `(message type, template)`; e.g.,
                    `('human', '{user_input}')`
                4. 2-tuple of `(message class, template)`
                5. A string which is shorthand for `('human', template)`; e.g.,
                    `'{user_input}'`
            template_format: Format of the template.
            **kwargs: Additional keyword arguments passed to `BasePromptTemplate`,
                including (but not limited to):
                - `input_variables`: A list of the names of the variables whose values
                    are required as inputs to the prompt.
                - `optional_variables`: A list of the names of the variables for
                    placeholder or `MessagePlaceholder` that are optional.
                    These variables are auto inferred from the prompt and user need not
                    provide them.
                - `partial_variables`: A dictionary of the partial variables the prompt
                    template carries.
                    Partial variables populate the template so that you don't need to
                    pass them in every time you call the prompt.
                - `validate_template`: Whether to validate the template.
                - `input_types`: A dictionary of the types of the variables the prompt
                    template expects.
            Instantiation from a list of message templates:
                    ("human", "Hello, how are you?"),
                    ("human", "That's good to hear."),
            Instantiation from mixed message formats:
                    SystemMessage(content="hello"),
        messages_ = [
            _convert_to_message_template(message, template_format)
            for message in messages
        # Automatically infer input variables from messages
        input_vars: set[str] = set()
        optional_variables: set[str] = set()
        partial_vars: dict[str, Any] = {}
        for message in messages_:
            if isinstance(message, MessagesPlaceholder) and message.optional:
                partial_vars[message.variable_name] = []
                optional_variables.add(message.variable_name)
            elif isinstance(
                message, (BaseChatPromptTemplate, BaseMessagePromptTemplate)
                input_vars.update(message.input_variables)
            "input_variables": sorted(input_vars),
            "optional_variables": sorted(optional_variables),
            "partial_variables": partial_vars,
        cast("type[ChatPromptTemplate]", super()).__init__(messages=messages_, **kwargs)
            `["langchain", "prompts", "chat"]`
        return ["langchain", "prompts", "chat"]
        """Combine two prompt templates.
            other: Another prompt template.
            Combined prompt template.
        partials = {**self.partial_variables}
        # Need to check that other has partial variables since it may not be
        # a ChatPromptTemplate.
        if hasattr(other, "partial_variables") and other.partial_variables:
            partials.update(other.partial_variables)
        # Allow for easy combining
        if isinstance(other, ChatPromptTemplate):
            return ChatPromptTemplate(messages=self.messages + other.messages).partial(
                **partials
            other, (BaseMessagePromptTemplate, BaseMessage, BaseChatPromptTemplate)
            return ChatPromptTemplate(messages=[*self.messages, other]).partial(
        if isinstance(other, (list, tuple)):
            other_ = ChatPromptTemplate.from_messages(other)
            return ChatPromptTemplate(messages=self.messages + other_.messages).partial(
        if isinstance(other, str):
            prompt = HumanMessagePromptTemplate.from_template(other)
            return ChatPromptTemplate(messages=[*self.messages, prompt]).partial(
        msg = f"Unsupported operand type for +: {type(other)}"
    def validate_input_variables(cls, values: dict) -> Any:
        """Validate input variables.
        If `input_variables` is not set, it will be set to the union of all input
        variables in the messages.
            values: values to validate.
            Validated values.
            ValueError: If input variables do not match.
        messages = values["messages"]
        input_vars: set = set()
        optional_variables = set()
        input_types: dict[str, Any] = values.get("input_types", {})
            if isinstance(message, (BaseMessagePromptTemplate, BaseChatPromptTemplate)):
            if isinstance(message, MessagesPlaceholder):
                if "partial_variables" not in values:
                    values["partial_variables"] = {}
                    message.optional
                    and message.variable_name not in values["partial_variables"]
                    values["partial_variables"][message.variable_name] = []
                if message.variable_name not in input_types:
                    input_types[message.variable_name] = list[AnyMessage]
        if "partial_variables" in values:
            input_vars -= set(values["partial_variables"])
        if optional_variables:
            input_vars -= optional_variables
        if "input_variables" in values and values.get("validate_template"):
            if input_vars != set(values["input_variables"]):
                    "Got mismatched input_variables. "
                    f"Expected: {input_vars}. "
                    f"Got: {values['input_variables']}"
            values["input_variables"] = sorted(input_vars)
            values["optional_variables"] = sorted(optional_variables)
        values["input_types"] = input_types
    def from_template(cls, template: str, **kwargs: Any) -> ChatPromptTemplate:
        """Create a chat prompt template from a template string.
        Creates a chat template consisting of a single message assumed to be from the
        human.
            template: Template string
        prompt_template = PromptTemplate.from_template(template, **kwargs)
        message = HumanMessagePromptTemplate(prompt=prompt_template)
        return cls.from_messages([message])
    def from_messages(
    ) -> ChatPromptTemplate:
            template = ChatPromptTemplate.from_messages(
            A chat prompt template.
        return cls(messages, template_format=template_format)
        """Format the chat template into a list of finalized messages.
            **kwargs: Keyword arguments to use for filling in template variables
                in all the template messages in this chat template.
            ValueError: If messages are of unexpected types.
            List of formatted messages.
        kwargs = self._merge_partial_and_user_variables(**kwargs)
        for message_template in self.messages:
            if isinstance(message_template, BaseMessage):
                result.extend([message_template])
                message_template, (BaseMessagePromptTemplate, BaseChatPromptTemplate)
                message = message_template.format_messages(**kwargs)
                result.extend(message)
                msg = f"Unexpected input: {message_template}"
        """Async format the chat template into a list of finalized messages.
            ValueError: If unexpected input.
                message = await message_template.aformat_messages(**kwargs)
                raise ValueError(msg)  # noqa:TRY004
    def partial(self, **kwargs: Any) -> ChatPromptTemplate:
        """Get a new `ChatPromptTemplate` with some input variables already filled in.
            **kwargs: Keyword arguments to use for filling in template variables.
                Ought to be a subset of the input variables.
            A new `ChatPromptTemplate`.
                    ("system", "You are an AI assistant named {name}."),
                    ("human", "Hi I'm {user}"),
                    ("ai", "Hi there, {user}, I'm {name}."),
                    ("human", "{input}"),
            template2 = template.partial(user="Lucy", name="R2D2")
            template2.format_messages(input="hello")
    def append(self, message: MessageLikeRepresentation) -> None:
        """Append a message to the end of the chat template.
            message: representation of a message to append.
        self.messages.append(_convert_to_message_template(message))
    def extend(self, messages: Sequence[MessageLikeRepresentation]) -> None:
        """Extend the chat template with a sequence of messages.
            messages: Sequence of message representations to append.
        self.messages.extend(
            [_convert_to_message_template(message) for message in messages]
    def __getitem__(self, index: int) -> MessageLike: ...
    def __getitem__(self, index: slice) -> ChatPromptTemplate: ...
    def __getitem__(self, index: int | slice) -> MessageLike | ChatPromptTemplate:
        """Use to index into the chat template.
            If index is an int, returns the message at that index.
            If index is a slice, returns a new `ChatPromptTemplate` containing the
                messages in that slice.
        if isinstance(index, slice):
            start, stop, step = index.indices(len(self.messages))
            messages = self.messages[start:stop:step]
            return ChatPromptTemplate.from_messages(messages)
        return self.messages[index]
        """Return the length of the chat template."""
        return len(self.messages)
        """Name of prompt type. Used for serialization."""
        return "chat"
        """Save prompt to file.
            file_path: path to file.
        # TODO: handle partials
        return "\n\n".join(msg.pretty_repr(html=html) for msg in self.messages)
def _create_template_from_message_type(
    template: str | list,
) -> BaseMessagePromptTemplate:
    """Create a message prompt template from a message type and template string.
        message_type: The type of the message template (e.g., `'human'`, `'ai'`, etc.)
        template: The template string.
        A message prompt template of the appropriate type.
        ValueError: If unexpected message type.
        message: BaseMessagePromptTemplate = HumanMessagePromptTemplate.from_template(
            template, template_format=template_format
        message = AIMessagePromptTemplate.from_template(
            cast("str", template), template_format=template_format
    elif message_type == "system":
        message = SystemMessagePromptTemplate.from_template(
    elif message_type == "placeholder":
            if template[0] != "{" or template[-1] != "}":
                    f"Invalid placeholder template: {template}."
                    " Expected a variable name surrounded by curly braces."
            var_name = template[1:-1]
            message = MessagesPlaceholder(variable_name=var_name, optional=True)
                var_name_wrapped, is_optional = template
                    "Unexpected arguments for placeholder message type."
                    " Expected either a single string variable name"
                    " or a list of [variable_name: str, is_optional: bool]."
                    f" Got: {template}"
            if not isinstance(is_optional, bool):
                msg = f"Expected is_optional to be a boolean. Got: {is_optional}"
            if not isinstance(var_name_wrapped, str):
                msg = f"Expected variable name to be a string. Got: {var_name_wrapped}"
            if var_name_wrapped[0] != "{" or var_name_wrapped[-1] != "}":
                    f"Invalid placeholder template: {var_name_wrapped}."
            var_name = var_name_wrapped[1:-1]
            message = MessagesPlaceholder(variable_name=var_name, optional=is_optional)
            f"Unexpected message type: {message_type}. Use one of 'human',"
            f" 'user', 'ai', 'assistant', or 'system'."
def _convert_to_message_template(
    message: MessageLikeRepresentation,
) -> BaseMessage | BaseMessagePromptTemplate | BaseChatPromptTemplate:
    """Instantiate a message from a variety of message formats.
    3. 2-tuple of `(message type, template)`; e.g., `('human', '{user_input}')`
    5. A string which is shorthand for `('human', template)`; e.g., `'{user_input}'`
        message: A representation of a message in one of the supported formats.
        ValueError: If 2-tuple does not have 2 elements.
        message_: BaseMessage | BaseMessagePromptTemplate | BaseChatPromptTemplate = (
    elif isinstance(message, BaseMessage):
    elif isinstance(message, str):
        message_ = _create_template_from_message_type(
            "human", message, template_format=template_format
    elif isinstance(message, (tuple, dict)):
            if set(message.keys()) != {"content", "role"}:
                    "Expected dict to have exact keys 'role' and 'content'."
                    f" Got: {message}"
            message_type_str = message["role"]
            template = message["content"]
            if len(message) != 2:  # noqa: PLR2004
                msg = f"Expected 2-tuple of (role, template), got {message}"
        if isinstance(message_type_str, str):
                message_type_str, template, template_format=template_format
            hasattr(message_type_str, "model_fields")
            and "type" in message_type_str.model_fields
            message_type = message_type_str.model_fields["type"].default
                message_type, template, template_format=template_format
            message_ = message_type_str(
                prompt=PromptTemplate.from_template(
# For backwards compat:
_convert_to_message = _convert_to_message_template
from langchain_core.prompt_values import ChatPromptValue, ChatPromptValueConcrete
    BaseStringMessagePromptTemplate,
    MessageLike,
    MessagePromptTemplateT,
    _convert_to_message,
    _create_template_from_message_type,
    "BaseMessagePromptTemplate",
    "BaseStringMessagePromptTemplate",
    "ChatPromptValue",
    "ChatPromptValueConcrete",
    "MessageLike",
    "MessagePromptTemplateT",
    "_convert_to_message",
    "_create_template_from_message_type",
from langchain_core.chat_sessions import ChatSession
from litellm.llms.openai.chat.gpt_transformation import OpenAIGPTConfig
class BasetenConfig(OpenAIGPTConfig):
    Reference: https://inference.baseten.co/v1
    Below are the parameters:
    response_format: Optional[dict] = None
    top_p: Optional[int] = None
    tools: Optional[list] = None
    presence_penalty: Optional[int] = None
    frequency_penalty: Optional[int] = None
    stream_options: Optional[dict] = None
        response_format: Optional[dict] = None,
        stop: Optional[list] = None,
        top_p: Optional[int] = None,
        tool_choice: Optional[str] = None,
        tools: Optional[list] = None,
        presence_penalty: Optional[int] = None,
        frequency_penalty: Optional[int] = None,
        locals_ = locals().copy()
        for key, value in locals_.items():
            if key != "self" and value is not None:
                setattr(self.__class__, key, value)
    def get_config(cls):
        return super().get_config()
    def get_supported_openai_params(self, model: str) -> list:
        Get the supported OpenAI params for the given model
            "max_tokens",
            "seed",
            "tool_choice",
            "stream_options",
    def map_openai_params(
        non_default_params: dict,
        drop_params: bool,
        supported_openai_params = self.get_supported_openai_params(model=model)
            if param == "max_completion_tokens":
                optional_params["max_tokens"] = value
            elif param in supported_openai_params:
                optional_params[param] = value
    def _get_openai_compatible_provider_info(self, api_base: str, api_key: str) -> tuple:
        Get the OpenAI compatible provider info for Baseten
        # Default to Model API
        default_api_base = "https://inference.baseten.co/v1"
        default_api_key = api_key or "BASETEN_API_KEY"
        return default_api_base, default_api_key
    def is_dedicated_deployment(model: str) -> bool:
        Check if the model is a dedicated deployment (8-digit alphanumeric code)
        # Remove 'baseten/' prefix if present
        model_id = model.replace("baseten/", "")
        # Check if it's an 8-digit alphanumeric code
        return bool(re.match(r'^[a-zA-Z0-9]{8}$', model_id))
    def get_api_base_for_model(model: str) -> str:
        Get the appropriate API base URL for the given model
        if BasetenConfig.is_dedicated_deployment(model):
            # Extract the model ID (remove 'baseten/' prefix if present)
            return f"https://model-{model_id}.api.baseten.co/environments/production/sync/v1"
            # Use Model API
            return "https://inference.baseten.co/v1" Cerebras Chat Completions API
this is OpenAI compatible - no translation needed / occurs
from litellm.utils import supports_reasoning
class CerebrasConfig(OpenAIGPTConfig):
    Reference: https://inference-docs.cerebras.ai/api-reference/chat-completions
    reasoning_effort: Optional[str] = None
        stop: Optional[str] = None,
        reasoning_effort: Optional[str] = None,
        supported_params = [
        # Only add reasoning_effort for models that support it
        if supports_reasoning(model=model, custom_llm_provider="cerebras"):
            supported_params.append("reasoning_effort")
        return supported_params
Sambanova Chat Completions API
from typing import Any, Coroutine, List, Literal, Optional, Union, overload
    handle_messages_with_content_list_to_str_conversion,
from litellm.types.llms.openai import AllMessageValues
class SambanovaConfig(OpenAIGPTConfig):
    Reference: https://docs.sambanova.ai/cloud/api-reference/
    temperature: Optional[int] = None
    stop: Optional[Union[str, list]] = None
        from litellm.utils import supports_function_calling
        params = [
            "top_k",
        if supports_function_calling(model, custom_llm_provider="sambanova"):
            params.append("tools")
            params.append("tool_choice")
            params.append("parallel_tool_calls")
        map max_completion_tokens param to max_tokens
    def _transform_messages(
        self, messages: List[AllMessageValues], model: str, is_async: Literal[True]
    ) -> Coroutine[Any, Any, List[AllMessageValues]]:
        is_async: Literal[False] = False,
    ) -> List[AllMessageValues]:
        self, messages: List[AllMessageValues], model: str, is_async: bool = False
    ) -> Union[List[AllMessageValues], Coroutine[Any, Any, List[AllMessageValues]]]:
        Transform messages to handle content list conversion.
        SambaNova API doesn't support content as a list - only string content.
        This converts content lists like [{"type": "text", "text": "..."}] to strings.
        async def _async_transform():
            return handle_messages_with_content_list_to_str_conversion(messages)
            return _async_transform()
        messages = handle_messages_with_content_list_to_str_conversion(messages)
Support for OpenAI's `/v1/chat/completions` endpoint. 
Calls done in OpenAI/openai.py as TogetherAI is openai-compatible.
Docs: https://docs.together.ai/reference/completions-1
from ..openai.chat.gpt_transformation import OpenAIGPTConfig
class TogetherAIConfig(OpenAIGPTConfig):
        Only some together models support response_format / tool calling
        Docs: https://docs.together.ai/docs/json-mode
        supports_function_calling: Optional[bool] = None
            model_info = get_model_info(model, custom_llm_provider="together_ai")
            supports_function_calling = model_info.get(
                "supports_function_calling", False
            verbose_logger.debug(f"Error getting supported openai params: {e}")
        optional_params = super().get_supported_openai_params(model)
        if supports_function_calling is not True:
                "Only some together models support function calling/response_format. Docs - https://docs.together.ai/docs/function-calling"
            optional_params.remove("tools")
            optional_params.remove("tool_choice")
            optional_params.remove("function_call")
            optional_params.remove("response_format")
        mapped_openai_params = super().map_openai_params(
            non_default_params, optional_params, model, drop_params
        if "response_format" in mapped_openai_params and mapped_openai_params[
        ] == {"type": "text"}:
            mapped_openai_params.pop("response_format")
        return mapped_openai_params
from typing import Any, Dict, Iterator, List, Optional, Union
class ChatClient:
    def __init__(self, base_url: str, api_key: Optional[str] = None):
        Initialize the ChatClient.
            base_url (str): The base URL of the LiteLLM proxy server (e.g., "http://localhost:8000")
            api_key (Optional[str]): API key for authentication. If provided, it will be sent as a Bearer token.
        self._base_url = base_url.rstrip("/")  # Remove trailing slash if present
    def _get_headers(self) -> Dict[str, str]:
        Get the headers for API requests, including authorization if api_key is set.
            Dict[str, str]: Headers to use for API requests
        if self._api_key:
            headers["Authorization"] = f"Bearer {self._api_key}"
    def completions(
        messages: List[Dict[str, str]],
        return_request: bool = False,
    ) -> Union[Dict[str, Any], requests.Request]:
        Create a chat completion.
            model (str): The model to use for completion
            messages (List[Dict[str, str]]): The messages to generate a completion for
            temperature (Optional[float]): Sampling temperature between 0 and 2
            top_p (Optional[float]): Nucleus sampling parameter between 0 and 1
            n (Optional[int]): Number of completions to generate
            max_tokens (Optional[int]): Maximum number of tokens to generate
            presence_penalty (Optional[float]): Presence penalty between -2.0 and 2.0
            frequency_penalty (Optional[float]): Frequency penalty between -2.0 and 2.0
            user (Optional[str]): Unique identifier for the end user
            return_request (bool): If True, returns the prepared request object instead of executing it
            Union[Dict[str, Any], requests.Request]: Either the completion response from the server or
            a prepared request object if return_request is True
            UnauthorizedError: If the request fails with a 401 status code
            requests.exceptions.RequestException: If the request fails with any other error
        url = f"{self._base_url}/chat/completions"
        # Build request data with required fields
        data: Dict[str, Any] = {"model": model, "messages": messages}
        # Add optional parameters if provided
            data["temperature"] = temperature
            data["top_p"] = top_p
            data["n"] = n
            data["max_tokens"] = max_tokens
            data["presence_penalty"] = presence_penalty
            data["frequency_penalty"] = frequency_penalty
            data["user"] = user
        request = requests.Request("POST", url, headers=self._get_headers(), json=data)
        if return_request:
        # Prepare and send the request
            response = session.send(request.prepare())
        except requests.exceptions.HTTPError as e:
            if e.response.status_code == 401:
                raise UnauthorizedError(e)
    def completions_stream(
    ) -> Iterator[Dict[str, Any]]:
        Create a streaming chat completion.
            Dict[str, Any]: Streaming response chunks from the server
            "stream": True
        # Make streaming request
            response = session.post(
                headers=self._get_headers(), 
                stream=True
            # Parse SSE stream
            for line in response.iter_lines():
                    line = line.decode('utf-8')
                    if line.startswith('data: '):
                        data_str = line[6:]  # Remove 'data: ' prefix
                        if data_str.strip() == '[DONE]':
                            chunk = json.loads(data_str)
from rich.prompt import Prompt
from ... import Client
from ...chat import ChatClient
def _get_available_models(ctx: click.Context) -> List[Dict[str, Any]]:
    """Get list of available models from the proxy server"""
        client = Client(base_url=ctx.obj["base_url"], api_key=ctx.obj["api_key"])
        models_list = client.models.list()
        # Ensure we return a list of dictionaries
        if isinstance(models_list, list):
            # Filter to ensure all items are dictionaries
            return [model for model in models_list if isinstance(model, dict)]
        click.echo(f"Warning: Could not fetch models list: {e}", err=True)
def _select_model(console: Console, available_models: List[Dict[str, Any]]) -> Optional[str]:
    """Interactive model selection"""
    if not available_models:
        console.print("[yellow]No models available or could not fetch models list.[/yellow]")
        model_name = Prompt.ask("Please enter a model name")
        return model_name if model_name.strip() else None
    # Display available models in a table
    table = Table(title="Available Models")
    table.add_column("Model ID", style="green")
    table.add_column("Owned By", style="yellow")
    MAX_MODELS_TO_DISPLAY = 200
    models_to_display: List[Dict[str, Any]] = available_models[:MAX_MODELS_TO_DISPLAY]
    for i, model in enumerate(models_to_display):  # Limit to first 200 models
            str(i + 1),
            str(model.get("id", "")),
            str(model.get("owned_by", ""))
    if len(available_models) > MAX_MODELS_TO_DISPLAY:
        console.print(f"\n[dim]... and {len(available_models) - MAX_MODELS_TO_DISPLAY} more models[/dim]")
            choice = Prompt.ask(
                "\nSelect a model by entering the index number (or type a model name directly)",
                default="1"
            # Try to parse as index
                if 0 <= index < len(available_models):
                    return available_models[index]["id"]
                    console.print(f"[red]Invalid index. Please enter a number between 1 and {len(available_models)}[/red]")
                # Not a number, treat as model name
                if choice:
                    return choice
                    console.print("[red]Please enter a valid model name or index[/red]")
            console.print("\n[yellow]Model selection cancelled.[/yellow]")
@click.command()
@click.argument("model", required=False)
    default=0.7,
    help="Sampling temperature between 0 and 2 (default: 0.7)",
    "--max-tokens",
    help="Maximum number of tokens to generate",
    "--system",
    help="System message to set the behavior of the assistant",
def chat(
    temperature: float,
    """Interactive chat with streaming responses
        # Chat with a specific model
        litellm-proxy chat gpt-4
        # Chat without specifying model (will show model selection)
        litellm-proxy chat
        # Chat with custom settings
        litellm-proxy chat gpt-4 --temperature 0.9 --system "You are a helpful coding assistant"
    # If no model specified, show model selection
        available_models = _get_available_models(ctx)
        model = _select_model(console, available_models)
            console.print("[red]No model selected. Exiting.[/red]")
    client = ChatClient(ctx.obj["base_url"], ctx.obj["api_key"])
    # Initialize conversation history
    messages: List[Dict[str, Any]] = []
    # Add system message if provided
    if system:
        messages.append({"role": "system", "content": system})
    # Display welcome message
    console.print(Panel.fit(
        f"[bold blue]LiteLLM Interactive Chat[/bold blue]\n"
        f"Model: [green]{model}[/green]\n"
        f"Temperature: [yellow]{temperature}[/yellow]\n"
        f"Max Tokens: [yellow]{max_tokens or 'unlimited'}[/yellow]\n\n"
        f"Type your messages and press Enter. Type '/quit' or '/exit' to end the session.\n"
        f"Type '/help' for more commands.",
        title="🤖 Chat Session"
            # Get user input
                user_input = console.input("\n[bold cyan]You:[/bold cyan] ").strip()
            except (EOFError, KeyboardInterrupt):
                console.print("\n[yellow]Chat session ended.[/yellow]")
            # Handle special commands
            should_exit, messages, new_model = _handle_special_commands(
                console, user_input, messages, system, ctx
            if should_exit:
            if new_model:
            # Check if this was a special command that was handled (not a normal message)
            if user_input.lower().startswith(('/quit', '/exit', '/q', '/help', '/clear', '/history', '/save', '/load', '/model')) or not user_input:
            # Add user message to conversation
            messages.append({"role": "user", "content": user_input})
            # Display assistant label
            console.print("\n[bold green]Assistant:[/bold green]")
            # Stream the response
            assistant_content = _stream_response(
            # Add assistant message to conversation history
            if assistant_content:
                messages.append({"role": "assistant", "content": assistant_content})
                console.print("[red]Error: No content received from the model[/red]")
        console.print("\n[yellow]Chat session interrupted.[/yellow]")
def _show_help(console: Console):
    """Show help for interactive chat commands"""
    help_text = """
[bold]Interactive Chat Commands:[/bold]
[cyan]/help[/cyan]     - Show this help message
[cyan]/quit[/cyan]     - Exit the chat session (also /exit, /q)
[cyan]/clear[/cyan]    - Clear conversation history
[cyan]/history[/cyan]  - Show conversation history
[cyan]/model[/cyan]    - Switch to a different model
[cyan]/save <name>[/cyan] - Save conversation to file
[cyan]/load <name>[/cyan] - Load conversation from file
[bold]Tips:[/bold]
- Your conversation history is maintained during the session
- Use Ctrl+C to interrupt at any time
- Responses are streamed in real-time
- You can switch models mid-conversation with /model
    console.print(Panel(help_text, title="Help"))
def _show_history(console: Console, messages: List[Dict[str, Any]]):
    """Show conversation history"""
        console.print("[yellow]No conversation history.[/yellow]")
    console.print(Panel.fit("[bold]Conversation History[/bold]", title="History"))
    for i, message in enumerate(messages, 1):
        role = message["role"]
        if role == "system":
            console.print(f"[dim]{i}. [bold magenta]System:[/bold magenta] {content}[/dim]")
        elif role == "user":
            console.print(f"{i}. [bold cyan]You:[/bold cyan] {content}")
        elif role == "assistant":
            console.print(f"{i}. [bold green]Assistant:[/bold green] {content[:100]}{'...' if len(content) > 100 else ''}")
def _save_conversation(console: Console, messages: List[Dict[str, Any]], command: str):
    """Save conversation to a file"""
    parts = command.split()
    if len(parts) < 2:
        console.print("[red]Usage: /save <filename>[/red]")
    filename = parts[1]
    if not filename.endswith('.json'):
        with open(filename, 'w') as f:
            json.dump(messages, f, indent=2)
        console.print(f"[green]Conversation saved to {filename}[/green]")
        console.print(f"[red]Error saving conversation: {e}[/red]")
def _load_conversation(console: Console, command: str, system: Optional[str]) -> List[Dict[str, Any]]:
    """Load conversation from a file"""
        console.print("[red]Usage: /load <filename>[/red]")
            messages = json.load(f)
        console.print(f"[green]Conversation loaded from {filename}[/green]")
        console.print(f"[red]File not found: {filename}[/red]")
        console.print(f"[red]Error loading conversation: {e}[/red]")
    # Return empty list or just system message if load failed
        return [{"role": "system", "content": system}]
def _handle_special_commands(
    messages: List[Dict[str, Any]], 
    system: Optional[str],
    ctx: click.Context
) -> tuple[bool, List[Dict[str, Any]], Optional[str]]:
    """Handle special chat commands. Returns (should_exit, updated_messages, updated_model)"""
    if user_input.lower() in ['/quit', '/exit', '/q']:
        console.print("[yellow]Chat session ended.[/yellow]")
        return True, messages, None
    elif user_input.lower() == '/help':
        _show_help(console)
        return False, messages, None
    elif user_input.lower() == '/clear':
            new_messages.append({"role": "system", "content": system})
        console.print("[green]Conversation history cleared.[/green]")
        return False, new_messages, None
    elif user_input.lower() == '/history':
        _show_history(console, messages)
    elif user_input.lower().startswith('/save'):
        _save_conversation(console, messages, user_input)
    elif user_input.lower().startswith('/load'):
        new_messages = _load_conversation(console, user_input, system)
    elif user_input.lower() == '/model':
        new_model = _select_model(console, available_models)
            console.print(f"[green]Switched to model: {new_model}[/green]")
            return False, messages, new_model
    elif not user_input:
    # Not a special command
def _stream_response(console: Console, client: ChatClient, model: str, messages: List[Dict[str, Any]], temperature: float, max_tokens: Optional[int]) -> Optional[str]:
    """Stream the model response and return the complete content"""
        assistant_content = ""
        for chunk in client.completions_stream(
            if "choices" in chunk and len(chunk["choices"]) > 0:
                delta = chunk["choices"][0].get("delta", {})
                content = delta.get("content", "")
                    assistant_content += content
                    console.print(content, end="")
        console.print()  # Add newline after streaming
        return assistant_content if assistant_content else None
        console.print(f"\n[red]Error: HTTP {e.response.status_code}[/red]")
            error_body = e.response.json()
            console.print(f"[red]{error_body.get('error', {}).get('message', 'Unknown error')}[/red]")
            console.print(f"[red]{e.response.text}[/red]")
        console.print(f"\n[red]Error: {str(e)}[/red]")
