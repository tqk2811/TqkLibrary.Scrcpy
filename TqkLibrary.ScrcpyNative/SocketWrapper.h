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
	// Recycles packet data buffers across ReadPackage calls so we don't malloc/free one per frame.
	// Owned by this wrapper (single read thread), torn down in the destructor.
	AVBufferPool* _packetBufferPool{ nullptr };
	int _packetBufferPoolSize{ 0 };
};
#endif // !SocketWrapper_H