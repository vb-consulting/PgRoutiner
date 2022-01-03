REM starting main build... see more on targets on rid catalog: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
REM *******************************************************************************************************************

REM cleaning solution
REM *****************
dotnet clean --configuration SelfContained

REM win10-x64 publish
REM *****************
dotnet publish --runtime win10-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishReadyToRun=true --self-contained --output "_exe\win10-x64"

REM linux-x64 publish
REM *****************
dotnet publish --runtime linux-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\linux-x64"

REM osx-x64 publish
REM *****************
dotnet publish --runtime osx-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\osx-x64"
