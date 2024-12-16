FROM almalinux:latest

RUN yum -y update && \
    yum -y install \
    gcc \
    clang \
    make \
    && yum clean all

WORKDIR /workspace

COPY .. .

WORKDIR /workspace/build_linux

CMD ["make"]
