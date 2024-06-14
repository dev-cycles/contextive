package tech.contextive.contextive

import com.intellij.openapi.application.ApplicationManager
import io.mockk.*
import org.junit.jupiter.api.AfterEach
import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.CsvSource
import java.net.URI
import java.nio.file.Path
import java.util.concurrent.CompletableFuture

class LanguageServerDownloadSchedulerTest {

    @AfterEach
    fun unMock() {
        unmockkAll()
    }

    private fun mockkImmediatePooledThread() {
        val callback = slot<Runnable>()
        mockkStatic(ApplicationManager::class)
        every { ApplicationManager.getApplication() } returns mockk {
            every { executeOnPooledThread(capture(callback)) } answers {
                callback.captured.run()
                CompletableFuture.completedFuture(0)
            }
        }
    }

    @ParameterizedTest
    @CsvSource("false,1", "true,0")
    fun givenAlreadyDownloaded_ShouldCallbackImmediately(languageServerExists: Boolean, expectedDownloadCallCount: Int) {
        // Arrange
        mockkImmediatePooledThread()
        val lsPath = mockk<Path>()
        val fileSystem = mockk<FileSystem> {
            every { exists(lsPath) } returns languageServerExists
            every { createDirectories(lsPath) } returns lsPath
            every { setExecutable(lsPath) } returns lsPath
        }
        val lsUrl = mockk<URI>()
        val resolver = mockk<LanguageServerLocationResolver> {
            every { path() } returns lsPath
            every { url() } returns lsUrl
        }

        val downloader = mockk<LanguageServerDownloader>(relaxed = true)
        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem)
        var pathInCallback: Path = Path.of("")

        // Act
        scheduler.scheduleDownloadIfRequired { p -> pathInCallback = p }

        // Assert
        assertEquals(lsPath, pathInCallback)
        verify(exactly = expectedDownloadCallCount) {
            downloader.download(lsUrl, lsPath)
            fileSystem.createDirectories(lsPath)
            fileSystem.setExecutable(lsPath)
        }
    }

}