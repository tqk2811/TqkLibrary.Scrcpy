#include "pch.h"
#include "Scrcpy.h"
#include "MediaDecoder.h"
#include "ProcessWrapper.h"
#include "FrameConventer.h"
#include "ScrcpyInstance.h"
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
	if (this->_scrcpyInstance != nullptr) {
		_mutex.unlock();
		return false;
	}
	this->_scrcpyInstance = new ScrcpyInstance(this, config, nativeConfig);
	if (!this->_scrcpyInstance->Start()) {
		delete this->_scrcpyInstance;
		this->_scrcpyInstance = nullptr;
		_mutex.unlock();
		return false;
	}

	_mutex.unlock();
	return true;
}

void Scrcpy::Stop() {
	_mutex.lock();

	if (this->_scrcpyInstance != nullptr) {
		delete this->_scrcpyInstance;
		this->_scrcpyInstance = nullptr;
	}

	_mutex.unlock();
}

bool Scrcpy::ControlCommand(const BYTE* command, const int sizeInByte) {
	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance != nullptr && this->_scrcpyInstance->_control != nullptr) {
		result = this->_scrcpyInstance->_control->ControlCommand(command, sizeInByte);
	}

	_mutex.unlock();

	return result;
}

bool Scrcpy::GetScreenShot(BYTE* buffer, const int sizeInByte, const int w, const int h, const int lineSize) {
	//init class convert img here
	//copy lock and ref frame then unlock (for mini time lock)
	//convert frame

	AVFrame frame{ 0 };

	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance != nullptr && this->_scrcpyInstance->_video != nullptr) {
		result = this->_scrcpyInstance->_video->RefCurrentFrame(&frame);
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
	if (this->_scrcpyInstance != nullptr && this->_scrcpyInstance->_video != nullptr) {
		result = this->_scrcpyInstance->_video->GetScreenSize(w, h);
	}

	_mutex.unlock();
	return result;
}
bool Scrcpy::Draw(D3DImageView* d3d_imgView, IUnknown* surface, bool isNewSurface) {
	assert(d3d_imgView != nullptr);

	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance != nullptr &&
		this->_scrcpyInstance->_video != nullptr &&
		this->_scrcpyInstance->_video->_h264_mediaDecoder != nullptr &&
		this->_scrcpyInstance->_video->_h264_mediaDecoder->m_d3d11_convert != nullptr) {

		result = d3d_imgView->Draw(
			this->_scrcpyInstance->_video->_h264_mediaDecoder->m_d3d11_convert,
			surface,
			isNewSurface);
	}
	_mutex.unlock();
	return result;
}
bool Scrcpy::RegisterClipboardEvent(const ClipboardReceivedDelegate callback) {
	this->clipboardCallback = callback;
	return true;
}
bool Scrcpy::RegisterClipboardAcknowledgementEvent(ClipboardAcknowledgementDelegate callback) {
	this->clipboardAcknowledgementCallback = callback;
	return true;
}