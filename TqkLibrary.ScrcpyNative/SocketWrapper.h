#ifndef _H_SocketWrapper_H_
#define _H_SocketWrapper_H_
#define PACKET_BUFFER_SIZE 1 << 18//256k
#define HEADER_SIZE 12
class SocketWrapper
{
public:
	SocketWrapper(SOCKET sock);
	~SocketWrapper();
	void Stop();
	int ReadAll(BYTE* buff, int length);
	int Write(const BYTE* buff, int length);
	bool ChangeBlockMode(bool isBlock);
	bool ChangeBufferSize(int sizeInByte = PACKET_BUFFER_SIZE);
	bool ReadPackage(AVPacket* packet);
	AVCodecID ReadCodecId();
private:
	SOCKET _sock;
};
#endif // !SocketWrapper_H