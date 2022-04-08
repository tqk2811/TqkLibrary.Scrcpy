#include "pch.h"
#include "Scrcpy.h"
#include "ProcessWrapper.h"
#include "FrameConventer.h"
#include "ScrcpyWorking.h"
#include "Video.h"
#include "Control.h"

Scrcpy::Scrcpy(LPCWSTR deviceId) {
	this->_deviceId = deviceId;
}

Scrcpy::~Scrcpy() {
	Stop();
}

bool Scrcpy::Connect(LPCWSTR config, const ScrcpyNativeConfig& nativeConfig) {
	_mutex.lock();
	if (this->_scrcpyWorking != nullptr) {
		_mutex.unlock();
		return false;
	}
	this->_scrcpyWorking = new ScrcpyWorking(this, config, nativeConfig);
	if (!this->_scrcpyWorking->Start()) {
		delete this->_scrcpyWorking;
		this->_scrcpyWorking = nullptr;
		_mutex.unlock();
		return false;
	}

	_mutex.unlock();
	return true;
}

void Scrcpy::Stop() {
	_mutex.lock();

	if (this->_scrcpyWorking != nullptr) {
		delete this->_scrcpyWorking;
		this->_scrcpyWorking = nullptr;
	}

	_mutex.unlock();
}

bool Scrcpy::ControlCommand(const BYTE* command, const int sizeInByte) {
	_mutex.lock();

	bool result = false;
	if (this->_scrcpyWorking != nullptr && this->_scrcpyWorking->_control != nullptr) {
		result = this->_scrcpyWorking->_control->ControlCommand(command, sizeInByte);
	}

	_mutex.unlock();

	return result;
}

bool Scrcpy::GetScreenShot(BYTE* buffer, const int sizeInByte, int w, int h, int lineSize) {
	//init class convert img here
	//copy lock and ref frame then unlock (for mini time lock)
	//convert frame

	AVFrame frame{ 0 };

	_mutex.lock();

	bool result = false;
	if (this->_scrcpyWorking != nullptr && this->_scrcpyWorking->_video != nullptr) {
		result = this->_scrcpyWorking->_video->RefCurrentFrame(&frame);
	}

	_mutex.unlock();

	if (result) {
		FrameConventer convert;
		result = convert.Convert(&frame, buffer, sizeInByte, w, h, lineSize);
		av_frame_unref(&frame);
	}
	return result;
}

bool Scrcpy::GetScreenSize(int& w, int& h) {
	_mutex.lock();

	bool result = false;
	if (this->_scrcpyWorking != nullptr && this->_scrcpyWorking->_video != nullptr) {
		result = this->_scrcpyWorking->_video->GetScreenSize(w, h);
	}

	_mutex.unlock();
	return result;
}