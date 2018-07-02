#!/bin/bash
set -e
script_dir=`pwd`
echo "Starting Unitystation buildscript from:"
echo $script_dir

echo "Please enter your steam developer-upload credentials"
read -p 'Username: ' uservar
read -sp 'Password: ' passvar

$script_dir/ContentBuilder/builder/steamcmd.exe +login $uservar $passvar <<EOF
run_app_build $script_dir/ContentBuilder/scripts/app_build_801140.vdf
quit
EOF