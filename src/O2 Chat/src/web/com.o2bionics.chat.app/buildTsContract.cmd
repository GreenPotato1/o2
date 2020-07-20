@setlocal enableextensions enabledelayedexpansion

set exec="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\TextTransform.exe" 
if not exist %exec% set exec="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\TextTransform.exe" 
if not exist %exec% set exec="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\TextTransform.exe"
if not exist %exec% set exec="C:\Program Files (x86)\Common Files\Microsoft Shared\TextTemplating\14.0\TextTransform.exe"

pushd .\ts\contract

for %%x in (*.t4) do (
  echo Transforming '%%x'.
  set y=%%x

  rem Replace .t4 with .ts
  set z=!y:~0,-1!s

  cmd.exe /c call %exec% -out !z! %%x
)
popd
