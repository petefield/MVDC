#!/bin/sh
# Write the Blazor WASM configuration from the environment variable so the
# browser-side app calls the correct (host-mapped) API URL.

if [ -n "$API_BASE_ADDRESS" ]; then
    cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "ApiBaseAddress": "${API_BASE_ADDRESS}"
}
EOF
    echo "Injected ApiBaseAddress=${API_BASE_ADDRESS} into appsettings.json"
fi

exec nginx -g 'daemon off;'
