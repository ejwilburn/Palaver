#!/bin/bash
PUBLISH_ROOT="/var/www/html/Palaver"
DEV_ROOT="/home/ej/dev/Palaver/Palaver2"

cp -u -v -R "$DEV_ROOT/Content" "$PUBLISH_ROOT"
cp -u -v -R "$DEV_ROOT/Scripts" "$PUBLISH_ROOT"
install -v -g "www-data" -d "$PUBLISH_ROOT/bin"
for file in $DEV_ROOT/bin/*.dll; do \
	install -C -v -g "www-data" "$file" "$PUBLISH_ROOT/bin"; \
done
cp -u -v -R "$DEV_ROOT/Views" "$PUBLISH_ROOT"
cp -u -v "$DEV_ROOT/Global.asax" "$PUBLISH_ROOT"
chown -R --from=ej:ej ej:www-data "$DEV_ROOT"
touch "$PUBLISH_ROOT/Web.config"

