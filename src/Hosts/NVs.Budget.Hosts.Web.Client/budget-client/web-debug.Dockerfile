# Use Node.js 20 as base image
FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Expose port 4200 (default Angular dev server port)
EXPOSE 4200

# Expose port 49153 (for hot reload)
EXPOSE 49153

# Start the development server
CMD ["npm", "start", "--", "--host", "0.0.0.0", "--port", "4200", "--poll", "2000"] 