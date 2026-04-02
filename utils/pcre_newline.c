/* This module contains internal functions for testing newlines when more than
one kind of newline is to be recognized. When a newline is found, its length is
returned. In principle, we could implement several newline "types", each
referring to a different set of newline characters. At present, PCRE supports
only NLTYPE_FIXED, which gets handled without these functions, NLTYPE_ANYCRLF,
and NLTYPE_ANY. The full list of Unicode newline characters is taken from
http://unicode.org/unicode/reports/tr18/. */
*      Check for newline at given position       *
/* It is guaranteed that the initial value of ptr is less than the end of the
string that is being processed.
  ptr          pointer to possible newline
  type         the newline type
  endptr       pointer to the end of the string
  lenptr       where to return the length
  utf          TRUE if in utf mode
Returns:       TRUE or FALSE
BOOL
PRIV(is_newline)(PCRE_PUCHAR ptr, int type, PCRE_PUCHAR endptr, int *lenptr,
(void)utf;
  GETCHAR(c, ptr);
if (type == NLTYPE_ANYCRLF) switch(c)
  case 0x000a: *lenptr = 1; return TRUE;             /* LF */
  case 0x000d: *lenptr = (ptr < endptr - 1 && ptr[1] == 0x0a)? 2 : 1;
               return TRUE;                          /* CR */
  default: return FALSE;
/* NLTYPE_ANY */
else switch(c)
  case 0x000a:                                       /* LF */
  case 0x000b:                                       /* VT */
  case 0x000c: *lenptr = 1; return TRUE;             /* FF */
  case 0x0085: *lenptr = utf? 2 : 1; return TRUE;    /* NEL */
  case 0x2028:                                       /* LS */
  case 0x2029: *lenptr = 3; return TRUE;             /* PS */
  case 0x0085:                                       /* NEL */
  case 0x2029: *lenptr = 1; return TRUE;             /* PS */
*     Check for newline at previous position     *
/* It is guaranteed that the initial value of ptr is greater than the start of
the string that is being processed.
  startptr     pointer to the start of the string
PRIV(was_newline)(PCRE_PUCHAR ptr, int type, PCRE_PUCHAR startptr, int *lenptr,
  BACKCHAR(ptr);
  case 0x000a: *lenptr = (ptr > startptr && ptr[-1] == 0x0d)? 2 : 1;
               return TRUE;                         /* LF */
  case 0x000d: *lenptr = 1; return TRUE;            /* CR */
  case 0x000b:                                      /* VT */
  case 0x000c:                                      /* FF */
  case 0x0085: *lenptr = utf? 2 : 1; return TRUE;   /* NEL */
  case 0x2028:                                      /* LS */
  case 0x2029: *lenptr = 3; return TRUE;            /* PS */
/* End of pcre_newline.c */
