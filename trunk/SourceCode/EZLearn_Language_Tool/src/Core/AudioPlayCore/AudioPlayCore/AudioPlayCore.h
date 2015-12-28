// AudioPlayCore.h : main header file for the AudioPlayCore DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CAudioPlayCoreApp
// See AudioPlayCore.cpp for the implementation of this class
//

class CAudioPlayCoreApp : public CWinApp
{
private:
	
	
	
public:
	CAudioPlayCoreApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
