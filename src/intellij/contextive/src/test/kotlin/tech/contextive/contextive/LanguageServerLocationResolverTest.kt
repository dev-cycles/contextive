package tech.contextive.contextive

import com.github.stefanbirkner.systemlambda.SystemLambda.restoreSystemProperties
import io.mockk.every
import io.mockk.mockk
import io.mockk.mockkStatic
import org.junit.jupiter.api.Assertions
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.Arguments
import org.junit.jupiter.params.provider.MethodSource
import java.util.*
import java.util.stream.Stream
import kotlin.streams.asStream
import com.intellij.ide.plugins.PluginManagerCore
import com.intellij.ide.plugins.IdeaPluginDescriptor
import com.intellij.openapi.extensions.PluginId
import io.mockk.unmockkAll
import net.harawata.appdirs.AppDirsFactory
import org.junit.jupiter.api.AfterEach
import org.junit.jupiter.api.Test
import java.net.URI
import java.nio.file.Path

class LanguageServerLocationResolverTest {

    @AfterEach
    fun `Remove all mocks`() {
        unmockkAll()
    }

    private fun mockPluginManager(basePath: String, version: String) {
        val plugin = mockk<IdeaPluginDescriptor> {
            every { getVersion() } answers { version }
        }
        mockkStatic(PluginManagerCore::class)
        every {
            PluginManagerCore.getPlugin(
                match<PluginId> { it.idString == "tech.contextive.contextive" }
            )
        } answers { plugin }
        mockkStatic(AppDirsFactory::class)
        every {
            AppDirsFactory.getInstance()
        } returns mockk {
            every {
                getUserDataDir("Contextive", null, "tech.contextive")
            } returns basePath
        }
    }

    @ParameterizedTest
    @MethodSource("generateUrlArgs")
    fun shouldGetUrl(version: String, osName: String, archName: String, expectedUrl: URI) {
        restoreSystemProperties {
            System.setProperty("os.name", osName)
            System.setProperty("os.arch", archName)
            mockPluginManager("", version)
            val resolver = LanguageServerLocationResolver()

            val url = resolver.url()

            Assertions.assertEquals(expectedUrl, url)
        }

    }

    @Test
    fun shouldRestoreJnaNoClassPath() {
        restoreSystemProperties {
            System.setProperty("jna.noclasspath", "true")
            mockPluginManager("", "1.10")

            val resolver = LanguageServerLocationResolver()
            resolver.path()

            Assertions.assertEquals(System.getProperty("jna.noclasspath"), "true")
        }

    }

    @ParameterizedTest
    @MethodSource("generatePathArgs")
    fun shouldGetPath(basePath: String, version: String, osName: String, expectedPath: String) {
        restoreSystemProperties {
            System.setProperty("os.name", osName)
            mockPluginManager(basePath, version)
            val resolver = LanguageServerLocationResolver()

            val path = resolver.path()

            Assertions.assertEquals(Path.of(expectedPath), path)
        }

    }

    companion object {

        private val osOpts = sequenceOf(
            Triple ( "Windows", "win", ".exe"),
            Triple ( "MacOs X", "osx", ""),
            Triple ( UUID.randomUUID().toString(), "linux", "")
        )

        private val archOpts = sequenceOf(
            Pair ( "aarch64", "arm64" ),
            Pair ( UUID.randomUUID().toString() , "x64" )
        )

        private val versions = sequenceOf(
            "1.10.0",
            "2.5.1"
        )

        @JvmStatic
        fun generateUrlArgs(): Stream<Arguments> {
            val seq = sequence {
                osOpts.forEach { osOpt ->
                    archOpts.forEach { archOpt ->
                        versions.forEach { version ->
                            val expectedUrl =
                                URI("https://github.com/dev-cycles/contextive/releases/download/v%s/Contextive.LanguageServer-%s-%s-%1\$s.zip".format(
                                    version,
                                    osOpt.second,
                                    archOpt.second
                                ))
                            yield(
                                Arguments.of(
                                    version,
                                    osOpt.first,
                                    archOpt.first,
                                    expectedUrl
                                )
                            )
                        }

                    }
                }
            }

            return seq.asStream()
        }

        @JvmStatic
        fun generatePathArgs(): Stream<Arguments> {
            val seq = sequence {
                osOpts.forEach { osOpt ->
                        versions.forEach { version ->
                            val basePath = "/%s".format(UUID.randomUUID().toString())
                            val expectedPath = "%s/language-server/%s/Contextive.LanguageServer%s".format(
                                basePath,
                                version,
                                osOpt.third
                            )
                            yield(
                                Arguments.of(
                                    basePath,
                                    version,
                                    osOpt.first,
                                    expectedPath
                                )
                            )
                        }
                }
            }

            return seq.asStream()
        }
    }
}