@echo off
pushd %~dp0

echo Automatically serving sphinx docs
sphinx-autobuild . .\_build

IF %ERRORLEVEL% NEQ 0 Echo sphinx-autobuild is not installed. Run 'pip install sphinx-autobuild'

popd