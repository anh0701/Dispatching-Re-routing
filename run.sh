#!/bin/bash

podman-compose -f ./D.API/mssql.yaml up -d

dotnet watch run --project ./D.API/D.API.csproj &
dotnet watch run --project ./D.UI/D.UI.csproj


