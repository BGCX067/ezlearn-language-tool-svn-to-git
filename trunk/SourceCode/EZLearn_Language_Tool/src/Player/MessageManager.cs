using System;
using System.Xml;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
/// <summary>
/// 多國語言處理類別
/// 因應多國語言的需求, 特別設計一個以 XML 為存檔格式的類別
/// Usage:
///  
///            MessageManager L_Manager = new MessageManager("Language.xml");
///            L_Manager.CreateNewMessageTable();
///            L_Manager.Load_MultiLanguage();
///            String strTitles=L_Manager.getMessage("ProgramTitle", "S_Chinese"); // ProgramTitle 簡體版
/// 
/// Abstract Storage Structure 
///   [ item 1]
///   [ item 2] ----> ArrayList ->[A lang][...]
//                              ->[B lang][...]

// XML File Example
/*
 string[] LanguageTable ={      "<!-- MultiLanguage Setup File (多國語言設定檔)",
                                     " ",
                                     "    You can add your native language to the EZLearn Helper.",
                                     "    你可以加入自己國家慣用的語言到 EZLearn 語言學習機.",
            						 "    Ex:",
                        			 "<LanguageItem Title=\"Version_Text\"",
                                     "T_Chinese=\"版本\"",
                                     "S_Chinese=\"版本\"",
                                     "English=\"Ver.\"",
                                     "/>",
                                     " ",
                                     "    Manual Edit: NotePad.exe (*the saving format, please choose UTF-8).",
									 "-->",

									 "<EZLearn_Multi_Language FileName=\"Default_Mutilanguage_Data\" >",
									 " ",

									 "<Title value=\"ProgramTitle\" >",
                                     "    <Language Name=\"T_Chinese\" ",
                                     "         value=\"EZLearn 語言學習機\"",
                                     "    />",
                                     "    <Language Name=\"S_Chinese\" ",
                                     "         value=\"EZLearn 言机\"",
                                     "    />",
                                     "    <Language Name=\"English\" ",
                                     "         value=\"EZLearn Helper\"",
                                     "    />",
                                     "</Title>",
                                     " ",

                                     "<Title value=\"menuItem9.Text\" >",
                                     "    <Language Name=\"T_Chinese\" ",
                                     "         value=\"功能\"",
                                     "    />",
                                     "    <Language Name=\"S_Chinese\" ",
                                     "         value=\"功能\"",
                                     "    />",
                                     "    <Language Name=\"English\" ",
                                     "         value=\"Function\"",
                                     "    />",
                                     "</Title>",
                                     " ",

                                     "<Title value=\"menuItem34.Text\" >",
                                     "    <Language Name=\"T_Chinese\" ",
                                     "         value=\"網際網路上的電子字典\"",
                                     "    />",
                                     "    <Language Name=\"S_Chinese\" ",
                                     "         value=\"网网路上的子字典\"",
                                     "    />",
                                     "    <Language Name=\"English\" ",
                                     "         value=\"On-Line Dictionaries\"",
                                     "    />",
                                     "</Title>",
                                     " ",
									 "</EZLearn_Multi_Language>"};
*/
/// </summary>
namespace Player {
    public class MessageManager {

        public string strDefaultLanguage = "English"; // T_Chinese ; S_Chinese ; English


        string[] LanguageNameArray ={ "T_Chinese", "S_Chinese", "English" };
        string[][] LanguageItemTable ={
            //            Item Name    , T_Chinese, S_Chinese, Engish, Other etc.

            // 系統資訊
            new string[]{"Version_Text", "版本", "版本", "Ver." },
            new string[]{"ProgramTitle", "EZLearn 語言學習機", "EZLearn 语言学习机", "EZLearn Language Helper" },
            new string[]{"System_Second", "秒數", "秒数", "Sec" },
            new string[]{"System_AllStart", "開始播放", "开始播放", "Start Play" },
            new string[]{"System_Resume", "繼續", "继续", "Resume" },
            new string[]{"System_PauseMessage", "暫停", "暂停", "Pause" },
            new string[]{"System_PauseTitle", "暫停播放", "暂停播放", "Pause" },
            new string[]{"System_AllStopMessage", "播放停止", "播放停止", "Stop Play" },
            new string[]{"System_AllStopTitle", "語音檔播放停止", "语音文件播放停止", "Stop" },


            // 主面版
            new string[]{"MainMenu_File", "檔案", "档案", "File" },
            new string[]{"MainMenu_Open", "開啟語音檔", "开启语音文件", "Open an audio file" },
            new string[]{"MainMenu_Close", "關閉", "关闭", "Close" },

            new string[]{"MainMenu_Function", "功能", "功能", "Ops" },
            // 功能
            new string[]{"On_Line_Dictionary", "網路字典", "网路字典", "On-line Dictionaries" },
            new string[]{"Collegite_Dictionary", "Collegite Dictionary (英英字典 有聲音)", "Collegite Dictionary (英英字典 有声音)", "Collegite Dictionary (audio)" },
            new string[]{"Chinese_Dictionary", "國語字典", "國語字典", "Chinese Dictionary (Tawian)" },
            new string[]{"CAMBRIDGE_Dictionary", "CAMBRIDGE Dictionary (用字比較簡單)", "CAMBRIDGE Dictionary (用字比较简单)", "CAMBRIDGE Dictionary (for Beginner)" },
            new string[]{"Add_New_Item", "(+) 新增項目 ...", "(+) 新增项目 ...", "(+) Add a new Dictionar" },

            
            
            new string[]{"Option", "選項 ...", "选项 ...", "Option ..." },
            new string[]{"Always_On_Top_On", "永遠在最上層 (開啟)", "Always on Top (ON)", "Always on Top (ON)" },
            new string[]{"Always_On_Top_Off", "永遠在最上層 (關閉)", "Always on Top (OFF)", "Always on Top (OFF)" },
            new string[]{"AudioRecorder", "錄音", "录音", "Audio Recorder" },
           

            // 字幕選項
            new string[]{"Lyric_Option", "字幕選項 ...", "字幕选项 ...", "Lyric Option ..." },
            new string[]{"Lyric_Function_On_Off", "顯示/關閉 字幕功能 (預設是開啟)", "显示/关闭字幕功能(预设是开启)", "Show/Off Lyric Function" },
            new string[]{"Lyric_Function_Show_Next_Text", "顯示下一句", "显示下一句", "Show Next Text" },
            

            // 測驗選項
            new string[]{"Test_Main", "測驗", "测验", "Games" },
            new string[]{"Test_Japaness_alphabet", "日文字母遊戲", "日文字母游戏", "Japanese Alphabet Game" },
            new string[]{"Test_Japaness_Syllabary", "日文五十音圖 (漢字對照表)", "日文五十音图 (汉字对照表)", "Japaness Syllabary" },
            
            // Version 選項
            new string[]{"Version_String", "版本: ", "版本: ", "Ver.  " },
            new string[]{"Version_NewVersionMessage", "EZLearn 已經有新版本了", "EZLearn 已经有新版本了", "A new version is released!" },
            new string[]{"Version_This_Is_Beta", "EZLearn (Beta 版本)", "EZLearn (Beta 版本)", "EZLearn (Beta)" },
            new string[]{"Version_FirewallProglem", "版本更新檢查, 有問題 (請檢查防火牆) ...", "版本更新检查, 有问题(请检查防火墙) ...", "Version Checking has problem. (Please check the firewall setting) ..." },
            new string[]{"Version_This_is_new", "最新版本", "最新版本", "This is a new version" },
            new string[]{"Version_Update", "Update", "Update", "Update" },
            new string[]{"Version_CurrentVersionString", "目前版本資訊:", "目前版本信息:", "Page" },
            new string[]{"Version_CheckingWaitingMessage", "版本檢查中, 請等一下...", "版本检查中, 请等一下...", "Please wait (checking the version)" },

            

            // Help 
              new string[]{"Help_String", "手冊", "手冊", "Help" },
              new string[]{"Help__1", "1. 按數字後, 按 Enter 可索引播放位置", "1. 按数字后, 按Enter 可索引播放位置", "1. Input the audio index you want to play (ex:32.4)" },
              new string[]{"Help__2", "2. 重複播放 --> 按 [Enter] 鍵 or [Shift] 鍵", "2. 重复播放--> 按[Enter] 键 or [Shift] 键", "2. Replay again --> [Enter] or [Shift]"},
              new string[]{"Help__3", "3. 索引之前的語音位置 --> 按 [上/下] 鍵", "3. 索引之前的语音位置--> 按[上/下] 键", "3. Replay the last section --> [Up] or [Down]" },
              new string[]{"Help__4", "4. 暫停 --> 按 [Ctrl] 鍵", "4. 暂停--> 按[Ctrl] 键", "4. Pause --> [Ctrl]" },
              new string[]{"Help__5", "5. 可直接輸入數字, 不必清除前面的數字", "5. 可直接输入数字, 不必清除前面的数字", "5. You can direct input the section command without erasing the previous typing" },
              new string[]{"Help__FigureHelp", "圖解說明", "图解说明", "Picture Helper" },
            
            new string[]{"Help__OnLineDocument", "線上說明", "联机帮助", "On-line Document" },
            new string[]{"Help__DevelopInfo", "EZLearn 開發資訊", "EZLearn 开发信息", "DevelopInfo" },
            new string[]{"Help__OtherProduct_1", "朗文英英字典輔助工具", "朗文英英字典辅助工具", "Longman Dictionary Helper" },

            // 語言
            new string[]{"Language__Item", "語言", "语言", "Language" },
            new string[]{"Language__TranChinese", "正體中文", "正体中文", "Chinese (Tranditional)" },
            new string[]{"Language__SimpleChinese", "簡體中文", "简体中文", "Chinese (Simple)" },
            new string[]{"Language__English", "English", "English", "English" },


            // ToolTips  選項

            // 主選單
            new string[]{"MainToolTips__Input", "請輸入索引秒數  (↑上一個索引) (↓下一個索引)\n簡單格式: 起始索引\n例如:  113\n\n擴充格式: 起始 [,結束][,重複次數][,註解]\n例如:\n    113,115,20,讀賣新聞", "请输入索引秒数 (↑上一个索引) (↓下一个索引)\n简单格式: 起始索引\n例如:  113\n\n扩充格式: 起始[,结束][,重复次数][,批注]\n例如:\n    113,115,20,读卖新闻", "Input audio index Command  (↑ Previous) (↓ Next)\n1. Start Index\nex:  113\n\n2. Start [,End][,Repeat Time][,Comment]\nex:\n    113,115,20,The New York City" },
            new string[]{"MainToolTips__Pause", "暫停", "暂停", "Pause" },
            new string[]{"MainToolTips__Resume", "繼續", "继续", "Resume" },
            new string[]{"MainToolTips__Scroll", "拖曳滑鼠可直接跳過熟悉語音段落", "拖曳鼠标可直接跳过熟悉语音段落", "Scoll the bar, you can skip the audio section" },
             new string[]{"MainToolTips__ClearAudioSection", "清除串列", "清除串行", "Clear Section" },
             new string[]{"MainToolTips__ClearCurSection", "移除目前的索引", "移除目前的索引", "Clear Current Section" },
             new string[]{"MainToolTips__DisplaySectionWindow", "顯示索引面版", "显示索引面版", "Open Information Window" },

          

            // skin
            new string[]{"Skin_SaveMessage", "面版更新完成", "面版更新完成", "Skin Info Saved!" },

            // 語音片段
             new string[]{"AudioSection_Section_On", "片段播放 ON", "片段播放 ON", "Section Play On" },
            new string[]{"AudioSection_Section_Off", "片段播放 Off", "片段播放 Off", "Section Play Off" },
            new string[]{"AudioSection_Section_AutoRepeatOn", "語音片段自動循環  ON", "语音片段自动循环 ON", "Section Auto Repeat ON" },
            new string[]{"AudioSection_Section_RepeatPlay", "重複播放", "重复播放", "Repeat Play" },
            new string[]{"AudioSection_Section_Error_WrongCommand", "狀態: 錯誤的索引", "状态: 错误的索引", "You enter a wrong command" },
            new string[]{"AudioSection_Section_IndexMessage", "索引位置 ", "索引位置 ", "Index to " },
            new string[]{"AudioSection_Section_RemovedMessage", " 已從串列中移除", " 已从串行中移除", " was removed" },
            new string[]{"AudioSection_Section_DoNotRemoveMessage", " 不在串列中, 不動作", " 不在串行中, 不动作", " does not exist" },


            // IndexList 面版
             new string[]{"IndexListForm_System_Start", "起始:", "起始:", "Start:" },
             new string[]{"IndexListForm_System_End", "結束:", "結束:", "End:" },
              new string[]{"IndexListForm_System_ListenAgain", "再聽一次", "再听一次", "Listen Again" },

            // IndexList ToolTips
             new string[]{"IndexListForm_Tool_ModifySection", "修改語音片段 ", "修改语音片段 ", "Modify the Section Command" },
             new string[]{"IndexListForm_Tool_SectionAutoRepeat", "語音片段自動循環 (從第一段開始播放)", "语音片段自动循环(从第一段开始播放)", "Audio Section Auto Repeat" },
             new string[]{"IndexListForm_Tool_RepeatPlay", "目前片段無限重複播放", "目前片段无限重复播放", "Current Section Repeat Play" },
            new string[]{"IndexListForm_Tool_Delete", "刪除->目前的片段 [Del]", "刪除->目前的片段 [Del]", "Delete current Section [Del]" },
            new string[]{"IndexListForm_Tool_Sorting", "排序->語音片段(由小到大)", "排序->语音片段(由小到大)", "Sorting the Section" },
            new string[]{"IndexListForm_Tool_DeleteAllSection", "刪除所有的片段", "刪除所有的片段", "Delete all Section" },
            new string[]{"IndexListForm_Tool_OpenLesteningBombWindow", "開啟聽力轟炸面版", "开启听力轰炸面版", "Open the Listening Bomb Window" }, 
            new string[]{"IndexListForm_Tool_AddnewSection", "新增->目前的語音片段", "新增->目前的语音片段", "Add a new Audio Section" },

           
            // 聽力轟炸面版
            new string[]{"LesteningBombWindow_System_Title", "聽力轟炸面版", "听力轰炸面版", "Audio Bombing Window" },
            new string[]{"LesteningBombWindow_System_FileLoop", "目前檔案片段播放完畢, 繼續播放", "目前档案片段播放完毕, 继续播放", "Single file Section repeat" },
            new string[]{"LesteningBombWindow_System_SectionLoop", "片段循環", "片段循环", "Section Loop" },

            new string[]{"LesteningBombWindow_System_FileRepeatMessage", "目前檔案片段播放完畢, 繼續播放", "目前档案档片段播放完毕, 继续播放", "Single File Section Repeat" },
            new string[]{"LesteningBombWindow_System_FileLoopOnMessage", "單一檔案循環 ON", "单一档案循环 ON", "Single File Loop ON" },
            new string[]{"LesteningBombWindow_System_FileLoopOffMessage", "單一檔案循環 OFF", "单一档案循环 OFF", "Single File Loop OFF" },

             new string[]{"LesteningBombWindow_System_OpenEZUFile", "請指定檔案名稱", "请指定文件名", "Open File" },
            new string[]{"LesteningBombWindow_System_SaveEZUFile", "請設定檔案名稱", "请设定文件名", "Save" },

            //  聽力轟炸面版 XML 
             new string[]{"LesteningBombWindow_System_XMLError", "XML 語法檢查錯誤", "XML 语法检查错误", "XML Pasing Error" },
            new string[]{"LesteningBombWindow_System_SaveOK", "聽力轟炸面版資訊: 多檔資訊存檔成功","听力轰炸面版信息: 多文件信息存盘成功", "Multi File Information Saved!" },

            // 聽力轟炸面版 ToolTips
            new string[]{"LesteningBombWindow_Tool_OpenFileList", "開啟檔案列", "开启档案列", "Open the files you want to listen" },
            new string[]{"LesteningBombWindow_Tool_SaveFileList", "儲存目前檔案串列", "储存目前档案串行", "Save the files into an configure file" },
            new string[]{"LesteningBombWindow_Tool_ClearFileList", "清除列表", "清除列表", "Clear all File" },

            // Option 面版
            new string[]{"Option_Title", "Option Setting...", "Option Setting...", "Option Setting..." },
            new string[]{"Option_Generail", "一般", "一般", "General" },
            new string[]{"Option_CurDir", "目前路徑", "目前路径", "Current Directory" },
             new string[]{"Option_Skin", "面版", "面版", "Skin" },
             new string[]{"Option_SkinTheme", "面版主題", "面版主題", "Skins" },
            new string[]{"Option_Author", "作者", "作者", "Author" },
            new string[]{"Option_Announce", "基於知識無價的原則, 你可以自由散佈, 重製整個軟體", "基于知识无价的原则, 你可以自由散布, 重制整个软件", "This software is created for one who want to speak or listen fluent language. It's free." },
             new string[]{"Option_CopyRight", "版權聲明", "版权声明", "Copyright Info" },
            new string[]{"Option_Suggestion", "也歡迎提供功能上的任何建議", "也欢迎提供功能上的任何建议", "Any suggestion is welcome !" },
             new string[]{"Option_String1", "這是一個非常陽春的工具, 歡迎大家使用", "这是一个非常阳春的工具, 欢迎大家使用", "The EZLearn is a very simple tool for learning language. Wish you enjoy a happy learning day!" },
             new string[]{"Option_AuthorName", "井民全", "井民全", "Tomas Jing (井民全)" },


             new string[]{"Option_Keyboard_Title", "鍵盤設定", "键盘设定", "Keyboard" },
             new string[]{"Option_Keyboard_LeftQIndex", "Left 向左快速索引", "Left 向左快速索引", "Left Quick Index" },
             new string[]{"Option_Keyboard_RightQIndex", "Right 向右快速索引", "Right 向右快速索引", "Right Quick Index" },

            new string[]{"End", "結尾標記", "结尾标记", "End" }
                };

        string[] UsageText ={"<!-- MultiLanguage Setup File (多國語言設定檔)",
                                     " ",
                                     "    You can add your native language to the EZLearn Helper.",
                                     "    你可以加入自己國家慣用的語言到 EZLearn 語言學習機.",
            						 "    Ex:",
                        			 "<LanguageItem Title=\"Version_Text\"",
                                     "T_Chinese=\"版本\"",
                                     "S_Chinese=\"版本\"",
                                     "English=\"Ver.\"",
                                     "/>",
                                     " ",
                                     "    Manual Edit: NotePad.exe (*the saving format, please choose UTF-8).",
									 "-->"};

        System.Collections.Hashtable LanguageTable = new System.Collections.Hashtable();
        /*
        public MessageManager(String strFilename) {
            this.strFilename = strFilename;

        }

        */

        // 建立多國語言屬性訊息表
        public void CreateNewMessageTable(string strFilename) {

            string[] Default = CreateXMLConfigString(LanguageItemTable);

            // Create a initial mult-language setup file
            Create_A_New_XML_File(strFilename, Default);
        }


        public string getMessage(string strTitle) {
            return getMessage(strTitle, strDefaultLanguage);
        }

        public string getMessage(string strTitle, string strLanguage) {
            // Step 1: 由 strTitle 選擇想要查詢的項目, 傳回語言串列
            ArrayList LanguagePairList = (ArrayList)LanguageTable[strTitle];

            if (LanguagePairList == null)// 若沒有對應的 item 文字設定, 回傳 ?
                return "?";

            // Step 2: 比對是否有 strLanguage 的描述
            for (int i = 0; i < LanguagePairList.Count; i++) {
                string[] strLanguageNameValuePair = (string[])LanguagePairList[i];
                if (strLanguageNameValuePair[0].Equals(strLanguage)) {
                    return strLanguageNameValuePair[1]; // 回傳該語言的文字描述
                }
            }

            // 若沒有該語言文字描述, 則傳回 英文描述
            return getMessage(strTitle, "English");
        }

        public void LoadLanguageFromNet() {


            // Step 1: 連到多國語言設定網頁
            string myuri = "http://ezlearnlanguage.wiki.zoho.com/HomePage.html";
            WebRequest webr = WebRequest.Create(myuri);

            // Step 2: 取得網頁的串流物件
            Stream rc = webr.GetResponse().GetResponseStream();
            StreamReader read = new StreamReader(rc, System.Text.UTF8Encoding.UTF8); // 必须指定 UTF8 编码, 否则中文会变成乱码

            // Step 3: 將資料讀取出來
            while (true) {
                string abc = read.ReadLine();
                if (abc == null)
                    break;
                int l1 = abc.IndexOf("多國語言設定檔");

                if (l1 != -1) // 尋找多國語言設定檔 段落
                {
                    // Console.Write(abc);

                    string LanguageConfigText = abc.Replace("&lt;", "<").Replace("<br>", "\r\n").Replace("&nbsp;", " ").Replace("&gt;", ">");
                    Console.Write(LanguageConfigText);
                }
            }
            // Console.Write("=================================");
            // int l1 = abc.IndexOf("多國語言設定檔");

        }

        // 由 strFilename 中載入語言設定資料
        public bool Load_MultiLanguage(String strFilename) {
            XmlValidatingReader reader = null;


            try {
                XmlTextReader txtreader = new XmlTextReader(strFilename);
                txtreader.WhitespaceHandling = WhitespaceHandling.None;

                //Implement the validating reader over the text reader. 
                reader = new XmlValidatingReader(txtreader);
                reader.ValidationType = ValidationType.None;


                //Parse the XML fragment.  If they exist, display the   
                //prefix and namespace URI of each element.


                // 讀取一行 XML 指令
                ArrayList Language_Name_ValuePairArray = null;

                while (reader.Read()) {

                    switch (reader.NodeType) {
                        // 讀取 value = "字串"

                        case XmlNodeType.Element:
                        //Console.WriteLine("<{0}>", reader.LocalName); // Debug Information


                        string ElementName = reader.LocalName;
                        String strTitle, strLanguageName, strLanguageValue;
                        int Step = 0;

                        switch (ElementName) {
                            case "LanguageItem":
                            // 讀取各種語言的文字說明
                            String[] strLanguageItemArray = new String[reader.AttributeCount];
                            for (int i = 0; i < reader.AttributeCount; i++) {
                                strLanguageItemArray[i] = (string)reader.GetAttribute(i);

                            }

                            // 讀完後, 放到 Hash 中
                            string Title = strLanguageItemArray[0]; // 利用 變數名稱當作 key, 把多國語言陣列放到 Hash 中
                            LanguageTable.Add(Title, strLanguageItemArray);
                            break;

                            case "Title":
                            strTitle = (string)reader.GetAttribute(0);
                            // 遇到新的 變數Title, 建立語言對應表
                            // [ ]
                            // [ ] ----> ArrayList ->[A lang][...]
                            //                     ->[B lang][...]
                            Language_Name_ValuePairArray = new ArrayList();
                            LanguageTable.Add(strTitle, Language_Name_ValuePairArray);
                            break;
                            case "Language":
                            strLanguageName = (string)reader.GetAttribute(0);
                            strLanguageValue = (string)reader.GetAttribute(1);
                            String[] Language_Name_ValuePair = new String[2];
                            Language_Name_ValuePair[0] = strLanguageName;
                            Language_Name_ValuePair[1] = strLanguageValue;
                            if (Language_Name_ValuePairArray != null)
                                Language_Name_ValuePairArray.Add(Language_Name_ValuePair);
                            else
                                return false;
                            //Language_Name_ValuePairArray.Add(Language_Name_ValuePair); // 把收集到的目前語言收起來
                            break;
                        }

                        break;
                    }
                }
                txtreader.Close();

            } catch (System.Xml.XmlException ee) {
                System.Windows.Forms.MessageBox.Show("XML 語法檢查錯誤\n\n請修正 Dictionary.xml內容.\n\n如果想要回復預設值,\n請刪除 Dictionary.xml 檔即可");
                Console.WriteLine("Exception " + ee);
                return false;
            }
            return true;
        }






        class MessageTimer : System.Windows.Forms.Timer {
            public System.Windows.Forms.Label InfoLabel;
            string strMessage;
            public bool bAutoClear;

            // 建構子核心初始化函式
            private void initial(System.Windows.Forms.Label InfoLabel, string strMessage, int mil_sec, bool bAutoClear) {
                this.InfoLabel = InfoLabel;
                this.strMessage = strMessage;
                Interval = mil_sec;
                this.bAutoClear = bAutoClear;
            }

            // 核心建構子
            public MessageTimer(System.Windows.Forms.Label InfoLabel, string strMessage, int mil_sec, bool bAutoClear) {
                initial(InfoLabel, strMessage, mil_sec, bAutoClear);
            }

            // 方便建構子
            public MessageTimer(System.Windows.Forms.Label InfoLabel, string strMessage, int mil_sec) {
                bool bAutoClear = true;
                initial(InfoLabel, strMessage, mil_sec, bAutoClear);
            }

        }


        // 顯示訊息公用核心函式 (指定顯示秒數)
        public static void ShowInformation(System.Windows.Forms.Label InfoLabel, string strMessage, int mil_sec, bool bAutoClear) {
            InfoLabel.Text = strMessage; // 立即顯示
            MessageTimer myMessageTimer = new MessageTimer(InfoLabel, strMessage, mil_sec);
            myMessageTimer.Tick += new EventHandler(ShowMessage_TimerEventProcessor);
            myMessageTimer.Start(); // 自動清除計時開始

        }

        // 顯示訊息方便函式 
        public static void ShowInformation(System.Windows.Forms.Label InfoLabel, string strMessage, int mil_sec) {
            bool bAutoClear = true;
            ShowInformation(InfoLabel, strMessage, mil_sec, bAutoClear);
        }

        // 顯示訊息公用核心函式-- 事件處理器
        private static void ShowMessage_TimerEventProcessor(Object sender, EventArgs e) {
            MessageTimer myMessageTimer = (MessageTimer)sender;
            myMessageTimer.Stop(); // 停止計時

            if (myMessageTimer.bAutoClear) {
                myMessageTimer.InfoLabel.Text = "               ";
            }
        }


        // XML Utilities
        // Create a new XML file
        public void Create_A_New_XML_File(String strFilename, String[] XMLContent) {
            using (System.IO.StreamWriter writer = System.IO.File.CreateText(strFilename)) {
                for (int i = 0; i < XMLContent.Length; i++) {
                    writer.WriteLine(XMLContent[i]);
                }

            }// end of using
        }

        // 由下面的表格建立 XML 描述字串 (XMLContent)
        /*
                string[][] LanguageItemTable ={
                        //            Item Name    , T_Chinese, S_Chinese, Engish, Other etc.
                        new string[]{"Version_Text", "版本", "版本", "Ver." },
                        new string[]{"ProgramTitle", "EZLearn 語言學習機", "EZLearn 言机", "EZLearn Helper" }
                };
         
             XML 描述字串:
             string[] Default ={     "<EZLearn_Multi_Language FileName=\"Default_Mutilanguage_Data\" >",
									 "<Title value=\"ProgramTitle\" >",
                                     "    <Language Name=\"T_Chinese\" ",
                                     "         value=\"EZLearn 語言學習機\"",
                                     "    />",
                                     "    <Language Name=\"S_Chinese\" ",
                                     "         value=\"EZLearn 言机\"",
                                     "    />",
                                     "    <Language Name=\"English\" ",
                                     "         value=\"EZLearn Helper\"",
                                     "    />",
                                     "</Title>"
                                     "</EZLearn_Multi_Language>"
        */
        public string[] CreateXMLConfigString(string[][] LanguageItemTable) {
            System.Collections.ArrayList XMLConfigCollection = new System.Collections.ArrayList();


            for (int i = 0; i < UsageText.Length; i++) {
                XMLConfigCollection.Add(UsageText[i]);
            }

            // XML 語言描述檔 起始
            XMLConfigCollection.Add("<EZLearn_Multi_Language FileName=\"Default_Mutilanguage_Data\" >");
            for (int i = 0; i < LanguageItemTable.Length; i++) {
                string[] LanguageItemArray = LanguageItemTable[i];
                int LanguageNum = LanguageNameArray.Length;
                string Item = LanguageItemArray[0];

                string xmlTitle = "<Title value=\"" + Item + "\" >\r\n";
                string xmlItem_Text = "";
                for (int j = 0; j < LanguageNum; j++) {
                    string LanguageName = LanguageNameArray[j]; // 取得目前 語言名稱
                    string Item_Text = LanguageItemArray[j + 1]; // 取得目前 item 在 指定語言的文字表示

                    xmlItem_Text += "    <Language Name=\"" + LanguageName + "\" " +
                                 "         value=\"" + Item_Text + "\"" +
                                 "    />\r\n";
                }
                string xmlString = xmlTitle + xmlItem_Text + "</Title>\r\n";
                XMLConfigCollection.Add(xmlString);
            }
            XMLConfigCollection.Add("</EZLearn_Multi_Language>\r\n");
            string[] ConfigString_XMLString = new string[XMLConfigCollection.Count];
            for (int i = 0; i < XMLConfigCollection.Count; i++) {
                ConfigString_XMLString[i] = (string)XMLConfigCollection[i];
            }
            XMLConfigCollection = null;
            return ConfigString_XMLString;
        }



    }

}// end of Player namespace