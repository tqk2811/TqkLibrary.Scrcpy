#include "pch.h"
#include "Scrcpy_pch.h"

ScrcpyInstance::ScrcpyInstance(Scrcpy* scrcpy, const ScrcpyNativeConfig& nativeConfig) {
	this->_scrcpy = scrcpy;
	this->_nativeConfig = nativeConfig;
}

ScrcpyInstance::~ScrcpyInstance() {
	if (this->_control != nullptr) {
		this->_control->Stop();
		delete this->_control;
	}

	if (this->_audio != nullptr) {
		this->_audio->Stop();
		delete this->_audio;
	}

	if (this->_video != nullptr) {
		this->_video->Stop();
		delete this->_video;
	}

	if (this->_wsa_isStartUp) WSACleanup();
}

bool ScrcpyInstance::Connect(SOCKET videoSock, SOCKET audioSock, SOCKET controlSock) {
	// WSAStartup is still needed so recv/send work inside Video/Audio/Control threads
	WSAData wsaData{ 0 };
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
		return false;
	this->_wsa_isStartUp = true;

	if (this->_nativeConfig.IsVideo) {
		this->_video = new Video(this->_scrcpy, videoSock, this->_nativeConfig);
		if (!this->_video->Init()) return false;
		this->_video->SetNotifyDisconnect(true);
	}

	if (this->_nativeConfig.IsAudio) {
		this->_audio = new Audio(this->_scrcpy, audioSock, this->_nativeConfig);
		if (!this->_audio->Init()) return false;
		if (!this->_nativeConfig.IsVideo)
			this->_audio->SetNotifyDisconnect(true);
	}

	if (this->_nativeConfig.IsControl) {
		this->_control = new Control(this->_scrcpy, controlSock);
		if (!this->_nativeConfig.IsVideo && !this->_nativeConfig.IsAudio)
			this->_control->SetNotifyDisconnect(true);
	}

	if (this->_nativeConfig.IsVideo)
		this->_video->Start();
	if (this->_nativeConfig.IsAudio)
		this->_audio->Start();
	if (this->_nativeConfig.IsControl)
		this->_control->Start();

	if (this->_nativeConfig.IsVideo)
		return this->_video->WaitForFirstFrame(this->_nativeConfig.ConnectionTimeout);

	return true;
}
