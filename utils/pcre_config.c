/* This module contains the external function pcre_config(). */
/* Keep the original link size. */
static int real_link_size = LINK_SIZE;
* Return info about what features are configured *
/* This function has an extensible interface so that additional items can be
added compatibly.
  what             what information is required
  where            where to put the information
Returns:           0 if data returned, negative on error
PCRE_EXP_DEFN int PCRE_CALL_CONVENTION
pcre_config(int what, void *where)
pcre16_config(int what, void *where)
switch (what)
  case PCRE_CONFIG_UTF8:
#if defined COMPILE_PCRE16
  *((int *)where) = 0;
  return PCRE_ERROR_BADOPTION;
#if defined SUPPORT_UTF
  *((int *)where) = 1;
  case PCRE_CONFIG_UTF16:
#if defined COMPILE_PCRE8
  case PCRE_CONFIG_UNICODE_PROPERTIES:
  case PCRE_CONFIG_JIT:
#ifdef SUPPORT_JIT
  case PCRE_CONFIG_JITTARGET:
  *((const char **)where) = PRIV(jit_get_target)();
  *((const char **)where) = NULL;
  case PCRE_CONFIG_NEWLINE:
  *((int *)where) = NEWLINE;
  case PCRE_CONFIG_BSR:
#ifdef BSR_ANYCRLF
  case PCRE_CONFIG_LINK_SIZE:
  *((int *)where) = real_link_size;
  case PCRE_CONFIG_POSIX_MALLOC_THRESHOLD:
  *((int *)where) = POSIX_MALLOC_THRESHOLD;
  case PCRE_CONFIG_MATCH_LIMIT:
  *((unsigned long int *)where) = MATCH_LIMIT;
  case PCRE_CONFIG_MATCH_LIMIT_RECURSION:
  *((unsigned long int *)where) = MATCH_LIMIT_RECURSION;
  case PCRE_CONFIG_STACKRECURSE:
#ifdef NO_RECURSE
  default: return PCRE_ERROR_BADOPTION;
/* End of pcre_config.c */
