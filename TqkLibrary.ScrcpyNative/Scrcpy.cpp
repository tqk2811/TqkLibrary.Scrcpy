#include "pch.h"
#include "ProcessWrapper.h"
#include "ws2tcpip.h"
#include "Scrcpy.h"

#pragma comment(lib,"ws2_32.lib")

#define IPV4_LOCALHOST 0x7F000001
const wchar_t* adbPath = L"adb.exe";
const int portMin = 5000;
const int portMax = 65535;
const int sockTimeoutSecond = 5;

SOCKET CreateListenSock(int port, int backlog, const timeval timeout) {
	SOCKET sock = socket(AF_INET, SOCK_STREAM, NULL);
	if (sock == INVALID_SOCKET) {
		return INVALID_SOCKET;
	}

	int reuse = 1;
	if (setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, (const char*)&reuse, sizeof(reuse)) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}

	sockaddr_in addr;
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	addr.sin_addr.s_addr = htonl(IPV4_LOCALHOST);
	//InetPton(AF_INET, L"127.0.0.1", &addr.sin_addr);
	if (bind(sock, (sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}

	if (listen(sock, backlog) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}
	fd_set rfds;
	FD_ZERO(&rfds);
	FD_SET(sock, &rfds);
	if (select(sock, &rfds, NULL, NULL, &timeout) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}

	return sock;
}

SOCKET FindPort(int& port, int backlog, const timeval timeout, int maxTry = 100) {
	SOCKET sock = INVALID_SOCKET;
	port = -1;
	for (int i = 0; i < maxTry && sock == INVALID_SOCKET; i++) {
		int range = portMax - portMin + 1;
		port = rand() % range + portMin;
		sock = CreateListenSock(port, backlog, timeout);
	}

	return sock;
}

SOCKET AcceptConnection(SOCKET sock)
{
	sockaddr_in addr;
	int addrLen = sizeof(addr);
	SOCKET client = accept(sock, (sockaddr*)&addr, &addrLen);
	if (client == INVALID_SOCKET) {
		return INVALID_SOCKET;
	}
	return client;
}


Scrcpy::Scrcpy(LPCWSTR deviceId) {
	this->_deviceId = deviceId;
}

Scrcpy::~Scrcpy() {
	Stop();
}

bool Scrcpy::Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig) {
	_allMutext.lock();

	if (this->_video != nullptr || this->_control != nullptr) {
		_allMutext.unlock();
		return false;
	}

	WSAData wsaData{ 0 };
	int res = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (res != 0) {
		_allMutext.unlock();
		return false;
	}

	DWORD exitCode = RunAdbProcess(L"reverse --remove localabstract:scrcpy");
	exitCode = RunAdbProcess(L"push scrcpy-server /sdcard/scrcpy-server-tqk.jar");
	if (exitCode != 0) {
		_allMutext.unlock();
		return false;
	}

	int backlog = 1;
	if (nativeConfig.IsControl) backlog = 2;
	const timeval timeout{ 1 , 0 };

	int port = -1;
	SOCKET sock = FindPort(port, backlog, timeout);
	if (sock == INVALID_SOCKET) {
		_allMutext.unlock();
		return false;
	}

	std::wstring arg(L"reverse localabstract:scrcpy tcp:");
	arg.append(std::to_wstring(port));
	exitCode = RunAdbProcess(arg.c_str());
	if (exitCode != 0) {
		_allMutext.unlock();
		return false;
	}

	//run main process
	LPCWSTR cmds[]
	{
		L"-s",
		this->_deviceId.c_str(),
		L"shell CLASSPATH=/sdcard/scrcpy-server-tqk.jar",
		L"app_process / com.genymobile.scrcpy.Server",
		config
	};
	std::wstring args(adbPath);
	for (int i = 0; i < 5; i++)
	{
		args.append(L" ");
		args.append(cmds[i]);
	}
	this->_process = new ProcessWrapper((LPWSTR)args.c_str());


	SOCKET video = AcceptConnection(sock);
	if (video == INVALID_SOCKET) {
		closesocket(sock);
		delete this->_process;
		this->_process = nullptr;
		_allMutext.unlock();
		return false;
	}
	SOCKET control = INVALID_SOCKET;
	if (nativeConfig.IsControl) {
		control = AcceptConnection(sock);
		if (control == INVALID_SOCKET) {
			closesocket(video);
			closesocket(sock);
			delete this->_process;
			this->_process = nullptr;
			_allMutext.unlock();
			return false;
		}
	}
	closesocket(sock);

	//work with socket in thread
	this->_video = new Video(video, nativeConfig.PacketBufferLength, nativeConfig.HwType);
	if (!this->_video->Init()) {
		_allMutext.unlock();
		Stop();
		return false;
	}
	if (nativeConfig.IsControl) this->_control = new Control(control);

	this->_video->Start();
	if (nativeConfig.IsControl) this->_control->Start();
	_allMutext.unlock();
	return true;
}


void Scrcpy::Stop() {
	_allMutext.lock();

	if (this->_process != nullptr) {
		delete this->_process;
		this->_process = nullptr;
	}

	this->_controlMutext.lock();
	if (this->_control != nullptr) {
		this->_control->Stop();
		delete this->_control;
		this->_control = nullptr;
	}
	this->_controlMutext.unlock();


	this->_videoMutext.lock();
	if (this->_video != nullptr) {
		this->_video->Stop();
		delete this->_video;
		this->_video = nullptr;
	}
	this->_videoMutext.unlock();

	_allMutext.unlock();
}

bool Scrcpy::ControlCommand(const BYTE* command, const int sizeInByte) {
	_controlMutext.lock();

	bool result = false;
	if (this->_control != nullptr) {
		result = this->_control->ControlCommand(command, sizeInByte);
	}

	_controlMutext.unlock();

	return result;
}

DWORD Scrcpy::RunAdbProcess(LPCWSTR argument)
{
	LPCWSTR cmds[]
	{
		L"-s",
		this->_deviceId.c_str(),
		argument
	};
	std::wstring args(adbPath);
	for (int i = 0; i < 3; i++)
	{
		args.append(L" ");
		args.append(cmds[i]);
	}
	ProcessWrapper p((LPWSTR)args.c_str());
	return p.GetExitCode();
}




int Scrcpy::GetScreenBufferSize() {
	_videoMutext.lock();
	int result = 0;
	if (this->_video != nullptr) {
		result = this->_video->GetScreenBufferSize();
	}
	_videoMutext.unlock();
	return result;
}
bool Scrcpy::GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize) {
	_videoMutext.lock();
	bool result = false;
	if (this->_video != nullptr) {
		result = this->_video->GetScreenShot(buffer, sizeInByte, w, h, lineSize);
	}
	_videoMutext.unlock();
	return result;
}
bool Scrcpy::GetScreenSize(int& w, int& h) {
	_videoMutext.lock();
	bool result = false;
	if (this->_video != nullptr) {
		result = this->_video->GetScreenSize(w, h);
	}
	_videoMutext.unlock();
	return result;
}