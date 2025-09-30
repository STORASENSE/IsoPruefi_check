## BUILD ENVIRONMENT ###########
################################

# Use the official Node.js Alpine image (adjust based on your project's requirements)
FROM node:24-alpine AS build

# Set the working directory inside the container
WORKDIR /usr/src/app

# Copy package files and install dependencies as root (this is necessary)
COPY package*.json ./
RUN npm ci

# Copy the project files
COPY . .

# Build the React app for production
RUN npm run build

# Create a non-root user for security
RUN addgroup -g 1001 -S buildgroup && \
    adduser -u 1001 -S -G buildgroup builduser && \
    chown -R builduser:buildgroup /usr/src/app

# Switch to non-root user
USER builduser

################################
#### PRODUCTION ENVIRONMENT ####
################################

# Use the unprivileged NGINX image for production
FROM nginxinc/nginx-unprivileged:1.29-alpine AS production

# Copy nginx configuration with correct ownership
COPY --from=build --chown=nginx:nginx /usr/src/app/default.conf /etc/nginx/conf.d/default.conf

# Copy the build output from the dist folder into the Nginx html directory
COPY --from=build --chown=nginx:nginx /usr/src/app/dist /usr/share/nginx/html

# Expose port 8080 (unprivileged port)
EXPOSE 8080

# Run Nginx in the foreground
ENTRYPOINT ["nginx", "-g", "daemon off;"]
