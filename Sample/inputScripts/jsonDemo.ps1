param([string]$esDeptManager,[string]$nombresDept,[string]$sexes,[string]$titulacions,[string]$prova)

#$CrLf = "`r`n"
$CrLf = [Environment]::NewLine
function nomArxiu{
    return $($MyInvocation.ScriptName.Split(".")[0])
}


function json2Text( $jsonResposta){
    
    [System.String]$resposta = ""
    
    $depurat = $jsonResposta | get-member -type NoteProperty
    
    foreach($propietat in $depurat ){
        $name=$propietat.Name
        $values=$jsonResposta."$($propietat.Name)"
        foreach($value in $values){
            $nomValor = $value | Get-Member -MemberType NoteProperty
            if($nomValor){
                # si hi ha un segon nivell en el arbre, volca el nom del nivell i el contingut
                $valor = $value."$($nomvalor.name)"
                $resposta += $($name+","+$valor +$CrLf)
            }else{
                # si no hi ha un segon nivell volca els valors
                $resposta += $($value + $CrLf)
            }
            
        
        }
    }
    
    
    

    return $resposta
}

function var2File($file,$data){

    if($(Test-Path $file)){
        rm $file
    }    

    $writer = [System.IO.StreamWriter]($file)
    
    $writer.Write($data)
    
    $writer.Close()

}

#$requestURL = "http://192.168.222.60/index.php/consulta"
$requestURL = "http://192.168.1.60/index.php/consulta"

if($PSBoundParameters.Count -eq 0){
    
    $arxiusortida = $($(nomArxiu) + "_parms.csv")
    
    $jsonResponse =   (new-object net.webclient).DownloadString($requestURL)  | ConvertFrom-Json
    
    var2File $arxiusortida $(json2Text $jsonResponse)

}else{

    [System.Boolean]$esElPrimer = $true

    foreach($parametre in $PSBoundParameters.GetEnumerator()){
        if($esElPrimer){
            $requestURL += "?" + $parametre.Key + "="+ [uri]::EscapeDataString($parametre.Value)
            $esElPrimer = $false
        }else{
            $requestURL += "&" + $parametre.Key + "="+ [uri]::EscapeDataString($parametre.Value)
        }
        
    }

    $arxiusortida = $($(nomArxiu )+ "_out.txt")
     
    $jsonResponse =   (new-object net.webclient).DownloadString($requestURL)  | ConvertFrom-Json

    if($jsonResponse -and ($jsonResponse.Count -lt 1000 )){
        Write-Output $(json2Text $jsonResponse)
        #var2File $arxiusortida $(json2Text $jsonResponse)
    }
    if(($jsonResponse.Count -gt 1000 )){
        Write-Error "massa resultats"
    }
    

}

