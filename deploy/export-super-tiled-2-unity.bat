@echo off
pushd %~dp0

set UnityExe="C:\Program Files\Unity\Hub\Editor\2018.2.6f1\Editor\Unity.exe"
set UnityProj="../SuperTiled2Unity"
set UnityMethod=SuperTiled2Unity.Editor.ST2USettings.DeploySuperTiled2Unity

echo Deploying SuperTiled2Unity
echo Using Editor: %UnityExe%

start /wait "" %UnityExe% -quit --nographics -batchmode -projectPath %UnityProj% -executeMethod %UnityMethod% -logFile output.log
echo Done!

popd
