package tech.contextive.contextive

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import io.mockk.*
import org.bouncycastle.util.test.SimpleTest.runTest
import org.junit.jupiter.api.AfterEach
import org.junit.jupiter.api.Assertions.*
import org.junit.jupiter.api.Test
import java.net.URI
import java.nio.file.Path
import java.util.concurrent.CompletableFuture
import kotlin.test.assertIs

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
        // dependencies
        val resolver: LanguageServerLocationResolver,
        val downloader: LanguageServerDownloader,
        val fileSystem: FileSystem,
        val lspManager: LspManager,
        // objects
        val project: Project,
        val lsPath: Path,
        val lsPathParent: Path,
        val lsUrl: URI)

    private fun setupMocks(languageServerExists: Boolean): Mocks {
        val project = mockk<Project>()
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
        val lspManager = mockk<LspManager>(relaxed = true)
        return Mocks(resolver, downloader, fileSystem, lspManager, project, lsPath, lsPathParent, lsUrl)
    }

    @Test
    fun givenAlreadyDownloaded_ShouldCallbackImmediately() {
        // Arrange
        val (resolver, downloader, fileSystem, lspManager, project, lsPath, lsPathParent, lsUrl) = setupMocks(true)

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem, lspManager)

        // Act
        val status = scheduler.scheduleDownloadIfRequired(project)

        // Assert
        assertEquals(lsPath, (status as DownloadStatus.DOWNLOADED).path)

        verify(exactly = 0) {
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
            lspManager.start(project, any())
        }
        coVerify(exactly = 0) {
            downloader.download(project, lsUrl, lsPath)
        }
    }

    @Test
    fun givenNotYetDownloaded_ShouldCallbackAfterDownloading() {
        mockkImmediatePooledThread()
        val (resolver, downloader, fileSystem, lspManager, project, lsPath, lsPathParent, lsUrl) = setupMocks(false)

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem, lspManager)

        // Act
        val status = scheduler.scheduleDownloadIfRequired(project)

        // Assert
        assertIs<DownloadStatus.SCHEDULED>(status)
        verify(exactly = 1) {
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
            lspManager.start(project, any())
        }
        coVerify(exactly = 1) {
            downloader.download(project, lsUrl, lsPath)
        }
    }

    @Test
    fun givenMidDownload_ShouldDoNothing() {
        mockkNeverExecutePooledThread()
        val (resolver, downloader, fileSystem, lspManager, project, lsPath, lsPathParent, lsUrl) = setupMocks(false)

        val scheduler = LanguageServerDownloadScheduler(resolver, downloader, fileSystem, lspManager)

        // Act
        // Start Download (callbacks never called because of mock to pooled thread executor)
        scheduler.scheduleDownloadIfRequired(project)
        // Change mocks to ensure callbacks would be called if download would proceed or complete
        mockkImmediatePooledThread()
        val status = scheduler.scheduleDownloadIfRequired(project)

        // Assert
        // Neither callback called, nor download initiated
        assertIs<DownloadStatus.DOWNLOADING>(status)
        verify(exactly = 0) {
            fileSystem.createDirectories(lsPathParent)
            fileSystem.setExecutable(lsPath)
            lspManager.start(project, any())
        }
        coVerify(exactly = 0) {
            downloader.download(project, lsUrl, lsPath)
        }
    }

}