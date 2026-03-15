#!/bin/sh
# Write the Blazor WASM configuration from the environment variable so the
# browser-side app calls the correct (host-mapped) API URL.

exec nginx -g 'daemon off;'
