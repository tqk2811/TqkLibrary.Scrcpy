#include "pch.h"
#include "libav.h"
#include "ParsePacket.h"
#include "Utils.h"

ParsePacket::ParsePacket(const AVCodec* codec_decoder) {
	this->_codec_decoder = codec_decoder;
}

ParsePacket::~ParsePacket() {
	av_packet_unref(_pending);
	av_packet_free(&_pending);

	av_parser_close(_parser);

	avcodec_close(_codec_ctx);
	avcodec_free_context(&_codec_ctx);
}
bool ParsePacket::Init() {
	this->_codec_ctx = avcodec_alloc_context3(this->_codec_decoder);
	if (this->_codec_ctx == nullptr)
		return false;


	this->_parser = av_parser_init(this->_codec_decoder->id);
	if (this->_parser == nullptr)
		return false;
	this->_parser->flags |= PARSER_FLAG_COMPLETE_FRAMES;


	this->_pending = av_packet_alloc();
	if (this->_pending == nullptr)
		return false;

	return true;
}




bool ParsePacket::ParserPushPacket(AVPacket* packet) {
	bool is_config = packet->pts == AV_NOPTS_VALUE;
	if (has_pending || is_config) {
		if (has_pending) {
			//save old pending size before change size
			int offset = _pending->size;

			//increase size pending packet for copy at start pointer + pending->size
			//pending: [------------|       add size packet->size     ]
			assert(avcheck(av_grow_packet(_pending, packet->size)));

			//pending: [------------|    packet->data copy to here    ]
			memcpy(_pending->data + offset, packet->data, packet->size);
		}
		else//is_config
		{
			//just ref
			assert(avcheck(av_packet_ref(_pending, packet)));
			has_pending = true;
		}

		if (!is_config)//set pending pts/dts -> for parse
		{
			_pending->pts = packet->pts;
			_pending->dts = packet->dts;
			_pending->flags = packet->flags;
		}
	}

	if (!is_config)//if has pts -> config completed
	{
		if (has_pending)
		{
			//parse pending
			StreamParse(_pending);

			//free packet data
			av_packet_unref(packet);

			//move data and pointer from pending to packet (and free pending->data)
			av_packet_move_ref(packet, _pending);
			has_pending = false;
		}
		else StreamParse(packet);//not pending & not config -> parse packet

		return true;//packet ready
	}
	else return false;//packet is pending
}

void ParsePacket::StreamParse(AVPacket* packet) {
	uint8_t* in_data = packet->data;
	int in_len = packet->size;
	uint8_t* out_data = nullptr;
	int out_len = 0;
	int r = av_parser_parse2(_parser, _codec_ctx,
		&out_data, &out_len, in_data, in_len,
		AV_NOPTS_VALUE, AV_NOPTS_VALUE, -1);

	// PARSER_FLAG_COMPLETE_FRAMES is set
	assert(r == in_len);
	assert(out_len == in_len);

	if (_parser->key_frame != 0)
	{
		packet->flags |= AV_PKT_FLAG_KEY;
	}
}