bif_impl UINT ProcessExist(optl<StrArg> aProcess)
	// Return the discovered PID or zero if none.
	return aProcess.has_value() ? ProcessExist(aProcess.value()) : GetCurrentProcessId();
bif_impl UINT ProcessGetParent(optl<StrArg> aProcess)
	TCHAR buf[MAX_INTEGER_SIZE];
	return ProcessExist(aProcess.has_value() ? aProcess.value() : ITOA(GetCurrentProcessId(), buf), true);
bif_impl FResult ProcessClose(StrArg aProcess, UINT &aRetVal)
	aRetVal = 0; // Set default in case of failure.
	if (auto pid = ProcessExist(aProcess, false, false))
		if (auto hProcess = OpenProcess(PROCESS_TERMINATE, FALSE, pid))
			if (TerminateProcess(hProcess, 0))
				aRetVal = pid; // Indicate success.
static FResult ProcessGetPathName(optl<StrArg> aProcess, StrRet &aRetVal, bool aGetNameOnly)
	auto pid = aProcess.has_value() ? ProcessExist(aProcess.value(), false, false) : GetCurrentProcessId();
	if (!pid)
		return FError(ERR_NO_PROCESS, nullptr, ErrorPrototype::Target);
	TCHAR process_name[MAX_PATH];
	if (!GetProcessName(pid, process_name, _countof(process_name), aGetNameOnly))
	return aRetVal.Copy(process_name) ? OK : FR_E_OUTOFMEM;
bif_impl FResult ProcessGetName(optl<StrArg> aProcess, StrRet &aRetVal)
	return ProcessGetPathName(aProcess, aRetVal, true);
bif_impl FResult ProcessGetPath(optl<StrArg> aProcess, StrRet &aRetVal)
	return ProcessGetPathName(aProcess, aRetVal, false);
void MsgSleepWithListLines(int aSleepDuration, Line *waiting_line, DWORD start_time);
static FResult ProcessWait(StrArg aProcess, optl<double> aTimeout, UINT &aRetVal, bool aWaitClose)
	HANDLE proc = NULL;
	// This section is similar to that used for WINWAIT and RUNWAIT:
	bool wait_indefinitely;
	int sleep_duration, remainder;
	DWORD start_time = GetTickCount();
	Line *waiting_line = g_script.mCurrLine;
	if (aTimeout.has_value()) // The param containing the timeout value was specified.
		wait_indefinitely = false;
		sleep_duration = (int)(aTimeout.value_or(0) * 1000); // Can be zero.
		wait_indefinitely = true;
		sleep_duration = 0; // Just to catch any bugs.
	{ // Always do the first iteration so that at least one check is done.
		if (proc && WaitForSingleObject(proc, 0) != WAIT_TIMEOUT)
			// Reset proc so that if a timeout occurs in this iteration, the PID won't be returned.
			// Don't reset pid directly because there might be other matching processes.
			CloseHandle(proc);
			proc = NULL;
		if (!proc)
			pid = ProcessExist(aProcess);
		if ((!aWaitClose) == (pid != 0)) // i.e. condition of this cmd is satisfied.
			// For WaitClose: Since PID cannot always be determined (i.e. if process never existed,
			// there was no need to wait for it to close), for consistency, return 0 on success.
			aRetVal = pid;
		// Must use int or any negative result (due to exceeding sleep_duration) will be lost due to DWORD type:
		if (wait_indefinitely || (remainder = sleep_duration - (GetTickCount() - start_time)) > SLEEP_INTERVAL_HALF)
			if (aWaitClose && (proc || (proc = OpenProcess(SYNCHRONIZE, FALSE, pid))))
				switch (MsgWaitForMultipleObjects(1, &proc, FALSE, wait_indefinitely ? INFINITE : remainder, QS_ALLINPUT))
				case WAIT_OBJECT_0:
					// This process has closed, but we may need another iteration to verify that no other
					// matching processes are running.
				case WAIT_OBJECT_0 + 1:
					MsgSleepWithListLines(-1, waiting_line, start_time);
				case WAIT_TIMEOUT:
					return OK; // Avoid the 100ms sleep below.
				// In case of failure, fall through:
			MsgSleepWithListLines(100, waiting_line, start_time);  // For performance reasons, don't check as often as the WinWait family does.
		else // Done waiting.
			if (proc)
			// Return 0 if ProcessWait times out; or the PID of the process that still exists
			// if ProcessWaitClose times out.
bif_impl FResult ProcessWait(StrArg aProcess, optl<double> aTimeout, UINT &aRetVal)
	return ProcessWait(aProcess, aTimeout, aRetVal, false);
bif_impl FResult ProcessWaitClose(StrArg aProcess, optl<double> aTimeout, UINT &aRetVal)
	return ProcessWait(aProcess, aTimeout, aRetVal, true);
bif_impl FResult ProcessSetPriority(StrArg aPriority, optl<StrArg> aProcess, UINT &aRetVal)
	DWORD priority;
	switch (_totupper(*aPriority))
	case 'L': priority = IDLE_PRIORITY_CLASS; break;
	case 'B': priority = BELOW_NORMAL_PRIORITY_CLASS; break;
	case 'N': priority = NORMAL_PRIORITY_CLASS; break;
	case 'A': priority = ABOVE_NORMAL_PRIORITY_CLASS; break;
	case 'H': priority = HIGH_PRIORITY_CLASS; break;
	case 'R': priority = REALTIME_PRIORITY_CLASS; break;
		// Since above didn't break, aPriority was invalid.
	DWORD pid = aProcess.has_nonempty_value() ? ProcessExist(aProcess.value(), false, false) : GetCurrentProcessId();
	HANDLE hProcess = OpenProcess(PROCESS_SET_INFORMATION, FALSE, pid);
	if (!hProcess)
	DWORD error = SetPriorityClass(hProcess, priority) ? NOERROR : GetLastError();
bif_impl FResult Run(StrArg aTarget, optl<StrArg> aWorkingDir, optl<StrArg> aOptions, ResultToken *aOutPID)
	HANDLE hprocess;
	auto result = g_script.ActionExec(aTarget, nullptr, aWorkingDir.value_or_null(), true
		, aOptions.value_or_null(), &hprocess, true, true);
	if (hprocess)
		if (aOutPID)
			aOutPID->SetValue((UINT)GetProcessId(hprocess));
		CloseHandle(hprocess);
DWORD GetProcessName(DWORD aProcessID, LPTSTR aBuf, DWORD aBufSize, bool aGetNameOnly)
	HANDLE hproc;
	// Windows XP/2003 would require PROCESS_QUERY_INFORMATION, but those OSes are not supported.
	if (  !(hproc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, aProcessID))  )
	// Benchmarks showed that attempting GetModuleBaseName/GetModuleFileNameEx
	// first did not help performance.  Also, QueryFullProcessImageName appeared
	// to be slower than the following.
	DWORD buf_length = GetProcessImageFileName(hproc, aBuf, aBufSize);
	if (buf_length)
		if (aGetNameOnly)
			// Convert full path to just name.
			cp = _tcsrchr(aBuf, '\\');
				tmemmove(aBuf, cp + 1, _tcslen(cp)); // Includes the null terminator.
			// Convert device path to logical path.
			TCHAR device_path[MAX_PATH];
			TCHAR letter[3];
			letter[1] = ':';
			letter[2] = '\0';
			// Get the mask of valid drive letters to avoid some unnecessary QueryDosDevice calls.
			// Mapping drive Z: to a network folder is relatively common, so this can avoid many
			// calls.  On typical systems, it just avoids querying A: and 'B:.
			DWORD mask = GetLogicalDrives();
			for (int i = 0; i < 26; ++i)
				if (!(mask & (1 << i)))
				*letter = 'A' + i;
				DWORD device_path_length = QueryDosDevice(letter, device_path, _countof(device_path));
				if (device_path_length > 2) // Includes two null terminators.
					device_path_length -= 2;
					if (!_tcsncmp(device_path, aBuf, device_path_length)
						&& aBuf[device_path_length] == '\\') // Relies on short-circuit evaluation.
						// Copy drive letter:
						aBuf[0] = letter[0];
						aBuf[1] = letter[1];
						// Contract path to remove remainder of device name.
						tmemmove(aBuf + 2, aBuf + device_path_length, buf_length - device_path_length + 1);
						buf_length -= device_path_length - 2;
	return buf_length;
