	'在目前目錄建立 shortcut
	Set oWS = WScript.CreateObject("WScript.Shell")
	sLinkFile = oWS.CurrentDirectory+"\EZLearn.LNK"
    Set oLink = oWS.CreateShortcut(sLinkFile)
	oLink.TargetPath = oWS.CurrentDirectory+"\EZLearn.exe"
    oLink.WorkingDirectory = oWS.CurrentDirectory' 設定捷徑的工作路徑: 與執行檔一樣   (如果不指定 ,則預設路徑為  C:\Documents and Settings\使用者帳號)
	oLink.Save
	
	'把 shortcut 固定到開始 menu
	Set objShell = CreateObject("Shell.Application")
    Set objFolder = objShell.Namespace(oWS.CurrentDirectory)
    Set objApp = objFolder.ParseName("EZLearn.LNK")
    If objApp Is Nothing Then
        MsgBox ("Executable not found.  Make sure the path and file name are correct.")
    Else
        objApp.InvokeVerb ("P&in to Start Menu")
    End If

	' 參考資料:
	'http://www.vbforums.com/showthread.php?t=534820
	