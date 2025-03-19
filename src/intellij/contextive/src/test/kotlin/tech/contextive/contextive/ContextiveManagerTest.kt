package tech.contextive.contextive

import com.intellij.openapi.project.Project
import com.intellij.platform.lsp.api.LspServerSupportProvider
import io.mockk.*
import org.junit.jupiter.api.Test
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.CsvSource
import java.nio.file.Path
import java.util.*

class ContextiveManagerTest {

    private fun getContextiveActiveChecker(isActive: Boolean) = mockk<ContextiveActiveChecker> {
        every { isActive() } returns isActive
    }

    private fun getImmediateDownloadScheduler(project: Project, expectedDownloadPath: Path) : LanguageServerDownloadScheduler {
        return mockk<LanguageServerDownloadScheduler>(relaxed = true) {
            every {
                scheduleDownloadIfRequired(project)
            } returns DownloadStatus.DOWNLOADED(expectedDownloadPath)
        }
    }

    private fun getImmediateDownloadScheduler(project: Project) = getImmediateDownloadScheduler(project, Path.of(""))

    @ParameterizedTest
    @CsvSource("false,0", "true,1")
    fun givenContextiveFilePresence_EnsureStartedIfPresent(isContextiveActive: Boolean, expectedServerStartInvocationCount: Int) {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)
        val contextiveActiveChecker = getContextiveActiveChecker(isContextiveActive)

        val contextiveManager = ContextiveManager(contextiveActiveChecker, getImmediateDownloadScheduler(project), project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify(exactly = expectedServerStartInvocationCount) { serverStarter.ensureServerStarted(ofType(LspDescriptor::class)) }
    }

    @ParameterizedTest
    @CsvSource("false,0", "true,1")
    fun givenStarting_EnsureLanguageServerIsDownloaded(isContextiveActive: Boolean, expectedServerStartInvocationCount: Int)
    {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)
        val contextiveLsDownloader = getImmediateDownloadScheduler(project)
        val contextiveActiveChecker = getContextiveActiveChecker(isContextiveActive)

        val contextiveManager = ContextiveManager(contextiveActiveChecker, contextiveLsDownloader, project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify(exactly = expectedServerStartInvocationCount) { contextiveLsDownloader.scheduleDownloadIfRequired(project) }
    }

    @Test
    fun givenStarting_EnsureStartedWithLocationDownloadedTo()
    {
        // Arrange
        val project = getMockedProject()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)
        val mockPath = Path.of("/" + UUID.randomUUID())
        val contextiveActiveChecker = getContextiveActiveChecker(true)
        val contextiveLsDownloader = getImmediateDownloadScheduler(project, mockPath)

        val contextiveManager = ContextiveManager(contextiveActiveChecker, contextiveLsDownloader, project, serverStarter)

        // Act
        contextiveManager.startIfRequired()

        // Assert
        verify { serverStarter.ensureServerStarted(match<LspDescriptor> { it.languageServerPath == mockPath }) }
    }
}