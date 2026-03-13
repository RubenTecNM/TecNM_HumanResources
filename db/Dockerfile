FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
RUN mkdir -p /usr/config
COPY ./db/setup.sql /usr/config/setup.sql
COPY ./db/entrypoint.sh /usr/config/entrypoint.sh
RUN chmod +x /usr/config/entrypoint.sh

USER mssql
ENTRYPOINT ["/bin/bash", "/usr/config/entrypoint.sh"]
