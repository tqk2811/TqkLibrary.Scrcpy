#include "pch.h"
#include <winsock2.h>
#include "SocketWrapper.h"
#include "Utils.h"

SocketWrapper::SocketWrapper(SOCKET sock) {
	assert(sock != INVALID_SOCKET);
	this->_sock = sock;
}

SocketWrapper::~SocketWrapper() {
	// Socket is owned and closed by C# — do not closesocket here
	if (this->_packetBufferPool)
		av_buffer_pool_uninit(&this->_packetBufferPool);
}

int SocketWrapper::ReadAll(BYTE* buff, int length) {
	int result = recv(this->_sock, (char*)buff, length, MSG_WAITALL);
#if _DEBUG
	if (result == SOCKET_ERROR)
	{
		int err = WSAGetLastError();
		printf("SocketWrapper::ReadAll error code %d\r\n", err);
	}
#endif
	return result;
}
int SocketWrapper::Write(const BYTE* buff, int length) {
	int result = send(this->_sock, (const char*)buff, length, NULL);//MSG_OOB
#if _DEBUG
	if (result == SOCKET_ERROR)
	{
		int err = WSAGetLastError();
		printf("SocketWrapper::Write error code %d\r\n", err);
	}
#endif
	return result;
}

void SocketWrapper::Stop() {
	shutdown(this->_sock, SD_BOTH);
}

bool SocketWrapper::ChangeBlockMode(bool isBlock) {
	u_long iMode = isBlock ? 0 : 1;
	if (ioctlsocket(this->_sock, FIONBIO, &iMode) == SOCKET_ERROR) {
		return false;
	}
	return true;
}
bool SocketWrapper::ChangeBufferSize(int sizeInByte) {
	return setsockopt(this->_sock, SOL_SOCKET, SO_RCVBUF, (LPCSTR)&sizeInByte, sizeof(int)) != SOCKET_ERROR;
}

bool SocketWrapper::ReadPackage(AVPacket* packet) {

#define SC_PACKET_FLAG_CONFIG    (UINT64_C(1) << 63)
#define SC_PACKET_FLAG_KEY_FRAME (UINT64_C(1) << 62)
#define SC_PACKET_PTS_MASK (SC_PACKET_FLAG_KEY_FRAME - 1)

	if (!packet)
		return false;

	BYTE header_buffer[HEADER_SIZE];
	if (this->ReadAll(header_buffer, HEADER_SIZE) != HEADER_SIZE)
		return false;

	UINT64 pts_flags = sc_read64be(header_buffer);
	INT32 len = sc_read32be(header_buffer + 8);

	// Recycle a pooled, reference-counted buffer instead of allocating a new one per packet
	// (what av_new_packet does). avcodec_send_packet takes its own reference, so a buffer only
	// returns to the pool once every reference is dropped: reuse is always safe — if the decoder
	// still holds it, the pool hands out a fresh buffer instead. Decoders may read up to
	// AV_INPUT_BUFFER_PADDING_SIZE bytes past the data, so include that padding and zero it.
	int required = len + AV_INPUT_BUFFER_PADDING_SIZE;
	if (this->_packetBufferPool == nullptr || this->_packetBufferPoolSize < required)
	{
		if (this->_packetBufferPool)
			av_buffer_pool_uninit(&this->_packetBufferPool);
		// Grow with headroom so normal keyframe-size jitter doesn't rebuild the pool every frame.
		this->_packetBufferPoolSize = required + required / 2;
		this->_packetBufferPool = av_buffer_pool_init(this->_packetBufferPoolSize, nullptr);
		if (this->_packetBufferPool == nullptr)
		{
			this->_packetBufferPoolSize = 0;
			return false;
		}
	}

	av_packet_unref(packet);
	AVBufferRef* buffer = av_buffer_pool_get(this->_packetBufferPool);
	if (buffer == nullptr)
		return false;

	packet->buf = buffer;
	packet->data = buffer->data;
	packet->size = len;
	memset(packet->data + len, 0, AV_INPUT_BUFFER_PADDING_SIZE);

	if (this->ReadAll(packet->data, len) != len)
	{
		return false;
	}

	if (pts_flags & SC_PACKET_FLAG_CONFIG) {
		packet->pts = AV_NOPTS_VALUE;
	}
	else {
		packet->pts = pts_flags & SC_PACKET_PTS_MASK;
	}

	if (pts_flags & SC_PACKET_FLAG_KEY_FRAME) {
		packet->flags |= AV_PKT_FLAG_KEY;
	}

	packet->dts = packet->pts;
	return true;
}
AVCodecID SocketWrapper::ReadCodecId() {
	BYTE codec_buffer[4];
	if (this->ReadAll(codec_buffer, 4) != 4)
		return AVCodecID::AV_CODEC_ID_NONE;

	uint32_t raw_codec_id = sc_read32be(codec_buffer);

	if (raw_codec_id == 0 ||	//stream explicitly disabled by the device
		raw_codec_id == 1)		//stream configuration error on the device
		return AVCodecID::AV_CODEC_ID_NONE;

	return sc_demuxer_to_avcodec_id(raw_codec_id);
}