/* This module contains an internal function for validating UTF-16 character
strings. */
*         Validate a UTF-16 string                *
/* This function is called (optionally) at the start of compile or match, to
check that a supposed UTF-16 string is actually valid. The early check means
that subsequent code can assume it is dealing with a valid string. The check
can be turned off for maximum performance, but the consequences of supplying an
invalid string are then undefined.
From release 8.21 more information about the details of the error are passed
back in the returned value:
PCRE_UTF16_ERR0  No error
PCRE_UTF16_ERR1  Missing low surrogate at the end of the string
PCRE_UTF16_ERR2  Invalid low surrogate
PCRE_UTF16_ERR3  Isolated low surrogate
PCRE_UTF16_ERR4  Not allowed character
  string       points to the string
  length       length of string, or -1 if the string is zero-terminated
  errp         pointer to an error position offset variable
Returns:       = 0    if the string is a valid UTF-16 string
               > 0    otherwise, setting the offset of the bad character
PRIV(valid_utf)(PCRE_PUCHAR string, int length, int *erroroffset)
#if defined(SUPPORT_UTF) && defined(SUPPORT_UTF_VALIDATION)
register PCRE_PUCHAR p;
register pcre_uchar c;
  for (p = string; *p != 0; p++);
  length = p - string;
for (p = string; length-- > 0; p++)
  c = *p;
  if ((c & 0xf800) != 0xd800)
    /* Normal UTF-16 code point. Neither high nor low surrogate. */
    /* This is probably a BOM from a different byte-order.
    Regardless, the string is rejected. */
    if (c == 0xfffe)
      *erroroffset = p - string;
      return PCRE_UTF16_ERR4;
  else if ((c & 0x0400) == 0)
    /* High surrogate. */
    /* Must be a followed by a low surrogate. */
      return PCRE_UTF16_ERR1;
    p++;
    length--;
    if ((*p & 0xfc00) != 0xdc00)
      return PCRE_UTF16_ERR2;
    /* Isolated low surrogate. Always an error. */
    return PCRE_UTF16_ERR3;
#else  /* SUPPORT_UTF */
(void)(string);  /* Keep picky compilers happy */
(void)(length);
return PCRE_UTF16_ERR0;   /* This indicates success */
/* End of pcre16_valid_utf16.c */
