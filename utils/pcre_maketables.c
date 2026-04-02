/* This module contains the external function pcre_maketables(), which builds
character tables for PCRE in the current locale. The file is compiled on its
own as part of the PCRE library. However, it is also included in the
compilation of dftables.c, in which case the macro DFTABLES is defined. */
#ifndef DFTABLES
#  ifdef HAVE_CONFIG_H
#  include "config.h"
#  endif
#  include "pcre_internal.h"
*           Create PCRE character tables         *
/* This function builds a set of character tables for use by PCRE and returns
a pointer to them. They are build using the ctype functions, and consequently
their contents will depend upon the current locale setting. When compiled as
part of the library, the store is obtained via PUBL(malloc)(), but when
compiled inside dftables, use malloc().
Arguments:   none
Returns:     pointer to the contiguous block of data
const unsigned char *
pcre_maketables(void)
pcre16_maketables(void)
unsigned char *yield, *p;
yield = (unsigned char*)(PUBL(malloc))(tables_length);
yield = (unsigned char*)malloc(tables_length);
if (yield == NULL) return NULL;
p = yield;
/* First comes the lower casing table */
for (i = 0; i < 256; i++) *p++ = tolower(i);
/* Next the case-flipping table */
for (i = 0; i < 256; i++) *p++ = islower(i)? toupper(i) : tolower(i);
/* Then the character class tables. Don't try to be clever and save effort on
exclusive ones - in some locales things may be different. Note that the table
for "space" includes everything "isspace" gives, including VT in the default
locale. This makes it work for the POSIX class [:space:]. Note also that it is
possible for a character to be alnum or alpha without being lower or upper,
such as "male and female ordinals" (\xAA and \xBA) in the fr_FR locale (at
least under Debian Linux's locales as of 12/2005). So we must test for alnum
specially. */
memset(p, 0, cbit_length);
for (i = 0; i < 256; i++)
  if (isdigit(i)) p[cbit_digit  + i/8] |= 1 << (i&7);
  if (isupper(i)) p[cbit_upper  + i/8] |= 1 << (i&7);
  if (islower(i)) p[cbit_lower  + i/8] |= 1 << (i&7);
  if (isalnum(i)) p[cbit_word   + i/8] |= 1 << (i&7);
  if (i == '_')   p[cbit_word   + i/8] |= 1 << (i&7);
  if (isspace(i)) p[cbit_space  + i/8] |= 1 << (i&7);
  if (isxdigit(i))p[cbit_xdigit + i/8] |= 1 << (i&7);
  if (isgraph(i)) p[cbit_graph  + i/8] |= 1 << (i&7);
  if (isprint(i)) p[cbit_print  + i/8] |= 1 << (i&7);
  if (ispunct(i)) p[cbit_punct  + i/8] |= 1 << (i&7);
  if (iscntrl(i)) p[cbit_cntrl  + i/8] |= 1 << (i&7);
p += cbit_length;
/* Finally, the character type table. In this, we exclude VT from the white
space chars, because Perl doesn't recognize it as such for \s and for comments
within regexes. */
  int x = 0;
  if (i != 0x0b && isspace(i)) x += ctype_space;
  if (isalpha(i)) x += ctype_letter;
  if (isdigit(i)) x += ctype_digit;
  if (isxdigit(i)) x += ctype_xdigit;
  if (isalnum(i) || i == '_') x += ctype_word;
  /* Note: strchr includes the terminating zero in the characters it considers.
  In this instance, that is ok because we want binary zero to be flagged as a
  meta-character, which in this sense is any character that terminates a run
  of data characters. */
  if (strchr("\\*+?{^.$|()[", i) != 0) x += ctype_meta;
  *p++ = x;
/* End of pcre_maketables.c */
