# Load test për RabbitMQ - Krijo shumë reviews shpejt

param(
    [int]$Count = 10,
    [int]$DelayMs = 500
)

Write-Host "=== RabbitMQ Load Test ===" -ForegroundColor Cyan
Write-Host "Duke krijuar $Count reviews me $DelayMs ms delay..." -ForegroundColor Yellow
Write-Host ""

$comments = @(
    @{ rating = 5; comment = "Excellent service! Amazing food and friendly staff." },
    @{ rating = 4; comment = "Very good overall, just a bit slow." },
    @{ rating = 3; comment = "Average experience, nothing special." },
    @{ rating = 2; comment = "Poor quality food, slow service, too expensive." },
    @{ rating = 1; comment = "Terrible! Rude staff, dirty place, awful food, very expensive." }
)

$businessId = [guid]::NewGuid().ToString()
$results = @()

for ($i = 1; $i -le $Count; $i++) {
    $template = $comments | Get-Random
    
    $review = @{
        businessId = $businessId
        userId = "load-test-user-$i"
        rating = $template.rating
        comment = $template.comment
    } | ConvertTo-Json
    
    try {
        $result = Invoke-RestMethod -Uri "http://localhost:5102/api/reviews" -Method Post -Body $review -ContentType "application/json"
        Write-Host "[$i/$Count] ✓ Review created: $($result.id) (Rating: $($result.rating))" -ForegroundColor Green
        $results += $result
    } catch {
        Write-Host "[$i/$Count] ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds $DelayMs
}

Write-Host "`nDuke pritur që të gjitha events të përpunohen..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "`nStatistika:" -ForegroundColor Cyan
Write-Host "  Reviews të krijuara: $($results.Count)" -ForegroundColor Gray

$analyses = Invoke-RestMethod -Uri "http://localhost:5103/api/analyses" -Method Get
$testAnalyses = $analyses | Where-Object { $_.businessId -eq $businessId }

Write-Host "  Analyses të krijuara: $($testAnalyses.Count)" -ForegroundColor Gray
Write-Host "  Success rate: $([math]::Round(($testAnalyses.Count / $results.Count) * 100, 2))%" -ForegroundColor $(if ($testAnalyses.Count -eq $results.Count) { "Green" } else { "Yellow" })

Write-Host "`nShpërndarja e kategorive:" -ForegroundColor Cyan
$testAnalyses | Group-Object category | ForEach-Object {
    $color = switch ($_.Name) {
        "Positive" { "Green" }
        "NeedsImprovement" { "Yellow" }
        "Critical" { "Red" }
        default { "Gray" }
    }
    Write-Host "  $($_.Name): $($_.Count)" -ForegroundColor $color
}

Write-Host "`nAverage Sentiment Score: $([math]::Round(($testAnalyses | Measure-Object -Property sentimentScore -Average).Average, 3))" -ForegroundColor Cyan
