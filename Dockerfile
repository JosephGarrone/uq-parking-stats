# Base to build from
FROM node:latest

# Create app directory
RUN mkdir -p /usr/src/uq-parking-stats
WORKDIR /usr/src/uq-parking-stats

# Install dependencies
COPY package.json /usr/src/uq-parking-stats
RUN npm install

# Bundle app source
COPY . /usr/src/uq-parking-stats

# Port to expose from inside container
ENV PORT=3001
EXPOSE 3001

CMD ["npm", "start"]
