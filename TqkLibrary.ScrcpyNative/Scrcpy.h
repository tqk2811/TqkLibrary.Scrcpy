#ifndef Scrcpy_H
#define Scrcpy_H
class Scrcpy
{
public: 
	Scrcpy(LPCWSTR deviceId);
	~Scrcpy();
	
private:
	std::wstring _deviceId;
};

#endif