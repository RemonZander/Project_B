cmd /c 'dotnet run --project "..\..\..\Project B V2.0.csproj" 5-19-2023 < input.txt'

$content1 = (Get-Content -Path ".\output.txt" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\output.txt" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    throw "The output files are not identical. Test failed!"
} else {
    Write-Host "The output files are identical."
}

$content1 = (Get-Content -Path ".\gebruikers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\gebruikers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    throw "The gebruikers.json files are not identical. Test failed!"
} else {
    Write-Host "The gebruikers.json files are identical."
}

$content1 = (Get-Content -Path ".\rondleidingen.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\rondleidingen.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    throw "The rondleidingen.json files are not identical. Test failed!"
} else {
    Write-Host "The files rondleidingen.json are identical."
}

$content1 = (Get-Content -Path ".\rondleidingenweekschema.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\rondleidingenweekschema.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    throw "The rondleidingenweekschema.json files are not identical. Test failed!"
} else {
    Write-Host "The files rondleidingenweekschema.json are identical."
}

Write-Host "test SUCCES!!!"