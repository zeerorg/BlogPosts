# Update the following variables for your own settings:
function Upload-Everything {
  'q' | .\Compile\MarkdownCheck.exe
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/compiled" --source .\html
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/photos" --source .\Photos
  az storage blob upload-batch --destination "https://zeerorgblogmdfiles.blob.core.windows.net/posts" --source .\src
  az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --account-name zeerorgprocessedblog
}

function Upload-Photo ($fileLocation) {
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name photos --name $fileName --file $fileLocation --account-name zeerorgprocessedblog
}

function Upload-MDFile ($fileLocation) {
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name posts --name $fileName --file $fileLocation --account-name zeerorgblogmdfiles
}

function Upload-HTMLFile ($fileLocation) {
  'q' | .\Compile\MarkdownCheck.exe
  $fileName = (Get-Item $fileLocation).Name
  echo $fileLocation $fileName
  az storage blob upload --container-name compiled --name $fileName --file $fileLocation --account-name zeerorgprocessedblog
}

function Upload-AllHTML {
  'q' | .\Compile\MarkdownCheck.exe
  az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/compiled" --source .\html
}

function Upload-JSON {
  az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --account-name zeerorgprocessedblog
}

function Start-FileServer {
  http-server . --cors "*"
}

function Watch-MDCompile {
  .\Compile\MarkdownCheck.exe
}

Export-ModuleMember -Function * -Alias *