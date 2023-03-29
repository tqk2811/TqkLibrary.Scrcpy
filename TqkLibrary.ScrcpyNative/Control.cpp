#include "pch.h"
#include "Scrcpy_pch.h"

#define CONTROL_MSG_MAX_SIZE (1 << 18)//256k
enum ScrcpyControlReceivedType : BYTE
{
	DEVICE_MSG_TYPE_CLIPBOARD = 0,
	DEVICE_MSG_TYPE_ACK_CLIPBOARD = 1,
};
Control::Control(Scrcpy* scrcpy, SOCKET sock) {
	this->scrcpy = scrcpy;
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
	if (this->_threadHandle != INVALID_HANDLE_VALUE)
		WaitForSingleObject(this->_threadHandle, INFINITE);
}
DWORD WINAPI Control::MyThreadFunction(LPVOID lpParam) {
	((Control*)lpParam)->threadStart();
	return 0;
}
bool Control::ControlCommand(const BYTE* command, const int sizeInByte) {
	if (this->_sockControl->Write(command, sizeInByte) == SOCKET_ERROR) {
		return false;
	}
	return true;
}

void Control::threadStart() {
	this->_sockControl->ChangeBlockMode(true);
	this->_sockControl->ChangeBufferSize(CONTROL_MSG_MAX_SIZE);
	while (!this->_isStop) {

		int readSize = this->_sockControl->ReadAll(this->_buffer, 1);
		if (readSize != 1)
			return;

		ScrcpyControlReceivedType type = (ScrcpyControlReceivedType)this->_buffer[0];

		switch (type)
		{
		case DEVICE_MSG_TYPE_CLIPBOARD:
		{
			if (this->_sockControl->ReadAll(this->_buffer, 4) != 4)
				return;

			UINT32 len = sc_read32be(this->_buffer);
			if (len != 0)
			{
				assert(len <= CONTROL_MSG_MAX_SIZE);

				if (this->_sockControl->ReadAll(this->_buffer, len) != len)
					return;
			}
			this->scrcpy->ControlClipboardCallback(this->_buffer, len);

			break;
		}
		case DEVICE_MSG_TYPE_ACK_CLIPBOARD:
		{
			if (this->_sockControl->ReadAll(this->_buffer, 8) != 8)
				return;
			UINT64 sequence = sc_read64be(this->_buffer);
			this->scrcpy->ControlClipboardAcknowledgementCallback(sequence);
			break;
		}
		default:
			break;
		}
	}
}