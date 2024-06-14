package tech.contextive.contextive

import com.intellij.openapi.application.ApplicationManager
import io.mockk.*
import org.junit.jupiter.api.AfterEach
import org.junit.jupiter.api.Assertions.*
import org.junit.jupiter.api.Test
import java.net.URI
import java.nio.file.Path
import java.util.concurrent.CompletableFuture

class LanguageServerDownloadSchedulerTest {

    @AfterEach
    fun unMock() {
        unmockkAll()
        LanguageServerDownloadScheduler.resetDownloadingStatus()
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

    private fun mockkNeverExecutePooledThread() {
        mockkStatic(ApplicationManager::class)
        every { ApplicationManager.getApplication() } returns mockk {
            every { executeOnPooledThread(any()) } answers {
                CompletableFuture.completedFuture(0)
            }
        }
    }

    private data class Mocks(
        val resolver: LanguageServerLocationResolver,
        val downloader: LanguageServerDownloader,
        val fileSystem: FileSystem,
        val lsPath: Path,
        val lsPathParent: Path,
        val lsUrl: URI)

    private fun setupMocks(languageServerExists: Boolean): Mocks {
        val lsPathParent = mockk<Path>()
        val lsPath = mockk<Path> {
            every { parent } returns lsPathParent
        }
        val fileSystem = mockk<FileSystem> {
            every { exists(lsPath) } returns languageServerExists
            every { createDirectories(lsPathParent) } returns lsPathParent
            every { setExecutable(lsPath) } returns lsPath
        }
        val lsUrl = mockk<URI>()
        val resolver = mockk<LanguageServerLocationResolver> {
            every { path() } returns lsPath
            every { url() } returns lsUrl
        }
        val downloader = mockk<LanguageServerDownloader>(relaxed = true)
        return Mocks(resolver, downloader, fileSystem, lsPath, lsPathParent, lsUrl)
    }

    @Test
    fun givenAlreadyDownloaded_ShouldCallbackImmediately() {
        // Arrange
        val  (resolver, downloader, fileSystem, lsPath, lsPathParent, lsUrl) = setupMocks(true)
        var pathInCallback: Path = Path.of("")

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem)

        // Act
        scheduler.scheduleDownloadIfRequired( { }, { p -> pathInCallback = p })

        // Assert
        assertEquals(lsPath, pathInCallback)

        verify(exactly = 0) {
            downloader.download(lsUrl, lsPath)
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
        }
    }

    @Test
    fun givenNotYetDownloaded_ShouldCallbackAfterDownloading() {
        mockkImmediatePooledThread()
        val  (resolver, downloader, fileSystem, lsPath, lsPathParent, lsUrl) = setupMocks(false)
        var calledBack = false

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem)

        // Act
        scheduler.scheduleDownloadIfRequired( { calledBack = true }, { })

        // Assert
        assertTrue(calledBack)
        verify(exactly = 1) {
            downloader.download(lsUrl, lsPath)
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
        }
    }

    @Test
    fun givenMidDownload_ShouldDoNothing() {
        mockkNeverExecutePooledThread()
        val  (resolver, downloader, fileSystem, lsPath, lsPathParent, lsUrl) = setupMocks(false)
        var calledBack = false
        var pathInCallback: Path = Path.of("")

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem)

        // Act
        // Start Download (callbacks never called because of mock to pooled thread executor)
        scheduler.scheduleDownloadIfRequired( { calledBack = true }, { p -> pathInCallback = p })
        // Change mocks to ensure callbacks would be called if download would proceed or complete
        mockkImmediatePooledThread()
        scheduler.scheduleDownloadIfRequired( { calledBack = true }, { p -> pathInCallback = p })

        // Assert
        // Neither callback called, nor download initiated
        assertFalse(calledBack)
        assertEquals(Path.of(""), pathInCallback)
        verify(exactly = 0) {
            downloader.download(lsUrl, lsPath)
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
        }
    }

}