import org.jetbrains.kotlin.gradle.dsl.JvmTarget
import org.jetbrains.kotlin.gradle.tasks.KotlinJvmCompile

plugins {
  id("java")
  id("org.jetbrains.kotlin.jvm") version "2.2.20"
  id("org.jetbrains.intellij.platform") version "2.9.0"
  id("com.github.ben-manes.versions") version "0.52.0"
}

group = "tech.contextive"
version = "1.17.6"

repositories {
  mavenCentral()
  intellijPlatform{
    defaultRepositories()
  }
}

dependencies {
  implementation("net.harawata:appdirs:1.4.0")
  implementation("net.lingala.zip4j:zip4j:2.11.5")
  testImplementation(kotlin("test"))
  testImplementation("org.junit.jupiter:junit-jupiter-params:6.0.0-RC3")
  testRuntimeOnly("org.junit.platform:junit-platform-launcher:6.0.0-RC3")
  testImplementation("io.mockk:mockk:1.14.5")
  testImplementation("com.github.stefanbirkner:system-lambda:1.2.1")
  intellijPlatform {
    intellijIdeaUltimate("2025.1.5.1")
      // switching to 2025.2.2 started causing binary incompatibilities:
      // `Class com.intellij.openapi.extensions.PluginId does not have member field 'com.intellij.openapi.extensions.PluginId$Companion Companion'`
      // leaving it at 2025.1.5.1 resolves the issue, and builds a plugin that seems to work in 2025.2.2
      // This will need updating and checking with 2025.3 as Ultimate and Community are consolidating
      // Binary compatibility issues with 2024.* has prompted to changesinceBuild to 251
      // intellijIdeaUltimate("2025.2.2")
  }
}

tasks {
  patchPluginXml {
    sinceBuild.set("251")
    untilBuild.set(provider { null })
  }

  withType<JavaCompile> {
    sourceCompatibility = "21"
    targetCompatibility = "21"
  }

  withType<KotlinJvmCompile> {
    compilerOptions {
      jvmTarget = JvmTarget.JVM_21
    }
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
    val javaToolchains = project.extensions.getByType<JavaToolchainService>()
    javaLauncher.set(javaToolchains.launcherFor {
        languageVersion.set(JavaLanguageVersion.of(21))
    })
    testLogging {
      events("PASSED", "SKIPPED", "FAILED")
    }
  }
}
