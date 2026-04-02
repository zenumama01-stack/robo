from .container_auto_param import ContainerAutoParam
from .local_environment_param import LocalEnvironmentParam
from .container_reference_param import ContainerReferenceParam
__all__ = ["FunctionShellToolParam", "Environment"]
Environment: TypeAlias = Union[ContainerAutoParam, LocalEnvironmentParam, ContainerReferenceParam]
class FunctionShellToolParam(TypedDict, total=False):
    type: Required[Literal["shell"]]
    environment: Optional[Environment]
