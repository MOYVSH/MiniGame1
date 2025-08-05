set WORKSPACE=..\..
set LUBAN_DLL=%WORKSPACE%\LubanConfig\Template\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -d bin ^
    -c cs-bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=%WORKSPACE%\Assets\Game\MiniGame_Res\Config ^
    -x outputCodeDir=%WORKSPACE%\Assets\Game\Scripts\ConfigCode
pause