    import pandas as pandas
PANDAS_INSTRUCTIONS = format_instructions(library="pandas", extra="datalib")
class PandasProxy(LazyProxy[Any]):
            import pandas
            raise MissingDependencyError(PANDAS_INSTRUCTIONS) from err
        return pandas
    pandas = PandasProxy()
