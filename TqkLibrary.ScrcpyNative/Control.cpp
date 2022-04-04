#include "pch.h"
#include "Control.h"


Control::Control(SOCKET sock) {
	this->_sockControl = new SocketWrapper(sock);
}

Control::~Control() {
	delete this->_sockControl;
}

void Control::Stop() {
	this->_sockControl->Stop();
}