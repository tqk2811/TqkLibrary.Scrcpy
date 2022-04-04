#include "pch.h"
#include "Video.h"
#include "libav.h"
#include "ParsePacket.h"
#include "MediaDecoder.h"
const int HEADER_SIZE = 12;
#define NO_PTS UINT64_MAX
Video::Video(SOCKET sock, int buffSize, AVHWDeviceType hwType) {
	assert(buffSize > 0);
	this->buffSize = buffSize;
	this->_videoSock = new SocketWrapper(sock);
	this->_videoBuffer = new BYTE[buffSize];
	const AVCodec* h264_decoder = avcodec_find_decoder(AV_CODEC_ID_H264);
	this->_parsePacket = new ParsePacket(h264_decoder);
	this->_h264_mediaDecoder = new MediaDecoder(h264_decoder, hwType);
}

Video::~Video() {
	delete this->_videoSock;
	delete this->_videoBuffer;
	av_frame_unref(&this->tempFrame);
}



void Video::Start() {
	//
	this->threadStart();
}



void Video::Stop() { 
	this->_videoSock->Stop();
}

void Video::threadStart() {
	assert(this->_videoSock->ReadAll(this->_videoBuffer, 64) == 64);//device name
	std::string name((const char*)this->_videoBuffer, 64);
	printf(name.c_str());
	printf("\r\n");

	assert(this->_videoSock->ReadAll(this->_videoBuffer, 2) == 2);//width
	UINT16 width = sc_read16be(this->_videoBuffer);
	printf(std::string("width:").append(std::to_string(width)).append("\r\n").c_str());

	assert(this->_videoSock->ReadAll(this->_videoBuffer, 2) == 2);//height
	UINT16 height = sc_read16be(this->_videoBuffer);
	printf(std::string("height:").append(std::to_string(height)).append("\r\n").c_str());

	while (true)
	{
		assert(this->_videoSock->ReadAll(this->_videoBuffer, HEADER_SIZE) == HEADER_SIZE);

		UINT64 pts = sc_read64be(this->_videoBuffer);
		INT32 len = sc_read32be(&this->_videoBuffer[8]);
		assert(len > 0 && len <= this->buffSize);

		if ((pts == NO_PTS || (pts & AV_NOPTS_VALUE) == 0) && len > 0)
		{
			if (this->_videoSock->ReadAll(this->_videoBuffer, len) != len)
				return;
#if _DEBUG
			printf(std::string("pts:").append(std::to_string(pts)).append("  ,len:").append(std::to_string(len)).append("\r\n").c_str());
#endif

			AVPacket packet;
			int err = av_new_packet(&packet, len);
			if (err < 0)
				return;

			memcpy(packet.data, this->_videoBuffer, len);
			packet.pts = pts != NO_PTS ? (INT64)pts : AV_NOPTS_VALUE;

			if (this->_parsePacket->ParserPushPacket(&packet))
			{
				AVFrame* frame{ nullptr };
				if (this->_h264_mediaDecoder->Decode(&packet, &frame)) {

					//lock ref to frame
					mtx.lock();
					av_frame_ref(&this->tempFrame, frame);
					mtx.unlock();

					av_frame_unref(frame);
					av_frame_free(&frame);
				}
			}
			av_packet_unref(&packet);
		}
		else
		{
			return;
		}
	}
}