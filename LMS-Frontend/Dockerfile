# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --frozen-lockfile

COPY . .

# VITE_API_BASE_URL is injected at build time via --build-arg
ARG VITE_API_BASE_URL=https://liveexpert.ai/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# ── Stage 2: Serve via nginx ──────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
