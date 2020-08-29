# windows-service-file-system-watcher
Serviço do Windows que utiliza a classe FileSystemWatcher para "escutar" uma pasta específica e ao identificar um novo arquivo realiza o upload para o Azure Blob Storage.
Para a instalação do serviço em produção, utilizei o cmd com os seguintes comandos:
sc <servername> create <servicename> binPath=<file.exe>
sc <servername> query <servicename>
sc <servername> start <servicename>
sc <servername> stop <servicename>
sc <servername> delete <servicename>
