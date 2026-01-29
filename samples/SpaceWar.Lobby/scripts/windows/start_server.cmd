@set LOBBY_SERVER_URL=http://localhost:9999
@pushd %~dp0\..\..\..\LobbyServer
git submodule update --init --recursive %cd%
start dotnet run -c Release --project "%cd%\src\LobbyServer"
@popd
