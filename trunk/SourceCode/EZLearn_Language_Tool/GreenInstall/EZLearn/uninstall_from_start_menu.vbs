	'把 EZLearn.lnk 從 開始 menu 移除
	Set oWS = WScript.CreateObject("WScript.Shell")
	Set objShell = CreateObject("Shell.Application")
    Set objFolder = objShell.Namespace(oWS.CurrentDirectory)
    Set objApp = objFolder.ParseName("EZLearn.LNK")
    If objApp Is Nothing Then
        MsgBox ("Executable not found.  Make sure the path and file name are correct.")
    Else
        objApp.InvokeVerb ("Unp&in from Start Menu")
    End If

	' 參考資料:
	'http://www.vbforums.com/showthread.php?t=534820
	