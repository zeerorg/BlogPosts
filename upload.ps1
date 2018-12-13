# Update the following variables for your own settings:
'q' | .\build.ps1
az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/compiled" --source .\html
az storage blob upload-batch --destination "https://zeerorgprocessedblog.blob.core.windows.net/photos" --source .\Photos
az storage blob upload-batch --destination "https://zeerorgblogmdfiles.blob.core.windows.net/posts" --source .\src
az storage blob upload --container-name metadata --name "main.json" --file ".\main.json" --account-name zeerorgprocessedblog

echo "DONE"