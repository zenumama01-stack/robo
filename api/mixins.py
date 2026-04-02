from collections.abc import Mapping
class EqualityComparableID:
    __slots__ = ()
            return self.id == other.id
class HashableID(EqualityComparableID):
        return self.id
class DataMapping(Mapping):
    def __contains__(self, item):
        return item in self.data
    def __getattr__(self, name):
            return self.data[name]
            raise AttributeError from None
    def __getitem__(self, key):
            return getattr(self, key)
            raise KeyError from None
        return iter(self.data)
        return len(self.data)
from django.contrib.auth import REDIRECT_FIELD_NAME
from django.contrib.auth.views import redirect_to_login
from django.shortcuts import resolve_url
class AccessMixin:
    Abstract CBV mixin that gives access mixins the same customizable
    functionality.
    login_url = None
    permission_denied_message = ""
    raise_exception = False
    redirect_field_name = REDIRECT_FIELD_NAME
    def get_login_url(self):
        Override this method to override the login_url attribute.
        login_url = self.login_url or settings.LOGIN_URL
        if not login_url:
                f"{self.__class__.__name__} is missing the login_url attribute. Define "
                f"{self.__class__.__name__}.login_url, settings.LOGIN_URL, or override "
                f"{self.__class__.__name__}.get_login_url()."
        return str(login_url)
    def get_permission_denied_message(self):
        Override this method to override the permission_denied_message
        attribute.
        return self.permission_denied_message
    def get_redirect_field_name(self):
        Override this method to override the redirect_field_name attribute.
        return self.redirect_field_name
    def handle_no_permission(self):
        if self.raise_exception or self.request.user.is_authenticated:
            raise PermissionDenied(self.get_permission_denied_message())
        path = self.request.build_absolute_uri()
        resolved_login_url = resolve_url(self.get_login_url())
        # If the login url is the same scheme and net location then use the
        # path as the "next" url.
        login_scheme, login_netloc = urlsplit(resolved_login_url)[:2]
        current_scheme, current_netloc = urlsplit(path)[:2]
        if (not login_scheme or login_scheme == current_scheme) and (
            not login_netloc or login_netloc == current_netloc
            path = self.request.get_full_path()
        return redirect_to_login(
            resolved_login_url,
            self.get_redirect_field_name(),
class LoginRequiredMixin(AccessMixin):
    """Verify that the current user is authenticated."""
    def dispatch(self, request, *args, **kwargs):
        if not request.user.is_authenticated:
            return self.handle_no_permission()
        return super().dispatch(request, *args, **kwargs)
class PermissionRequiredMixin(AccessMixin):
    """Verify that the current user has all specified permissions."""
    permission_required = None
    def get_permission_required(self):
        Override this method to override the permission_required attribute.
        Must return an iterable.
        if self.permission_required is None:
                f"{self.__class__.__name__} is missing the "
                f"permission_required attribute. Define "
                f"{self.__class__.__name__}.permission_required, or override "
                f"{self.__class__.__name__}.get_permission_required()."
        if isinstance(self.permission_required, str):
            perms = (self.permission_required,)
            perms = self.permission_required
        return perms
    def has_permission(self):
        Override this method to customize the way permissions are checked.
        perms = self.get_permission_required()
        return self.request.user.has_perms(perms)
        if not self.has_permission():
class UserPassesTestMixin(AccessMixin):
    Deny a request with a permission error if the test_func() method returns
    False.
    def test_func(self):
            "{} is missing the implementation of the test_func() method.".format(
                self.__class__.__name__
    def get_test_func(self):
        Override this method to use a different test_func method.
        return self.test_func
        user_test_result = self.get_test_func()()
        if not user_test_result:
# RemovedInDjango70Warning: When the deprecation ends, remove completely.
from django.utils.deprecation import RemovedInDjango70Warning
# RemovedInDjango70Warning.
class OrderableAggMixin:
    allow_order_by = True
    def __init_subclass__(cls, /, *args, **kwargs):
            "OrderableAggMixin is deprecated. Use Aggregate and allow_order_by "
            "instead.",
            category=RemovedInDjango70Warning,
            stacklevel=1,
        super().__init_subclass__(*args, **kwargs)
class StorageSettingsMixin:
    def _clear_cached_properties(self, setting, **kwargs):
        """Reset setting based property values."""
        if setting == "MEDIA_ROOT":
            self.__dict__.pop("base_location", None)
            self.__dict__.pop("location", None)
        elif setting == "MEDIA_URL":
            self.__dict__.pop("base_url", None)
        elif setting == "FILE_UPLOAD_PERMISSIONS":
            self.__dict__.pop("file_permissions_mode", None)
        elif setting == "FILE_UPLOAD_DIRECTORY_PERMISSIONS":
            self.__dict__.pop("directory_permissions_mode", None)
    def _value_or_setting(self, value, setting):
        return setting if value is None else value
NOT_PROVIDED = object()
class FieldCacheMixin:
    An API for working with the model's fields value cache.
    Subclasses must set self.cache_name to a unique entry for the cache -
    typically the field’s name.
    def cache_name(self):
    def get_cached_value(self, instance, default=NOT_PROVIDED):
            return instance._state.fields_cache[self.cache_name]
            if default is NOT_PROVIDED:
    def is_cached(self, instance):
        return self.cache_name in instance._state.fields_cache
    def set_cached_value(self, instance, value):
        instance._state.fields_cache[self.cache_name] = value
    def delete_cached_value(self, instance):
        del instance._state.fields_cache[self.cache_name]
class CheckFieldDefaultMixin:
    _default_hint = ("<valid default>", "<invalid default>")
    def _check_default(self):
            self.has_default()
            and self.default is not None
            and not callable(self.default)
                    "%s default should be a callable instead of an instance "
                    "so that it's not shared between all field instances."
                    % (self.__class__.__name__,),
                        "Use a callable instead, e.g., use `%s` instead of "
                        "`%s`." % self._default_hint
                    id="fields.E010",
        errors.extend(self._check_default())
from django.db.models.fields import DecimalField, FloatField, IntegerField
from django.db.models.functions import Cast
class FixDecimalInputMixin:
    def as_postgresql(self, compiler, connection, **extra_context):
        # Cast FloatField to DecimalField as PostgreSQL doesn't support the
        # following function signatures:
        # - LOG(double, double)
        # - MOD(double, double)
        output_field = DecimalField(decimal_places=sys.float_info.dig, max_digits=1000)
        clone = self.copy()
        clone.set_source_expressions(
                    Cast(expression, output_field)
                    if isinstance(expression.output_field, FloatField)
                    else expression
                for expression in self.get_source_expressions()
        return clone.as_sql(compiler, connection, **extra_context)
class FixDurationInputMixin:
    def as_mysql(self, compiler, connection, **extra_context):
        sql, params = super().as_sql(compiler, connection, **extra_context)
        if self.output_field.get_internal_type() == "DurationField":
            sql = "CAST(%s AS SIGNED)" % sql
    def as_oracle(self, compiler, connection, **extra_context):
            self.output_field.get_internal_type() == "DurationField"
            and not connection.features.supports_aggregation_over_interval_types
            expression = self.get_source_expressions()[0]
            options = self._get_repr_options()
            from django.db.backends.oracle.functions import (
                IntervalToSeconds,
                SecondsToInterval,
            return compiler.compile(
                SecondsToInterval(
                    self.__class__(IntervalToSeconds(expression), **options)
        return super().as_sql(compiler, connection, **extra_context)
class NumericOutputFieldMixin:
    def _resolve_output_field(self):
        source_fields = self.get_source_fields()
        if any(isinstance(s, DecimalField) for s in source_fields):
            return DecimalField()
        if any(isinstance(s, IntegerField) for s in source_fields):
            return FloatField()
        return super()._resolve_output_field() if source_fields else FloatField()
"""Mixin classes for custom array types that don't inherit from ndarray."""
from numpy.core import umath as um
__all__ = ['NDArrayOperatorsMixin']
def _disables_array_ufunc(obj):
    """True when __array_ufunc__ is set to None."""
        return obj.__array_ufunc__ is None
def _binary_method(ufunc, name):
    """Implement a forward binary method with a ufunc, e.g., __add__."""
    def func(self, other):
        if _disables_array_ufunc(other):
        return ufunc(self, other)
    func.__name__ = '__{}__'.format(name)
def _reflected_binary_method(ufunc, name):
    """Implement a reflected binary method with a ufunc, e.g., __radd__."""
        return ufunc(other, self)
    func.__name__ = '__r{}__'.format(name)
def _inplace_binary_method(ufunc, name):
    """Implement an in-place binary method with a ufunc, e.g., __iadd__."""
        return ufunc(self, other, out=(self,))
    func.__name__ = '__i{}__'.format(name)
def _numeric_methods(ufunc, name):
    """Implement forward, reflected and inplace binary methods with a ufunc."""
    return (_binary_method(ufunc, name),
            _reflected_binary_method(ufunc, name),
            _inplace_binary_method(ufunc, name))
def _unary_method(ufunc, name):
    """Implement a unary special method with a ufunc."""
    def func(self):
        return ufunc(self)
class NDArrayOperatorsMixin:
    """Mixin defining all operator special methods using __array_ufunc__.
    This class implements the special methods for almost all of Python's
    builtin operators defined in the `operator` module, including comparisons
    (``==``, ``>``, etc.) and arithmetic (``+``, ``*``, ``-``, etc.), by
    deferring to the ``__array_ufunc__`` method, which subclasses must
    implement.
    It is useful for writing classes that do not inherit from `numpy.ndarray`,
    but that should support arithmetic and numpy universal functions like
    arrays as described in `A Mechanism for Overriding Ufuncs
    <https://numpy.org/neps/nep-0013-ufunc-overrides.html>`_.
    As an trivial example, consider this implementation of an ``ArrayLike``
    class that simply wraps a NumPy array and ensures that the result of any
    arithmetic operation is also an ``ArrayLike`` object::
        class ArrayLike(np.lib.mixins.NDArrayOperatorsMixin):
                self.value = np.asarray(value)
            # One might also consider adding the built-in list type to this
            # list, to support operations like np.add(array_like, list)
            _HANDLED_TYPES = (np.ndarray, numbers.Number)
            def __array_ufunc__(self, ufunc, method, *inputs, **kwargs):
                out = kwargs.get('out', ())
                for x in inputs + out:
                    # Only support operations with instances of _HANDLED_TYPES.
                    # Use ArrayLike instead of type(self) for isinstance to
                    # allow subclasses that don't override __array_ufunc__ to
                    # handle ArrayLike objects.
                    if not isinstance(x, self._HANDLED_TYPES + (ArrayLike,)):
                # Defer to the implementation of the ufunc on unwrapped values.
                inputs = tuple(x.value if isinstance(x, ArrayLike) else x
                               for x in inputs)
                if out:
                    kwargs['out'] = tuple(
                        x.value if isinstance(x, ArrayLike) else x
                        for x in out)
                result = getattr(ufunc, method)(*inputs, **kwargs)
                if type(result) is tuple:
                    # multiple return values
                    return tuple(type(self)(x) for x in result)
                elif method == 'at':
                    # no return value
                    # one return value
                    return type(self)(result)
                return '%s(%r)' % (type(self).__name__, self.value)
    In interactions between ``ArrayLike`` objects and numbers or numpy arrays,
    the result is always another ``ArrayLike``:
        >>> x = ArrayLike([1, 2, 3])
        >>> x - 1
        ArrayLike(array([0, 1, 2]))
        >>> 1 - x
        ArrayLike(array([ 0, -1, -2]))
        >>> np.arange(3) - x
        ArrayLike(array([-1, -1, -1]))
        >>> x - np.arange(3)
        ArrayLike(array([1, 1, 1]))
    Note that unlike ``numpy.ndarray``, ``ArrayLike`` does not allow operations
    with arbitrary, unrecognized types. This ensures that interactions with
    ArrayLike preserve a well-defined casting hierarchy.
    # Like np.ndarray, this mixin class implements "Option 1" from the ufunc
    # overrides NEP.
    # comparisons don't have reflected and in-place versions
    __lt__ = _binary_method(um.less, 'lt')
    __le__ = _binary_method(um.less_equal, 'le')
    __eq__ = _binary_method(um.equal, 'eq')
    __ne__ = _binary_method(um.not_equal, 'ne')
    __gt__ = _binary_method(um.greater, 'gt')
    __ge__ = _binary_method(um.greater_equal, 'ge')
    # numeric methods
    __add__, __radd__, __iadd__ = _numeric_methods(um.add, 'add')
    __sub__, __rsub__, __isub__ = _numeric_methods(um.subtract, 'sub')
    __mul__, __rmul__, __imul__ = _numeric_methods(um.multiply, 'mul')
    __matmul__, __rmatmul__, __imatmul__ = _numeric_methods(
        um.matmul, 'matmul')
    # Python 3 does not use __div__, __rdiv__, or __idiv__
    __truediv__, __rtruediv__, __itruediv__ = _numeric_methods(
        um.true_divide, 'truediv')
    __floordiv__, __rfloordiv__, __ifloordiv__ = _numeric_methods(
        um.floor_divide, 'floordiv')
    __mod__, __rmod__, __imod__ = _numeric_methods(um.remainder, 'mod')
    __divmod__ = _binary_method(um.divmod, 'divmod')
    __rdivmod__ = _reflected_binary_method(um.divmod, 'divmod')
    # __idivmod__ does not exist
    # TODO: handle the optional third argument for __pow__?
    __pow__, __rpow__, __ipow__ = _numeric_methods(um.power, 'pow')
    __lshift__, __rlshift__, __ilshift__ = _numeric_methods(
        um.left_shift, 'lshift')
    __rshift__, __rrshift__, __irshift__ = _numeric_methods(
        um.right_shift, 'rshift')
    __and__, __rand__, __iand__ = _numeric_methods(um.bitwise_and, 'and')
    __xor__, __rxor__, __ixor__ = _numeric_methods(um.bitwise_xor, 'xor')
    __or__, __ror__, __ior__ = _numeric_methods(um.bitwise_or, 'or')
    # unary methods
    __neg__ = _unary_method(um.negative, 'neg')
    __pos__ = _unary_method(um.positive, 'pos')
    __abs__ = _unary_method(um.absolute, 'abs')
    __invert__ = _unary_method(um.invert, 'invert')
