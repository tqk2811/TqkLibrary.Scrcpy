#ifndef Video_H
#define Video_H

class Video
{
public:
	Video(SOCKET sock, int buffSize, AVHWDeviceType hwType);
	~Video();
	void Start();
	void Stop();
private:
	int buffSize{ 0 };
	ParsePacket* _parsePacket{ nullptr };
	MediaDecoder* _h264_mediaDecoder{ nullptr };
	SocketWrapper* _videoSock{ nullptr };
	BYTE* _videoBuffer{ nullptr };
	AVFrame tempFrame{ 0 };
	std::mutex mtx;
	void threadStart();
};

#endif // !Video_H