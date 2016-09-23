@ECHO OFF
ECHO Creating questions.sqlite
sqlite3.exe questions.sqlite ".read questions.sql"
ECHO Done
pause