/* This module contains an internal function that is used to match an extended
class. It is used by both pcre_exec() and pcre_def_exec(). */
*           Compare string utilities             *
/* The following two functions compares two strings. Basically an strcmp
for non 8 bit characters.
  str1        first string
  str2        second string
Returns:      0 if both string are equal (like strcmp), 1 otherwise
PRIV(strcmp_uc_uc)(const pcre_uchar *str1, const pcre_uchar *str2)
pcre_uchar c1;
pcre_uchar c2;
while (*str1 != '\0' || *str2 != '\0')
  c1 = *str1++;
  c2 = *str2++;
  if (c1 != c2)
    return ((c1 > c2) << 1) - 1;
/* Both length and characters must be equal. */
PRIV(strcmp_uc_c8)(const pcre_uchar *str1, const char *str2)
const pcre_uint8 *ustr2 = (pcre_uint8 *)str2;
while (*str1 != '\0' || *ustr2 != '\0')
  c2 = (pcre_uchar)*ustr2++;
/* The following two functions compares two, fixed length
strings. Basically an strncmp for non 8 bit characters.
  num         size of the string
PRIV(strncmp_uc_uc)(const pcre_uchar *str1, const pcre_uchar *str2, unsigned int num)
while (num-- > 0)
PRIV(strncmp_uc_c8)(const pcre_uchar *str1, const char *str2, unsigned int num)
/* The following function returns with the length of
a zero terminated string. Basically an strlen for non 8 bit characters.
  str         string
Returns:      length of the string
unsigned int
PRIV(strlen_uc)(const pcre_uchar *str)
unsigned int len = 0;
while (*str++ != 0)
  len++;
/* End of pcre_string_utils.c */
