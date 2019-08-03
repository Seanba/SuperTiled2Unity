@echo off
pushd %~dp0

set UnityExe="C:\Program Files\UnityEds\2019.2.0f1\Editor\Unity.exe"
set UnityProj="../SuperTiled2Unity"
set UnityMethod=SuperTiled2Unity.Editor.SuperTiled2Unity_Config.DeploySuperTiled2Unity

echo Deploying SuperTiled2Unity
echo Using Editor: %UnityExe%

start /wait "" %UnityExe% -quit --nographics -batchmode -projectPath %UnityProj% -executeMethod %UnityMethod% -logFile output.log
echo Done!

popd
