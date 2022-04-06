#ifndef ParsePacket_H
#define ParsePacket_H
class ParsePacket
{
public:
	ParsePacket(const AVCodec* codec_decoder);
	~ParsePacket();

	bool Init();
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="packet"></param>
	/// <returns>True when packet are done, else pending</returns>
	bool ParserPushPacket(AVPacket* packet);
private:
	const AVCodec* _codec_decoder = nullptr;
	AVCodecContext* _codec_ctx = nullptr;
	AVCodecParserContext* _parser = nullptr;
	bool has_pending = false;
	AVPacket* _pending = nullptr;
	void StreamParse(AVPacket* packet);
};
#endif // !ParsePacket_H