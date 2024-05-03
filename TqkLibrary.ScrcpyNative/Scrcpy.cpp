#include "pch.h"
#include "Scrcpy_pch.h"

Scrcpy::Scrcpy(LPCWSTR deviceId) {
	this->_deviceId = deviceId;
}

Scrcpy::~Scrcpy() {
	Stop();
}

bool Scrcpy::Connect(const ScrcpyNativeConfig& nativeConfig) {

	_mutex_instance.lock();
	_mutex.lock();
	bool result = false;
	if (this->_scrcpyInstance == nullptr) {
		_mutex.unlock();

		auto instance = new ScrcpyInstance(this, nativeConfig);
		if (instance->Start()) {//long work

			_mutex.lock();
			this->_scrcpyInstance = instance;
			_mutex.unlock();

			result = true;
		}
		else
		{
			delete instance;
		}
	}
	else _mutex.unlock();
	_mutex_instance.unlock();

	return result;
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
		if (this->_scrcpyInstance->_video->IsNewFrame(this->cache.pts))
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
		result = FrameConventer::Convert(&temp, buffer, sizeInByte, w, h, lineSize);
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

bool Scrcpy::GetDeviceName(BYTE* buffer, int sizeInByte) {
	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance != nullptr && this->_scrcpyInstance->_video != nullptr) {
		result = this->_scrcpyInstance->_video->GetDeviceName(buffer, sizeInByte);
	}

	_mutex.unlock();
	return result;
}

bool Scrcpy::IsHaveScrcpyInstance() {
	_mutex.lock();

	bool result = this->_scrcpyInstance != nullptr;

	_mutex.unlock();
	return result;
}

bool Scrcpy::Draw(RenderTextureSurfaceClass* renderSurface, IUnknown* surface, bool isNewSurface, bool& isNewtargetView) {
	assert(renderSurface != nullptr);

	_mutex.lock();

	bool result = false;
	if (this->_scrcpyInstance == nullptr)
	{
		renderSurface->Shutdown();
	}
	else if (this->_scrcpyInstance != nullptr &&
		this->_scrcpyInstance->_video != nullptr) {
		result = this->_scrcpyInstance->_video->Draw(
			renderSurface,
			surface,
			isNewSurface,
			isNewtargetView);
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

bool Scrcpy::RegisterDisconnectEvent(OnDisconnectDelegate onDisconnectDelegate) {
	this->disconnectCallback = onDisconnectDelegate;
	return true;
}
bool Scrcpy::RegisterUhdiOutputEvent(UhdiOutputDelegate uhdiOutputDelegate) {
	this->_uhdiOutputDelegate = uhdiOutputDelegate;
	return true;
}
void Scrcpy::VideoDisconnectCallback() {
	if (this->disconnectCallback) this->disconnectCallback();
}

void Scrcpy::ControlClipboardCallback(BYTE* buffer, int length) {
	if (this->clipboardCallback) this->clipboardCallback(buffer, length);
}
void Scrcpy::ControlClipboardAcknowledgementCallback(UINT64 sequence) {
	if (this->clipboardAcknowledgementCallback) this->clipboardAcknowledgementCallback(sequence);
}
void Scrcpy::UhdiOutputCallback(UINT16 id, UINT16 size, const BYTE* buff) {
	if (this->_uhdiOutputDelegate) this->_uhdiOutputDelegate(id, size, buff);
}

LPCWSTR Scrcpy::GetDeviceId() {
	return this->_deviceId.c_str();
}
INT64 Scrcpy::ReadAudioFrame(AVFrame* pFrame, INT64 last_pts, DWORD waitFrameTime)
{
	INT64 result = -1;

	//_mutex.lock();
	if (this->_scrcpyInstance != nullptr &&
		this->_scrcpyInstance->_audio != nullptr
		)
	{
		result = this->_scrcpyInstance->_audio->ReadAudioFrame(pFrame, last_pts, waitFrameTime);
	}
	//_mutex.unlock();

	return result;
}