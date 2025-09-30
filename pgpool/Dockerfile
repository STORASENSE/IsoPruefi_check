FROM ubuntu:22.04

# Install dependencies
RUN apt-get update && \
    apt-get install -y pgpool2 postgresql-client gettext-base && \
    rm -rf /var/lib/apt/lists/*

# Copy configuration template
COPY pgpool.conf /etc/pgpool2/pgpool.conf.template
COPY start.sh /start.sh
RUN chmod +x /start.sh

# Expose Pgpool port
EXPOSE 9999

# Start with environment substitution
CMD ["/start.sh"]