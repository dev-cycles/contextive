plugins {
  id("java")
  id("org.jetbrains.kotlin.jvm") version "1.9.23"
  id("org.jetbrains.intellij") version "1.17.2"
}

group = "tech.contextive"
version = "1.10.5"

repositories {
  mavenCentral()
}

dependencies {
  implementation("net.lingala.zip4j:zip4j:2.11.5")
}

// Configure Gradle IntelliJ Plugin
// Read more: https://plugins.jetbrains.com/docs/intellij/tools-gradle-intellij-plugin.html
intellij {
  version.set("2023.3.2")
  type.set("IU") // Target IDE Platform

  plugins.set(listOf(/* Plugin Dependencies */))
}

tasks {
  // Set the JVM compatibility versions
  withType<JavaCompile> {
    sourceCompatibility = "17"
    targetCompatibility = "17"
  }
  withType<org.jetbrains.kotlin.gradle.tasks.KotlinCompile> {
    kotlinOptions.jvmTarget = "17"
  }

  patchPluginXml {
    sinceBuild.set("233")
    untilBuild.set("241.*")
  }

  signPlugin {
    certificateChain.set(
      System.getenv("CERTIFICATE_CHAIN") ?:
      File(System.getenv("CERTIFICATE_CHAIN_PATH")).readText(Charsets.UTF_8)
    )
    privateKey.set(System.getenv("PRIVATE_KEY") ?:
      File(System.getenv("PRIVATE_KEY_PATH")).readText(Charsets.UTF_8)
    )
    password.set(System.getenv("PRIVATE_KEY_PASSWORD"))
  }

  publishPlugin {
    token.set(System.getenv("PUBLISH_TOKEN"))
  }
}
