/* This module contains some convenience functions for extracting substrings
from the subject string after a regex match has succeeded. The original idea
for these functions came from Scott Wimer. */
*           Find number for named string         *
/* This function is used by the get_first_set() function below, as well
as being generally available. It assumes that names are unique.
  code        the compiled regex
  stringname  the name whose number is required
Returns:      the number of the named parentheses, or a negative number
                (PCRE_ERROR_NOSUBSTRING) if not found
pcre_get_stringnumber(const pcre *code, const char *stringname)
pcre16_get_stringnumber(const pcre16 *code, PCRE_SPTR16 stringname)
int entrysize;
int top, bot;
pcre_uchar *nametable;
if ((rc = pcre_fullinfo(code, NULL, PCRE_INFO_NAMECOUNT, &top)) != 0)
if (top <= 0) return PCRE_ERROR_NOSUBSTRING;
if ((rc = pcre_fullinfo(code, NULL, PCRE_INFO_NAMEENTRYSIZE, &entrysize)) != 0)
if ((rc = pcre_fullinfo(code, NULL, PCRE_INFO_NAMETABLE, &nametable)) != 0)
if ((rc = pcre16_fullinfo(code, NULL, PCRE_INFO_NAMECOUNT, &top)) != 0)
if ((rc = pcre16_fullinfo(code, NULL, PCRE_INFO_NAMEENTRYSIZE, &entrysize)) != 0)
if ((rc = pcre16_fullinfo(code, NULL, PCRE_INFO_NAMETABLE, &nametable)) != 0)
while (top > bot)
  int mid = (top + bot) / 2;
  pcre_uchar *entry = nametable + entrysize*mid;
  int c = STRCMP_UC_UC((pcre_uchar *)stringname,
    (pcre_uchar *)(entry + IMM2_SIZE));
  if (c == 0) return GET2(entry, 0);
  if (c > 0) bot = mid + 1; else top = mid;
return PCRE_ERROR_NOSUBSTRING;
*     Find (multiple) entries for named string   *
/* This is used by the get_first_set() function below, as well as being
generally available. It is used when duplicated names are permitted.
  stringname  the name whose entries required
  firstptr    where to put the pointer to the first entry
  lastptr     where to put the pointer to the last entry
Returns:      the length of each entry, or a negative number
pcre_get_stringtable_entries(const pcre *code, const char *stringname,
  char **firstptr, char **lastptr)
pcre16_get_stringtable_entries(const pcre16 *code, PCRE_SPTR16 stringname,
  PCRE_UCHAR16 **firstptr, PCRE_UCHAR16 **lastptr)
pcre_uchar *nametable, *lastentry;
lastentry = nametable + entrysize * (top - 1);
    pcre_uchar *first = entry;
    pcre_uchar *last = entry;
    while (first > nametable)
      if (STRCMP_UC_UC((pcre_uchar *)stringname,
        (pcre_uchar *)(first - entrysize + IMM2_SIZE)) != 0) break;
      first -= entrysize;
    while (last < lastentry)
        (pcre_uchar *)(last + entrysize + IMM2_SIZE)) != 0) break;
      last += entrysize;
    *firstptr = (char *)first;
    *lastptr = (char *)last;
    *firstptr = (PCRE_UCHAR16 *)first;
    *lastptr = (PCRE_UCHAR16 *)last;
    return entrysize;
*    Find first set of multiple named strings    *
/* This function allows for duplicate names in the table of named substrings.
It returns the number of the first one that was set in a pattern match.
  code         the compiled regex
  stringname   the name of the capturing substring
  ovector      the vector of matched substrings
Returns:       the number of the first that is set,
               or the number of the last one if none are set,
               or a negative number on error
get_first_set(const pcre *code, const char *stringname, int *ovector)
get_first_set(const pcre16 *code, PCRE_SPTR16 stringname, int *ovector)
const REAL_PCRE *re = (const REAL_PCRE *)code;
pcre_uchar *entry;
char *first, *last;
PCRE_UCHAR16 *first, *last;
if ((re->options & PCRE_DUPNAMES) == 0 && (re->flags & PCRE_JCHANGED) == 0)
  return pcre_get_stringnumber(code, stringname);
entrysize = pcre_get_stringtable_entries(code, stringname, &first, &last);
  return pcre16_get_stringnumber(code, stringname);
entrysize = pcre16_get_stringtable_entries(code, stringname, &first, &last);
if (entrysize <= 0) return entrysize;
for (entry = (pcre_uchar *)first; entry <= (pcre_uchar *)last; entry += entrysize)
  int n = GET2(entry, 0);
  if (ovector[n*2] >= 0) return n;
return GET2(entry, 0);
/* Exported function for AutoHotkey use: */
pcre_get_first_set(const pcre *code, const char *stringname, int *ovector)
pcre16_get_first_set(const pcre16 *code, PCRE_SPTR16 stringname, int *ovector)
return get_first_set(code, stringname, ovector);
*      Copy captured string to given buffer      *
/* This function copies a single captured substring into a given buffer.
Note that we use memcpy() rather than strncpy() in case there are binary zeros
in the string.
  subject        the subject string that was matched
  ovector        pointer to the offsets table
  stringcount    the number of substrings that were captured
                   (i.e. the yield of the pcre_exec call, unless
                   that was zero, in which case it should be 1/3
                   of the offset table size)
  stringnumber   the number of the required substring
  buffer         where to put the substring
  size           the size of the buffer
Returns:         if successful:
                   the length of the copied string, not including the zero
                   that is put on the end; can be zero
                 if not successful:
                   PCRE_ERROR_NOMEMORY (-6) buffer too small
                   PCRE_ERROR_NOSUBSTRING (-7) no such captured substring
pcre_copy_substring(const char *subject, int *ovector, int stringcount,
  int stringnumber, char *buffer, int size)
pcre16_copy_substring(PCRE_SPTR16 subject, int *ovector, int stringcount,
  int stringnumber, PCRE_UCHAR16 *buffer, int size)
int yield;
if (stringnumber < 0 || stringnumber >= stringcount)
stringnumber *= 2;
yield = ovector[stringnumber+1] - ovector[stringnumber];
if (size < yield + 1) return PCRE_ERROR_NOMEMORY;
memcpy(buffer, subject + ovector[stringnumber], IN_UCHARS(yield));
buffer[yield] = 0;
return yield;
*   Copy named captured string to given buffer   *
/* This function copies a single captured substring into a given buffer,
identifying it by name. If the regex permits duplicate names, the first
substring that is set is chosen.
  code           the compiled regex
  stringname     the name of the required substring
pcre_copy_named_substring(const pcre *code, const char *subject,
  int *ovector, int stringcount, const char *stringname,
  char *buffer, int size)
pcre16_copy_named_substring(const pcre16 *code, PCRE_SPTR16 subject,
  int *ovector, int stringcount, PCRE_SPTR16 stringname,
  PCRE_UCHAR16 *buffer, int size)
int n = get_first_set(code, stringname, ovector);
if (n <= 0) return n;
return pcre_copy_substring(subject, ovector, stringcount, n, buffer, size);
return pcre16_copy_substring(subject, ovector, stringcount, n, buffer, size);
*      Copy all captured strings to new store    *
/* This function gets one chunk of store and builds a list of pointers and all
of the captured substrings in it. A NULL pointer is put on the end of the list.
  listptr        set to point to the list of pointers
Returns:         if successful: 0
                   PCRE_ERROR_NOMEMORY (-6) failed to get store
pcre_get_substring_list(const char *subject, int *ovector, int stringcount,
  const char ***listptr)
pcre16_get_substring_list(PCRE_SPTR16 subject, int *ovector, int stringcount,
  PCRE_SPTR16 **listptr)
int size = sizeof(pcre_uchar *);
int double_count = stringcount * 2;
pcre_uchar **stringlist;
pcre_uchar *p;
for (i = 0; i < double_count; i += 2)
  size += sizeof(pcre_uchar *) + IN_UCHARS(ovector[i+1] - ovector[i] + 1);
stringlist = (pcre_uchar **)(PUBL(malloc))(size);
if (stringlist == NULL) return PCRE_ERROR_NOMEMORY;
*listptr = (const char **)stringlist;
*listptr = (PCRE_SPTR16 *)stringlist;
p = (pcre_uchar *)(stringlist + stringcount + 1);
  int len = ovector[i+1] - ovector[i];
  memcpy(p, subject + ovector[i], IN_UCHARS(len));
  *stringlist++ = p;
  p += len;
  *p++ = 0;
*stringlist = NULL;
*   Free store obtained by get_substring_list    *
/* This function exists for the benefit of people calling PCRE from non-C
programs that can call its functions, but not free() or (PUBL(free))()
directly.
Argument:   the result of a previous pcre_get_substring_list()
Returns:    nothing
PCRE_EXP_DEFN void PCRE_CALL_CONVENTION
pcre_free_substring_list(const char **pointer)
pcre16_free_substring_list(PCRE_SPTR16 *pointer)
(PUBL(free))((void *)pointer);
*      Copy captured string to new store         *
/* This function copies a single captured substring into a piece of new
store
  stringptr      where to put a pointer to the substring
                   the length of the string, not including the zero that
                   is put on the end; can be zero
                   PCRE_ERROR_NOSUBSTRING (-7) substring not present
pcre_get_substring(const char *subject, int *ovector, int stringcount,
  int stringnumber, const char **stringptr)
pcre16_get_substring(PCRE_SPTR16 subject, int *ovector, int stringcount,
  int stringnumber, PCRE_SPTR16 *stringptr)
pcre_uchar *substring;
substring = (pcre_uchar *)(PUBL(malloc))(IN_UCHARS(yield + 1));
if (substring == NULL) return PCRE_ERROR_NOMEMORY;
memcpy(substring, subject + ovector[stringnumber], IN_UCHARS(yield));
substring[yield] = 0;
*stringptr = (const char *)substring;
*stringptr = (PCRE_SPTR16)substring;
*   Copy named captured string to new store      *
/* This function copies a single captured substring, identified by name, into
new store. If the regex permits duplicate names, the first substring that is
set is chosen.
  stringptr      where to put the pointer
                   PCRE_ERROR_NOMEMORY (-6) couldn't get memory
pcre_get_named_substring(const pcre *code, const char *subject,
  const char **stringptr)
pcre16_get_named_substring(const pcre16 *code, PCRE_SPTR16 subject,
  PCRE_SPTR16 *stringptr)
return pcre_get_substring(subject, ovector, stringcount, n, stringptr);
return pcre16_get_substring(subject, ovector, stringcount, n, stringptr);
*       Free store obtained by get_substring     *
Argument:   the result of a previous pcre_get_substring()
pcre_free_substring(const char *pointer)
pcre16_free_substring(PCRE_SPTR16 pointer)
/* End of pcre_get.c */
