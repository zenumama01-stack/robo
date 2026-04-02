from .dpo_method_param import DpoMethodParam
from .supervised_method_param import SupervisedMethodParam
from .reinforcement_method_param import ReinforcementMethodParam
__all__ = ["JobCreateParams", "Hyperparameters", "Integration", "IntegrationWandb", "Method"]
class JobCreateParams(TypedDict, total=False):
    model: Required[Union[str, Literal["babbage-002", "davinci-002", "gpt-3.5-turbo", "gpt-4o-mini"]]]
    """The name of the model to fine-tune.
    You can select one of the
    training_file: Required[str]
    """The ID of an uploaded file that contains training data.
    The hyperparameters used for the fine-tuning job. This value is now deprecated
    integrations: Optional[Iterable[Integration]]
    """A list of integrations to enable for your fine-tuning job."""
    method: Method
    """The seed controls the reproducibility of the job.
    Passing in the same seed and job parameters should produce the same results, but
    may differ in rare cases. If a seed is not specified, one will be generated for
    you.
    A string of up to 64 characters that will be added to your fine-tuned model
    validation_file: Optional[str]
    """The ID of an uploaded file that contains validation data.
class Hyperparameters(TypedDict, total=False):
    The hyperparameters used for the fine-tuning job.
    This value is now deprecated in favor of `method`, and should be passed in under the `method` parameter.
class IntegrationWandb(TypedDict, total=False):
    project: Required[str]
    entity: Optional[str]
    tags: SequenceNotStr[str]
class Integration(TypedDict, total=False):
    type: Required[Literal["wandb"]]
    """The type of integration to enable.
    Currently, only "wandb" (Weights and Biases) is supported.
    wandb: Required[IntegrationWandb]
class Method(TypedDict, total=False):
    type: Required[Literal["supervised", "dpo", "reinforcement"]]
    dpo: DpoMethodParam
    reinforcement: ReinforcementMethodParam
    supervised: SupervisedMethodParam
