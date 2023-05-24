find ./ -type f -name "*.sln" -print0 | xargs -r0 /bin/bash -c 'for f in "$@"; do echo "$f"; cd "$(dirname $f)"; paket restore; done;' ''
cd src/vscode/contextive
npm install