__all__ = ["ReinforcementHyperparametersParam"]
class ReinforcementHyperparametersParam(TypedDict, total=False):
    compute_multiplier: Union[Literal["auto"], float]
    eval_interval: Union[Literal["auto"], int]
    eval_samples: Union[Literal["auto"], int]
    reasoning_effort: Literal["default", "low", "medium", "high"]
