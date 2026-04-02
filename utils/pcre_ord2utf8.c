character value into a UTF8 string. */
*       Convert character value to UTF-8         *
and encodes it as a UTF-8 character in 1 to 6 pcre_uchars.
  buffer     pointer to buffer for result - at least 6 pcre_uchars long
register int i, j;
for (i = 0; i < PRIV(utf8_table1_size); i++)
  if ((int)cvalue <= PRIV(utf8_table1)[i]) break;
buffer += i;
for (j = i; j > 0; j--)
 *buffer-- = 0x80 | (cvalue & 0x3f);
 cvalue >>= 6;
*buffer = PRIV(utf8_table2)[i] | cvalue;
return i + 1;
/* End of pcre_ord2utf8.c */
