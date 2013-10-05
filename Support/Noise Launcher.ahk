^m::
IfWinExist Noise At 10feet
{
    WinActivate
}
else
{
    Run J:\Sandbox\Noise\Noise.TenFooter\bin\x64\Debug\Noise.TenFooter.exe, J:\Sandbox\Noise\Noise.TenFooter\bin\x64\Debug
    WinWait Noise At 10feet
    WinActivate
}