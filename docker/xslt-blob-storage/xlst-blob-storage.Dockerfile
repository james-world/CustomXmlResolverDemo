ARG AZURITE_VERSION="latest"
FROM mcr.microsoft.com/azure-storage/azurite:${AZURITE_VERSION}

# Install azure-storage-blob python package
RUN apk update && \
    apk --no-cache add py3-pip && \
    apk add --virtual=build gcc libffi-dev musl-dev python3-dev && \
    pip3 install --upgrade pip && \
    pip3 install azure-storage-blob==12.12.0

# Copy init_azurite_py script
COPY ./init_azurite.py init_azurite.py

# Copy local blobs to azurite
COPY ./init_containers init_containers

# Run the blob emulator and initialize the blob containers
CMD python3 init_azurite.py --directory=init_containers & \
    azurite-blob --blobHost 0.0.0.0 --blobPort 10000 --inMemoryPersistence