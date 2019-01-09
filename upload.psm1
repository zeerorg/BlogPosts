# Update the following variables for your own settings:
function Upload-Everything {
  'q' | .\Compile\MarkdownCheck.exe
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/compiled" --source .\html
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/photos" --source .\Photos
  az storage blob upload-batch --destination "https://zeerorgblogmdfiles.blob.core.windows.net/posts" --source .\src
  az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --account-name zeerorgprocessedblog
}

function Upload-ALLMD {
  az storage blob upload-batch --destination "https://zeerorgblogmdfiles.blob.core.windows.net/posts" --source .\src --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-TestEverything {
  $test_conn = 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;'
  az storage blob upload-batch --connection-string $test_conn --destination "compiled" --source .\html --content-cache-control "max-age=0, must-revalidate, public"
  az storage blob upload-batch --connection-string $test_conn --destination "photos" --source .\Photos --content-cache-control "max-age=0, must-revalidate, public"
  az storage blob upload-batch --connection-string $test_conn --destination "posts" --source .\src --content-cache-control "max-age=0, must-revalidate, public"
  az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --connection-string $test_conn --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-Photo ($fileLocation) {
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name photos --name $fileName --file $fileLocation --account-name zeerorgprocessedblog --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-MDFile ($fileLocation) {
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name posts --name $fileName --file $fileLocation --account-name zeerorgblogmdfiles --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-HTMLFile ($fileLocation) {
  'q' | .\Compile\MarkdownCheck.exe
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name compiled --name $fileName --file $fileLocation --account-name zeerorgprocessedblog --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-AllHTML {
  'q' | .\Compile\MarkdownCheck.exe
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/compiled" --source .\html --content-cache-control "max-age=0, must-revalidate, public"
}

function Upload-JSON {
  az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --account-name zeerorgprocessedblog --content-cache-control "max-age=0, must-revalidate, public"
}

function Start-FileServer {
  http-server . --cors "*" -c-1
}

function Watch-MDCompile {
  .\Compile\MarkdownCheck.exe
}

Export-ModuleMember -Function * -Alias *