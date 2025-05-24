import org.jetbrains.kotlin.gradle.dsl.JvmTarget
import org.jetbrains.kotlin.gradle.tasks.KotlinJvmCompile

plugins {
  id("java")
  id("org.jetbrains.kotlin.jvm") version "2.1.10"
  id("org.jetbrains.intellij.platform") version "2.3.0"
  id("com.github.ben-manes.versions") version "0.52.0"
}

group = "tech.contextive"
version = "1.17.1"

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
  testImplementation("org.junit.jupiter:junit-jupiter-params:5.12.0")
  testRuntimeOnly("org.junit.platform:junit-platform-launcher")
  testImplementation("io.mockk:mockk:1.13.17")
  testImplementation("com.github.stefanbirkner:system-lambda:1.2.1")
  intellijPlatform {
    //intellijIdeaUltimate("251-EAP-SNAPSHOT", useInstaller = false)
    intellijIdeaUltimate("2024.3.4.1")
  }
}

tasks {
  patchPluginXml {
    sinceBuild.set("241")
    untilBuild.set("251.*")
  }

  withType<JavaCompile> {
    sourceCompatibility = "17"
    targetCompatibility = "17"
  }

  withType<KotlinJvmCompile> {
    compilerOptions {
      jvmTarget = JvmTarget.JVM_17
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
    testLogging {
      events("PASSED", "SKIPPED", "FAILED")
    }
  }
}
