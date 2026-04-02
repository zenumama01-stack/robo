class Hashes(OneDriveObjectBase):
    def crc32_hash(self):
        """Gets and sets the crc32Hash
                The crc32Hash
        if "crc32Hash" in self._prop_dict:
            return self._prop_dict["crc32Hash"]
    @crc32_hash.setter
    def crc32_hash(self, val):
        self._prop_dict["crc32Hash"] = val
    def sha1_hash(self):
        """Gets and sets the sha1Hash
                The sha1Hash
        if "sha1Hash" in self._prop_dict:
            return self._prop_dict["sha1Hash"]
    @sha1_hash.setter
    def sha1_hash(self, val):
        self._prop_dict["sha1Hash"] = val
from typing import TYPE_CHECKING, BinaryIO, NoReturn
from pip._internal.exceptions import HashMismatch, HashMissing, InstallationError
from pip._internal.utils.misc import read_chunks
# The recommended hash algo of the moment. Change this whenever the state of
# the art changes; it won't hurt backward compatibility.
FAVORITE_HASH = "sha256"
# Names of hashlib algorithms allowed by the --hash option and ``pip hash``
# Currently, those are the ones at least as collision-resistant as sha256.
STRONG_HASHES = ["sha256", "sha384", "sha512"]
class Hashes:
    """A wrapper that builds multiple hashes at once and checks them against
    known-good values
    def __init__(self, hashes: dict[str, list[str]] | None = None) -> None:
        :param hashes: A dict of algorithm names pointing to lists of allowed
        allowed = {}
        if hashes is not None:
            for alg, keys in hashes.items():
                # Make sure values are always sorted (to ease equality checks)
                allowed[alg] = [k.lower() for k in sorted(keys)]
        self._allowed = allowed
    def __and__(self, other: Hashes) -> Hashes:
        if not isinstance(other, Hashes):
        # If either of the Hashes object is entirely empty (i.e. no hash
        # specified at all), all hashes from the other object are allowed.
        if not other:
        # Otherwise only hashes that present in both objects are allowed.
        new = {}
        for alg, values in other._allowed.items():
            if alg not in self._allowed:
            new[alg] = [v for v in values if v in self._allowed[alg]]
        return Hashes(new)
    def digest_count(self) -> int:
        return sum(len(digests) for digests in self._allowed.values())
    def is_hash_allowed(self, hash_name: str, hex_digest: str) -> bool:
        """Return whether the given hex digest is allowed."""
        return hex_digest in self._allowed.get(hash_name, [])
    def check_against_chunks(self, chunks: Iterable[bytes]) -> None:
        """Check good hashes against ones built from iterable of chunks of
        Raise HashMismatch if none match.
        gots = {}
        for hash_name in self._allowed.keys():
                gots[hash_name] = hashlib.new(hash_name)
                raise InstallationError(f"Unknown hash name: {hash_name}")
            for hash in gots.values():
                hash.update(chunk)
        for hash_name, got in gots.items():
            if got.hexdigest() in self._allowed[hash_name]:
        self._raise(gots)
    def _raise(self, gots: dict[str, _Hash]) -> NoReturn:
        raise HashMismatch(self._allowed, gots)
    def check_against_file(self, file: BinaryIO) -> None:
        """Check good hashes against a file-like object
        return self.check_against_chunks(read_chunks(file))
    def check_against_path(self, path: str) -> None:
        with open(path, "rb") as file:
            return self.check_against_file(file)
    def has_one_of(self, hashes: dict[str, str]) -> bool:
        """Return whether any of the given hashes are allowed."""
        for hash_name, hex_digest in hashes.items():
            if self.is_hash_allowed(hash_name, hex_digest):
        """Return whether I know any known-good hashes."""
        return bool(self._allowed)
        return self._allowed == other._allowed
            ",".join(
                    ":".join((alg, digest))
                    for alg, digest_list in self._allowed.items()
                    for digest in digest_list
class MissingHashes(Hashes):
    """A workalike for Hashes used when we're missing a hash for a requirement
    It computes the actual hash of the requirement and raises a HashMissing
    exception showing it to the user.
        """Don't offer the ``hashes`` kwarg."""
        # Pass our favorite hash in to generate a "gotten hash". With the
        # empty list, it will never match, so an error will always raise.
        super().__init__(hashes={FAVORITE_HASH: []})
        raise HashMissing(gots[FAVORITE_HASH].hexdigest())
