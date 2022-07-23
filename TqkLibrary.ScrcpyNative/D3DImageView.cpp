#include "pch.h"
#include "D3DImageView.h"


D3DImageView::D3DImageView() {
}

D3DImageView::~D3DImageView() {
	this->Shutdown();
}

void D3DImageView::Shutdown() {
	this->m_renderTextureSurface.Shutdown();
}

bool D3DImageView::IsNewFrame(INT64 pts) {
	bool isNewFrame = this->m_currentPts < pts;
	if(isNewFrame) this->m_currentPts = pts;
	return isNewFrame;
}