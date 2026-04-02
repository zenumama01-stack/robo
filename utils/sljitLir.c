#include "sljitLir.h"
#define CHECK_ERROR() \
	do { \
		if (SLJIT_UNLIKELY(compiler->error)) \
			return compiler->error; \
	} while (0)
#define CHECK_ERROR_PTR() \
			return NULL; \
#define CHECK_ERROR_VOID() \
#define FAIL_IF(expr) \
		if (SLJIT_UNLIKELY(expr)) \
#define PTR_FAIL_IF(expr) \
#define FAIL_IF_NULL(ptr) \
		if (SLJIT_UNLIKELY(!(ptr))) { \
			compiler->error = SLJIT_ERR_ALLOC_FAILED; \
			return SLJIT_ERR_ALLOC_FAILED; \
#define PTR_FAIL_IF_NULL(ptr) \
#define PTR_FAIL_WITH_EXEC_IF(ptr) \
			compiler->error = SLJIT_ERR_EX_ALLOC_FAILED; \
#if !(defined SLJIT_CONFIG_UNSUPPORTED && SLJIT_CONFIG_UNSUPPORTED)
#define GET_OPCODE(op) \
	((op) & ~(SLJIT_INT_OP | SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C | SLJIT_KEEP_FLAGS))
#define GET_FLAGS(op) \
	((op) & (SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C))
#define GET_ALL_FLAGS(op) \
	((op) & (SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C | SLJIT_KEEP_FLAGS))
#define BUF_SIZE	4096
#if (defined SLJIT_32BIT_ARCHITECTURE && SLJIT_32BIT_ARCHITECTURE)
#define ABUF_SIZE	2048
#define ABUF_SIZE	4096
/* Jump flags. */
#define JUMP_LABEL	0x1
#define JUMP_ADDR	0x2
/* SLJIT_REWRITABLE_JUMP is 0x1000. */
#if (defined SLJIT_CONFIG_X86_32 && SLJIT_CONFIG_X86_32) || (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64)
	#define PATCH_MB	0x4
	#define PATCH_MW	0x8
#if (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64)
	#define PATCH_MD	0x10
#if (defined SLJIT_CONFIG_ARM_V5 && SLJIT_CONFIG_ARM_V5) || (defined SLJIT_CONFIG_ARM_V7 && SLJIT_CONFIG_ARM_V7)
	#define IS_BL		0x4
	#define PATCH_B		0x8
#if (defined SLJIT_CONFIG_ARM_V5 && SLJIT_CONFIG_ARM_V5)
	#define CPOOL_SIZE	512
#if (defined SLJIT_CONFIG_ARM_THUMB2 && SLJIT_CONFIG_ARM_THUMB2)
	#define IS_CONDITIONAL	0x04
	#define IS_BL		0x08
	/* cannot be encoded as branch */
	#define B_TYPE0		0x00
	/* conditional + imm8 */
	#define B_TYPE1		0x10
	/* conditional + imm20 */
	#define B_TYPE2		0x20
	/* IT + imm24 */
	#define B_TYPE3		0x30
	/* imm11 */
	#define B_TYPE4		0x40
	/* imm24 */
	#define B_TYPE5		0x50
	/* BL + imm24 */
	#define BL_TYPE6	0x60
	/* 0xf00 cc code for branches */
#if (defined SLJIT_CONFIG_PPC_32 && SLJIT_CONFIG_PPC_32) || (defined SLJIT_CONFIG_PPC_64 && SLJIT_CONFIG_PPC_64)
	#define UNCOND_B	0x04
	#define PATCH_B		0x08
	#define ABSOLUTE_B	0x10
#if (defined SLJIT_CONFIG_MIPS_32 && SLJIT_CONFIG_MIPS_32)
	#define IS_MOVABLE	0x04
	#define IS_JAL		0x08
	#define IS_BIT26_COND	0x10
	#define IS_BIT16_COND	0x20
	#define IS_COND		(IS_BIT26_COND | IS_BIT16_COND)
	#define PATCH_B		0x40
	#define PATCH_J		0x80
	/* instruction types */
	#define UNMOVABLE_INS	0
	/* 1 - 31 last destination register */
	#define FCSR_FCC	32
	/* no destination (i.e: store) */
	#define MOVABLE_INS	33
#endif /* !(defined SLJIT_CONFIG_UNSUPPORTED && SLJIT_CONFIG_UNSUPPORTED) */
/* Utils can still be used even if SLJIT_CONFIG_UNSUPPORTED is set. */
#include "sljitUtils.c"
#if (defined SLJIT_EXECUTABLE_ALLOCATOR && SLJIT_EXECUTABLE_ALLOCATOR)
#include "sljitExecAllocator.c"
#if (defined SLJIT_SSE2_AUTO && SLJIT_SSE2_AUTO) && !(defined SLJIT_SSE2 && SLJIT_SSE2)
#error SLJIT_SSE2_AUTO cannot be enabled without SLJIT_SSE2
/*  Public functions                                                     */
#if (defined SLJIT_CONFIG_ARM_V5 && SLJIT_CONFIG_ARM_V5) || ((defined SLJIT_SSE2 && SLJIT_SSE2) && ((defined SLJIT_CONFIG_X86_32 && SLJIT_CONFIG_X86_32) || (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64)))
#define SLJIT_NEEDS_COMPILER_INIT 1
static int compiler_initialized = 0;
/* A thread safe initialization. */
static void init_compiler(void);
SLJIT_API_FUNC_ATTRIBUTE struct sljit_compiler* sljit_create_compiler(void)
	struct sljit_compiler *compiler = (struct sljit_compiler*)SLJIT_MALLOC(sizeof(struct sljit_compiler));
	SLJIT_ZEROMEM(compiler, sizeof(struct sljit_compiler));
	SLJIT_COMPILE_ASSERT(
		sizeof(sljit_b) == 1 && sizeof(sljit_ub) == 1
		&& sizeof(sljit_h) == 2 && sizeof(sljit_uh) == 2
		&& sizeof(sljit_i) == 4 && sizeof(sljit_ui) == 4
		&& ((sizeof(sljit_w) == 4 && sizeof(sljit_uw) == 4) || (sizeof(sljit_w) == 8 && sizeof(sljit_uw) == 8)),
		invalid_integer_types);
	/* Only the non-zero members must be set. */
	compiler->error = SLJIT_SUCCESS;
	compiler->buf = (struct sljit_memory_fragment*)SLJIT_MALLOC(BUF_SIZE);
	compiler->abuf = (struct sljit_memory_fragment*)SLJIT_MALLOC(ABUF_SIZE);
	if (!compiler->buf || !compiler->abuf) {
		if (compiler->buf)
			SLJIT_FREE(compiler->buf);
		if (compiler->abuf)
			SLJIT_FREE(compiler->abuf);
		SLJIT_FREE(compiler);
	compiler->buf->next = NULL;
	compiler->buf->used_size = 0;
	compiler->abuf->next = NULL;
	compiler->abuf->used_size = 0;
	compiler->temporaries = -1;
	compiler->saveds = -1;
#if (defined SLJIT_CONFIG_X86_32 && SLJIT_CONFIG_X86_32)
	compiler->args = -1;
	compiler->cpool = (sljit_uw*)SLJIT_MALLOC(CPOOL_SIZE * sizeof(sljit_uw) + CPOOL_SIZE * sizeof(sljit_ub));
	if (!compiler->cpool) {
	compiler->cpool_unique = (sljit_ub*)(compiler->cpool + CPOOL_SIZE);
	compiler->cpool_diff = 0xffffffff;
	compiler->delay_slot = UNMOVABLE_INS;
#if (defined SLJIT_NEEDS_COMPILER_INIT && SLJIT_NEEDS_COMPILER_INIT)
	if (!compiler_initialized) {
		init_compiler();
		compiler_initialized = 1;
	return compiler;
SLJIT_API_FUNC_ATTRIBUTE void sljit_free_compiler(struct sljit_compiler *compiler)
	struct sljit_memory_fragment *buf;
	struct sljit_memory_fragment *curr;
	buf = compiler->buf;
	while (buf) {
		curr = buf;
		buf = buf->next;
		SLJIT_FREE(curr);
	buf = compiler->abuf;
	SLJIT_FREE(compiler->cpool);
SLJIT_API_FUNC_ATTRIBUTE void sljit_free_code(void* code)
	/* Remove thumb mode flag. */
	SLJIT_FREE_EXEC((void*)((sljit_uw)code & ~0x1));
#elif (defined SLJIT_CONFIG_PPC_64 && SLJIT_CONFIG_PPC_64)
	/* Resolve indirection. */
	code = (void*)(*(sljit_uw*)code);
	SLJIT_FREE_EXEC(code);
SLJIT_API_FUNC_ATTRIBUTE void sljit_set_label(struct sljit_jump *jump, struct sljit_label* label)
	if (SLJIT_LIKELY(!!jump) && SLJIT_LIKELY(!!label)) {
		jump->flags &= ~JUMP_ADDR;
		jump->flags |= JUMP_LABEL;
		jump->u.label = label;
SLJIT_API_FUNC_ATTRIBUTE void sljit_set_target(struct sljit_jump *jump, sljit_uw target)
	if (SLJIT_LIKELY(!!jump)) {
		SLJIT_ASSERT(jump->flags & SLJIT_REWRITABLE_JUMP);
		jump->flags &= ~JUMP_LABEL;
		jump->flags |= JUMP_ADDR;
		jump->u.target = target;
/*  Private functions                                                    */
static void* ensure_buf(struct sljit_compiler *compiler, int size)
	sljit_ub *ret;
	struct sljit_memory_fragment *new_frag;
	if (compiler->buf->used_size + size <= (int)(BUF_SIZE - sizeof(sljit_uw) - sizeof(void*))) {
		ret = compiler->buf->memory + compiler->buf->used_size;
		compiler->buf->used_size += size;
		return ret;
	new_frag = (struct sljit_memory_fragment*)SLJIT_MALLOC(BUF_SIZE);
	PTR_FAIL_IF_NULL(new_frag);
	new_frag->next = compiler->buf;
	compiler->buf = new_frag;
	new_frag->used_size = size;
	return new_frag->memory;
static void* ensure_abuf(struct sljit_compiler *compiler, int size)
	if (compiler->abuf->used_size + size <= (int)(ABUF_SIZE - sizeof(sljit_uw) - sizeof(void*))) {
		ret = compiler->abuf->memory + compiler->abuf->used_size;
		compiler->abuf->used_size += size;
	new_frag = (struct sljit_memory_fragment*)SLJIT_MALLOC(ABUF_SIZE);
	new_frag->next = compiler->abuf;
	compiler->abuf = new_frag;
SLJIT_API_FUNC_ATTRIBUTE void* sljit_alloc_memory(struct sljit_compiler *compiler, int size)
	CHECK_ERROR_PTR();
#if (defined SLJIT_64BIT_ARCHITECTURE && SLJIT_64BIT_ARCHITECTURE)
	if (size <= 0 || size > 128)
	size = (size + 7) & ~7;
	if (size <= 0 || size > 64)
	size = (size + 3) & ~3;
	return ensure_abuf(compiler, size);
static SLJIT_INLINE void reverse_buf(struct sljit_compiler *compiler)
	struct sljit_memory_fragment *buf = compiler->buf;
	struct sljit_memory_fragment *prev = NULL;
	struct sljit_memory_fragment *tmp;
		tmp = buf->next;
		buf->next = prev;
		prev = buf;
		buf = tmp;
	} while (buf != NULL);
	compiler->buf = prev;
static SLJIT_INLINE void set_label(struct sljit_label *label, struct sljit_compiler *compiler)
	label->next = NULL;
	label->size = compiler->size;
	if (compiler->last_label)
		compiler->last_label->next = label;
		compiler->labels = label;
	compiler->last_label = label;
static SLJIT_INLINE void set_jump(struct sljit_jump *jump, struct sljit_compiler *compiler, int flags)
	jump->next = NULL;
	jump->flags = flags;
	if (compiler->last_jump)
		compiler->last_jump->next = jump;
		compiler->jumps = jump;
	compiler->last_jump = jump;
static SLJIT_INLINE void set_const(struct sljit_const *const_, struct sljit_compiler *compiler)
	const_->next = NULL;
	const_->addr = compiler->size;
	if (compiler->last_const)
		compiler->last_const->next = const_;
		compiler->consts = const_;
	compiler->last_const = const_;
#define ADDRESSING_DEPENDS_ON(exp, reg) \
	(((exp) & SLJIT_MEM) && (((exp) & 0xf) == reg || (((exp) >> 4) & 0xf) == reg))
#if (defined SLJIT_DEBUG && SLJIT_DEBUG)
#define FUNCTION_CHECK_OP() \
	SLJIT_ASSERT(!GET_FLAGS(op) || !(op & SLJIT_KEEP_FLAGS)); \
	switch (GET_OPCODE(op)) { \
	case SLJIT_NOT: \
	case SLJIT_CLZ: \
	case SLJIT_AND: \
	case SLJIT_OR: \
	case SLJIT_XOR: \
	case SLJIT_SHL: \
	case SLJIT_LSHR: \
	case SLJIT_ASHR: \
		SLJIT_ASSERT(!(op & (SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C))); \
	case SLJIT_NEG: \
		SLJIT_ASSERT(!(op & (SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_C))); \
	case SLJIT_MUL: \
		SLJIT_ASSERT(!(op & (SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_C))); \
	case SLJIT_FCMP: \
		SLJIT_ASSERT(!(op & (SLJIT_INT_OP | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C | SLJIT_KEEP_FLAGS))); \
		SLJIT_ASSERT((op & (SLJIT_SET_E | SLJIT_SET_S))); \
	case SLJIT_ADD: \
		SLJIT_ASSERT(!(op & (SLJIT_SET_S | SLJIT_SET_U))); \
	case SLJIT_SUB: \
	case SLJIT_ADDC: \
	case SLJIT_SUBC: \
		SLJIT_ASSERT(!(op & (SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O))); \
	default: \
		/* Nothing allowed */ \
		SLJIT_ASSERT(!(op & (SLJIT_INT_OP | SLJIT_SET_E | SLJIT_SET_S | SLJIT_SET_U | SLJIT_SET_O | SLJIT_SET_C | SLJIT_KEEP_FLAGS))); \
#define FUNCTION_CHECK_IS_REG(r) \
	((r) == SLJIT_UNUSED || (r) == SLJIT_LOCALS_REG || \
	((r) >= SLJIT_TEMPORARY_REG1 && (r) <= SLJIT_TEMPORARY_REG3 && (r) <= SLJIT_TEMPORARY_REG1 - 1 + compiler->temporaries) || \
	((r) >= SLJIT_SAVED_REG1 && (r) <= SLJIT_SAVED_REG3 && (r) <= SLJIT_SAVED_REG1 - 1 + compiler->saveds)) \
#define FUNCTION_CHECK_SRC(p, i) \
	SLJIT_ASSERT(compiler->temporaries != -1 && compiler->saveds != -1); \
	if (((p) >= SLJIT_TEMPORARY_REG1 && (p) <= SLJIT_TEMPORARY_REG1 - 1 + compiler->temporaries) || \
			((p) >= SLJIT_SAVED_REG1 && (p) <= SLJIT_SAVED_REG1 - 1 + compiler->saveds) || \
			(p) == SLJIT_LOCALS_REG) \
		SLJIT_ASSERT(i == 0); \
	else if ((p) == SLJIT_IMM) \
		; \
	else if ((p) & SLJIT_MEM) { \
		SLJIT_ASSERT(FUNCTION_CHECK_IS_REG((p) & 0xf)); \
		if ((p) & 0xf0) { \
			SLJIT_ASSERT(FUNCTION_CHECK_IS_REG(((p) >> 4) & 0xf)); \
			SLJIT_ASSERT(((p) & 0xf0) != (SLJIT_LOCALS_REG << 4) && !(i & ~0x3)); \
		} else \
			SLJIT_ASSERT((((p) >> 4) & 0xf) == 0); \
		SLJIT_ASSERT(((p) >> 9) == 0); \
#define FUNCTION_CHECK_DST(p, i) \
			(p) == SLJIT_UNUSED) \
#define FUNCTION_FCHECK(p, i) \
	if ((p) >= SLJIT_FLOAT_REG1 && (p) <= SLJIT_FLOAT_REG4) \
#define FUNCTION_CHECK_OP1() \
	if (GET_OPCODE(op) >= SLJIT_MOV && GET_OPCODE(op) <= SLJIT_MOVU_SI) { \
		SLJIT_ASSERT(!GET_ALL_FLAGS(op)); \
        if (GET_OPCODE(op) >= SLJIT_MOVU && GET_OPCODE(op) <= SLJIT_MOVU_SI) { \
		SLJIT_ASSERT(!(src & SLJIT_MEM) || (src & 0xf) != SLJIT_LOCALS_REG); \
		SLJIT_ASSERT(!(dst & SLJIT_MEM) || (dst & 0xf) != SLJIT_LOCALS_REG); \
		if ((src & SLJIT_MEM) && (src & 0xf)) \
			SLJIT_ASSERT((dst & 0xf) != (src & 0xf) && ((dst >> 4) & 0xf) != (src & 0xf)); \
#if (defined SLJIT_VERBOSE && SLJIT_VERBOSE)
SLJIT_API_FUNC_ATTRIBUTE void sljit_compiler_verbose(struct sljit_compiler *compiler, FILE* verbose)
	compiler->verbose = verbose;
static char* reg_names[] = {
	(char*)"<noreg>", (char*)"t1", (char*)"t2", (char*)"t3",
	(char*)"te1", (char*)"te2", (char*)"s1", (char*)"s2",
	(char*)"s3", (char*)"se1", (char*)"se2", (char*)"lcr"
static char* freg_names[] = {
	(char*)"<noreg>", (char*)"float_r1", (char*)"float_r2", (char*)"float_r3", (char*)"float_r4"
#if (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64) || (defined SLJIT_CONFIG_PPC_64 && SLJIT_CONFIG_PPC_64)
	#define SLJIT_PRINT_D	"I64"
	#define SLJIT_PRINT_D	"l"
	#define SLJIT_PRINT_D	""
#define sljit_verbose_param(p, i) \
	if ((p) & SLJIT_IMM) \
		fprintf(compiler->verbose, "#%"SLJIT_PRINT_D"d", (i)); \
		if ((p) & 0xf) { \
			if (i) { \
				if (((p) >> 4) & 0xf) \
					fprintf(compiler->verbose, "[%s + %s * %d]", reg_names[(p) & 0xF], reg_names[((p) >> 4)& 0xF], 1 << (i)); \
					fprintf(compiler->verbose, "[%s + #%"SLJIT_PRINT_D"d]", reg_names[(p) & 0xF], (i)); \
			else { \
					fprintf(compiler->verbose, "[%s + %s]", reg_names[(p) & 0xF], reg_names[((p) >> 4)& 0xF]); \
					fprintf(compiler->verbose, "[%s]", reg_names[(p) & 0xF]); \
			fprintf(compiler->verbose, "[#%"SLJIT_PRINT_D"d]", (i)); \
		fprintf(compiler->verbose, "%s", reg_names[p]);
#define sljit_verbose_fparam(p, i) \
	if ((p) & SLJIT_MEM) { \
				if (((p) >> 4) & 0xF) \
		fprintf(compiler->verbose, "%s", freg_names[p]);
static SLJIT_CONST char* op_names[] = {
	/* op0 */
	(char*)"breakpoint", (char*)"nop",
	(char*)"umul", (char*)"smul", (char*)"udiv", (char*)"sdiv",
	/* op1 */
	(char*)"mov", (char*)"mov.ub", (char*)"mov.sb", (char*)"mov.uh",
	(char*)"mov.sh", (char*)"mov.ui", (char*)"mov.si", (char*)"movu",
	(char*)"movu.ub", (char*)"movu.sb", (char*)"movu.uh", (char*)"movu.sh",
	(char*)"movu.ui", (char*)"movu.si", (char*)"not", (char*)"neg",
	(char*)"clz",
	/* op2 */
	(char*)"add", (char*)"addc", (char*)"sub", (char*)"subc",
	(char*)"mul", (char*)"and", (char*)"or", (char*)"xor",
	(char*)"shl", (char*)"lshr", (char*)"ashr",
	/* fop1 */
	(char*)"fcmp", (char*)"fmov", (char*)"fneg", (char*)"fabs",
	/* fop2 */
	(char*)"fadd", (char*)"fsub", (char*)"fmul", (char*)"fdiv"
static char* jump_names[] = {
	(char*)"c_equal", (char*)"c_not_equal",
	(char*)"c_less", (char*)"c_greater_equal",
	(char*)"c_greater", (char*)"c_less_equal",
	(char*)"c_sig_less", (char*)"c_sig_greater_equal",
	(char*)"c_sig_greater", (char*)"c_sig_less_equal",
	(char*)"c_overflow", (char*)"c_not_overflow",
	(char*)"c_mul_overflow", (char*)"c_mul_not_overflow",
	(char*)"c_float_equal", (char*)"c_float_not_equal",
	(char*)"c_float_less", (char*)"c_float_greater_equal",
	(char*)"c_float_greater", (char*)"c_float_less_equal",
	(char*)"c_float_nan", (char*)"c_float_not_nan",
	(char*)"jump", (char*)"fast_call",
	(char*)"call0", (char*)"call1", (char*)"call2", (char*)"call3"
/*  Arch dependent                                                       */
static SLJIT_INLINE void check_sljit_generate_code(struct sljit_compiler *compiler)
	/* If debug and verbose are disabled, all arguments are unused. */
	SLJIT_UNUSED_ARG(compiler);
	SLJIT_ASSERT(compiler->size > 0);
	jump = compiler->jumps;
	while (jump) {
		/* All jumps have target. */
		SLJIT_ASSERT(jump->flags & (JUMP_LABEL | JUMP_ADDR));
		jump = jump->next;
static SLJIT_INLINE void check_sljit_emit_enter(struct sljit_compiler *compiler, int args, int temporaries, int saveds, int local_size)
	SLJIT_UNUSED_ARG(args);
	SLJIT_UNUSED_ARG(temporaries);
	SLJIT_UNUSED_ARG(saveds);
	SLJIT_UNUSED_ARG(local_size);
	SLJIT_ASSERT(args >= 0 && args <= 3);
	SLJIT_ASSERT(temporaries >= 0 && temporaries <= SLJIT_NO_TMP_REGISTERS);
	SLJIT_ASSERT(saveds >= 0 && saveds <= SLJIT_NO_GEN_REGISTERS);
	SLJIT_ASSERT(args <= saveds);
	SLJIT_ASSERT(local_size >= 0 && local_size <= SLJIT_MAX_LOCAL_SIZE);
	if (SLJIT_UNLIKELY(!!compiler->verbose))
		fprintf(compiler->verbose, "  enter args=%d temporaries=%d saveds=%d local_size=%d\n", args, temporaries, saveds, local_size);
static SLJIT_INLINE void check_sljit_set_context(struct sljit_compiler *compiler, int args, int temporaries, int saveds, int local_size)
		fprintf(compiler->verbose, "  fake_enter args=%d temporaries=%d saveds=%d local_size=%d\n", args, temporaries, saveds, local_size);
static SLJIT_INLINE void check_sljit_emit_return(struct sljit_compiler *compiler, int op, int src, sljit_w srcw)
	SLJIT_UNUSED_ARG(op);
	SLJIT_UNUSED_ARG(src);
	SLJIT_UNUSED_ARG(srcw);
	if (op != SLJIT_UNUSED) {
		SLJIT_ASSERT(op >= SLJIT_MOV && op <= SLJIT_MOV_SI);
		FUNCTION_CHECK_SRC(src, srcw);
		SLJIT_ASSERT(src == 0 && srcw == 0);
	if (SLJIT_UNLIKELY(!!compiler->verbose)) {
		if (op == SLJIT_UNUSED)
			fprintf(compiler->verbose, "  return\n");
			fprintf(compiler->verbose, "  return %s ", op_names[op]);
			sljit_verbose_param(src, srcw);
			fprintf(compiler->verbose, "\n");
static SLJIT_INLINE void check_sljit_emit_fast_enter(struct sljit_compiler *compiler, int dst, sljit_w dstw, int args, int temporaries, int saveds, int local_size)
	SLJIT_UNUSED_ARG(dst);
	SLJIT_UNUSED_ARG(dstw);
	compiler->temporaries = temporaries;
	compiler->saveds = saveds;
	FUNCTION_CHECK_DST(dst, dstw);
		fprintf(compiler->verbose, "  fast_enter ");
		sljit_verbose_param(dst, dstw);
		fprintf(compiler->verbose, " args=%d temporaries=%d saveds=%d local_size=%d\n", args, temporaries, saveds, local_size);
static SLJIT_INLINE void check_sljit_emit_fast_return(struct sljit_compiler *compiler, int src, sljit_w srcw)
		fprintf(compiler->verbose, "  fast_return ");
static SLJIT_INLINE void check_sljit_emit_op0(struct sljit_compiler *compiler, int op)
	SLJIT_ASSERT((op >= SLJIT_BREAKPOINT && op <= SLJIT_SMUL)
		|| ((op & ~SLJIT_INT_OP) >= SLJIT_UDIV && (op & ~SLJIT_INT_OP) <= SLJIT_SDIV));
		fprintf(compiler->verbose, "  %s%s\n", !(op & SLJIT_INT_OP) ? "" : "i", op_names[GET_OPCODE(op)]);
static SLJIT_INLINE void check_sljit_emit_op1(struct sljit_compiler *compiler, int op,
	int dst, sljit_w dstw,
	int src, sljit_w srcw)
#if (defined SLJIT_VERBOSE && SLJIT_VERBOSE) || (defined SLJIT_DEBUG && SLJIT_DEBUG)
	if (SLJIT_UNLIKELY(compiler->skip_checks)) {
		compiler->skip_checks = 0;
	SLJIT_ASSERT(GET_OPCODE(op) >= SLJIT_MOV && GET_OPCODE(op) <= SLJIT_CLZ);
	FUNCTION_CHECK_OP();
	FUNCTION_CHECK_OP1();
		fprintf(compiler->verbose, "  %s%s%s%s%s%s%s%s ", !(op & SLJIT_INT_OP) ? "" : "i", op_names[GET_OPCODE(op)],
			!(op & SLJIT_SET_E) ? "" : "E", !(op & SLJIT_SET_S) ? "" : "S", !(op & SLJIT_SET_U) ? "" : "U", !(op & SLJIT_SET_O) ? "" : "O", !(op & SLJIT_SET_C) ? "" : "C", !(op & SLJIT_KEEP_FLAGS) ? "" : "K");
		fprintf(compiler->verbose, ", ");
static SLJIT_INLINE void check_sljit_emit_op2(struct sljit_compiler *compiler, int op,
	int src1, sljit_w src1w,
	int src2, sljit_w src2w)
	SLJIT_UNUSED_ARG(src1);
	SLJIT_UNUSED_ARG(src1w);
	SLJIT_UNUSED_ARG(src2);
	SLJIT_UNUSED_ARG(src2w);
	SLJIT_ASSERT(GET_OPCODE(op) >= SLJIT_ADD && GET_OPCODE(op) <= SLJIT_ASHR);
	FUNCTION_CHECK_SRC(src1, src1w);
	FUNCTION_CHECK_SRC(src2, src2w);
		sljit_verbose_param(src1, src1w);
		sljit_verbose_param(src2, src2w);
static SLJIT_INLINE void check_sljit_get_register_index(int reg)
	SLJIT_UNUSED_ARG(reg);
	SLJIT_ASSERT(reg > 0 && reg <= SLJIT_NO_REGISTERS);
static SLJIT_INLINE void check_sljit_emit_op_custom(struct sljit_compiler *compiler,
	void *instruction, int size)
	SLJIT_UNUSED_ARG(instruction);
	SLJIT_ASSERT(instruction);
static SLJIT_INLINE void check_sljit_emit_fop1(struct sljit_compiler *compiler, int op,
	SLJIT_ASSERT(sljit_is_fpu_available());
	SLJIT_ASSERT(GET_OPCODE(op) >= SLJIT_FCMP && GET_OPCODE(op) <= SLJIT_FABS);
	FUNCTION_FCHECK(src, srcw);
	FUNCTION_FCHECK(dst, dstw);
		fprintf(compiler->verbose, "  %s%s%s ", op_names[GET_OPCODE(op)],
			!(op & SLJIT_SET_E) ? "" : "E", !(op & SLJIT_SET_S) ? "" : "S");
		sljit_verbose_fparam(dst, dstw);
		sljit_verbose_fparam(src, srcw);
static SLJIT_INLINE void check_sljit_emit_fop2(struct sljit_compiler *compiler, int op,
	SLJIT_ASSERT(GET_OPCODE(op) >= SLJIT_FADD && GET_OPCODE(op) <= SLJIT_FDIV);
	FUNCTION_FCHECK(src1, src1w);
	FUNCTION_FCHECK(src2, src2w);
		fprintf(compiler->verbose, "  %s ", op_names[GET_OPCODE(op)]);
		sljit_verbose_fparam(src1, src1w);
		sljit_verbose_fparam(src2, src2w);
static SLJIT_INLINE void check_sljit_emit_label(struct sljit_compiler *compiler)
		fprintf(compiler->verbose, "label:\n");
static SLJIT_INLINE void check_sljit_emit_jump(struct sljit_compiler *compiler, int type)
	SLJIT_UNUSED_ARG(type);
	SLJIT_ASSERT(!(type & ~(0xff | SLJIT_REWRITABLE_JUMP)));
	SLJIT_ASSERT((type & 0xff) >= SLJIT_C_EQUAL && (type & 0xff) <= SLJIT_CALL3);
		fprintf(compiler->verbose, "  jump%s <%s>\n", !(type & SLJIT_REWRITABLE_JUMP) ? "" : "R", jump_names[type & 0xff]);
static SLJIT_INLINE void check_sljit_emit_cmp(struct sljit_compiler *compiler, int type,
	SLJIT_ASSERT(!(type & ~(0xff | SLJIT_INT_OP | SLJIT_REWRITABLE_JUMP)));
	SLJIT_ASSERT((type & 0xff) >= SLJIT_C_EQUAL && (type & 0xff) <= SLJIT_C_SIG_LESS_EQUAL);
		fprintf(compiler->verbose, "  %scmp%s <%s> ", !(type & SLJIT_INT_OP) ? "" : "i", !(type & SLJIT_REWRITABLE_JUMP) ? "" : "R", jump_names[type & 0xff]);
static SLJIT_INLINE void check_sljit_emit_fcmp(struct sljit_compiler *compiler, int type,
	SLJIT_ASSERT((type & 0xff) >= SLJIT_C_FLOAT_EQUAL && (type & 0xff) <= SLJIT_C_FLOAT_NOT_NAN);
		fprintf(compiler->verbose, "  fcmp%s <%s> ", !(type & SLJIT_REWRITABLE_JUMP) ? "" : "R", jump_names[type & 0xff]);
static SLJIT_INLINE void check_sljit_emit_ijump(struct sljit_compiler *compiler, int type, int src, sljit_w srcw)
	SLJIT_ASSERT(type >= SLJIT_JUMP && type <= SLJIT_CALL3);
		fprintf(compiler->verbose, "  ijump <%s> ", jump_names[type]);
static SLJIT_INLINE void check_sljit_emit_cond_value(struct sljit_compiler *compiler, int op, int dst, sljit_w dstw, int type)
	SLJIT_ASSERT(type >= SLJIT_C_EQUAL && type < SLJIT_JUMP);
	SLJIT_ASSERT(op == SLJIT_MOV || GET_OPCODE(op) == SLJIT_OR);
	SLJIT_ASSERT(GET_ALL_FLAGS(op) == 0 || GET_ALL_FLAGS(op) == SLJIT_SET_E || GET_ALL_FLAGS(op) == SLJIT_KEEP_FLAGS);
		fprintf(compiler->verbose, "  cond_set%s%s <%s> ", !(op & SLJIT_SET_E) ? "" : "E",
			!(op & SLJIT_KEEP_FLAGS) ? "" : "K", op_names[GET_OPCODE(op)]);
		fprintf(compiler->verbose, ", <%s>\n", jump_names[type]);
static SLJIT_INLINE void check_sljit_emit_const(struct sljit_compiler *compiler, int dst, sljit_w dstw, sljit_w init_value)
	SLJIT_UNUSED_ARG(init_value);
		fprintf(compiler->verbose, "  const ");
		fprintf(compiler->verbose, ", #%"SLJIT_PRINT_D"d\n", init_value);
static SLJIT_INLINE int emit_mov_before_return(struct sljit_compiler *compiler, int op, int src, sljit_w srcw)
	/* Return if don't need to do anything. */
		return SLJIT_SUCCESS;
	if (src == SLJIT_RETURN_REG && op == SLJIT_MOV)
	if (src == SLJIT_RETURN_REG && (op == SLJIT_MOV || op == SLJIT_MOV_UI || op == SLJIT_MOV_SI))
	compiler->skip_checks = 1;
	return sljit_emit_op1(compiler, op, SLJIT_RETURN_REG, 0, src, srcw);
/* CPU description section */
#define SLJIT_CPUINFO_PART1 " 32bit ("
#elif (defined SLJIT_64BIT_ARCHITECTURE && SLJIT_64BIT_ARCHITECTURE)
#define SLJIT_CPUINFO_PART1 " 64bit ("
#error "Internal error: CPU type info missing"
#if (defined SLJIT_LITTLE_ENDIAN && SLJIT_LITTLE_ENDIAN)
#define SLJIT_CPUINFO_PART2 "little endian + "
#elif (defined SLJIT_BIG_ENDIAN && SLJIT_BIG_ENDIAN)
#define SLJIT_CPUINFO_PART2 "big endian + "
#if (defined SLJIT_UNALIGNED && SLJIT_UNALIGNED)
#define SLJIT_CPUINFO_PART3 "unaligned)"
#define SLJIT_CPUINFO_PART3 "aligned)"
#define SLJIT_CPUINFO SLJIT_CPUINFO_PART1 SLJIT_CPUINFO_PART2 SLJIT_CPUINFO_PART3
	#include "sljitNativeX86_common.c"
#elif (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64)
#elif (defined SLJIT_CONFIG_ARM_V5 && SLJIT_CONFIG_ARM_V5)
	#include "sljitNativeARM_v5.c"
#elif (defined SLJIT_CONFIG_ARM_V7 && SLJIT_CONFIG_ARM_V7)
#elif (defined SLJIT_CONFIG_ARM_THUMB2 && SLJIT_CONFIG_ARM_THUMB2)
	#include "sljitNativeARM_Thumb2.c"
#elif (defined SLJIT_CONFIG_PPC_32 && SLJIT_CONFIG_PPC_32)
	#include "sljitNativePPC_common.c"
#elif (defined SLJIT_CONFIG_MIPS_32 && SLJIT_CONFIG_MIPS_32)
	#include "sljitNativeMIPS_common.c"
#if !(defined SLJIT_CONFIG_MIPS_32 && SLJIT_CONFIG_MIPS_32)
SLJIT_API_FUNC_ATTRIBUTE struct sljit_jump* sljit_emit_cmp(struct sljit_compiler *compiler, int type,
	/* Default compare for most architectures. */
	int flags, tmp_src, condition;
	sljit_w tmp_srcw;
	check_sljit_emit_cmp(compiler, type, src1, src1w, src2, src2w);
	condition = type & 0xff;
	if (SLJIT_UNLIKELY((src1 & SLJIT_IMM) && !(src2 & SLJIT_IMM))) {
		/* Immediate is prefered as second argument by most architectures. */
		switch (condition) {
		case SLJIT_C_LESS:
			condition = SLJIT_C_GREATER;
		case SLJIT_C_GREATER_EQUAL:
			condition = SLJIT_C_LESS_EQUAL;
		case SLJIT_C_GREATER:
			condition = SLJIT_C_LESS;
		case SLJIT_C_LESS_EQUAL:
			condition = SLJIT_C_GREATER_EQUAL;
		case SLJIT_C_SIG_LESS:
			condition = SLJIT_C_SIG_GREATER;
		case SLJIT_C_SIG_GREATER_EQUAL:
			condition = SLJIT_C_SIG_LESS_EQUAL;
		case SLJIT_C_SIG_GREATER:
			condition = SLJIT_C_SIG_LESS;
		case SLJIT_C_SIG_LESS_EQUAL:
			condition = SLJIT_C_SIG_GREATER_EQUAL;
		type = condition | (type & (SLJIT_INT_OP | SLJIT_REWRITABLE_JUMP));
		tmp_src = src1;
		src1 = src2;
		src2 = tmp_src;
		tmp_srcw = src1w;
		src1w = src2w;
		src2w = tmp_srcw;
	if (condition <= SLJIT_C_NOT_ZERO)
		flags = SLJIT_SET_E;
	else if (condition <= SLJIT_C_LESS_EQUAL)
		flags = SLJIT_SET_U;
		flags = SLJIT_SET_S;
	PTR_FAIL_IF(sljit_emit_op2(compiler, SLJIT_SUB | flags | (type & SLJIT_INT_OP),
		SLJIT_UNUSED, 0, src1, src1w, src2, src2w));
	return sljit_emit_jump(compiler, condition | (type & SLJIT_REWRITABLE_JUMP));
SLJIT_API_FUNC_ATTRIBUTE struct sljit_jump* sljit_emit_fcmp(struct sljit_compiler *compiler, int type,
	int flags, condition;
	check_sljit_emit_fcmp(compiler, type, src1, src1w, src2, src2w);
	if (condition <= SLJIT_C_FLOAT_NOT_EQUAL)
	sljit_emit_fop1(compiler, SLJIT_FCMP | flags, src1, src1w, src2, src2w);
#else /* SLJIT_CONFIG_UNSUPPORTED */
/* Empty function bodies for those machines, which are not (yet) supported. */
SLJIT_API_FUNC_ATTRIBUTE SLJIT_CONST char* sljit_get_platform_name()
	return "unsupported";
	SLJIT_UNUSED_ARG(verbose);
SLJIT_API_FUNC_ATTRIBUTE void* sljit_generate_code(struct sljit_compiler *compiler)
	SLJIT_UNUSED_ARG(code);
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_enter(struct sljit_compiler *compiler, int args, int temporaries, int saveds, int local_size)
	return SLJIT_ERR_UNSUPPORTED;
SLJIT_API_FUNC_ATTRIBUTE void sljit_set_context(struct sljit_compiler *compiler, int args, int temporaries, int saveds, int local_size)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_return(struct sljit_compiler *compiler, int op, int src, sljit_w srcw)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_fast_enter(struct sljit_compiler *compiler, int dst, sljit_w dstw, int args, int temporaries, int saveds, int local_size)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_fast_return(struct sljit_compiler *compiler, int src, sljit_w srcw)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_op0(struct sljit_compiler *compiler, int op)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_op1(struct sljit_compiler *compiler, int op,
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_op2(struct sljit_compiler *compiler, int op,
SLJIT_API_FUNC_ATTRIBUTE int sljit_get_register_index(int reg)
	return reg;
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_op_custom(struct sljit_compiler *compiler,
SLJIT_API_FUNC_ATTRIBUTE int sljit_is_fpu_available(void)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_fop1(struct sljit_compiler *compiler, int op,
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_fop2(struct sljit_compiler *compiler, int op,
SLJIT_API_FUNC_ATTRIBUTE struct sljit_label* sljit_emit_label(struct sljit_compiler *compiler)
SLJIT_API_FUNC_ATTRIBUTE struct sljit_jump* sljit_emit_jump(struct sljit_compiler *compiler, int type)
	SLJIT_UNUSED_ARG(jump);
	SLJIT_UNUSED_ARG(label);
	SLJIT_UNUSED_ARG(target);
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_ijump(struct sljit_compiler *compiler, int type, int src, sljit_w srcw)
SLJIT_API_FUNC_ATTRIBUTE int sljit_emit_cond_value(struct sljit_compiler *compiler, int op, int dst, sljit_w dstw, int type)
SLJIT_API_FUNC_ATTRIBUTE struct sljit_const* sljit_emit_const(struct sljit_compiler *compiler, int dst, sljit_w dstw, sljit_w initval)
	SLJIT_UNUSED_ARG(initval);
SLJIT_API_FUNC_ATTRIBUTE void sljit_set_jump_addr(sljit_uw addr, sljit_uw new_addr)
	SLJIT_UNUSED_ARG(addr);
	SLJIT_UNUSED_ARG(new_addr);
SLJIT_API_FUNC_ATTRIBUTE void sljit_set_const(sljit_uw addr, sljit_w new_constant)
	SLJIT_UNUSED_ARG(new_constant);
