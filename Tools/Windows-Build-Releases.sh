#!/bin/bash
set -e
script_dir=`pwd`
echo "Starting Unitystation buildscript from:"
echo $script_dir
cd ..
cd UnityProject
project_dir=$(pwd)
echo "Starting to build from Unityproject directory:"
echo $project_dir
winlog=$script_dir/Logs/WindowsBuild.log
osxlog=$script_dir/Logs/OSXBuild.log
linlog=$script_dir/Logs/LinuxBuild.log
srvlog=$script_dir/Logs/ServerBuild.log

echo "Attempting build of UnityStation for Windows"
/cygdrive/c/Programs/UEdit/Editor/Unity.exe \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $winlog \
	-executeMethod BuildScript.PerformWindowsBuild \
	-quit
rc0=$?
echo "Build logs (Windows)"
if pgrep -F "$winlog" &>/dev/null; 
	then cat $winlog 
else 
	echo nope 
fi


echo "Attempting build of UnityStation for OSX"
/cygdrive/c/Programs/UEdit/Editor/Unity.exe \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $osxlog \
	-executeMethod BuildScript.PerformOSXBuild \
	-quit
rc1=$?
echo "Build logs (OSX)"
if pgrep -F "$osxlog" &>/dev/null; 
	then cat $osxlog 
else 
	echo nope 
fi

echo "Attempting build of UnityStation for Linux"
/cygdrive/c/Programs/UEdit/Editor/Unity.exe \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $linlog \
	-executeMethod BuildScript.PerformLinuxBuild \
	-quit
rc2=$?
echo "Build logs (Linux)"
if pgrep -F "$linlog" &>/dev/null; 
	then cat $linlog 
else 
	echo nope 
fi

echo "Attempting build of UnityStation Server"
/cygdrive/c/Programs/UEdit/Editor/Unity.exe \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $srvlog \
	-executeMethod BuildScript.PerformServerBuild \
	-quit
rc3=$?
echo "Build logs (Server)"
if pgrep -F "$srvlog" &>/dev/null; 
	then cat $srvlog 
else 
	echo nope 
fi
echo "Building finished successfully"

echo "Post processing builds"
cp $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/libsteam_api64.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/libsteam_api64.so
cp $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/libsteam_api.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/libsteam_api.so
cp $script_dir/steam1007/linux64/steamclient.so $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins/x86_64/steamclient.so
cp -Rf $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Plugins $script_dir/ContentBuilder/content/Server/Unitystation-Server_Data/Mono

echo "Post-Processing done"
echo "Starting upload to steam"

echo "Please enter your steam developer-upload credentials"
read -p 'Username: ' uservar
read -sp 'Password: ' passvar

$script_dir/ContentBuilder/builder/steamcmd.exe +login $uservar $passvar <<EOF
run_app_build $script_dir/ContentBuilder/scripts/app_build_801140.vdf
quit
EOF