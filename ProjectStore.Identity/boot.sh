#!/bin/bash
dotnet ef database update
exec dotnet ProjectStore.Identity.dll