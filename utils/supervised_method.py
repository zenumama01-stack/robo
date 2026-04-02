from .supervised_hyperparameters import SupervisedHyperparameters
__all__ = ["SupervisedMethod"]
class SupervisedMethod(BaseModel):
    hyperparameters: Optional[SupervisedHyperparameters] = None
