# windows-service-file-system-watcher
Serviço do Windows que utiliza a classe FileSystemWatcher para "escutar" uma pasta específica e ao identificar um novo arquivo realiza o upload para o Azure Blob Storage.
Para a instalação do serviço em produção, utilizei o cmd com os comandos sc.
