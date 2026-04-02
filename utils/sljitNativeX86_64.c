/* x86 64-bit arch dependent functions. */
static int emit_load_imm64(struct sljit_compiler *compiler, int reg, sljit_w imm)
	buf = (sljit_ub*)ensure_buf(compiler, 1 + 2 + sizeof(sljit_w));
	INC_SIZE(2 + sizeof(sljit_w));
	*buf++ = REX_W | ((reg_map[reg] <= 7) ? 0 : REX_B);
	*buf++ = 0xb8 + (reg_map[reg] & 0x7);
	if (type < SLJIT_JUMP) {
		*code_ptr++ = get_jump_code(type ^ 0x1) - 0x10;
		*code_ptr++ = 10 + 3;
	SLJIT_COMPILE_ASSERT(reg_map[TMP_REG3] == 9, tmp3_is_9_first);
	*code_ptr++ = REX_W | REX_B;
	*code_ptr++ = 0xb8 + 1;
	jump->addr = (sljit_uw)code_ptr;
		jump->flags |= PATCH_MD;
		*(sljit_w*)code_ptr = jump->u.target;
	code_ptr += sizeof(sljit_w);
	*code_ptr++ = REX_B;
	*code_ptr++ = 0xff;
	*code_ptr++ = (type >= SLJIT_FAST_CALL) ? 0xd1 /* call */ : 0xe1 /* jmp */;
static sljit_ub* generate_fixed_jump(sljit_ub *code_ptr, sljit_w addr, int type)
	sljit_w delta = addr - ((sljit_w)code_ptr + 1 + sizeof(sljit_hw));
	if (delta <= SLJIT_W(0x7fffffff) && delta >= SLJIT_W(-0x80000000)) {
		*code_ptr++ = (type == 2) ? 0xe8 /* call */ : 0xe9 /* jmp */;
		*(sljit_w*)code_ptr = delta;
		SLJIT_COMPILE_ASSERT(reg_map[TMP_REG3] == 9, tmp3_is_9_second);
		*(sljit_w*)code_ptr = addr;
		*code_ptr++ = (type == 2) ? 0xd1 /* call */ : 0xe1 /* jmp */;
	int size, pushed_size;
	size = saveds;
	/* Including the return address saved by the call instruction. */
	pushed_size = (saveds + 1) * sizeof(sljit_w);
	if (saveds >= 2)
		size += saveds - 1;
	/* Saving the virtual stack pointer. */
	compiler->has_locals = local_size > 0;
	if (local_size > 0) {
		pushed_size += sizeof(sljit_w);
	if (saveds >= 4)
		size += saveds - 3;
	if (temporaries >= 5) {
		size += (5 - 4) * 2;
	size += args * 3;
	if (size > 0) {
		if (saveds >= 5) {
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_EREG2] >= 8, saved_ereg2_is_hireg);
			*buf++ = REX_B;
			PUSH_REG(reg_lmap[SLJIT_SAVED_EREG2]);
		if (saveds >= 4) {
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_EREG1] >= 8, saved_ereg1_is_hireg);
			PUSH_REG(reg_lmap[SLJIT_SAVED_EREG1]);
		if (saveds >= 3) {
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_REG3] >= 8, saved_reg3_is_hireg);
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_REG3] < 8, saved_reg3_is_loreg);
			PUSH_REG(reg_lmap[SLJIT_SAVED_REG3]);
		if (saveds >= 2) {
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_REG2] >= 8, saved_reg2_is_hireg);
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_REG2] < 8, saved_reg2_is_loreg);
			PUSH_REG(reg_lmap[SLJIT_SAVED_REG2]);
		if (saveds >= 1) {
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_SAVED_REG1] < 8, saved_reg1_is_loreg);
			PUSH_REG(reg_lmap[SLJIT_SAVED_REG1]);
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_TEMPORARY_EREG2] >= 8, temporary_ereg2_is_hireg);
			PUSH_REG(reg_lmap[SLJIT_TEMPORARY_EREG2]);
			SLJIT_COMPILE_ASSERT(reg_map[SLJIT_LOCALS_REG] >= 8, locals_reg_is_hireg);
			PUSH_REG(reg_lmap[SLJIT_LOCALS_REG]);
			*buf++ = REX_W;
			*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG1] << 3) | 0x7;
			*buf++ = REX_W | REX_R;
			*buf++ = 0xc0 | (reg_lmap[SLJIT_SAVED_REG2] << 3) | 0x6;
			*buf++ = 0xc0 | (reg_lmap[SLJIT_SAVED_REG3] << 3) | 0x2;
			*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG1] << 3) | 0x1;
			*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG2] << 3) | 0x2;
			*buf++ = REX_W | REX_B;
			*buf++ = 0xc0 | (reg_map[SLJIT_SAVED_REG3] << 3) | 0x0;
	local_size = ((local_size + pushed_size + 16 - 1) & ~(16 - 1)) - pushed_size;
	local_size += 4 * sizeof(sljit_w);
		/* Allocate the stack for the function itself. */
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 4);
		INC_SIZE(4);
		*buf++ = 0x83;
		*buf++ = 0xc0 | (5 << 3) | 4;
		/* Pushed size must be divisible by 8. */
		SLJIT_ASSERT(!(pushed_size & 0x7));
		if (pushed_size & 0x8) {
			*buf++ = 5 * sizeof(sljit_w);
			local_size -= 5 * sizeof(sljit_w);
		} else {
			*buf++ = 4 * sizeof(sljit_w);
			local_size -= 4 * sizeof(sljit_w);
		FAIL_IF(emit_load_imm64(compiler, SLJIT_TEMPORARY_REG1, local_size));
		/* In case of Win64, local_size is always > 4 * sizeof(sljit_w) */
		if (local_size <= 127) {
			*buf++ = local_size;
			buf = (sljit_ub*)ensure_buf(compiler, 1 + 7);
			INC_SIZE(7);
			*buf++ = 0x81;
			*(sljit_hw*)buf = local_size;
			buf += sizeof(sljit_hw);
	if (compiler->has_locals) {
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 5);
		INC_SIZE(5);
		*buf++ = 0x8d;
		*buf++ = 0x40 | (reg_lmap[SLJIT_LOCALS_REG] << 3) | 0x4;
		*buf = 4 * sizeof(sljit_w);
	int pushed_size;
	if (temporaries >= 5)
	compiler->local_size = ((local_size + pushed_size + 16 - 1) & ~(16 - 1)) - pushed_size;
	compiler->local_size += 4 * sizeof(sljit_w);
	if (compiler->local_size > 0) {
		if (compiler->local_size <= 127) {
			*buf++ = 0xc0 | (0 << 3) | 4;
			*buf = compiler->local_size;
			*(sljit_hw*)buf = compiler->local_size;
	size = 1 + compiler->saveds;
	if (compiler->saveds >= 2)
		size += compiler->saveds - 1;
	if (compiler->has_locals)
	if (compiler->saveds >= 4)
		size += compiler->saveds - 3;
	if (compiler->temporaries >= 5)
		POP_REG(reg_lmap[SLJIT_LOCALS_REG]);
	if (compiler->temporaries >= 5) {
		POP_REG(reg_lmap[SLJIT_TEMPORARY_EREG2]);
	if (compiler->saveds >= 1)
	if (compiler->saveds >= 2) {
		POP_REG(reg_lmap[SLJIT_SAVED_REG2]);
	if (compiler->saveds >= 3) {
		POP_REG(reg_lmap[SLJIT_SAVED_REG3]);
	if (compiler->saveds >= 4) {
		POP_REG(reg_lmap[SLJIT_SAVED_EREG1]);
	if (compiler->saveds >= 5) {
		POP_REG(reg_lmap[SLJIT_SAVED_EREG2]);
static int emit_do_imm32(struct sljit_compiler *compiler, sljit_ub rex, sljit_ub opcode, sljit_w imm)
	if (rex != 0) {
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 2 + sizeof(sljit_hw));
		INC_SIZE(2 + sizeof(sljit_hw));
		*buf++ = rex;
		*(sljit_hw*)buf = (sljit_hw)imm;
		buf = (sljit_ub*)ensure_buf(compiler, 1 + 1 + sizeof(sljit_hw));
		INC_SIZE(1 + sizeof(sljit_hw));
	sljit_ub rex = 0;
	/* The immediate operand must be 32 bit. */
	SLJIT_ASSERT(!(a & SLJIT_IMM) || compiler->mode32 || IS_HALFWORD(imma));
	if ((b & SLJIT_MEM) && !(b & 0xf0) && NOT_HALFWORD(immb)) {
		if (emit_load_imm64(compiler, TMP_REG3, immb))
		immb = 0;
		if (b & 0xf)
			b |= TMP_REG3 << 4;
			b |= TMP_REG3;
	if (!compiler->mode32 && !(flags & EX86_NO_REXW))
		rex |= REX_W;
	else if (flags & EX86_REX)
		rex |= REX;
			inst_size += 1 + sizeof(sljit_hw); /* SIB byte required to avoid RIP based addressing. */
			if (reg_map[b & 0x0f] >= 8)
				rex |= REX_B;
			if (immb != 0 && !(b & 0xf0)) {
					inst_size += sizeof(sljit_hw);
		if ((b & 0xf) == SLJIT_LOCALS_REG && (b & 0xf0) == 0)
		if ((b & 0xf0) != SLJIT_UNUSED) {
			if (reg_map[(b >> 4) & 0x0f] >= 8)
				rex |= REX_X;
	else if (!(flags & EX86_SSE2) && reg_map[b] >= 8)
	else if (reg_map[b] >= 8)
			imma &= compiler->mode32 ? 0x1f : 0x3f;
		/* reg_map[SLJIT_PREF_SHIFT_REG] is less than 8. */
		if (!(flags & EX86_SSE2) && reg_map[a] >= 8)
			rex |= REX_R;
		if (reg_map[a] >= 8)
	if (rex)
			*buf_ptr = reg_lmap[a] << 3;
		*buf_ptr++ |= 0xc0 + ((!(flags & EX86_SSE2)) ? reg_lmap[b] : b);
		*buf_ptr++ |= 0xc0 + reg_lmap[b];
		SLJIT_ASSERT((b & 0xf0) != (SLJIT_LOCALS_REG << 4));
				*buf_ptr++ |= reg_lmap[b & 0x0f];
				*buf_ptr++ = reg_lmap[b & 0x0f] | (reg_lmap[(b >> 4) & 0x0f] << 3);
					*buf_ptr++ = (sljit_ub)immb; /* 8 bit displacement. */
					*(sljit_hw*)buf_ptr = (sljit_hw)immb; /* 32 bit displacement. */
					buf_ptr += sizeof(sljit_hw);
			*buf_ptr++ = (sljit_ub)(reg_lmap[b & 0x0f] | (reg_lmap[(b >> 4) & 0x0f] << 3) | (immb << 6));
		*buf_ptr++ = 0x25;
			*buf_ptr = (sljit_ub)imma;
			*(short*)buf_ptr = (short)imma;
			*(sljit_hw*)buf_ptr = (sljit_hw)imma;
	SLJIT_COMPILE_ASSERT(reg_map[SLJIT_TEMPORARY_REG2] == 6 && reg_map[SLJIT_TEMPORARY_REG1] < 8 && reg_map[SLJIT_TEMPORARY_REG3] < 8, args_registers);
	buf = (sljit_ub*)ensure_buf(compiler, 1 + ((type < SLJIT_CALL3) ? 3 : 6));
	INC_SIZE((type < SLJIT_CALL3) ? 3 : 6);
	if (type >= SLJIT_CALL3) {
		*buf++ = 0xc0 | (0x2 << 3) | reg_lmap[SLJIT_TEMPORARY_REG3];
	*buf++ = 0xc0 | (0x7 << 3) | reg_lmap[SLJIT_TEMPORARY_REG1];
	SLJIT_COMPILE_ASSERT(reg_map[SLJIT_TEMPORARY_REG2] == 2 && reg_map[SLJIT_TEMPORARY_REG1] < 8 && reg_map[SLJIT_TEMPORARY_REG3] < 8, args_registers);
		*buf++ = 0xc0 | (0x0 << 3) | reg_lmap[SLJIT_TEMPORARY_REG3];
	*buf++ = 0xc0 | (0x1 << 3) | reg_lmap[SLJIT_TEMPORARY_REG1];
	if (dst == SLJIT_UNUSED)
		dst = TMP_REGISTER;
	if (dst >= SLJIT_TEMPORARY_REG1 && dst <= TMP_REGISTER) {
		if (reg_map[dst] < 8) {
			POP_REG(reg_lmap[dst]);
			buf = (sljit_ub*)ensure_buf(compiler, 1 + 2);
			INC_SIZE(2);
		/* REX_W is not necessary (src is not immediate). */
		compiler->mode32 = 1;
	if ((src & SLJIT_IMM) && NOT_HALFWORD(srcw)) {
		FAIL_IF(emit_load_imm64(compiler, TMP_REGISTER, srcw));
		src = TMP_REGISTER;
	if (src >= SLJIT_TEMPORARY_REG1 && src <= TMP_REGISTER) {
		if (reg_map[src] < 8) {
			PUSH_REG(reg_lmap[src]);
			buf = (sljit_ub*)ensure_buf(compiler, 1 + 2 + 1);
			INC_SIZE(2 + 1);
		SLJIT_ASSERT(IS_HALFWORD(srcw));
		*(sljit_hw*)buf = (sljit_hw)srcw;
/*  Extend input                                                         */
static int emit_mov_int(struct sljit_compiler *compiler, int sign,
	sljit_ub* code;
	int dst_r;
	compiler->mode32 = 0;
	if (dst == SLJIT_UNUSED && !(src & SLJIT_MEM))
		return SLJIT_SUCCESS; /* Empty instruction. */
	if (src & SLJIT_IMM) {
			if (sign || ((sljit_uw)srcw <= 0x7fffffff)) {
				code = emit_x86_instruction(compiler, 1, SLJIT_IMM, (sljit_w)(sljit_i)srcw, dst, dstw);
				FAIL_IF(!code);
				*code = 0xc7;
			return emit_load_imm64(compiler, dst, srcw);
	dst_r = (dst >= SLJIT_TEMPORARY_REG1 && dst <= SLJIT_SAVED_REG3) ? dst : TMP_REGISTER;
	if ((dst & SLJIT_MEM) && (src >= SLJIT_TEMPORARY_REG1 && src <= SLJIT_SAVED_REG3))
		dst_r = src;
		if (sign) {
			code = emit_x86_instruction(compiler, 1, dst_r, 0, src, srcw);
			*code++ = 0x63;
			FAIL_IF(emit_mov(compiler, dst_r, 0, src, srcw));
	if (dst & SLJIT_MEM) {
		code = emit_x86_instruction(compiler, 1, dst_r, 0, dst, dstw);
		*code = 0x89;
