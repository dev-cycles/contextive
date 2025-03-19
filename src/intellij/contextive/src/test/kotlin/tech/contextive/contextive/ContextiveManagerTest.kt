package tech.contextive.contextive

import com.intellij.openapi.project.Project
import com.intellij.platform.lsp.api.LspServerSupportProvider
import io.mockk.*
import org.junit.jupiter.api.Test
import java.nio.file.Path
import java.util.*

class ContextiveManagerTest {

    private fun getImmediateDownloadScheduler(project: Project, expectedDownloadPath: Path) : LanguageServerDownloadScheduler {
        return mockk<LanguageServerDownloadScheduler>(relaxed = true) {
            every {
                scheduleDownloadIfRequired(project)
            } returns DownloadStatus.DOWNLOADED(expectedDownloadPath)
        }
    }

    private fun getImmediateDownloadScheduler(project: Project) = getImmediateDownloadScheduler(project, Path.of(""))

    @Test
    fun givenContextiveFilePresence_EnsureStartedIfPresent() {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)

        val contextiveManager = ContextiveManager(getImmediateDownloadScheduler(project), project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify(exactly = 1) { serverStarter.ensureServerStarted(ofType(LspDescriptor::class)) }
    }

    @Test
    fun givenStarting_EnsureLanguageServerIsDownloaded()
    {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)
        val contextiveLsDownloader = getImmediateDownloadScheduler(project)

        val contextiveManager = ContextiveManager(contextiveLsDownloader, project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify(exactly = 1) { contextiveLsDownloader.scheduleDownloadIfRequired(project) }
    }

    @Test
    fun givenStarting_EnsureStartedWithLocationDownloadedTo()
    {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)
        val mockPath = Path.of("/" + UUID.randomUUID())
        val contextiveLsDownloader = getImmediateDownloadScheduler(project, mockPath)

        val contextiveManager = ContextiveManager(contextiveLsDownloader, project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify { serverStarter.ensureServerStarted(match<LspDescriptor> { it.languageServerPath == mockPath }) }
    }
}