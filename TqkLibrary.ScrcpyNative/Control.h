#ifndef Control_H
#define Control_H

class Control
{
public:
	Control(SOCKET sock);
	~Control();
	void Stop();
private:
	SocketWrapper* _sockControl{ nullptr };
};

#endif // !Control_H

