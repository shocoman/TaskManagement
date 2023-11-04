#!/bin/env sh

rm -rf ./Data/Migrations
rm ./Data/tasks.db

dotnet ef migrations add InitialCreate -o ./Data/Migrations
dotnet ef database update
