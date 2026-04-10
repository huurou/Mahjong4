try {
    Set-Location $PSScriptRoot

    # カバレッジ結果を格納する TestResults ディレクトリを初期化
    $resultsRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\TestResults"))
    if (-not (Test-Path -Path $resultsRoot)) {
        New-Item -ItemType Directory -Path $resultsRoot | Out-Null
    } else {
        Get-ChildItem -Path $resultsRoot -Recurse -Force | Remove-Item -Recurse -Force
    }

    $solutionRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
    $testsRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\tests"))
    $testProjects = Get-ChildItem -Path $testsRoot -Recurse -Filter *.csproj | Where-Object { $_.Name -match '\.Tests\.csproj$' }
    if (-not $testProjects -or $testProjects.Count -eq 0) {
        throw "テスト プロジェクト (*.Tests.csproj) が $testsRoot 配下に見つかりません。"
    }

    # slnx ファイルを検索
    $slnxFile = Get-ChildItem -Path (Join-Path $solutionRoot "*.slnx") | Select-Object -First 1
    if (-not $slnxFile) {
        throw "slnx ファイルが $solutionRoot に見つかりません。"
    }

    # 先にソリューション全体をビルド（カバレッジ収集時のビルド競合を防止）
    Write-Output "ソリューションをビルドしています..."
    dotnet build $slnxFile.FullName
    if ($LASTEXITCODE -ne 0) {
        throw "ソリューションのビルドに失敗しました。"
    }

    # 各テストプロジェクトを対象にカバレッジを計測
    Write-Output "テストを実行し、コードカバレッジ情報を収集しています..."
    $projectCoverageFiles = @()
    foreach ($project in $testProjects) {
        $projectPath = $project.FullName
        $projectName = $project.BaseName
        $projectResultsDir = Join-Path $resultsRoot $projectName
        if (-not (Test-Path -Path $projectResultsDir)) {
            New-Item -ItemType Directory -Path $projectResultsDir | Out-Null
        }

        Write-Output "カバレッジ付きでテストを実行中: $projectPath"
        $coverageOutputFile = Join-Path $projectResultsDir "coverage.cobertura.xml"
        dotnet test --project $projectPath --no-build --coverage --coverage-output-format cobertura --coverage-output $coverageOutputFile
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet test が $projectPath で失敗しました。"
        }

        $coverageFile = Get-ChildItem -Path $projectResultsDir -Recurse -Filter "*.cobertura.xml" | Sort-Object -Descending LastWriteTime | Select-Object -First 1
        if (-not $coverageFile) {
            throw "カバレッジ ファイルが $projectPath で生成されませんでした。"
        }
        $projectCoverageFiles += $coverageFile.FullName
        Write-Output "検出したカバレッジ ファイル: $($coverageFile.FullName)"
    }

    $coverageReportSource = $projectCoverageFiles -join ";"

    # ReportGenerator を使用して HTML レポートを作成
    $coverageRoot = Join-Path $resultsRoot "CoverageReport"
    if (Test-Path -Path $coverageRoot) {
        Remove-Item -Path $coverageRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Path $coverageRoot | Out-Null

    # slnx からプロジェクト名を取得し、テスト・AppHost・ServiceDefaults・エントリポイント(Web) を除外した assemblyfilters を構築
    [xml]$slnx = Get-Content $slnxFile.FullName
    $excludePatterns = @("*.Tests", "*.AppHost", "*.ServiceDefaults", "*.Web")
    $excludeAssemblies = $slnx.Solution.SelectNodes("//Project") |
        ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Path) } |
        Where-Object {
            $name = $_
            $excludePatterns | Where-Object { $name -like $_ } | Select-Object -First 1
        } |
        ForEach-Object { "-$_" }
    $allAssemblies = @("+*") + $excludeAssemblies
    $assemblyFilter = $allAssemblies -join ";"

    try {
        Write-Output "HTML レポートを生成しています..."
        reportgenerator -reports:$coverageReportSource -targetdir:$coverageRoot -reporttypes:Html "-assemblyfilters:$assemblyFilter"
    }
    catch {
        throw "reportgenerator の実行に失敗しました。dotnet-reportgenerator-globaltool がインストールされているか確認してください (例: dotnet tool install -g dotnet-reportgenerator-globaltool)。"
    }

    # レポートのパスをブラウザで開ける形式に整形
    $reportPath = Join-Path -Path $coverageRoot -ChildPath "index.html"
    if (-not (Test-Path -Path $reportPath)) {
        throw "生成されたレポート ファイルが見つかりません: $reportPath"
    }

    $absoluteReportPath = Resolve-Path $reportPath | Select-Object -ExpandProperty Path
    $fileUrl = "file:///" + ($absoluteReportPath -replace '\\','/')

    # Chrome または既定のハンドラーでレポートを起動
    Write-Output "レポートをブラウザで開いています..."
    $chromeCandidates = @(
        "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
        "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
        "$env:ProgramFiles\\Google\\Chrome\\Application\\chrome.exe",
        "$env:ProgramFiles(x86)\\Google\\Chrome\\Application\\chrome.exe",
        "$env:LocalAppData\\Google\\Chrome\\Application\\chrome.exe"
    )

    $chromePath = $chromeCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($chromePath) {
        Write-Output "Google Chrome でレポートを開いています..."
        & $chromePath $fileUrl
    } else {
        Write-Warning "Google Chrome が見つかりませんでした。既定のハンドラーで開きます。"
        Invoke-Item $reportPath
    }

    Write-Output "コードカバレッジ レポートの生成が完了しました。"
}
catch {
    Write-Error "エラーが発生しました: $_"
    Write-Error "スタック トレース: $($_.ScriptStackTrace)"
    Pause
}
