# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p3ppc.colourfulPartyPanel/*" -Force -Recurse
dotnet publish "./p3ppc.colourfulPartyPanel.csproj" -c Release -o "$env:RELOADEDIIMODS/p3ppc.colourfulPartyPanel" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location