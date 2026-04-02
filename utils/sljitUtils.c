/* ------------------------------------------------------------------------ */
/*  Locks                                                                   */
#if (defined SLJIT_EXECUTABLE_ALLOCATOR && SLJIT_EXECUTABLE_ALLOCATOR) || (defined SLJIT_UTIL_GLOBAL_LOCK && SLJIT_UTIL_GLOBAL_LOCK)
#include "windows.h"
static HANDLE allocator_mutex = 0;
static SLJIT_INLINE void allocator_grab_lock(void)
	/* No idea what to do if an error occures. Static mutexes should never fail... */
	if (!allocator_mutex)
		allocator_mutex = CreateMutex(NULL, TRUE, NULL);
		WaitForSingleObject(allocator_mutex, INFINITE);
static SLJIT_INLINE void allocator_release_lock(void)
	ReleaseMutex(allocator_mutex);
#endif /* SLJIT_EXECUTABLE_ALLOCATOR */
#if (defined SLJIT_UTIL_GLOBAL_LOCK && SLJIT_UTIL_GLOBAL_LOCK)
static HANDLE global_mutex = 0;
SLJIT_API_FUNC_ATTRIBUTE void SLJIT_CALL sljit_grab_lock(void)
	if (!global_mutex)
		global_mutex = CreateMutex(NULL, TRUE, NULL);
		WaitForSingleObject(global_mutex, INFINITE);
SLJIT_API_FUNC_ATTRIBUTE void SLJIT_CALL sljit_release_lock(void)
	ReleaseMutex(global_mutex);
#endif /* SLJIT_UTIL_GLOBAL_LOCK */
#else /* _WIN32 */
#include "pthread.h"
static pthread_mutex_t allocator_mutex = PTHREAD_MUTEX_INITIALIZER;
	pthread_mutex_lock(&allocator_mutex);
	pthread_mutex_unlock(&allocator_mutex);
static pthread_mutex_t global_mutex = PTHREAD_MUTEX_INITIALIZER;
	pthread_mutex_lock(&global_mutex);
	pthread_mutex_unlock(&global_mutex);
#endif /* _WIN32 */
/*  Stack                                                                   */
#if (defined SLJIT_UTIL_STACK && SLJIT_UTIL_STACK)
#include <unistd.h>
/* Planning to make it even more clever in the future. */
static sljit_w sljit_page_align = 0;
SLJIT_API_FUNC_ATTRIBUTE struct sljit_stack* SLJIT_CALL sljit_allocate_stack(sljit_uw limit, sljit_uw max_limit)
		sljit_uw uw;
	} base;
	SYSTEM_INFO si;
	if (limit > max_limit || limit < 1)
	if (!sljit_page_align) {
		GetSystemInfo(&si);
		sljit_page_align = si.dwPageSize - 1;
		sljit_page_align = sysconf(_SC_PAGESIZE);
		/* Should never happen. */
		if (sljit_page_align < 0)
			sljit_page_align = 4096;
		sljit_page_align--;
	/* Align limit and max_limit. */
	max_limit = (max_limit + sljit_page_align) & ~sljit_page_align;
	stack = (struct sljit_stack*)SLJIT_MALLOC(sizeof(struct sljit_stack));
	if (!stack)
	base.ptr = VirtualAlloc(0, max_limit, MEM_RESERVE, PAGE_READWRITE);
	if (!base.ptr) {
		SLJIT_FREE(stack);
	stack->base = base.uw;
	stack->limit = stack->base;
	stack->max_limit = stack->base + max_limit;
	if (sljit_stack_resize(stack, stack->base + limit)) {
		sljit_free_stack(stack);
	base.ptr = mmap(0, max_limit, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANON, -1, 0);
	if (base.ptr == MAP_FAILED) {
	stack->limit = stack->base + limit;
	stack->top = stack->base;
	return stack;
#undef PAGE_ALIGN
SLJIT_API_FUNC_ATTRIBUTE void SLJIT_CALL sljit_free_stack(struct sljit_stack* stack)
	VirtualFree((void*)stack->base, 0, MEM_RELEASE);
	munmap((void*)stack->base, stack->max_limit - stack->base);
SLJIT_API_FUNC_ATTRIBUTE sljit_w SLJIT_CALL sljit_stack_resize(struct sljit_stack* stack, sljit_uw new_limit)
	sljit_uw aligned_old_limit;
	sljit_uw aligned_new_limit;
	if ((new_limit > stack->max_limit) || (new_limit < stack->base))
	aligned_new_limit = (new_limit + sljit_page_align) & ~sljit_page_align;
	aligned_old_limit = (stack->limit + sljit_page_align) & ~sljit_page_align;
	if (aligned_new_limit != aligned_old_limit) {
		if (aligned_new_limit > aligned_old_limit) {
			if (!VirtualAlloc((void*)aligned_old_limit, aligned_new_limit - aligned_old_limit, MEM_COMMIT, PAGE_READWRITE))
			if (!VirtualFree((void*)aligned_new_limit, aligned_old_limit - aligned_new_limit, MEM_DECOMMIT))
	stack->limit = new_limit;
	if (new_limit >= stack->limit) {
	if (aligned_new_limit < aligned_old_limit)
		madvise((void*)aligned_new_limit, aligned_old_limit - aligned_new_limit, MADV_DONTNEED);
#endif /* SLJIT_UTIL_STACK */
