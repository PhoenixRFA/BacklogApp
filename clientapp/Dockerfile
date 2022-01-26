FROM node:14-alpine as build

WORKDIR /app
COPY /clientapp/package*.json .
RUN npm install
COPY /clientapp/. .
RUN npm run build

FROM nginx:alpine
COPY /clientapp/nginx.conf /etc/nginx/conf.d/default.conf
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .