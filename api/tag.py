from ..model.auto_tagged import AutoTagged
class Tag(OneDriveObjectBase):
    def auto_tagged(self):
        Gets and sets the autoTagged
            :class:`AutoTagged<onedrivesdk.model.auto_tagged.AutoTagged>`:
                The autoTagged
        if "autoTagged" in self._prop_dict:
            if isinstance(self._prop_dict["autoTagged"], OneDriveObjectBase):
                return self._prop_dict["autoTagged"]
                self._prop_dict["autoTagged"] = AutoTagged(self._prop_dict["autoTagged"])
    @auto_tagged.setter
    def auto_tagged(self, val):
        self._prop_dict["autoTagged"] = val
"""Provides an :class:`~git.objects.base.Object`-based type for annotated tags.
This defines the :class:`TagObject` class, which represents annotated tags.
For lightweight tags, see the :mod:`git.refs.tag` module.
__all__ = ["TagObject"]
from git.compat import defenc
from git.util import Actor, hex_to_bin
from .util import get_object_type_by_name, parse_actor_and_date
# typing ----------------------------------------------
from typing import List, TYPE_CHECKING, Union
if sys.version_info >= (3, 8):
# ---------------------------------------------------
class TagObject(base.Object):
    """Annotated (i.e. non-lightweight) tag carrying additional information about an
    object we are pointing to.
    See :manpage:`gitglossary(7)` on "tag object":
    https://git-scm.com/docs/gitglossary#def_tag_object
    type: Literal["tag"] = "tag"
        "tagger",
        "tagged_date",
        "tagger_tz_offset",
        "message",
        object: Union[None, base.Object] = None,
        tag: Union[None, str] = None,
        tagger: Union[None, Actor] = None,
        tagged_date: Union[int, None] = None,
        tagger_tz_offset: Union[int, None] = None,
        message: Union[str, None] = None,
    ) -> None:  # @ReservedAssignment
        """Initialize a tag object with additional data.
            20 byte SHA1.
        :param object:
            :class:`~git.objects.base.Object` instance of object we are pointing to.
        :param tag:
            Name of this tag.
        :param tagger:
            :class:`~git.util.Actor` identifying the tagger.
        :param tagged_date: int_seconds_since_epoch
            The DateTime of the tag creation.
            Use :func:`time.gmtime` to convert it into a different format.
        :param tagger_tz_offset: int_seconds_west_of_utc
            The timezone that the `tagged_date` is in, in a format similar to
            :attr:`time.altzone`.
        if object is not None:
            self.object: Union["Commit", "Blob", "Tree", "TagObject"] = object
        if tag is not None:
            self.tag = tag
        if tagger is not None:
            self.tagger = tagger
        if tagged_date is not None:
            self.tagged_date = tagged_date
        if tagger_tz_offset is not None:
            self.tagger_tz_offset = tagger_tz_offset
        """Cache all our attributes at once."""
        if attr in TagObject.__slots__:
            ostream = self.repo.odb.stream(self.binsha)
            lines: List[str] = ostream.read().decode(defenc, "replace").splitlines()
            _obj, hexsha = lines[0].split(" ")
            _type_token, type_name = lines[1].split(" ")
            object_type = get_object_type_by_name(type_name.encode("ascii"))
            self.object = object_type(self.repo, hex_to_bin(hexsha))
            self.tag = lines[2][4:]  # tag <tag name>
            if len(lines) > 3:
                tagger_info = lines[3]  # tagger <actor> <date>
                    self.tagger,
                    self.tagged_date,
                    self.tagger_tz_offset,
                ) = parse_actor_and_date(tagger_info)
            # Line 4 empty - it could mark the beginning of the next header.
            # In case there really is no message, it would not exist.
            # Otherwise a newline separates header from message.
            if len(lines) > 5:
                self.message = "\n".join(lines[5:])
                self.message = ""
        # END check our attributes
"""Provides a :class:`~git.refs.reference.Reference`-based type for lightweight tags.
This defines the :class:`TagReference` class (and its alias :class:`Tag`), which
represents lightweight tags. For annotated tags (which are git objects), see the
:mod:`git.objects.tag` module.
__all__ = ["TagReference", "Tag"]
from typing import Any, TYPE_CHECKING, Type, Union
from git.types import AnyGitObject, PathLike
    from git.objects import Commit, TagObject
    from git.refs import SymbolicReference
# ------------------------------------------------------------------------------
class TagReference(Reference):
    """A lightweight tag reference which either points to a commit, a tag object or any
    other object. In the latter case additional information, like the signature or the
    tag-creator, is available.
    This tag object will always point to a commit object, but may carry additional
    information in a tag object::
     tagref = TagReference.list_items(repo)[0]
     print(tagref.commit.message)
     if tagref.tag is not None:
        print(tagref.tag.message)
    _common_default = "tags"
    _common_path_default = Reference._common_path_default + "/" + _common_default
    @property  # type: ignore[misc]
    def commit(self) -> "Commit":  # LazyMixin has unrelated commit method
        """:return: Commit object the tag ref points to
            If the tag points to a tree or blob.
        obj = self.object
        while obj.type != "commit":
            if obj.type == "tag":
                # It is a tag object which carries the commit as an object - we can point to anything.
                obj = obj.object
                        "Cannot resolve commit as tag %s points to a %s object - "
                        + "use the `.object` property instead to access it"
                    % (self, obj.type)
    def tag(self) -> Union["TagObject", None]:
            Tag object this tag ref points to, or ``None`` in case we are a lightweight
    # Make object read-only. It should be reasonably hard to adjust an existing tag.
    def object(self) -> AnyGitObject:
        return Reference._get_object(self)
        cls: Type["TagReference"],
        reference: Union[str, "SymbolicReference"] = "HEAD",
        logmsg: Union[str, None] = None,
    ) -> "TagReference":
        """Create a new tag reference.
            The :class:`~git.repo.base.Repo` to create the tag in.
            The name of the tag, e.g. ``1.0`` or ``releases/1.0``.
            The prefix ``refs/tags`` is implied.
        :param reference:
            A reference to the :class:`~git.objects.base.Object` you want to tag.
            The referenced object can be a commit, tree, or blob.
        :param logmsg:
            If not ``None``, the message will be used in your tag object. This will also
            create an additional tag object that allows to obtain that information,
            e.g.::
                tagref.tag.message
            Synonym for the `logmsg` parameter. Included for backwards compatibility.
            `logmsg` takes precedence if both are passed.
            If ``True``, force creation of a tag even though that tag already exists.
            Additional keyword arguments to be passed to :manpage:`git-tag(1)`.
            A new :class:`TagReference`.
        if "ref" in kwargs and kwargs["ref"]:
            reference = kwargs["ref"]
        if "message" in kwargs and kwargs["message"]:
            kwargs["m"] = kwargs["message"]
            del kwargs["message"]
        if logmsg:
            kwargs["m"] = logmsg
            kwargs["f"] = True
        args = (path, reference)
        repo.git.tag(*args, **kwargs)
        return TagReference(repo, "%s/%s" % (cls._common_path_default, path))
    def delete(cls, repo: "Repo", *tags: "TagReference") -> None:  # type: ignore[override]
        """Delete the given existing tag or tags."""
        repo.git.tag("-d", *tags)
# Provide an alias.
Tag = TagReference
