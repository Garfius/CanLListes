param([string]$inputMails,[string]$listMailName,[string]$nomLlarg,[boolean]$nomeslocal,[string]$moderador,[boolean]$ocult,[boolean]$removeList)
add-pssnapin Microsoft.Exchange.Management.Powershell.Snapin

$Alias=$listMailName.Split("@")[0]
$Grup=$nomLlarg

Remove-DistributionGroup $listMailName -Confirm:$false

if($removeList){
    exit
}

$Grup = New-DistributionGroup -Name $Grup -Alias $Alias -Confirm:$False -MemberDepartRestriction "Closed" -MemberJoinRestriction "Closed"  -ManagedBy "adminbusties@uoc.local" -OrganizationalUnit "uoc.local/Usuaris/Llistes" -PrimarySmtpAddress $listMailName

if(!$grup){
    Write-Output "no s'ha pogut crear"
    Write-error "no s'ha pogut crear"
    exit
}

if($ocult){
    Set-DistributionGroup $Grup -HiddenFromAddressListsEnabled:$ocult
}
if($nomeslocal){
    Set-DistributionGroup $Grup -RequireSenderAuthenticationEnabled:$nomeslocal
}

if($moderador){
    Set-DistributionGroup $Grup -ModeratedBy $moderador,"adminbusties@uoc.local" -ModerationEnabled:$true
}

#Get-DistributionGroup $Grup | Add-AdPermission -User "uoc\exchange servers" -AccessRights genericall

[int]$contador = 0
Write-output "Tractant el grup: $grup"
    
$reader = [System.IO.File]::OpenText($inputMails)
try {
	for(;;) {
		$line = $reader.ReadLine()
		if ($line -eq $null )  { break }
			if($line.trim() -ne ""){
                Write-output $("Afegint: "+ $line)
				#Add-DistributionGroupMember $Grup -member $line
				Add-DistributionGroupMember $Grup -member $(Get-Recipient $line)
			    $contador ++
            }				
	}
}catch{
	Write-error $("NO ES TROBA CAP BUSTIA PER "+ $line)
		
}

$reader.Close()
Write-Output $("Total de correus: "+$contador.ToString())

