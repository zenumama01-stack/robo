from .dpo_hyperparameters_param import DpoHyperparametersParam
__all__ = ["DpoMethodParam"]
class DpoMethodParam(TypedDict, total=False):
    hyperparameters: DpoHyperparametersParam
