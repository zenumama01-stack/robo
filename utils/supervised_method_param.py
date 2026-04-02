from .supervised_hyperparameters_param import SupervisedHyperparametersParam
__all__ = ["SupervisedMethodParam"]
class SupervisedMethodParam(TypedDict, total=False):
    hyperparameters: SupervisedHyperparametersParam
