__all__ = ["CustomToolInputFormat", "Text", "Grammar"]
class Grammar(BaseModel):
    definition: str
    syntax: Literal["lark", "regex"]
    type: Literal["grammar"]
CustomToolInputFormat: TypeAlias = Annotated[Union[Text, Grammar], PropertyInfo(discriminator="type")]
class Grammar(TypedDict, total=False):
CustomToolInputFormat: TypeAlias = Union[Text, Grammar]
