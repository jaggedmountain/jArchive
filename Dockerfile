#
#multi-stage target: ui
#
FROM node:18 as ui
ARG commit
WORKDIR /app
COPY ./jarchive-ui/package.json ./jarchive-ui/package-lock.json ./
RUN npm install
COPY /jarchive-ui .
RUN $(npm bin)/ng build --output-path /app/dist && \
    sed -i s/##COMMIT##/"$commit"/ /app/dist/index.html &&  \
    echo $commit > /app/dist/commit.txt

#
#multi-stage target: api
#
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS api
COPY ./jarchive-api /app
WORKDIR /app
RUN dotnet publish -c Release -o /app/dist -r linux-x64 --self-contained false

#
#multi-stage target: prod
#
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS prod
ARG commit
WORKDIR /app
COPY --from=api /app/dist /app
COPY --from=ui /app/dist /app/wwwroot
ENV COMMIT=$commit
ENV DOTNET_HOSTBUILDER__RELOADCONFIGCHANGE=false
ENV ASPNETCORE_URLS=http://*:5000
EXPOSE 5000
CMD [ "./Jarchive" ]
