from ..model.item_delta_collection_page import ItemDeltaCollectionPage
class ItemDeltaCollectionResponse(ItemsCollectionResponse):
            :class:`ItemDeltaCollectionPage<onedrivesdk.request.item_delta_collection.ItemDeltaCollectionPage>`:
        token = self._prop_dict["@delta.token"] if "@delta.token" in self._prop_dict else None
        delta_link = self._prop_dict["@odata.deltaLink"] if "@odata.deltaLink" in self._prop_dict else None
        next_page_link = self._prop_dict["@odata.nextLink"] if "@odata.nextLink" in self._prop_dict else None
            self._collection_page._token = token
            self._collection_page._delta_link = delta_link
            self._collection_page._next_page_link = next_page_link
            self._collection_page = ItemDeltaCollectionPage(self._prop_dict["value"],
                                                            delta_link,
                                                            next_page_link)
from ..request.item_delta import ItemDeltaRequest