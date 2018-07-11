#!/bin/bash
set -e
script_dir=`pwd`
echo "Starting Unitystation buildscript from:"
echo $script_dir
cd ..
cd UnityProject
project_dir=$(cygpath -m `pwd`)
echo "Starting to build from Unityproject directory:"
echo $project_dir
winlog=$script_dir/Logs/WindowsBuild.log
osxlog=$script_dir/Logs/OSXBuild.log
linlog=$script_dir/Logs/LinuxBuild.log
srvlog=$script_dir/Logs/ServerBuild.log
vdfdir=$(cygpath -m $script_dir/ContentBuilder/scripts/)

echo "Attempting build of UnityStation for Windows"
/cygdrive/c/Program\ Files/Unity/Editor/Unity.exe \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $(cygpath -m $winlog) \
	-projectPath $project_dir \
	-executeMethod BuildScript.PerformWindowsBuild \
	-quit
rc0=$?
echo "Build logs (Windows)"
cat $winlog 

 echo "Attempting build of UnityStation for OSX"
 /cygdrive/c/Program\ Files/Unity/Editor/Unity.exe \
 	-batchmode \
 	-nographics \
 	-silent-crashes \
 	-projectPath $project_dir \
 	-logFile $(cygpath -m $osxlog) \
 	-executeMethod BuildScript.PerformOSXBuild \
 	-quit
 rc1=$?
 echo "Build logs (OSX)"
 cat $osxlog 

 echo "Attempting build of UnityStation for Linux"
 /cygdrive/c/Program\ Files/Unity/Editor/Unity.exe \
 	-batchmode \
 	-nographics \
 	-silent-crashes \
 	-projectPath $project_dir \
 	-logFile $(cygpath -m $linlog) \
 	-executeMethod BuildScript.PerformLinuxBuild \
 	-quit
 rc2=$?
 echo "Build logs (Linux)"
 cat $linlog 
 
# todo, not working
# echo "Attempting build of UnityStation for Android"
# /cygdrive/c/Program\ Files/Unity/Editor/Unity.exe \
# 	-batchmode \
# 	-nographics \
# 	-silent-crashes \
# 	-projectPath $project_dir \
# 	-logFile $(cygpath -m $srvlog) \
# 	-executeMethod BuildScript.PerformAndroidBuild \
# 	-quit
# rc3=$?
# echo "Build logs (Android)"
# cat $srvlog 

# server build is now executed on server itself
# echo "Attempting build of UnityStation Server"
# /cygdrive/c/Program\ Files/Unity/Editor/Unity.exe \
# 	-batchmode \
# 	-nographics \
# 	-silent-crashes \
# 	-projectPath $project_dir \
# 	-logFile $(cygpath -m $srvlog) \
# 	-executeMethod BuildScript.PerformServerBuild \
# 	-quit
# rc3=$?
# echo "Build logs (Server)"
# cat $srvlog 

echo "Building finished successfully"

 # echo "Post processing builds"
 # cp $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/libsteam_api64.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/libsteam_api64.so
 # cp $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/libsteam_api.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/libsteam_api.so
 # cp $script_dir/steam1007/linux64/steamclient.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/steamclient.so
 # cp -Rf $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Mono

echo "Post-Processing done"
echo "Starting upload to steam"

echo "Please enter your steam developer-upload credentials"
read -p 'Username: ' uservar
read -sp 'Password: ' passvar
read -p 'Steam Guard Code (Optional) :' guardvar

$script_dir/ContentBuilder/builder/steamcmd.exe +login $uservar $passvar $guardvar <<EOF
run_app_build $vdfdir/app_build_801140.vdf
quit
EOF