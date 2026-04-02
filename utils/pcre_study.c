/* This module contains the external function pcre_study(), along with local
supporting functions. */
#define SET_BIT(c) start_bits[c/8] |= (1 << (c&7))
/* Returns from set_start_bits() */
enum { SSB_FAIL, SSB_DONE, SSB_CONTINUE, SSB_UNKNOWN };
*   Find the minimum subject length for a group  *
/* Scan a parenthesized group and compute the minimum length of subject that
is needed to match it. This is a lower bound; it does not mean there is a
string of that length that matches. In UTF8 mode, the result is in characters
rather than bytes.
  code            pointer to start of group (the bracket)
  startcode       pointer to start of the whole pattern
  options         the compiling options
  int             RECURSE depth
Returns:   the minimum length
           -1 if \C in UTF-8 mode or (*ACCEPT) was encountered
           -2 internal error (missing capturing bracket)
           -3 internal error (opcode not listed)
find_minlength(const pcre_uchar *code, const pcre_uchar *startcode, int options,
  int recurse_depth)
BOOL had_recurse = FALSE;
register pcre_uchar *cc = (pcre_uchar *)code + 1 + LINK_SIZE;
if (*code == OP_CBRA || *code == OP_SCBRA ||
    *code == OP_CBRAPOS || *code == OP_SCBRAPOS) cc += IMM2_SIZE;
  int d, min;
  pcre_uchar *cs, *ce;
    /* If there is only one branch in a condition, the implied branch has zero
    length, so we don't add anything. This covers the DEFINE "condition"
    automatically. */
    cs = cc + GET(cc, 1);
    if (*cs != OP_ALT)
      cc = cs + 1 + LINK_SIZE;
    /* Otherwise we can fall through and treat it the same as any other
    d = find_minlength(cc, startcode, options, recurse_depth);
    /* ACCEPT makes things far too complicated; we have to give up. */
    /* Reached end of a branch; if it's a ket it is the end of a nested
    call. If it's ALT it is an alternation in a nested call. If it is END it's
    the end of the outer call. All can be handled by the same code. If an
    ACCEPT was previously encountered, use the length that was in force at that
    time, and pass back the shortest ACCEPT length. */
    if (length < 0 || (!had_recurse && branchlength < length))
      length = branchlength;
    if (op != OP_ALT) return length;
    had_recurse = FALSE;
    /* Skip over a subpattern that has a {0} or {0,x} quantifier */
    /* Handle literal characters and + repetitions */
    cc += (cc[1] == OP_PROP || cc[1] == OP_NOTPROP)? 4 : 2;
    cc += 2 + IMM2_SIZE + ((cc[1 + IMM2_SIZE] == OP_PROP
      || cc[1 + IMM2_SIZE] == OP_NOTPROP)? 2 : 0);
    /* Handle single-char non-literal matchers */
    /* "Any newline" might match two characters, but it also might match just
    one. */
    branchlength += 1;
    /* The single-byte matcher means we can't proceed in UTF-8 mode. (In
    non-UTF-8 mode \C will actually be turned into OP_ALLANY, so won't ever
    appear, but leave the code, just in case.) */
    if (utf) return -1;
    /* For repeated character types, we have to test for \p and \P, which have
    an extra two bytes of parameters. */
    if (cc[1] == OP_PROP || cc[1] == OP_NOTPROP) cc += 2;
    cc += PRIV(OP_lengths)[op];
    if (cc[1 + IMM2_SIZE] == OP_PROP
      || cc[1 + IMM2_SIZE] == OP_NOTPROP) cc += 2;
    /* Backreferences and subroutine calls are treated in the same way: we find
    the minimum length for the subpattern. A recursion, however, causes an
    a flag to be set that causes the length of this branch to be ignored. The
    logic is that a recursion can only make sense if there is another
    alternation that stops the recursing. That will provide the minimum length
    (when no recursion happens). A backreference within the group that it is
    referencing behaves in the same way.
    If PCRE_JAVASCRIPT_COMPAT is set, a backreference to an unset bracket
    matches an empty string (by default it causes a matching failure), so in
    that case we must set the minimum length to zero. */
    if ((options & PCRE_JAVASCRIPT_COMPAT) == 0)
      ce = cs = (pcre_uchar *)PRIV(find_bracket)(startcode, utf, GET2(cc, 1));
      if (cs == NULL) return -2;
      do ce += GET(ce, 1); while (*ce == OP_ALT);
      if (cc > cs && cc < ce)
        d = 0;
        had_recurse = TRUE;
        d = find_minlength(cs, startcode, options, recurse_depth);
    else d = 0;
    /* Handle repeated back references */
      min = GET2(cc, 1);
    branchlength += min * d;
    /* We can easily detect direct recursion, but not mutual recursion. This is
    caught by a recursion depth count. */
    cs = ce = (pcre_uchar *)startcode + GET(cc, 1);
    if ((cc > cs && cc < ce) || recurse_depth > 10)
      branchlength += find_minlength(cs, startcode, options, recurse_depth + 1);
    /* Anything else does not or need not match a character. We can get the
    item's length from the table, but for those that can match zero occurrences
    of a character, we must take special action for UTF-8 characters. As it
    happens, the "NOT" versions of these opcodes are used at present only for
    ASCII characters, so they could be omitted from this list. However, in
    future that may change, so we include them here so as not to leave a
    gotcha for a future maintainer. */
    /* Skip these, but we need to add in the name length. */
    cc += PRIV(OP_lengths)[op] + cc[1];
    /* The remaining opcodes are just skipped over. */
    /* This should not occur: we list all opcodes explicitly so that when
    new ones get added they are properly considered. */
    return -3;
*      Set a bit and maybe its alternate case    *
/* Given a character, set its first byte's bit in the table, and also the
corresponding bit for the other version of a letter if we are caseless. In
UTF-8 mode, for characters greater than 127, we can only do the caseless thing
when Unicode property support is available.
  start_bits    points to the bit map
  p             points to the character
  caseless      the caseless flag
  cd            the block with char table pointers
  utf           TRUE for UTF-8 / UTF-16 mode
Returns:        pointer after the character
set_table_bit(pcre_uint8 *start_bits, const pcre_uchar *p, BOOL caseless,
  compile_data *cd, BOOL utf)
unsigned int c = *p;
SET_BIT(c);
if (utf && c > 127)
  GETCHARINC(c, p);
    pcre_uchar buff[6];
    c = UCD_OTHERCASE(c);
    (void)PRIV(ord2utf)(c, buff);
    SET_BIT(buff[0]);
/* Not UTF-8 mode, or character is less than 127. */
if (caseless && (cd->ctypes[c] & ctype_letter) != 0) SET_BIT(cd->fcc[c]);
return p + 1;
if (c > 0xff)
  c = 0xff;
  caseless = FALSE;
*     Set bits for a positive character type     *
/* This function sets starting bits for a character type. In UTF-8 mode, we can
only do a direct setting for bytes less than 128, as otherwise there can be
confusion with bytes in the middle of UTF-8 characters. In a "traditional"
environment, the tables will only recognize ASCII characters anyway, but in at
least one Windows environment, some higher bytes bits were set in the tables.
So we deal with that case by considering the UTF-8 encoding.
  start_bits     the starting bitmap
  cbit type      the type of character wanted
  table_limit    32 for non-UTF-8; 16 for UTF-8
  cd             the block with char table pointers
Returns:         nothing
set_type_bits(pcre_uint8 *start_bits, int cbit_type, int table_limit,
  compile_data *cd)
for (c = 0; c < table_limit; c++) start_bits[c] |= cd->cbits[c+cbit_type];
if (table_limit == 32) return;
for (c = 128; c < 256; c++)
  if ((cd->cbits[c/8] & (1 << (c&7))) != 0)
*     Set bits for a negative character type     *
/* This function sets starting bits for a negative character type such as \D.
In UTF-8 mode, we can only do a direct setting for bytes less than 128, as
otherwise there can be confusion with bytes in the middle of UTF-8 characters.
Unlike in the positive case, where we can set appropriate starting bits for
specific high-valued UTF-8 characters, in this case we have to set the bits for
all high-valued characters. The lowest is 0xc2, but we overkill by starting at
0xc0 (192) for simplicity.
set_nottype_bits(pcre_uint8 *start_bits, int cbit_type, int table_limit,
for (c = 0; c < table_limit; c++) start_bits[c] |= ~cd->cbits[c+cbit_type];
if (table_limit != 32) for (c = 24; c < 32; c++) start_bits[c] = 0xff;
*          Create bitmap of starting bytes       *
/* This function scans a compiled unanchored expression recursively and
attempts to build a bitmap of the set of possible starting bytes. As time goes
by, we may be able to get more clever at doing this. The SSB_CONTINUE return is
useful for parenthesized groups in patterns such as (a*)b where the group
provides some optional starting bytes but scanning must continue at the outer
level to find at least one mandatory byte. At the outermost level, this
function fails unless the result is SSB_DONE.
  code         points to an expression
  start_bits   points to a 32-byte table, initialized to 0
  utf          TRUE if in UTF-8 / UTF-16 mode
  cd           the block with char table pointers
Returns:       SSB_FAIL     => Failed to find any starting bytes
               SSB_DONE     => Found mandatory starting bytes
               SSB_CONTINUE => Found optional starting bytes
               SSB_UNKNOWN  => Hit an unrecognized opcode
set_start_bits(const pcre_uchar *code, pcre_uint8 *start_bits, BOOL utf,
int yield = SSB_DONE;
int table_limit = utf? 16:32;
int table_limit = 32;
/* ========================================================================= */
/* The following comment and code was inserted in January 1999. In May 2006,
when it was observed to cause compiler warnings about unused values, I took it
out again. If anybody is still using OS/2, they will have to put it back
manually. */
/* This next statement and the later reference to dummy are here in order to
trick the optimizer of the IBM C compiler for OS/2 into generating correct
code. Apparently IBM isn't going to fix the problem, and we would rather not
disable optimization (in this module it actually makes a big difference, and
the pcre module can use all the optimization it can get). */
volatile int dummy;
  BOOL try_next = TRUE;
  const pcre_uchar *tcode = code + 1 + LINK_SIZE;
      *code == OP_CBRAPOS || *code == OP_SCBRAPOS) tcode += IMM2_SIZE;
  while (try_next)    /* Loop for items in this branch */
    switch(*tcode)
      /* If we reach something we don't understand, it means a new opcode has
      been created that hasn't been added to this code. Hopefully this problem
      will be discovered during testing. */
      return SSB_UNKNOWN;
      /* Fail for a valid opcode that implies no starting bits. */
      return SSB_FAIL;
      /* We can ignore word boundary tests. */
      tcode++;
      /* If we hit a bracket or a positive lookahead assertion, recurse to set
      bits from within the subpattern. If it can't find anything, we have to
      give up. If it finds some mandatory character(s), we are done for this
      branch. Otherwise, carry on scanning after the subpattern. */
      rc = set_start_bits(tcode, start_bits, utf, cd);
      if (rc == SSB_FAIL || rc == SSB_UNKNOWN) return rc;
      if (rc == SSB_DONE) try_next = FALSE; else
        do tcode += GET(tcode, 1); while (*tcode == OP_ALT);
        tcode += 1 + LINK_SIZE;
      /* If we hit ALT or KET, it means we haven't found anything mandatory in
      this branch, though we might have found something optional. For ALT, we
      continue with the next alternative, but we have to arrange that the final
      result from subpattern is SSB_CONTINUE rather than SSB_DONE. For KET,
      return SSB_CONTINUE: if this is the top level, that indicates failure,
      but after a nested subpattern, it causes scanning to continue. */
      yield = SSB_CONTINUE;
      try_next = FALSE;
      return SSB_CONTINUE;
      /* Skip over callout */
      tcode += 1 /* opcode */ + 1 /* arglen */ + tcode[1] /* arg */ + 1 /* \0 */ + 1 /* callout number */ + 2*LINK_SIZE; /* AutoHotkey */
      /* Skip over lookbehind and negative lookahead assertions */
      /* BRAZERO does the bracket, but carries on. */
      rc = set_start_bits(++tcode, start_bits, utf, cd);
/* =========================================================================
      See the comment at the head of this function concerning the next line,
      which was an old fudge for the benefit of OS/2.
      dummy = 1;
  ========================================================================= */
      do tcode += GET(tcode,1); while (*tcode == OP_ALT);
      /* SKIPZERO skips the bracket. */
      /* Single-char * or ? sets the bit and tries the next item */
      tcode = set_table_bit(start_bits, tcode + 1, FALSE, cd, utf);
      tcode = set_table_bit(start_bits, tcode + 1, TRUE, cd, utf);
      /* Single-char upto sets the bit and tries the next */
      tcode = set_table_bit(start_bits, tcode + 1 + IMM2_SIZE, FALSE, cd, utf);
      tcode = set_table_bit(start_bits, tcode + 1 + IMM2_SIZE, TRUE, cd, utf);
      /* At least one single char sets the bit and stops */
      tcode += IMM2_SIZE;
      (void)set_table_bit(start_bits, tcode + 1, FALSE, cd, utf);
      (void)set_table_bit(start_bits, tcode + 1, TRUE, cd, utf);
      /* Special spacing and line-terminating items. These recognize specific
      lists of characters. The difference between VSPACE and ANYNL is that the
      latter can match the two-character CRLF sequence, but that is not
      relevant for finding the first character, so their code here is
      identical. */
      SET_BIT(0x09);
      SET_BIT(0x20);
        SET_BIT(0xC2);  /* For U+00A0 */
        SET_BIT(0xE1);  /* For U+1680, U+180E */
        SET_BIT(0xE2);  /* For U+2000 - U+200A, U+202F, U+205F */
        SET_BIT(0xE3);  /* For U+3000 */
        SET_BIT(0xA0);
        SET_BIT(0xFF);  /* For characters > 255 */
      SET_BIT(0x0A);
      SET_BIT(0x0B);
      SET_BIT(0x0C);
      SET_BIT(0x0D);
        SET_BIT(0xC2);  /* For U+0085 */
        SET_BIT(0xE2);  /* For U+2028, U+2029 */
        SET_BIT(0x85);
      /* Single character types set the bits and stop. Note that if PCRE_UCP
      is set, we do not see these op codes because \d etc are converted to
      properties. Therefore, these apply in the case when only characters less
      than 256 are recognized to match the types. */
      set_nottype_bits(start_bits, cbit_digit, table_limit, cd);
      set_type_bits(start_bits, cbit_digit, table_limit, cd);
      /* The cbit_space table has vertical tab as whitespace; we have to
      ensure it is set as not whitespace. */
      set_nottype_bits(start_bits, cbit_space, table_limit, cd);
      start_bits[1] |= 0x08;
      not set it from the table. */
      c = start_bits[1];    /* Save in case it was already set */
      set_type_bits(start_bits, cbit_space, table_limit, cd);
      start_bits[1] = (start_bits[1] & ~0x08) | c;
      set_nottype_bits(start_bits, cbit_word, table_limit, cd);
      set_type_bits(start_bits, cbit_word, table_limit, cd);
      /* One or more character type fudges the pointer and restarts, knowing
      it will hit a single character type and stop there. */
      tcode += 1 + IMM2_SIZE;
      /* Zero or more repeats of character types set the bits and then
      try again. */
      tcode += IMM2_SIZE;  /* Fall through */
      switch(tcode[1])
        ensure it gets set as not whitespace. */
        avoid setting it. */
      tcode += 2;
      /* Character class where all the information is in a bit map: set the
      bits and either carry on or not, according to the repeat count. If it was
      a negative class, and we are operating with UTF-8 characters, any byte
      with a value >= 0xc4 is a potentially valid starter because it starts a
      character with a value > 255. */
        start_bits[24] |= 0xf0;              /* Bits for 0xc4 - 0xc8 */
        memset(start_bits+25, 0xff, 7);      /* Bits for 0xc9 - 0xff */
      SET_BIT(0xFF);                         /* For characters > 255 */
        pcre_uint8 *map;
        map = (pcre_uint8 *)tcode;
        /* In UTF-8 mode, the bits in a bit map correspond to character
        values, not to byte values. However, the bit map we are constructing is
        for byte values. So we have to do a conversion for characters whose
        value is > 127. In fact, there are only two possible starting bytes for
        characters in the range 128 - 255. */
          for (c = 0; c < 16; c++) start_bits[c] |= map[c];
            if ((map[c/8] && (1 << (c&7))) != 0)
              int d = (c >> 6) | 0xc0;            /* Set bit for this starter */
              start_bits[d/8] |= (1 << (d&7));    /* and then skip on to the */
              c = (c & 0xc0) + 0x40 - 1;          /* next relevant character. */
          /* In non-UTF-8 mode, the two bit maps are completely compatible. */
          for (c = 0; c < 32; c++) start_bits[c] |= map[c];
        /* Advance past the bit map, and act on what follows. For a zero
        minimum repeat, continue; otherwise stop processing. */
        tcode += 32 / sizeof(pcre_uchar);
        switch (*tcode)
          if (GET2(tcode, 1) == 0) tcode += 1 + 2 * IMM2_SIZE;
            else try_next = FALSE;
      break; /* End of bitmap class handling */
      }      /* End of switch */
    }        /* End of try_next loop */
  code += GET(code, 1);   /* Advance to next branch */
*          Study a compiled expression           *
/* This function is handed a compiled expression that it must study to produce
information that will speed up the matching. It returns a pcre[16]_extra block
which then gets handed back to pcre_exec().
  re        points to the compiled expression
  options   contains option bits
  errorptr  points to where to place error messages;
            set NULL unless error
Returns:    pointer to a pcre[16]_extra block, with study_data filled in and
              the appropriate flags set;
            NULL on error or if no optimization possible
PCRE_EXP_DEFN pcre_extra * PCRE_CALL_CONVENTION
pcre_study(const pcre *external_re, int options, const char **errorptr)
PCRE_EXP_DEFN pcre16_extra * PCRE_CALL_CONVENTION
pcre16_study(const pcre16 *external_re, int options, const char **errorptr)
BOOL bits_set = FALSE;
pcre_uint8 start_bits[32];
PUBL(extra) *extra = NULL;
const REAL_PCRE *re = (const REAL_PCRE *)external_re;
if (re == NULL || re->magic_number != MAGIC_NUMBER)
  *errorptr = "argument is not a compiled regular expression";
if ((re->flags & PCRE_MODE) == 0)
  *errorptr = "argument is compiled in 16 bit mode";
  *errorptr = "argument is compiled in 8 bit mode";
if ((options & ~PUBLIC_STUDY_OPTIONS) != 0)
  *errorptr = "unknown or incorrect option bit(s) set";
code = (pcre_uchar *)re + re->name_table_offset +
  (re->name_count * re->name_entry_size);
/* For an anchored pattern, or an unanchored pattern that has a first char, or
a multiline pattern that matches only at "line starts", there is no point in
seeking a list of starting bytes. */
if ((re->options & PCRE_ANCHORED) == 0 &&
    (re->flags & (PCRE_FIRSTSET|PCRE_STARTLINE)) == 0)
  /* Set the character tables in the block that is passed around */
  if (tables == NULL)
    (void)pcre_fullinfo(external_re, NULL, PCRE_INFO_DEFAULT_TABLES,
    (void *)(&tables));
    (void)pcre16_fullinfo(external_re, NULL, PCRE_INFO_DEFAULT_TABLES,
  compile_block.lcc = tables + lcc_offset;
  compile_block.fcc = tables + fcc_offset;
  compile_block.cbits = tables + cbits_offset;
  compile_block.ctypes = tables + ctypes_offset;
  /* See if we can find a fixed set of initial characters for the pattern. */
  memset(start_bits, 0, 32 * sizeof(pcre_uint8));
  rc = set_start_bits(code, start_bits, (re->options & PCRE_UTF8) != 0,
    &compile_block);
  bits_set = rc == SSB_DONE;
  if (rc == SSB_UNKNOWN)
    *errorptr = "internal error: opcode not recognized";
/* Find the minimum length of subject string. */
switch(min = find_minlength(code, code, re->options, 0))
  case -2: *errorptr = "internal error: missing capturing bracket"; return NULL;
  case -3: *errorptr = "internal error: opcode not recognized"; return NULL;
/* If a set of starting bytes has been identified, or if the minimum length is
greater than zero, or if JIT optimization has been requested, get a
pcre[16]_extra block and a pcre_study_data block. The study data is put in the
latter, which is pointed to by the former, which may also get additional data
set later by the calling program. At the moment, the size of pcre_study_data
is fixed. We nevertheless save it in a field for returning via the
pcre_fullinfo() function so that if it becomes variable in the future,
we don't have to change that code. */
if (bits_set || min > 0
    || (options & PCRE_STUDY_JIT_COMPILE) != 0
  extra = (PUBL(extra) *)(PUBL(malloc))
    (sizeof(PUBL(extra)) + sizeof(pcre_study_data));
  if (extra == NULL)
    *errorptr = "failed to get memory";
  study = (pcre_study_data *)((char *)extra + sizeof(PUBL(extra)));
  extra->flags = PCRE_EXTRA_STUDY_DATA;
  extra->study_data = study;
  study->size = sizeof(pcre_study_data);
  study->flags = 0;
  /* Set the start bits always, to avoid unset memory errors if the
  study data is written to a file, but set the flag only if any of the bits
  are set, to save time looking when none are. */
  if (bits_set)
    study->flags |= PCRE_STUDY_MAPPED;
    memcpy(study->start_bits, start_bits, sizeof(start_bits));
  else memset(study->start_bits, 0, 32 * sizeof(pcre_uint8));
    pcre_uint8 *ptr = start_bits;
    printf("Start bits:\n");
    for (i = 0; i < 32; i++)
      printf("%3d: %02x%s", i * 8, *ptr++, ((i + 1) & 0x7) != 0? " " : "\n");
  /* Always set the minlength value in the block, because the JIT compiler
  makes use of it. However, don't set the bit unless the length is greater than
  zero - the interpretive pcre_exec() and pcre_dfa_exec() needn't waste time
  checking the zero case. */
    study->flags |= PCRE_STUDY_MINLEN;
    study->minlength = min;
  else study->minlength = 0;
  /* If JIT support was compiled and requested, attempt the JIT compilation.
  If no starting bytes were found, and the minimum length is zero, and JIT
  compilation fails, abandon the extra block and return NULL. */
  extra->executable_jit = NULL;
  if ((options & PCRE_STUDY_JIT_COMPILE) != 0) PRIV(jit_compile)(re, extra);
  if (study->flags == 0 && (extra->flags & PCRE_EXTRA_EXECUTABLE_JIT) == 0)
    pcre_free_study(extra);
    pcre16_free_study(extra);
    extra = NULL;
return extra;
*          Free the study data                   *
/* This function frees the memory that was obtained by pcre_study().
Argument:   a pointer to the pcre[16]_extra block
PCRE_EXP_DEFN void
pcre_free_study(pcre_extra *extra)
pcre16_free_study(pcre16_extra *extra)
if ((extra->flags & PCRE_EXTRA_EXECUTABLE_JIT) != 0 &&
  PRIV(jit_free)(extra->executable_jit);
PUBL(free)(extra);
/* End of pcre_study.c */
