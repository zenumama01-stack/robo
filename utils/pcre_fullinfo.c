/* This module contains the external function pcre_fullinfo(), which returns
information about a compiled pattern. */
*        Return info about compiled pattern      *
/* This is a newer "info" function which has an extensible interface so
that additional items can be added compatibly.
  argument_re      points to compiled code
  extra_data       points extra data, or NULL
pcre_fullinfo(const pcre *argument_re, const pcre_extra *extra_data,
  int what, void *where)
pcre16_fullinfo(const pcre16 *argument_re, const pcre16_extra *extra_data,
if (re == NULL || where == NULL) return PCRE_ERROR_NULL;
if (extra_data != NULL && (extra_data->flags & PCRE_EXTRA_STUDY_DATA) != 0)
/* Check that this pattern was compiled in the correct bit mode */
  case PCRE_INFO_OPTIONS:
  *((unsigned long int *)where) = re->options & PUBLIC_COMPILE_OPTIONS;
  case PCRE_INFO_SIZE:
  *((size_t *)where) = re->size;
  case PCRE_INFO_STUDYSIZE:
  *((size_t *)where) = (study == NULL)? 0 : study->size;
  case PCRE_INFO_JITSIZE:
  *((size_t *)where) =
      (extra_data != NULL &&
      (extra_data->flags & PCRE_EXTRA_EXECUTABLE_JIT) != 0 &&
      extra_data->executable_jit != NULL)?
    PRIV(jit_get_size)(extra_data->executable_jit) : 0;
  *((size_t *)where) = 0;
  case PCRE_INFO_CAPTURECOUNT:
  *((int *)where) = re->top_bracket;
  case PCRE_INFO_BACKREFMAX:
  *((int *)where) = re->top_backref;
  case PCRE_INFO_FIRSTBYTE:
  *((int *)where) =
    ((re->flags & PCRE_FIRSTSET) != 0)? re->first_char :
    ((re->flags & PCRE_STARTLINE) != 0)? -1 : -2;
  /* Make sure we pass back the pointer to the bit vector in the external
  block, not the internal copy (with flipped integer fields). */
  case PCRE_INFO_FIRSTTABLE:
  *((const pcre_uint8 **)where) =
    (study != NULL && (study->flags & PCRE_STUDY_MAPPED) != 0)?
      ((const pcre_study_data *)extra_data->study_data)->start_bits : NULL;
  case PCRE_INFO_MINLENGTH:
    (study != NULL && (study->flags & PCRE_STUDY_MINLEN) != 0)?
      (int)(study->minlength) : -1;
  case PCRE_INFO_JIT:
  *((int *)where) = extra_data != NULL &&
                    extra_data->executable_jit != NULL;
  case PCRE_INFO_LASTLITERAL:
    ((re->flags & PCRE_REQCHSET) != 0)? re->req_char : -1;
  case PCRE_INFO_NAMEENTRYSIZE:
  *((int *)where) = re->name_entry_size;
  case PCRE_INFO_NAMECOUNT:
  *((int *)where) = re->name_count;
  case PCRE_INFO_NAMETABLE:
  *((const pcre_uchar **)where) = (const pcre_uchar *)re + re->name_table_offset;
  case PCRE_INFO_DEFAULT_TABLES:
  *((const pcre_uint8 **)where) = (const pcre_uint8 *)(PRIV(default_tables));
  /* From release 8.00 this will always return TRUE because NOPARTIAL is
  no longer ever set (the restrictions have been removed). */
  case PCRE_INFO_OKPARTIAL:
  *((int *)where) = (re->flags & PCRE_NOPARTIAL) == 0;
  case PCRE_INFO_JCHANGED:
  *((int *)where) = (re->flags & PCRE_JCHANGED) != 0;
  case PCRE_INFO_HASCRORLF:
  *((int *)where) = (re->flags & PCRE_HASCRORLF) != 0;
/* End of pcre_fullinfo.c */
