@echo off
chcp 65001 > nul
echo 正在编译大乐透随机选号器...

set CSC="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
set REFS=/reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.dll

%CSC% /target:winexe /optimize %REFS% Program.cs LottoForm.cs

if %ERRORLEVEL% equ 0 (
    echo 编译成功！生成文件: DaLeTou.exe
) else (
    echo 编译失败，请检查上面的错误信息。
)
pause
