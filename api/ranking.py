    text search query for 'digital camera' ranked by ascending price.
    The list method for the resource should be called with the "rankBy"
    parameter.  5 parameters to rankBy are currently supported by the API. They
    are:
    "relevancy"
    "modificationTime:ascending"
    "modificationTime:descending"
    "price:ascending"
    "price:descending"
    These parameters can be combined
    The default ranking is "relevancy" if the rankBy parameter is omitted.
    # The rankBy parameter to the list method causes results to be ranked, in
    # this case by ascending price.
        source="public", country="US", q="digital camera", rankBy="price:ascending"
