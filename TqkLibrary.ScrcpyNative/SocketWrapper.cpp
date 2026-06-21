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

	// scrcpy v4.0 wire format (ref app/src/demuxer.c @ v4.0).
	// A 12-byte meta header precedes each packet. The MSB of byte 0 is the
	// session/media discriminator:
	//   header[0] & 0x80 == 1 -> SESSION packet (video only): 12-byte header,
	//                            NO payload (carries width/height/client_resized).
	//                            The decoder re-derives the frame size from the
	//                            bitstream, so it is safe to skip it here.
	//   header[0] & 0x80 == 0 -> MEDIA packet: 12-byte header + <len> payload.
	// Flag bits moved vs v3.x: CONFIG 1<<63 -> 1<<62, KEY 1<<62 -> 1<<61.
#define SC_PACKET_FLAG_CONFIG    (UINT64_C(1) << 62)
#define SC_PACKET_FLAG_KEY_FRAME (UINT64_C(1) << 61)
#define SC_PACKET_PTS_MASK (SC_PACKET_FLAG_KEY_FRAME - 1)

	if (!packet)
		return false;

	BYTE header_buffer[HEADER_SIZE];

	// Skip any session header(s) until a media header is received.
	for (;;) {
		if (this->ReadAll(header_buffer, HEADER_SIZE) != HEADER_SIZE)
			return false;

		if (!(header_buffer[0] & 0x80))
			break; // media packet

		// Session packet: no payload follows; consume and keep reading.
	}

	UINT64 pts_flags = sc_read64be(header_buffer);
	INT32 len = sc_read32be(header_buffer + 8);

	if (len <= 0) {
		// v4.0: a zero packet length is invalid.
		return false;
	}

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