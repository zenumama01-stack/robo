from .image_url_delta import ImageURLDelta
__all__ = ["ImageURLDeltaBlock"]
class ImageURLDeltaBlock(BaseModel):
    image_url: Optional[ImageURLDelta] = None
