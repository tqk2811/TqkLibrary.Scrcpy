#include "pch.h"
#include "Control.h"
#define CONTROL_MSG_MAX_SIZE 1 << 18//256k
enum ScrcpyControlReceivedType : BYTE
{
	DEVICE_MSG_TYPE_CLIPBOARD = 0,
};
Control::Control(SOCKET sock) {
	this->_sockControl = new SocketWrapper(sock);
	this->_buffer = new BYTE[CONTROL_MSG_MAX_SIZE];
}

Control::~Control() {
	delete this->_sockControl;
	delete this->_buffer;
	CloseHandle(this->_threadHandle);
}
void Control::Start() {
	this->_threadHandle =
		CreateThread(
			NULL,                   // default security attributes
			0,                      // use default stack size  
			MyThreadFunction,       // thread function name
			this,					// argument to thread function 
			0,                      // use default creation flags 
			&_threadId);
}
void Control::Stop() {
	this->_sockControl->Stop();
	this->_isStop = true;
	WaitForSingleObject(this->_threadHandle, INFINITE);
}
DWORD WINAPI Control::MyThreadFunction(LPVOID lpParam) {
	((Control*)lpParam)->threadStart();
	return 0;
}


void Control::threadStart() {
	while (!this->_isStop) {
		
		if (this->_sockControl->ReadAll(this->_buffer, 1) != 1)
			return;
		
		ScrcpyControlReceivedType type = (ScrcpyControlReceivedType)this->_buffer[0];

		switch (type)
		{
		case DEVICE_MSG_TYPE_CLIPBOARD:
		{
			if (this->_sockControl->ReadAll(this->_buffer, 4) != 4)
				return;
			
			UINT32 len = sc_read32be(this->_buffer);
			if (len == 0) break;
			assert(len > CONTROL_MSG_MAX_SIZE);
			
			if (this->_sockControl->ReadAll(this->_buffer, len) != len)
				return;

			//send to c#
			
			break;
		}
		default:
			break;
		}
	}
}