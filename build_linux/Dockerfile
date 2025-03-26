FROM almalinux:latest

RUN dnf -y update && \
    dnf -y install \
    gcc \
    clang \
    make \
    && dnf clean all

WORKDIR /workspace

COPY .. .

WORKDIR /workspace/build_linux

CMD ["make"]
