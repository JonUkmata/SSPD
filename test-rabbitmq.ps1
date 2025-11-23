# Script për testimin e RabbitMQ event flow

Write-Host "=== Test i RabbitMQ Event Flow ===" -ForegroundColor Cyan

# 1. Testo lidhjen me RabbitMQ Management UI
Write-Host "`n1. RabbitMQ Management UI: http://localhost:15672" -ForegroundColor Yellow
Write-Host "   Username: guest, Password: guest" -ForegroundColor Gray

# 2. Krijo një review negative
Write-Host "`n2. Duke krijuar review negative..." -ForegroundColor Yellow
$negativeReview = @{
    businessId = [guid]::NewGuid().ToString()
    userId = "test-user-negative"
    rating = 1
    comment = "Terrible service! Very slow, expensive, and poor quality food. Rude staff."
} | ConvertTo-Json

try {
    $result1 = Invoke-RestMethod -Uri "http://localhost:5102/api/reviews" -Method Post -Body $negativeReview -ContentType "application/json"
    Write-Host "   ✓ Review krijuar: $($result1.id)" -ForegroundColor Green
    Write-Host "   Rating: $($result1.rating), Comment: $($result1.comment)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Gabim: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Prit pak që eventi të përpunohet
Write-Host "`n3. Duke pritur që eventi të përpunohet (3 sekonda)..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# 4. Kontrollo ReviewService logs për publishing
Write-Host "`n4. ReviewService - Event Publishing:" -ForegroundColor Yellow
docker logs projektiperrekomandime-reviewservice-1 2>&1 | Select-String -Pattern "Publishing.*$($result1.id)" | Select-Object -Last 2 | ForEach-Object {
    Write-Host "   $_" -ForegroundColor Gray
}

# 5. Kontrollo AnalysisService logs për consumption
Write-Host "`n5. AnalysisService - Event Consumption:" -ForegroundColor Yellow
docker logs projektiperrekomandime-analysisservice-1 2>&1 | Select-String -Pattern "$($result1.id)" | Select-Object -Last 3 | ForEach-Object {
    Write-Host "   $_" -ForegroundColor Gray
}

# 6. Merr analizën e krijuar
Write-Host "`n6. Duke kërkuar analizën për review $($result1.id)..." -ForegroundColor Yellow
Start-Sleep -Seconds 1
$analyses = Invoke-RestMethod -Uri "http://localhost:5103/api/analyses" -Method Get
$analysis = $analyses | Where-Object { $_.reviewId -eq $result1.id }

if ($analysis) {
    Write-Host "   ✓ Analiza u gjet!" -ForegroundColor Green
    Write-Host "   Analysis ID: $($analysis.id)" -ForegroundColor Gray
    Write-Host "   Sentiment Score: $($analysis.sentimentScore)" -ForegroundColor $(if ($analysis.sentimentScore -lt 0.5) { "Red" } else { "Green" })
    Write-Host "   Category: $($analysis.category)" -ForegroundColor $(if ($analysis.category -eq "Critical") { "Red" } elseif ($analysis.category -eq "NeedsImprovement") { "Yellow" } else { "Green" })
    Write-Host "   Problems: $($analysis.problemsIdentified -join ', ')" -ForegroundColor Gray
} else {
    Write-Host "   ✗ Analiza nuk u gjet!" -ForegroundColor Red
}

# 7. Krijo një review positive për krahasim
Write-Host "`n7. Duke krijuar review positive për krahasim..." -ForegroundColor Yellow
$positiveReview = @{
    businessId = [guid]::NewGuid().ToString()
    userId = "test-user-positive"
    rating = 5
    comment = "Amazing experience! Excellent food, perfect service, very friendly staff."
} | ConvertTo-Json

try {
    $result2 = Invoke-RestMethod -Uri "http://localhost:5102/api/reviews" -Method Post -Body $positiveReview -ContentType "application/json"
    Write-Host "   ✓ Review krijuar: $($result2.id)" -ForegroundColor Green
    
    Start-Sleep -Seconds 3
    
    $analyses = Invoke-RestMethod -Uri "http://localhost:5103/api/analyses" -Method Get
    $analysis2 = $analyses | Where-Object { $_.reviewId -eq $result2.id }
    
    if ($analysis2) {
        Write-Host "   ✓ Analiza u gjet!" -ForegroundColor Green
        Write-Host "   Sentiment Score: $($analysis2.sentimentScore)" -ForegroundColor Green
        Write-Host "   Category: $($analysis2.category)" -ForegroundColor Green
    }
} catch {
    Write-Host "   ✗ Gabim: $($_.Exception.Message)" -ForegroundColor Red
}

# 8. Statistika finale
Write-Host "`n8. Statistika finale:" -ForegroundColor Yellow
$allAnalyses = Invoke-RestMethod -Uri "http://localhost:5103/api/analyses" -Method Get
$positive = ($allAnalyses | Where-Object { $_.category -eq "Positive" }).Count
$needsImprovement = ($allAnalyses | Where-Object { $_.category -eq "NeedsImprovement" }).Count
$critical = ($allAnalyses | Where-Object { $_.category -eq "Critical" }).Count

Write-Host "   Total Analyses: $($allAnalyses.Count)" -ForegroundColor Cyan
Write-Host "   Positive: $positive" -ForegroundColor Green
Write-Host "   Needs Improvement: $needsImprovement" -ForegroundColor Yellow
Write-Host "   Critical: $critical" -ForegroundColor Red

Write-Host "`n=== Testi përfundoi me sukses! ===" -ForegroundColor Cyan
Write-Host "`nPër më shumë detaje, shiko RabbitMQ Management UI:" -ForegroundColor Yellow
Write-Host "http://localhost:15672/#/queues/%2F/analysis.review.created" -ForegroundColor Blue
