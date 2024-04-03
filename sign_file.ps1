<#
.SYNOPSIS
    This script signs a file using Minisign.

.DESCRIPTION
    The script takes an input file as a parameter, signs it using a private key, and verifies the signature. 
    The private key is fetched from an environment variable or a local file. 
    If the private key is not valid, the script will exit with an error. 
    The script also downloads and extracts Minisign if it's not already present.

.PARAMETER inputfile
    The file to be signed.

.EXAMPLE
    .\sign_file.ps1 -inputfile "path\to\file"

.NOTES
    The script requires Minisign to be available. If it's not, the script will download it from GitHub.
    The private key is expected to be in base64 format.
#>

param ([string] $inputfile)

# Vars
$secretKeyPathLocal = "D:\*\*\*\dhcgn-github.key.base64"
$secretKeyHash = "C73659FD39DC0C1A4A58E2CCB517322A78CB5D8DEAAFCE8E6D1A4BBEEFD08EFC"
$minisignDownloadUrl = 'https://github.com/jedisct1/minisign/releases/download/0.10/minisign-0.10-win64.zip'
$publicKey = "RWS6WvbGy1Vj62jz6zVQfFIy+gcXJVK1nyGOZxpOLOIQmhTziYNk9B/g"

# Secrets
$privateKey = $env:MINISIGN_KEY

if ($privateKey.Length -eq "") {
    Write-Host "::debug::Secret was empty"
    $privateKey = Get-Content $secretKeyPathLocal
}

$privateKey = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($privateKey))

Set-Content -Path private_key -Value $privateKey
Get-FileHash -Path private_key -Algorithm SHA256

if ((Get-FileHash -Path private_key -Algorithm SHA256).Hash -ne $secretKeyHash) {
    Write-Host "::error::Private key is not valid"
    Exit -1
}

if (-Not (Test-Path "minisign.zip")) {
    Invoke-WebRequest -Uri $minisignDownloadUrl -OutFile minisign.zip
}

Expand-Archive -Path minisign.zip -DestinationPath . -Force
Set-Alias -Name minisign -Value .\minisign-win64\minisign.exe

Write-Host "::debug::Sign file"
"" | minisign -Sm $inputfile -s private_key
Remove-Item -Path private_key -Force
if ($LASTEXITCODE -ne 0) {
    Write-Host "::error::Could't sign file"
    Exit -1
}

Write-Host "::debug::Verify signature"
minisign -VP $publicKey -m $inputfile
if ($LASTEXITCODE -ne 0) {
    Write-Host "::error::Couldn't verify signature"
    Exit -1
}