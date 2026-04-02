from .container_auto import ContainerAuto
from .local_environment import LocalEnvironment
from .container_reference import ContainerReference
__all__ = ["FunctionShellTool", "Environment"]
Environment: TypeAlias = Annotated[
    Union[ContainerAuto, LocalEnvironment, ContainerReference, None], PropertyInfo(discriminator="type")
class FunctionShellTool(BaseModel):
    """A tool that allows the model to execute shell commands."""
    type: Literal["shell"]
    """The type of the shell tool. Always `shell`."""
    environment: Optional[Environment] = None
