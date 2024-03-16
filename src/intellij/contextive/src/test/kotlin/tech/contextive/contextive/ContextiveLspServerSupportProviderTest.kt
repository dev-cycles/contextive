package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories
import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import org.junit.jupiter.api.Assertions.*
import org.junit.jupiter.api.Test
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import io.mockk.*


class ContextiveLspServerSupportProviderTest {

    @Test
    fun ShouldAlwaysStartLanguageServer() {
        val lspServerSupportProvider = ContextiveLspServerSupportProvider()

        val project = mockk<Project> {
            every { getService<BaseProjectDirectories>(any()) } returns
                mockk {
                    every { getBaseDirectories() } returns emptySet()
                }
        }
        val file = mockk<VirtualFile>()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter> {
            every { ensureServerStarted(any()) } returns Unit
        }

        lspServerSupportProvider.fileOpened(project, file, serverStarter);

        verify { serverStarter.ensureServerStarted(any()) }

        confirmVerified(serverStarter)
    }
}