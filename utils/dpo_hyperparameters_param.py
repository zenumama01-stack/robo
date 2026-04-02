__all__ = ["DpoHyperparametersParam"]
class DpoHyperparametersParam(TypedDict, total=False):
    batch_size: Union[Literal["auto"], int]
    beta: Union[Literal["auto"], float]
    learning_rate_multiplier: Union[Literal["auto"], float]
    n_epochs: Union[Literal["auto"], int]
