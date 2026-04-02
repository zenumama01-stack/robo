	return "x86" SLJIT_CPUINFO;
   32b register indexes:
     0 - EAX
     1 - ECX
     2 - EDX
     3 - EBX
     4 - none
     5 - EBP
     6 - ESI
     7 - EDI
   64b register indexes:
     0 - RAX
     1 - RCX
     2 - RDX
     3 - RBX
     5 - RBP
     6 - RSI
     7 - RDI
     8 - R8   - From now on REX prefix is required
     9 - R9
    10 - R10
    11 - R11
    12 - R12
    13 - R13
    14 - R14
    15 - R15
/* Last register + 1. */
#define TMP_REGISTER	(SLJIT_NO_REGISTERS + 1)
static SLJIT_CONST sljit_ub reg_map[SLJIT_NO_REGISTERS + 2] = {
  0, 0, 2, 1, 0, 0, 3, 6, 7, 0, 0, 4, 5
#define CHECK_EXTRA_REGS(p, w, do) \
	if (p >= SLJIT_TEMPORARY_EREG1 && p <= SLJIT_TEMPORARY_EREG2) { \
		w = compiler->temporaries_start + (p - SLJIT_TEMPORARY_EREG1) * sizeof(sljit_w); \
		p = SLJIT_MEM1(SLJIT_LOCALS_REG); \
		do; \
	else if (p >= SLJIT_SAVED_EREG1 && p <= SLJIT_SAVED_EREG2) { \
		w = compiler->saveds_start + (p - SLJIT_SAVED_EREG1) * sizeof(sljit_w); \
#else /* SLJIT_CONFIG_X86_32 */
#define TMP_REG2	(SLJIT_NO_REGISTERS + 2)
#define TMP_REG3	(SLJIT_NO_REGISTERS + 3)
/* Note: r12 & 0x7 == 0b100, which decoded as SIB byte present
   Note: avoid to use r12 and r13 for memory addessing
   therefore r12 is better for SAVED_EREG than SAVED_REG. */
/* 1st passed in rdi, 2nd argument passed in rsi, 3rd in rdx. */
static SLJIT_CONST sljit_ub reg_map[SLJIT_NO_REGISTERS + 4] = {
  0, 0, 6, 1, 8, 11, 3, 15, 14, 13, 12, 4, 2, 7, 9
/* low-map. reg_map & 0x7. */
static SLJIT_CONST sljit_ub reg_lmap[SLJIT_NO_REGISTERS + 4] = {
  0, 0, 6, 1, 0, 3,  3, 7,  6,  5,  4,  4, 2, 7, 1
/* 1st passed in rcx, 2nd argument passed in rdx, 3rd in r8. */
  0, 0, 2, 1, 11, 13, 3, 6, 7, 14, 12, 15, 10, 8, 9
  0, 0, 2, 1, 3,  5,  3, 6, 7,  6,  4,  7, 2,  0, 1
#define REX_W		0x48
#define REX_R		0x44
#define REX_X		0x42
#define REX_B		0x41
#define REX		0x40
typedef unsigned int sljit_uhw;
typedef int sljit_hw;
#define IS_HALFWORD(x)		((x) <= 0x7fffffffll && (x) >= -0x80000000ll)
#define NOT_HALFWORD(x)		((x) > 0x7fffffffll || (x) < -0x80000000ll)
#define CHECK_EXTRA_REGS(p, w, do)
#endif /* SLJIT_CONFIG_X86_32 */
#define TMP_FREG	(SLJIT_FLOAT_REG4 + 1)
/* Size flags for emit_x86_instruction: */
#define EX86_BIN_INS		0x0010
#define EX86_SHIFT_INS		0x0020
#define EX86_REX		0x0040
#define EX86_NO_REXW		0x0080
#define EX86_BYTE_ARG		0x0100
#define EX86_HALF_ARG		0x0200
#define EX86_PREF_66		0x0400
#define EX86_PREF_F2		0x0800
#define EX86_SSE2		0x1000
#define INC_SIZE(s)			(*buf++ = (s), compiler->size += (s))
#define INC_CSIZE(s)			(*code++ = (s), compiler->size += (s))
#define PUSH_REG(r)			(*buf++ = (0x50 + (r)))
#define POP_REG(r)			(*buf++ = (0x58 + (r)))
#define RET()				(*buf++ = (0xc3))
#define RETN(n)				(*buf++ = (0xc2), *buf++ = n, *buf++ = 0)
/* r32, r/m32 */
#define MOV_RM(mod, reg, rm)		(*buf++ = (0x8b), *buf++ = (mod) << 6 | (reg) << 3 | (rm))
static sljit_ub get_jump_code(int type)
	switch (type) {
	case SLJIT_C_EQUAL:
	case SLJIT_C_FLOAT_EQUAL:
		return 0x84;
	case SLJIT_C_NOT_EQUAL:
	case SLJIT_C_FLOAT_NOT_EQUAL:
		return 0x85;
	case SLJIT_C_FLOAT_LESS:
		return 0x82;
	case SLJIT_C_FLOAT_GREATER_EQUAL:
		return 0x83;
	case SLJIT_C_FLOAT_GREATER:
		return 0x87;
	case SLJIT_C_FLOAT_LESS_EQUAL:
		return 0x86;
		return 0x8c;
		return 0x8d;
		return 0x8f;
		return 0x8e;
	case SLJIT_C_OVERFLOW:
	case SLJIT_C_MUL_OVERFLOW:
		return 0x80;
	case SLJIT_C_NOT_OVERFLOW:
	case SLJIT_C_MUL_NOT_OVERFLOW:
		return 0x81;
	case SLJIT_C_FLOAT_NAN:
		return 0x8a;
	case SLJIT_C_FLOAT_NOT_NAN:
		return 0x8b;
static sljit_ub* generate_far_jump_code(struct sljit_jump *jump, sljit_ub *code_ptr, int type);
static sljit_ub* generate_fixed_jump(sljit_ub *code_ptr, sljit_w addr, int type);
static sljit_ub* generate_near_jump_code(struct sljit_jump *jump, sljit_ub *code_ptr, sljit_ub *code, int type)
	int short_jump;
	sljit_uw label_addr;
		label_addr = (sljit_uw)(code + jump->u.label->size);
		label_addr = jump->u.target;
	short_jump = (sljit_w)(label_addr - (jump->addr + 2)) >= -128 && (sljit_w)(label_addr - (jump->addr + 2)) <= 127;
	if ((sljit_w)(label_addr - (jump->addr + 1)) > 0x7fffffffll || (sljit_w)(label_addr - (jump->addr + 1)) < -0x80000000ll)
		return generate_far_jump_code(jump, code_ptr, type);
		if (short_jump)
			*code_ptr++ = 0xeb;
		short_jump = 0;
	else if (short_jump) {
		*code_ptr++ = get_jump_code(type) - 0x10;
	if (short_jump) {
		jump->flags |= PATCH_MB;
		code_ptr += sizeof(sljit_b);
		code_ptr += sizeof(sljit_hw);
	sljit_ub *code;
	sljit_ub *code_ptr;
	sljit_ub *buf_end;
	sljit_ub len;
	struct sljit_const *const_;
	check_sljit_generate_code(compiler);
	reverse_buf(compiler);
	/* Second code generation pass. */
	code = (sljit_ub*)SLJIT_MALLOC_EXEC(compiler->size);
	PTR_FAIL_WITH_EXEC_IF(code);
	code_ptr = code;
	label = compiler->labels;
	const_ = compiler->consts;
		buf_ptr = buf->memory;
		buf_end = buf_ptr + buf->used_size;
			len = *buf_ptr++;
			if (len > 0) {
				/* The code is already generated. */
				SLJIT_MEMMOVE(code_ptr, buf_ptr, len);
				code_ptr += len;
				buf_ptr += len;
				if (*buf_ptr >= 4) {
					if (!(jump->flags & SLJIT_REWRITABLE_JUMP))
						code_ptr = generate_near_jump_code(jump, code_ptr, code, *buf_ptr - 4);
						code_ptr = generate_far_jump_code(jump, code_ptr, *buf_ptr - 4);
				else if (*buf_ptr == 0) {
					label->addr = (sljit_uw)code_ptr;
					label->size = code_ptr - code;
					label = label->next;
				else if (*buf_ptr == 1) {
					const_->addr = ((sljit_uw)code_ptr) - sizeof(sljit_w);
					const_ = const_->next;
					*code_ptr++ = (*buf_ptr == 2) ? 0xe8 /* call */ : 0xe9 /* jmp */;
					buf_ptr++;
					*(sljit_w*)code_ptr = *(sljit_w*)buf_ptr - ((sljit_w)code_ptr + sizeof(sljit_w));
					buf_ptr += sizeof(sljit_w) - 1;
					code_ptr = generate_fixed_jump(code_ptr, *(sljit_w*)(buf_ptr + 1), *buf_ptr);
		} while (buf_ptr < buf_end);
		SLJIT_ASSERT(buf_ptr == buf_end);
	} while (buf);
	SLJIT_ASSERT(!label);
	SLJIT_ASSERT(!jump);
	SLJIT_ASSERT(!const_);
		if (jump->flags & PATCH_MB) {
			SLJIT_ASSERT((sljit_w)(jump->u.label->addr - (jump->addr + sizeof(sljit_b))) >= -128 && (sljit_w)(jump->u.label->addr - (jump->addr + sizeof(sljit_b))) <= 127);
			*(sljit_ub*)jump->addr = (sljit_ub)(jump->u.label->addr - (jump->addr + sizeof(sljit_b)));
		} else if (jump->flags & PATCH_MW) {
			if (jump->flags & JUMP_LABEL) {
				*(sljit_w*)jump->addr = (sljit_w)(jump->u.label->addr - (jump->addr + sizeof(sljit_w)));
				SLJIT_ASSERT((sljit_w)(jump->u.label->addr - (jump->addr + sizeof(sljit_hw))) >= -0x80000000ll && (sljit_w)(jump->u.label->addr - (jump->addr + sizeof(sljit_hw))) <= 0x7fffffffll);
				*(sljit_hw*)jump->addr = (sljit_hw)(jump->u.label->addr - (jump->addr + sizeof(sljit_hw)));
				*(sljit_w*)jump->addr = (sljit_w)(jump->u.target - (jump->addr + sizeof(sljit_w)));
				SLJIT_ASSERT((sljit_w)(jump->u.target - (jump->addr + sizeof(sljit_hw))) >= -0x80000000ll && (sljit_w)(jump->u.target - (jump->addr + sizeof(sljit_hw))) <= 0x7fffffffll);
				*(sljit_hw*)jump->addr = (sljit_hw)(jump->u.target - (jump->addr + sizeof(sljit_hw)));
		else if (jump->flags & PATCH_MD)
			*(sljit_w*)jump->addr = jump->u.label->addr;
	/* Maybe we waste some space because of short jumps. */
	SLJIT_ASSERT(code_ptr <= code + compiler->size);
	compiler->error = SLJIT_ERR_COMPILED;
	compiler->executable_size = compiler->size;
	return (void*)code;
static int emit_cum_binary(struct sljit_compiler *compiler,
	sljit_ub op_rm, sljit_ub op_mr, sljit_ub op_imm, sljit_ub op_eax_imm,
	int src2, sljit_w src2w);
static int emit_non_cum_binary(struct sljit_compiler *compiler,
static int emit_mov(struct sljit_compiler *compiler,
	int src, sljit_w srcw);
static SLJIT_INLINE int emit_save_flags(struct sljit_compiler *compiler)
	*buf++ = 0x9c; /* pushfd */
	buf = (sljit_ub*)ensure_buf(compiler, 1 + 6);
	INC_SIZE(6);
	*buf++ = 0x9c; /* pushfq */
	*buf++ = 0x48;
	*buf++ = 0x8d; /* lea esp/rsp, [esp/rsp + sizeof(sljit_w)] */
	*buf++ = 0x64;
	*buf++ = sizeof(sljit_w);
	compiler->flags_saved = 1;
static SLJIT_INLINE int emit_restore_flags(struct sljit_compiler *compiler, int keep_flags)
	*buf++ = 0x8d; /* lea esp/rsp, [esp/rsp - sizeof(sljit_w)] */
	*buf++ = (sljit_ub)-(int)sizeof(sljit_w);
	*buf++ = 0x9d; /* popfd / popfq */
	compiler->flags_saved = keep_flags;
#include <malloc.h>
static void SLJIT_CALL sljit_touch_stack(sljit_w local_size)
	/* Workaround for calling _chkstk. */
	alloca(local_size);
#include "sljitNativeX86_32.c"
#include "sljitNativeX86_64.c"
	if (dst == SLJIT_UNUSED) {
		/* No destination, doesn't need to setup flags. */
		if (src & SLJIT_MEM) {
			code = emit_x86_instruction(compiler, 1, TMP_REGISTER, 0, src, srcw);
			*code = 0x8b;
		code = emit_x86_instruction(compiler, 1, src, 0, dst, dstw);
			return emit_do_imm(compiler, 0xb8 + reg_map[dst], srcw);
			if (!compiler->mode32) {
				if (NOT_HALFWORD(srcw))
				return emit_do_imm32(compiler, (reg_map[dst] >= 8) ? REX_B : 0, 0xb8 + reg_lmap[dst], srcw);
		if (!compiler->mode32 && NOT_HALFWORD(srcw)) {
			FAIL_IF(emit_load_imm64(compiler, TMP_REG2, srcw));
			code = emit_x86_instruction(compiler, 1, TMP_REG2, 0, dst, dstw);
		code = emit_x86_instruction(compiler, 1, SLJIT_IMM, srcw, dst, dstw);
		code = emit_x86_instruction(compiler, 1, dst, 0, src, srcw);
	/* Memory to memory move. Requires two instruction. */
	code = emit_x86_instruction(compiler, 1, TMP_REGISTER, 0, dst, dstw);
#define EMIT_MOV(compiler, dst, dstw, src, srcw) \
	FAIL_IF(emit_mov(compiler, dst, dstw, src, srcw));
	check_sljit_emit_op0(compiler, op);
	switch (GET_OPCODE(op)) {
	case SLJIT_BREAKPOINT:
		*buf = 0xcc;
	case SLJIT_NOP:
		*buf = 0x90;
	case SLJIT_UMUL:
	case SLJIT_SMUL:
	case SLJIT_UDIV:
	case SLJIT_SDIV:
			reg_map[SLJIT_TEMPORARY_REG1] == 0
			&& reg_map[SLJIT_TEMPORARY_REG2] == 2
			&& reg_map[TMP_REGISTER] > 7,
			invalid_register_assignment_for_div_mul);
			&& reg_map[SLJIT_TEMPORARY_REG2] < 7
			&& reg_map[TMP_REGISTER] == 2,
		compiler->mode32 = op & SLJIT_INT_OP;
		op = GET_OPCODE(op);
		if (op == SLJIT_UDIV) {
#if (defined SLJIT_CONFIG_X86_32 && SLJIT_CONFIG_X86_32) || defined(_WIN64)
			EMIT_MOV(compiler, TMP_REGISTER, 0, SLJIT_TEMPORARY_REG2, 0);
			buf = emit_x86_instruction(compiler, 1, SLJIT_TEMPORARY_REG2, 0, SLJIT_TEMPORARY_REG2, 0);
			buf = emit_x86_instruction(compiler, 1, TMP_REGISTER, 0, TMP_REGISTER, 0);
			*buf = 0x33;
		if (op == SLJIT_SDIV) {
			/* CDQ instruction */
			*buf = 0x99;
			if (compiler->mode32) {
		*buf++ = 0xf7;
		*buf = 0xc0 | ((op >= SLJIT_UDIV) ? reg_map[TMP_REGISTER] : reg_map[SLJIT_TEMPORARY_REG2]);
		size = (!compiler->mode32 || op >= SLJIT_UDIV) ? 3 : 2;
		size = (!compiler->mode32) ? 3 : 2;
		if (!compiler->mode32)
			*buf++ = REX_W | ((op >= SLJIT_UDIV) ? REX_B : 0);
		else if (op >= SLJIT_UDIV)
		*buf = 0xc0 | ((op >= SLJIT_UDIV) ? reg_lmap[TMP_REGISTER] : reg_lmap[SLJIT_TEMPORARY_REG2]);
		*buf = 0xc0 | reg_map[SLJIT_TEMPORARY_REG2];
		switch (op) {
			*buf |= 4 << 3;
			*buf |= 5 << 3;
			*buf |= 7 << 3;
#if (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64) && !defined(_WIN64)
		EMIT_MOV(compiler, SLJIT_TEMPORARY_REG2, 0, TMP_REGISTER, 0);
#define ENCODE_PREFIX(prefix) \
		code = (sljit_ub*)ensure_buf(compiler, 1 + 1); \
		FAIL_IF(!code); \
		INC_CSIZE(1); \
		*code = (prefix); \
static int emit_mov_byte(struct sljit_compiler *compiler, int sign,
	int work_r;
		code = emit_x86_instruction(compiler, 1 | EX86_BYTE_ARG | EX86_NO_REXW, SLJIT_IMM, srcw, dst, dstw);
		*code = 0xc6;
	dst_r = (dst >= SLJIT_TEMPORARY_REG1 && dst <= TMP_REGISTER) ? dst : TMP_REGISTER;
	if ((dst & SLJIT_MEM) && src >= SLJIT_TEMPORARY_REG1 && src <= SLJIT_NO_REGISTERS) {
		if (reg_map[src] >= 4) {
			SLJIT_ASSERT(dst_r == TMP_REGISTER);
			EMIT_MOV(compiler, TMP_REGISTER, 0, src, 0);
	else if (src >= SLJIT_TEMPORARY_REG1 && src <= SLJIT_NO_REGISTERS && reg_map[src] >= 4) {
		/* src, dst are registers. */
		SLJIT_ASSERT(dst >= SLJIT_TEMPORARY_REG1 && dst <= TMP_REGISTER);
		if (reg_map[dst] < 4) {
			if (dst != src)
				EMIT_MOV(compiler, dst, 0, src, 0);
			code = emit_x86_instruction(compiler, 2, dst, 0, dst, 0);
			*code++ = 0x0f;
			*code = sign ? 0xbe : 0xb6;
				/* shl reg, 24 */
				code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, SLJIT_IMM, 24, dst, 0);
				*code |= 0x4 << 3;
				/* shr/sar reg, 24 */
				*code |= 0x7 << 3;
				/* and dst, 0xff */
				code = emit_x86_instruction(compiler, 1 | EX86_BIN_INS, SLJIT_IMM, 255, dst, 0);
				*(code + 1) |= 0x4 << 3;
		/* src can be memory addr or reg_map[src] < 4 on x86_32 architectures. */
		code = emit_x86_instruction(compiler, 2, dst_r, 0, src, srcw);
		if (dst_r == TMP_REGISTER) {
			/* Find a non-used register, whose reg_map[src] < 4. */
			if ((dst & 0xf) == SLJIT_TEMPORARY_REG1) {
				if ((dst & 0xf0) == (SLJIT_TEMPORARY_REG2 << 4))
					work_r = SLJIT_TEMPORARY_REG3;
					work_r = SLJIT_TEMPORARY_REG2;
				if ((dst & 0xf0) != (SLJIT_TEMPORARY_REG1 << 4))
					work_r = SLJIT_TEMPORARY_REG1;
				else if ((dst & 0xf) == SLJIT_TEMPORARY_REG2)
			if (work_r == SLJIT_TEMPORARY_REG1) {
				ENCODE_PREFIX(0x90 + reg_map[TMP_REGISTER]);
				code = emit_x86_instruction(compiler, 1, work_r, 0, dst_r, 0);
				*code = 0x87;
			code = emit_x86_instruction(compiler, 1, work_r, 0, dst, dstw);
			*code = 0x88;
		code = emit_x86_instruction(compiler, 1 | EX86_REX | EX86_NO_REXW, dst_r, 0, dst, dstw);
static int emit_mov_half(struct sljit_compiler *compiler, int sign,
		code = emit_x86_instruction(compiler, 1 | EX86_HALF_ARG | EX86_NO_REXW | EX86_PREF_66, SLJIT_IMM, srcw, dst, dstw);
	if ((dst & SLJIT_MEM) && (src >= SLJIT_TEMPORARY_REG1 && src <= SLJIT_NO_REGISTERS))
		*code = sign ? 0xbf : 0xb7;
		code = emit_x86_instruction(compiler, 1 | EX86_NO_REXW | EX86_PREF_66, dst_r, 0, dst, dstw);
static int emit_unary(struct sljit_compiler *compiler, int un_index,
		EMIT_MOV(compiler, TMP_REGISTER, 0, src, srcw);
		code = emit_x86_instruction(compiler, 1, 0, 0, TMP_REGISTER, 0);
		*code++ = 0xf7;
		*code |= (un_index) << 3;
	if (dst == src && dstw == srcw) {
		/* Same input and output */
		code = emit_x86_instruction(compiler, 1, 0, 0, dst, dstw);
		EMIT_MOV(compiler, dst, 0, src, srcw);
	EMIT_MOV(compiler, dst, dstw, TMP_REGISTER, 0);
static int emit_not_with_flags(struct sljit_compiler *compiler,
		*code |= 0x2 << 3;
		code = emit_x86_instruction(compiler, 1, TMP_REGISTER, 0, TMP_REGISTER, 0);
		*code = 0x0b;
		code = emit_x86_instruction(compiler, 1, dst, 0, dst, 0);
static int emit_clz(struct sljit_compiler *compiler, int op,
	if (SLJIT_UNLIKELY(dst == SLJIT_UNUSED)) {
		/* Just set the zero flag. */
		code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, SLJIT_IMM, 31, TMP_REGISTER, 0);
		code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, SLJIT_IMM, !(op & SLJIT_INT_OP) ? 63 : 31, TMP_REGISTER, 0);
		*code |= 0x5 << 3;
	if (SLJIT_UNLIKELY(src & SLJIT_IMM)) {
		srcw = 0;
	code = emit_x86_instruction(compiler, 2, TMP_REGISTER, 0, src, srcw);
	*code = 0xbd;
	if (dst >= SLJIT_TEMPORARY_REG1 && dst <= TMP_REGISTER)
		dst_r = dst;
		/* Find an unused temporary register. */
		if ((dst & 0xf) != SLJIT_TEMPORARY_REG1 && (dst & 0xf0) != (SLJIT_TEMPORARY_REG1 << 4))
			dst_r = SLJIT_TEMPORARY_REG1;
		else if ((dst & 0xf) != SLJIT_TEMPORARY_REG2 && (dst & 0xf0) != (SLJIT_TEMPORARY_REG2 << 4))
			dst_r = SLJIT_TEMPORARY_REG2;
			dst_r = SLJIT_TEMPORARY_REG3;
		EMIT_MOV(compiler, dst, dstw, dst_r, 0);
	EMIT_MOV(compiler, dst_r, 0, SLJIT_IMM, 32 + 31);
	dst_r = (dst >= SLJIT_TEMPORARY_REG1 && dst <= TMP_REGISTER) ? dst : TMP_REG2;
	EMIT_MOV(compiler, dst_r, 0, SLJIT_IMM, !(op & SLJIT_INT_OP) ? 64 + 63 : 32 + 31);
	code = emit_x86_instruction(compiler, 2, dst_r, 0, TMP_REGISTER, 0);
	*code = 0x45;
	code = emit_x86_instruction(compiler, 1 | EX86_BIN_INS, SLJIT_IMM, 31, dst_r, 0);
	code = emit_x86_instruction(compiler, 1 | EX86_BIN_INS, SLJIT_IMM, !(op & SLJIT_INT_OP) ? 63 : 31, dst_r, 0);
	*(code + 1) |= 0x6 << 3;
	if (dst & SLJIT_MEM)
		EMIT_MOV(compiler, dst, dstw, TMP_REG2, 0);
	int update = 0;
	int dst_is_ereg = 0;
	int src_is_ereg = 0;
	#define src_is_ereg 0
	check_sljit_emit_op1(compiler, op, dst, dstw, src, srcw);
	CHECK_EXTRA_REGS(dst, dstw, dst_is_ereg = 1);
	CHECK_EXTRA_REGS(src, srcw, src_is_ereg = 1);
	if (GET_OPCODE(op) >= SLJIT_MOV && GET_OPCODE(op) <= SLJIT_MOVU_SI) {
		SLJIT_COMPILE_ASSERT(SLJIT_MOV + 7 == SLJIT_MOVU, movu_offset);
		if (op >= SLJIT_MOVU) {
			update = 1;
			op -= 7;
			case SLJIT_MOV_UB:
				srcw = (unsigned char)srcw;
			case SLJIT_MOV_SB:
				srcw = (signed char)srcw;
			case SLJIT_MOV_UH:
				srcw = (unsigned short)srcw;
			case SLJIT_MOV_SH:
				srcw = (signed short)srcw;
			case SLJIT_MOV_UI:
				srcw = (unsigned int)srcw;
			case SLJIT_MOV_SI:
				srcw = (signed int)srcw;
			if (SLJIT_UNLIKELY(dst_is_ereg))
				return emit_mov(compiler, dst, dstw, src, srcw);
		if (SLJIT_UNLIKELY(update) && (src & SLJIT_MEM) && !src_is_ereg && (src & 0xf) && (srcw != 0 || (src & 0xf0) != 0)) {
			code = emit_x86_instruction(compiler, 1, src & 0xf, 0, src, srcw);
			*code = 0x8d;
			src &= SLJIT_MEM | 0xf;
		if (SLJIT_UNLIKELY(dst_is_ereg) && (!(op == SLJIT_MOV || op == SLJIT_MOV_UI || op == SLJIT_MOV_SI) || (src & SLJIT_MEM))) {
			SLJIT_ASSERT(dst == SLJIT_MEM1(SLJIT_LOCALS_REG));
		case SLJIT_MOV:
			FAIL_IF(emit_mov_byte(compiler, 0, dst, dstw, src, (src & SLJIT_IMM) ? (unsigned char)srcw : srcw));
			FAIL_IF(emit_mov_byte(compiler, 1, dst, dstw, src, (src & SLJIT_IMM) ? (signed char)srcw : srcw));
			FAIL_IF(emit_mov_half(compiler, 0, dst, dstw, src, (src & SLJIT_IMM) ? (unsigned short)srcw : srcw));
			FAIL_IF(emit_mov_half(compiler, 1, dst, dstw, src, (src & SLJIT_IMM) ? (signed short)srcw : srcw));
			FAIL_IF(emit_mov_int(compiler, 0, dst, dstw, src, (src & SLJIT_IMM) ? (unsigned int)srcw : srcw));
			FAIL_IF(emit_mov_int(compiler, 1, dst, dstw, src, (src & SLJIT_IMM) ? (signed int)srcw : srcw));
		if (SLJIT_UNLIKELY(dst_is_ereg) && dst == TMP_REGISTER)
			return emit_mov(compiler, SLJIT_MEM1(SLJIT_LOCALS_REG), dstw, TMP_REGISTER, 0);
		if (SLJIT_UNLIKELY(update) && (dst & SLJIT_MEM) && (dst & 0xf) && (dstw != 0 || (dst & 0xf0) != 0)) {
			code = emit_x86_instruction(compiler, 1, dst & 0xf, 0, dst, dstw);
	if (SLJIT_UNLIKELY(GET_FLAGS(op)))
	case SLJIT_NOT:
		if (SLJIT_UNLIKELY(op & SLJIT_SET_E))
			return emit_not_with_flags(compiler, dst, dstw, src, srcw);
		return emit_unary(compiler, 0x2, dst, dstw, src, srcw);
	case SLJIT_NEG:
		if (SLJIT_UNLIKELY(op & SLJIT_KEEP_FLAGS) && !compiler->flags_saved)
			FAIL_IF(emit_save_flags(compiler));
		return emit_unary(compiler, 0x3, dst, dstw, src, srcw);
	case SLJIT_CLZ:
		return emit_clz(compiler, op, dst, dstw, src, srcw);
	#undef src_is_ereg
#define BINARY_IMM(_op_imm_, _op_mr_, immw, arg, argw) \
	if (IS_HALFWORD(immw) || compiler->mode32) { \
		code = emit_x86_instruction(compiler, 1 | EX86_BIN_INS, SLJIT_IMM, immw, arg, argw); \
		*(code + 1) |= (_op_imm_); \
		FAIL_IF(emit_load_imm64(compiler, TMP_REG2, immw)); \
		code = emit_x86_instruction(compiler, 1, TMP_REG2, 0, arg, argw); \
		*code = (_op_mr_); \
#define BINARY_EAX_IMM(_op_eax_imm_, immw) \
	FAIL_IF(emit_do_imm32(compiler, (!compiler->mode32) ? REX_W : 0, (_op_eax_imm_), immw))
	*(code + 1) |= (_op_imm_);
	FAIL_IF(emit_do_imm(compiler, (_op_eax_imm_), immw))
		EMIT_MOV(compiler, TMP_REGISTER, 0, src1, src1w);
		if (src2 & SLJIT_IMM) {
			BINARY_IMM(op_imm, op_mr, src2w, TMP_REGISTER, 0);
			code = emit_x86_instruction(compiler, 1, TMP_REGISTER, 0, src2, src2w);
			*code = op_rm;
	if (dst == src1 && dstw == src1w) {
			if ((dst == SLJIT_TEMPORARY_REG1) && (src2w > 127 || src2w < -128) && (compiler->mode32 || IS_HALFWORD(src2w))) {
			if ((dst == SLJIT_TEMPORARY_REG1) && (src2w > 127 || src2w < -128)) {
				BINARY_EAX_IMM(op_eax_imm, src2w);
				BINARY_IMM(op_imm, op_mr, src2w, dst, dstw);
		else if (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) {
			code = emit_x86_instruction(compiler, 1, dst, dstw, src2, src2w);
		else if (src2 >= SLJIT_TEMPORARY_REG1 && src2 <= TMP_REGISTER) {
			/* Special exception for sljit_emit_cond_value. */
			code = emit_x86_instruction(compiler, 1, src2, src2w, dst, dstw);
			*code = op_mr;
			EMIT_MOV(compiler, TMP_REGISTER, 0, src2, src2w);
	/* Only for cumulative operations. */
	if (dst == src2 && dstw == src2w) {
		if (src1 & SLJIT_IMM) {
			if ((dst == SLJIT_TEMPORARY_REG1) && (src1w > 127 || src1w < -128) && (compiler->mode32 || IS_HALFWORD(src1w))) {
			if ((dst == SLJIT_TEMPORARY_REG1) && (src1w > 127 || src1w < -128)) {
				BINARY_EAX_IMM(op_eax_imm, src1w);
				BINARY_IMM(op_imm, op_mr, src1w, dst, dstw);
			code = emit_x86_instruction(compiler, 1, dst, dstw, src1, src1w);
		else if (src1 >= SLJIT_TEMPORARY_REG1 && src1 <= SLJIT_NO_REGISTERS) {
			code = emit_x86_instruction(compiler, 1, src1, src1w, dst, dstw);
	/* General version. */
		EMIT_MOV(compiler, dst, 0, src1, src1w);
			BINARY_IMM(op_imm, op_mr, src2w, dst, 0);
			code = emit_x86_instruction(compiler, 1, dst, 0, src2, src2w);
		/* This version requires less memory writing. */
		else if (src2 >= SLJIT_TEMPORARY_REG1 && src2 <= SLJIT_NO_REGISTERS) {
	if ((dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) && dst != src2) {
static int emit_mul(struct sljit_compiler *compiler,
	dst_r = (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) ? dst : TMP_REGISTER;
	/* Register destination. */
	if (dst_r == src1 && !(src2 & SLJIT_IMM)) {
		code = emit_x86_instruction(compiler, 2, dst_r, 0, src2, src2w);
		*code = 0xaf;
	else if (dst_r == src2 && !(src1 & SLJIT_IMM)) {
		code = emit_x86_instruction(compiler, 2, dst_r, 0, src1, src1w);
	else if (src1 & SLJIT_IMM) {
			EMIT_MOV(compiler, dst_r, 0, SLJIT_IMM, src2w);
			src2 = dst_r;
			src2w = 0;
		if (src1w <= 127 && src1w >= -128) {
			code = emit_x86_instruction(compiler, 1, dst_r, 0, src2, src2w);
			*code = 0x6b;
			code = (sljit_ub*)ensure_buf(compiler, 1 + 1);
			INC_CSIZE(1);
			*code = (sljit_b)src1w;
			*code = 0x69;
			code = (sljit_ub*)ensure_buf(compiler, 1 + 4);
			INC_CSIZE(4);
			*(sljit_w*)code = src1w;
		else if (IS_HALFWORD(src1w)) {
			*(sljit_hw*)code = (sljit_hw)src1w;
			EMIT_MOV(compiler, TMP_REG2, 0, SLJIT_IMM, src1w);
			if (dst_r != src2)
				EMIT_MOV(compiler, dst_r, 0, src2, src2w);
			code = emit_x86_instruction(compiler, 2, dst_r, 0, TMP_REG2, 0);
	else if (src2 & SLJIT_IMM) {
		/* Note: src1 is NOT immediate. */
		if (src2w <= 127 && src2w >= -128) {
			code = emit_x86_instruction(compiler, 1, dst_r, 0, src1, src1w);
			*code = (sljit_b)src2w;
			*(sljit_w*)code = src2w;
		else if (IS_HALFWORD(src2w)) {
			*(sljit_hw*)code = (sljit_hw)src2w;
			if (dst_r != src1)
				EMIT_MOV(compiler, dst_r, 0, src1, src1w);
		/* Neither argument is immediate. */
		if (ADDRESSING_DEPENDS_ON(src2, dst_r))
			dst_r = TMP_REGISTER;
	if (dst_r == TMP_REGISTER)
static int emit_lea_binary(struct sljit_compiler *compiler,
	int dst_r, done = 0;
	/* These cases better be left to handled by normal way. */
	if (dst == src1 && dstw == src1w)
	if (dst == src2 && dstw == src2w)
	if (src1 >= SLJIT_TEMPORARY_REG1 && src1 <= SLJIT_NO_REGISTERS) {
		if (src2 >= SLJIT_TEMPORARY_REG1 && src2 <= SLJIT_NO_REGISTERS) {
			/* It is not possible to be both SLJIT_LOCALS_REG. */
			if (src1 != SLJIT_LOCALS_REG || src2 != SLJIT_LOCALS_REG) {
				code = emit_x86_instruction(compiler, 1, dst_r, 0, SLJIT_MEM2(src1, src2), 0);
				done = 1;
		if ((src2 & SLJIT_IMM) && (compiler->mode32 || IS_HALFWORD(src2w))) {
			code = emit_x86_instruction(compiler, 1, dst_r, 0, SLJIT_MEM1(src1), (int)src2w);
			code = emit_x86_instruction(compiler, 1, dst_r, 0, SLJIT_MEM1(src1), src2w);
		if ((src1 & SLJIT_IMM) && (compiler->mode32 || IS_HALFWORD(src1w))) {
			code = emit_x86_instruction(compiler, 1, dst_r, 0, SLJIT_MEM1(src2), (int)src1w);
			code = emit_x86_instruction(compiler, 1, dst_r, 0, SLJIT_MEM1(src2), src1w);
	if (done) {
			return emit_mov(compiler, dst, dstw, TMP_REGISTER, 0);
static int emit_cmp_binary(struct sljit_compiler *compiler,
	if (src1 == SLJIT_TEMPORARY_REG1 && (src2 & SLJIT_IMM) && (src2w > 127 || src2w < -128) && (compiler->mode32 || IS_HALFWORD(src2w))) {
	if (src1 == SLJIT_TEMPORARY_REG1 && (src2 & SLJIT_IMM) && (src2w > 127 || src2w < -128)) {
		BINARY_EAX_IMM(0x3d, src2w);
			BINARY_IMM(0x7 << 3, 0x39, src2w, src1, 0);
			code = emit_x86_instruction(compiler, 1, src1, 0, src2, src2w);
			*code = 0x3b;
	if (src2 >= SLJIT_TEMPORARY_REG1 && src2 <= SLJIT_NO_REGISTERS && !(src1 & SLJIT_IMM)) {
		code = emit_x86_instruction(compiler, 1, src2, 0, src1, src1w);
		*code = 0x39;
			src1 = TMP_REGISTER;
			src1w = 0;
		BINARY_IMM(0x7 << 3, 0x39, src2w, src1, src1w);
static int emit_test_binary(struct sljit_compiler *compiler,
		BINARY_EAX_IMM(0xa9, src2w);
	if (src2 == SLJIT_TEMPORARY_REG1 && (src2 & SLJIT_IMM) && (src1w > 127 || src1w < -128) && (compiler->mode32 || IS_HALFWORD(src1w))) {
	if (src2 == SLJIT_TEMPORARY_REG1 && (src1 & SLJIT_IMM) && (src1w > 127 || src1w < -128)) {
		BINARY_EAX_IMM(0xa9, src1w);
			if (IS_HALFWORD(src2w) || compiler->mode32) {
				code = emit_x86_instruction(compiler, 1, SLJIT_IMM, src2w, src1, 0);
				*code = 0xf7;
				FAIL_IF(emit_load_imm64(compiler, TMP_REG2, src2w));
				code = emit_x86_instruction(compiler, 1, TMP_REG2, 0, src1, 0);
				*code = 0x85;
			if (IS_HALFWORD(src1w) || compiler->mode32) {
				code = emit_x86_instruction(compiler, 1, SLJIT_IMM, src1w, src2, 0);
				FAIL_IF(emit_load_imm64(compiler, TMP_REG2, src1w));
				code = emit_x86_instruction(compiler, 1, TMP_REG2, 0, src2, 0);
			code = emit_x86_instruction(compiler, 1, src1, src1w, src2, 0);
			code = emit_x86_instruction(compiler, 1, SLJIT_IMM, src2w, TMP_REGISTER, 0);
			code = emit_x86_instruction(compiler, 1, TMP_REG2, 0, TMP_REGISTER, 0);
static int emit_shift(struct sljit_compiler *compiler,
	sljit_ub mode,
	if ((src2 & SLJIT_IMM) || (src2 == SLJIT_PREF_SHIFT_REG)) {
			code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, src2, src2w, dst, dstw);
			*code |= mode;
			code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, src2, src2w, TMP_REGISTER, 0);
		if (dst == SLJIT_PREF_SHIFT_REG && src2 == SLJIT_PREF_SHIFT_REG) {
			code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, SLJIT_PREF_SHIFT_REG, 0, TMP_REGISTER, 0);
			EMIT_MOV(compiler, SLJIT_PREF_SHIFT_REG, 0, TMP_REGISTER, 0);
			code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, src2, src2w, dst, 0);
	if (dst == SLJIT_PREF_SHIFT_REG) {
		EMIT_MOV(compiler, SLJIT_PREF_SHIFT_REG, 0, src2, src2w);
	else if (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS && dst != src2 && !ADDRESSING_DEPENDS_ON(src2, dst)) {
		if (src1 != dst)
		EMIT_MOV(compiler, TMP_REGISTER, 0, SLJIT_PREF_SHIFT_REG, 0);
		code = emit_x86_instruction(compiler, 1 | EX86_SHIFT_INS, SLJIT_PREF_SHIFT_REG, 0, dst, 0);
		/* This case is really difficult, since ecx itself may used for
		   addressing, and we must ensure to work even in that case. */
		EMIT_MOV(compiler, TMP_REG2, 0, SLJIT_PREF_SHIFT_REG, 0);
		/* [esp - 4] is reserved for eflags. */
		EMIT_MOV(compiler, SLJIT_MEM1(SLJIT_LOCALS_REG), -(int)(2 * sizeof(sljit_w)), SLJIT_PREF_SHIFT_REG, 0);
		EMIT_MOV(compiler, SLJIT_PREF_SHIFT_REG, 0, TMP_REG2, 0);
		EMIT_MOV(compiler, SLJIT_PREF_SHIFT_REG, 0, SLJIT_MEM1(SLJIT_LOCALS_REG), -(int)(2 * sizeof(sljit_w)));
static int emit_shift_with_flags(struct sljit_compiler *compiler,
	sljit_ub mode, int set_flags,
	/* The CPU does not set flags if the shift count is 0. */
		if ((src2w & 0x3f) != 0 || (compiler->mode32 && (src2w & 0x1f) != 0))
			return emit_shift(compiler, mode, dst, dstw, src1, src1w, src2, src2w);
		if ((src2w & 0x1f) != 0)
		if (!set_flags)
			return emit_mov(compiler, dst, dstw, src1, src1w);
		/* OR dst, src, 0 */
		return emit_cum_binary(compiler, 0x0b, 0x09, 0x1 << 3, 0x0d,
			dst, dstw, src1, src1w, SLJIT_IMM, 0);
	if (!(dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS))
		FAIL_IF(emit_cmp_binary(compiler, src1, src1w, SLJIT_IMM, 0));
	FAIL_IF(emit_shift(compiler,mode, dst, dstw, src1, src1w, src2, src2w));
	if (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS)
		return emit_cmp_binary(compiler, dst, dstw, SLJIT_IMM, 0);
	check_sljit_emit_op2(compiler, op, dst, dstw, src1, src1w, src2, src2w);
	CHECK_EXTRA_REGS(src1, src1w, (void)0);
	CHECK_EXTRA_REGS(src2, src2w, (void)0);
	if (GET_OPCODE(op) >= SLJIT_MUL) {
		else if (SLJIT_UNLIKELY(op & SLJIT_KEEP_FLAGS) && !compiler->flags_saved)
	case SLJIT_ADD:
		if (!GET_FLAGS(op)) {
			if (emit_lea_binary(compiler, dst, dstw, src1, src1w, src2, src2w) != SLJIT_ERR_UNSUPPORTED)
				return compiler->error;
		return emit_cum_binary(compiler, 0x03, 0x01, 0x0 << 3, 0x05,
			dst, dstw, src1, src1w, src2, src2w);
	case SLJIT_ADDC:
		if (SLJIT_UNLIKELY(compiler->flags_saved)) /* C flag must be restored. */
			FAIL_IF(emit_restore_flags(compiler, 1));
		else if (SLJIT_UNLIKELY(op & SLJIT_KEEP_FLAGS))
		return emit_cum_binary(compiler, 0x13, 0x11, 0x2 << 3, 0x15,
	case SLJIT_SUB:
			if ((src2 & SLJIT_IMM) && emit_lea_binary(compiler, dst, dstw, src1, src1w, SLJIT_IMM, -src2w) != SLJIT_ERR_UNSUPPORTED)
			return emit_cmp_binary(compiler, src1, src1w, src2, src2w);
	case SLJIT_SUBC:
		return emit_non_cum_binary(compiler, 0x1b, 0x19, 0x3 << 3, 0x1d,
	case SLJIT_MUL:
		return emit_mul(compiler, dst, dstw, src1, src1w, src2, src2w);
	case SLJIT_AND:
			return emit_test_binary(compiler, src1, src1w, src2, src2w);
		return emit_cum_binary(compiler, 0x23, 0x21, 0x4 << 3, 0x25,
	case SLJIT_OR:
	case SLJIT_XOR:
		return emit_cum_binary(compiler, 0x33, 0x31, 0x6 << 3, 0x35,
	case SLJIT_SHL:
		return emit_shift_with_flags(compiler, 0x4 << 3, GET_FLAGS(op),
	case SLJIT_LSHR:
		return emit_shift_with_flags(compiler, 0x5 << 3, GET_FLAGS(op),
	case SLJIT_ASHR:
		return emit_shift_with_flags(compiler, 0x7 << 3, GET_FLAGS(op),
	check_sljit_get_register_index(reg);
	if (reg == SLJIT_TEMPORARY_EREG1 || reg == SLJIT_TEMPORARY_EREG2
			|| reg == SLJIT_SAVED_EREG1 || reg == SLJIT_SAVED_EREG2)
	return reg_map[reg];
	check_sljit_emit_op_custom(compiler, instruction, size);
	SLJIT_ASSERT(size > 0 && size < 16);
	SLJIT_MEMMOVE(buf, instruction, size);
/*  Floating point operators                                             */
#if (defined SLJIT_SSE2_AUTO && SLJIT_SSE2_AUTO)
static int sse2_available = 0;
/* Alignment + 2 * 16 bytes. */
static sljit_i sse2_data[3 + 4 + 4];
static sljit_i *sse2_buffer;
static void init_compiler()
	int features = 0;
	sse2_buffer = (sljit_i*)(((sljit_uw)sse2_data + 15) & ~0xf);
	sse2_buffer[0] = 0;
	sse2_buffer[1] = 0x80000000;
	sse2_buffer[4] = 0xffffffff;
	sse2_buffer[5] = 0x7fffffff;
#ifdef __GNUC__
	/* AT&T syntax. */
	asm (
		"pushl %%ebx\n"
		"movl $0x1, %%eax\n"
		"cpuid\n"
		"popl %%ebx\n"
		"movl %%edx, %0\n"
		: "=g" (features)
		:
		: "%eax", "%ecx", "%edx"
#elif defined(_MSC_VER) || defined(__BORLANDC__)
	/* Intel syntax. */
	__asm {
		mov eax, 1
		push ebx
		cpuid
		pop ebx
		mov features, edx
	#error "SLJIT_SSE2_AUTO is not implemented for this C compiler"
	sse2_available = (features >> 26) & 0x1;
	/* Always available. */
static int emit_sse2(struct sljit_compiler *compiler, sljit_ub opcode,
	int xmm1, int xmm2, sljit_w xmm2w)
	buf = emit_x86_instruction(compiler, 2 | EX86_PREF_F2 | EX86_SSE2, xmm1, 0, xmm2, xmm2w);
	*buf++ = 0x0f;
	*buf = opcode;
static int emit_sse2_logic(struct sljit_compiler *compiler, sljit_ub opcode,
	buf = emit_x86_instruction(compiler, 2 | EX86_PREF_66 | EX86_SSE2, xmm1, 0, xmm2, xmm2w);
static SLJIT_INLINE int emit_sse2_load(struct sljit_compiler *compiler,
	int dst, int src, sljit_w srcw)
	return emit_sse2(compiler, 0x10, dst, src, srcw);
static SLJIT_INLINE int emit_sse2_store(struct sljit_compiler *compiler,
	int dst, sljit_w dstw, int src)
	return emit_sse2(compiler, 0x11, src, dst, dstw);
#if !(defined SLJIT_SSE2_AUTO && SLJIT_SSE2_AUTO)
static int sljit_emit_sse2_fop1(struct sljit_compiler *compiler, int op,
	check_sljit_emit_fop1(compiler, op, dst, dstw, src, srcw);
	if (GET_OPCODE(op) == SLJIT_FCMP) {
		if (dst >= SLJIT_FLOAT_REG1 && dst <= SLJIT_FLOAT_REG4)
			dst_r = TMP_FREG;
			FAIL_IF(emit_sse2_load(compiler, dst_r, dst, dstw));
		return emit_sse2_logic(compiler, 0x2e, dst_r, src, srcw);
	if (op == SLJIT_FMOV) {
			return emit_sse2_load(compiler, dst, src, srcw);
		if (src >= SLJIT_FLOAT_REG1 && src <= SLJIT_FLOAT_REG4)
			return emit_sse2_store(compiler, dst, dstw, src);
		FAIL_IF(emit_sse2_load(compiler, TMP_FREG, src, srcw));
		return emit_sse2_store(compiler, dst, dstw, TMP_FREG);
	if (dst >= SLJIT_FLOAT_REG1 && dst <= SLJIT_FLOAT_REG4) {
			FAIL_IF(emit_sse2_load(compiler, dst_r, src, srcw));
	case SLJIT_FNEG:
		FAIL_IF(emit_sse2_logic(compiler, 0x57, dst_r, SLJIT_MEM0(), (sljit_w)sse2_buffer));
	case SLJIT_FABS:
		FAIL_IF(emit_sse2_logic(compiler, 0x54, dst_r, SLJIT_MEM0(), (sljit_w)(sse2_buffer + 4)));
	if (dst_r == TMP_FREG)
static int sljit_emit_sse2_fop2(struct sljit_compiler *compiler, int op,
	check_sljit_emit_fop2(compiler, op, dst, dstw, src1, src1w, src2, src2w);
		if (dst == src1)
			; /* Do nothing here. */
		else if (dst == src2 && (op == SLJIT_FADD || op == SLJIT_FMUL)) {
			/* Swap arguments. */
			src2 = src1;
			src2w = src1w;
		else if (dst != src2)
			FAIL_IF(emit_sse2_load(compiler, dst_r, src1, src1w));
			FAIL_IF(emit_sse2_load(compiler, TMP_FREG, src1, src1w));
	case SLJIT_FADD:
		FAIL_IF(emit_sse2(compiler, 0x58, dst_r, src2, src2w));
	case SLJIT_FSUB:
		FAIL_IF(emit_sse2(compiler, 0x5c, dst_r, src2, src2w));
	case SLJIT_FMUL:
		FAIL_IF(emit_sse2(compiler, 0x59, dst_r, src2, src2w));
	case SLJIT_FDIV:
		FAIL_IF(emit_sse2(compiler, 0x5e, dst_r, src2, src2w));
#if (defined SLJIT_SSE2_AUTO && SLJIT_SSE2_AUTO) || !(defined SLJIT_SSE2 && SLJIT_SSE2)
static int emit_fld(struct sljit_compiler *compiler,
	if (src >= SLJIT_FLOAT_REG1 && src <= SLJIT_FLOAT_REG4) {
		*buf++ = 0xd9;
		*buf = 0xc0 + src - 1;
	*buf = 0xdd;
static int emit_fop(struct sljit_compiler *compiler,
	sljit_ub st_arg, sljit_ub st_arg2,
	sljit_ub m64fp_arg, sljit_ub m64fp_arg2,
		*buf++ = st_arg;
		*buf = st_arg2 + src;
	*buf++ = m64fp_arg;
	*buf |= m64fp_arg2;
static int emit_fop_regs(struct sljit_compiler *compiler,
	int src)
static int sljit_emit_fpu_fop1(struct sljit_compiler *compiler, int op,
#if !(defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64)
		FAIL_IF(emit_fld(compiler, dst, dstw));
		FAIL_IF(emit_fop(compiler, 0xd8, 0xd8, 0xdc, 0x3 << 3, src, srcw));
		/* Copy flags. */
		EMIT_MOV(compiler, TMP_REGISTER, 0, SLJIT_TEMPORARY_REG1, 0);
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 3);
		INC_SIZE(3);
		*buf++ = 0xdf;
		*buf++ = 0xe0;
		/* Note: lahf is not supported on all x86-64 architectures. */
		*buf++ = 0x9e;
		EMIT_MOV(compiler, SLJIT_TEMPORARY_REG1, 0, TMP_REGISTER, 0);
			FAIL_IF(emit_fop_regs(compiler, 0xdf, 0xe8, src));
			FAIL_IF(emit_fld(compiler, src, srcw));
			FAIL_IF(emit_fld(compiler, dst + ((dst >= SLJIT_FLOAT_REG1 && dst <= SLJIT_FLOAT_REG4) ? 1 : 0), dstw));
			FAIL_IF(emit_fop_regs(compiler, 0xdd, 0xd8, 0));
		FAIL_IF(emit_fop_regs(compiler, 0xd9, 0xe0, 0));
		FAIL_IF(emit_fop_regs(compiler, 0xd9, 0xe1, 0));
	FAIL_IF(emit_fop(compiler, 0xdd, 0xd8, 0xdd, 0x3 << 3, dst, dstw));
static int sljit_emit_fpu_fop2(struct sljit_compiler *compiler, int op,
	if (src1 >= SLJIT_FLOAT_REG1 && src1 <= SLJIT_FLOAT_REG4 && dst == src1) {
		FAIL_IF(emit_fld(compiler, src2, src2w));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xc0, src1));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xe8, src1));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xc8, src1));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xf8, src1));
	FAIL_IF(emit_fld(compiler, src1, src1w));
	if (src2 >= SLJIT_FLOAT_REG1 && src2 <= SLJIT_FLOAT_REG4 && dst == src2) {
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xc0, src2));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xe0, src2));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xc8, src2));
			FAIL_IF(emit_fop_regs(compiler, 0xde, 0xf0, src2));
		FAIL_IF(emit_fop(compiler, 0xd8, 0xc0, 0xdc, 0x0 << 3, src2, src2w));
		FAIL_IF(emit_fop(compiler, 0xd8, 0xe0, 0xdc, 0x4 << 3, src2, src2w));
		FAIL_IF(emit_fop(compiler, 0xd8, 0xc8, 0xdc, 0x1 << 3, src2, src2w));
		FAIL_IF(emit_fop(compiler, 0xd8, 0xf0, 0xdc, 0x6 << 3, src2, src2w));
	if (sse2_available)
		return sljit_emit_sse2_fop1(compiler, op, dst, dstw, src, srcw);
		return sljit_emit_fpu_fop1(compiler, op, dst, dstw, src, srcw);
		return sljit_emit_sse2_fop2(compiler, op, dst, dstw, src1, src1w, src2, src2w);
		return sljit_emit_fpu_fop2(compiler, op, dst, dstw, src1, src1w, src2, src2w);
/*  Conditional instructions                                             */
	check_sljit_emit_label(compiler);
	/* We should restore the flags before the label,
	   since other taken jumps has their own flags as well. */
	if (SLJIT_UNLIKELY(compiler->flags_saved))
		PTR_FAIL_IF(emit_restore_flags(compiler, 0));
	if (compiler->last_label && compiler->last_label->size == compiler->size)
		return compiler->last_label;
	label = (struct sljit_label*)ensure_abuf(compiler, sizeof(struct sljit_label));
	PTR_FAIL_IF(!label);
	set_label(label, compiler);
	buf = (sljit_ub*)ensure_buf(compiler, 2);
	*buf++ = 0;
	return label;
	check_sljit_emit_jump(compiler, type);
	if (SLJIT_UNLIKELY(compiler->flags_saved)) {
		if ((type & 0xff) <= SLJIT_JUMP)
	jump = (struct sljit_jump*)ensure_abuf(compiler, sizeof(struct sljit_jump));
	PTR_FAIL_IF_NULL(jump);
	set_jump(jump, compiler, type & SLJIT_REWRITABLE_JUMP);
	type &= 0xff;
	if (type >= SLJIT_CALL1)
		PTR_FAIL_IF(call_with_args(compiler, type));
	/* Worst case size. */
	compiler->size += (type >= SLJIT_JUMP) ? 5 : 6;
	compiler->size += (type >= SLJIT_JUMP) ? (10 + 3) : (2 + 10 + 3);
	PTR_FAIL_IF_NULL(buf);
	*buf++ = type + 4;
	return jump;
	check_sljit_emit_ijump(compiler, type, src, srcw);
		if (type <= SLJIT_JUMP)
			FAIL_IF(emit_restore_flags(compiler, 0));
	if (type >= SLJIT_CALL1) {
		if (src == SLJIT_TEMPORARY_REG3) {
		if ((src & SLJIT_MEM) && (src & 0xf) == SLJIT_LOCALS_REG && type >= SLJIT_CALL3) {
			if (src & 0xf0) {
				srcw += sizeof(sljit_w);
		if ((src & SLJIT_MEM) && (src & 0xf) == SLJIT_LOCALS_REG) {
				srcw += sizeof(sljit_w) * (type - SLJIT_CALL0);
#if (defined SLJIT_CONFIG_X86_64 && SLJIT_CONFIG_X86_64) && defined(_WIN64)
		FAIL_IF(call_with_args(compiler, type));
	if (src == SLJIT_IMM) {
		FAIL_IF_NULL(jump);
		set_jump(jump, compiler, JUMP_ADDR);
		jump->u.target = srcw;
		compiler->size += 5;
		compiler->size += 10 + 3;
		code = (sljit_ub*)ensure_buf(compiler, 2);
		FAIL_IF_NULL(code);
		*code++ = type + 4;
		code = emit_x86_instruction(compiler, 1, 0, 0, src, srcw);
		*code++ = 0xff;
		*code |= (type >= SLJIT_FAST_CALL) ? (2 << 3) : (4 << 3);
	sljit_ub cond_set = 0;
	int reg;
	check_sljit_emit_cond_value(compiler, op, dst, dstw, type);
		cond_set = 0x94;
		cond_set = 0x95;
		cond_set = 0x92;
		cond_set = 0x93;
		cond_set = 0x97;
		cond_set = 0x96;
		cond_set = 0x9c;
		cond_set = 0x9d;
		cond_set = 0x9f;
		cond_set = 0x9e;
		cond_set = 0x90;
		cond_set = 0x91;
		cond_set = 0x9a;
		cond_set = 0x9b;
	reg = (op == SLJIT_MOV && dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) ? dst : TMP_REGISTER;
	buf = (sljit_ub*)ensure_buf(compiler, 1 + 4 + 4);
	INC_SIZE(4 + 4);
	/* Set low register to conditional flag. */
	*buf++ = (reg_map[reg] <= 7) ? 0x40 : REX_B;
	*buf++ = cond_set;
	*buf++ = 0xC0 | reg_lmap[reg];
	*buf++ = REX_W | (reg_map[reg] <= 7 ? 0 : (REX_B | REX_R));
	*buf++ = 0xb6;
	*buf = 0xC0 | (reg_lmap[reg] << 3) | reg_lmap[reg];
	if (reg == TMP_REGISTER) {
		if (op == SLJIT_MOV) {
			return sljit_emit_op2(compiler, op, dst, dstw, dst, dstw, TMP_REGISTER, 0);
		if (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_TEMPORARY_REG3) {
			buf = (sljit_ub*)ensure_buf(compiler, 1 + 3 + 3);
			INC_SIZE(3 + 3);
			/* Set low byte to conditional flag. */
			*buf++ = 0xC0 | reg_map[dst];
			*buf = 0xC0 | (reg_map[dst] << 3) | reg_map[dst];
			/* Set al to conditional flag. */
			*buf++ = 0xC0;
			if (dst >= SLJIT_SAVED_REG1 && dst <= SLJIT_NO_REGISTERS)
				*buf = 0xC0 | (reg_map[dst] << 3);
				*buf = 0xC0;
				EMIT_MOV(compiler, dst, dstw, SLJIT_TEMPORARY_REG1, 0);
			EMIT_MOV(compiler, TMP_REGISTER, 0, dst, 0);
			buf = (sljit_ub*)ensure_buf(compiler, 1 + 3 + 3 + 1);
			INC_SIZE(3 + 3 + 1);
			*buf++ = 0x90 + reg_map[TMP_REGISTER];
SLJIT_API_FUNC_ATTRIBUTE struct sljit_const* sljit_emit_const(struct sljit_compiler *compiler, int dst, sljit_w dstw, sljit_w init_value)
	check_sljit_emit_const(compiler, dst, dstw, init_value);
	const_ = (struct sljit_const*)ensure_abuf(compiler, sizeof(struct sljit_const));
	PTR_FAIL_IF(!const_);
	set_const(const_, compiler);
	reg = (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) ? dst : TMP_REGISTER;
	if (emit_load_imm64(compiler, reg, init_value))
	if (emit_mov(compiler, dst, dstw, SLJIT_IMM, init_value))
	*buf++ = 1;
	if (reg == TMP_REGISTER && dst != SLJIT_UNUSED)
		if (emit_mov(compiler, dst, dstw, TMP_REGISTER, 0))
	return const_;
	*(sljit_w*)addr = new_addr - (addr + 4);
	*(sljit_uw*)addr = new_addr;
	*(sljit_w*)addr = new_constant;
