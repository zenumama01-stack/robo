/* x86 32-bit arch dependent functions. */
static int emit_do_imm(struct sljit_compiler *compiler, sljit_ub opcode, sljit_w imm)
	sljit_ub *buf;
	buf = (sljit_ub*)ensure_buf(compiler, 1 + 1 + sizeof(sljit_w));
	FAIL_IF(!buf);
	INC_SIZE(1 + sizeof(sljit_w));
	*buf++ = opcode;
	*(sljit_w*)buf = imm;
static sljit_ub* generate_far_jump_code(struct sljit_jump *jump, sljit_ub *code_ptr, int type)
	if (type == SLJIT_JUMP) {
		*code_ptr++ = 0xe9;
		jump->addr++;
	else if (type >= SLJIT_FAST_CALL) {
		*code_ptr++ = 0xe8;
		*code_ptr++ = 0x0f;
		*code_ptr++ = get_jump_code(type);
		jump->addr += 2;
	if (jump->flags & JUMP_LABEL)
		jump->flags |= PATCH_MW;
		*(sljit_w*)code_ptr = jump->u.target - (jump->addr + 4);
	code_ptr += 4;
	return code_ptr;
	CHECK_ERROR();
	check_sljit_emit_enter(compiler, args, temporaries, saveds, local_size);
	compiler->args = args;
	compiler->flags_saved = 0;
#if (defined SLJIT_X86_32_FASTCALL && SLJIT_X86_32_FASTCALL)
	size = 1 + (saveds <= 3 ? saveds : 3) + (args > 0 ? (args * 2) : 0) + (args > 2 ? 2 : 0);
	size = 1 + (saveds <= 3 ? saveds : 3) + (args > 0 ? (2 + args * 3) : 0);
	buf = (sljit_ub*)ensure_buf(compiler, 1 + size);
	INC_SIZE(size);
	PUSH_REG(reg_map[TMP_REGISTER]);
#if !(defined SLJIT_X86_32_FASTCALL && SLJIT_X86_32_FASTCALL)
	if (args > 0) {
		*buf++ = 0x8b;
		*buf++ = 0xc4 | (reg_map[TMP_REGISTER] << 3);
	if (saveds > 2)
		PUSH_REG(reg_map[SLJIT_SAVED_REG3]);
	if (saveds > 1)
		PUSH_REG(reg_map[SLJIT_SAVED_REG2]);
	if (saveds > 0)
		PUSH_REG(reg_map[SLJIT_SAVED_REG1]);
		*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG1] << 3) | reg_map[SLJIT_TEMPORARY_REG3];
	if (args > 1) {
		*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG2] << 3) | reg_map[SLJIT_TEMPORARY_REG2];
	if (args > 2) {
		*buf++ = 0x44 | (reg_map[SLJIT_SAVED_REG3] << 3);
		*buf++ = 0x24;
		*buf++ = sizeof(sljit_w) * (3 + 2); /* saveds >= 3 as well. */
		*buf++ = 0x40 | (reg_map[SLJIT_SAVED_REG1] << 3) | reg_map[TMP_REGISTER];
		*buf++ = sizeof(sljit_w) * 2;
		*buf++ = 0x40 | (reg_map[SLJIT_SAVED_REG2] << 3) | reg_map[TMP_REGISTER];
		*buf++ = sizeof(sljit_w) * 3;
		*buf++ = 0x40 | (reg_map[SLJIT_SAVED_REG3] << 3) | reg_map[TMP_REGISTER];
		*buf++ = sizeof(sljit_w) * 4;
	local_size = (local_size + sizeof(sljit_uw) - 1) & ~(sizeof(sljit_uw) - 1);
	compiler->temporaries_start = local_size;
	if (temporaries > 3)
		local_size += (temporaries - 3) * sizeof(sljit_uw);
	compiler->saveds_start = local_size;
	if (saveds > 3)
		local_size += (saveds - 3) * sizeof(sljit_uw);
	if (local_size > 1024) {
		FAIL_IF(emit_do_imm(compiler, 0xb8 + reg_map[SLJIT_TEMPORARY_REG1], local_size));
		FAIL_IF(sljit_emit_ijump(compiler, SLJIT_CALL1, SLJIT_IMM, SLJIT_FUNC_OFFSET(sljit_touch_stack)));
	compiler->local_size = local_size;
	if (local_size > 0)
		return emit_non_cum_binary(compiler, 0x2b, 0x29, 0x5 << 3, 0x2d,
			SLJIT_LOCALS_REG, 0, SLJIT_LOCALS_REG, 0, SLJIT_IMM, local_size);
	CHECK_ERROR_VOID();
	check_sljit_set_context(compiler, args, temporaries, saveds, local_size);
	compiler->local_size = (local_size + sizeof(sljit_uw) - 1) & ~(sizeof(sljit_uw) - 1);
	compiler->temporaries_start = compiler->local_size;
		compiler->local_size += (temporaries - 3) * sizeof(sljit_uw);
	compiler->saveds_start = compiler->local_size;
		compiler->local_size += (saveds - 3) * sizeof(sljit_uw);
	check_sljit_emit_return(compiler, op, src, srcw);
	SLJIT_ASSERT(compiler->args >= 0);
	FAIL_IF(emit_mov_before_return(compiler, op, src, srcw));
	if (compiler->local_size > 0)
		FAIL_IF(emit_cum_binary(compiler, 0x03, 0x01, 0x0 << 3, 0x05,
				SLJIT_LOCALS_REG, 0, SLJIT_LOCALS_REG, 0, SLJIT_IMM, compiler->local_size));
	size = 2 + (compiler->saveds <= 3 ? compiler->saveds : 3);
	if (compiler->args > 2)
		size += 2;
	if (compiler->args > 0)
	if (compiler->saveds > 0)
		POP_REG(reg_map[SLJIT_SAVED_REG1]);
	if (compiler->saveds > 1)
		POP_REG(reg_map[SLJIT_SAVED_REG2]);
	if (compiler->saveds > 2)
		POP_REG(reg_map[SLJIT_SAVED_REG3]);
	POP_REG(reg_map[TMP_REGISTER]);
		RETN(sizeof(sljit_w));
		RET();
		RETN(compiler->args * sizeof(sljit_w));
/*  Operators                                                            */
/* Size contains the flags as well. */
static sljit_ub* emit_x86_instruction(struct sljit_compiler *compiler, int size,
	/* The register or immediate operand. */
	int a, sljit_w imma,
	/* The general operand (not immediate). */
	int b, sljit_w immb)
	sljit_ub *buf_ptr;
	int flags = size & ~0xf;
	int inst_size;
	/* Both cannot be switched on. */
	SLJIT_ASSERT((flags & (EX86_BIN_INS | EX86_SHIFT_INS)) != (EX86_BIN_INS | EX86_SHIFT_INS));
	/* Size flags not allowed for typed instructions. */
	SLJIT_ASSERT(!(flags & (EX86_BIN_INS | EX86_SHIFT_INS)) || (flags & (EX86_BYTE_ARG | EX86_HALF_ARG)) == 0);
	/* Both size flags cannot be switched on. */
	SLJIT_ASSERT((flags & (EX86_BYTE_ARG | EX86_HALF_ARG)) != (EX86_BYTE_ARG | EX86_HALF_ARG));
#if (defined SLJIT_SSE2 && SLJIT_SSE2)
	/* SSE2 and immediate is not possible. */
	SLJIT_ASSERT(!(a & SLJIT_IMM) || !(flags & EX86_SSE2));
	size &= 0xf;
	inst_size = size;
	if (flags & EX86_PREF_F2)
		inst_size++;
	if (flags & EX86_PREF_66)
	/* Calculate size of b. */
	inst_size += 1; /* mod r/m byte. */
	if (b & SLJIT_MEM) {
		if ((b & 0x0f) == SLJIT_UNUSED)
			inst_size += sizeof(sljit_w);
		else if (immb != 0 && !(b & 0xf0)) {
			/* Immediate operand. */
			if (immb <= 127 && immb >= -128)
				inst_size += sizeof(sljit_b);
		if ((b & 0xf) == SLJIT_LOCALS_REG && !(b & 0xf0))
			b |= SLJIT_LOCALS_REG << 4;
		if ((b & 0xf0) != SLJIT_UNUSED)
			inst_size += 1; /* SIB byte. */
	/* Calculate size of a. */
	if (a & SLJIT_IMM) {
		if (flags & EX86_BIN_INS) {
			if (imma <= 127 && imma >= -128) {
				inst_size += 1;
				flags |= EX86_BYTE_ARG;
			} else
				inst_size += 4;
		else if (flags & EX86_SHIFT_INS) {
			imma &= 0x1f;
			if (imma != 1) {
				inst_size ++;
		} else if (flags & EX86_BYTE_ARG)
		else if (flags & EX86_HALF_ARG)
			inst_size += sizeof(short);
		SLJIT_ASSERT(!(flags & EX86_SHIFT_INS) || a == SLJIT_PREF_SHIFT_REG);
	buf = (sljit_ub*)ensure_buf(compiler, 1 + inst_size);
	PTR_FAIL_IF(!buf);
	/* Encoding the byte. */
	INC_SIZE(inst_size);
		*buf++ = 0xf2;
		*buf++ = 0x66;
	buf_ptr = buf + size;
	/* Encode mod/rm byte. */
	if (!(flags & EX86_SHIFT_INS)) {
		if ((flags & EX86_BIN_INS) && (a & SLJIT_IMM))
			*buf = (flags & EX86_BYTE_ARG) ? 0x83 : 0x81;
		if ((a & SLJIT_IMM) || (a == 0))
			*buf_ptr = 0;
		else if (!(flags & EX86_SSE2))
			*buf_ptr = reg_map[a] << 3;
			*buf_ptr = a << 3;
			if (imma == 1)
				*buf = 0xd1;
				*buf = 0xc1;
			*buf = 0xd3;
	if (!(b & SLJIT_MEM))
		*buf_ptr++ |= 0xc0 + ((!(flags & EX86_SSE2)) ? reg_map[b] : b);
		*buf_ptr++ |= 0xc0 + reg_map[b];
	else if ((b & 0x0f) != SLJIT_UNUSED) {
		if ((b & 0xf0) == SLJIT_UNUSED || (b & 0xf0) == (SLJIT_LOCALS_REG << 4)) {
			if (immb != 0) {
					*buf_ptr |= 0x40;
					*buf_ptr |= 0x80;
			if ((b & 0xf0) == SLJIT_UNUSED)
				*buf_ptr++ |= reg_map[b & 0x0f];
				*buf_ptr++ |= 0x04;
				*buf_ptr++ = reg_map[b & 0x0f] | (reg_map[(b >> 4) & 0x0f] << 3);
					*buf_ptr++ = immb; /* 8 bit displacement. */
					*(sljit_w*)buf_ptr = immb; /* 32 bit displacement. */
					buf_ptr += sizeof(sljit_w);
			*buf_ptr++ = reg_map[b & 0x0f] | (reg_map[(b >> 4) & 0x0f] << 3) | (immb << 6);
		*buf_ptr++ |= 0x05;
		if (flags & EX86_BYTE_ARG)
			*buf_ptr = imma;
			*(short*)buf_ptr = imma;
		else if (!(flags & EX86_SHIFT_INS))
			*(sljit_w*)buf_ptr = imma;
	return !(flags & EX86_SHIFT_INS) ? buf : (buf + 1);
/*  Call / return instructions                                           */
static SLJIT_INLINE int call_with_args(struct sljit_compiler *compiler, int type)
	buf = (sljit_ub*)ensure_buf(compiler, type >= SLJIT_CALL3 ? 1 + 2 + 1 : 1 + 2);
	INC_SIZE(type >= SLJIT_CALL3 ? 2 + 1 : 2);
	if (type >= SLJIT_CALL3)
		PUSH_REG(reg_map[SLJIT_TEMPORARY_REG3]);
	*buf++ = 0xc0 | (reg_map[SLJIT_TEMPORARY_REG3] << 3) | reg_map[SLJIT_TEMPORARY_REG1];
	buf = (sljit_ub*)ensure_buf(compiler, type - SLJIT_CALL0 + 1);
	INC_SIZE(type - SLJIT_CALL0);
	if (type >= SLJIT_CALL2)
		PUSH_REG(reg_map[SLJIT_TEMPORARY_REG2]);
	PUSH_REG(reg_map[SLJIT_TEMPORARY_REG1]);
	check_sljit_emit_fast_enter(compiler, dst, dstw, args, temporaries, saveds, local_size);
	CHECK_EXTRA_REGS(dst, dstw, (void)0);
	if (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_NO_REGISTERS) {
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 1);
		INC_SIZE(1);
		POP_REG(reg_map[dst]);
	else if (dst & SLJIT_MEM) {
		buf = emit_x86_instruction(compiler, 1, 0, 0, dst, dstw);
		*buf++ = 0x8f;
	/* For UNUSED dst. Uncommon, but possible. */
	check_sljit_emit_fast_return(compiler, src, srcw);
	CHECK_EXTRA_REGS(src, srcw, (void)0);
	if (src >= SLJIT_TEMPORARY_REG1 && src <= SLJIT_NO_REGISTERS) {
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 1 + 1);
		INC_SIZE(1 + 1);
		PUSH_REG(reg_map[src]);
	else if (src & SLJIT_MEM) {
		buf = emit_x86_instruction(compiler, 1, 0, 0, src, srcw);
		*buf++ = 0xff;
		*buf |= 6 << 3;
		/* SLJIT_IMM. */
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 5 + 1);
		INC_SIZE(5 + 1);
		*buf++ = 0x68;
		*(sljit_w*)buf = srcw;
		buf += sizeof(sljit_w);
