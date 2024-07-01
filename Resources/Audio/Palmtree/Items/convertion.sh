    #!/bin/bash

# Verifica se o diretório foi passado como argumento
if [ -z "$1" ]; then
  echo "Por favor, forneça o diretório contendo os arquivos .ogg."
  exit 1
fi

# Diretório contendo os arquivos .ogg
DIR="$1"

# Itera sobre todos os arquivos .ogg no diretório
for FILE in "$DIR"/*.ogg; do
  # Define o nome do arquivo de saída
  OUTPUT="${FILE%.ogg}_mono.ogg"

  # Converte o arquivo de estéreo para mono
  ffmpeg -i "$FILE" -ac 1 "$OUTPUT"
done

echo "Conversão concluída."
