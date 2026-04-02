  The machine code generator part (this module) was written by Zoltan Herczeg
                      Copyright (c) 2010-2012
/* All-in-one: Since we use the JIT compiler only from here,
we just include it. This way we don't need to touch the build
system files. */
#define SLJIT_MALLOC(size) (PUBL(malloc))(size)
#define SLJIT_FREE(ptr) (PUBL(free))(ptr)
#define SLJIT_CONFIG_AUTO 1
#define SLJIT_CONFIG_STATIC 1
#define SLJIT_VERBOSE 0
#define SLJIT_DEBUG 0
#include "sljit/sljitLir.c"
#if defined SLJIT_CONFIG_UNSUPPORTED && SLJIT_CONFIG_UNSUPPORTED
#error Unsupported architecture
/* Allocate memory on the stack. Fast, but limited size. */
#define LOCAL_SPACE_SIZE 32768
#define STACK_GROWTH_RATE 8192
/* Enable to check that the allocation could destroy temporaries. */
#if defined SLJIT_DEBUG && SLJIT_DEBUG
#define DESTROY_REGISTERS 1
Short summary about the backtracking mechanism empolyed by the jit code generator:
The code generator follows the recursive nature of the PERL compatible regular
expressions. The basic blocks of regular expressions are condition checkers
whose execute different commands depending on the result of the condition check.
The relationship between the operators can be horizontal (concatenation) and
vertical (sub-expression) (See struct fallback_common for more details).
  'ab' - 'a' and 'b' regexps are concatenated
  'a+' - 'a' is the sub-expression of the '+' operator
The condition checkers are boolean (true/false) checkers. Machine code is generated
for the checker itself and for the actions depending on the result of the checker.
The 'true' case is called as the hot path (expected path), and the other is called as
the 'fallback' path. Branch instructions are expesive for all CPUs, so we avoid taken
branches on the hot path.
 Greedy star operator (*) :
   Hot path: match happens.
   Fallback path: match failed.
 Non-greedy star operator (*?) :
   Hot path: no need to perform a match.
   Fallback path: match is required.
The following example shows how the code generated for a capturing bracket
with two alternatives. Let A, B, C, D are arbirary regular expressions, and
we have the following regular expression:
   A(B|C)D
The generated code will be the following:
 A hot path
 '(' hot path (pushing arguments to the stack)
 B hot path
 ')' hot path (pushing arguments to the stack)
 D hot path
 return with successful match
 D fallback path
 ')' fallback path (If we arrived from "C" jump to the fallback of "C")
 B fallback path
 C expected path
 jump to D hot path
 C fallback path
 A fallback path
 Notice, that the order of fallback code paths are the opposite of the fast
 code paths. In this way the topmost value on the stack is always belong
 to the current fallback code path. The fallback code path must check
 whether there is a next alternative. If so, it needs to jump back to
 the hot path eventually. Otherwise it needs to clear out its own stack
 frame and continue the execution on the fallback code paths.
Saved stack frames:
Atomic blocks and asserts require reloading the values of local variables
when the fallback mechanism performed. Because of OP_RECURSE, the locals
are not necessarly known in compile time, thus we need a dynamic restore
mechanism.
The stack frames are stored in a chain list, and have the following format:
([ capturing bracket offset ][ start value ][ end value ])+ ... [ 0 ] [ previous head ]
Thus we can restore the locals to a particular point in the stack.
typedef struct jit_arguments {
  /* Pointers first. */
  struct sljit_stack *stack;
  const pcre_uchar *str;
  const pcre_uchar *begin;
  const pcre_uchar *end;
  int *offsets;
  pcre_uchar *ptr;
  /* Everything else after. */
  int offsetcount;
  int calllimit;
  pcre_uint8 notbol;
  pcre_uint8 noteol;
  pcre_uint8 notempty;
  pcre_uint8 notempty_atstart;
} jit_arguments;
typedef struct executable_function {
  void *executable_func;
  PUBL(jit_callback) callback;
  void *userdata;
  sljit_uw executable_size;
} executable_function;
typedef struct jump_list {
  struct sljit_jump *jump;
  struct jump_list *next;
} jump_list;
enum stub_types { stack_alloc };
typedef struct stub_list {
  enum stub_types type;
  int data;
  struct sljit_jump *start;
  struct sljit_label *leave;
  struct stub_list *next;
} stub_list;
typedef int (SLJIT_CALL *jit_function)(jit_arguments *args);
/* The following structure is the key data type for the recursive
code generator. It is allocated by compile_hotpath, and contains
the aguments for compile_fallbackpath. Must be the first member
of its descendants. */
typedef struct fallback_common {
  /* Concatenation stack. */
  struct fallback_common *prev;
  jump_list *nextfallbacks;
  /* Internal stack (for component operators). */
  struct fallback_common *top;
  jump_list *topfallbacks;
  /* Opcode pointer. */
  pcre_uchar *cc;
} fallback_common;
typedef struct assert_fallback {
  fallback_common common;
  jump_list *condfailed;
  /* Less than 0 (-1) if a frame is not needed. */
  int framesize;
  /* Points to our private memory word on the stack. */
  int localptr;
  /* For iterators. */
  struct sljit_label *hotpath;
} assert_fallback;
typedef struct bracket_fallback {
  /* Where to coninue if an alternative is successfully matched. */
  struct sljit_label *althotpath;
  /* For rmin and rmax iterators. */
  struct sljit_label *recursivehotpath;
  /* For greedy ? operator. */
  struct sljit_label *zerohotpath;
  /* Contains the branches of a failed condition. */
    /* Both for OP_COND, OP_SCOND. */
    assert_fallback *assert;
    /* For OP_ONCE. -1 if not needed. */
  } u;
} bracket_fallback;
typedef struct bracketpos_fallback {
  /* Reverting stack is needed. */
  /* Allocated stack size. */
  int stacksize;
} bracketpos_fallback;
typedef struct braminzero_fallback {
} braminzero_fallback;
typedef struct iterator_fallback {
  /* Next iteration. */
} iterator_fallback;
typedef struct recurse_entry {
  struct recurse_entry *next;
  /* Contains the function entry. */
  struct sljit_label *entry;
  /* Collects the calls until the function is not created. */
  jump_list *calls;
  /* Points to the starting opcode. */
  int start;
} recurse_entry;
typedef struct recurse_fallback {
} recurse_fallback;
typedef struct compiler_common {
  struct sljit_compiler *compiler;
  pcre_uchar *start;
  int localsize;
  int *localptrs;
  const pcre_uint8 *fcc;
  sljit_w lcc;
  int cbraptr;
  int nltype;
  int bsr_nltype;
  int endonly;
  sljit_w ctypes;
  sljit_uw name_table;
  sljit_w name_count;
  sljit_w name_entry_size;
  struct sljit_label *acceptlabel;
  stub_list *stubs;
  recurse_entry *entries;
  recurse_entry *currententry;
  jump_list *accept;
  jump_list *calllimit;
  jump_list *stackalloc;
  jump_list *revertframes;
  jump_list *wordboundary;
  jump_list *anynewline;
  jump_list *hspace;
  jump_list *vspace;
  jump_list *casefulcmp;
  jump_list *caselesscmp;
  BOOL jscript_compat;
#ifdef SUPPORT_UTF_OPTION /* AutoHotkey: Helps detect any usages which aren't macro'd out. */
  BOOL use_ucp;
  jump_list *utfreadchar;
  jump_list *utfreadtype8;
  jump_list *getucd;
} compiler_common;
/* For byte_sequence_compare. */
typedef struct compare_context {
  int sourcereg;
#if defined SLJIT_UNALIGNED && SLJIT_UNALIGNED
  int ucharptr;
    sljit_i asint;
    sljit_uh asushort;
    sljit_ub asbyte;
    sljit_ub asuchars[4];
    sljit_uh asuchars[2];
  } c;
  } oc;
} compare_context;
enum {
  frame_end = 0,
  frame_setstrbegin = -1
/* Undefine sljit macros. */
#undef CMP
/* Used for accessing the elements of the stack. */
#define STACK(i)      ((-(i) - 1) * (int)sizeof(sljit_w))
#define TMP1          SLJIT_TEMPORARY_REG1
#define TMP2          SLJIT_TEMPORARY_REG3
#define TMP3          SLJIT_TEMPORARY_EREG2
#define STR_PTR       SLJIT_SAVED_REG1
#define STR_END       SLJIT_SAVED_REG2
#define STACK_TOP     SLJIT_TEMPORARY_REG2
#define STACK_LIMIT   SLJIT_SAVED_REG3
#define ARGUMENTS     SLJIT_SAVED_EREG1
#define CALL_COUNT    SLJIT_SAVED_EREG2
#define RETURN_ADDR   SLJIT_TEMPORARY_EREG1
/* Locals layout. */
/* These two locals can be used by the current opcode. */
#define LOCALS0          (0 * sizeof(sljit_w))
#define LOCALS1          (1 * sizeof(sljit_w))
/* Two local variables for possessive quantifiers (char1 cannot use them). */
#define POSSESSIVE0      (2 * sizeof(sljit_w))
#define POSSESSIVE1      (3 * sizeof(sljit_w))
/* Head of the last recursion. */
#define RECURSIVE_HEAD   (4 * sizeof(sljit_w))
/* Max limit of recursions. */
#define CALL_LIMIT       (5 * sizeof(sljit_w))
/* Last known position of the requested byte. */
#define REQ_CHAR_PTR     (6 * sizeof(sljit_w))
/* End pointer of the first line. */
#define FIRSTLINE_END    (7 * sizeof(sljit_w))
/* The output vector is stored on the stack, and contains pointers
to characters. The vector data is divided into two groups: the first
group contains the start / end character pointers, and the second is
the start pointers when the end of the capturing group has not yet reached. */
#define OVECTOR_START    (8 * sizeof(sljit_w))
#define OVECTOR(i)       (OVECTOR_START + (i) * sizeof(sljit_w))
#define OVECTOR_PRIV(i)  (common->cbraptr + (i) * sizeof(sljit_w))
#define PRIV_DATA(cc)    (common->localptrs[(cc) - common->start])
#define MOV_UCHAR  SLJIT_MOV_UB
#define MOVU_UCHAR SLJIT_MOVU_UB
#define MOV_UCHAR  SLJIT_MOV_UH
#define MOVU_UCHAR SLJIT_MOVU_UH
#error Unsupported compiling mode
/* Shortcuts. */
#define DEFINE_COMPILER \
  struct sljit_compiler *compiler = common->compiler
#define OP1(op, dst, dstw, src, srcw) \
  sljit_emit_op1(compiler, (op), (dst), (dstw), (src), (srcw))
#define OP2(op, dst, dstw, src1, src1w, src2, src2w) \
  sljit_emit_op2(compiler, (op), (dst), (dstw), (src1), (src1w), (src2), (src2w))
#define LABEL() \
  sljit_emit_label(compiler)
#define JUMP(type) \
  sljit_emit_jump(compiler, (type))
#define JUMPTO(type, label) \
  sljit_set_label(sljit_emit_jump(compiler, (type)), (label))
#define JUMPHERE(jump) \
  sljit_set_label((jump), sljit_emit_label(compiler))
#define CMP(type, src1, src1w, src2, src2w) \
  sljit_emit_cmp(compiler, (type), (src1), (src1w), (src2), (src2w))
#define CMPTO(type, src1, src1w, src2, src2w, label) \
  sljit_set_label(sljit_emit_cmp(compiler, (type), (src1), (src1w), (src2), (src2w)), (label))
#define COND_VALUE(op, dst, dstw, type) \
  sljit_emit_cond_value(compiler, (op), (dst), (dstw), (type))
static pcre_uchar* bracketend(pcre_uchar* cc)
SLJIT_ASSERT((*cc >= OP_ASSERT && *cc <= OP_ASSERTBACK_NOT) || (*cc >= OP_ONCE && *cc <= OP_SCOND));
SLJIT_ASSERT(*cc >= OP_KET && *cc <= OP_KETRPOS);
return cc;
/* Functions whose might need modification for all new supported opcodes:
 next_opcode
 get_localspace
 set_localptrs
 get_framesize
 init_frame
 get_localsize
 copy_locals
 compile_hotpath
 compile_fallbackpath
static pcre_uchar *next_opcode(compiler_common *common, pcre_uchar *cc)
SLJIT_UNUSED_ARG(common);
switch(*cc)
  return cc + 1;
  if (UTF_ENABLED(common->utf)) return NULL;
  if (UTF_ENABLED(common->utf) && HAS_EXTRALEN(cc[-1])) cc += GET_EXTRALEN(cc[-1]);
  return cc + 1 + 2;
  cc += 1 + IMM2_SIZE;
  return cc + 1 + 2 * IMM2_SIZE;
  return cc + 1 + 32 / sizeof(pcre_uchar);
  return cc + GET(cc, 1);
  return cc + 1 + LINK_SIZE;
  return cc + 1 + LINK_SIZE + IMM2_SIZE;
static int get_localspace(compiler_common *common, pcre_uchar *cc, pcre_uchar *ccend)
int localspace = 0;
pcre_uchar *alternative;
/* Calculate important variables (like stack size) and checks whether all opcodes are supported. */
while (cc < ccend)
    localspace += sizeof(sljit_w);
    cc += 1 + LINK_SIZE + IMM2_SIZE;
    /* Might be a hidden SCOND. */
    alternative = cc + GET(cc, 1);
    if (*alternative == OP_KETRMAX || *alternative == OP_KETRMIN)
    cc = next_opcode(common, cc);
    if (cc == NULL)
return localspace;
static void set_localptrs(compiler_common *common, int localptr, pcre_uchar *ccend)
pcre_uchar *cc = common->start;
    common->localptrs[cc - common->start] = localptr;
    localptr += sizeof(sljit_w);
    SLJIT_ASSERT(cc != NULL);
/* Returns with -1 if no need for frame. */
static int get_framesize(compiler_common *common, pcre_uchar *cc, BOOL recursive)
pcre_uchar *ccend = bracketend(cc);
BOOL possessive = FALSE;
BOOL setsom_found = FALSE;
if (!recursive && (*cc == OP_CBRAPOS || *cc == OP_SCBRAPOS))
  length = 3;
    if (!setsom_found)
      length += 2;
      setsom_found = TRUE;
    cc += (*cc == OP_SET_SOM) ? 1 : 1 + LINK_SIZE;
    length += 3;
/* Possessive quantifiers can use a special case. */
if (SLJIT_UNLIKELY(possessive) && length == 3)
  return length + 1;
static void init_frame(compiler_common *common, pcre_uchar *cc, int stackpos, int stacktop, BOOL recursive)
DEFINE_COMPILER;
/* >= 1 + shortest item size (2) */
SLJIT_UNUSED_ARG(stacktop);
SLJIT_ASSERT(stackpos >= stacktop + 2);
stackpos = STACK(stackpos);
if (recursive || (*cc != OP_CBRAPOS && *cc != OP_SCBRAPOS))
      OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0));
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackpos, SLJIT_IMM, frame_setstrbegin);
      stackpos += (int)sizeof(sljit_w);
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackpos, TMP1, 0);
    offset = (GET2(cc, 1 + LINK_SIZE)) << 1;
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackpos, SLJIT_IMM, OVECTOR(offset));
    OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset));
    OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1));
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackpos, TMP2, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackpos, SLJIT_IMM, frame_end);
SLJIT_ASSERT(stackpos == STACK(stacktop));
static SLJIT_INLINE int get_localsize(compiler_common *common, pcre_uchar *cc, pcre_uchar *ccend)
int localsize = 2;
/* Calculate the sum of the local variables. */
    localsize++;
    localsize += 2;
SLJIT_ASSERT(cc == ccend);
return localsize;
static void copy_locals(compiler_common *common, pcre_uchar *cc, pcre_uchar *ccend,
  BOOL save, int stackptr, int stacktop)
int srcw[2];
BOOL tmp1next = TRUE;
BOOL tmp1empty = TRUE;
BOOL tmp2empty = TRUE;
  start,
  loop,
  end
} status;
status = save ? start : loop;
stackptr = STACK(stackptr - 2);
stacktop = STACK(stacktop - 1);
if (!save)
  stackptr += sizeof(sljit_w);
  if (stackptr < stacktop)
    OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(STACK_TOP), stackptr);
    tmp1empty = FALSE;
    OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(STACK_TOP), stackptr);
    tmp2empty = FALSE;
  /* The tmp1next must be TRUE in either way. */
while (status != end)
  switch(status)
    case start:
    SLJIT_ASSERT(save);
    count = 1;
    srcw[0] = RECURSIVE_HEAD;
    status = loop;
    case loop:
    if (cc >= ccend)
      status = end;
      srcw[0] = PRIV_DATA(cc);
      SLJIT_ASSERT(srcw[0] != 0);
      srcw[0] = OVECTOR_PRIV(GET2(cc, 1 + LINK_SIZE));
      srcw[1] = OVECTOR_PRIV(GET2(cc, 1 + LINK_SIZE));
    case end:
    SLJIT_ASSERT_STOP();
  while (count > 0)
    count--;
    if (save)
      if (tmp1next)
        if (!tmp1empty)
          OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackptr, TMP1, 0);
        OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), srcw[count]);
        tmp1next = FALSE;
        if (!tmp2empty)
          OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), stackptr, TMP2, 0);
        OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), srcw[count]);
        tmp1next = TRUE;
        SLJIT_ASSERT(!tmp1empty);
        OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), srcw[count], TMP1, 0);
        tmp1empty = stackptr >= stacktop;
        SLJIT_ASSERT(!tmp2empty);
        OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), srcw[count], TMP2, 0);
        tmp2empty = stackptr >= stacktop;
SLJIT_ASSERT(cc == ccend && stackptr == stacktop && (save || (tmp1empty && tmp2empty)));
static SLJIT_INLINE BOOL ispowerof2(unsigned int value)
return (value & (value - 1)) == 0;
static SLJIT_INLINE void set_jumps(jump_list *list, struct sljit_label *label)
while (list)
  /* sljit_set_label is clever enough to do nothing
  if either the jump or the label is NULL */
  sljit_set_label(list->jump, label);
  list = list->next;
static SLJIT_INLINE void add_jump(struct sljit_compiler *compiler, jump_list **list, struct sljit_jump* jump)
jump_list *list_item = sljit_alloc_memory(compiler, sizeof(jump_list));
if (list_item)
  list_item->next = *list;
  list_item->jump = jump;
  *list = list_item;
static void add_stub(compiler_common *common, enum stub_types type, int data, struct sljit_jump *start)
stub_list* list_item = sljit_alloc_memory(compiler, sizeof(stub_list));
  list_item->type = type;
  list_item->data = data;
  list_item->start = start;
  list_item->leave = LABEL();
  list_item->next = common->stubs;
  common->stubs = list_item;
static void flush_stubs(compiler_common *common)
stub_list* list_item = common->stubs;
while (list_item)
  JUMPHERE(list_item->start);
  switch(list_item->type)
    case stack_alloc:
    add_jump(compiler, &common->stackalloc, JUMP(SLJIT_FAST_CALL));
  JUMPTO(SLJIT_JUMP, list_item->leave);
  list_item = list_item->next;
common->stubs = NULL;
static SLJIT_INLINE void decrease_call_count(compiler_common *common)
OP2(SLJIT_SUB | SLJIT_SET_E, CALL_COUNT, 0, CALL_COUNT, 0, SLJIT_IMM, 1);
add_jump(compiler, &common->calllimit, JUMP(SLJIT_C_ZERO));
static SLJIT_INLINE void allocate_stack(compiler_common *common, int size)
/* May destroy all locals and registers except TMP2. */
OP2(SLJIT_ADD, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, size * sizeof(sljit_w));
#ifdef DESTROY_REGISTERS
OP1(SLJIT_MOV, TMP1, 0, SLJIT_IMM, 12345);
OP1(SLJIT_MOV, TMP3, 0, TMP1, 0);
OP1(SLJIT_MOV, RETURN_ADDR, 0, TMP1, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, TMP1, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, TMP1, 0);
add_stub(common, stack_alloc, 0, CMP(SLJIT_C_GREATER, STACK_TOP, 0, STACK_LIMIT, 0));
static SLJIT_INLINE void free_stack(compiler_common *common, int size)
OP2(SLJIT_SUB, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, size * sizeof(sljit_w));
static SLJIT_INLINE void reset_ovector(compiler_common *common, int length)
struct sljit_label *loop;
/* At this point we can freely use all temporary registers. */
/* TMP1 returns with begin - 1. */
OP2(SLJIT_SUB, SLJIT_TEMPORARY_REG1, 0, SLJIT_MEM1(SLJIT_SAVED_REG1), SLJIT_OFFSETOF(jit_arguments, begin), SLJIT_IMM, IN_UCHARS(1));
if (length < 8)
  for (i = 0; i < length; i++)
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(i), SLJIT_TEMPORARY_REG1, 0);
  OP2(SLJIT_ADD, SLJIT_TEMPORARY_REG2, 0, SLJIT_LOCALS_REG, 0, SLJIT_IMM, OVECTOR_START - sizeof(sljit_w));
  OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG3, 0, SLJIT_IMM, length);
  loop = LABEL();
  OP1(SLJIT_MOVU, SLJIT_MEM1(SLJIT_TEMPORARY_REG2), sizeof(sljit_w), SLJIT_TEMPORARY_REG1, 0);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_TEMPORARY_REG3, 0, SLJIT_TEMPORARY_REG3, 0, SLJIT_IMM, 1);
  JUMPTO(SLJIT_C_NOT_ZERO, loop);
static SLJIT_INLINE void copy_ovector(compiler_common *common, int topbracket)
struct sljit_jump *earlyexit;
/* At this point we can freely use all registers. */
OP1(SLJIT_MOV, SLJIT_SAVED_REG3, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1));
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1), STR_PTR, 0);
OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG1, 0, ARGUMENTS, 0);
OP1(SLJIT_MOV_SI, SLJIT_TEMPORARY_REG2, 0, SLJIT_MEM1(SLJIT_TEMPORARY_REG1), SLJIT_OFFSETOF(jit_arguments, offsetcount));
OP2(SLJIT_SUB, SLJIT_TEMPORARY_REG3, 0, SLJIT_MEM1(SLJIT_TEMPORARY_REG1), SLJIT_OFFSETOF(jit_arguments, offsets), SLJIT_IMM, sizeof(int));
OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG1, 0, SLJIT_MEM1(SLJIT_TEMPORARY_REG1), SLJIT_OFFSETOF(jit_arguments, begin));
OP2(SLJIT_ADD, SLJIT_SAVED_REG1, 0, SLJIT_LOCALS_REG, 0, SLJIT_IMM, OVECTOR_START);
/* Unlikely, but possible */
earlyexit = CMP(SLJIT_C_EQUAL, SLJIT_TEMPORARY_REG2, 0, SLJIT_IMM, 0);
OP2(SLJIT_SUB, SLJIT_SAVED_REG2, 0, SLJIT_MEM1(SLJIT_SAVED_REG1), 0, SLJIT_TEMPORARY_REG1, 0);
OP2(SLJIT_ADD, SLJIT_SAVED_REG1, 0, SLJIT_SAVED_REG1, 0, SLJIT_IMM, sizeof(sljit_w));
/* Copy the integer value to the output buffer */
OP2(SLJIT_ASHR, SLJIT_SAVED_REG2, 0, SLJIT_SAVED_REG2, 0, SLJIT_IMM, 1);
OP1(SLJIT_MOVU_SI, SLJIT_MEM1(SLJIT_TEMPORARY_REG3), sizeof(int), SLJIT_SAVED_REG2, 0);
OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_TEMPORARY_REG2, 0, SLJIT_TEMPORARY_REG2, 0, SLJIT_IMM, 1);
JUMPHERE(earlyexit);
/* Calculate the return value, which is the maximum ovector value. */
if (topbracket > 1)
  OP2(SLJIT_ADD, SLJIT_TEMPORARY_REG1, 0, SLJIT_LOCALS_REG, 0, SLJIT_IMM, OVECTOR_START + topbracket * 2 * sizeof(sljit_w));
  OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG2, 0, SLJIT_IMM, topbracket + 1);
  /* OVECTOR(0) is never equal to SLJIT_SAVED_REG3. */
  OP1(SLJIT_MOVU, SLJIT_TEMPORARY_REG3, 0, SLJIT_MEM1(SLJIT_TEMPORARY_REG1), -(2 * (sljit_w)sizeof(sljit_w)));
  OP2(SLJIT_SUB, SLJIT_TEMPORARY_REG2, 0, SLJIT_TEMPORARY_REG2, 0, SLJIT_IMM, 1);
  CMPTO(SLJIT_C_EQUAL, SLJIT_TEMPORARY_REG3, 0, SLJIT_SAVED_REG3, 0, loop);
  OP1(SLJIT_MOV, SLJIT_RETURN_REG, 0, SLJIT_TEMPORARY_REG2, 0);
  OP1(SLJIT_MOV, SLJIT_RETURN_REG, 0, SLJIT_IMM, 1);
static SLJIT_INLINE BOOL char_has_othercase(compiler_common *common, pcre_uchar* cc)
/* Detects if the character has an othercase. */
if (UTF_ENABLED(common->utf))
  GETCHAR(c, cc);
  if (c > 127)
    return c != UCD_OTHERCASE(c);
  return common->fcc[c] != c;
  c = *cc;
return MAX_255(c) ? common->fcc[c] != c : FALSE;
static SLJIT_INLINE unsigned int char_othercase(compiler_common *common, unsigned int c)
/* Returns with the othercase. */
if (UTF_ENABLED(common->utf) && c > 127)
  return UCD_OTHERCASE(c);
return TABLE_GET(c, common->fcc, c);
static unsigned int char_get_othercase_bit(compiler_common *common, pcre_uchar* cc)
/* Detects if the character and its othercase has only 1 bit difference. */
unsigned int c, oc, bit;
  if (c <= 127)
    oc = common->fcc[c];
    oc = UCD_OTHERCASE(c);
    oc = c;
  oc = TABLE_GET(c, common->fcc, c);
SLJIT_ASSERT(c != oc);
bit = c ^ oc;
/* Optimized for English alphabet. */
if (c <= 127 && bit == 0x20)
  return (0 << 8) | 0x20;
/* Since c != oc, they must have at least 1 bit difference. */
if (!ispowerof2(bit))
  n = GET_EXTRALEN(*cc);
  while ((bit & 0x3f) == 0)
    n--;
    bit >>= 6;
  return (n << 8) | bit;
return (0 << 8) | bit;
#else /* COMPILE_PCRE8 */
if (UTF_ENABLED(common->utf) && c > 65535)
  if (bit >= (1 << 10))
    bit >>= 10;
    return (bit < 256) ? ((2 << 8) | bit) : ((3 << 8) | (bit >> 8));
return (bit < 256) ? ((0 << 8) | bit) : ((1 << 8) | (bit >> 8));
#endif /* COMPILE_PCRE16 */
static SLJIT_INLINE void check_input_end(compiler_common *common, jump_list **fallbacks)
add_jump(compiler, fallbacks, CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0));
static void read_char(compiler_common *common)
/* Reads the character into TMP1, updates STR_PTR.
Does not check STR_END. TMP2 Destroyed. */
OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), 0);
  jump = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xc0);
  jump = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xd800);
  add_jump(compiler, &common->utfreadchar, JUMP(SLJIT_FAST_CALL));
  JUMPHERE(jump);
OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(1));
static void peek_char(compiler_common *common)
/* Reads the character into TMP1, keeps STR_PTR.
  OP2(SLJIT_SUB, STR_PTR, 0, STR_PTR, 0, TMP2, 0);
static void read_char8_type(compiler_common *common)
/* Reads the character type into TMP1, updates STR_PTR. Does not check STR_END. */
  OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), 0);
  /* This can be an extra read in some situations, but hopefully
  it is needed in most cases. */
  OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP2), common->ctypes);
  jump = CMP(SLJIT_C_LESS, TMP2, 0, SLJIT_IMM, 0xc0);
  add_jump(compiler, &common->utfreadtype8, JUMP(SLJIT_FAST_CALL));
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_IMM, 0);
  jump = CMP(SLJIT_C_GREATER, TMP2, 0, SLJIT_IMM, 255);
  /* Skip low surrogate if necessary. */
  OP2(SLJIT_AND, TMP2, 0, TMP2, 0, SLJIT_IMM, 0xfc00);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP2, 0, SLJIT_IMM, 0xd800);
  COND_VALUE(SLJIT_MOV, TMP2, 0, SLJIT_C_EQUAL);
  OP2(SLJIT_SHL, TMP2, 0, TMP2, 0, SLJIT_IMM, 1);
  OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, TMP2, 0);
/* The ctypes array contains only 256 values. */
static void skip_char_back(compiler_common *common)
/* Goes one character back. Affects STR_PTR and TMP1. Does not check begin. */
struct sljit_label *label;
  label = LABEL();
  OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), -IN_UCHARS(1));
  OP2(SLJIT_SUB, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(1));
  OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0xc0);
  CMPTO(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, 0x80, label);
#if defined SUPPORT_UTF && defined COMPILE_PCRE16
  OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0xfc00);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0xdc00);
  COND_VALUE(SLJIT_MOV, TMP1, 0, SLJIT_C_EQUAL);
  OP2(SLJIT_SHL, TMP1, 0, TMP1, 0, SLJIT_IMM, 1);
  OP2(SLJIT_SUB, STR_PTR, 0, STR_PTR, 0, TMP1, 0);
static void check_newlinechar(compiler_common *common, int nltype, jump_list **fallbacks, BOOL jumpiftrue)
/* Character comes in TMP1. Checks if it is a newline. TMP2 may be destroyed. */
if (nltype == NLTYPE_ANY)
  add_jump(compiler, &common->anynewline, JUMP(SLJIT_FAST_CALL));
  add_jump(compiler, fallbacks, JUMP(jumpiftrue ? SLJIT_C_NOT_ZERO : SLJIT_C_ZERO));
else if (nltype == NLTYPE_ANYCRLF)
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, CHAR_CR);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, CHAR_NL);
  COND_VALUE(SLJIT_OR | SLJIT_SET_E, TMP2, 0, SLJIT_C_EQUAL);
  SLJIT_ASSERT(nltype == NLTYPE_FIXED && common->newline < 256);
  add_jump(compiler, fallbacks, CMP(jumpiftrue ? SLJIT_C_EQUAL : SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, common->newline));
static void do_utfreadchar(compiler_common *common)
/* Fast decoding a UTF-8 character. TMP1 contains the first byte
of the character (>= 0xc0). Return char value in TMP1, length - 1 in TMP2. */
sljit_emit_fast_enter(compiler, RETURN_ADDR, 0, 1, 5, 5, common->localsize);
/* Searching for the first zero. */
OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x20);
jump = JUMP(SLJIT_C_NOT_ZERO);
/* Two byte sequence. */
OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(1));
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x1f);
OP2(SLJIT_SHL, TMP1, 0, TMP1, 0, SLJIT_IMM, 6);
OP2(SLJIT_AND, TMP2, 0, TMP2, 0, SLJIT_IMM, 0x3f);
OP2(SLJIT_OR, TMP1, 0, TMP1, 0, TMP2, 0);
OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, IN_UCHARS(1));
sljit_emit_fast_return(compiler, RETURN_ADDR, 0);
OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x10);
/* Three byte sequence. */
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x0f);
OP2(SLJIT_SHL, TMP1, 0, TMP1, 0, SLJIT_IMM, 12);
OP2(SLJIT_SHL, TMP2, 0, TMP2, 0, SLJIT_IMM, 6);
OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(2));
OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(2));
OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, IN_UCHARS(2));
/* Four byte sequence. */
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x07);
OP2(SLJIT_SHL, TMP1, 0, TMP1, 0, SLJIT_IMM, 18);
OP2(SLJIT_SHL, TMP2, 0, TMP2, 0, SLJIT_IMM, 12);
OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(3));
OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(3));
OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, IN_UCHARS(3));
static void do_utfreadtype8(compiler_common *common)
/* Fast decoding a UTF-8 character type. TMP2 contains the first byte
of the character (>= 0xc0). Return value in TMP1. */
struct sljit_jump *compare;
OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP2, 0, SLJIT_IMM, 0x20);
OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(0));
OP2(SLJIT_AND, TMP2, 0, TMP2, 0, SLJIT_IMM, 0x1f);
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x3f);
OP2(SLJIT_OR, TMP2, 0, TMP2, 0, TMP1, 0);
compare = CMP(SLJIT_C_GREATER, TMP2, 0, SLJIT_IMM, 255);
JUMPHERE(compare);
/* We only have types for characters less than 256. */
OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP2), (sljit_w)PRIV(utf8_table4) - 0xc0);
OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, TMP1, 0);
/* Fast decoding a UTF-16 character. TMP1 contains the first 16 bit char
of the character (>= 0xd800). Return char value in TMP1, length - 1 in TMP2. */
jump = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xdc00);
/* Do nothing, only return. */
/* Combine two 16 bit characters. */
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x3ff);
OP2(SLJIT_SHL, TMP1, 0, TMP1, 0, SLJIT_IMM, 10);
OP2(SLJIT_AND, TMP2, 0, TMP2, 0, SLJIT_IMM, 0x3ff);
OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x10000);
/* UCD_BLOCK_SIZE must be 128 (see the assert below). */
#define UCD_BLOCK_MASK 127
#define UCD_BLOCK_SHIFT 7
static void do_getucd(compiler_common *common)
/* Search the UCD record for the character comes in TMP1.
Returns chartype in TMP1 and UCD offset in TMP2. */
SLJIT_ASSERT(UCD_BLOCK_SIZE == 128 && sizeof(ucd_record) == 8);
OP2(SLJIT_LSHR, TMP2, 0, TMP1, 0, SLJIT_IMM, UCD_BLOCK_SHIFT);
OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP2), (sljit_w)PRIV(ucd_stage1));
OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, UCD_BLOCK_MASK);
OP2(SLJIT_SHL, TMP2, 0, TMP2, 0, SLJIT_IMM, UCD_BLOCK_SHIFT);
OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, TMP2, 0);
OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, (sljit_w)PRIV(ucd_stage2));
OP1(SLJIT_MOV_UH, TMP2, 0, SLJIT_MEM2(TMP2, TMP1), 1);
OP1(SLJIT_MOV, TMP1, 0, SLJIT_IMM, (sljit_w)PRIV(ucd_records) + SLJIT_OFFSETOF(ucd_record, chartype));
OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM2(TMP1, TMP2), 3);
static SLJIT_INLINE struct sljit_label *mainloop_entry(compiler_common *common, BOOL hascrorlf, BOOL firstline)
struct sljit_label *mainloop;
struct sljit_label *newlinelabel = NULL;
struct sljit_jump *end = NULL;
struct sljit_jump *nl = NULL;
struct sljit_jump *singlechar;
jump_list *newline = NULL;
BOOL newlinecheck = FALSE;
BOOL readuchar = FALSE;
if (!(hascrorlf || firstline) && (common->nltype == NLTYPE_ANY ||
    common->nltype == NLTYPE_ANYCRLF || common->newline > 255))
  newlinecheck = TRUE;
  /* Search for the end of the first line. */
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, STR_PTR, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END, STR_END, 0);
  if (common->nltype == NLTYPE_FIXED && common->newline > 255)
    mainloop = LABEL();
    end = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
    OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(-1));
    OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(0));
    CMPTO(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, (common->newline >> 8) & 0xff, mainloop);
    CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, common->newline & 0xff, mainloop);
    OP2(SLJIT_SUB, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(1));
    /* Continual stores does not cause data dependency. */
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END, STR_PTR, 0);
    read_char(common);
    check_newlinechar(common, common->nltype, &newline, TRUE);
    CMPTO(SLJIT_C_LESS, STR_PTR, 0, STR_END, 0, mainloop);
    set_jumps(newline, LABEL());
  JUMPHERE(end);
  OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0);
start = JUMP(SLJIT_JUMP);
if (newlinecheck)
  newlinelabel = LABEL();
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, common->newline & 0xff);
  nl = JUMP(SLJIT_JUMP);
/* Increasing the STR_PTR here requires one less jump in the most common case. */
if (UTF_ENABLED(common->utf)) readuchar = TRUE;
if (newlinecheck) readuchar = TRUE;
if (readuchar)
  CMPTO(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, (common->newline >> 8) & 0xff, newlinelabel);
  singlechar = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xc0);
  OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP1), (sljit_w)PRIV(utf8_table4) - 0xc0);
  JUMPHERE(singlechar);
  singlechar = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xd800);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0xd800);
JUMPHERE(start);
  JUMPHERE(nl);
return mainloop;
static SLJIT_INLINE void fast_forward_first_char(compiler_common *common, pcre_uchar first_char, BOOL caseless, BOOL firstline)
struct sljit_label *start;
struct sljit_jump *leave;
struct sljit_jump *found;
pcre_uchar oc, bit;
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, STR_END, 0);
  OP1(SLJIT_MOV, STR_END, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END);
start = LABEL();
leave = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
oc = first_char;
  oc = TABLE_GET(first_char, common->fcc, first_char);
  if (first_char > 127 && UTF_ENABLED(common->utf))
    oc = UCD_OTHERCASE(first_char);
if (first_char == oc)
  found = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, first_char);
  bit = first_char ^ oc;
  if (ispowerof2(bit))
    OP2(SLJIT_OR, TMP2, 0, TMP1, 0, SLJIT_IMM, bit);
    found = CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, first_char | bit);
    OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, first_char);
    OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, oc);
    found = JUMP(SLJIT_C_NOT_ZERO);
  CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xc0, start);
  CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xd800, start);
JUMPTO(SLJIT_JUMP, start);
JUMPHERE(found);
JUMPHERE(leave);
  OP1(SLJIT_MOV, STR_END, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0);
static SLJIT_INLINE void fast_forward_newline(compiler_common *common, BOOL firstline)
struct sljit_jump *lastchar;
struct sljit_jump *firstchar;
struct sljit_jump *foundcr = NULL;
struct sljit_jump *notfoundnl;
  lastchar = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
  OP1(SLJIT_MOV, TMP1, 0, ARGUMENTS, 0);
  OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, str));
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, begin));
  firstchar = CMP(SLJIT_C_LESS_EQUAL, STR_PTR, 0, TMP2, 0);
  OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, IN_UCHARS(2));
  OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, STR_PTR, 0, TMP1, 0);
  COND_VALUE(SLJIT_MOV, TMP2, 0, SLJIT_C_GREATER_EQUAL);
  OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(-2));
  OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(-1));
  CMPTO(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, (common->newline >> 8) & 0xff, loop);
  CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, common->newline & 0xff, loop);
  JUMPHERE(firstchar);
  JUMPHERE(lastchar);
skip_char_back(common);
if (common->nltype == NLTYPE_ANY || common->nltype == NLTYPE_ANYCRLF)
  foundcr = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_CR);
check_newlinechar(common, common->nltype, &newline, FALSE);
set_jumps(newline, loop);
  leave = JUMP(SLJIT_JUMP);
  JUMPHERE(foundcr);
  notfoundnl = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
  JUMPHERE(notfoundnl);
static SLJIT_INLINE void fast_forward_start_bits(compiler_common *common, sljit_uw start_bits, BOOL firstline)
jump = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 255);
OP1(SLJIT_MOV, TMP1, 0, SLJIT_IMM, 255);
OP2(SLJIT_AND, TMP2, 0, TMP1, 0, SLJIT_IMM, 0x7);
OP2(SLJIT_LSHR, TMP1, 0, TMP1, 0, SLJIT_IMM, 3);
OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP1), start_bits);
OP2(SLJIT_SHL, TMP2, 0, SLJIT_IMM, 1, TMP2, 0);
OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, TMP2, 0);
  OP1(SLJIT_MOV, TMP1, 0, TMP3, 0);
static SLJIT_INLINE struct sljit_jump *search_requested_char(compiler_common *common, pcre_uchar req_char, BOOL caseless, BOOL has_firstchar)
struct sljit_jump *toolong;
struct sljit_jump *alreadyfound;
struct sljit_jump *foundoc = NULL;
struct sljit_jump *notfound;
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), REQ_CHAR_PTR);
OP2(SLJIT_ADD, TMP1, 0, STR_PTR, 0, SLJIT_IMM, REQ_BYTE_MAX);
toolong = CMP(SLJIT_C_LESS, TMP1, 0, STR_END, 0);
alreadyfound = CMP(SLJIT_C_LESS, STR_PTR, 0, TMP2, 0);
if (has_firstchar)
  OP2(SLJIT_ADD, TMP1, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(1));
  OP1(SLJIT_MOV, TMP1, 0, STR_PTR, 0);
notfound = CMP(SLJIT_C_GREATER_EQUAL, TMP1, 0, STR_END, 0);
OP1(MOV_UCHAR, TMP2, 0, SLJIT_MEM1(TMP1), 0);
oc = req_char;
  oc = TABLE_GET(req_char, common->fcc, req_char);
  if (req_char > 127 && UTF_ENABLED(common->utf))
    oc = UCD_OTHERCASE(req_char);
if (req_char == oc)
  found = CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, req_char);
  bit = req_char ^ oc;
    OP2(SLJIT_OR, TMP2, 0, TMP2, 0, SLJIT_IMM, bit);
    found = CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, req_char | bit);
    foundoc = CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, oc);
OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, IN_UCHARS(1));
JUMPTO(SLJIT_JUMP, loop);
if (foundoc)
  JUMPHERE(foundoc);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), REQ_CHAR_PTR, TMP1, 0);
JUMPHERE(alreadyfound);
JUMPHERE(toolong);
return notfound;
static void do_revertframes(compiler_common *common)
OP1(SLJIT_MOV, TMP1, 0, STACK_TOP, 0);
/* Drop frames until we reach STACK_TOP. */
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(TMP1), 0);
jump = CMP(SLJIT_C_SIG_LESS_EQUAL, TMP2, 0, SLJIT_IMM, frame_end);
OP2(SLJIT_ADD, TMP2, 0, TMP2, 0, SLJIT_LOCALS_REG, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(TMP2), 0, SLJIT_MEM1(TMP1), sizeof(sljit_w));
OP1(SLJIT_MOV, SLJIT_MEM1(TMP2), sizeof(sljit_w), SLJIT_MEM1(TMP1), 2 * sizeof(sljit_w));
OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, 3 * sizeof(sljit_w));
JUMPTO(SLJIT_JUMP, mainloop);
jump = CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, frame_end);
/* End of dropping frames. */
jump = CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, frame_setstrbegin);
/* Set string begin. */
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(TMP1), sizeof(sljit_w));
OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, 2 * sizeof(sljit_w));
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), TMP2, 0);
/* Unknown command. */
static void check_wordboundary(compiler_common *common)
struct sljit_jump *beginend;
#if !(defined COMPILE_PCRE8) || defined SUPPORT_UTF
SLJIT_COMPILE_ASSERT(ctype_word == 0x10, ctype_word_must_be_16);
sljit_emit_fast_enter(compiler, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, 1, 5, 5, common->localsize);
/* Get type of the previous char, and put it to LOCALS1. */
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, SLJIT_IMM, 0);
beginend = CMP(SLJIT_C_LESS_EQUAL, STR_PTR, 0, TMP1, 0);
/* Testing char type. */
if (common->use_ucp)
  OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, 1);
  jump = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_UNDERSCORE);
  add_jump(compiler, &common->getucd, JUMP(SLJIT_FAST_CALL));
  OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, ucp_Ll);
  OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, ucp_Lu - ucp_Ll);
  COND_VALUE(SLJIT_MOV, TMP2, 0, SLJIT_C_LESS_EQUAL);
  OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, ucp_Nd - ucp_Ll);
  OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, ucp_No - ucp_Nd);
  COND_VALUE(SLJIT_OR, TMP2, 0, SLJIT_C_LESS_EQUAL);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, TMP2, 0);
  jump = CMP(SLJIT_C_GREATER, TMP1, 0, SLJIT_IMM, 255);
  /* Here LOCALS1 has already been zeroed. */
  jump = NULL;
  OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP1), common->ctypes);
  OP2(SLJIT_LSHR, TMP1, 0, TMP1, 0, SLJIT_IMM, 4 /* ctype_word */);
  OP2(SLJIT_AND, TMP1, 0, TMP1, 0, SLJIT_IMM, 1);
  if (jump != NULL)
JUMPHERE(beginend);
OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, 0);
beginend = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
peek_char(common);
/* Testing char type. This is a code duplication. */
  /* TMP2 may be destroyed by peek_char. */
  OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP1), common->ctypes);
  OP2(SLJIT_LSHR, TMP2, 0, TMP2, 0, SLJIT_IMM, 4 /* ctype_word */);
  OP2(SLJIT_AND, TMP2, 0, TMP2, 0, SLJIT_IMM, 1);
OP2(SLJIT_XOR | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1);
sljit_emit_fast_return(compiler, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0);
static void check_anynewline(compiler_common *common)
/* Check whether TMP1 contains a newline character. TMP2 destroyed. */
OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x0a);
OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x0d - 0x0a);
OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x85 - 0x0a);
  COND_VALUE(SLJIT_OR, TMP2, 0, SLJIT_C_EQUAL);
  OP2(SLJIT_OR, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x1);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x2029 - 0x0a);
#endif /* SUPPORT_UTF || COMPILE_PCRE16 */
static void check_hspace(compiler_common *common)
OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x09);
OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x20);
OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0xa0);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x1680);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x180e);
  OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, 0x2000);
  OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x200A - 0x2000);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x202f - 0x2000);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x205f - 0x2000);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 0x3000 - 0x2000);
static void check_vspace(compiler_common *common)
#define CHAR1 STR_END
#define CHAR2 STACK_TOP
static void do_casefulcmp(compiler_common *common)
OP1(SLJIT_MOV, TMP3, 0, CHAR1, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, CHAR2, 0);
OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, IN_UCHARS(1));
OP1(MOVU_UCHAR, CHAR1, 0, SLJIT_MEM1(TMP1), IN_UCHARS(1));
OP1(MOVU_UCHAR, CHAR2, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(1));
jump = CMP(SLJIT_C_NOT_EQUAL, CHAR1, 0, CHAR2, 0);
OP2(SLJIT_SUB | SLJIT_SET_E, TMP2, 0, TMP2, 0, SLJIT_IMM, IN_UCHARS(1));
JUMPTO(SLJIT_C_NOT_ZERO, label);
OP1(SLJIT_MOV, CHAR1, 0, TMP3, 0);
OP1(SLJIT_MOV, CHAR2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0);
#define LCC_TABLE STACK_LIMIT
static void do_caselesscmp(compiler_common *common)
OP1(SLJIT_MOV, TMP3, 0, LCC_TABLE, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, CHAR1, 0);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, CHAR2, 0);
OP1(SLJIT_MOV, LCC_TABLE, 0, SLJIT_IMM, common->lcc);
jump = CMP(SLJIT_C_GREATER, CHAR1, 0, SLJIT_IMM, 255);
OP1(SLJIT_MOV_UB, CHAR1, 0, SLJIT_MEM2(LCC_TABLE, CHAR1), 0);
jump = CMP(SLJIT_C_GREATER, CHAR2, 0, SLJIT_IMM, 255);
OP1(SLJIT_MOV_UB, CHAR2, 0, SLJIT_MEM2(LCC_TABLE, CHAR2), 0);
OP1(SLJIT_MOV, LCC_TABLE, 0, TMP3, 0);
OP1(SLJIT_MOV, CHAR1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0);
OP1(SLJIT_MOV, CHAR2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1);
#undef LCC_TABLE
#undef CHAR1
#undef CHAR2
#if defined SUPPORT_UTF && defined SUPPORT_UCP
static const pcre_uchar *SLJIT_CALL do_utf_caselesscmp(pcre_uchar *src1, jit_arguments *args, pcre_uchar *end1)
/* This function would be ineffective to do in JIT level. */
int c1, c2;
const pcre_uchar *src2 = args->ptr;
const pcre_uchar *end2 = args->end;
while (src1 < end1)
  if (src2 >= end2)
  GETCHARINC(c1, src1);
  GETCHARINC(c2, src2);
  if (c1 != c2 && c1 != UCD_OTHERCASE(c2)) return 0;
return src2;
#endif /* SUPPORT_UTF && SUPPORT_UCP */
static pcre_uchar *byte_sequence_compare(compiler_common *common, BOOL caseless, pcre_uchar *cc,
    compare_context* context, jump_list **fallbacks)
unsigned int othercasebit = 0;
pcre_uchar *othercasechar = NULL;
int utflength;
if (caseless && char_has_othercase(common, cc))
  othercasebit = char_get_othercase_bit(common, cc);
  SLJIT_ASSERT(othercasebit);
  /* Extracting bit difference info. */
  othercasechar = cc + (othercasebit >> 8);
  othercasebit &= 0xff;
  othercasechar = cc + (othercasebit >> 9);
  if ((othercasebit & 0x100) != 0)
    othercasebit = (othercasebit & 0xff) << 8;
if (context->sourcereg == -1)
  if (context->length >= 4)
    OP1(SLJIT_MOV_SI, TMP1, 0, SLJIT_MEM1(STR_PTR), -context->length);
  else if (context->length >= 2)
    OP1(SLJIT_MOV_UH, TMP1, 0, SLJIT_MEM1(STR_PTR), -context->length);
    OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(STR_PTR), -context->length);
  context->sourcereg = TMP2;
utflength = 1;
if (UTF_ENABLED(common->utf) && HAS_EXTRALEN(*cc))
  utflength += GET_EXTRALEN(*cc);
  context->length -= IN_UCHARS(1);
  /* Unaligned read is supported. */
  if (othercasebit != 0 && othercasechar == cc)
    context->c.asuchars[context->ucharptr] = *cc | othercasebit;
    context->oc.asuchars[context->ucharptr] = othercasebit;
    context->c.asuchars[context->ucharptr] = *cc;
    context->oc.asuchars[context->ucharptr] = 0;
  context->ucharptr++;
  if (context->ucharptr >= 4 || context->length == 0 || (context->ucharptr == 2 && context->length == 1))
  if (context->ucharptr >= 2 || context->length == 0)
      OP1(SLJIT_MOV_SI, context->sourcereg, 0, SLJIT_MEM1(STR_PTR), -context->length);
      OP1(SLJIT_MOV_UH, context->sourcereg, 0, SLJIT_MEM1(STR_PTR), -context->length);
    else if (context->length >= 1)
      OP1(SLJIT_MOV_UB, context->sourcereg, 0, SLJIT_MEM1(STR_PTR), -context->length);
    context->sourcereg = context->sourcereg == TMP1 ? TMP2 : TMP1;
    switch(context->ucharptr)
      case 4 / sizeof(pcre_uchar):
      if (context->oc.asint != 0)
        OP2(SLJIT_OR, context->sourcereg, 0, context->sourcereg, 0, SLJIT_IMM, context->oc.asint);
      add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, context->sourcereg, 0, SLJIT_IMM, context->c.asint | context->oc.asint));
      case 2 / sizeof(pcre_uchar):
      if (context->oc.asushort != 0)
        OP2(SLJIT_OR, context->sourcereg, 0, context->sourcereg, 0, SLJIT_IMM, context->oc.asushort);
      add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, context->sourcereg, 0, SLJIT_IMM, context->c.asushort | context->oc.asushort));
      if (context->oc.asbyte != 0)
        OP2(SLJIT_OR, context->sourcereg, 0, context->sourcereg, 0, SLJIT_IMM, context->oc.asbyte);
      add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, context->sourcereg, 0, SLJIT_IMM, context->c.asbyte | context->oc.asbyte));
    context->ucharptr = 0;
  /* Unaligned read is unsupported. */
  if (context->length > 0)
    OP2(SLJIT_OR, context->sourcereg, 0, context->sourcereg, 0, SLJIT_IMM, othercasebit);
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, context->sourcereg, 0, SLJIT_IMM, *cc | othercasebit));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, context->sourcereg, 0, SLJIT_IMM, *cc));
  utflength--;
while (utflength > 0);
#define SET_TYPE_OFFSET(value) \
  if ((value) != typeoffset) \
    if ((value) > typeoffset) \
      OP2(SLJIT_SUB, typereg, 0, typereg, 0, SLJIT_IMM, (value) - typeoffset); \
      OP2(SLJIT_ADD, typereg, 0, typereg, 0, SLJIT_IMM, typeoffset - (value)); \
  typeoffset = (value);
#define SET_CHAR_OFFSET(value) \
  if ((value) != charoffset) \
    if ((value) > charoffset) \
      OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, (value) - charoffset); \
      OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, charoffset - (value)); \
  charoffset = (value);
static void compile_xclass_hotpath(compiler_common *common, pcre_uchar *cc, jump_list **fallbacks)
jump_list *found = NULL;
jump_list **list = (*cc & XCL_NOT) == 0 ? &found : fallbacks;
int compares;
struct sljit_jump *jump = NULL;
pcre_uchar *ccbegin;
BOOL needstype = FALSE, needsscript = FALSE, needschar = FALSE;
BOOL charsaved = FALSE;
int typereg = TMP1, scriptreg = TMP1;
unsigned int typeoffset;
int invertcmp, numberofcmps;
unsigned int charoffset;
/* Although SUPPORT_UTF must be defined, we are not necessary in utf mode. */
check_input_end(common, fallbacks);
if ((*cc++ & XCL_MAP) != 0)
  OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(TMP1), (sljit_w)cc);
  add_jump(compiler, list, JUMP(SLJIT_C_NOT_ZERO));
  charsaved = TRUE;
  cc += 32 / sizeof(pcre_uchar);
/* Scanning the necessary info. */
ccbegin = cc;
compares = 0;
while (*cc != XCL_END)
  compares++;
  if (*cc == XCL_SINGLE)
    needschar = TRUE;
  else if (*cc == XCL_RANGE)
    SLJIT_ASSERT(*cc == XCL_PROP || *cc == XCL_NOTPROP);
      needstype = TRUE;
      needsscript = TRUE;
      case PT_SPACE:
      case PT_PXSPACE:
/* Simple register allocation. TMP1 is preferred if possible. */
if (needstype || needsscript)
  if (needschar && !charsaved)
  if (needschar)
    if (needstype)
      typereg = RETURN_ADDR;
    if (needsscript)
      scriptreg = TMP3;
  else if (needstype && needsscript)
  /* In all other cases only one of them was specified, and that can goes to TMP1. */
    if (scriptreg == TMP1)
      OP1(SLJIT_MOV, scriptreg, 0, SLJIT_IMM, (sljit_w)PRIV(ucd_records) + SLJIT_OFFSETOF(ucd_record, script));
      OP1(SLJIT_MOV_UB, scriptreg, 0, SLJIT_MEM2(scriptreg, TMP2), 3);
      OP2(SLJIT_SHL, TMP2, 0, TMP2, 0, SLJIT_IMM, 3);
      OP2(SLJIT_ADD, TMP2, 0, TMP2, 0, SLJIT_IMM, (sljit_w)PRIV(ucd_records) + SLJIT_OFFSETOF(ucd_record, script));
      OP1(SLJIT_MOV_UB, scriptreg, 0, SLJIT_MEM1(TMP2), 0);
/* Generating code. */
cc = ccbegin;
charoffset = 0;
numberofcmps = 0;
typeoffset = 0;
  compares--;
  invertcmp = (compares == 0 && list != fallbacks);
    cc ++;
      GETCHARINC(c, cc);
      c = *cc++;
    if (numberofcmps < 3 && (*cc == XCL_SINGLE || *cc == XCL_RANGE))
      OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, c - charoffset);
      COND_VALUE(numberofcmps == 0 ? SLJIT_MOV : SLJIT_OR, TMP2, 0, SLJIT_C_EQUAL);
      numberofcmps++;
    else if (numberofcmps > 0)
      jump = JUMP(SLJIT_C_NOT_ZERO ^ invertcmp);
      jump = CMP(SLJIT_C_EQUAL ^ invertcmp, TMP1, 0, SLJIT_IMM, c - charoffset);
    SET_CHAR_OFFSET(c);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, c - charoffset);
      COND_VALUE(numberofcmps == 0 ? SLJIT_MOV : SLJIT_OR, TMP2, 0, SLJIT_C_LESS_EQUAL);
      COND_VALUE(SLJIT_OR | SLJIT_SET_E, TMP2, 0, SLJIT_C_LESS_EQUAL);
      jump = CMP(SLJIT_C_LESS_EQUAL ^ invertcmp, TMP1, 0, SLJIT_IMM, c - charoffset);
    if (*cc == XCL_NOTPROP)
      invertcmp ^= 0x1;
      if (list != fallbacks)
        if ((cc[-1] == XCL_NOTPROP && compares > 0) || (cc[-1] == XCL_PROP && compares == 0))
      else if (cc[-1] == XCL_NOTPROP)
      jump = JUMP(SLJIT_JUMP);
      OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_Lu - typeoffset);
      OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_Ll - typeoffset);
      OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_Lt - typeoffset);
      c = PRIV(ucp_typerange)[(int)cc[1] * 2];
      SET_TYPE_OFFSET(c);
      jump = CMP(SLJIT_C_LESS_EQUAL ^ invertcmp, typereg, 0, SLJIT_IMM, PRIV(ucp_typerange)[(int)cc[1] * 2 + 1] - c);
      jump = CMP(SLJIT_C_EQUAL ^ invertcmp, typereg, 0, SLJIT_IMM, (int)cc[1] - typeoffset);
      jump = CMP(SLJIT_C_EQUAL ^ invertcmp, scriptreg, 0, SLJIT_IMM, (int)cc[1]);
      if (*cc == PT_SPACE)
        jump = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, 11 - charoffset);
      SET_CHAR_OFFSET(9);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, 13 - 9);
      SET_TYPE_OFFSET(ucp_Zl);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_Zs - ucp_Zl);
      OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, CHAR_UNDERSCORE - charoffset);
      /* ... fall through */
      SET_TYPE_OFFSET(ucp_Ll);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_Lu - ucp_Ll);
      COND_VALUE((*cc == PT_ALNUM) ? SLJIT_MOV : SLJIT_OR, TMP2, 0, SLJIT_C_LESS_EQUAL);
      SET_TYPE_OFFSET(ucp_Nd);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, typereg, 0, SLJIT_IMM, ucp_No - ucp_Nd);
    add_jump(compiler, compares > 0 ? list : fallbacks, jump);
if (found != NULL)
  set_jumps(found, LABEL());
#undef SET_TYPE_OFFSET
#undef SET_CHAR_OFFSET
static pcre_uchar *compile_char1_hotpath(compiler_common *common, pcre_uchar type, pcre_uchar *cc, jump_list **fallbacks)
compare_context context;
struct sljit_jump *jump[4];
pcre_uchar propdata[5];
switch(type)
  add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, STR_PTR, 0, TMP1, 0));
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, str));
  add_jump(compiler, &common->wordboundary, JUMP(SLJIT_FAST_CALL));
  add_jump(compiler, fallbacks, JUMP(type == OP_NOT_WORD_BOUNDARY ? SLJIT_C_NOT_ZERO : SLJIT_C_ZERO));
  read_char8_type(common);
  OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, ctype_digit);
  add_jump(compiler, fallbacks, JUMP(type == OP_DIGIT ? SLJIT_C_ZERO : SLJIT_C_NOT_ZERO));
  OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, ctype_space);
  add_jump(compiler, fallbacks, JUMP(type == OP_WHITESPACE ? SLJIT_C_ZERO : SLJIT_C_NOT_ZERO));
  OP2(SLJIT_AND | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, ctype_word);
  add_jump(compiler, fallbacks, JUMP(type == OP_WORDCHAR ? SLJIT_C_ZERO : SLJIT_C_NOT_ZERO));
    jump[0] = CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, (common->newline >> 8) & 0xff);
    jump[1] = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
    add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, common->newline & 0xff));
    JUMPHERE(jump[1]);
    JUMPHERE(jump[0]);
    check_newlinechar(common, common->nltype, fallbacks, TRUE);
    jump[0] = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xc0);
    jump[0] = CMP(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, 0xd800);
  propdata[0] = 0;
  propdata[1] = type == OP_NOTPROP ? XCL_NOTPROP : XCL_PROP;
  propdata[2] = cc[0];
  propdata[3] = cc[1];
  propdata[4] = XCL_END;
  compile_xclass_hotpath(common, propdata, fallbacks);
  return cc + 2;
  jump[0] = CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_CR);
  jump[2] = CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_NL);
  jump[3] = JUMP(SLJIT_JUMP);
  check_newlinechar(common, common->bsr_nltype, fallbacks, FALSE);
  JUMPHERE(jump[2]);
  JUMPHERE(jump[3]);
  add_jump(compiler, &common->hspace, JUMP(SLJIT_FAST_CALL));
  add_jump(compiler, fallbacks, JUMP(type == OP_NOT_HSPACE ? SLJIT_C_NOT_ZERO : SLJIT_C_ZERO));
  add_jump(compiler, &common->vspace, JUMP(SLJIT_FAST_CALL));
  add_jump(compiler, fallbacks, JUMP(type == OP_NOT_VSPACE ? SLJIT_C_NOT_ZERO : SLJIT_C_ZERO));
  OP2(SLJIT_SUB, TMP1, 0, TMP1, 0, SLJIT_IMM, ucp_Mc);
  add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS_EQUAL, TMP1, 0, SLJIT_IMM, ucp_Mn - ucp_Mc));
  jump[0] = CMP(SLJIT_C_GREATER_EQUAL, STR_PTR, 0, STR_END, 0);
  OP1(SLJIT_MOV, TMP3, 0, STR_PTR, 0);
  CMPTO(SLJIT_C_LESS_EQUAL, TMP1, 0, SLJIT_IMM, ucp_Mn - ucp_Mc, label);
  OP1(SLJIT_MOV, STR_PTR, 0, TMP3, 0);
    OP2(SLJIT_ADD, TMP2, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(2));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, STR_END, 0));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, (common->newline >> 8) & 0xff));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, common->newline & 0xff));
  else if (common->nltype == NLTYPE_FIXED)
    OP2(SLJIT_ADD, TMP2, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(1));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, common->newline));
    jump[1] = CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_CR);
    OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP2, 0, STR_END, 0);
    jump[2] = JUMP(SLJIT_C_GREATER);
    add_jump(compiler, fallbacks, JUMP(SLJIT_C_LESS));
    /* Equal. */
    OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(STR_PTR), IN_UCHARS(1));
    jump[3] = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_NL);
    add_jump(compiler, fallbacks, JUMP(SLJIT_JUMP));
    if (common->nltype == NLTYPE_ANYCRLF)
      add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS, TMP2, 0, STR_END, 0));
      add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, CHAR_NL));
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, STR_PTR, 0);
      add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, STR_PTR, 0, STR_END, 0));
      add_jump(compiler, fallbacks, JUMP(SLJIT_C_ZERO));
      OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1);
  OP1(SLJIT_MOV, TMP2, 0, ARGUMENTS, 0);
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(TMP2), SLJIT_OFFSETOF(jit_arguments, begin));
  add_jump(compiler, fallbacks, CMP(SLJIT_C_GREATER, STR_PTR, 0, TMP1, 0));
  OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP2), SLJIT_OFFSETOF(jit_arguments, notbol));
  add_jump(compiler, fallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, 0));
  jump[1] = CMP(SLJIT_C_GREATER, STR_PTR, 0, TMP1, 0);
  jump[0] = JUMP(SLJIT_JUMP);
  add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, STR_PTR, 0, STR_END, 0));
    OP2(SLJIT_SUB, TMP2, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(2));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS, TMP2, 0, TMP1, 0));
    check_newlinechar(common, common->nltype, fallbacks, FALSE);
  OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP2), SLJIT_OFFSETOF(jit_arguments, noteol));
  if (!common->endonly)
    compile_char1_hotpath(common, OP_EODN, cc, fallbacks);
    add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS, STR_PTR, 0, STR_END, 0));
  jump[1] = CMP(SLJIT_C_LESS, STR_PTR, 0, STR_END, 0);
    add_jump(compiler, fallbacks, CMP(SLJIT_C_GREATER, TMP2, 0, STR_END, 0));
  if (UTF_ENABLED(common->utf) && HAS_EXTRALEN(*cc)) length += GET_EXTRALEN(*cc);
  if (type == OP_CHAR || !char_has_othercase(common, cc) || char_get_othercase_bit(common, cc) != 0)
    OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(length));
    add_jump(compiler, fallbacks, CMP(SLJIT_C_GREATER, STR_PTR, 0, STR_END, 0));
    context.length = IN_UCHARS(length);
    context.sourcereg = -1;
    context.ucharptr = 0;
    return byte_sequence_compare(common, type == OP_CHARI, cc, &context, fallbacks);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, c);
  OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_IMM, char_othercase(common, c));
  return cc + length;
      OP1(SLJIT_MOV_UB, TMP1, 0, SLJIT_MEM1(STR_PTR), 0);
      if (type == OP_NOT || !char_has_othercase(common, cc))
        add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, c));
        /* Since UTF8 code page is fixed, we know that c is in [a-z] or [A-Z] range. */
        OP2(SLJIT_OR, TMP2, 0, TMP1, 0, SLJIT_IMM, 0x20);
        add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, c | 0x20));
      /* Skip the variable-length character. */
      OP1(MOV_UCHAR, TMP1, 0, SLJIT_MEM1(TMP1), (sljit_w)PRIV(utf8_table4) - 0xc0);
      GETCHARLEN(c, cc, length);
    oc = char_othercase(common, c);
      OP2(SLJIT_OR, TMP1, 0, TMP1, 0, SLJIT_IMM, bit);
      add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, c | bit));
      add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, oc));
  jump[0] = NULL;
  /* This check only affects 8 bit mode. In other modes, we
  always need to compare the value with 255. */
    jump[0] = CMP(SLJIT_C_GREATER, TMP1, 0, SLJIT_IMM, 255);
    if (type == OP_CLASS)
      add_jump(compiler, fallbacks, jump[0]);
#endif /* SUPPORT_UTF || !COMPILE_PCRE8 */
  if (jump[0] != NULL)
  return cc + 32 / sizeof(pcre_uchar);
  compile_xclass_hotpath(common, cc + LINK_SIZE, fallbacks);
  return cc + GET(cc, 0) - 1;
  length = GET(cc, 0);
  SLJIT_ASSERT(length > 0);
    OP1(SLJIT_MOV, TMP3, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, begin));
    OP1(SLJIT_MOV, TMP2, 0, SLJIT_IMM, length);
    add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS_EQUAL, STR_PTR, 0, TMP3, 0));
    OP2(SLJIT_SUB | SLJIT_SET_E, TMP2, 0, TMP2, 0, SLJIT_IMM, 1);
    return cc + LINK_SIZE;
  OP2(SLJIT_SUB, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(length));
  add_jump(compiler, fallbacks, CMP(SLJIT_C_LESS, STR_PTR, 0, TMP1, 0));
static SLJIT_INLINE pcre_uchar *compile_charn_hotpath(compiler_common *common, pcre_uchar *cc, pcre_uchar *ccend, jump_list **fallbacks)
/* This function consumes at least one input character. */
/* To decrease the number of length checks, we try to concatenate the fixed length character sequences. */
pcre_uchar *ccbegin = cc;
int size;
context.length = 0;
  if (*cc == OP_CHAR)
    size = 1;
    if (UTF_ENABLED(common->utf) && HAS_EXTRALEN(cc[1]))
      size += GET_EXTRALEN(cc[1]);
  else if (*cc == OP_CHARI)
      if (char_has_othercase(common, cc + 1) && char_get_othercase_bit(common, cc + 1) == 0)
        size = 0;
      else if (HAS_EXTRALEN(cc[1]))
  cc += 1 + size;
  context.length += IN_UCHARS(size);
while (size > 0 && context.length <= 128);
if (context.length > 0)
  /* We have a fixed-length byte sequence. */
  OP2(SLJIT_ADD, STR_PTR, 0, STR_PTR, 0, SLJIT_IMM, context.length);
  do cc = byte_sequence_compare(common, *cc == OP_CHARI, cc + 1, &context, fallbacks); while (context.length > 0);
/* A non-fixed length character will be checked if length == 0. */
return compile_char1_hotpath(common, *cc, cc + 1, fallbacks);
static struct sljit_jump *compile_ref_checks(compiler_common *common, pcre_uchar *cc, jump_list **fallbacks)
int offset = GET2(cc, 1) << 1;
if (!common->jscript_compat)
  if (fallbacks == NULL)
    OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1));
    OP2(SLJIT_SUB | SLJIT_SET_E, SLJIT_UNUSED, 0, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1));
    return JUMP(SLJIT_C_NOT_ZERO);
  add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1)));
return CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1));
/* Forward definitions. */
static void compile_hotpath(compiler_common *, pcre_uchar *, pcre_uchar *, fallback_common *);
static void compile_fallbackpath(compiler_common *, struct fallback_common *);
#define PUSH_FALLBACK(size, ccstart, error) \
    fallback = sljit_alloc_memory(compiler, (size)); \
    if (SLJIT_UNLIKELY(sljit_get_compiler_error(compiler))) \
      return error; \
    memset(fallback, 0, size); \
    fallback->prev = parent->top; \
    fallback->cc = (ccstart); \
    parent->top = fallback; \
  while (0)
#define PUSH_FALLBACK_NOVALUE(size, ccstart) \
      return; \
#define FALLBACK_AS(type) ((type*)fallback)
static pcre_uchar *compile_ref_hotpath(compiler_common *common, pcre_uchar *cc, jump_list **fallbacks, BOOL withchecks, BOOL emptyfail)
if (withchecks && !common->jscript_compat)
if (UTF_ENABLED(common->utf) && *cc == OP_REFI)
  SLJIT_ASSERT(TMP1 == SLJIT_TEMPORARY_REG1 && STACK_TOP == SLJIT_TEMPORARY_REG2 && TMP2 == SLJIT_TEMPORARY_REG3);
  if (withchecks)
    jump = CMP(SLJIT_C_EQUAL, TMP1, 0, TMP2, 0);
  /* Needed to save important temporary registers. */
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, STACK_TOP, 0);
  OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG2, 0, ARGUMENTS, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_TEMPORARY_REG2), SLJIT_OFFSETOF(jit_arguments, ptr), STR_PTR, 0);
  sljit_emit_ijump(compiler, SLJIT_CALL3, SLJIT_IMM, SLJIT_FUNC_OFFSET(do_utf_caselesscmp));
  OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0);
  add_jump(compiler, fallbacks, CMP(SLJIT_C_EQUAL, SLJIT_RETURN_REG, 0, SLJIT_IMM, 0));
  OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_RETURN_REG, 0);
  OP2(SLJIT_SUB | SLJIT_SET_E, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1), TMP1, 0);
    jump = JUMP(SLJIT_C_ZERO);
  add_jump(compiler, *cc == OP_REF ? &common->casefulcmp : &common->caselesscmp, JUMP(SLJIT_FAST_CALL));
  if (emptyfail)
    add_jump(compiler, fallbacks, jump);
return cc + 1 + IMM2_SIZE;
static SLJIT_INLINE pcre_uchar *compile_ref_iterator_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
fallback_common *fallback;
pcre_uchar type;
struct sljit_jump *zerolength;
int min = 0, max = 0;
BOOL minimize;
PUSH_FALLBACK(sizeof(iterator_fallback), cc, NULL);
type = cc[1 + IMM2_SIZE];
minimize = (type & 0x1) != 0;
  min = GET2(cc, 1 + IMM2_SIZE + 1);
  max = GET2(cc, 1 + IMM2_SIZE + 1 + IMM2_SIZE);
  cc += 1 + IMM2_SIZE + 1 + 2 * IMM2_SIZE;
if (!minimize)
  if (min == 0)
    allocate_stack(common, 2);
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), STR_PTR, 0);
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, 0);
    /* Temporary release of STR_PTR. */
    OP2(SLJIT_SUB, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, sizeof(sljit_w));
    zerolength = compile_ref_checks(common, ccbegin, NULL);
    /* Restore if not zero length. */
    OP2(SLJIT_ADD, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, sizeof(sljit_w));
    allocate_stack(common, 1);
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), SLJIT_IMM, 0);
    zerolength = compile_ref_checks(common, ccbegin, &fallback->topfallbacks);
  if (min > 1 || max > 1)
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, SLJIT_IMM, 0);
  compile_ref_hotpath(common, ccbegin, &fallback->topfallbacks, FALSE, FALSE);
    OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0);
    OP2(SLJIT_ADD, TMP1, 0, TMP1, 0, SLJIT_IMM, 1);
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, TMP1, 0);
    if (min > 1)
      CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, min, label);
    if (max > 1)
      jump = CMP(SLJIT_C_GREATER_EQUAL, TMP1, 0, SLJIT_IMM, max);
      JUMPTO(SLJIT_JUMP, label);
  if (max == 0)
    /* Includes min > 1 case as well. */
  JUMPHERE(zerolength);
  FALLBACK_AS(iterator_fallback)->hotpath = LABEL();
  decrease_call_count(common);
if (type != OP_CRMINSTAR)
if (max > 0)
  add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_GREATER_EQUAL, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, max));
compile_ref_hotpath(common, ccbegin, &fallback->topfallbacks, TRUE, TRUE);
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(STACK_TOP), STACK(1));
  OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(1), TMP1, 0);
  CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, min, FALLBACK_AS(iterator_fallback)->hotpath);
else if (max > 0)
  OP2(SLJIT_ADD, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, 1);
static SLJIT_INLINE pcre_uchar *compile_recurse_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
recurse_entry *entry = common->entries;
recurse_entry *prev = NULL;
int start = GET(cc, 1);
PUSH_FALLBACK(sizeof(recurse_fallback), cc, NULL);
while (entry != NULL)
  if (entry->start == start)
  prev = entry;
  entry = entry->next;
if (entry == NULL)
  entry = sljit_alloc_memory(compiler, sizeof(recurse_entry));
  if (SLJIT_UNLIKELY(sljit_get_compiler_error(compiler)))
  entry->next = NULL;
  entry->entry = NULL;
  entry->calls = NULL;
  entry->start = start;
  if (prev != NULL)
    prev->next = entry;
    common->entries = entry;
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0));
OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), TMP2, 0);
if (entry->entry == NULL)
  add_jump(compiler, &entry->calls, JUMP(SLJIT_FAST_CALL));
  JUMPTO(SLJIT_FAST_CALL, entry->entry);
/* Leave if the match is failed. */
add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, 0));
static pcre_uchar *compile_assert_hotpath(compiler_common *common, pcre_uchar *cc, assert_fallback *fallback, BOOL conditional)
fallback_common altfallback;
pcre_uchar opcode;
pcre_uchar bra = OP_BRA;
jump_list *tmp = NULL;
jump_list **target = (conditional) ? &fallback->condfailed : &fallback->common.topfallbacks;
jump_list **found;
/* Saving previous accept variables. */
struct sljit_label *save_acceptlabel = common->acceptlabel;
struct sljit_jump *brajump = NULL;
jump_list *save_accept = common->accept;
if (*cc == OP_BRAZERO || *cc == OP_BRAMINZERO)
  SLJIT_ASSERT(!conditional);
  bra = *cc;
localptr = PRIV_DATA(cc);
SLJIT_ASSERT(localptr != 0);
framesize = get_framesize(common, cc, FALSE);
fallback->framesize = framesize;
fallback->localptr = localptr;
opcode = *cc;
SLJIT_ASSERT(opcode >= OP_ASSERT && opcode <= OP_ASSERTBACK_NOT);
found = (opcode == OP_ASSERT || opcode == OP_ASSERTBACK) ? &tmp : target;
cc += GET(cc, 1);
if (bra == OP_BRAMINZERO)
  /* This is a braminzero fallback path. */
  OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(STACK_TOP), STACK(0));
  free_stack(common, 1);
  brajump = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_IMM, 0);
if (framesize < 0)
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, STACK_TOP, 0);
  allocate_stack(common, framesize + 2);
  OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr);
  OP2(SLJIT_SUB, TMP2, 0, STACK_TOP, 0, SLJIT_IMM, -STACK(framesize + 1));
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, TMP2, 0);
  init_frame(common, ccbegin, framesize + 1, 2, FALSE);
memset(&altfallback, 0, sizeof(fallback_common));
while (1)
  common->acceptlabel = NULL;
  common->accept = NULL;
  altfallback.top = NULL;
  altfallback.topfallbacks = NULL;
  if (*ccbegin == OP_ALT)
  altfallback.cc = ccbegin;
  compile_hotpath(common, ccbegin + 1 + LINK_SIZE, cc, &altfallback);
    common->acceptlabel = save_acceptlabel;
    common->accept = save_accept;
  common->acceptlabel = LABEL();
  if (common->accept != NULL)
    set_jumps(common->accept, common->acceptlabel);
  /* Reset stack. */
    OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr);
    if ((opcode != OP_ASSERT_NOT && opcode != OP_ASSERTBACK_NOT) || conditional)
      /* We don't need to keep the STR_PTR, only the previous localptr. */
      OP2(SLJIT_ADD, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_IMM, (framesize + 1) * sizeof(sljit_w));
      add_jump(compiler, &common->revertframes, JUMP(SLJIT_FAST_CALL));
  if (opcode == OP_ASSERT_NOT || opcode == OP_ASSERTBACK_NOT)
    /* We know that STR_PTR was stored on the top of the stack. */
    if (conditional)
      OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(STACK_TOP), 0);
    else if (bra == OP_BRAZERO)
        OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(STACK_TOP), framesize * sizeof(sljit_w));
        OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(STACK_TOP), (framesize + 1) * sizeof(sljit_w));
        OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, TMP1, 0);
    else if (framesize >= 0)
      /* For OP_BRA and OP_BRAMINZERO. */
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_MEM1(STACK_TOP), framesize * sizeof(sljit_w));
  add_jump(compiler, found, JUMP(SLJIT_JUMP));
  compile_fallbackpath(common, altfallback.top);
  set_jumps(altfallback.topfallbacks, LABEL());
  if (*cc != OP_ALT)
/* None of them matched. */
if (opcode == OP_ASSERT || opcode == OP_ASSERTBACK)
  /* Assert is failed. */
  if (conditional || bra == OP_BRAZERO)
    /* The topmost item should be 0. */
    if (bra == OP_BRAZERO)
      free_stack(common, framesize + 1);
      free_stack(common, framesize + 2);
  if (bra != OP_BRAZERO)
    add_jump(compiler, target, jump);
  /* Assert is successful. */
  set_jumps(tmp, LABEL());
    /* Keep the STR_PTR on the top of the stack. */
    else if (bra == OP_BRAMINZERO)
    if (bra == OP_BRA)
      OP2(SLJIT_ADD, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_IMM, (framesize + 2) * sizeof(sljit_w));
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), bra == OP_BRAZERO ? STR_PTR : SLJIT_IMM, 0);
    fallback->hotpath = LABEL();
    sljit_set_label(jump, fallback->hotpath);
    JUMPTO(SLJIT_JUMP, fallback->hotpath);
    JUMPHERE(brajump);
    if (framesize >= 0)
    set_jumps(fallback->common.topfallbacks, LABEL());
  /* AssertNot is successful. */
    if (bra != OP_BRA)
    SLJIT_ASSERT(found == &fallback->common.topfallbacks);
    fallback->common.topfallbacks = NULL;
static sljit_w SLJIT_CALL do_searchovector(sljit_w refno, sljit_w* locals, pcre_uchar *name_table)
int condition = FALSE;
pcre_uchar *slotA = name_table;
pcre_uchar *slotB;
sljit_w name_count = locals[LOCALS0 / sizeof(sljit_w)];
sljit_w name_entry_size = locals[LOCALS1 / sizeof(sljit_w)];
sljit_w no_capture;
locals += OVECTOR_START / sizeof(sljit_w);
no_capture = locals[1];
for (i = 0; i < name_count; i++)
  slotA += name_entry_size;
if (i < name_count)
  while (slotB > name_table)
    slotB -= name_entry_size;
      condition = locals[GET2(slotB, 0) << 1] != no_capture;
    for (i++; i < name_count; i++)
      slotB += name_entry_size;
return condition;
static sljit_w SLJIT_CALL do_searchgroups(sljit_w recno, sljit_w* locals, pcre_uchar *name_table)
sljit_w group_num = locals[POSSESSIVE0 / sizeof(sljit_w)];
      condition = GET2(slotB, 0) == group_num;
  Handling bracketed expressions is probably the most complex part.
  Stack layout naming characters:
    S - Push the current STR_PTR
    0 - Push a 0 (NULL)
    A - Push the current STR_PTR. Needed for restoring the STR_PTR
        before the next alternative. Not pushed if there are no alternatives.
    M - Any values pushed by the current alternative. Can be empty, or anything.
    C - Push the previous OVECTOR(i), OVECTOR(i+1) and OVECTOR_PRIV(i) to the stack.
    L - Push the previous local (pointed by localptr) to the stack
   () - opional values stored on the stack
  ()* - optonal, can be stored multiple times
  The following list shows the regular expression templates, their PCRE byte codes
  and stack layout supported by pcre-sljit.
  (?:)                     OP_BRA     | OP_KET                A M
  ()                       OP_CBRA    | OP_KET                C M
  (?:)+                    OP_BRA     | OP_KETRMAX        0   A M S   ( A M S )*
                           OP_SBRA    | OP_KETRMAX        0   L M S   ( L M S )*
  (?:)+?                   OP_BRA     | OP_KETRMIN        0   A M S   ( A M S )*
                           OP_SBRA    | OP_KETRMIN        0   L M S   ( L M S )*
  ()+                      OP_CBRA    | OP_KETRMAX        0   C M S   ( C M S )*
                           OP_SCBRA   | OP_KETRMAX        0   C M S   ( C M S )*
  ()+?                     OP_CBRA    | OP_KETRMIN        0   C M S   ( C M S )*
                           OP_SCBRA   | OP_KETRMIN        0   C M S   ( C M S )*
  (?:)?    OP_BRAZERO    | OP_BRA     | OP_KET            S ( A M 0 )
  (?:)??   OP_BRAMINZERO | OP_BRA     | OP_KET            S ( A M 0 )
  ()?      OP_BRAZERO    | OP_CBRA    | OP_KET            S ( C M 0 )
  ()??     OP_BRAMINZERO | OP_CBRA    | OP_KET            S ( C M 0 )
  (?:)*    OP_BRAZERO    | OP_BRA     | OP_KETRMAX      S 0 ( A M S )*
           OP_BRAZERO    | OP_SBRA    | OP_KETRMAX      S 0 ( L M S )*
  (?:)*?   OP_BRAMINZERO | OP_BRA     | OP_KETRMIN      S 0 ( A M S )*
           OP_BRAMINZERO | OP_SBRA    | OP_KETRMIN      S 0 ( L M S )*
  ()*      OP_BRAZERO    | OP_CBRA    | OP_KETRMAX      S 0 ( C M S )*
           OP_BRAZERO    | OP_SCBRA   | OP_KETRMAX      S 0 ( C M S )*
  ()*?     OP_BRAMINZERO | OP_CBRA    | OP_KETRMIN      S 0 ( C M S )*
           OP_BRAMINZERO | OP_SCBRA   | OP_KETRMIN      S 0 ( C M S )*
    A - Push the alternative index (starting from 0) on the stack.
        Not pushed if there is no alternatives.
  The next list shows the possible content of a bracket:
  (|)     OP_*BRA    | OP_ALT ...         M A
  (?()|)  OP_*COND   | OP_ALT             M A
  (?>|)   OP_ONCE    | OP_ALT ...         [stack trace] M A
  (?>|)   OP_ONCE_NC | OP_ALT ...         [stack trace] M A
                                          Or nothing, if trace is unnecessary
static pcre_uchar *compile_bracket_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
int localptr = 0;
int offset = 0;
pcre_uchar *hotpath;
pcre_uchar ket;
BOOL has_alternatives;
struct sljit_jump *skip;
struct sljit_label *rmaxlabel = NULL;
struct sljit_jump *braminzerojump = NULL;
PUSH_FALLBACK(sizeof(bracket_fallback), cc, NULL);
hotpath = ccbegin + 1 + LINK_SIZE;
if ((opcode == OP_COND || opcode == OP_SCOND) && cc[1 + LINK_SIZE] == OP_DEF)
  /* Drop this bracket_fallback. */
  parent->top = fallback->prev;
  return bracketend(cc);
ket = *(bracketend(cc) - 1 - LINK_SIZE);
SLJIT_ASSERT(ket == OP_KET || ket == OP_KETRMAX || ket == OP_KETRMIN);
SLJIT_ASSERT(!((bra == OP_BRAZERO && ket == OP_KETRMIN) || (bra == OP_BRAMINZERO && ket == OP_KETRMAX)));
has_alternatives = *cc == OP_ALT;
if (SLJIT_UNLIKELY(opcode == OP_COND) || SLJIT_UNLIKELY(opcode == OP_SCOND))
  has_alternatives = (*hotpath == OP_RREF) ? FALSE : TRUE;
  if (*hotpath == OP_NRREF)
    stacksize = GET2(hotpath, 1);
    if (common->currententry == NULL || stacksize == RREF_ANY)
      has_alternatives = FALSE;
    else if (common->currententry->start == 0)
      has_alternatives = stacksize != 0;
      has_alternatives = stacksize != GET2(common->start, common->currententry->start + 1 + LINK_SIZE);
if (SLJIT_UNLIKELY(opcode == OP_COND) && (*cc == OP_KETRMAX || *cc == OP_KETRMIN))
  opcode = OP_SCOND;
if (SLJIT_UNLIKELY(opcode == OP_ONCE_NC))
  opcode = OP_ONCE;
if (opcode == OP_CBRA || opcode == OP_SCBRA)
  /* Capturing brackets has a pre-allocated space. */
  offset = GET2(ccbegin, 1 + LINK_SIZE);
  localptr = OVECTOR_PRIV(offset);
  offset <<= 1;
  FALLBACK_AS(bracket_fallback)->localptr = localptr;
  hotpath += IMM2_SIZE;
else if (opcode == OP_ONCE || opcode == OP_SBRA || opcode == OP_SCOND)
  /* Other brackets simply allocate the next entry. */
  localptr = PRIV_DATA(ccbegin);
  if (opcode == OP_ONCE)
    FALLBACK_AS(bracket_fallback)->u.framesize = get_framesize(common, ccbegin, FALSE);
/* Instructions before the first alternative. */
stacksize = 0;
if ((ket == OP_KETRMAX) || (ket == OP_KETRMIN && bra != OP_BRAMINZERO))
  stacksize++;
if (stacksize > 0)
  allocate_stack(common, stacksize);
  OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stacksize), SLJIT_IMM, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stacksize), STR_PTR, 0);
  /* This is a fallback path! (Since the hot-path of OP_BRAMINZERO matches to the empty string) */
  if (ket != OP_KETRMIN)
    braminzerojump = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_IMM, 0);
    if (opcode == OP_ONCE || opcode >= OP_SBRA)
      jump = CMP(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_IMM, 0);
      OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(STACK_TOP), STACK(1));
      /* Nothing stored during the first run. */
      skip = JUMP(SLJIT_JUMP);
      /* Checking zero-length iteration. */
      if (opcode != OP_ONCE || FALLBACK_AS(bracket_fallback)->u.framesize < 0)
        /* When we come from outside, localptr contains the previous STR_PTR. */
        braminzerojump = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr);
        /* Except when the whole stack frame must be saved. */
        braminzerojump = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_MEM1(TMP1), (FALLBACK_AS(bracket_fallback)->u.framesize + 1) * sizeof(sljit_w));
      JUMPHERE(skip);
if (ket == OP_KETRMIN)
  FALLBACK_AS(bracket_fallback)->recursivehotpath = LABEL();
if (ket == OP_KETRMAX)
  rmaxlabel = LABEL();
  if (has_alternatives && opcode != OP_ONCE && opcode < OP_SBRA)
    FALLBACK_AS(bracket_fallback)->althotpath = rmaxlabel;
/* Handling capturing brackets and alternatives. */
  if (FALLBACK_AS(bracket_fallback)->u.framesize < 0)
    /* Neither capturing brackets nor recursions are not found in the block. */
      OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr);
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(1), TMP2, 0);
      OP2(SLJIT_SUB, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, STACK_TOP, 0, SLJIT_IMM, sizeof(sljit_w));
    else if (ket == OP_KETRMAX || has_alternatives)
    if (ket == OP_KETRMIN || ket == OP_KETRMAX || has_alternatives)
      allocate_stack(common, FALLBACK_AS(bracket_fallback)->u.framesize + 2);
      OP2(SLJIT_SUB, TMP2, 0, STACK_TOP, 0, SLJIT_IMM, -STACK(FALLBACK_AS(bracket_fallback)->u.framesize + 1));
      init_frame(common, ccbegin, FALLBACK_AS(bracket_fallback)->u.framesize + 1, 2, FALSE);
      allocate_stack(common, FALLBACK_AS(bracket_fallback)->u.framesize + 1);
      OP2(SLJIT_SUB, TMP2, 0, STACK_TOP, 0, SLJIT_IMM, -STACK(FALLBACK_AS(bracket_fallback)->u.framesize));
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), TMP1, 0);
      init_frame(common, ccbegin, FALLBACK_AS(bracket_fallback)->u.framesize, 1, FALSE);
else if (opcode == OP_CBRA || opcode == OP_SCBRA)
  /* Saving the previous values. */
  allocate_stack(common, 3);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, STR_PTR, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(2), TMP1, 0);
else if (opcode == OP_SBRA || opcode == OP_SCOND)
  /* Saving the previous value. */
else if (has_alternatives)
  /* Pushing the starting string pointer. */
/* Generating code for the first alternative. */
if (opcode == OP_COND || opcode == OP_SCOND)
  if (*hotpath == OP_CREF)
    SLJIT_ASSERT(has_alternatives);
    add_jump(compiler, &(FALLBACK_AS(bracket_fallback)->u.condfailed),
      CMP(SLJIT_C_EQUAL, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(GET2(hotpath, 1) << 1), SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1)));
    hotpath += 1 + IMM2_SIZE;
  else if (*hotpath == OP_NCREF)
    jump = CMP(SLJIT_C_NOT_EQUAL, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(stacksize << 1), SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1));
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE1, STACK_TOP, 0);
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS0, SLJIT_IMM, common->name_count);
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1, SLJIT_IMM, common->name_entry_size);
    OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG1, 0, SLJIT_IMM, stacksize);
    OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG2, 0, SLJIT_LOCALS_REG, 0);
    OP1(SLJIT_MOV, SLJIT_TEMPORARY_REG3, 0, SLJIT_IMM, common->name_table);
    sljit_emit_ijump(compiler, SLJIT_CALL3, SLJIT_IMM, SLJIT_FUNC_OFFSET(do_searchovector));
    OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE1);
    add_jump(compiler, &(FALLBACK_AS(bracket_fallback)->u.condfailed), CMP(SLJIT_C_EQUAL, SLJIT_TEMPORARY_REG1, 0, SLJIT_IMM, 0));
  else if (*hotpath == OP_RREF || *hotpath == OP_NRREF)
    /* Never has other case. */
    FALLBACK_AS(bracket_fallback)->u.condfailed = NULL;
    if (common->currententry == NULL)
    else if (stacksize == RREF_ANY)
      stacksize = 1;
      stacksize = stacksize == 0;
      stacksize = stacksize == GET2(common->start, common->currententry->start + 1 + LINK_SIZE);
    if (*hotpath == OP_RREF || stacksize || common->currententry == NULL)
      SLJIT_ASSERT(!has_alternatives);
      if (stacksize != 0)
        if (*cc == OP_ALT)
          hotpath = cc + 1 + LINK_SIZE;
          hotpath = cc;
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, SLJIT_IMM, GET2(common->start, common->currententry->start + 1 + LINK_SIZE));
      sljit_emit_ijump(compiler, SLJIT_CALL3, SLJIT_IMM, SLJIT_FUNC_OFFSET(do_searchgroups));
    SLJIT_ASSERT(has_alternatives && *hotpath >= OP_ASSERT && *hotpath <= OP_ASSERTBACK_NOT);
    /* Similar code as PUSH_FALLBACK macro. */
    assert = sljit_alloc_memory(compiler, sizeof(assert_fallback));
    memset(assert, 0, sizeof(assert_fallback));
    assert->common.cc = hotpath;
    FALLBACK_AS(bracket_fallback)->u.assert = assert;
    hotpath = compile_assert_hotpath(common, hotpath, assert, TRUE);
compile_hotpath(common, hotpath, cc, fallback);
    /* TMP2 which is set here used by OP_KETRMAX below. */
      OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(STACK_TOP), 0);
    else if (ket == OP_KETRMIN)
      /* Move the STR_PTR to the localptr. */
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_MEM1(STACK_TOP), 0);
    stacksize = (ket == OP_KETRMIN || ket == OP_KETRMAX || has_alternatives) ? 2 : 1;
    OP2(SLJIT_ADD, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_IMM, (FALLBACK_AS(bracket_fallback)->u.framesize + stacksize) * sizeof(sljit_w));
      OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(STACK_TOP), STACK(0));
if (ket != OP_KET || bra != OP_BRA)
if (has_alternatives && opcode != OP_ONCE)
if (ket != OP_KET)
else if (bra != OP_BRA)
if (has_alternatives)
  if (opcode != OP_ONCE)
  if (ket != OP_KETRMAX)
    FALLBACK_AS(bracket_fallback)->althotpath = LABEL();
/* Must be after the hotpath label. */
if (offset != 0)
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1), STR_PTR, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 0), TMP1, 0);
      CMPTO(SLJIT_C_NOT_EQUAL, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, STR_PTR, 0, rmaxlabel);
      /* TMP2 must contain the starting STR_PTR. */
      CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, STR_PTR, 0, rmaxlabel);
    JUMPTO(SLJIT_JUMP, rmaxlabel);
  FALLBACK_AS(bracket_fallback)->zerohotpath = LABEL();
  /* This is a fallback path! (From the viewpoint of OP_BRAMINZERO) */
  JUMPTO(SLJIT_JUMP, ((braminzero_fallback*)parent)->hotpath);
  if (braminzerojump != NULL)
    JUMPHERE(braminzerojump);
    /* We need to release the end pointer to perform the
    fallback for the zero-length iteration. When
    framesize is < 0, OP_ONCE will do the release itself. */
    if (opcode == OP_ONCE && FALLBACK_AS(bracket_fallback)->u.framesize >= 0)
    else if (ket == OP_KETRMIN && opcode != OP_ONCE)
  /* Continue to the normal fallback. */
if ((ket != OP_KET && bra != OP_BRAMINZERO) || bra == OP_BRAZERO)
/* Skip the other alternatives. */
while (*cc == OP_ALT)
static pcre_uchar *compile_bracketpos_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
int cbraprivptr = 0;
BOOL zero = FALSE;
pcre_uchar *ccbegin = NULL;
int stack;
struct sljit_label *loop = NULL;
struct jump_list *emptymatch = NULL;
PUSH_FALLBACK(sizeof(bracketpos_fallback), cc, NULL);
if (*cc == OP_BRAPOSZERO)
  zero = TRUE;
FALLBACK_AS(bracketpos_fallback)->localptr = localptr;
switch(opcode)
  ccbegin = cc + 1 + LINK_SIZE;
  offset = GET2(cc, 1 + LINK_SIZE);
  cbraprivptr = OVECTOR_PRIV(offset);
  ccbegin = cc + 1 + LINK_SIZE + IMM2_SIZE;
FALLBACK_AS(bracketpos_fallback)->framesize = framesize;
  stacksize = (opcode == OP_CBRAPOS || opcode == OP_SCBRAPOS) ? 2 : 1;
  if (!zero)
  FALLBACK_AS(bracketpos_fallback)->stacksize = stacksize;
  if (opcode == OP_CBRAPOS || opcode == OP_SCBRAPOS)
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stacksize - 1), SLJIT_IMM, 1);
  stacksize = framesize + 1;
  if (opcode == OP_BRAPOS || opcode == OP_SBRAPOS)
  OP2(SLJIT_SUB, TMP2, 0, STACK_TOP, 0, SLJIT_IMM, -STACK(stacksize - 1));
  stack = 0;
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(0), SLJIT_IMM, 1);
    stack++;
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stack), STR_PTR, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stack), TMP1, 0);
  init_frame(common, cc, stacksize - 1, stacksize - framesize, FALSE);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), cbraprivptr, STR_PTR, 0);
while (*cc != OP_KETRPOS)
  fallback->top = NULL;
  fallback->topfallbacks = NULL;
  compile_hotpath(common, ccbegin, cc, fallback);
      OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), cbraprivptr);
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset), TMP1, 0);
      if (opcode == OP_SBRAPOS)
        OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(STACK_TOP), STACK(0));
    if (opcode == OP_SBRAPOS || opcode == OP_SCBRAPOS)
      add_jump(compiler, &emptymatch, CMP(SLJIT_C_EQUAL, TMP1, 0, STR_PTR, 0));
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stacksize - 1), SLJIT_IMM, 0);
      OP2(SLJIT_ADD, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_IMM, stacksize * sizeof(sljit_w));
      OP2(SLJIT_ADD, STACK_TOP, 0, TMP2, 0, SLJIT_IMM, stacksize * sizeof(sljit_w));
        OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(TMP2), (framesize + 1) * sizeof(sljit_w));
      OP1(SLJIT_MOV, SLJIT_MEM1(TMP2), (framesize + 1) * sizeof(sljit_w), STR_PTR, 0);
  flush_stubs(common);
  compile_fallbackpath(common, fallback->top);
  set_jumps(fallback->topfallbacks, LABEL());
      OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), cbraprivptr);
      /* Last alternative. */
      if (*cc == OP_KETRPOS)
      OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(TMP2), (framesize + 1) * sizeof(sljit_w));
    add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_NOT_EQUAL, SLJIT_MEM1(STACK_TOP), STACK(stacksize - 1), SLJIT_IMM, 0));
  else /* TMP2 is set to [localptr] above. */
    add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_NOT_EQUAL, SLJIT_MEM1(TMP2), (stacksize - 1) * sizeof(sljit_w), SLJIT_IMM, 0));
set_jumps(emptymatch, LABEL());
static SLJIT_INLINE pcre_uchar *get_iterator_parameters(compiler_common *common, pcre_uchar *cc, pcre_uchar *opcode, pcre_uchar *type, int *arg1, int *arg2, pcre_uchar **end)
int class_len;
*opcode = *cc;
if (*opcode >= OP_STAR && *opcode <= OP_POSUPTO)
  *type = OP_CHAR;
else if (*opcode >= OP_STARI && *opcode <= OP_POSUPTOI)
  *type = OP_CHARI;
  *opcode -= OP_STARI - OP_STAR;
else if (*opcode >= OP_NOTSTAR && *opcode <= OP_NOTPOSUPTO)
  *type = OP_NOT;
  *opcode -= OP_NOTSTAR - OP_STAR;
else if (*opcode >= OP_NOTSTARI && *opcode <= OP_NOTPOSUPTOI)
  *type = OP_NOTI;
  *opcode -= OP_NOTSTARI - OP_STAR;
else if (*opcode >= OP_TYPESTAR && *opcode <= OP_TYPEPOSUPTO)
  *opcode -= OP_TYPESTAR - OP_STAR;
  *type = 0;
  SLJIT_ASSERT(*opcode >= OP_CLASS || *opcode <= OP_XCLASS);
  *type = *opcode;
  class_len = (*type < OP_XCLASS) ? (int)(1 + (32 / sizeof(pcre_uchar))) : GET(cc, 0);
  *opcode = cc[class_len - 1];
  if (*opcode >= OP_CRSTAR && *opcode <= OP_CRMINQUERY)
    *opcode -= OP_CRSTAR - OP_STAR;
    if (end != NULL)
      *end = cc + class_len;
    SLJIT_ASSERT(*opcode == OP_CRRANGE || *opcode == OP_CRMINRANGE);
    *arg1 = GET2(cc, (class_len + IMM2_SIZE));
    *arg2 = GET2(cc, class_len);
    if (*arg2 == 0)
      SLJIT_ASSERT(*arg1 != 0);
      *opcode = (*opcode == OP_CRRANGE) ? OP_UPTO : OP_MINUPTO;
    if (*arg1 == *arg2)
      *opcode = OP_EXACT;
      *end = cc + class_len + 2 * IMM2_SIZE;
if (*opcode == OP_UPTO || *opcode == OP_MINUPTO || *opcode == OP_EXACT || *opcode == OP_POSUPTO)
  *arg1 = GET2(cc, 0);
  cc += IMM2_SIZE;
if (*type == 0)
  *type = *cc;
    *end = next_opcode(common, cc);
  *end = cc + 1;
  if (UTF_ENABLED(common->utf) && HAS_EXTRALEN(*cc)) *end += GET_EXTRALEN(*cc);
static pcre_uchar *compile_iterator_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
int arg1 = -1, arg2 = -1;
pcre_uchar* end;
jump_list *nomatch = NULL;
cc = get_iterator_parameters(common, cc, &opcode, &type, &arg1, &arg2, &end);
  if (type == OP_ANYNL || type == OP_EXTUNI)
    if (opcode == OP_STAR || opcode == OP_UPTO)
    if (opcode == OP_UPTO || opcode == OP_CRRANGE)
    compile_char1_hotpath(common, type, cc, &fallback->topfallbacks);
      if (opcode == OP_CRRANGE && arg2 > 0)
        CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, arg2, label);
      if (opcode == OP_UPTO || (opcode == OP_CRRANGE && arg1 > 0))
        jump = CMP(SLJIT_C_GREATER_EQUAL, TMP1, 0, SLJIT_IMM, arg1);
    OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, 1);
    compile_char1_hotpath(common, type, cc, &nomatch);
    if (opcode <= OP_PLUS || (opcode == OP_CRRANGE && arg1 == 0))
      CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, arg1 + 1, label);
    set_jumps(nomatch, LABEL());
    if (opcode == OP_PLUS || opcode == OP_CRRANGE)
      add_jump(compiler, &fallback->topfallbacks,
        CMP(SLJIT_C_LESS, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, opcode == OP_PLUS ? 2 : arg2 + 1));
  if (opcode == OP_MINPLUS)
    add_jump(compiler, &fallback->topfallbacks, JUMP(SLJIT_JUMP));
  if (opcode == OP_CRMINRANGE)
  if (opcode == OP_QUERY)
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, SLJIT_IMM, 1);
  if (opcode != OP_POSSTAR)
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE1, STR_PTR, 0);
  if (opcode != OP_POSUPTO)
    if (opcode == OP_POSPLUS)
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, SLJIT_IMM, 2);
    add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_LESS, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE0, SLJIT_IMM, 2));
  OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), POSSESSIVE1);
return end;
static SLJIT_INLINE pcre_uchar *compile_fail_accept_hotpath(compiler_common *common, pcre_uchar *cc, fallback_common *parent)
if (*cc == OP_FAIL)
if (*cc == OP_ASSERT_ACCEPT || common->currententry != NULL)
  /* No need to check notempty conditions. */
  if (common->acceptlabel == NULL)
    add_jump(compiler, &common->accept, JUMP(SLJIT_JUMP));
    JUMPTO(SLJIT_JUMP, common->acceptlabel);
  add_jump(compiler, &common->accept, CMP(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0)));
  CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), common->acceptlabel);
OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, notempty));
add_jump(compiler, &fallback->topfallbacks, CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, 0));
OP1(SLJIT_MOV_UB, TMP2, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, notempty_atstart));
  add_jump(compiler, &common->accept, CMP(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, 0));
  CMPTO(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, 0, common->acceptlabel);
  add_jump(compiler, &common->accept, CMP(SLJIT_C_NOT_EQUAL, TMP2, 0, STR_PTR, 0));
  CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, STR_PTR, 0, common->acceptlabel);
static SLJIT_INLINE pcre_uchar *compile_close_hotpath(compiler_common *common, pcre_uchar *cc)
int offset = GET2(cc, 1);
/* Data will be discarded anyway... */
if (common->currententry != NULL)
OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR_PRIV(offset));
static void compile_hotpath(compiler_common *common, pcre_uchar *cc, pcre_uchar *ccend, fallback_common *parent)
    cc = compile_char1_hotpath(common, *cc, cc + 1, parent->top != NULL ? &parent->top->nextfallbacks : &parent->topfallbacks);
    PUSH_FALLBACK_NOVALUE(sizeof(fallback_common), cc);
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), STR_PTR, 0);
    cc = compile_charn_hotpath(common, cc, ccend, parent->top != NULL ? &parent->top->nextfallbacks : &parent->topfallbacks);
    cc = compile_iterator_hotpath(common, cc, parent);
    if (cc[1 + (32 / sizeof(pcre_uchar))] >= OP_CRSTAR && cc[1 + (32 / sizeof(pcre_uchar))] <= OP_CRMINRANGE)
    if (*(cc + GET(cc, 1)) >= OP_CRSTAR && *(cc + GET(cc, 1)) <= OP_CRMINRANGE)
    if (cc[1 + IMM2_SIZE] >= OP_CRSTAR && cc[1 + IMM2_SIZE] <= OP_CRMINRANGE)
      cc = compile_ref_iterator_hotpath(common, cc, parent);
      cc = compile_ref_hotpath(common, cc, parent->top != NULL ? &parent->top->nextfallbacks : &parent->topfallbacks, TRUE, FALSE);
    cc = compile_recurse_hotpath(common, cc, parent);
    PUSH_FALLBACK_NOVALUE(sizeof(assert_fallback), cc);
    cc = compile_assert_hotpath(common, cc, FALLBACK_AS(assert_fallback), FALSE);
    PUSH_FALLBACK_NOVALUE(sizeof(braminzero_fallback), cc);
    cc = bracketend(cc + 1);
    if (*(cc - 1 - LINK_SIZE) != OP_KETRMIN)
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(1), STR_PTR, 0);
    FALLBACK_AS(braminzero_fallback)->hotpath = LABEL();
    if (cc[1] > OP_ASSERTBACK_NOT)
    cc = compile_bracket_hotpath(common, cc, parent);
    cc = compile_bracketpos_hotpath(common, cc, parent);
    cc = compile_fail_accept_hotpath(common, cc, parent);
    cc = compile_close_hotpath(common, cc);
#undef PUSH_FALLBACK
#undef PUSH_FALLBACK_NOVALUE
#undef FALLBACK_AS
#define COMPILE_FALLBACKPATH(current) \
    compile_fallbackpath(common, (current)); \
#define CURRENT_AS(type) ((type*)current)
static void compile_iterator_fallbackpath(compiler_common *common, struct fallback_common *current)
pcre_uchar *cc = current->cc;
struct sljit_label *label = NULL;
cc = get_iterator_parameters(common, cc, &opcode, &type, &arg1, &arg2, NULL);
    set_jumps(current->topfallbacks, LABEL());
    CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_IMM, 0, CURRENT_AS(iterator_fallback)->hotpath);
      arg2 = 0;
    else if (opcode == OP_PLUS)
      arg2 = 1;
    jump = CMP(SLJIT_C_LESS_EQUAL, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, arg2 + 1);
    OP2(SLJIT_SUB, SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_MEM1(STACK_TOP), STACK(1), SLJIT_IMM, 1);
    JUMPTO(SLJIT_JUMP, CURRENT_AS(iterator_fallback)->hotpath);
    free_stack(common, 2);
    current->topfallbacks = NULL;
  compile_char1_hotpath(common, type, cc, &current->topfallbacks);
    CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, arg2 + 1, label);
  if (opcode == OP_CRMINRANGE && arg1 == 0)
    CMPTO(SLJIT_C_LESS, TMP1, 0, SLJIT_IMM, arg1 + 2, CURRENT_AS(iterator_fallback)->hotpath);
  jump = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_IMM, 0);
static void compile_ref_iterator_fallbackpath(compiler_common *common, struct fallback_common *current)
if ((type & 0x1) == 0)
static void compile_recurse_fallbackpath(compiler_common *common, struct fallback_common *current)
static void compile_assert_fallbackpath(compiler_common *common, struct fallback_common *current)
SLJIT_ASSERT(*cc != OP_BRAMINZERO);
if (*cc == OP_BRAZERO)
  SLJIT_ASSERT(current->topfallbacks == NULL);
if (CURRENT_AS(assert_fallback)->framesize < 0)
    CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_IMM, 0, CURRENT_AS(assert_fallback)->hotpath);
  if (*cc == OP_ASSERT_NOT || *cc == OP_ASSERTBACK_NOT)
if (*cc == OP_ASSERT || *cc == OP_ASSERTBACK)
  OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), CURRENT_AS(assert_fallback)->localptr);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), CURRENT_AS(assert_fallback)->localptr, SLJIT_MEM1(STACK_TOP), CURRENT_AS(assert_fallback)->framesize * sizeof(sljit_w));
  /* We know there is enough place on the stack. */
  JUMPTO(SLJIT_JUMP, CURRENT_AS(assert_fallback)->hotpath);
static void compile_bracket_fallbackpath(compiler_common *common, struct fallback_common *current)
int opcode;
int localptr = CURRENT_AS(bracket_fallback)->localptr;
pcre_uchar *ccprev;
jump_list *jumplist = NULL;
jump_list *jumplistitem = NULL;
struct sljit_jump *brazero = NULL;
struct sljit_jump *once = NULL;
struct sljit_jump *cond = NULL;
struct sljit_label *rminlabel = NULL;
ket = *(bracketend(ccbegin) - 1 - LINK_SIZE);
  has_alternatives = (ccbegin[1 + LINK_SIZE] >= OP_ASSERT && ccbegin[1 + LINK_SIZE] <= OP_ASSERTBACK_NOT) || CURRENT_AS(bracket_fallback)->u.condfailed != NULL;
  offset = (GET2(ccbegin, 1 + LINK_SIZE)) << 1;
    brazero = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, 0);
  if (bra != OP_BRAMINZERO)
    if (opcode >= OP_SBRA || opcode == OP_ONCE)
      if (opcode != OP_ONCE || CURRENT_AS(bracket_fallback)->u.framesize < 0)
        CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, CURRENT_AS(bracket_fallback)->recursivehotpath);
        CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_MEM1(TMP1), (CURRENT_AS(bracket_fallback)->u.framesize + 1) * sizeof(sljit_w), CURRENT_AS(bracket_fallback)->recursivehotpath);
      JUMPTO(SLJIT_JUMP, CURRENT_AS(bracket_fallback)->recursivehotpath);
  rminlabel = LABEL();
  brazero = CMP(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, 0);
if (SLJIT_UNLIKELY(opcode == OP_ONCE))
  if (CURRENT_AS(bracket_fallback)->u.framesize >= 0)
  once = JUMP(SLJIT_JUMP);
else if (SLJIT_UNLIKELY(opcode == OP_COND) || SLJIT_UNLIKELY(opcode == OP_SCOND))
    /* Always exactly one alternative. */
    jumplistitem = sljit_alloc_memory(compiler, sizeof(jump_list));
    if (SLJIT_UNLIKELY(!jumplistitem))
    jumplist = jumplistitem;
    jumplistitem->next = NULL;
    jumplistitem->jump = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, 1);
else if (*cc == OP_ALT)
  /* Build a jump list. Get the last successfully matched branch index. */
    /* Append as the last item. */
    if (jumplist != NULL)
      jumplistitem->next = sljit_alloc_memory(compiler, sizeof(jump_list));
      jumplistitem = jumplistitem->next;
    jumplistitem->jump = CMP(SLJIT_C_EQUAL, TMP1, 0, SLJIT_IMM, count++);
  while (*cc == OP_ALT);
  cc = ccbegin + GET(ccbegin, 1);
COMPILE_FALLBACKPATH(current->top);
if (current->topfallbacks)
  /* Conditional block always has at most one alternative. */
  if (ccbegin[1 + LINK_SIZE] >= OP_ASSERT && ccbegin[1 + LINK_SIZE] <= OP_ASSERTBACK_NOT)
    assert = CURRENT_AS(bracket_fallback)->u.assert;
    if (assert->framesize >= 0 && (ccbegin[1 + LINK_SIZE] == OP_ASSERT || ccbegin[1 + LINK_SIZE] == OP_ASSERTBACK))
      OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), assert->localptr);
      OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), assert->localptr, SLJIT_MEM1(STACK_TOP), assert->framesize * sizeof(sljit_w));
    cond = JUMP(SLJIT_JUMP);
    set_jumps(CURRENT_AS(bracket_fallback)->u.assert->condfailed, LABEL());
  else if (CURRENT_AS(bracket_fallback)->u.condfailed != NULL)
    set_jumps(CURRENT_AS(bracket_fallback)->u.condfailed, LABEL());
    current->top = NULL;
    current->nextfallbacks = NULL;
      ccprev = cc + 1 + LINK_SIZE;
      if (opcode != OP_COND && opcode != OP_SCOND)
        if (localptr != 0 && opcode != OP_ONCE)
          OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr);
      compile_hotpath(common, ccprev, cc, current);
    /* Instructions after the current alternative is succesfully matched. */
    /* There is a similar code in compile_bracket_hotpath. */
      if (CURRENT_AS(bracket_fallback)->u.framesize < 0)
        OP2(SLJIT_ADD, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_IMM, (CURRENT_AS(bracket_fallback)->u.framesize + 2) * sizeof(sljit_w));
    if (stacksize > 0) {
      if (opcode != OP_ONCE || CURRENT_AS(bracket_fallback)->u.framesize >= 0)
        /* We know we have place at least for one item on the top of the stack. */
        SLJIT_ASSERT(stacksize == 1);
      OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(stacksize), SLJIT_IMM, count++);
    JUMPTO(SLJIT_JUMP, CURRENT_AS(bracket_fallback)->althotpath);
      SLJIT_ASSERT(jumplist);
      JUMPHERE(jumplist->jump);
      jumplist = jumplist->next;
    SLJIT_ASSERT(!current->nextfallbacks);
  SLJIT_ASSERT(!jumplist);
  if (cond != NULL)
    SLJIT_ASSERT(opcode == OP_COND || opcode == OP_SCOND);
    if ((ccbegin[1 + LINK_SIZE] == OP_ASSERT_NOT || ccbegin[1 + LINK_SIZE] == OP_ASSERTBACK_NOT) && assert->framesize >= 0)
    JUMPHERE(cond);
  /* Free the STR_PTR. */
  if (localptr == 0)
  /* Using both tmp register is better for instruction scheduling. */
  OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(STACK_TOP), STACK(1));
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(offset + 1), TMP2, 0);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_MEM1(STACK_TOP), STACK(2));
  free_stack(common, 3);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_MEM1(STACK_TOP), STACK(0));
else if (opcode == OP_ONCE)
    /* Reset head and drop saved frame. */
    stacksize = (ket == OP_KETRMAX || ket == OP_KETRMIN || *cc == OP_ALT) ? 2 : 1;
    free_stack(common, CURRENT_AS(bracket_fallback)->u.framesize + stacksize);
  else if (ket == OP_KETRMAX || (*cc == OP_ALT && ket != OP_KETRMIN))
    /* The STR_PTR must be released. */
  JUMPHERE(once);
  /* Restore previous localptr */
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), localptr, SLJIT_MEM1(STACK_TOP), CURRENT_AS(bracket_fallback)->u.framesize * sizeof(sljit_w));
    /* See the comment below. */
  CMPTO(SLJIT_C_NOT_EQUAL, STR_PTR, 0, SLJIT_IMM, 0, CURRENT_AS(bracket_fallback)->recursivehotpath);
    JUMPTO(SLJIT_JUMP, CURRENT_AS(bracket_fallback)->zerohotpath);
    JUMPHERE(brazero);
  /* OP_ONCE removes everything in case of a fallback, so we don't
  need to explicitly release the STR_PTR. The extra release would
  affect badly the free_stack(2) above. */
  CMPTO(SLJIT_C_NOT_EQUAL, TMP1, 0, SLJIT_IMM, 0, rminlabel);
    free_stack(common, bra == OP_BRAMINZERO ? 2 : 1);
static void compile_bracketpos_fallbackpath(compiler_common *common, struct fallback_common *current)
if (CURRENT_AS(bracketpos_fallback)->framesize < 0)
  if (*current->cc == OP_CBRAPOS || *current->cc == OP_SCBRAPOS)
    offset = (GET2(current->cc, 1 + LINK_SIZE)) << 1;
  free_stack(common, CURRENT_AS(bracketpos_fallback)->stacksize);
OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), CURRENT_AS(bracketpos_fallback)->localptr);
  /* Drop the stack frame. */
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), CURRENT_AS(bracketpos_fallback)->localptr, SLJIT_MEM1(STACK_TOP), CURRENT_AS(bracketpos_fallback)->framesize * sizeof(sljit_w));
static void compile_braminzero_fallbackpath(compiler_common *common, struct fallback_common *current)
assert_fallback fallback;
if (current->cc[1] > OP_ASSERTBACK_NOT)
  /* Manual call of compile_bracket_hotpath and compile_bracket_fallbackpath. */
  compile_bracket_hotpath(common, current->cc, current);
  compile_bracket_fallbackpath(common, current->top);
  memset(&fallback, 0, sizeof(fallback));
  fallback.common.cc = current->cc;
  fallback.hotpath = CURRENT_AS(braminzero_fallback)->hotpath;
  /* Manual call of compile_assert_hotpath. */
  compile_assert_hotpath(common, current->cc, &fallback, FALSE);
SLJIT_ASSERT(!current->nextfallbacks && !current->topfallbacks);
static void compile_fallbackpath(compiler_common *common, struct fallback_common *current)
while (current)
  if (current->nextfallbacks != NULL)
    set_jumps(current->nextfallbacks, LABEL());
  switch(*current->cc)
    OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), TMP1, 0);
    compile_iterator_fallbackpath(common, current);
    compile_ref_iterator_fallbackpath(common, current);
    compile_recurse_fallbackpath(common, current);
    compile_assert_fallbackpath(common, current);
    compile_bracket_fallbackpath(common, current);
    compile_bracketpos_fallbackpath(common, current);
    compile_braminzero_fallbackpath(common, current);
  current = current->prev;
static SLJIT_INLINE void compile_recurse(compiler_common *common)
pcre_uchar *cc = common->start + common->currententry->start;
pcre_uchar *ccbegin = cc + 1 + LINK_SIZE + (*cc == OP_BRA ? 0 : IMM2_SIZE);
int localsize = get_localsize(common, ccbegin, ccend);
int framesize = get_framesize(common, cc, TRUE);
int alternativesize;
BOOL needsframe;
SLJIT_ASSERT(*cc == OP_BRA || *cc == OP_CBRA || *cc == OP_CBRAPOS || *cc == OP_SCBRA || *cc == OP_SCBRAPOS);
needsframe = framesize >= 0;
if (!needsframe)
  framesize = 0;
alternativesize = *(cc + GET(cc, 1)) == OP_ALT ? 1 : 0;
SLJIT_ASSERT(common->currententry->entry == NULL);
common->currententry->entry = LABEL();
set_jumps(common->currententry->calls, common->currententry->entry);
sljit_emit_fast_enter(compiler, TMP2, 0, 1, 5, 5, common->localsize);
allocate_stack(common, localsize + framesize + alternativesize);
OP1(SLJIT_MOV, SLJIT_MEM1(STACK_TOP), STACK(localsize + framesize + alternativesize - 1), TMP2, 0);
copy_locals(common, ccbegin, ccend, TRUE, localsize + framesize + alternativesize, framesize + alternativesize);
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), RECURSIVE_HEAD, STACK_TOP, 0);
if (needsframe)
  init_frame(common, cc, framesize + alternativesize - 1, alternativesize, FALSE);
if (alternativesize > 0)
  if (altfallback.cc != ccbegin)
  compile_hotpath(common, altfallback.cc, cc, &altfallback);
  altfallback.cc = cc + 1 + LINK_SIZE;
OP1(SLJIT_MOV, TMP3, 0, SLJIT_IMM, 0);
set_jumps(common->accept, LABEL());
OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), RECURSIVE_HEAD);
  OP1(SLJIT_MOV, TMP3, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0));
  OP2(SLJIT_SUB, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, (framesize + alternativesize) * sizeof(sljit_w));
  OP2(SLJIT_ADD, STACK_TOP, 0, STACK_TOP, 0, SLJIT_IMM, (framesize + alternativesize) * sizeof(sljit_w));
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), TMP3, 0);
OP1(SLJIT_MOV, TMP3, 0, SLJIT_IMM, 1);
copy_locals(common, ccbegin, ccend, FALSE, localsize + framesize + alternativesize, framesize + alternativesize);
free_stack(common, localsize + framesize + alternativesize);
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(STACK_TOP), sizeof(sljit_w));
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), RECURSIVE_HEAD, TMP2, 0);
sljit_emit_fast_return(compiler, SLJIT_MEM1(STACK_TOP), 0);
#undef COMPILE_FALLBACKPATH
#undef CURRENT_AS
void
PRIV(jit_compile)(const REAL_PCRE *re, PUBL(extra) *extra)
fallback_common rootfallback;
compiler_common common_data;
compiler_common *common = &common_data;
const pcre_uint8 *tables = re->tables;
pcre_study_data *study;
pcre_uchar *ccend;
executable_function *function;
struct sljit_label *mainloop = NULL;
struct sljit_label *empty_match_found;
struct sljit_label *empty_match_fallback;
struct sljit_jump *alloc_error;
struct sljit_jump *reqbyte_notfound = NULL;
struct sljit_jump *empty_match;
SLJIT_ASSERT((extra->flags & PCRE_EXTRA_STUDY_DATA) != 0);
study = extra->study_data;
if (!tables)
  tables = PRIV(default_tables);
memset(&rootfallback, 0, sizeof(fallback_common));
rootfallback.cc = (pcre_uchar *)re + re->name_table_offset + re->name_count * re->name_entry_size;
common->compiler = NULL;
common->start = rootfallback.cc;
common->cbraptr = OVECTOR_START + (re->top_bracket + 1) * 2 * sizeof(sljit_w);
common->fcc = tables + fcc_offset;
common->lcc = (sljit_w)(tables + lcc_offset);
common->nltype = NLTYPE_FIXED;
switch(re->options & PCRE_NEWLINE_BITS)
  /* Compile-time default */
  switch (NEWLINE)
    case -1: common->newline = (CHAR_CR << 8) | CHAR_NL; common->nltype = NLTYPE_ANY; break;
    case -2: common->newline = (CHAR_CR << 8) | CHAR_NL; common->nltype = NLTYPE_ANYCRLF; break;
    default: common->newline = NEWLINE; break;
  case PCRE_NEWLINE_CR: common->newline = CHAR_CR; break;
  case PCRE_NEWLINE_LF: common->newline = CHAR_NL; break;
       PCRE_NEWLINE_LF: common->newline = (CHAR_CR << 8) | CHAR_NL; break;
  case PCRE_NEWLINE_ANY: common->newline = (CHAR_CR << 8) | CHAR_NL; common->nltype = NLTYPE_ANY; break;
  case PCRE_NEWLINE_ANYCRLF: common->newline = (CHAR_CR << 8) | CHAR_NL; common->nltype = NLTYPE_ANYCRLF; break;
  default: return;
if ((re->options & PCRE_BSR_ANYCRLF) != 0)
  common->bsr_nltype = NLTYPE_ANYCRLF;
else if ((re->options & PCRE_BSR_UNICODE) != 0)
  common->bsr_nltype = NLTYPE_ANY;
common->endonly = (re->options & PCRE_DOLLAR_ENDONLY) != 0;
common->ctypes = (sljit_w)(tables + ctypes_offset);
common->name_table = (sljit_w)((pcre_uchar *)re + re->name_table_offset);
common->name_count = re->name_count;
common->name_entry_size = re->name_entry_size;
common->entries = NULL;
common->currententry = NULL;
common->calllimit = NULL;
common->stackalloc = NULL;
common->revertframes = NULL;
common->wordboundary = NULL;
common->anynewline = NULL;
common->hspace = NULL;
common->vspace = NULL;
common->casefulcmp = NULL;
common->caselesscmp = NULL;
common->jscript_compat = (re->options & PCRE_JAVASCRIPT_COMPAT) != 0;
#ifdef SUPPORT_UTF_OPTION
common->utf = (re->options & PCRE_UTF8) != 0;
common->use_ucp = (re->options & PCRE_UCP) != 0;
common->utfreadchar = NULL;
common->utfreadtype8 = NULL;
common->getucd = NULL;
ccend = bracketend(rootfallback.cc);
SLJIT_ASSERT(*rootfallback.cc == OP_BRA && ccend[-(1 + LINK_SIZE)] == OP_KET);
common->localsize = get_localspace(common, rootfallback.cc, ccend);
if (common->localsize < 0)
common->localsize += common->cbraptr + (re->top_bracket + 1) * sizeof(sljit_w);
if (common->localsize > SLJIT_MAX_LOCAL_SIZE)
common->localptrs = (int*)SLJIT_MALLOC((ccend - rootfallback.cc) * sizeof(int));
if (!common->localptrs)
memset(common->localptrs, 0, (ccend - rootfallback.cc) * sizeof(int));
set_localptrs(common, common->cbraptr + (re->top_bracket + 1) * sizeof(sljit_w), ccend);
compiler = sljit_create_compiler();
if (!compiler)
  SLJIT_FREE(common->localptrs);
common->compiler = compiler;
/* Main pcre_jit_exec entry. */
sljit_emit_enter(compiler, 1, 5, 5, common->localsize);
/* Register init. */
reset_ovector(common, (re->top_bracket + 1) * 2);
  OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), REQ_CHAR_PTR, SLJIT_TEMPORARY_REG1, 0);
OP1(SLJIT_MOV, ARGUMENTS, 0, SLJIT_SAVED_REG1, 0);
OP1(SLJIT_MOV, TMP1, 0, SLJIT_SAVED_REG1, 0);
OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, str));
OP1(SLJIT_MOV, STR_END, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, end));
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, stack));
OP1(SLJIT_MOV_SI, TMP1, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, calllimit));
OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(TMP2), SLJIT_OFFSETOF(struct sljit_stack, base));
OP1(SLJIT_MOV, STACK_LIMIT, 0, SLJIT_MEM1(TMP2), SLJIT_OFFSETOF(struct sljit_stack, limit));
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), CALL_LIMIT, TMP1, 0);
/* Main part of the matching */
  mainloop = mainloop_entry(common, (re->flags & PCRE_HASCRORLF) != 0, (re->options & PCRE_FIRSTLINE) != 0);
  /* Forward search if possible. */
    fast_forward_first_char(common, re->first_char, (re->flags & PCRE_FCH_CASELESS) != 0, (re->options & PCRE_FIRSTLINE) != 0);
  else if ((re->flags & PCRE_STARTLINE) != 0)
    fast_forward_newline(common, (re->options & PCRE_FIRSTLINE) != 0);
  else if ((re->flags & PCRE_STARTLINE) == 0 && study != NULL && (study->flags & PCRE_STUDY_MAPPED) != 0)
    fast_forward_start_bits(common, (sljit_uw)study->start_bits, (re->options & PCRE_FIRSTLINE) != 0);
  reqbyte_notfound = search_requested_char(common, re->req_char, (re->flags & PCRE_RCH_CASELESS) != 0, (re->flags & PCRE_FIRSTSET) != 0);
/* Store the current STR_PTR in OVECTOR(0). */
/* Copy the limit of allowed recursions. */
OP1(SLJIT_MOV, CALL_COUNT, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), CALL_LIMIT);
compile_hotpath(common, rootfallback.cc, ccend, &rootfallback);
  sljit_free_compiler(compiler);
empty_match = CMP(SLJIT_C_EQUAL, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0));
empty_match_found = LABEL();
/* This means we have a match. Update the ovector. */
copy_ovector(common, re->top_bracket + 1);
leave = LABEL();
sljit_emit_return(compiler, SLJIT_MOV, SLJIT_RETURN_REG, 0);
empty_match_fallback = LABEL();
compile_fallbackpath(common, rootfallback.top);
SLJIT_ASSERT(rootfallback.prev == NULL);
/* Check we have remaining characters. */
OP1(SLJIT_MOV, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0));
  if ((re->options & PCRE_FIRSTLINE) == 0)
    if (study != NULL && study->minlength > 1)
      OP2(SLJIT_ADD, TMP1, 0, STR_PTR, 0, SLJIT_IMM, IN_UCHARS(study->minlength));
      CMPTO(SLJIT_C_LESS_EQUAL, TMP1, 0, STR_END, 0, mainloop);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, TMP1, 0, STR_END, 0);
      COND_VALUE(SLJIT_MOV, TMP2, 0, SLJIT_C_GREATER);
      OP2(SLJIT_SUB | SLJIT_SET_U, SLJIT_UNUSED, 0, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END);
      COND_VALUE(SLJIT_OR | SLJIT_SET_E, TMP2, 0, SLJIT_C_GREATER_EQUAL);
      JUMPTO(SLJIT_C_ZERO, mainloop);
      CMPTO(SLJIT_C_LESS, STR_PTR, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), FIRSTLINE_END, mainloop);
if (reqbyte_notfound != NULL)
  JUMPHERE(reqbyte_notfound);
/* Copy OVECTOR(1) to OVECTOR(0) */
OP1(SLJIT_MOV, SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(0), SLJIT_MEM1(SLJIT_LOCALS_REG), OVECTOR(1));
OP1(SLJIT_MOV, SLJIT_RETURN_REG, 0, SLJIT_IMM, PCRE_ERROR_NOMATCH);
JUMPTO(SLJIT_JUMP, leave);
JUMPHERE(empty_match);
CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, SLJIT_IMM, 0, empty_match_fallback);
CMPTO(SLJIT_C_EQUAL, TMP2, 0, SLJIT_IMM, 0, empty_match_found);
CMPTO(SLJIT_C_NOT_EQUAL, TMP2, 0, STR_PTR, 0, empty_match_found);
JUMPTO(SLJIT_JUMP, empty_match_fallback);
common->currententry = common->entries;
while (common->currententry != NULL)
  /* Might add new entries. */
  compile_recurse(common);
  common->currententry = common->currententry->next;
/* Allocating stack, returns with PCRE_ERROR_JIT_STACKLIMIT if fails. */
/* This is a (really) rare case. */
set_jumps(common->stackalloc, LABEL());
/* RETURN_ADDR is not a saved register. */
OP1(SLJIT_MOV, TMP1, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(jit_arguments, stack));
OP1(SLJIT_MOV, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(struct sljit_stack, top), STACK_TOP, 0);
OP2(SLJIT_ADD, TMP2, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(struct sljit_stack, limit), SLJIT_IMM, STACK_GROWTH_RATE);
sljit_emit_ijump(compiler, SLJIT_CALL2, SLJIT_IMM, SLJIT_FUNC_OFFSET(sljit_stack_resize));
alloc_error = CMP(SLJIT_C_NOT_EQUAL, SLJIT_RETURN_REG, 0, SLJIT_IMM, 0);
OP1(SLJIT_MOV, STACK_TOP, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(struct sljit_stack, top));
OP1(SLJIT_MOV, STACK_LIMIT, 0, SLJIT_MEM1(TMP1), SLJIT_OFFSETOF(struct sljit_stack, limit));
OP1(SLJIT_MOV, TMP2, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), LOCALS1);
/* Allocation failed. */
JUMPHERE(alloc_error);
/* We break the return address cache here, but this is a really rare case. */
OP1(SLJIT_MOV, SLJIT_RETURN_REG, 0, SLJIT_IMM, PCRE_ERROR_JIT_STACKLIMIT);
/* Call limit reached. */
set_jumps(common->calllimit, LABEL());
OP1(SLJIT_MOV, SLJIT_RETURN_REG, 0, SLJIT_IMM, PCRE_ERROR_MATCHLIMIT);
if (common->revertframes != NULL)
  set_jumps(common->revertframes, LABEL());
  do_revertframes(common);
if (common->wordboundary != NULL)
  set_jumps(common->wordboundary, LABEL());
  check_wordboundary(common);
if (common->anynewline != NULL)
  set_jumps(common->anynewline, LABEL());
  check_anynewline(common);
if (common->hspace != NULL)
  set_jumps(common->hspace, LABEL());
  check_hspace(common);
if (common->vspace != NULL)
  set_jumps(common->vspace, LABEL());
  check_vspace(common);
if (common->casefulcmp != NULL)
  set_jumps(common->casefulcmp, LABEL());
  do_casefulcmp(common);
if (common->caselesscmp != NULL)
  set_jumps(common->caselesscmp, LABEL());
  do_caselesscmp(common);
if (common->utfreadchar != NULL)
  set_jumps(common->utfreadchar, LABEL());
  do_utfreadchar(common);
if (common->utfreadtype8 != NULL)
  set_jumps(common->utfreadtype8, LABEL());
  do_utfreadtype8(common);
if (common->getucd != NULL)
  set_jumps(common->getucd, LABEL());
  do_getucd(common);
executable_func = sljit_generate_code(compiler);
executable_size = sljit_get_generated_code_size(compiler);
if (executable_func == NULL)
function = SLJIT_MALLOC(sizeof(executable_function));
if (function == NULL)
  /* This case is highly unlikely since we just recently
  freed a lot of memory. Although not impossible. */
  sljit_free_code(executable_func);
function->executable_func = executable_func;
function->executable_size = executable_size;
function->callback = NULL;
function->userdata = NULL;
extra->executable_jit = function;
extra->flags |= PCRE_EXTRA_EXECUTABLE_JIT;
static int jit_machine_stack_exec(jit_arguments *arguments, executable_function *function)
   void* executable_func;
   jit_function call_executable_func;
} convert_executable_func;
pcre_uint8 local_area[LOCAL_SPACE_SIZE];
struct sljit_stack local_stack;
local_stack.top = (sljit_w)&local_area;
local_stack.base = local_stack.top;
local_stack.limit = local_stack.base + LOCAL_SPACE_SIZE;
local_stack.max_limit = local_stack.limit;
arguments->stack = &local_stack;
convert_executable_func.executable_func = function->executable_func;
return convert_executable_func.call_executable_func(arguments);
PRIV(jit_exec)(const REAL_PCRE *re, void *executable_func,
  const pcre_uchar *subject, int length, int start_offset, int options,
  int match_limit, int *offsets, int offsetcount)
executable_function *function = (executable_function*)executable_func;
jit_arguments arguments;
int maxoffsetcount;
int retval;
/* Sanity checks should be handled by pcre_exec. */
arguments.stack = NULL;
arguments.str = subject + start_offset;
arguments.begin = subject;
arguments.end = subject + length;
arguments.calllimit = match_limit; /* JIT decreases this value less times. */
arguments.notbol = (options & PCRE_NOTBOL) != 0;
arguments.noteol = (options & PCRE_NOTEOL) != 0;
arguments.notempty = (options & PCRE_NOTEMPTY) != 0;
arguments.notempty_atstart = (options & PCRE_NOTEMPTY_ATSTART) != 0;
arguments.offsets = offsets;
/* pcre_exec() rounds offsetcount to a multiple of 3, and then uses only 2/3 of
the output vector for storing captured strings, with the remainder used as
workspace. We don't need the workspace here. For compatibility, we limit the
number of captured strings in the same way as pcre_exec(), so that the user
gets the same result with and without JIT. */
if (offsetcount != 2)
  offsetcount = ((offsetcount - (offsetcount % 3)) * 2) / 3;
maxoffsetcount = (re->top_bracket + 1) * 2;
if (offsetcount > maxoffsetcount)
  offsetcount = maxoffsetcount;
arguments.offsetcount = offsetcount;
if (function->callback)
  arguments.stack = (struct sljit_stack*)function->callback(function->userdata);
  arguments.stack = (struct sljit_stack*)function->userdata;
if (arguments.stack == NULL)
  retval = jit_machine_stack_exec(&arguments, function);
  retval = convert_executable_func.call_executable_func(&arguments);
if (retval * 2 > offsetcount)
  retval = 0;
PRIV(jit_free)(void *executable_func)
sljit_free_code(function->executable_func);
SLJIT_FREE(function);
PRIV(jit_get_size)(void *executable_func)
return (int)((executable_function*)executable_func)->executable_size;
const char*
PRIV(jit_get_target)(void)
return sljit_get_platform_name();
PCRE_EXP_DECL pcre_jit_stack *
pcre_jit_stack_alloc(int startsize, int maxsize)
PCRE_EXP_DECL pcre16_jit_stack *
pcre16_jit_stack_alloc(int startsize, int maxsize)
if (startsize < 1 || maxsize < 1)
if (startsize > maxsize)
  startsize = maxsize;
startsize = (startsize + STACK_GROWTH_RATE - 1) & ~(STACK_GROWTH_RATE - 1);
maxsize = (maxsize + STACK_GROWTH_RATE - 1) & ~(STACK_GROWTH_RATE - 1);
return (PUBL(jit_stack)*)sljit_allocate_stack(startsize, maxsize);
PCRE_EXP_DECL void
pcre_jit_stack_free(pcre_jit_stack *stack)
pcre16_jit_stack_free(pcre16_jit_stack *stack)
sljit_free_stack((struct sljit_stack*)stack);
pcre_assign_jit_stack(pcre_extra *extra, pcre_jit_callback callback, void *userdata)
pcre16_assign_jit_stack(pcre16_extra *extra, pcre16_jit_callback callback, void *userdata)
if (extra != NULL &&
    (extra->flags & PCRE_EXTRA_EXECUTABLE_JIT) != 0 &&
    extra->executable_jit != NULL)
  function = (executable_function*)extra->executable_jit;
  function->callback = callback;
  function->userdata = userdata;
#else  /* SUPPORT_JIT */
/* These are dummy functions to avoid linking errors when JIT support is not
being compiled. */
(void)startsize;
(void)maxsize;
(void)stack;
(void)extra;
(void)callback;
(void)userdata;
/* End of pcre_jit_compile.c */
