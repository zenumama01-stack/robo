"""Query with grouping against the shopping search API"""
    """Get and print a feed of public products in the United States mathing a
    text search query for 'digital camera' and grouped by the 8 top brands.
    The list method of the resource should be called with the "crowdBy"
    parameter.  Each parameter should be designed as <attribute>:<occurence>,
    where <occurrence> is the number of that <attribute> that will be used. For
    example, to crowd by the 5 top brands, the parameter would be "brand:5". The
    possible rules for crowding are currently:
    account_id:<occurrence> (eg account_id:5)
    brand:<occurrence> (eg brand:5)
    condition:<occurrence> (eg condition:3)
    gtin:<occurrence> (eg gtin:10)
    price:<occurrence> (eg price:10)
    Multiple crowding rules should be specified by separating them with a comma,
    for example to crowd by the top 5 brands and then condition of those items,
    the parameter should be crowdBy="brand:5,condition:3"
    # The crowdBy parameter to the list method causes the results to be grouped,
    # in this case by the top 8 brands.
    request = resource.list(
        source="public", country="US", q="digital camera", crowdBy="brand:8"
