"""Query that is restricted by a parameter against the public shopping search
API"""
    "digital camera", that are created by "Canon" available in the
    The "restrictBy" parameter controls which types of results are returned.
    Multiple values for a single restrictBy can be separated by the "|" operator,
    so to look for all products created by Canon, Sony, or Apple:
    restrictBy = 'brand:canon|sony|apple'
    Multiple restricting parameters should be separated by a comma, so for
    products created by Sony with the word "32GB" in the title:
    restrictBy = 'brand:sony,title:32GB'
        source="public", country="US", restrictBy="brand:canon", q="Digital Camera"
