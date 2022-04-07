#include "pch.h"
#include "ProcessWrapper.h"

ProcessWrapper::ProcessWrapper(wchar_t* args) {
	this->_isCreateSuccess =
		CreateProcess(
			NULL,
			args,
			NULL,
			NULL,
			FALSE,
			CREATE_NO_WINDOW,
			NULL,
			NULL,
			&this->_si,
			&this->_pi);
}

ProcessWrapper::~ProcessWrapper() {
	if (_isCreateSuccess)
	{
		if (!WaitForExit(0)) TerminateProcess(this->_pi.hProcess, -1);
		CloseHandle(this->_pi.hProcess);
	}
}

bool ProcessWrapper::WaitForExit(DWORD timeout) {
	return WaitForSingleObject(this->_pi.hProcess, timeout) == WAIT_OBJECT_0;
}

bool ProcessWrapper::GetExitCodeProcess(LPDWORD lpExitCode) {
	return ::GetExitCodeProcess(this->_pi.hProcess, lpExitCode);
}
DWORD ProcessWrapper::GetExitCode() {
	DWORD code{ 0 };
	WaitForExit();
	GetExitCodeProcess(&code);
	return code;
}
//
//int ProcessWrapper::WriteStdIn(const BYTE* buff, int buffSize) {
//	return 0;
//}
//int ProcessWrapper::ReadStdOut(BYTE* buff, int buffSize) {
//	return 0;
//}
//int ProcessWrapper::ReadStdErr(BYTE* buff, int buffSize) {
//	return 0;
//}