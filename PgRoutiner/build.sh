echo starting main build... see more on targets on rid catalog: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
echo *******************************************************************************************************************

echo cleaning solution
echo *****************
echo dotnet clean --configuration SelfContained
dotnet clean --configuration SelfContained

echo win10-x64 publish
echo *****************
echo dotnet publish --runtime win10-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishReadyToRun=true --self-contained --output "_exe\win10-x64"
dotnet publish --runtime win10-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishReadyToRun=true --self-contained --output "_exe\win10-x64"

echo linux-x64 publish
echo *****************
echo dotnet publish -r linux-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\linux-x64"
dotnet publish --runtime linux-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\linux-x64"

echo osx-x64 publish
echo *****************
echo dotnet publish -r osx-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\osx-x64"
dotnet publish --runtime osx-x64 --configuration SelfContained /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained --output "_exe\osx-x64"
