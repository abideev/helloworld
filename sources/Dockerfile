FROM maven:3.6.1-jdk-8-alpine  AS build

COPY pom.xml /tmp/
COPY src /tmp/src/

WORKDIR /tmp/

RUN mvn package


FROM openjdk:8-jdk-alpine

EXPOSE 8080

CMD java -jar /data/hello-world-0.0.1.jar
COPY --from=build /tmp/target/hello-world-0.0.1.jar /data/hello-world-0.0.1.jar
