if ($args.Length -eq 0) {return}

$fileSearch = $true
if ($args.IndexOf('-dr') -gt -1) {
    $fileSearch = $false
} 

foreach ($arg in $args) {
    if ($arg -notmatch '-dr') 
    {
        $param += "*$arg*"
    }
}

$type = @({directory},{file})[$fileSearch]

Write-Host "searching for $type - ""$($param.Replace('*', " ").Replace("  ", " ").Trim())"""

$fileName = Get-ChildItem -Recurse -File:($fileSearch) -Directory:(!$fileSearch) -ErrorAction SilentlyContinue -Filter $param | Select-Object -ExpandProperty FullName -First 1
if ($fileName) 
{
    Write-Host $fileName
    start-process -filepath $fileName
}
else 
{
    Write-Host "the $type was not found"
}
