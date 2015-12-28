

// --------------- Utility.h -----------
HRESULT GetPin( IBaseFilter * pFilter, PIN_DIRECTION dirrequired, int iNum, IPin **ppPin);
HRESULT PrintAllFilter(IGraphBuilder *pGB);// 印出 Grpah Manager中管理的所有 Filters 名稱
HRESULT ShowFilterInfo(IBaseFilter *filter); // 察看 Filter 的資料

void DXShowError(HRESULT hr); // Show the error message
// 加入一個 Filter 到 指定的 filter graph 中
HRESULT AddFilterByCLSID(
    IGraphBuilder *pGraph,  // [in] 目前的 Filter Graph Manager
    const GUID& clsid,      // [in] 要加入的 Filer class id   例如: CLSID_AviDest
    LPCWSTR wszName,        // [in] 你的 Filter 要叫的名字
    IBaseFilter **ppF);      // [out] 回傳 Filter instance 位址

// 連接兩個 Filters
HRESULT ConnectFilters(
    IGraphBuilder *pGraph, // [in] 指定兩個 Filters 所在的 Filter Graph Manager
    IPin *pOut,            // [in] 指定 upstream filter 的輸出 pin 接腳
    IBaseFilter *pDest) ;
IPin * GetOutPin( IBaseFilter * pFilter, int nPin );

// 連接兩個 Filters
HRESULT ConnectDirectFilters(
    IGraphBuilder *pGraph, // [in] 指定兩個 Filters 所在的 Filter Graph Manager
    IPin *pOut,            // [in] 指定 upstream filter 的輸出 pin 接腳
    IBaseFilter *pDest,    // [in] 指定 downstream filter.
	const AM_MEDIA_TYPE *pmt)  ;

// 連接兩個 Filters [直接指定 Filter 版本]
HRESULT ConnectDirectFilters(
    IGraphBuilder *pGraph,  // [in] 指定兩個 Filters 所在的 Filter Graph Manager
    IBaseFilter *pSrc,      // [in] 指定 upstream filter 
    IBaseFilter *pDest,
	const AM_MEDIA_TYPE *pmt) ;    // [in] 指定 downstream filter

// 執行一個 Graph
HRESULT PlayFileWait(IFilterGraph *pFG);

TCHAR *getTimeFormatMessage(GUID *pFormat); // 傳回 TimeFormat 文字表示 (除錯使用);

// XVID CLSID=64697678-0000-0010-8000-00AA00389B71
DEFINE_GUID(CLSID_XVID,
0x64697678, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

// The Following Setting should be added to your project property
// Library Setting
// Library Path 
// F:\Program Files\Microsoft Platform SDK for Windows Server 2003 R2\Samples\Multimedia\DirectShow\Filters\PushSource\Debug_Unicode
// Library Dependencies: PushSource.lib
// --------------- end of PushSource  ------------