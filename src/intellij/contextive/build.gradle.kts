plugins {
  id("java")
  id("org.jetbrains.kotlin.jvm") version "1.9.23"
  id("org.jetbrains.intellij") version "1.17.2"
}

group = "tech.contextive"
version = "1.11.0"

repositories {
  mavenCentral()
}

dependencies {
  implementation("net.lingala.zip4j:zip4j:2.11.5")
  testImplementation(kotlin("test"))
  testImplementation("org.junit.jupiter:junit-jupiter-params:5.10.0")
  testRuntimeOnly("org.junit.platform:junit-platform-launcher")
  testImplementation("io.mockk:mockk:1.13.10")
  testImplementation("com.github.stefanbirkner:system-lambda:1.1.0")
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
      System.getenv("CERTIFICATE_CHAIN_PATH")?.let { File(it).readText(Charsets.UTF_8) }
    )
    privateKey.set(
      System.getenv("PRIVATE_KEY") ?:
      System.getenv("PRIVATE_KEY_PATH")?. let { File(it ).readText(Charsets.UTF_8) }
    )
    password.set(System.getenv("PRIVATE_KEY_PASSWORD"))
  }

  publishPlugin {
    token.set(System.getenv("PUBLISH_TOKEN"))
  }

  test {
    useJUnitPlatform()
    testLogging {
      events("PASSED", "SKIPPED", "FAILED")
    }
  }
}
