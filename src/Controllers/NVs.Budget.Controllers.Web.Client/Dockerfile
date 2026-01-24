FROM node:20 AS node-builder
COPY ["budget-client", "/client"]
RUN npm i -g @angular/cli
WORKDIR /client
RUN npm ci
RUN npm run build

FROM alpine:latest AS export
COPY --from=node-builder /client/dist/budget-client/browser /client/dist/budget-client/browser
