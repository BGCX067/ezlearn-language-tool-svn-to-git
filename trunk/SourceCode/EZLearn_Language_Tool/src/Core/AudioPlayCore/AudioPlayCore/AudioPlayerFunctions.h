#ifdef AudioPlayerFunctionsExport
   #define AudioPlayerAPI extern "C" __declspec(dllexport)
#else
   #define  AudioPlayerAPI extern "C" __declspec(dllimport)
#endif

AudioPlayerAPI BOOL Play_File_Test(float fPlayRate);


