*       Match character against an XCLASS        *
/* This function is called to match a character against an extended class that
might contain values > 255 and/or Unicode properties.
  c           the character
  data        points to the flag byte of the XCLASS data
Returns:      TRUE if character matches, else FALSE
PRIV(xclass)(int c, const pcre_uchar *data, BOOL utf)
int t;
BOOL negated = (*data & XCL_NOT) != 0;
/* In 8 bit mode, this must always be TRUE. Help the compiler to know that. */
utf = TRUE;
/* Character values < 256 are matched against a bitmap, if one is present. If
not, we still carry on, because there may be ranges that start below 256 in the
additional data. */
if (c < 256)
  if ((*data & XCL_MAP) != 0 &&
    (((pcre_uint8 *)(data + 1))[c/8] & (1 << (c&7))) != 0)
    return !negated; /* char found */
/* First skip the bit map if present. Then match against the list of Unicode
properties or large chars or ranges that end with a large char. We won't ever
encounter XCL_PROP or XCL_NOTPROP when UCP support is not compiled. */
if ((*data++ & XCL_MAP) != 0) data += 32 / sizeof(pcre_uchar);
while ((t = *data++) != XCL_END)
  if (t == XCL_SINGLE)
      GETCHARINC(x, data); /* macro generates multiple statements */
      x = *data++;
    if (c == x) return !negated;
  else if (t == XCL_RANGE)
      GETCHARINC(y, data); /* macro generates multiple statements */
      y = *data++;
    if (c >= x && c <= y) return !negated;
  else  /* XCL_PROP & XCL_NOTPROP */
    switch(*data)
      if (t == XCL_PROP) return !negated;
      if ((prop->chartype == ucp_Lu || prop->chartype == ucp_Ll ||
           prop->chartype == ucp_Lt) == (t == XCL_PROP)) return !negated;
      if ((data[1] == PRIV(ucp_gentype)[prop->chartype]) == (t == XCL_PROP))
        return !negated;
      if ((data[1] == prop->chartype) == (t == XCL_PROP)) return !negated;
      if ((data[1] == prop->script) == (t == XCL_PROP)) return !negated;
           PRIV(ucp_gentype)[prop->chartype] == ucp_N) == (t == XCL_PROP))
             == (t == XCL_PROP))
           c == CHAR_FF || c == CHAR_CR) == (t == XCL_PROP))
           PRIV(ucp_gentype)[prop->chartype] == ucp_N || c == CHAR_UNDERSCORE)
      /* This should never occur, but compilers may mutter if there is no
      default. */
    data += 2;
return negated;   /* char did not match */
/* End of pcre_xclass.c */
