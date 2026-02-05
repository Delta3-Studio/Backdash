#!/bin/bash
export LOBBY_SERVER_URL=http://localhost:9999
pushd "$(dirname "$0")/../../../LobbyServer" || exit
git submodule update --init --recursive "$(pwd)"
git submodule update --recursive --remote "$(pwd)"
dotnet run -c Release ./src/LobbyServer --project "$(pwd)/src/LobbyServer"
popd || exit
