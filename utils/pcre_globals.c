/* This module contains global variables that are exported by the PCRE library.
PCRE is thread-clean and doesn't use any global variables in the normal sense.
However, it calls memory allocation and freeing functions via the four
indirections below, and it can optionally do callouts, using the fifth
indirection. These values can be changed by the caller, but are shared between
all threads.
For MS Visual Studio and Symbian OS, there are problems in initializing these
variables to non-local functions. In these cases, therefore, an indirection via
a local function is used.
Also, when compiling for Virtual Pascal, things are done differently, and
global variables are not used. */
#if defined _MSC_VER || defined  __SYMBIAN32__
static void* LocalPcreMalloc(size_t aSize)
  return malloc(aSize);
static void LocalPcreFree(void* aPtr)
  free(aPtr);
PCRE_EXP_DATA_DEFN void *(*PUBL(malloc))(size_t) = LocalPcreMalloc;
PCRE_EXP_DATA_DEFN void  (*PUBL(free))(void *) = LocalPcreFree;
PCRE_EXP_DATA_DEFN void *(*PUBL(stack_malloc))(size_t) = LocalPcreMalloc;
PCRE_EXP_DATA_DEFN void  (*PUBL(stack_free))(void *) = LocalPcreFree;
PCRE_EXP_DATA_DEFN int   (*PUBL(callout))(PUBL(callout_block) *) = NULL;
#elif !defined VPCOMPAT
PCRE_EXP_DATA_DEFN void *(*PUBL(malloc))(size_t) = malloc;
PCRE_EXP_DATA_DEFN void  (*PUBL(free))(void *) = free;
PCRE_EXP_DATA_DEFN void *(*PUBL(stack_malloc))(size_t) = malloc;
PCRE_EXP_DATA_DEFN void  (*PUBL(stack_free))(void *) = free;
/* End of pcre_globals.c */
