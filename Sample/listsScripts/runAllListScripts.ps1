$EmailRegex = '^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z1]{1,4})$'

$arxius = Get-ChildItem $(Resolve-Path .) | Where-Object {($_.FullName.ToLower().Contains(".ps1") -and ($_.Name -match $EmailRegex))} | select FullName

foreach($arxiu in $arxius){
    & $arxiu.FullName

}

