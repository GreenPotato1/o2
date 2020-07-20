$prefix = "dev" 
#$prefix = "staging"

$c1 = New-SelfSignedCertificate -DnsName "*.chat-$prefix.o2bionics.com" -CertStoreLocation cert:\LocalMachine\My -NotAfter (Get-Date).AddMonths(1200)
$c2 = New-SelfSignedCertificate -DnsName "chat-$prefix.o2bionics.com" -CertStoreLocation cert:\LocalMachine\My -NotAfter (Get-Date).AddMonths(1200)
$c3 = New-SelfSignedCertificate -DnsName "$prefix.net.customer" -CertStoreLocation cert:\LocalMachine\My -NotAfter (Get-Date).AddMonths(1200)

# add certificates to the "Trusted Root Certification Authorities"
$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList Root, LocalMachine
$rootStore.Open("MaxAllowed")
$rootStore.Add($c1)
$rootStore.Add($c2)
$rootStore.Add($c3)
$rootStore.Close()

