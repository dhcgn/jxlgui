Set-Location $PSScriptRoot

git rev-parse HEAD > .\jxlgui.buildinfo\assets\commitid
get-date -Format FileDateTimeUniversal > .\jxlgui.buildinfo\assets\date

$latesttag = $(git describe --abbrev=0)

if ($LASTEXITCODE -eq 0)
{
    $latesttag > .\jxlgui.buildinfo\assets\version
}else{
    "0.0.0" > .\jxlgui.buildinfo\assets\version
}
