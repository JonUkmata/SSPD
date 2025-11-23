# Script për monitorim real-time të RabbitMQ event flow

Write-Host "=== RabbitMQ Real-Time Monitor ===" -ForegroundColor Cyan
Write-Host "Shtyp Ctrl+C për të ndalur" -ForegroundColor Yellow
Write-Host ""

$reviewServiceContainer = "projektiperrekomandime-reviewservice-1"
$analysisServiceContainer = "projektiperrekomandime-analysisservice-1"

# Merr timestamp-in aktual
$startTime = Get-Date

Write-Host "Duke monitoruar events që nga: $startTime" -ForegroundColor Gray
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""

# Shfaq logs në real-time
$job1 = Start-Job -ScriptBlock {
    param($container, $startTime)
    docker logs -f --since "$startTime" $container 2>&1 | ForEach-Object {
        if ($_ -match "Publishing|event published") {
            Write-Host "[PUBLISH] $_" -ForegroundColor Green
        }
    }
} -ArgumentList $reviewServiceContainer, $startTime.ToString("yyyy-MM-ddTHH:mm:ss")

$job2 = Start-Job -ScriptBlock {
    param($container, $startTime)
    docker logs -f --since "$startTime" $container 2>&1 | ForEach-Object {
        if ($_ -match "Message received") {
            Write-Host "[CONSUME] $_" -ForegroundColor Yellow
        }
        if ($_ -match "Analysis created") {
            Write-Host "[ANALYZE] $_" -ForegroundColor Cyan
        }
    }
} -ArgumentList $analysisServiceContainer, $startTime.ToString("yyyy-MM-ddTHH:mm:ss")

try {
    while ($true) {
        Receive-Job -Job $job1
        Receive-Job -Job $job2
        Start-Sleep -Milliseconds 100
    }
} finally {
    Stop-Job -Job $job1, $job2
    Remove-Job -Job $job1, $job2
}
