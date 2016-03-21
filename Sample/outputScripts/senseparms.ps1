param([string]$inputMails)
add-pssnapin Microsoft.Exchange.Management.Powershell.Snapin

$Alias=$nomMail.Split("@")[0]
$Grup=$nomLlarg

Remove-DistributionGroup $nomLlarg -Confirm:$false

$Grup = New-DistributionGroup -Name $Grup -Alias $Alias -Confirm:$False -MemberDepartRestriction "Closed" -MemberJoinRestriction "Closed"  -ManagedBy "adminbusties@uoc.test" -OrganizationalUnit ".local/Usuaris/Llistes" -PrimarySmtpAddress $nomMail

Set-DistributionGroup $Grup -HiddenFromAddressListsEnabled:$ocult

Set-DistributionGroup $Grup -RequireSenderAuthenticationEnabled:$nomeslocal

Get-DistributionGroup $Grup | Add-AdPermission -User "uoc\exchange servers" -AccessRights genericall


if ($Grup){
	Write-output "Tractant el grup: $grup"

	$reader = [System.IO.File]::OpenText($inputMails)
	try {
		for(;;) {
			$line = $reader.ReadLine()
			if (($line -eq $null) -or ($line.trim() -eq ""))  { break }
				
				Write-output $("Afegint"+ $line)
				#Add-DistributionGroupMember $Grup -member $line
				Add-DistributionGroupMember $Grup -member $(Get-Recipient $line)
			
				
		}
	}catch{
		Write-error $("NO ES TROBA CAP BUSTIA PER"+ $line)
		
	}

	$reader.Close()
    Write-Host "Tot correcte ?"
}else{
	Write-error "Error, no s'ha creat correctament"
}

