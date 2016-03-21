param([string]$main,[string]$substract)

$a = New-Object System.Collections.ArrayList

$readerAtreure = [System.IO.File]::OpenText($substract)
for(;;) {
    $line = $readerAtreure.ReadLine()
    if (($line -eq $null) -or ($line.trim() -eq ""))  { break }
    $a.Add($line) | Out-Null
}
$readerAtreure.close()

# ara es te el que s'ha de treure en memoria arraylist
$arxiuTemporal = $($(Convert-Path .)+"\temp.txt")

Remove-Item $arxiuTemporal
Rename-Item $main $arxiuTemporal

$readerTemp = [System.IO.File]::OpenText($arxiuTemporal)
$writerBons = [System.IO.StreamWriter]($main)
for(;;) {
    $line = $readerTemp.ReadLine()

    if (($line -eq $null) -or ($line.trim() -eq ""))  { break }

    if(!$a.Contains($line)){
        $writerBons.WriteLine($line)
    }

}
$writerBons.Close()
$readerTemp.Close()

Remove-Variable a
Remove-Item $arxiuTemporal
