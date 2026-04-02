__all__ = ["Alpha", "AsyncAlpha"]
class Alpha(SyncAPIResource):
    def graders(self) -> Graders:
        return Graders(self._client)
    def with_raw_response(self) -> AlphaWithRawResponse:
        return AlphaWithRawResponse(self)
    def with_streaming_response(self) -> AlphaWithStreamingResponse:
        return AlphaWithStreamingResponse(self)
class AsyncAlpha(AsyncAPIResource):
    def graders(self) -> AsyncGraders:
        return AsyncGraders(self._client)
    def with_raw_response(self) -> AsyncAlphaWithRawResponse:
        return AsyncAlphaWithRawResponse(self)
    def with_streaming_response(self) -> AsyncAlphaWithStreamingResponse:
        return AsyncAlphaWithStreamingResponse(self)
class AlphaWithRawResponse:
    def __init__(self, alpha: Alpha) -> None:
        self._alpha = alpha
    def graders(self) -> GradersWithRawResponse:
        return GradersWithRawResponse(self._alpha.graders)
class AsyncAlphaWithRawResponse:
    def __init__(self, alpha: AsyncAlpha) -> None:
    def graders(self) -> AsyncGradersWithRawResponse:
        return AsyncGradersWithRawResponse(self._alpha.graders)
class AlphaWithStreamingResponse:
    def graders(self) -> GradersWithStreamingResponse:
        return GradersWithStreamingResponse(self._alpha.graders)
class AsyncAlphaWithStreamingResponse:
    def graders(self) -> AsyncGradersWithStreamingResponse:
        return AsyncGradersWithStreamingResponse(self._alpha.graders)
