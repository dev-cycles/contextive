package tech.contextive.contextive

import com.intellij.openapi.project.BaseProjectDirectories
import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import org.junit.jupiter.api.Assertions.*
import org.junit.jupiter.api.Test
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.jetbrains.rd.generator.nova.PredefinedType
import io.mockk.*
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.CsvSource


class ContextiveLspServerSupportProviderTest {

    private fun getMockedProject(isContextiveFilePresent: Boolean): Project {
        val baseDirectory = mockk<VirtualFile>() {
            every { findFileByRelativePath(".contextive/definitions.yml") } returns
                    mockk {
                        every { exists() } returns isContextiveFilePresent
                    }
        }

        return mockk<Project> {
            every { getService<BaseProjectDirectories>(any()) } returns
                    mockk {
                        every { getBaseDirectories() } returns setOf(baseDirectory)
                    }
        }
    }

    @ParameterizedTest
    @CsvSource("true,1", "false,0")
    fun onlyEnsureServerStartedIfContextiveFileIsPresent(isContextiveFilePresent: Boolean, expectedServerStartInvocationCount: Int) {
        val project = getMockedProject(isContextiveFilePresent)
        val file = mockk<VirtualFile>()
        val serverStarter = mockk<LspServerSupportProvider.LspServerStarter>(relaxed = true)

        val lspServerSupportProvider = ContextiveLspServerSupportProvider()

        lspServerSupportProvider.fileOpened(project, file, serverStarter);

        verify(exactly = expectedServerStartInvocationCount) { serverStarter.ensureServerStarted(any()) }
    }

}