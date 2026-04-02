__all__ = ["ReinforcementHyperparameters"]
class ReinforcementHyperparameters(BaseModel):
    """The hyperparameters used for the reinforcement fine-tuning job."""
    compute_multiplier: Union[Literal["auto"], float, None] = None
    Multiplier on amount of compute used for exploring search space during training.
    eval_interval: Union[Literal["auto"], int, None] = None
    """The number of training steps between evaluation runs."""
    eval_samples: Union[Literal["auto"], int, None] = None
    """Number of evaluation samples to generate per training step."""
    reasoning_effort: Optional[Literal["default", "low", "medium", "high"]] = None
    """Level of reasoning effort."""
