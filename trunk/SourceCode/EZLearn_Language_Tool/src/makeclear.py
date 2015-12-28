# Make clear Python version
#                  by Jing
import os
import glob
import win32ui
import win32con

# Specified the file types which you want to clear
strDeletePattern=['*.exe','*.obj','*.pch','*.pdb','*.idb','*.ncb','BuildLog.*','*.ilk','*.suo','*.bak','*.class']

# save old directory info
oldDir=os.getcwd()
top=oldDir

# Creat a deletion string for prompting user
strDeleteString=""
for s in strDeletePattern:
    strDeleteString=strDeleteString+" "+s
    
# Show the prompt message to user 
ret=win32ui.MessageBox('Project Directory:\n\n\t'+top+'    '+"\n\n\tDelete"+strDeleteString+'\n\n Are you sure ? \n','Make Clear Operator',win32con.MB_OKCANCEL)
if(ret==win32con.IDCANCEL):
    print 'Cancel'
else:
    # txtFile=open('./remove.txt','w')
    count=0
    for root, dirs, files in os.walk(top, topdown=False):
        for name in dirs:
            FullDirname=os.path.join(root, name)
            #print FullDirname
            os.chdir(FullDirname) # into the directory
            for i in strDeletePattern :
                f=os.path.join(FullDirname, i)
                RemoveList=glob.glob(f)
                for x in RemoveList:
                    os.remove(x)
                    count=count+1
                    #txtFile.write(x)
                    #txtFile.write('\n')
    #txtFile.close()                  
    win32ui.MessageBox('%d files are removed!   '%count,'Job Done!',win32con.MB_OK)
    os.chdir(oldDir)











    




