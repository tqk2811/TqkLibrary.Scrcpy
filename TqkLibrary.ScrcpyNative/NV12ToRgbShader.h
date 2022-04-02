#ifndef NV12ToRgbShader_H
#define NV12ToRgbShader_H
class NV12ToRgbShader
{
public:
	NV12ToRgbShader();
	~NV12ToRgbShader();

	bool Init();

	bool Convert(const AVFrame* frame, BYTE* buff, int buffSize);
private:


	bool InitShader();
};
#endif // !NV12ToRgbShader_H



