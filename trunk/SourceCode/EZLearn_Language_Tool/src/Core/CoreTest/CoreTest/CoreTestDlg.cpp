// CoreTestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CoreTest.h"
#include "CoreTestDlg.h"
#include <AudioPlayerFunctions.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// 把计ㄏノ场だ
struct MyThreadInfo{
	float fPlayRate;
};

// CCoreTestDlg dialog
// Thread function 场だ
UINT  MyThreadFun(LPVOID LParam){
	MyThreadInfo *pInfo1=(MyThreadInfo*) LParam;

	BOOL bret=Play_File_Test(pInfo1->fPlayRate);
	if(FALSE==bret){
		MessageBox(NULL,_T("Play_File_Test failure"),_T("Error"),MB_OK);
	}
	return(0);
}



MyThreadInfo Info1;
// タ盽硉冀
void CCoreTestDlg::OnBnClickedButton1()
{
	Info1.fPlayRate=1.0f;
	AfxBeginThread(MyThreadFun,(LPVOID)&Info1);	
}

// 搭硉冀
void CCoreTestDlg::OnBnClickedOk()
{
	
	Info1.fPlayRate=0.5f;
	AfxBeginThread(MyThreadFun,(LPVOID)&Info1);	
	
	// OnOK();
}

// 0.8 冀
void CCoreTestDlg::OnBnClickedButton2()
{
	
	Info1.fPlayRate=0.8f;
	AfxBeginThread(MyThreadFun,(LPVOID)&Info1);	
}



CCoreTestDlg::CCoreTestDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CCoreTestDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CCoreTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CCoreTestDlg, CDialog)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDOK, &CCoreTestDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDC_BUTTON1, &CCoreTestDlg::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &CCoreTestDlg::OnBnClickedButton2)
END_MESSAGE_MAP()


// CCoreTestDlg message handlers

BOOL CCoreTestDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CCoreTestDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CCoreTestDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}




