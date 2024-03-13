# Variables
$rgName = "RG-AZ-204"
$location = "westeurope"
$vmSize = "Standard_B1s"
$winVmName = "VM-WIN-WEU-C2-1"
$ubuVmName = "VM-UBU-WEU-C2-1"
$winUrn = "MicrosoftWindowsServer:WindowsServer:2019-Datacenter:latest"
$ubuUrn = "Canonical:UbuntuServer:19-04:latest"
$cred = (Get-Credential -Message "Admin credentials for the VMs:")
$text = "Hello, World!"
$userData = [System.Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($text))
$tag = @{"chapter" = 2}

# Create resource group
New-AzResourceGroup -Name $rgName -Location $location -Tag $tag -Force

# Create Windows VM
New-AzVm -Name $winVmName -ResourceGroupName $rgName -Location $location -ImageName $winUrn -Credential $cred -Size $vmSize -UserData $userData

# Create Ubuntu VM
New-AzVm -Name $ubuVmName -ResourceGroupName $rgName -Location $location -ImageName $ubuUrn -Credential $cred -Size $vmSize -UserData $userData