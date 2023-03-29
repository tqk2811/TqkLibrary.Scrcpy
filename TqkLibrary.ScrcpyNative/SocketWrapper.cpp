#include "pch.h"
#include <winsock2.h>
#include "SocketWrapper.h"
#include "Utils.h"

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

	if (!avcheck(av_new_packet(packet, len))) {
		return false;
	}

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