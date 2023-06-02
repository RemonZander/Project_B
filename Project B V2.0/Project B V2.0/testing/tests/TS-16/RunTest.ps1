cmd /c 'dotnet run --project "..\..\..\Project B V2.0.csproj" 5-19-2023 < input.txt'
$result = ""
$thow = false

$content1 = (Get-Content -Path ".\output.txt" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\output.txt" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    Write-Host "The output files are not identical. test failed!"
    $throw = true
    $result = $result + "The output files are not identical. test failed!\n"
} else {
    Write-Host "The output files are identical."
    $result = $result + "The output files are identical.\n"
}

$content1 = (Get-Content -Path ".\gebruikers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\gebruikers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    Write-Host "The gebruikers.json files are not identical. Test failed!"
    $throw = true
    $result = $result + "The gebruikers.json files are not identical. Test failed!\n"
} else {
    Write-Host "The gebruikers.json files are identical."
    $result = $result + "The gebruikers.json files are identical.\n"
}

$content1 = (Get-Content -Path ".\rondleidingen.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\rondleidingen.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    Write-Host "The rondleidingen.json files are not identical. Test failed!"
    $throw = true
    $result = $result + "The rondleidingen.json files are not identical. Test failed!\n"
} else {
    Write-Host "The files rondleidingen.json are identical."
    $result = $result + "The rondleidingen.json files are identical.\n"
}

$content1 = (Get-Content -Path ".\rondleidingenweekschema.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\rondleidingenweekschema.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    Write-Host "The rondleidingenweekschema.json files are not identical. Test failed!"
    $result = $result + "The rondleidingenweekschema.json files are not identical. Test failed!\n"
    $throw = true
} else {
    Write-Host "The files rondleidingenweekschema.json are identical."
    $result = $result + "The rondleidingenweekschema.json files are identical.\n"
}

$content1 = (Get-Content -Path ".\medewerkers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"
$content2 = (Get-Content -Path ".\expected results\medewerkers.json" -raw) -replace "`r`n?", "`n" -replace " +`n", "`n"

Write-Host $content1
Write-Host $content2
if ($content1 -ne $content2) {
    Write-Host "The medewerkers.json files are not identical. Test failed!"
    $result = $result + "The medewerkers.json files are not identical. Test failed!\n"
    $throw = true
} else {
    Write-Host "The files medewerkers.json are identical."
    $result = $result + "The medewerkers.json files are identical.\n"
}

Write-Host $result
if ($throw){
    throw "The files were not identical!"
}

Write-Host "test SUCCES!!!"