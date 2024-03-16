package tech.contextive.contextive

import com.intellij.execution.configurations.GeneralCommandLine
import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.project.BaseProjectDirectories.Companion.getBaseDirectories
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.intellij.platform.lsp.api.ProjectWideLspServerDescriptor
import com.intellij.testFramework.utils.vfs.getFile

private val LOG = logger<ContextiveLspServerSupportProvider>()

class ContextiveLspServerSupportProvider : LspServerSupportProvider {
    override fun fileOpened(
        project: Project,
        file: VirtualFile,
        serverStarter: LspServerSupportProvider.LspServerStarter
    ) {
        val contextiveDefinitionsFile = project.getBaseDirectories().first().findFileByRelativePath(".contextive/definitions.yml");
        if (contextiveDefinitionsFile?.exists() == true) {
            serverStarter.ensureServerStarted(ContextiveLspServerDescriptor(project))
        }
    }

}

private class ContextiveLspServerDescriptor(project: Project) : ProjectWideLspServerDescriptor(project, "Contextive") {
    override fun isSupportedFile(file: VirtualFile) = true
    override fun createCommandLine(): GeneralCommandLine =
        downloadLanguageServerIfNotFound().run { GeneralCommandLine(this.toString()) }
}