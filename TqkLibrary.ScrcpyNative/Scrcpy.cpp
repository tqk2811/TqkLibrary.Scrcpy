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

	av_frame_unref(&this->cache);

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

	AVFrame temp{ 0 };

	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance != nullptr &&
		this->_scrcpyInstance->_video != nullptr) {
		if (this->_scrcpyInstance->_video->_h264_mediaDecoder->IsNewFrame(this->cache.pts))
		{
			av_frame_unref(&this->cache);
			result = this->_scrcpyInstance->_video->GetCurrentRgbaFrame(&this->cache);
		}
		else
			result = true;
	}
	if (result) av_frame_ref(&temp, &this->cache);

	_mutex.unlock();

	if (result) {
		FrameConventer convert;
		result = convert.Convert(&temp, buffer, sizeInByte, w, h, lineSize);
	}
	av_frame_unref(&temp);
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
		this->_scrcpyInstance->_video->_h264_mediaDecoder != nullptr) {
		result = this->_scrcpyInstance->_video->_h264_mediaDecoder->Draw(
			d3d_imgView,
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