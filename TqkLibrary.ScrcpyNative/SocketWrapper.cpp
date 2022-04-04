#include "pch.h"
#include <winsock2.h>
#include "SocketWrapper.h"

SocketWrapper::SocketWrapper(SOCKET sock) {
	assert(sock != INVALID_SOCKET);
	this->_sock = sock;
}

SocketWrapper::~SocketWrapper() {
	closesocket(this->_sock);
}

int SocketWrapper::ReadAll(BYTE* buff, int length) {
	return recv(this->_sock, (char*)buff, length, MSG_WAITALL);
}
int SocketWrapper::Write(const BYTE* buff, int length) {
	return send(this->_sock, (const char*)buff, length, NULL);//MSG_OOB
}

void SocketWrapper::Stop() {
	shutdown(this->_sock, SD_BOTH);
}