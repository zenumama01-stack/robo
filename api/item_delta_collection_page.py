class ItemDeltaCollectionPage(ItemsCollectionPage):
    def __init__(self, prop_list, token=None, delta_link=None, next_page_link=None):
        super(ItemDeltaCollectionPage, self).__init__(prop_list)
        self._next_page_link = next_page_link
        self._token = token
        self._delta_link = delta_link
    def token(self):
        """Gets the token property from the
        ItemDeltaCollectionPage
                The token property from the ItemDeltaCollectionPage
        return self._token
    def delta_link(self):
        """Gets the deltaLink property from the
                The deltaLink property from the ItemDeltaCollectionPage
        return self._delta_link
    def next_page_link(self):
        """Gets the nextLink property from the
                The nextLink property from the ItemDeltaCollectionPage
