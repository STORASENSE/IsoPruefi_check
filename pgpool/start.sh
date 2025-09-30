#!/bin/bash

# Substitute environment variables in pgpool.conf template
envsubst < /etc/pgpool2/pgpool.conf.template > /etc/pgpool2/pgpool.conf

# For SCRAM-SHA-256, we need plaintext password in pool_passwd
echo "$POSTGRES_USERNAME:$POSTGRES_PASSWORD" > /etc/pgpool2/pool_passwd
chmod 600 /etc/pgpool2/pool_passwd

# Start pgpool
exec pgpool -n -f /etc/pgpool2/pgpool.conf