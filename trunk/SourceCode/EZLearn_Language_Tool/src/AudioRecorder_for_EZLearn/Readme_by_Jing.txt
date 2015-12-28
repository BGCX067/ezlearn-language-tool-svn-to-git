- 聲音擷取技術 (使用 DirectShow)
包含: 如何選取擷取裝置, 擷取的聲音輸入

Step 1: 把 WavDest_for_EZLearn Project 放到下面的位置, 他會產生 WavDest.ax
F:\Program Files\Microsoft Platform SDK for Windows Server 2003 R2\Samples\Multimedia\DirectShow\Filters

Step 2: 把 AudioRecord_for_EZLearn 放到
F:\Program Files\Microsoft Platform SDK for Windows Server 2003 R2\Samples\Multimedia\DirectShow\Capture

Step 3: 編譯 AudioRecord 專案, 會產生 AudioRecorder.exe


- 與原版不同的地方
WavDest: 移除了 largeint.lib 的連結, 增加 DirectX 的 link path
AudioCap: 增加 DirectX 的 link path