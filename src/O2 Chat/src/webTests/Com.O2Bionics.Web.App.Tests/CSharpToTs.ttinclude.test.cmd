pushd t4
if exist ts\contract\*.ts del /F /Q ts\contract\*.ts

Echo "There must be exceptions in the console output as the test t4 file contains errors."
Echo "The generated .ts files must stay the same as before."
call ..\..\..\web\com.o2bionics.chat.app\buildTsContract.cmd
popd