/* This module contains pcre_exec(), the externally visible function that does
pattern matching using an NFA algorithm, trying to mimic Perl as closely as
possible. There are also some static supporting functions. */
/* Undefine some potentially clashing cpp symbols */
#undef min
#undef max
/* Values for setting in md->match_function_type to indicate two special types
of call to match(). We do it this way to save on using another stack variable,
as stack usage is to be discouraged. */
#define MATCH_CONDASSERT     1  /* Called to check a condition assertion */
#define MATCH_CBEGROUP       2  /* Could-be-empty unlimited repeat group */
/* Non-error returns from the match() function. Error returns are externally
defined PCRE_ERROR_xxx codes, which are all negative. */
#define MATCH_MATCH        1
#define MATCH_NOMATCH      0
/* Special internal returns from the match() function. Make them sufficiently
negative to avoid the external error codes. */
#define MATCH_ACCEPT       (-999)
#define MATCH_COMMIT       (-998)
#define MATCH_KETRPOS      (-997)
#define MATCH_ONCE         (-996)
#define MATCH_PRUNE        (-995)
#define MATCH_SKIP         (-994)
#define MATCH_SKIP_ARG     (-993)
#define MATCH_THEN         (-992)
/* Maximum number of ints of offset to save on the stack for recursive calls.
If the offset vector is bigger, malloc is used. This should be a multiple of 3,
because the offset vector is always a multiple of 3 long. */
#define REC_STACK_SAVE_MAX 30
/* Min and max values for the common repeats; for the maxima, 0 => infinity */
static const char rep_min[] = { 0, 0, 1, 1, 0, 0 };
static const char rep_max[] = { 0, 0, 0, 0, 1, 1 };
*        Debugging function to print chars       *
/* Print a sequence of chars in printable format, stopping at the end of the
subject if the requested.
  p           points to characters
  length      number to print
  is_subject  TRUE if printing from within md->start_subject
  md          pointer to matching data block, if is_subject is TRUE
pchars(const pcre_uchar *p, int length, BOOL is_subject, match_data *md)
unsigned int c;
if (is_subject && length > md->end_subject - p) length = md->end_subject - p;
  if (isprint(c = *(p++))) printf("%c", c); else printf("\\x%02x", c);
*          Match a back-reference                *
/* Normally, if a back reference hasn't been set, the length that is passed is
negative, so the match always fails. However, in JavaScript compatibility mode,
the length passed is zero. Note that in caseless UTF-8 mode, the number of
subject bytes matched may be different to the number of reference bytes.
  offset      index into the offset vector
  eptr        pointer into the subject
  length      length of reference to be matched (number of bytes)
  md          points to match data block
  caseless    TRUE if caseless
Returns:      < 0 if not matched, otherwise the number of subject bytes matched
match_ref(int offset, register PCRE_PUCHAR eptr, int length, match_data *md,
  BOOL caseless)
PCRE_PUCHAR eptr_start = eptr;
register PCRE_PUCHAR p = md->start_subject + md->offset_vector[offset];
if (eptr >= md->end_subject)
  printf("matching subject <null>");
  printf("matching subject ");
  pchars(eptr, length, TRUE, md);
printf(" against backref ");
pchars(p, length, FALSE, md);
/* Always fail if reference not set (and not JavaScript compatible). */
if (length < 0) return -1;
/* Separate the caseless case for speed. In UTF-8 mode we can only do this
properly if Unicode properties are supported. Otherwise, we can check only
ASCII characters. */
  if (md->utf)
    /* Match characters up to the end of the reference. NOTE: the number of
    bytes matched may differ, because there are some characters whose upper and
    lower case versions code as different numbers of bytes. For example, U+023A
    (2 bytes in UTF-8) is the upper case version of U+2C65 (3 bytes in UTF-8);
    a sequence of 3 of the former uses 6 bytes, as does a sequence of two of
    the latter. It is important, therefore, to check the length along the
    reference, not along the subject (earlier code did this wrong). */
    PCRE_PUCHAR endptr = p + length;
    while (p < endptr)
      int c, d;
      if (eptr >= md->end_subject) return -1;
      GETCHARINC(c, eptr);
      GETCHARINC(d, p);
      if (c != d && c != UCD_OTHERCASE(d)) return -1;
  /* The same code works when not in UTF-8 mode and in UTF-8 mode when there
  is no UCP support. */
    if (eptr + length > md->end_subject) return -1;
      if (TABLE_GET(*p, md->lcc, *p) != TABLE_GET(*eptr, md->lcc, *eptr)) return -1;
      eptr++;
/* In the caseful case, we can just compare the bytes, whether or not we
are in UTF-8 mode. */
  while (length-- > 0) if (*p++ != *eptr++) return -1;
return (int)(eptr - eptr_start);
/***************************************************************************
****************************************************************************
                   RECURSION IN THE match() FUNCTION
The match() function is highly recursive, though not every recursive call
increases the recursive depth. Nevertheless, some regular expressions can cause
it to recurse to a great depth. I was writing for Unix, so I just let it call
itself recursively. This uses the stack for saving everything that has to be
saved for a recursive call. On Unix, the stack can be large, and this works
fine.
It turns out that on some non-Unix-like systems there are problems with
programs that use a lot of stack. (This despite the fact that every last chip
has oodles of memory these days, and techniques for extending the stack have
been known for decades.) So....
There is a fudge, triggered by defining NO_RECURSE, which avoids recursive
calls by keeping local variables that need to be preserved in blocks of memory
obtained from malloc() instead instead of on the stack. Macros are used to
achieve this so that the actual code doesn't look very different to what it
always used to.
The original heap-recursive code used longjmp(). However, it seems that this
can be very slow on some operating systems. Following a suggestion from Stan
Switzer, the use of longjmp() has been abolished, at the cost of having to
provide a unique number for each call to RMATCH. There is no way of generating
a sequence of numbers at compile time in C. I have given them names, to make
them stand out more clearly.
Crude tests on x86 Linux show a small speedup of around 5-8%. However, on
FreeBSD, avoiding longjmp() more than halves the time taken to run the standard
tests. Furthermore, not using longjmp() means that local dynamic variables
don't have indeterminate values; this has meant that the frame size can be
reduced because the result can be "passed back" by straight setting of the
variable instead of being passed in the frame.
***************************************************************************/
/* Numbers for RMATCH calls. When this list is changed, the code at HEAP_RETURN
below must be updated in sync.  */
enum { RM1=1, RM2,  RM3,  RM4,  RM5,  RM6,  RM7,  RM8,  RM9,  RM10,
       RM11,  RM12, RM13, RM14, RM15, RM16, RM17, RM18, RM19, RM20,
       RM21,  RM22, RM23, RM24, RM25, RM26, RM27, RM28, RM29, RM30,
       RM31,  RM32, RM33, RM34, RM35, RM36, RM37, RM38, RM39, RM40,
       RM41,  RM42, RM43, RM44, RM45, RM46, RM47, RM48, RM49, RM50,
       RM51,  RM52, RM53, RM54, RM55, RM56, RM57, RM58, RM59, RM60,
       RM61,  RM62, RM63, RM64, RM65, RM66 };
/* These versions of the macros use the stack, as normal. There are debugging
versions and production versions. Note that the "rw" argument of RMATCH isn't
actually used in this definition. */
#ifndef NO_RECURSE
#define REGISTER register
#define RMATCH(ra,rb,rc,rd,re,rw) \
  printf("match() called in line %d\n", __LINE__); \
  rrc = match(ra,rb,mstart,rc,rd,re,rdepth+1); \
  printf("to line %d\n", __LINE__); \
#define RRETURN(ra) \
  printf("match() returned %d from line %d ", ra, __LINE__); \
  return ra; \
  rrc = match(ra,rb,mstart,rc,rd,re,rdepth+1)
#define RRETURN(ra) return ra
/* These versions of the macros manage a private stack on the heap. Note that
the "rd" argument of RMATCH isn't actually used in this definition. It's the md
argument of match(), which never changes. */
#define REGISTER
#define RMATCH(ra,rb,rc,rd,re,rw)\
  heapframe *newframe = (heapframe *)(PUBL(stack_malloc))(sizeof(heapframe));\
  if (newframe == NULL) RRETURN(PCRE_ERROR_NOMEMORY);\
  frame->Xwhere = rw; \
  newframe->Xeptr = ra;\
  newframe->Xecode = rb;\
  newframe->Xmstart = mstart;\
  newframe->Xoffset_top = rc;\
  newframe->Xeptrb = re;\
  newframe->Xrdepth = frame->Xrdepth + 1;\
  newframe->Xprevframe = frame;\
  frame = newframe;\
  DPRINTF(("restarting from line %d\n", __LINE__));\
  goto HEAP_RECURSE;\
  L_##rw:\
  DPRINTF(("jumped back to line %d\n", __LINE__));\
#define RRETURN(ra)\
  heapframe *oldframe = frame;\
  frame = oldframe->Xprevframe;\
  if (oldframe != &frame_zero) (PUBL(stack_free))(oldframe);\
  if (frame != NULL)\
    rrc = ra;\
    goto HEAP_RETURN;\
  return ra;\
/* Structure for remembering the local variables in a private frame */
typedef struct heapframe {
  struct heapframe *Xprevframe;
  /* Function arguments that may change */
  PCRE_PUCHAR Xeptr;
  const pcre_uchar *Xecode;
  PCRE_PUCHAR Xmstart;
  int Xoffset_top;
  eptrblock *Xeptrb;
  unsigned int Xrdepth;
  /* Function local variables */
  PCRE_PUCHAR Xcallpat;
  PCRE_PUCHAR Xcharptr;
  PCRE_PUCHAR Xdata;
  PCRE_PUCHAR Xnext;
  PCRE_PUCHAR Xpp;
  PCRE_PUCHAR Xprev;
  PCRE_PUCHAR Xsaved_eptr;
  recursion_info Xnew_recursive;
  BOOL Xcur_is_word;
  BOOL Xcondition;
  BOOL Xprev_is_word;
  int Xprop_type;
  int Xprop_value;
  int Xprop_fail_result;
  int Xoclength;
  pcre_uchar Xocchars[6];
  int Xcodelink;
  int Xctype;
  unsigned int Xfc;
  int Xfi;
  int Xlength;
  int Xmax;
  int Xmin;
  int Xnumber;
  int Xoffset;
  int Xop;
  int Xsave_capture_last;
  int Xsave_offset1, Xsave_offset2, Xsave_offset3;
  int Xstacksave[REC_STACK_SAVE_MAX];
  eptrblock Xnewptrb;
  /* Where to jump back to */
  int Xwhere;
} heapframe;
*         Match from current position            *
/* This function is called recursively in many circumstances. Whenever it
returns a negative (error) response, the outer incarnation must also return the
same response. */
/* These macros pack up tests that are used for partial matching, and which
appear several times in the code. We set the "hit end" flag if the pointer is
at the end of the subject and also past the start of the subject (i.e.
something has been matched). For hard partial matching, we then return
immediately. The second one is used when we already know we are past the end of
the subject. */
#define CHECK_PARTIAL()\
  if (md->partial != 0 && eptr >= md->end_subject && \
      eptr > md->start_used_ptr) \
    md->hitend = TRUE; \
    if (md->partial > 1) RRETURN(PCRE_ERROR_PARTIAL); \
#define SCHECK_PARTIAL()\
  if (md->partial != 0 && eptr > md->start_used_ptr) \
/* Performance note: It might be tempting to extract commonly used fields from
the md structure (e.g. utf, end_subject) into individual variables to improve
performance. Tests using gcc on a SPARC disproved this; in the first case, it
made performance worse.
   eptr        pointer to current character in subject
   ecode       pointer to current position in compiled code
   mstart      pointer to the current match start position (can be modified
                 by encountering \K)
   offset_top  current top pointer
   md          pointer to "static" info for the match
   eptrb       pointer to chain of blocks containing eptr at start of
                 brackets - for testing for empty matches
   rdepth      the recursion depth
Returns:       MATCH_MATCH if matched            )  these values are >= 0
               MATCH_NOMATCH if failed to match  )
               a negative MATCH_xxx value for PRUNE, SKIP, etc
               a negative PCRE_ERROR_xxx value if aborted by an error condition
                 (e.g. stopped by repeated call or recursion limit)
match(REGISTER PCRE_PUCHAR eptr, REGISTER const pcre_uchar *ecode,
  PCRE_PUCHAR mstart, int offset_top, match_data *md, eptrblock *eptrb,
  unsigned int rdepth)
/* These variables do not need to be preserved over recursion in this function,
so they can be ordinary variables in all cases. Mark some of them with
"register" because they are used a lot in loops. */
register int  rrc;         /* Returns from recursive calls */
register int  i;           /* Used for loops not involving calls to RMATCH() */
register unsigned int c;   /* Character values not kept over RMATCH() calls */
register BOOL utf;         /* Local copy of UTF flag for speed */
BOOL minimize, possessive; /* Quantifier options */
BOOL caseless;
/* When recursion is not being used, all "local" variables that have to be
preserved over calls to RMATCH() are part of a "frame". We set up the top-level
frame on the stack here; subsequent instantiations are obtained from the heap
whenever RMATCH() does a "recursion". See the macro definitions above. Putting
the top-level on the stack rather than malloc-ing them all gives a performance
boost in many cases where there is not much "recursion". */
heapframe frame_zero;
heapframe *frame = &frame_zero;
frame->Xprevframe = NULL;            /* Marks the top level */
/* Copy in the original argument variables */
frame->Xeptr = eptr;
frame->Xecode = ecode;
frame->Xmstart = mstart;
frame->Xoffset_top = offset_top;
frame->Xeptrb = eptrb;
frame->Xrdepth = rdepth;
/* This is where control jumps back to to effect "recursion" */
HEAP_RECURSE:
/* Macros make the argument variables come from the current frame */
#define eptr               frame->Xeptr
#define ecode              frame->Xecode
#define mstart             frame->Xmstart
#define offset_top         frame->Xoffset_top
#define eptrb              frame->Xeptrb
#define rdepth             frame->Xrdepth
/* Ditto for the local variables */
#define charptr            frame->Xcharptr
#define callpat            frame->Xcallpat
#define codelink           frame->Xcodelink
#define data               frame->Xdata
#define next               frame->Xnext
#define pp                 frame->Xpp
#define prev               frame->Xprev
#define saved_eptr         frame->Xsaved_eptr
#define new_recursive      frame->Xnew_recursive
#define cur_is_word        frame->Xcur_is_word
#define condition          frame->Xcondition
#define prev_is_word       frame->Xprev_is_word
#define prop_type          frame->Xprop_type
#define prop_value         frame->Xprop_value
#define prop_fail_result   frame->Xprop_fail_result
#define oclength           frame->Xoclength
#define occhars            frame->Xocchars
#define ctype              frame->Xctype
#define fc                 frame->Xfc
#define fi                 frame->Xfi
#define length             frame->Xlength
#define max                frame->Xmax
#define min                frame->Xmin
#define number             frame->Xnumber
#define offset             frame->Xoffset
#define op                 frame->Xop
#define save_capture_last  frame->Xsave_capture_last
#define save_offset1       frame->Xsave_offset1
#define save_offset2       frame->Xsave_offset2
#define save_offset3       frame->Xsave_offset3
#define stacksave          frame->Xstacksave
#define newptrb            frame->Xnewptrb
/* When recursion is being used, local variables are allocated on the stack and
get preserved during recursion in the normal way. In this environment, fi and
i, and fc and c, can be the same variables. */
#else         /* NO_RECURSE not defined */
#define fi i
#define fc c
/* Many of the following variables are used only in small blocks of the code.
My normal style of coding would have declared them within each of those blocks.
However, in order to accommodate the version of this code that uses an external
"stack" implemented on the heap, it is easier to declare them all here, so the
declarations can be cut out in a block. The only declarations within blocks
below are for variables that do not have to be preserved over a recursive call
to RMATCH(). */
const pcre_uchar *charptr;
const pcre_uchar *callpat;
const pcre_uchar *data;
const pcre_uchar *next;
PCRE_PUCHAR       pp;
const pcre_uchar *prev;
PCRE_PUCHAR       saved_eptr;
recursion_info new_recursive;
BOOL cur_is_word;
BOOL condition;
BOOL prev_is_word;
int prop_type;
int prop_value;
int prop_fail_result;
int oclength;
pcre_uchar occhars[6];
int codelink;
int ctype;
int max;
int min;
int number;
int op;
int save_capture_last;
int save_offset1, save_offset2, save_offset3;
int stacksave[REC_STACK_SAVE_MAX];
eptrblock newptrb;
/* There is a special fudge for calling match() in a way that causes it to
measure the size of its basic stack frame when the stack is being used for
recursion. The second argument (ecode) being NULL triggers this behaviour. It
cannot normally ever be NULL. The return is the negated value of the frame
size. */
if (ecode == NULL)
  if (rdepth == 0)
    return match((PCRE_PUCHAR)&rdepth, NULL, NULL, 0, NULL, NULL, 1);
    int len = (int)((char *)&rdepth - (char *)eptr);
    return (len > 0)? -len : len;
#endif     /* NO_RECURSE */
/* To save space on the stack and in the heap frame, I have doubled up on some
of the local variables that are used only in localised parts of the code, but
still need to be preserved over recursive calls of match(). These macros define
the alternative names that are used. */
#define allow_zero    cur_is_word
#define cbegroup      condition
#define code_offset   codelink
#define condassert    condition
#define matched_once  prev_is_word
#define foc           number
#define save_mark     data
/* These statements are here to stop the compiler complaining about unitialized
variables. */
prop_value = 0;
prop_fail_result = 0;
/* This label is used for tail recursion, which is used in a few cases even
when NO_RECURSE is not defined, in order to reduce the amount of stack that is
used. Thanks to Ian Taylor for noticing this possibility and sending the
original patch. */
TAIL_RECURSE:
/* OK, now we can get on with the real code of the function. Recursive calls
are specified by the macro RMATCH and RRETURN is used to return. When
NO_RECURSE is *not* defined, these just turn into a recursive call to match()
and a "return", respectively (possibly with some debugging if PCRE_DEBUG is
defined). However, RMATCH isn't like a function call because it's quite a
complicated macro. It has to be used in one particular way. This shouldn't,
however, impact performance when true recursion is being used. */
utf = UTF_ENABLED(md->utf);       /* Local copy of the flag */
/* First check that we haven't called match() too many times, or that we
haven't exceeded the recursive call limit. */
if (md->match_call_count++ >= md->match_limit) RRETURN(PCRE_ERROR_MATCHLIMIT);
if (rdepth >= md->match_limit_recursion) RRETURN(PCRE_ERROR_RECURSIONLIMIT);
/* At the start of a group with an unlimited repeat that may match an empty
string, the variable md->match_function_type is set to MATCH_CBEGROUP. It is
done this way to save having to use another function argument, which would take
up space on the stack. See also MATCH_CONDASSERT below.
When MATCH_CBEGROUP is set, add the current subject pointer to the chain of
such remembered pointers, to be checked when we hit the closing ket, in order
to break infinite loops that match no characters. When match() is called in
other circumstances, don't add to the chain. The MATCH_CBEGROUP feature must
NOT be used with tail recursion, because the memory block that is used is on
the stack, so a new one may be required for each match(). */
if (md->match_function_type == MATCH_CBEGROUP)
  newptrb.epb_saved_eptr = eptr;
  newptrb.epb_prev = eptrb;
  eptrb = &newptrb;
  md->match_function_type = 0;
/* Now start processing the opcodes. */
  minimize = possessive = FALSE;
  op = *ecode;
    md->nomatch_mark = ecode + 2;
    md->mark = NULL;    /* In case previously set by assertion */
    RMATCH(eptr, ecode + PRIV(OP_lengths)[*ecode] + ecode[1], offset_top, md,
      eptrb, RM55);
    if ((rrc == MATCH_MATCH || rrc == MATCH_ACCEPT || rrc == MATCH_KETRPOS) &&
         md->mark == NULL) md->mark = ecode + 2;
    /* A return of MATCH_SKIP_ARG means that matching failed at SKIP with an
    argument, and we must check whether that argument matches this MARK's
    argument. It is passed back in md->start_match_ptr (an overloading of that
    variable). If it does match, we reset that variable to the current subject
    position and return MATCH_SKIP. Otherwise, pass back the return code
    unaltered. */
    else if (rrc == MATCH_SKIP_ARG &&
        STRCMP_UC_UC(ecode + 2, md->start_match_ptr) == 0)
      md->start_match_ptr = eptr;
      RRETURN(MATCH_SKIP);
    RRETURN(rrc);
    RRETURN(MATCH_NOMATCH);
    /* COMMIT overrides PRUNE, SKIP, and THEN */
    RMATCH(eptr, ecode + PRIV(OP_lengths)[*ecode], offset_top, md,
      eptrb, RM52);
    if (rrc != MATCH_NOMATCH && rrc != MATCH_PRUNE &&
        rrc != MATCH_SKIP && rrc != MATCH_SKIP_ARG &&
        rrc != MATCH_THEN)
    RRETURN(MATCH_COMMIT);
    /* PRUNE overrides THEN */
      eptrb, RM51);
    if (rrc != MATCH_NOMATCH && rrc != MATCH_THEN) RRETURN(rrc);
    RRETURN(MATCH_PRUNE);
      eptrb, RM56);
    if ((rrc == MATCH_MATCH || rrc == MATCH_ACCEPT) &&
    /* SKIP overrides PRUNE and THEN */
      eptrb, RM53);
    if (rrc != MATCH_NOMATCH && rrc != MATCH_PRUNE && rrc != MATCH_THEN)
    md->start_match_ptr = eptr;   /* Pass back current position */
    /* Note that, for Perl compatibility, SKIP with an argument does NOT set
    nomatch_mark. There is a flag that disables this opcode when re-matching a
    pattern that ended with a SKIP for which there was not a matching MARK. */
    if (md->ignore_skip_arg)
      ecode += PRIV(OP_lengths)[*ecode] + ecode[1];
      eptrb, RM57);
    /* Pass back the current skip name by overloading md->start_match_ptr and
    returning the special MATCH_SKIP_ARG return code. This will either be
    caught by a matching MARK, or get to the top, where it causes a rematch
    with the md->ignore_skip_arg flag set. */
    md->start_match_ptr = ecode + 2;
    RRETURN(MATCH_SKIP_ARG);
    /* For THEN (and THEN_ARG) we pass back the address of the opcode, so that
    the branch in which it occurs can be determined. Overload the start of
    match pointer to do this. */
      eptrb, RM54);
    if (rrc != MATCH_NOMATCH) RRETURN(rrc);
    md->start_match_ptr = ecode;
    RRETURN(MATCH_THEN);
    RMATCH(eptr, ecode + PRIV(OP_lengths)[*ecode] + ecode[1], offset_top,
      md, eptrb, RM58);
    /* Handle an atomic group that does not contain any capturing parentheses.
    This can be handled like an assertion. Prior to 8.13, all atomic groups
    were handled this way. In 8.13, the code was changed as below for ONCE, so
    that backups pass through the group and thereby reset captured values.
    However, this uses a lot more stack, so in 8.20, atomic groups that do not
    contain any captures generate OP_ONCE_NC, which can be handled in the old,
    less stack intensive way.
    Check the alternative branches in turn - the matching won't pass the KET
    for this kind of subpattern. If any one branch matches, we carry on as at
    the end of a normal bracket, leaving the subject pointer, but resetting
    the start-of-match value in case it was changed by \K. */
    prev = ecode;
    saved_eptr = eptr;
    save_mark = md->mark;
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM64);
      if (rrc == MATCH_MATCH)  /* Note: _not_ MATCH_ACCEPT */
        mstart = md->start_match_ptr;
      if (rrc == MATCH_THEN)
        next = ecode + GET(ecode,1);
        if (md->start_match_ptr < next &&
            (*ecode == OP_ALT || *next == OP_ALT))
          rrc = MATCH_NOMATCH;
      ecode += GET(ecode,1);
      md->mark = save_mark;
    while (*ecode == OP_ALT);
    /* If hit the end of the group (which could be repeated), fail */
    if (*ecode != OP_ONCE_NC && *ecode != OP_ALT) RRETURN(MATCH_NOMATCH);
    /* Continue as from after the group, updating the offsets high water
    mark, since extracts may have been taken. */
    do ecode += GET(ecode, 1); while (*ecode == OP_ALT);
    offset_top = md->end_offset_top;
    eptr = md->end_match_ptr;
    /* For a non-repeating ket, just continue at this level. This also
    happens for a repeating ket if no characters were matched in the group.
    This is the forcible breaking of infinite loops as implemented in Perl
    5.005. */
    if (*ecode == OP_KET || eptr == saved_eptr)
      ecode += 1+LINK_SIZE;
    /* The repeating kets try the rest of the pattern or restart from the
    preceding bracket, in the appropriate order. The second "call" of match()
    uses tail recursion, to avoid using another stack frame. */
    if (*ecode == OP_KETRMIN)
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM65);
      ecode = prev;
      goto TAIL_RECURSE;
    else  /* OP_KETRMAX */
      md->match_function_type = MATCH_CBEGROUP;
      RMATCH(eptr, prev, offset_top, md, eptrb, RM66);
      ecode += 1 + LINK_SIZE;
    /* Handle a capturing bracket, other than those that are possessive with an
    unlimited repeat. If there is space in the offset vector, save the current
    subject position in the working slot at the top of the vector. We mustn't
    change the current values of the data slot, because they may be set from a
    previous iteration of this group, and be referred to by a reference inside
    the group. A failure to match might occur after the group has succeeded,
    if something later on doesn't match. For this reason, we need to restore
    the working value and also the values of the final offsets, in case they
    were set by a previous iteration of the same bracket.
    If there isn't enough space in the offset vector, treat this as if it were
    a non-capturing bracket. Don't worry about setting the flag for the error
    case here; that is handled in the code for KET. */
    number = GET2(ecode, 1+LINK_SIZE);
    offset = number << 1;
    printf("start bracket %d\n", number);
    printf("subject=");
    pchars(eptr, 16, TRUE, md);
    if (offset < md->offset_max)
      save_offset1 = md->offset_vector[offset];
      save_offset2 = md->offset_vector[offset+1];
      save_offset3 = md->offset_vector[md->offset_end - number];
      save_capture_last = md->capture_last;
      DPRINTF(("saving %d %d %d\n", save_offset1, save_offset2, save_offset3));
      md->offset_vector[md->offset_end - number] =
        (int)(eptr - md->start_subject);
        if (op >= OP_SBRA) md->match_function_type = MATCH_CBEGROUP;
          eptrb, RM1);
        if (rrc == MATCH_ONCE) break;  /* Backing up through an atomic group */
        /* If we backed up to a THEN, check whether it is within the current
        branch by comparing the address of the THEN that is passed back with
        the end of the branch. If it is within the current branch, and the
        branch is one of two or more alternatives (it either starts or ends
        with OP_ALT), we have reached the limit of THEN's action, so convert
        the return code to NOMATCH, which will cause normal backtracking to
        happen from now on. Otherwise, THEN is passed back to an outer
        alternative. This implements Perl's treatment of parenthesized groups,
        where a group not containing | does not affect the current alternative,
        that is, (X) is NOT the same as (X|(*F)). */
        /* Anything other than NOMATCH is passed back. */
        md->capture_last = save_capture_last;
        ecode += GET(ecode, 1);
        if (*ecode != OP_ALT) break;
      DPRINTF(("bracket %d failed\n", number));
      md->offset_vector[offset] = save_offset1;
      md->offset_vector[offset+1] = save_offset2;
      md->offset_vector[md->offset_end - number] = save_offset3;
      /* At this point, rrc will be one of MATCH_ONCE or MATCH_NOMATCH. */
    /* FALL THROUGH ... Insufficient room for saving captured contents. Treat
    as a non-capturing bracket. */
    /* VVVVVVVVVVVVVVVVVVVVVVVVV */
    DPRINTF(("insufficient capture room: treat as non-capturing\n"));
    /* Non-capturing or atomic group, except for possessive with unlimited
    repeat and ONCE group with no captures. Loop for all the alternatives.
    When we get to the final alternative within the brackets, we used to return
    the result of a recursive call to match() whatever happened so it was
    possible to reduce stack usage by turning this into a tail recursion,
    except in the case of a possibly empty group. However, now that there is
    the possiblity of (*THEN) occurring in the final alternative, this
    optimization is no longer always possible.
    We can optimize if we know there are no (*THEN)s in the pattern; at present
    this is the best that can be done.
    MATCH_ONCE is returned when the end of an atomic group is successfully
    reached, but subsequent matching fails. It passes back up the tree (causing
    captured values to be reset) until the original atomic group level is
    reached. This is tested by comparing md->once_target with the start of the
    group. At this point, the return is converted into MATCH_NOMATCH so that
    previous backup points can be taken. */
    DPRINTF(("start non-capturing bracket\n"));
      if (op >= OP_SBRA || op == OP_ONCE) md->match_function_type = MATCH_CBEGROUP;
      /* If this is not a possibly empty group, and there are no (*THEN)s in
      the pattern, and this is the final alternative, optimize as described
      above. */
      else if (!md->hasthen && ecode[GET(ecode, 1)] != OP_ALT)
        ecode += PRIV(OP_lengths)[*ecode];
      /* In all other cases, we have to make another call to match(). */
      RMATCH(eptr, ecode + PRIV(OP_lengths)[*ecode], offset_top, md, eptrb,
        RM2);
      /* See comment in the code for capturing groups above about handling
      THEN. */
      if (rrc != MATCH_NOMATCH)
        if (rrc == MATCH_ONCE)
          const pcre_uchar *scode = ecode;
          if (*scode != OP_ONCE)           /* If not at start, find it */
            while (*scode == OP_ALT) scode += GET(scode, 1);
            scode -= GET(scode, 1);
          if (md->once_target == scode) rrc = MATCH_NOMATCH;
    /* Handle possessive capturing brackets with an unlimited repeat. We come
    here from BRAZERO with allow_zero set TRUE. The offset_vector values are
    handled similarly to the normal case above. However, the matching is
    different. The end of these brackets will always be OP_KETRPOS, which
    returns MATCH_KETRPOS without going further in the pattern. By this means
    we can handle the group by iteration rather than recursion, thereby
    reducing the amount of stack needed. */
    allow_zero = FALSE;
    POSSESSIVE_CAPTURE:
    printf("start possessive bracket %d\n", number);
      matched_once = FALSE;
      code_offset = (int)(ecode - md->start_code);
      /* Each time round the loop, save the current subject position for use
      when the group matches. For MATCH_MATCH, the group has matched, so we
      restart it with a new subject starting position, remembering that we had
      at least one match. For MATCH_NOMATCH, carry on with the alternatives, as
      usual. If we haven't matched any alternatives in any iteration, check to
      see if a previous iteration matched. If so, the group has matched;
      continue from afterwards. Otherwise it has failed; restore the previous
      capture values before returning NOMATCH. */
          eptrb, RM63);
        if (rrc == MATCH_KETRPOS)
          ecode = md->start_code + code_offset;
          matched_once = TRUE;
      if (!matched_once)
      if (allow_zero || matched_once)
    /* Non-capturing possessive bracket with unlimited repeat. We come here
    from BRAZERO with allow_zero = TRUE. The code is similar to the above,
    without the capturing complication. It is written out separately for speed
    and cleanliness. */
    POSSESSIVE_NON_CAPTURE:
        eptrb, RM48);
    if (matched_once || allow_zero)
    /* Control never reaches here. */
    /* Conditional group: compilation checked that there are no more than
    two branches. If the condition is false, skipping the first branch takes us
    past the end if there is only one branch, but that's OK because that is
    exactly what going to the ket would do. */
    codelink = GET(ecode, 1);
    /* Because of the way auto-callout works during compile, a callout item is
    inserted between OP_COND and an assertion condition. */
    if (ecode[LINK_SIZE+1] == OP_CALLOUT)
      int arglen = ecode[LINK_SIZE+1 + 1]; /* AutoHotkey: arg len and null-terminated arg precede the standard bytes */
      const pcre_uchar *cnptr = ecode + LINK_SIZE+1 + 1 + arglen + 1;
        cb.version          = 2;   /* Version 1 of the callout block */
        cb.offset_vector    = md->offset_vector;
        cb.subject          = (PCRE_SPTR)md->start_subject;
        cb.subject          = (PCRE_SPTR16)md->start_subject;
        cb.subject_length   = (int)(md->end_subject - md->start_subject);
        cb.start_match      = (int)(mstart - md->start_subject);
        cb.current_position = (int)(eptr - md->start_subject);
        cb.next_item_length = GET(cnptr, 3);
        cb.capture_top      = offset_top/2;
        cb.capture_last     = md->capture_last;
        cb.mark             = md->nomatch_mark;
        if ((rrc = (*PUBL(callout))(&cb)) > 0) RRETURN(MATCH_NOMATCH);
        if (rrc < 0) RRETURN(rrc);
      ecode += arglen + PRIV(OP_lengths)[OP_CALLOUT];
    condcode = ecode[LINK_SIZE+1];
    /* Now see what the actual condition is */
    if (condcode == OP_RREF || condcode == OP_NRREF)    /* Recursion test */
      if (md->recursive == NULL)                /* Not recursing => FALSE */
        condition = FALSE;
        int recno = GET2(ecode, LINK_SIZE + 2);   /* Recursion group number*/
        condition = (recno == RREF_ANY || recno == md->recursive->group_num);
        /* If the test is for recursion into a specific subpattern, and it is
        false, but the test was set up by name, scan the table to see if the
        name refers to any other numbers, and test them. The condition is true
        if any one is set. */
        if (!condition && condcode == OP_NRREF)
          pcre_uchar *slotA = md->name_table;
          for (i = 0; i < md->name_count; i++)
            if (GET2(slotA, 0) == recno) break;
            slotA += md->name_entry_size;
          /* Found a name for the number - there can be only one; duplicate
          names for different numbers are allowed, but not vice versa. First
          scan down for duplicates. */
          if (i < md->name_count)
            pcre_uchar *slotB = slotA;
            while (slotB > md->name_table)
              slotB -= md->name_entry_size;
              if (STRCMP_UC_UC(slotA + IMM2_SIZE, slotB + IMM2_SIZE) == 0)
                condition = GET2(slotB, 0) == md->recursive->group_num;
                if (condition) break;
            /* Scan up for duplicates */
            if (!condition)
              slotB = slotA;
              for (i++; i < md->name_count; i++)
                slotB += md->name_entry_size;
        /* Chose branch according to the condition */
        ecode += condition? 1 + IMM2_SIZE : GET(ecode, 1);
    else if (condcode == OP_CREF || condcode == OP_NCREF)  /* Group used test */
      offset = GET2(ecode, LINK_SIZE+2) << 1;  /* Doubled ref number */
      condition = offset < offset_top && md->offset_vector[offset] >= 0;
      /* If the numbered capture is unset, but the reference was by name,
      scan the table to see if the name refers to any other numbers, and test
      them. The condition is true if any one is set. This is tediously similar
      to the code above, but not close enough to try to amalgamate. */
      if (!condition && condcode == OP_NCREF)
        int refno = offset >> 1;
          if (GET2(slotA, 0) == refno) break;
        /* Found a name for the number - there can be only one; duplicate names
        for different numbers are allowed, but not vice versa. First scan down
        for duplicates. */
              offset = GET2(slotB, 0) << 1;
              condition = offset < offset_top &&
                md->offset_vector[offset] >= 0;
    else if (condcode == OP_DEF)     /* DEFINE - always false */
    /* The condition is an assertion. Call match() to evaluate it - setting
    md->match_function_type to MATCH_CONDASSERT causes it to stop at the end of
    an assertion. */
      md->match_function_type = MATCH_CONDASSERT;
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, NULL, RM3);
      if (rrc == MATCH_MATCH)
        if (md->end_offset_top > offset_top)
          offset_top = md->end_offset_top;  /* Captures may have happened */
        condition = TRUE;
        ecode += 1 + LINK_SIZE + GET(ecode, LINK_SIZE + 2);
        while (*ecode == OP_ALT) ecode += GET(ecode, 1);
      /* PCRE doesn't allow the effect of (*THEN) to escape beyond an
      assertion; it is therefore treated as NOMATCH. */
      else if (rrc != MATCH_NOMATCH && rrc != MATCH_THEN)
        RRETURN(rrc);         /* Need braces because of following else */
        ecode += codelink;
    /* We are now at the branch that is to be obeyed. As there is only one, can
    use tail recursion to avoid using another stack frame, except when there is
    unlimited repeat of a possibly empty group. In the latter case, a recursive
    call to match() is always required, unless the second alternative doesn't
    exist, in which case we can just plough on. Note that, for compatibility
    with Perl, the | in a conditional group is NOT treated as creating two
    alternatives. If a THEN is encountered in the branch, it propagates out to
    the enclosing alternative (unless nested in a deeper set of alternatives,
    of course). */
    if (condition || *ecode == OP_ALT)
      if (op != OP_SCOND)
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM49);
     /* Condition false & no alternative; continue after the group. */
    /* Before OP_ACCEPT there may be any number of OP_CLOSE opcodes,
    to close any currently open capturing brackets. */
    number = GET2(ecode, 1);
      printf("end bracket %d at *ACCEPT", number);
    md->capture_last = number;
    if (offset >= md->offset_max) md->offset_overflow = TRUE; else
      md->offset_vector[offset] =
        md->offset_vector[md->offset_end - number];
      md->offset_vector[offset+1] = (int)(eptr - md->start_subject);
      if (offset_top <= offset) offset_top = offset + 2;
    ecode += 1 + IMM2_SIZE;
    /* End of the pattern, either real or forced. */
    /* If we have matched an empty string, fail if not in an assertion and not
    in a recursion if either PCRE_NOTEMPTY is set, or if PCRE_NOTEMPTY_ATSTART
    is set and we have matched at the start of the subject. In both cases,
    backtracking will then try other alternatives, if any. */
    if (eptr == mstart && op != OP_ASSERT_ACCEPT &&
         md->recursive == NULL &&
         (md->notempty ||
           (md->notempty_atstart &&
             mstart == md->start_subject + md->start_offset)))
    /* Otherwise, we have a match. */
    md->end_match_ptr = eptr;           /* Record where we ended */
    md->end_offset_top = offset_top;    /* and how many extracts were taken */
    md->start_match_ptr = mstart;       /* and the start (\K can modify) */
    /* For some reason, the macros don't work properly if an expression is
    given as the argument to RRETURN when the heap is in use. */
    rrc = (op == OP_END)? MATCH_MATCH : MATCH_ACCEPT;
    /* Assertion brackets. Check the alternative branches in turn - the
    matching won't pass the KET for an assertion. If any one branch matches,
    the assertion is true. Lookbehind assertions have an OP_REVERSE item at the
    start of each branch to move the current point backwards, so the code at
    this level is identical to the lookahead case. When the assertion is part
    of a condition, we want to return immediately afterwards. The caller of
    this incarnation of the match() function will have set MATCH_CONDASSERT in
    md->match_function type, and one of these opcodes will be the first opcode
    that is processed. We use a local variable that is preserved over calls to
    match() to remember this case. */
    if (md->match_function_type == MATCH_CONDASSERT)
      condassert = TRUE;
    else condassert = FALSE;
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, NULL, RM4);
      if (rrc == MATCH_MATCH || rrc == MATCH_ACCEPT)
        mstart = md->start_match_ptr;   /* In case \K reset it */
      /* PCRE does not allow THEN to escape beyond an assertion; it is treated
      as NOMATCH. */
    if (*ecode == OP_KET) RRETURN(MATCH_NOMATCH);
    /* If checking an assertion for a condition, return MATCH_MATCH. */
    if (condassert) RRETURN(MATCH_MATCH);
    /* Continue from after the assertion, updating the offsets high water
    mark, since extracts may have been taken during the assertion. */
    do ecode += GET(ecode,1); while (*ecode == OP_ALT);
    /* Negative assertion: all branches must fail to match. Encountering SKIP,
    PRUNE, or COMMIT means we must assume failure without checking subsequent
    branches. */
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, NULL, RM5);
      if (rrc == MATCH_MATCH || rrc == MATCH_ACCEPT) RRETURN(MATCH_NOMATCH);
      if (rrc == MATCH_SKIP || rrc == MATCH_PRUNE || rrc == MATCH_COMMIT)
    if (condassert) RRETURN(MATCH_MATCH);  /* Condition assertion */
    /* Move the subject pointer back. This occurs only at the start of
    each branch of a lookbehind assertion. If we are too close to the start to
    move back, this match function fails. When working with UTF-8 we move
    back a number of characters, not bytes. */
      i = GET(ecode, 1);
      while (i-- > 0)
        eptr--;
        if (eptr < md->start_subject) RRETURN(MATCH_NOMATCH);
        BACKCHAR(eptr);
    /* No UTF-8 support, or not in UTF-8 mode: count is byte count */
      eptr -= GET(ecode, 1);
    /* Save the earliest consulted character, then skip to next op code */
    if (eptr < md->start_used_ptr) md->start_used_ptr = eptr;
    /* The callout item calls an external function, if one is provided, passing
    details of the match so far. This is mainly for debugging, though the
    function is able to force a failure. */
    length = ecode[1]; /* AutoHotkey: arg len and null-terminated arg precede the standard bytes */
      const pcre_uchar *cnptr = ecode + 2 + length + 1;
      cb.user_callout     = (ecode + 2); /* AutoHotkey */
    ecode += 2 + length + 2 + 2*LINK_SIZE;
    /* Recursion either matches the current regex, or some subexpression. The
    offset data is the offset to the starting bracket from the start of the
    whole pattern. (This is so that it works from duplicated subpatterns.)
    The state of the capturing groups is preserved over recursion, and
    re-instated afterwards. We don't know how many are started and not yet
    finished (offset_top records the completed total) so we just have to save
    all the potential data. There may be up to 65535 such values, which is too
    large to put on the stack, but using malloc for small numbers seems
    expensive. As a compromise, the stack is used when there are no more than
    REC_STACK_SAVE_MAX values to store; otherwise malloc is used.
    There are also other values that have to be saved. We use a chained
    sequence of blocks that actually live on the stack. Thanks to Robin Houston
    for the original version of this logic. It has, however, been hacked around
    a lot, so he is not to blame for the current way it works. */
      recursion_info *ri;
      callpat = md->start_code + GET(ecode, 1);
      recno = (callpat == md->start_code)? 0 :
      /* Check for repeating a recursion without advancing the subject pointer.
      This should catch convoluted mutual recursions. (Some simple cases are
      caught at compile time.) */
        if (recno == ri->group_num && eptr == ri->subject_position)
          RRETURN(PCRE_ERROR_RECURSELOOP);
      /* Add to "recursing stack" */
      new_recursive.subject_position = eptr;
      /* Where to continue from afterwards */
      /* Now save the offset data */
      new_recursive.saved_max = md->offset_end;
      if (new_recursive.saved_max <= REC_STACK_SAVE_MAX)
        new_recursive.offset_save = stacksave;
        new_recursive.offset_save =
          (int *)(PUBL(malloc))(new_recursive.saved_max * sizeof(int));
        if (new_recursive.offset_save == NULL) RRETURN(PCRE_ERROR_NOMEMORY);
      memcpy(new_recursive.offset_save, md->offset_vector,
            new_recursive.saved_max * sizeof(int));
      /* OK, now we can do the recursion. After processing each alternative,
      restore the offset data. If there were nested recursions, md->recursive
      might be changed, so reset it before looping. */
      DPRINTF(("Recursing into group %d\n", new_recursive.group_num));
      cbegroup = (*callpat >= OP_SBRA);
        if (cbegroup) md->match_function_type = MATCH_CBEGROUP;
        RMATCH(eptr, callpat + PRIV(OP_lengths)[*callpat], offset_top,
          md, eptrb, RM6);
        memcpy(md->offset_vector, new_recursive.offset_save,
        md->recursive = new_recursive.prevrec;
          DPRINTF(("Recursion matched\n"));
          if (new_recursive.offset_save != stacksave)
            (PUBL(free))(new_recursive.offset_save);
          /* Set where we got to in the subject, and reset the start in case
          it was changed by \K. This *is* propagated back out of a recursion,
          for Perl compatibility. */
          goto RECURSION_MATCHED;        /* Exit loop; end processing */
        /* PCRE does not allow THEN to escape beyond a recursion; it is treated
          DPRINTF(("Recursion gave error %d\n", rrc));
        callpat += GET(callpat, 1);
      while (*callpat == OP_ALT);
      DPRINTF(("Recursion didn't match\n"));
    RECURSION_MATCHED:
    /* An alternation is the end of a branch; scan along to find the end of the
    bracketed group and go to there. */
    /* BRAZERO, BRAMINZERO and SKIPZERO occur just before a bracket group,
    indicating that it may occur zero times. It may repeat infinitely, or not
    at all - i.e. it could be ()* or ()? or even (){0} in the pattern. Brackets
    with fixed upper repeat limits are compiled as a number of copies, with the
    optional ones preceded by BRAZERO or BRAMINZERO. */
    next = ecode + 1;
    RMATCH(eptr, next, offset_top, md, eptrb, RM10);
    do next += GET(next, 1); while (*next == OP_ALT);
    ecode = next + 1 + LINK_SIZE;
    RMATCH(eptr, next + 1+LINK_SIZE, offset_top, md, eptrb, RM11);
    ecode++;
    next = ecode+1;
    do next += GET(next,1); while (*next == OP_ALT);
    /* BRAPOSZERO occurs before a possessive bracket group. Don't do anything
    here; just jump to the group, with allow_zero set TRUE. */
    op = *(++ecode);
    if (op == OP_CBRAPOS || op == OP_SCBRAPOS) goto POSSESSIVE_CAPTURE;
      goto POSSESSIVE_NON_CAPTURE;
    /* End of a group, repeated or non-repeating. */
    prev = ecode - GET(ecode, 1);
    /* If this was a group that remembered the subject start, in order to break
    infinite repeats of empty string matches, retrieve the subject start from
    the chain. Otherwise, set it NULL. */
    if (*prev >= OP_SBRA || *prev == OP_ONCE)
      saved_eptr = eptrb->epb_saved_eptr;   /* Value at start of group */
      eptrb = eptrb->epb_prev;              /* Backup to previous group */
    else saved_eptr = NULL;
    /* If we are at the end of an assertion group or a non-capturing atomic
    group, stop matching and return MATCH_MATCH, but record the current high
    water mark for use by positive assertions. We also need to record the match
    start in case it was changed by \K. */
    if ((*prev >= OP_ASSERT && *prev <= OP_ASSERTBACK_NOT) ||
         *prev == OP_ONCE_NC)
      md->end_match_ptr = eptr;      /* For ONCE_NC */
      md->end_offset_top = offset_top;
      md->start_match_ptr = mstart;
      RRETURN(MATCH_MATCH);         /* Sets md->mark */
    /* For capturing groups we have to check the group number back at the start
    and if necessary complete handling an extraction by setting the offsets and
    bumping the high water mark. Whole-pattern recursion is coded as a recurse
    into group 0, so it won't be picked up here. Instead, we catch it when the
    OP_END is reached. Other recursion is handled here. We just have to record
    the current subject position and start match pointer and give a MATCH
    return. */
    if (*prev == OP_CBRA || *prev == OP_SCBRA ||
        *prev == OP_CBRAPOS || *prev == OP_SCBRAPOS)
      number = GET2(prev, 1+LINK_SIZE);
      printf("end bracket %d", number);
      /* Handle a recursively called group. */
      if (md->recursive != NULL && md->recursive->group_num == number)
        md->end_match_ptr = eptr;
        RRETURN(MATCH_MATCH);
      /* Deal with capturing */
        /* If offset is greater than offset_top, it means that we are
        "skipping" a capturing group, and that group's offsets must be marked
        unset. In earlier versions of PCRE, all the offsets were unset at the
        start of matching, but this doesn't work because atomic groups and
        assertions can cause a value to be set that should later be unset.
        Example: matching /(?>(a))b|(a)c/ against "ac". This sets group 1 as
        part of the atomic group, but this is not on the final matching path,
        so must be unset when 2 is set. (If there is no group 2, there is no
        problem, because offset_top will then be 2, indicating no capture.) */
        if (offset > offset_top)
          register int *iptr = md->offset_vector + offset_top;
          register int *iend = md->offset_vector + offset;
          while (iptr < iend) *iptr++ = -1;
        /* Now make the extraction */
    /* For an ordinary non-repeating ket, just continue at this level. This
    also happens for a repeating ket if no characters were matched in the
    group. This is the forcible breaking of infinite loops as implemented in
    Perl 5.005. For a non-repeating atomic group that includes captures,
    establish a backup point by processing the rest of the pattern at a lower
    level. If this results in a NOMATCH return, pass MATCH_ONCE back to the
    original OP_ONCE level, thereby bypassing intermediate backup points, but
    resetting any captures that happened along the way. */
      if (*prev == OP_ONCE)
        RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM12);
        md->once_target = prev;  /* Level at which to change to MATCH_NOMATCH */
        RRETURN(MATCH_ONCE);
      ecode += 1 + LINK_SIZE;    /* Carry on at this level */
    /* OP_KETRPOS is a possessive repeating ket. Remember the current position,
    and return the MATCH_KETRPOS. This makes it possible to do the repeats one
    at a time from the outer level, thus saving stack. */
    if (*ecode == OP_KETRPOS)
      RRETURN(MATCH_KETRPOS);
    /* The normal repeating kets try the rest of the pattern or restart from
    the preceding bracket, in the appropriate order. In the second case, we can
    use tail recursion to avoid using another stack frame, unless we have an
    an atomic group or an unlimited repeat of a group that can match an empty
      RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM7);
        RMATCH(eptr, prev, offset_top, md, eptrb, RM8);
      if (*prev >= OP_SBRA)    /* Could match an empty string */
        RMATCH(eptr, prev, offset_top, md, eptrb, RM50);
      if (*prev >= OP_SBRA) md->match_function_type = MATCH_CBEGROUP;
      RMATCH(eptr, prev, offset_top, md, eptrb, RM13);
      if (rrc == MATCH_ONCE && md->once_target == prev) rrc = MATCH_NOMATCH;
        RMATCH(eptr, ecode + 1 + LINK_SIZE, offset_top, md, eptrb, RM9);
        md->once_target = prev;
    /* Not multiline mode: start of subject assertion, unless notbol. */
    if (md->notbol && eptr == md->start_subject) RRETURN(MATCH_NOMATCH);
    /* Start of subject assertion */
    if (eptr != md->start_subject) RRETURN(MATCH_NOMATCH);
    /* Multiline mode: start of subject unless notbol, or after any newline. */
    if (eptr != md->start_subject &&
        (eptr == md->end_subject || !WAS_NEWLINE(eptr)))
    /* Start of match assertion */
    if (eptr != md->start_subject + md->start_offset) RRETURN(MATCH_NOMATCH);
    /* Reset the start of match point */
    mstart = eptr;
    /* Multiline mode: assert before any newline, or before end of subject
    unless noteol is set. */
    if (eptr < md->end_subject)
      { if (!IS_NEWLINE(eptr)) RRETURN(MATCH_NOMATCH); }
      if (md->noteol) RRETURN(MATCH_NOMATCH);
      SCHECK_PARTIAL();
    /* Not multiline mode: assert before a terminating newline or before end of
    subject unless noteol is set. */
    if (!md->endonly) goto ASSERT_NL_OR_EOS;
    /* ... else fall through for endonly */
    /* End of subject assertion (\z) */
    if (eptr < md->end_subject) RRETURN(MATCH_NOMATCH);
    /* End of subject or ending \n assertion (\Z) */
    ASSERT_NL_OR_EOS:
    if (eptr < md->end_subject &&
        (!IS_NEWLINE(eptr) || eptr != md->end_subject - md->nllen))
    /* Either at end of string or \n before end. */
    /* Word boundary assertions */
      /* Find out if the previous and current characters are "word" characters.
      It takes a bit more work in UTF-8 mode. Characters > 255 are assumed to
      be "non-word" characters. Remember the earliest consulted character for
      partial matching. */
        /* Get status of previous character */
        if (eptr == md->start_subject) prev_is_word = FALSE; else
          PCRE_PUCHAR lastptr = eptr - 1;
          BACKCHAR(lastptr);
          if (lastptr < md->start_used_ptr) md->start_used_ptr = lastptr;
          GETCHAR(c, lastptr);
          if (md->use_ucp)
            if (c == '_') prev_is_word = TRUE; else
              prev_is_word = (cat == ucp_L || cat == ucp_N);
          prev_is_word = c < 256 && (md->ctypes[c] & ctype_word) != 0;
        /* Get status of next character */
          cur_is_word = FALSE;
          GETCHAR(c, eptr);
            if (c == '_') cur_is_word = TRUE; else
              cur_is_word = (cat == ucp_L || cat == ucp_N);
          cur_is_word = c < 256 && (md->ctypes[c] & ctype_word) != 0;
      /* Not in UTF-8 mode, but we may still have PCRE_UCP set, and for
      consistency with the behaviour of \w we do use it in this case. */
          if (eptr <= md->start_used_ptr) md->start_used_ptr = eptr - 1;
            c = eptr[-1];
          prev_is_word = MAX_255(eptr[-1])
            && ((md->ctypes[eptr[-1]] & ctype_word) != 0);
          c = *eptr;
        cur_is_word = MAX_255(*eptr)
          && ((md->ctypes[*eptr] & ctype_word) != 0);
      /* Now see if the situation is what we want */
      if ((*ecode++ == OP_WORD_BOUNDARY)?
           cur_is_word == prev_is_word : cur_is_word != prev_is_word)
    /* Match a single character type; inline for speed */
    if (IS_NEWLINE(eptr)) RRETURN(MATCH_NOMATCH);
    if (eptr >= md->end_subject)   /* DO NOT merge the eptr++ here; it must */
      {                            /* not be updated before SCHECK_PARTIAL. */
    if (utf) ACROSSCHAR(eptr < md->end_subject, *eptr, eptr++);
    /* Match a single byte, even in UTF-8 mode. This opcode really does match
    any byte, even newline, independent of the setting of PCRE_DOTALL. */
    GETCHARINCTEST(c, eptr);
    if (
       c < 256 &&
       (md->ctypes[c] & ctype_digit) != 0
       c > 255 ||
       (md->ctypes[c] & ctype_digit) == 0
       (md->ctypes[c] & ctype_space) != 0
       (md->ctypes[c] & ctype_space) == 0
       (md->ctypes[c] & ctype_word) != 0
       (md->ctypes[c] & ctype_word) == 0
      default: RRETURN(MATCH_NOMATCH);
      if (eptr < md->end_subject && *eptr == 0x0a) eptr++;
      if (md->bsr_anycrlf) RRETURN(MATCH_NOMATCH);
      case 0x0a:      /* LF */
      case 0x0b:      /* VT */
      case 0x0c:      /* FF */
      case 0x0d:      /* CR */
      case 0x85:      /* NEL */
      case 0x2028:    /* LINE SEPARATOR */
      case 0x2029:    /* PARAGRAPH SEPARATOR */
    if the support is in the binary; otherwise a compile-time error occurs. */
      switch(ecode[1])
        if (op == OP_NOTPROP) RRETURN(MATCH_NOMATCH);
        if ((prop->chartype == ucp_Lu ||
             prop->chartype == ucp_Lt) == (op == OP_NOTPROP))
        if ((ecode[2] != PRIV(ucp_gentype)[prop->chartype]) == (op == OP_PROP))
        if ((ecode[2] != prop->chartype) == (op == OP_PROP))
        if ((ecode[2] != prop->script) == (op == OP_PROP))
        if ((PRIV(ucp_gentype)[prop->chartype] == ucp_L ||
             PRIV(ucp_gentype)[prop->chartype] == ucp_N) == (op == OP_NOTPROP))
        if ((PRIV(ucp_gentype)[prop->chartype] == ucp_Z ||
               == (op == OP_NOTPROP))
             c == CHAR_UNDERSCORE) == (op == OP_NOTPROP))
        /* This should never occur */
        RRETURN(PCRE_ERROR_INTERNAL);
      ecode += 3;
    /* Match an extended Unicode sequence. We will get here only if the support
    is in the binary; otherwise a compile-time error occurs. */
    if (UCD_CATEGORY(c) == ucp_M) RRETURN(MATCH_NOMATCH);
    while (eptr < md->end_subject)
      int len = 1;
      if (!utf) c = *eptr; else { GETCHARLEN(c, eptr, len); }
      eptr += len;
    /* Match a back reference, possibly repeatedly. Look past the end of the
    item to see if there is repeat information following. The code is similar
    to that for character classes, but repeated for efficiency. Then obey
    similar code to character type repeats - written out again for speed.
    However, if the referenced string is the empty string, always treat
    it as matched, any number of times (otherwise there could be infinite
    loops). */
    caseless = op == OP_REFI;
    offset = GET2(ecode, 1) << 1;               /* Doubled ref number */
    /* If the reference is unset, there are two possibilities:
    (a) In the default, Perl-compatible state, set the length negative;
    this ensures that every attempt at a match fails. We can't just fail
    here, because of the possibility of quantifiers with zero minima.
    (b) If the JavaScript compatibility flag is set, set the length to zero
    so that the back reference matches an empty string.
    Otherwise, set the length to the length of what was matched by the
    referenced subpattern. */
    if (offset >= offset_top || md->offset_vector[offset] < 0)
      length = (md->jscript_compat)? 0 : -1;
      length = md->offset_vector[offset+1] - md->offset_vector[offset];
    /* Set up for repetition, or handle the non-repeated case */
      c = *ecode++ - OP_CRSTAR;
      minimize = (c & 1) != 0;
      min = rep_min[c];                 /* Pick up values from tables; */
      max = rep_max[c];                 /* zero for max => infinity */
      if (max == 0) max = INT_MAX;
      minimize = (*ecode == OP_CRMINRANGE);
      min = GET2(ecode, 1);
      max = GET2(ecode, 1 + IMM2_SIZE);
      ecode += 1 + 2 * IMM2_SIZE;
      default:               /* No repeat follows */
      if ((length = match_ref(offset, eptr, length, md, caseless)) < 0)
        CHECK_PARTIAL();
      eptr += length;
      continue;              /* With the main loop */
    /* Handle repeated back references. If the length of the reference is
    zero, just continue with the main loop. If the length is negative, it
    means the reference is unset in non-Java-compatible mode. If the minimum is
    zero, we can continue at the same level without recursion. For any other
    minimum, carrying on will result in NOMATCH. */
    if (length == 0) continue;
    if (length < 0 && min == 0) continue;
    /* First, ensure the minimum number of matches are present. We get back
    the length of the reference string explicitly rather than passing the
    address of eptr, so that eptr can be a register variable. */
    for (i = 1; i <= min; i++)
      int slength;
      if ((slength = match_ref(offset, eptr, length, md, caseless)) < 0)
      eptr += slength;
    /* If min = max, continue at the same level without recursion.
    They are not both allowed to be zero. */
    if (min == max) continue;
    /* If minimizing, keep trying and advancing the pointer */
    if (minimize)
      for (fi = min;; fi++)
        RMATCH(eptr, ecode, offset_top, md, eptrb, RM14);
        if (fi >= max) RRETURN(MATCH_NOMATCH);
    /* If maximizing, find the longest string and work backwards */
      pp = eptr;
      for (i = min; i < max; i++)
      while (eptr >= pp)
        RMATCH(eptr, ecode, offset_top, md, eptrb, RM15);
        eptr -= length;
    /* Match a bit-mapped character class, possibly repeatedly. This op code is
    used when all the characters in the class have values in the range 0-255,
    and either the matching is caseful, or the characters are in the range
    0-127 when UTF-8 processing is enabled. The only difference between
    OP_CLASS and OP_NCLASS occurs when a data character outside the range is
    encountered.
    First, look past the end of the item to see if there is repeat information
    following. Then obey similar code to character type repeats - written out
    again for speed. */
      /* The data variable is saved across frames, so the byte map needs to
      be stored there. */
#define BYTE_MAP ((pcre_uint8 *)data)
      data = ecode + 1;                /* Save for matching */
      ecode += 1 + (32 / sizeof(pcre_uchar)); /* Advance past the item */
        min = max = 1;
      /* First, ensure the minimum number of matches are present. */
            if (op == OP_CLASS) RRETURN(MATCH_NOMATCH);
            if ((BYTE_MAP[c/8] & (1 << (c&7))) == 0) RRETURN(MATCH_NOMATCH);
          c = *eptr++;
      /* If max == min we can continue with the main loop without the
      need to recurse. */
      /* If minimizing, keep testing the rest of the expression and advancing
      the pointer while it matches the class. */
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM16);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM17);
      /* If maximizing, find the longest possible run, then work backwards. */
            GETCHARLEN(c, eptr, len);
              if (op == OP_CLASS) break;
              if ((BYTE_MAP[c/8] & (1 << (c&7))) == 0) break;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM18);
            if (eptr-- == pp) break;        /* Stop if tried at original pos */
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM19);
#undef BYTE_MAP
    /* Match an extended character class. This opcode is encountered only
    when UTF-8 mode mode is supported. Nevertheless, we may not be in UTF-8
    mode, because Unicode properties are supported in non-UTF-8 mode. */
      data = ecode + 1 + LINK_SIZE;                /* Save for matching */
      ecode += GET(ecode, 1);                      /* Advance past the item */
        if (!PRIV(xclass)(c, data, utf)) RRETURN(MATCH_NOMATCH);
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM20);
          GETCHARLENTEST(c, eptr, len);
          if (!PRIV(xclass)(c, data, utf)) break;
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM21);
          if (utf) BACKCHAR(eptr);
#endif    /* End of XCLASS */
    /* Match a single character, casefully */
      length = 1;
      GETCHARLEN(fc, ecode, length);
      if (length > md->end_subject - eptr)
        CHECK_PARTIAL();             /* Not SCHECK_PARTIAL() */
      while (length-- > 0) if (*ecode++ != *eptr++) RRETURN(MATCH_NOMATCH);
      if (md->end_subject - eptr < 1)
        SCHECK_PARTIAL();            /* This one can use SCHECK_PARTIAL() */
      if (ecode[1] != *eptr++) RRETURN(MATCH_NOMATCH);
      ecode += 2;
    /* Match a single character, caselessly. If we are at the end of the
    subject, give up immediately. */
      /* If the pattern character's value is < 128, we have only one byte, and
      we know that its other case must also be one byte long, so we can use the
      fast lookup table. We know that there is at least one byte left in the
      subject. */
      if (fc < 128)
        if (md->lcc[fc]
            != TABLE_GET(*eptr, md->lcc, *eptr)) RRETURN(MATCH_NOMATCH);
      /* Otherwise we must pick up the subject character. Note that we cannot
      use the value of "length" to check for sufficient bytes left, because the
      other case of the character may have more or fewer bytes.  */
        unsigned int dc;
        GETCHARINC(dc, eptr);
        ecode += length;
        /* If we have Unicode property support, we can use it to test the other
        case of the character, if there is one. */
        if (fc != dc)
          if (dc != UCD_OTHERCASE(fc))
#endif   /* SUPPORT_UTF */
      if (TABLE_GET(ecode[1], md->lcc, ecode[1])
    /* Match a single character repeatedly. */
    min = max = GET2(ecode, 1);
    goto REPEATCHAR;
    possessive = TRUE;
    min = 0;
    max = GET2(ecode, 1);
    minimize = *ecode == OP_MINUPTO || *ecode == OP_MINUPTOI;
    max = INT_MAX;
    min = 1;
    max = 1;
    c = *ecode++ - ((op < OP_STARI)? OP_STAR : OP_STARI);
    /* Common code for all repeated single-character matches. */
    REPEATCHAR:
      charptr = ecode;
      /* Handle multibyte character matching specially here. There is
      support for caseless matching if UCP support is present. */
      if (length > 1)
        if (op >= OP_STARI &&     /* Caseless */
            (othercase = UCD_OTHERCASE(fc)) != fc)
          oclength = PRIV(ord2utf)(othercase, occhars);
        else oclength = 0;
          if (eptr <= md->end_subject - length &&
            memcmp(eptr, charptr, IN_UCHARS(length)) == 0) eptr += length;
          else if (oclength > 0 &&
                   eptr <= md->end_subject - oclength &&
                   memcmp(eptr, occhars, IN_UCHARS(oclength)) == 0) eptr += oclength;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM22);
        else  /* Maximize */
          if (possessive) continue;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM23);
            if (eptr == pp) { RRETURN(MATCH_NOMATCH); }
#else   /* without SUPPORT_UCP */
      /* If the length of a UTF-8 character is 1, we fall through here, and
      obey the code as for non-UTF-8 characters below, though in this case the
      value of fc will always be < 128. */
      /* When not in UTF-8 mode, load a single-byte character. */
      fc = *ecode++;
    /* The value of fc at this point is always one character, though we may
    or may not be in UTF mode. The code is duplicated for the caseless and
    caseful cases, for speed, since matching characters is likely to be quite
    common. First, ensure the minimum number of matches are present. If min =
    max, continue at the same level without recursing. Otherwise, if
    minimizing, keep trying the rest of the expression and advancing one
    matching character if failing, up to the maximum. Alternatively, if
    maximizing, find the maximum number of characters and work backwards. */
    DPRINTF(("matching %c{%d,%d} against subject %.*s\n", fc, min, max,
      max, eptr));
    if (op >= OP_STARI)  /* Caseless */
      /* fc must be < 128 if UTF is enabled. */
      foc = md->fcc[fc];
      if (utf && fc > 127)
        foc = UCD_OTHERCASE(fc);
        foc = fc;
#endif /* SUPPORT_UCP */
        foc = TABLE_GET(fc, md->fcc, fc);
#endif /* COMPILE_PCRE8 */
        if (fc != *eptr && foc != *eptr) RRETURN(MATCH_NOMATCH);
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM24);
          if (fc != *eptr && foc != *eptr) break;
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM25);
    /* Caseful comparisons (includes all multi-byte characters) */
        if (fc != *eptr++) RRETURN(MATCH_NOMATCH);
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM26);
          if (fc != *eptr) break;
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM27);
    /* Match a negated single one-byte character. The character we are
    checking can be multibyte. */
    if (op == OP_NOTI)         /* The caseless case */
      register unsigned int ch, och;
      ch = *ecode++;
      /* ch must be < 128 if UTF is enabled. */
      och = md->fcc[ch];
      if (utf && ch > 127)
        och = UCD_OTHERCASE(ch);
        och = ch;
        och = TABLE_GET(ch, md->fcc, ch);
      if (ch == c || och == c) RRETURN(MATCH_NOMATCH);
    else    /* Caseful */
      if (*ecode++ == c) RRETURN(MATCH_NOMATCH);
    /* Match a negated single one-byte character repeatedly. This is almost a
    repeat of the code for a repeated single character, but I haven't found a
    nice way of commoning these up that doesn't require a test of the
    positive/negative option for each character match. Maybe that wouldn't add
    very much to the time taken, but character matching *is* what this is all
    about... */
    goto REPEATNOTCHAR;
    minimize = *ecode == OP_NOTMINUPTO || *ecode == OP_NOTMINUPTOI;
    c = *ecode++ - ((op >= OP_NOTSTARI)? OP_NOTSTARI: OP_NOTSTAR);
    /* Common code for all repeated single-byte matches. */
    REPEATNOTCHAR:
    /* The code is duplicated for the caseless and caseful cases, for speed,
    since matching characters is likely to be quite common. First, ensure the
    minimum number of matches are present. If min = max, continue at the same
    level without recursing. Otherwise, if minimizing, keep trying the rest of
    the expression and advancing one matching character if failing, up to the
    maximum. Alternatively, if maximizing, find the maximum number of
    characters and work backwards. */
    DPRINTF(("negative matching %c{%d,%d} against subject %.*s\n", fc, min, max,
    if (op >= OP_NOTSTARI)     /* Caseless */
        register unsigned int d;
          GETCHARINC(d, eptr);
          if (fc == d || (unsigned int) foc == d) RRETURN(MATCH_NOMATCH);
          if (fc == *eptr || foc == *eptr) RRETURN(MATCH_NOMATCH);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM28);
            if (fc == d || (unsigned int)foc == d) RRETURN(MATCH_NOMATCH);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM29);
      /* Maximize case */
            GETCHARLEN(d, eptr, len);
            if (fc == d || (unsigned int)foc == d) break;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM30);
            if (fc == *eptr || foc == *eptr) break;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM31);
    /* Caseful comparisons */
          if (fc == d) RRETURN(MATCH_NOMATCH);
          if (fc == *eptr++) RRETURN(MATCH_NOMATCH);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM32);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM33);
            if (fc == d) break;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM34);
            if (fc == *eptr) break;
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM35);
    /* Match a single character type repeatedly; several different opcodes
    share code. This is very similar to the code for single characters, but we
    repeat it in the interests of efficiency. */
    minimize = TRUE;
    goto REPEATTYPE;
    minimize = *ecode == OP_TYPEMINUPTO;
    c = *ecode++ - OP_TYPESTAR;
    /* Common code for all repeated single character type matches. Note that
    in UTF-8 mode, '.' matches a character of any length, but for the other
    character types, the valid characters are all one-byte long. */
    REPEATTYPE:
    ctype = *ecode++;      /* Code for the character type */
    if (ctype == OP_PROP || ctype == OP_NOTPROP)
      prop_fail_result = ctype == OP_NOTPROP;
      prop_type = *ecode++;
      prop_value = *ecode++;
    else prop_type = -1;
    /* First, ensure the minimum number of matches are present. Use inline
    code for maximizing the speed, and do the type test once at the start
    (i.e. keep it out of the loop). Separate the UTF-8 code completely as that
    is tidier. Also separate the UCP code, which can be the same for both UTF-8
    and single-bytes. */
    if (min > 0)
        switch(prop_type)
          if (prop_fail_result) RRETURN(MATCH_NOMATCH);
            int chartype;
            chartype = UCD_CHARTYPE(c);
            if ((chartype == ucp_Lu ||
                 chartype == ucp_Ll ||
                 chartype == ucp_Lt) == prop_fail_result)
            if ((UCD_CATEGORY(c) == prop_value) == prop_fail_result)
            if ((UCD_CHARTYPE(c) == prop_value) == prop_fail_result)
            if ((UCD_SCRIPT(c) == prop_value) == prop_fail_result)
            int category;
            category = UCD_CATEGORY(c);
            if ((category == ucp_L || category == ucp_N) == prop_fail_result)
            if ((UCD_CATEGORY(c) == ucp_Z || c == CHAR_HT || c == CHAR_NL ||
                   == prop_fail_result)
                 c == CHAR_VT || c == CHAR_FF || c == CHAR_CR)
            if ((category == ucp_L || category == ucp_N || c == CHAR_UNDERSCORE)
          /* This should not occur */
      /* Match extended Unicode sequences. We will get here only if the
      support is in the binary; otherwise a compile-time error occurs. */
      else if (ctype == OP_EXTUNI)
#endif     /* SUPPORT_UCP */
/* Handle all other cases when the coding is UTF-8 */
      if (utf) switch(ctype)
          ACROSSCHAR(eptr < md->end_subject, *eptr, eptr++);
        if (eptr > md->end_subject - min) RRETURN(MATCH_NOMATCH);
        eptr += min;
          if (c < 128 && (md->ctypes[c] & ctype_digit) != 0)
          if (*eptr >= 128 || (md->ctypes[*eptr] & ctype_digit) == 0)
          /* No need to skip more bytes - we know it's a 1-byte character */
          if (*eptr < 128 && (md->ctypes[*eptr] & ctype_space) != 0)
          if (*eptr >= 128 || (md->ctypes[*eptr] & ctype_space) == 0)
          if (*eptr < 128 && (md->ctypes[*eptr] & ctype_word) != 0)
          if (*eptr >= 128 || (md->ctypes[*eptr] & ctype_word) == 0)
        }  /* End switch(ctype) */
#endif     /* SUPPORT_UTF */
      /* Code for the non-UTF-8 case for minimum matching of operators other
      than OP_PROP and OP_NOTPROP. */
      switch(ctype)
        if (eptr > md->end_subject - min)
          switch(*eptr++)
          if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_digit) != 0)
          if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_digit) == 0)
          if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_space) != 0)
          if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_space) == 0)
          if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_word) != 0)
          if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_word) == 0)
    /* If min = max, continue at the same level without recursing */
    /* If minimizing, we have to test the rest of the pattern before each
    subsequent match. Again, separate the UTF-8 case for speed, and also
    separate the UCP cases. */
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM36);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM37);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM38);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM39);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM40);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM59);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM60);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM61);
            RMATCH(eptr, ecode, offset_top, md, eptrb, RM62);
            if ((category == ucp_L ||
                 category == ucp_N ||
                 c == CHAR_UNDERSCORE)
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM41);
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM42);
          if (ctype == OP_ANY && IS_NEWLINE(eptr))
            case OP_ANY:        /* This is the non-NL case */
            if (c < 256 && (md->ctypes[c] & ctype_digit) != 0)
            if (c >= 256 || (md->ctypes[c] & ctype_digit) == 0)
            if (c < 256 && (md->ctypes[c] & ctype_space) != 0)
            if (c >= 256 || (md->ctypes[c] & ctype_space) == 0)
            if (c < 256 && (md->ctypes[c] & ctype_word) != 0)
            if (c >= 256 || (md->ctypes[c] & ctype_word) == 0)
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM43);
            case OP_ANY:     /* This is the non-NL case */
            if (MAX_255(c) && (md->ctypes[c] & ctype_digit) != 0) RRETURN(MATCH_NOMATCH);
            if (!MAX_255(c) || (md->ctypes[c] & ctype_digit) == 0) RRETURN(MATCH_NOMATCH);
            if (MAX_255(c) && (md->ctypes[c] & ctype_space) != 0) RRETURN(MATCH_NOMATCH);
            if (!MAX_255(c) || (md->ctypes[c] & ctype_space) == 0) RRETURN(MATCH_NOMATCH);
            if (MAX_255(c) && (md->ctypes[c] & ctype_word) != 0) RRETURN(MATCH_NOMATCH);
            if (!MAX_255(c) || (md->ctypes[c] & ctype_word) == 0) RRETURN(MATCH_NOMATCH);
    /* If maximizing, it is worth using inline code for speed, doing the type
    test once at the start (i.e. keep it out of the loop). Again, keep the
    UTF-8 and UCP stuff separate. */
      pp = eptr;  /* Remember where we started */
            if (prop_fail_result) break;
            eptr+= len;
            if ((UCD_CATEGORY(c) == prop_value) == prop_fail_result) break;
            if ((UCD_CHARTYPE(c) == prop_value) == prop_fail_result) break;
            if ((UCD_SCRIPT(c) == prop_value) == prop_fail_result) break;
            if ((category == ucp_L || category == ucp_N ||
                 c == CHAR_UNDERSCORE) == prop_fail_result)
        /* eptr is now past the end of the maximum run */
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM44);
          if (UCD_CATEGORY(c) == ucp_M) break;
            len = 1;
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM45);
          for (;;)                        /* Move back over one extended */
            if (!utf) c = *eptr; else
#endif   /* SUPPORT_UCP */
          if (max < INT_MAX)
              if (IS_NEWLINE(eptr)) break;
          /* Handle unlimited UTF-8 repeat */
            eptr = md->end_subject;   /* Unlimited UTF-8 repeat */
          /* The byte case is the same as non-UTF8 */
          c = max - min;
          if (c > (unsigned int)(md->end_subject - eptr))
            eptr = md->end_subject;
          else eptr += c;
            if (c == 0x000d)
              if (++eptr >= md->end_subject) break;
              if (*eptr == 0x000a) eptr++;
              if (c != 0x000a &&
                  (md->bsr_anycrlf ||
                   (c != 0x000b && c != 0x000c &&
                    c != 0x0085 && c != 0x2028 && c != 0x2029)))
            BOOL gotspace;
              default: gotspace = FALSE; break;
              gotspace = TRUE;
            if (gotspace == (ctype == OP_NOT_HSPACE)) break;
            if (gotspace == (ctype == OP_NOT_VSPACE)) break;
            if (c < 256 && (md->ctypes[c] & ctype_digit) != 0) break;
            if (c >= 256 ||(md->ctypes[c] & ctype_digit) == 0) break;
            if (c < 256 && (md->ctypes[c] & ctype_space) != 0) break;
            if (c >= 256 ||(md->ctypes[c] & ctype_space) == 0) break;
            if (c < 256 && (md->ctypes[c] & ctype_word) != 0) break;
            if (c >= 256 || (md->ctypes[c] & ctype_word) == 0) break;
        /* eptr is now past the end of the maximum run. If possessive, we are
        done (no backing up). Otherwise, match at this position; anything other
        than no match is immediately returned. For nomatch, back up one
        character, unless we are matching \R and the last thing matched was
        \r\n, in which case, back up two bytes. */
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM46);
          if (ctype == OP_ANYNL && eptr > pp  && *eptr == '\n' &&
              eptr[-1] == '\r') eptr--;
              if (c != 0x000a && (md->bsr_anycrlf ||
                (c != 0x000b && c != 0x000c && c != 0x0085
                && c != 0x2028 && c != 0x2029
                ))) break;
            if (c == 0x09 || c == 0x20 || c == 0xa0
              || c == 0x1680 || c == 0x180e || (c >= 0x2000 && c <= 0x200A)
              || c == 0x202f || c == 0x205f || c == 0x3000
              ) break;
            if (c != 0x09 && c != 0x20 && c != 0xa0
              && c != 0x1680 && c != 0x180e && (c < 0x2000 || c > 0x200A)
              && c != 0x202f && c != 0x205f && c != 0x3000
            if (c == 0x0a || c == 0x0b || c == 0x0c || c == 0x0d || c == 0x85
              || c == 0x2028 || c == 0x2029
            if (c != 0x0a && c != 0x0b && c != 0x0c && c != 0x0d && c != 0x85
            if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_digit) != 0) break;
            if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_digit) == 0) break;
            if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_space) != 0) break;
            if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_space) == 0) break;
            if (MAX_255(*eptr) && (md->ctypes[*eptr] & ctype_word) != 0) break;
            if (!MAX_255(*eptr) || (md->ctypes[*eptr] & ctype_word) == 0) break;
        character (byte), unless we are matching \R and the last thing matched
        was \r\n, in which case, back up two bytes. */
          RMATCH(eptr, ecode, offset_top, md, eptrb, RM47);
      /* Get here if we can't make it match with any permitted repetitions */
    /* There's been some horrible disaster. Arrival here can only mean there is
    something seriously wrong in the code above or the OP_xxx definitions. */
    DPRINTF(("Unknown opcode %d\n", *ecode));
    RRETURN(PCRE_ERROR_UNKNOWN_OPCODE);
  /* Do not stick any code in here without much thought; it is assumed
  that "continue" in the code above comes out to here to repeat the main
  loop. */
  }             /* End of main loop */
/* When compiling to use the heap rather than the stack for recursive calls to
match(), the RRETURN() macro jumps here. The number that is saved in
frame->Xwhere indicates which label we actually want to return to. */
#define LBL(val) case val: goto L_RM##val;
HEAP_RETURN:
switch (frame->Xwhere)
  LBL( 1) LBL( 2) LBL( 3) LBL( 4) LBL( 5) LBL( 6) LBL( 7) LBL( 8)
  LBL( 9) LBL(10) LBL(11) LBL(12) LBL(13) LBL(14) LBL(15) LBL(17)
  LBL(19) LBL(24) LBL(25) LBL(26) LBL(27) LBL(29) LBL(31) LBL(33)
  LBL(35) LBL(43) LBL(47) LBL(48) LBL(49) LBL(50) LBL(51) LBL(52)
  LBL(53) LBL(54) LBL(55) LBL(56) LBL(57) LBL(58) LBL(63) LBL(64)
  LBL(65) LBL(66)
  LBL(21)
  LBL(16) LBL(18) LBL(20)
  LBL(22) LBL(23) LBL(28) LBL(30)
  LBL(32) LBL(34) LBL(42) LBL(46)
  LBL(36) LBL(37) LBL(38) LBL(39) LBL(40) LBL(41) LBL(44) LBL(45)
  LBL(59) LBL(60) LBL(61) LBL(62)
  DPRINTF(("jump error in pcre match: label %d non-existent\n", frame->Xwhere));
printf("+++jump error in pcre match: label %d non-existent\n", frame->Xwhere);
  return PCRE_ERROR_INTERNAL;
#undef LBL
#endif  /* NO_RECURSE */
Undefine all the macros that were defined above to handle this. */
#undef eptr
#undef ecode
#undef mstart
#undef offset_top
#undef eptrb
#undef flags
#undef callpat
#undef charptr
#undef data
#undef next
#undef pp
#undef prev
#undef saved_eptr
#undef new_recursive
#undef cur_is_word
#undef condition
#undef prev_is_word
#undef ctype
#undef length
#undef number
#undef offset
#undef op
#undef save_capture_last
#undef save_offset1
#undef save_offset2
#undef save_offset3
#undef stacksave
#undef newptrb
/* These two are defined as macros in both cases */
#undef fc
#undef fi
*         Execute a Regular Expression           *
/* This function applies a compiled re to a subject string and picks out
portions of the string if it matches. Two elements in the vector are set for
each substring: the offsets to the start and end of the substring.
  offsets         points to a vector of ints to be filled in with offsets
  offsetcount     the number of elements in the vector
Returns:          > 0 => success; value is the number of elements filled in
                  = 0 => success, but offsets is not big enough
pcre_exec(const pcre *argument_re, const pcre_extra *extra_data,
  PCRE_SPTR subject, int length, int start_offset, int options, int *offsets,
  int offsetcount)
pcre16_exec(const pcre16 *argument_re, const pcre16_extra *extra_data,
int rc, ocount, arg_offset_max;
BOOL using_temporary_offsets = FALSE;
BOOL anchored;
BOOL startline;
BOOL firstline;
match_data match_block;
match_data *md = &match_block;
const pcre_uint8 *tables;
PCRE_PUCHAR start_match = (PCRE_PUCHAR)subject + start_offset;
PCRE_PUCHAR end_subject;
PCRE_PUCHAR start_partial = NULL;
PCRE_PUCHAR req_char_ptr = start_match - 1;
const pcre_study_data *study;
const REAL_PCRE *re = (const REAL_PCRE *)argument_re;
/* Check for the special magic call that measures the size of the stack used
per recursive call of match(). */
if (re == NULL && extra_data == NULL && subject == NULL && length == -999 &&
    start_offset == -999)
  return -sizeof(heapframe);
  return match(NULL, NULL, NULL, 0, NULL, NULL, 0);
if ((options & ~PUBLIC_EXEC_OPTIONS) != 0) return PCRE_ERROR_BADOPTION;
if (re == NULL || subject == NULL || (offsets == NULL && offsetcount > 0))
  return PCRE_ERROR_NULL;
/* These two settings are used in the code for checking a UTF-8 string that
follows immediately afterwards. Other values in the md block are used only
during "normal" pcre_exec() processing, not when the JIT support is in use,
so they are set up later. */
utf = md->utf = UTF_ENABLED((re->options & PCRE_UTF8) != 0);
md->partial = ((options & PCRE_PARTIAL_HARD) != 0)? 2 :
              ((options & PCRE_PARTIAL_SOFT) != 0)? 1 : 0;
/* Check a UTF-8 string if required. Pass back the character offset and error
code for an invalid string if a results vector is available. */
  int errorcode = PRIV(valid_utf)((PCRE_PUCHAR)subject, length, &erroroffset);
    return (errorcode <= PCRE_UTF16_ERR1 && md->partial > 1)?
      PCRE_ERROR_SHORTUTF16 : PCRE_ERROR_BADUTF16;
    return (errorcode <= PCRE_UTF8_ERR5 && md->partial > 1)?
  /* Check that a start_offset points to the start of a UTF character. */
/* If the pattern was successfully studied with JIT support, run the JIT
executable instead of the rest of this function. Most options must be set at
compile time for the JIT code to be usable. Fallback to the normal code path if
an unsupported flag is set. In particular, JIT does not support partial
matching. */
if (extra_data != NULL
    && (extra_data->flags & PCRE_EXTRA_EXECUTABLE_JIT) != 0
    && extra_data->executable_jit != NULL
    && (extra_data->flags & PCRE_EXTRA_TABLES) == 0
    && (options & ~(PCRE_NO_UTF8_CHECK | PCRE_NOTBOL | PCRE_NOTEOL |
                    PCRE_NOTEMPTY | PCRE_NOTEMPTY_ATSTART)) == 0)
  return PRIV(jit_exec)(re, extra_data->executable_jit,
    (const pcre_uchar *)subject, length, start_offset, options,
    ((extra_data->flags & PCRE_EXTRA_MATCH_LIMIT) == 0)
    ? MATCH_LIMIT : extra_data->match_limit, offsets, offsetcount);
/* Carry on with non-JIT matching. This information is for finding all the
numbers associated with a given name, for condition testing. */
md->name_table = (pcre_uchar *)re + re->name_table_offset;
md->name_count = re->name_count;
md->name_entry_size = re->name_entry_size;
/* Fish out the optional data from the extra_data structure, first setting
the default values. */
study = NULL;
md->match_limit = MATCH_LIMIT;
md->match_limit_recursion = MATCH_LIMIT_RECURSION;
/* The table pointer is always in native byte order. */
tables = re->tables;
  register unsigned int flags = extra_data->flags;
  if ((flags & PCRE_EXTRA_MATCH_LIMIT) != 0)
    md->match_limit = extra_data->match_limit;
    md->match_limit_recursion = extra_data->match_limit_recursion;
  if ((flags & PCRE_EXTRA_TABLES) != 0) tables = extra_data->tables;
/* Set up other data */
anchored = ((re->options | options) & PCRE_ANCHORED) != 0;
/* The code starts after the real_pcre block and the capture name table. */
md->start_code = (const pcre_uchar *)re + re->name_table_offset +
  re->name_count * re->name_entry_size;
md->start_subject = (PCRE_PUCHAR)subject;
md->end_subject = md->start_subject + length;
end_subject = md->end_subject;
md->endonly = (re->options & PCRE_DOLLAR_ENDONLY) != 0;
md->use_ucp = (re->options & PCRE_UCP) != 0;
md->jscript_compat = (re->options & PCRE_JAVASCRIPT_COMPAT) != 0;
md->ignore_skip_arg = FALSE;
/* Some options are unpacked into BOOL variables in the hope that testing
them will be faster than individual option bits. */
md->notbol = (options & PCRE_NOTBOL) != 0;
md->noteol = (options & PCRE_NOTEOL) != 0;
md->notempty = (options & PCRE_NOTEMPTY) != 0;
md->notempty_atstart = (options & PCRE_NOTEMPTY_ATSTART) != 0;
md->hitend = FALSE;
md->mark = md->nomatch_mark = NULL;     /* In case never set */
md->recursive = NULL;                   /* No recursion at top level */
md->hasthen = (re->flags & PCRE_HASTHEN) != 0;
md->lcc = tables + lcc_offset;
md->fcc = tables + fcc_offset;
md->ctypes = tables + ctypes_offset;
/* Handle different \R options. */
switch (options & (PCRE_BSR_ANYCRLF|PCRE_BSR_UNICODE))
    md->bsr_anycrlf = (re->options & PCRE_BSR_ANYCRLF) != 0;
  md->bsr_anycrlf = TRUE;
  md->bsr_anycrlf = FALSE;
  case PCRE_BSR_ANYCRLF:
  case PCRE_BSR_UNICODE:
switch ((((options & PCRE_NEWLINE_BITS) == 0)? re->options :
        (pcre_uint32)options) & PCRE_NEWLINE_BITS)
/* Partial matching was originally supported only for a restricted set of
regexes; from release 8.00 there are no restrictions, but the bits are still
defined (though never set). So there's no harm in leaving this code. */
if (md->partial && (re->flags & PCRE_NOPARTIAL) != 0)
  return PCRE_ERROR_BADPARTIAL;
/* If the expression has got more back references than the offsets supplied can
hold, we get a temporary chunk of working store to use during the matching.
Otherwise, we can use the vector supplied, rounding down its size to a multiple
of 3. */
ocount = offsetcount - (offsetcount % 3);
arg_offset_max = (2*ocount)/3;
if (re->top_backref > 0 && re->top_backref >= ocount/3)
  ocount = re->top_backref * 3 + 3;
  md->offset_vector = (int *)(PUBL(malloc))(ocount * sizeof(int));
  if (md->offset_vector == NULL) return PCRE_ERROR_NOMEMORY;
  using_temporary_offsets = TRUE;
  DPRINTF(("Got memory to hold back references\n"));
else md->offset_vector = offsets;
md->offset_end = ocount;
md->offset_max = (2*ocount)/3;
md->offset_overflow = FALSE;
md->capture_last = -1;
/* Reset the working variable associated with each extraction. These should
never be used unless previously set, but they get saved and restored, and so we
initialize them to avoid reading uninitialized locations. Also, unset the
offsets for the matched string. This is really just for tidiness with callouts,
in case they inspect these fields. */
if (md->offset_vector != NULL)
  register int *iptr = md->offset_vector + ocount;
  register int *iend = iptr - re->top_bracket;
  if (iend < md->offset_vector + 2) iend = md->offset_vector + 2;
  while (--iptr >= iend) *iptr = -1;
  md->offset_vector[0] = md->offset_vector[1] = -1;
/* Set up the first character to match, if available. The first_char value is
      first_char2 = TABLE_GET(first_char, md->fcc, first_char);
    req_char2 = TABLE_GET(req_char, md->fcc, req_char);
/* ==========================================================================*/
/* Loop for handling unanchored repeated matching attempts; for anchored regexs
the loop runs just once. */
  PCRE_PUCHAR save_end_subject = end_subject;
  PCRE_PUCHAR new_start_match;
  line of a multiline string. That is, the match must be before or at the first
  newline. Implement this by temporarily adjusting end_subject so that we stop
  scanning at a newline. If the match fails at the newline, later code breaks
  this loop. */
    PCRE_PUCHAR t = start_match;
  starting point is not found, or if a known later character is not present.
  However, there is an option that disables these, for testing and for ensuring
  that all callouts do actually occur. The option can be set in the regex by
  (*NO_START_OPT) or passed in match-time options. */
    /* Advance to a unique first char if there is one. */
        while (start_match < end_subject &&
            *start_match != first_char && *start_match != first_char2)
          start_match++;
        while (start_match < end_subject && *start_match != first_char)
    /* Or to just after a linebreak for a multiline match */
      if (start_match > md->start_subject + start_offset)
          while (start_match < end_subject && !WAS_NEWLINE(start_match))
            ACROSSCHAR(start_match < end_subject, *start_match,
              start_match++);
        /* If we have just passed a CR and the newline option is ANY or ANYCRLF,
        and we are now at a LF, advance the match position by one more character.
        if (start_match[-1] == CHAR_CR &&
             start_match < end_subject &&
             *start_match == CHAR_NL)
    /* Or to a non-unique first byte after study */
      while (start_match < end_subject)
        register unsigned int c = *start_match;
    }   /* Starting optimizations */
  disabling is explicitly requested. */
  if (((options | re->options) & PCRE_NO_START_OPTIMIZE) == 0 && !md->partial)
    /* If the pattern was studied, a minimum subject length may be set. This is
    a lower bound; no actual string of that length may actually match the
        (pcre_uint32)(end_subject - start_match) < study->minlength)
      rc = MATCH_NOMATCH;
    must be later in the subject; otherwise the test starts at the match point.
    This optimization can save a huge amount of backtracking in patterns with
    nested unlimited repeats that aren't going to match. Writing separate code
    for cased/caseless versions makes it go faster, as does using an
    can take a long time, and give bad performance on quite ordinary patterns.
    This showed up when somebody was matching something like /^\d+C/ on a
    32-megabyte string... so we don't do this when the string is sufficiently
    long. */
    if (has_req_char && end_subject - start_match < REQ_BYTE_MAX)
      register PCRE_PUCHAR p = start_match + (has_first_char? 1:0);
        forcing a match failure. */
        if (p >= end_subject)
#ifdef PCRE_DEBUG  /* Sigh. Some compilers never learn. */
  printf(">>>> Match against: ");
  pchars(start_match, end_subject - start_match, TRUE, md);
  /* OK, we can now run the match. If "hitend" is set afterwards, remember the
  first starting point for which a partial match was found. */
  md->start_match_ptr = start_match;
  md->start_used_ptr = start_match;
  md->match_call_count = 0;
  md->end_offset_top = 0;
  rc = match(start_match, md->start_code, start_match, 2, md, NULL, 0);
  if (md->hitend && start_partial == NULL) start_partial = md->start_used_ptr;
  switch(rc)
    /* If MATCH_SKIP_ARG reaches this level it means that a MARK that matched
    the SKIP's arg was not found. In this circumstance, Perl ignores the SKIP
    entirely. The only way we can do that is to re-do the match at the same
    point, with a flag to force SKIP with an argument to be ignored. Just
    treating this case as NOMATCH does not work because it does not check other
    alternatives in patterns such as A(*SKIP:A)B|AC when the subject is AC. */
    case MATCH_SKIP_ARG:
    new_start_match = start_match;
    md->ignore_skip_arg = TRUE;
    /* SKIP passes back the next starting point explicitly, but if it is the
    same as the match we have just done, treat it as NOMATCH. */
    case MATCH_SKIP:
    if (md->start_match_ptr != start_match)
      new_start_match = md->start_match_ptr;
    /* NOMATCH and PRUNE advance by one character. THEN at this level acts
    exactly like PRUNE. Unset the ignore SKIP-with-argument flag. */
    case MATCH_NOMATCH:
    case MATCH_PRUNE:
    case MATCH_THEN:
    new_start_match = start_match + 1;
      ACROSSCHAR(new_start_match < end_subject, *new_start_match,
        new_start_match++);
    /* COMMIT disables the bumpalong, but otherwise behaves as NOMATCH. */
    case MATCH_COMMIT:
    goto ENDLOOP;
    /* Any other return is either a match, or some kind of error. */
  /* Control reaches here for the various types of "no match at this point"
  result. Reset the code to MATCH_NOMATCH for subsequent checking. */
  /* If PCRE_FIRSTLINE is set, the match must happen before or at the first
  newline in the subject (though it may continue over the newline). Therefore,
  if we have just failed to match, starting at a newline, do not continue. */
  if (firstline && IS_NEWLINE(start_match)) break;
  /* Advance to new matching position */
  start_match = new_start_match;
  /* Break the loop if the pattern is anchored or if we have passed the end of
  if (anchored || start_match > end_subject) break;
  or ANY or ANYCRLF, advance the match position by one more character. In
  normal matching start_match will aways be greater than the first position at
  this stage, but a failed *SKIP can cause a return at the same point, which is
  why the first test exists. */
  if (start_match > (PCRE_PUCHAR)subject + start_offset &&
      start_match[-1] == CHAR_CR &&
      *start_match == CHAR_NL &&
  md->mark = NULL;   /* Reset for start of next match attempt */
  }                  /* End of for(;;) "bumpalong" loop */
/* We reach here when rc is not MATCH_NOMATCH, or if one of the stopping
conditions is true:
(1) The pattern is anchored or the match was failed by (*COMMIT);
(2) We are past the end of the subject;
(3) PCRE_FIRSTLINE is set and we have failed to match at a newline, because
    this option requests that a match occur at or before the first newline in
    the subject.
When we have a match and the offset vector is big enough to deal with any
backreferences, captured substring offsets will already be set up. In the case
where we had to get some local store to hold offsets for backreference
processing, copy those that we can. In this case there need not be overflow if
certain parts of the pattern were not used, even though there are more
capturing parentheses than vector slots. */
ENDLOOP:
if (rc == MATCH_MATCH || rc == MATCH_ACCEPT)
  if (using_temporary_offsets)
    if (arg_offset_max >= 4)
      memcpy(offsets + 2, md->offset_vector + 2,
        (arg_offset_max - 2) * sizeof(int));
      DPRINTF(("Copied offsets from temporary memory\n"));
    if (md->end_offset_top > arg_offset_max) md->offset_overflow = TRUE;
    DPRINTF(("Freeing temporary memory\n"));
    (PUBL(free))(md->offset_vector);
  /* Set the return code to the number of captured strings, or 0 if there were
  too many to fit into the vector. */
  rc = (md->offset_overflow && md->end_offset_top >= arg_offset_max)?
    0 : md->end_offset_top/2;
  /* If there is space in the offset vector, set any unused pairs at the end of
  the pattern to -1 for backwards compatibility. It is documented that this
  happens. In earlier versions, the whole set of potential capturing offsets
  was set to -1 each time round the loop, but this is handled differently now.
  "Gaps" are set to -1 dynamically instead (this fixes a bug). Thus, it is only
  those at the end that need unsetting here. We can't just unset them all at
  the start of the whole thing because they may get set in one branch that is
  not the final matching branch. */
  if (md->end_offset_top/2 <= re->top_bracket && offsets != NULL)
    register int *iptr, *iend;
    int resetcount = 2 + re->top_bracket * 2;
    if (resetcount > offsetcount) resetcount = ocount;
    iptr = offsets + md->end_offset_top;
    iend = offsets + resetcount;
  /* If there is space, set up the whole thing as substring 0. The value of
  md->start_match_ptr might be modified if \K was encountered on the success
  matching path. */
  if (offsetcount < 2) rc = 0; else
    offsets[0] = (int)(md->start_match_ptr - md->start_subject);
    offsets[1] = (int)(md->end_match_ptr - md->start_subject);
  /* Return MARK data if requested */
  if (extra_data != NULL && (extra_data->flags & PCRE_EXTRA_MARK) != 0)
    *(extra_data->mark) = (pcre_uchar *)md->mark;
  DPRINTF((">>>> returning %d\n", rc));
/* Control gets here if there has been an error, or if the overall match
attempt has failed at all permitted starting positions. */
/* For anything other than nomatch or partial match, just return the code. */
if (rc != MATCH_NOMATCH && rc != PCRE_ERROR_PARTIAL)
  DPRINTF((">>>> error: returning %d\n", rc));
/* Handle partial matches - disable any mark data */
if (start_partial != NULL)
  DPRINTF((">>>> returning PCRE_ERROR_PARTIAL\n"));
  md->mark = NULL;
  if (offsetcount > 1)
    offsets[0] = (int)(start_partial - (PCRE_PUCHAR)subject);
    offsets[1] = (int)(end_subject - (PCRE_PUCHAR)subject);
  rc = PCRE_ERROR_PARTIAL;
/* This is the classic nomatch case */
  DPRINTF((">>>> returning PCRE_ERROR_NOMATCH\n"));
  rc = PCRE_ERROR_NOMATCH;
/* Return the MARK data if it has been requested. */
  *(extra_data->mark) = (pcre_uchar *)md->nomatch_mark;
/* End of pcre_exec.c */
