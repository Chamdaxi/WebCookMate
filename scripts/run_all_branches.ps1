$ErrorActionPreference = 'Stop'

# LÆ°u nhÃ¡nh hiá»‡n táº¡i
$currentBranch = (git rev-parse --abbrev-ref HEAD).Trim()

# Láº¥y stash cÃ³ nhÃ£n 'sqlite-run' náº¿u cÃ³
$sqliteStash = (git stash list | Select-String 'sqlite-run' | Select-Object -First 1)
if ($sqliteStash) {
    $sqliteStashRef = $sqliteStash.ToString().Split(':')[0]
} else {
    $sqliteStashRef = $null
}

$branches = @(git branch --format='%(refname:short)')
$results = @()

foreach ($b in $branches) {
    Write-Host "=== BRANCH:$b ==="
    try {
        git checkout $b | Out-Null
    } catch {
        $results += [pscustomobject]@{ Branch=$b; Restore=$false; Build=$false; Run=$false; Note='checkout_failed' }
        continue
    }

    $okRestore = $false
    $okBuild   = $false
    $okRun     = $false
    $note      = ''

    # Không apply stash; cấu hình biến môi trường cho tiến trình con khi chạy

    try { dotnet restore | Out-Null; $okRestore = $true } catch { $note = if ($note) { $note + ',restore_fail' } else { 'restore_fail' } }

    try {
        dotnet build -c Debug | Out-Null
        if ($LASTEXITCODE -eq 0) { $okBuild = $true } else { $note = if ($note) { $note + ',build_fail' } else { 'build_fail' } }
    } catch { $note = if ($note) { $note + ',build_fail' } else { 'build_fail' } }

    if ($okBuild) {
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        $env:ConnectionStrings__DefaultConnection = 'Data Source=app.db'
        $p = Start-Process -FilePath dotnet -ArgumentList 'run --no-build' -PassThru
        Start-Sleep -Seconds 5
        try { taskkill /PID $p.Id /F | Out-Null; $okRun = $true } catch { $note = if ($note) { $note + ',kill_fail' } else { 'kill_fail' } }
    }

    git reset --hard | Out-Null
    $results += [pscustomobject]@{ Branch=$b; Restore=$okRestore; Build=$okBuild; Run=$okRun; Note=$note }
}

# Trá»Ÿ láº¡i nhÃ¡nh ban Ä‘áº§u
git checkout $currentBranch | Out-Null

# In káº¿t quáº£
$results | Format-Table -AutoSize | Out-String | Write-Host
$json = $results | ConvertTo-Json -Depth 3
Set-Content -Path 'run_results.json' -Value $json -Encoding UTF8
Write-Host "Results saved to run_results.json"



