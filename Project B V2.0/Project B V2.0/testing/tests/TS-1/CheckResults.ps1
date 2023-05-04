$content1 = Get-Content -Path output.txt -Raw
$content2 = Get-Content -Path "expected results\output.txt" -Raw

if ($content1 -ne $content2) {
    throw "The output files are not identical. Test failed!"
} else {
    Write-Host "The output files are identical."
}

$content1 = Get-Content -Path gebruikers.json -Raw
$content2 = Get-Content -Path "expected results\gebruikers.json" -Raw

if ($content1 -ne $content2) {
    throw "The gebruikers.json files are not identical. Test failed!"
} else {
    Write-Host "The gebruikers.json files are identical."
}

$content1 = Get-Content -Path rondleidingen.json -Raw
$content2 = Get-Content -Path "expected results\rondleidingen.json" -Raw

if ($content1 -ne $content2) {
    throw "The rondleidingen.json files are not identical. Test failed!"
} else {
    Write-Host "The files rondleidingen.json are identical."
}

Write-Host "test SUCCES!!!"