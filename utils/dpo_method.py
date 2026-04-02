from .dpo_hyperparameters import DpoHyperparameters
__all__ = ["DpoMethod"]
class DpoMethod(BaseModel):
    """Configuration for the DPO fine-tuning method."""
    hyperparameters: Optional[DpoHyperparameters] = None
