/* This module contains the external function pcre_refcount(), which is an
auxiliary function that can be used to maintain a reference count in a compiled
pattern data block. This might be helpful in applications where the block is
shared by different users. */
*           Maintain reference count             *
/* The reference count is a 16-bit field, initialized to zero. It is not
possible to transfer a non-zero count from one host to a different host that
has a different byte order - though I can't see why anyone in their right mind
would ever want to do that!
  argument_re   points to compiled code
  adjust        value to add to the count
Returns:        the (possibly updated) count value (a non-negative number), or
                a negative error number
pcre_refcount(pcre *argument_re, int adjust)
pcre16_refcount(pcre16 *argument_re, int adjust)
if (re == NULL) return PCRE_ERROR_NULL;
if (re->magic_number != MAGIC_NUMBER) return PCRE_ERROR_BADMAGIC;
re->ref_count = (-adjust > re->ref_count)? 0 :
                (adjust + re->ref_count > 65535)? 65535 :
                re->ref_count + adjust;
return re->ref_count;
/* End of pcre_refcount.c */
