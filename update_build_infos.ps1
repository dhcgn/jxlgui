Set-Location $PSScriptRoot

git rev-parse HEAD > .\jxlgui.buildinfo\assets\commitid
get-date -Format FileDateTimeUniversal > .\jxlgui.buildinfo\assets\date

$latesttag = $(git describe --abbrev=0 --tags)

if ($LASTEXITCODE -eq 0)
{
    $latesttag > .\jxlgui.buildinfo\assets\version
}else{
    "0.0.0" > .\jxlgui.buildinfo\assets\version
}

# Check
Write-Host ("::notice  title=commitid::{0}" -f (Get-Content .\jxlgui.buildinfo\assets\commitid))
Write-Host ("::notice  title=date::{0}" -f (Get-Content .\jxlgui.buildinfo\assets\date))
Write-Host ("::notice  title=version::{0}" -f (Get-Content .\jxlgui.buildinfo\assets\version))

Exit 0