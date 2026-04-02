from ...types import eval_list_params, eval_create_params, eval_update_params
from ...types.eval_list_response import EvalListResponse
from ...types.eval_create_response import EvalCreateResponse
from ...types.eval_delete_response import EvalDeleteResponse
from ...types.eval_update_response import EvalUpdateResponse
from ...types.eval_retrieve_response import EvalRetrieveResponse
__all__ = ["Evals", "AsyncEvals"]
class Evals(SyncAPIResource):
    def with_raw_response(self) -> EvalsWithRawResponse:
        return EvalsWithRawResponse(self)
    def with_streaming_response(self) -> EvalsWithStreamingResponse:
        return EvalsWithStreamingResponse(self)
        data_source_config: eval_create_params.DataSourceConfig,
        testing_criteria: Iterable[eval_create_params.TestingCriterion],
    ) -> EvalCreateResponse:
        Create the structure of an evaluation that can be used to test a model's
        performance. An evaluation is a set of testing criteria and the config for a
        data source, which dictates the schema of the data used in the evaluation. After
        creating an evaluation, you can run it on different models and model parameters.
        We support several types of graders and datasources. For more information, see
        the [Evals guide](https://platform.openai.com/docs/guides/evals).
          data_source_config: The configuration for the data source used for the evaluation runs. Dictates the
              schema of the data used in the evaluation.
          testing_criteria: A list of graders for all eval runs in this group. Graders can reference
              variables in the data source using double curly braces notation, like
              `{{item.variable_name}}`. To reference the model's output, use the `sample`
              namespace (ie, `{{sample.output_text}}`).
          name: The name of the evaluation.
            "/evals",
                    "data_source_config": data_source_config,
                    "testing_criteria": testing_criteria,
                eval_create_params.EvalCreateParams,
            cast_to=EvalCreateResponse,
        eval_id: str,
    ) -> EvalRetrieveResponse:
        Get an evaluation by ID.
        if not eval_id:
            raise ValueError(f"Expected a non-empty value for `eval_id` but received {eval_id!r}")
            path_template("/evals/{eval_id}", eval_id=eval_id),
            cast_to=EvalRetrieveResponse,
    ) -> EvalUpdateResponse:
        Update certain properties of an evaluation.
          name: Rename the evaluation.
                eval_update_params.EvalUpdateParams,
            cast_to=EvalUpdateResponse,
        order_by: Literal["created_at", "updated_at"] | Omit = omit,
    ) -> SyncCursorPage[EvalListResponse]:
        List evaluations for a project.
          after: Identifier for the last eval from the previous pagination request.
          limit: Number of evals to retrieve.
          order: Sort order for evals by timestamp. Use `asc` for ascending order or `desc` for
          order_by: Evals can be ordered by creation time or last updated time. Use `created_at` for
              creation time or `updated_at` for last updated time.
            page=SyncCursorPage[EvalListResponse],
                        "order_by": order_by,
                    eval_list_params.EvalListParams,
            model=EvalListResponse,
    ) -> EvalDeleteResponse:
        Delete an evaluation.
            cast_to=EvalDeleteResponse,
class AsyncEvals(AsyncAPIResource):
    def with_raw_response(self) -> AsyncEvalsWithRawResponse:
        return AsyncEvalsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncEvalsWithStreamingResponse:
        return AsyncEvalsWithStreamingResponse(self)
    ) -> AsyncPaginator[EvalListResponse, AsyncCursorPage[EvalListResponse]]:
            page=AsyncCursorPage[EvalListResponse],
class EvalsWithRawResponse:
    def __init__(self, evals: Evals) -> None:
        self._evals = evals
            evals.create,
            evals.retrieve,
            evals.update,
            evals.list,
            evals.delete,
        return RunsWithRawResponse(self._evals.runs)
class AsyncEvalsWithRawResponse:
    def __init__(self, evals: AsyncEvals) -> None:
        return AsyncRunsWithRawResponse(self._evals.runs)
class EvalsWithStreamingResponse:
        return RunsWithStreamingResponse(self._evals.runs)
class AsyncEvalsWithStreamingResponse:
        return AsyncRunsWithStreamingResponse(self._evals.runs)
            f"/evals/{eval_id}",
