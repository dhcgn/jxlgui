$privateKey = $env:MINISIGN_KEY

if ($privateKey.Length -eq "")
{
    Write-Host "::debug::Secret was empty"
    $privateKey = Get-Content "D:\*\*\*\dhcgn-github.key.base64"
}

$privateKey = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($privateKey))

Set-Content -Path private_key -Value $privateKey
Get-FileHash -Path private_key -Algorithm SHA256

if ((Get-FileHash -Path private_key -Algorithm SHA256).Hash -ne "C73659FD39DC0C1A4A58E2CCB517322A78CB5D8DEAAFCE8E6D1A4BBEEFD08EFC")
{
    Write-Host "::error::Private key is not valid"
    Exit -1
}

if (-Not (Test-Path "minisign.zip")){
    Invoke-WebRequest -Uri 'https://github.com/jedisct1/minisign/releases/download/0.10/minisign-0.10-win64.zip' -OutFile minisign.zip
}

Expand-Archive -Path minisign.zip -DestinationPath . -Force
Set-Alias -Name minisign -Value .\minisign-win64\minisign.exe

Write-Host "::debug::Sign file"
"" | minisign -Sm .\output\jxlgui.wpf.exe -s private_key
Remove-Item -Path private_key -Force
if ($LASTEXITCODE -ne 0){
    Write-Host "::error::Could't sign file"
    Exit -1
}

Write-Host "::debug::Verify signature"
minisign -VP RWS6WvbGy1Vj62jz6zVQfFIy+gcXJVK1nyGOZxpOLOIQmhTziYNk9B/g -m .\output\jxlgui.wpf.exe
if ($LASTEXITCODE -ne 0){
    Write-Host "::error::Couldn't verify signature"
    Exit -1
}