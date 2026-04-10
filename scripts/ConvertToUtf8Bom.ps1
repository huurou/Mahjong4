<#
    すべてのテキストファイルの改行コードをCRLFへ統一し、.csファイルをUTF-8(BOM付き)へ変換する統合スクリプトです。
#>

# 除外対象ディレクトリ
$skipDirectoryNames = @(
    'bin',
    'obj',
    '.vs',
    '.git',
    'node_modules',
    'packages',
    'publish',
    'TestResults'
)

$skipBOMFilePattern = @(
    '.gitignore',
    '.gitattributes'
)

function Invoke-PauseIfStartedFromExplorer {
    try {
        $currentProcess = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $PID"
        if (-not $currentProcess) {
            return
        }

        $parentProcess = Get-CimInstance -ClassName Win32_Process -Filter ("ProcessId = {0}" -f $currentProcess.ParentProcessId)
        if (-not $parentProcess) {
            return
        }

        $parentName = [System.IO.Path]::GetFileNameWithoutExtension($parentProcess.Name)
        if ($parentName -ieq 'explorer') {
            Write-Host ''
            [void](Read-Host '処理が完了しました。ウィンドウを閉じるには Enter キーを押してください。')
        }
    }
    catch {
        # 親プロセスの特定に失敗した場合はそのまま終了
    }
}

function Test-IsBinaryFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    # 先頭数KBを確認し、NULLバイトが含まれる場合はバイナリとみなしてスキップ
    $bufferSize = 8192
    $buffer = New-Object byte[] $bufferSize
    $fileStream = $null

    try {
        $fileStream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        $bytesRead = $fileStream.Read($buffer, 0, $bufferSize)

        for ($i = 0; $i -lt $bytesRead; $i++) {
            if ($buffer[$i] -eq 0) {
                return $true
            }
        }

        return $false
    }
    finally {
        if ($fileStream) {
            $fileStream.Dispose()
        }
    }
}

function Initialize-EncodingSupport {
    try {
        Add-Type -AssemblyName 'System.Text.Encoding.CodePages' -ErrorAction Stop
    }
    catch {
        # Windows PowerShell では標準で読み込み済みのため無視
    }

    $codePagesProviderType = [System.Type]::GetType('System.Text.CodePagesEncodingProvider, System.Text.Encoding.CodePages', $false)
    if ($codePagesProviderType) {
        [System.Text.Encoding]::RegisterProvider($codePagesProviderType::Instance) | Out-Null
    }
}

function Get-FileContentWithEncoding {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    # バイト単位で読み込み、BOMがあれば除いた内容を返す
    $bytes = [System.IO.File]::ReadAllBytes($Path)

    if ($bytes.Length -eq 0) {
        return [PSCustomObject]@{
            Content  = ''
            Encoding = $null
        }
    }

    if ($bytes.Length -ge 4 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE -and $bytes[2] -eq 0x00 -and $bytes[3] -eq 0x00) {
        $encoding = [System.Text.Encoding]::UTF32
        $content = $encoding.GetString($bytes, 4, $bytes.Length - 4)
        return [PSCustomObject]@{ Content = $content; Encoding = $encoding }
    }

    if ($bytes.Length -ge 4 -and $bytes[0] -eq 0x00 -and $bytes[1] -eq 0x00 -and $bytes[2] -eq 0xFE -and $bytes[3] -eq 0xFF) {
        $encoding = [System.Text.Encoding]::GetEncoding('utf-32BE')
        $content = $encoding.GetString($bytes, 4, $bytes.Length - 4)
        return [PSCustomObject]@{ Content = $content; Encoding = $encoding }
    }

    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $encoding = [System.Text.Encoding]::UTF8
        $content = $encoding.GetString($bytes, 3, $bytes.Length - 3)
        return [PSCustomObject]@{ Content = $content; Encoding = $encoding }
    }

    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $encoding = [System.Text.Encoding]::Unicode
        $content = $encoding.GetString($bytes, 2, $bytes.Length - 2)
        return [PSCustomObject]@{ Content = $content; Encoding = $encoding }
    }

    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        $encoding = [System.Text.Encoding]::BigEndianUnicode
        $content = $encoding.GetString($bytes, 2, $bytes.Length - 2)
        return [PSCustomObject]@{ Content = $content; Encoding = $encoding }
    }

    $strictUtf8 = New-Object System.Text.UTF8Encoding ($false, $true)
    try {
        $content = $strictUtf8.GetString($bytes)
        return [PSCustomObject]@{ Content = $content; Encoding = (New-Object System.Text.UTF8Encoding $false) }
    }
    catch [System.Text.DecoderFallbackException] {
        # UTF-8 として失敗した場合は Shift_JIS とみなして変換
        $shiftJis = [System.Text.Encoding]::GetEncoding('shift_jis')
        $content = $shiftJis.GetString($bytes)
        return [PSCustomObject]@{ Content = $content; Encoding = $shiftJis }
    }
    catch {
        # それでも失敗する場合はシステム既定のエンコーディングで読み込み
        $defaultEncoding = [System.Text.Encoding]::Default
        $content = $defaultEncoding.GetString($bytes)
        return [PSCustomObject]@{ Content = $content; Encoding = $defaultEncoding }
    }
}

function Convert-LineEndingsToCrlf {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,
        [Parameter(Mandatory = $true)]
        [regex]$SkipPattern
    )

    $files = Get-ChildItem -Path $RootPath -Recurse -File -Force |
        Where-Object { $_.FullName -notmatch $SkipPattern.ToString() }

    $updated = 0
    $unchanged = 0
    $skipped = 0

    Write-Output ("Start(LineEndings): {0}" -f (Get-Date -Format o))
    Write-Output ("TotalFiles: {0}" -f $files.Count)
    Write-Output ""
    Write-Output "Details(LineEndings):"

    foreach ($file in $files) {
        try {
            if (Test-IsBinaryFile -Path $file.FullName) {
                Write-Output ("SKIP(binary): {0}" -f $file.FullName)
                $skipped++
                continue
            }

            $result = Get-FileContentWithEncoding -Path $file.FullName
            $content = $result.Content
            $encoding = if ($result.Encoding) { $result.Encoding } else { [System.Text.Encoding]::UTF8 }

            $normalized = [System.Text.RegularExpressions.Regex]::Replace($content, "`r`n|`n|`r", "`n")
            $crlfContent = $normalized -replace "`n", "`r`n"

            if ($content.Equals($crlfContent)) {
                Write-Output ("UNCHANGED: {0}" -f $file.FullName)
                $unchanged++
                continue
            }

            $writer = New-Object System.IO.StreamWriter($file.FullName, $false, $encoding)
            try {
                $writer.NewLine = "`r`n"
                $writer.Write($crlfContent)
            }
            finally {
                $writer.Dispose()
            }

            Write-Output ("UPDATED: {0}" -f $file.FullName)
            $updated++
        }
        catch {
            Write-Output ("ERROR: {0} -> {1}" -f $file.FullName, $_.Exception.Message)
            $skipped++
        }
    }

    Write-Output ""
    Write-Output "Summary(LineEndings):"
    Write-Output ("UpdatedFiles: {0}" -f $updated)
    Write-Output ("UnchangedFiles: {0}" -f $unchanged)
    Write-Output ("SkippedFiles: {0}" -f $skipped)
    Write-Output ("End(LineEndings): {0}" -f (Get-Date -Format o))

    return [PSCustomObject]@{
        Updated   = $updated
        Unchanged = $unchanged
        Skipped   = $skipped
    }
}

function Convert-CsFilesToUtf8Bom {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,
        [Parameter(Mandatory = $true)]
        [regex]$SkipPattern,
        [Parameter(Mandatory = $true)]
        [System.Text.Encoding]$Utf8BomEncoding
    )

    $files = Get-ChildItem -Path $RootPath -Recurse -File -Exclude $skipBOMFilePattern |
        Where-Object { $_.FullName -notmatch $SkipPattern.ToString() }

    $converted = 0
    $unchanged = 0
    $failed = 0
    $skipped = 0

    Write-Output ""
    Write-Output ("Start(UTF8-BOM): {0}" -f (Get-Date -Format o))
    Write-Output ("TargetFiles: {0}" -f $files.Count)

    foreach ($file in $files) {
        try {
            if (Test-IsBinaryFile -Path $file.FullName) {
                Write-Output ("SKIP(binary): {0}" -f $file.FullName)
                $skipped++
                continue
            }

            # 既にUTF-8 BOM付きならスキップ
            $headBytes = New-Object byte[] 3
            $fs = [System.IO.File]::Open($file.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
            try { [void]$fs.Read($headBytes, 0, 3) } finally { $fs.Dispose() }
            if ($headBytes[0] -eq 0xEF -and $headBytes[1] -eq 0xBB -and $headBytes[2] -eq 0xBF) {
                Write-Output ("UNCHANGED: {0}" -f $file.FullName)
                $unchanged++
                continue
            }

            $result = Get-FileContentWithEncoding -Path $file.FullName
            [System.IO.File]::WriteAllText($file.FullName, $result.Content, $Utf8BomEncoding)

            $sourceEncodingName = if ($result.Encoding) { $result.Encoding.EncodingName } else { 'Empty File' }
            Write-Output ("CONVERTED: {0} (元: {1})" -f $file.FullName, $sourceEncodingName)
            $converted++
        }
        catch {
            Write-Warning ("変換に失敗しました: {0} - {1}" -f $file.FullName, $_.Exception.Message)
            $failed++
        }
    }

    Write-Output ""
    Write-Output "Summary(UTF8-BOM):"
    Write-Output ("ConvertedFiles: {0}" -f $converted)
    Write-Output ("UnchangedFiles: {0}" -f $unchanged)
    Write-Output ("SkippedFiles: {0}" -f $skipped)
    Write-Output ("FailedFiles: {0}" -f $failed)
    Write-Output ("End(UTF8-BOM): {0}" -f (Get-Date -Format o))

    return [PSCustomObject]@{
        Converted = $converted
        Unchanged = $unchanged
        Skipped   = $skipped
        Failed    = $failed
    }
}

try {
    $rootDirectory = Resolve-Path (Join-Path $PSScriptRoot "..")
    $escapedSkipNames = $skipDirectoryNames | ForEach-Object { [regex]::Escape($_) }
    $skipDirectoryPattern = [regex]::new("(\\|/)({0})(\\|/)" -f ([string]::Join('|', $escapedSkipNames)), [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    Initialize-EncodingSupport
    $utf8BomEncoding = New-Object System.Text.UTF8Encoding $true

    Write-Output ("RootPath: {0}" -f $rootDirectory)

    try {
        $lineEndingResult = Convert-LineEndingsToCrlf -RootPath $rootDirectory -SkipPattern $skipDirectoryPattern | Select-Object -Last 1
    }
    catch {
        Write-Error ("CRLF 変換でエラーが発生しました: {0}" -f $_.Exception.Message)
        $lineEndingResult = $null
    }

    # 大量ログで集計結果を拾えないケースに備えて防御的に判定
    $hasLineEndingSummary = $false
    if ($lineEndingResult) {
        $lineEndingProperties = $lineEndingResult.PSObject.Properties.Name
        $hasLineEndingSummary = $lineEndingProperties -contains 'Updated' -and
            $lineEndingProperties -contains 'Unchanged' -and
            $lineEndingProperties -contains 'Skipped'
    }

    if (-not $hasLineEndingSummary) {
        Write-Warning 'LineEndings の集計結果を取得できなかったため、0件として処理を継続します。'
        $lineEndingResult = [PSCustomObject]@{
            Updated   = 0
            Unchanged = 0
            Skipped   = 0
        }
    }

    try {
        $utf8Result = Convert-CsFilesToUtf8Bom -RootPath $rootDirectory -SkipPattern $skipDirectoryPattern -Utf8BomEncoding $utf8BomEncoding | Select-Object -Last 1
    }
    catch {
        Write-Error ("UTF8-BOM 変換でエラーが発生しました: {0}" -f $_.Exception.Message)
        $utf8Result = $null
    }

    # UTF8-BOM 集計が欠落してもフェイルセーフにカウントを補う
    $hasUtf8Summary = $false
    if ($utf8Result) {
        $utf8Properties = $utf8Result.PSObject.Properties.Name
        $hasUtf8Summary = $utf8Properties -contains 'Converted' -and
            $utf8Properties -contains 'Unchanged' -and
            $utf8Properties -contains 'Skipped' -and
            $utf8Properties -contains 'Failed'
    }

    if (-not $hasUtf8Summary) {
        Write-Warning 'UTF8-BOM の集計結果を取得できなかったため、0件として処理を継続します。'
        $utf8Result = [PSCustomObject]@{
            Converted = 0
            Unchanged = 0
            Skipped   = 0
            Failed    = 0
        }
    }

    Write-Output ""
    Write-Output "Overall Summary:"
    Write-Output ("LineEndings -> Updated:{0} / Unchanged:{1} / Skipped:{2}" -f $lineEndingResult.Updated, $lineEndingResult.Unchanged, $lineEndingResult.Skipped)
    Write-Output ("UTF8-BOM -> Converted:{0} / Unchanged:{1} / Skipped:{2} / Failed:{3}" -f $utf8Result.Converted, $utf8Result.Unchanged, $utf8Result.Skipped, $utf8Result.Failed)
}
finally {
    Invoke-PauseIfStartedFromExplorer
}
