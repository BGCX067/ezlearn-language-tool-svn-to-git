#include "stdafx.h"
#include <dshow.h>
#include "Utility.h"

#define AudioPlayerFunctionsExport
#include "AudioPlayerFunctions.h"

// No export functions 
HRESULT InitAudioPlayer(IFilterGraph **ppFG);



AudioPlayerAPI BOOL Play_File_Test(float fPlayRate){
	ASSERT(fPlayRate!=0); // fPlayRate 必須不等於 0

	CComPtr<IFilterGraph> pFG=0;
	CComPtr<IMediaControl> pMC=0;
    CComPtr<IMediaEvent>   pME=0;
	CComPtr<IMediaSeeking> pSeek=0;

    HRESULT hr=InitAudioPlayer(&pFG);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("InitAudioPlayer faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}

	hr = pFG->QueryInterface(IID_IMediaControl, (void **)&pMC);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("QueryInterface IMediaControl faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}

	hr = pFG->QueryInterface(IID_IMediaEvent, (void **)&pME);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("QueryInterface IMediaEvent faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}

	hr = pFG->QueryInterface(IID_IMediaSeeking, (void **)&pSeek);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("QueryInterface IID_IMediaSeeking faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}

	hr=pSeek->SetRate(fPlayRate);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("SetRate faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}
	
	hr=pMC->Run();
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("Run faiure"),_T("Error"),MB_OK);
		#endif
		return FALSE;
	}

	
	long evCode = 0;
	hr=pME->WaitForCompletion(INFINITE, &evCode);
	if(hr!=S_OK){
		#ifdef _DEBUG
		switch(hr){
			case E_ABORT:
				MessageBox(NULL,_T("Time-out expired."),_T("Error"),MB_OK);
				break;
			case VFW_E_WRONG_STATE:
				MessageBox(NULL,_T("The filter graph is not running."),_T("Error"),MB_OK);
				break;
		}
		#endif
		return FALSE;
	}
	


	CoUninitialize();
	return TRUE;
}

 
HRESULT InitAudioPlayer(IFilterGraph **ppFG){
	// Step 1: Initialize COM
	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	// Step 2: Create filter graph 
	HRESULT hr = CoCreateInstance(CLSID_FilterGraph, NULL, CLSCTX_INPROC,IID_IFilterGraph, (void**) ppFG);
	if(FAILED(hr)) {
		#ifdef _DEBUG
		MessageBox(NULL,_T("CoCreateInstance faiure"),_T("Error"),MB_OK);
		#endif
		return hr;
	}
	
	// Step 3: Get a GraphBuilder interface from the filter graph 
	CComPtr<IGraphBuilder>pBuilder; // Graph builder
	hr = (*ppFG)->QueryInterface(IID_IGraphBuilder, (void **)&pBuilder);
	if(FAILED(hr)){
		#ifdef _DEBUG
		MessageBox(NULL,_T("QueryInterface IID_IGraphBuilder faiure"),_T("Error"),MB_OK);
		#endif
		return hr;
	}
	
	hr = pBuilder->RenderFile(_T("test.mp3"), NULL);
	if(FAILED(hr)){ 
		#ifdef _DEBUG
		MessageBox(NULL,_T("RenderFile faiure"),_T("Error"),MB_OK);
		#endif
		return hr;
	}

	PrintAllFilter(pBuilder);

	CComPtr<IGraphConfig> pGraphConfig;
	hr = (*ppFG)->QueryInterface(IID_IGraphConfig, (void **)&pBuilder);
	if(FAILED(hr)){
		#ifdef _DEBUG
		MessageBox(NULL,_T("QueryInterface IID_IGraphConfig faiure"),_T("Error"),MB_OK);
		#endif
		return hr;
	}
}

