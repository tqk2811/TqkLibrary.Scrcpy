#include "pch.h"
#include "ScrcpyWorking.h"
#include "Video.h"
#include "Control.h"
#include "ProcessWrapper.h"
#include "Scrcpy.h"
#include <chrono>

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

	u_long iMode = 1;
	if (ioctlsocket(sock, FIONBIO, &iMode) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}

	sockaddr_in addr;
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	addr.sin_addr.s_addr = htonl(IPV4_LOCALHOST);
	if (bind(sock, (sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR) {
		closesocket(sock);
		return INVALID_SOCKET;
	}

	if (listen(sock, backlog) == SOCKET_ERROR) {
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

SOCKET AcceptConnection(SOCKET sock, int timeout = 2000)
{
	sockaddr_in addr;
	int addrLen = sizeof(addr);
	auto start = std::chrono::high_resolution_clock::now();
	while (true)
	{
		SOCKET client = accept(sock, (sockaddr*)&addr, &addrLen);
		if (client != INVALID_SOCKET) {
			u_long iMode = 0;
			if (ioctlsocket(client, FIONBIO, &iMode) == SOCKET_ERROR) {
				closesocket(client);
				return INVALID_SOCKET;
			}
			return client;
		}
		else
		{
			auto end = std::chrono::high_resolution_clock::now();
			auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count();
			if (duration > timeout) {
				return INVALID_SOCKET;
			}
		}
	}
}

ScrcpyWorking::ScrcpyWorking(const Scrcpy* scrcpy, LPCWSTR config, const ScrcpyNativeConfig& nativeConfig) {
	this->_scrcpy = scrcpy;
	this->_config = config;
	this->_nativeConfig = nativeConfig;
}

ScrcpyWorking::~ScrcpyWorking() {
	if (this->_listenSock != INVALID_SOCKET)
		closesocket(this->_listenSock);

	if (this->_process != nullptr)
		delete this->_process;

	if (this->_control != nullptr) {
		this->_control->Stop();
		delete this->_control;
	}

	if (this->_process != nullptr) {
		this->_video->Stop();
		delete this->_video;
	}

	if (this->_wsa_isStartUp) WSACleanup();
}

DWORD ScrcpyWorking::RunAdbProcess(LPCWSTR argument)
{
	LPCWSTR cmds[]
	{
		L"-s",
		this->_scrcpy->_deviceId.c_str(),
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


bool ScrcpyWorking::Start() {
	WSAData wsaData{ 0 };
	int res = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (res != 0) {
		return false;
	}
	this->_wsa_isStartUp = true;

	DWORD exitCode = RunAdbProcess(L"reverse --remove localabstract:scrcpy");
	exitCode = RunAdbProcess(L"push scrcpy-server /sdcard/scrcpy-server-tqk.jar");
	if (exitCode != 0) {
		return false;
	}

	int backlog = 1;
	if (this->_nativeConfig.IsControl) backlog = 2;
	const timeval timeout{ 2 , 0 };

	int port = -1;
	this->_listenSock = FindPort(port, backlog, timeout);
	if (this->_listenSock == INVALID_SOCKET) {
		return false;
	}

	//port += 5;//test on failed connect
	std::wstring arg(L"reverse localabstract:scrcpy tcp:");
	arg.append(std::to_wstring(port));
	exitCode = RunAdbProcess(arg.c_str());
	if (exitCode != 0) {
		return false;
	}

	//run main process
	LPCWSTR cmds[5]
	{
		L"-s",
		this->_scrcpy->_deviceId.c_str(),
		L"shell CLASSPATH=/sdcard/scrcpy-server-tqk.jar",
		L"app_process / com.genymobile.scrcpy.Server",
		this->_config.c_str()
	};
	std::wstring args(adbPath);
	for (int i = 0; i < 5; i++)
	{
		args.append(L" ");
		args.append(cmds[i]);
	}
	this->_process = new ProcessWrapper((LPWSTR)args.c_str());


	SOCKET video = AcceptConnection(this->_listenSock, this->_nativeConfig.ConnectionTimeout);
	if (video == INVALID_SOCKET) {
		return false;
	}
	this->_video = new Video(video, (AVHWDeviceType)this->_nativeConfig.HwType);
	if (!this->_video->Init()) {
		return false;
	}

	SOCKET control = INVALID_SOCKET;
	if (this->_nativeConfig.IsControl) {
		control = AcceptConnection(this->_listenSock, this->_nativeConfig.ConnectionTimeout);
		if (control == INVALID_SOCKET) {
			return false;
		}
		this->_control = new Control(control);
	}

	this->_video->Start();//start video thread
	if (this->_nativeConfig.IsControl) this->_control->Start();//start control thread

	//close listen sock
	closesocket(this->_listenSock);
	this->_listenSock = INVALID_SOCKET;

	return true;
}