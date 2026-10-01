@echo off
setlocal enabledelayedexpansion
title Cai Dat Environment Vietnam (TCVN) cho Tekla Structures
cls
echo ===============================================================================
echo      BO CAI DAT ENVIRONMENT VIETNAM (TCVN) CHO TEKLA STRUCTURES
echo ===============================================================================
echo.
echo Chon phien ban Tekla Structures ban muon cai dat Environment Vietnam:
echo.
echo   [1] Cai cho Tekla Structures 2020.0
echo   [2] Cai cho Tekla Structures 2025.0
echo   [3] Cai cho CA HAI phien ban (2020.0 va 2025.0)
echo   [4] Thoat
echo.
set /p opt="Nhap lua chon cua ban [1, 2, 3, 4]: "

if "%opt%"=="1" goto install_2020
if "%opt%"=="2" goto install_2025
if "%opt%"=="3" goto install_both
if "%opt%"=="4" goto end
echo Lua chon khong hop le.
pause
exit /b

:install_2020
echo.
echo -------------------------------------------------------------------------------
echo [TIEN HANH] Dang cai dat Environment Vietnam cho Tekla Structures 2020.0...
set TARGET_2020=C:\TeklaStructures\2020.0\Environments\Vietnam
if not exist "%TARGET_2020%" mkdir "%TARGET_2020%"
xcopy /E /Y /I "%~dp0Environments\Vietnam_2020" "%TARGET_2020%"
if %errorlevel% neq 0 (
    echo [LOI] Khong the ghi file vao %TARGET_2020%. Hay chay script voi quyen 'Run as administrator'!
) else (
    echo [THANH CONG] Da cai dat Environment Vietnam vao Tekla 2020.0:
    echo            %TARGET_2020%
)
goto finish

:install_2025
echo.
echo -------------------------------------------------------------------------------
echo [TIEN HANH] Dang cai dat Environment Vietnam cho Tekla Structures 2025.0...
rem Kiem tra ca 2 duong dan thuong gap cua Tekla 2025
set TARGET_2025_A=C:\ProgramData\Trimble\Tekla Structures\2025.0\Environments\Vietnam
set TARGET_2025_B=C:\TeklaStructures\2025.0\Environments\Vietnam

set TARGET_2025=%TARGET_2025_A%
if exist "C:\TeklaStructures\2025.0\" set TARGET_2025=%TARGET_2025_B%

if not exist "%TARGET_2025%" mkdir "%TARGET_2025%"
xcopy /E /Y /I "%~dp0Environments\Vietnam_2025" "%TARGET_2025%"
if %errorlevel% neq 0 (
    echo [LOI] Khong the ghi file vao %TARGET_2025%. Hay chay script voi quyen 'Run as administrator'!
) else (
    echo [THANH CONG] Da cai dat Environment Vietnam vao Tekla 2025.0:
    echo            %TARGET_2025%
)
goto finish

:install_both
echo.
echo -------------------------------------------------------------------------------
echo [TIEN HANH] Cai dat cho Tekla Structures 2020.0...
set TARGET_2020=C:\TeklaStructures\2020.0\Environments\Vietnam
if not exist "%TARGET_2020%" mkdir "%TARGET_2020%"
xcopy /E /Y /I "%~dp0Environments\Vietnam_2020" "%TARGET_2020%"

echo.
echo [TIEN HANH] Cai dat cho Tekla Structures 2025.0...
set TARGET_2025_A=C:\ProgramData\Trimble\Tekla Structures\2025.0\Environments\Vietnam
set TARGET_2025_B=C:\TeklaStructures\2025.0\Environments\Vietnam
set TARGET_2025=%TARGET_2025_A%
if exist "C:\TeklaStructures\2025.0\" set TARGET_2025=%TARGET_2025_B%
if not exist "%TARGET_2025%" mkdir "%TARGET_2025%"
xcopy /E /Y /I "%~dp0Environments\Vietnam_2025" "%TARGET_2025%"

echo.
echo [THANH CONG] Da hoan tat cai dat cho ca 2 phien ban!
goto finish

:finish
echo.
echo ===============================================================================
echo HUONG DAN SU DUNG:
echo 1. Khoi dong Tekla Structures (2020 hoac 2025).
echo 2. Tai man hinh dang nhap:
echo    - Environment : Chon "Vietnam"
echo    - Role        : Chon "Cast-in-Place" / "Steel Detailing" / "All"
echo.
echo 3. CONG CU NHAP VAT LIEU TCVN (MATERIAL IMPORTER):
echo    - Thu muc: Environments\Vietnam\material-add\
echo    - File:    TCVN_Material_Importer.exe (chay truc tiep hoac qua Macro Tekla)
echo    - Giup tu dong nap 39 mac vat lieu TCVN (Be tong, Cot thep, Thep KC) vao MATDB.BIN.
echo.
echo Chuc ban lam viec hieu qua voi mo hinh BIM chuan TCVN!
echo ===============================================================================
pause

:end